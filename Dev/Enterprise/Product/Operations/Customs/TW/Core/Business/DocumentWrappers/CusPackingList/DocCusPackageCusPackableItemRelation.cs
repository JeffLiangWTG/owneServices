using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.TW.Business
{
	public class DocCusPackageCusPackableItemRelation : DocBaseWrapper
	{
		protected DocCusPackageCusPackableItemRelation(CusPackageCusPackableItemRelation cusPackageCusPackableItemRelation, BusinessObjectFactory factoryToWrap)
			: base(cusPackageCusPackableItemRelation, factoryToWrap)
		{
		}

		public static DocCusPackageCusPackableItemRelation New(CusPackageCusPackableItemRelation cusPackageCusPackableItemRelation, BusinessObjectFactory factoryToWrap)
		{
			return new DocCusPackageCusPackableItemRelation(cusPackageCusPackableItemRelation, factoryToWrap);
		}

		internal CusPackageCusPackableItemRelation Relation => (CusPackageCusPackableItemRelation)WrappedObject;

		#region Properties

		public ZDecimal PackedQty => Relation.PackedQty;

		public ZString GoodsDescription => Relation.GoodsDescription;

		public ZString Grouping => Relation.Grouping;

		public ZString PackedUQ => Relation.PackableUQ;

		ZDecimal NetWeight => Relation.NetWeight;

		ZString NetWeightUQ => Relation.NetWeightUQ;

		public ZString PackedQtyInfo => PackedQtyDetailInfo.GetDetail(PackedQtyDecimalPlace);

		public PackedDetailInfo PackedQtyDetailInfo => new(PackedQty, PackedUQ, Relation.Package.KP_PackageQty, 3);

		public ZString NetWeightInfo => NetWeightQtyDetailInfo.GetDetail(NetWeightDecimalPlace);

		public PackedDetailInfo NetWeightQtyDetailInfo => new(NetWeight, NetWeightUQ, Relation.Package.KP_PackageQty, 3);

		public ZInt PackedQtyDecimalPlace { get; set; }

		public ZInt NetWeightDecimalPlace { get; set; }

		#endregion
	}
}
