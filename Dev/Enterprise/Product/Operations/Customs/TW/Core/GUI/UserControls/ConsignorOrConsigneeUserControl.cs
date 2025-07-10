using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class ConsignorOrConsigneeUserControl : ZUserControl
	{
		public ConsignorOrConsigneeUserControl()
		{
			InitializeComponent();
		}

		public override ResourceStringData CaptionResourceString
		{
			get { return OrganisationFindBox.CaptionResourceString; }
			set { OrganisationFindBox.CaptionResourceString = value; }
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindToOrganisations))]
		[ZArchitecture.GUI.Testing.ExcludeFromBindToAttributesTest]
		public string BindToOrganisations
		{
			get { return OrganisationFindBox.BindToList; }
			set { OrganisationFindBox.BindToList = value; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			var orgBindingMember = (DataSource == null) ? "" : new KBindingMemberInfo(dataMember, "OrganisationPK");
			OrganisationFindBox.SetDataBinding(DataSource, orgBindingMember);
			AddressDropEdit.BindToList = "Organisation+Address_List";
		}
	}
}
