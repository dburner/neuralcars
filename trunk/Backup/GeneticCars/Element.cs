using System;
using System.Drawing;

namespace GeneticCars
{
    class Element : NeuralNetwork
    {
        public int StRazmnozevanj = 0;
        public const int MaxRazmnozevanj = 4;

        public Element()
        {

        }

        public Element(Element el)
        {
            //Kopirni konstruktor
            PutWeights(el.GetWeights());
        }

        public override void Reset()
        {
            StRazmnozevanj = 0;

            base.Reset();
        }

        public void Mutate()
        {
            double[] geni = GetWeights();

            double mutationRate = (double)Functions.rand.Next(1, 30) / 1000;

            for (int i = 0; i < geni.Length; i++)
            {
                if (Functions.rand.NextDouble() < mutationRate)
                {
                    geni[i] = (Functions.rand.NextDouble() * 2) - 1;
                }
            }

            PutWeights(geni);
        }

        public static Element Mate(Element e1, Element e2)
        {
            int length = e1.NumWeights();
            int mesto = (int)Math.Floor(length * Functions.rand.NextDouble());

            double[] geni = new double[length];
            double[] temp = e1.GetWeights();

            for (int i = 0; i < mesto; i++)
            {
                geni[i] = temp[i];
            }

            temp = e2.GetWeights();

            for (int i = mesto; i < length; i++)
            {
                geni[i] = temp[i];
            }

            Element novElement = new Element();
            novElement.PutWeights(geni);

            novElement.Mutate();

            return novElement;
        }
    }
}
