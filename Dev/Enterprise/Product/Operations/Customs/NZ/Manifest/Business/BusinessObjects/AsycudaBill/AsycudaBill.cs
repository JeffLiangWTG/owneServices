using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public partial class AsycudaBill : ASYCUDA.Business.AsycudaBill, Integration.Customs.ASYCUDA.NZManifest.IAsycudaBill
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[MaxLength("CustomsEntryNumberMaxLength")]
		[ReadOnlyMember(nameof(CustomsEntryNumber_ReadOnly))]
		[ResourceStringData("NZAsycudaBill.CustomsEntryNumber", Caption = "Export Delivery Order Number", ShortCaption = "Export Order No.")]
		public override ZString CustomsEntryNumber
		{
			get => base.CustomsEntryNumber;
			set
			{
				base.CustomsEntryNumber = value;
				if (!CustomsEntryNumber.IsEmpty)
				{
					CustomsEntryNumberType = Common.NZ.CusEntryNumberTypeList.Codes.FormalEntry;
				}
			}
		}

		protected override void ImportManifestSpecificValues(BillOfLading source)
		{
			if (IsOCR
				&& !source.CustomsEntryNumber.IsEmpty
				&& (source.CustomsEntryNumberType == Common.NZ.CusEntryNumberTypeList.Codes.FormalEntry || source.CustomsEntryNumberType == Common.NZ.CusEntryNumberTypeList.Codes.OutwardReportNumber))
			{
				CustomsEntryNumber = source.CustomsEntryNumber;
			}
		}

		protected override void CalculateShipmentTypeCore(ASYCUDA.Business.AsycudaBill bill)
		{
			base.CalculateShipmentTypeCore(bill);

			if (IsOCR)
			{
				ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			}
		}

		public bool IsOCR => (Header?.AMA_ManifestType ?? ZString.Empty) == NZManifestTypes.Codes.OCR;

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
		public new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;
		protected override ZString GetCountryCode() => Core.Constants.CountryCodes.NewZealand;
		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);
		protected override Type GetPackTypeCore() => typeof(AsycudaPack);

		protected override Type GetPackageContainerLinkTypeCore() => typeof(AsycudaContainerBillOrPackageLink);

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ABL_BolType = Core.Constants.ShipmentTypes.CoLoadMaster;
		}
	}
}
