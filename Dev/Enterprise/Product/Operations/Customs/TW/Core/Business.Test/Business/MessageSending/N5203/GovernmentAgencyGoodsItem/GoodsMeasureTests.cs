using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class GoodsMeasureTests : GoodsMeasureAbstractTests<N5203.GoodsMeasure>
	{
		protected override IGoodsMeasure GoodsMeasure => new N5203.GoodsMeasure(EntryLine);
	}
}
