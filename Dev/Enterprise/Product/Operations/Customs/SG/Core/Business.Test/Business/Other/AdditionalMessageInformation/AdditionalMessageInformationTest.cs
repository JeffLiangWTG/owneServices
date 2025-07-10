using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(AdditionalMessageInformation))]
	public class AdditionalMessageInformationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPrePopulateSupportingDocuments()
		{
			var docs = new SupportingDocumentCollectionTest.eDocs();
			var doc = new SupportingDocumentCollectionTest.eDoc();
			doc.FileName = "some document name.pdf";
			doc.DocType = "doc1";
			doc.FileSizeInMB = 0.15m;
			docs.Add(doc);
			var doc1 = new SupportingDocumentCollectionTest.eDoc();
			doc1.FileName = "some other name.xls";
			doc1.DocType = "doc2";
			doc1.FileSizeInMB = 7.57m;
			docs.Add(doc1);
			var message = Factory.New<CUSDECEDIMessage>();
			message.EM_MessageText = "UNH+CWISEB00002403+CUSDEC:D:05B:UN:040+INPDEC'BGM+914:::BKT+12247970000Z        200709197224+9'CST++1'LOC+11+CZ'LOC+88+PUB'DTM+178:20070928:102'GEI+5+:Y'MEA+ABK++TUB:2.0000'MEA+AAH++KGM:4535.9250'FTX+AAI+++FE-CERT-CARGOWISE-BKT(WATER)- WITH SUPPORTING DOCS'RFF+MS:V13T.V13T003'DOC+:::DOCUMENT ATTACHMENT+001::DOC1 - SOME_DOCUMENT_NAME.PDF'DOC+:::DOCUMENT ATTACHMENT+002::DOC2 - SOME_OTHER_NAME.XLS'DOC+:::DOCUMENT ATTACHMENT+003::DOC2 - NON EXISTING DOC.DOC'NAD+AE+12247970000Z++EDI DEMONSTRATION SYSTEM SG'NAD+DT++IGOR SAFONOV'CTA+IC+:P0342222 'COM+80012200:TE'NAD+IM+51100440000G++PUBLIC UTILITIES BORAD'UNS+D'DMS+INVOICE DETAILS'MOA+39:5000.00:SGD'TOD+++FOB'NAD+SE'DOC+380+1'DTM+3:20070918:102'CST+1+22019090'FTX+AAA+++OTHER'FTX+PRD+++ENER WATER'FTX+AAC+++N'LOC+27+LC'MEA+AAF++LTR:10.0000'MEA+AAE++LTR:8.0000'MEA+AAI++LTR:0.5000'PAC+1+3+CTN'PAC+16+2+BOT'MOA+63:5000.00'MOA+146:500.0000:SGD'RFF+IV:1'RFF+SE'TAX+5++++LTR:::30.0000'MOA+161:240.00'TAX+7++++:::7'MOA+124:366.80'UNS+S'CNT+5:1'TAX+5'MOA+161:240.00'TAX+1'MOA+63:5000.00'TAX+7'MOA+124:366.80'UNT+55+CWISEB00002403'";
			var additionalMessageInformation = new AdditionalMessageInformation(message, docs, AdditionalMessageInformation.BoundFormTypes.Declaration, Factory, "SG4");
			AssertEquals(2, additionalMessageInformation.SupportingDocuments.Count);
			AssertEquals(0.15m, additionalMessageInformation.GetDocSizeInMB(doc.UniqueKey));
			AssertEquals(7.57m, additionalMessageInformation.GetDocSizeInMB(doc1.UniqueKey));
			AssertEquals("001", additionalMessageInformation.SupportingDocuments[0].DocumentType);
			AssertEquals(doc.UniqueKey, additionalMessageInformation.SupportingDocuments[0].eDoc);
			AssertEquals("002", additionalMessageInformation.SupportingDocuments[1].DocumentType);
			AssertEquals(doc1.UniqueKey, additionalMessageInformation.SupportingDocuments[1].eDoc);
		}

		public void TestHasBrokerPasswordWithType()
		{
			var factory = new BusinessObjectFactory();
			var staff1 = factory.New<GlbStaff>();
			staff1.GS_Code = "GS1";
			staff1.GS_LoginName = "testadd";
			staff1.GS_WorkPhone = "1234";
			var wrapper1 = SGGlbStaffWrapper.Get(staff1);
			wrapper1.Tradenetv4Password.GP_UserID = "vv123";
			wrapper1.Tradenetv4Password.GP_MailBoxID = "uitest";
			wrapper1.Tradenetv4Password.CurrentDecryptedPassword = "Password";
			wrapper1.SGNationalTradePlatformPassword.GP_UserID = "vv124";
			var staff2 = factory.New<GlbStaff>();
			staff2.GS_Code = "GS2";
			staff2.GS_LoginName = "testsecond";
			var wrapper2 = SGGlbStaffWrapper.Get(staff2);
			wrapper2.Tradenetv4Password.GP_UserID = "vv124";
			wrapper2.Tradenetv4Password.CurrentDecryptedPassword = "Password";
			wrapper2.SGNationalTradePlatformPassword.CurrentDecryptedPassword = "Password";
			var staff3 = factory.New<GlbStaff>();
			staff3.GS_Code = "GS3";
			staff3.GS_LoginName = "testthird";
			factory.Save();
			var additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			additionalMessageInformation.AM_Broker = "GS1";
			Assert("Both user ID and password are not empty", additionalMessageInformation.HasBrokerPasswordWithType(PasswordTypesList.Codes.SG4));
		}

		public void TestSetDefaultValues()
		{
			GlbStaff.CurrentUser.GS_Code = "IGG";
			var currentWrapper = SGGlbStaffWrapper.Get(GlbStaff.CurrentUser);
			currentWrapper.Tradenetv4Password.GP_UserID = "MAIL123";
			currentWrapper.Tradenetv4Password.GP_CurrentPassword = "0KKwb6ydAlmfdLUP+jgqwUGsR6GLqfWCNrg9I0zCnfI=";
			currentWrapper.Tradenetv4Password.CurrentDecryptedPassword = "Password";
			AssertEquals("IGG", AdditionalMessageInformation.AM_Broker);
			AssertEquals("Password", AdditionalMessageInformation.AM_BrokerPassword);
		}

		public void TestReasonForAmending()
		{
			AdditionalMessageInformation.AM_ReasonForAmending = "";
			AssertEquals(false, AdditionalMessageInformation.AM_ReasonForAmendingInfo.HasErrors());
			additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Amendment, Factory);
			AdditionalMessageInformation.AM_ReasonForAmending = "";
			AssertEquals(true, AdditionalMessageInformation.AM_ReasonForAmendingInfo.HasErrors());
			AdditionalMessageInformation.AM_ReasonForAmending = "test";
			AssertEquals(false, AdditionalMessageInformation.AM_ReasonForAmendingInfo.HasErrors());
		}

		public void TestReasonForExtendingTemporaryImportPeriod()
		{
			AssertEquals(true, AdditionalMessageInformation.AM_ReasonForExtendingTemporaryImportPeriodInfo.ReadOnly);
			AdditionalMessageInformation.AM_ExtendingTemporaryImportPeriod = true;
			AssertEquals(false, AdditionalMessageInformation.AM_ReasonForExtendingTemporaryImportPeriodInfo.ReadOnly);
		}

		public void TestCancellationCode()
		{
			AdditionalMessageInformation.AM_CancellationCode = "";
			AssertEquals(true, AdditionalMessageInformation.AM_CancellationCodeInfo.HasErrors());
			AdditionalMessageInformation.AM_CancellationCode = "XXX";
			AssertEquals(true, AdditionalMessageInformation.AM_CancellationCodeInfo.HasErrors());
			AdditionalMessageInformation.AM_CancellationCode = ReasonForCancellationCodeList.Codes.C03;
			AssertEquals(false, AdditionalMessageInformation.AM_CancellationCodeInfo.HasErrors());
			AssertEquals(false, AdditionalMessageInformation.AM_CancellationCodeInfo.HasErrors());
		}

		public void TestSupportingDocuments()
		{
			AssertNotNull(AdditionalMessageInformation.SupportingDocuments);
		}

		public void TestLookups()
		{
			AssertNotNull(AdditionalMessageInformation.Lookups);
		}

		public virtual void TestPreSaveValidation()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			additionalMessageInformation.RunPreSaveValidation();
			AssertEquals(false, additionalMessageInformation.AM_CancellationCodeInfo.HasErrors());
			AssertEquals(false, additionalMessageInformation.AM_ExtendingTemporaryImportPeriodInfo.HasErrors());
			AssertEquals(false, additionalMessageInformation.AM_ReasonForAmendingInfo.HasErrors());
			AssertEquals(false, additionalMessageInformation.AM_ReasonForExtendingTemporaryImportPeriodInfo.HasErrors());
			AssertEquals(true, additionalMessageInformation.AM_BrokerInfo.HasErrors());
			additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Amendment, Factory);
			additionalMessageInformation.RunPreSaveValidation();
			AssertEquals(false, additionalMessageInformation.AM_CancellationCodeInfo.HasErrors());
			AssertEquals(false, additionalMessageInformation.AM_ExtendingTemporaryImportPeriodInfo.HasErrors());
			AssertEquals(true, additionalMessageInformation.AM_ReasonForAmendingInfo.HasErrors());
			AssertEquals(false, additionalMessageInformation.AM_ReasonForExtendingTemporaryImportPeriodInfo.HasErrors());
			AssertEquals(true, additionalMessageInformation.AM_BrokerInfo.HasErrors());
			additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Cancellation, Factory);
			additionalMessageInformation.RunPreSaveValidation();
			AssertEquals(true, additionalMessageInformation.AM_CancellationCodeInfo.HasErrors());
			AssertEquals(false, additionalMessageInformation.AM_ExtendingTemporaryImportPeriodInfo.HasErrors());
			AssertEquals(false, additionalMessageInformation.AM_ReasonForAmendingInfo.HasErrors());
			AssertEquals(false, additionalMessageInformation.AM_ReasonForExtendingTemporaryImportPeriodInfo.HasErrors());
			AssertEquals(true, additionalMessageInformation.AM_BrokerInfo.HasErrors());
		}

		public void TestAM_Broker()
		{
			ConfigureTradenet4Brokers();
			AdditionalMessageInformation.AM_Broker = "TST";
			AssertEquals(false, AdditionalMessageInformation.AM_BrokerInfo.HasErrors());
			AdditionalMessageInformation.AM_Broker = "";
			AssertEquals(true, AdditionalMessageInformation.AM_BrokerInfo.HasErrors());
			AssertHasErrorContaining(AdditionalMessageInformation.AM_BrokerInfo, "Please enter a value");
			AdditionalMessageInformation.AM_Broker = "DDD";
			AssertEquals(true, AdditionalMessageInformation.AM_BrokerInfo.HasErrors());
			AssertHasErrorContaining(AdditionalMessageInformation.AM_BrokerInfo, "Enter a valid selection");
			AdditionalMessageInformation.AM_Broker = "TSZ";
			AssertEquals(true, AdditionalMessageInformation.AM_BrokerInfo.HasErrors());
			AssertHasErrorContaining(AdditionalMessageInformation.AM_BrokerInfo, "The selected broker does not have a Work Telephone Number");
			AssertHasErrorContaining(AdditionalMessageInformation.AM_BrokerInfo, "The selected broker does not have Declarant Code");
		}

		void ConfigureTradenet4Brokers()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbStaff staff = factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_LoginName = "testadd";
			staff.GS_WorkPhone = "1234";
			var wrapper = SGGlbStaffWrapper.Get(staff);
			wrapper.Tradenetv4Password.GP_UserID = "vv123";
			wrapper.Tradenetv4Password.GP_MailBoxID = "uitest";
			wrapper.Tradenetv4Password.GP_CurrentPassword = "0KKwb6ydAlmfdLUP+jgqwUGsR6GLqfWCNrg9I0zCnfI=";
			wrapper.Tradenetv4Password.CurrentDecryptedPassword = "Password";
			GlbStaff staff1 = factory.New<GlbStaff>();
			staff1.GS_Code = "TSZ";
			staff1.GS_LoginName = "testsecond";
			var wrapper1 = SGGlbStaffWrapper.Get(staff1);
			wrapper1.Tradenetv4Password.GP_UserID = "vv124";
			wrapper1.Tradenetv4Password.GP_CurrentPassword = "0KKwb6ydAlmfdLUP+jgqwUGsR6GLqfWCNrg9I0zCnfI=";
			wrapper1.Tradenetv4Password.CurrentDecryptedPassword = "Password";
			factory.Save();
		}

		public void TestAM_BrokerPassword()
		{
			ConfigureTradenet4Brokers();
			AdditionalMessageInformation.AM_Broker = "TST";
			AdditionalMessageInformation.AM_BrokerPassword = "Password";
			AssertEquals(false, AdditionalMessageInformation.AM_BrokerPasswordInfo.HasErrors());
			AdditionalMessageInformation.AM_BrokerPassword = "";
			AssertEquals(true, AdditionalMessageInformation.AM_BrokerPasswordInfo.HasErrors());
			AssertHasErrorContaining(AdditionalMessageInformation.AM_BrokerPasswordInfo, "Please enter a value");
			AdditionalMessageInformation.AM_BrokerPassword = "wrong";
			AssertEquals(true, AdditionalMessageInformation.AM_BrokerPasswordInfo.HasErrors());
			AssertHasErrorContaining(AdditionalMessageInformation.AM_BrokerPasswordInfo, "Please enter the current TradeNet password");
		}

		#region IAdditionalMessageInformation
		public void TestReasonForAmending_()
		{
			AdditionalMessageInformation.AM_ReasonForAmending = "Good Reason for Amendment";
			AssertEquals("Good Reason for Amendment", IAdditionalMessageInformation.ReasonForAmending);
		}

		public void TestExtendingTemporaryImportPeriod_()
		{
			AdditionalMessageInformation.AM_ExtendingTemporaryImportPeriod = true;
			AssertEquals(true, IAdditionalMessageInformation.ExtendingTemporaryImportPeriod);
		}

		public void TestReasonForExtendingTemporaryImportPeriod_()
		{
			AdditionalMessageInformation.AM_ReasonForExtendingTemporaryImportPeriod = "Good Reason for Extend";
			AssertEquals("Good Reason for Extend", IAdditionalMessageInformation.ReasonForExtendingTemporaryImportPeriod);
		}

		public void TestCancellationCode_()
		{
			AdditionalMessageInformation.AM_CancellationCode = ReasonForCancellationCodeList.Codes.C02;
			AssertEquals(ReasonForCancellationCodeList.Codes.C02, IAdditionalMessageInformation.CancellationCode);
		}

		public void TestBroker_()
		{
			AdditionalMessageInformation.AM_Broker = "IGG";
			AssertEquals("IGG", IAdditionalMessageInformation.Broker);
		}

		public void TestSupportingDocuments_()
		{
			AdditionalMessageInformation.SupportingDocuments.AddNew();
			AdditionalMessageInformation.SupportingDocuments.AddNew();
			int i = 0;
			foreach (ICusAttachment cusAttachment in IAdditionalMessageInformation.SupportingDocuments)
			{
				i++;
			}

			AssertEquals(2, i);
		}

		public void TestMessagePreview()
		{
			AssertEquals("Message text", IAdditionalMessageInformation.MessageCreated("Message text"));
			AdditionalMessageInformation.OnMessageCreated += new MessageEventHandler(additionalMessageInformation_OnMessageCreated);
			AssertEquals("Tested: Message text", IAdditionalMessageInformation.MessageCreated("Message text"));
		}

		#endregion
		#region Implementation
		IAdditionalMessageInformation IAdditionalMessageInformation
		{
			get
			{
				return AdditionalMessageInformation;
			}
		}

		AdditionalMessageInformation AdditionalMessageInformation
		{
			get
			{
				if (additionalMessageInformation == null)
				{
					additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
					additionalMessageInformation.SetBrokerPassword();
				}

				return additionalMessageInformation;
			}
		}

		AdditionalMessageInformation additionalMessageInformation;
		void additionalMessageInformation_OnMessageCreated(MessageEventArgs args)
		{
			args.MessageText = "Tested: " + args.MessageText;
		}

		#endregion
		#region Overrides
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
		}
		#endregion
	}
}
