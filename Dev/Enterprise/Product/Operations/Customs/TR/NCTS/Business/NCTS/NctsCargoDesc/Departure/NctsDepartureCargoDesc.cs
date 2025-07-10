using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsDepartureCargoDesc : EU.NCTS.Business.NctsDepartureCargoDesc
		, Integration.Customs.TR.IDepartureCargoDesc
	{
		public NctsDepartureCargoDesc(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new INctsDepartureCargoDescLookups Lookups => (INctsDepartureCargoDescLookups)base.Lookups;

		protected override EU.NCTS.Business.NctsCommonCargoDescLookups GetNewPhase4Lookups() => new NctsDepartureCargoDescPhase4Lookups(this);

		protected override EU.NCTS.Business.NctsCommonCargoDescLookups GetNewPhase5Lookups() => new NctsDepartureCargoDescPhase4Lookups(this);

		public new INctsDepartureCargoDescValidation Validation => (INctsDepartureCargoDescValidation)base.Validation;

		protected override EU.NCTS.Business.NctsDepartureCargoDescPhase4Validation GetNewPhase4Validation() => new NctsDepartureCargoDescPhase4Validation(this);

		protected override EU.NCTS.Business.NctsDepartureCargoDescPhase5Validation GetNewPhase5Validation() => new NctsDepartureCargoDescPhase5Validation(this);

		public new EU.NCTS.Business.INctsPreviousDocumentCollection<NctsPreviousDocument> PreviousDocuments => (EU.NCTS.Business.INctsPreviousDocumentCollection<NctsPreviousDocument>)base.PreviousDocuments;
		protected override EU.NCTS.Business.INctsPreviousDocumentCollection<EU.NCTS.Business.NctsPreviousDocument> GetPreviousDocuments() => new EU.NCTS.Business.NctsPreviousDocumentCollection<NctsPreviousDocument>(this);

		[ChildEditable(true)]
		public NctsExportToOpenCollection ExportToOpenList
		{
			get
			{
				if (exportToOpenList == null)
				{
					exportToOpenList = new NctsExportToOpenCollection(this, CusSupportingInfoTypeList.Codes.ETO);
					exportToOpenList.Load();
					RegisterEditableChildObject(exportToOpenList);
				}
				return exportToOpenList;
			}
		}
		NctsExportToOpenCollection exportToOpenList;

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var types = base.GetCusSupportingInfoTypes();
			types.Add(CusSupportingInfoTypeList.Codes.ETO, typeof(NctsExportToOpen));
			return types;
		}

		public new EU.NCTS.Business.INctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalInfos => (EU.NCTS.Business.INctsAdditionalInfoCollection<NctsAdditionalInfo>)base.AdditionalInfos;
		protected override EU.NCTS.Business.INctsAdditionalInfoCollection<EU.NCTS.Business.NctsAdditionalInfo> GetNctsAdditionalInfoCollection() => new EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);

		public new EU.NCTS.Business.INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments => (EU.NCTS.Business.INctsSupportingDocumentCollection<NctsSupportingDocument>)base.SupportingDocuments;
		protected override EU.NCTS.Business.INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetNewNctsSupportingDocumentCollection() => new EU.NCTS.Business.NctsSupportingDocumentCollection<NctsSupportingDocument>(this);

		public new EU.NCTS.Business.INctsPackageCollection<NctsPackage, NctsDepartureCargoDesc> Packages => (EU.NCTS.Business.INctsPackageCollection<NctsPackage, NctsDepartureCargoDesc>)base.Packages;
		protected override EU.NCTS.Business.INctsPackageCollection<EU.NCTS.Business.NctsPackage, EU.NCTS.Business.NctsCommonCargoDesc> GetNctsPackageCollection() => new EU.NCTS.Business.NctsPackageCollection<NctsPackage, NctsDepartureCargoDesc>(this);

		[ChildEditable(true)]
		public new CusInBondFeeCollection<NctsCargoDescFee> Fees => (CusInBondFeeCollection<NctsCargoDescFee>)base.Fees;
		protected override ICusInBondFeeCollection<EU.NCTS.Business.NctsCargoDescFee> GetNctsCargoDescFeeCollection() => new CusInBondFeeCollection<NctsCargoDescFee>(this);

		[MaxLength(280)]
		public override ZString BY_Description
		{
			get => base.BY_Description;
			set => base.BY_Description = value;
		}

		public CusEntryNumber ExportDeclaration
		{
			get
			{
				if (exportDeclaration == null || exportDeclaration.IsDeleted)
				{
					exportDeclaration = CusEntryNumber.LoadOrCreate(this, Core.Constants.CountryCodes.Turkey);
					exportDeclaration.CE_EntryIsSystemGenerated = false;
					RegisterEditableChildObject(exportDeclaration);
				}
				return exportDeclaration;
			}
		}
		CusEntryNumber exportDeclaration;

		[ResourceStringData("Enterprise.Customs.TR.Business.NctsMovementHeader|ExportDeclarationNumber", Caption = "Declaration No")]
		[MaxLength(20)]
		public ZString ExportDeclarationNumber
		{
			get
			{
				var exportDeclarationNumber = ExportDeclaration.CE_EntryNum;
				return exportDeclarationNumber;
			}
			set
			{
				CheckMaximumLength(ExportDeclarationNumberInfo, value);
				ExportDeclaration.CE_EntryNum = value;
				ExportDeclarationNumberInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ExportDeclarationNumberInfo => GetZPropertyInfo(nameof(ExportDeclarationNumber));

		[List(nameof(Lookups) + "." + nameof(INctsDepartureCargoDescLookups.ExportDeclarationTypeList))]
		[ResourceStringData("Enterprise.Customs.TR.Business.NctsMovementHeader|ExportDecleration", Caption = "Type")]
		[MaxLength(3)]
		public ZString ExportDeclarationType
		{
			get
			{
				var exportDeclarationType = ExportDeclaration.CE_EntryType;
				return exportDeclarationType;
			}
			set
			{
				ExportDeclaration.CE_EntryType = value;
				ExportDeclarationTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExportDeclarationTypeInfo => GetZPropertyInfo(nameof(ExportDeclarationType));

		[ResourceStringData("Enterprise.Customs.TR.Business.NctsMovementHeader|IsDeclarationPartial", Caption = "Partial?")]
		public ZBool IsDeclarationPartial
		{
			get
			{
				var isDeclarationPartial = ExportDeclaration.CE_Category == "1" ? (ZBool)true : (ZBool)false;
				return isDeclarationPartial;
			}
			set
			{
				if (value)
				{
					ExportDeclaration.CE_Category = "1";
				}
				else
				{
					ExportDeclaration.CE_Category = "";
				}
				IsDeclarationPartialInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsDeclarationPartialInfo => GetZPropertyInfo(nameof(IsDeclarationPartial));

		protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);
		protected override Type PreviousDocumentType => typeof(NctsPreviousDocument);
		protected override Type SupportingDocumentType => typeof(NctsSupportingDocument);

		protected override bool EnableLightValidationIfAvailable => false;

		public const string CommodityCodeType = "HSN";

		[DecimalPlaces(2)]
		public override ZDecimal BY_MonetaryValue { get => base.BY_MonetaryValue; set => base.BY_MonetaryValue = value; }

		[DecimalPlaces(3)]
		public override ZDecimal BY_GrossWeight { get => base.BY_GrossWeight; set => base.BY_GrossWeight = value; }

		[DecimalPlaces(3)]
		public override ZDecimal BY_NetWeight { get => base.BY_NetWeight; set => base.BY_NetWeight = value; }

		[DecimalPlaces(3)]
		public override ZDecimal BY_CustomsSecondQuantity { get => base.BY_CustomsSecondQuantity; set => base.BY_CustomsSecondQuantity = value; }
	}
}
