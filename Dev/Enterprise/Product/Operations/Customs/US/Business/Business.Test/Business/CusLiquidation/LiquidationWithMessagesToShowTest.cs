using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(LiquidationWithMessagesToShow))]
	sealed class LiquidationWithMessagesToShowTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusLiquidation liquidation = Factory.New<CusLiquidation>();
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_LinkUniqueID = liquidation.PK;
			CusLiquidation liquidation2 = Factory.New<CusLiquidation>();
			MQEDIMessage message2 = Factory.New<MQEDIMessage>();
			message2.EM_LinkUniqueID = liquidation2.PK;
			declaration.Liquidations.Add(liquidation);
			declaration.Liquidations.Add(liquidation2);
			IMessageAttacheeInDeclaration liquidationWithAllMessages = new LiquidationWithMessagesToShow(declaration.Liquidations);
			AssertEquals(2, liquidationWithAllMessages.ParentPKsOfMessages.Count);
		}

		public void TestIMessageAttacheeInDeclaration()
		{
			CusLiquidation liquidation = Factory.New<CusLiquidation>();
			liquidation.B8_EntryNumber = "41449099";
			liquidation.B8_EntryFilerCode = "AZ2";
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_LinkUniqueID = liquidation.PK;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00013";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_EntryFilerCode = "AZ2";
			declaration.Liquidations.Add(liquidation);
			CusEntryHeader formalEntry = declaration.ActiveEntryHeaders.AddNew();
			formalEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			formalEntry.EntryNumber = "41449099";
			LiquidationWithMessagesToShow liquidationForGrid = new LiquidationWithMessagesToShow(declaration.Liquidations);
			IMessageAttacheeInDeclaration msgAttachee = liquidationForGrid;
			AssertEquals("HumanFriendlyReference", "41449099", msgAttachee.HumanFriendlyReference);
			AssertEquals(MessageAttacheeRecordType.Liquidation, msgAttachee.RecordType);
			AssertEquals(MessageAttacheeRecordTypeDescriptions.Liquidation, msgAttachee.RecordTypeDescription);
			AssertEquals("Status", "", msgAttachee.MessageStatus);
			AssertEquals("Entry Status", "", msgAttachee.EntryStatus);
			AssertEquals("Job Reference Number", "B00013", msgAttachee.JobReferenceNumber);
			AssertEquals(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), msgAttachee.Branch);
			AssertEquals("Processing District Port", "", msgAttachee.ProcessingDistrictPort);
			AssertEquals("Processing Office Code", "", msgAttachee.ProcessingOfficeCode);
			AssertEquals("Entry Filer Code", "AZ2", msgAttachee.EntryFilerCode);
			AssertEquals("TIBExpiryDate", ZDateTime.Empty, msgAttachee.TIBExpiryDate);
			AssertEquals("TIBNumOfExtensions", 0, msgAttachee.TIBNumOfExtensions);
			AssertEquals("Email Address Recipient", ZString.Empty, msgAttachee.GetFallbackEmailAddressRecipient());
			AssertEquals("Messages To Show in Grid on Declaration > Messages Tab", 1, msgAttachee.ParentPKsOfMessages.Count);
			AssertEquals("Transport Mode", Core.Constants.TransportModes.Sea, msgAttachee.TransportMode);
		}

		public void TestTIBNumOfExtensions()
		{
			var liquidation = Factory.New<CusLiquidation>();
			liquidation.B8_EntryNumber = "41449099";
			liquidation.B8_EntryFilerCode = "AZ2";
			liquidation.B8_SystemCreateDate = new ZDateTime(2000, 02, 02);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Liquidations.Add(liquidation);
			var msgAttachee = (IMessageAttacheeInDeclaration)new LiquidationWithMessagesToShow(declaration.Liquidations);
			AssertEquals("TIBNumOfExtensions", 0, msgAttachee.TIBNumOfExtensions);

			var liquidation1 = Factory.New<CusLiquidation>();
			liquidation1.B8_SystemCreateDate = new ZDateTime(2020, 02, 02);
			liquidation1.B8_NoOfSuspensions = 2;
			declaration.Liquidations.Add(liquidation1);
			AssertEquals("TIBNumOfExtensions", 2, msgAttachee.TIBNumOfExtensions);

			var liquidation2 = Factory.New<CusLiquidation>();
			liquidation2.B8_SystemCreateDate = new ZDateTime(2022, 02, 02);
			liquidation2.B8_NoOfSuspensions = 1;
			declaration.Liquidations.Add(liquidation2);
			var liquidation3 = Factory.New<CusLiquidation>();
			liquidation3.B8_SystemCreateDate = new ZDateTime(2023, 02, 02);
			liquidation3.B8_NoOfSuspensions = 0;
			declaration.Liquidations.Add(liquidation3);
			AssertEquals("TIBNumOfExtensions", 1, msgAttachee.TIBNumOfExtensions);
		}

		protected override BusinessObject GetNewBusinessObject() => new LiquidationWithMessagesToShow(new CusLiquidationCollection(Factory));
	}
}
