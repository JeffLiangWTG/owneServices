using System;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class CRLH1Test : BIRDHeaderUpdateTest
	{
		public void TestIBIRDHeaderIDRecord()
		{
			CRLH1 h1 = new CRLH1();
			h1.EntryFilerCode = "XJ5";
			h1.EntryNumber = "12345678";

			AssertEquals("XJ5", ((IBIRDHeaderIDRecord)h1).EntryFilerCode);
			AssertEquals("12345678", ((IBIRDHeaderIDRecord)h1).EntryNumber);
		}

		protected override IBIRDHeaderRecord[] GetPopulatedHeaderRecords()
		{
			CRLH1 h1 = new CRLH1();

			h1.UpdateActionCode = "A";
			h1.DistrictPortOfEntry = "8888";
			h1.EntryFilerCode = "XJ5";
			h1.EntryNumber = "12345678";
			h1.ImporterNumber = "12-1234567AB";
			h1.ModeOfTransportationMOTCode = TransportModeCodes.Codes.VesselContainer;
			h1.EstimatedDateOfArrival = new CargoWise.Types.ZDate(2009, 6, 10);
			h1.BondTypeCode = BondTypeList.Codes.ContinuousBond;
			h1.ReleaseCertificationCode = 1;
			h1.PresentationDate = new CargoWise.Types.ZDate(2009, 6, 11);
			h1.CarrierCode = "ABCD";
			h1.DistrictPortOfUnlading = "3901";
			h1.EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			h1.SuretyCode = "081";
			h1.ConsigneeNameAndAddress = "1";

			return new IBIRDHeaderRecord[] { h1 };
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

		protected override EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			return new CargoReleaseMessageBuilder(declaration.ActiveEntryHeaders.CargoReleaseEntry, UpdateActionCode.Add, true);
		}

		protected override Type GetTypeOfMessageBlock()
		{
			return typeof(CRLH1);
		}

		protected override string[] GetFieldNameToExcludeForTesting()
		{
			return new string[]
			{
				"ImportingVesselCode",//This is not populated in CargoReleaseMessageBuilder
				"OtherGovernmentAgencyOGACodes",//This is for future use.
			};
		}
	}
}
