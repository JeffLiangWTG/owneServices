using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class AsycudaBill : ASYCUDA.Business.AsycudaBill, Integration.Customs.ASYCUDA.UYManifest.IAsycudaBill, ASYCUDA.Business.ISelectionItem
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaBill.Schema
		{
			public const string ABL_Transshipment = "ABL_Transshipment";
		}

		public static class UYConstants
		{
			public const string BooleanTrueString = "S";
			public const string CountryMapType = "CNTRY";
			public const string FreightForwarderCategory = "ACA";
			public const string HouseBillCode = "HWB";
			public const string ImportTypeCode = "0";
			public const string InterchangeCode = "WS_MANIFIESTO";
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
		public new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;
		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);
		protected override ZString GetCountryCode() => Core.Constants.CountryCodes.Uruguay;
		protected override Type GetPackTypeCore() => typeof(AsycudaPack);
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);

		#region Shipper

		public override ZString[] ShipperRegNoTypes() => RegNoTypes();

		#endregion

		#region Consignee

		public override ZString[] ConsigneeRegNoTypes() => RegNoTypes();

		#endregion

		#region NotifyParty

		public override ZString[] NotifyPartyRegNoTypes() => RegNoTypes();

		#endregion

		ZString[] RegNoTypes()
		{
			return new ZString[] { UruguayOrgCusCodeInfo.OrgCusCodes.RUT, UruguayOrgCusCodeInfo.OrgCusCodes.CID, OrgCusCode.CodeTypes.PassportID };
		}

		#region ABL_Transshipment

		[ResourceStringData("AsycudaBill.ABL_Transshipment", Caption = "Transshipment")]
		public ZBool ABL_Transshipment
		{
			get => this.GetSystemDefinedValue<ZBool>(Customs.Business.GenAddOnHelper.IsTransshipment);
			set
			{
				var oldValue = ABL_Transshipment;
				this.SetSystemDefinedValue(Customs.Business.GenAddOnHelper.IsTransshipment, value);
				ABL_TransshipmentInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ABL_TransshipmentInfo => GetZPropertyInfo(Schema.ABL_Transshipment);

		#endregion

		#region Overrided properties

		protected override bool ABL_BillStatus_ReadOnly => true;

		protected override bool CustomsEntryNumber_ReadOnly => true;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get => HasManifestBeenSubmittedToCustoms
				? ResString.GetMultilingualString("DF0D1F40-33E4-4D3C-A92E-0B89F1FD37D9", "This Bill is already registered with Customs.\r\nIf you need to cancel it from Customs, you must use the Cancel option from Manifest menu.")
				: base.ReasonForNotAbleToDelete;
		}

		#endregion

		string ASYCUDA.Business.ISelectionItem.SelectionDescription(bool showStatus)
		{
			var function = "";
			if (showStatus)
			{
				function = MessageStatusProvider?.AllowCancellationMessage(Header) ?? false ? (NoResString)" - Cancel" : (NoResString)" - Original";
			}
			return FormattableString.Invariant($"Bill Number - {ABL_BillNumber}{function}");
		}

		protected override ZBool ShouldSynchronisePaymentType() => ZBool.True;

		public bool CanSendOriginalMessage() => ABL_BillStatus == ZString.Empty || ABL_BillStatus == CustomsStatusList.Codes.ERR;
	}
}
