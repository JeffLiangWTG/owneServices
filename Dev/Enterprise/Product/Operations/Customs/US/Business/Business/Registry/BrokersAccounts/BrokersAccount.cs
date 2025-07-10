using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class ManagedAccount : RegistryBusinessObjectTemplate
	{
		public ManagedAccount()
		{
		}

		public ManagedAccount(FallbackLevel fallbackLevel, BusinessObjectFactory factory, BrokersAccountCollection parentCollection)
			: base(fallbackLevel, factory)
		{
			this.parentCollection = parentCollection;
		}
		BrokersAccountCollection parentCollection;

		BrokersAccountCollection ParentCollection
		{
			get
			{
				if (parentCollection == null)
				{
					parentCollection = (BrokersAccountCollection)GetParentCollection(this, typeof(BrokersAccountCollection));
				}
				return parentCollection;
			}
		}

		public static class Schema
		{
			public const string BankAccount = "BankAccount";
			public const string PayerUnitNumber = "PayerUnitNumber";
			public const string ClientBranchDesignation = "ClientBranchDesignation";
		}

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString PayerUnitNumber
		{
			get { return payerUnitNumber; }
			set
			{
				SetNonPersistentPropertyValue(PayerUnitNumberInfo, ref payerUnitNumber, value);

				if (ParentCollection != null)
				{
					ParentCollection.RefreshAssociations();
				}

				if (!IsValidationSuspended)
				{
					ValidatePayerUnitNumber();
					ValidateBankAccount();
					ValidateClientBranchDesignation();
				}
			}
		}
		ZString payerUnitNumber;

		public ZPropertyInfo PayerUnitNumberInfo
		{
			get { return GetZPropertyInfo(Schema.PayerUnitNumber); }
		}

		public void ValidatePayerUnitNumber()
		{
			PayerUnitNumberInfo.ClearAllNotifications();

			if (CurrentFallbackLevel != null && CurrentFactory != null)
			{
				var isCreditPayment = false;

				var customsOrgPK = RatingDataRegistry.Instance.CustomsDisbursementCreditor.GetFallBackValueAtAllLevels(CurrentFallbackLevel.CompanyPK(false), CurrentFallbackLevel.BranchPK, CurrentFallbackLevel.DepartmentPK);
				var customsOrg = CurrentFactory.Load<OrgHeader>(customsOrgPK);

				if (customsOrg != null)
				{
					customsOrg.Reload();
					isCreditPayment = OrgHeaderWrapper.New(customsOrg).ZO_PayMethod == ACHPaymentTypeList.Codes.ACHCredit;
				}

				if (!isCreditPayment)
				{
					MandatoryValidation.CheckEntered(PayerUnitNumberInfo, "payer's unit number");
				}
			}

			if (!PayerUnitNumber.IsEmpty)
			{
				if (PayerUnitNumber.Length != 6)
				{
					PayerUnitNumberInfo.AddError(ValidationConstants.Statement.PayerUnitNoLength);
				}
			}

			if (AccBankAccount != null && ParentCollection != null)
			{
				if (!PayerUnitNumber.IsEmpty && ParentCollection.HasMultipleBankAccountsPerPUN(PayerUnitNumber))
				{
					PayerUnitNumberInfo.AddError(DuplicateBankAccountsForSamePUN);
				}
				else
				{
					ValidateDuplicateRowsPerPUNAndCBD(PayerUnitNumberInfo);
				}
			}
		}

		internal const string DuplicateBankAccountsForSamePUN = "A payer unit number should be associated with a single bank account.";

		internal const string DuplicateRowsForSamePUNAndCBD = "You have entered more than one row for the same combination of the payer unit number and client branch designation.";

		public ZGuid BankAccount
		{
			get { return bankAccount; }
			set
			{
				SetNonPersistentPropertyValue(BankAccountInfo, ref bankAccount, value);

				if (ParentCollection != null)
				{
					ParentCollection.RefreshAssociations();
				}

				if (!IsValidationSuspended)
				{
					ValidateBankAccount();
					ValidatePayerUnitNumber();
					ValidateClientBranchDesignation();
				}
			}
		}
		ZGuid bankAccount;

		public ZPropertyInfo BankAccountInfo
		{
			get { return GetZPropertyInfo(Schema.BankAccount); }
		}

		public void ValidateBankAccount()
		{
			BankAccountInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(BankAccountInfo, "bank account");
			if (BankAccountList != null)
			{
				ListValidation.ErrorIfInvalidPK(BankAccountInfo, BankAccountList, (NoResString)"You have entered an invalid bank account.");
			}
		}

		public AccBankAccount AccBankAccount
		{
			get { return CurrentFactory.Load<AccBankAccount>(BankAccount); }
		}

		public AccBankAccountCollection BankAccountList
		{
			get
			{
				if (fBankAccountList == null && CurrentFallbackLevel != null)
				{
					fBankAccountList = new AccBankAccountCollection(CurrentFactory, new ZQuery(AccBankAccountSchema.AB_GC, CurrentFallbackLevel.CompanyPK(false)));
				}

				if (fBankAccountList != null)
				{
					fBankAccountList.Load();
				}
				return fBankAccountList;
			}
		}
		AccBankAccountCollection fBankAccountList;

		[CargoWise.ComponentModel.MaxLength(2)]
		public ZString ClientBranchDesignation
		{
			get { return clientBranchDesignation; }
			set
			{
				SetNonPersistentPropertyValue(ClientBranchDesignationInfo, ref clientBranchDesignation, value);

				if (ParentCollection != null)
				{
					ParentCollection.RefreshAssociations();
				}

				if (!IsValidationSuspended)
				{
					ValidateClientBranchDesignation();
					ValidatePayerUnitNumber();
					ValidateBankAccount();
				}
			}
		}
		ZString clientBranchDesignation;

		public ZPropertyInfo ClientBranchDesignationInfo
		{
			get { return GetZPropertyInfo(Schema.ClientBranchDesignation); }
		}

		public void ValidateClientBranchDesignation()
		{
			ClientBranchDesignationInfo.ClearAllNotifications();

			if (AccBankAccount != null && ParentCollection != null)
			{
				if (!ClientBranchDesignation.IsEmpty && ParentCollection.HasMultiplePUNsPerClientBranchDesignation(ClientBranchDesignation))
				{
					ClientBranchDesignationInfo.AddError(DuplicatePUNsForSameCBD);
				}
				else
				{
					ValidateDuplicateRowsPerPUNAndCBD(ClientBranchDesignationInfo);
				}
			}
		}

		internal const string DuplicatePUNsForSameCBD = "A client branch designation should be associated with a single payer unit number.";

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidatePayerUnitNumber();
			ValidateBankAccount();
			ValidateClientBranchDesignation();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ManagedAccount(fallbackLevel, factory, null);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.BankAccount, BankAccount.ToString());
			writer.WriteElementString(Schema.PayerUnitNumber, PayerUnitNumber);
			writer.WriteElementString(Schema.ClientBranchDesignation, ClientBranchDesignation);
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			BankAccount = new ZGuid(reader.ReadElementString(Schema.BankAccount));
			PayerUnitNumber = reader.ReadElementString(Schema.PayerUnitNumber);
			ClientBranchDesignation = reader.ReadElementString(Schema.ClientBranchDesignation);
		}

		void ValidateDuplicateRowsPerPUNAndCBD(ZPropertyInfo propertyInfo)
		{
			if (AccBankAccount != null && ParentCollection != null)
			{
				if (ParentCollection.HasMultipleRows(PayerUnitNumber, ClientBranchDesignation))
				{
					propertyInfo.AddError(DuplicateRowsForSamePUNAndCBD);
				}
			}
		}

		#endregion
	}
}
