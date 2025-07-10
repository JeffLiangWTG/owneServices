using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusCodeTypeLanguage : AutoRefCusCodeTypeLanguage
	{
		public RefCusCodeTypeLanguage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
