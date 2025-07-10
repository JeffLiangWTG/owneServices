using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class MessageSendingObjectTest_IREQDOCMessageDataProvider : TestCaseWithFactory
	{
		[TestDate(2016, 07, 19)]
		public void TestReqdocProvider()
		{
			var zaTestHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			zaTestHelper.CreateCustomsOfficeCusCodeEntry("BFN");
			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.CustomsCodes.AddNew("CDP", "51051342", "ZA");
			testAgent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "TST", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var collection = new FinancialAccountNumberPortMapCollection();
			var mapping = collection.AddNew();
			mapping.OrganizationPK = testAgent.PK;
			mapping.CustomsOfficeCode = "BFN";
			mapping.FinancialAccountNumber = "3234002346";
			mapping.ImporterPays = true;
			mapping.AccountStartDay = 1;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			var testReference = "TestReference1";
			var testMessageSender = "TST51051342";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_OH_AgentOverride = testAgent.PK;
			instruction.CEI_Style = ProcedureCodes._20;
			invoiceLine.JI_CEI = instruction.PK;
			new LineMerger(declaration).DoMerge();
			var header = invoiceLine.CusEntryLine.Header;
			header.CH_BGMReference = testReference;
			var entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_ParentTable = "CusEntryHeader";
			entryNum.CE_ParentID = header.PK;
			entryNum.CE_EntryNum = "KFN201607115000007";
			entryNum.CE_EntryType = "MRN";
			entryNum.CE_Category = "CUS";
			entryNum.CE_IssueDate = ZDateTime.Today;
			entryNum.CE_EntryIsSystemGenerated = true;
			Factory.Save();
			var sendingObject = new MessageSendingObjectForREQDOC(header);
			sendingObject.DocumentMessageSource = "552700155ba6de52cnode1";
			var reqdocDataProvider = sendingObject as IREQDOCMessageDataProvider;
			CombineAssertions(() =>
			{
				AssertEquals("MessageType", MessageTypeList.Codes.Import, reqdocDataProvider.MessageType);
				AssertEquals("LocalReferenceNumber", testReference, reqdocDataProvider.LocalReferenceNumber);
				AssertEquals("SenderReference", ZString.Empty, reqdocDataProvider.SenderReference);
				AssertEquals("MessageFunction", MessageFunctionCodeList.Codes.Original, reqdocDataProvider.MessageFunction);
				AssertEquals("MessageTypeForDoc", MessageTypeList.Codes.Import, reqdocDataProvider.MessageTypeForDoc);
				AssertEquals("ManifestDocumentType", ZString.Empty, reqdocDataProvider.ManifestDocumentType);
				AssertEquals("FinancialAccountNumber", ZString.Empty, reqdocDataProvider.FinancialAccountNumber);
				AssertEquals("FinalMRN", "KFN201607115000007", reqdocDataProvider.FinalMRN);
				AssertEquals("DocumentMessageSource", "552700155ba6de52cnode1", reqdocDataProvider.DocumentMessageSource);
				AssertEquals("RequestDate", ZDateTime.Today, reqdocDataProvider.RequestDate);
				AssertEquals("StartDate", ZDateTime.Empty, reqdocDataProvider.StartDate);
				AssertEquals("EndDate", ZDateTime.Empty, reqdocDataProvider.EndDate);
				AssertEquals("MessageSender", testMessageSender, reqdocDataProvider.MessageSender);
				AssertEquals("ReleaseAuthority", ZString.Empty, reqdocDataProvider.ReleaseAuthority); // TODO: ToBeImplemented: Release Authority to be done by future WI #Willem 20160719
				AssertSame("Branch", declaration.Branch, reqdocDataProvider.Branch);
			});
		}
	}
}
