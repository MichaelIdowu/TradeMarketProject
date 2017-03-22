//Written by: Michael A. Idowu.
//Date: March, 2017.
//All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockUtility;

namespace TradeMarket
{
   

    class Program
    {
        static void Main(string[] args)
        {
            StockRegister register = new StockRegister();
            register.AddTransaction(new string[] {"foo", "B", "0.50", "SGP", "01 Jan 2016", "02 Jan 2016", "200", "100.25" });
            register.AddTransaction(new string[] {"foo", "S", "0.22", "AED", "05 Jan 2016", "07 Jan 2016", "450", "150.5" });
            register.AddTransaction(new string[] { "bar", "B", "0.21", "AED", "05 Jan 2016", "09 Jan 2016", "50", "300.5" });
            register.AddTransaction(new string[] { "foo", "S", "0.45", "SAR", "01 Jan 2016", "09 Jan 2016", "210", "600.25" });
            register.AddTransaction(new string[] { "foo", "S", "0.51", "SGP", "05 Jan 2016", "15 Jan 2016", "400", "450.5" });

            register.AddTransaction(new string[] { "bar", "S", "0.10", "BGA", "05 Jan 2016", "15 Jan 2016", "100", "550.5" });
            register.AddTransaction(new string[] { "cag", "B", "0.15", "SAR", "01 Jan 2016", "15 Jan 2016", "150", "733.25" });
            register.AddTransaction(new string[] { "def", "S", "0.23", "AED", "05 Jan 2016", "15 Jan 2016", "320", "256.5" });

            register.AddTransaction(new string[] { "gem", "B", "0.23", "USD", "05 Jan 2017", "07 Jan 2016", "290", "222.5" });

            string eFilename = "EntitiesReport.txt";
            string dFilename = "DatesReport.txt";

            register.RunEntitiesReport(eFilename);
            register.RunDatesReport(dFilename);

            string eFileText = System.IO.File.ReadAllText(eFilename);
            string dFileText = System.IO.File.ReadAllText(dFilename);
            // Display its content to the console all at once.
            Console.WriteLine("Entities report ({0}) contains: \n\n {1}", eFilename, eFileText);            
            Console.WriteLine("\n\n");
            Console.WriteLine("Market report by dates ({0}) contains: \n\n {1}", dFilename, dFileText);

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
