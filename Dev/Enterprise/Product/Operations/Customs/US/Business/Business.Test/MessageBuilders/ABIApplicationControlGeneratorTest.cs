using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ABIApplicationControlGeneratorTest : TestCaseWithFactory
	{
		[TestDate(1971, 9, 18)]
		public void TestAddMessage()
		{
			var blockControlGenerator = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			blockControlGenerator.AddMessageBlock(new ACHQT());
			var ediMessage = blockControlGenerator.CreateMessage<MQEDIMessage>(Factory);
			var applicationControlGenerator = ApplicationControlGenerator.New(CBPEDIInterchange.ApplicationCodes.USCustomsImport, "", GlbBranch.CurrentBranch);
			applicationControlGenerator.AddMessage(ediMessage);

			AssertEquals("A3901XJ5      09187101               89".PadRight(80), applicationControlGenerator.A.Serialise());
			AssertEquals("B018888XJ5                                  89             <<MSGNO PLACEHOLDER>>".PadRight(80) +
				"QT                      0000000000".PadRight(80) +
				"Y  8888XJ5  00001".PadRight(80), applicationControlGenerator.GetBody());
			AssertEquals("Z3901XJ5      09187101               89".PadRight(80), applicationControlGenerator.Z.Serialise());
		}

		[TestDate(1971, 9, 18)]
		public void TestAddMessage_K1()
		{
			var applicationControlGenerator = ApplicationControlGenerator.New(CBPEDIInterchange.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, GlbBranch.CurrentBranch);
			AssertEquals("A3901XJ5      091871     KI          89".PadRight(80), applicationControlGenerator.A.Serialise());
		}

		[TestDate(1971, 9, 18)]
		public void TestAddMessage_T1()
		{
			var applicationControlGenerator = ApplicationControlGenerator.New(CBPEDIInterchange.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.AddCBPFormCBPF5106DatatotheImporterFile, GlbBranch.CurrentBranch);
			AssertEquals("A3901XJ5      091871     TI          89".PadRight(80), applicationControlGenerator.A.Serialise());
		}

		[TestDate(1971, 9, 18)]
		public void TestAddMessage_T1_TP()
		{
			var applicationControlGenerator = ApplicationControlGenerator.New(CBPEDIInterchange.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.AddNewCBPFormCBPF5106DatatotheImporterFile, GlbBranch.CurrentBranch);
			AssertEquals("A3901XJ5      091871     TP          89".PadRight(80), applicationControlGenerator.A.Serialise());
		}

		[TestDate(1971, 9, 18)]
		public void TestNew()
		{
			var filer = new Registry.Business.Customs.US.ExportEntryFilerID();
			filer.EntryFilerID = "364-33-1434";
			filer.EntryFilerIDType = "S";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			var message = Factory.New<MQEDIMessage>();
			message.EM_MessageNum = "320989";
			var applicationControlGenerator = ApplicationControlGenerator.New(EDIMessage.ApplicationCodes.USCustomsImport, "XN", GlbBranch.CurrentBranch);
			AssertEquals(typeof(ABIApplicationControlGenerator), applicationControlGenerator.GetType());
			AssertEquals(typeof(APLA), applicationControlGenerator.A.GetType());
			AssertEquals(typeof(APLZ), applicationControlGenerator.Z.GetType());
			applicationControlGenerator.AddMessage(message);
			AssertEquals("A3901XJ5      09187101               89".PadRight(80), applicationControlGenerator.A.Serialise());
			AssertEquals("Z3901XJ5      09187101               89".PadRight(80), applicationControlGenerator.Z.Serialise());
		}

		[TestDate(1971, 9, 18)]
		public void TestDifferentOfficeCodeForAAndBRecords()
		{
			USCustomsDataRegistry.Instance.ARecordOfficeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "10");
			USCustomsDataRegistry.Instance.BRecordOfficeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "20");
			var blockControlGenerator = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			blockControlGenerator.AddMessageBlock(new ACHQT());
			var ediMessage = blockControlGenerator.CreateMessage<MQEDIMessage>(Factory);
			var applicationControlGenerator = ApplicationControlGenerator.New(CBPEDIInterchange.ApplicationCodes.USCustomsImport, "", GlbBranch.CurrentBranch);
			applicationControlGenerator.AddMessage(ediMessage);

			AssertEquals("A3901XJ5      09187101               10".PadRight(80), applicationControlGenerator.A.Serialise());
			AssertEquals("B018888XJ5                                  20             <<MSGNO PLACEHOLDER>>".PadRight(80) +
				"QT                      0000000000".PadRight(80) +
				"Y  8888XJ5  00001".PadRight(80), applicationControlGenerator.GetBody());
			AssertEquals("Z3901XJ5      09187101               10".PadRight(80), applicationControlGenerator.Z.Serialise());
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetupForSendMessage();
		}
	}
}
