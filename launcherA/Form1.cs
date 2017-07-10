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

        public SGDCLauncher()
        {
            InitializeComponent();
            launcherStartTime = DateTime.Now;
            if (Directory.Exists(Directory.GetCurrentDirectory() + "\\games\\"))
            {
                dirs = Directory.GetDirectories(Directory.GetCurrentDirectory() + "\\games\\", "*", SearchOption.TopDirectoryOnly); //get each directory in the /games folder
                gamesList = new List<Game>();
                foreach (string dir in dirs)
                {
                    string info = "";

                    try
                    {
                        foreach (string line in File.ReadLines(dir + "\\info.json"))
                        {
                            info += line;
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Failed loading info.json in " + dir);
                        Application.Exit();
                    }
                    gamesList.Add(JsonConvert.DeserializeObject<Game>(info));
                }
            } else
            {
                File.WriteAllLines(Directory.GetCurrentDirectory() + "\\error.txt", new string[] { "/games/ folder does not exist." });
                Application.Exit();
                Environment.Exit(1);
            }

            try
            {
                string message = "";
                foreach (string line in File.ReadLines(Directory.GetCurrentDirectory() + "\\message.txt"))
                {
                    message += line + "\n";
                }
                lblMessage.Text = message;
            } catch (Exception)
            {
                lblMessage.Text = "Failed loading message from message.txt. Make sure message.txt exists in the launcher directory.";

            }

            try
            {
                string subtitle = "";
                foreach (string line in File.ReadLines(Directory.GetCurrentDirectory() + "\\subtitle.txt"))
                {
                    subtitle += line;
                }
                lblSubtitle.Text = subtitle + "\n\nGames:";
            } catch (Exception)
            {
                File.WriteAllLines(Directory.GetCurrentDirectory() + "\\error.txt", new string[] { "subtitle.txt does not exist." });
                lblSubtitle.Text = "Subtitle\n\nGames:";
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
                Application.Exit();
                Environment.Exit(1);
            }


            populateSelectedGame();

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

        protected override void WndProc(ref Message m) //receive global hotkey event
        {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == KILLGAME_HOTKEY_ID) //F1
            {
                KillProcessAndChildrens(runningProcess.Id);
                updateGamePlayedTime();
            } else if (m.Msg == 0x0312 && m.WParam.ToInt32() == VOLUMEUP_HOTKEY_ID)
            {
                VolUp();
            } else if (m.Msg == 0x0312 && m.WParam.ToInt32() == VOLUMEDOWN_HOTKEY_ID)
            {
                VolDown();
            } else if (m.Msg == 0x0312 && m.WParam.ToInt32() == VOLUMEMUTE_HOTKEY_ID)
            {
                Mute();
            }
            base.WndProc(ref m);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) //respond to regular key presses when in focus (takes up, down, space/enter)
        {
            if (runningProcess == null)
            {
                if (keyData == Keys.Up)
                {
                    eventUp();
                    return true;
                }
                else if (keyData == Keys.Down)
                {
                    eventDown();
                    return true;
                }
                else if (keyData == Keys.Enter || keyData == Keys.Space)
                {
                    eventStart();
                    return true;
                }
            }

            // Call the base class
            return base.ProcessCmdKey(ref msg, keyData);
        }

        void eventUp() //pressed up
        {
            if (runningProcess == null)
            {
                selected -= 1;
                if (selected < 0)
                    selected = gamesList.Count - 1;
                //Console.WriteLine(gamesList.Count.ToString());
                populateSelectedGame();
            }
        }

        void eventDown() //pressed down
        {
            if (runningProcess == null)
            {
                selected += 1;
                if (selected > gamesList.Count - 1)
                    selected = 0;
                //Console.WriteLine(gamesList.Count.ToString());
                populateSelectedGame();
            }
        }

        void eventStart() //pressed start/space/enter
        {
            if (runningProcess == null)
            {
                try
                {
                    if (gamesList[selected].exeName != null && gamesList[selected].exeName != "" && gamesList[selected].exeName.Contains("."))
                    {
                        startGame(System.IO.Path.Combine(dirs[selected] + "\\", gamesList[selected].exeName));
                    }
                    else
                    {
                        startGame(Directory.GetFiles(dirs[selected], "*.exe")[0]); //take first exe you find
                    }
                }
                catch (Exception)
                {
                    selected = 0;
                    populateSelectedGame();
                }
            }
        }

        void startGame(string path) //start game from path, update game info accordingly
        {
            runningProcess = Process.Start(path);
            gamesList[selected].plays += 1;
            writeGamesJson();
            updatePlays();

            timeStarted = DateTime.Now;

            runningProcess.EnableRaisingEvents = true;
            runningProcess.Exited += new EventHandler(handleGameExit);
        }

        void handleGameExit(object sender, EventArgs e) //system listener for game exiting
        {
            runningProcess = null;
            updateGamePlayedTime();
        }
        
        void updateGamePlayedTime() //on game close, update the time, save it to file, and queue a screen update
        {
            TimeSpan total = DateTime.Now.Subtract(timeStarted);
            gamesList[selected].time += total.Minutes + 1;
            writeGamesJson();
            shouldUpdate = true; //have to queue because handleGameExit can call this which is on a different thread
        }

        void writeGamesJson() //manually serialize gamesList back to a json file, done strictly for prettier formatting in the json file
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

        void populateSelectedGame() //writes the games list to reflect selected game, and updates elements on the screen accordingly
        {
            string listText = "";
            for (int i = 0; i < gamesList.Count; i++)
            {
                if (i == selected)
                {
                    listText += "> " + gamesList[i].name + " <\n";
                } else
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
            
            updatePlays();
        }

        void updatePlays() //sets the lblPlays text to "Plays: x       Time Played: hh:mm"
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

        private void tmrBlink_Tick(object sender, EventArgs e) //blinks the "Start Game" label, or changes it to "Loading..." if a game is in progress. Also queues a screen update for weird thread cases.
        {
            if (runningProcess != null)
            {
                lblPressStart.Visible = true;
                lblPressStart.Text = "Loading Game...";
            } else
            {
                lblPressStart.Text = "Press Start";
                lblPressStart.Visible = !lblPressStart.Visible;
            }

            if (shouldUpdate)
            {
                populateSelectedGame();
                shouldUpdate = false;
            }
        }

        private void tmrController_Tick(object sender, EventArgs e) //every tick it polls the controllers for input. Does some kinda hacky stuff with "previous stick position" that gets the job done for navigation
        {
                
            bool didUp = false;
            bool didDown = false;
            bool didStart = false;
            for (int i = 0; i < controllers.Count; i++)
            {
                controllers[i].Update();
                controllers[i].setUDLR();

                if (controllers[i].A || controllers[i].Start)
                    didStart = true;
                if (controllers[i].Up)
                    didUp = true;
                if (controllers[i].Down)
                    didDown = true;
                controllers[i].LeftStickXPrev = controllers[i].LeftStick.Position.X;
                controllers[i].LeftStickYPrev = controllers[i].LeftStick.Position.Y;
            }

            if (didUp)
                eventUp();
            if (didDown)
                eventDown();
            if (didStart)
                eventStart();
        }
        //VOLUME STUFF
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
        //END VOLUME STUFF

        private static void KillProcessAndChildrens(int pid) //recursively kills any and all processes started by the game we launched, guaranteeing (I hope) its death
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
    }




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
}
