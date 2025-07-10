using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusTransportMeans))]
	class CusTransportMeansTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		CusTransportMeans GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var transportMeans = factory.New<CusTransportMeans>();
			transportMeans.TPM_ParentTableCode = CusInBondHeaderSchema.Constants.Prefix;
			return transportMeans;
		}
	}
}
