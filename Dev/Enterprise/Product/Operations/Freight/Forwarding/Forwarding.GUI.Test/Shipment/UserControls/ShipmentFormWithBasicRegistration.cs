using System.Linq;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ShipmentFormWithBasicRegistration : ZForm
	{
		public ShipmentFormWithBasicRegistration(ForwardingShipment businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
		}

		public ShipmentBasicRegistrationControl ShipmentBasicRegistrationControl
		{
			get;
			private set;
		}

		new void InitializeComponent()
		{
			BindingSource.DataSourceType = typeof(ForwardingShipment);

			var tabControl = new ZTabControl();
			tabControl.TabPages.Add(new ZTabPage { Name = "ShipmentDetailsTabPage" });
			Controls.Add(tabControl);

			ShipmentBasicRegistrationControl = new ShipmentBasicRegistrationControl();
			tabControl.AllTabPages.First().Controls.Add(ShipmentBasicRegistrationControl);

			BindingSource.SetBindingMember(ShipmentBasicRegistrationControl, ".");
		}
	}
}
