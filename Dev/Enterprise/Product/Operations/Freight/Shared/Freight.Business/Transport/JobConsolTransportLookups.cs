//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobConsolTransportLookups
//
//    This class should be used for overriding collections in AutoJobConsolTransportLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public class JobConsolTransportLookups : AutoJobConsolTransportLookups
	{
		public JobConsolTransportLookups(AutoJobConsolTransport parent) : base(parent)
		{
		}

		#region Carriers

		public OrgHeaderCollection Carriers
		{
			get
			{
				switch (Transport.JW_TransportMode)
				{
					case Core.Constants.TransportModes.Air:
						return new AirShippingProviderCollection(Factory);

					case Core.Constants.TransportModes.Sea:
					case Core.Constants.TransportModes.InlandWaterwayTransport:
						return new SeaShippingProviderCollection(Factory);

					case Core.Constants.TransportModes.Rail:
						return new TransportScheduleRailShippingProviderCollection(Factory);

					case Core.Constants.TransportModes.Road:
						return new TransportScheduleLineHaulShippingProviderCollection(Factory);

					default:
						return new ShippingProviderCollection(Factory);
				}
			}
		}

		#endregion

		#region Creditor

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Creditor)]
		public OrgHeaderCollection CreditorList
		{
			get
			{
				if (creditorList == null)
				{
					creditorList = new CreditorCollection(Factory, ignoreCurrentCompanyInFilter: true);
				}
				return creditorList;
			}
		}

		OrganisationsFindBoxCollection creditorList;

		#endregion

		#region DistanceUnit_List

		public CodeDescriptionPairList DistanceUnit_List
		{
			get
			{
				return Factory.GetCachedValue("Transport.DistanceUnit_List", () =>
					{
						CodeDescriptionPairList result = new CodeDescriptionPairList();
						result.AddPair(Constants.Length.Kilometres, Res.GetString("129cd538-6ddb-46d1-9d0f-a010bf41ea60", "Kilometers"));
						result.AddPair(Constants.Length.Miles, Res.GetString("a29a5795-fac0-4e34-a79e-c409bb7deedb", "Miles"));
						return result;
					});
			}
		}

		#endregion

		#region FlightStatus_List

		public CodeDescriptionPairList FlightStatus_List => GetFlightStatusList(Factory);

		public static CodeDescriptionPairList GetFlightStatusList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Transport.FlightStatus_List", () =>
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(Constants.FlightScheduleStatus.Active, Res.GetString("091804C7-5CE8-4F24-82A2-18C4A7C4930B", "Active"));
				result.AddPair(Constants.FlightScheduleStatus.ArrivalDelay, Res.GetString("EF7F4907-3F63-4186-8487-676E920B6724", "Arrival Delay"));
				result.AddPair(Constants.FlightScheduleStatus.Arrived, Res.GetString("1F42F01A-5D18-410E-9168-CF1FEDCD5776", "Arrived"));
				result.AddPair(Constants.FlightScheduleStatus.Cancelled, Res.GetString("BEEC3B0E-3A66-40F3-832F-D8D9640DDBE6", "Canceled"));
				result.AddPair(Constants.FlightScheduleStatus.Departed, Res.GetString("66075360-B588-43EA-B6CA-CE7D12B7DAD5", "Departed"));
				result.AddPair(Constants.FlightScheduleStatus.DepartureDelay, Res.GetString("040D4A06-D55D-4B2A-B375-F775484008F8", "Departure Delay"));
				result.AddPair(Constants.FlightScheduleStatus.Diversion, Res.GetString("7DBF12B7-08BB-4E8E-8CCB-D3A39D83CC52", "Diversion"));
				result.AddPair(Constants.FlightScheduleStatus.Matched, Res.GetString("24BECB6F-A6DB-4B95-875C-1B32A74F2E4A", "Matched"));
				result.AddPair(Constants.FlightScheduleStatus.PartiallyMatched, Res.GetString("C6FAD063-8A5C-4241-B63F-AF0ACFF62D64", "Partially Matched"));
				result.AddPair(Constants.FlightScheduleStatus.PreArrival, Res.GetString("429F14C2-CA4D-4809-81F0-7354B45D4747", "Pre Arrival"));
				result.AddPair(Constants.FlightScheduleStatus.PreDeparture, Res.GetString("7AE46A9D-B8D3-46F2-AFCF-41A791E7FEA3", "Pre Departure"));
				result.AddPair(Constants.FlightScheduleStatus.Unmatched, Res.GetString("91A2283A-B4DB-4FDC-B521-C7B83BBAE50A", "Unmatched"));
				result.AddPair(Constants.FlightScheduleStatus.Unknown, Res.GetString("14E5BA04-810D-42B2-942D-832F25CFBBBF", "Unknown"));
				return result;
			});
		}

		#endregion

		#region Implementation

		AutoJobConsolTransport Transport
		{
			get { return (AutoJobConsolTransport)Parent; }
		}

		#endregion
	}
}
