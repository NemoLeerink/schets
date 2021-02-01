using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Resources;
using System.IO;

namespace SchetsEditor
{
    public class SchetsWin : Form
    {   
        MenuStrip menuStrip;
        SchetsControl schetscontrol;
        ISchetsTool huidigeTool;
        Panel paneel;
        bool vast;
        ResourceManager resourcemanager
            = new ResourceManager("SchetsEditor.Properties.Resources"
                                 , Assembly.GetExecutingAssembly()
                                 );

        public static ISchetsTool[] deTools = { new PenTool()
                                    , new LijnTool()
                                    , new RechthoekTool()
                                    , new VolRechthoekTool()
                                    , new OvaalTool()
                                    , new VolOvaalTool()
                                    , new TekstTool()
                                    , new GumTool()
                                    };

        private void veranderAfmeting(object o, EventArgs ea)
        {
            schetscontrol.Size = new Size ( this.ClientSize.Width  - 70
                                          , this.ClientSize.Height - 50);
            paneel.Location = new Point(64, this.ClientSize.Height - 30);
        }

        private void klikToolMenu(object obj, EventArgs ea)
        {
            this.huidigeTool = (ISchetsTool)((ToolStripMenuItem)obj).Tag;
        }

        private void klikToolButton(object obj, EventArgs ea)
        {
            this.huidigeTool = (ISchetsTool)((RadioButton)obj).Tag;
        }

