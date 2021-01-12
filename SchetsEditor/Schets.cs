using System;
using System.Collections.Generic;
using System.Drawing;

namespace SchetsEditor
{
    public class Schets
    {
        private Bitmap bitmap;
        private List<TekenElement> elementen;

        public Schets()
        {
            bitmap = new Bitmap(1, 1);
            elementen = new List<TekenElement>();
        }
        public Graphics BitmapGraphics
        {
            get { return Graphics.FromImage(bitmap); }
        }
        public List<TekenElement> TekenElementen
        {
            get { return elementen ; }
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
    }
    public class TekenElement 
    {
        // (soort, beginpunt, eindpunt, kleur, eventuele tekst)
        
        public TekenElement(Color elementKleur, Point elementBeginpunt, Point elementEindpunt, String elementTekst, String elementSoort) 
        {
            Color kleur = elementKleur;
            Point beginpunt = elementBeginpunt;
            Point eindpunt = elementEindpunt;
            String tekst = elementTekst;
            String soort = elementSoort;

            Console.WriteLine(kleur);
            Console.WriteLine(beginpunt);
            Console.WriteLine(eindpunt);
            Console.WriteLine(tekst);
            Console.WriteLine(soort);
        }
    }
}
