using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.ZA.Business.XmlSerializers")]
	public class FinancialAccountNumberPortMap : RegistryBusinessObjectTemplate
	{
		public const decimal DefaultVatDefermentAmount = 999999999.99m;

		public FinancialAccountNumberPortMap()
		{
		}

		public FinancialAccountNumberPortMap(FallbackLevel fallbackLevel, BusinessObjectFactory factory, FinancialAccountNumberPortMapCollection collection)
			: base(fallbackLevel, factory)
		{
			this.collection = collection;
		}

		readonly FinancialAccountNumberPortMapCollection collection;

		public static class Schema
		{
			public const string OrganizationPK = "OrganizationPK";
			public const string CustomsOfficeCode = "CustomsOfficeCode";
			public const string FinancialAccountNumber = "FinancialAccountNumber";
			public const string Cash = "Cash";
			public const string CreditorPK = "CreditorPK";
			public const string ImporterPays = "ImporterPays";
			public const string AccountStartDay = "AccountStartDay";
			public const string DutyDefermentAmount = "DutyDefermentAmount";
			public const string PaymentDay = "PaymentDay";
			public const string AutoAllocationAllowed = "AutoAllocationAllowed";
			public const string VatDefermentAmount = "VatDefermentAmount";

			public const int FinancialAccountNumberMaxLength = 10;
		}

		#region OrganizationPK

		[List(nameof(Organizations))]
		[RelatedBusinessObject("Organization")]
		[ResourceStringData("Enterprise.Customs.ZA.DataRegistry.Business.FinancialAccountNumberPortMap|OrganizationPK", Caption = "Organization")]
		public ZGuid OrganizationPK
		{
			get { return organizationPK; }
			set
			{
				SetNonPersistentPropertyValue(OrganizationPKInfo, ref organizationPK, value);

				if (!IsValidationSuspended)
				{
					ValidateOrganizationPK();
				}
			}
		}
		ZGuid organizationPK;

		public ZPropertyInfo OrganizationPKInfo
		{
			get { return GetZPropertyInfo(Schema.OrganizationPK); }
		}

		void ValidateOrganizationPK()
		{
			OrganizationPKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(OrganizationPKInfo, "Organization");
			ListValidation.ErrorIfInvalidPK(OrganizationPKInfo, Organizations);
			var agent = Organization;
			if (agent != null && agent.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.AgentCode, Core.Constants.CountryCodes.SouthAfrica).IsEmpty)
			{
				OrganizationPKInfo.AddError(ValidOrganizationIsRequired);
			}
		}

		public const string ValidOrganizationIsRequired = "Please enter an Organization with a valid Agent Code.";

		public OrgHeader Organization
		{
			get { return CurrentFactory.Load<OrgHeader>(OrganizationPK); }
		}

		public OrgHeaderCollection Organizations
		{
			get { return new OrgHeaderCollection(CurrentFactory); }
		}

		#endregion

		#region CustomsOfficeCode

		[List(nameof(CustomsOfficeCodeList))]
		[MaxLength(Universal.ZZRefCusCodeListCombined.Schema.ZZD_CodeMaxLength)]
		[ResourceStringData("Enterprise.Customs.ZA.DataRegistry.Business.FinancialAccountNumberPortMap|CustomsOfficeCode", Caption = "Customs Office Code", MediumCaption = "Customs Office")]
		public ZString CustomsOfficeCode
		{
			get { return customsOfficeCode; }
			set
			{
				SetNonPersistentPropertyValue(CustomsOfficeCodeInfo, ref customsOfficeCode, value);

				if (!IsValidationSuspended)
				{
					ValidateCustomsOfficeCode();
				}
			}
		}
		ZString customsOfficeCode;

		public CodeDescriptionPairList CustomsOfficeCodeList
		{
			get { return ZARefCusCodeListTypes.GetCustomsOfficeList(CurrentFactory); }
		}

		public ZPropertyInfo CustomsOfficeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CustomsOfficeCode); }
		}

		void ValidateCustomsOfficeCode()
		{
			CustomsOfficeCodeInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidCode(CustomsOfficeCodeInfo, CustomsOfficeCodeList);

			if (Cash)
			{
				if (!CustomsOfficeCode.IsEmpty)
				{
					CustomsOfficeCodeInfo.AddError(CustomsOfficeMustBeBlankForCash);
				}
			}
			else
			{
				MandatoryValidation.CheckEntered(CustomsOfficeCodeInfo, "Customs Office Code");
			}

			if (collection != null)
			{
				foreach (FinancialAccountNumberPortMap creditorMapping in collection)
				{
					if (creditorMapping != this && creditorMapping.OrganizationPK == this.OrganizationPK && creditorMapping.CustomsOfficeCode == this.CustomsOfficeCode)
					{
						CustomsOfficeCodeInfo.AddError(DuplicateCustomsOfficeCode);
						break;
					}
				}
			}
		}

		public const string DuplicateCustomsOfficeCode = "You have already entered this district port code.";
		public static string CustomsOfficeMustBeBlankForCash => Enterprise.Customs.ZA.Business.Res.GetString("dd3f4600-484a-48ba-8532-d5b05b97d1dd", "A Customs Office must not be entered for a 'Cash' Financial Account Number.");

		#endregion

		#region FinancialAccountNumber

		[MaxLength(Schema.FinancialAccountNumberMaxLength)]
		[ResourceStringData("Enterprise.Customs.ZA.DataRegistry.Business.FinancialAccountNumberPortMap|FinancialAccountNumber", Caption = "Financial Account Number (FAN)", MediumCaption = "Financial Acc. No.", ShortCaption = "FAN")]
		public ZString FinancialAccountNumber
		{
			get { return financialAccountNumber; }
			set
			{
				SetNonPersistentPropertyValue(FinancialAccountNumberInfo, ref financialAccountNumber, value);

				if (!IsValidationSuspended)
				{
					ValidateFinancialAccountNumber();
				}
			}
		}
		ZString financialAccountNumber;

		public ZPropertyInfo FinancialAccountNumberInfo
		{
			get { return GetZPropertyInfo(Schema.FinancialAccountNumber); }
		}

		void ValidateFinancialAccountNumber()
		{
			FinancialAccountNumberInfo.ClearAllNotifications();
			if (FinancialAccountNumber.IsEmpty)
			{
				FinancialAccountNumberInfo.AddError(FinancialAccountNumberIsRequired);
			}
			else if (FinancialAccountNumber.KeepNumericCharacters().Length != 10 || FinancialAccountNumber.Length != 10)
			{
				FinancialAccountNumberInfo.AddError(FinancialAccountNumberMustBeValid);
			}
		}
		public const string FinancialAccountNumberIsRequired = "Please enter a Financial Account Number (FAN)";
		public const string FinancialAccountNumberMustBeValid = "Financial Account Number(FAN) must have 10 numeric characters";

		#endregion

		#region Cash

		[ResourceStringData("Enterprise.Customs.ZA.DataRegistry.Business.FinancialAccountNumberPortMap|Cash", Caption = "Cash", ShortCaption = "Cash")]
		public ZBool Cash
		{
			get { return cash; }
			set
			{
				SetNonPersistentPropertyValue(CashInfo, ref cash, value);
				if (!IsValidationSuspended)
				{
					ValidateCash();
				}
			}
		}
		ZBool cash;

		public ZPropertyInfo CashInfo
		{
			get { return GetZPropertyInfo(Schema.Cash); }
		}

		void ValidateCash()
		{
			CashInfo.ClearAllNotifications();
			if (Cash && (collection?.OfType<FinancialAccountNumberPortMap>().Any(x => this.OrganizationPK == x.OrganizationPK && this.PK != x.PK && this.Cash == x.Cash) ?? false))
			{
				CashInfo.AddError(OnlyOneCashFANPerOrg);
			}
		}

		public static string OnlyOneCashFANPerOrg => Enterprise.Customs.ZA.Business.Res.GetString("776de85a-c8da-4f91-80a1-6a755e7c528a", "Only one Financial Account Number may be flagged as 'Cash' per Organization");

		#endregion

		#region CreditorPK

		[List(nameof(Creditors))]
		[ResourceStringData("Enterprise.Customs.ZA.DataRegistry.Business.FinancialAccountNumberPortMap|CreditorPK", Caption = "Creditor")]
		public ZGuid CreditorPK
		{
			get { return creditorPK; }
			set
			{
				var oldValue = creditorPK;
				SetNonPersistentPropertyValue(CreditorPKInfo, ref creditorPK, value);
				if (oldValue != CreditorPK && CreditorPK.IsValid && ImporterPays)
				{
					ImporterPays = ZBool.False;
				}
				if (!IsValidationSuspended)
				{
					ValidateCreditorPK();
				}
			}
		}
		ZGuid creditorPK;

		public ZPropertyInfo CreditorPKInfo
		{
			get { return GetZPropertyInfo(Schema.CreditorPK); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "This property is implicitly referenced by ZAttribute")]
		bool CreditorPK_ReadOnly
		{
			get { return ImporterPays; }
		}

		void ValidateCreditorPK()
		{
			CreditorPKInfo.ClearAllNotifications();
			if (!ImporterPays)
			{
				if (CreditorPK.IsEmpty)
				{
					CreditorPKInfo.AddError(EitherCreditorOrImporterPaysIsNeeded);
				}
				else
				{
					ListValidation.ErrorIfInvalidPK(CreditorPKInfo, Creditors);
				}
			}
		}

		public const string EitherCreditorOrImporterPaysIsNeeded = "Please either enter a Creditor or tick Importer Pays.";

		public CreditorCollection Creditors
		{
			get { return new CreditorCollection(CurrentFactory); }
		}

		#endregion

		#region ImporterPays

		[ResourceStringData("Enterprise.Customs.ZA.DataRegistry.Business.FinancialAccountNumberPortMap|ImporterPays", Caption = "Importer Pays", ShortCaption = "Imp. Pays")]
		public ZBool ImporterPays
		{
			get { return importerPays; }
			set
			{
				var oldValue = importerPays;
				SetNonPersistentPropertyValue(ImporterPaysInfo, ref importerPays, value);
				if (!IsCopying && oldValue != importerPays && ImporterPays)
				{
					if (!CreditorPK.IsEmpty)
					{
						CreditorPK = ZGuid.Empty;
					}
					if (AutoAllocationAllowed)
					{
						AutoAllocationAllowed = false;
					}
				}
				if (!IsValidationSuspended)
				{
					ValidateImporterPays();
				}
			}
		}
		ZBool importerPays;

		public ZPropertyInfo ImporterPaysInfo
		{
			get { return GetZPropertyInfo(Schema.ImporterPays); }
		}

		void ValidateImporterPays()
		{
			ImporterPaysInfo.ClearAllNotifications();
			ValidateCreditorPK();
		}

		#endregion

		#region AccountStartDay

		[ResourceStringData("Enterprise.Customs.ZA.DataRegistry.Business.FinancialAccountNumberPortMap|AccountStartDay", Caption = "Account Start Day", ShortCaption = "Start Day")]
		public ZInt AccountStartDay
		{
			get { return accountStartDay; }
			set
			{
				SetNonPersistentPropertyValue(AccountStartDayInfo, ref accountStartDay, value);
				if (!IsValidationSuspended)
				{
					ValidateAccountStartDay();
				}
			}
		}
		ZInt accountStartDay;

		public ZPropertyInfo AccountStartDayInfo
		{
			get { return GetZPropertyInfo(Schema.AccountStartDay); }
		}

		void ValidateAccountStartDay()
		{
			AccountStartDayInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AccountStartDayInfo, "Account Start Day");
			if (AccountStartDay < 1 || AccountStartDay > 31)
			{
				AccountStartDayInfo.AddError(MustBeAValidCalendarDay);
			}
		}

		public static string MustBeAValidCalendarDay => Enterprise.Customs.ZA.Business.Res.GetString("b038c939-6dc3-4def-b7ad-d384b527ea69", "The Account Start Day must be a valid calendar day (1-31).");

		#endregion

		#region DutyDefermentAmount

		[ResourceStringData("Enterprise.Customs.ZA.DataRegistry.Business.FinancialAccountNumberPortMap|DutyDefermentAmount", Caption = "Duty Deferment Amount")]
		public ZDecimal DutyDefermentAmount
		{
			get { return dutyDefermentAmount; }
			set
			{
				SetNonPersistentPropertyValue(DutyDefermentAmountInfo, ref dutyDefermentAmount, value);
			}
		}
		ZDecimal dutyDefermentAmount;

		public ZPropertyInfo DutyDefermentAmountInfo
		{
			get { return GetZPropertyInfo(Schema.DutyDefermentAmount); }
		}

		#endregion

		#region PaymentDay

		[ResourceStringData("Enterprise.Customs.ZA.DataRegistry.Business.FinancialAccountNumberPortMap|PaymentDay", Caption = "Payment Day")]
		public ZInt PaymentDay
		{
			get { return paymentDay; }
			set
			{
				SetNonPersistentPropertyValue(PaymentDayInfo, ref paymentDay, value);
				if (!IsValidationSuspended)
				{
					ValidatePaymentDay();
				}
			}
		}
		ZInt paymentDay;

		public ZPropertyInfo PaymentDayInfo
		{
			get { return GetZPropertyInfo(Schema.PaymentDay); }
		}

		void ValidatePaymentDay()
		{
			PaymentDayInfo.ClearAllNotifications();
			if (!PaymentDay.IsEmpty)
			{
				if (PaymentDay < 1 || PaymentDay > 31)
				{
					PaymentDayInfo.AddError(PaymentDayMustBeAValidCalendarDay);
				}
			}
		}

		public static string PaymentDayMustBeAValidCalendarDay => Enterprise.Customs.ZA.Business.Res.GetString("EAA00761-FFEF-4ACC-B2C4-520AFDCEC3A1", "The Payment Day must be a valid calendar day (1-31).");

		#endregion

		#region AutoAllocationAllowed

		[ResourceStringData("Enterprise.Customs.ZA.DataRegistry.Business.FinancialAccountNumberPortMap|AutoAllocationAllowed", Caption = "Auto Allocation Allowed")]
		public ZBool AutoAllocationAllowed
		{
			get { return autoAllocationAllowed; }
			set
			{
				var oldValue = autoAllocationAllowed;
				SetNonPersistentPropertyValue(AutoAllocationAllowedInfo, ref autoAllocationAllowed, value);
				if (!IsCopying && oldValue != autoAllocationAllowed && AutoAllocationAllowed)
				{
					ImporterPays = false;
				}
			}
		}
		ZBool autoAllocationAllowed;

		public ZPropertyInfo AutoAllocationAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.AutoAllocationAllowed); }
		}

		#endregion

		#region VatDefermentAmount

		[ResourceStringData("Enterprise.Customs.ZA.DataRegistry.Business.FinancialAccountNumberPortMap|VatDefermentAmount", Caption = "Vat Deferment Amount")]
		public ZDecimal VatDefermentAmount
		{
			get => vatDefermentAmount;
			set => SetNonPersistentPropertyValue(VatDefermentAmountInfo, ref vatDefermentAmount, value);
		}
		ZDecimal vatDefermentAmount;

		public ZPropertyInfo VatDefermentAmountInfo => GetZPropertyInfo(Schema.VatDefermentAmount);

		#endregion

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateOrganizationPK();
			ValidateCustomsOfficeCode();
			ValidateFinancialAccountNumber();
			ValidateCash();
			ValidateCreditorPK();
			ValidateAccountStartDay();
			ValidatePaymentDay();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new FinancialAccountNumberPortMap(fallbackLevel, factory, null);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.OrganizationPK, OrganizationPK.ToString());
			writer.WriteElementString(Schema.CustomsOfficeCode, CustomsOfficeCode);
			writer.WriteElementString(Schema.FinancialAccountNumber, FinancialAccountNumber);
			writer.WriteElementString(Schema.Cash, Cash.ToString());
			writer.WriteElementString(Schema.ImporterPays, ImporterPays.ToString());
			writer.WriteElementString(Schema.CreditorPK, (ImporterPays ? ZGuid.Empty : CreditorPK).ToString());
			writer.WriteElementString(Schema.AccountStartDay, AccountStartDay.ToString());
			writer.WriteElementString(Schema.DutyDefermentAmount, DutyDefermentAmount.ToString(2));
			writer.WriteElementString(Schema.PaymentDay, PaymentDay.ToString());
			writer.WriteElementString(Schema.AutoAllocationAllowed, AutoAllocationAllowed.ToString());
			writer.WriteElementString(Schema.VatDefermentAmount, VatDefermentAmount.ToString());
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			OrganizationPK = ZGuid.TryParse(reader.ReadElementString(Schema.OrganizationPK), out var orgPKValue) ? orgPKValue : ZGuid.Empty;
			CustomsOfficeCode = reader.ReadElementString(Schema.CustomsOfficeCode);
			FinancialAccountNumber = reader.ReadElementString(Schema.FinancialAccountNumber);
			Cash = reader.ReadElementStringAsZBool(Schema.Cash);
			ImporterPays = reader.ReadElementStringAsZBool(Schema.ImporterPays);

			if (!ZGuid.TryParse(reader.ReadElementString(Schema.CreditorPK), out var creditorPKValue) || ImporterPays)
			{
				CreditorPK = ZGuid.Empty;
			}
			else
			{
				CreditorPK = creditorPKValue;
			}

			AccountStartDay = reader.ReadElementStringAsZInt(Schema.AccountStartDay);
			DutyDefermentAmount = reader.ReadElementStringAsZDecimal(Schema.DutyDefermentAmount);
			PaymentDay = reader.ReadElementStringAsZInt(Schema.PaymentDay);
			AutoAllocationAllowed = reader.ReadElementStringAsZBool(Schema.AutoAllocationAllowed);
			VatDefermentAmount = reader.ReadElementStringAsZDecimal(Schema.VatDefermentAmount);
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			vatDefermentAmount = DefaultVatDefermentAmount;
		}
		#endregion
	}
}
