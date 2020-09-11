using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Abstractions
{
    public interface IDatabase
    {
        double getBalance();
        void updateBalance(string bankAccountNumber, int ammount);
        void outcomeTransfer(string bankAccountNumber, int ammount);
        void incomeTransfer(string bankAccountNumber, int ammount);
        double getCreditMonthsLeft();
        double getCreditAmmountLeft();
        void getCredit(int creditValue, int months);
        bool isAllowedToGetCredit(string bankAccountNumber, int creditValue);
        IEnumerable<IHistory> getHistory(string bankAccountNumber);
    }
}
