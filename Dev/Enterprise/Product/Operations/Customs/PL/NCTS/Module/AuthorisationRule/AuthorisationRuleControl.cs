using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Customs.PL.NCTS.Module;

public partial class AuthorisationRuleControl : ZFilterStripControl
{
	public AuthorisationRuleControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
		: base(gridCollection, filterBusinessObject)
	{
		InitializeComponent();
	}
}
