using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class STATACREQDOCSendingObjectTest_IREQDOCMessageDataProvider : TestCaseWithFactory
	{
		[TestDate(2016, 07, 19)]
		public void TestSTATACREQDOCProvider()
		{
			var zaTestHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			zaTestHelper.CreateCustomsOfficeCusCodeEntry("BFN");
			zaTestHelper.CreateCustomsStatusCusCodeEntry("1");
			Factory.Save();
			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, "51051342", Core.Constants.CountryCodes.SouthAfrica);
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
			Factory.Save();
			var reqdocDataProvider = new STATACREQDOCSendingObject(new STATACREQDOCSendingObjectParent(Factory), mapping) as IREQDOCMessageDataProvider;
			CombineAssertions(() =>
			{
				AssertEquals("MessageType", MessageTypeList.Codes.StatementOfAccount, reqdocDataProvider.MessageType);
				AssertEquals("LocalReferenceNumber", ZString.Empty, reqdocDataProvider.LocalReferenceNumber);
				AssertEquals("SenderReference", mapping.FinancialAccountNumber, reqdocDataProvider.SenderReference);
				AssertEquals("MessageFunction", MessageFunctionCodeList.Codes.Original, reqdocDataProvider.MessageFunction);
				AssertEquals("MessageTypeForDoc", MessageTypeList.Codes.StatementOfAccount, reqdocDataProvider.MessageTypeForDoc);
				AssertEquals("ManifestDocumentType", ZString.Empty, reqdocDataProvider.ManifestDocumentType);
				AssertEquals("FinancialAccountNumber", mapping.FinancialAccountNumber, reqdocDataProvider.FinancialAccountNumber);
				AssertEquals("FinalMRN", ZString.Empty, reqdocDataProvider.FinalMRN);
				AssertEquals("ObjectID", ZString.Empty, reqdocDataProvider.DocumentMessageSource);
				AssertEquals("RequestDate", ZDateTime.Today, reqdocDataProvider.RequestDate);
				AssertEquals("StartDate", new ZDateTime(2016, 07, 01), reqdocDataProvider.StartDate);
				AssertEquals("EndDate", ZDateTime.Today, reqdocDataProvider.EndDate);
				AssertEquals("MessageSender", "TST51051342", reqdocDataProvider.MessageSender);
				AssertEquals("ReleaseAuthority", ZString.Empty, reqdocDataProvider.ReleaseAuthority);
				AssertSame("Branch", GlbBranch.CurrentBranch, reqdocDataProvider.Branch);
			});
		}
	}
}
