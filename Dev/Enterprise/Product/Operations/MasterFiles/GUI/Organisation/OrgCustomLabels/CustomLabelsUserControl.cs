using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;

#if WINZOR
using Graphics = System.Drawing.BGraphics;
#endif

namespace Enterprise.MasterFiles.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class CustomLabelsUserControl /*SuppressCodeSmell Reason=compile time checking not possible as data source unknown*/ : ZUserControl
	{
		public CustomLabelsUserControl()
		{
			InitializeComponent();
			this.NoFieldsAvailableLabel.Text = Enterprise.MasterFiles.GUI.Res.GetString("CustomLabelsUserControl|72aec1e5-e2cd-4322-91fa-e5175642f5fd", "-- No Custom Fields Defined --");
		}

		public ICustomLabelsProvider CustomLabelsProvider
		{
			get { return fCustomLabelsProvider; }
			set
			{
				if (fCustomLabelsProvider != null)
				{
					if (fCustomLabelsProvider.ConfigOrgProvider != null)
					{
						fCustomLabelsProvider.ConfigOrgProvider.ConfigOrgChanged += new EventHandler(OnConfigOrg_Changed);
					}
				}

				fCustomLabelsProvider = value;

				if (fCustomLabelsProvider != null)
				{
					if (fCustomLabelsProvider.ConfigOrgProvider != null)
					{
						fCustomLabelsProvider.ConfigOrgProvider.ConfigOrgChanged += new EventHandler(OnConfigOrg_Changed);
					}
					OnConfigOrg_Changed(this, EventArgs.Empty);
				}
			}
		}

		string[] fPropertyNamesToExclude = Array.Empty<string>();
		public string[] PropertyNamesToExclude
		{
			get { return fPropertyNamesToExclude; }
			set
			{
				fPropertyNamesToExclude = value;
				OnConfigOrg_Changed(this, EventArgs.Empty);
			}
		}

		string fBindToMember = "";
		public string BindToMember
		{
			get { return fBindToMember; }
			set { fBindToMember = value; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			this.fUserControlIsBound = dataSource != null;
		}

		#region Implementation

		ICustomLabelsProvider fCustomLabelsProvider;
		bool fUserControlIsBound;
		OrgHeader fPreviousConfigOrg;

		#region Constants

		const int XMargin = 10;
		const int YMargin = 10;
		const int XGapBetweenControls = 30;
		const int YGapBetweenControls = 5;
		const int EditControlHeight = 20;

		const int GapBetweenLabelAndEditControl = 10;
		const int MaximumLabelWidth = 300;

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (fPreviousConfigOrg != null)
				{
					((IBindingList)fPreviousConfigOrg.CustomLabels).ListChanged -= new ListChangedEventHandler(OnConfigOrgCustomLabels_ListChanged);
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Populating the Custom Controls

		void PopulateCustomFields()
		{
			int x = XMargin;
			int index = 0;

			// populate each line of controls
			while (index < CustomFields.Count)
			{
				index = PopulateVerticalLineOfControls(index, x, out x);
			}

			if (fUserControlIsBound)
			{
				SetDataBinding(DataSource, "");
			}
		}

		int PopulateVerticalLineOfControls(int propertyIndex, int x, out int afterX)
		{
			var g = CreateGraphics();

			ArrayList currentVerticalEditControls = new ArrayList();
			int maxEditFieldRight = 0;
			int maxEditFieldLeft = 0;
			int y = ControlDpiScalingHelper.ScaleToCurrentDpiY(YMargin);

			int index;

			for (index = propertyIndex; index < CustomFields.Count; index++)
			{
				// create and show the edit and label controls
				CustomLabelInfo field = CustomFields[index] as CustomLabelInfo;

				if (field != null && field.IsEnabled &&
					!((IList)PropertyNamesToExclude).Contains(field.PropertyName))
				{
					ZLabel label = CreateAndShowNewLabel(field, g, x, y);
					Control editField = CreateAndShowNewEditField(field, label.Right, y);

					maxEditFieldRight = Math.Max(maxEditFieldRight, editField.Right);
					maxEditFieldLeft = Math.Max(maxEditFieldLeft, editField.Left);
					currentVerticalEditControls.Add(editField);

					// set the next X/Y positions for the next label/control pair
					bool isNextEditFieldBelowBottom = editField.Top + ControlDpiScalingHelper.ScaleToCurrentDpiY(EditControlHeight + EditControlHeight * 2 + YMargin) > Height;
					if (isNextEditFieldBelowBottom)
					{
						x = maxEditFieldRight + ControlDpiScalingHelper.ScaleToCurrentDpiX(XGapBetweenControls);
						break;
					}
					else
					{
						y = editField.Top + ControlDpiScalingHelper.ScaleToCurrentDpiY(EditControlHeight + YGapBetweenControls);
					}
				}
			}

			// make sure all edit controls are aligned the same
			foreach (Control editField in currentVerticalEditControls)
			{
				ControlDpiScalingHelper.SetLeft(editField, maxEditFieldLeft, false);
			}

			afterX = x;
			return index + 1;
		}

		ZLabel CreateAndShowNewLabel(CustomLabelInfoBase field, Graphics g, int x, int y)
		{
			ZLabel label = new ZLabel();
			label.Text = field.Caption + ": ";
			ControlDpiScalingHelper.SetWidth(ref label, (int)g.MeasureString(label.Text, label.Font, MaximumLabelWidth).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(GapBetweenLabelAndEditControl), false);
			ControlDpiScalingHelper.SetLeft(ref label, x, false);
			ControlDpiScalingHelper.SetTop(ref label, y, false);

			Controls.Add(label);
			label.Show();
			return label;
		}

		Control CreateAndShowNewEditField(CustomLabelInfoBase field, int x, int y)
		{
			CustomLabelControlFactory controlFactory = new CustomLabelControlFactory(CustomFields);
			Control editField = controlFactory.NewCustomFieldControl(field);
			this.ContainerControlToolTip.SetToolTip(editField, field.Hint);

			IHintExtension hintExtension = editField.GetExtension<IHintExtension>();
			if (hintExtension != null)
			{
				hintExtension.Caption = field.Caption;
				hintExtension.Description = field.Hint;
			}

			ControlDpiScalingHelper.SetLeft(ref editField, x, false);
			ControlDpiScalingHelper.SetTop(ref editField, y, false);
			editField.Name = field.PropertyName + "Bound" + editField.GetType().Name;
			editField.SetBindingMember(((string.IsNullOrEmpty(BindToMember)) ? "" : BindToMember + ".") + field.PropertyName);

			Controls.Add(editField);
			editField.Show();
			return editField;
		}

		#endregion

		#region Binding Individual Controls

		new IBusiness DataSource
		{
			get
			{
				IBusiness result;
				Form form = FindForm();

				if (form is ZForm)
				{
					result = ((ZForm)form).BusinessEntity;
				}
				else if (form == null)
				{
					throw new Exception(
						"Form not available, therefore DataSource not available. " +
						"Try setting the CustomFields property after you've added this control to a form.");
				}
				else
				{
					throw new Exception("Incorrect form type used (type " + form.GetType().FullName + ")");
				}

				return result;
			}
		}

		#endregion

		#region Handling Label Configuration Changes

		void OnConfigOrg_Changed(object sender, EventArgs e)
		{
			if (fPreviousConfigOrg != null)
			{
				((IBindingList)fPreviousConfigOrg.CustomLabels).ListChanged -= new ListChangedEventHandler(OnConfigOrgCustomLabels_ListChanged);
			}

			OrgHeader configOrg = (CustomLabelsProvider == null || CustomLabelsProvider.ConfigOrgProvider == null) ? null : CustomLabelsProvider.ConfigOrgProvider.ConfigOrg;

			if (configOrg != null)
			{
				((IBindingList)configOrg.CustomLabels).ListChanged += new ListChangedEventHandler(OnConfigOrgCustomLabels_ListChanged);
				CustomFields = CustomLabelsProvider.GetCustomFields(configOrg, configOrg.Factory);
			}

			fPreviousConfigOrg = configOrg;
		}

		void OnConfigOrgCustomLabels_ListChanged(object sender, ListChangedEventArgs e)
		{
			CustomFields = CustomLabelsProvider.GetCustomFields(CustomLabelsProvider.ConfigOrgProvider.ConfigOrg, CustomLabelsProvider.ConfigOrgProvider.Factory);
		}

		CustomLabelInfoList fCustomFields;
		CustomLabelInfoList CustomFields
		{
			get { return fCustomFields; }
			set
			{
				fCustomFields = value;
				fCustomFields.SortByPosition();

				if (!IsDisposed && !Disposing)
				{
					List<Control> controlsToRemove = new List<Control>();
					foreach (Control ctrl in Controls)
					{
						if (ctrl != null && ctrl != NoFieldsAvailableLabel)
						{
							controlsToRemove.Add(ctrl);
						}
					}

					foreach (Control control in controlsToRemove)
					{
						Controls.Remove(control);
						control.Dispose();
					}

					NoFieldsAvailableLabel.Visible = true;

					if (value != null)
					{
						NoFieldsAvailableLabel.Visible = !HasEnabledFields;
						PopulateCustomFields();
					}
				}
			}
		}

		bool HasEnabledFields
		{
			get
			{
				bool result = false;

				foreach (CustomLabelInfoBase field in CustomFields)
				{
					if (field.IsEnabled && !((IList)PropertyNamesToExclude).Contains(field.PropertyName))
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		#endregion

		#endregion
	}
}
