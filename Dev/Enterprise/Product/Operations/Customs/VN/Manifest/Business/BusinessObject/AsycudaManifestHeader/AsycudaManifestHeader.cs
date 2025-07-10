using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.VN.Manifest.Business
{
	public class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader, Integration.Customs.ASYCUDA.VNManifest.IAsycudaManifestHeader
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;

		[ResourceStringData("Enterprise.Customs.VN.Manifest.Business.AsycudaManifestHeader|RegistrationNumber", Caption = "Document Number", ShortCaption = "Document No")]
		public override ZString RegistrationNumber { get => base.RegistrationNumber; set => base.RegistrationNumber = value; }

		[ResourceStringData("Enterprise.Customs.VN.Manifest.Business.AsycudaManifestHeader|RegistrationYear", Caption = "Document Year", ShortCaption = "Year")]
		public override ZInt RegistrationYear { get => base.RegistrationYear; set => base.RegistrationYear = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AMA_ManifestType = VNManifestTypes.Codes.VSW;
			AMA_MessageStatus = MessageStatusCodeList.Codes.NotSent;
		}

		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.VietNam;
		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);
		protected override bool AMA_MessageStatus_ReadOnly => false;
		protected override bool RegistrationDetails_ReadOnly => false;
		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);
		protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(this);
		protected override Type GetBillTypeCore() => typeof(AsycudaBill);
	}
}
