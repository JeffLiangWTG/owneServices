using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class InBondTransportModeCodes : Messaging.Business.TransportModeCodes
	{
		public readonly string[] ValidCodes = new string[5]
			{
				Codes.VesselNonContainer,
				Codes.VesselContainer,
				Codes.RailNonContainer,
				Codes.TruckNonContainer,
				Codes.AirNonContainer
			};

		public static InBondTransportModeCodes GetCachedValue(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("InBondTransportModeCodesListNew", () => { return GetNewACEList(); });
		}

		static InBondTransportModeCodes GetNewACEList()
		{
			var result = new InBondTransportModeCodes();
			result.AddPair(Codes.FixedTransportInstallations, Descriptions.FixedTransportInstallations);
			return result;
		}

		public InBondTransportModeCodes()
		{
			var list = new List<CodeDescriptionPair>(new TypedEnumerable<CodeDescriptionPair>(this));
			foreach (CodeDescriptionPair pair in list)
			{
				if (!ValidCodes.Contains(pair.Code))
				{
					Remove(pair);
				}
			}
		}

		public static bool IsSeaOrRail(string transportMode)
		{
			return transportMode == Codes.VesselNonContainer
				|| transportMode == Codes.VesselContainer
				|| transportMode == Codes.RailNonContainer;
		}

		public static InBondTransportModeCodes GetNonAMSCachedValue(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("InBondTransportModeCodesNonAMSList", () =>
			{
				var codeDescriptionPairList = new InBondTransportModeCodes();
				codeDescriptionPairList.Clear();
				codeDescriptionPairList.AddPair(Codes.TruckNonContainer, Descriptions.TruckNonContainer);
				codeDescriptionPairList.AddPair(Codes.AirNonContainer, Descriptions.AirNonContainer);
				codeDescriptionPairList.AddPair(Codes.FixedTransportInstallations, Descriptions.FixedTransportInstallations);
				return codeDescriptionPairList;
			});
		}
	}
}
