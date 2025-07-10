using CargoWise.Types;

using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Rateable;

namespace Enterprise.Freight.CFS.Business
{
	public class GatePassShipmentRatingAdapter : CFSShipmentRatingAdapter<GatePassShipment>
	{
		protected internal GatePassShipmentRatingAdapter(GatePassShipment parent) : base(parent) { }

		public override AdapterType AdapterType => AdapterType.GatePass;

		public override IJobDatesProvider JobDatesProvider
		{
			get
			{
				return new GatePassShipmentJobDateProvider(Parent);
			}
		}

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var measures = (RateableMeasureSet)base.RateableMeasures;
				ZDateTime lclStorageCommences = Parent.DocsAndCartage.JP_LCLStorageCommences;
				ZDateTime lclAvailable = Parent.DocsAndCartage.JP_LCLAvailable;

				if (lclStorageCommences.IsEmpty || lclAvailable.IsEmpty)
				{
					foreach (CommonContainer container in Parent.Containers)
					{
						if (!container.JC_LCLStorageCommences.IsEmpty
							&& (lclStorageCommences.IsEmpty || container.JC_LCLStorageCommences > lclStorageCommences))
						{
							lclStorageCommences = container.JC_LCLStorageCommences;
						}

						if (!container.JC_LCLAvailable.IsEmpty
							&& (lclAvailable.IsEmpty || container.JC_LCLAvailable > lclAvailable))
						{
							lclAvailable = container.JC_LCLAvailable;
						}
					}
				}

				var timeIsSet = false;

				if (!lclStorageCommences.IsEmpty)
				{
					var deliveredDate = JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate);
					lclStorageCommences = lclStorageCommences.Date;
					if (lclStorageCommences <= deliveredDate)
					{
						var firstDay = !lclAvailable.IsEmpty && Env.Registry.Rating.IncludeCFSFreeStorageDaysInCalculation
							? lclAvailable
							: lclStorageCommences;

						measures.Time = new TimeInfo(firstDay, deliveredDate);
						timeIsSet = true;
					}
				}

				if (!timeIsSet)
				{
					measures.Time = TimeInfo.Empty;
				}

				return measures;
			}
		}
	}
}
