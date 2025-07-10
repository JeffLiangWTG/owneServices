using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Guarantees.Testing
{
	sealed class GuaranteeTransactionFilterControlTest : TestCaseWithFactory
	{
		public void TestControls_GuaranteeTransactionsGrid()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			var collection = new CusGuaranteeLineTransactionCollection(guaranteeHeader);
			var filterStripBusinessObject = new GuaranteeTransactionFilterStripBusinessObject();

			using (var form = new ZForm(guaranteeHeader))
			using (var control = new GuaranteeTransactionFilterControl(collection, filterStripBusinessObject))
			{
				form.Controls.Add(control);
				form.Show();

				System.Windows.Forms.Application.DoEvents();

				CombineAssertions(() =>
				{
					var grid = control.Grid;

					var columnNames = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
					AssertSequencesEqual("Columns", new[] {
						AutoCusPermitLineTransaction.Schema.CPL_SystemCreateUser,
						AutoCusPermitLineTransaction.Schema.CPL_SystemCreateTimeUtc,
						AutoCusPermitLineTransaction.Schema.CPL_SystemLastEditUser,
						AutoCusPermitLineTransaction.Schema.CPL_SystemLastEditTimeUtc,
						AutoCusPermitLineTransaction.Schema.CPL_TransactionDate,
						AutoCusPermitLineTransaction.Schema.CPL_TransactionType,
						"TransactionTypeDescription",
						AutoCusPermitLineTransaction.Schema.CPL_Reference,
						AutoCusPermitLineTransaction.Schema.CPL_TranValue,
						AutoCusPermitLineTransaction.Schema.CPL_Comment,
						AutoCusPermitLineTransaction.Schema.CPL_AppId,
						AutoCusPermitLineTransaction.Schema.CPL_TransactionStatus,
						"TransactionStatusDescription",
						AutoCusPermitLineTransaction.Schema.CPL_Procedure
					}, columnNames);

					var statusDescriptionColumn = grid.GetColumnStyle("TransactionStatusDescription");
					AssertEquals("TransactionStatusDescription IsVisible", false, statusDescriptionColumn.IsVisible);
				});
			}
		}
	}
}
