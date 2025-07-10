using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ZA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public partial class AsycudaBill
	{
		public new class Schema : ManifestBase.AsycudaBill.Schema
		{
			public new const int ABL_BillNumberMaxLength = 10;
			public const string CargoReleaseStatus = "CargoReleaseStatus";
			public const int CargoReleaseStatusMaxLength = 2;
			public const string CargoReleaseStatusOtherDescription = "RelStatusDesc";
			public const int CargoReleaseStatusOtherDescriptionMaxLength = 70;
		}

		AsycudaBillValidationForRegularBill RegularBillValidation => Validation as AsycudaBillValidationForRegularBill;

		#region CargoReleaseStatus

		[MaxLength(Schema.CargoReleaseStatusMaxLength)]
		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CargoReleaseStatusList))]
		[ResourceStringData("ZAAsycudaBill.CargoReleaseStatus", ShortCaption = "Cargo Rel. Status", Caption = "Cargo Release Status")]
		public ZString CargoReleaseStatus
		{
			get => CusEntryNumber?.CE_EntryStatus ?? ZString.Empty;
			set
			{
				var cusEntryNum = CusEntryNumber;
				var oldValue = CargoReleaseStatus;
				CheckMaximumLength(CargoReleaseStatusInfo, value);
				if (!value.IsEmpty)
				{
					if (cusEntryNum == null)
					{
						cusEntryNumber = CustomsEntryNumbers.AddNew();
					}
					cusEntryNumber.CE_EntryStatus = value;
				}
				else
				{
					if (cusEntryNum != null)
					{
						cusEntryNum.CE_EntryStatus = value;
					}
				}

				CargoReleaseStatusInfo.RefreshBinding(oldValue);
				if (oldValue != CargoReleaseStatus)
				{
					ClearCargoReleaseStatusOtherDescriptionIfNeeded();
				}
				if (!IsValidationSuspended)
				{
					RegularBillValidation?.ValidateCargoReleaseStatus();
				}
			}
		}

		void ClearCargoReleaseStatusOtherDescriptionIfNeeded()
		{
			if (!IsCargoReleaseStatusOtherDescriptionVisible && !CargoReleaseStatusOtherDescription.IsEmpty)
			{
				CargoReleaseStatusOtherDescription = ZString.Empty;
			}
		}

		public ZPropertyInfo CargoReleaseStatusInfo => GetZPropertyInfo(Schema.CargoReleaseStatus);

		public ZBool IsCargoReleaseStatusVisible => (Header?.IsAQM ?? false);

		internal const string CargoReleaseStatus_Other = "51";

		#endregion

		#region CargoReleaseStatusDescription

		public ZString CargoReleaseStatusDescription => IsCargoReleaseStatusOtherDescriptionVisible
			? CargoReleaseStatusOtherDescription
			: (ZString)Lookups.CargoReleaseStatusList.GetDescriptionFromCode(CargoReleaseStatus);

		[MaxLength(Schema.CargoReleaseStatusOtherDescriptionMaxLength)]
		[ResourceStringData("ZAAsycudaBill.CargoReleaseStatusOtherDescription", Caption = "Release Status Description", ShortCaption = "Rel. Status Desc.")]
		public ZString CargoReleaseStatusOtherDescription
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.CargoReleaseStatusOtherDescription);
			set
			{
				var oldValue = CargoReleaseStatusOtherDescription;
				CheckMaximumLength(CargoReleaseStatusOtherDescriptionInfo, value);
				this.SetSystemDefinedValue(Schema.CargoReleaseStatusOtherDescription, value);
				CargoReleaseStatusOtherDescriptionInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					RegularBillValidation?.ValdiateCargoReleaseStatusOtherDescription();
				}
			}
		}

		public ZPropertyInfo CargoReleaseStatusOtherDescriptionInfo => GetZPropertyInfo(nameof(CargoReleaseStatusOtherDescription));

		public ZBool IsCargoReleaseStatusOtherDescriptionVisible => IsCargoReleaseStatusVisible && CargoReleaseStatus == CargoReleaseStatus_Other;

		#endregion

		[MaxLength(Schema.ABL_BillNumberMaxLength)]
		public override ZString ABL_BillIssuer
		{
			get => base.ABL_BillIssuer;
			set => base.ABL_BillIssuer = value;
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		protected override bool ABL_BillStatus_ReadOnly => true;

		protected override void DefaultOnCountryChanged()
		{
			base.DefaultOnCountryChanged();

			if (!ABL_JS_Shipment.IsEmpty)
			{
				DefaultBillIssuerFromShipment(Shipment);
			}
		}

		internal void DefaultBillIssuerFromShipment(ForwardingShipment shipment)
		{
			if (shipment != null && shipment.JS_ShipmentType == Core.Constants.ShipmentTypes.StandardHouse)
			{
				ABL_BillIssuer = (AsycudaManifestHeaderExtensions.ZaOrgProxyForManifestMessaging(Factory)?.CustomsCodes.GetCustomsRegNo(OrgCusCode.SouthAfricaCodeTypes.BillIssuer, CountryCode) ?? ZString.Empty).Left(ABL_BillIssuerInfo.MaxLength);
			}
		}
	}
}
