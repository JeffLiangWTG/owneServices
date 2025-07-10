using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business.MessageManagers;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class DeclarationMessageManagerWrapperTest : TestCaseWithFactory
	{
		public void TestPerformFunctionOperationalAction()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeList("TW", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BT", "Nanjing Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var customsOffice = TWRefCusCodeListLoader.GetCustomsOffice(Factory, "BT", ZDateTime.Now);
			customsOffice.ZZD_IsSea = true;
			customsOffice.ZZD_IsAir = false;
			Factory.Save();
			var company = GlbCompany.CurrentCompany;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			Factory.Save();
			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			var testDeclaration_IMP = Factory.New<JobDeclaration>();
			testDeclaration_IMP.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			testDeclaration_IMP.JE_CustomsOffice = "BT";
			testDeclaration_IMP.JE_DeclarationReference = "TWB000001";
			testDeclaration_IMP.MessageInitiator = messageInitiator;
			var entryInstruction = testDeclaration_IMP.CusEntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			entryInstruction.CEI_CustomsOffice = "BT";
			Factory.Save();
			var notificationCollector = new MessageNotificationCollector_ForTest();
			var sendsMessagesToCustomsGUI = new SendsMessagesToCustomsShutterUpperer();
			var ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, ediMessageCollection.Length);
			PerformFunctionOperationalAction(testDeclaration_IMP, notificationCollector, sendsMessagesToCustomsGUI);
			CombineAssertions("Result Test", () =>
			{
				ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("Message after runner", 0, ediMessageCollection.Length);
			}

			);
			var newInvoiceLine = testDeclaration_IMP.Invoices.AddNew().InvoiceLines.AddNew();
			entryInstruction.CEI_BoxNumber = "123";
			testDeclaration_IMP.DoMerge(sendsMessagesToCustomsGUI);
			Factory.Save();
			testDeclaration_IMP.JE_GS_NKCusAgent = "TT";
			testDeclaration_IMP.JE_CustomsProfile = "123-3";
			var extPassword1 = Factory.New<GlbExternalPassword>();
			extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword1.GP_GC = company.PK;
			extPassword1.GP_MailBoxID = "123-3";
			extPassword1.GP_UserID = "001";
			extPassword1.GP_GS = staff.PK;
			extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			var entry = testDeclaration_IMP.ActiveEntryHeaders[0];
			PerformFunctionOperationalAction(testDeclaration_IMP, notificationCollector, sendsMessagesToCustomsGUI);
			CombineAssertions("Result Test", () =>
			{
				ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("Message after runner", 1, ediMessageCollection.Length);
				AssertEquals(1, ediMessageCollection.Count(msg => msg.EM_LinkUniqueID == entry.PK));
			});
		}

		void PerformFunctionOperationalAction(JobDeclaration testDeclaration, MessageNotificationCollector_ForTest notificationCollector, SendsMessagesToCustomsShutterUpperer sendsMessagesToCustomsGUI)
		{
			var messageType = testDeclaration.CustomsMessageType;
			var wrapper = new JobDeclarationMessageSendingObjectParent(testDeclaration, messageType);
			if (wrapper.SendingObjectsCollection.Any())
			{
				wrapper.SendingObjectsCollection[0].ShouldSend = true;
			}

			var messageManagerWrapper = new DeclarationMessageManagerWrapper(testDeclaration, wrapper, notificationCollector, sendsMessagesToCustomsGUI, messageType);
			messageManagerWrapper.PerformFunctionOperationalAction();
		}
	}
}
