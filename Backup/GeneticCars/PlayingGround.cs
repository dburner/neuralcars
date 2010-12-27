using System;
using System.Collections;
using System.Drawing;
using System.Xml;
using System.Globalization;
using System.Windows.Forms;

namespace GeneticCars
{
    static class PlayingGround
    {
        public static Bitmap field;
        public static Point[] Points;
        public static Point StartPlace;

        public static Bitmap ImportFromSCG()
        {
            string path = string.Format(@"{0}\{1}", Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf(@"\")), "Pot1.svg");

            XmlDocument document = new XmlDocument();
            document.Load(path);

            XmlNodeList list = document.GetElementsByTagName("g");

            string vsebina = list[0].InnerXml;

            string zacetek = vsebina.Substring(vsebina.IndexOf("M ") + 2);

            string[] tocke = zacetek.Split('L');

            Points = new Point[tocke.Length];

            int index = 0;
            foreach (string tocka in tocke)
            {
                string[] st = tocka.Split(',');

                double stevilo;
                if (!Double.TryParse(st[1], out stevilo))
                {
                    st[1] = st[1].Substring(0, st[1].IndexOf('z'));
                }

                Points[index++] = new Point(Convert.ToInt32(Convert.ToDouble(st[0], CultureInfo.InvariantCulture)), Convert.ToInt32(Convert.ToDouble(st[1], CultureInfo.InvariantCulture)));
            }

            StartPlace = Points[0];

            field = new Bitmap(800, 600);

            using (Graphics g = Graphics.FromImage(field))
            {
                g.Clear(Color.Green);
                
                Pen pen = new Pen(Color.White, 70);
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                g.DrawLines(pen, Points);
            }

            return field;
        }

        public static void Paint (Graphics g)
        {
            for (int i = 0; i < Points.Length; i++)
            {
                g.FillEllipse(new SolidBrush(Color.Purple), new Rectangle(Points[i], new Size(20, 20)));
                g.DrawString(i.ToString(), new Font("Times New Roman", 10), new SolidBrush(Color.Yellow), Points[i].X + 2, Points[i].Y + 2);
            }
        }
    }
}
