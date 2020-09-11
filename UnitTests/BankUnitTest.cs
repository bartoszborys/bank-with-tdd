using System;
using System.Collections.Generic;
using Xunit;
using Bank;
using Bank.Abstractions;
using Bank.DefaultBank;
using System.Threading.Tasks;
using System.Linq;

namespace BaknUnitTests
{
    public class Bank
    {
        private class MockDbContext : IDatabase
        {
            public double loanInterestRate = 3.7;
            public double balance = 0;
            public List<MockBankHistory> bankHistory = new List<MockBankHistory>();
            public long creditMonthsPassed;
            public long creditMonths;
            public long creditAmmount;
            public double minimalCreditBalace = 0.15;

            public double getBalance()
            {
                return this.balance;
            }

            public void getCredit(int creditValue, int months)
            {
                this.creditAmmount = creditValue;
                this.creditMonths = months;
                this.balance += creditValue;
            }

            public double getCreditAmmountLeft()
            {
                return (this.creditAmmount * this.loanInterestRate) - (this.creditAmmount * this.loanInterestRate / this.creditMonths) * this.creditMonthsPassed;
            }

            public double getCreditMonthsLeft()
            {
                return this.creditMonths - this.creditMonthsPassed;
            }

            public IEnumerable<IHistory> getHistory(string bankAccountNumber)
            {
                return this.bankHistory;
            }

            public void incomeTransfer(string bankAccountNumber, int ammount)
            {
                this.bankHistory.Add((new MockBankHistory(ammount, TransferType.ReceiveTransfer, bankAccountNumber)));
                this.balance += ammount;
            }

            public bool isAllowedToGetCredit(string bankAccountNumber, int creditValue)
            {
                return this.balance >= (creditValue * this.minimalCreditBalace);
            }

            public void outcomeTransfer(string bankAccountNumber, int ammount)
            {
                this.bankHistory.Add((new MockBankHistory(ammount, TransferType.SendTransfer, bankAccountNumber)));
                this.balance -= ammount;
            }

            public void updateBalance(string bankAccountNumber, int ammount)
            {
                if(ammount >= 0)
                {
                    this.bankHistory.Add((new MockBankHistory(ammount, TransferType.InsertATM, bankAccountNumber)));
                }
                else
                {
                    this.bankHistory.Add((new MockBankHistory(ammount, TransferType.WithdrawATM, bankAccountNumber)));
                }
                this.balance += ammount;
            }
        }

        public class MockBankHistory : IHistory
        {
            public long ammount;
            public TransferType type;
            public string toAccount;

            public MockBankHistory(long ammount, TransferType type, string toAccount)
            {
                this.ammount = ammount;
                this.type = type;
                this.toAccount = toAccount;
            }

            public string getText()
            {
                switch(this.type)
                {
                    case TransferType.SendTransfer:
                        return $"Wys³ano ${this.ammount} pieniêdzy do ${this.toAccount}";

                    case TransferType.ReceiveTransfer:
                        return $"Wys³ano ${this.ammount} pieniêdzy do ${this.toAccount}";

                    case TransferType.WithdrawATM:
                        return $"Wyp³acono ${this.ammount} pieniêdzy";

                    case TransferType.InsertATM:
                        return $"Wp³acono ${this.ammount} pieniêdzy";
                }

                return "Invalid operation";
            }
        }


        private string ValidBankAccount = "123456";
        private string InvalidBankAccount = "1234567";

        private MockDbContext dbContext;
        private IBank bank;

        public Bank()
        {
            this.dbContext = new MockDbContext();
            this.bank = new DefaultBank(this.dbContext, this.ValidBankAccount);
        }

        [Fact]
        public void ShouldErrorOnInvalidBankAccount()
        {
            try
            {
                new DefaultBank(this.dbContext, this.InvalidBankAccount);
                Assert.True(false);
            }
            catch (ArgumentException)
            {
                Assert.True(true);
            }
        }

        [Fact]
        public void ShouldErrorWhenAtmAddZero()
        {
            try
            {
                this.updateATM(0);
                Assert.True(false);
            }
            catch (ArgumentException)
            {
                Assert.True(true);
            }
        }

        [Fact]
        public void ShouldInsertMoneyToATM()
        {
            this.dbContext.balance = 0;
            this.updateATM(50);
        }

        [Fact]
        public void ShouldWithdrawMoneyFromATM()
        {
            this.dbContext.balance = 50;
            this.updateATM(-50);
        }
        
