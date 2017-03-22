//Written by: Michael A. Idowu.
//Date: March, 2017.
//All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using System.IO;

namespace StockUtility
{
    public enum AccountType
    {
        CommonStock,            //(CS) - for the cash sale of stock by , e.g. 
                                // credit (Par value) into the CS account, credit (additional payment if exists) in the PC account; ...
                                // ...  debit cash or assets received to the CA or NC account, respectively.
        PreferredStock,         //(PS)
        TreasuryStock,          // (TS - stockholders' equity), e.g. board of directors elects a company to buy back shares from shareholders - reduces outstanding stock on the open market ...
                                // ... result: debit TS and credit CA accounts
        PaidInCapital,          //(PC) - used for stock accounts
        Cash,                   //(CA) - cash account
        NonCashAssetOrService   //(NC) - for non-cash assets or services received in exchange for stock, based on market value, ...
                                // ... e.g. selling shares to product design firm for services rendered
    }

    public enum AssetOrServiceValueDependency
    {
        TradingMarket,          //determine the market value, if there is a trading market for them
        FairMarket              // assumes there is no trading market 
    }

    public enum StockTransactionType
    {
        Sell, // - sell for cash, assets or services received
        Buy // 
    }

    public enum CurrencyType
    {
        USD,
        GBP,
        SGP,
        AED,
        SAR
    }

    class Transaction
    {
        public string Entity;
        public StockTransactionType TransactionType;
        public double AgreedFx;
        public string Currency;
        public DateTime InstructionDate;
        public DateTime SettlementDate;
        public int Units;
        public double PricePerUnit;

        private int SettlementDateOffset;

        public double TradeAmountUSD { get { return PricePerUnit * Units * AgreedFx; } }
        public bool IsValidSettlementDate { get { return CheckSettlementDate(); } }
        public string EffectiveSettlementDate()
        {
            bool flag = IsValidSettlementDate;
            DateTime dt = SettlementDate.AddDays(SettlementDateOffset);
            return dt.ToString("dd MMM yyyy");
        }

        public bool CheckSettlementDate()
        {
            SettlementDateOffset = 0; //to be added to determine next working day 

            switch (SettlementDate.DayOfWeek)
            {
                case DayOfWeek.Monday:
                case DayOfWeek.Tuesday:
                case DayOfWeek.Wednesday:
                case DayOfWeek.Thursday:
                    return true;
                //break;                    
                case DayOfWeek.Friday:
                    if (!(Currency.Trim().ToUpper().Equals("AED") || Currency.Trim().ToUpper().Equals("SAR")))
                        return true;
                    if (Currency.Trim().ToUpper().Equals("AED") || Currency.Trim().ToUpper().Equals("SAR"))
                        SettlementDateOffset = 2; //to be added to determine next working day
                    break;
                case DayOfWeek.Saturday:
                    if (!(Currency.Trim().ToUpper().Equals("AED") || Currency.Trim().ToUpper().Equals("SAR")))
                        SettlementDateOffset = 2;
                    if (Currency.Trim().ToUpper().Equals("AED") || Currency.Trim().ToUpper().Equals("SAR"))
                        SettlementDateOffset = 1;
                    break;
                case DayOfWeek.Sunday:
                    if (!(Currency.Trim().ToUpper().Equals("AED") || Currency.Trim().ToUpper().Equals("SAR")))
                        SettlementDateOffset = 1;
                    if (Currency.Trim().ToUpper().Equals("AED") || Currency.Trim().ToUpper().Equals("SAR"))
                        return true;
                    break;
                    //default:
                    //return false;
            }
            return false;
        }

        public Transaction(string entity, StockTransactionType transactionType, double agreedFx, string currency,
                      DateTime instructionDate, DateTime settlementDate, int units, double pricePerUnit)
        {
            Entity = entity;
            TransactionType = transactionType;
            AgreedFx = agreedFx;
            Currency = currency;
            InstructionDate = instructionDate;
            SettlementDate = settlementDate;
            Units = units;
            PricePerUnit = pricePerUnit;
        }

        public Transaction(string[] instruction)
        {
            Entity = instruction[0];
            TransactionType = instruction[1].Trim().ToUpper().Equals("B") ? StockTransactionType.Buy : StockTransactionType.Sell;
            AgreedFx = Double.Parse(instruction[2]);
            Currency = instruction[3];
            InstructionDate = DateTime.Parse(instruction[4]);
            SettlementDate = DateTime.Parse(instruction[5]);
            Units = Int32.Parse(instruction[6]);
            PricePerUnit = Double.Parse(instruction[7]);
        }
    }

    class HeaderRecord
    {
        public string Header;
        public double TradeAmountUSD_B;
        public double TradeAmountUSD_S;

        public int RankB;
        public int RankS;

        public List<Transaction> Transactions;

        public HeaderRecord(string header)
        {
            Header = header;
            Transactions = new List<Transaction>();
        }

