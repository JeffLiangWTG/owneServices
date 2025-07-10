using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class RefZonePivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUniqueMembers_RegistryAllowSameUNLOCOEnabled() => TestUniqueMembers(allowedSameUNLOCO: true);

		public void TestUniqueMembers_RegistryAllowSameUNLOCODisabled() => TestUniqueMembers(allowedSameUNLOCO: false);

		void TestUniqueMembers(bool allowedSameUNLOCO)
		{
			RatingDataRegistry.Instance.AllowSameUNLOCOInRatingInternationalZones.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, allowedSameUNLOCO);

			var allZone = AssertLocationIsAllowed(allowed: true, zoneCode: "ALLZ", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.All, zoneMode: Constants.RateMode.ALL, clearUNLOCOsAfterAsserting: false);

			AssertLocationIsAllowed(allowed: false, zoneCode: "ALL2", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.All, zoneMode: Constants.RateMode.ALL);
			AssertLocationIsAllowed(allowed: true, zoneCode: "RPTZ", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.Reporting, zoneMode: Constants.RateMode.ALL);
			AssertLocationIsAllowed(allowed: true, zoneCode: "RATZ", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.Rating, zoneMode: Constants.RateMode.ALL);
			AssertLocationIsAllowed(allowed: true, zoneCode: "IMPZ", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.RatingImport, zoneMode: Constants.RateMode.ALL);
			AssertLocationIsAllowed(allowed: true, zoneCode: "EXPZ", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.RatingExport, zoneMode: Constants.RateMode.ALL);
			AssertLocationIsAllowed(allowed: true, zoneCode: "TXL1", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.Tax, zoneMode: Constants.RateMode.ALL);

			allZone.FZ_IsActive = false;
			AssertLocationIsAllowed(allowed: true, zoneCode: "ALL3", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.All, zoneMode: Constants.RateMode.ALL);
			allZone.RunPreSaveValidation();
			AssertNoErrors("Shouldn't run validation if zone is inactive", ((RefZonePivot)allZone.UNLOCOs.GetRelationshipBusinessObject(allZone.UNLOCOs[0])).F2_ParentIDInfo);

			AssertLocationIsAllowed(allowed: true, zoneCode: "RATA", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.Rating, zoneMode: Constants.RateMode.AIR, clearUNLOCOsAfterAsserting: false);
			AssertLocationIsAllowed(allowed: allowedSameUNLOCO, zoneCode: "RATB", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.Rating, zoneMode: Constants.RateMode.AIR, expectedToHaveWarning: allowedSameUNLOCO);

			AssertLocationIsAllowed(allowed: true, zoneCode: "EXPA", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.RatingExport, zoneMode: Constants.RateMode.AIR, clearUNLOCOsAfterAsserting: false);
			AssertLocationIsAllowed(allowed: allowedSameUNLOCO, zoneCode: "EXPB", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.RatingExport, zoneMode: Constants.RateMode.AIR, expectedToHaveWarning: allowedSameUNLOCO);

			AssertLocationIsAllowed(allowed: true, zoneCode: "IMPA", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.RatingImport, zoneMode: Constants.RateMode.AIR, clearUNLOCOsAfterAsserting: false);
			AssertLocationIsAllowed(allowed: allowedSameUNLOCO, zoneCode: "IMPB", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.RatingImport, zoneMode: Constants.RateMode.AIR, expectedToHaveWarning: allowedSameUNLOCO);

			AssertLocationIsAllowed(allowed: true, zoneCode: "TXAA", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.Tax, zoneMode: Constants.RateMode.AIR, clearUNLOCOsAfterAsserting: false);
			AssertLocationIsAllowed(allowed: true, zoneCode: "TXAB", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.Tax, zoneMode: Constants.RateMode.AIR, clearUNLOCOsAfterAsserting: false);

			// a rating zone without a location
			var ratingAllZone = CreateZone("RATC", RefZoneHeaderLookups.ZoneTypeCodes.Rating, Constants.RateMode.ALL);
			ratingAllZone.RunPreSaveValidation();
			AssertLocationIsAllowed(allowed: true, zoneCode: "RATD", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.Rating, zoneMode: Constants.RateMode.ALL);

			AssertLocationIsAllowed(allowed: true, zoneCode: "EXPD", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.RatingExport, zoneMode: Constants.RateMode.ALL);
			AssertLocationIsAllowed(allowed: true, zoneCode: "IMPD", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.RatingImport, zoneMode: Constants.RateMode.ALL);
			AssertLocationIsAllowed(allowed: true, zoneCode: "TXL2", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.Tax, zoneMode: Constants.RateMode.ALL);
		}

		public void TestUniqueMembers_TAXZoneTypeException()
		{
			var taxZone = CreateZone("TXAL", RefZoneHeaderLookups.ZoneTypeCodes.Tax, Constants.RateMode.ALL);
			AddLocationToZone(taxZone, LocationHelper.GetLocationFromString("AUSYD", Factory));
			taxZone.RunPreSaveValidation();
			AssertNoErrors(((RefZonePivot)taxZone.UNLOCOs.GetRelationshipBusinessObject(taxZone.UNLOCOs[0])).F2_ParentIDInfo);

			AssertLocationIsAllowed(allowed: true, zoneCode: "TAX1", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.Tax, zoneMode: Constants.RateMode.ALL);

			var taxAirZone = CreateZone("TXAR", RefZoneHeaderLookups.ZoneTypeCodes.Tax, Constants.RateMode.AIR);
			AddLocationToZone(taxAirZone, LocationHelper.GetLocationFromString("AUSYD", Factory));
			taxZone.RunPreSaveValidation();
			AssertNoErrors(((RefZonePivot)taxZone.UNLOCOs.GetRelationshipBusinessObject(taxZone.UNLOCOs[0])).F2_ParentIDInfo);

			AssertLocationIsAllowed(allowed: true, zoneCode: "TAX2", zoneType: RefZoneHeaderLookups.ZoneTypeCodes.Tax, zoneMode: Constants.RateMode.ALL);
		}

		public void TestContractInternationalZones()
		{
			FreightDataRegistry.Instance.AllowSameUNLOCOForContractInternationalZones.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var contractZone1 = CreateZone("CON1", RefZoneHeaderLookups.ZoneTypeCodes.Contract, Constants.RateMode.ALL);
			AddLocationToZone(contractZone1, LocationHelper.GetLocationFromString("AUSYD", Factory));
			contractZone1.RunPreSaveValidation();
			AssertNoErrors(((RefZonePivot)contractZone1.UNLOCOs.GetRelationshipBusinessObject(contractZone1.UNLOCOs[0])).F2_ParentIDInfo);

			var contractZone2 = CreateZone("CON2", RefZoneHeaderLookups.ZoneTypeCodes.Contract, Constants.RateMode.ALL);
			AddLocationToZone(contractZone2, LocationHelper.GetLocationFromString("AUSYD", Factory));
			contractZone2.RunPreSaveValidation();
			AssertHasWarning(((RefZonePivot)contractZone2.UNLOCOs.GetRelationshipBusinessObject(contractZone2.UNLOCOs[0])).F2_ParentIDInfo,
				"AUSYD is included in multiple Contract International Zones: CON1, CON2. It may result in Allocation Routes with different but overlapping International Zones as Load / Discharge Ports being loaded into the ‘Contract & Allocation Routes Search Form’ when launched from relevant jobs.");

			var contractZone3 = CreateZone("CON3", RefZoneHeaderLookups.ZoneTypeCodes.Contract, Constants.RateMode.ALL);
			AddLocationToZone(contractZone3, LocationHelper.GetLocationFromString("AUSYD", Factory));
			contractZone3.RunPreSaveValidation();
			AssertHasWarning(((RefZonePivot)contractZone3.UNLOCOs.GetRelationshipBusinessObject(contractZone3.UNLOCOs[0])).F2_ParentIDInfo,
				"AUSYD is included in multiple Contract International Zones: CON1, CON2, CON3. It may result in Allocation Routes with different but overlapping International Zones as Load / Discharge Ports being loaded into the ‘Contract & Allocation Routes Search Form’ when launched from relevant jobs.");
		}

		#region Implementation

		/// <summary>
		/// Create a new zone, add location AUSYD to it, and validate it.
		/// </summary>
		/// <param name="allowed">If not allowed then there should be location validation errors.</param>
		/// <param name="zoneCode"></param>
		/// <param name="zoneType"></param>
		/// <param name="zoneMode"></param>
		/// <param name="expectedToHaveWarning">When TRUE: There should be location validation warnings when validate the zone. Default FALSE.</param>
		/// <param name="clearUNLOCOsAfterAsserting">When TRUE: After asserting, remove all locations from the new zone. Default TRUE.</param>
		/// <returns></returns>
		protected RefZoneHeader AssertLocationIsAllowed(
			bool allowed,
			string zoneCode,
			string zoneType,
			string zoneMode = Constants.RateMode.ALL,
			bool expectedToHaveWarning = false,
			bool clearUNLOCOsAfterAsserting = true)
		{
			var zone = CreateZone(zoneCode, zoneType, zoneMode);

			AddLocationToZone(zone, LocationHelper.GetLocationFromString("AUSYD", Factory));
			zone.RunPreSaveValidation();

			var propInfo = ((RefZonePivot)zone.UNLOCOs.GetRelationshipBusinessObject(zone.UNLOCOs[0])).F2_ParentIDInfo;
			if (!allowed)
			{
				AssertHasErrors($"Zone {zoneCode}-{zoneType}-{zoneMode} should have location validation error(s)", propInfo);
			}
			else if (expectedToHaveWarning)
			{
				AssertHasWarnings($"Zone {zoneCode}-{zoneType}-{zoneMode} should have location validation warning(s)", propInfo);
			}
			else
			{
				AssertNoNotifications($"Zone {zoneCode}-{zoneType}-{zoneMode} should not have location validation notification(s)", propInfo);
			}

			if (clearUNLOCOsAfterAsserting)
			{
				zone.UNLOCOs.RemoveAll();
			}

			return zone;
		}

		RefZoneHeader CreateZone(string zoneCode, string zoneType, string zoneMode)
		{
			var zone = Factory.NewWithValidTestData<RefZoneHeader>();
			zone.FZ_Code = zoneCode;
			zone.FZ_ZoneType = zoneType;
			zone.FZ_ZoneMode = zoneMode;
			zone.FZ_Description = zoneCode;
			Factory.Save();

			return zone;
		}

		void AddLocationToZone(RefZoneHeader zone, ILocation location)
		{
			if (location is RefUNLOCO)
			{
				zone.UNLOCOs.Add((RefUNLOCO)location);
			}
			else if (location is RefCountry)
			{
				zone.Countries.Add((RefCountry)location);
			}

			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(RefZonePivotSchema.Constants.TableName);
		}

		#endregion
	}
}
