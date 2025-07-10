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
	public class CartageLegController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public CartageLegController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.CartageLeg; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.CartageLeg; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CommonCartageLeg); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			CommonCartageLeg leg = (CommonCartageLeg)businessEntity;
			CartageLegForm result = new CartageLegForm(leg);
			return result;
		}

		public override IZForm ShowNewForm()
		{
			throw new ControllerShowNewFormNotSupportedException("New Cartage Legs are not supported");
		}

		protected override void SetStrategyProvider(BusinessObjectFactory factory)
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(factory, new CartageBehaviorStrategyProvider());
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.LocalTransportLegs; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.LocalTransportLegsNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.LocalTransportLegsEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.LocalTransportLegsDelete; }
		}
	}
}
