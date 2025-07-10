using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	internal class AgencyVoyageAccountingEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
	{
		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
		{
			var query = new ZQuery(JobVoyAccountSchema.NA_JobNumber, code).AddToFilter(JobVoyAccountSchema.NA_GC, GlbCompany.CurrentCompany.PK.ToGuid());
			return factory.LoadTop1<VoyageAccount>(query);
		}

		public ZString ExpectedCodeFormat { get; } = "NAJobNumber";
		public ZString ExampleCodeFormat { get; } = "VA99999999";
	}
}
