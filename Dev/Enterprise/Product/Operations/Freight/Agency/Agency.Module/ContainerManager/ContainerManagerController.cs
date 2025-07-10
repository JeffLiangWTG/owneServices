using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Freight.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module
{
	public class ContainerManagerController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ControllerID ID
		{
			get { return ControllerIDs.AgencyContainerManager; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefContainerStock); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ContainerManagerForm((RefContainerStock)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AgencyContainerManager; }
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AgencyContainerManager; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AgencyContainerManagerNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AgencyContainerManagerEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AgencyContainerManagerDelete; }
		}

		#endregion
	}
}


