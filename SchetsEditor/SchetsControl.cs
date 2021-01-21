using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SchetsEditor
{   public class SchetsControl : UserControl
    {   
        private Schets schets;
        private Color penkleur;
        public List<TekenElement> elementen = new List<TekenElement>();

        ISchetsTool huidigeTool;

        ISchetsTool[] deTools = { new PenTool()
                                    , new LijnTool()
                                    , new RechthoekTool()
                                    , new VolRechthoekTool()
                                    , new OvaalTool()
                                    , new VolOvaalTool()
                                    , new TekstTool()
                                    , new GumTool()
                                    };

        public Color PenKleur
        { get { return penkleur; }
        }
        public Schets Schets
        { get { return schets;   }
        }
        public SchetsControl()
        {   this.BorderStyle = BorderStyle.Fixed3D;
            this.schets = new Schets();
            this.Paint += this.teken;
            this.Resize += this.veranderAfmeting;
            this.veranderAfmeting(null, null);
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }
        private void teken(object o, PaintEventArgs pea)
        {   schets.Teken(pea.Graphics);
        }
        private void veranderAfmeting(object o, EventArgs ea)
        {   schets.VeranderAfmeting(this.ClientSize);
            this.Invalidate();
        }
        public Graphics MaakBitmapGraphics()
        {   Graphics g = schets.BitmapGraphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            return g;
        }

        public void Schoon(object o, EventArgs ea)
        {   schets.Schoon();
            elementen.Clear();
            this.Invalidate();
        }
        public void Roteer(object o, EventArgs ea)
        {   schets.VeranderAfmeting(new Size(this.ClientSize.Height, this.ClientSize.Width));
            schets.Roteer();
            this.Invalidate();
        }
        public void VeranderKleur(object obj, EventArgs ea)
        {   string kleurNaam = ((ComboBox)obj).Text;
            penkleur = Color.FromName(kleurNaam);
        }
        public void VeranderKleurViaMenu(object obj, EventArgs ea)
        {   string kleurNaam = ((ToolStripMenuItem)obj).Text;
            penkleur = Color.FromName(kleurNaam);
        }
        public void maakNieuwElement(Color kleur, Point p1, Point p2, Char tekst, String soort)
        {
            TekenElement element = new TekenElement(kleur, p1, p2, tekst, soort);
            elementen.Add(element);
        }

        private void selectTool(String soort)
        {
            List<string> soortenlist = new List<string>(new string[] { "pen", "lijn", "kader", "vlak", "ovaal", "ovaalvol", "tekst", "gum" });
            int index = soortenlist.IndexOf(soort);
            huidigeTool = deTools[index];
        }

        public void tekenOpGr()
        {
            Graphics gr = MaakBitmapGraphics();
            foreach (TekenElement el in elementen)
            {
                selectTool(el.soort);
                huidigeTool.Teken(gr, el.beginpunt, el.eindpunt, el.kleur, el.tekst);
            }

            this.schets.Teken(gr);
        }
    }
    public class TekenElement
    {
        // (soort, beginpunt, eindpunt, kleur, eventuele tekst)
        public String soort;
        public Color kleur;
        public Point beginpunt;
        public Point eindpunt;
        public Char tekst;

        public TekenElement(Color elementKleur, Point elementBeginpunt, Point elementEindpunt, Char charTekst, String elementSoort)
        {
            kleur = elementKleur;
            beginpunt = elementBeginpunt;
            eindpunt = elementEindpunt;
            tekst = charTekst;
            soort = elementSoort;
        }

    }
}
