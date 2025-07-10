using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.HRM.Common
{
	public class HrlStaffPolicy : AutoHrlStaffPolicy, IHrlStaffPolicy
	{
		public HrlStaffPolicy(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
