using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	abstract class CustomsEntriesAndEntryLinesUserControlAbstractTest<TCustomsEntryUserControl> : TestCaseWithFactory
		where TCustomsEntryUserControl : BaseCustomsEntryUserControl
	{
		public void TestEntryLineAdditionalDataUserControl()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				form.Show();

				var messageUserControl = (CustomsEntriesAndEntryLinesUserControl)form.CustomsBrokerageUserControl.MessageUserControl;
				messageUserControl.FindSingle<ZTabControl>("EntryLinesMessagesTabControl").SelectedTab = messageUserControl.FindSingle<ZTabPage>("EntryLinesTabPage");
				var entryLineAdditionalDataUserControl = messageUserControl.FindSingle<ZUserControl>("EntryLineAdditionalDataUserControl");
				AssertEquals(true, entryLineAdditionalDataUserControl.Visible);

				var extendedInfoGroupBox = messageUserControl.FindSingle<ZGroupBox>("ExtendedInfoGroupBox");
				AssertEquals(false, extendedInfoGroupBox.Visible);
			}
		}

		public virtual void TestEntryLineGridColumns()
		{
			using (var form = new ZForm(declaration))
			{
				var userControl = CreateCustomsEntriesAndEntryLinesUserControl();
				form.Controls.Add(userControl);
				form.Show();

				var entryLineGrid = userControl.FindSingle<ZGrid>("EntryLineGrid");
				CombineAssertions(() =>
				{
					AssertNotNull("User control should have FormattedTariff", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.FormattedTariff));
					AssertNotNull("User control should have CL_Calc_NetWeightInKG column", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Calc_NetWeightInKG));
					AssertNotNull("User control should have CL_Calc_CustomsSecondQuantity column", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Calc_CustomsSecondQuantity));
					AssertNotNull("User control should have CL_CustomsSecondUnitQty column", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_CustomsSecondUnitQty));
					AssertNotNull("User control should have CL_AssignedNumber column", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_AssignedNumber));
					AssertNotNull("User control should have CL_Model column", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Model));
					AssertNotNull("User control should have CL_BrandName column", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_BrandName));
					AssertNotNull("User control should have CL_Permit column", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Permit));
					AssertNotNull("User control should have CL_DGCode column", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_DGCode));
					AssertNotNull("User control should have CL_Specification column", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_Specification));
					AssertNotNull("User control should have CL_SupplierPartNumber column", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_SupplierPartNumber));
					AssertNotNull("User control should have CL_CustomsOwnerPartNo column", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_CustomsOwnerPartNo));
					AssertNotNull("User control should have CL_GoodsOrigin column", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_GoodsOrigin));
					AssertNotNull("User control should have CL_CertificateOfOriginNumber column", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_CertificateOfOriginNumber));
					AssertNotNull("User control should have CL_PreviousEntryNumber column", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_PreviousEntryNumber));
					AssertNotNull("User control should have CL_PreviousBondedEntryNumber column", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_PreviousBondedEntryNumber));
				});
			}
		}

		public void TestFeesGridColumns()
		{
			using (var form = new ZForm(declaration))
			{
				var userControl = CreateCustomsEntriesAndEntryLinesUserControl();
				form.Controls.Add(userControl);
				form.Show();

				var feesGrid = userControl.FindSingle<ZGrid>("FeesGrid");
				CombineAssertions(() =>
				{
					AssertNotNull("User control should have ChargeType column", feesGrid.GetColumnStyle(AutoDutyTaxFeeCharge.Schema.ChargeType));
					AssertNotNull("User control should have ChargeTypeDesc column", feesGrid.GetColumnStyle("ChargeTypeDesc"));
					AssertNotNull("User control should have ChargeAmount column", feesGrid.GetColumnStyle(AutoDutyTaxFeeCharge.Schema.ChargeAmount));
					AssertNotNull("User control should have MethodOfPayment column", feesGrid.GetColumnStyle(AutoDutyTaxFeeCharge.Schema.MethodOfPayment));
					AssertNotNull("User control should have MethodOfPaymentDesc column", feesGrid.GetColumnStyle("MethodOfPaymentDesc"));
				});

				AssertEquals("BindingMember of FeesGrid is CustomsEntryHeaders.DutyTaxFeeCharges", "CustomsEntryHeaders.DutyTaxFeeCharges", feesGrid.GetBindingMember());
			}
		}

		public void TestDefaultOrderAndVisibleColumnsForFeesGrid()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				form.Show();

				var messageUserControl = (CustomsEntriesAndEntryLinesUserControl)form.CustomsBrokerageUserControl.MessageUserControl;
				var grid = messageUserControl.FindSingle<ZGrid>("FeesGrid");
				int columnIndex = 0;
				grid.ResetColumns();
				AssertEquals(5, grid.Columns.Count);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, DutyTaxFeeCharge.Schema.ChargeTypeDesc, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, DutyTaxFeeCharge.Schema.MethodOfPaymentDesc, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, DutyTaxFeeCharge.Schema.ChargeAmount, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, DutyTaxFeeCharge.Schema.ChargeType, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, DutyTaxFeeCharge.Schema.MethodOfPayment, false);
			}
		}

		public void TestMessageTabPage()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				form.Show();

				var messageUserControl = (CustomsEntriesAndEntryLinesUserControl)form.CustomsBrokerageUserControl.MessageUserControl;
				var messagesTabPage = messageUserControl.FindSingleOrDefault<ZTabPage>("MessageTabPage");
				AssertNull("Can not be found, messageTabPage TabVisible is False", messagesTabPage);
			}
		}

		public void TestSplitContainerPanelMinSize()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MessagesTabPage;
				form.Show();

				CombineAssertions(() =>
				{
					var messageUserControl = (CustomsEntriesAndEntryLinesUserControl)form.CustomsBrokerageUserControl.MessageUserControl;
					var mainHorizontalSplitContainer = messageUserControl.FindSingleOrDefault<KSplitContainer>("MainHorizontalSplitContainer");
					AssertEquals(241, mainHorizontalSplitContainer.Panel1MinSize);

					var topVerticalSplitContainer = messageUserControl.FindSingleOrDefault<KSplitContainer>("TopVerticalSplitContainer");
					AssertEquals(852, topVerticalSplitContainer.Panel1MinSize);
					AssertEquals(280, topVerticalSplitContainer.Panel2MinSize);
				});
			}
		}

		protected abstract TCustomsEntryUserControl CreateCustomsEntriesAndEntryLinesUserControl();

		protected List<ZString> ExpectedEntryLineGridColumnNamesInSortOrderList
		{
			get
			{
				if (fExpectedEntryLineGridColumnNamesInSortOrderList == null)
				{
					fExpectedEntryLineGridColumnNamesInSortOrderList = new List<ZString>
					{
						CusEntryLine.Schema.CL_LineNumber,
						CusEntryLine.Schema.FormattedTariff,
						CusEntryLine.Schema.CL_Procedure,
						CusEntryLine.Schema.CL_CustomsValue,
						CusEntryLine.Schema.CL_Calc_GoodsDescription,
						CusEntryLine.Schema.CL_EntryLineUnitPrice,
						CusEntryLine.Schema.CL_EntryLineQty,
						CusEntryLine.Schema.CL_EntryLineUQ,
						CusEntryLine.Schema.CL_EntryLineUQDescription,
						CusEntryLine.Schema.CL_Calc_NetWeightInKG,
						CusEntryLine.Schema.CL_Calc_CustomsSecondQuantity,
						CusEntryLine.Schema.CL_CustomsSecondUnitQty
					};
				}
				return fExpectedEntryLineGridColumnNamesInSortOrderList;
			}
		}
		List<ZString> fExpectedEntryLineGridColumnNamesInSortOrderList;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		}
		protected JobDeclaration declaration;

		void AssertColumnDefaultOrderAndVisible(ZGrid grid, ZInt columnIndex, string expectedColumnName, bool expectedVisibility)
		{
			var column = grid.Columns[columnIndex];
			AssertEquals(expectedColumnName, column.ColumnName);
			AssertEquals(expectedVisibility, column.IsVisible);
		}
	}
}
