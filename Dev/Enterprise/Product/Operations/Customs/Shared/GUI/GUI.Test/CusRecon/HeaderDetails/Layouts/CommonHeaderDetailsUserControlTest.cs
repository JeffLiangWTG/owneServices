using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CommonHeaderDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingWithControls()
		{
			var declaration = SetupCusReconDeclaration();

			using (var form = new ZForm(declaration))
			using (var panel = new DynamicLayoutPanel())
			{
				var bag = CommonHeaderDetailsControlBag.Instance;
				var layout = new PanelLayout();
				RegisterControlBag(layout, bag);

				form.Controls.Add(panel);
				panel.UpdateLayout(new LayoutsForTesting(layout));
				form.Show();
				UserIdleWorker.Flush();

				System.Windows.Forms.Application.DoEvents();

				CombineAssertions("TestBindingWithControls", () =>
				{
					var entryTypeDropEdit = panel.FindSingle<ZDropEdit>(bag.EntryTypeDropEdit.ControlName);
					AssertEquals("EntryTypeDropEdit", declaration.CRD_ApplicationCode, entryTypeDropEdit.CodeBox.Text);

					var entryStatusTextBox = panel.FindSingle<ZTextBox>(bag.EntryStatusTextBox.ControlName);
					AssertEquals("EntryStatusTextBox", declaration.CRD_CustomsStatus, entryStatusTextBox.Text);

					var customsOfficeFindBox = panel.FindSingle<ZCodeFindBox>(bag.CustomsOfficeCodeFindBox.ControlName);
					AssertEquals("CustomsOfficeFindBox", declaration.CRD_CustomsOffice, customsOfficeFindBox.CodeBox.Text);

					var periodFromDateEdit = panel.FindSingle<ZDateEdit>(bag.PeriodFromDateEdit.ControlName);
					AssertEquals("PeriodFromDateEdit", declaration.CRD_PeriodFrom, periodFromDateEdit.DateTimeValue.Date);

					var periodToDateEdit = panel.FindSingle<ZDateEdit>(bag.PeriodToDateEdit.ControlName);
					AssertEquals("PeriodToDateEdit", declaration.CRD_PeriodTo, periodToDateEdit.DateTimeValue.Date);

					var authorizationNumberDropEdit = panel.FindSingle<ZGuidDropEdit>(bag.AuthorizationNumberGuidDropEdit.ControlName);
					AssertEquals("AuthorizationNumberGuidDropEdit", "12345678", authorizationNumberDropEdit.CodeBox.Text);

					var declarantAddressFindBox = panel.FindSingle<ZAddressControl>(bag.DeclarantAddressControl.ControlName);
					AssertEquals("DeclarantAddressFindBox", nameof(declaration.CRD_OA_DeclarantAddress), declarantAddressFindBox.BindTo);

					var representativeAddressFindBox = panel.FindSingle<ZAddressControl>(bag.RepresentativeAddressControl.ControlName);
					AssertEquals("RepresentativeAddressFindBox", nameof(declaration.CRD_OA_RepresentativeAddress), representativeAddressFindBox.BindTo);

					var buyingAgentAddressFindBox = panel.FindSingle<ZAddressControl>(bag.BuyingAgentAddressControl.ControlName);
					AssertEquals("BuyingAgentAddressFindBox", nameof(declaration.CRD_OA_BuyingAgentAddress), buyingAgentAddressFindBox.BindTo);

					var declarationTypeDropEdit = panel.FindSingle<ZDropEdit>(bag.DeclarationTypeDropEdit.ControlName);
					AssertEquals("DeclarationTypeDropEdit", nameof(declaration.CRD_DeclarationType), declarationTypeDropEdit.BindTo);

					var declarantTypeDropEdit = panel.FindSingle<ZDropEdit>(bag.DeclarantTypeDropEdit.ControlName);
					AssertEquals("DeclarantTypeDropEdit", nameof(declaration.CRD_DeclarantType), declarantTypeDropEdit.BindTo);

					var messageStatusTextBox = panel.FindSingle<ZTextBox>(bag.MessageStatusTextBox.ControlName);
					AssertEquals("MessageStatusTextBox", nameof(declaration.CRD_MessageStatus), messageStatusTextBox.BindTo);
				});
			}
		}

		void RegisterControlBag(PanelLayout layout, CommonHeaderDetailsControlBag bag)
		{
			layout.RegisterControlBag(bag);
			layout.Include(bag.EntryTypeDropEdit);
			layout.Include(bag.EntryStatusTextBox);
			layout.Include(bag.CustomsOfficeCodeFindBox);
			layout.Include(bag.PeriodFromDateEdit);
			layout.Include(bag.PeriodToDateEdit);
			layout.Include(bag.AuthorizationNumberGuidDropEdit);
			layout.Include(bag.DeclarantAddressControl);
			layout.Include(bag.RepresentativeAddressControl);
			layout.Include(bag.BuyingAgentAddressControl);
			layout.Include(bag.DeclarationTypeDropEdit);
			layout.Include(bag.DeclarantTypeDropEdit);
			layout.Include(bag.MessageStatusTextBox);
		}

		CusReconDeclaration SetupCusReconDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<CusReconDeclaration>();
			declaration.CRD_ApplicationCode = CusReconDeclarationApplicationCodeList.Codes.CLS;
			declaration.CRD_CustomsStatus = "PRS";
			declaration.CRD_CustomsOffice = "DE001";
			declaration.CRD_PeriodFrom = new CargoWise.Types.ZDate(2020, 10, 19);
			declaration.CRD_PeriodTo = new CargoWise.Types.ZDate(2020, 10, 31);

			var authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			authorisationHeader.CPH_Type = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			authorisationHeader.CPH_Number = "12345678";
			declaration.CRD_CPH_ReconClearanceAuthorisation = authorisationHeader.PK;

			return declaration;
		}
	}
}
