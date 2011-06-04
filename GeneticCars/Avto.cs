using System;
using System.Collections.Generic;
using System.Drawing;

using OpenTK.Graphics.OpenGL;

namespace GeneticCars
{
    class Avto
    {
        enum Podlaga { Cesta, Trava };
        Podlaga podlaga;

        object imageLocker = new object();
        Bitmap image;

        Bitmap pot;

        object backgroundLocker = new object();
        static Bitmap BackgroundImage;

        public const int Doseg = 80;
        
        static readonly PointF StartingPoint = PlayingGround.StartPlace;
        PointF Position = new PointF(StartingPoint.X, StartingPoint.Y);

        public const double Radious = 8;

        object pozLocker = new object();
        public Point Pozicija
        { 
            get 
            {
                lock (pozLocker)
                {
                    lock (imageLocker)
                    {
                        return new Point((int)Position.X + image.Width / 2, (int)Position.Y + image.Height / 2);
                    }
                }
            } 
        }

        PointF Velocity = new PointF();

        float angle = 0;
        public float Angle { get { return angle; } }

        float Aceleration = 0;

        public bool risiCrte = false;

        public int Lap { get; private set; }

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
            barvaAvtomobila = color;
            BackgroundImage = PlayingGround.field;
            //init image
            InitImage(color, fill);
        }

        public void SetStartPoz(int dx, int dy)
        {
            Position.X = PlayingGround.StartPlace.X + dx;
            Position.Y = PlayingGround.StartPlace.Y + dy;
        }

        public virtual void Reset()
        {
            lock (pozLocker)
            {
                Position = new PointF(StartingPoint.X, StartingPoint.Y);
            }

            Velocity = new PointF();

            angle = 0;
            Aceleration = 0;
            Lap = 0;

            if (risiCrte)
            {
                lock (backgroundLocker)
                {
                    pot = new Bitmap(BackgroundImage.Width, BackgroundImage.Height);
                }
            }
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
            lock (imageLocker)
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
            GL.Translate(p.X - 410, -(p.Y) + 325, 0);
            GL.Rotate(-angle, 0, 0, 1);
            GL.Rotate(90, 1, 0, 0);
            GL.Rotate(90, 0, 1, 0);
            GL.Scale(2, 2, 2); // 0.05

            if (this.Player) GL.BindTexture(TextureTarget.Texture2D, MainFrm.meshBlueTex);
            else GL.BindTexture(TextureTarget.Texture2D, MainFrm.meshRedTex);
            GL.Begin(BeginMode.Triangles);

            foreach (Meshomatic.Tri t in MainFrm.avtoModel.Tris)
            {
                foreach (Meshomatic.Point po in t.Points())
                {
                    Meshomatic.Vector3 v = MainFrm.avtoModel.Vertices[po.Vertex];
                    Meshomatic.Vector3 n = MainFrm.avtoModel.Normals[po.Normal];
                    Meshomatic.Vector2 tc = MainFrm.avtoModel.TexCoords[po.TexCoord];
                    GL.Normal3(n.X, n.Y, n.Z);
                    GL.TexCoord2(tc.X, 1 - tc.Y);
                    GL.Vertex3(v.X, v.Y, v.Z);
                }
            }

            GL.End();

            GL.PopMatrix();

            if (this.Player)
            {
                // rotate camera
                GL.PushMatrix();

                GL.MatrixMode(MatrixMode.Projection);
                OpenTK.Matrix4 perspective = OpenTK.Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, 820 / 650, 1, 10000);
                GL.LoadMatrix(ref perspective);

                if (MainFrm.getView() == MainFrm.ViewMode.FirstPerson)
                {
                    GL.Rotate(15, 1, 0, 0);
                    GL.Rotate(-90, 1, 0, 0);
                    GL.Rotate(90 + this.angle, 0, 0, 1);
                    GL.Translate(-p.X + 410, (p.Y) - 325, -30);
                }
                else if (MainFrm.getView() == MainFrm.ViewMode.Top)
                {
                    GL.Translate(0, 0, -1000);
                }
                else if (MainFrm.getView() == MainFrm.ViewMode.TopFollowing)
                {
                    GL.Translate(-p.X + 410, +(p.Y) - 325, -500);
                }
                else if (MainFrm.getView() == MainFrm.ViewMode.TopFollowingRelative)
                {
                    GL.Rotate(90 + this.angle, 0, 0, 1);
                    GL.Translate(-p.X + 410, +(p.Y) - 325, -500);
                }

                GL.PopMatrix();
            }
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
            int prevNajblizjaTocka = najblizjaTocka;

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

