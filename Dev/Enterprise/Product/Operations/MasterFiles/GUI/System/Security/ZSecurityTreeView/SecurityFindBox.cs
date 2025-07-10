using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	[DefaultBindingProperty("LookupKey")]
	[DefaultDataSourceBindingMember(null)]
#if DEBUG
	[ZArchitecture.GUI.Testing.SuppressTypeDescriptionProviderAttributeChecker]
#endif
	public partial class SecurityFindBox : ZUserControl, IExtendedControl, INotificationDataMembers
	{
		#region Custom Adornment Layout

		class SecurityFindBoxAdornmentLayout : AdornmentLayout<SecurityFindBox>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(SecurityFindBox source)
			{
				yield return source.CodeTextBox;
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(SecurityFindBox source)
			{
				yield return new IconLayout(source.SelectButton, IconAlignment.Center);
			}
		}

		#endregion

		CheckpointLookupKey lookupKey;

		#region Constructors

		static SecurityFindBox()
		{
			NotificationAdornmentFactory.RegisterCustomLayout(new SecurityFindBoxAdornmentLayout());
		}

		public SecurityFindBox()
		{
			InitializeComponent();
			SelectButton.Font = OFont.GetFontBold();
			Extensions = new DefaultControlExtensionCollection(this);
		}

		#endregion

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public CheckpointLookupKey LookupKey
		{
			get { return lookupKey; }
			set
			{
				lookupKey = value;
				CodeTextBox.Text = value.Code;
				HumanReadableNameTextBox.Text = Env.Security.FindCheckPoint(value)?.HumanReadableName;
			}
		}

		void CodeTextBox_Validated(object sender, EventArgs e)
		{
			CommitValue();
		}

		void CommitValue()
		{
			if (!lookupKey.CodeEquals(CodeTextBox.Text))
			{
				LookupKey = new CheckpointLookupKey(CodeTextBox.Text, Guid.Empty);
				HumanReadableNameTextBox.Text = Env.Security.FindCheckPoint(LookupKey)?.HumanReadableName;
				Binding binding = DataBindings[nameof(LookupKey)];
				if (binding != null)
				{
					binding.WriteValue();
				}
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			CodeTextBox.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX((int)(Width * 0.3));
		}

		void SelectButton_Click(object sender, EventArgs e)
		{
			SelectSecurity();
		}

		void SelectSecurity()
		{
			using (SecuritySelectionForm selectionForm = new SecuritySelectionForm())
			{
				selectionForm.NavigateToNode(lookupKey);
				if (ZFormModaliser.ShowDialogWithoutDispose(selectionForm) == DialogResult.OK)
				{
					LookupKey = selectionForm.LookupKey;
					CodeTextBox.Text = LookupKey.Code;
					HumanReadableNameTextBox.Text = Env.Security.FindCheckPoint(LookupKey)?.HumanReadableName;
				}
			}
		}

		#region INotificationDataMembers Members

		string[] INotificationDataMembers.NotificationDataMembers
		{
			get { return new string[] { DataMember.Replace(".LookupKey", ".LookupKeyValidationProxy") }; }
		}

		#endregion

		#region IDataBoundControl Members

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			DataBoundControl.GetDefaultImplementation(this).SetDataBinding(dataSource, dataMember);
		}

		public override Type DataSourceType
		{
			get { return typeof(CheckpointLookupKey); }
		}

		#endregion

		#region IExtendedControl Members

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		public IControlExtensionCollection Extensions { get; private set; }

		#endregion

		#region GetPropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<SecurityFindBox>()
			.Property("LookupKey", CheckpointLookupKey.Empty, false)
			.Result;
		}

		#endregion
	}
}
