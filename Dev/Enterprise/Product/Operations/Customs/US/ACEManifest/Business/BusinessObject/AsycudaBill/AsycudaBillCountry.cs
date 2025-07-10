using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business.CustomValues;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public partial class AsycudaBill : ITariffFormatProvider
	{
		public new class Schema : ASYCUDA.Business.AsycudaBill.Schema
		{
			public const string FDAIndicator = "FDAIndicator";
			public const string ABL_Tariff = "ABL_Tariff";
			public const string GoodsOrigin = "GoodsOrigin";
			public new const int CustomsEntryNumberTypeMaxLength = 2;
			public const int CustomsEntryNumberMaxLength = 11;
		}

		public override ZDecimal ABL_GoodsValue
		{
			get => base.ABL_GoodsValue.Truncate();
			set => base.ABL_GoodsValue = value.Truncate();
		}

		public override ZString ABL_RL_NKOrigin
		{
			get => base.ABL_RL_NKOrigin;
			set
			{
				var oldValue = base.ABL_RL_NKOrigin;
				if (oldValue != value)
				{
					base.ABL_RL_NKOrigin = value;
					if (!IsValidationSuspended && Validation is AsycudaBillValidationForRegularBill validationForRegularBill)
					{
						validationForRegularBill.ValidateGoodsOrigin();
					}
				}
			}
		}

		#region Tariff

		[BusinessObjectTestExclude]
		[MaxLength(15)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.TariffCollection))]
		[ResourceStringData("Enterprise.Customs.US.ACEManifest.Business.AsycudaBill.ABL_Tariff", Caption = "Tariff")]
		public ZString ABL_Tariff
		{
			get => GetPack()?.PackedItem.API_FormattedTariff ?? ZString.Empty;
			set
			{
				var oldValue = GetPack()?.PackedItem.API_Tariff ?? ZString.Empty;
				if (oldValue != value)
				{
					CheckMaximumLength(ABL_TariffInfo, value);
					Pack.PackedItem.API_Tariff = value;
					if (!IsValidationSuspended && Validation is AsycudaBillValidationForRegularBill validationForRegularBill)
					{
						validationForRegularBill.ValidateABL_Tariff();
					}
					ABL_TariffInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo ABL_TariffInfo => GetZPropertyInfo(Schema.ABL_Tariff);

		#endregion

		#region ITariffFormatProvider

		ITariffFormatter ITariffFormatProvider.TariffFormatter => new US.Business.TariffFormatter();

		#endregion

		#region Pack

		public new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;

		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);

		protected override Type GetPackTypeCore() => typeof(AsycudaPack);

		AsycudaPack GetPack() => Packs.Cast<AsycudaPack>().FirstOrDefault();

		AsycudaPack Pack
		{
			get
			{
				if (!IsDeleted && (pack == null || pack.IsDeleted))
				{
					if (pack != null)
					{
						UnRegisterEditableChildObject(pack);
						pack = null;
					}
					if (!IsDeleting)
					{
						pack = GetPack() ?? Packs.AddNew();
						RegisterEditableChildObject(pack);
					}
				}
				return pack;
			}
		}
		AsycudaPack pack;

		#endregion

		#region GoodsOrigin
		[BusinessObjectTestExclude]
		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.GoodsOrigins))]
		[ResourceStringData("Enterprise.Customs.US.ACEManifest.Business.GoodsOrigins", Caption = "Country/Region of Origin", MediumCaption = "Ctry/Rgn. of Origin")]
		public ZString GoodsOrigin
		{
			get
			{
				var result = PackedItemGoodsOrigin;
				if (result == ZString.Empty)
				{
					result = Origin?.RL_RN_NKCountryCode ?? ZString.Empty;
				}
				return result;
			}
			set
			{
				var oldValue = PackedItemGoodsOrigin;
				if (oldValue != value)
				{
					CheckMaximumLength(GoodsOriginInfo, value);
					Pack.PackedItem.API_RN_NKGoodsOrigin = value;
					if (!IsValidationSuspended && Validation is AsycudaBillValidationForRegularBill validationForRegularBill)
					{
						validationForRegularBill.ValidateGoodsOrigin();
					}
				}
				GoodsOriginInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo GoodsOriginInfo => GetZPropertyInfo(Schema.GoodsOrigin);

		ZString PackedItemGoodsOrigin => GetPack()?.PackedItem.API_RN_NKGoodsOrigin ?? ZString.Empty;

		#endregion

		#region FDAIndicator

		[ResourceStringData("Enterprise.Customs.US.ACEManifest.Business.AsycudaBill.FDAIndicator", Caption = "Is FDA?")]
		public ZBool FDAIndicator
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.FDAIndicator);
			set
			{
				var oldValue = FDAIndicator;
				this.SetSystemDefinedValue(Schema.FDAIndicator, value);
				FDAIndicatorInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo FDAIndicatorInfo => GetZPropertyInfo(Schema.FDAIndicator);

		#endregion

		#region CustomsEntryNumber

		[MaxLength(Schema.CustomsEntryNumberTypeMaxLength)]
		[ReadOnlyMember(nameof(CustomsEntryNumberType_ReadOnly))]
		public override ZString CustomsEntryNumberType
		{
			get => base.CustomsEntryNumberType;
			set
			{
				var oldValue = base.CustomsEntryNumberType;
				if (oldValue != value)
				{
					base.CustomsEntryNumberType = value.Left(Schema.CustomsEntryNumberTypeMaxLength);
					Validation.ValidateCustomsEntryNumber();
				}
			}
		}

		bool CustomsEntryNumberType_ReadOnly => !(Header?.IsExpressCourier ?? false);

		[MaxLength(Schema.CustomsEntryNumberMaxLength)]
		[ReadOnlyMember(nameof(CustomsEntryNumber_ReadOnly))]
		public override ZString CustomsEntryNumber
		{
			get => base.CustomsEntryNumber;
			set
			{
				base.CustomsEntryNumber = value.Left(Schema.CustomsEntryNumberMaxLength);
				Validation.ValidateCustomsEntryNumberType();
			}
		}

		protected override bool CustomsEntryNumber_ReadOnly => !(Header?.IsExpressCourier ?? false);

		protected override void DeleteCustomsNumbersNoLongerApplicable()
		{
			var customsNumbersToDelete = CustomsEntryNumbers.Cast<ASYCUDA.Business.ABLEntryNum>().Where(x => x.CE_EntryNum.IsEmpty && x.CE_EntryType.IsEmpty).ToArray();
			foreach (var customsNumber in customsNumbersToDelete)
			{
				CustomsEntryNumbers.RemoveAndDelete(customsNumber);
			}
		}

		#endregion
	}
}
