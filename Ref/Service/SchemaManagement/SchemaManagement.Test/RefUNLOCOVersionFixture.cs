using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	[TestFixture]
	public class RefUNLOCOVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			var result = new List<object>();
			var unloco = new RefUNLOCO();
			unloco.SetDefaultValues();
			unloco.RL_PK = Guid.NewGuid();
			unloco.RL_RN_NKCountryCode = "AU";
			unloco.RL_Code = "AUSYD";
			unloco.RL_PortName = "Port";
			unloco.RL_R3 = Guid.Parse("DF3F8EB0-F079-4300-BDF9-CEAC4C06C31A");
			unloco.RL_RW = Guid.Parse("70A768DF-A3AC-4D4C-9EF1-EAE62CAC0BD5");
			unloco.RL_GeoLocation = new Point(-148.083, -19.900) { SRID = 4326 };
			result.Add(unloco);

			result.Add(new RefLocoMap()
			{
				RY_PK = Guid.NewGuid(),
				RY_RN_NKCountryCode = "AU",
				RY_RL_NKLocoPort = unloco.RL_Code,
				RY_LocalPortCode = "Port",
				RY_SystemUsage = "ABC"
			});

			result.Add(new RefUNLOCOUtcOffset()
			{
				RLO_PK = Guid.NewGuid(),
				RLO_OffsetMinutesFromUtc = 4,
				RLO_RL_NKCode = unloco.RL_Code,
				RLO_StartTimeUtc = DateTime.UtcNow.AddMonths(-3),
				RLO_EndTimeUtc = DateTime.UtcNow.AddMonths(3),
			});
			return result.ToArray();
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefUNLOCO unloco)
			{
				unloco.RL_PortName = "XX";
			}

			if (data is RefLocoMap locoMap)
			{
				locoMap.RY_LocalPortCode = "XX";
			}

			if (data is RefUNLOCOUtcOffset offset)
			{
				offset.RLO_OffsetMinutesFromUtc = 5;
			}
			return true;
		}

		protected override object[] PrepareFKReferencedData()
		{
			var unloco = new RefUNLOCO();
			unloco.SetDefaultValues();
			unloco.RL_PK = Guid.Parse("A4DB5AD3-490A-4BBA-BEE8-4645233D90D6");
			unloco.RL_RN_NKCountryCode = "AU";
			unloco.RL_Code = "BBBBB";
			unloco.RL_PortName = "Port";
			unloco.RL_R3 = Guid.Parse("DF3F8EB0-F079-4300-BDF9-CEAC4C06C31A");
			unloco.RL_RW = Guid.Parse("70A768DF-A3AC-4D4C-9EF1-EAE62CAC0BD5");
			unloco.RL_GeoLocation = new Point(-18.083, -19.900) { SRID = 4326 };
			return new object[] { unloco };
		}

		protected override bool UpdateFKColumnData(object data)
		{
			if (data is RefUNLOCOUtcOffset offset)
			{
				offset.RLO_RL_NKCode = "BBBBB";
				return true;
			}
			if (data is RefLocoMap locoMap)
			{
				locoMap.RY_RL_NKLocoPort = "BBBBB";
				return true;
			}
			return false;
		}

		protected override void PrepareDb(string dbName)
		{
			base.PrepareDb(dbName);
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var refCountry = new RefCountry();
				refCountry.SetDefaultValues();
				refCountry.RN_PK = Guid.NewGuid();
				refCountry.RN_Code = "AU";
				refCountry.RN_Desc = "Australia";
				refCountry.RN_CountryDialingCode = "61";
				refCountry.RN_AddressFormattingRule = "DEF";
				refCountry.RN_PostcodeValidationRule = "MBE";
				refCountry.RN_StateProvinceValidationRule = "MBE";
				refCountry.RN_IsoAlpha3Code = "AUS";
				refCountry.RN_IsoNumericUNM49Code = "036";
				refCountry.RN_ValidationStatus = "NAV";
				context.RefCountries.Add(refCountry);
				context.RefCountryStates.Add(new RefCountryStates
				{
					RW_PK = Guid.Parse("70A768DF-A3AC-4D4C-9EF1-EAE62CAC0BD5"),
					RW_Code = "AU",
					RW_Description = "Australia",
					RW_IsActive = true,
					RW_RegionName = "NSW",
					RW_RN_NKCountryCode = "AU"
				});
				context.RefTimeZoneSets.Add(new RefTimeZoneSet
				{
					R3_PK = Guid.Parse("DF3F8EB0-F079-4300-BDF9-CEAC4C06C31A"),
					R3_IsActive = true,
					R3_TimeZoneSetName = "AEST"
				});
				context.SaveChanges();
				context.Database.ExecuteSqlRaw($"DELETE {nameof(RefDbVersionControl)}");
			}
		}
	}
}
