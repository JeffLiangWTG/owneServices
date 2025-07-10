using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(GlbExternalPassword), "PK")]
	public class GlbExternalPasswordAuthorisation : AutoGlbExternalPasswordAuthorisation
	{
		public GlbExternalPasswordAuthorisation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
