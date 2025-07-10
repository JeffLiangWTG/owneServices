using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Universal
{
	[CodeAlive("Under Development")]
	public sealed class ZZRefCusCodeListAttribute : AutoZZRefCusCodeListAttribute
	{
		public ZZRefCusCodeListAttribute(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
