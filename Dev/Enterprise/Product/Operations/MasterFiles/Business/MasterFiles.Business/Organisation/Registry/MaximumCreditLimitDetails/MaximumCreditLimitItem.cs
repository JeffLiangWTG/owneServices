using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class MaximumCreditLimitItem : RegistryBusinessObjectTemplate
	{
		public MaximumCreditLimitItem()
		{
			SetDefaults();
		}

		public MaximumCreditLimitItem(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
			SetDefaults();
		}

		void SetDefaults()
		{
			using (SuspendSettingHasChanges())
			{
				ComprehensiveReportEnabled = true;
				FailureRiskEnabled = true;
				LatePaymentRiskEnabled = true;
				CommercialBureauEnquiryEnabled = true;
				if (CurrentFallbackLevel?.Level == Enterprise.Integration.RegistryStorageFlags.Company)
				{
					var company = CurrentFactory.Load<GlbCompany>(CurrentFallbackLevel.CompanyPK(false));
					if (company != null && (company.LocalCurrency) != null)
					{
						CurrencyPK = company.LocalCurrency.PK;
					}
				}
			}
		}

		#region Schema

		public static class Schema
		{
			public const string CreditLimit = "CreditLimit";
			public const string CurrencyPK = "CurrencyPK";
			public const string ComprehensiveReportEnabled = "ComprehensiveReportEnabled";
			public const string FailureRiskEnabled = "FailureRiskEnabled";
			public const string LatePaymentRiskEnabled = "LatePaymentRiskEnabled";
			public const string CommercialBureauEnquiryEnabled = "CommercialBureauEnquiryEnabled";
		}

		#endregion

		#region Bound Properties

		ZString creditLimit;
		[MaxLength(12)]
		public ZString CreditLimit
		{
			get => creditLimit;
			set
			{
				SetNonPersistentPropertyValue(CreditLimitInfo, ref creditLimit, value);

				if (!IsValidationSuspended)
				{
					ValidateCreditLimit();
				}
			}
		}

		void ValidateCreditLimit()
		{
			CreditLimitInfo.ClearAllNotifications();

			if (string.IsNullOrWhiteSpace(creditLimit) || long.TryParse(creditLimit, out long creditLimitLong) && creditLimitLong < 0 || !long.TryParse(creditLimit, out _))
			{
				var error = Res.GetString("5583E282-22B2-4DD0-9409-B26239CBDD4A", "Credit Limit must be a natural number.");
				CreditLimitInfo.AddError(error);
			}
		}

		public ZPropertyInfo CreditLimitInfo => GetZPropertyInfo(nameof(CreditLimit));

		ZGuid currencyPK;
		[RelatedBusinessObject("Currency")]
		[List("Currencies")]
		[ResourceStringData("05C3C858-FC35-4FC7-A37D-BF3DA44A3899", Caption = "Currency")]
		public ZGuid CurrencyPK
		{
			get { return currencyPK; }
			set
			{
				SetNonPersistentPropertyValue(CurrencyPKInfo, ref currencyPK, value);
				if (!IsValidationSuspended)
				{
					ValidateCurrencyPK();
				}
			}
		}

		protected void ValidateCurrencyPK()
		{
			CurrencyPKInfo.ClearAllNotifications();

			if (CurrencyPK.IsEmpty)
			{
				var error = Res.GetString("5159737A-11E6-44C9-AF1D-D1B721D401CB", "Currency can not be empty.");
				CurrencyPKInfo.AddError(error);
			}

			TypeValidation.CheckValidGuid(CurrencyPKInfo);
			ListValidation.ErrorIfInvalidPK(CurrencyPKInfo);

			if (!CurrencyPKInfo.HasErrors())
			{
				CheckDuplicateRow();
			}
		}

		void CheckDuplicateRow()
		{
			if (ParentCollection != null)
			{
				foreach (MaximumCreditLimitItem item in ParentCollection)
				{
					item.ClearRowNotifications();
					if (ParentCollection.OfType<MaximumCreditLimitItem>().Count(x => x.CurrencyPK == item.CurrencyPK) > 1)
					{
						item.AddRowError(Res.GetString("EA6B0D1B-8CF9-488C-AE8A-990327611DF2", "Settings for following Currency already exist - {0}", item.Currency.RX_Code));
					}
				}
			}
		}

		public RefCurrency Currency
		{
			get { return (RefCurrency)CurrentFactory.Load(typeof(RefCurrency), currencyPK); }
		}

		public ZPropertyInfo CurrencyPKInfo
		{
			get { return GetZPropertyInfo(Schema.CurrencyPK); }
		}

		public RefCurrencyCollection Currencies
		{
			get
			{
				return new RefCurrencyCollection(CurrentFactory);
			}
		}

		ZBool comprehensiveReportEnabled;
		public ZBool ComprehensiveReportEnabled
		{
			get => comprehensiveReportEnabled;
			set
			{
				SetNonPersistentPropertyValue(ComprehensiveReportEnabledInfo, ref comprehensiveReportEnabled, value);
				ClearRowNotifications();
				if (!comprehensiveReportEnabled)
				{
					ValidateAtLeastOneReportTypeSelected();
				}
			}
		}

		public ZPropertyInfo ComprehensiveReportEnabledInfo => GetZPropertyInfo(nameof(ComprehensiveReportEnabled));

		ZBool failureRiskEnabled;
		public ZBool FailureRiskEnabled
		{
			get => failureRiskEnabled;
			set
			{
				SetNonPersistentPropertyValue(FailureRiskEnabledInfo, ref failureRiskEnabled, value);
				ClearRowNotifications();
				if (!failureRiskEnabled)
				{
					ValidateAtLeastOneReportTypeSelected();
				}
			}
		}

		public ZPropertyInfo FailureRiskEnabledInfo => GetZPropertyInfo(nameof(FailureRiskEnabled));

		ZBool latePaymentRiskEnabled;
		public ZBool LatePaymentRiskEnabled
		{
			get => latePaymentRiskEnabled;
			set
			{
				SetNonPersistentPropertyValue(LatePaymentRiskEnabledInfo, ref latePaymentRiskEnabled, value);
				ClearRowNotifications();
				if (!latePaymentRiskEnabled)
				{
					ValidateAtLeastOneReportTypeSelected();
				}
			}
		}

		public ZPropertyInfo LatePaymentRiskEnabledInfo => GetZPropertyInfo(nameof(LatePaymentRiskEnabled));

		ZBool commercialBureauEnquiryEnabled;
		public ZBool CommercialBureauEnquiryEnabled
		{
			get => commercialBureauEnquiryEnabled;
			set
			{
				SetNonPersistentPropertyValue(CommercialBureauEnquiryEnabledInfo, ref commercialBureauEnquiryEnabled, value);
				ClearRowNotifications();
				if (!commercialBureauEnquiryEnabled)
				{
					ValidateAtLeastOneReportTypeSelected();
				}
			}
		}

		public ZPropertyInfo CommercialBureauEnquiryEnabledInfo => GetZPropertyInfo(nameof(CommercialBureauEnquiryEnabled));

		void ValidateAtLeastOneReportTypeSelected()
		{
			if (!ComprehensiveReportEnabled && !FailureRiskEnabled && !LatePaymentRiskEnabled && !CommercialBureauEnquiryEnabled)
			{
				AddRowError(Res.GetString("04A669B3-CC9C-4B65-93A4-BBB1FBF1F566", "Please select at least 1 Report Type"));
			}
		}

		public BusinessObjectCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (MaximumCreditLimitCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}

				return null;
			}
		}

		#endregion

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			CreditLimit = new ZString(reader.ReadElementString(Schema.CreditLimit));
			CurrencyPK = new ZGuid(reader.ReadElementString(Schema.CurrencyPK));
			ComprehensiveReportEnabled = new ZBool(reader.ReadElementString(Schema.ComprehensiveReportEnabled));
			FailureRiskEnabled = new ZBool(reader.ReadElementString(Schema.FailureRiskEnabled));
			LatePaymentRiskEnabled = new ZBool(reader.ReadElementString(Schema.LatePaymentRiskEnabled));
			CommercialBureauEnquiryEnabled = new ZBool(reader.ReadElementString(Schema.CommercialBureauEnquiryEnabled));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.CreditLimit, CreditLimit);
			writer.WriteElementString(Schema.CurrencyPK, CurrencyPK.ToString());
			writer.WriteElementString(Schema.ComprehensiveReportEnabled, ComprehensiveReportEnabled.ToString());
			writer.WriteElementString(Schema.FailureRiskEnabled, FailureRiskEnabled.ToString());
			writer.WriteElementString(Schema.LatePaymentRiskEnabled, LatePaymentRiskEnabled.ToString());
			writer.WriteElementString(Schema.CommercialBureauEnquiryEnabled, CommercialBureauEnquiryEnabled.ToString());
		}

		protected override void RunPreSaveValidationCore()
		{
			ValidateCreditLimit();
			ValidateCurrencyPK();
			ValidateAtLeastOneReportTypeSelected();
			base.RunPreSaveValidationCore();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new MaximumCreditLimitItem(fallbackLevel, factory);
		}
	}
}
