using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class JobServiceLink : AutoJobServiceLink
	{
		public JobServiceLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }
	}
}
