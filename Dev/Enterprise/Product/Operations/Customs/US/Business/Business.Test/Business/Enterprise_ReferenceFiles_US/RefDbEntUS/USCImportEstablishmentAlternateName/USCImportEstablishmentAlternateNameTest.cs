using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.RefDbEntUS.Testing
{
	[TestedType(typeof(USCImportEstablishmentAlternateName))]
	class USCImportEstablishmentAlternateNameTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<USCImportEstablishment>();
			var altName = factory.New<USCImportEstablishmentAlternateName>();
			altName.IA_IE = header.PK;
			return altName;
		}
	}
}
