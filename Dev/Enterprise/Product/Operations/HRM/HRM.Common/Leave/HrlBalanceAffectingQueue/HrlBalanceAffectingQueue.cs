using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class HrlBalanceAffectingQueue : AutoHrlBalanceAffectingQueue
	{
		public HrlBalanceAffectingQueue(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
