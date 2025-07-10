using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.US.GUI
{
	[DefaultDataSourceBindingMember(null)]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class ZUSOrganisationControl : ZUserControl, IBindTo
	{
		public ZUSOrganisationControl()
		{
			InitializeComponent();
		}

		#region OrgChanged

		public event EventHandler OrgChanged
		{
			add { defaultControl.OrgChanged += value; }
			remove { defaultControl.OrgChanged -= value; }
		}

		#endregion

		#region Set Control Size

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);
			SetControlSize();
		}

		protected void SetControlSize()
		{
			var controlWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(ControlWidth);
			var controlHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(ControlHeight);

			if (Width != controlWidth || Height != controlHeight)
			{
				SuspendLayout();
				try
				{
					ControlDpiScalingHelper.SetWidth(this, controlWidth, false);
					ControlDpiScalingHelper.SetHeight(this, controlHeight, false);
				}
				finally
				{
					ResumeLayout();
				}
			}
		}

		public const int ControlWidth = 261;
		public const int ControlHeight = 182;

		#endregion

		#region Synchronising the DefaultControl Captions

		[DefaultValue("")]
		public virtual string Caption
		{
			get { return defaultControl.Text; }
			set { defaultControl.Text = value; }
		}

		#endregion

		#region SelectFromPopupForm

		public void SelectFromPopupForm()
		{
			if (Organisation != null)
			{
				SelectFormPopupFormCore();
			}
		}

		void SelectFormPopupFormCore()
		{
#if DEBUG
			if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
			{
				PopupShown = true;
			}
			else
#endif
			{
				this.defaultControl.OrganisationFindBox.SelectFromPopupForm();
			}
		}

		protected USOrganisation Organisation
		{
			get { return defaultControl.USOrganisation; }
		}

#if DEBUG

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public bool PopupShown { get; set; }

#endif
		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZUSOrganisationControl>().Result;
		}

		#endregion

		#region IBindTo Members

		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public string BindTo
		{
			get { return BindingMemberHelper.BindingMember; }
			set { BindingMemberHelper.BindingMember = value; }
		}

		ControlBindingMemberHelper BindingMemberHelper
		{
			get { return bindingMemberHelper ?? (bindingMemberHelper = ControlBindingMemberHelper.Get(this)); }
		}
		ControlBindingMemberHelper bindingMemberHelper;

		protected Type BindingSourceDataSourceType
		{
			get { return KBindingSource.GetBindingSource(this) == null ? null : KBindingSource.GetBindingSource(this).DataSourceType; }
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindToOrganisations))]
		[ZArchitecture.GUI.Testing.ExcludeFromBindToAttributesTest]
		public string BindToOrganisations
		{
			get { return defaultControl.BindToOrganisations; }
			set { defaultControl.BindToOrganisations = value; }
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindToContacts
		{
			get { return defaultControl.ContactBindToList; }
			set { defaultControl.ContactBindToList = value; }
		}

		#endregion

		#region IDataBoundControl Members

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				defaultControl.BindTo = new KBindingMemberInfo(BindTo, USOrganisation.Schema.ZO_OH_Organisation);
				defaultControl.AddressBindTo = new KBindingMemberInfo(BindTo, USOrganisation.Schema.ZO_OA_Address).BindingMember;
				defaultControl.ContactBindTo = new KBindingMemberInfo(BindTo, USOrganisation.Schema.ZO_Contact).BindingMember;
				defaultControl.PhoneBindTo = new KBindingMemberInfo(BindTo, USOrganisation.Schema.ZO_Phone).BindingMember;
			}
			defaultControl.SetDataBinding(dataSource, new KBindingMemberInfo(dataMember, USOrganisation.Schema.ZO_OH_Organisation));

			this.dataSource = dataSource;
			this.dataMember = dataMember;
		}

		protected override object DataSourceCore
		{
			get { return dataSource; }
		}
		object dataSource;

		protected override string DataMemberCore
		{
			get { return dataMember; }
		}
		string dataMember;

		public override Type DataSourceType
		{
			get { return typeof(USOrganisation); }
		}

		#endregion
	}
}
