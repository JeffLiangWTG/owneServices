using System;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.Testing
{
	using Enterprise.BatchProcessor;
	using Enterprise.Customs.NZ.Business.BatchProcessor;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Customs.NZ.Business.Declaration.Testing;
	using Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NZCInterchange))]
	class NZCInterchangeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTSWOutgoingInterchangeProviderEndToEnd()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "123");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);

			declaration.JE_MessageType = "EXP";
			declaration.JE_MessageSubType = declaration.Lookups.MessageSubTypeList[0].Code;

			var entryHeader = (Declaration.FormalEntry.CusEntryHeader)declaration.CusEntryHeader;
			var messageBuilder = new TSWEX1MessageBuilder(entryHeader, null, MessageBuilder.MessageTypes.Original);
			messageBuilder.GenerateMessage();
			var message = messageBuilder.MessageBusinessObject;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var messageCollection = new NonDependentEDIMessageCollection(factory2);
			var message2 = factory2.Load<EDIMessage>(message.PK);
			messageCollection.Add(message2);

			var logger = new LoggingInformation();
			var provider = new NZTSWInterchangeProvider(logger, messageCollection);
			AssertEquals(1, provider.Interchanges.Length);

			AssertEquals(EDIInterchange.Status.eHubQueued, provider.Interchanges[0].EI_Status);
			AssertEquals(EDIInterchange.TransportType.eHub, provider.Interchanges[0].EI_TransportType);
		}
	}
}
