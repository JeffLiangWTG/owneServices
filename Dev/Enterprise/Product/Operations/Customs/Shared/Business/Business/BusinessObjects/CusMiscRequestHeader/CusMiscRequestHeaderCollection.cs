using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Will be used as a module collection in a country-specific sln")]
	public class CusMiscRequestHeaderCollection : ActiveBusinessObjectCollection<CusMiscRequestHeader>
	{
		public CusMiscRequestHeaderCollection(BusinessObjectFactory factory, GlbCompany company)
			: base(factory, new ZQuery(CusMiscRequestHeaderSchema.CMR_GB, company.Branches.GetPKs()))
		{
		}

		protected override bool AllowNew => false;
	}
}
