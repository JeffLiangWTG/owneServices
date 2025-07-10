using System;
using System.ComponentModel;
using System.Windows.Forms;

using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public class ZOrganisationFindBoxColumnStyle : ZGuidFindBoxColumnStyle, ICustomKeyHandlingGridColumn
	{
		public ZOrganisationFindBoxColumnStyle(ZOrganisationFindBoxColumnStyleInfo columnInfo)
			: base(() => new ZOrganisationGridFindBox(), columnInfo)
		{
		}

		protected void ShowTempOrgPopup()
		{
			OrgFindBox.ShowTemporaryOrgPopup(null);
		}

		ITemporaryOrganisationFindBox OrgFindBox
		{
			get { return (ITemporaryOrganisationFindBox)EditControl; }
		}

		#region ZOrganisationGridFindBox

		public class ZOrganisationGridFindBox : ZGridGuidFindBox, ITemporaryOrganisationFindBox, IZOrganisationGridFindBox
		{
			public ZOrganisationGridFindBox()
			{
				temporaryOrgPopupProvider = new TemporaryOrganisationPopupProvider(this);
			}

			readonly TemporaryOrganisationPopupProvider temporaryOrgPopupProvider;

			protected override EmbeddedModulePopup CreateEmbeddedPopup(ZFilterModule module)
				=> temporaryOrgPopupProvider.CreateEmbeddedPopup(module)
					?? base.CreateEmbeddedPopup(module);

			#region ITemporaryOrganisationFindBox Members

			void ITemporaryOrganisationFindBox.ShowTemporaryOrgPopup(OrganisationEmdeddedModulePopup parentFindBoxPopup)
			{
				temporaryOrgPopupProvider.ShowTemporaryOrgPopup(parentFindBoxPopup);
				temporaryOrgPopupProvider.PopupClosed += new EventHandler(TemporaryOrgPopupProvider_PopupClosed);
			}

			IOrgHeaderCollection ITemporaryOrganisationFindBox.List
			{
				get { return (IOrgHeaderCollection)List; }
			}

			Form ITemporaryOrganisationFindBox.ParentForm
			{
				get { return FindForm(); }
			}

			#endregion

			#region CodeBox

			public override ZCodeBox GetCodeBox()
			{
				return new GridTemporaryOrganisationsCodeBox(this);
			}

			internal class GridTemporaryOrganisationsCodeBox : TemporaryOrganisationsCodeBox
			{
				public GridTemporaryOrganisationsCodeBox(ITemporaryOrganisationFindBox findBox) : base(findBox)
				{
				}
			}

			#endregion

			void TemporaryOrgPopupProvider_PopupClosed(object sender, EventArgs e)
			{
				if (ColumnStyle != null)
				{
					BeginInvoke(new MethodInvoker(((ZOrganisationFindBoxColumnStyle)ColumnStyle).HandlePopupClosed));
				}
				temporaryOrgPopupProvider.PopupClosed -= new EventHandler(TemporaryOrgPopupProvider_PopupClosed);
			}
		}

		#endregion

		protected void HandlePopupClosed()
		{
			IsEditing = true;
			HandleSelectFromPopup(this, EventArgs.Empty);
			parentDataGrid.BeginEdit(this, LastFocusedCell.RowNumber);
		}

		#region ICustomKeyHandlingGridColumn Members

		bool ICustomKeyHandlingGridColumn.ShouldProcessCmdKey(ref Message m, Keys keyData)
		{
			bool shouldNavigateLeft = (GridControl.SelectionStart == 0 && GridControl.SelectionLength == 0 && keyData == Keys.Left);
			bool shouldNavigateRight = (GridControl.SelectionStart == GridControl.Text.Length && GridControl.SelectionLength == 0 && keyData == Keys.Right);

			return (base.ShouldProcessCmdKey(ref m, keyData) || keyData == Keys.Tab || shouldNavigateLeft || shouldNavigateRight || keyData == Keys.Down || keyData == Keys.Up || keyData == Keys.Enter) && IsTempTyped;
		}

		bool ICustomKeyHandlingGridColumn.ProcessCmdKey(ref Message m, Keys keyData)
		{
			if (base.ProcessCmdKey(ref m, keyData))
			{
				return true;
			}
			else
			{
				ShowTempOrgPopup();
				return true;
			}
		}

		bool IsTempTyped
		{
			get { return (FindBox.Code == "TEMP" && OrgFindBox.List != null && OrgFindBox.List.AllowNewTemporaryOrganisations); }
		}

		#endregion
	}

	public class ZOrganisationFindBoxColumnStyleInfo : ZGuidFindBoxColumnStyleInfo, IZColumnStyleInfo
	{
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZOrganisationFindBoxColumnStyle); }
		}
	}
}
