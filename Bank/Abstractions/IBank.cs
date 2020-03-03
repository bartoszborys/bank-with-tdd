using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Abstractions
{
    public interface IBank
    {
        IEnumerable<string> GetHistory();
        Task UpdateATM(int money);
        IEnumerable<object> CreditStatus();
        void Credit(int creditValue, int months);
        Task UpdateTransfer(long validBankAccount, int v);
    }
}
