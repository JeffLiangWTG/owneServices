using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusConditionCode : AutoRefCusConditionCode
	{
		public RefCusConditionCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
