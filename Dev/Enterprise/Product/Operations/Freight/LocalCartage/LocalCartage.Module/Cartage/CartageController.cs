using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Module
{
	public class CartageController : ZController, ICartageController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public CartageController()
		{
		}

		public override ControllerID ID => ControllerIDs.Cartage;
		public override ModuleIdentifier ModuleID => ModuleIDs.Cartage;

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CommonCartage); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			CommonCartage cartage = (CommonCartage)businessEntity;
			cartage.IsRoot = true;
			return new CartageForm(cartage);
		}

		protected override void SetStrategyProvider(BusinessObjectFactory factory)
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(factory, new CartageBehaviorStrategyProvider());
		}

		protected override SecurityCheckpoint CheckPointForView => Env.Security.TransportJob;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.TransportJobEdit;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.TransportJobNew;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.TransportJobDelete;

		bool ICartageController.IsFormOpen(CommonCartage cartage)
		{
			return IsFormShownFor(cartage);
		}

		protected virtual CartageCRMSecurityProvider CRMSecurityProvider => new CartageCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return CRMSecurityProvider.GetSecurityCheckpoint(bizObject as CommonCartage, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return CRMSecurityProvider.GetSecurityCheckpoint(bizObject as CommonCartage, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return CRMSecurityProvider.GetSecurityCheckpoint(bizObject as CommonCartage, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}
	}
}
