using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS11Test : TestCaseWithFactory
	{
		public void TestUpdateDeclaration()
		{
			var importerOfRecord = Factory.NewWithValidTestData<OrgHeader>();
			importerOfRecord.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567AB", GlbCompany.CurrentCompany.Country);

			var ultimateConsignee = Factory.NewWithValidTestData<OrgHeader>();
			ultimateConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567CC", GlbCompany.CurrentCompany.Country);

			var declaration = Factory.New<JobDeclaration>();
			var aens11 = new AENS11()
			{
				ImporterOfRecordNumber = "12-1234567AB",
				ConsigneeNumber = "12-1234567CC",
				USStateOfDestinationCode = "CC",
				ForeignTradeZoneIdentifier = "7654321",
			};

			var notifications = new NotificationBuffer();
			((IBIRDHeaderRecord)aens11).Update(declaration, notifications);
			AssertEquals(importerOfRecord.PK, declaration.IOROrgPK);
			AssertEquals(ultimateConsignee.MainAddress.PK, declaration.JE_OA_ConsigneeAddress);
			AssertEquals("CC", declaration.US_DestinationState);
			AssertEquals(EntryTypeList.Codes.ConsumptionFTZ, declaration.US_EntryType);
			AssertEquals("7654321", declaration.US_FTZNo);

			aens11 = new AENS11()
			{
				NewForeignTradeZoneIdentifier = "987654321"
			};
			((IBIRDHeaderRecord)aens11).Update(declaration, notifications);
			AssertEquals("987654321", declaration.US_FTZNo);
		}
	}
}
