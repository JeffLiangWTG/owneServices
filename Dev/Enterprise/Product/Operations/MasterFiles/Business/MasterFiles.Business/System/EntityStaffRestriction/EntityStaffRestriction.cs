using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class EntityStaffRestriction : AutoEntityStaffRestriction
	{
		public EntityStaffRestriction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
