using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class ContactPhoneDiallerUserControl : MultiPhoneDiallerUserControl, IBindingMemberForCompileTimeCheckProvider
	{
		public ContactPhoneDiallerUserControl()
		{
			InitializeComponent();
		}

		#region DataBinding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			UpdateCurrentOrgBinding();
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindToOrg))]
		[DefaultValue("")]
		public string BindToOrg
		{
			get { return bindToOrg; }
			set
			{
				bindToOrg = value;
				UpdateCurrentOrgBinding();
			}
		}
		string bindToOrg = "";

		void UpdateCurrentOrgBinding()
		{
			if (!this.IsDesignMode())
			{
				DataBindings.RemoveBinding(nameof(BindedOrgPk));
				if (DataSource != null && !string.IsNullOrEmpty(BindToOrg))
				{
					DataBindings.Add(new KBinding(nameof(BindedOrgPk), DataSource, BindToOrg));
				}
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZGuid BindedOrgPk
		{
			get { return bindedOrgPk; }
			set
			{
				bindedOrgPk = value;
				RefreshControls();
			}
		}
		ZGuid bindedOrgPk;

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ContactPhoneDiallerUserControl>()
				.Property("BindedOrgPk", ZGuid.Empty, false)
				.Result;
		}

		#endregion

		#endregion

		#region Contact

		protected OrgContact Contact
		{
			get
			{
				if (CurrentDataItem == null || !(CurrentDataItem is ZGuid))
				{
					return null;
				}

				var factory = DataSource is IBusiness ? ((IBusiness)DataSource).Factory : null;
				if (factory != null)
				{
					return factory.Load<OrgContact>((ZGuid)CurrentDataItem);
				}

				return null;
			}
		}

		#endregion

		#region CurrentOrg

		public OrgHeader CurrentOrg
		{
			get
			{
				if (BindedOrgPk.IsValid)
				{
					var factory = DataSource is IBusiness ? ((IBusiness)DataSource).Factory : null;
					if (factory != null)
					{
						return factory.Load<OrgHeader>(BindedOrgPk);
					}
				}

				var contact = Contact;
				if (contact != null)
				{
					return contact.Header;
				}

				return null;
			}
			set
			{
			}
		}

		#endregion

		#region DialInfo

		protected override PhoneDialInfo GetDefaultDialInfo()
		{
			var builder = GetNewDialInfoBuilder();
			return builder.GetDefaultDialInfo(Contact, CurrentOrg);
		}

		protected override IEnumerable<PhoneDialInfo> AlternativePhoneDialInfoList
		{
			get
			{
				var builder = GetNewDialInfoBuilder();
				return builder.GetAlternativePhoneDialInfos(Contact, CurrentOrg);
			}
		}

		protected virtual ContactPhoneDialInfoBuilder GetNewDialInfoBuilder()
		{
			return new ContactPhoneDialInfoBuilder();
		}

		#endregion

		#region Dial

		protected override void PopulateRelatedCommunication(OrgSalesCall relatedCommunication)
		{
			base.PopulateRelatedCommunication(relatedCommunication);

			var contact = Contact;
			if (contact != null)
			{
				relatedCommunication.OQ_OC = contact.PK;
			}
		}

		#endregion

		#region Messages

		protected override string NoDefaultPhoneContactDialsCaption
		{
			get { return ResString.GetMultilingualString("aa1886c4-3005-4eb9-a5ab-dda5778f922a", "Contact does not have a Work or Office phone contact details."); }
		}

		protected override string NoPhoneContactDetailsCaption
		{
			get { return ResString.GetMultilingualString("0d7e5fdc-c059-43c1-b1de-a38221c0556d", "No phone contact details available."); }
		}

		#endregion

		#region IBindingMemberForCompileTimeCheckProvider

		CompileTimeCheckBindingMemberCollection IBindingMemberForCompileTimeCheckProvider.GetBindingMembersForCompileTimeCheck(Type dataSourceType, string dataMember)
		{
			CompileTimeCheckBindingMemberCollection result = new CompileTimeCheckBindingMemberCollection();
			if (!string.IsNullOrEmpty(BindToOrg))
			{
				result.Add(new CompileTimeCheckBindingMember(dataSourceType, typeof(ZGuid), BindToOrg));
			}
			return result;
		}

		#endregion
	}
}
