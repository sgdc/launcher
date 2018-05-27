using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using AxWMPLib;
using System.Runtime.InteropServices;
using SlimDX;
using System.Management;


namespace launcherA
{
    
    public partial class SGDCLauncher : Form
    {

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        const int KILLGAME_HOTKEY_ID = 1;
        const int VOLUMEUP_HOTKEY_ID = 2;
        const int VOLUMEDOWN_HOTKEY_ID = 3;
        const int VOLUMEMUTE_HOTKEY_ID = 4;

        string[] dirs;


        List<Game> gamesList;

        Process runningProcess;
        DateTime timeStarted;
        DateTime launcherStartTime;

        int selected;
        List<GamepadState> controllers;

        bool shouldUpdate = false;

        bool attract = false;

        int gameDelay = 100; //100 ticks of no restarting
        int currentDelay = 0;

        //Executes upon the start of the program
        //Moves the cursor, gets the start time (used to calculate how long a game has been running)
        //checks the /games/ folder and reads through every info.json folder within
        //checks for a config.json in the same folder as the executable, and then parses it to update the program with that info
        //makes the video player have no UI and play on loop
        //runs PopulateSelectedGame() which updates the title, video, stats, and description to the first selected game
        //sets up some listeners for Function key presses and adds four Gamepads to the controllers List
        public SGDCLauncher() 
        {
            InitializeComponent(); //super()
            //move the mouse out of the way if it was somehow in the middle of the screen
            Cursor.Position = new Point(1920, 1080);
            launcherStartTime = DateTime.Now; //get start time
            if (Directory.Exists(Directory.GetCurrentDirectory() + "\\games\\")) //if /games/ exists, load in games
            {
                dirs = Directory.GetDirectories(Directory.GetCurrentDirectory() + "\\games\\", "*", SearchOption.TopDirectoryOnly); //get each directory in the /games folder
                gamesList = new List<Game>();
                foreach (string dir in dirs)
                {
                    string info = "";
                    try
                    {
                        foreach (string line in File.ReadLines(dir + "\\info.json")) //read in info.json
                        {
                            info += line;
                        }
                        gamesList.Add(JsonConvert.DeserializeObject<Game>(info)); //parse info.json to type Game, and add it to gamesList
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Failed loading info.json in " + dir);
                    }
                }
            } else
            {
                File.WriteAllLines(Directory.GetCurrentDirectory() + "\\error.txt", new string[] { "/games/ folder does not exist." });
                MessageBox.Show("/games/ folder does not exist");
            }

            if (File.Exists(Directory.GetCurrentDirectory() + "\\config.json"))
            {
                //we have a config
                string config = "";
                foreach (string line in File.ReadAllLines(Directory.GetCurrentDirectory() + "\\config.json"))
                {
                    config += line;
                }
                Console.WriteLine(config);
                Config c = JsonConvert.DeserializeObject<Config>(config);
                if (c.message != null && c.message != "") //if a message is set
                {
                    lblMessage.Text = c.message;
                }

                if (c.subtitle != null && c.subtitle != "") //if a subtitle is set
                {
                    lblSubtitle.Text = c.subtitle + "\n\nGames:";
                }

                if (c.attractActivate > 1000) //has to be at least 1 second; how long you wait with no input until activating attract mode
                {
                    tmrAttractWait.Interval = c.attractActivate;
                    tmrAttractWait.Stop();
                    tmrAttractWait.Start();
                }

                if (c.attractScroll > 100)
                {
                    tmrAttract.Interval = c.attractScroll;
                    tmrAttract.Stop();
                    tmrAttract.Start();
                }

                if (c.startBlink > 0)
                {
                    tmrBlink.Interval = c.startBlink;
                    tmrBlink.Stop();
                    tmrBlink.Start();
                }
            }

            if (File.Exists(Directory.GetCurrentDirectory() + "\\logo.png"))
            {
                pbLogo.BackgroundImage = Image.FromFile(Directory.GetCurrentDirectory() + "\\logo.png"); //logo overwrite
            }

            //override video player settings
            mpVideo.settings.setMode("loop", true);
            mpVideo.uiMode = "none";

            runningProcess = null;
            timeStarted = default(DateTime); //can't use null so...okay

            if (gamesList == null || gamesList.Count <= 0)
            {
                //no games in games folder
                File.WriteAllLines(Directory.GetCurrentDirectory() + "\\error.txt", new string[] { "/games/ folder is empty." });
                MessageBox.Show("/games/ folder is empty");
            }


            PopulateSelectedGame();

            RegisterHotKey(this.Handle, KILLGAME_HOTKEY_ID, 0, (int)Keys.F1);
            RegisterHotKey(this.Handle, VOLUMEUP_HOTKEY_ID, 0, (int)Keys.F2);
            RegisterHotKey(this.Handle, VOLUMEDOWN_HOTKEY_ID, 0, (int)Keys.F3);
            RegisterHotKey(this.Handle, VOLUMEMUTE_HOTKEY_ID, 0, (int)Keys.F4);


            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            controllers = new List<GamepadState>();
            controllers.Add(new GamepadState(SlimDX.XInput.UserIndex.One));
            controllers.Add(new GamepadState(SlimDX.XInput.UserIndex.Two));
            controllers.Add(new GamepadState(SlimDX.XInput.UserIndex.Three));
            controllers.Add(new GamepadState(SlimDX.XInput.UserIndex.Four));
        }

