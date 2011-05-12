using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            using (T03_Immediate_Mode_Cube example = new T03_Immediate_Mode_Cube())
            {
                //Utilities.SetWindowTitle(example);
                example.Run(30.0, 0.0);
            }

        }
    }
}
