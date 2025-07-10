using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocAddressZone : AutoJobDocAddressZone
	{
		public JobDocAddressZone(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
