using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CreateAndSubmitBrokerageJobRunnerTest : TestCaseWithFactory
	{
		public void TestCreateBrokerageJob()
		{
			var mutex = DeclarationBeingCreatedForShipmentMutexCreator.Create(shipment2.PK);
			mutex.Lock();
			var runner = new CreateAndSubmitBrokerageJobRunnerForTesting();
			var log = new DummyOperationalActionSectionLog();
			runner.Log = log;
			runner.Execute(shipment1);
			runner.Execute(shipment2);
			runner.Execute(shipment3);
			runner.Execute(shipment4);
			runner.Execute(shipment5);
			runner.Execute(shipment6);

			AssertEquals("WARNING: [HL Z00000001]: This Shipment does not exist or has already been deleted.", log.messages[0]);
			AssertEquals("WARNING: [HL Z00000002]: Someone else is already in the process of creating a declaration for shipment.\r\nYou should be able to access the declaration when the person has saved the record. Please try later.", log.messages[1]);
			AssertEquals("WARNING: [HL Z00000003]: Your company is not the nominated customs broker. You have chosen not to create a declaration.", log.messages[2]);
			AssertEquals("WARNING: [HL Z00000004]: This Shipment already has a Brokerage Job.", log.messages[3]);
			AssertEquals("INFO: [HL Z00000005]: A Brokerage Job was created successfully.", log.messages[4]);
			AssertEquals("WARNING: [HL Z00000006]: This is a Air Shipment. You have chosen not to create a declaration.", log.messages[5]);

			var questions = runner.AllQuestions.ToList();
			questions[0].Answer = true;
			questions[1].Answer = true;
			questions[2].Answer = true;
			runner.Execute(shipment3);
			runner.Execute(shipment6);
			AssertEquals("INFO: [HL Z00000003]: A Brokerage Job was imported successfully.", log.messages[6]);
			AssertEquals("INFO: [HL Z00000006]: A Brokerage Job was created successfully.", log.messages[7]);

			mutex.Unlock();

			Assert(!DeclarationBeingCreatedForShipmentMutexCreator.Create(shipment5.PK).HasLock);
		}

		public void TestCreateAndSubmitBrokerageJob()
		{
			var runner = new CreateAndSubmitBrokerageJobRunnerForTesting();
			runner.ExecuteSubmit = true;
			var questions = runner.AllQuestions.ToList();
			questions[1].Answer = true;
			questions[2].Answer = true;
			var log = new DummyOperationalActionSectionLog();
			runner.Log = log;
			runner.Execute(shipment3);
			runner.Execute(shipment4);
			runner.Execute(shipment5);

			AssertEquals("INFO: [HL Z00000003]: A Brokerage Job was imported successfully.", log.messages[0]);
			AssertEquals("INFO: [HL Z00000003]: Submit Succeeded.", log.messages[1]);
			AssertEquals("INFO: [HL Z00000004]: Submit Succeeded.", log.messages[2]);
			AssertEquals("INFO: [HL Z00000005]: A Brokerage Job was imported successfully.", log.messages[3]);
			AssertEquals("INFO: [HL Z00000005]: Submit Succeeded.", log.messages[4]);
		}

		#region Implementation

		ForwardingShipment shipment1;
		ForwardingShipment shipment2;
		ForwardingShipment shipment3;
		ForwardingShipment shipment4;
		ForwardingShipment shipment5;
		ForwardingShipment shipment6;

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.SetCountry("AU");

			GlbCompany otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			otherCompany.GC_Code = "XYZ";
			GlbBranch newBranch = otherCompany.Branches.AddNew();
			newBranch.GB_RL_NKHomePort = "USCHI";
			newBranch.GB_Code = "QAZ";
			Factory.Save();

			shipment1 = new BusinessObjectFactory().New<ForwardingShipment>();
			shipment1.FillWithValidTestData();
			shipment1.JS_UniqueConsignRef = "Z00000001";
			shipment2 = Factory.New<ForwardingShipment>();
			shipment2.FillWithValidTestData();
			shipment2.JS_UniqueConsignRef = "Z00000002";
			shipment3 = Factory.New<ForwardingShipment>();
			shipment3.FillWithValidTestData();
			shipment3.JS_UniqueConsignRef = "Z00000003";
			var declaration3 = Factory.New<BaseJobDeclaration>();
			declaration3.JE_JS = shipment3.PK;
			declaration3.JE_DeclarationReference = shipment3.JS_UniqueConsignRef;
			declaration3.JE_GB = newBranch.PK;
			shipment4 = Factory.New<ForwardingShipment>();
			shipment4.FillWithValidTestData();
			shipment4.JS_UniqueConsignRef = "Z00000004";
			var declaration4 = Factory.New<BaseJobDeclaration>();
			declaration4.JE_JS = shipment4.PK;
			declaration4.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration4.JE_DeclarationReference = shipment4.JS_UniqueConsignRef;
			shipment5 = Factory.New<ForwardingShipment>();
			shipment5.FillWithValidTestData();
			shipment5.JS_UniqueConsignRef = "Z00000005";
			shipment5.JS_RL_NKOrigin = "AUSYD";
			shipment5.JS_RL_NKDestination = "NZAKL";
			shipment5.JS_OH_ExportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var declaration5 = Factory.New<BaseJobDeclaration>();
			declaration5.JE_JS = shipment5.PK;
			declaration5.JE_DeclarationReference = shipment5.JS_UniqueConsignRef;
			declaration5.JE_GB = newBranch.PK;
			shipment6 = Factory.New<ForwardingShipment>();
			shipment6.FillWithValidTestData();
			shipment6.JS_UniqueConsignRef = "Z00000006";
			shipment6.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment6.JS_RL_NKOrigin = "AUSYD";
			shipment6.JS_RL_NKDestination = "NZAKL";
			shipment6.JS_OH_ExportBroker = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
		}

		#endregion
	}
}
