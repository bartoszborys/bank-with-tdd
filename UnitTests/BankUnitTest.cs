using System;
using System.Collections.Generic;
using Xunit;
using Bank;
using Bank.Abstractions;
using Bank.DefaultBank;
using Bank.Exceptions;

namespace BaknUnitTests
{
    public class Bank
    {
        private class MockDbContext : IDatabase
        {
            public long balance = 0;
            public List<MockBankHistory> bankHistory;
            public DateTime creditStart;
            public long creditMonths;
            public long creditAmmount;
        }

        public class MockBankHistory
        {
            public long ammount;
            public TransferType type;
            public long toAccount;

            public MockBankHistory(long ammount, TransferType type, long toAccount)
            {
                this.ammount = ammount;
                this.type = type;
                this.toAccount = toAccount;
            }

            public string toString()
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


        private long ValidBankAccount = 12345;
        private long InvalidBankAccount = 54321;

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
            Assert.Throws<InvalidBankNumber>("Invalid bank account number is valid", () => new DefaultBank(this.InvalidBankAccount));
        }

        [Fact]
        public async void ShouldErrorWhenAtmAddZero()
        {
            await Assert.ThrowsAsync<InvalidBankNumber>("Invalid state - add/remove 0 money to/from ATM", () => this.updateATM(0));
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
            Assert.ThrowsAsync<NotEnoughMoney>("Should not able to get from ATM while balance lesser than wanted", () => this.bank.UpdateATM(-50) );
        }

        private void updateATM(int money)
        {
            long bankAccountBefore = this.dbContext.balance;
            this.bank.UpdateATM(money);     
            Assert.Equal(bankAccountBefore + money, this.dbContext.balance);
        }

        [Fact]
        public async void ShouldNotTransferWhileNotEnoughtMoney()
        {
            this.dbContext.balance = 0;
            await Assert.ThrowsAsync<NotEnoughMoney>("Invalid bank account while transfer", () => this.bank.UpdateTransfer(this.ValidBankAccount, -50));
        }

        [Fact]
        public async void ShouldErrorWhenTransferToInvalidBankNumber()
        {
            await Assert.ThrowsAsync<InvalidBankNumber>("Invalid bank account while transfer", () => this.bank.UpdateTransfer(this.InvalidBankAccount, -50));
        }

        [Fact]
        public void ShouldSendMoneyToOther()
        {
            this.dbContext.balance = 512;
            this.bank.UpdateTransfer(this.ValidBankAccount, -50);
        }

        [Fact]
        public async void ShouldErrorWhenReceiveFromInvalidBankNumber()
        {
            await Assert.ThrowsAsync<InvalidBankNumber>("Invalid bank account while transfer", () => this.bank.UpdateTransfer(this.InvalidBankAccount, 50));
        }

        [Fact]
        public void ShouldReceiveMoneyFromOther()
        {
            this.bank.UpdateTransfer(this.ValidBankAccount, 50);
        }

        [Fact]
        public async void ShouldErrorWhenSendOrReceiveZero()
        {
            await Assert.ThrowsAsync<InvalidBankNumber>("Invalid bank account while transfer", () => this.bank.UpdateTransfer(this.ValidBankAccount, 0));
        }

        [Fact]
        public void ShouldNotBeAbleToGetCredit()
        {
            int creditValue = 20000;
            int months = 12 * 3;

            this.dbContext.balance = 0;

            Assert.ThrowsAsync<NotEnoughMoney>("Should not give credit while client has low account balance", () => this.bank.Credit(creditValue, months);
        }

        [Fact]
        public void ShouldBeAbleToGetCredit()
        {
            this.dbContext.creditMonths = 0;
            this.dbContext.creditAmmount = 0;

            int creditValue = 20000;
            int months = 12 * 3;

            long balanceBefore = this.dbContext.balance;
            this.bank.Credit(creditValue, months);

            Assert.Equal(balanceBefore + creditValue, this.dbContext.balance);
        }

        [Fact]
        public void ShouldReviewCreditState()
        {
            this.dbContext.creditMonths = 0;
            this.dbContext.creditAmmount = 0;
            this.dbContext.creditStart = DateTime.Now.AddMonths(-5);

            int creditValue = 20000;
            int months = 12 * 3;

            long balanceBefore = this.dbContext.balance;
            this.bank.Credit(creditValue, months);

            Assert.Equal(20000, this.bank.CreditStatus());
        }

        [Fact]
        public void ShouldRevievBankHistory()
        {
            int money = 50;

            List<string> expected = new List<string>();
            expected.Add((new MockBankHistory(money, TransferType.InsertATM, this.ValidBankAccount)).toString());
            expected.Add((new MockBankHistory(money, TransferType.InsertATM, this.ValidBankAccount)).toString());

            this.bank.UpdateATM(money);
            this.bank.UpdateATM(money);
            
            Assert.Equal(expected, this.bank.GetHistory());
        }
    }
}
