using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USDeclarationFSISLineCollection))]
	class USDelarationFSISLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			var datetimeToSet = new ZDateTime(2016, 11, 09);
			declaration.US_InspecDate = datetimeToSet;
			var importingEstNoToSet = new ZString("Test2");
			declaration.US_FSISInspec = importingEstNoToSet;
			var fFSISLine2 = declaration.FSISLines.AddNew();
			AssertEquals(PartyTypeList.Codes.Importer, fFSISLine2.US_CertifyingIndividual);
			AssertEquals(datetimeToSet, fFSISLine2.US_DateOfInspection);
			AssertEquals(importingEstNoToSet, fFSISLine2.US_ImportingEstNo);
		}

		protected override CargoWise.EntityFramework.BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<JobDeclaration>().FSISLines;
		}
	}
}
