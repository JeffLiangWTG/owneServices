using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CusPermitForm))]
	sealed class CusPermitFormTest : ZFormBasherTest
	{
		public void TestValueFromType()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			var rule = permitHeader.CusPermitRules.AddNew();
			var exception = rule.CusPermitRuleExceptions.AddNew();

			using (var form = new CusPermitFormForTest(permitHeader))
			{
				form.Show();

				rule.CPR_RuleCode = BaseCusPermitRule.RuleCodes.Tariff;
				var style = form.PermitRuleGrid.GetColumnStyle("CPR_ValueFrom") as ZMultiControlColumnStyleInfo;
				AssertEquals(nameof(FieldType.Text), rule[style.FieldTypeColumnName]);
				style = form.PermitRuleExceptionGrid.GetColumnStyle("CPE_ValueFrom") as ZMultiControlColumnStyleInfo;
				AssertEquals(nameof(FieldType.Text), exception[style.FieldTypeColumnName]);

				rule.CPR_RuleCode = BaseCusPermitRule.RuleCodes.CountryOfOrigin;
				style = form.PermitRuleGrid.GetColumnStyle("CPR_ValueFrom") as ZMultiControlColumnStyleInfo;
				AssertEquals(nameof(FieldType.TextCodeFindBox), rule[style.FieldTypeColumnName]);
				style = form.PermitRuleExceptionGrid.GetColumnStyle("CPE_ValueFrom") as ZMultiControlColumnStyleInfo;
				AssertEquals(nameof(FieldType.TextCodeFindBox), exception[style.FieldTypeColumnName]);
			}
		}

		public void TestTransactionStatus()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();

			using (var form = new CusPermitFormForTest(permitHeader))
			{
				var transaction = permitHeader.CusPermitLineTransactions.AddNew();
				var columnText = "CPL_TransactionStatus";

				form.Show();

				var style = form.PermitTransactionsGrid.GetColumnStyle(columnText) as ZTextBoxColumnStyleInfo;
				AssertNotNull("Column shoud be there", style);
				int columnIndex = -1;
				for (int i = 0; i < form.PermitTransactionsGrid.Columns.Count; i++)
				{
					if (form.PermitTransactionsGrid.Columns[i].ColumnName == columnText)
					{
						columnIndex = i;
						break;
					}
				}
				AssertNotEquals(-1, columnIndex);
				var readOnlyAttributeData = transaction.GetType().GetMember(columnText)[0].CustomAttributes.First(c => c.AttributeType == typeof(System.ComponentModel.ReadOnlyAttribute));
				AssertNotNull("Read only attribute exists", readOnlyAttributeData);
				AssertEquals("Read only set to true", true, readOnlyAttributeData.ConstructorArguments[0].Value);
				AssertEquals(true, transaction.Lookups.PermitTransactionStatuses.ContainsOnly(new string[] { "PND", "" }));
			}
		}

		public void TestTransactionProcedure()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();

			using (var form = new CusPermitFormForTest(permitHeader))
			{
				var transaction = permitHeader.CusPermitLineTransactions.AddNew();
				transaction.CPL_Procedure = "A48";
				var columnText = "CPL_Procedure";

				form.Show();

				var style = form.PermitTransactionsGrid.GetColumnStyle(columnText) as ZTextBoxColumnStyleInfo;
				AssertNotNull("Column shoud be there", style);
				int columnIndex = -1;
				for (int i = 0; i < form.PermitTransactionsGrid.Columns.Count; i++)
				{
					if (form.PermitTransactionsGrid.Columns[i].ColumnName == columnText)
					{
						columnIndex = i;
						break;
					}
				}
				AssertNotEquals(-1, columnIndex);
			}
		}

		public void TestVisibility()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			using (var form = new CusPermitFormForTest(permitHeader))
			{
				form.Show();
				Assert(form.ValueBalanceZCalcEdit.Visible);
				Assert(form.QuantityBalanceZCalcEdit.Visible);
				permitHeader.TransactionCategory = PermitTransactionCategoryList.Codes.VAL;
				Assert(!form.ValueBalanceZCalcEdit.Visible);
				Assert(!form.QuantityBalanceZCalcEdit.Visible);
				permitHeader.TransactionCategory = "XXX";
				Assert(!form.ValueBalanceZCalcEdit.Visible);
				Assert(!form.QuantityBalanceZCalcEdit.Visible);
				permitHeader.TransactionCategory = "";
				Assert(!form.ValueBalanceZCalcEdit.Visible);
				Assert(!form.QuantityBalanceZCalcEdit.Visible);

				permitHeader.TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
				permitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.BTH;
				Assert(form.ValueBalanceZCalcEdit.Visible);
				Assert(form.QuantityBalanceZCalcEdit.Visible);
				permitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
				Assert(form.ValueBalanceZCalcEdit.Visible);
				Assert(!form.QuantityBalanceZCalcEdit.Visible);
				permitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.QTY;
				Assert(!form.ValueBalanceZCalcEdit.Visible);
				Assert(form.QuantityBalanceZCalcEdit.Visible);
				permitHeader.CPH_QtyValIndicator = "XXX";
				Assert(!form.ValueBalanceZCalcEdit.Visible);
				Assert(!form.QuantityBalanceZCalcEdit.Visible);
				permitHeader.CPH_QtyValIndicator = "";
				Assert(!form.ValueBalanceZCalcEdit.Visible);
				Assert(!form.QuantityBalanceZCalcEdit.Visible);
			}
		}

		public void TestVisibilityForZA()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			permitHeader.CPH_RN_NKCountryCode = "ZA";
			using (var form = new CusPermitFormForTest(permitHeader))
			{
				form.Show();
				Assert(form.PermitTransactionsGroupBox.Visible);
				Assert(form.PermitNumberZTextBox.Visible);
				Assert(form.UnitOfMeasureZTextBox.Visible);
				Assert(form.QtyValIndicatorZDropEdit.Visible);
				Assert(!form.PermitNumberZCodeFindBox.Visible);
				Assert(!form.PermitNumberCustomLabel.Visible);

				permitHeader.CPH_Type = "REB";
				Assert(!form.PermitTransactionsGroupBox.Visible);
				Assert(!form.PermitNumberZTextBox.Visible);
				Assert(!form.UnitOfMeasureZTextBox.Visible);
				Assert(!form.QtyValIndicatorZDropEdit.Visible);
				Assert(form.PermitNumberZCodeFindBox.Visible);
				Assert(form.PermitNumberCustomLabel.Visible);
			}
		}

		public void TestClosedPermit()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();

			// - create form for a permit
			using (var form = new CusPermitFormForTest(permitHeader))
			{
				form.Show();

				// - check that fields are editable
				Assert(!form.PermitNumberZTextBox.ReadOnly);
				Assert(!form.StartDateZDateEdit.ReadOnly);
				Assert(!form.PermitTransactionsGrid.ReadOnly);

				// - check that closed indicator is read-only
				Assert(form.PermitClosedZCheckBox.ReadOnly);

				// - check that closed indicator on form is not set
				Assert(!form.PermitClosedZCheckBox.Checked);

				// - close permit
				permitHeader.CPH_IsClosed = true;

				// - check that fields are read-only
				Assert(form.PermitNumberZTextBox.ReadOnly);
				Assert(form.StartDateZDateEdit.ReadOnly);
				Assert(form.PermitTransactionsGrid.ReadOnly);

				// - check that closed indicator is read-only
				Assert(form.PermitClosedZCheckBox.ReadOnly);

				// - check that closed indicator on form is set
				Assert(form.PermitClosedZCheckBox.Checked);
			}
		}

		public void TestPermitDetailsGroupBoxTabOrder()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			using (var form = new CusPermitFormForTest(permitHeader))
			{
				form.Show();

				var permitDetailsGroupbox = form.FindSingle<ZGroupBox>("PermitDetailsGroupBox");
				CombineAssertions(() =>
				{
					AssertEquals("PermitTypeZDropEdit TabIndex", 0, permitDetailsGroupbox.FindSingle<ZDropEdit>("PermitTypeZDropEdit").TabIndex);
					AssertEquals("PermitSubTypeZDropEdit TabIndex", 1, permitDetailsGroupbox.FindSingle<ZDropEdit>("PermitSubTypeZDropEdit").TabIndex);
					AssertEquals("PermitHolderZGuidFindBox TabIndex", 2, permitDetailsGroupbox.FindSingle<ZGuidFindBox>("PermitHolderZGuidFindBox").TabIndex);
					AssertEquals("PermitNumberZTextBox TabIndex", 4, permitDetailsGroupbox.FindSingle<ZTextBox>("PermitNumberZTextBox").TabIndex);
					AssertEquals("PermitClosedZCheckBox TabIndex", 5, permitDetailsGroupbox.FindSingle<ZCheckBox>("PermitClosedZCheckBox").TabIndex);
					AssertEquals("StartDateZDateEdit TabIndex", 6, permitDetailsGroupbox.FindSingle<ZDateEdit>("StartDateZDateEdit").TabIndex);
					AssertEquals("EndDateZDateEdit TabIndex", 7, permitDetailsGroupbox.FindSingle<ZDateEdit>("EndDateZDateEdit").TabIndex);
					AssertEquals("QtyValIndicatorZDropEdit TabIndex", 8, permitDetailsGroupbox.FindSingle<ZDropEdit>("QtyValIndicatorZDropEdit").TabIndex);
					AssertEquals("UnitOfMeasureZTextBox TabIndex", 9, permitDetailsGroupbox.FindSingle<ZTextBox>("UnitOfMeasureZTextBox").TabIndex);
				});
			}
		}

		public void TestPermitTypes()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			using (var form = new CusPermitFormForTest(permitHeader))
			{
				form.Show();
				var permitTypeZDropEdit = form.FindSingle<ZDropEdit>("PermitTypeZDropEdit");
				AssertEquals("The Permit Type list should be empty", permitTypeZDropEdit.List.Count, 0);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = new CusPermitForm(Factory.NewWithValidTestData<BaseCusPermitHeader>());
			form.ControllerID = ControllerIDs.Customs.Permits;
			return form;
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		sealed class CusPermitFormForTest : CusPermitForm
		{
			public CusPermitFormForTest(BaseCusPermitHeader permitHeader)
				: base(permitHeader)
			{
			}

			public new ZCalcEdit ValueBalanceZCalcEdit => base.ValueBalanceZCalcEdit;

			public new ZCalcEdit QuantityBalanceZCalcEdit => base.QuantityBalanceZCalcEdit;

			public new ZGrid PermitRuleGrid => base.PermitRuleGrid;

			public new ZGrid PermitTransactionsGrid => base.PermitTransactionsGrid;

			public new ZGrid PermitRuleExceptionGrid => base.PermitRuleExceptionGrid;

			public new ZGroupBox PermitRuleExceptionGroupBox => base.PermitRuleExceptionGroupBox;

			public new ZTextBox PermitNumberZTextBox => base.PermitNumberZTextBox;

			public new ZCodeFindBox PermitNumberZCodeFindBox => base.PermitNumberZCodeFindBox;

			public new ZLabel PermitNumberCustomLabel => base.PermitNumberCustomLabel;

			public new ZCheckBox PermitClosedZCheckBox => base.PermitClosedZCheckBox;

			public new ZDateEdit StartDateZDateEdit => base.StartDateZDateEdit;

			public new ZGroupBox PermitTransactionsGroupBox => base.PermitTransactionsGroupBox;

			public new ZTextBox UnitOfMeasureZTextBox => base.UnitOfMeasureZTextBox;

			public new ZDropEdit QtyValIndicatorZDropEdit => base.QtyValIndicatorZDropEdit;
		}
	}
}
