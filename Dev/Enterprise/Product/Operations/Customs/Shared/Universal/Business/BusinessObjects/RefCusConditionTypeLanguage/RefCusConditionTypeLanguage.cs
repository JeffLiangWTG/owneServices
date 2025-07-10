using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public class RefCusConditionTypeLanguage : AutoRefCusConditionTypeLanguage
	{
		public RefCusConditionTypeLanguage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
