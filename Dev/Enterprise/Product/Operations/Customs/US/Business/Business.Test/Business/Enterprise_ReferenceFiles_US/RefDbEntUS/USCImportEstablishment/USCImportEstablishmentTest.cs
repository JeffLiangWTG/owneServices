using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.RefDbEntUS.Testing
{
	[TestedType(typeof(USCImportEstablishment))]
	class USCImportEstablishmentTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return base.GetNewBusinessObjectForDeleteTest(factory);
		}
	}
}
