//Written by: Michael A. Idowu.
//Date: March, 2017.
//All rights reserved.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StockUtility;

//Note: the AAA (Arrange, Act, Assert) pattern is used in writing unit tests
namespace StockUtility.Test
{
    [TestClass]
    public class StockUtilityTest
    {


        [TestMethod]
        public void Test_AddTransactions()
        {
            //Arrange
            StockRegister register = new StockRegister();

            //Act
            register.AddTransaction(new string[] { "foo", "B", "0.50", "SGP", "01 Jan 2016", "02 Jan 2016", "200", "100.25" });
            register.AddTransaction(new string[] { "bar", "B", "0.22", "AED", "05 Jan 2016", "09 Jan 2016", "50", "350.5" });

            //Assert
            Assert.AreEqual(true, register.RankingRequired); //"true" when transaction is successfully inserted          
        }

        [TestMethod]
        public void Test_NoOfDistinctClients()
        {
            //Arrange
            StockRegister register = new StockRegister();

            //Act
            register.AddTransaction(new string[] { "foo", "B", "0.50", "SGP", "01 Jan 2016", "02 Jan 2016", "200", "100.25" });
            register.AddTransaction(new string[] { "foo", "S", "0.22", "AED", "05 Jan 2016", "07 Jan 2016", "450", "150.5" });
            register.AddTransaction(new string[] { "bar", "B", "0.22", "AED", "05 Jan 2016", "09 Jan 2016", "50", "350.5" });
            register.AddTransaction(new string[] { "foo", "S", "0.50", "SAR", "01 Jan 2016", "09 Jan 2016", "210", "600.25" });
            register.AddTransaction(new string[] { "foo", "S", "0.22", "SGP", "05 Jan 2016", "15 Jan 2016", "400", "450.5" });

            register.AddTransaction(new string[] { "bar", "S", "0.22", "BGA", "05 Jan 2016", "15 Jan 2016", "100", "250.5" });
            register.AddTransaction(new string[] { "cab", "B", "0.50", "SAR", "01 Jan 2016", "15 Jan 2016", "150", "333.25" });
            register.AddTransaction(new string[] { "tas", "S", "0.22", "AED", "05 Jan 2016", "15 Jan 2016", "320", "456.5" });

            //Assert
            Assert.AreEqual(4, register.NoOfClients); //should return the number of distinct clients
        }

        [TestMethod]
        public void Test_AdjustedSettlementDate_SARcurrency()
        {
            //Arrange
            StockRegister register = new StockRegister();

            //Act
            register.AddTransaction(new string[] { "ca", "B", "0.50", "SAR", "01 Jan 2016", "15 Jan 2016", "150", "333.25" });

            //Assert
            Assert.AreEqual("17 Jan 2016", register.GetAdjustedSettlementDate());
            Assert.AreEqual("Sunday", DateTime.Parse(register.GetAdjustedSettlementDate()).DayOfWeek.ToString());

        }

        [TestMethod]
        public void Test_AdjustedSettlementDate_AEDcurrency()
        {
            //Arrange
            StockRegister register = new StockRegister();

            //Act
            register.AddTransaction(new string[] { "bari", "S", "0.22", "AED", "05 Jan 2016", "22 Jan 2016", "320", "456.5" });

            //Assert
            Assert.AreEqual("24 Jan 2016", register.GetAdjustedSettlementDate());
            Assert.AreEqual("Sunday", DateTime.Parse(register.GetAdjustedSettlementDate()).DayOfWeek.ToString());
        }

        [TestMethod]
        public void Test_AdjustedSettlementDate_NonAEDcurr_NonSARcurr()
        {
            //Arrange
            StockRegister register = new StockRegister();

            //Act
            register.AddTransaction(new string[] { "foo", "B", "0.50", "SGP", "01 Jan 2016", "02 Jan 2016", "200", "100.25" });

            //Assert
            Assert.AreEqual("04 Jan 2016", register.GetAdjustedSettlementDate());
            Assert.AreEqual("Monday", DateTime.Parse(register.GetAdjustedSettlementDate()).DayOfWeek.ToString());
        }

