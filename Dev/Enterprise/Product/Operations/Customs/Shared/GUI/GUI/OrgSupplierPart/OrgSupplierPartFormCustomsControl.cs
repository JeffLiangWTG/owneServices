using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class OrgSupplierPartFormCustomsControl : ZUserControl
	{
		public OrgSupplierPartFormCustomsControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			InitializeForm();
		}

		protected virtual void InitializeForm()
		{
			SetupPivotDescriptionColumn();
			SetupAuditColumns();
			SetupPivotGridContextMenu();
		}

		void SetupAuditColumns()
		{
			var zDateEditColumnStyleInfo3 = new ZDateEditColumnStyleInfo();
			var zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(PivotGrid)).BeginInit();
			PivotGrid.SuspendLayout();
			zDateEditColumnStyleInfo3.CaptionResourceString = Res.GetData("OrgSupplierPartFormCustomsControl|84C09998-5A10-46FD-A8A8-5967C6B65B88", "Last Audited Date");
			zDateEditColumnStyleInfo3.ColumnName = "CI_LastAuditedDate";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Res.GetData("OrgSupplierPartFormCustomsControl|02C20BAC-18B8-40C6-84C2-1E2CDBA3C368", "Last Audited User");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "LastAuditedUserFullName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			PivotGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			PivotGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			((System.ComponentModel.ISupportInitialize)(PivotGrid)).EndInit();
			PivotGrid.ResumeLayout(false);
			PivotGrid.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		void SetupPivotDescriptionColumn()
		{
			var zTextBoxColumnStyleCIDescInfo = new ZTextBoxColumnStyleInfo();
			SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(PivotGrid)).BeginInit();
			PivotGrid.SuspendLayout();
			zTextBoxColumnStyleCIDescInfo.ColumnName = "CI_Description";
			zTextBoxColumnStyleCIDescInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleCIDescInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			PivotGrid.ColumnStyles.Add(zTextBoxColumnStyleCIDescInfo);
			((System.ComponentModel.ISupportInitialize)(PivotGrid)).EndInit();
			PivotGrid.ResumeLayout(false);
			PivotGrid.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#region PivotGridContextMenu

		protected void SetupPivotGridContextMenu()
		{
			var gridContextMenu = PivotGrid?.ContextMenu;
			if (gridContextMenu != null)
			{
				var auditClassificationLineMenuItem = new ZMenuItem(AuditClassificationLinesMenuItemCaption, AuditClassificationLinesMenuItem_Click);
				gridContextMenu.MenuItems.Add(auditClassificationLineMenuItem);
			}
		}

		void AuditClassificationLinesMenuItem_Click(object sender, EventArgs e)
		{
			MakeSureThereIsASelectedLineWhenTheContextMenuIsRaisedAndActedUpon();
			ShowBatchAuditDialog(PivotGrid.SelectedElements);
		}

		void MakeSureThereIsASelectedLineWhenTheContextMenuIsRaisedAndActedUpon()
		{
			if (PivotGrid.SelectedRowCount == 0)
			{
				var currentRow = PivotGrid.CurrentRowIndex;
				PivotGrid.Select(currentRow);
			}
		}

		public static MultilingualString AuditClassificationLinesMenuItemCaption => ResString.GetMultilingualString("d71b9ccd-6246-4459-992c-61b4ab258dee", "Audit Classification Line(s)");

		#endregion

		public void ShowBatchAuditDialog(BusinessObject[] cusClassPartPivots)
		{
			var part = (OrgSupplierPart)(((ZForm)ParentForm).BusinessEntity);
			var skipDialogBoxForm = false;
			var defaultReference = ZString.Empty;

			if (part != null)
			{
				foreach (var classPartPivot in cusClassPartPivots.OfType<BaseCusClassPartPivot>())
				{
					Security.SecurityCheckpoint security = null;

					if (classPartPivot.IsHTB)
					{
						security = Env.Security.CustomsSupplierPartAuditImport.IsAllowed ? Env.Security.CustomsSupplierPartAuditExport : Env.Security.CustomsSupplierPartAuditImport;
					}
					else if (classPartPivot.IsImportClassification)
					{
						security = Env.Security.CustomsSupplierPartAuditImport;
					}
					else if (classPartPivot.IsExportClassification)
					{
						security = Env.Security.CustomsSupplierPartAuditExport;
					}

					if (security != null)
					{
						var caption = Res.GetString("3b4ac460-3e49-4436-8533-3323574001ff", "Audit Classification Lookup");
						var recordTypeName = Res.GetString("d6e6d981-cf6d-4dfe-b7c7-100005c0d2e3", "Classification Pivot");

						defaultReference = WriteToLogFormInvoker.ShowWriteToLogForm(part, classPartPivot, recordTypeName, security, caption, defaultReference, new BusinessObjectLoggerOptions(), Globals.Message.ShowInformation, x => skipDialogBoxForm = x, skipDialogBoxForm);
					}
				}
			}
		}
	}
}