        public void UpdateTradeAmountUSD()
        {
            TradeAmountUSD_B = 0;
            TradeAmountUSD_S = 0;

            foreach (Transaction tr in Transactions)
            {
                if (tr.TransactionType == StockTransactionType.Buy)
                    TradeAmountUSD_B += tr.TradeAmountUSD;
                else
                    TradeAmountUSD_S += tr.TradeAmountUSD;
            }
        }
    }

    public class StockRegister
    {
        static private Dictionary<string, HeaderRecord> Clients;
        static private Dictionary<string, HeaderRecord> Dates;
        public bool RankingRequired;

        public int NoOfClients { get { return Clients.Count; } }

        public int GetClientRanking_B(string strKey)
        {
            if (RankingRequired) RankClients();
            try
            {
                return Clients[strKey].RankB;
            }
            catch { }
            return 0; //i.e. return "0" for no record found
        }

        public int GetClientRanking_S(string strKey)
        {
            if (RankingRequired) RankClients();

            try
            {
                return Clients[strKey].RankS;
            }
            catch { }
            return 0; //i.e. return "0" for no record found
        }

        public string GetAdjustedSettlementDate()
        {
            string res = "";
            foreach (KeyValuePair<string, HeaderRecord> pair in Dates)
            {
                res = pair.Key; 
            }
            return res; //i.e. for the last transaction
        }

        public StockRegister()
        {
            Clients = new Dictionary<string, HeaderRecord>();
            Dates = new Dictionary<string, HeaderRecord>();
            RankingRequired = false;
        }

        public void AddTransaction(string[] transaction)
        {
            Transaction tr = new Transaction(transaction);
            string strKeyEntity = tr.Entity;
            string strKeyDate = tr.EffectiveSettlementDate(); // for handles settlement date that falls on a weekend

            if (Clients.ContainsKey(strKeyEntity) == false)
            {

                Clients.Add(strKeyEntity, new HeaderRecord(strKeyEntity));
            }
            Clients[strKeyEntity].Transactions.Add(tr); // new Transaction(transaction));
            Clients[strKeyEntity].UpdateTradeAmountUSD();

            if (Dates.ContainsKey(strKeyDate) == false)
            {
                Dates.Add(strKeyDate, new HeaderRecord(strKeyDate));
            }

            Dates[strKeyDate].Transactions.Add(tr);
            Dates[strKeyDate].UpdateTradeAmountUSD();

            RankingRequired = true;
        }

        public void RankClients()
        {
            List<double> arrB = new List<double>(); List<double> arrS = new List<double>();
            foreach (KeyValuePair<string, HeaderRecord> pair in Clients)
            {
                arrB.Add(pair.Value.TradeAmountUSD_B); arrS.Add(pair.Value.TradeAmountUSD_S);
            }
            arrB.Sort(); arrS.Sort(); arrB.Reverse(); arrS.Reverse();


            foreach (KeyValuePair<string, HeaderRecord> pair in Clients)
            {
                for (int i = 0; i < arrB.Count; i++)
                {
                    if (pair.Value.TradeAmountUSD_B == arrB[i])
                    {
                        pair.Value.RankB = i + 1; break;
                    }
                }
                for (int j = 0; j < arrS.Count; j++)
                {
                    if (pair.Value.TradeAmountUSD_S == arrS[j])
                    {
                        pair.Value.RankS = j + 1; break;
                    }
                }
            }

            RankingRequired = false;
        }

