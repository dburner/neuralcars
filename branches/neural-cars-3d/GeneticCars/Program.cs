using System;

namespace GeneticCars
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (MainFrm mainform = new MainFrm())
            {
                mainform.Run(30.0, 0.0);
            }

        }
    }
}