            //Določimo v katerem krogu je.
            if ((najblizjaTocka != prevNajblizjaTocka) && (prejsnaTocka == (PlayingGround.Points.Length - 1)) && (najblizjaTocka == 0))
            {
                Lap++;
            }
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

            lock (pozLocker)
            {
                Position.X += Velocity.X;
                Position.Y += Velocity.Y;
            }

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
        
        public void Update(List<Element> ComputerPlayers, Avto Player)
        {
            Update();

            if (CheckCollision(this, (Avto)Player))
            {
                CollisionResponse(this, (Avto)Player);
            }

            foreach (Avto e in ComputerPlayers)
            {
                if (CheckCollision(this, (Avto)e))
                {
                    CollisionResponse(this, (Avto)e);
                    break;
                }
            }

            //Izracunamo se status:
            GetStatus();
        }

        private static bool CheckCollisionX(Avto p1, Avto p2)
        {
            if (p1 == p2)
                return false;

            double razdX = Math.Sqrt(Math.Pow(p1.Pozicija.X - p2.Pozicija.X, 2));
            return razdX < (2 * Radious);
        }

        private static bool CheckCollisionY(Avto p1, Avto p2)
        {
            if (p1 == p2)
                return false;

            double razdY = Math.Sqrt(Math.Pow(p1.Pozicija.Y - p2.Pozicija.Y, 2));
            return razdY < (2 * Radious);
        }

        private static bool CheckCollision(Avto p1, Avto p2)
        {
            if (p1 == p2)
                return false;

            /*if ((p1.CollisionDisabled-- > 0) ||
                (p2.CollisionDisabled-- > 0))
                return false;
            */
            //double razdX = Math.Sqrt(Math.Pow(p1.Pozicija.X - p2.Pozicija.X, 2));
            //double razdY = Math.Sqrt(Math.Pow(p1.Pozicija.Y - p2.Pozicija.Y, 2));

            //return (razdX + razdY) < (2 * Radious);
            return CheckCollisionX(p1, p2) && CheckCollisionY(p1, p2);
        }

        public bool Player = false;

        Random rand = new Random();

        private void CollisionResponse(Avto p1, Avto p2)
        {
            /*
             * Nikakor mi ni uspelo narediti collision reaction, ki bi bil vsaj soliden.
             * Ideja tega je, da se naključno izbere avto, ki bo izrinjen. Izrivamo ga v smeri, ki jo narekuje
             * drug avto, v vsaki dimenziji posebej, dokler se avta ne prekrivata več.
             * 
             * V teoriji se sliši kar ok, a izgleda bolj kot ne crap. Ampak jst nimam ideje kako bi naredil boljše, probal sem
             * 100 različnih stvari!
             */

            Avto rini, umikaj;

            if (rand.NextDouble() > 0.5)
            {
                rini = p1;
                umikaj = p2;
            }
            else
            {
                rini = p2;
                umikaj = p1;
            }

            float faktor = 0.05f;

            while (CheckCollisionX(p1, p2))
            {
                lock (pozLocker)
                {
                    float X =  faktor * rini.Velocity.X;
                    if (X == 0) X = 0.1f * (rini.Position.X - umikaj.Position.X);
                    umikaj.Position.X += X;
                }
            }

            while(CheckCollisionY(p1, p2))
            {
                lock (pozLocker)
                {
                    float Y = faktor * rini.Velocity.Y;
                    if (Y == 0) Y = 0.1f * (rini.Position.Y - umikaj.Position.Y);
                    umikaj.Position.Y += Y;
                }
            }
        }

        void DolociPodlago()
        {
            int X;
            int Y;

            lock (pozLocker)
            {
                lock (imageLocker)
                {
                    X = (int)Position.X + image.Width / 2;
                    Y = (int)Position.Y + image.Height / 2;
                }
            }

            lock (backgroundLocker)
            {
                if ((X < 0) || (X >= BackgroundImage.Width) || (Y < 0) || (Y >= BackgroundImage.Height))
                {
                    podlaga = Podlaga.Cesta;
                    return;
                }

                if (BackgroundImage.GetPixel(X, Y).ToArgb() != Color.White.ToArgb())
                    podlaga = Podlaga.Trava;
                else podlaga = Podlaga.Cesta;
            }
        }

        Bitmap RotateImage(Bitmap b, float angle)
        {
            lock (imageLocker)
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
}
