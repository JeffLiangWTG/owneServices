using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Universal.Internal
{
	[CodeAlive("Under Development")]
	public class ZZRefCarrier : AutoZZRefCarrier
	{
		public ZZRefCarrier(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
