using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	public sealed class SailingsForTestClasses
	{
		public SailingsForTestClasses(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		#region Sea Voyages

		void SetUpVoyage()
		{
			RefUNLOCO r1 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUMEL") as RefUNLOCO;
			RefUNLOCO r2 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUSYD") as RefUNLOCO;
			RefUNLOCO r3 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "USLAX") as RefUNLOCO;

			var vessel = RefVessel.LookupVesselByName("APL IVORY", Factory).First();

			fVoyage = Factory.New<JobVoyage>();
			fVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			fVoyage.JV_VoyageFlight = "12";
			fVoyage.JV_RV_NKVessel = vessel.RV_FK;
			VoyageOrigin o1 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin o2 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin o3 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageDestination d1 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination d2 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination d3 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;

			o1.JA_RL_NKPortOfLoading = r1.RL_Code;
			o1.JA_E_DEP = ZDateTime.Today.AddDays(5);
			fVoyage.Origins.Add(o1);

			o2.JA_RL_NKPortOfLoading = r2.RL_Code;
			o2.JA_E_DEP = ZDateTime.Today.AddDays(8);
			fVoyage.Origins.Add(o2);

			o3.JA_RL_NKPortOfLoading = r3.RL_Code;
			o3.JA_E_DEP = ZDateTime.Today.AddDays(35);
			fVoyage.Origins.Add(o3);

			d1.JB_RL_NKPortOfDischarge = r2.RL_Code;
			d1.JB_E_ARV = ZDateTime.Today.AddDays(7);
			fVoyage.Destinations.Add(d1);

			d2.JB_RL_NKPortOfDischarge = r3.RL_Code;
			d2.JB_E_ARV = ZDateTime.Today.AddDays(37);
			fVoyage.Destinations.Add(d2);

			d3.JB_RL_NKPortOfDischarge = r1.RL_Code;
			d3.JB_E_ARV = ZDateTime.Today.AddDays(55);
			fVoyage.Destinations.Add(d3);

			fVoyage.GenerateSailings();
		}

		public JobVoyage Voyage
		{
			get
			{
				if (fVoyage == null)
				{
					SetUpVoyage();
				}
				return fVoyage;
			}
		}

		public JobSailing SydLaxSailing
		{
			get
			{
				if (fSydLaxSailing == null)
				{
					fSydLaxSailing = GetSailingFromPortPair(Voyage, "AUSYD", "USLAX");
				}
				return fSydLaxSailing;
			}
		}

		public JobSailing MelSydSailing
		{
			get
			{
				if (fMelSydSailing == null)
				{
					fMelSydSailing = GetSailingFromPortPair(Voyage, "AUMEL", "AUSYD");
				}
				return fMelSydSailing;
			}
		}

		public JobSailing LaxMelSailing
		{
			get
			{
				if (fLaxMelSailing == null)
				{
					fLaxMelSailing = GetSailingFromPortPair(Voyage, "USLAX", "AUMEL");
				}
				return fLaxMelSailing;
			}
		}

		public VoyageOrigin LaxVoyOrigin
		{
			get
			{
				if (fLaxVoyOrigin == null)
				{
					fLaxVoyOrigin = GetOriginFromLoad(Voyage, "USLAX");
				}
				return fLaxVoyOrigin;
			}
		}

		public VoyageOrigin MelVoyOrigin
		{
			get
			{
				if (fMelVoyOrigin == null)
				{
					fMelVoyOrigin = GetOriginFromLoad(Voyage, "AUMEL");
				}
				return fMelVoyOrigin;
			}
		}

		public VoyageDestination LaxVoyDestination
		{
			get
			{
				if (fLaxVoyDestination == null)
				{
					fLaxVoyDestination = GetDestinationFromDischarge(Voyage, "USLAX");
				}
				return fLaxVoyDestination;
			}
		}

		public VoyageDestination MelVoyDestination
		{
			get
			{
				if (fMelVoyDestination == null)
				{
					fMelVoyDestination = GetDestinationFromDischarge(Voyage, "AUMEL");
				}
				return fMelVoyDestination;
			}
		}

		#endregion

		#region Air Flights

		void SetUpFlight()
		{
			fFlight = Factory.New<JobVoyage>();
			fFlight.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			fFlight.JV_VoyageFlight = "QF12";
			VoyageOrigin o1 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin o2 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin o3 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageDestination d1 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination d2 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination d3 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			RefUNLOCO r1 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUMEL") as RefUNLOCO;
			RefUNLOCO r2 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUSYD") as RefUNLOCO;
			RefUNLOCO r3 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "USLAX") as RefUNLOCO;
			RefUNLOCO r4 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUSYD") as RefUNLOCO;
			RefUNLOCO r5 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "USLAX") as RefUNLOCO;
			RefUNLOCO r6 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUMEL") as RefUNLOCO;

			o1.JA_RL_NKPortOfLoading = r1.RL_Code;
			o1.JA_E_DEP = ZDateTime.Today.AddDays(5).AddHours(10);
			fFlight.Origins.Add(o1);

			o2.JA_RL_NKPortOfLoading = r2.RL_Code;
			o2.JA_E_DEP = ZDateTime.Today.AddDays(5).AddHours(13);
			fFlight.Origins.Add(o2);

			o3.JA_RL_NKPortOfLoading = r3.RL_Code;
			o3.JA_E_DEP = ZDateTime.Today.AddDays(6).AddHours(10);
			fFlight.Origins.Add(o3);

			d1.JB_RL_NKPortOfDischarge = r4.RL_Code;
			d1.JB_E_ARV = ZDateTime.Today.AddDays(5).AddHours(11);
			fFlight.Destinations.Add(d1);

			d2.JB_RL_NKPortOfDischarge = r5.RL_Code;
			d2.JB_E_ARV = ZDateTime.Today.AddDays(6).AddHours(6);
			fFlight.Destinations.Add(d2);

			d3.JB_RL_NKPortOfDischarge = r6.RL_Code;
			d3.JB_E_ARV = ZDateTime.Today.AddDays(6).AddHours(23);
			fFlight.Destinations.Add(d3);

			fFlight.GenerateSailings();
		}

		public JobVoyage Flight
		{
			get
			{
				if (fFlight == null)
				{
					SetUpFlight();
				}
				return fFlight;
			}
		}

		public JobSailing SydLaxFlightLeg
		{
			get
			{
				if (fSydLaxFlightLeg == null)
				{
					fSydLaxFlightLeg = GetSailingFromPortPair(Flight, "AUSYD", "USLAX");
				}
				return fSydLaxFlightLeg;
			}
		}

		public JobSailing MelSydFlightLeg
		{
			get
			{
				if (fMelSydFlightLeg == null)
				{
					fMelSydFlightLeg = GetSailingFromPortPair(Flight, "AUMEL", "AUSYD");
				}
				return fMelSydFlightLeg;
			}
		}

		public JobSailing LaxMelFlightLeg
		{
			get
			{
				if (fLaxMelFlightLeg == null)
				{
					fLaxMelFlightLeg = GetSailingFromPortPair(Flight, "USLAX", "AUMEL");
				}
				return fLaxMelFlightLeg;
			}
		}

		public VoyageOrigin MelFlightOrigin
		{
			get
			{
				if (fMelFlightOrigin == null)
				{
					fMelFlightOrigin = GetOriginFromLoad(Flight, "AUMEL");
				}
				return fMelFlightOrigin;
			}
		}

		public VoyageDestination LaxFlightDestination
		{
			get
			{
				if (fLaxFlightDestination == null)
				{
					fLaxFlightDestination = GetDestinationFromDischarge(Flight, "USLAX");
				}
				return fLaxFlightDestination;
			}
		}

		#endregion

		#region Rail Journey

		void SetUpJourney()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "GABRIELA";

			fJourney = Factory.New<JobVoyage>();
			fJourney.JV_AirSeaRoad = Core.Constants.TransportModes.Rail;
			fJourney.JV_VoyageFlight = "1";
			fJourney.JV_RV_NKVessel = vessel.RV_FK;
			VoyageOrigin o1 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin o2 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin o3 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageDestination d1 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination d2 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination d3 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			RefUNLOCO r1 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUMEL") as RefUNLOCO;
			RefUNLOCO r2 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUSYD") as RefUNLOCO;
			RefUNLOCO r3 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "USLAX") as RefUNLOCO;
			RefUNLOCO r4 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUSYD") as RefUNLOCO;
			RefUNLOCO r5 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "USLAX") as RefUNLOCO;
			RefUNLOCO r6 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUMEL") as RefUNLOCO;

			o1.JA_RL_NKPortOfLoading = r1.RL_Code;
			o1.JA_E_DEP = ZDateTime.Today.AddDays(5);
			fJourney.Origins.Add(o1);

			o2.JA_RL_NKPortOfLoading = r2.RL_Code;
			o2.JA_E_DEP = ZDateTime.Today.AddDays(8);
			fJourney.Origins.Add(o2);

			o3.JA_RL_NKPortOfLoading = r3.RL_Code;
			o3.JA_E_DEP = ZDateTime.Today.AddDays(35);
			fJourney.Origins.Add(o3);

			d1.JB_RL_NKPortOfDischarge = r4.RL_Code;
			d1.JB_E_ARV = ZDateTime.Today.AddDays(7);
			fJourney.Destinations.Add(d1);

			d2.JB_RL_NKPortOfDischarge = r5.RL_Code;
			d2.JB_E_ARV = ZDateTime.Today.AddDays(37);
			fJourney.Destinations.Add(d2);

			d3.JB_RL_NKPortOfDischarge = r6.RL_Code;
			d3.JB_E_ARV = ZDateTime.Today.AddDays(55);
			fJourney.Destinations.Add(d3);

			fJourney.GenerateSailings();
		}

		public JobVoyage Journey
		{
			get
			{
				if (fJourney == null)
				{
					SetUpJourney();
				}
				return fJourney;
			}
		}

		public JobSailing SydLaxSector
		{
			get
			{
				if (fSydLaxSector == null)
				{
					fSydLaxSector = GetSailingFromPortPair(Journey, "AUSYD", "USLAX");
				}
				return fSydLaxSector;
			}
		}

		public JobSailing MelSydSector
		{
			get
			{
				if (fMelSydSector == null)
				{
					fMelSydSector = GetSailingFromPortPair(Journey, "AUMEL", "AUSYD");
				}
				return fMelSydSector;
			}
		}

		public JobSailing LaxMelSector
		{
			get
			{
				if (fLaxMelSector == null)
				{
					fLaxMelSector = GetSailingFromPortPair(Journey, "USLAX", "AUMEL");
				}
				return fLaxMelSector;
			}
		}

		public VoyageOrigin MelJourOrigin
		{
			get
			{
				if (fMelJourOrigin == null)
				{
					fMelJourOrigin = GetOriginFromLoad(Journey, "AUMEL");
				}
				return fMelJourOrigin;
			}
		}

		public VoyageDestination LaxJourDestination
		{
			get
			{
				if (fLaxJourDestination == null)
				{
					fLaxJourDestination = GetDestinationFromDischarge(Journey, "USLAX");
				}
				return fLaxJourDestination;
			}
		}

		#endregion

		#region SetupFCLLCLDates

		public void SetupFCLLCLDates(JobSailing sailing)
		{
			var dep = sailing.JX_JA_E_DEP;
			var arv = sailing.JX_JB_E_ARV;

			sailing.JX_DepotReceivalCommences = dep.AddHours(-4);
			sailing.JX_DepotCutOff = dep.AddHours(-3);
			sailing.Origin.JA_ReceivalCommences = dep.AddHours(-2);
			sailing.Origin.JA_CutOff = dep.AddHours(-1);
			sailing.Destination.JB_AvailabilityDate = arv.AddHours(1);
			sailing.Destination.JB_StorageDate = arv.AddHours(2);
			sailing.JX_DepotAvailabilityDate = arv.AddHours(3);
			sailing.JX_DepotStorageDate = arv.AddHours(4);
		}

		#endregion

		#region Implementation

		JobVoyage fVoyage;
		VoyageOrigin fLaxVoyOrigin;
		VoyageOrigin fMelVoyOrigin;
		VoyageDestination fLaxVoyDestination;
		VoyageDestination fMelVoyDestination;
		JobSailing fSydLaxSailing;
		JobSailing fMelSydSailing;
		JobSailing fLaxMelSailing;

		JobVoyage fFlight;
		VoyageOrigin fMelFlightOrigin;
		VoyageDestination fLaxFlightDestination;
		JobSailing fSydLaxFlightLeg;
		JobSailing fMelSydFlightLeg;
		JobSailing fLaxMelFlightLeg;

		JobVoyage fJourney;
		VoyageOrigin fMelJourOrigin;
		VoyageDestination fLaxJourDestination;
		JobSailing fSydLaxSector;
		JobSailing fMelSydSector;
		JobSailing fLaxMelSector;

		readonly BusinessObjectFactory Factory;

		public JobSailing GetSailingFromPortPair(JobVoyage voyageToSearch, ZString load, ZString discharge)
		{
			JobSailing result = null;
			foreach (JobSailing sailing in voyageToSearch.Sailings)
			{
				if ((sailing.Origin.JA_RL_NKPortOfLoading == load)
					&& (sailing.Destination.JB_RL_NKPortOfDischarge == discharge))
				{
					result = sailing;
					result.JX_IsPublished = true;
					break;
				}
			}
			return result;
		}

		public VoyageOrigin GetOriginFromLoad(JobVoyage voyageToSearch, ZString load)
		{
			VoyageOrigin result = null;
			foreach (VoyageOrigin origin in voyageToSearch.Origins)
			{
				if (origin.JA_RL_NKPortOfLoading == load)
				{
					result = origin;
					break;
				}
			}
			return result;
		}

		public VoyageDestination GetDestinationFromDischarge(JobVoyage voyageToSearch, ZString discharge)
		{
			VoyageDestination result = null;
			foreach (VoyageDestination destination in voyageToSearch.Destinations)
			{
				if (destination.JB_RL_NKPortOfDischarge == discharge)
				{
					result = destination;
					break;
				}
			}
			return result;
		}

		#endregion
	}
}
