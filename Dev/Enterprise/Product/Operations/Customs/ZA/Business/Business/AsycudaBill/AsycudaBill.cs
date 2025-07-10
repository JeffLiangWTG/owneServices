using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ZA.Business
{
	[SystemDefinedValues]
	[UniversalDataContext(DataContextType.ZAOutTurn)]
	[DependentBusinessObject(typeof(AsycudaManifestHeader), nameof(AsycudaManifestHeader.Bills))]
	public class AsycudaBill : ManifestBase.AsycudaBill
		, Integration.Customs.ZA.IAsycudaBill
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : ManifestBase.AsycudaBill.Schema
		{
			public const string LRN = "LRN";
			public const string MRN = "MRN";
			public const string CustomsCPC = "CustomsCPC";

			public const int CustomsCPCMaxLength = 5;
			public const int BillIssuerMaxLength = 20;
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		[RelatedBusinessObject("Header")]
		public override ZGuid ABL_AMA
		{
			get { return base.ABL_AMA; }
			set
			{
				var oldValue = ABL_AMA;
				base.ABL_AMA = value;
				if (oldValue != ABL_AMA && !IsCopying)
				{
					var header = Header;
					if (header != null)
					{
						header.MarkAsNeedingValidation();
						header.Containers.MarkAsNeedingValidation();
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaBill.ABL_BillIssuer", Caption = "Bill Issuer")]
		[MaxLength(Schema.BillIssuerMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.BillIssuers))]
		public override ZString ABL_BillIssuer
		{
			get => base.ABL_BillIssuer;
			set => base.ABL_BillIssuer = value;
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaBill.ABL_BillStatus", Caption = "Bill Status")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CustomsStatusList))]
		[ReadOnlyMember(nameof(ABL_BillStatus_ReadOnly))]
		public override ZString ABL_BillStatus
		{
			get => base.ABL_BillStatus;
			set => base.ABL_BillStatus = value;
		}

		internal bool ABL_BillStatus_ReadOnly => true;

		public override ZString ABL_GoodsLocation
		{
			get => base.ABL_GoodsLocation;
			set
			{
				var oldValue = ABL_GoodsLocation;
				base.ABL_GoodsLocation = value;
				if (oldValue != ABL_GoodsLocation && !IsCopying)
				{
					Header?.MarkAsNeedingValidation();
				}
			}
		}

		#region LRN

		[ReadOnlyMember(nameof(LRNReadOnly))]
		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaBill.LRN", Caption = "LRN")]
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public ZString LRN
		{
			get => LRNEntryNumber.CE_EntryNum;
			set
			{
				var oldValue = LRN;
				LRNEntryNumber.CE_EntryNum = value;
				if (oldValue != LRN)
				{
					LRNInfo.RefreshBinding(oldValue);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateLRN();
				}
			}
		}

		public ZPropertyInfo LRNInfo => GetZPropertyInfo(Schema.LRN);

		ZBool LRNReadOnly => !IsExport;

		CusEntryNumber LRNEntryNumber
		{
			get
			{
				if (localReferenceEntryNumber == null || localReferenceEntryNumber.IsDeleted || localReferenceEntryNumber.CE_EntryType != CusEntryNumberTypes.SouthAfrica.LocalReferenceNumber)
				{
					localReferenceEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.SouthAfrica.LocalReferenceNumber, Core.Constants.CountryCodes.SouthAfrica);
					if (localReferenceEntryNumber != null)
					{
						RegisterEditableChildObject(localReferenceEntryNumber);
					}
				}
				return localReferenceEntryNumber;
			}
		}
		CusEntryNumber localReferenceEntryNumber;

		#endregion

		#region MRN

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaBill.MRN", Caption = "MRN")]
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public ZString MRN
		{
			get => MRNEntryNumber.CE_EntryNum;
			set
			{
				var oldValue = MRN;
				MRNEntryNumber.CE_EntryNum = value;
				if (oldValue != MRN)
				{
					MRNInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateMRN();
				}
			}
		}

		public ZPropertyInfo MRNInfo => GetZPropertyInfo(Schema.MRN);

		CusEntryNumber MRNEntryNumber
		{
			get
			{
				if (mrnEntryNumber == null || mrnEntryNumber.IsDeleted || mrnEntryNumber.CE_EntryType != EntryType_MRN)
				{
					mrnEntryNumber = CusEntryNumber.LoadOrCreate(this, EntryType_MRN, Core.Constants.CountryCodes.SouthAfrica);
					if (mrnEntryNumber != null)
					{
						RegisterEditableChildObject(mrnEntryNumber);
					}
				}
				return mrnEntryNumber;
			}
		}
		CusEntryNumber mrnEntryNumber;

		const string EntryType_MRN = "MRN";

		#endregion

		#region CustomsCPC

		[ReadOnlyMember(nameof(CustomsCPCReadOnly))]
		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaBill.CustomsCPC", Caption = "Export CPC")]
		[MaxLength(Schema.CustomsCPCMaxLength)]
		public ZString CustomsCPC
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.CustomsCPC);
			set
			{
				var oldValue = CustomsCPC;
				CheckMaximumLength(CustomsCPCInfo, value);
				this.SetSystemDefinedValue(Schema.CustomsCPC, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCustomsCPC();
				}
				CustomsCPCInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CustomsCPCInfo => GetZPropertyInfo(nameof(CustomsCPC));

		ZBool CustomsCPCReadOnly => !IsExport;

		#endregion

		public ZBool IsExport => Header?.IsExport ?? ZBool.False;

		public ZString CountryCode => GetCountryCode();

		public override ZString ABL_BolType
		{
			get => base.ABL_BolType;
			set
			{
				var oldValue = ABL_BolType;
				base.ABL_BolType = value;
				if (oldValue != ABL_BolType && !IsCopying)
				{
					var header = Header;
					if (header != null)
					{
						header.MarkAsNeedingValidation();
						header.Containers.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString ABL_BillNumber
		{
			get => base.ABL_BillNumber;
			set
			{
				var oldValue = ABL_BillNumber;
				base.ABL_BillNumber = value;
				if (oldValue != ABL_BillNumber && !IsCopying)
				{
					Header?.MarkAsNeedingValidation();
				}
			}
		}

		[LightValidationTestExempt]
		public override ZInt ABL_ClusterKey
		{
			get => base.ABL_ClusterKey;
			set => base.ABL_ClusterKey = value;
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaBill.ABL_E_ARV", Caption = "Arrival Date")]
		public override ZDateTime ABL_E_ARV
		{
			get => base.ABL_E_ARV;
			set => base.ABL_E_ARV = value;
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaBill.ABL_E_DEP", Caption = "Estimated Departure Time", MediumCaption = "Est. Departure", ShortCaption = "ETD")]
		public override ZDateTime ABL_E_DEP
		{
			get => base.ABL_E_DEP;
			set => base.ABL_E_DEP = value;
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaBill.ABL_RL_NKPortOfDischarge", Caption = "Port of Discharge", ShortCaption = "Discharge")]
		public override ZString ABL_RL_NKPortOfDischarge
		{
			get => base.ABL_RL_NKPortOfDischarge;
			set => base.ABL_RL_NKPortOfDischarge = value;
		}

		[ResourceStringData("Enterprise.Customs.ZA.Business.AsycudaBill.ABL_RL_NKPortOfLoading", Caption = "Port of Loading", MediumCaption = "Load Port", ShortCaption = "Load")]
		public override ZString ABL_RL_NKPortOfLoading
		{
			get => base.ABL_RL_NKPortOfLoading;
			set => base.ABL_RL_NKPortOfLoading = value;
		}

		public new ManifestBase.AsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (ManifestBase.AsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;
		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new ManifestBase.AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);

		protected override ManifestBase.AsycudaBillValidation GetNewValidation()
		{
			return IsChildMasterBill
				? (AsycudaBillValidation)new AsycudaBillValidationForMasterChild(this)
				: new AsycudaBillValidationForRegularBill(this);
		}

		public new AsycudaBillValidation Validation => (AsycudaBillValidation)base.Validation;

		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;

		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);
		protected override Type GetPackTypeCore() => typeof(AsycudaPack);

		protected override Type GetPackageContainerLinkTypeCore() => typeof(AsycudaContainerBillOrPackageLink);

		bool IsChildMasterBill => ABL_BolType == ChildBolCode;

		public const string ChildBolCode = "BOL";
		public const string HouseBillCode = "HWB";
	}
}
