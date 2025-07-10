using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(OGADispositionDataCollection))]
	sealed class OGADispositionDataCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddNewIfNotExist()
		{
			var so701_7 = new ASESSO70_01();
			so701_7.PGALineLevelStatusCode = "AA";
			so701_7.GovernmentAgencyCode = "EPA";
			so701_7.GovernmentAgencyProgramCode = "VNE";
			so701_7.StatusActionDate = "123013";
			so701_7.BeginningCBPLine = "1";
			so701_7.BeginningPGALine = "0";
			var newOne = Declaration.OGADispositionCodes.AddNewIfNotExist(so701_7);
			AssertEquals("AA", newOne.US_Code);
			AssertEquals((ZShort)1, newOne.US_Order);
			AssertEquals(new ZDateTime("2013-12-30"), newOne.US_DispositionDate);

			var so701_8 = new ASESSO70_01();
			so701_8.PGALineLevelStatusCode = "AA";
			so701_8.GovernmentAgencyCode = "EPA";
			so701_8.GovernmentAgencyProgramCode = "VNE";
			so701_8.StatusActionDate = "123013";
			so701_8.BeginningCBPLine = "1";
			so701_8.BeginningPGALine = "0";
			var newOne2 = Declaration.OGADispositionCodes.AddNewIfNotExist(so701_8);
			AssertEquals("No New element should be not added", newOne2.PK, newOne.PK);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OGADispositionData>();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Declaration.OGADispositionCodes;
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;
	}
}
