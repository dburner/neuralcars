using System;
using System.Drawing;

using OpenTK.Graphics.OpenGL; // Dodal Alex

namespace GeneticCars
{
    class Avto
    {
        enum Podlaga { Cesta, Trava };
        Podlaga podlaga;

        Bitmap image;
        Bitmap pot;
        static Bitmap BackgroundImage;

        public const int Doseg = 80;
        
        static readonly PointF StartingPoint = PlayingGround.StartPlace;
        PointF Position = new PointF(StartingPoint.X, StartingPoint.Y);

        public Point Pozicija { get { return new Point((int)Position.X + image.Width / 2, (int)Position.Y + image.Height / 2); } }

        PointF Velocity = new PointF();

        float angle = 0;
        public float Angle { get { return angle; } }

        float Aceleration = 0;

        public bool risiCrte = false;

        protected int najblizjaTocka = 0;
        protected int steviloPrevozenih = 0;
        protected Color barvaAvtomobila; // Dodal Alex

        protected double cost = 0;
        public double Cost
        {
            get
            {
                return cost;
            }
        }

        public static double CostOfBest = 0;

        public Avto(Color color, bool fill)
        {
            barvaAvtomobila = color; // Dodal Alex
            BackgroundImage = PlayingGround.field;
            //init image
            InitImage(color, fill);
        }

        public virtual void Reset()
        {
            Position = new PointF(StartingPoint.X, StartingPoint.Y);
            Velocity = new PointF();

            angle = 0;
            Aceleration = 0;

            if (risiCrte)
                pot = new Bitmap(BackgroundImage.Width, BackgroundImage.Height);
            else if (pot != null)
            {
                pot.Dispose();
                pot = null;
            }

            najblizjaTocka = 0;
            steviloPrevozenih = 0;

            cost = 0;
        }

        private void InitImage(Color color, bool fill)
        {
            image = new Bitmap(20, 20);

            using (Graphics g = Graphics.FromImage(image))
            {
                Brush brush = new SolidBrush(Color.Black);
                
                if (fill)
                {
                    Brush pen = new SolidBrush(color);
                    g.FillRectangle(pen, 5, 5, 13, 10);
                }
                else
                {
                    Pen pen = new Pen(color);
                    g.DrawRectangle(pen, 5, 5, 13, 10);
                }
                
                g.FillRectangle(brush, 7, 3, 4, 4);
                g.FillRectangle(brush, 13, 3, 4, 4);
                g.FillRectangle(brush, 7, 13, 4, 4);
                g.FillRectangle(brush, 13, 13, 4, 4);
            }
        }

        public void setColor(Color c, bool fill)
        {
            InitImage(c, fill);
        }

        public void SetRisiCrte(bool risicrte)
        {
            risiCrte = risicrte;
        }

        public void PaintOpenGL()
        {

            RotateImage(image, angle);
            Point p = this.Pozicija;

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Translate(p.X - 410, -(p.Y) + 325, -3.5);
            GL.Rotate(-angle, 0, 0, 1);

            GL.Begin(BeginMode.Quads);
            GL.Color3(this.barvaAvtomobila);

            GL.Vertex3(-7, +5, 0);
            GL.Vertex3(+7, +5, 0);
            GL.Vertex3(+7, -5, 0);
            GL.Vertex3(-7, -5, 0);

            GL.End();

            GL.PopMatrix();

            //g.DrawImage(RotateImage(image, angle), Position);

            //if (risiCrte)
            //{
            //    g.DrawImage(pot, new Point(0, 0));
                
            //    const double stInputov = 9;
            //    const double kot = 180 / (stInputov - 1);

            //    //dorisemo crte.
            //    for (int i = 0; i < stInputov; i++)
            //    {
            //        Point Poz = Pozicija;
            //        float a = angle + (float)(90 - (i * kot));

            //        double rad = Functions.DegreeToRadian(a);

            //        Poz.X += (int)(Doseg * Math.Cos(rad));
            //        Poz.Y += (int)(Doseg * Math.Sin(rad));

            //        Pen p = new Pen(Color.LightBlue, 2);
            //        if (i == 4) p = new Pen(Color.HotPink, 2);
            //        g.DrawLine(p, Pozicija, Poz);
            //    }
            //}
        }

        public void Turn(float x)
        {
            if (x == 0) return;

            const float degree = 3;

            angle += x > 0 ? degree : -degree;
        }

        public void Accelerate(float x)
        {
            if (x == 0) Aceleration = 0;
            else
            {
                const float Naprej = 0.4F;
                const float Nazaj = -0.25F;
                Aceleration = x > 0 ? Naprej : Nazaj;
            }
        }

