using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class HrlProcessingRun : AutoHrlProcessingRun
	{
		public HrlProcessingRun(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
