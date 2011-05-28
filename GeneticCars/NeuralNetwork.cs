using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

namespace GeneticCars
{
    class NeuralNetwork : Avto
    {
        #region LegWork

        public class Neuron
        {
            public readonly int stInput;

            public double[] Weight;

            public Neuron(int st)
            {
                stInput = st;

                Weight = new double[stInput + 1];

                for (int i = 0; i < stInput + 1; i++)
                {
                    Weight[i] = (Functions.rand.NextDouble() * 2) - 1;
                }
            }

            public Neuron(string line)
            {
                string[] inputs = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                List<double> list = new List<double>();

                foreach (string str in inputs)
                {
                    list.Add(Convert.ToDouble(str, CultureInfo.InvariantCulture));
                }

                Weight = list.ToArray();
                stInput = Weight.Length - 1;
            }

            public void Write (StreamWriter sw)
            {
                sw.WriteLine("Neuron");

                string line = string.Empty;

                foreach (double w in Weight)
                    line += w.ToString(CultureInfo.InvariantCulture) + " ";

                sw.WriteLine(line);
            }
        }

        public class Layer
        {
            public readonly int stNeuron;
            public Neuron[] Neuron;

            public Layer(int stneuron, int stInput)
            {
                stNeuron = stneuron;

                Neuron = new Neuron[stNeuron];

                for (int i = 0; i < stNeuron; i++)
                {
                    Neuron[i] = new Neuron(stInput);
                }
            }

