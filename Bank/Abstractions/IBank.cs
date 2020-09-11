using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Abstractions
{
    public interface IBank
    {
        IEnumerable<IHistory> GetHistory();
        void UpdateATM(int ammount);
        double CreditAmmountLeft();
        double CreditMonthsLeft();
        void Credit(int creditValue, int months);
        void UpdateTransfer(string bankAccountNumber, int value);
    }
}
