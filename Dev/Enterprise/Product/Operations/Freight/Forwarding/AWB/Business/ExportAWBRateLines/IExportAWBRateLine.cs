using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public interface IExportAWBRateLine : IBusiness
	{
		NatureAndQtyOfGoods NatureAndQtyOfGoods { get; }
		NatureAndQtyOfGoods NatureAndQtyOfGoodsText { get; }
		NatureAndQtyOfGoodsVolume NatureAndQtyOfGoodsVolume { get; }
		NatureAndQtyOfGoodsDimensions NatureAndQtyOfGoodsDimensions { get; }
		NatureAndQtyOfGoodsSLAC NatureAndQtyOfGoodsSLAC { get; }
		NatureAndQtyOfGoodsOrigin NatureAndQtyOfGoodsOrigin { get; }

		ZString NatureAndQtyOfGoodsDescription { get; set; }
		ZPropertyInfo NatureAndQtyOfGoodsDescriptionInfo { get; }

		[List("NatureAndQtyOfGoodsTypeList")]
		ZString NatureAndQtyOfGoodsType { get; set; }
		ZPropertyInfo NatureAndQtyOfGoodsTypeInfo { get; }

		CodeDescriptionPairList NatureAndQtyOfGoodsTypeList { get; }
	}
}