            public Layer(string input)
            {
                string[] neurons = input.Split(new string[] { "Neuron", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                List<Neuron> list = new List<Neuron>();

                foreach (string str in neurons)
                {
                    list.Add(new Neuron(str.Trim()));
                }

                Neuron = list.ToArray();
                stNeuron = Neuron.Length;
            }

            public void Write(StreamWriter sw)
            {
                sw.WriteLine("Layer");

                foreach (Neuron n in Neuron)
                {
                    n.Write(sw);
                }
            }
        }

        public class Network
        {
            readonly int stInput;
            readonly int stOutput;
            readonly int stHiddenLayers;
            readonly int stNeuronosPerHiddenLayer;

            Layer[] Layers;

            public Network(int input, int output, int HiddenLayers, int NeuronsPerHiddenLayer)
            {
                stInput = input;
                stOutput = output;
                stHiddenLayers = HiddenLayers;
                stNeuronosPerHiddenLayer = NeuronsPerHiddenLayer;

                Layers = new Layer[1 + stHiddenLayers];

                for (int i = 0, st = stInput; i < stHiddenLayers; i++)
                {
                    Layers[i] = new Layer(stNeuronosPerHiddenLayer, st);
                    st = stNeuronosPerHiddenLayer;
                }

                Layers[stHiddenLayers] = new Layer(output, stNeuronosPerHiddenLayer);
            }

            public Network(string input)
            {
                string[] layers = input.Split(new string[] { "Layer" }, StringSplitOptions.RemoveEmptyEntries);

                List<Layer> list = new List<Layer>();

                foreach (string str in layers)
                {
                    list.Add(new Layer(str.Trim()));
                }

                Layers = list.ToArray();

                stInput = Layers[0].Neuron[0].stInput;
                stOutput = Layers[Layers.Length - 1].stNeuron;
                stHiddenLayers = Layers.Length - 1;

                if (stHiddenLayers > 0)
                    stNeuronosPerHiddenLayer = Layers[1].stNeuron;
            }

            public void Write(StreamWriter sw)
            {
                sw.WriteLine("Network");

                foreach (Layer l in Layers)
                {
                    l.Write(sw);
                }
            }

            public double[] Update(double[] inputs)
            {
                double[] outputs = null;

                for (int i = 0; i < stHiddenLayers + 1; i++)
                {
                    if (i > 0) inputs = outputs;

                    outputs = new double[Layers[i].stNeuron];

                    for (int j = 0; j < Layers[i].stNeuron; j++)
                    {
                        double netInput = 0;

                        for (int k = 0; k < Layers[i].Neuron[j].stInput - 1; k++)
                        {
                            netInput += Layers[i].Neuron[j].Weight[k] * inputs[k];
                        }

                        netInput += Layers[i].Neuron[j].Weight[Layers[i].Neuron[j].stInput] * -1;

                        outputs[j] = netInput;
                    }
                }

                return outputs;
            }

            public int NumWeights()
            {
                int rez = 0;

                for (int i = 0; i < Layers.Length; i++)
                {
                    for (int j = 0; j < Layers[i].Neuron.Length; j++)
                    {
                        rez += Layers[i].Neuron[j].Weight.Length;
                    }
                }

                return rez;
            }

            public void PutWeights(double[] weights)
            {
                int stevec = 0;
                for (int i = 0; i < Layers.Length; i++)
                {
                    for (int j = 0; j < Layers[i].Neuron.Length; j++)
                    {
                        for (int k = 0; k < Layers[i].Neuron[j].Weight.Length; k++)
                        {
                            Layers[i].Neuron[j].Weight[k] = weights[stevec++];
                        }
                    }
                }
            }

            public double[] GetWeights()
            {
                ArrayList arr = new ArrayList();

                for (int i = 0; i < Layers.Length; i++)
                {
                    for (int j = 0; j < Layers[i].Neuron.Length; j++)
                    {
                        for (int k = 0; k < Layers[i].Neuron[j].Weight.Length; k++)
                        {
                            arr.Add(Layers[i].Neuron[j].Weight[k]);
                        }
                    }
                }

                return (double[])arr.ToArray(typeof(double));
            }
        }
        
        #endregion

        Bitmap BackgroundImage;
        public Network network;
        
        const int stInputov = 9;
        const int stOutputov = 2;
        const int stHidenLayer = 2;// orig: 2
        const int stNevronovNaHidenLayer = 8;

        public NeuralNetwork() : base(Color.Red, false)
        {
            BackgroundImage = PlayingGround.field;

            network = new Network(stInputov, stOutputov, stHidenLayer, stNevronovNaHidenLayer);
        }

        public double[] GetWeights()
        {
            return network.GetWeights();
        }

        public void PutWeights(double[] weights)
        {
            network.PutWeights(weights);
        }

        public int NumWeights()
        {
            return network.NumWeights();
        }

        public override void Update()
        {
            double[] inputs = new double[stInputov];

            //Inputs so definirani kot oddaljenost avta od ovire.

            double kot = 180 / (stInputov - 1);
            for (int i = 0; i < stInputov; i++)
            {
                float angle = Angle + (float)(90 - (i * kot));
                inputs[i] = GetOddaljenost(angle);
            }

            double[] outputs = network.Update(inputs);

            Turn((float)outputs[0]);
            Accelerate((float)outputs[1]);

            base.Update();
        }

        double GetOddaljenost(float angle)
        {
            //Funkcija vrne oddaljenost ovire od sredine avta.
            double enota = 1;
            double oddaljenost = 0;
            double rad = Functions.DegreeToRadian(angle);

            Point Poz = Pozicija;

            while (AliJeOvira(Poz) && (oddaljenost < Doseg))
            {
                oddaljenost += enota;
                enota *= 2;

                Poz.X += (int)(enota * Math.Cos(rad));
                Poz.Y += (int)(enota * Math.Sin(rad));
            }

            return oddaljenost;
        }

        bool AliJeOvira(Point p)
        {
            if ((p.X < 0) || (p.X >= BackgroundImage.Width) || (p.Y < 0) || (p.Y >= BackgroundImage.Height)) return false;

            if (BackgroundImage.GetPixel(p.X, p.Y).ToArgb() != Color.White.ToArgb())
                return false;
            else return true;
        }
    }
}
