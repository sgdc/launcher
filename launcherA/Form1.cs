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
using System.Management;
using CefSharp;
using CefSharp.WinForms;
using CefSharp.Internals;

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

        const int UP_ID = 5;
        const int DOWN_ID = 6;
        const int START_ID = 7;

        const int REFRESH_ID = 8;

        string[] dirs;


        List<Game> gamesList;

        Process runningProcess;
        DateTime timeStarted;

        int selected;
        List<GamepadState> controllers;

        bool attract = false;

        bool hasRegisteredArrows = false; // has registered hotkeys for the arrows/space/enter
        bool didCloseGame = false;

        int gameDelay = 100; // 100 ticks of no restarting
        int currentDelay = 0;
        List<int> ticksHeldMacro = new List<int>();
        int ticksUntilForceQuit = 63;

        WebIO webIO;
        Config loadedConfig;

        // Executes upon the start of the program
        // Moves the cursor, gets the start time (used to calculate how long a game has been running)
        // checks the /games/ folder and reads through every info.json folder within
        // checks for a _config.json in the same folder as the executable, and then parses it to update the program with that info
        // makes the video player have no UI and play on loop
        // runs PopulateSelectedGame() which updates the title, video, stats, and description to the first selected game
        // sets up some listeners for Function key presses and adds four Gamepads to the controllers List
        public SGDCLauncher() 
        {
            InitializeComponent(); // super()

            // if /games/ exists, load in games
            if (Directory.Exists(Directory.GetCurrentDirectory() + "\\games\\")) 
            {
                dirs = Directory.GetDirectories(Directory.GetCurrentDirectory() + "\\games\\", "*", SearchOption.TopDirectoryOnly); // get each directory in the /games folder
                gamesList = new List<Game>();
                foreach (string dir in dirs)
                {
                    string info = "";
                    try
                    {
                        foreach (string line in File.ReadLines(dir + "\\info.json")) // read in info.json
                        {
                            info += line;
                        }
                        // parse info.json to type Game, and add it to gamesList
                        Game game = JsonConvert.DeserializeObject<Game>(info);

                        // attempt to find .webm video in folder, inject into game definition if found
                        string[] webms = Directory.GetFiles(dir, "*.webm");
                        if (webms.Length > 0)
                        {
                            string[] split = webms[0].Split('\\');

                            // "gameFolderName/videoName.webm"
                            game.foundVideo = split[split.Length - 2] + "/" + split[split.Length - 1];
                        }

                        gamesList.Add(game); 
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

            // Set default config
            Config c = DefaultConfig();

            // Check for config override
            if (File.Exists(Directory.GetCurrentDirectory() + "\\_config.json"))
            {
                // we have a config
                string config = "";
                foreach (string line in File.ReadAllLines(Directory.GetCurrentDirectory() + "\\_config.json"))
                {
                    config += line;
                }
                Console.WriteLine(config);
                c = JsonConvert.DeserializeObject<Config>(config);
            } else
            {
                // if there's no config, write the default to disc
                File.WriteAllText(Directory.GetCurrentDirectory() + "\\_config.json", JsonConvert.SerializeObject(c));
            }
            loadedConfig = c;

            if (c.attractActivate > 1000) // has to be at least 1 second; how long you wait with no input until activating attract mode
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

            if (c.borderless)
            {
                FormBorderStyle = FormBorderStyle.None;
            }

            // webBrowser
            webBrowser.LoadUrlAsync(string.Format("file:// /{0}/_index.html", Directory.GetCurrentDirectory()));
            webIO = new WebIO();
            webIO.logo = "./logo.png";
            webIO.staticVideoPath = "./videos/static.mp4";
            webIO.games = gamesList;
            webIO.pageConfig = c;
            SendWebIO();

            runningProcess = null;
            timeStarted = default(DateTime); // can't use null so...okay

            if (gamesList == null || gamesList.Count <= 0)
            {
                // no games in games folder
                File.WriteAllLines(Directory.GetCurrentDirectory() + "\\error.txt", new string[] { "/games/ folder is empty." });
                MessageBox.Show("/games/ folder is empty");
            }


            PopulateSelectedGame();

            //  Register all inputs (will run regardless of focus)
            RegisterHotKey(this.Handle, KILLGAME_HOTKEY_ID, 0, (int)Keys.F1);
            RegisterHotKey(this.Handle, VOLUMEUP_HOTKEY_ID, 0, (int)Keys.F2);
            RegisterHotKey(this.Handle, VOLUMEDOWN_HOTKEY_ID, 0, (int)Keys.F3);
            RegisterHotKey(this.Handle, VOLUMEMUTE_HOTKEY_ID, 0, (int)Keys.F4);
            RegisterHotKey(this.Handle, REFRESH_ID, 0, (int)Keys.F5);

            RegisterArrows(true);

            WindowState = FormWindowState.Maximized;
            controllers = new List<GamepadState>();
            controllers.Add(new GamepadState(Controller_Wrapper.PlayerIndex.One));
            ticksHeldMacro.Add(0);

            controllers.Add(new GamepadState(Controller_Wrapper.PlayerIndex.Two));
            ticksHeldMacro.Add(0);

            controllers.Add(new GamepadState(Controller_Wrapper.PlayerIndex.Three));
            ticksHeldMacro.Add(0);

            controllers.Add(new GamepadState(Controller_Wrapper.PlayerIndex.Four));
            ticksHeldMacro.Add(0);
        }

        // receive global hotkey event, used to overwrite system reaction to F1-F4
        protected override void WndProc(ref Message m) 
        {
            int kId = m.WParam.ToInt32();
            if (m.Msg == 0x0312) //  Key Press
            {
                if (kId == KILLGAME_HOTKEY_ID) // F1
                {
                    if (runningProcess != null && !runningProcess.HasExited)
                        KillProcessAndChildrens(runningProcess.Id);
                    UpdateGamePlayedTime();
                }
                else if (kId == VOLUMEUP_HOTKEY_ID) // F2
                {
                    VolUp();
                }
                else if (kId == VOLUMEDOWN_HOTKEY_ID) // F3
                {
                    VolDown();
                }
                else if (kId == VOLUMEMUTE_HOTKEY_ID) // F4
                {
                    Mute();
                } else if (kId == REFRESH_ID) // F5
                {
                    //webBrowser.Reload();
                }

                if (runningProcess == null && SGDCLauncher.ActiveForm != null)
                {
                    //  only bother checking for these cases if there's no current process and the the window is in focus
                    //  this ensures they get responded to regardless of what inside the program has the focus
                    //  was having problems with CefSharp stealing inputs without this
                    if (kId == UP_ID)
                    {
                        EventUp();
                    } else if (kId == DOWN_ID)
                    {
                        EventDown(false);
                    } else if (kId == START_ID)
                    {
                        EventStart();
                    }

                }
            }
            base.WndProc(ref m); // call normal process for those keys
        }

        // User Pressed Up
        // Go to the previous entry in the gamesList, set it to selected, and run PopulateSelectedGame
        void EventUp() 
        {
            if (runningProcess == null && gamesList != null)
            {
                ResetAttract();
                selected -= 1;
                if (selected < 0)
                    selected = gamesList.Count - 1;
                // Console.WriteLine(gamesList.Count.ToString());
                // BrowserExecute("Press('Up');");
                PopulateSelectedGame();
            }
        }

        // User Pressed Down
        // Go to the next entry in the gamesList, set it to selected, and run PopulateSelectedGame
        // auto means automatic, and if it's true (because the Attract Mode is what hit down), it wil not reset the Attract Mode timer.
        void EventDown(bool auto)
        {
            if (runningProcess == null && gamesList != null)
            {
                if (!auto)
                    ResetAttract();
                selected += 1;
                if (selected > gamesList.Count - 1)
                    selected = 0;

                PopulateSelectedGame();
            }
        }

        // User wants to load a game.
        // Make sure no game is loading, check that a valid executable file exists (either defined as exeName in info.json or just the first exe in the folder) and run it
        // StartGame() handles tracking the loaded process.
        void EventStart() // pressed start/space/enter
        {
            if (runningProcess == null && currentDelay <= 0 && gamesList != null)
            {
                ResetAttract();
                BrowserExecute("Press('Start');");
                try
                {
                    if (gamesList[selected].exeName != null && gamesList[selected].exeName != "" && gamesList[selected].exeName.Contains("."))
                    {
                        StartGame(System.IO.Path.Combine(dirs[selected] + "\\", gamesList[selected].exeName));
                    }
                    else
                    {
                        StartGame(Directory.GetFiles(dirs[selected], "*.exe")[0]); // take first exe you find
                    }
                }
                catch (Exception)
                {
                    selected = 0;
                    PopulateSelectedGame();
                }
            }
        }

        // Given `path`, launch that game and update stats
        void StartGame(string path) 
        {
            runningProcess = Process.Start(path);
            gamesList[selected].plays += 1;
            WriteGamesJson();

            timeStarted = DateTime.Now;

            runningProcess.EnableRaisingEvents = true;
            runningProcess.Exited += new EventHandler(HandleGameExit); // set HandleGameExit() as the handler for the game being closed.

            webIO.loadedGame = true;
            SendWebIO();
        }

        // system listener for game exiting
        void HandleGameExit(object sender, EventArgs e)
        {
            runningProcess = null;
            currentDelay = gameDelay;
            UpdateGamePlayedTime();

            didCloseGame = true;

            webIO.loadedGame = false;
            SendWebIO();
        }

        // Resets tmrAttractWait -- if tmrAttractWait activates, it sets attract mode to true.
        void ResetAttract()
        {
            attract = false;
            tmrAttractWait.Stop();
            tmrAttractWait.Start();
            Console.WriteLine("Reset attract.");
        }

        // on game close, update the time, save it to file, and queue a screen update
        void UpdateGamePlayedTime() 
        {
            TimeSpan total = DateTime.Now.Subtract(timeStarted);
            gamesList[selected].time += total.Minutes + 1;
            WriteGamesJson();
        }

        // manually serialize gamesList back to a json file, done strictly for prettier formatting in the json file
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
                if (g.exeName != null && g.exeName.Length > 1) // only put it there if it existed
                    s += "\t\"exeName\": \"" + g.exeName + "\",\n";
                if (g.videoName != null && g.videoName.Length > 1) // only put it there if it existed
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

        // writes the games list to reflect selected game, and updates elements on the screen accordingly
        void PopulateSelectedGame() 
        {
            if (gamesList != null && gamesList.Count > 0)
            {
                webIO.selectedGameIndex = selected;
                webIO.currentlySelectedGame = gamesList[selected];
                SendWebIO();
            }
        }

        //  ------------------- VOLUME STUFF
        // looked all of this up online -- this is how you interface with Win32 to perform system events like "Volume Up"
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
        //  --------------- END VOLUME STUFF

        // recursively kills any and all processes started by the game we launched, hopefully guaranteeing its death
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
                //  Process already exited.
            }

            if (processCollection != null)
            {
                foreach (ManagementObject mo in processCollection)
                {
                    KillProcessAndChildrens(Convert.ToInt32(mo["ProcessID"])); // kill child processes(also kills childrens of childrens etc.)
                }
            }
        }

        // every tick poll the controllers for input, and perform actions as necessary
        // set to 16ms / tick
        private void TmrController_Tick(object sender, EventArgs e) 
        {
            BrowserSize();
            bool didUp = false;
            bool didDown = false;
            bool didStart = false;
            for (int i = 0; controllers != null && i < controllers.Count; i++)
            {
                controllers[i].Update();

                // tracks previous and current stick location
                // if previous stick was less than left, and current stick is left, that's the frame they pressed left
                controllers[i].SetUDLR();

                if (controllers[i].A || controllers[i].Start)
                    didStart = true;
                if (controllers[i].Up)
                    didUp = true;
                if (controllers[i].Down)
                    didDown = true;
                controllers[i].LeftStickXPrev = controllers[i].LeftStick.X;
                controllers[i].LeftStickYPrev = controllers[i].LeftStick.Y;
                controllers[i].DPadPrev = controllers[i].DPad;

                if (loadedConfig.closeWithBackAndStart && runningProcess != null && !runningProcess.HasExited)
                {
                    if (controllers[i].Back && controllers[i].Start)
                    {
                        // holding Back + Start Macro
                        ticksHeldMacro[i] += 1;
                        if (ticksHeldMacro[i] > ticksUntilForceQuit)
                        {
                            Console.Write("Do Close from Macro.");
                            ticksHeldMacro[i] = 0;
                            KillProcessAndChildrens(runningProcess.Id);
                            UpdateGamePlayedTime();
                        }
                    } else if (ticksHeldMacro[i] > 0)
                    {
                        ticksHeldMacro[i] -= 1;
                    }
                }
            }

            if (didUp)
                EventUp();
            if (didDown)
                EventDown(false);
            if (didStart)
                EventStart();

            // hijacking this to also reduce delay -- as long as currentDelay is > 0, a new game can't be started.
            // This was done to prevent accidental game starts the instant a game was closed. It's set to 100 ticks by default.
            if (currentDelay > 0)
                currentDelay--;

            if (webIO != null && webIO.pageConfig != null && webIO.pageConfig.lockMouse)
                Cursor.Position = new Point(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            // Un-register and re-register arrow keys/enter/space when window does/does not have focus
            // sends true when focused, false otherwise
            RegisterArrows(SGDCLauncher.ActiveForm != null);

            if (didCloseGame)
            {
                didCloseGame = false;
                this.TopMost = true;
                this.TopMost = false;
                this.Activate();
            }
        }

        // if attract mode is enabled, this will scroll through the games list
        private void TmrAttract_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("Got attract tick");
            if (attract && (runningProcess == null || runningProcess.HasExited))
            {
                EventDown(true); // manually "scroll down" once every tick
            }
        }

        // if attract mode isn't enabled, this timer ticks towards executing this event (gets reset to zero upon user action)
        // if this event is executed, attract mode gets enabled
        private void TmrAttractWait_Tick(object sender, EventArgs e)
        {
            attract = true;
            Console.WriteLine("Setting Attract to True.");
        }

        public void BrowserExecute(string scr)
        {
            if (!webBrowser.CanExecuteJavascriptInMainFrame)
                webBrowser.ExecuteScriptAsyncWhenPageLoaded(scr);
            else
                webBrowser.ExecuteScriptAsync(scr);
        }

        public void SendDataToBrowser(string data)
        {
            BrowserExecute("ReceiveData(" + data + ");");
        }

        public void SendWebIO()
        {
            SendDataToBrowser(JsonConvert.SerializeObject(webIO));
        }

        public void BrowserSize()
        {
            if (SGDCLauncher.ActiveForm != null)
            {
                if (loadedConfig.borderless)
                {
                    webBrowser.Width = SGDCLauncher.ActiveForm.Width;
                    webBrowser.Height = SGDCLauncher.ActiveForm.Height;
                } else
                {
                    webBrowser.Width = SGDCLauncher.ActiveForm.Width - 15;
                    webBrowser.Height = SGDCLauncher.ActiveForm.Height - 15;
                }
            }
        }

        public Config DefaultConfig()
        {
            Config c = new Config();
            c.message = "The Jacobus Arcade Machine is presented by the Stevens Game Development Club.\n\nIf this machine is malfunctioning, or you have any questions, comments, or concerns please reach out to us at sgdc@stevens.edu\n\nLearn more about what we do at http:// sgdc.dev";
            c.subtitle = "Jacobus Arcade Machine";
            c.attractActivate = 45000;
            c.attractScroll = 10000;
            c.startBlink = 450;
            c.borderless = false;
            c.lockMouse = false;
            c.closeWithBackAndStart = true;

            return c;
        }

        public void RegisterArrows(bool reg)
        {
            if (reg && !hasRegisteredArrows)
            {
                RegisterHotKey(this.Handle, UP_ID, 0, (int)Keys.Up);
                RegisterHotKey(this.Handle, DOWN_ID, 0, (int)Keys.Down);
                RegisterHotKey(this.Handle, START_ID, 0, (int)Keys.Enter);
                RegisterHotKey(this.Handle, START_ID, 0, (int)Keys.Space);
                hasRegisteredArrows = true;
            } else if (!reg && hasRegisteredArrows)
            {
                UnregisterHotKey(this.Handle, UP_ID);
                UnregisterHotKey(this.Handle, DOWN_ID);
                UnregisterHotKey(this.Handle, START_ID);
                hasRegisteredArrows = false;
            }
        }
    }

    // json class definitions
    public class Game
    {
        public string name { get; set; }
        public string description { get; set; }
        public string exeName { get; set; }
        public string foundVideo { get; set; }
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
        public bool borderless { get; set; }
        public bool lockMouse { get; set; }

        // Support a macro for holding Back+Start for 1 second with a game open to force close the game
        // Defaults to true
        public bool closeWithBackAndStart { get; set; }
    }

    public class WebIO
    {
        public Game currentlySelectedGame { get; set; }
        public string selectedGameVideo { get; set; }
        public List<Game> games { get; set; }
        public Config pageConfig { get; set; }
        public string logo { get; set; }
        public int selectedGameIndex { get; set; }
        public string staticVideoPath { get; set; }
        public bool loadedGame { get; set; }
    }
}
