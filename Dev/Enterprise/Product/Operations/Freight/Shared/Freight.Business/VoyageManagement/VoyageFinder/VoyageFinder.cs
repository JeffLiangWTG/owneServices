using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Freight.Business
{
	public sealed class VoyageFinder : AutoVoyageFinder
	{
		public VoyageFinder(IVoyageFinderParent voyageFinderParent)
			: base(new BusinessObjectFactory())
		{
			this.Parent = voyageFinderParent;
			this.CarrierPK = voyageFinderParent.CarrierPK;
		}

		#region Properties

		[List("Lookups.Vessels")]
		public override ZString JV_RV_NKVessel
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JV_RV_NKVessel; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.JV_RV_NKVessel = value; }
		}

		[List("Lookups.Carriers")]
		public override ZGuid CarrierPK
		{
			get { return base.CarrierPK; }
			set { base.CarrierPK = value; }
		}

		#endregion

		public JobVoyage GetMatchingVoyage()
		{
			JobVoyage voyage = null;

			switch (Parent.TransportMode)
			{
				case Constants.TransportModes.Sea:
				case Constants.TransportModes.Rail:
					voyage = new JobVoyage.Loader(Factory).Load(Parent.TransportMode, JV_RV_NKVessel, JV_VoyageFlight, CarrierPK);
					break;
			}

			return voyage;
		}

		public void SetupVoyage(JobVoyage voyage)
		{
			if (voyage != null)
			{
				voyage.JV_VoyageFlight = JV_VoyageFlight;
				voyage.JV_RV_NKVessel = JV_RV_NKVessel;
				voyage.JV_OH_Line = CarrierPK;
				AddOriginAndDestination(voyage);
			}
		}

		public JobSailing GetSailingForShipmentFromVoyage(JobVoyage voyage)
		{
			JobSailing requiredSailing = null;

			if (voyage != null)
			{
				if (voyage.Sailings != null)
				{
					foreach (JobSailing sailing in voyage.Sailings)
					{
						if ((sailing.Origin.JA_RL_NKPortOfLoading == Parent.LoadPort)
							&& (sailing.Destination.JB_RL_NKPortOfDischarge == Parent.DischargePort))
						{
							requiredSailing = sailing;
						}
					}
				}
			}
			return requiredSailing;
		}

		public void AddOriginAndDestination(JobVoyage voyage)
		{
			if (voyage != null)
			{
				if (voyage.Origins.GetOriginFromLoading(Parent.LoadPort) == null)
				{
					VoyageOrigin origin = voyage.Origins.AddNew();
					origin.JA_RL_NKPortOfLoading = Parent.LoadPort;
				}

				if (voyage.Destinations.GetDestinationFromDischarge(Parent.DischargePort) == null)
				{
					VoyageDestination destination = voyage.Destinations.AddNew();
					destination.JB_RL_NKPortOfDischarge = Parent.DischargePort;
				}

				voyage.GenerateSailings();
			}
		}

		public IVoyageFinderParent Parent { get; private set; }

		public VoyageFinderLookups Lookups
		{
			get { return lookups ?? (lookups = new VoyageFinderLookups(this)); }
		}
		VoyageFinderLookups lookups;

		#region Implementation

		protected override VoyageFinderValidation GetNewValidation()
		{
			switch (Parent.TransportMode)
			{
				case Constants.TransportModes.Sea:
					return new VoyageFinderSeaValidation(this);

				case Constants.TransportModes.Rail:
					return new VoyageFinderRailValidation(this);

				default:
					return new VoyageFinderValidation(this);
			}
		}

		#endregion
	}
}