        //receive global hotkey event, used to overwrite system reaction to F1-F4
        protected override void WndProc(ref Message m) 
        {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == KILLGAME_HOTKEY_ID) //F1
            {
                if (runningProcess != null && !runningProcess.HasExited)
                    KillProcessAndChildrens(runningProcess.Id);
                UpdateGamePlayedTime();
            } else if (m.Msg == 0x0312 && m.WParam.ToInt32() == VOLUMEUP_HOTKEY_ID) //F2
            {
                VolUp();
            } else if (m.Msg == 0x0312 && m.WParam.ToInt32() == VOLUMEDOWN_HOTKEY_ID) //F3
            {
                VolDown();
            } else if (m.Msg == 0x0312 && m.WParam.ToInt32() == VOLUMEMUTE_HOTKEY_ID) //F4
            {
                Mute();
            }
            base.WndProc(ref m); //call normal process for those keys
        }

        //respond to regular key presses when in focus (takes up, down, space/enter)
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) 
        {
            if (runningProcess == null)
            {
                if (keyData == Keys.Up)
                {
                    EventUp();
                    return true;
                }
                else if (keyData == Keys.Down)
                {
                    EventDown(false);
                    return true;
                }
                else if (keyData == Keys.Enter || keyData == Keys.Space)
                {
                    EventStart();
                    return true;
                }
            }

            // Call the base class
            return base.ProcessCmdKey(ref msg, keyData);
        }

        //User Pressed Up
        //Go to the previous entry in the gamesList, set it to selected, and run PopulateSelectedGame
        void EventUp() 
        {
            if (runningProcess == null && gamesList != null)
            {
                ResetAttract();
                selected -= 1;
                if (selected < 0)
                    selected = gamesList.Count - 1;
                //Console.WriteLine(gamesList.Count.ToString());
                PopulateSelectedGame();
            }
        }

        //User Pressed Down
        //Go to the next entry in the gamesList, set it to selected, and run PopulateSelectedGame
        //auto means automatic, and if it's true (because the Attract Mode is what hit down), it wil not reset the Attract Mode timer.
        void EventDown(bool auto)
        {
            if (runningProcess == null && gamesList != null)
            {
                if (!auto)
                    ResetAttract();
                selected += 1;
                if (selected > gamesList.Count - 1)
                    selected = 0;
                //Console.WriteLine(gamesList.Count.ToString());
                PopulateSelectedGame();
            }
        }

        //User wants to load a game.
        //Make sure no game is loading, check that a valid executable file exists (either defined as exeName in info.json or just the first exe in the folder) and run it
        //StartGame() handles tracking the loaded process.
        void EventStart() //pressed start/space/enter
        {
            if (runningProcess == null && currentDelay <= 0 && gamesList != null)
            {
                ResetAttract();
                try
                {
                    if (gamesList[selected].exeName != null && gamesList[selected].exeName != "" && gamesList[selected].exeName.Contains("."))
                    {
                        StartGame(System.IO.Path.Combine(dirs[selected] + "\\", gamesList[selected].exeName));
                    }
                    else
                    {
                        StartGame(Directory.GetFiles(dirs[selected], "*.exe")[0]); //take first exe you find
                    }
                }
                catch (Exception)
                {
                    selected = 0;
                    PopulateSelectedGame();
                }
            }
        }

        //Given `path`, launch that game and update stats
        void StartGame(string path) 
        {
            runningProcess = Process.Start(path);
            gamesList[selected].plays += 1;
            WriteGamesJson();
            UpdatePlays();

            timeStarted = DateTime.Now;

            runningProcess.EnableRaisingEvents = true;
            runningProcess.Exited += new EventHandler(HandleGameExit); //set HandleGameExit() as the handler for the game being closed.
        }

        //system listener for game exiting
        void HandleGameExit(object sender, EventArgs e)
        {
            runningProcess = null;
            currentDelay = gameDelay;
            UpdateGamePlayedTime();
        }

        //Resets tmrAttractWait -- if tmrAttractWait activates, it sets attract mode to true.
        void ResetAttract()
        {
            attract = false;
            tmrAttractWait.Stop();
            tmrAttractWait.Start();
            Console.WriteLine("Reset attract.");
        }

        //on game close, update the time, save it to file, and queue a screen update
        void UpdateGamePlayedTime() 
        {
            TimeSpan total = DateTime.Now.Subtract(timeStarted);
            gamesList[selected].time += total.Minutes + 1;
            WriteGamesJson();
            shouldUpdate = true; //have to queue because handleGameExit can call this which is on a different thread
        }

        //manually serialize gamesList back to a json file, done strictly for prettier formatting in the json file
        void WriteGamesJson() 
        {
            for (int i = 0; i < gamesList.Count; i++)
            {
                string s = "";
                Game g = gamesList[i];
                s += "{\n";
                s += "\t\"name\": \"" + g.name + "\",\n";
                s += "\t\"description\": \"" + g.description + "\",\n";
                s += "\t\"devs\": \"" + g.devs + "\",\n";
                if (g.exeName != null && g.exeName.Length > 1) //only put it there if it existed
                    s += "\t\"exeName\": \"" + g.exeName + "\",\n";
                if (g.videoName != null && g.videoName.Length > 1) //only put it there if it existed
                    s += "\t\"videoName\": \"" + g.videoName + "\",\n";
                if (g.time > 0)
                    s += "\t\"time\": " + g.time + ",\n";
                if (g.plays > 0)
                    s += "\t\"plays\": " + g.plays + "\n";
                s += "}";
                try
                {
                    File.WriteAllText(dirs[i] + "\\info.json", s);
                } catch (Exception e)
                {
                    Console.WriteLine("Could not write to " + dirs[i] + ".\n" + e.ToString());
                }
            }
        }

        //writes the games list to reflect selected game, and updates elements on the screen accordingly
        void PopulateSelectedGame() 
        {
            if (gamesList != null && gamesList.Count > 0)
            {
                string listText = "";
                for (int i = 0; i < gamesList.Count; i++)
                {
                    if (i == selected)
                    {
                        listText += "> " + gamesList[i].name + " <\n";
                    }
                    else
                    {
                        listText += gamesList[i].name + "\n";
                    }
                }
                lblList.Text = listText;
                lblTitle.Text = gamesList[selected].name;
                lblDesc.Text = gamesList[selected].description;
                lblDevs.Text = "By: " + gamesList[selected].devs;
                if (gamesList[selected].videoName != null && gamesList[selected].videoName != "" && File.Exists(System.IO.Path.Combine(Directory.GetCurrentDirectory() + "\\videos\\", gamesList[selected].videoName))) //if specified video exists
                    mpVideo.URL = System.IO.Path.Combine(Directory.GetCurrentDirectory() + "\\videos\\", gamesList[selected].videoName);
                else if (File.Exists(Directory.GetCurrentDirectory() + "\\videos\\static.mp4"))
                {
                    mpVideo.URL = System.IO.Path.Combine(Directory.GetCurrentDirectory() + "\\videos\\", "static.mp4");
                }

                UpdatePlays();
            }
        }

        //sets the lblPlays text to "Plays: x       Time Played: d?:hh:mm"
        void UpdatePlays() 
        {
            string display = "Plays: " + gamesList[selected].plays.ToString() + "   Time Played: ";
            int hours = (int)Math.Floor(gamesList[selected].time / 60.0);
            int days = (int)hours / 24;
            hours -= (days * 24);
            int minutes = gamesList[selected].time % 60;
            string time = "";
            if (days > 0)
                time += days.ToString() + "d:";
            time += (hours < 10 ? "0" + hours.ToString() : hours.ToString()) + "h:" + (minutes < 10 ? "0" + minutes.ToString() : minutes.ToString()) + "m";
            lblPlays.Text = display + time;
        }

        // ------------------- VOLUME STUFF
        //looked all of this up online -- this is how you interface with Win32 to perform system events like "Volume Up"
        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg,
            IntPtr wParam, IntPtr lParam);

        private void Mute()
        {
            SendMessageW(this.Handle, WM_APPCOMMAND, this.Handle,
                (IntPtr)APPCOMMAND_VOLUME_MUTE);
        }

        private void VolDown()
        {
            SendMessageW(this.Handle, WM_APPCOMMAND, this.Handle,
                (IntPtr)APPCOMMAND_VOLUME_DOWN);
        }

        private void VolUp()
        {
            SendMessageW(this.Handle, WM_APPCOMMAND, this.Handle,
                (IntPtr)APPCOMMAND_VOLUME_UP);
        }
        // --------------- END VOLUME STUFF

        //recursively kills any and all processes started by the game we launched, hopefully guaranteeing its death
        private static void KillProcessAndChildrens(int pid) 
        {
            ManagementObjectSearcher processSearcher = new ManagementObjectSearcher
              ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection processCollection = processSearcher.Get();

            try
            {
                Process proc = Process.GetProcessById(pid);
                if (!proc.HasExited) proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }

            if (processCollection != null)
            {
                foreach (ManagementObject mo in processCollection)
                {
                    KillProcessAndChildrens(Convert.ToInt32(mo["ProcessID"])); //kill child processes(also kills childrens of childrens etc.)
                }
            }
        }

        //every tick poll the controllers for input, and perform actions as necessary
        private void TmrController_Tick(object sender, EventArgs e) 
        {
            bool didUp = false;
            bool didDown = false;
            bool didStart = false;
            for (int i = 0; controllers != null && i < controllers.Count; i++)
            {
                controllers[i].Update();

                //tracks previous and current stick location
                //if previous stick was less than left, and current stick is left, that's the frame they pressed left
                controllers[i].SetUDLR();

                if (controllers[i].A || controllers[i].Start)
                    didStart = true;
                if (controllers[i].Up)
                    didUp = true;
                if (controllers[i].Down)
                    didDown = true;
                controllers[i].LeftStickXPrev = controllers[i].LeftStick.Position.X;
                controllers[i].LeftStickYPrev = controllers[i].LeftStick.Position.Y;
                controllers[i].DPadPrev = controllers[i].DPad;
            }

            if (didUp)
                EventUp();
            if (didDown)
                EventDown(false);
            if (didStart)
                EventStart();

            //hijacking this to also reduce delay -- as long as currentDelay is > 0, a new game can't be started.
            //This was done to prevent accidental game starts the instant a game was closed. It's set to 100 ticks by default.
            if (currentDelay > 0)
                currentDelay--;
        }

        //blinks the "Start Game" label, or changes it to "Loading..." if a game is in progress.
        //Also queues a screen update for weird thread cases.
        private void TmrBlink_Tick(object sender, EventArgs e) 
        {
            if (attract)
            {
                lblPressStart.Visible = !lblPressStart.Visible;
                lblPressStart.Text = "Press Start"; //change if you want a custom Attract Mode text
            }
            else if (runningProcess != null)
            {
                lblPressStart.Visible = true;
                lblPressStart.Text = "Loading Game...";
            }
            else
            {
                lblPressStart.Text = "Press Start";
                lblPressStart.Visible = !lblPressStart.Visible;
            }

            if (shouldUpdate)
            {
                PopulateSelectedGame();
                shouldUpdate = false;
            }
        }

        //if attract mode is enabled, this will scroll through the games list
        private void TmrAttract_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("Got attract tick");
            if (attract && (runningProcess == null || runningProcess.HasExited))
            {
                EventDown(true); //manually "scroll down" once every tick
            }
        }

        //if attract mode isn't enabled, this timer ticks towards executing this event (gets reset to zero upon user action)
        //if this event is executed, attract mode gets enabled
        private void TmrAttractWait_Tick(object sender, EventArgs e)
        {
            attract = true;
            Console.WriteLine("Setting Attract to True.");
        }
    }

    //json class definitions
    public class Game
    {
        public string name { get; set; }
        public string description { get; set; }
        public string exeName { get; set; }
        public string videoName { get; set; }
        public string devs { get; set; }
        public int plays { get; set; }
        public int time { get; set; }
    }

    public class Config
    {
        public string message { get; set; }
        public string subtitle { get; set; }
        public int attractActivate { get; set; }
        public int attractScroll { get; set; }
        public int startBlink { get; set; }
    }
}
