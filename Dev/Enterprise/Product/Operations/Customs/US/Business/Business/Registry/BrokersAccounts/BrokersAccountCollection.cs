using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class BrokersAccountCollection : RegistryBusinessObjectCollectionTemplate
	{
		public BrokersAccountCollection()
			: base()
		{
		}

		public BrokersAccountCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ManagedAccount this[int i]
		{
			get { return (ManagedAccount)Elements[i]; }
		}

		public new ManagedAccount AddNew()
		{
			return (ManagedAccount)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BrokersAccountCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ManagedAccount(CurrentFallbackLevel, CurrentFactory, this);
		}

		public bool ContainsPayerUnitNo(ZString payerUnitNo, string branchDesignation = "")
		{
			var account = GetMatchedBrokersAccount(payerUnitNo, branchDesignation);
			return account != null;
		}

		public ZGuid GetAssociatedBankAccount(ZString payerUnitNo, string branchDesignation)
		{
			var matchedAccount = GetMatchedBrokersAccount(payerUnitNo, branchDesignation);
			if (matchedAccount != null)
			{
				return matchedAccount.BankAccount;
			}
			return ZGuid.Empty;
		}

		ManagedAccount GetMatchedBrokersAccount(ZString payerUnitNo, string branchDesignation)
		{
			var accounts = this.Cast<ManagedAccount>();
			var matchedAccount = accounts.FirstOrDefault(x => x.PayerUnitNumber == payerUnitNo &&
								x.ClientBranchDesignation == branchDesignation) ?? accounts.FirstOrDefault(x => x.PayerUnitNumber == payerUnitNo && x.ClientBranchDesignation.IsEmpty);
			return matchedAccount;
		}

		public ZString GetPayerUnitNoByBranchDesignation(ZString branchDesignation)
		{
			var result = ZString.Empty;
			var account = this.Cast<ManagedAccount>().FirstOrDefault(x => x.ClientBranchDesignation == branchDesignation) ?? this.Cast<ManagedAccount>().FirstOrDefault(x => x.ClientBranchDesignation.IsEmpty);

			if (account != null)
			{
				result = account.PayerUnitNumber;
			}
			return result;
		}

		public bool HasMultipleBankAccountsPerPUN(ZString payerUnitNo)
		{
			InitialiseAssociations();
			List<ZGuid> bankAccounts;
			if (bankAccountsPerPUN.TryGetValue(payerUnitNo, out bankAccounts))
			{
				return bankAccounts.Count > 1;
			}
			return false;
		}

		public bool HasMultiplePUNsPerBankAccount(ZGuid bankAccount, out bool hasEmptyPUN)
		{
			InitialiseAssociations();
			hasEmptyPUN = false;
			List<ZString> payerUnitNumbers;
			if (punsPerBankAccount.TryGetValue(bankAccount, out payerUnitNumbers))
			{
				hasEmptyPUN = payerUnitNumbers.Contains(ZString.Empty);
				return payerUnitNumbers.Count > 1;
			}
			return false;
		}

		public bool HasMultiplePUNsPerClientBranchDesignation(ZString clientBranchDesignation)
		{
			InitialiseAssociations();
			List<ZString> payerUnitNumbers;
			if (punsPerClientBranch.TryGetValue(clientBranchDesignation, out payerUnitNumbers))
			{
				return payerUnitNumbers.Count > 1;
			}
			return false;
		}

		public bool HasMultipleRows(ZString payerUnitNo, ZString clientBranchDesignation)
		{
			InitialiseAssociations();
			List<ManagedAccount> result;
			if (accountsPerPUNAndCBD.TryGetValue(GetKeyForAccountsPerPUNAndCBD(payerUnitNo, clientBranchDesignation), out result))
			{
				return result.Count > 1;
			}
			return false;
		}

		void InitialiseAssociations()
		{
			if (!initialised)
			{
				initialised = true;

				bankAccountsPerPUN = new Dictionary<ZString, List<ZGuid>>();
				punsPerBankAccount = new Dictionary<ZGuid, List<ZString>>();
				accountsPerPUNAndCBD = new Dictionary<ZString, List<ManagedAccount>>();
				punsPerClientBranch = new Dictionary<ZString, List<ZString>>();

				foreach (ManagedAccount account in this)
				{
					SortOutToBankAccountsPerPUN(account);

					SortOutToPunsPerBankAccount(account);

					SortOutToAccountsPerPUNAndCBD(account);

					SortOutToPunsPerClientBranch(account);
				}
			}
		}

		internal void RefreshAssociations()
		{
			initialised = false;
		}

		void SortOutToBankAccountsPerPUN(ManagedAccount account)
		{
			List<ZGuid> bankAccounts;
			if (!bankAccountsPerPUN.TryGetValue(account.PayerUnitNumber, out bankAccounts))
			{
				bankAccounts = new List<ZGuid>();
				bankAccountsPerPUN.Add(account.PayerUnitNumber, bankAccounts);
			}

			if (!bankAccounts.Contains(account.BankAccount))
			{
				bankAccounts.Add(account.BankAccount);
			}
		}

		void SortOutToPunsPerBankAccount(ManagedAccount account)
		{
			List<ZString> pUNs;
			if (!punsPerBankAccount.TryGetValue(account.BankAccount, out pUNs))
			{
				pUNs = new List<ZString>();
				punsPerBankAccount.Add(account.BankAccount, pUNs);
			}

			if (!pUNs.Contains(account.PayerUnitNumber))
			{
				pUNs.Add(account.PayerUnitNumber);
			}
		}

		void SortOutToAccountsPerPUNAndCBD(ManagedAccount account)
		{
			List<ManagedAccount> accounts;
			var key = GetKeyForAccountsPerPUNAndCBD(account.PayerUnitNumber, account.ClientBranchDesignation);

			if (!accountsPerPUNAndCBD.TryGetValue(key, out accounts))
			{
				accounts = new List<ManagedAccount>();
				accountsPerPUNAndCBD.Add(key, accounts);
			}

			if (!accounts.Contains(account))
			{
				accounts.Add(account);
			}
		}

		void SortOutToPunsPerClientBranch(ManagedAccount account)
		{
			if (!account.PayerUnitNumber.IsEmpty)
			{
				List<ZString> pUNs;
				if (!punsPerClientBranch.TryGetValue(account.ClientBranchDesignation, out pUNs))
				{
					pUNs = new List<ZString>();
					punsPerClientBranch.Add(account.ClientBranchDesignation, pUNs);
				}

				if (!pUNs.Contains(account.PayerUnitNumber))
				{
					pUNs.Add(account.PayerUnitNumber);
				}
			}
		}

		string GetKeyForAccountsPerPUNAndCBD(ZString payerUnitNo, ZString clientBranch)
		{
			return payerUnitNo + ":" + clientBranch;
		}

		bool initialised;
		Dictionary<ZString, List<ZGuid>> bankAccountsPerPUN;
		Dictionary<ZGuid, List<ZString>> punsPerBankAccount;
		Dictionary<ZString, List<ManagedAccount>> accountsPerPUNAndCBD;
		Dictionary<ZString, List<ZString>> punsPerClientBranch;

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			RefreshAssociations();

			foreach (ManagedAccount account in this)
			{
				account.RunPreSaveValidation();
			}
		}
	}
}
