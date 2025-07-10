using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class CartageLegsModuleButtonGrid : ZModuleButtonGrid
	{
		public CartageLegsModuleButtonGrid()
			: base()
		{
		}

		protected override BusinessObject GetObjectToEdit(BusinessObject selected)
		{
			CommonCartageLeg cartageLeg = (CommonCartageLeg)selected;
			return cartageLeg.Cartage;
		}

		protected override ZController GetNewControllerCore(BusinessObject selected)
		{
			return ZControllerFactory.Create(ControllerIDs.Cartage);
		}
	}
}
