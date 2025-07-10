using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	[DependentBusinessObject(typeof(JobDeclaration), "Invoices")]
	public class JobComInvoiceHeader : AutoNZJobComInvoiceHeader,
		Integration.Customs.NZ.IJobComInvoiceHeader,
		ICurrencyConverterDataProvider,
		IAddInfoWithSyncPropertySupporter,
		IAddInfoWithSyncProperty
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoNZJobComInvoiceHeader.Schema
		{
			public const string JZ_UpliftType = "JZ_UpliftType";
			public const string JZ_ExchangeRateIndicator = "JZ_ExchangeRateIndicator";
			public const string JZ_RN_NKDefaultExport = "JZ_RN_NKDefaultExport";
			public const string JZ_DefaultQualifiesForPreferentialDuty = "JZ_DefaultQualifiesForPreferentialDuty";
			public const string JZ_DefaultPreferentialCountryGroup = "JZ_DefaultPreferentialCountryGroup";
			public const string JZ_DefaultOriginRegion = "JZ_DefaultOriginRegion";
			public const string MiscSupplierName = "MiscSupplierName";
			public new const string JZ_OA_SellerAddress = "JZ_OA_SellerAddress";
		}

		#region NZ Typed Collections and Objects
		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			if (JobDeclaration != null && JobDeclaration.IsECIWriteoff)
			{
				return new JobComInvoiceHeaderValidationECIWriteOff(this);
			}

			return new JobComInvoiceHeaderValidationFormalEntry(this);
		}

		public new JobComInvoiceHeaderLookups Lookups
		{
			get { return (JobComInvoiceHeaderLookups)base.Lookups; }
		}

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups()
		{
			return new JobComInvoiceHeaderLookups(this);
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		public new JobComInvoiceLineViewCollection JobComInvoiceLines
		{
			get { return (JobComInvoiceLineViewCollection)base.JobComInvoiceLines; }
		}

		Integration.Customs.NZ.IJobComInvoiceLineViewCollection Integration.Customs.NZ.IJobComInvoiceHeader.JobComInvoiceLines => JobComInvoiceLines;

		protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			JobComInvoiceLineViewCollection result = null;
			if (JobDeclaration != null)
			{
				result = new JobComInvoiceLineViewCollection(this, JobDeclaration.InvoiceLines);
			}
			return result;
		}

		protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
		{
			InvoiceLineDependentCollection result = new InvoiceLineDependentCollection(this);
			result.Load();
			return new JobComInvoiceLineViewCollection(this, result);
		}

		[ChildEditable(true)]
		public new JobComInvChargeCollection<InvoiceCharge> Charges
		{
			get { return base.Charges as JobComInvChargeCollection<InvoiceCharge>; }
		}

		protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges()
		{
			return new JobComInvChargeCollection<InvoiceCharge>(this);
		}
		#endregion

		public override ZGuid JZ_JE
		{
			get { return base.JZ_JE; }
			set
			{
				bool hasChanged = JZ_JE != value;
				base.JZ_JE = value;
				if (!IsCopying)
				{
					if (hasChanged && !IsDataChangeSuspendedByFakeDeclaration)
					{
						JobComInvoiceLines.MarkAsNeedingValidation();

						JobDeclaration declaration = JobDeclaration;
						if (declaration != null && declaration.IsECIWriteoff)
						{
							declaration.JE_ECI_InvoiceAmountInfo.RefreshBinding();
						}
					}

					if (JZ_RX_NKInvoice_Currency.IsEmpty && JobDeclaration != null && !JobDeclaration.IsPersistent)
					{
						using (SuspendSettingHasChanges())
						using (GetValidationSuspender())
						{
							JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
						}
					}
				}
			}
		}

		protected override CurrencyConverter GetNewCurrencyConverter()
		{
			CurrencyConverter result = new CurrencyConverterWithFixedExchangeRatesDataProvider(Factory, this);

#if DEBUG
			if (Globals.IsTest)
			{
				if (JobDeclaration == null)
				{
					result = null;
				}
				else if (JobDeclaration.IsInTestMode)
				{
					result = new NZCurrencyConverterWithTestExchangeRates(JobDeclaration);
				}
			}
#endif

			return result;
		}

		#region ICurrencyConverterDataProvider Implementation

		ZDateTime ICurrencyConverterDataProvider.DateOfValuation
		{
			get { return JobDeclaration != null ? ((ICurrencyConverterDataProvider)JobDeclaration).DateOfValuation : ZDateTime.Today; }
		}
		GlbCompany ICurrencyConverterDataProvider.Company
		{
			get { return JobDeclaration != null ? ((ICurrencyConverterDataProvider)JobDeclaration).Company : GlbCompany.CurrentCompany; }
		}

		ZBool? ICurrencyConverterDataProvider.IsReciprocalOverride
		{
			get { return JobDeclaration != null ? ((ICurrencyConverterDataProvider)JobDeclaration).IsReciprocalOverride : null; }
		}

		ZString ICurrencyConverterDataProvider.LocalCurrencyCodeOverride
		{
			get { return JobDeclaration != null ? ((ICurrencyConverterDataProvider)JobDeclaration).LocalCurrencyCodeOverride : ZString.Empty; }
		}

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get { return JobDeclaration != null ? ((ICurrencyConverterDataProvider)JobDeclaration).MaximumDaysToFallback : 0; }
		}

		ZArchitecture.Core.ExchangeRateType ICurrencyConverterDataProvider.RateType
		{
			get { return JobDeclaration != null ? ((ICurrencyConverterDataProvider)JobDeclaration).RateType : ZArchitecture.Core.ExchangeRateType.Customs; }
		}

		#endregion

		protected override ZString LocalCurrencyCodeCore
		{
			get { return JobDeclaration.LocalCurrencyConstantCode; }
		}

		public override ZGuid JZ_OH_Supplier
		{
			get { return base.JZ_OH_Supplier; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobComInvoiceHeader.Schema.JZ_OH_Supplier) && base.JZ_OH_Supplier != value)
				{
					var gstCodeOfPreviousSupplier = GetSupplierGstCode();

					base.JZ_OH_Supplier = value;
					SetDefaultRelationshipIndicator();
					if (!MiscSupplierName.IsEmpty)
					{
						var cachedMiscOrgPK = JobDeclaration?.CachedMiscOrgPK;
						if (cachedMiscOrgPK == null || cachedMiscOrgPK.Value != value)
						{
							MiscSupplierName = ZString.Empty;
						}
					}

					if (JZ_SupplierGSTNumber.IsEmpty || JZ_SupplierGSTNumber == gstCodeOfPreviousSupplier)
					{
						JZ_SupplierGSTNumber = GetSupplierGstCode();
					}
				}
			}
		}

		ZString GetSupplierGstCode()
		{
			var gstCode = Supplier?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.GSTCode, Core.Constants.CountryCodes.NewZealand) ?? ZString.Empty;
			if (!GSTNumberValidation.ValidateGSTNumber(gstCode))
			{
				gstCode = ZString.Empty;
			}
			return gstCode;
		}

		public override ZGuid JZ_OH_Buyer
		{
			get { return base.JZ_OH_Buyer; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobComInvoiceHeader.Schema.JZ_OH_Buyer) && base.JZ_OH_Buyer != value)
				{
					base.JZ_OH_Buyer = value;
					SetDefaultRelationshipIndicator();
				}
			}
		}

		protected void SetDefaultRelationshipIndicator()
		{
			if (JobDeclaration?.IsImport ?? false)
			{
				var link = SupplierBuyerLink;
				if (link != null && !link.OL_RelatedParty.IsEmpty)
				{
					JZ_RelationshipIndicator = link.OL_RelatedParty.Left(JZ_RelationshipIndicatorInfo.MaxLength);
				}
			}
		}

		#region Zero Rating Flags/Logic
		public override ZString JZ_IsZeroRatedDuty
		{
			get { return base.JZ_IsZeroRatedDuty; }
			set
			{
				ZString oldValue = base.JZ_IsZeroRatedDuty;
				base.JZ_IsZeroRatedDuty = value;
				if (value != oldValue)
				{
					foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
					{
						invoiceLine.ZeroRatedDutyDescriptionInfo.RefreshBinding();
					}

					if (!IsCopying)
					{
						ClearInvoiceLineValuesIfSame(JZ_IsZeroRatedDuty, JobComInvoiceLine.Schema.JI_IsZeroRatedDuty);
					}
				}
			}
		}

		public ZString ZeroRatedDutyDescription
		{
			get { return Lookups.YesNoList.GetDescriptionFromCode(EffectiveIsZeroRatedDutyAsString); }
		}

		public ZPropertyInfo ZeroRatedDutyDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ZeroRatedDutyDescription)); }
		}

		public ZString EffectiveIsZeroRatedDutyAsString
		{
			get { return JZ_IsZeroRatedDuty.IsEmpty && JobDeclaration != null ? JobDeclaration.JE_IsZeroRatedAll : JZ_IsZeroRatedDuty; }
		}

		public override ZString JZ_IsZeroRatedExcise
		{
			get { return base.JZ_IsZeroRatedExcise; }
			set
			{
				ZString oldValue = base.JZ_IsZeroRatedExcise;
				base.JZ_IsZeroRatedExcise = value;
				if (value != oldValue)
				{
					foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
					{
						invoiceLine.ZeroRatedExciseDescriptionInfo.RefreshBinding();
					}
					if (!IsCopying)
					{
						ClearInvoiceLineValuesIfSame(JZ_IsZeroRatedExcise, JobComInvoiceLine.Schema.JI_IsZeroRatedExcise);
					}
				}
			}
		}

		public ZString ZeroRatedExciseDescription
		{
			get { return Lookups.YesNoList.GetDescriptionFromCode(EffectiveIsZeroRatedExciseAsString); }
		}

		public ZPropertyInfo ZeroRatedExciseDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ZeroRatedExciseDescription)); }
		}

		public ZString EffectiveIsZeroRatedExciseAsString
		{
			get { return JZ_IsZeroRatedExcise.IsEmpty && JobDeclaration != null ? JobDeclaration.JE_IsZeroRatedAll : JZ_IsZeroRatedExcise; }
		}

		public override ZString JZ_IsZeroRatedLevies
		{
			get { return base.JZ_IsZeroRatedLevies; }
			set
			{
				ZString oldValue = base.JZ_IsZeroRatedLevies;
				base.JZ_IsZeroRatedLevies = value;
				if (value != oldValue)
				{
					foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
					{
						invoiceLine.ZeroRatedLeviesDescriptionInfo.RefreshBinding();
					}

					if (!IsCopying)
					{
						ClearInvoiceLineValuesIfSame(JZ_IsZeroRatedLevies, JobComInvoiceLine.Schema.JI_IsZeroRatedLevies);
					}
				}
			}
		}

		public ZString ZeroRatedLeviesDescription
		{
			get { return Lookups.YesNoList.GetDescriptionFromCode(EffectiveIsZeroRatedLeviesAsString); }
		}

		public ZPropertyInfo ZeroRatedLeviesDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ZeroRatedLeviesDescription)); }
		}

		public ZString EffectiveIsZeroRatedLeviesAsString
		{
			get { return JZ_IsZeroRatedLevies.IsEmpty && JobDeclaration != null ? JobDeclaration.JE_IsZeroRatedAll : JZ_IsZeroRatedLevies; }
		}

		public override ZString JZ_IsZeroRatedGST
		{
			get { return base.JZ_IsZeroRatedGST; }
			set
			{
				ZString oldValue = base.JZ_IsZeroRatedGST;
				base.JZ_IsZeroRatedGST = value;
				if (value != oldValue)
				{
					foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
					{
						invoiceLine.ZeroRatedGSTDescriptionInfo.RefreshBinding();
					}

					if (!IsCopying)
					{
						ClearInvoiceLineValuesIfSame(JZ_IsZeroRatedGST, JobComInvoiceLine.Schema.JI_IsZeroRatedGST);
					}
				}
			}
		}

		public ZString ZeroRatedGSTDescription
		{
			get { return Lookups.YesNoList.GetDescriptionFromCode(EffectiveIsZeroRatedGSTAsString); }
		}

		public ZPropertyInfo ZeroRatedGSTDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ZeroRatedGSTDescription)); }
		}

		public ZString EffectiveIsZeroRatedGSTAsString
		{
			get { return JZ_IsZeroRatedGST.IsEmpty && JobDeclaration != null ? JobDeclaration.JE_IsZeroRatedAll : JZ_IsZeroRatedGST; }
		}
		#endregion

		protected override ZBool IsJZ_InvoiceCurrExRateUserEnterableCore
		{
			get
			{
				return JZ_ExchangeRateIndicator == ExchangeRateIndicatorList.Codes.ForwardCover &&
					JobDeclaration != null && JobDeclaration.IsFormalEntry && JobDeclaration.IsExport;
			}
			set { throw new NotSupportedException("For NZ, this is a calculated property."); }
		}

		protected override void ResetToDefaultIsJZ_InvoiceCurrExRateUserEnterableIfPossible()
		{
		}

		#region Proxied AddInfo Properties
		public ZString JZ_ExchangeRateIndicator
		{
			get { return JZ_InvoiceCurrExRateType; }
			set { JZ_InvoiceCurrExRateType = value; }
		}

		public ZPropertyInfo JZ_ExchangeRateIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JZ_ExchangeRateIndicator, x => JZ_InvoiceCurrExRateTypeInfo); }
		}

		[ResourceStringData("NZJobComInvoiceHeader|JZ_RN_NKCountryOfExport", Caption = "Country/Region of Export")]
		[AddInfoSyncProperty("RN_NKCountryOfExport", typeof(ZString))]
		public override ZString JZ_RN_NKCountryOfExport
		{
			get => base.JZ_RN_NKCountryOfExport;
			set
			{
				base.JZ_RN_NKCountryOfExport = value;
				JobComInvoiceLines.MarkAsNeedingValidation();
			}
		}

		public ZString JZ_RN_NKDefaultExport
		{
			get { return JZ_RN_NKCountryOfExport; }
			set
			{
				ZString oldValue = JZ_RN_NKCountryOfExport;
				JZ_RN_NKCountryOfExport = value;
				if (value != oldValue)
				{
					foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
					{
						invoiceLine.Validation.ValidateJI_RN_NKCountryOfExport();
					}
					if (!IsCopying)
					{
						ClearInvoiceLineValuesIfSame(JZ_RN_NKDefaultExport, JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport);
					}
				}
			}
		}

		public ZPropertyInfo JZ_RN_NKDefaultExportInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JZ_RN_NKDefaultExport, x => JZ_RN_NKCountryOfExportInfo); }
		}

		public override ZString JZ_RN_NKDefaultOrigin
		{
			get { return base.JZ_RN_NKDefaultOrigin; }
			set
			{
				ZString oldValue = JZ_RN_NKDefaultOrigin;
				base.JZ_RN_NKDefaultOrigin = value;
				if (value != oldValue)
				{
					JobComInvoiceLines.MarkAsNeedingValidation();
					foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
					{
						invoiceLine.Validation.ValidateJI_CountryOfOrigin();
					}

					if (!IsCopying)
					{
						ClearInvoiceLineValuesIfSame(JZ_RN_NKDefaultOrigin, JobComInvoiceLine.Schema.JI_CountryOfOrigin);
					}
				}
			}
		}

		void ClearInvoiceLineValuesIfSame(IZType invoiceValue, string invoiceLineFieldName)
		{
			foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
			{
				IZType invoiceLineValue = (IZType)invoiceLine[invoiceLineFieldName];

				if (invoiceLineValue.Equals(invoiceValue))
				{
					using (invoiceLine.SuspendEffectiveValue(invoiceLineFieldName, invoiceValue))
					{
						invoiceLine[invoiceLineFieldName] = invoiceLineValue.Default;
					}

					ZPropertyInfo infoToRefresh = invoiceLine.ZPropertyInfoHash[invoiceLineFieldName];
					if (infoToRefresh != null)
					{
						infoToRefresh.RefreshBinding();
					}
				}
			}
		}

		public ZString JZ_DefaultOriginRegion
		{
			get { return JZ_OriginRegion; }
			set
			{
				ZString oldValue = JZ_DefaultOriginRegion;
				JZ_OriginRegion = value;

				if (!IsCopying && value != oldValue)
				{
					ClearInvoiceLineValuesIfSame(JZ_DefaultOriginRegion, JobComInvoiceLine.Schema.JI_OriginRegion);
				}
			}
		}

		public ZPropertyInfo JZ_DefaultOriginRegionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JZ_DefaultOriginRegion, x => JZ_OriginRegionInfo); }
		}

		[ResourceStringData("NZJobComInvoiceHeader|JZ_QualifiesForPreferentialDuty", Caption = "Qualifies For Preferential Duty")]
		public override ZString JZ_QualifiesForPreferentialDuty
		{
			get => base.JZ_QualifiesForPreferentialDuty;
			set => base.JZ_QualifiesForPreferentialDuty = value;
		}

		public ZString JZ_DefaultQualifiesForPreferentialDuty
		{
			get { return JZ_QualifiesForPreferentialDuty; }
			set
			{
				ZString oldValue = JZ_QualifiesForPreferentialDuty;
				JZ_QualifiesForPreferentialDuty = value;
				if (value != oldValue)
				{
					foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
					{
						invoiceLine.Validation.ValidateJI_QualifiesForPreferentialDuty();
					}

					if (!IsCopying)
					{
						ClearInvoiceLineValuesIfSame(JZ_DefaultQualifiesForPreferentialDuty, JobComInvoiceLine.Schema.JI_QualifiesForPreferentialDuty);
					}
				}
			}
		}

		public ZPropertyInfo JZ_DefaultQualifiesForPreferentialDutyInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JZ_DefaultQualifiesForPreferentialDuty, x => JZ_QualifiesForPreferentialDutyInfo); }
		}

		public ZString JZ_DefaultPreferentialCountryGroup
		{
			get { return JZ_PreferentialCountryGroup; }
			set
			{
				ZString oldValue = JZ_PreferentialCountryGroup;
				JZ_PreferentialCountryGroup = value;
				if (value != oldValue)
				{
					foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
					{
						invoiceLine.Validation.ValidateJI_PreferentialCountryGroup();
					}

					if (!IsCopying)
					{
						ClearInvoiceLineValuesIfSame(JZ_DefaultPreferentialCountryGroup, JobComInvoiceLine.Schema.JI_PreferentialCountryGroup);
					}
				}
			}
		}

		public ZPropertyInfo JZ_DefaultPreferentialCountryGroupInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JZ_DefaultPreferentialCountryGroup, x => JZ_PreferentialCountryGroupInfo); }
		}

		#region JZ_OA_SellerAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public new ZAddress JZ_OA_SellerAddress_ZAddress
		{
			get
			{
				if (fJZ_OA_SellerAddress_ZAddress == null)
				{
					fJZ_OA_SellerAddress_ZAddress = GetNewJZ_OA_SellerAddress_ZAddress();
					fJZ_OA_SellerAddress_ZAddress.IsOrgVisible = true;
				}
				return fJZ_OA_SellerAddress_ZAddress;
			}
		}
		ZAddress fJZ_OA_SellerAddress_ZAddress;

		protected new ZAddress GetNewJZ_OA_SellerAddress_ZAddress()
		{
			var result = new ZAddress(JZ_OA_SellerAddressInfo);
			result.DefaultAddressType = AddressType.OFC;
			result.GetDefaultAddress = header => GetMainAddressPK(header as OrgHeader);
			return result;
		}

		ZGuid GetMainAddressPK(OrgHeader header)
		{
			return header != null ? header.MainAddress.PK : ZGuid.Empty;
		}

		#endregion

		#endregion

		public bool IsECIWriteOff
		{
			get { return JobDeclaration != null && JobDeclaration.IsECIWriteoff; }
		}

		public override ZString JZ_RX_NKInvoice_Currency
		{
			get { return base.JZ_RX_NKInvoice_Currency; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JZ_RX_NKInvoice_Currency))
				{
					var oldValue = JZ_RX_NKInvoice_Currency;
					base.JZ_RX_NKInvoice_Currency = value;
					if (!IsCopying && JZ_RX_NKInvoice_Currency != oldValue)
					{
						DefaultExchangeRateIndicator();
						JobDeclaration?.MarkAsNeedingValidation();
					}
				}
			}
		}

		internal void DefaultExchangeRateIndicator()
		{
			RefCurrency invoiceCurrency = Invoice_Currency;
			if (invoiceCurrency == null)
			{
				if (JZ_RX_NKInvoice_Currency.IsEmpty)
				{
					JZ_ExchangeRateIndicator = ZString.Empty;
				}
			}
			else if (invoiceCurrency.RX_Code == Enterprise.Core.Constants.CurrencyCodes.NewZealand)
			{
				JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.NZD;
			}
			else
			{
				if (JZ_ExchangeRateIndicator == ExchangeRateIndicatorList.Codes.NZD)
				{
					JZ_ExchangeRateIndicator = ZString.Empty;
				}

				if (JobDeclaration != null)
				{
					if (JobDeclaration.IsExport)
					{
						if (!JZ_RX_NKInvoice_Currency.IsEmpty)
						{
							var otherInvoiceHeaderForTheSameCurrency = JobDeclaration.GetFirstInvoiceCurrencyIndicatorOfSameCurrency(invoiceCurrency.RX_Code, this);
							if (otherInvoiceHeaderForTheSameCurrency != null && !otherInvoiceHeaderForTheSameCurrency.JZ_ExchangeRateIndicator.IsEmpty)
							{
								JZ_ExchangeRateIndicator = otherInvoiceHeaderForTheSameCurrency.JZ_ExchangeRateIndicator;
							}
						}
					}
					else
					{
						JZ_ExchangeRateIndicator = ZString.Empty;
					}
				}
			}
		}

		protected override void SetSupplierFromDeclaration()
		{
			base.SetSupplierFromDeclaration();
			JobDeclaration declaration = JobDeclaration;
			if (declaration != null && declaration.JE_OH_Supplier == declaration.CachedMiscOrgPK)
			{
				MiscSupplierName = declaration.MiscSupplierName;
			}
		}

		public ZString MiscSupplierName
		{
			get { return JZ_SupplierName; }
			set { JZ_SupplierName = value.ToUpper(); }
		}

		public override ZString JZ_SupplierMiscFields
		{
			get { return SupplierIsMiscAndAllowedToBeMisc ? Schema.MiscSupplierName : ""; }
		}

		public bool SupplierIsMiscAndAllowedToBeMisc => Factory.GetValue(ref fSupplierIsMiscAndAllowedToBeMisc, delegate
		{
			JobDeclaration declaration = JobDeclaration;
			return declaration != null
				&& declaration.AllowsMiscSupplierAndImporter
				&& JZ_OH_Supplier == declaration.CachedMiscOrgPK;
		});

		CachedProperty<bool> fSupplierIsMiscAndAllowedToBeMisc;

		public override ZString SupplierName
		{
			get { return SupplierIsMiscAndAllowedToBeMisc ? MiscSupplierName : base.SupplierName; }
			set
			{
				if (SupplierIsMiscAndAllowedToBeMisc)
				{
					MiscSupplierName = value;
				}

				SupplierNameInfo.RefreshBinding();
			}
		}

		public override ZPropertyInfo SupplierNameInfo
		{
			get { return GetZPropertyInfo(nameof(SupplierName)); }
		}

		public int SupplierName_MaxLength
		{
			get { return JZ_SupplierNameInfo.MaxLength; }
		}

		protected bool SupplierName_ReadOnly
		{
			get { return !SupplierIsMiscAndAllowedToBeMisc; }
		}

		public ZDateTime DateForDutyRate
		{
			get { return JobDeclaration != null ? JobDeclaration.DateForDutyRate : ZDateTime.Today; }
		}

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext()
		{
			var dec = JobDeclaration;
			return dec == null ? base.GetStandaloneIncoTermAndChargeFactoryCountryContext() : ((IApportionInvoiceHolder)dec).CountryContext;
		}

		IAddInfoWithSyncProperty IAddInfoWithSyncPropertySupporter.AddInfo => this;

		void IAddInfoWithSyncProperty.EnableSynchronization() { }

		IZType IAddInfoWithSyncProperty.GetAddInfoValue(IZType data, Type addInfoValueType) =>
			(IZType)Activator.CreateInstance(addInfoValueType, data);

		ZPropertyInfo IAddInfoWithSyncProperty.AddInfoProperty => JZ_AddInfoInfo;
	}
}
