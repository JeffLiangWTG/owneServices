using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.Universal.Helper;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader, Integration.Customs.ASYCUDA.NZManifest.IAsycudaManifestHeader, INZManifestHeader
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("NZ.AsycudaManifestHeader.RegistrationNumber", Caption = "Entry Number", ShortCaption = "Ent. Number")]
		public override ZString RegistrationNumber { get => base.RegistrationNumber; set => base.RegistrationNumber = value; }

		[ResourceStringData("NZ.AsycudaManifestHeader.AMA_OA_DeconsolidateAddress", Caption = "Goods Location")]
		public override ZGuid AMA_OA_DeconsolidateAddress
		{
			get => base.AMA_OA_DeconsolidateAddress;
			set => base.AMA_OA_DeconsolidateAddress = value;
		}

		protected override ZBool IsDeconsolidatorEnabledCore => true;

		protected override void UpdateAsycudaBillOnManifestTypeChanged(ASYCUDA.Business.AsycudaBill bill, ZString manifestType)
		{
			base.UpdateAsycudaBillOnManifestTypeChanged(bill, manifestType);

			if (manifestType == NZManifestTypes.Codes.OCR)
			{
				bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			}
		}

		public new ASYCUDA.Business.IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> Bills => (ASYCUDA.Business.IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>)base.Bills;
		public new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;
		public new ASYCUDA.Business.CusPersonCollection<CusPerson, AsycudaManifestHeader> Persons => (ASYCUDA.Business.CusPersonCollection<CusPerson, AsycudaManifestHeader>)base.Persons;

		[ChildEditable]
		[ChildEditableTestExclude]
		public DeliveryNotificationParty DeliveryNotificationParty
		{
			get
			{
				if (deliveryNotificationParty is null)
				{
					deliveryNotificationParty = new DeliveryNotificationParty(this);
					RegisterEditableChildObject(deliveryNotificationParty);
				}
				return deliveryNotificationParty;
			}
		}
		DeliveryNotificationParty deliveryNotificationParty;

		protected override IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(this);
		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);
		protected override ASYCUDA.Business.CusPersonCollection CreateNewCusPersonCollection() => new ASYCUDA.Business.CusPersonCollection<CusPerson, AsycudaManifestHeader>(this);

		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;
		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.NewZealand;
		protected override Type GetBillTypeCore() => typeof(AsycudaBill);
		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);
		protected override Type GetPersonTypeCore() => typeof(CusPerson);

		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);

		#region INZManifestHeader Implementation

		ZString INZManifestHeader.JobName => "AsycudaManifestHeader"; // Only for program, not to show to the user.
		ZString INZManifestHeader.DocumentParentType => Core.Constants.DocManagerCodes.AsycudaManifest;
		Logs INZManifestHeader.Logs => Logs;
		ZString INZManifestHeader.MasterBillNumber => AMA_MasterBill;
		ZString INZManifestHeader.JobNumber => AMA_JobReference;
		CusEntryNumber INZManifestHeader.LoadAndCreateRegistrationNumber()
		{
			var registrationEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, AMA_RN_NKCountry);
			return registrationEntryNumber;
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			AMA_ManifestType = NZManifestTypes.Codes.ICR;
		}
#endif
	}
}
