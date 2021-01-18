using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Resources;

namespace SchetsEditor
{
    public class SchetsWin : Form
    {   
        MenuStrip menuStrip;
        List<TekenElement> elementen = new List<TekenElement>();
        SchetsControl schetscontrol;
        ISchetsTool huidigeTool;
        Panel paneel;
        bool vast;
        ResourceManager resourcemanager
            = new ResourceManager("SchetsEditor.Properties.Resources"
                                 , Assembly.GetExecutingAssembly()
                                 );

        ISchetsTool[] deTools = { new PenTool()
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

        public SchetsWin()
        {
            Point beginpunt = new Point(0, 0);
            Point eindpunt;

            
            String[] deKleuren = { "Black", "Red", "Green", "Blue"
                                 , "Yellow", "Magenta", "Cyan" 
                                 };

            this.ClientSize = new Size(700, 500);
            huidigeTool = deTools[0];

            schetscontrol = new SchetsControl();
            schetscontrol.Location = new Point(64, 10);
            schetscontrol.MouseDown += (object o, MouseEventArgs mea) =>
                                       {   vast=true;  
                                           // Pas aanroepen als het uitkomt
                                           //huidigeTool.MuisVast(schetscontrol, mea.Location);

                                           // mea.location is startpunt
                                           beginpunt = new Point(mea.Location.X, mea.Location.Y);
                                           
                                           
                                       };
            schetscontrol.MouseMove += (object o, MouseEventArgs mea) =>
                                       {   if (vast)
                                           huidigeTool.MuisDrag(schetscontrol, mea.Location); 
                                       };
            schetscontrol.MouseUp   += (object o, MouseEventArgs mea) =>
                                       {   if (vast)
                                           // pas aanroepen wanneer het uitkomt
                                           //huidigeTool.MuisLos (schetscontrol, mea.Location);
                                           vast = false;

                                           // mea.location is eindpunt
                                           // maak nieuw element aan
                                           if (huidigeTool.ToString() != "tekst")
                                           {
                                               eindpunt = new Point(mea.Location.X, mea.Location.Y);
                                               //Console.WriteLine("Beginpunt:" + beginpunt);
                                               maakNieuwElement(schetscontrol.PenKleur, beginpunt, eindpunt, (char)0, huidigeTool.ToString());

                                           }

                                           // Methode die elementen in lijst tekent 
                                           zetOpBitmap();
                                       };
            schetscontrol.KeyPress +=  (object o, KeyPressEventArgs kpea) => 
                                       {   
                                           // pas aanroepen wanneer het uitkomt
                                           // huidigeTool.Letter  (schetscontrol, kpea.KeyChar);

                                           // je typt, beginpunt = beginpunt. Nieuwe beginpunt wordt 40 op de x hoger. Eindpunt niet perse nodig
       
                                           eindpunt = new Point(beginpunt.X + 40, beginpunt.Y);
                                           maakNieuwElement(schetscontrol.PenKleur, beginpunt, eindpunt, kpea.KeyChar, huidigeTool.ToString());
                                           beginpunt.X += 40;

                                           zetOpBitmap();
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

        private void zetOpBitmap() {
            Console.WriteLine("Aantal elementen is: " + elementen.Count);
            foreach (TekenElement el in elementen) {
                // Selecteer de juiste kleur
                schetscontrol.PenKleur = el.kleur;
                //Console.WriteLine(schetscontrol.PenKleur);


                // selecteer aan de hand van soort property de juiste tool
                selectTool(el.soort);
                //Console.WriteLine(huidigeTool);
                
                huidigeTool.MuisVast(schetscontrol, el.beginpunt);

                if (el.soort == "tekst") {
                    huidigeTool.Letter(schetscontrol, el.tekst);
                }
                // else toevoegen?
                huidigeTool.MuisLos(schetscontrol, el.eindpunt);

            }
           
        }

        private void selectTool(String soort)
        {
            List<string> soortenlist = new List<string>(new string[] { "pen", "lijn", "kader", "vlak", "ovaal", "ovaal vol", "tekst", "gum" });
            int index = soortenlist.IndexOf(soort);
            huidigeTool = deTools[index];
        }

        private void maakNieuwElement(Color kleur, Point p1, Point p2, Char tekst, String soort) 
        {
            TekenElement element = new TekenElement(kleur, p1, p2, tekst, soort);
            elementen.Add(element);
        }
        
        private void maakFileMenu()
        {   
            ToolStripMenuItem menu = new ToolStripMenuItem("File");
            menu.MergeAction = MergeAction.MatchOnly;
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
            menu.DropDownItems.Add("Roteer", null, schetscontrol.Roteer );
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
            
            Button b; Label l; ComboBox cbb;
            b = new Button(); 
            b.Text = "Clear";  
            b.Location = new Point(  0, 0); 
            b.Click += schetscontrol.Schoon; 
            paneel.Controls.Add(b);
            
            b = new Button(); 
            b.Text = "Rotate"; 
            b.Location = new Point( 80, 0); 
            b.Click += schetscontrol.Roteer; 
            paneel.Controls.Add(b);
            
            l = new Label();  
            l.Text = "Penkleur:"; 
            l.Location = new Point(180, 3); 
            l.AutoSize = true;               
            paneel.Controls.Add(l);
            
            cbb = new ComboBox(); cbb.Location = new Point(240, 0); 
            cbb.DropDownStyle = ComboBoxStyle.DropDownList; 
            cbb.SelectedValueChanged += schetscontrol.VeranderKleur;
            foreach (string k in kleuren)
                cbb.Items.Add(k);
            cbb.SelectedIndex = 0;
            paneel.Controls.Add(cbb);
        }
    }
}
