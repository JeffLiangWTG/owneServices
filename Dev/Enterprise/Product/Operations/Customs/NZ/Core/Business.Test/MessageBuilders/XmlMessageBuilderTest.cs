using System;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.Testing
{
	public class XmlMessageBuilderTest : MessageBuilderTest
	{
		public override void TestGenerateTestMessage()
		{
			NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			DecCreator.SetupTestConsignmentDetails();
			DecCreator.SetupTestForAir();
			DecCreator.SetupTestForImportFromAU();
			DecCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			DecCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			DecCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			DecCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			DecCreator.MergeDeclaration();
			DecCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);

			messageBuilder = new XmlMessageBuilder(Declaration.CusEntryHeader);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("0", messageBuilder.PostedMessageNumber);
			messageBuilder.GenerateMessage();
			Assert(messageBuilder.PostedMessageNumber != "0");
			AssertEquals("This is implemented in inherited classes.", "", messageBuilder.GetMessageText());
		}

		[TestDate(2024, 3, 1)]
		public void TestGenerateQueuedMessageForDeclaration()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.NewZealand))
			{
				DecCreator.SetupTestConsignmentDetails();
				var queuedDate = new ZDateTime(2024, 3, 21);
				Declaration.JE_EDITransmitDate = queuedDate;
				messageBuilder = new XmlMessageBuilder(Declaration.CusEntryHeader);
				messageBuilder.GenerateMessage();
				var message = messageBuilder.MessageBusinessObject;
				AssertEquals("Message is queued in UTC date", queuedDate.ToUniversalBranchTime(Factory).AddMinutes(15), message.EM_HeldUntilDate);
			}
		}

		public void TestSetParentMessagingStatusAfterMessagePostingSetsJE_EntrySubmittedDate()
		{
			NZCustomsDataRegistry.Instance.ExportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NZCustomsDataRegistry.Instance.ImportEciTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			ECIDecCreator = new TestECIWriteOffCreator(Declaration);
			AssertNotNull("Should be instantiated", ECIDecCreator);

			ECIDecCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			ECIDecCreator.SetupTestForAir();
			ECIDecCreator.SetupTestForImportFromAU();
			ECIDecCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			AssertEquals("Precondition: JobDeclaration.JE_EDITransmitDate", ZDate.Empty, Declaration.JE_EDITransmitDate);

			messageBuilder = new XmlMessageBuilder(Declaration.CusEntryHeader);
			messageBuilder.GenerateMessage();
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals(FormalEntryStatusList.Codes.SentToCustoms, Declaration.JE_EntryStatus);
			AssertEquals("JobDeclaration.JE_EntrySubmittedDate after post has a value", true, Declaration.JE_EntrySubmittedDate.IsValid);
			AssertEquals("JobDeclaration.JE_EntrySubmittedDate after post", Declaration.CachedTodaysDate, Declaration.JE_EntrySubmittedDate);
		}

		public override void TestGenerateLiveMessage()
		{
			//TOTO: implement
			Assert(true);
		}

		#region DecCreator
		TestFormalEntryCreator DecCreator
		{
			get
			{
				if (fDecCreator == null)
				{
					fDecCreator = new TestFormalEntryCreator(Declaration);
				}
				return fDecCreator;
			}
		}
		TestFormalEntryCreator fDecCreator;

		protected TestECIWriteOffCreator ECIDecCreator
		{
			get { return (TestECIWriteOffCreator)eciDecCreator; }
			set { eciDecCreator = value; }
		}
		protected TestDeclarationCreator eciDecCreator;
		#endregion

		JobDeclaration Declaration
		{
			get
			{
				if (jobDeclaration == null)
				{
					jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				}

				return jobDeclaration;
			}
		}
		JobDeclaration jobDeclaration;
	}
}
