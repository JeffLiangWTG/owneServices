using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusConditionLanguage : AutoRefCusConditionLanguage
	{
		public RefCusConditionLanguage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
