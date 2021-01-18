using System;
using System.Collections.Generic;
using System.Drawing;

namespace SchetsEditor
{
    public class Schets
    {
        private Bitmap bitmap;
        public List<TekenElement> elementen = new List<TekenElement>();

        public Schets()
        {
            bitmap = new Bitmap(1, 1);
        }
        public Graphics BitmapGraphics
        {
            get { return Graphics.FromImage(bitmap); }
        }

        public void VeranderAfmeting(Size sz)
        {
            if (sz.Width > bitmap.Size.Width || sz.Height > bitmap.Size.Height)
            {
                Bitmap nieuw = new Bitmap( Math.Max(sz.Width,  bitmap.Size.Width)
                                         , Math.Max(sz.Height, bitmap.Size.Height)
                                         );
                Graphics gr = Graphics.FromImage(nieuw);
                gr.FillRectangle(Brushes.White, 0, 0, sz.Width, sz.Height);
                gr.DrawImage(bitmap, 0, 0);
                bitmap = nieuw;
            }
        }
        public void Teken(Graphics gr)
        {
            gr.DrawImage(bitmap, 0, 0);
        }
        public void Schoon()
        {
            Graphics gr = Graphics.FromImage(bitmap);
            gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
        }
        public void Roteer()
        {
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
        }
        public void maakNieuwElement(Color kleur, Point p1, Point p2, Char tekst, String soort)
        {
            TekenElement element = new TekenElement(kleur, p1, p2, tekst, soort);
            elementen.Add(element);
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

            /*Console.WriteLine(kleur);
            Console.WriteLine(beginpunt);
            Console.WriteLine(eindpunt);
            Console.WriteLine(tekst);
            Console.WriteLine(soort);*/
        }

        /*  public Bitmap MaakBitmap() {
              //

              Bitmap bitmap = new Bitmap(1,1);
              return new Bitmap(0,0);
          }*/
    }
}
