using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CASurtax
{
	public class TariffDataProducer : ITariffDataProducer
	{
		public TariffDataProducer(IDbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			this.connection = connection;
		}

		readonly IDbConnection connection;

		public IEnumerable<RefCusTariff> GetTariff(IEnumerable<string> tariffCodes, int codeLength)
		{
			Argument.NotNull(tariffCodes, nameof(tariffCodes));
			var tariffCodesPredicate = string.Join(",", tariffCodes.Select(x => ($"'{x}'")));
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = $@"SELECT TARIFF, EFF_DATE, DESC1 FROM TPHS
WHERE LEFT(TARIFF, {codeLength}) IN ({tariffCodesPredicate})
AND LEN(TARIFF) = 13";
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var tariff = new RefCusTariff
						{
							ZZ1_TariffCode = reader[0].ToString().Replace(".", ""),
							ZZ1_StartDate = DateTime.Parse(reader[1].ToString()),
							ZZ1_Description = reader[2].ToString(),
							ZZ1_ZZI_NKTariffType = Constants.CanadaHarmonizedTariff
						};
						yield return tariff;
					}
				}
			}
		}
	}
}
