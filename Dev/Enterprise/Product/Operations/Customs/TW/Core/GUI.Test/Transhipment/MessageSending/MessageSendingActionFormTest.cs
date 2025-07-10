using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CusInBondHeader = Enterprise.Customs.TW.Business.CusInBondHeader;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(TranshipmentMessageSendingForm))]
	public sealed class MessageSendingActionFormTest : TWMessageSendingFormTest
	{
		public override void TestMessageMenuItem()
		{
			using (var formForTest = new TranshipmentForm(Header))
			{
				formForTest.Show();
				var menuItem = formForTest.Menu.MenuItems.FindByText("Send Transhipment Application", true);
				AssertNotNull(menuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Factory.Save();
				menuItem.PerformClick();
				AssertEquals(typeof(TranshipmentMessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new TranshipmentMessageSendingForm(Wrapper);
		}

		protected override BaseMessageSendingObjectParent Wrapper
		{
			get
			{
				if (transhipmentWrapper == null)
				{
					transhipmentWrapper = new TranshipmentMessageSendingObjectParent(Header, MessageTypeList.Codes.TRA);
				}

				return transhipmentWrapper;
			}
		}

		TranshipmentMessageSendingObjectParent transhipmentWrapper;
		CusInBondHeader Header
		{
			get
			{
				if (header == null)
				{
					var staff = Factory.NewWithValidTestData<GlbStaff>();
					staff.GS_Code = "TT";
					var company = GlbCompany.CurrentCompany;
					var extPassword1 = Factory.New<Business.GlbExternalPassword>();
					extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
					extPassword1.GP_GC = company.PK;
					extPassword1.GP_MailBoxID = "123-3";
					extPassword1.GP_UserID = "001TEST";
					extPassword1.GP_GS = staff.PK;
					extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
					header = Factory.NewWithValidTestData<CusInBondHeader>();
					header.ReceiptOffice = "AA";
					header.UnladingOffice = "BB";
					header.TW_BoxNumber = "123";
					var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
					cusNum1.CE_ParentID = header.PK;
					cusNum1.CE_Category = "CUS";
					cusNum1.CE_EntryType = "TRS";
					cusNum1.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
					cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
					cusNum1.CE_EntryNum = "NO1";
					header.BH_GS_NKCusAgent = "TT";
					header.BH_CustomsProfile = "123-3";
					header.ArrivalBill.B0_ReferenceID = "1234";
					header.MovementBill.B0_ReferenceID = "1234";
					Factory.Save();
				}

				return header;
			}
		}

		CusInBondHeader header;
	}
}
