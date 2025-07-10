using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public class JobTradeLaneController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ControllerID ID
		{
			get { return ControllerIDs.TradeLane; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobTradeLane); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new JobTradeLaneForm((JobTradeLane)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.TradeLane;
			}
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.TradeLaneNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.TradeLaneView; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.TradeLaneDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.TradeLaneEdit; }
		}

		#endregion
	}
}
