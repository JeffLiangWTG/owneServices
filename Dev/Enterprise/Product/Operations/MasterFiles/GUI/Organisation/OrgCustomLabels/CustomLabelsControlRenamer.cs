using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI
{
	/// <summary>
	/// Renames and shows/hides custom label controls.
	/// </summary>
	public class CustomLabelControlRenamer : IDisposable
	{
		public CustomLabelControlRenamer(ZLabel label, Control editControl, ICustomLabelsProvider customLabelsProvider, string propName, bool captionWithColon = true)
		{
			if (editControl == null)
			{
				throw new ArgumentNullException(nameof(editControl), "");
			}

			this.fLabel = label;
			this.fEditControl = editControl;
			this.fCustomLabelsProvider = customLabelsProvider;
			this.fPropName = propName;
			this.fCustomLabelsProvider.ConfigOrgProvider.ConfigOrgChanged += new EventHandler(OnConfigOrg_Changed);
			fCaptionWithColon = captionWithColon;
			OnConfigOrg_Changed(this, EventArgs.Empty);
		}

		#region Implementation

		protected ZLabel fLabel;
		protected Control fEditControl;
		protected string fPropName;
		protected ICustomLabelsProvider fCustomLabelsProvider;
		protected OrgHeader fPreviousConfigOrg;
		readonly bool fCaptionWithColon;

		protected void OnConfigOrg_Changed(object sender, EventArgs e)
		{
			if (fPreviousConfigOrg != null)
			{
				((IBindingList)fPreviousConfigOrg.CustomLabels).ListChanged -= new ListChangedEventHandler(OnCustomLabels_ListChanged);
			}
			OrgHeader configOrg = fCustomLabelsProvider.ConfigOrgProvider.ConfigOrg;
			if (configOrg != null)
			{
				((IBindingList)configOrg.CustomLabels).ListChanged += new ListChangedEventHandler(OnCustomLabels_ListChanged);
			}
			fPreviousConfigOrg = configOrg;
			SetupLabelAndControl();
		}

		protected void OnCustomLabels_ListChanged(object sender, ListChangedEventArgs e)
		{
			SetupLabelAndControl();
		}

		protected void SetupLabelAndControl()
		{
			if ((fLabel == null || (!fLabel.IsDisposed && !fLabel.Disposing)) &&
				!fEditControl.IsDisposed && !fEditControl.IsDisposed)
			{
				CustomLabelInfoList fieldList = fCustomLabelsProvider.GetCustomFields(fCustomLabelsProvider.ConfigOrgProvider.ConfigOrg, fCustomLabelsProvider.ConfigOrgProvider.Factory);
				CustomLabelInfoBase field = fieldList.GetFieldByPropertyName(fPropName)
					?? throw new Exception("Could not find custom label with property '" + fPropName + "'");

				if (fLabel != null)
				{
					SetCustomFieldLabelCaption(field.Caption);
					fLabel.Visible = field.IsEnabled;
					fEditControl.Visible = field.IsEnabled;
				}
				if (fEditControl is IDynamicToolTip)
				{
					((IDynamicToolTip)fEditControl).QueryToolTip += new QueryToolTipEventHandler(field.OnQueryToolTip);
				}
			}
		}

		/// <summary>
		/// Set the text on a ZLabel, sizing the text to fit the label's width.
		/// </summary>
		protected void SetCustomFieldLabelCaption(string caption)
		{
			using (var g = fLabel.CreateGraphics())
			{
				int textWidth;
				do
				{
					var captionText = string.Format("{0}{1}", caption, fCaptionWithColon ? ":" : string.Empty);
					textWidth = (int)g.MeasureString(captionText, fLabel.Font).Width;
					if (textWidth > fLabel.Width)
					{
						caption = caption.Substring(0, caption.Length - 1);
					}
				}
				while (textWidth > fLabel.Width);
			}

			fLabel.Text = string.Format("{0}{1}", caption, fCaptionWithColon ? ":" : string.Empty);
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			fCustomLabelsProvider.ConfigOrgProvider.ConfigOrgChanged -= new EventHandler(OnConfigOrg_Changed);
			if (fCustomLabelsProvider.ConfigOrgProvider.ConfigOrg != null)
			{
				((IBindingList)fCustomLabelsProvider.ConfigOrgProvider.ConfigOrg.CustomLabels).ListChanged -= new ListChangedEventHandler(OnCustomLabels_ListChanged);
			}
		}

		#endregion
	}
}
