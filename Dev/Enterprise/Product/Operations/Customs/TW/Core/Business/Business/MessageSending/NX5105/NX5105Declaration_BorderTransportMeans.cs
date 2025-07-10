using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	#region Declaration Border Transport Means

	class NX5105Declaration_BorderTransportMeans : ITransportMeans
	{
		readonly JobDeclaration declaration;

		public NX5105Declaration_BorderTransportMeans(CusEntryHeader entryHeader)
		{
			var header = Argument.NotNull(entryHeader, "entryHeader");
			declaration = Argument.NotNull(header.Declaration, "declaration");
		}

		ZDate ITransportMeans.ArrivalDateTime => declaration.JE_DateOfArrival.Date;

		ZString ITransportMeans.TypeCode => declaration.IsSea ? MessageConstants.BorderTransportMeansTypeCodes._1 : MessageConstants.BorderTransportMeansTypeCodes._4;

		IEnumerable<ZString> ITransportMeans.ItineraryRoutingCountryCodes => declaration.Itineraries.Cast<ItineraryData>().Select(x => x.CY_Code);

		#region Not Applicable 

		ZString ITransportMeans.ID => ZString.Empty;

		ZString ITransportMeans.JourneyID => ZString.Empty;

		ZString ITransportMeans.Registration => ZString.Empty;

		ZString ITransportMeans.Name => ZString.Empty;

		ZString ITransportMeans.CallSignID => ZString.Empty;

		#endregion
	}

	#endregion

	#region Consignment Border Transport Means

	class NX5105Consignment_BorderTransportMeans : ITransportMeans
	{
		readonly JobDeclaration declaration;

		public NX5105Consignment_BorderTransportMeans(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, "declaration");
		}

		ZString ITransportMeans.ID => SharedHelper.GetTransportID(declaration);

		ZString ITransportMeans.JourneyID => SharedHelper.GetVoyageFlightNo(declaration);

		ZString ITransportMeans.Registration => declaration.JE_VesselArrivalReg;

		#region Not Applicable

		ZDate ITransportMeans.ArrivalDateTime => ZDate.Empty;

		ZString ITransportMeans.TypeCode => ZString.Empty;

		IEnumerable<ZString> ITransportMeans.ItineraryRoutingCountryCodes => null;

		ZString ITransportMeans.Name => ZString.Empty;

		ZString ITransportMeans.CallSignID => ZString.Empty;

		#endregion
	}

	#endregion
}
