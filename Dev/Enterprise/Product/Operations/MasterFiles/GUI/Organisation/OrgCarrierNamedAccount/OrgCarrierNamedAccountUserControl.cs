using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgCarrierNamedAccountUserControl : ZUserControl
	{
		public OrgCarrierNamedAccountUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			var organisation = (OrgHeader)dataSource;
			if (organisation != null)
			{
				OrgCarrierNamedAccountFilterStripCommonControl = new OrgCarrierNamedAccountFilterStripCommonControl(organisation.CarrierNamedAccounts, new UrsNamedAccountFilterBusinessObject());
				Controls.Add(OrgCarrierNamedAccountFilterStripCommonControl);
			}
		}

		OrgCarrierNamedAccountFilterStripCommonControl OrgCarrierNamedAccountFilterStripCommonControl;
	}
}