        [TestMethod]
        public void Test_RankClients()
        {
            
            //Arrange
            StockRegister register = new StockRegister();

            //Act
            register.AddTransaction(new string[] { "foo", "B", "0.50", "SGP", "01 Jan 2016", "02 Jan 2016", "200", "100.25" });
            register.AddTransaction(new string[] { "foo", "S", "0.22", "AED", "05 Jan 2016", "07 Jan 2016", "450", "150.5" });
            register.AddTransaction(new string[] { "bar", "B", "0.22", "AED", "05 Jan 2016", "09 Jan 2016", "50", "350.5" });
            register.AddTransaction(new string[] { "foo", "S", "0.50", "SAR", "01 Jan 2016", "09 Jan 2016", "210", "600.25" });
            register.AddTransaction(new string[] { "foo", "S", "0.22", "SGP", "05 Jan 2016", "15 Jan 2016", "400", "450.5" });

            register.AddTransaction(new string[] { "bar", "S", "0.22", "BGA", "05 Jan 2016", "15 Jan 2016", "100", "250.5" });
            register.AddTransaction(new string[] { "ca", "B", "0.50", "SAR", "01 Jan 2016", "15 Jan 2016", "150", "333.25" });
            register.AddTransaction(new string[] { "bari", "S", "0.22", "AED", "05 Jan 2016", "15 Jan 2016", "320", "456.5" });
            
            //Assert 
            Assert.AreEqual(1, register.GetClientRanking_S("foo"));
            Assert.AreEqual(2, register.GetClientRanking_B("foo"));
            Assert.AreEqual(4, register.GetClientRanking_S("ca"));
            Assert.AreEqual(1, register.GetClientRanking_B("ca"));
        }

        [TestMethod]
        public void Test_RunReportByClients()
        {

            //Arrange
            StockRegister register = new StockRegister();

            //Act
            register.AddTransaction(new string[] { "foo", "B", "0.50", "SGP", "01 Jan 2016", "02 Jan 2016", "200", "100.25" });
            register.AddTransaction(new string[] { "foo", "S", "0.22", "AED", "05 Jan 2016", "07 Jan 2016", "450", "150.5" });
            register.AddTransaction(new string[] { "bar", "B", "0.22", "AED", "05 Jan 2016", "09 Jan 2016", "50", "350.5" });
            register.AddTransaction(new string[] { "foo", "S", "0.50", "SAR", "01 Jan 2016", "09 Jan 2016", "210", "600.25" });
            register.AddTransaction(new string[] { "foo", "S", "0.22", "SGP", "05 Jan 2016", "15 Jan 2016", "400", "450.5" });

            register.AddTransaction(new string[] { "bar", "S", "0.22", "BGA", "05 Jan 2016", "15 Jan 2016", "100", "250.5" });
            register.AddTransaction(new string[] { "ca", "B", "0.50", "SAR", "01 Jan 2016", "15 Jan 2016", "150", "333.25" });
            register.AddTransaction(new string[] { "bari", "S", "0.22", "AED", "05 Jan 2016", "15 Jan 2016", "320", "456.5" });

            register.RunEntitiesReport("EntitiesReport_TestOutput.txt");

            //Assert
            // Read the "Clients Report" file.
            string text = System.IO.File.ReadAllText("EntitiesReport_TestOutput.txt");
            // Display its content to the console all at once.
            Console.WriteLine("Clients report (EntitiesReport_TestOutput.txt) contains: \n\n {0}", text);
        }

        [TestMethod]
        public void Test_RunReportByDates()
        {

            //Arrange
            StockRegister register = new StockRegister();

            //Act
            register.AddTransaction(new string[] { "foo", "B", "0.50", "SGP", "01 Jan 2016", "02 Jan 2016", "200", "100.25" });
            register.AddTransaction(new string[] { "foo", "S", "0.22", "AED", "05 Jan 2016", "07 Jan 2016", "450", "150.5" });
            register.AddTransaction(new string[] { "bar", "B", "0.22", "AED", "05 Jan 2016", "09 Jan 2016", "50", "350.5" });
            register.AddTransaction(new string[] { "foo", "S", "0.50", "SAR", "01 Jan 2016", "09 Jan 2016", "210", "600.25" });
            register.AddTransaction(new string[] { "foo", "S", "0.22", "SGP", "05 Jan 2016", "15 Jan 2016", "400", "450.5" });

            register.AddTransaction(new string[] { "bar", "S", "0.22", "BGA", "05 Jan 2016", "15 Jan 2016", "100", "250.5" });
            register.AddTransaction(new string[] { "ca", "B", "0.50", "SAR", "01 Jan 2016", "15 Jan 2016", "150", "333.25" });
            register.AddTransaction(new string[] { "bari", "S", "0.22", "AED", "05 Jan 2016", "15 Jan 2016", "320", "456.5" });

            register.RunDatesReport("DatesReport_TestOutput.txt");

            //Assert
            // Read the "Market Report by Dates" file.
            string text = System.IO.File.ReadAllText("DatesReport_TestOutput.txt");
            // Display its content to the console all at once.
            Console.WriteLine("Market report by dates (DatesReport_TestOutput.txt) contains: \n\n {0}", text);
        }
    }
}
