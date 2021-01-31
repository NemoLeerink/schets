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

        public void maakNieuwElement(String s)
        {
            TekenElement element = new TekenElement(s);
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

        private bool inEllipse(Point location, Point centre, double xRadius, double yRadius)
        {
            if (xRadius <= 0.0 || yRadius <= 0.0)
                return false;
            Point normalized = new Point(location.X - centre.X, location.Y - centre.Y);
            return ((double)(normalized.X * normalized.X) / (xRadius * xRadius)) +
                ((double)(normalized.Y * normalized.Y) / (yRadius * yRadius)) <= 1.0;
        }

        private bool inRectangle(Point location, Point bp, Point ep)
        {
            int minX = Math.Min(bp.X, ep.X);
            int minY = Math.Min(bp.Y, ep.Y);
            int lenX = Math.Abs(bp.X - ep.X);
            int lenY = Math.Abs(bp.Y - ep.Y);
            Rectangle r = new Rectangle(minX, minY, lenX, lenY);
            if (r.Contains(location))
                return true;
            else
                return false;
        }

        private bool closeToLine(Point location, Point bp, Point ep, int marge)
        {
            int minX = Math.Min(bp.X, ep.X);
            int maxX = Math.Max(bp.X, ep.X);
            int minY = Math.Min(bp.Y, ep.Y);
            int maxY = Math.Max(bp.Y, ep.Y);
            double deltax = ep.X - bp.X;
            double deltay = ep.Y - bp.Y;

            if (bp.X == ep.X)
            {
                Rectangle r = new Rectangle(minX-marge, minY, maxX - minX + marge*2, maxY - minY);
                if (r.Contains(location))
                    return true;
            }
            else
            {
                double rc = deltay / deltax;
                double b = rc * bp.X - bp.Y;

                for (int j = minX; j <= maxX; j++)
                {
                    if (location.X - j >= -marge &&
                        location.X - j <= marge &&
                        (location.Y - j * rc + b) >= -marge &&
                        (location.Y - j * rc + b) <= marge)
                        return true;
                }
            }
            return false;
        }

        public void verwijderElement(Point p1) 
        {
            bool stop = false;
            for (int i = elementen.Count - 1; i >= 0; i--)
            {
                if (elementen[i].soort == "lijn" || elementen[i].soort == "pen" && stop == false)
                {
                    if (closeToLine(p1, elementen[i].beginpunt, elementen[i].eindpunt, 5))
                    {
                        elementen.RemoveAt(i);
                        stop = true;
                        break;
                    }
                }
                else if (elementen[i].soort == "ovaal" || elementen[i].soort == "ovaalvol" && stop == false)
                {
                    int xRadius = Math.Abs(elementen[i].eindpunt.X - elementen[i].beginpunt.X) / 2;
                    int yRadius = Math.Abs(elementen[i].eindpunt.Y - elementen[i].beginpunt.Y) / 2;
                    int minX = Math.Min(elementen[i].beginpunt.X, elementen[i].eindpunt.X);
                    int minY = Math.Min(elementen[i].beginpunt.Y, elementen[i].eindpunt.Y);
                    Point centre = new Point(minX + xRadius, minY + yRadius);
                    if (inEllipse(p1, centre, xRadius, yRadius))
                    {
                        elementen.RemoveAt(i);
                        break;
                    }
                }
                else if (stop == false)
                {
                    if (inRectangle(p1, elementen[i].beginpunt, elementen[i].eindpunt))
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

        public TekenElement(String s)
        {
            string[] w;
            char[] separators = { ' ' };

            w = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            if (w.Length == 7)
            {
                kleur = Color.FromName(w[0]);
                beginpunt = new Point(int.Parse(w[1]), int.Parse(w[2]));
                eindpunt = new Point(int.Parse(w[3]), int.Parse(w[4]));
                tekst = char.Parse(w[5]); ;
                soort = w[6];

            }
        }

        public override string ToString()
        {
            return $"{kleur.Name} {beginpunt.X} {beginpunt.Y} {eindpunt.X} {eindpunt.Y} {tekst} {soort}";
        }
    }
}
