using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public class RouteValidation : ZValidation
	{
		readonly Route route;

		public RouteValidation(Route route)
			: base(route)
		{
			Argument.NotNull(route, "route");
			this.route = route;
		}

		public override void ValidateAll()
		{
			ValidateCarrierSCAC();
			ValidateDeparture();
			ValidateArrival();
		}

		public override Type AutoValidationType => typeof(RouteValidation);

		public void ValidateCarrierSCAC()
		{
			ValidateCalculatedProperty(route.CarrierSCACInfo);
		}

		public void ValidateDeparture()
		{
			ValidateCalculatedProperty(route.DepartureInfo);
		}

		public void ValidateArrival()
		{
			ValidateCalculatedProperty(route.ArrivalInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Needed for test TestValidateCarrierSCAC (RouteValidationTest)")]
		void CheckCarrierSCAC()
		{
			CarrierSCACHelper.CheckCarrierSCAC(route.CarrierSCACInfo, route.Factory);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Needed for test TestSetValues_With2LegsAndNoEtdForFirstLeg (RouteTest)")]
		void CheckDeparture()
		{
			if (route.DepartureInfo.Value.IsEmpty || route.Departure.IsEmpty || !route.Departure.IsValid)
			{
				route.DepartureInfo.AddError(Res.GetString("d6b00f40-e60b-4c87-9d0c-cdfa94ba8c16", "Departure date for first leg has not been set."));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Needed for test TestSetValues_With2LegsAndNoEtaForLastLeg (RouteTest)")]
		void CheckArrival()
		{
			if (route.ArrivalInfo.Value.IsEmpty || route.Arrival.IsEmpty || !route.Arrival.IsValid)
			{
				route.ArrivalInfo.AddError(Res.GetString("5b94f053-5725-4964-ab2c-b653b1c885f8", "Arrival date for last leg has not been set."));
			}
		}
	}
}
