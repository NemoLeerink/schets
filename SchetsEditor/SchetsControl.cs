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
        {
            schets.Schoon();
            Graphics g = schets.BitmapGraphics;
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
            // Console.WriteLine(soort);
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

        private bool contains(Point location, Point centre, double xRadius, double yRadius)
        {
            if (xRadius <= 0.0 || yRadius <= 0.0)
                return false;
            Point normalized = new Point(location.X - centre.X, location.Y - centre.Y);
            return ((double)(normalized.X * normalized.X) / (xRadius * xRadius)) +
                ((double)(normalized.Y * normalized.Y) / (yRadius * yRadius)) <= 1.0;
        }

        public void verwijderElement(Point p1) 
        {
            for (int i = elementen.Count - 1; i >= 0; i--)
            {
                if (elementen[i].soort == "lijn" || elementen[i].soort == "pen")
                {
                    int mostx;
                    int leastx;
                    if (elementen[i].beginpunt.X > elementen[i].eindpunt.X)
                    {
                        mostx = elementen[i].beginpunt.X;
                        leastx = elementen[i].eindpunt.X;
                    }
                    else
                    {
                        mostx = elementen[i].eindpunt.X;
                        leastx = elementen[i].beginpunt.X;
                    }
                    double deltax = elementen[i].eindpunt.X - elementen[i].beginpunt.X;
                    double deltay = elementen[i].eindpunt.Y - elementen[i].beginpunt.Y;
                    double rc = deltay / deltax;
                    double b = rc * elementen[i].beginpunt.X - elementen[i].beginpunt.Y;
                    
                    for (int j = leastx; j <= mostx; j++)
                    {
                        if (p1.X - j >= -5 &&
                            p1.X - j <= 5 &&
                            (p1.Y - j*rc + b) >= -5 &&
                            (p1.Y - j*rc + b) <= 5)
                        {
                            elementen.RemoveAt(i);
                            break;
                        }
                    }
                }
                else if (elementen[i].soort == "ovaal" || elementen[i].soort == "ovaalvol")
                {
                    int xRadius = Math.Abs(elementen[i].eindpunt.X - elementen[i].beginpunt.X) / 2;
                    int yRadius = Math.Abs(elementen[i].eindpunt.Y - elementen[i].beginpunt.Y) / 2;
                    int minX = Math.Min(elementen[i].beginpunt.X, elementen[i].eindpunt.X);
                    int minY = Math.Min(elementen[i].beginpunt.Y, elementen[i].eindpunt.Y);
                    Point centre = new Point(minX + xRadius, minY + yRadius);
                    if (contains(p1, centre, xRadius, yRadius))
                    {
                        elementen.RemoveAt(i);
                        break;
                    }
                }
                else
                {
                    if (p1.X - elementen[i].beginpunt.X >= 0 &&
                        p1.X - elementen[i].eindpunt.X <= 0 &&
                        p1.Y - elementen[i].beginpunt.Y >= 0 &&
                        p1.Y - elementen[i].eindpunt.Y <= 0)
                    {
                        elementen.RemoveAt(i);
                        break;
                    }
                }
            }
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

        public override string ToString()
        {
            return $"{kleur.Name} {beginpunt.X} {beginpunt.Y} {eindpunt.X} {eindpunt.Y} {tekst} {soort}";
        }
    }
}
