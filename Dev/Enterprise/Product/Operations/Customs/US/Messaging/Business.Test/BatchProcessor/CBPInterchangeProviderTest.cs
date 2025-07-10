using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class InterchangeProviderTest : TestCaseWithFactory
	{
		[TestDate(2006, 08, 16)]
		public void TestInterchangesCreatedByMessages()
		{
			var collection = new NonDependentEDIMessageCollection(Factory);
			var block1 = new BlockControlGeneratorForTesting();
			block1.AddMessageBlock(new ZZZC() { StringC = "C1" });
			var message1 = block1.CreateMessage<CBPMessageForTesting>(Factory);
			var block2 = new BlockControlGeneratorForTesting();
			block2.AddMessageBlock(new ZZZC() { StringC = "C2" });
			var message2 = block2.CreateMessage<CBPMessageForTesting>(Factory);
			collection.Add(message1);
			collection.Add(message2);

			EDIInterchange[] interchanges = new CBPInterchangeProviderForTesting(collection).Interchanges;

			AssertEquals(EDIMessage.Status.Sent, message1.EM_Status);
			AssertEquals(EDIMessage.Status.Sent, message2.EM_Status);

			AssertEquals(2, interchanges.Length);

			AssertInterchange(interchanges[0],
				CBPEDIInterchange.ApplicationCodeForTesting,
				"Z¿ºA",
				"Z¿ºB".PadRight(80) +
				"Z¿ºC                          C1".PadRight(80) +
				"Z¿ºY",
				"Z¿ºZ",
				ApplicationIdentifierCodeList.DummyForTesting1);
			AssertInterchange(interchanges[1],
				CBPEDIInterchange.ApplicationCodeForTesting,
				"Z¿ºA",
				"Z¿ºB".PadRight(80) +
				"Z¿ºC                          C2".PadRight(80) +
				"Z¿ºY",
				"Z¿ºZ",
				ApplicationIdentifierCodeList.DummyForTesting1);
		}

		[TestDate(2006, 08, 16)]
		public void TestInterchangesCreatedByMessagesHasCorrectApplicationCode()
		{
			var collection = new NonDependentEDIMessageCollection(Factory);
			var block1 = new BlockControlGeneratorForTesting();
			block1.AddMessageBlock(new ZZZC() { StringC = "C1" });
			var message1 = block1.CreateMessage<CBPMessageForTesting>(Factory);
			AssertEquals(CBPEDIInterchange.ApplicationCodeForTesting, message1.EM_ApplicationCode);
			var block2 = new BlockControlGeneratorForTesting("@#@");
			block2.AddMessageBlock(new ZZZC() { StringC = "C2" });
			var message2 = block2.CreateMessage<CBPMessageForTesting>(Factory);
			AssertEquals("@#@", message2.EM_ApplicationCode);
			collection.Add(message1);
			collection.Add(message2);

			EDIInterchange[] interchanges = new CBPInterchangeProviderForTesting(collection).Interchanges;

			AssertEquals(EDIMessage.Status.Sent, message1.EM_Status);
			AssertEquals(EDIMessage.Status.Discarded, message2.EM_Status);

			AssertEquals(1, interchanges.Length);

			var interchange1 = interchanges[0];

			AssertInterchange(interchange1,
				CBPEDIInterchange.ApplicationCodeForTesting,
				"Z¿ºA",
				"Z¿ºB".PadRight(80) +
				"Z¿ºC                          C1".PadRight(80) +
				"Z¿ºY",
				"Z¿ºZ",
				ApplicationIdentifierCodeList.DummyForTesting1);
			AssertEquals(0, message1.Notes.FindByDescription("Processing Log").Length);
			AssertEquals(@"Message  will be discarded for the following reason:
There is no Customs Interchange Sender ID set up
for Company - EDI (OrgProxy:EDICUS), Branch - BNE", message2.Notes.FindByDescription("Processing Log").Single().ST_NoteText);
			if (CargoWise.Common.ErrorReporter.LastKeyReported == "ApplicationControlGenerator for Application Code '@#@' and Application Identifier '₧ƒ' is unknown")
			{
				AssertEquals("Cannot determine the ApplicationControlGenerator object for Application Code '@#@' and Application Identifier '₧ƒ'", CargoWise.Common.ErrorReporter.LastMessageReported);
				CargoWise.Common.ErrorReporter.Clear();
			}
			using (var generatorDisposable = ApplicationControlGeneratorTestClass.TemporarySetup(message1.Branch))
			{
				generatorDisposable.generator.settingDetailsForTesting = "SETTING DETAILS FOR TESTING";
				generatorDisposable.generator.A.FilerID = ZString.Empty;
				message1 = block1.CreateMessage<CBPMessageForTesting>(Factory);
				collection = new NonDependentEDIMessageCollection(Factory);
				collection.Add(message1);
				interchanges = new CBPInterchangeProviderForTesting(collection).Interchanges;
				AssertEquals(0, interchanges.Length);
				AssertEquals(EDIMessage.Status.Discarded, message1.EM_Status);
				AssertEquals(@"Message  will be discarded for the following reason:
There is no Customs Interchange Sender ID set up
SETTING DETAILS FOR TESTING", message1.Notes.FindByDescription("Processing Log").Single().ST_NoteText);
			}
		}

		void AssertInterchange(EDIInterchange interchange, string applicationCode, string headerText, string bodyText, string footerText, string interchangeType)
		{
			AssertEquals(applicationCode, interchange.EI_ApplicationCode);
			AssertEquals(headerText, interchange.EI_HeaderText);
			AssertMultilineASCIIEquals("body text", bodyText, interchange.EI_BodyText);
			AssertEquals(footerText, interchange.EI_FooterText);
			AssertEquals(interchangeType, interchange.EI_InterchangeType);
		}

		[TestDate(2006, 08, 16)]
		public void TestApplicationCodeMessageTypeInABlock()
		{
			var collection = new NonDependentEDIMessageCollection(Factory);
			var block1 = new BlockControlGeneratorForTesting();
			block1.AddMessageBlock(new ZZZC() { StringC = "C1" });
			var message1 = block1.CreateMessage<CBPMessageForTesting>(Factory);
			collection.Add(message1);

			EDIInterchange[] interchanges = new CBPInterchangeProviderForTesting(collection).Interchanges;
			AssertEquals("Z¿ºA", interchanges[0].EI_HeaderText);
			AssertEquals(ApplicationIdentifierCodeList.DummyForTesting1, interchanges[0].EI_InterchangeType);
		}

		[TestDate(2010, 08, 30)]
		public void TestGetCollationKey()
		{
			var collection = new NonDependentEDIMessageCollection(Factory);
			var block1 = new BlockControlGeneratorForTesting();
			block1.AddMessageBlock(new ZZZC() { StringC = "C1" });
			var message1 = block1.CreateMessage<CBPMessageForTesting>(Factory);
			collection.Add(message1);

			var provider = new CBPInterchangeProviderForTesting(collection);
			AssertEquals("Collation Key should not be set for CBP Messaging to ensure Message Sort is not corrupted by Interchange Provider collation", "DONOTCOLLATE", provider.GetCollationKey_protected(message1));
		}

		[TestDate(2021, 12, 02)]
		public void TestInterchangeHasSameBranchFromMessage()
		{
			var auCompany = Factory.NewWithValidTestData<GlbCompany>();
			auCompany.GC_Name = "AU Company";
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var auBranch = auCompany.Branches.AddNew();
			auBranch.GB_Code = "~A~";
			auBranch.GB_BranchName = "AU Branch";

			var othBranch = auCompany.Branches.AddNew();
			othBranch.GB_Code = "~O~";
			othBranch.GB_BranchName = "Other Branch";
			Factory.Save();

			NonDependentEDIMessageCollection messageCollection;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				messageCollection = new NonDependentEDIMessageCollection(Factory);
				var messageBlock = new BlockControlGeneratorForTesting();
				messageBlock.AddMessageBlock(new ZZZC() { StringC = "C1" });
				var message = messageBlock.CreateMessage<CBPMessageForTesting>(Factory);
				messageCollection.Add(message);
				AssertEquals("Message is created under AU branch", auBranch.PK, message.EM_GB);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, othBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var interchanges = new CBPInterchangeProviderForTesting(messageCollection).Interchanges;
				AssertEquals("Interchange is created under AU branch", auBranch.PK, interchanges[0].EI_GB);
			}
		}

		[TestDate(2022, 02, 02)]
		public void TestMessageIsDiscardedWhenBodyTextIsEmpty()
		{
			try
			{
				var collection = new NonDependentEDIMessageCollection(Factory);
				var block1 = new BlockControlGeneratorForTesting();
				block1.AddMessageBlock(new ZZZC() { StringC = "C1" });
				var message1 = block1.CreateMessage<CBPMessageForTesting>(Factory);
				message1.EM_MessageText = ZString.Empty;
				var block2 = new BlockControlGeneratorForTesting();
				block2.AddMessageBlock(new ZZZC() { StringC = "C2" });
				var message2 = block2.CreateMessage<CBPMessageForTesting>(Factory);
				collection.Add(message1);
				collection.Add(message2);
				var interchanges = new CBPInterchangeProviderForTesting(collection).Interchanges;
				AssertEquals("Only 1 interchange is created", 1, interchanges.Length);
				AssertEquals("Message is discarded becasue message text is empty", EDIMessage.Status.Discarded, message1.EM_Status);
				AssertEquals(ZGuid.Empty, message1.EM_EI);
				AssertEquals("Message is sent", EDIMessage.Status.Sent, message2.EM_Status);
				AssertEquals(interchanges[0].PK, message2.EM_EI);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		sealed class CBPInterchangeProviderForTesting : CBPInterchangeProvider
		{
			public CBPInterchangeProviderForTesting(NonDependentEDIMessageCollection messages)
				: base(messages)
			{
			}

			public string GetCollationKey_protected(EDIMessage message) => GetCollationKey(message);

			protected override Type InterchangeType => typeof(CBPEDIInterchangeForTesting);
		}
	}
}
