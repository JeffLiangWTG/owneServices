using System;

using CargoWise.EntityFramework;

using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class BulkRateUpdatesController : ZSingletonController
	{
		public BulkRateUpdatesController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new BulkUpdateWizard(DefaultRateType);
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.Browse;
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get
			{
				SecurityCheckpoint securityCheckpoint = Env.Security.ClientRatesBulkUpdate;

				if (DefaultRateType == RatingConstants.RatingHeaderTypes.Costing)
				{
					securityCheckpoint = Env.Security.CostingRatesBulkUpdate;
				}
				else if (DefaultRateType == RatingConstants.RatingHeaderTypes.ClientRate)
				{
					securityCheckpoint = Env.Security.ClientRatesBulkUpdate;
				}
				else if (DefaultRateType == RatingConstants.RatingHeaderTypes.Tariff)
				{
					securityCheckpoint = Env.Security.CompanyTariffRatesBulkUpdate;
				}
				else if (DefaultRateType == RatingConstants.RatingHeaderTypes.IntercompanyTariff)
				{
					securityCheckpoint = Env.Security.IntercompanyTariffsBulkUpdate;
				}

				return securityCheckpoint;
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.BulkRateUpdates; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ClientRate); }
		}

		public string DefaultRateType
		{
			get { return fDefaultRateType; }
			set { fDefaultRateType = value; }
		}

		string fDefaultRateType;
	}
}


