using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CALINF
{
	internal class CALINFCallHelper
	{
		public CALINFCallHelper(ZString locationCodeListId, CodeListResponsibleAgencyCodeList locationCodeListAgency, bool mustConvertToIataLocationCode = false, BusinessObjectFactory factory = null)
		{
			this.locationCodeListId = locationCodeListId;
			this.locationCodeListAgency = locationCodeListAgency;
			this.mustConvertToIataLocationCode = mustConvertToIataLocationCode;
			this.factory = factory;
		}

		public List<ICALINFCallInformation> PrepareDepartureData(VoyageOriginDependentCollection origins)
		{
			var callDetails = new List<ICALINFCallInformation>();
			if (origins != null)
			{
				foreach (VoyageOrigin origin in origins)
				{
					var location = GetFinalLocationCode(origin.JA_RL_NKPortOfLoading);
					var callDate = !origin.JA_A_DEP.IsEmpty ? origin.JA_A_DEP : origin.JA_E_DEP;
					var callInfo = new CALINFCall(location, locationCodeListId, locationCodeListAgency, callDate);
					callDetails.Add(callInfo);
				}
			}
			return callDetails;
		}

		public List<ICALINFCallInformation> PrepareDestinationData(VoyageDestinationDependentCollection destinations)
		{
			var callDetails = new List<ICALINFCallInformation>();
			if (destinations != null)
			{
				foreach (VoyageDestination destination in destinations)
				{
					var location = GetFinalLocationCode(destination.JB_RL_NKPortOfDischarge);
					var callDate = !destination.JB_A_ARV.IsEmpty ? destination.JB_A_ARV : destination.JB_E_ARV;
					var callInfo = new CALINFCall(location, locationCodeListId, locationCodeListAgency, callDate);
					callDetails.Add(callInfo);
				}
			}
			return callDetails;
		}

		ZString GetFinalLocationCode(ZString locationCode)
		{
			var location = locationCode;
			if (mustConvertToIataLocationCode)
			{
				location = MessageBuilderHelper.UnlocoToIata(factory, ZBool.True, location);
			}
			return location;
		}

		readonly ZString locationCodeListId;
		readonly CodeListResponsibleAgencyCodeList locationCodeListAgency;
		readonly bool mustConvertToIataLocationCode;
		readonly BusinessObjectFactory factory;
	}
}
