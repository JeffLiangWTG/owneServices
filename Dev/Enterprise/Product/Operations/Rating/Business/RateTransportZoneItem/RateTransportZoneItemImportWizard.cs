using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RateTransportZoneItemImportWizard : ImportWizard
	{
		public RateTransportZoneItemImportWizard(string countryCode, IImportCollectionInfo collectionInfo, ISettingsStorage settingsStorage, IFileMapper fileMapper)
			: base(collectionInfo, settingsStorage, fileMapper)
		{
			this.countryCode = countryCode;
			Factory = collectionInfo.Collection.Factory;
			refCityTownPostcodeHelper = new RefCityTownPostcodeHelper(null, countryCode, string.Empty, string.Empty, string.Empty);
		}

		readonly string countryCode;
		readonly RefCityTownPostcodeHelper refCityTownPostcodeHelper;
		const string StateCodeColumnPath = "CityTown+State+RW_Code"; // Constant mapping string
		const string StateDescriptionColumnPath = "CityTown+State+RW_Description"; // Constant mapping string

		protected override void ImportIntoBizObjCore(Action<BusinessObject, string, object> setValue, IEnumerable<ImportWizardMapping> mappedRecords, BusinessObject bizObj, string[] values)
		{
			importingPostcode = null;
			var state = string.Empty;
			isPreviewing = bizObj is ImportWizardPreviewLine;

			var mapping = mappedRecords.Where(m => m.HasMappedFrom());
			var stateMapping = mapping.FirstOrDefault(m => m.MappingName == StateCodeColumnPath);
			if (stateMapping != null && stateMapping.FileColumnIndexOrder.Any())
			{
				state = stateMapping.GetMappedFieldValue(values);
			}

			foreach (var map in mapping)
			{
				var value = map.GetMappedFieldValue(values);
				ReadMappingValueIntoBizo(bizObj, map.MappingName, value, state);
			}
		}

		RefPostCode importingPostcode;
		bool isPreviewing;

		void ReadMappingValueIntoBizo(BusinessObject bizObj, string mappingName, string value, string state)
		{
			switch (mappingName)
			{
				case AutoRateTransportZoneItem.Schema.TQ_FromPostCode:
					importingPostcode = GetOrCreatePostcode(value);
					bizObj[mappingName] = importingPostcode?.RK_CityTownPostCode ?? string.Empty;
					break;
				case AutoRateTransportZoneItem.Schema.TQ_ToPostCode:
					var postCode = GetOrCreatePostcode(value);
					bizObj[mappingName] = postCode?.RK_CityTownPostCode ?? string.Empty;
					break;
				case AutoRateTransportZoneItem.Schema.TQ_R9_CityTown:
					bizObj[mappingName] = GetCityTownFromPostcode(value, state);
					break;
				case AutoRateTransportZoneItem.Schema.TQ_FromDistance:
				case AutoRateTransportZoneItem.Schema.TQ_ToDistance:
					bizObj[mappingName] = ZInt.ParseSafe(value, 0);
					break;
				case AutoRateTransportZoneItem.Schema.TQ_IsBeyond:
				case AutoRateTransportZoneItem.Schema.TQ_IsExcludingPostCode:
					if (!string.IsNullOrEmpty(value))
					{
						bizObj[mappingName] = ZBool.ParseSafe(value, ZBool.False);
					}
					break;
				case StateCodeColumnPath:
				case StateDescriptionColumnPath:
					if (isPreviewing)
					{
						bizObj[mappingName] = value;
					}
					break;
				default:
					break;
			}
		}

		ZGuid GetCityTownFromPostcode(string city, string state)
		{
			ZGuid result = ZGuid.Empty;
			if (importingPostcode != null)
			{
				var stateIsEmpty = string.IsNullOrEmpty(state);
				var cityTownFound = importingPostcode.CityTowns.FirstOrDefault(c => c.R9_InternationalName.EqualsIgnoringCase(city) && (stateIsEmpty || c.R9_RW_NKState.EqualsIgnoringCase(state)));
				if (cityTownFound != null)
				{
					result = cityTownFound.PK;
				}
				else if (!isPreviewing && refCityTownPostcodeHelper.ConfirmWithUser(state, city, importingPostcode))
				{
					result = GetOrCreateCityTownAndLinkToPostcode(city, state, importingPostcode);
				}
			}
			else
			{
				result = GetOrCreateCityTown(city, string.Empty, state);
			}

			return result;
		}

		public new BusinessObjectFactory Factory { get; }

		#region GetOrCreatePostcode
		RefPostCode GetOrCreatePostcode(string code)
		{
			return FindPostcode(code) ?? Factory.Load<RefPostCode>(refCityTownPostcodeHelper.GetPostcodePKFromCode(code));
		}

		RefPostCode FindPostcode(string code)
		{
			var query = new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, code).
				AddToFilter(RefPostCodeSchema.RK_IsActive, true).
				AddToFilter(RefPostCodeSchema.RK_RN_NKCountry, countryCode);
			return Factory.LoadTop1<RefPostCode>(query);
		}
		#endregion

		#region GetOrCreateCityTown
		ZGuid GetOrCreateCityTownAndLinkToPostcode(string city, string state, RefPostCode postcode)
		{
			ZGuid result;
			var factory = Factory.CreateNewFactory();
			var cityTownFromDB = factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, city).
				AddToFilter(RefCityTownSchema.R9_RW_NKState, state).
				AddToFilter(RefCityTownSchema.R9_RN_NKCountry, countryCode));

			if (cityTownFromDB == null)
			{
				var newCityTown = factory.New<RefCityTown>();
				newCityTown.R9_InternationalName = city;

				var refCountryStates = refCityTownPostcodeHelper.GetOrCreateCountryState(state, city);
				if (refCountryStates != null)
				{
					newCityTown.R9_RW_NKState = refCountryStates.RW_Code;
				}

				newCityTown.R9_IsActive = true;
				newCityTown.R9_IsSystem = false;
				newCityTown.R9_RN_NKCountry = countryCode;
				result = newCityTown.PK;
			}
			else
			{
				result = cityTownFromDB.PK;
			}

			var pivot = factory.New<RefCityPCodePivot>();
			pivot.R0_RK = postcode.PK;
			pivot.R0_R9 = result;
			pivot.R0_IsSystem = false;

			factory.Save();

			return result;
		}

		ZGuid GetOrCreateCityTown(string code, string postcode, string state)
		{
			return FindCityTown(code, state)?.PK ?? refCityTownPostcodeHelper.GetCityTownPKFromCode(code, postcode, state);
		}

		RefCityTown FindCityTown(string code, string state)
		{
			RefCityTown result = null;
			var query = new ZQuery(RefCityTownSchema.R9_InternationalName, code).
				AddToFilter(RefCityTownSchema.R9_IsActive, true).
				AddToFilter(RefCityTownSchema.R9_RN_NKCountry, countryCode);
			result = Factory.LoadTop1<RefCityTown>(query.ShallowClone().AddToFilter(RefCityTownSchema.R9_RW_NKState, state));
			if (result == null)
			{
				result = Factory.LoadTop1<RefCityTown>(query);
			}

			return result;
		}
		#endregion
	}
}
