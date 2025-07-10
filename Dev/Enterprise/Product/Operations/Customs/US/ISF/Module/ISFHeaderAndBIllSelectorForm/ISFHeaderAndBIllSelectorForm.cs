using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Module
{
	public partial class ISFHeaderAndBIllSelectorForm : ZChildForm
	{
		public ISFHeaderAndBIllSelectorForm(ISFHeaderRow row)
			: base(row)
		{
			this.Text = "Create Shipment From " + row.Header.HumanReadableName;
			row.MasterBillPKInfo.ValueChanged += new EventHandler(MasterBillPKInfo_ValueChanged);
			MasterBillPKInfo_ValueChanged(null, EventArgs.Empty);
			containerGrid.ContextMenu.MenuItems.Add("-");
			containerGrid.ContextMenu.MenuItems.Add(ResString.GetMultilingualString("ISFHeaderAndBIllSelectorForm|ContainerGrid|TickCopy", "Tick Copy"), ContainerGrid_TickCopy);
			containerGrid.ContextMenu.MenuItems.Add(ResString.GetMultilingualString("ISFHeaderAndBIllSelectorForm|ContainerGrid|UntickCopy", "Un-Tick Copy"), ContainerGrid_UnTickCopy);
			lineGrid.ContextMenu.MenuItems.Add("-");
			lineGrid.ContextMenu.MenuItems.Add(ResString.GetMultilingualString("ISFHeaderAndBIllSelectorForm|LineGrid|TickCopy", "Tick Copy"), LineGrid_TickCopy);
			lineGrid.ContextMenu.MenuItems.Add(ResString.GetMultilingualString("ISFHeaderAndBIllSelectorForm|LineGrid|UntickCopy", "Un-Tick Copy"), LineGrid_UnTickCopy);
		}

		public override string FormHeading
		{
			get { return Text; }
		}

		public new ISFHeaderRow BusinessEntity
		{
			get { return (ISFHeaderRow)base.BusinessEntity; }
		}

		#region Implementation

		void LineGrid_TickCopy(object sender, EventArgs e)
		{
			TickOrUnTickLine(true);
		}

		void LineGrid_UnTickCopy(object sender, EventArgs e)
		{
			TickOrUnTickLine(false);
		}

		void TickOrUnTickLine(bool shouldCopy)
		{
			if (lineGrid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowError(SelectAtLeastOneLine);
			}
			else
			{
				foreach (ISFLineRow row in lineGrid.SelectedElements)
				{
					row.ShouldCopy = shouldCopy;
				}
			}
		}

		internal const string SelectAtLeastOneLine = "Please select at least one line.";

		void ContainerGrid_TickCopy(object sender, EventArgs e)
		{
			TickOrUnTickContainer(true);
		}

		void ContainerGrid_UnTickCopy(object sender, EventArgs e)
		{
			TickOrUnTickContainer(false);
		}

		void TickOrUnTickContainer(bool shouldCopy)
		{
			if (containerGrid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowError(SelectAtLeastOneContainer);
			}
			else
			{
				foreach (ISFContainerRow row in containerGrid.SelectedElements)
				{
					row.ShouldCopy = shouldCopy;
				}
			}
		}

		internal const string SelectAtLeastOneContainer = "Please select at least one container.";

		void MasterBillPKInfo_ValueChanged(object sender, EventArgs e)
		{
			bool isOceanBillData = BusinessEntity.IsOceanBillData;
			using (billsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				List<string> columns = new List<string>(billsGrid.Columns.Count);
				foreach (var column in billsGrid.Columns)
				{
					string columnName = column.ColumnStyle.MappingName;
					if (columnName == ISFBillRow.Schema.HouseBillPK && isOceanBillData)
					{
						columnName = ISFBillRow.Schema.OceanBillPK;
					}
					else if (columnName == ISFBillRow.Schema.OceanBillPK && !isOceanBillData)
					{
						columnName = ISFBillRow.Schema.HouseBillPK;
					}
					columns.Add(columnName);
				}
				billsGrid.SetAvailability(isOceanBillData, ISFBillRow.Schema.OceanBillPK);
				billsGrid.SetColumnVisible(isOceanBillData, ISFBillRow.Schema.OceanBillPK);
				billsGrid.SetAvailability(!isOceanBillData, ISFBillRow.Schema.HouseBillPK);
				billsGrid.SetColumnVisible(!isOceanBillData, ISFBillRow.Schema.HouseBillPK);
				billsGrid.ReOrderColumns(columns);
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			this.InitializeComponent();
		}

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(businessEntityForValidation, "Shipment", "create", "created", includeIgnoreOption);
		}

		void CreateButton_Click(object sender, EventArgs e)
		{
			ValidateAll(ValidationType.Full);
			if (BusinessEntityForValidation.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				ShowConsolForm(BusinessEntity);
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void ShowConsolForm(ISFHeaderRow row)
		{
			ForwardingConsol consol = new JobConsolCreator(row).GetConsol(new BusinessObjectFactory());
			ZController consolController = ZControllerFactory.Create(ControllerIDs.JobConsol);
			consolController.SetFormsModalTo(this);
			ZForm consolForm = null;
			if (consol.IsInDatabase)
			{
				consolForm = (ZForm)consolController.ShowEditForm(consol);
			}
			else
			{
				consolForm = (ZForm)consolController.ShowFormForNewEntity(consol);
			}
			if (consolForm != null)
			{
				consolForm.Closed += new EventHandler(consolForm_Closed);
				consolForm.Text += string.Format(" With Data From {0}", row.Header.HumanReadableName);
				FormHeaderRowTable.Add(consolForm, new ISFShipmentCreateObj(row, consol.PK));
				CreateButton.Enabled = false;
				cancelAndClossButton.Enabled = false;
			}
		}

		class ISFShipmentCreateObj
		{
			public ISFShipmentCreateObj(ISFHeaderRow headerRow, ZGuid consolPK)
			{
				this.headerRow = headerRow;
				this.ConsolPK = consolPK;
				CurrentBillIndex = 0;
			}

			public readonly ZGuid ConsolPK;
			readonly ISFHeaderRow headerRow;

			public int TotalBills
			{
				get { return headerRow.Bills.Count; }
			}

			public ISFBillRow GetNextBill()
			{
				ISFBillRow row = null;
				int totalBills = TotalBills;
				if (totalBills > 0 && CurrentBillIndex < totalBills)
				{
					row = headerRow.Bills[CurrentBillIndex++];
				}
				return row;
			}

			public int CurrentBillIndex;
		}

		Dictionary<ZForm, ISFShipmentCreateObj> FormHeaderRowTable
		{
			get { return formHeaderRowTable ?? (formHeaderRowTable = new Dictionary<ZForm, ISFShipmentCreateObj>()); }
		}
		Dictionary<ZForm, ISFShipmentCreateObj> formHeaderRowTable;

		void consolForm_Closed(object sender, EventArgs e)
		{
			ZForm consolForm = (ZForm)sender;
			consolForm.Closed -= new EventHandler(consolForm_Closed);
			ISFShipmentCreateObj obj;
			if (FormHeaderRowTable.TryGetValue(consolForm, out obj))
			{
				FormHeaderRowTable.Remove(consolForm);
				ShowNextShipmentForm(obj, false);
			}
			else
			{
				CloseIfNothingElseToDo();
			}
		}

		void ShowNextShipmentForm(ISFShipmentCreateObj obj, bool previousJobWasCancelled)
		{
			ISFBillRow billRow = obj.GetNextBill();
			if (billRow != null)
			{
				if (!previousJobWasCancelled || Globals.Message.Show("Previous shipment create was cancelled.\r\nWould you like to continue with the rest?", "Previous Shipment Cancelled", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					JobShipmentCreator creator = new JobShipmentCreator(billRow);
					var factory = new BusinessObjectFactory();
					ChildEditableService.SetState(factory, ChildEditableServiceStates.Shipment);
					ForwardingShipment shipment = new JobShipmentCreator(billRow).GetShipment(factory, obj.ConsolPK);

					ZController shipmentController = ZControllerFactory.Create(ControllerIDs.JobShipment);
					shipmentController.SetFormsModalTo(this);
					ZForm shipmentForm = (ZForm)shipmentController.ShowFormForNewEntity(shipment);
					shipmentForm.Closed += new EventHandler(shipmentForm_Closed);
					ZStringBuilder builder = new ZStringBuilder(shipmentForm.Text);
					CusISFBill bill = billRow.HouseBill ?? billRow.OceanBill;
					if (bill != null && !bill.BB_BillNum.IsEmpty)
					{
						builder.Append("With Data From " + bill.BillTypeDescriptonAndNumber);
					}
					int totalBills = obj.TotalBills;
					if (totalBills > 1)
					{
						builder.Append(string.Format("({0} of {1} Shipments)", obj.CurrentBillIndex, totalBills));
					}

					shipmentForm.Text = builder.ToStringWithDelimiterBetweenAppends(" ");
					shipmentForm.Refresh();
					FormHeaderRowTable.Add(shipmentForm, obj);
				}
				else
				{
					Close();
				}
			}
			else
			{
				CloseIfNothingElseToDo();
			}
		}

		void CloseIfNothingElseToDo()
		{
			if (FormHeaderRowTable.Count == 0)
			{
				Close();
			}
		}

		void shipmentForm_Closed(object sender, EventArgs e)
		{
			ZForm shipmentForm = (ZForm)sender;
			shipmentForm.Closed -= new EventHandler(shipmentForm_Closed);

			ISFShipmentCreateObj obj;
			if (FormHeaderRowTable.TryGetValue(shipmentForm, out obj))
			{
				FormHeaderRowTable.Remove(shipmentForm);
				ShowNextShipmentForm(obj, !((ForwardingShipment)shipmentForm.BusinessEntity).IsInDatabase);
			}
			else
			{
				CloseIfNothingElseToDo();
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (formHeaderRowTable != null)
			{
				foreach (KeyValuePair<ZForm, ISFShipmentCreateObj> pair in formHeaderRowTable)
				{
					if (!pair.Key.IsDisposed)
					{
						pair.Key.Closed -= new EventHandler(shipmentForm_Closed);
						pair.Key.Closed -= new EventHandler(consolForm_Closed);
						pair.Key.Dispose();
					}
				}
			}
		}

		#endregion
	}
}