        private void afsluiten(object obj, EventArgs ea)
        {
            this.Close();
        }
        private void opslaan(object obj, EventArgs ea)
        {

            String fileName = "";
            
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Text file|*.txt";
            saveFileDialog1.Title = "Save an Text File";
            saveFileDialog1.ShowDialog();
   
            if (saveFileDialog1.FileName != "")
            {
                fileName = saveFileDialog1.FileName;
            }
   
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    
                    foreach (TekenElement el in this.schetscontrol.elementen)
                    {
                        sw.WriteLine(el.ToString());
                        Console.WriteLine(el.ToString());
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void openen(object obj, EventArgs ea)
        {
            string pad = "";
            try
            {
                OpenFileDialog bestand = new OpenFileDialog();
                if (bestand.ShowDialog() == DialogResult.OK)
                {
                    pad = bestand.FileName;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            try
            {
                this.schetscontrol.elementen.Clear();
                StreamReader sr = new StreamReader(pad);
                string regel;
                
                while ((regel = sr.ReadLine()) != null)
                {
                        this.schetscontrol.maakNieuwElement(regel);
                }
                sr.Close();
  
                this.schetscontrol.tekenOpGr();
                this.schetscontrol.Invalidate();
                
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.ToString());
            }
        }
        

        public SchetsWin()
        {

            String[] deKleuren = { "Black", "Red", "Green", "Blue"
                                 , "Yellow", "Magenta", "Cyan", "White"
                                 };

            this.ClientSize = new Size(700, 500);
            huidigeTool = deTools[0];

            schetscontrol = new SchetsControl();
            schetscontrol.Location = new Point(64, 10);
            schetscontrol.MouseDown += (object o, MouseEventArgs mea) =>
                                       {   vast=true;  
                                           huidigeTool.MuisVast(schetscontrol, mea.Location);
                                           
                                       };
            schetscontrol.MouseMove += (object o, MouseEventArgs mea) =>
                                       {   if (vast)
                                           huidigeTool.MuisDrag(schetscontrol, mea.Location); 
                                       };
            schetscontrol.MouseUp   += (object o, MouseEventArgs mea) =>
                                       {   if (vast)
   
                                           huidigeTool.MuisLos (schetscontrol, mea.Location, huidigeTool.ToString());
                                           vast = false;
                                          
                                       };
            schetscontrol.KeyPress +=  (object o, KeyPressEventArgs kpea) => 
                                       {   
                                           huidigeTool.Letter  (schetscontrol, kpea.KeyChar, huidigeTool.ToString());
                                       };
            this.Controls.Add(schetscontrol);

            menuStrip = new MenuStrip();
            menuStrip.Visible = false;
            this.Controls.Add(menuStrip);
            this.maakFileMenu();
            this.maakToolMenu(deTools);
            this.maakAktieMenu(deKleuren);
            this.maakToolButtons(deTools);
            this.maakAktieButtons(deKleuren);
            this.Resize += this.veranderAfmeting;
            this.veranderAfmeting(null, null);
        } 
        
        private void maakFileMenu()
        {   
            ToolStripMenuItem menu = new ToolStripMenuItem("File");
            menu.MergeAction = MergeAction.MatchOnly;
            menu.DropDownItems.Add("Openen", null, this.openen);
            menu.DropDownItems.Add("Opslaan", null, this.opslaan);
            menu.DropDownItems.Add("Sluiten", null, this.afsluiten);
            menuStrip.Items.Add(menu);
        }

        private void maakToolMenu(ICollection<ISchetsTool> tools)
        {   
            ToolStripMenuItem menu = new ToolStripMenuItem("Tool");
            foreach (ISchetsTool tool in tools)
            {   ToolStripItem item = new ToolStripMenuItem();
                item.Tag = tool;
                item.Text = tool.ToString();
                item.Image = (Image)resourcemanager.GetObject(tool.ToString());
                item.Click += this.klikToolMenu;
                menu.DropDownItems.Add(item);
            }
            menuStrip.Items.Add(menu);
        }

        private void maakAktieMenu(String[] kleuren)
        {   
            ToolStripMenuItem menu = new ToolStripMenuItem("Aktie");
            menu.DropDownItems.Add("Clear", null, schetscontrol.Schoon );
            ToolStripMenuItem submenu = new ToolStripMenuItem("Kies kleur");
            foreach (string k in kleuren)
                submenu.DropDownItems.Add(k, null, schetscontrol.VeranderKleurViaMenu);
            menu.DropDownItems.Add(submenu);
            menuStrip.Items.Add(menu);
        }

        private void maakToolButtons(ICollection<ISchetsTool> tools)
        {
            int t = 0;
            foreach (ISchetsTool tool in tools)
            {
                RadioButton b = new RadioButton();
                b.Appearance = Appearance.Button;
                b.Size = new Size(45, 62);
                b.Location = new Point(10, 10 + t * 62);
                b.Tag = tool;
                b.Text = tool.ToString();
                b.Image = (Image)resourcemanager.GetObject(tool.ToString());
                b.TextAlign = ContentAlignment.TopCenter;
                b.ImageAlign = ContentAlignment.BottomCenter;
                b.Click += this.klikToolButton;
                this.Controls.Add(b);
                if (t == 0) b.Select();
                t++;
            }
        }

        private void maakAktieButtons(String[] kleuren)
        {   
            paneel = new Panel();
            paneel.Size = new Size(600, 24);
            this.Controls.Add(paneel);
            
            Button b; Label l; ComboBox cbb; Label l2; NumericUpDown n;
            b = new Button(); 
            b.Text = "Clear";  
            b.Location = new Point(  0, 0); 
            b.Click += schetscontrol.Schoon; 
            paneel.Controls.Add(b);
            
            l = new Label();  
            l.Text = "Penkleur:"; 
            l.Location = new Point(120, 3); 
            l.AutoSize = true;               
            paneel.Controls.Add(l);
            
            cbb = new ComboBox();
            cbb.Location = new Point(180, 0); 
            cbb.DropDownStyle = ComboBoxStyle.DropDownList; 
            cbb.SelectedValueChanged += schetscontrol.VeranderKleur;
            foreach (string k in kleuren)
                cbb.Items.Add(k);
            cbb.SelectedIndex = 0;
            paneel.Controls.Add(cbb);

            l2 = new Label();
            l2.Text = "Pendikte:";
            l2.Location = new Point(320, 3);
            l2.AutoSize = true;
            paneel.Controls.Add(l2);

            n = new NumericUpDown();
            n.Location = new Point(400, 0);
            n.Minimum = 1;
            n.Maximum = 20;
            n.Value = 3;
            n.ValueChanged += schetscontrol.VeranderDikte;
            paneel.Controls.Add(n);
        }
    }
}