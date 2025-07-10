using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	public class ContainerLoadListController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ControllerID ID
		{
			get { return ControllerIDs.ContainerLoadList; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CYContainerLoadList); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ContainerLoadListForm((CYContainerLoadList)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.ContainerLoadList;
			}
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		#endregion
	}
}
