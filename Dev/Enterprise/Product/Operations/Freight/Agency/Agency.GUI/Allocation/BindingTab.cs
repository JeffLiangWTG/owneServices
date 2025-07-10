using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	internal class BindingTab : ZBindingTabPage
	{
		public readonly BusinessObject BizObj;

		public BindingTab(BusinessObject bizObj)
		{
			this.BizObj = bizObj;
		}

		protected override object DataSource
		{
			get { return BizObj; }
		}
	}
}
