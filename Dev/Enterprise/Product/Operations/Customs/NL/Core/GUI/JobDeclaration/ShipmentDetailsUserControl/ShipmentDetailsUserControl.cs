using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public partial class ShipmentDetailsUserControl : ZUserControl
{
	public ShipmentDetailsUserControl()
	{
		InitializeComponent();

#if DEBUG
		TypeDescriptor.AddAttributes(ShipmentDetailsIncoTermsUserControl, new SuppressControlRequiresTextBasherAttribute());
		TypeDescriptor.AddAttributes(AgreedPlaceCodeFindBox, new SuppressControlRequiresTextBasherAttribute());
#endif
		ShipmentDetailsIncoTermsUserControl.AllowOutsideOfParent();
	}
}
