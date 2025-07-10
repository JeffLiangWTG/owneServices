using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.Forwarding.GUI
{
	public sealed class VirtualFindBox : IFindBox
	{
		public void Initialize(IBusinessObjectCollection boCollection, EmbeddedModulePopup modulePopupForm)
		{
			businessObjectCollection = boCollection;
			modulePopup = modulePopupForm;
		}

		public string Code { get; set; }

		public string Description { get; set; }

		public IFindBoxListProvider ListProvider => (IFindBoxListProvider)businessObjectCollection;

		public IFindBoxPopup PopupForm => modulePopup;

		IBusinessObjectCollection businessObjectCollection;
		EmbeddedModulePopup modulePopup;
	}
}
