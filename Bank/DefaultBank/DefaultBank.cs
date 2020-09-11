using Bank.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.DefaultBank
{
    public class DefaultBank : IBank
    {
        private IDatabase dbContext;
        private string bankAccountNumber;

        public DefaultBank(IDatabase dbContext, string bankAccountNumber)
        {
            if(!this.IsValidBankNumber(bankAccountNumber))
            {
                throw new ArgumentException("Wrong bank number");
            }

            this.dbContext = dbContext;
            this.bankAccountNumber = bankAccountNumber;
        }

        private bool IsValidBankNumber(string bankNumber)
        {
            int lastNumber = (int)char.GetNumericValue(bankNumber.Last());
            return (lastNumber % 2) == 0;
        }

        public void Credit(int creditValue, int months)
        {
            if (!this.dbContext.isAllowedToGetCredit(this.bankAccountNumber, creditValue))
            {
                throw new ArgumentException("Its not possible to get credit with low balance");
            }

            this.dbContext.getCredit(creditValue, months);
        }

        public double CreditAmmountLeft()
        {
            return this.dbContext.getCreditAmmountLeft();
        }

        public double CreditMonthsLeft()
        {
            return this.dbContext.getCreditMonthsLeft();
        }

        public IEnumerable<IHistory> GetHistory()
        {
            return this.dbContext.getHistory(this.bankAccountNumber);
        }

        public void UpdateATM(int ammount)
        {
            if (ammount == 0)
            {
                throw new ArgumentException("Its not possible to send zero.");
            }

            if (ammount < 0 && this.dbContext.getBalance() < Math.Abs(ammount))
            {
                throw new ArgumentException("Its not possible to withdraw money when balance is lower than wanted ammount.");
            }
            

            this.dbContext.updateBalance(this.bankAccountNumber, ammount);
        }

        public void UpdateTransfer(string bankAccountNumber, int ammount)
        {
            if (!this.IsValidBankNumber(bankAccountNumber))
            {
                throw new ArgumentException("Wrong bank number");
            }

            if (ammount == 0)
            {
                throw new ArgumentException("Its not possible to send zero.");
            }

            if (ammount < 0 && this.dbContext.getBalance() < Math.Abs(ammount))
            {
                throw new ArgumentException("Its not possible to send money when balance is lower than wanted ammount.");
            }

            if (ammount < 0)
            {
                this.dbContext.outcomeTransfer(bankAccountNumber, ammount);
                return;
            }

            this.dbContext.incomeTransfer(bankAccountNumber, ammount);
        }
    }
}
