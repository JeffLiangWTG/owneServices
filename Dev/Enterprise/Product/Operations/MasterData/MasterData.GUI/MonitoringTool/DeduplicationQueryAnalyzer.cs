using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Data;
using Enterprise.Core.Forms;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.MasterData.GUI
{
	[TestExcludeZWinFormsAllHaveFormBashers]
	public partial class DeduplicationQueryAnalyzer : ZChildForm
	{
		public DeduplicationQueryAnalyzer(DeduplicationMonitoringMethodNameItem item)
		{
			InitializeComponent();

			Item = item;
		}

		delegate (DataTable data, string message) ExecuteQueryDelegate();

		public override string FormVerb => string.Empty;

		protected override bool AllowNew => false;

		#region Form Caption

		public override string FormCaption => Res.GetString("7c2efab8-060b-481f-ad94-15a298690734", "Debug Query Analyzer");

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			RunCommand();
		}

		DeduplicationMonitoringMethodNameItem item;
		DeduplicationMonitoringMethodNameItem Item
		{
			get => item;
			set
			{
				item = value;
				if (item != null)
				{
					QueryTextBox.Text = item.DBCommandText;
					QueryTextBox.SelectionStart = 0;
					QueryTextBox.SelectionLength = 0;
				}
			}
		}

		protected virtual void RunCommand()
		{
			this.Cursor = Cursors.WaitCursor;

			SetCommandStatus((NoResString)"Executing Query..."); // De-duplication Tool

			var method = new ExecuteQueryDelegate(ExecuteQuery);
			method.BeginInvoke(ExecuteQueryCallBack, method);
		}

		void ExecuteQueryCallBack(IAsyncResult asyncResult)
		{
			if (InvokeRequired)
			{
				Invoke(new AsyncCallback(ExecuteQueryCallBack), asyncResult);
			}
			else
			{
				SetCommandStatus((NoResString)"Processing Results..."); // De-duplication Tool

				var method = (ExecuteQueryDelegate)asyncResult.AsyncState;
				var result = method.EndInvoke(asyncResult);

				if (result.data != null)
				{
					BindDataTableToDataGrid(result.data);
				}

				SetCommandStatus(result.message);
				QueryTextBox.Focus();

				this.Cursor = Cursors.Default;
			}
		}

		void BindDataTableToDataGrid(DataTable dataTable)
		{
			GridViewResult.TableStyles.Clear();
			var dataGridTableStyle = new DataGridTableStyle();
			foreach (DataColumn column in dataTable.Columns)
			{
				var (columnStyleInfo, gridColumnStyle) = CreateGridColumnStyle(column);
				GridViewResult.Columns.Add(columnStyleInfo);
				dataGridTableStyle.GridColumnStyles.Add(gridColumnStyle);
			}

			GridViewResult.TableStyles.Add(dataGridTableStyle);
			GridViewResult.DataSource = dataTable;
			GridViewResult.RemoveAction = RemoveAction.NoRemovePossible;
		}

		(ZGridColumnInfo, DataGridColumnStyle) CreateGridColumnStyle(DataColumn column)
		{
			ZGridColumnInfo columnStyleInfo;
			DataGridColumnStyle gridColumnStyle;

			var dataType = column.DataType.UnderlyingSystemType.ToString();
			var widthDpiX = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			switch (dataType)
			{
				case "System.Boolean":
					columnStyleInfo = new ZCheckBoxColumnStyleInfo(column.ColumnName, widthDpiX);
					gridColumnStyle = new ZCheckBoxColumnStyle((ZCheckBoxColumnStyleInfo)columnStyleInfo);
					break;
				case "System.DateTime":
					columnStyleInfo = new ZDateEditColumnStyleInfo(column.ColumnName, widthDpiX);
					gridColumnStyle = new ZDateEditColumnStyle((ZDateEditColumnStyleInfo)columnStyleInfo);
					break;
				default:
					columnStyleInfo = new ZTextBoxColumnStyleInfo(column.ColumnName, widthDpiX);
					gridColumnStyle = new ZTextBoxColumnStyle((ZTextBoxColumnStyleInfo)columnStyleInfo);
					break;
			}

			columnStyleInfo.IsReadOnly = true;
			gridColumnStyle.ReadOnly = true;
			gridColumnStyle.HeaderText = column.ColumnName;
			return (columnStyleInfo, gridColumnStyle);
		}

		void SetCommandStatus(string status)
		{
			MessageStatusBarPanel.Text = status;
			MainStatusBar.Refresh();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected (DataTable Data, string Message) ExecuteQuery()
		{
			var message = string.Empty;
			var table = new DataTable();
			table.Locale = CultureInfo.CurrentCulture;

			try
			{
				using (Db.DisposableActionForDbConnection())
				using (var command = Db.Connection.Command(Item.DBCommandText))
				{
					foreach (var pair in Item.Tables)
					{
						command.AddTableValuedParameter("@hashedValueTable" + pair.Key, "dbo.TVP_int", pair.Value);
					}

					var dataAdapter = command.NewDataAdapter();
					message = dataAdapter.Fill(table) + (NoResString)" record(s) returned";
				}
			}
			catch (SqlException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}

			return (table, message);
		}

		void QueryTextBox_KeyUp(object sender, KeyEventArgs e)
		{
			switch (e.KeyData)
			{
				case (Keys.Control | Keys.A):
					{
						QueryTextBox.SelectionStart = 0;
						QueryTextBox.SelectionLength = QueryTextBox.Text.Length;
						break;
					}
			}
		}
	}
}
