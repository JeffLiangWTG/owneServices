using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ShippersDeclarationForDangerousGoods : DocDataObject
	{
		public ShippersDeclarationForDangerousGoodsCollection Pages { get; set; }

		public ZString ErrorMessage { get; set; }
	}
}