        public void RunEntitiesReport(string fileName)
        {
            if (RankingRequired)  RankClients();
            
            FileStream fs = null;
            int pd = 20; int pd2 = 10;
            try
            {
                fs = new FileStream(fileName, FileMode.OpenOrCreate);
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    int id = 0;
                    foreach (KeyValuePair<string, HeaderRecord> pair in Clients)
                    {
                        writer.WriteLine(" {0} {1} {2} {3} {4} {5}", "Id ", "Entity".PadRight(pd), "Outgoing USD Amount".PadRight(pd), "Incoming USD Amount".PadRight(pd), "Rank (Outgoing)".PadRight(pd), "Rank (Incoming)".PadRight(pd));
                        writer.WriteLine(" {0} {1} {2} {3} {4} {5}", "---", "------".PadRight(pd), "-------------------".PadRight(pd), "-------------------".PadRight(pd), "---------------".PadRight(pd), "---------------".PadRight(pd));

                        writer.WriteLine(" {0} {1} {2} {3} {4} {5}", (++id).ToString().PadRight(4),
                            pair.Value.Header.PadRight(pd),
                            pair.Value.TradeAmountUSD_B.ToString().PadRight(pd), pair.Value.TradeAmountUSD_S.ToString().PadRight(pd),
                            pair.Value.RankB.ToString().PadRight(pd), pair.Value.RankS.ToString().PadRight(pd));

                        writer.WriteLine(" {0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", "".PadRight(pd2),
                            "Entity".PadRight(pd2), "USD Amount".ToString().PadLeft(pd2),
                            "Type".ToString().PadRight(pd2), "AgreedFx".ToString().PadLeft(pd2),
                            "Currency".PadRight(pd2), "InstructionDate".ToString().PadRight(pd2), "SettlementDate".ToString().PadLeft(pd2),
                            "Units".ToString().PadLeft(pd2), "PricePerUnit".ToString().PadLeft(pd2));

                        writer.WriteLine(" {0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", "".PadRight(pd2),
                            "------".PadRight(pd2), "----------".ToString().PadLeft(pd2),
                            "----".ToString().PadRight(pd2), "--------".ToString().PadLeft(pd2),
                            "--------".PadRight(pd2), "---------------".ToString().PadRight(pd2), "--------------".ToString().PadLeft(pd2),
                            "-----".ToString().PadLeft(pd2), "------------".ToString().PadLeft(pd2));

                        foreach (Transaction tr in pair.Value.Transactions)
                        {
                            writer.WriteLine(" {0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", "".PadRight(pd2),
                            tr.Entity.PadRight(pd2), tr.TradeAmountUSD.ToString().PadLeft(pd2),
                            tr.TransactionType.ToString().PadRight(pd2), tr.AgreedFx.ToString().PadLeft(pd2),
                            tr.Currency.PadRight(pd2), tr.InstructionDate.ToString("dd MMM yyyy").PadRight(pd2), tr.SettlementDate.ToString("dd MMM yyyy").PadLeft(pd - 2),
                            tr.Units.ToString().PadLeft(pd2), tr.PricePerUnit.ToString().PadLeft(pd2));
                        }
                        writer.WriteLine();
                    }
                }
            }
            finally { if (fs != null) fs.Dispose(); }
        }

        public void RunDatesReport(string fileName)
        {
            if (RankingRequired) RankClients();

            FileStream fs = null;
            int pd = 20; int pd2 = 10;
            try
            {
                fs = new FileStream(fileName, FileMode.OpenOrCreate);
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    int id = 0;
                    foreach (KeyValuePair<string, HeaderRecord> pair in Dates)
                    {
                        DateTime strDay = DateTime.Parse(pair.Value.Header);

                        writer.WriteLine(" {0} {1} {2} {3} ", "Id ", "SettlementDate (Expected)".PadRight(pd), "Outgoing USD Amount".PadRight(pd), "Incoming USD Amount".PadRight(pd));
                        writer.WriteLine(" {0} {1} {2} {3} ", "---", "-------------------------".PadRight(pd), "-------------------".PadRight(pd), "-------------------".PadRight(pd));

                        writer.WriteLine(" {0} {1} {2} {3}", (++id).ToString().PadRight(4),
                            (pair.Value.Header+" "+strDay.DayOfWeek).PadRight(pd+5),
                            pair.Value.TradeAmountUSD_B.ToString().PadRight(pd), pair.Value.TradeAmountUSD_S.ToString().PadRight(pd));

                        writer.WriteLine(" {0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", "".PadRight(pd2),
                            "Entity".PadRight(pd2), "USD Amount".ToString().PadLeft(pd2),
                            "Type".ToString().PadRight(pd2), "AgreedFx".ToString().PadLeft(pd2),
                            "Currency".PadRight(pd2), "InstructionDate".ToString().PadRight(pd2), "SettlementDate".ToString().PadLeft(pd2),
                            "Units".ToString().PadLeft(pd2), "PricePerUnit".ToString().PadLeft(pd2));

                        writer.WriteLine(" {0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", "".PadRight(pd2),
                            "------".PadRight(pd2), "----------".ToString().PadLeft(pd2),
                            "----".ToString().PadRight(pd2), "--------".ToString().PadLeft(pd2),
                            "--------".PadRight(pd2), "---------------".ToString().PadRight(pd2), "--------------".ToString().PadLeft(pd2),
                            "-----".ToString().PadLeft(pd2), "------------".ToString().PadLeft(pd2));

                        foreach (Transaction tr in pair.Value.Transactions)
                        {
                            writer.WriteLine(" {0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", "".PadRight(pd2),
                            tr.Entity.PadRight(pd2), tr.TradeAmountUSD.ToString().PadLeft(pd2),
                            tr.TransactionType.ToString().PadRight(pd2), tr.AgreedFx.ToString().PadLeft(pd2),
                            tr.Currency.PadRight(pd2), tr.InstructionDate.ToString("dd MMM yyyy").PadRight(pd2), tr.SettlementDate.ToString("dd MMM yyyy").PadLeft(pd - 2),
                            tr.Units.ToString().PadLeft(pd2), tr.PricePerUnit.ToString().PadLeft(pd2));
                        }
                        writer.WriteLine();
                    }
                }
            }
            finally { if (fs != null) fs.Dispose(); }
        }
    }

}
