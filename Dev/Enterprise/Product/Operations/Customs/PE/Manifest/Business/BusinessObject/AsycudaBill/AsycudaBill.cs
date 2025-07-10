using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.PE.Manifest.Business
{
	public class AsycudaBill : ASYCUDA.Business.AsycudaBill, Integration.Customs.ASYCUDA.PEManifest.IAsycudaBill
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaBill.Schema
		{
			public const string CargoNature = "CargoNature";
			public const int CargoNatureMaxLength = 2;
			public const string CargoCondition = "CargoCondition";
			public const int CargoConditionMaxLength = 2;
		}

		protected override ZString GetCountryCode() => Core.Constants.CountryCodes.Peru;

		protected override ZBool ShouldSynchronisePaymentType() => true;

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;

		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);

		protected override Type GetPackTypeCore() => typeof(AsycudaPack);

		public new AsycudaPackCollection Packs => (AsycudaPackCollection)base.Packs;

		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new AsycudaPackCollection(this);

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);

		AsycudaBillValidationForRegularBill RegularBillValidation => Validation as AsycudaBillValidationForRegularBill;

		[ResourceStringData("EE003CB3-80A6-4D25-A319-A16083E91D97", Caption = "Issue Date")]
		public override ZDate ABL_BillIssueDate { get => base.ABL_BillIssueDate; set => base.ABL_BillIssueDate = value; }

		[MaxLength(Schema.CargoNatureMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CargoNatureList))]
		[ResourceStringData("AsycudaBill.CargoNature", Caption = "Cargo Nature")]
		public ZString CargoNature
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.CargoNature);
			set
			{
				var oldValue = CargoNature;
				CheckMaximumLength(CargoNatureInfo, value);
				this.SetSystemDefinedValue(Schema.CargoNature, value);
				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidateCargoNature();
				}
				CargoNatureInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CargoNatureInfo => GetZPropertyInfo(Schema.CargoNature);

		[MaxLength(Schema.CargoConditionMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CargoConditionList))]
		[ResourceStringData("AsycudaBill.CargoCondition", Caption = "Cargo Condition")]
		public ZString CargoCondition
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.CargoCondition);
			set
			{
				var oldValue = CargoCondition;
				CheckMaximumLength(CargoConditionInfo, value);
				this.SetSystemDefinedValue(Schema.CargoCondition, value);
				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidateCargoCondition();
				}
				CargoConditionInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CargoConditionInfo => GetZPropertyInfo(Schema.CargoCondition);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
		}

		protected override bool IsManifestUQNeedToConvertCore => false;

		ZString[] RegNoTypes()
		{
			return new ZString[] { OrgCusCode.PeruCodeTypes.GovernmentTaxFileCode, OrgCusCode.PeruCodeTypes.DNI, OrgCusCode.CodeTypes.PassportID };
		}

		public override ZString[] ShipperRegNoTypes() => RegNoTypes();

		public override ZString[] ConsigneeRegNoTypes() => RegNoTypes();

		public override ZString[] NotifyPartyRegNoTypes() => RegNoTypes();
	}
}
