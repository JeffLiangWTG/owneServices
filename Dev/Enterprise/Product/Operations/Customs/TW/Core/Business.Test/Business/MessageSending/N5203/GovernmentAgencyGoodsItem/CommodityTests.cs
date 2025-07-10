using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CommodityTests : CommodityAbstractTests<Commodity>
	{
		protected override ICommodity Commodity => new Commodity(EntryLine, invoiceLine);
	}
}
