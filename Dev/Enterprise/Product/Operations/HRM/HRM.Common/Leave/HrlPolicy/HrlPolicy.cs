using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class HrlPolicy : AutoHrlPolicy
	{
		public HrlPolicy(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
