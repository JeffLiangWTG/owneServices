using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class AllocateNumberButtonClickEventHandlerTest : TestCaseWithFactory
	{
		public void TestAllocateEntryNumberButton()
		{
			AssertAllocateEntryNumberButton(true, ZString.Empty);
			AssertAllocateEntryNumberButton(false, ClearanceStatusCodeList.Codes.C1);
		}

		void AssertAllocateEntryNumberButton(bool expectEnable, ZString entryStatus)
		{
			var declaration = Declaration;
			var cusHead1 = declaration.EntryHeader;
			var entryInstruction = declaration.CusEntryInstruction;
			cusHead1.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_Style = "";
			var entryHeader = declaration.EntryHeader;

			entryHeader.EntryNumber = "CABF0945600030";
			var entryNumber = entryHeader.CusEntryNumber;
			entryNumber.CE_EntryStatus = entryStatus;

			Factory.Save();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				var allocateEntryNumberButton = jobDeclarationUserControl.FindSingle<ZButton>("AllocateEntryNumberButton");
				AssertEquals(expectEnable, jobDeclarationUserControl.AllocateEntryNumberButton.Enabled);
			}
		}

		public void TestJobDeclarationAllocateWhenStopped()
		{
			var declaration = Declaration;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				var modifyEntryNumberButton = jobDeclarationUserControl.FindSingle<ZButton>("ModifyEntryNumberButton");
				var cusHead1 = declaration.EntryHeader;
				var entryInstruction = declaration.CusEntryInstruction;
				cusHead1.CH_CEI_Instruction = entryInstruction.PK;
				entryInstruction.CEI_Style = "";
				declaration.EntryHeader.CH_Status = "";
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				modifyEntryNumberButton.PerformClick();
				AssertEquals("The generator rules could not be found for the job.", UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.CusEntryInstruction.CEI_Style = "G1";
				declaration.EntryNumber = "AA01";
				Factory.Save();
				var factory2 = new BusinessObjectFactory();
				factory2.RefreshEnabled = false;
				var declarationInFactory2 = factory2.Load<JobDeclaration>(declaration.PK);
				var entryNumber = declarationInFactory2.EntryNumber;
				var warningMessage = string.Format("There is Entry Number (AA01) already allocated for this job. Are you sure you wish to continue?", entryNumber);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				modifyEntryNumberButton.PerformClick();
				AssertEquals("Entry Number is not allocated", entryNumber, declaration.EntryNumber);
				AssertEquals(warningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.EntryNumber = "";
				Factory.Save();
				AssertEquals(true, declarationInFactory2.LockEntryNumberAllocationMutex);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				modifyEntryNumberButton.PerformClick();
				AssertEquals(declaration.GetEntryNumberAllocationMutexLockInfo() + " is in the process of allocating Entry Number for this job.\r\nPlease re-open the job later.", UnitTestUserNotification.Instance.LastMessage.Text);
				var company = declaration.Company;
				var provider = company.CustomsNumberProvider;
				provider.CustomsNumbers.DeleteAll();
				provider.CustomsNumberWrappers.RemoveAndDeleteAll();
				declarationInFactory2.UnlockEntryNumberAllocationMutex();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				modifyEntryNumberButton.PerformClick();
				AssertNotEquals(declaration.GetEntryNumberAllocationMutexLockInfo() + " is in the process of allocating Entry Number for this job.\r\nPlease re-open the job later.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				modifyEntryNumberButton.PerformClick();
				AssertContains(@"This form must be saved before Entry Number is allocated.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2020, 8, 5)]
		public void TestJobDeclarationAllocateWhenIsWaitingForResponseOrHasBeenLodgedAtCustoms()
		{
			var declaration = Declaration;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				var modifyEntryNumberButton = jobDeclarationUserControl.FindSingle<ZButton>("ModifyEntryNumberButton");
				var entryHeader = declaration.EntryHeader;
				var entryInstruction = declaration.CusEntryInstruction;
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryHeader.CH_EntryStatus = EntryStatusCodeList.Codes.IEM;
				entryHeader.CH_DeclarationIncoterm = "FOB";
				entryInstruction.CEI_DateForDuty = new ZDateTime(2020, 08, 05);
				entryInstruction.CEI_Style = "G5";
				declaration.JE_TransportMode = "SEA";
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				modifyEntryNumberButton.PerformClick();
				AssertEquals("The job is still waiting for a response or has been acknowledged by the customs. Allocating a new entry number means that this declaration job will be treated as a new entry in the customs’ system. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNullOrEmpty(declaration.EntryNumber);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var dialog = (AllocateNumberForm)obj;
					var allocateNumber = (AllocateNumber)dialog.BusinessEntity;
					dialog.FindSingle<ZButton>("OKButton").PerformClick();
				}

				);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				modifyEntryNumberButton.PerformClick();
				AssertNotNullOrEmpty(declaration.EntryNumber);
			}
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.NewWithValidTestData<JobDeclaration>();
					declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
					declaration.JE_GS_NKCusAgent = "TT";
					declaration.JE_CustomsProfile = "123-3";
					declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
					declaration.ActiveEntryHeaders.AddNew();
					var entryInstruction = declaration.CusEntryInstruction;
					entryInstruction.CEI_CustomsOffice = "BB";
					entryInstruction.CEI_Style = "G1";
					entryInstruction.CEI_BoxNumber = "123";
					var invoice = declaration.Invoices.AddNew();
					invoice.JZ_OH_Supplier = orgHeader.PK;
					Factory.Save();
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Buyer TW";
			orgHeader.OH_FullName = "Buyer TW";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			orgCusCode.OK_OH = orgHeader.PK;
			orgCusCode.OK_CustomsRegNo = "1245";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			var company = GlbCompany.CurrentCompany;
			var extPassword1 = Factory.New<Business.GlbExternalPassword>();
			extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword1.GP_GC = company.PK;
			extPassword1.GP_MailBoxID = "123-3";
			extPassword1.GP_UserID = "001";
			extPassword1.GP_GS = staff.PK;
			extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();
		}

		OrgHeader orgHeader;
	}
}
