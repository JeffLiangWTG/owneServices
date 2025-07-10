using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader
		, Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader
		, Integration.Customs.ICusCodeDataTypeSupporter
		, IMessageAttachee
		, IRegistrationNoEntryProvider
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new partial class Schema : ASYCUDA.Business.AsycudaManifestHeader.Schema
		{
			public const string DepartureFlight = "DepartureFlight";
			public const int DepartureFlightMaxLength = 15;
			public const string DepartureCountryCode = "DepartureCountryCode";
			public const int DepartureCountryCodeMaxLength = 2;
			public const string TransshipmentCountry = "TransshipmentCountry";
			public const int TransshipmentCountryMaxLength = 2;
			public const string TransshipmentLocation = "TransshipmentLocation";
			public const int TransshipmentLocationMaxLength = 40;
			public const string TransshipmentReference = "TransshipmentReference";
			public const int TransshipmentReferenceMaxLength = 15;
			public const string TransshipmentConveyanceCountry = "TransshipmentConveyanceCountry";
			public const int TransshipmentConveyanceCountryMaxLength = 2;
			public const string PreviousContainerNo = "PreviousContainerNo";
			public const string NewContainerNo = "NewContainerNo";
			public const string ProcedureCode = "ProcedureCode";
			public const int ContainerNoMaxLength = 20;
			public const string TempRegNo = "TempRegNo";
			public const int TempRegNoMaxLength = 20;
			public const string TempRegNoDate = "TempRegNoDate";
			public const string DischargeRecordNo = "DischargeRecordNo";
			public const int DischargeRecordNoMaxLength = 20;
			public const string DischargeRecordNoDate = "DischargeRecordNoDate";
			public const string ClosureNo = "ClosureNo";
			public const int ClosureNoMaxLength = 40;
			public const string ClosureNoDate = "ClosureNoDate";
			public const string InspectionClerk = "InspectionClerk";
			public const int InspectionClerkMaxLength = 50;
			public const string TotalBoxQty = "TotalBoxQty";
			public const string NumberOfBills = "NumberOfBills";
			public const string PresentationCustomsOffice = "PresentationCustomsOffice";
			public const int PresentationCustomsOfficeMaxLength = 10;
			public const string ImportExportCustomsOffice = "ImportExportCustomsOffice";
			public const int ImportExportCustomsOfficeMaxLength = 10;
			public const string DischargeLoadingCustomsOffice = "DischargeLoadingCustomsOffice";
			public const int DischargeLoadingCustomsOfficeMaxLength = 10;
			public const string GoodsDescription = "GoodsDescription";
			public const int GoodsDescriptionMaxLength = 175;
			public const string CustomsValue = "CustomsValue";
			public const string CustomsValueCurrency = "CustomsValueCurrency";
			public const int CustomsValueCurrencyMaxLength = 3;
			public const string ExchangeRate = "ExchangeRate";
			public const string OtherValue = "OtherValue";
			public const string OtherValueCurrency = "OtherValueCurrency";
			public const int OtherValueCurrencyMaxLength = 3;
			public const string FreightValue = "FreightValue";
			public const string FreightValueCurrency = "FreightValueCurrency";
			public const int FreightValueCurrencyMaxLength = 3;
			public const string InsuranceValue = "InsuranceValue";
			public const string InsuranceValueCurrency = "InsuranceValueCurrency";
			public const int InsuranceValueCurrencyMaxLength = 3;
			public const string GuaranteeType = "GuaranteeType";
			public const int GuaranteeTypeMaxLength = 8;
			public const string GuaranteeRefNo = "GuaranteeRefNo";
			public const int GuaranteeRefNoMaxLength = 15;
			public const string GuaranteeAmount = "GuaranteeAmount";
			public const string GoodsLocationCode = "GoodsLocationCode";
			public const int GoodsLocationCodeMaxLength = 10;
			public const string LocationInformation = "LocationInformation";
			public const string StampTaxValue = "StampTaxValue";
			public const string MessageMode = "MessageMode";
			public const int MessageModeMaxLength = 3;
		}

		public static class Constants
		{
			public const string DefaultImportProcedureCode = "4000";
			public const string DefaultExportProcedureCode = "1000";
			public const string DefaultPortOfFirstArrival = "TRIST";
		}

		#region DepartureProperties

		[MaxLength(Schema.DepartureFlightMaxLength)]
		public ZString DepartureFlight
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.DepartureFlight);
			set
			{
				var oldValue = DepartureFlight;
				CheckMaximumLength(DepartureFlightInfo, value);
				this.SetSystemDefinedValue(Schema.DepartureFlight, value);
				SetPropertyValue(DepartureFlightInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDepartureFlight();
				}
				DepartureFlightInfo.RefreshBinding(oldValue);

				if (!IsCopying && AMA_Voyage.IsEmpty)
				{
					AMA_Voyage = DepartureFlight.SubstringSafe(0, Schema.AMA_VoyageMaxLength);
				}
			}
		}

		public ZPropertyInfo DepartureFlightInfo => GetZPropertyInfo(Schema.DepartureFlight);

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.DepartureCountryCode", Caption = "Departure Country Code", ShortCaption = "Dep.Ctry.Code")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CountryList))]
		[MaxLength(Schema.DepartureCountryCodeMaxLength)]
		public ZString DepartureCountryCode
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.DepartureCountryCode);
			set
			{
				var oldValue = DepartureCountryCode;
				CheckMaximumLength(DepartureCountryCodeInfo, value);
				this.SetSystemDefinedValue(Schema.DepartureCountryCode, value);
				SetPropertyValue(DepartureCountryCodeInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDepartureCountryCode();
				}
				DepartureCountryCodeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo DepartureCountryCodeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.DepartureCountryCode); }
		}

		#endregion

		#region TransshipmentProperties

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.TransshipmentCountry", Caption = "Transshipment Country", ShortCaption = "Trans.Country")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CountryList))]
		[MaxLength(Schema.TransshipmentCountryMaxLength)]
		public ZString TransshipmentCountry
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.TransshipmentCountry);
			set
			{
				var oldValue = TransshipmentCountry;
				CheckMaximumLength(TransshipmentCountryInfo, value);
				this.SetSystemDefinedValue(Schema.TransshipmentCountry, value);
				SetPropertyValue(TransshipmentCountryInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTransshipmentCountry();
				}
				TransshipmentCountryInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TransshipmentCountryInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.TransshipmentCountry); }
		}

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.TransshipmentConveyanceCountry", Caption = "Transshipment Conveyance Country/Region", ShortCaption = "Trans.Con.Ctry/Rgn.")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CountryList))]
		[MaxLength(Schema.TransshipmentConveyanceCountryMaxLength)]
		public ZString TransshipmentConveyanceCountry
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.TransshipmentConveyanceCountry);
			set
			{
				var oldValue = TransshipmentConveyanceCountry;
				CheckMaximumLength(TransshipmentConveyanceCountryInfo, value);
				this.SetSystemDefinedValue(Schema.TransshipmentConveyanceCountry, value);
				SetPropertyValue(TransshipmentConveyanceCountryInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTransshipmentConveyanceCountry();
				}
				TransshipmentConveyanceCountryInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TransshipmentConveyanceCountryInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.TransshipmentConveyanceCountry); }
		}

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.TransshipmentLocation", Caption = "Transshipment Location", ShortCaption = "Trans.Location")]
		[MaxLength(Schema.TransshipmentLocationMaxLength)]
		public ZString TransshipmentLocation
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.TransshipmentLocation);
			set
			{
				var oldValue = TransshipmentLocation;
				CheckMaximumLength(TransshipmentLocationInfo, value);
				this.SetSystemDefinedValue(Schema.TransshipmentLocation, value);
				SetPropertyValue(TransshipmentLocationInfo, value);
				TransshipmentLocationInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TransshipmentLocationInfo => GetZPropertyInfo(Schema.TransshipmentLocation);

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.TransshipmentReference", Caption = "Transshipment Reference", ShortCaption = "Trans.Ref.")]
		[MaxLength(Schema.TransshipmentReferenceMaxLength)]
		public ZString TransshipmentReference
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.TransshipmentReference);
			set
			{
				var oldValue = TransshipmentReference;
				CheckMaximumLength(TransshipmentReferenceInfo, value);
				this.SetSystemDefinedValue(Schema.TransshipmentReference, value);
				SetPropertyValue(TransshipmentReferenceInfo, value);
				TransshipmentReferenceInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TransshipmentReferenceInfo => GetZPropertyInfo(Schema.TransshipmentReference);

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			AMA_ApplicationCode = ApplicationCodeTypeList.Codes.TRETrade;
			AMA_ManifestType = TRETradeManifestTypes.Codes.TRETrade;
			AMA_TransportMode = TransportTypeList.Codes.Air;
			AMA_Nature = ShipmentTypeList.Codes.Import23;
			AMA_AgentType = Core.Constants.AgentType.Courier;
			AMA_OA_Carrier = GlbCompany.CurrentCompany?.OrgProxy?.MainAddress?.PK ?? ZGuid.Empty;
		}

		protected override ZString GetDefaultCountryCode()
		{
			return Core.Constants.CountryCodes.Turkey;
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = Enterprise.Customs.TR.ETrade.Business.Res.GetString("092FAC50-7BCB-4DA4-852C-34F16EC82375", "E-Trade");
				if (!AMA_JobReference.IsEmpty)
				{
					result += " " + AMA_JobReference;
				}
				return result;
			}
		}

		public override bool HasContainers => false;

		public override ResourceStringData VoyageFlightNoLabel
		{
			get
			{
				if (IsSea)
				{
					return Res.GetData("9d160684-493a-444f-84c8-c44e60f628e2", "Border Voyage");
				}
				else if (IsRoad)
				{
					return Res.GetData("895c5c3d-dbfe-4581-ba90-888527e196f8", "Border Truck Ref");
				}
				else
				{
					return Res.GetData("50195746-4DC9-4D2B-80B9-E1E497AC8611", "Border Flight");
				}
			}
		}

		public ResourceStringData DepartureFlightLabel
		{
			get
			{
				if (IsSea)
				{
					return Res.GetData("54d0b37c-cac9-4bff-8096-ea81c9b974bd", "Departure Voyage");
				}
				else if (IsRoad)
				{
					return Res.GetData("99fa3ed8-3993-403b-a27d-e733c103a3e7", "Departure Truck Ref");
				}
				else
				{
					return Res.GetData("62ed0997-a54f-46a1-8fa9-223ba6420d65", "Departure Flight");
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.AMA_CustomsOffice", Caption = "Arrival Customs Office", ShortCaption = "Arrival. Cus. Off")]
		public override ZString AMA_CustomsOffice { get => base.AMA_CustomsOffice; set => base.AMA_CustomsOffice = value; }

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.AMA_DateAtCustomsOffice", Caption = "Arrival Date")]
		public override ZDateTime AMA_DateAtCustomsOffice
		{
			get => base.AMA_DateAtCustomsOffice;
			set
			{
				var oldValue = AMA_DateAtCustomsOffice;
				base.AMA_DateAtCustomsOffice = value;
				if (oldValue != AMA_DateAtCustomsOffice && !IsCopying)
				{
					ReCalcTotalCustomsValue();
					ReCalcExchangeRate();
					ReCalcFreightValue();
					ReCalcInsuranceValue();
					ReCalcStatisticalValue();
					Bills?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.AMA_RN_NKConveyanceNationality", Caption = "Vessel Country", ShortCaption = "Vessel Ctry.")]
		public override ZString AMA_RN_NKConveyanceNationality
		{
			get => base.AMA_RN_NKConveyanceNationality;
			set
			{
				var oldValue = AMA_RN_NKConveyanceNationality;
				base.AMA_RN_NKConveyanceNationality = value;
				if (oldValue != AMA_RN_NKConveyanceNationality && !IsCopying)
				{
					Bills?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString AMA_RL_NKPortOfFirstArrival
		{
			get => base.AMA_RL_NKPortOfFirstArrival;
			set
			{
				var oldValue = AMA_RL_NKPortOfFirstArrival;
				base.AMA_RL_NKPortOfFirstArrival = value;
				if (oldValue != AMA_RL_NKPortOfFirstArrival && !IsCopying)
				{
					Bills?.MarkAsNeedingValidation();
				}
			}
		}

		public new AsycudaBillCollection Bills => (AsycudaBillCollection)base.Bills;
		protected override IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollection(this);
		protected override ASYCUDA.Business.AsycudaManifestHeaderDocWrapper GetDocWrapperCore() => new AsycudaManifestHeaderDocWrapper(this);

		public void CalculateDuties()
		{
			CalculateStamp();
			if (AMA_Nature == ShipmentTypeList.Codes.Import23)
			{
				foreach (AsycudaBill bill in Bills)
				{
					bill.CalculateCustomsDuty();
					bill.CalculateBanderolDuty();
				}
			}
		}

		void CalculateStamp()
		{
			var oldValue = StampTaxValue;
			stampTaxValueCached = null;
			var masterBill = MasterBill as AsycudaBill;
			masterBill.CalculateStampTaxToMasterBill();
			StampTaxValueInfo.RefreshBinding(oldValue);
		}

		void ModifyBillsMessageStatus(ZString status)
		{
			foreach (AsycudaBill bill in Bills)
			{
				bill.ABL_MessageStatus = status;
			}
		}

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.StampTaxValue", Caption = "Stamp Tax", ShortCaption = "S.Tax.")]
		[DecimalPlaces(2)]
		public ZDecimal StampTaxValue => CachedValueHelper.GetValue(ref stampTaxValueCached, () => { return MasterBill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.StampTax)?.AET_ChargeAmount ?? ZDecimal.Zero; });

		CachedValue<ZDecimal> stampTaxValueCached;

		public ZPropertyInfo StampTaxValueInfo => GetZPropertyInfo(Schema.StampTaxValue);

		protected override Type GetBillTypeCore() => typeof(AsycudaBill);
		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;
		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);
		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);
		public override ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship => ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.One;

		public new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;

		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);
		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);

		public new ZString RegistrationStatusDescription => Lookups.RegistrationStatusList.GetDescriptionFromCode(RegistrationStatus);

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.TransportModeList))]
		public override ZString AMA_TransportMode
		{
			get => base.AMA_TransportMode;
			set
			{
				var oldValue = base.AMA_TransportMode;
				if (oldValue != value)
				{
					base.AMA_TransportMode = value;
					AMA_TransportModeInfo.RefreshBinding(oldValue);
					SetNatureIfRequired();
				}
			}
		}

		void SetNatureIfRequired()
		{
			if (!IsCopying)
			{
				var newValue = IsAir ? ShipmentTypeList.Codes.Import23 : ShipmentTypeList.Codes.Export22;
				if (AMA_Nature != newValue)
				{
					AMA_Nature = newValue;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.AMA_Nature", Caption = "Nature")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.Natures))]
		public override ZString AMA_Nature
		{
			get => base.AMA_Nature;
			set
			{
				var oldValue = base.AMA_Nature;
				if (oldValue != value)
				{
					base.AMA_Nature = value;
					AMA_NatureInfo.RefreshBinding(oldValue);
					if (AMA_Nature == ShipmentTypeList.Codes.Import23)
					{
						AMA_TransportMode = TransportTypeList.Codes.Air;
					}

					DefaultHeaderFields();
				}
			}
		}

		void DefaultHeaderFields()
		{
			AMA_DateAtCustomsOffice = ZDateTime.Today;

			if (IsImport)
			{
				ProcedureCode = Constants.DefaultImportProcedureCode;
				AMA_RN_NKConveyanceNationality = Core.Constants.CountryCodes.Turkey;
				AMA_RL_NKPortOfFirstArrival = Constants.DefaultPortOfFirstArrival;
			}
			else
			{
				ProcedureCode = Constants.DefaultExportProcedureCode;
				DepartureCountryCode = Core.Constants.CountryCodes.Turkey;
			}
		}

		public override ZString GetNewJobReference(BusinessObjectFactory factory)
		{
			var target = new ASYCUDA.Business.ManifestJobNumberGeneratorTarget();
			var generator = new NumberGenerator
			{
				Factory = factory,
				Context = new NumberGeneratorContext(),
				BaseFountain = Env.NumberFountains.TRETradeJobReference,
				FountainGetter = Env.NumberFountains.GetTREtradeJobReferenceGeneratorFountain,
				PrimaryTarget = target
			};
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.Generate();
			generator.EnforceMaxLengths();
			return target.Value.ToUpper();
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.OtherValue", Caption = "Domestic Expenditure Value", ShortCaption = "Expenditure")]
		[BusinessObjectTestExclude]
		public ZDecimal OtherValue
		{
			get { return CalcOtherValue; }
		}
		public ZPropertyInfo OtherValueInfo => GetZPropertyInfo(Schema.OtherValue);

		ZDecimal CalcOtherValue => CachedValueHelper.GetValue(ref otherValueCached, () => { return Bills.Cast<AsycudaBill>().Sum(x => x.ABL_OtherValue); });

		CachedValue<ZDecimal> otherValueCached;

		public void ReCalcOtherValue()
		{
			otherValueCached = null;
			OtherValueInfo.RefreshBinding();
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.OtherValueCurrency", Caption = "")]
		[MaxLength(Schema.OtherValueCurrencyMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CurrenciesList))]
		public ZString OtherValueCurrency => Core.Constants.CurrencyCodes.Turkey;

		public ZPropertyInfo OtherValueCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.OtherValueCurrency); }
		}

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.PresentationCustomsOffice", Caption = "Presentation Customs Office", ShortCaption = "Present. Cus. Off.")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CustomsOfficeList))]
		[MaxLength(Schema.PresentationCustomsOfficeMaxLength)]
		public ZString PresentationCustomsOffice
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.PresentationCustomsOffice);
			set
			{
				var oldValue = PresentationCustomsOffice;
				CheckMaximumLength(PresentationCustomsOfficeInfo, value);
				this.SetSystemDefinedValue(Schema.PresentationCustomsOffice, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePresentationCustomsOffice();
				}
				PresentationCustomsOfficeInfo.RefreshBinding(oldValue);

				if (!IsCopying)
				{
					var presentationCustomsOffice = PresentationCustomsOffice;
					if (ImportExportCustomsOffice.IsEmpty)
					{
						ImportExportCustomsOffice = presentationCustomsOffice;
					}

					if (DischargeLoadingCustomsOffice.IsEmpty)
					{
						DischargeLoadingCustomsOffice = presentationCustomsOffice;
					}

					if (IsImport && AMA_CustomsOffice.IsEmpty)
					{
						AMA_CustomsOffice = presentationCustomsOffice;
					}
				}
			}
		}
		public ZPropertyInfo PresentationCustomsOfficeInfo => GetZPropertyInfo(Schema.PresentationCustomsOffice);

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.ImportExportCustomsOffice", Caption = "Import Customs Office", ShortCaption = "Import.Cus.Off.")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CustomsOfficeList))]
		[MaxLength(Schema.ImportExportCustomsOfficeMaxLength)]
		public ZString ImportExportCustomsOffice
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.ImportExportCustomsOffice);
			set
			{
				var oldValue = ImportExportCustomsOffice;
				CheckMaximumLength(ImportExportCustomsOfficeInfo, value);
				this.SetSystemDefinedValue(Schema.ImportExportCustomsOffice, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateImportExportCustomsOffice();
				}
				ImportExportCustomsOfficeInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo ImportExportCustomsOfficeInfo => GetZPropertyInfo(Schema.ImportExportCustomsOffice);

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.DischargeLoadingCustomsOffice", Caption = "Discharge Customs Office", ShortCaption = "Discharge.Cus.Off.")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CustomsOfficeList))]
		[MaxLength(Schema.DischargeLoadingCustomsOfficeMaxLength)]
		public ZString DischargeLoadingCustomsOffice
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.DischargeLoadingCustomsOffice);
			set
			{
				var oldValue = DischargeLoadingCustomsOffice;
				CheckMaximumLength(DischargeLoadingCustomsOfficeInfo, value);
				this.SetSystemDefinedValue(Schema.DischargeLoadingCustomsOffice, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDischargeLoadingCustomsOffice();
				}
				DischargeLoadingCustomsOfficeInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo DischargeLoadingCustomsOfficeInfo => GetZPropertyInfo(Schema.DischargeLoadingCustomsOffice);

		[MaxLength(Schema.GoodsDescriptionMaxLength)]
		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.GoodsDescription", Caption = "Goods Description", ShortCaption = "Goods Description")]
		[BusinessObjectTestExclude]
		public ZString GoodsDescription
		{
			get { return MasterBill?.ABL_GoodsDescription ?? ZString.Empty; }
			set
			{
				var oldValue = GoodsDescription;
				CheckMaximumLength(GoodsDescriptionInfo, value);
				var masterBill = MasterBill;

				if (masterBill != null)
				{
					masterBill.ABL_GoodsDescription = value;
					masterBill.Validation.ValidateABL_GoodsDescription();
				}

				GoodsDescriptionInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo GoodsDescriptionInfo => GetWrappedZPropertyInfo(Schema.GoodsDescription, x => MasterBill.ABL_GoodsDescriptionInfo);
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CustomsMessageStatusList))]
		public override ZString AMA_MessageStatus
		{
			get => base.AMA_MessageStatus;
			set => base.AMA_MessageStatus = value;
		}

		#region ExchangeRate

		[DecimalPlaces(5)]
		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.ExchangeRate", Caption = "Exchange Rate (EUR)", ShortCaption = "Rate (EUR)")]
		public ZDecimal ExchangeRate => CachedValueHelper.GetValue(ref calcExchangeRateCached, () =>
					{
						return CurrencyHelper.GetExchangeRate(new RefCurrencyCurrencyConverter(Branch.Company, Factory, AMA_DateAtCustomsOffice, ZArchitecture.Core.ExchangeRateType.Customs, 0), Core.Constants.CurrencyCodes.EuropeanUnion);
					});

		CachedValue<ZDecimal> calcExchangeRateCached;

		public ZPropertyInfo ExchangeRateInfo => GetZPropertyInfo(Schema.ExchangeRate);

		void ReCalcExchangeRate()
		{
			calcExchangeRateCached = null;
			ExchangeRateInfo.RefreshBinding();
		}

		#endregion

		public ZDecimal ConvertUsingCustomsRate(ZDateTime dateForRate, ZDecimal amount, ZString originalCurrencyCode, ZString destinationCurrencyCode, GlbCompany company)
			=> CurrencyHelper.ConvertUsingCustomsRate(dateForRate, amount, originalCurrencyCode, destinationCurrencyCode, company, Factory);

		#region CustomsValue

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.CustomsValue", Caption = "Customs Value", ShortCaption = "Customs Value")]
		[BusinessObjectTestExclude]
		public ZDecimal CustomsValue
		{
			get => CalcCustomsValue;
		}
		public ZPropertyInfo CustomsValueInfo => GetZPropertyInfo(Schema.CustomsValue);

		protected ZDecimal CalcCustomsValue => CachedValueHelper.GetValue(ref calcCustomsValueCached, () =>
					{
						var total = ZDecimal.Zero;
						foreach (AsycudaBill bill in Bills)
						{
							total += bill.ABL_CustomsValue;
						}

						return total;
					});

		CachedValue<ZDecimal> calcCustomsValueCached;

		public void ReCalcTotalCustomsValue()
		{
			calcCustomsValueCached = null;
			CustomsValueInfo.RefreshBinding();
		}

		void ReCalcStatisticalValue()
		{
			foreach (AsycudaBill asycudaBill in Bills)
			{
				asycudaBill.ReCalcStatisticalValue();
			}
		}

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.CustomsValueCurrency", Caption = "")]
		[MaxLength(Schema.CustomsValueCurrencyMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CurrenciesList))]
		public ZString CustomsValueCurrency => Core.Constants.CurrencyCodes.EuropeanUnion;

		public ZPropertyInfo CustomsValueCurrencyInfo => GetZPropertyInfo(Schema.CustomsValueCurrency);

		#endregion

		#region FreightValue

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.FreightValue", Caption = "Freight Value", ShortCaption = "Freight Value")]
		[BusinessObjectTestExclude]
		public ZDecimal FreightValue
		{
			get => CalcFreightValue;
		}
		public ZPropertyInfo FreightValueInfo => GetZPropertyInfo(Schema.FreightValue);

		ZDecimal CalcFreightValue => CachedValueHelper.GetValue(ref calcFreightValueCached, () =>
					{
						var total = ZDecimal.Zero;
						foreach (AsycudaBill bill in Bills)
						{
							total += ConvertUsingCustomsRate(AMA_DateAtCustomsOffice, bill.ABL_TransportValue, bill.ABL_RX_NKTransportValueCurrency, Core.Constants.CurrencyCodes.EuropeanUnion, this.Branch?.Company) + bill.PrecedentFreightToDisplay;
						}
						return total;
					});

		CachedValue<ZDecimal> calcFreightValueCached;

		public void ReCalcFreightValue()
		{
			calcFreightValueCached = null;
			FreightValueInfo.RefreshBinding();
		}

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.FreightValueCurrency", Caption = "")]
		[MaxLength(Schema.FreightValueCurrencyMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CurrenciesList))]
		public ZString FreightValueCurrency => Core.Constants.CurrencyCodes.EuropeanUnion;

		public ZPropertyInfo FreightValueCurrencyInfo => GetZPropertyInfo(Schema.FreightValueCurrency);

		#endregion

		#region InsuranceValue

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.InsuranceValue", Caption = "Insurance Value", ShortCaption = "Insurance Value")]
		[BusinessObjectTestExclude]
		public ZDecimal InsuranceValue
		{
			get => CalcInsuranceValue;
		}
		public ZPropertyInfo InsuranceValueInfo => GetZPropertyInfo(Schema.InsuranceValue);

		ZDecimal CalcInsuranceValue => CachedValueHelper.GetValue(ref calcInsuranceValueCached, () =>
					{
						var total = ZDecimal.Zero;
						foreach (AsycudaBill bill in Bills)
						{
							total += ConvertUsingCustomsRate(AMA_DateAtCustomsOffice, bill.ABL_InsuranceValue, bill.ABL_RX_NKInsuranceValueCurrency, Core.Constants.CurrencyCodes.EuropeanUnion, this.Branch?.Company);
						}

						return total;
					});

		CachedValue<ZDecimal> calcInsuranceValueCached;

		public void ReCalcInsuranceValue()
		{
			calcInsuranceValueCached = null;
			InsuranceValueInfo.RefreshBinding();
		}

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.InsuranceValueCurrency", Caption = "")]
		[MaxLength(Schema.InsuranceValueCurrencyMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CurrenciesList))]
		public ZString InsuranceValueCurrency => Core.Constants.CurrencyCodes.EuropeanUnion;

		public ZPropertyInfo InsuranceValueCurrencyInfo => GetZPropertyInfo(Schema.InsuranceValueCurrency);

		#endregion

		#region Guarantee

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.GuaranteeType", Caption = "Guarantee Type")]
		[MaxLength(Schema.GuaranteeTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.BondTypeList))]
		public ZString GuaranteeType
		{
			get { return Guarantee?.PW_BondType ?? ZString.Empty; }
			set
			{
				var oldValue = GuaranteeType;
				if (oldValue != value)
				{
					CheckMaximumLength(GuaranteeTypeInfo, value);
					var guarantee = Guarantee ?? CreateCusBondDetailHeader();
					guarantee.PW_BondType = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateGuaranteeType();
					}
					GuaranteeTypeInfo.RefreshBinding(oldValue);
				}
				MarkAsNeedingValidation();
			}
		}

		public ZPropertyInfo GuaranteeTypeInfo => GetZPropertyInfo(Schema.GuaranteeType);

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.GuaranteeRefNo", Caption = "Guarantee Ref.No")]
		[MaxLength(Schema.GuaranteeRefNoMaxLength)]
		public ZString GuaranteeRefNo
		{
			get { return Guarantee?.PW_BondNumber ?? ZString.Empty; }
			set
			{
				var oldValue = GuaranteeRefNo;
				if (oldValue != value)
				{
					CheckMaximumLength(GuaranteeRefNoInfo, value);
					var guarantee = Guarantee ?? CreateCusBondDetailHeader();
					guarantee.PW_BondNumber = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateGuaranteeRefNo();
					}
					GuaranteeRefNoInfo.RefreshBinding(oldValue);
				}
				MarkAsNeedingValidation();
			}
		}
		public ZPropertyInfo GuaranteeRefNoInfo => GetZPropertyInfo(Schema.GuaranteeRefNo);

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.GuaranteeAmount", Caption = "Guarantee Amount (TRY)", ShortCaption = "G.Amount (TRY)")]
		public ZDecimal GuaranteeAmount
		{
			get { return Guarantee?.PW_BondAmount ?? ZDecimal.Zero; }
			set
			{
				var oldValue = GuaranteeAmount;
				if (oldValue != value)
				{
					var guarantee = Guarantee ?? CreateCusBondDetailHeader();
					guarantee.PW_BondAmount = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateGuaranteeAmount();
					}
					GuaranteeAmountInfo.RefreshBinding();
				}
				MarkAsNeedingValidation();
			}
		}
		public ZPropertyInfo GuaranteeAmountInfo => GetZPropertyInfo(Schema.GuaranteeAmount);

		[ChildEditable(true)]
		public CusBondDetailCollection<AsycudaManifestHeader> Guarantees
		{
			get
			{
				if (fBondDetails == null)
				{
					fBondDetails = new CusBondDetailCollection<AsycudaManifestHeader>(this);
					fBondDetails.Load();
					this.RegisterEditableChildObject(fBondDetails);
				}
				return fBondDetails;
			}
		}
		CusBondDetailCollection<AsycudaManifestHeader> fBondDetails;

		CusBondDetail Guarantee
		{
			get
			{
				if (fGuarantee == null || fGuarantee.IsDeleted)
				{
					fGuarantee = LoadCusBondDetailHeader();
				}
				return fGuarantee;
			}
		}
		CusBondDetail fGuarantee;

		CusBondDetail LoadCusBondDetailHeader()
		{
			return Guarantees.Cast<CusBondDetail>().FirstOrDefault(e => e.PW_ParentID == PK && !e.IsDeleted);
		}

		CusBondDetail CreateCusBondDetailHeader()
		{
			using (SuspendSettingHasChanges())
			using (SuspendMarkingAsNeedingValidation())
			{
				return Guarantees.AddNew();
			}
		}

		#endregion

		public ResourceStringData ImportExportCustomsOfficeLabel
		{
			get { return IsExport ? Res.GetData("8FDB3F7E-D667-4201-872A-B18F4FF84180", "Export Cus. Off") : Res.GetData("51BE3CD6-D0BB-461F-9165-231151B6E063", "Import Cus. Off"); }
		}

		public ResourceStringData DischargeLoadingCustomsOfficeLabel
		{
			get { return IsImport ? Res.GetData("9DB5D503-D311-495E-A9BC-DA80FC6F4B31", "Discharge Cus. Off") : Res.GetData("F79CC0C0-1663-4F58-9556-5008DBB85CDC", "Loading Cus. Off"); }
		}

		#region Container

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.PreviousContainerNo", Caption = "Previous Container No", ShortCaption = "Prev. Container No")]
		[MaxLength(Schema.ContainerNoMaxLength)]
		public ZString PreviousContainerNo
		{
			get { return PreviousContainer?.ACN_ContainerNumber ?? ZString.Empty; }
			set
			{
				var oldValue = PreviousContainerNo;
				if (oldValue != value && !IsCopying)
				{
					SetContainerNumber(PreviousContainer, value, PreviousContainerCode);
				}
				PreviousContainerNoInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PreviousContainerNoInfo => GetZPropertyInfo(Schema.PreviousContainerNo);

		public AsycudaContainer PreviousContainer
		{
			get
			{
				if (previousContainer == null || previousContainer.IsDeleted)
				{
					previousContainer = GetContainer(PreviousContainerCode);
				}
				return previousContainer;
			}
		}
		AsycudaContainer previousContainer;

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.NewContainerNo", Caption = "New Container No")]
		[MaxLength(Schema.ContainerNoMaxLength)]
		public ZString NewContainerNo
		{
			get { return NewContainer?.ACN_ContainerNumber ?? ZString.Empty; }
			set
			{
				var oldValue = NewContainerNo;
				if (oldValue != value && !IsCopying)
				{
					SetContainerNumber(NewContainer, value, NewContainerCode);
				}
				NewContainerNoInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo NewContainerNoInfo => GetZPropertyInfo(Schema.NewContainerNo);

		public AsycudaContainer NewContainer
		{
			get
			{
				if (newContainer == null || newContainer.IsDeleted)
				{
					newContainer = GetContainer(NewContainerCode);
				}
				return newContainer;
			}
		}
		AsycudaContainer newContainer;

		AsycudaContainer GetContainer(string containerLevel)
		{
			return Containers.Cast<AsycudaContainer>().FirstOrDefault(e => e.ContainerLevel == containerLevel && !e.IsDeleted);
		}

		AsycudaContainer CreateContainer(string containerLevel)
		{
			AsycudaContainer result = null;
			using (this.SuspendSettingHasChanges())
			using (this.SuspendMarkingAsNeedingValidation())
			{
				result = Containers.AddNew();
				result.ContainerLevel = containerLevel;
			}

			return result;
		}

		public const string PreviousContainerCode = "P";
		public const string NewContainerCode = "N";

		void SetContainerNumber(AsycudaContainer container, ZString containerNo, ZString containerLevel)
		{
			if (container == null)
			{
				container = CreateContainer(containerLevel);
			}

			if (container != null)
			{
				container.ACN_ContainerNumber = containerNo;
			}
		}

		#endregion

		#region Procedure

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.ProcedureCode", Caption = "Procedure Code")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.Procedures))]
		public ZString ProcedureCode
		{
			get => MasterBill?.ABL_Procedure ?? ZString.Empty;
			set
			{
				var oldValue = ProcedureCode;
				CheckMaximumLength(ProcedureCodeInfo, value);
				var masterBill = MasterBill;
				if (masterBill != null)
				{
					masterBill.ABL_Procedure = value;
					masterBill.Validation.ValidateABL_Procedure();
				}
				ProcedureCodeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ProcedureCodeInfo => GetWrappedZPropertyInfo(Schema.ProcedureCode, x => MasterBill.ABL_ProcedureInfo);
		#endregion

		#region ETradeData

		#region Properties

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.MessageModeList))]
		[MaxLength(Schema.MessageModeMaxLength)]
		public ZString MessageMode
		{
			get
			{
				var result = this.GetSystemDefinedValue<ZString>(Schema.MessageMode);
				if (result.IsEmpty)
				{
					result = TRMessageTypes.Codes.TRE;
				}
				return result;
			}
			set
			{
				var oldValue = MessageMode;
				CheckMaximumLength(MessageModeInfo, value);
				this.SetSystemDefinedValue(Schema.MessageMode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMessageMode();
				}
				MessageModeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo MessageModeInfo => GetZPropertyInfo(Schema.MessageMode);

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.TempRegNo", Caption = "Temp.Reg.No")]
		[MaxLength(Schema.TempRegNoMaxLength)]
		public ZString TempRegNo
		{
			get => TRGNOETradeData?.CY_Data ?? ZString.Empty;
			set
			{
				var oldValue = TempRegNo;
				if (oldValue != value && !value.IsEmpty)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, GetUpdatedMessage(oldValue, value, Schema.TempRegNo));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				CheckMaximumLength(TempRegNoInfo, value);
				var tradeData = TRGNOETradeData ?? CreateETradeData(CusCodeDataTypeList.Codes.TRGNO);
				tradeData.CY_Data = value;
				TempRegNoInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TempRegNoInfo => GetZPropertyInfo(Schema.TempRegNo);

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.TempRegNoDate", Caption = "Temp.Reg.No Date")]
		public ZDateTime TempRegNoDate
		{
			get => TRGNOETradeData?.CY_Date ?? ZDateTime.Empty;
			set
			{
				var oldValue = TempRegNoDate;
				if (oldValue != value && !value.IsEmpty)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, GetUpdatedMessage(oldValue.ToString(), value.ToString(), Schema.TempRegNoDate));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				var tradeData = TRGNOETradeData ?? CreateETradeData(CusCodeDataTypeList.Codes.TRGNO);
				tradeData.CY_Date = value;
				TempRegNoDateInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TempRegNoDateInfo => GetZPropertyInfo(Schema.TempRegNo);

		ETradeData TRGNOETradeData
		{
			get
			{
				if (trgnoETradeData == null || trgnoETradeData.IsDeleted)
				{
					trgnoETradeData = LoadETradeData(CusCodeDataTypeList.Codes.TRGNO);
				}
				return trgnoETradeData;
			}
		}
		ETradeData trgnoETradeData;

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.DischargeRecordNo", Caption = "Discharge Record No", ShortCaption = "Dis.Record No")]
		[MaxLength(Schema.DischargeRecordNoMaxLength)]
		public ZString DischargeRecordNo
		{
			get => DRNOETradeData?.CY_Data ?? ZString.Empty;
			set
			{
				var oldValue = DischargeRecordNo;
				if (oldValue != value && !value.IsEmpty)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, GetUpdatedMessage(oldValue, value, Schema.DischargeRecordNo));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				CheckMaximumLength(DischargeRecordNoInfo, value);
				var tradeData = DRNOETradeData ?? CreateETradeData(CusCodeDataTypeList.Codes.DRNO);
				tradeData.CY_Data = value;
				DischargeRecordNoInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo DischargeRecordNoInfo => GetZPropertyInfo(Schema.DischargeRecordNo);

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.DischargeRecordNoDate", Caption = "Dis.Record No Date")]
		public ZDateTime DischargeRecordNoDate
		{
			get => DRNOETradeData?.CY_Date ?? ZDateTime.Empty;
			set
			{
				var oldValue = DischargeRecordNoDate;
				if (oldValue != value && !value.IsEmpty)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, GetUpdatedMessage(oldValue.ToString(), value.ToString(), Schema.DischargeRecordNoDate));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				var tradeData = DRNOETradeData ?? CreateETradeData(CusCodeDataTypeList.Codes.DRNO);
				tradeData.CY_Date = value;
				DischargeRecordNoDateInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo DischargeRecordNoDateInfo => GetZPropertyInfo(Schema.DischargeRecordNoDate);

		ETradeData DRNOETradeData
		{
			get
			{
				if (drnoETradeData == null || drnoETradeData.IsDeleted)
				{
					drnoETradeData = LoadETradeData(CusCodeDataTypeList.Codes.DRNO);
				}
				return drnoETradeData;
			}
		}
		ETradeData drnoETradeData;

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.ClosureNo", Caption = "Closure No")]
		[MaxLength(Schema.ClosureNoMaxLength)]
		public ZString ClosureNo
		{
			get => CLNOETradeData?.CY_Data ?? ZString.Empty;
			set
			{
				var oldValue = ClosureNo;
				if (oldValue != value && !value.IsEmpty)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, GetUpdatedMessage(oldValue, value, Schema.ClosureNo));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				CheckMaximumLength(ClosureNoInfo, value);
				var tradeData = CLNOETradeData ?? CreateETradeData(CusCodeDataTypeList.Codes.CLNO);
				tradeData.CY_Data = value;
				ClosureNoInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo ClosureNoInfo => GetZPropertyInfo(Schema.ClosureNo);

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.ClosureNoDate", Caption = "Closure No Date")]
		public ZDateTime ClosureNoDate
		{
			get => CLNOETradeData?.CY_Date ?? ZDateTime.Empty;
			set
			{
				var oldValue = ClosureNoDate;
				if (oldValue != value && !value.IsEmpty)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, GetUpdatedMessage(oldValue.ToString(), value.ToString(), Schema.ClosureNoDate));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				var tradeData = CLNOETradeData ?? CreateETradeData(CusCodeDataTypeList.Codes.CLNO);
				tradeData.CY_Date = value;
				ClosureNoDateInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ClosureNoDateInfo => GetZPropertyInfo(Schema.ClosureNoDate);

		ETradeData CLNOETradeData
		{
			get
			{
				if (clnoETradeData == null || clnoETradeData.IsDeleted)
				{
					clnoETradeData = LoadETradeData(CusCodeDataTypeList.Codes.CLNO);
				}
				return clnoETradeData;
			}
		}
		ETradeData clnoETradeData;

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.InspectionClerk", Caption = "Inspection Clerk")]
		[MaxLength(Schema.InspectionClerkMaxLength)]
		public ZString InspectionClerk
		{
			get => INSCLKETradeData?.CY_Data ?? ZString.Empty;
			set
			{
				var oldValue = InspectionClerk;
				if (oldValue != value && !value.IsEmpty)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, GetUpdatedMessage(oldValue, value, Schema.InspectionClerk));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				CheckMaximumLength(InspectionClerkInfo, value);
				var tradeData = INSCLKETradeData ?? CreateETradeData(CusCodeDataTypeList.Codes.INSCLK);
				tradeData.CY_Data = value;
				InspectionClerkInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo InspectionClerkInfo => GetZPropertyInfo(Schema.InspectionClerk);

		ETradeData INSCLKETradeData
		{
			get
			{
				if (insclkETradeData == null || insclkETradeData.IsDeleted)
				{
					insclkETradeData = LoadETradeData(CusCodeDataTypeList.Codes.INSCLK);
				}
				return insclkETradeData;
			}
		}
		ETradeData insclkETradeData;

		#endregion

		#region CalcProperties

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.NumberOfBills", Caption = "Number of Bills")]
		public virtual ZInt NumberOfBills
		{
			get
			{
				if (numberOfBillsCached == null)
				{
					numberOfBillsCached = CalcNumberOfBills;
				}
				return numberOfBillsCached.Value;
			}
		}
		ZInt? numberOfBillsCached;

		public ZPropertyInfo NumberOfBillsInfo => GetZPropertyInfo(Schema.NumberOfBills);

		ZInt CalcNumberOfBills => Bills?.Count ?? ZInt.Zero;

		public void ReCalcNumberOfBills()
		{
			numberOfBillsCached = null;
			NumberOfBillsInfo.RefreshBinding();
		}

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.TotalBoxQty", Caption = "Total Box Qty")]
		public ZInt TotalBoxQty
		{
			get
			{
				if (totalBoxQtyCached == null)
				{
					totalBoxQtyCached = CalcTotalBoxQty;
				}
				return totalBoxQtyCached.Value;
			}
		}
		ZInt? totalBoxQtyCached;

		public ZPropertyInfo TotalBoxQtyInfo => GetZPropertyInfo(Schema.TotalBoxQty);

		ZInt CalcTotalBoxQty => Bills?.Cast<AsycudaBill>().Sum(x => x.ABL_ManifestQty) ?? ZInt.Zero;

		public void ReCalcTotalBoxQty()
		{
			totalBoxQtyCached = null;
			TotalBoxQtyInfo.RefreshBinding();
		}

		public override ZString AMA_ApplicationCode
		{
			get => base.AMA_ApplicationCode;
			set
			{
				var oldValue = AMA_ApplicationCode;
				base.AMA_ApplicationCode = value;

				if (!IsCopying && oldValue != AMA_ApplicationCode)
				{
					foreach (AsycudaBill bill in Bills)
					{
						bill.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ASYCUDA.Business.BaseMessageSendingNotificationHelper GetMessageSendingNotificationHelper()
		{
			return new TRMessageSendingNotificationHelper(this);
		}
		#endregion

		ETradeData LoadETradeData(string code)
		{
			return ETradeDatas.Cast<ETradeData>().FirstOrDefault(e => e.CY_ParentID == PK && e.CY_Code == code && !e.IsDeleted);
		}

		ETradeData CreateETradeData(string code)
		{
			using (SuspendSettingHasChanges())
			{
				var result = ETradeDatas.AddNew();
				using (result.SuspendSettingHasChanges())
				{
					result.CY_Code = code;
					result.CY_Type = ApplicationCodeTypeList.Codes.TRETrade;
				}
				return result;
			}
		}

		[ChildEditable(true)]
		public ETradeDataCollection ETradeDatas
		{
			get
			{
				if (fETradeDatas == null)
				{
					fETradeDatas = new ETradeDataCollection(this, ApplicationCodeTypeList.Codes.TRETrade);
					fETradeDatas.Load();
					RegisterEditableChildObject(fETradeDatas);
				}
				return fETradeDatas;
			}
		}
		ETradeDataCollection fETradeDatas;

		protected override IDictionary<ZString, Type> SupportedCusCodeDataTypes
		{
			get
			{
				var result = new Dictionary<ZString, Type>();
				result.Add(ApplicationCodeTypeList.Codes.TRETrade, typeof(ETradeData));
				return result;
			}
		}
		#endregion
		#region Goods Location Properties

		public override ZGuid AMA_OA_Carrier
		{
			get => base.AMA_OA_Carrier;
			set
			{
				var oldValue = base.AMA_OA_Carrier;
				base.AMA_OA_Carrier = value;
				if (oldValue != AMA_OA_Carrier && !IsCopying)
				{
					var customsCode = this.Carrier?.Header?.CustomsCodes;
					if (customsCode != null)
					{
						var cpwOrcptCode = customsCode.GetCustomsRegNo(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, AMA_RN_NKCountry, AMA_OA_Carrier).SubstringSafe(0, Schema.GoodsLocationCodeMaxLength);
						if (cpwOrcptCode.IsEmpty || GetRefCusCodeListCombined(cpwOrcptCode) == null)
						{
							cpwOrcptCode = customsCode.GetCustomsRegNo(OrgCusCode.CodeTypes.TerminalControlledPremisesID, AMA_RN_NKCountry, AMA_OA_Carrier).SubstringSafe(0, Schema.GoodsLocationCodeMaxLength);
						}
						this.GoodsLocationCode = cpwOrcptCode.IsEmpty || GetRefCusCodeListCombined(cpwOrcptCode) == null ? ZString.Empty : cpwOrcptCode;
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.GoodsLocationCode", Caption = "Goods Loc. Code")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.GoodsLocationCodeList))]
		[MaxLength(Schema.GoodsLocationCodeMaxLength)]
		[BusinessObjectTestExclude]
		public ZString GoodsLocationCode
		{
			get { return MasterBill?.ABL_GoodsLocation ?? ZString.Empty; }
			set
			{
				var oldValue = GoodsLocationCode;
				if (MasterBill != null)
				{
					MasterBill.ABL_GoodsLocation = value;
				}

				if (oldValue != GoodsLocationCode)
				{
					goodsLocationCode = GetRefCusCodeListCombined(GoodsLocationCode);
					LocationInformation = goodsLocationCode != null ? goodsLocationCode.ZZD_Description.SubstringSafe(0, AutoAsycudaBill.Schema.ABL_LocationInformationMaxLength) : ZString.Empty;
					if (!IsValidationSuspended)
					{
						Validation.ValidateGoodsLocationCode();
					}
				}
				GoodsLocationCodeInfo.RefreshBinding(oldValue);
			}
		}
		ZZRefCusCodeListCombined goodsLocationCode;

		ZZRefCusCodeListCombined GetRefCusCodeListCombined(ZString code)
		{
			if (!code.IsEmpty)
			{
				var query = Lookups.GoodsLocationCodeList.CompleteFilter;
				query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, code);
				return Factory.Load<ZZRefCusCodeListCombined>(query).FirstOrDefault();
			}
			return null;
		}

		public ZPropertyInfo GoodsLocationCodeInfo
		{
			get { return GetZPropertyInfo(Schema.GoodsLocationCode); }
		}

		[ResourceStringData("Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader.LocationInformation", Caption = "Goods Location")]
		[BusinessObjectTestExclude]
		public ZString LocationInformation
		{
			get { return MasterBill?.ABL_LocationInformation ?? ZString.Empty; }
			set
			{
				var oldValue = LocationInformation;
				var masterBill = MasterBill;
				if (masterBill != null)
				{
					masterBill.ABL_LocationInformation = value;
					Validation.ValidateLocationInformation();
				}
				LocationInformationInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo LocationInformationInfo
		{
			get { return GetZPropertyInfo(Schema.LocationInformation); }
		}
		#endregion

		public ZDateTime EffectiveDateForDutyRate => Factory.GetValue(ref effectiveDateForDutyRateCached, () => ApplicationBusinessProvider.GetEffectiveDateForDutyRate(this));

		CachedProperty<ZDateTime> effectiveDateForDutyRateCached;

		public override ZGuid AMA_GB
		{
			get => base.AMA_GB;
			set
			{
				var oldValue = AMA_GB;
				base.AMA_GB = value;
				if (!IsCopying && oldValue != AMA_GB)
				{
					Bills?.MarkAsNeedingValidation();
				}
			}
		}

		#region ITRMessageHeader

		ZString IMessageAttachee.MessageStatus
		{
			get => AMA_MessageStatus;
			set
			{
				var oldValue = AMA_MessageStatus;
				AMA_MessageStatus = value;
				if (!IsCopying && oldValue != AMA_MessageStatus)
				{
					ModifyBillsMessageStatus(AMA_MessageStatus);
				}
			}
		}

		ZString IMessageAttachee.CustomsStatus { get => RegistrationStatus; set => RegistrationStatus = value; }

		ZGuid IMessageAttachee.GlobalBranchPK => AMA_GB;

		ZString IMessageAttachee.JobReference => AMA_JobReference;

		IBusinessObjectCollection IMessageAttachee.Messages => Messages;

		#endregion

		#region IAsycudaManifestHeader

		IBusinessObjectCollection Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader.Messages => Messages;

		ICollection Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader.Bills => Bills;

		#endregion

		#region IRegistrationNoEntryProvider

		ZString IRegistrationNoEntryProvider.RegistrationNumber { get => RegistrationNumber; set => RegistrationNumber = value; }
		ZDateTime IRegistrationNoEntryProvider.RegistrationDate { get => RegistrationDate; set => RegistrationDate = value; }
		SecurityCheckpoint IRegistrationNoEntryProvider.CanModifyRegistrationNumbers => Env.Security.TRETradeModifyRegistrationNumbers;
		BusinessObject IRegistrationNoEntryProvider.ParentBusinessObject => this;

		#endregion

		ZString GetUpdatedMessage(ZString originalValue, ZString newValue, ZString fieldName)
		{
			return ZString.Format((NoResString)"{0} has been updated. Was {1} Now {2}", fieldName, originalValue, newValue);
		}
	}
}
