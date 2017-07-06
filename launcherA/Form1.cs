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

namespace launcherA
{

    public partial class Form1 : Form
    {
        List<Game> gamesList;
        int selected = 0;
        
        public Form1()
        {
            InitializeComponent();
            string games = "";
            foreach (string line in File.ReadLines("games.json"))
            {
                games += line;
            }
            gamesList = JsonConvert.DeserializeObject<List<Game>>(games);
            populateSelectedGame();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                MessageBox.Show("You pressed the F1 key");
                return true;    // indicate that you handled this keystroke
            }
            else if (keyData == Keys.Up)
            {
                selected -= 1;
                if (selected < 0)
                    selected = gamesList.Count - 1;
                populateSelectedGame();
                return true;
            }
            else if (keyData == Keys.Down)
            {
                selected += 1;
                if (selected >= gamesList.Count)
                    selected = 0;
                populateSelectedGame();
                return true;
            }
            else if (keyData == Keys.Enter || keyData == Keys.Space)
            {
                try
                {
                    Process.Start(gamesList[selected].path);
                } catch (Exception)
                {
                    selected = 0;
                    populateSelectedGame();
                }
            }

            // Call the base class
            return base.ProcessCmdKey(ref msg, keyData);
        }

        void populateSelectedGame ()
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
            ((Label)GetControlByName("lblList")).Text = listText;
            ((Label)GetControlByName("lblTitle")).Text = gamesList[selected].name;
            ((Label)GetControlByName("lblDesc")).Text = gamesList[selected].description;
            ((PictureBox)GetControlByName("picGif")).Image = Image.FromFile(gamesList[selected].gifPath);
            ((PictureBox)GetControlByName("picGif")).Refresh();
        }

        Control GetControlByName(string Name)
        {
            foreach (Control c in this.Controls)
                if (c.Name == Name)
                    return c;

            return null;
        }
    }




    public class Game
    {
        public string name { get; set; }
        public string description { get; set; }
        public string path { get; set; }
        public string gifPath { get; set; }
    }
}
