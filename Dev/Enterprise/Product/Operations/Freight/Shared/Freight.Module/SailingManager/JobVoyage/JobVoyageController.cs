using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public abstract class JobVoyageController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		protected abstract ZString TransportType { get; }

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobVoyage); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			JobVoyage voyage = (JobVoyage)businessEntity;
			if (!voyage.IsInDatabase)
			{
				using (voyage.SuspendSettingHasChanges())
				{
					voyage.JV_AirSeaRoad = TransportType;
				}
			}
			return new ZJobVoyageForm(voyage);
		}
	}
}
