using System;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;

namespace Enterprise.MasterFiles.OrganizationMergeServiceClientApp
{
	public partial class OrganizationMergeServiceClientForm : Form
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Column name")]
		public OrganizationMergeServiceClientForm(OrgMergerClientBusiness businessEntity = null)
		{
			this.businessEntity = businessEntity ?? new OrgMergerClientBusiness();

			InitializeComponent();

			oldOrgCodesTable = new DataTable("OldOrgCodes");
			oldOrgCodesTable.Columns.Add("Code", typeof(string));
			oldOrgCodesGrid.DataSource = oldOrgCodesTable;

			oldOrgCodesTable.RowChanged += new DataRowChangeEventHandler(oldOrgCodesTable_RowChanged);
		}

		void oldOrgCodesTable_RowChanged(object sender, DataRowChangeEventArgs e)
		{
			if (e.Action != DataRowAction.Delete && e.Action != DataRowAction.Nothing && e.Action != DataRowAction.Rollback)
			{
				if (!isChangingToUpperCase)
				{
					isChangingToUpperCase = true;
					if (e.Row != null && e.Row[0] != null)
					{ e.Row[0] = e.Row[0].ToString().ToUpper(); }
				}
				isChangingToUpperCase = false;
			}
		}
		bool isChangingToUpperCase;

		readonly DataTable oldOrgCodesTable;

		readonly OrgMergerClientBusiness businessEntity;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this is not part of enterprise")]
		void bMerge_Click(object sender, EventArgs e)
		{
			var loginName = nameTextbox.Text;
			var password = passwordTextbox.Text;

			try
			{
				string result;
				if (tabControl.SelectedTab == gridTabPage)
				{
					var newOrgCode = newOrgCodeTextBox.Text;
					var oldCodes = oldOrgCodesTable.Rows.Cast<DataRow>().Select(row => row[0].ToString());

					result = businessEntity.Merge(loginName, password, newOrgCode, oldCodes.ToArray());
				}
				else
				{
					result = businessEntity.Merge(loginName, password, xmlTextbox.Text);
				}

				MessageBox.Show(this, string.IsNullOrEmpty(result) ? "Organizations merged successfully." : result);
			}
			// When clicking 'merge' without a valid end point, the first click will throw EndpointNotFoundException and later clicks will throw CommunicationObjectFaultedException
			catch (EndpointNotFoundException)
			{
				ShowCouldNotConnectMessage();
			}
			catch (CommunicationObjectFaultedException)
			{
				ShowCouldNotConnectMessage();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this is not part of enterprise")]
		void ShowCouldNotConnectMessage()
		{
			const string errorMessage =
@"Endpoint not found. Please check the endpoint address in Enterprise.MasterFiles.OrganizationMergeServiceClientApp.exe.config and confirm that it is correct and the web service is running.

To check the address is correct you may paste the URL into a web browser and navigate it. A working, valid web service will return XML containing the webservice's definition.";

			MessageBox.Show(this, errorMessage, "Endpoint not found", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);
		}

		void OrganizationMergeServiceClientForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			Application.Exit();
		}
	}
}
