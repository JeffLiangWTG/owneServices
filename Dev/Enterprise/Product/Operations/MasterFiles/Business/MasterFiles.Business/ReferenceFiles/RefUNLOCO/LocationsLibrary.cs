using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Macros;

namespace Enterprise.MasterFiles.Business
{
	public sealed class LocationsLibrary : MacroLibrary
	{
		public LocationsLibrary(BusinessObjectFactory factory)
		{
			this.helper = new UnlocoHelper(factory);
		}

		readonly UnlocoHelper helper;

		protected override IEnumerable<IHandler> MacroHandlers
		{
			get
			{
				yield return new Handler<Func<string, string>>(
					"CityCountry",
					cityCountryMacroDescription,
					unloco => helper.GetCityCountry(unloco));

				yield return new Handler<Func<string, bool>>(
					"IsUnloco",
					isUnlocoMacroDescription,
					unloco => helper.IsUnloco(unloco));
			}
		}

		#region SuppressResourceStringsCheckRegion

		const string cityCountryMacroDescription = "If the value is a valid UNLOCO, this macro returns the port name and the country code. If the country must have a state/province code entered then that code will also be included before the country code.";
		const string isUnlocoMacroDescription = "Determines if a given string is a UNLOCO.";

		#endregion
	}
}
