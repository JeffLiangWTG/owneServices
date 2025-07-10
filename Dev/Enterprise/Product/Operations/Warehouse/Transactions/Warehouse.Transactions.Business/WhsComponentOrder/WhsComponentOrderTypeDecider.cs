using System;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Transactions.Business
{
	[Immutable]
	public class WhsComponentOrderTypeDecider : WhsPickableDocketTypeDecider
	{
		public override Type GetTypeForNew() => typeof(WhsWorkOrder);
		public override Type GetTypeForBinding() => typeof(WhsComponentOrder);
	}
}
