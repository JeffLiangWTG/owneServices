using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.DataTransfer;

public class CarrierServiceLevelsMapper
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "This one is not epxected to be translated")]
	public CarrierServiceLevelsMapper(OrgHeader carrier)
	{
		serviceLevelsCache = carrier.MiscServ.CarrierServiceLevels
			.Cast<OrgCarrierServiceLevel>()
			.ToDictionary(s => s.PL_CarrierServiceLevelDescription, s => s.PL_Code);

		var query = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, carrier.PK);
		query.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel);

		mappingsCache = carrier.Factory
			.Load<OrgPatternMatchOverride>(query)
			.ToDictionary(m => m.OO_ForeignCode, m => m.OO_LocalCode);
	}

	/// <summary>
	///		Gets or creates a mapped service level for a given TACT service level.
	/// </summary>
	/// <param name="carrier">
	///		The carrier organization header to get mapping for. This parameter is required even though the same carrier is passed in the constructor
	///		because this class is designed to be used in a multithreaded environment. All manipulations with the carrier (like service level creation)
	///		must be done on the carrier instance and factory for the particular thread the method is called from. Since we can process TACT rates for the same carrier
	///		from multiple threads, a different instance of OrgHeader for the same carrier will be used and we need to make sure the right instance is used by
	///		the thread.
	///
	///		The carrier in the constructor is only used to preload service levels and mappings for the carrier into the cache so that all threads processing
	///		rates for this carrier could use the same cache. But each thread must pass its own Carrier instance in the method.
	/// </param>
	/// <param name="tactServiceLevel">
	///		The TACT service level to map.
	/// </param>
	public (string serviceLevel, string error) GetMappedServiceLevel(OrgHeader carrier, string tactServiceLevel)
	{
		// Actually the synchronization is not required as it is handled by the TACTImporter, but just in case it won't hurt to have it here as well
		lock (mappingsCache)
		{
			return GetMappedServiceLevelCore(carrier, tactServiceLevel);
		}
	}

	(string serviceLevel, string error) GetMappedServiceLevelCore(OrgHeader carrier, string tactServiceLevel)
	{
		// Try to get mapping from cache
		if (mappingsCache.TryGetValue(tactServiceLevel, out var mappedCode))
		{
			return (mappedCode, null);
		}

		// Mapping doesn't exist yet, so, we need to create a service level for the carrier and then map it to the carrier service level (the one from TACT)

		// But, before creating a new service level we need to make sure we don't have one created already.
		// When a service level is created from TACT import, we put TACT carrier service level as a description.
		// So, we check if we already have a service level with such description.
		// (the serviceLevelsCache has service description as a key and service level as a value, for faster search)
		var localCode = serviceLevelsCache.GetOrAdd(tactServiceLevel, () =>
		{
			// A service level for this TACT service level is not yet created, so, we will try to generate one.
			// TACT service level is 4 characters long, CW1 service level is 3 characters long.
			// The way we generate a service level code is that we take first 2 characters from the TACT service level,
			// and then add an index from 1 to 9. If all indexes are used, then we won't create and fail with the error below.
			//
			// Also, since usually carrier service level in TACT rates looks like this: EK03, EK01, EK02, we will try to
			// maintain the indexing so that EK03 is mapped to EK3 in CW1, EK02 is mapped to EK2. Same we do for other formats
			// like AF10, AF20, AF30 which will be mapped to AF1, AF2 and AF3 correspondingly.
			var generatedCode = GenerateServiceLevelCode(serviceLevelsCache.Values, tactServiceLevel);
			if (!string.IsNullOrEmpty(generatedCode))
			{
				var serviceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
				serviceLevel.PL_Code = generatedCode;
				serviceLevel.PL_CarrierServiceLevelDescription = tactServiceLevel;
			}

			return generatedCode;
		});

		if (string.IsNullOrEmpty(localCode))
		{
			return (null,
				Res.GetString("ae40be9c-50be-11e7-9ce0-fcaa14295823",
					"Can't allocate service level code for carrier {0} and TACT carrier service level '{1}'",
					carrier.OH_Code, tactServiceLevel));
		}

		// Map the local service level code to a TACT service level
		var mapping = carrier.Factory.New<OrgPatternMatchOverride>();
		using (mapping.GetValidationSuspender())
		using (mapping.SuspendSettingHasChanges())
		{
			mapping.OO_OH = carrier.PK;
			mapping.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel;
			mapping.OO_ForeignCode = tactServiceLevel;
			mapping.OO_LocalCode = localCode;
		}

		mappingsCache[tactServiceLevel] = localCode;
		carrier.Factory.Save();

		return (localCode, null);
	}

	static string GenerateServiceLevelCode(IEnumerable<ZString> existingService, string tactServiceLevel)
	{
		var prefix = tactServiceLevel.Substring(0, 2);
		var maybeNumber = tactServiceLevel.Substring(2);

		// Since usually carrier service level in TACT rates looks like this: EK03, EK01, EK02, we will try to
		// maintain the indexing so that EK03 is mapped to EK3 in CW1, EK02 is mapped to EK2. Same we do for other formats
		// like AF10, AF20, AF30 which will be mapped to AF1, AF2 and AF3 correspondingly.
		if (int.TryParse(maybeNumber, out int number))
		{
			number = Math.Abs(number);

			if (number > 9)
			{
				number /= 10;
			}

			if (number > 0)
			{
				var code = prefix + number;

				if (!existingService.Contains(code))
				{
					return code;
				}
			}
		}

		for (char suffix = '1'; suffix <= '9'; suffix = (char)(suffix + 1))
		{
			var code = prefix + suffix;

			if (!existingService.Contains(code))
			{
				return code;
			}
		}

		return null;
	}

	readonly IDictionary<ZString, ZString> serviceLevelsCache;
	readonly IDictionary<ZString, ZString> mappingsCache;
}