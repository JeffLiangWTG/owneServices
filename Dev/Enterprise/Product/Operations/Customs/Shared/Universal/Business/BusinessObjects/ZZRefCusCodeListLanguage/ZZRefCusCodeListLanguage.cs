using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Universal
{
	[CodeAlive("Under Development")]
	public sealed class ZZRefCusCodeListLanguage : AutoZZRefCusCodeListLanguage
	{
		public ZZRefCusCodeListLanguage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
