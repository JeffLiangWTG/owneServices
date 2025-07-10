using System;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Transactions.Business
{
	[Immutable]
	public class WhsPickableDocketTypeDecider : WhsDocketTypeDecider
	{
		public override Type GetTypeForNew()
		{
			return typeof(WhsOrder); // throw new NotSupportedException("Abstract type.");
		}

		public override Type GetTypeForBinding()
		{
			return typeof(WhsPickableDocket); // throw new NotSupportedException("Abstract type.");
		}
	}
}
