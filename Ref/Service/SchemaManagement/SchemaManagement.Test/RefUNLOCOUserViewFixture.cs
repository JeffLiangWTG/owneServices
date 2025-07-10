using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test;

[TestFixture]
[TransactionedTestCase]
class RefUNLOCOUserViewFixtures
{
	[Test]
	public void View()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
		{
			context.RefCountries.Add(new RefCountry()
			{
				RN_PK = Guid.NewGuid(),
				RN_Code = "AU",
				RN_IsActive = true,
				RN_Desc = "Australia",
				RN_EconomicGrouping = "",
				RN_CountryDialingCode = "61",
				RN_AddressFormattingRule = "DEF",
				RN_PostcodeValidationRule = "MBE",
				RN_StateProvinceValidationRule = "MBE",
				RN_RX_NKLocalCurrency = "",
				RN_RX_NKAirWaybillCurrency = "",
				RN_IsoAlpha3Code = "AUS",
				RN_IsoNumericUNM49Code = "036",
				RN_ValidationStatus = "NAV"
			});
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
			var geoLocationPoint = (Point)new WKTReader().Read("POINT(-148.083 -19.900)");
			geoLocationPoint.SRID = 4326;
			var unlocoUserView = new RefUNLOCOUserView()
			{
				RL_PK = Guid.NewGuid(),
				RL_Code = "AUSYD",
				RL_IsActive = true,
				RL_PortName = "Port",
				RL_NameWithDiacriticals = "",
				RL_IATA = "",
				RL_CoOrdinates = "",
				RL_HasAirport = true,
				RL_HasSeaport = true,
				RL_HasRail = true,
				RL_HasRoad = true,
				RL_HasPost = true,
				RL_HasCustomsLodge = true,
				RL_HasUnload = true,
				RL_HasStore = true,
				RL_HasTerminal = true,
				RL_HasDischarge = true,
				RL_HasOutport = true,
				RL_HasBorderCrossing = true,
				RL_R3 = Guid.Parse("DF3F8EB0-F079-4300-BDF9-CEAC4C06C31A"),
				RL_RN_NKCountryCode = "AU",
				RL_RW = Guid.Parse("70A768DF-A3AC-4D4C-9EF1-EAE62CAC0BD5"),
				RL_IATARegionCode = "",
				RL_GeoLocation = geoLocationPoint,
				RL_UserOverride = false,
				RL_IsPublished = true
			};
			context.RefUNLOCOUserViews.Add(unlocoUserView);
			context.SaveChanges();
			Assert.That(context.DataSetChangeHistories.Where(x => x.DCH_ParentPK == unlocoUserView.RL_PK).ToArray(), Has.Length.EqualTo(1));
		}
		using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
		{
			var unlocoUserView = context.RefUNLOCOUserViews.FirstOrDefault();
			Assert.AreEqual("AUSYD", unlocoUserView.RL_Code);
			Assert.AreEqual(true, unlocoUserView.RL_IsActive);
			Assert.AreEqual("Port", unlocoUserView.RL_PortName);
			Assert.AreEqual("AU", unlocoUserView.RL_RN_NKCountryCode);
			Assert.AreEqual(Guid.Parse("DF3F8EB0-F079-4300-BDF9-CEAC4C06C31A"), unlocoUserView.RL_R3);
			Assert.AreEqual(Guid.Parse("70A768DF-A3AC-4D4C-9EF1-EAE62CAC0BD5"), unlocoUserView.RL_RW);

			var versionControl = context.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == unlocoUserView.RL_PK);
			Assert.AreEqual(false, versionControl.RVC_Deleted);

			context.SaveChanges();
			unlocoUserView.RL_Code = "AUSYE";
			unlocoUserView.RL_IsActive = false;
			unlocoUserView.RL_PortName = "Modified Port";
			unlocoUserView.RL_IsPublished = false;
			context.SaveChanges();
		}

		using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
		{
			var unlocoUserView = context.RefUNLOCOUserViews.FirstOrDefault();
			Assert.AreEqual("AUSYE", unlocoUserView.RL_Code);
			Assert.AreEqual(false, unlocoUserView.RL_IsActive);
			Assert.AreEqual("Modified Port", unlocoUserView.RL_PortName);

			var versionControl = context.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == unlocoUserView.RL_PK);
			Assert.AreEqual(true, versionControl.RVC_Deleted);
		}

		using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
		{
			var unlocoUserView = context.RefUNLOCOUserViews.FirstOrDefault();
			context.RefUNLOCOUserViews.Remove(unlocoUserView);
			Assert.Throws<DbUpdateException>(() => context.SaveChanges(), "Should not enable Delete in top-level tables");
		}
	}
}
