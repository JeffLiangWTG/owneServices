using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class EDIMenuStub : ZMenuItem, IEDIMenu
	{
		public EDIMenuStub()
		{
			this.Caption = ResString.GetMultilingualString("fe0bab75-8239-44b4-8b86-acf405c6684f", "&Brokerage");

			declarationDeactivatedMenuItem = new ZMenuItem(ResString.GetMultilingualString("0e2729db-a50e-409c-8202-28574ff25464", "Declaration deactivated, to reactivate go to Actions->Make Active"));
			MenuItems.Add(declarationDeactivatedMenuItem);
		}

		public BaseJobDeclaration Declaration
		{
			get { return fDeclaration; }
			set
			{
				fDeclaration = value;
			}
		}

		#region implementation

		readonly MenuItem declarationDeactivatedMenuItem;
		BaseJobDeclaration fDeclaration;

		#endregion
	}
}
