using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.Module
{
	public partial class RefCusTradeGroupFilterControl : ZFilterStripControl
	{
		public RefCusTradeGroupFilterControl(IBusinessObjectCollection gridCollection, RefCusTradeGroupFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