        private Point pointLine(Point p1, Point p2, Point p3)
        {
            System.Diagnostics.Debug.Assert(p1 != p2);

            //double u = (((p3.X - p1.X)  * (p2.X - p1.X)) + ((p3.Y - p1.Y) * (p2.Y - p1.Y))) / (Math.Pow(Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2)), 2));
            double u = (((p3.X - p1.X) * (p2.X - p1.X)) + ((p3.Y - p1.Y) * (p2.Y - p1.Y))) / (Math.Pow(Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y)), 2));

            Point rez = new Point((int)Math.Floor(p1.X + u * (p2.X - p1.X)), (int)Math.Floor(p1.Y + u * (p2.Y - p1.Y)));

            return rez;
        }

        public void GetStatus()
        {
            double raz;
            int nas = najblizjaTocka;

            double bestcost = cost;

            cost = 1000 * steviloPrevozenih;
            double pristeta_pot = 0;

            do
            {
                najblizjaTocka = nas;
                nas = najblizjaTocka + 2 >= PlayingGround.Points.Length ? 0 : najblizjaTocka + 1;
                steviloPrevozenih++;

                Point p = PlayingGround.Points[nas];

                raz = Math.Pow(p.X - Pozicija.X, 2) + Math.Pow(p.Y - Pozicija.Y, 2);

                pristeta_pot = Math.Sqrt(Math.Pow(p.X - PlayingGround.Points[najblizjaTocka].X, 2) + Math.Pow(p.Y - PlayingGround.Points[najblizjaTocka].Y, 2));
                cost += pristeta_pot;

            } while (raz < Math.Pow(Doseg, 2));

            cost -= pristeta_pot;

            steviloPrevozenih--;

            int prejsnaTocka = najblizjaTocka - 1 < 0 ? PlayingGround.Points.Length - 1 : najblizjaTocka - 1;

            Point razd = pointLine(PlayingGround.Points[prejsnaTocka], PlayingGround.Points[nas], Pozicija);

            cost += Math.Sqrt(Math.Pow(razd.X - PlayingGround.Points[prejsnaTocka].X, 2) + Math.Pow(razd.Y - PlayingGround.Points[prejsnaTocka].Y, 2));

            if (cost < bestcost) cost = bestcost;   //Zagotovimo, da se uposteva najbolsi rezultat kar ga je avto kdaj naredil.
            if (cost > CostOfBest) CostOfBest = cost;
        }

        public virtual void Update()
        {
            double rad = Functions.DegreeToRadian(angle);

            DolociPodlago();

            const float TrenjeCesta = 0.9F;
            const float TrenjeTrava = 0.75F;

            Velocity.X += Aceleration * (float)Math.Cos(rad);
            Velocity.X *= podlaga == Podlaga.Cesta ? TrenjeCesta : TrenjeTrava;

            Velocity.Y += Aceleration * (float)Math.Sin(rad);
            Velocity.Y *= podlaga == Podlaga.Cesta ? TrenjeCesta : TrenjeTrava;

            PointF staraP = new PointF(Pozicija.X, Pozicija.Y);

            Position.X += Velocity.X;
            Position.Y += Velocity.Y;

            if (risiCrte)
            {
                using (Graphics g = Graphics.FromImage(pot))
                {
                    g.DrawLine(new Pen(Color.Navy, 1), staraP, Pozicija);
                }
            }

            //Izracunamo se status:
            GetStatus();
        }

        void DolociPodlago()
        {
            int X = (int)Position.X + image.Width / 2;
            int Y = (int)Position.Y + image.Height / 2;

            if ((X < 0) || (X >= BackgroundImage.Width) || (Y < 0) || (Y >= BackgroundImage.Height))
            {
                podlaga = Podlaga.Cesta;
                return;
            }

            if (BackgroundImage.GetPixel(X, Y).ToArgb() != Color.White.ToArgb())
                podlaga = Podlaga.Trava;
            else podlaga = Podlaga.Cesta;
        }

        Bitmap RotateImage(Bitmap b, float angle)
        {
            Bitmap newImage = new Bitmap(b.Width, b.Height);
            
            using (Graphics g = Graphics.FromImage(newImage))
            {
                float x = (float)b.Width / 2;
                float y = (float)b.Height / 2;

                g.TranslateTransform(x, y);
                g.RotateTransform(angle);
                g.TranslateTransform(-x, -y);
                g.DrawImage(b, new Point(0, 0));
            }

            return newImage;
        }
    }
}
