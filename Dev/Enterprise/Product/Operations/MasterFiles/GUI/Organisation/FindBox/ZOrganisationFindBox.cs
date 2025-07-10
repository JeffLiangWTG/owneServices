using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	[Serializable]
	public class ZOrganisationFindBoxException : OdysseyException
	{
		public ZOrganisationFindBoxException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected ZOrganisationFindBoxException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[SuppressFormDesignerAnalysis]
	[CompositeFieldControl]
	public class ZOrganisationFindBox : ZGuidFindBox, ITemporaryOrganisationFindBox
	{
		[ToolboxItem(false)]
		public new class Bare : ZOrganisationFindBox
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		public ZOrganisationFindBox()
		{
			this.ModuleID = ModuleIDs.Organisation;
			temporaryOrgPopupProvider = new TemporaryOrganisationPopupProvider(this);
		}

		public override ModuleIdentifier ModuleID
		{
			set
			{
				if (!(value is OrgModuleIdentifier))
				{
					throw new ZOrganisationFindBoxException("Module for a ZOrganisationFindBox must be Organisations");
				}

				base.ModuleID = value;
			}
		}

		public override IList List
		{
			get { return base.List; }
			set
			{
				if (value != null && !(value is IOrgHeaderCollection))
				{
					throw new ZOrganisationFindBoxException("BindToList of a ZOrganisationFindBox must implement IOrgHeaderCollection");
				}
				base.List = value;
			}
		}

		#region Implementation

		readonly TemporaryOrganisationPopupProvider temporaryOrgPopupProvider;

		protected override EmbeddedModulePopup CreateEmbeddedPopup(ZFilterModule module)
			=> temporaryOrgPopupProvider.CreateEmbeddedPopup(module)
				?? base.CreateEmbeddedPopup(module);

		protected override void SetControlSize(int charLength)
		{
			base.SetControlSize(charLength - 1);
		}

		protected override bool AutoCompleteText(bool explicitAutoComplete)
		{
			if (OrgCollection != null && OrgCollection.AllowNewTemporaryOrganisations && string.Equals(CodeBox.Text, "TEMP", StringComparison.OrdinalIgnoreCase))
			{
				temporaryOrgPopupProvider.ShowTemporaryOrgPopup(null);
				return true;
			}
			else
			{
				return base.AutoCompleteText(explicitAutoComplete);
			}
		}

		protected IOrgHeaderCollection OrgCollection
		{
			get { return (IOrgHeaderCollection)List; }
		}

		#region CodeBox

		public override ZCodeBox GetCodeBox()
		{
			return new FormTemporaryOrganisationsCodeBox(this);
		}

		class FormTemporaryOrganisationsCodeBox : TemporaryOrganisationsCodeBox
		{
			public FormTemporaryOrganisationsCodeBox(ITemporaryOrganisationFindBox findBox)
				: base(findBox)
			{
			}

			protected override void OnValidating(CancelEventArgs e)
			{
				if (Text.ToUpper() == "TEMP")
				{
					OrganisationFindBox.ShowTemporaryOrgPopup(null);
					e.Cancel = true;
				}
				else
				{
					base.OnValidating(e);
				}
			}
		}

		#endregion

		#endregion

		#region ITemporaryOrganisationFindBox Members

		void ITemporaryOrganisationFindBox.ShowTemporaryOrgPopup(OrganisationEmdeddedModulePopup parentFindBoxPopup)
		{
			temporaryOrgPopupProvider.ShowTemporaryOrgPopup(parentFindBoxPopup);
		}

		Form ITemporaryOrganisationFindBox.ParentForm
		{
			get { return FindForm(); }
		}

		IOrgHeaderCollection ITemporaryOrganisationFindBox.List
		{
			get { return (IOrgHeaderCollection)List; }
		}

		#endregion
	}
}
