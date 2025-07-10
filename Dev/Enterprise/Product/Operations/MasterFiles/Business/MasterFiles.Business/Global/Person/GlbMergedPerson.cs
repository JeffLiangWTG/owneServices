using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbMergedPerson : AutoGlbMergedPerson
	{
		public GlbMergedPerson(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
