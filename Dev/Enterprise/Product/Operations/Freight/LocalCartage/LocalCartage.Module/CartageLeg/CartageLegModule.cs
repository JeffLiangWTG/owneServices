using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.GUI;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Module
{
	public class CartageLegModule : ZFilterGridModule
	{
		public CartageLegModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CartageLeg; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.CartageLeg);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CartageLegFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ModuleCartageLegCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CartageLegFilterStripBusinessObject();
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public IZForm ShowCartageLegForm(CommonCartageLeg leg)
		{
			if (AllowEdit)
			{
				return ShowEditForm(leg);
			}
			else
			{
				return ShowViewForm(leg);
			}
		}

		protected override BusinessObjectFactory GetNewFactory()
		{
			BusinessObjectFactory moduleFactory = base.GetNewFactory();
			CommonCartageBehaviorStrategyProvider.SetProvider(moduleFactory, new CartageBehaviorStrategyProvider()); //DocumentSupport
			return moduleFactory;
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.LocalTransport; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.LocalTransportLegs; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.CartageLegWorkflowDescriptorCode; }
		}

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewAdditionalMenuItems());
			MenuItem localTransportJobItem = new ZMenuItem(ResString.GetMultilingualString("431b7bf3-dcfe-40f3-bd45-71b705d2a144", "Open Transport Job"), delegate
			{ LocalTransportJobItemMenuClick(); });
			result.Add(localTransportJobItem);
			return result.ToArray();
		}

		void LocalTransportJobItemMenuClick()
		{
			if (SelectedBusinessObjects.Length > 0)
			{
				CommonCartageLeg leg = (CommonCartageLeg)SelectedBusinessObjects[0];
				cartageForm = (CartageForm)ZControllerFactory.Create(ControllerIDs.Cartage).ShowEditForm(leg.Cartage);
				cartageForm?.SelectCartageLeg(leg);
			}
		}
		internal CartageForm cartageForm;
	}
}
