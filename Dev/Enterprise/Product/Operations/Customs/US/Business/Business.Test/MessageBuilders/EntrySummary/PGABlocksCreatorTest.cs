using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	class PGABlocksCreatorTest : TestCaseWithFactory
	{
		public void TestShouldOnlyOneOIRecordIsGenerated()
		{
			SetUpData();

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;

			var action = GetAction(declaration);

			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertEquals("Should have only one OI record in each message", 2, ((string)message.EM_MessageText).Split(new[] { "OI" }, StringSplitOptions.RemoveEmptyEntries).Length);
		}

		protected OrgAddress AddCustomsAddress(OrgHeader header, ZString adr1, ZString adr2, ZString city, ZString state, ZString postCode, ZString phone, ZString fax, ZString email)
		{
			var impoterCustomsAddress = header.Addresses.AddNew();
			impoterCustomsAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.CustomsAddressOfRecord);
			UpdateAddress(impoterCustomsAddress, adr1, adr2, city, state, postCode, phone, fax, email);
			return impoterCustomsAddress;
		}

		protected void UpdateAddress(OrgAddress address, ZString adr1, ZString adr2, ZString city, ZString state, ZString postCode, ZString phone, ZString fax, ZString email)
		{
			address.OA_Address1 = adr1;
			address.OA_Address2 = adr2;
			address.OA_City = city;
			address.OA_State = state;
			address.OA_PostCode = postCode;
			address.OA_Phone = phone;
			address.OA_Fax = fax;
			address.OA_Email = email;
		}

		protected virtual void SetUpData()
		{
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			USCustomsDataRegistry.Instance.DoDefaultShipTo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLine = invoiceLine.CusEntryLine;
		}

		public void ClearPGAContactInfo(JobDeclaration declaration)
		{
			declaration.US_FDAContactName = ZString.Empty;
			declaration.US_FDAContactPhoneNo = ZString.Empty;
			declaration.US_FDAContactEmail = ZString.Empty;
		}

		protected ISEAdditionalData GetAction(JobDeclaration declaration, bool certifyCargoRelease = true)
		{
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(declaration.ActiveEntryHeaders.EntrySummaryEntry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_CertifyCargoRelease = certifyCargoRelease;
			action.US_AcknowledgeAndSign = true;
			return ACEEntrySummaryMessageSendingOption.New(action);
		}

		PGABlocksCreator pgaBlocksCreator;
		protected PGABlocksCreator PGABlocksCreator => pgaBlocksCreator ?? (pgaBlocksCreator = new PGABlocksCreator());

		protected JobDeclaration declaration;
		protected JobComInvoiceLine invoiceLine;
		protected CusEntryLine entryLine;
	}
}
