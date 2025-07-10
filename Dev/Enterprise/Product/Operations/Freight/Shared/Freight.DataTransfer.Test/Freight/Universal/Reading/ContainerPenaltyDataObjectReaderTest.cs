using System;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Freight.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;
using ContainerPenaltyBO = Enterprise.Freight.Business.ContainerPenalty;
using ContainerPenaltyDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.ContainerPenalty;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class ContainerPenaltyDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestPopulateNewBusinessObjectWithEmptyCurrency()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG1";

			var address = Factory.New<OrgAddress>();
			address.OA_OH = org.PK;
			address.OA_Code = "TESTADDR1";
			address.OA_Address1 = "1804 Fudrucker Way";

			var dataObject = SetupDataObject("IMP");
			dataObject.Currency = new Currency() { Code = ZString.Empty };

			var container = Factory.New<CommonContainer>();
			var reader = new ContainerPenaltyDataObjectReader(dataObject, logger, Factory, container);
			reader.ReadIntoBusinessObject();
			AssertNoExceptionThrown(Factory.SaveForTesting);
			AssertEquals("AUD", container.ImportPenalties[0].CPY_RX_NKCurrency);

			dataObject = SetupDataObject("EXP");
			dataObject.Currency = new Currency() { Code = ZString.Empty };
			reader = new ContainerPenaltyDataObjectReader(dataObject, logger, Factory, container);
			reader.ReadIntoBusinessObject();
			AssertNoExceptionThrown(Factory.SaveForTesting);
			AssertEquals("CNY", container.ExportPenalties[0].CPY_RX_NKCurrency);
		}

		public void TestPopulateNewBusinessObject()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG1";

			var address = Factory.New<OrgAddress>();
			address.OA_OH = org.PK;
			address.OA_Code = "TESTADDR1";
			address.OA_Address1 = "1804 Fudrucker Way";

			var dataObject = SetupDataObject("IMP");

			var container = Factory.New<CommonContainer>();
			var reader = new ContainerPenaltyDataObjectReader(dataObject, logger, Factory, container);
			reader.ReadIntoBusinessObject();

			AssertEquals(1, container.ImportPenalties.Count);
			AssertResult(container.ImportPenalties[0], org, "IMP");

			dataObject = SetupDataObject("EXP");
			reader = new ContainerPenaltyDataObjectReader(dataObject, logger, Factory, container);
			reader.ReadIntoBusinessObject();
			AssertEquals(1, container.ExportPenalties.Count);
			AssertResult(container.ExportPenalties[0], org, "EXP");
		}

		public void TestPopulateExistedBusinessObject()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG1";

			var address = Factory.New<OrgAddress>();
			address.OA_OH = org.PK;
			address.OA_Code = "TESTADDR1";
			address.OA_Address1 = "1804 Fudrucker Way";

			var dataObject = SetupDataObject("IMP");

			var container = Factory.New<CommonContainer>();
			var penalty = container.ImportPenalties.AddNew();
			penalty.CPY_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention;
			penalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			penalty.CPY_Duration = new ZDateTime(2020, 1, 23);
			penalty.CPY_PerUnitCost = 100m;

			var reader = new ContainerPenaltyDataObjectReader(dataObject, logger, Factory, container);
			reader.ReadIntoBusinessObject();

			AssertEquals(1, container.ImportPenalties.Count);
			AssertResult(container.ImportPenalties[0], org, "IMP");

			dataObject = SetupDataObject("EXP");
			penalty = container.ExportPenalties.AddNew();
			penalty.CPY_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention;
			penalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			penalty.CPY_Duration = new ZDateTime(2020, 1, 26);
			penalty.CPY_PerUnitCost = 110m;
			reader = new ContainerPenaltyDataObjectReader(dataObject, logger, Factory, container);
			reader.ReadIntoBusinessObject();
			AssertEquals(1, container.ExportPenalties.Count);
			AssertResult(container.ExportPenalties[0], org, "EXP");
		}

		#region Implementation

		ContainerPenaltyDataObject SetupDataObject(ZString processType)
		{
			var creditor = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.Creditor),
				OrganizationCode = "TESTORG1",
				Address1 = "1804 Fudrucker Way"
			};

			if (processType == "IMP")
			{
				return new ContainerPenaltyDataObject
				{
					PenaltyType = new CodeDescriptionPair { Code = Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention },
					CreditorType = new CodeDescriptionPair { Code = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier },
					Creditor = creditor,
					Location = new UNLOCO { Code = "AUSYD" },
					FreeTime = new ZDateTime(2020, 1, 1),
					Duration = new ZDateTime(2020, 1, 10),
					TimeUnit = TimeUnit.Days,
					PerUnitCost = 3.2m,
					TotalCost = 500m,
					Currency = new Currency { Code = "AUD" },
					ProcessType = new CodeDescriptionPair { Code = "IMP" }
				};
			}
			else
			{
				return new ContainerPenaltyDataObject
				{
					PenaltyType = new CodeDescriptionPair { Code = Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention },
					CreditorType = new CodeDescriptionPair { Code = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier },
					Creditor = creditor,
					Location = new UNLOCO { Code = "CNSHA" },
					FreeTime = new ZDateTime(2020, 1, 2),
					Duration = new ZDateTime(2020, 1, 11),
					TimeUnit = TimeUnit.Days,
					PerUnitCost = 4.2m,
					TotalCost = 600m,
					Currency = new Currency { Code = "CNY" },
					ProcessType = new CodeDescriptionPair { Code = "EXP" }
				};
			}
		}

		void AssertResult(ContainerPenaltyBO penalty, OrgHeader expectedOrg, ZString processType)
		{
			if (processType == "IMP")
			{
				AssertEquals(Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention, penalty.CPY_PenaltyType);
				AssertEquals(Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier, penalty.CPY_CreditorType);
				AssertEquals("AUSYD", penalty.CPY_RL_NKLocation);
				AssertEquals(ZDateTime.Empty, penalty.CPY_FreeTime);
				AssertEquals(expectedOrg.PK, penalty.CPY_OH_Creditor);
				AssertEquals((ZDateTime)TimeSpan.FromDays(9), penalty.CPY_Duration);
				AssertEquals(Core.Constants.ContainerPenaltyTimeUnit.Codes.Days, penalty.CPY_TimeUnit);
				AssertEquals(3.2m, penalty.CPY_PerUnitCost);
				AssertEquals(500m, penalty.CPY_TotalCost);
				AssertEquals("AUD", penalty.CPY_RX_NKCurrency);
				AssertEquals("IMP", penalty.CPY_ProcessType);
			}
			else
			{
				AssertEquals(Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention, penalty.CPY_PenaltyType);
				AssertEquals(Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier, penalty.CPY_CreditorType);
				AssertEquals("CNSHA", penalty.CPY_RL_NKLocation);
				AssertEquals((ZDateTime)TimeSpan.FromDays(1), penalty.CPY_FreeTime);
				AssertEquals(expectedOrg.PK, penalty.CPY_OH_Creditor);
				AssertEquals((ZDateTime)TimeSpan.FromDays(10), penalty.CPY_Duration);
				AssertEquals(Core.Constants.ContainerPenaltyTimeUnit.Codes.Days, penalty.CPY_TimeUnit);
				AssertEquals(4.2m, penalty.CPY_PerUnitCost);
				AssertEquals(600m, penalty.CPY_TotalCost);
				AssertEquals("CNY", penalty.CPY_RX_NKCurrency);
				AssertEquals("EXP", penalty.CPY_ProcessType);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
		}

		TestErrorLogger logger;

		#endregion
	}
}