        [Fact]
        public void ShouldNotWithdrawFromAtmWhenNotEnoughMoney()
        {
            this.dbContext.balance = 49;

            try
            {
                this.bank.UpdateATM(-50);
                Assert.True(false);
            }
            catch (ArgumentException)
            {
                Assert.True(true);
            }

        }

        private void updateATM(int money)
        {
            double bankAccountBefore = this.dbContext.balance;
            this.bank.UpdateATM(money);     
            Assert.Equal(bankAccountBefore + money, this.dbContext.balance);
        }

        [Fact]
        public void ShouldNotTransferWhileNotEnoughtMoney()
        {
            this.dbContext.balance = 0;

            try
            {
                this.bank.UpdateTransfer(this.ValidBankAccount, -50);
                Assert.True(false);
            }
            catch (ArgumentException)
            {
                Assert.True(true);
            }
        }

        [Fact]
        public void ShouldErrorWhenTransferToInvalidBankNumber()
        {
            try
            {
                this.bank.UpdateTransfer(this.InvalidBankAccount, -50);
                Assert.True(false);
            }
            catch (ArgumentException)
            {
                Assert.True(true);
            }
        }

        [Fact]
        public void ShouldSendMoneyToOther()
        {
            this.dbContext.balance = 512;
            this.bank.UpdateTransfer(this.ValidBankAccount, -50);
        }

        [Fact]
        public void ShouldErrorWhenReceiveFromInvalidBankNumber()
        {
            try
            {
                this.bank.UpdateTransfer(this.InvalidBankAccount, 50);
                Assert.True(false);
            }
            catch (ArgumentException)
            {
                Assert.True(true);
            }
        }

        [Fact]
        public void ShouldReceiveMoneyFromOther()
        {
            this.bank.UpdateTransfer(this.ValidBankAccount, 50);
        }

        [Fact]
        public void ShouldErrorWhenSendOrReceiveZero()
        {
            try
            {
                this.bank.UpdateTransfer(this.ValidBankAccount, 0);
                Assert.True(false);
            }
            catch(ArgumentException)
            {
                Assert.True(true);
            }
        }

        [Fact]
        public void ShouldNotBeAbleToGetCredit()
        {
            int creditValue = 20000;
            int months = 12 * 3;

            this.dbContext.balance = 0;

            Assert.ThrowsAsync<ArgumentException>("Should not give a credit while client has low account balance", () => Task.Run(() => this.bank.Credit(creditValue, months)));
        }

        [Fact]
        public void ShouldBeAbleToGetCredit()
        {

            const int creditValue = 20000;
            const int months = 12 * 3;

            this.dbContext.creditMonths = 0;
            this.dbContext.creditAmmount = 0;

            this.dbContext.balance = creditValue * this.dbContext.minimalCreditBalace;

            double balanceBefore = this.dbContext.balance;
            this.bank.Credit(creditValue, months);

            Assert.Equal(balanceBefore + creditValue, this.dbContext.balance);
        }

        [Fact]
        public void ShouldReviewCreditState()
        {
            int creditValue = 20000;
            int months = 12 * 3;
            const int monthsPassed = 5;

            this.dbContext.creditMonths = 0;
            this.dbContext.creditAmmount = 0;
            this.dbContext.creditMonthsPassed = monthsPassed;

            this.dbContext.balance = creditValue * this.dbContext.minimalCreditBalace;

            double creditWithLoan = creditValue * this.dbContext.loanInterestRate;
            double creditAfterPassedMonths = creditWithLoan - (creditWithLoan / months) * monthsPassed;


            this.bank.Credit(creditValue, months);


            Assert.Equal(creditAfterPassedMonths, this.bank.CreditAmmountLeft());
            Assert.Equal(months - monthsPassed, this.bank.CreditMonthsLeft());
        }

        [Fact]
        public void ShouldRevievBankHistory()
        {
            int money = 50;

            List<IHistory> expected = new List<IHistory>();
            expected.Add((new MockBankHistory(money, TransferType.InsertATM, this.ValidBankAccount)));
            expected.Add((new MockBankHistory(money, TransferType.InsertATM, this.ValidBankAccount)));

            this.bank.UpdateATM(money);
            this.bank.UpdateATM(money);

            var expectedResult = expected.Select(item => item.getText()).ToArray();
            var result= this.bank.GetHistory().Select(item => item.getText()).ToArray();
            Assert.Equal(expectedResult, result);
        }
    }
}
