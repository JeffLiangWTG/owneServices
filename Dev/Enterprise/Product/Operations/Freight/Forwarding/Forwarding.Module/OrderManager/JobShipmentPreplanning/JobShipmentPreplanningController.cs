using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public class JobShipmentPreplanningController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.JobShipmentPreplanning; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.JobShipmentPreplanning; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobShipmentPreplanning); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new JobShipmentPreplanningForm((JobShipmentPreplanning)businessEntity);
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OrderPreAdviceView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrderPreAdviceNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrderPreAdviceEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrderPreAdviceDelete; }
		}

		#endregion
	}
}
