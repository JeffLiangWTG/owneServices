using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.GUI
{
	public partial class ZOrganisationControlWithMiscellaneous : ZOrganisationControl
	{
		readonly KTableLayoutPanel tableLayoutPanel;
		public ZOrganisationControlWithMiscellaneous()
		{
			InitializeComponent();
			tableLayoutPanel = new KTableLayoutPanel();
			tableLayoutPanel.Parent = MiscellaneousTab;
			tableLayoutPanel.ColumnCount = 2;
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			tableLayoutPanel.Dock = DockStyle.Fill;
			tableLayoutPanel.Visible = true;
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);
			if (isNotFinalizing)
			{
				if (MiscellaneousTab != null)
				{
					MiscellaneousTab.Dispose();
				}
				if (AddressTab != null)
				{
					AddressTab.Dispose();
				}
				if (AdditionalAddressInfoTab != null)
				{
					AdditionalAddressInfoTab.Dispose();
				}
				if (ContactInfoTab != null)
				{
					ContactInfoTab.Dispose();
				}
			}
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindToMiscellaneousFields))]
		public string BindToMiscellaneousFields { get; set; }

		ZString miscellaneousFields = "UNINITIALISED";

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZString MiscellaneousFields
		{
			get { return miscellaneousFields; }
			set
			{
				if (value != miscellaneousFields)
				{
					miscellaneousFields = value;
					ChangeTabVisibility();
				}
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				SetAbsoluteDataSourceBinding(this, "MiscellaneousFields", BindingHelper.GetNestedControlDataMember(BindTo, dataMember, BindToMiscellaneousFields), false);
			}
		}

		void ChangeTabVisibility()
		{
			if (!MiscellaneousFields.IsEmpty)
			{
				if (DetailsTabControl.Controls.Contains(AddressTab))
				{
					DetailsTabControl.Controls.Remove(AddressTab);
				}
				if (DetailsTabControl.Controls.Contains(ContactInfoTab))
				{
					DetailsTabControl.Controls.Remove(ContactInfoTab);
				}
				if (!DetailsTabControl.Controls.Contains(MiscellaneousTab))
				{
					DetailsTabControl.Controls.Add(MiscellaneousTab);
				}
			}
			else
			{
				if (!DetailsTabControl.Controls.Contains(AddressTab))
				{
					DetailsTabControl.Controls.Add(AddressTab);
				}
				if (!DetailsTabControl.Controls.Contains(ContactInfoTab))
				{
					DetailsTabControl.Controls.Add(ContactInfoTab);
				}
				if (DetailsTabControl.Controls.Contains(MiscellaneousTab))
				{
					DetailsTabControl.Controls.Remove(MiscellaneousTab);
				}
			}

			if (!MiscellaneousFields.IsEmpty)
			{
				tableLayoutPanel.SuspendLayout();
				try
				{
					tableLayoutPanel.RowCount = 0;
					tableLayoutPanel.Controls.Clear();
					foreach (string fieldName in MiscellaneousFields.Split(','))
					{
						tableLayoutPanel.RowCount++;
						tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
						ZLabel label = new ZLabel();
						label.Name = fieldName + "Label";
						ZTextBox textBox = new ZTextBox();
						textBox.Name = fieldName + "TextBox";
						MissingResourceStringChecker.ExcludeFromTest(textBox);

						tableLayoutPanel.Controls.Add(label);
						tableLayoutPanel.Controls.Add(textBox);
						if (fieldName != "UNINITIALISED")
						{
							DataBoundControl.Get(textBox).SetDataBinding(DataSource, fieldName);
							ZString caption = textBox.Extensions.Get<IHintExtension>().ShortCaption;
							if (caption.IsEmpty)
							{
								BusinessObject bizo = BindingContext[DataSource, new KBindingMemberInfo(DataMember).BindingPath].GetCurrent() as BusinessObject;
								caption = bizo.ZPropertyInfoHash[fieldName].HumanReadableName;
							}
							label.Text = caption;
						}
					}
				}
				finally
				{
					tableLayoutPanel.ResumeLayout();
				}
			}
		}

		#region SelectFromPopupForm

		public void SelectFromPopupForm()
		{
			SelectFromPopupFormCore();
		}

		void SelectFromPopupFormCore()
		{
#if DEBUG
			if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
			{
				PopupShown = true;
			}
			else
#endif
			{
				this.OrganisationFindBox.SelectFromPopupForm();
			}
		}

#if DEBUG
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public bool PopupShown { get; set; }
#endif
		#endregion

		#region Metadata

		public static new PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZOrganisationControlWithMiscellaneous>()
				.Property<ZString>("MiscellaneousFields", "UNINITIALISED", false)
				.Result;
		}

		#endregion
	}
}
