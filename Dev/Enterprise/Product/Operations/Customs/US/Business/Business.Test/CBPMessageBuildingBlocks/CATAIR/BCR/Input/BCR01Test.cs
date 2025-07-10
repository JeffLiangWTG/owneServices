using System;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class BCR01Test : BIRDHeaderUpdateTest
	{
		public void TestIBIRDHeaderIDRecord()
		{
			BCR01 bcr01 = new BCR01();
			bcr01.FilerCode = "XJ5";
			bcr01.EntryNumber = "12345678";

			AssertEquals("XJ5", ((IBIRDHeaderIDRecord)bcr01).EntryFilerCode);
			AssertEquals("12345678", ((IBIRDHeaderIDRecord)bcr01).EntryNumber);
		}

		protected override IBIRDHeaderRecord[] GetPopulatedHeaderRecords()
		{
			BCR01 bcr01 = new BCR01();

			bcr01.UpdateActionCode = "A";
			bcr01.DistrictPortOfEntry = "8888";
			bcr01.FilerCode = "XJ5";
			bcr01.EntryNumber = "12345678";
			bcr01.ModeOfTransportationMOTCode = TransportModeCodes.Codes.TruckContainer;
			bcr01.ImporterOfRecord = "12-1234567AB";
			bcr01.BondType = 8;
			bcr01.SuretyCode = "081";
			bcr01.UltimateConsignee = "";
			bcr01.DateOfArrival = new CargoWise.Types.ZDate(2009, 1, 1);
			bcr01.EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			bcr01.CarrierCode = "APLU";
			bcr01.ConsigneeNameAndAddress = "1";

			BCR01 bcr01WithUltimateConsignee = new BCR01();
			bcr01WithUltimateConsignee.UpdateActionCode = "A";
			bcr01WithUltimateConsignee.DistrictPortOfEntry = "8888";
			bcr01WithUltimateConsignee.FilerCode = "XJ5";
			bcr01WithUltimateConsignee.EntryNumber = "12345678";
			bcr01WithUltimateConsignee.ModeOfTransportationMOTCode = TransportModeCodes.Codes.TruckContainer;
			bcr01WithUltimateConsignee.ImporterOfRecord = "12-1234567AB";
			bcr01WithUltimateConsignee.BondType = 8;
			bcr01WithUltimateConsignee.SuretyCode = "081";
			bcr01WithUltimateConsignee.UltimateConsignee = "12-1234567CC";
			bcr01WithUltimateConsignee.DateOfArrival = new CargoWise.Types.ZDate(2009, 1, 1);
			bcr01WithUltimateConsignee.EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			bcr01WithUltimateConsignee.CarrierCode = "APLU";

			return new IBIRDHeaderRecord[] { bcr01, bcr01WithUltimateConsignee };
		}

		protected override EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			return new BorderCargoReleaseMessageBuilder(declaration.ActiveEntryHeaders.CargoReleaseEntry, UpdateActionCode.Add);
		}

		protected override Type GetTypeOfMessageBlock()
		{
			return typeof(BCR01);
		}

		protected override void PrepareDeclaration(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			base.PrepareDeclaration(declaration, headerRecord);

			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
		}

		protected override void SetUpData()
		{
			base.SetUpData();

			OrgHeader importerOfRecord = Factory.NewWithValidTestData<OrgHeader>();
			importerOfRecord.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567AB", GlbCompany.CurrentCompany.Country);

			OrgHeader ultimateConsignee = Factory.NewWithValidTestData<OrgHeader>();
			ultimateConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567CC", GlbCompany.CurrentCompany.Country);

			Factory.Save();
		}

		protected override string[] GetFieldNameToExcludeForTesting()
		{
			return new string[]
			{
				"EntryImmediateDeliveryIndicator",//This is for future use
			};
		}
	}
}
