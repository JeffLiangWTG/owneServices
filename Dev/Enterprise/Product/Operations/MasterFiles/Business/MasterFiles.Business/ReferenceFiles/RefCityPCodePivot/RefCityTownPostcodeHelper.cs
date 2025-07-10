using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.Business
{
	public class RefCityTownPostcodeHelper
	{
		public RefCityTownPostcodeHelper(IBusinessObjectCollection collection, string countryCode, string postcode, string cityTown, string state)
		{
			CountryCode = countryCode;
			parentPostcode = postcode;
			parentState = state;
			parentCityTown = cityTown;
			parentCollection = collection;
			Factory = new BusinessObjectFactory();
		}

		readonly IBusinessObjectCollection parentCollection;

		internal ICityTownPostcodeUserInteraction UserInteraction => (ICityTownPostcodeUserInteraction)parentCollection;

		readonly string parentPostcode;

		readonly string parentState;

		readonly string parentCityTown;

		bool suspendInstanceSave;

		BusinessObjectFactory Factory { get; set; }

		internal string CountryCode { get; }

		public ZGuid GetPostcodePKFromCode(string postcode)
		{
			postcode = AsyncTaskSynchronizer.Run(() => FindCandidateCityTownsFromWebByPostcode(postcode));
			var result = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, postcode)
							.AddToFilter(RefPostCodeSchema.RK_RN_NKCountry, CountryCode)
							.AddToFilter(RefPostCodeSchema.RK_IsActive, true))?.PK ?? ZGuid.Invalid;
			if (!result.IsValid)
			{
				if (SystemDataRegistry.Instance.DisableNonVerifiablePostcodeWarning.Value ||
					ConfirmWithUser(Res.GetString("bff2e1a3-7d54-47b0-ada4-f224ca8e5218", "Postcode {0}", postcode)))
				{
					result = GetOrCreatePostcode(postcode, false)?.PK ?? result;
					if (!string.IsNullOrEmpty(parentCityTown))
					{
						var city = GetOrCreateCityTown(parentCityTown, parentState, false);
						CreateRefCityTownPostcodePivot(city.PK, result, false);
					}
				}
			}

			return result;
		}

		public ZGuid GetCityTownPKFromCode(string cityTown, string postcode, string state)
		{
			var cityWithoutDiacritics = StripDiacritics.RemoveDiacritics(cityTown, CountryCode);
			AsyncTaskSynchronizer.Run(() => FindCandidateCityTownsFromWebByCityTown(cityWithoutDiacritics, state));
			var result = ZGuid.Invalid;
			var query = new ZQuery(RefCityTownSchema.R9_InternationalName, cityWithoutDiacritics)
				.AddToFilter(RefCityTownSchema.R9_RN_NKCountry, CountryCode)
				.AddToFilter(RefCityTownSchema.R9_IsActive, true);
			if (string.IsNullOrEmpty(state))
			{
				var cityTowns = Factory.Load<RefCityTown>(query);
				if (cityTowns != null && cityTowns.Any())
				{
					result = cityTowns[0].PK;
					if (UserInteraction != null && cityTowns.Length > 1)
					{
						result = UserInteraction.OnSelectionNeeded(result);
					}
				}
			}
			else
			{
				result = Factory.Load<RefCityTown>(query.AddToFilter(RefCityTownSchema.R9_RW_NKState, state)).SingleOrDefault()?.PK ?? ZGuid.Invalid;
			}

			if (!result.IsValid &&
				(SystemDataRegistry.Instance.DisableNonVerifiableCityTownWarning.Value ||
				ConfirmWithUser(Res.GetString("de12ef06-2b2b-4022-9ff1-bc007b25d2a8", "City/Town {0}{1}", cityTown, string.IsNullOrEmpty(state) ? "" : ", " + state))))
			{
				result = GetOrCreateCityTown(cityTown, state, false).PK;

				if (!string.IsNullOrEmpty(postcode) ||
					!string.IsNullOrEmpty(postcode = parentPostcode))
				{
					var refPostcode = GetOrCreatePostcode(postcode, false);
					CreateRefCityTownPostcodePivot(result, refPostcode.PK, false);
				}
			}

			return result;
		}

		public ZGuid GetCityTownPKFromCode(string cityTown)
		{
			return GetCityTownPKFromCode(cityTown, string.Empty, string.Empty);
		}

		public bool ConfirmWithUser(string userInput)
		{
			var message = Res.GetString("bf523dae-e2cb-4cee-84ca-82a78b196453", @"{0}, {1} not found. Would you like to add this to reference data?
Note: If you choose not to add this you will not be able to save this row.", userInput, CountryCode);
			return ShowWarningConfirm(message);
		}

		bool ShowWarningConfirm(string message)
		{
			var result = Globals.Message.Show(message, Res.GetString("a50fc19e-b1b8-4f54-98d3-2d297f220d33", "Warning"), ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Warning);
			return result == ZDialogResult.Yes;
		}

		public bool ConfirmWithUser(string state, string cityTownName, RefPostCode importingPostcode)
		{
			var cityTownFromDB = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, cityTownName).
				AddToFilter(RefCityTownSchema.R9_RW_NKState, state).
				AddToFilter(RefCityTownSchema.R9_RN_NKCountry, CountryCode));
			var confirmed = false;

			if (cityTownFromDB != null)
			{
				var associated = cityTownFromDB.PostCodes.Any() ?
					Res.GetString("5D7DEBA6-2C33-4278-82CE-FC05B47C07CF", "{0} is associated with {1} etc", cityTownFromDB.R9_InternationalName, string.Join(", ", cityTownFromDB.PostCodes.Select(u => u.RK_CityTownPostCode).Take(2))) :
					Res.GetString("57A7D2EA-F685-4192-9834-1B5ADD486D02", "{0} is not associated with any postcode", cityTownFromDB.R9_InternationalName);

				var postcodeLinkedTo = importingPostcode.CityTowns.Any() ?
					Res.GetString("1E93FCE4-02AE-416D-B28F-0E615EC369A2", "{0} is linked to {1}", importingPostcode.RK_CityTownPostCode, string.Join(", ", importingPostcode.CityTowns.Select(u => u.R9_InternationalName))) :
					Res.GetString("4684D818-10C7-4309-B740-A79F850EB47E", "{0} is not linked to any city/town", importingPostcode.RK_CityTownPostCode);

				var message = Res.GetString("84D2C796-6FA4-48C2-91C9-1CABCC353B67", @"{0} {1} {2} and postcode {3} exist in your database, however they are not linked together. {4} and {5}. It may be a mistake in the import file. Are you sure you want to link these together?
Note: If you choose not to add this you will not be able to save this row.", cityTownFromDB.R9_InternationalName, state, CountryCode, importingPostcode.RK_CityTownPostCode, associated, postcodeLinkedTo);
				var result = Globals.Message.Show(message, Res.GetString("a50fc19e-b1b8-4f54-98d3-2d297f220d33", "Warning"), ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Warning);

				confirmed = result == ZDialogResult.Yes;
			}
			else
			{
				confirmed = ConfirmWithUser(Res.GetString("1be44b09-2483-4099-a00e-6e38030048ce", "{0} {1}", importingPostcode.RK_CityTownPostCode, cityTownName));
			}

			return confirmed;
		}

		async Task<string> FindCandidateCityTownsFromWebByPostcode(string postcode)
		{
			var matchedResult = postcode;
			if (!string.IsNullOrWhiteSpace(postcode))
			{
				var addressHolder = GetAddressHolder("", "", postcode);
				var respond = await GetCityTownPostcodeListFromWebAsync(addressHolder);
				if (respond != null && respond.Any())
				{
					if (respond.Length == 1 &&
						IsSamePostcodeAsWebResult(postcode, respond.First().Postcode))
					{
						matchedResult = respond.First().Postcode;
					}
					var respondList = respond.ToList();
					var newCityTowns = respond.DistinctBy(r => r.City)
						.Where(c => !Factory.Exists(typeof(RefCityTown),
						new ZQuery(RefCityTownSchema.R9_RN_NKCountry, CountryCode)
						.AddToFilter(RefCityTownSchema.R9_RW_NKState, FindStateCode(c.State, c.City, false))
						.AddToFilter(RefCityTownSchema.R9_InternationalName, c.City)
						.AddToFilter(RefCityTownSchema.R9_IsActive, true)));
					foreach (var newCity in newCityTowns)
					{
						respond = await GetCityTownPostcodeListFromWebAsync(GetAddressHolder(newCity.City, newCity.State, ""));
						if (respond != null && respond.Any())
						{
							respondList.AddRange(respond);
						}
					}

					OrganizeAndSaveCityTowns(respondList.ToArray());
				}
			}

			return matchedResult;
		}

		bool IsSamePostcodeAsWebResult(string userInput, string webResult)
		{
			return StripUserInputPostcode(userInput) == StripUserInputPostcode(webResult);
		}

		string StripUserInputPostcode(string input)
		{
			var stringBuilder = new StringBuilder();

			foreach (var character in input)
			{
				if (char.IsLetterOrDigit(character))
				{
					stringBuilder.Append(char.ToUpperInvariant(character));
				}
			}

			return stringBuilder.ToString();
		}

		async Task<bool> FindCandidateCityTownsFromWebByCityTown(string city, string state)
		{
			var result = false;
			if (!string.IsNullOrWhiteSpace(city))
			{
				var addressHolder = GetAddressHolder(city, state, "");
				var respond = await GetCityTownPostcodeListFromWebAsync(addressHolder);
				if (respond != null && respond.Any())
				{
					OrganizeAndSaveCityTowns(respond);
					result = true;
				}
			}

			return result;
		}

		protected virtual async Task<CandidateCityTown[]> GetCityTownPostcodeListFromWebAsync(ISupportWebAddressValidation addressHolder)
		{
			var message = Res.GetString("565af498-1445-4547-bb60-a1e7d536a1cd", "Searching {0}{1}{2}{3} from WiseCloud.",
				string.IsNullOrWhiteSpace(addressHolder.City) ? "" : addressHolder.City + ", ",
				string.IsNullOrWhiteSpace(addressHolder.State) ? "" : addressHolder.State + ", ",
				string.IsNullOrWhiteSpace(addressHolder.Postcode) ? "" : addressHolder.Postcode + ", ",
				CountryCode);
			UserInteraction?.OnBusy(message);
			var result = await AddressValidationService.GetCityTownAsync(addressHolder, new CancellationTokenSource());
			UserInteraction?.OnCompleted();
			return result?.Where(r => !string.IsNullOrWhiteSpace(r.City)).ToArray();
		}

		void OrganizeAndSaveCityTowns(CandidateCityTown[] candidates, bool throwExceptionWhenZSaveExceptionOccur = false)
		{
			var importingString = candidates.Length == 1 ?
				Res.GetString("bb54ca30-388e-419b-9ec3-c5f8f7cffcb4", "1 Postcode and City") :
				Res.GetString("9b760c8d-2e9c-4f4a-88d5-d20024f445ee", "{0} Postcodes and Cities", candidates.Length);
			UserInteraction?.OnBusy(Res.GetString("338b9a8b-21fc-4f59-b19f-ea4564cc4a35", "Updating Reference Data, Importing {0}", importingString));

			try
			{
				OrganizeAndSaveCityTownsCore(candidates);
				UserInteraction?.OnCompleted();
			}
			catch (ZSaveException)
			{
				UserInteraction?.OnCompleted();
				Factory = new BusinessObjectFactory();

				if (throwExceptionWhenZSaveExceptionOccur)
				{
					throw;
				}

				if (ShowWarningConfirm(Res.GetString("bcc6b354-1c0b-4739-9380-38369cdd0a5f", "Failed to update Reference Data due to other people change the Reference Data at the same time, do you want to retry?")))
				{
					OrganizeAndSaveCityTowns(candidates, true);
				}
			}
		}

		protected virtual void OrganizeAndSaveCityTownsCore(CandidateCityTown[] candidates)
		{
			suspendInstanceSave = true;

			foreach (var item in candidates)
			{
				if (!string.IsNullOrWhiteSpace(item.Postcode))
				{
					var cityTown = GetCityTown(item.City, item.State, true);
					if (cityTown != null && !cityTown.R9_IsActive)
					{
						cityTown.R9_IsActive = true;
					}

					var postcode = GetOrCreatePostcode(item.Postcode, true);

					if (!string.IsNullOrWhiteSpace(item.City))
					{
						cityTown = GetOrCreateCityTown(item.City, item.State, true);
						CreateRefCityTownPostcodePivot(cityTown.PK, postcode.PK, true);
					}
				}
			}

			suspendInstanceSave = false;

			SaveChanges();
		}

		void SaveChanges()
		{
			if (!suspendInstanceSave)
			{
				Factory.Save();
			}
		}

		void CreateRefCityTownPostcodePivot(ZGuid cityTownPK, ZGuid postcodePK, bool isSystem)
		{
			if (!Factory.Exists(typeof(RefCityPCodePivot), (new ZQuery(RefCityPCodePivotSchema.R0_R9, cityTownPK).AddToFilter(RefCityPCodePivotSchema.R0_RK, postcodePK))))
			{
				var pivot = Factory.New<RefCityPCodePivot>();
				pivot.R0_IsSystem = isSystem;
				pivot.R0_RK = postcodePK;
				pivot.R0_R9 = cityTownPK;
				SaveChanges();
			}
		}

		RefPostCode GetOrCreatePostcode(string code, bool isSystem)
		{
			var postcode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_RN_NKCountry, CountryCode)
				.AddToFilter(RefPostCodeSchema.RK_CityTownPostCode, code)
				.AddToFilter(RefPostCodeSchema.RK_IsActive, true));

			if (postcode == null && code.Length <= RefPostCodeSchema.RK_CityTownPostCode.MaxLength)
			{
				postcode = Factory.New<RefPostCode>();
				postcode.RK_CityTownPostCode = code;
				postcode.RK_IsActive = true;
				postcode.RK_IsSystem = isSystem;
				postcode.RK_RN_NKCountry = CountryCode;
				SaveChanges();
			}

			return postcode;
		}

		RefCityTown GetOrCreateCityTown(string city, string state, bool isSystem)
		{
			var stateCode = GetOrCreateCountryState(state, city)?.RW_Code ?? "";
			return Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_RN_NKCountry, CountryCode)
				.AddToFilter(RefCityTownSchema.R9_InternationalName, city)
				.AddToFilter(RefCityTownSchema.R9_RW_NKState, stateCode)
				.AddToFilter(RefCityTownSchema.R9_IsActive, true))
				?? CreateCityTown(city, stateCode, isSystem);
		}

		RefCityTown GetCityTown(string city, string state, bool isSystem)
		{
			RefCityTown cityTown = null;
			var stateCode = FindStateCode(state, city, true);
			if (string.IsNullOrWhiteSpace(stateCode))
			{
				stateCode = "";
			}

			if (isSystem)
			{
				cityTown = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_RN_NKCountry, CountryCode)
				.AddToFilter(RefCityTownSchema.R9_InternationalName, city)
				.AddToFilter(RefCityTownSchema.R9_RW_NKState, stateCode));
			}
			else
			{
				cityTown = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_RN_NKCountry, CountryCode)
				.AddToFilter(RefCityTownSchema.R9_InternationalName, city)
				.AddToFilter(RefCityTownSchema.R9_RW_NKState, ""));
			}

			return cityTown;
		}

		RefCityTown CreateCityTown(string city, string state, bool isSystem)
		{
			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_IsActive = true;
			cityTown.R9_IsSystem = isSystem;
			cityTown.R9_RN_NKCountry = CountryCode;
			cityTown.R9_RW_NKState = state;
			cityTown.R9_R3_TimeZone = ZGuid.Empty;
			cityTown.R9_InternationalName = city;
			SaveChanges();
			return cityTown;
		}

		public RefCountryStates GetOrCreateCountryState(string state, string city)
		{
			var stateCode = FindStateCode(state, city, true);
			if (string.IsNullOrWhiteSpace(stateCode))
			{
				return null;
			}

			var result = Factory.LoadTop1<RefCountryStates>(new ZQuery(RefCountryStatesSchema.RW_Code, stateCode)
				.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, CountryCode)
				.AddToFilter(RefCountryStatesSchema.RW_IsActive, true));
			if (result == null)
			{
				result = Factory.New<RefCountryStates>();
				result.RW_IsActive = true;
				result.RW_IsSystem = true;
				result.RW_Description = state;
				result.RW_Code = stateCode;
				result.RW_RN_NKCountryCode = CountryCode;
				SaveChanges();
			}

			return result;
		}

		ZString FindStateCode(string state, string city, bool returnNewStateCodeAnyWay)
		{
			if (string.IsNullOrWhiteSpace(state))
			{
				return null;
			}

			ZString newStateCode = default;
			var query = new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, CountryCode).AddToFilter(RefCountryStatesSchema.RW_IsActive, true);
			var resultState = Factory.LoadTop1<RefCountryStates>(query.AddToFilter(RefCountryStatesSchema.RW_Code, state));

			if (resultState == null)
			{
				query = new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, CountryCode).AddToFilter(RefCountryStatesSchema.RW_IsActive, true);
				resultState = Factory.LoadTop1<RefCountryStates>(query.AddToFilter(RefCountryStatesSchema.RW_Description, state));
			}

			if (resultState == null && state != city)
			{
				query = new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, CountryCode).AddToFilter(RefCountryStatesSchema.RW_IsActive, true);
				resultState = Factory.LoadTop1<RefCountryStates>(query.AddToFilter(RefCountryStatesSchema.RW_Description, city));
			}

			if (resultState == null)
			{
				newStateCode = state.Length > RefCountryStatesSchema.RW_Code.MaxLength ? state.Substring(0, RefCountryStatesSchema.RW_Code.MaxLength) : state;
				query = new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, CountryCode).AddToFilter(RefCountryStatesSchema.RW_IsActive, true);
				resultState = Factory.LoadTop1<RefCountryStates>(query.AddToFilter(RefCountryStatesSchema.RW_Code, newStateCode));
			}

			return resultState?.RW_Code ?? (returnNewStateCodeAnyWay ? newStateCode : ZString.Empty);
		}

		#region AddressHolderForWebRequest
		public class AddressHolderForWebRequest : ISupportWebAddressValidation
		{
			public ZString Language
			{
				get; set;
			}

			public ZPropertyInfo LanguageInfo => null;

			public int Language_MaxLength => 0;

			public CodeDescriptionPairList LanguageList => null;

			[DocumentFieldExcludeFromMap]
			public ZString UnrestrictedAdditionalAddressInformation { get; set; }

			public ZPropertyInfo UnrestrictedAdditionalAddressInformationInfo => null;

			public CodeDescriptionPairList AdditionalAddressInfoList { get; }

			public ZString AddressCode { get; set; }

			public ZString Address1 { get; set; }

			public ZPropertyInfo Address1Info => null;

			public int Address1_MaxLength => 0;

			public ZString Address2 { get; set; }

			public ZPropertyInfo Address2Info => null;

			public int Address2_MaxLength => 0;

			public ZString City { get; set; }

			public ZPropertyInfo CityInfo => null;

			public int City_MaxLength => 0;

			public ZString Postcode { get; set; }

			public ZPropertyInfo PostcodeInfo => null;

			public int Postcode_MaxLength => 0;

			public ZString CompanyName { get; set; }

			public ZPropertyInfo CompanyNameInfo => null;

			public int CompanyName_MaxLength => 0;

			public ZString StateCode { get; set; }

			public ZPropertyInfo StateCodeInfo => null;

			public int StateCode_MaxLength => 0;

			public CodeDescriptionPairList StateCodeList => null;

			public ZString State { get; set; }

			public int State_MaxLength => 0;

			public ZString CountryCodeISO2 { get; set; }

			public int CountryCodeISO2_MaxLength => 0;

			public RefCountry Country => null;

			public RefCountryCollection CountryCodeList => null;

			public ZString DisplayText { get; set; }

			public ZGuid EntityPK => ZGuid.Empty;

			public ZString AddressRecordGUID => "";

			public ZString AddressSourceTable => "";

			public ZString ValidationStatus { get; set; }

			public ZString AddressMap { get; set; }

			public ZString ClosestPort { get; set; }

			public ZString Addressee => "";

			public ZGeography GeoLocation { get; set; }

			public bool NeedValidation => false;

			public bool IsUpdatingCityTown { get; set; }
			public bool IsValidatingAddress { get; set; }
			public bool IsExactPointFound { get; set; }
			public bool IsValidatedByBackgroundService { get; set; }

			public bool IsErrorSuppressed => false;

			public bool IsTSAKnownAddress => false;

			public bool IsMIDAddress => false;

			public bool IsRowDeletedOrNull => false;

			public bool IsJobDocAddress => false;

			public bool IsRowDeletedOrDetachedOrNull => false;

			public event EventHandler TriggerWebAddressValidation { add { } remove { } }
			public event EventHandler AddressValidationStatusChanged { add { } remove { } }
			public event EventHandler TriggerWebGetCityTown { add { } remove { } }

			public void ClearWebAddressValidationHandler()
			{
			}

			public void ClearWebGetCityTownHandler()
			{
			}

			public Task<CandidateCityTown[]> GetCityTownAsync(CancellationTokenSource cancellationToken) => null;

			public bool GetReadOnlySecurity(PropertyDescriptor property) => false;

			public void PreValidationForAddressValidationService()
			{
			}

			public void ResetValidationStatus(ZPropertyInfo propertyInfo)
			{
			}

			public Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest) => null;

			public void ValidatePostcodeAndStateForAddress()
			{
			}
		}

		AddressHolderForWebRequest GetAddressHolder(string cityTown, string state, string postcode)
		{
			var addressHolder = new AddressHolderForWebRequest();
			addressHolder.Postcode = postcode;
			addressHolder.City = cityTown;
			addressHolder.State = state;
			addressHolder.CountryCodeISO2 = CountryCode;
			addressHolder.Language = "";
			return addressHolder;
		}

		#endregion
	}

	public interface ICityTownPostcodeUserInteraction
	{
		ZGuid OnSelectionNeeded(ZGuid defaultPK);
		void OnBusy(string message);
		void OnCompleted();
		bool AreEventsSubscribed { get; set; }
		event EventHandler<BusyEventArgs> Busy;
		event EventHandler Completed;
		event EventHandler<SelectionNeededEventArgs> SelectionNeeded;
	}

	public class BusyEventArgs : EventArgs
	{
		public BusyEventArgs(string message)
		{
			Message = message;
		}

		public string Message { get; }
	}

	public class SelectionNeededEventArgs : EventArgs
	{
		public SelectionNeededEventArgs(ZGuid selectedPK)
		{
			SelectedPK = selectedPK;
		}

		public ZGuid SelectedPK { get; set; }
	}
}
