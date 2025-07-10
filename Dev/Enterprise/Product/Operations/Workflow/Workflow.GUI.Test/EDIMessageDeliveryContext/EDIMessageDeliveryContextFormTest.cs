using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.GUI.Test.EDIMessageDeliveryContext
{
	[TestedType(typeof(EDIMessageDeliveryContextForm))]
	class EDIMessageDeliveryContextFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => GetNewForm();

		EDIMessageDeliveryContextForm GetNewForm() => new EDIMessageDeliveryContextForm(Factory.New<EDIMessageDeliveryContextSelector>());

		public void TestFormFields()
		{
			using (var form = GetNewForm())
			{
				var textBoxs = form.FindAll<ZTextBox>();
				AssertEquals(1, textBoxs.Count(t => t.BindTo == EDIMessageDeliveryContextSelectorSchema.Constants.ECS_Code));
				AssertEquals(1, textBoxs.Count(t => t.BindTo == EDIMessageDeliveryContextSelectorSchema.Constants.ECS_Description));

				var dropEdits = form.FindAll<ZDropEdit>();
				AssertEquals(1, dropEdits.Count(t => t.BindTo == EDIMessageDeliveryContextSelectorSchema.Constants.ECS_ProcessType));
			}
		}

		public void TestFormGrid()
		{
			using (var form = GetNewForm())
			{
				var grid = form.FindSingle<ZGrid>();
				var columns = grid.ColumnStyles.Cast<ZGridColumnInfo>();
				AssertEquals(3, columns.Count());

				var column = columns.First(c => c.ColumnName == EDIMessageDeliveryContextLineSchema.Constants.ECL_ContextType);
				AssertEquals(true, column is ZTextBoxColumnStyleInfo);
				AssertEquals(true, column.IsVisible);

				column = columns.First(c => c.ColumnName == EDIMessageDeliveryContextLineSchema.Constants.ECL_Description);
				AssertEquals(true, column is ZTextBoxColumnStyleInfo);
				AssertEquals(true, column.IsVisible);

				column = columns.First(c => c.ColumnName == EDIMessageDeliveryContextLineSchema.Constants.ECL_Value);
				var macroColumn = column as ZMacrosFindBoxColumnStyleInfo;
				AssertNotNull(macroColumn);
				AssertEquals(true, column.IsVisible);
				AssertEquals(true, macroColumn.IsUsedForExpressions);
			}
		}
	}
}
