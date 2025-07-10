using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public partial class AsycudaBill : ASYCUDA.Business.AsycudaBill
		, Integration.Customs.ASYCUDA.ZAManifest.IAsycudaBill
		, ICaseNumberCollectionProvider
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool IsManifestUQNeedToConvertCore => false;

		public override ZGuid ABL_JS_Shipment
		{
			get => base.ABL_JS_Shipment;
			set
			{
				var oldValue = ABL_JS_Shipment;
				var country = Header?.AMA_RN_NKCountry ?? ZString.Empty;
				base.ABL_JS_Shipment = value;
				if (!IsCopying && oldValue != ABL_JS_Shipment)
				{
					var currentCountry = Header?.AMA_RN_NKCountry ?? ZString.Empty;
					if (!currentCountry.IsEmpty && country == currentCountry)
					{
						DefaultBillIssuerFromShipment(Shipment);
					}
				}
			}
		}

		public override ZString ABL_ConsigneeName
		{
			get => base.ABL_ConsigneeName;
			set
			{
				bool hasChanges = value != ABL_ConsigneeName;
				base.ABL_ConsigneeName = value;
				if (!IsCopying && hasChanges && value == ConsigneeToOrder)
				{
					SetToOrderConsignmentDetails();
				}
			}
		}

		void SetToOrderConsignmentDetails()
		{
			ABL_ConsigneeStreet1 = NoAddressSupplied;
			ABL_ConsigneeStreet2 = ZString.Empty;
			ABL_ConsigneeCity = ZString.Empty;
			ABL_ConsigneeState = ZString.Empty;
			ABL_ConsigneePostcode = ZString.Empty;
			ABL_RN_NKConsigneeCountry = ZString.Empty;
		}

		const string ConsigneeToOrder = "TO ORDER";
		const string NoAddressSupplied = "NO ADDRESS SUPPLIED";

		public bool ToOrderConsignment => ABL_ConsigneeName == ConsigneeToOrder && ABL_ConsigneeStreet1 == NoAddressSupplied;

		bool ConsigneeAddressReadOnly => ToOrderConsignment;

		[ReadOnlyMember(nameof(ConsigneeAddressReadOnly))]
		public override ZString ABL_ConsigneeStreet2 { get => base.ABL_ConsigneeStreet2; set => base.ABL_ConsigneeStreet2 = value; }

		[ReadOnlyMember(nameof(ConsigneeAddressReadOnly))]
		public override ZString ABL_ConsigneeCity { get => base.ABL_ConsigneeCity; set => base.ABL_ConsigneeCity = value; }

		[ReadOnlyMember(nameof(ConsigneeAddressReadOnly))]
		public override ZString ABL_ConsigneePostcode { get => base.ABL_ConsigneePostcode; set => base.ABL_ConsigneePostcode = value; }

		[ReadOnlyMember(nameof(ConsigneeAddressReadOnly))]
		public override ZString ABL_ConsigneeState { get => base.ABL_ConsigneeState; set => base.ABL_ConsigneeState = value; }

		[ReadOnlyMember(nameof(ConsigneeAddressReadOnly))]
		public override ZString ABL_RN_NKConsigneeCountry { get => base.ABL_RN_NKConsigneeCountry; set => base.ABL_RN_NKConsigneeCountry = value; }

		public override ZString ABL_BolType
		{
			get => base.ABL_BolType;
			set
			{
				base.ABL_BolType = value;
				CustomsEntryNumbers.MarkAsNeedingValidation();
			}
		}

		#region CaseNumbers

		[ChildEditable(true)]
		public CaseNumberCollection CaseNumbers
		{
			get
			{
				if (casenumbers == null)
				{
					casenumbers = new CaseNumberCollection(this);
					casenumbers.Load();
					RegisterEditableChildObject(casenumbers);
				}
				return casenumbers;
			}
		}
		CaseNumberCollection casenumbers;

		BusinessObject ICaseNumberCollectionProvider.Master => this;

		public ZBool CaseNumbersVisible => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Customs.Universal.Constants.FunctionalityTypes.ZAManifestCaseNumbers
				, Core.Constants.CountryCodes.SouthAfrica
				, ZDateTime.Today) && (Header?.IsBillLevelManifestType ?? ZBool.False);

		#endregion

		public new AsycudaPackCollection Packs => (AsycudaPackCollection)base.Packs;
		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new AsycudaPackCollection(this);
		protected override ZString GetCountryCode() => Core.Constants.CountryCodes.SouthAfrica;
		protected override Type GetPackTypeCore() => typeof(AsycudaPack);
		protected override Type GetPackageContainerLinkTypeCore() => typeof(AsycudaContainerBillOrPackageLink);
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);
		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);
		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;
		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaBillFetchStrategy(this);

		protected override IDictionary<ZString, Type> SupportedCusCodeDataTypes
		{
			get
			{
				var result = base.SupportedCusCodeDataTypes;
				result.Add(CusCodeDataTypeList.Codes.CaseNumber, typeof(CaseNumber));
				return result;
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		[ResourceStringData("ZA.Manifest.Business.AsycudaBill.CustomsEntryNumber", Caption = "Local Reference No / LRN")]
		public override ZString CustomsEntryNumber
		{
			get => base.CustomsEntryNumber;
			set => base.CustomsEntryNumber = value;
		}

		[ResourceStringData("ZA.Manifest.Business.AsycudaBill.CustomsEntryNumberType", Caption = "LRN Type")]
		public override ZString CustomsEntryNumberType
		{
			get => base.CustomsEntryNumberType;
			set
			{
				base.CustomsEntryNumberType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCustomsEntryNumber();
				}
			}
		}

		[ChildEditable]
		public new ABLEntryNumCollection CustomsEntryNumbers => (ABLEntryNumCollection)base.CustomsEntryNumbers;

		protected override ASYCUDA.Business.ABLEntryNumCollection CreateNewABLEntryNumCollection() => new ABLEntryNumCollection(this);
	}
}
