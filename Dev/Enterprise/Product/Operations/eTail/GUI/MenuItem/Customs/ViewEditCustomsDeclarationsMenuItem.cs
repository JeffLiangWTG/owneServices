using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.GUI
{
	public class ViewEditCustomsDeclarationsMenuItem : BaseHVLVMenuItem
	{
		public ViewEditCustomsDeclarationsMenuItem(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("bb9ebbe8-0e86-4eb0-8f19-510669eab8c4", "View/Edit Customs Declarations"), shipment)
		{
		}

		protected override Action MenuAction => () =>
		{
			var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration);
			module.FilterBusinessObject.SetExternalDefaults(GetFilterDefaults());
#if DEBUG
			//There's a unit test ( ViewEditFormalDeclarationMenuItem_ShouldShowJobDeclarationModule )
			//that tries to access the FSBO after the module is disposed when the using scope ends.
			//So we have to keep it undisposed for this one case.
			if (Globals.IsTest)
			{
				module.FilterBusinessObject.NullOutParentModuleOnDispose = false;
			}
#endif
			using (var modulePopup = new EmbeddedModulePopup(module))
			{
				ZFormModaliser.ShowDialogAndDispose(modulePopup);
			}
		};

		FilterBusinessObjectDefaults GetFilterDefaults()
		{
			var result = new FilterBusinessObjectDefaults();
			result.Add(new FilterBusinessObjectDefault(RelatedHVLShipment, "Property", shipment.PK));
			return result;
		}

		#region SuppressResourceStringsCheckRegion

		const string RelatedHVLShipment = "Related HVL Shipment";

		#endregion
	}
}
