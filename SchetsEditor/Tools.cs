using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SchetsEditor
{
    public interface ISchetsTool
    {
        void MuisVast(SchetsControl s, Point p);
        void MuisDrag(SchetsControl s, Point p);
        void MuisLos(SchetsControl s, Point p, String huidigeTool);
        void Letter(SchetsControl s, char c, String huidigeTool);
        void Teken(Graphics g, Point p1, Point p2, Color kleur, Char charTekst);
    }

    public abstract class StartpuntTool : ISchetsTool
    {
        protected Point startpunt;
        protected Brush kwast;

        public virtual void MuisVast(SchetsControl s, Point p)
        {
            startpunt = p;
        }
        public virtual void MuisLos(SchetsControl s, Point p, String huidigeTool)
        {
            kwast = new SolidBrush(s.PenKleur);
        }
        public abstract void MuisDrag(SchetsControl s, Point p);
        public abstract void Letter(SchetsControl s, char c, String huidigeTool);
        public abstract void Teken(Graphics g, Point p1, Point p2, Color kleur, Char charTekst);
    }

    public class TekstTool : StartpuntTool
    {
        public override string ToString() { return "tekst"; }

        public override void MuisDrag(SchetsControl s, Point p) { }

        public override void Letter(SchetsControl s, char c, string huidigeTool)
        {
            Graphics g = s.CreateGraphics();
            Font font = new Font("Tahoma", 40);
            string tekst = c.ToString();

            SizeF sz =
                g.MeasureString(tekst, font, startpunt.X, StringFormat.GenericTypographic);

            s.maakNieuwElement(s.PenKleur, new Point(startpunt.X, startpunt.Y), new Point(startpunt.X + (int)sz.Width, startpunt.Y + (int)sz.Height), c, huidigeTool);

            startpunt.X += (int)sz.Width;

            Console.WriteLine("Count is: " + s.elementen.Count);

            s.tekenOpGr();
            s.Invalidate();
        }
        public override void Teken(Graphics g, Point p1, Point p2, Color kleur, Char c)
        {
            if (c >= 32)
            {
                Font font = new Font("Tahoma", 40);
                string tekst = c.ToString();

                g.DrawString(tekst, font, new SolidBrush(kleur),
                                              p1, StringFormat.GenericTypographic);

            }
        }
    }

    public abstract class TweepuntTool : StartpuntTool
    {
        public static Rectangle Punten2Rechthoek(Point p1, Point p2)
        {
            return new Rectangle(new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y))
                                , new Size(Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y))
                                );
        }
        public static Pen MaakPen(Brush b, int dikte)
        {
            Pen pen = new Pen(b, dikte);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            return pen;
        }
        public override void MuisVast(SchetsControl s, Point p)
        {
            base.MuisVast(s, p);
            kwast = Brushes.Gray;
        }
        public override void MuisDrag(SchetsControl s, Point p)
        {
            s.Refresh();
            this.Bezig(s.CreateGraphics(), this.startpunt, p);
        }
        public override void MuisLos(SchetsControl s, Point p, String huidigeTool)
        {
            base.MuisLos(s, p, huidigeTool);
            if (huidigeTool != "gum")
                s.maakNieuwElement(s.PenKleur, this.startpunt, p, (char)0, huidigeTool);
            else
                s.verwijderElement(this.startpunt);

            Console.WriteLine("Count is: " + s.elementen.Count);

            s.tekenOpGr();      
            s.Invalidate();
        }
        
        public override void Letter(SchetsControl s, char c, String huidigeTool)
        {
        }
        public abstract void Bezig(Graphics g, Point p1, Point p2);

        public virtual void Compleet(Graphics g, Point p1, Point p2)
        {
            this.Bezig(g, p1, p2);
        }
    }

    public class RechthoekTool : TweepuntTool
    {
        public override string ToString() { return "kader"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawRectangle(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }
        public override void Teken(Graphics g, Point p1, Point p2, Color kleur, Char c)
        {
            g.DrawRectangle(MaakPen(new SolidBrush(kleur), 3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }

    public class VolRechthoekTool : RechthoekTool
    {
        public override string ToString() { return "vlak"; }

        public override void Compleet(Graphics g, Point p1, Point p2)
        {
            g.FillRectangle(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
        }

        public override void Teken(Graphics g, Point p1, Point p2, Color kleur, Char c)
        {
            g.FillRectangle(new SolidBrush(kleur), TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }

    /// <summary>
    /// Ovaal en gevulde ovaal toegevoegd
    /// </summary>
    public class OvaalTool : TweepuntTool
    {
        public override string ToString() { return "ovaal"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawEllipse(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }
        public override void Teken(Graphics g, Point p1, Point p2, Color kleur, Char c)
        {
            g.DrawEllipse(MaakPen(new SolidBrush(kleur), 3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }

    public class VolOvaalTool : OvaalTool
    {
        public override string ToString() { return "ovaalvol"; }

        public override void Compleet(Graphics g, Point p1, Point p2)
        {
            g.FillEllipse(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
        }

        public override void Teken(Graphics g, Point p1, Point p2, Color kleur, char c)
        {
            g.FillEllipse(new SolidBrush(kleur), TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }

    public class LijnTool : TweepuntTool
    {
        public override string ToString() { return "lijn"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawLine(MaakPen(this.kwast, 3), p1, p2);
        }

        public override void Teken(Graphics g, Point p1, Point p2, Color kleur, Char c)
        {
            g.DrawLine(new Pen(new SolidBrush(kleur)), p1, p2);
        }
    }

    public class PenTool : LijnTool
    {
        public override string ToString() { return "pen"; }

        public override void MuisDrag(SchetsControl s, Point p)
        {
            // this.MuisLos(s, p);
            this.MuisLos(s, p, "pen");
            this.MuisVast(s, p);
        }
    }

    public class GumTool : TweepuntTool
    {
        public override string ToString() { return "gum"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
        }

        public override void Teken(Graphics g, Point p1, Point p2, Color kleur, Char c)
        {
        }
    }
}
