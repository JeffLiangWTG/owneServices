using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class AsycudaPack : ASYCUDA.Business.AsycudaPack
		, Integration.Customs.ASYCUDA.UYManifest.IAsycudaPack
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;
		protected new AsycudaPackValidation Validation => (AsycudaPackValidation)base.Validation;
		protected override ManifestBase.AsycudaPackValidation GetNewValidation() => new AsycudaPackValidation(this);

		public new class Schema : ASYCUDA.Business.AsycudaPack.Schema
		{
			public const string APA_ArrivedQuantity = "APA_ArrivedQuantity";
			public const string APA_ArrivedWeight = "APA_ArrivedWeight";
			public const int APA_ArrivedWeightPrecision = 11;
			public const int APA_ArrivedWeightScale = 3;
		}

		#region APA_ArrivedQuantity

		[ResourceStringData("AsycudaPack.APA_ArrivedQuantity", Caption = "Arrived Qty")]
		public ZInt APA_ArrivedQuantity
		{
			get => this.GetSystemDefinedValue<ZInt>(Customs.Business.GenAddOnHelper.ArrivedQuantity);
			set
			{
				var oldValue = APA_ArrivedQuantity;
				this.SetSystemDefinedValue(Customs.Business.GenAddOnHelper.ArrivedQuantity, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateArrivedQuantity();
					Validation.ValidateArrivedWeight();
				}
				APA_ArrivedQuantityInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo APA_ArrivedQuantityInfo => GetZPropertyInfo(Schema.APA_ArrivedQuantity);

		#endregion

		#region APA_ArrivedWeight

		[DecimalPlaces(Schema.APA_ArrivedWeightScale)]
		[DecimalPrecision(Schema.APA_ArrivedWeightPrecision)]
		[ResourceStringData("AsycudaPack.APA_ArrivedWeight", Caption = "Arrived Weight")]
		public ZDecimal APA_ArrivedWeight
		{
			get => this.GetSystemDefinedValue<ZDecimal>(Customs.Business.GenAddOnHelper.ArrivedWeight);
			set
			{
				var oldValue = APA_ArrivedWeight;
				this.SetSystemDefinedValue(Customs.Business.GenAddOnHelper.ArrivedWeight, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateArrivedWeight();
					Validation.ValidateArrivedQuantity();
				}
				APA_ArrivedWeightInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo APA_ArrivedWeightInfo => GetZPropertyInfo(Schema.APA_ArrivedWeight);

		#endregion

		public override bool CanDelete => Bill.CanDelete;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get => !CanDelete
				? ResString.GetMultilingualString("AF619F98-6159-43A6-BA30-FC7EA7A7FA4C", "This Bill is already registered with Customs.")
				: base.ReasonForNotAbleToDelete;
		}
	}
}
