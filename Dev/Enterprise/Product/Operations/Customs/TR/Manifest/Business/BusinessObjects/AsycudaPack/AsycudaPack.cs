using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaPack : ASYCUDA.Business.AsycudaPack, Integration.Customs.ASYCUDA.TRManifest.IAsycudaPack
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			APA_PackUQ = Pack;
		}

		public new AsycudaContainerBillOrPackageLink Pivot => (AsycudaContainerBillOrPackageLink)base.Pivot;

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		public new AsycudaContainer Container => (AsycudaContainer)base.Container;

		protected override ManifestBase.AsycudaPackPackedItemPivotCollection CreateNewAsycudaPackCollection() => new AsycudaPackPackedItemPivotCollection(this);

		public new AsycudaPackPackedItemPivotCollection PackedItems => (AsycudaPackPackedItemPivotCollection)base.PackedItems;

		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

		public new AsycudaPackValidation Validation => (AsycudaPackValidation)base.Validation;

		protected override ManifestBase.AsycudaPackValidation GetNewValidation() => new AsycudaPackValidation(this);

		public new AsycudaPackLookups Lookups => (AsycudaPackLookups)base.Lookups;

		protected override ManifestBase.AsycudaPackLookups GetNewLookups() => new AsycudaPackLookups(this);

		#region PropertiesForDocWrapper

		public ZDecimal TotalWeightOfPackedItemsInKG => GetAllPackedItems().Cast<AsycudaPackedItem>().Sum(x => Core.Constants.Weight.ConvertSafe(x.API_NetWeight, x.API_NetWeightUQ, Constants.Weight.Kilograms));
		public ZDecimal TotalAPIGrossWeightInKg => GetAllPackedItems().Cast<AsycudaPackedItem>().Sum(x => Business.AsycudaHelper.ConvertWeightQty(x.API_GrossWeight, x.API_GrossWeightUQ, Constants.Weight.Kilograms));
		public ZShort BillSequenceNo => Bill?.ABL_SequenceNumber ?? ZShort.Zero;
		public ZString BillNumber => Bill?.ABL_BillNumber ?? ZString.Empty;

		public ZString ContainerNumber => Container?.ACN_ContainerNumber ?? APA_MarksAndNumbers;
		public ZString ContainerSealNumber => Container?.ACN_Seal1 ?? ZString.Empty;

		public ZString ContainerType
		{
			get
			{
				var result = ZString.Empty;

				if (Container != null)
				{
					switch (Container.Relation)
					{
						case RelationList.Codes.Foreign:
							result = TurkishConstants.ContainerForeign;
							break;
						case RelationList.Codes.Local:
							result = TurkishConstants.ContainerLocal;
							break;
					}
				}

				return result;
			}
		}

		public ZString GrossWeightUQ => Kilograms;
		public ZString PackUQ => Pack;
		public const string Kilograms = "KGM";
		public const string Pack = "BI";

		public ZDecimal PackWeightInKG => Core.Constants.Weight.ConvertSafe(APA_Weight, APA_WeightUQ, Constants.Weight.Kilograms);

		#endregion

		public bool HasEmptyContainer => Container != null && Container.ACN_EmptyFullIndicator == ASYCUDA.Business.EmptyFullIndicatorList.Codes.EmptyContainer;

		protected override ASYCUDA.Business.IAsycudaPackedItemCollection<ASYCUDA.Business.AsycudaPackedItem, ASYCUDA.Business.AsycudaPack> CreateNewAsycudaPackedItemCollection()
		{
			return new AsycudaPackedItemCollection(this);
		}
	}
}
