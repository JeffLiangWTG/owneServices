using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.TW.Business
{
	public class DocPkgPackage : DocBasePkgPackage
	{
		DocPkgPackage(CusPackage cusPackage, BusinessObjectFactory factory)
			: base(cusPackage, factory)
		{
		}

		internal CusPackage Package => (CusPackage)WrappedObject;

		public static DocPkgPackage New(CusPackage cusPackage, BusinessObjectFactory factoryToWrap)
		{
			return new DocPkgPackage(cusPackage, factoryToWrap);
		}

		public ZString PackNoInfo => MarksAndNumbers;

		public ZString GrossWeightInfo => PackedGrossWeightDetailInfo.GetDetail(MaxPackedGrossWeightDecimalPlace);

		internal ZInt MaxPackedGrossWeightDecimalPlace { get; set; }

		internal PackedDetailInfo PackedGrossWeightDetailInfo => new(Weight, WeightUQ, PackageQty, 3);

		public ZString NetWeightInfo => PackedNetWeightDetailInfo.GetDetail(MaxPackedNetWeightDecimalPlace);

		internal PackedDetailInfo PackedNetWeightDetailInfo => new(NetWeight, WeightUQ, PackageQty, 3);

		internal ZInt MaxPackedNetWeightDecimalPlace { get; set; }

		#region Volume

		public ZString VolumeInfo => DocumentWrapperHelper.GetPackageVolumeInfo(ZString.Empty, PackedVolumeDetailInfo.GetDetail(MaxPackedVolumeDecimalPlace), Dimension);

		internal PackedDetailInfo PackedVolumeDetailInfo => new(Volume, VolumeUQInfo, PackageQty, 3);

		internal ZInt MaxPackedVolumeDecimalPlace { get; set; }

		ZString Dimension => DocumentWrapperHelper.GetPackageDimension(Length, Width, Height, DimensionUQ);

		public ZString VolumeUQInfo => DocumentWrapperHelper.GetPackageVolumeUQInfo(VolumeUQ);

		#endregion
		#region Collection

		public IEnumerable<CusPackageCusPackableItemRelation> PackedItemRelations
		{
			get
			{
				if (packedItemRelations == null)
				{
					Package.PackableItemRelataions.Sort(CusPackageCusPackableItemRelation.Schema.InvoiceLineNumber, ListSortDirection.Ascending);
					packedItemRelations = Package.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().Where(x => x.IsPacked);
				}
				return packedItemRelations;
			}
		}
		IEnumerable<CusPackageCusPackableItemRelation> packedItemRelations;

		public DocCusPackageCusPackableItemRelationCollection PackedItemRelationsRelation => packedItemRelationsRelation ??= new DocCusPackageCusPackableItemRelationCollection(PackedItemRelations, Factory);
		DocCusPackageCusPackableItemRelationCollection packedItemRelationsRelation;
		#endregion
	}
}
