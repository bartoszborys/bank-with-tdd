using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bank.Abstractions;
using Bank.DefaultBank;

namespace Bank
{
    public enum TransferType
    {
        SendTransfer,
        ReceiveTransfer,
        WithdrawATM,
        InsertATM,
    }

    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
