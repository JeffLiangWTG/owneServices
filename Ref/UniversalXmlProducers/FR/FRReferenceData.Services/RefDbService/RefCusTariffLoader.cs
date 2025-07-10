using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class RefCusTariffLoader
	{
		public RefCusTariffLoader(IRefDataLoader refDataLoader)
		{
			RefDataLoader = refDataLoader;
		}

		internal readonly IRefDataLoader RefDataLoader;

		protected IRefDataLoader GetRefDataLoader => RefDataLoader;

		public Task<IEnumerable<RefCusTariff>> GetEUDeclarableTariffsFromRefDB(DateTime thatDateTime)
		{
			return Task.Factory.StartNew(() => RefDataLoader.LoadData<RefCusTariff>(CreateFilterQuery(thatDateTime)).Result);
		}

		protected string CreateFilterQuery(DateTime fromThatDateTime)
		{
			var formattedStartDate = fromThatDateTime.ToString("yyyy-MM-ddThh:mm:ss", CultureInfo.InvariantCulture);
			var formattedEndDate = GetQueryEndDate().ToString("yyyy-MM-ddThh:mm:ss", CultureInfo.InvariantCulture);
			return $"RefCusTariffUpdate?$filter=ZZ1_ZZZ_NKDataGrouping eq 'EUN' and ZZ1_StartDate le {formattedStartDate}Z and ZZ1_EndDate ge {formattedEndDate}Z";
		}

		protected virtual DateTime GetQueryEndDate() => DateTime.Today;
	}
}
