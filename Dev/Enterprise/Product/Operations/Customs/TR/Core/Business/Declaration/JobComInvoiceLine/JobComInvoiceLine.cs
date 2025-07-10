using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class JobComInvoiceLine : AutoJobComInvoiceLine
		, Integration.Customs.TR.IJobComInvoiceLine
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static class Constants
		{
			public static class ReturningGoodsReasonCode
			{
				public const string Other = "0";
			}

			public static class NatureOfTransactionCode
			{
				public const string code_11 = "11";
			}
		}

		#region Statistical Value

		public ZDecimal JI_StatisticalValueUSD => Factory.GetValue(ref cachedStatisticalValueUSD, () => CurrencyConverter.ConvertExact(new Money(JI_Calc_StatisticalValue, LocalCurrency), USD).Amount);

		CachedProperty<ZDecimal> cachedStatisticalValueUSD;

		public ZGuid JI_CurrencyUSD => USD.PK;

		RefCurrency USD => Factory.GetCachedValue("RefCurrency_USD", () => RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates));

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ZG_UsedGoodsCode = UsedGoodsCodeList.Codes.K1;
			ZG_PriceType = PriceTypeList.Codes._01;
			JI_ValuationCode = Constants.NatureOfTransactionCode.code_11;
		}

		public override ZGuid JI_OA_ManufacturerAddress
		{
			get => GetEffectiveValueToReturn(base.JI_OA_ManufacturerAddress, Declaration.JE_OA_ManufacturerAddress);
			set
			{
				var oldValue = JI_OA_ManufacturerAddress;
				base.JI_OA_ManufacturerAddress = GetEffectiveValueToSet(value, Declaration.JE_OA_ManufacturerAddress);
			}
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			ApportionedCharges.SetReadOnlyIncludingChildren(false);
		}

		protected override ZAddress GetNewJI_OA_ManufacturerAddress_ZAddress()
		{
			var zAddress = base.GetNewJI_OA_ManufacturerAddress_ZAddress();

			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(GetMainAddress);
			return zAddress;
		}

		ZGuid GetMainAddress(IOrgHeader orgHeader)
		{
			ZGuid result = ZGuid.Empty;
			OrgHeader organisation = orgHeader as OrgHeader;
			if (organisation != null)
			{
				result = organisation.MainAddress.PK;
			}
			return result;
		}

		ZGuid GetEffectiveValueToReturn(ZGuid originalValue, ZGuid backupValue)
		{
			return originalValue.IsEmpty ? backupValue : originalValue;
		}

		ZGuid GetEffectiveValueToSet(ZGuid valuePassed, ZGuid backupValue)
		{
			var result = valuePassed;
			if (valuePassed == backupValue)
			{
				result = ZGuid.Empty;
			}
			return result;
		}

		#region Vehicles

		public new ICusVehicleCollection<CusVehicle, JobComInvoiceLine> Vehicles => (ICusVehicleCollection<CusVehicle, JobComInvoiceLine>)base.Vehicles;

		protected override ICusVehicleCollection<Customs.Business.CusVehicle, BaseJobComInvoiceLine> GetNewCusVehicleCollection() => new CusVehicleCollection<CusVehicle, JobComInvoiceLine>(this);

		public override VehicleRelationshipType VehicleRelationship => VehicleRelationshipType.Many;

		#endregion

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
		public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;
		public new JobComInvoiceLineLookups Lookups => (JobComInvoiceLineLookups)base.Lookups;
		public new JobComInvoiceLineValidation Validation => (JobComInvoiceLineValidation)base.Validation;
		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;
		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);
		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();

			result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			result[CusSupportingInfoTypeList.Codes.AviationFuelType] = typeof(AviationFuelType);
			return result;
		}

		protected override IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> CreateNewInvoiceLineApportionedChargeCollection() => new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>(this);

		protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection() => new InvoiceLineChargeCollection<InvoiceLineCharge>(this);
		public new InvoiceLineChargeCollection<InvoiceLineCharge> Charges => (InvoiceLineChargeCollection<InvoiceLineCharge>)base.Charges;

		#region AviationFuelType

		[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
		[ChildEditable(true)]
		public AviationFuelTypeCollection AviationFuelTypeCollection
		{
			get
			{
				if (fAviationFuelTypeCollection == null)
				{
					fAviationFuelTypeCollection = new AviationFuelTypeCollection(this);
					fAviationFuelTypeCollection.Load();
					RegisterEditableChildObject(fAviationFuelTypeCollection);
				}
				return fAviationFuelTypeCollection;
			}
		}
		AviationFuelTypeCollection fAviationFuelTypeCollection;

		#endregion

		protected override EU.Business.Declaration.JobComInvoiceLineTaxCollection CreateTaxCollection()
		{
			return new JobComInvoiceLineTaxCollection(this);
		}

		public new JobComInvoiceLineTaxCollection Taxes => (JobComInvoiceLineTaxCollection)base.Taxes;

		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups()
		{
			JobComInvoiceLineLookups result;
			if (IsImport)
			{
				result = new ImportJobComInvoiceLineLookups(this);
			}
			else if (IsExport)
			{
				result = new ExportJobComInvoiceLineLookups(this);
			}
			else
			{
				result = new JobComInvoiceLineLookups(this);
			}
			return result;
		}

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
		{
			JobComInvoiceLineValidation result;
			if (IsImport)
			{
				result = new ImportJobComInvoiceLineValidation(this);
			}
			else if (IsExport)
			{
				result = new ExportJobComInvoiceLineValidation(this);
			}
			else
			{
				result = new JobComInvoiceLineValidation(this);
			}
			return result;
		}

		protected override ZDecimal GetJI_Calc_CIF()
		{
			return ZDecimal.Zero; //TODO, We will calculate CIF in future work item. becasue TR is not EU, we cannot use EuCustomsValuationCalculator and EUIncoTermAndCustomsChargeFactory.
		}

		#region Entry-Exit Purpose Fields

		public bool IsEntryExitPurposeCodeVisibility
		{
			get
			{
				ZString[] allowedRejimCodes = { "2100", "3151", "5100", "5171", "6121", "6321", "6771" };
				return allowedRejimCodes.Contains(ProcedureCode);
			}
		}
		public bool IsEntryExitPurposeDetailVisibility => IsEntryExitPurposeCodeVisibility && ZG_EntryExitPurposeCode == EntryExitPurposeCodeList.Codes._05;

		#endregion

		public bool ReturningGoodsReasonCodeVisibility => ZG_ReturningGoodsReasonCode == Constants.ReturningGoodsReasonCode.Other;

		public override ZDecimal JI_NetWeight
		{
			get => base.JI_NetWeight;
			set
			{
				var oldValue = JI_NetWeight;
				if (oldValue != value)
				{
					base.JI_NetWeight = value;
					Declaration?.TotalCustomsQuantityInfo.RefreshBinding();
				}
			}
		}

		[DecimalPlaces(3)]
		public override ZDecimal JI_InvoiceQuantity { get => base.JI_InvoiceQuantity; set => base.JI_InvoiceQuantity = value; }

		[DecimalPlaces(3)]
		public override ZDecimal JI_CustomsQuantity { get => base.JI_CustomsQuantity; set => base.JI_CustomsQuantity = value; }

		[ResourceStringData("2F6B516A-92C7-4A92-A867-A54108C7818A", ShortCaption = "[36] Preference Code", Caption = "[36] Preference Code")]
		public override ZString JI_PrimaryPreference { get => base.JI_PrimaryPreference; set => base.JI_PrimaryPreference = value; }

		[DecimalPlaces(3)]
		[ResourceStringData("80B96672-C38B-48E6-8B04-150B6CE5E3E4", ShortCaption = "[41] Supporting Qty", Caption = "[41] Supporting Qty")]
		public override ZDecimal JI_CustomsSecondQuantity { get => base.JI_CustomsSecondQuantity; set => base.JI_CustomsSecondQuantity = value; }

		public override ZString UniversalTariffType => Universal.Constants.TariffTypes.HarmonizedSystem;

		[ResourceStringData("3F9EC91A-32A3-400D-9D45-FADE8FD8F01F", ShortCaption = "Turkish Desc.", Caption = "Turkish Description")]
		public override ZString JI_NDescription { get => base.JI_NDescription; set => base.JI_NDescription = value; }

		[ResourceStringData("1013D0D9-A5E2-4C26-BF80-E982D3EA5EA8", ShortCaption = "English Desc.", Caption = "English Description")]
		public override ZString JI_Description { get => base.JI_Description; set => base.JI_Description = value; }

		public ResourceStringData EntryExitPurposeCodeDependingMessageType
		{
			get { return (Declaration?.IsMiscellaneous ?? false) ? Res.GetData("317934D4-9368-4E9A-937D-B9B0450461E8", "Entry Purpose Code") : Res.GetData("0A53399D-0E20-44C3-886B-DAFDEDFE43D0", "Exit Purpose Code"); }
		}

		public ResourceStringData EntryExitPurposeDetailDependingMessageType
		{
			get { return (Declaration?.IsMiscellaneous ?? false) ? Res.GetData("EC04CF18-C3F1-4BD2-A834-872581CD414E", "Entry Purpose Detail") : Res.GetData("863A766E-4DFC-49A5-82D5-ECBDEACB291B", "Exit Purpose Detail"); }
		}

		protected override bool ShouldSetDescriptionWhenTariffChanges => false;

		[MaxLength(16)]
		public override ZString JI_Tariff
		{
			get => base.JI_Tariff;
			set
			{
				var oldValue = base.JI_Tariff;
				base.JI_Tariff = value;

				if (oldValue != JI_Tariff)
				{
					JI_NDescription = TariffDescription.Left(JobComInvoiceLine.Schema.JI_NDescriptionMaxLength).TrimEnd();
				}
			}
		}
		protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.Turkey;
		protected override Type TypeOfPartUsedCore => typeof(MasterFiles.OrgSupplierPart);

		protected override Customs.Business.TariffFormatter TariffFormatter
		{
			get { return new TariffFormatter(); }
		}

		#region AddInfo

		protected override EU.Business.Declaration.AddInfoJobComInvoiceLine GetNewAddInfo() => new AddInfoJobComInvoiceLine(JI_AddInfoInfo);

		protected new AddInfoJobComInvoiceLine AddInfo => base.AddInfo;

		public new AddInfoJobComInvoiceLineValidation AddInfoValidation => AddInfo.Validation;

		public new AddInfoJobComInvoiceLineLookups AddInfoLookups => AddInfo.Lookups;

		[ResourceStringData("F67395AD-41F7-4DB6-9C8A-5F584A867A2B", ShortCaption = "Used Good", Caption = "Used Good")]
		public override ZString ZG_UsedGoodsCode { get => base.ZG_UsedGoodsCode; set => base.ZG_UsedGoodsCode = value; }

		[ResourceStringData("DF3A617C-F502-4EA9-8CC1-6629777A0269", ShortCaption = "Ret.to Origin", Caption = "Return to Origin")]
		public override ZBool ZG_ReturnToOrigin { get => base.ZG_ReturnToOrigin; set => base.ZG_ReturnToOrigin = value; }

		[ResourceStringData("F1C6AD53-1AC4-4D43-902F-4D9A288E8285", ShortCaption = "Sec.Treat.Pro.", Caption = "Secondary Treated Product")]
		public override ZBool ZG_SecondaryTreatedProduct { get => base.ZG_SecondaryTreatedProduct; set => base.ZG_SecondaryTreatedProduct = value; }

		[ReadOnlyMember(nameof(IsNotImportOrExport))]
		[ResourceStringData("A17A79AA-E938-4D44-A8A3-E3D70C2B4419", ShortCaption = "Return Goods Reason", Caption = "Return Goods Reason")]
		public override ZString ZG_ReturningGoodsReasonCode
		{
			get => base.ZG_ReturningGoodsReasonCode;
			set
			{
				var oldValue = ZG_ReturningGoodsReasonCode;
				base.ZG_ReturningGoodsReasonCode = value;
				if (!IsCopying && ZG_ReturningGoodsReasonCode != oldValue && ZG_ReturningGoodsReasonCode != JobComInvoiceLine.Constants.ReturningGoodsReasonCode.Other)
				{
					ZG_ReturningGoodsReasonDetail = ZString.Empty;
				}
			}
		}

		[ResourceStringData("06D52F92-C1D9-4D18-BC6E-1D81DD19AB7B", ShortCaption = "Details", Caption = "Details")]
		public override ZString ZG_ReturningGoodsReasonDetail { get => base.ZG_ReturningGoodsReasonDetail; set => base.ZG_ReturningGoodsReasonDetail = value; }

		[ResourceStringData("C2A97F71-8524-48E0-8D2D-B351D0E4434F", ShortCaption = "Ex.Un.Pack Code", Caption = "Export Union Package Code")]
		public override ZString ZG_ExportUnionPackCode { get => base.ZG_ExportUnionPackCode; set => base.ZG_ExportUnionPackCode = value; }

		[ResourceStringData("192A1FFE-BA64-4D24-9021-C2693A945CCF", ShortCaption = "Thread Code", Caption = "Thread Code")]
		public override ZString ZG_ExportUnionThreadCode { get => base.ZG_ExportUnionThreadCode; set => base.ZG_ExportUnionThreadCode = value; }

		[ResourceStringData("794A9ECC-3E86-4E5F-BE15-DF78B2E9EDF5", ShortCaption = "Production Year", Caption = "Production Year")]
		public override ZShort ZG_ExportUnionProductionYear { get => base.ZG_ExportUnionProductionYear; set => base.ZG_ExportUnionProductionYear = value; }

		[ResourceStringData("33A6ECB3-9775-4D12-9A38-00F9DCA37774", ShortCaption = "Ecological", Caption = "Ecological")]
		public override ZBool ZG_ExportUnionEcological { get => base.ZG_ExportUnionEcological; set => base.ZG_ExportUnionEcological = value; }

		[ResourceStringData("1AA006FF-3A21-48DB-A835-28C28B38E2F3", ShortCaption = "Def.Instalment", Caption = "[48.] Deferred Instalment")]
		public override ZString ZG_ExportUnionDeferredInstallment { get => base.ZG_ExportUnionDeferredInstallment; set => base.ZG_ExportUnionDeferredInstallment = value; }

		[ReadOnlyMember(nameof(IsNotImportOrExport))]
		[ResourceStringData("8686551A-00BF-446D-A6D3-F2BAE811F2EB", ShortCaption = "Inward Pro. Line No", Caption = "Inward Processing Line No")]
		public override ZString ZG_InwardProcessingLicenseLineNumber { get => base.ZG_InwardProcessingLicenseLineNumber; set => base.ZG_InwardProcessingLicenseLineNumber = value; }

		public bool IsNotImportOrExport => !IsImport && !IsExport;

		[ResourceStringData("868E65D1-BEE9-409F-AB94-5BC9F266442E", ShortCaption = "Border Trade City", Caption = "Border Trade City")]
		public override ZString ZG_RW_NKBorderTradeStateCode { get => base.ZG_RW_NKBorderTradeStateCode; set => base.ZG_RW_NKBorderTradeStateCode = value; }

		[ResourceStringData("023918A2-8AC4-4716-913B-F13297D1805B", ShortCaption = "Additional Code", Caption = "Additional Code")]
		public override ZString ZG_ExportUnionAdditionalTariffCode { get => base.ZG_ExportUnionAdditionalTariffCode; set => base.ZG_ExportUnionAdditionalTariffCode = value; }

		[ResourceStringData("81992455-CDDB-420B-B690-F2FE278D4CF4", ShortCaption = "Excess Stock", Caption = "Excess Stock")]
		public override ZBool ZG_ExcessStock { get => base.ZG_ExcessStock; set => base.ZG_ExcessStock = value; }

		public override ZString ZG_EntryExitPurposeCode
		{
			get => base.ZG_EntryExitPurposeCode;
			set
			{
				var oldValue = ZG_EntryExitPurposeCode;
				base.ZG_EntryExitPurposeCode = value;
				if (!IsCopying && ZG_EntryExitPurposeCode != oldValue && ZG_EntryExitPurposeCode != EntryExitPurposeCodeList.Codes._05)
				{
					ZG_EntryExitPurposeDetail = ZString.Empty;
				}
			}
		}

		[ResourceStringData("0F92CA35-2544-4F34-8B88-8CD1954290AD", ShortCaption = "Entry/Exit P.Det.", Caption = "Entry/Exit Purpose Detail")]
		public override ZString ZG_EntryExitPurposeDetail { get => base.ZG_EntryExitPurposeDetail; set => base.ZG_EntryExitPurposeDetail = value; }

		[ResourceStringData("B27B63AA-E1EF-4C4F-AE12-49CB1833C514", ShortCaption = "Pro.Description", Caption = "Processing Description")]
		public override ZString ZG_ProcessingDescription { get => base.ZG_ProcessingDescription; set => base.ZG_ProcessingDescription = value; }

		[ResourceStringData("A9FA5E97-F06D-4817-BA7B-19AA2A07082F", ShortCaption = "[43] Valuation", Caption = "[43] Valuation Method")]
		public override ZString ZG_CommercialPaymentCode { get => base.ZG_CommercialPaymentCode; set => base.ZG_CommercialPaymentCode = value; }

		[ResourceStringData("DA5E73DE-6398-402F-A154-2A19505B38BD", ShortCaption = "Price Type", Caption = "Price Type")]
		public override ZString ZG_PriceType { get => base.ZG_PriceType; set => base.ZG_PriceType = value; }

		#endregion

		public override ZGuid JI_JZ
		{
			get => base.JI_JZ;
			set
			{
				var oldValue = base.JI_JZ;
				base.JI_JZ = value;
				if (!IsCopying && oldValue != JI_JZ)
				{
					AddInfo.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString JI_Procedure
		{
			get => base.JI_Procedure;
			set
			{
				var oldValue = base.JI_Procedure;
				base.JI_Procedure = value;
				if (oldValue != JI_Procedure && !IsCopying)
				{
					InvoiceHeader.MarkAsNeedingValidation();
					if (!IsEntryExitPurposeCodeVisibility)
					{
						ZG_EntryExitPurposeCode = ZString.Empty;
						ZG_EntryExitPurposeDetail = ZString.Empty;
					}
				}

				Declaration.IsActualImportInfo.RefreshBinding();
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.TaxOrFeeCodeList))]
		[ResourceStringData("E4D0598A-E8C7-446C-8E54-FA4923F91A94", ShortCaption = "VAT Code", Caption = "VAT Code")]
		public override ZString JI_ZZF_NKTaxType { get => base.JI_ZZF_NKTaxType; set => base.JI_ZZF_NKTaxType = value; }

		protected override int MaxLengthOfPrimaryPreference => AutoJobComInvoiceLine.Schema.JI_PrimaryPreferenceMaxLength;

		[MaxLength(20)]
		[ResourceStringData("3239EC5A-673D-4855-AC67-59B075240623", ShortCaption = "Prev.Dec.No", Caption = "Prev.Declaration No")]
		public override ZString JI_PreviousEntryNumber { get => base.JI_PreviousEntryNumber; set => base.JI_PreviousEntryNumber = value; }

		[ResourceStringData("9D842417-5415-40AF-AD78-11FC2821C287", ShortCaption = "Line No", Caption = "Prev.Dec.Line No")]
		public override ZShort JI_PreviousEntryLineNumber { get => base.JI_PreviousEntryLineNumber; set => base.JI_PreviousEntryLineNumber = value; }

		public bool IsPreviousEntryAvailable
		{
			get
			{
				var cusProcedure = CusProcedure;
				return (IsImport || IsExport) && cusProcedure != null && (cusProcedure.IsOutOfInwardProcessing()
					|| cusProcedure.IsIntoTemporaryExport() || cusProcedure.IsOutOfTemporaryExport()
					|| cusProcedure.IsIntoTemporaryImport() || cusProcedure.IsOutOfTemporaryImport()
					|| cusProcedure.IsIntoWarehouse() || cusProcedure.IsOutOfWarehouse());
			}
		}

		[DecimalPlaces(4)]
		[ResourceStringData("3E1EF4DA-245E-47ED-B91B-D8B04FB69852", Caption = "Calculation Qty 1", ShortCaption = "Calc. Qty 1")]
		public override ZDecimal JI_CustomsThirdQuantity { get => base.JI_CustomsThirdQuantity; set => base.JI_CustomsThirdQuantity = value; }

		[DecimalPlaces(4)]
		[ResourceStringData("A21E0621-BCF2-488F-9566-A13F041A8B49", Caption = "Calculation Qty 2", ShortCaption = "Calc. Qty 2")]
		public override ZDecimal JI_CustomsFourthQuantity { get => base.JI_CustomsFourthQuantity; set => base.JI_CustomsFourthQuantity = value; }

		[DecimalPlaces(4)]
		[ResourceStringData("5F0523B6-2159-48B2-AE69-91A4D7ADDFF1", Caption = "Calculation Qty 3", ShortCaption = "Calc. Qty 3")]
		public override ZDecimal JI_CustomsFifthQuantity { get => base.JI_CustomsFifthQuantity; set => base.JI_CustomsFifthQuantity = value; }

		protected override void SetDefaultTaxOrFeeCode()
		{
			var list = Lookups.TaxOrFeeCodeList;
			if (list.Count == 1)
			{
				JI_ZZF_NKTaxType = list[0].Code;
			}
			else
			{
				JI_ZZF_NKTaxType = ZString.Empty;
			}
		}

		[ResourceStringData("A0AFEA07-CB20-46E5-AD87-5848F9FC4221", ShortCaption = "[24] Tran. Nature", Caption = "The Nature of the transaction")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.NatureOfTransactionList))]
		[MaxLength(2)]
		public override ZString JI_ValuationCode { get => base.JI_ValuationCode; set => base.JI_ValuationCode = value; }

		public bool IsExemptFromStampDuty
		{
			get
			{
				var cusProcedure = CusProcedure;
				return  cusProcedure != null && (cusProcedure.IsIntoTemporaryImport()
					|| cusProcedure.IsIntoTemporaryExport() || cusProcedure.IsIntoInwardProcessing()
					|| cusProcedure.IsIntoOutwardProcessing() || cusProcedure.IsIntoWarehouse());
			}
		}
	}
}
