using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.MasterFiles.Business
{
#if DEBUG
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
#endif
	public static class DeniedPartyScreenerAsync
	{
		public delegate bool PartyScreenedDelegate();

		public static async Task<List<DpsResponseWithScreeningParty>> Screen(params ScreeningParty[] screeningParties)
		{
			return await Screen(null, screeningParties);
		}

		public static async Task<List<DpsResponseWithScreeningParty>> Screen(PartyScreenedDelegate partyScreened, params ScreeningParty[] screeningParties)
		{
			return await Screen(partyScreened, null, screeningParties);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It's only for diagnostic")]
		public static async Task<List<DpsResponseWithScreeningParty>> Screen(PartyScreenedDelegate partyScreened, IDpsServiceV4 service, params ScreeningParty[] screeningParties)
		{
			var responses = new List<DpsResponseWithScreeningParty>();
			var candidateCreator = new DpsCandidateCreator();

			foreach (var screeningParty in screeningParties)
			{
				if (!screeningParty.IsCurrentScreeningStatusValid && screeningParty.OrgCode != OrgHeader.UnmatchedOrganisationCode)
				{
					var dpsResponseAndRequest = await GetScreeningResults(screeningParty, service, candidateCreator);

					var responseWithScreeningParty = new DpsResponseWithScreeningParty(
						screeningParty,
						dpsResponseAndRequest.Response,
						dpsResponseAndRequest.RequestHeader);

					responses.Add(responseWithScreeningParty);

					WriteMessage(() => string.Format(CultureInfo.CurrentCulture,
						(NoResString)"{0}RESPONSE at {1}:{0}Screened entity:{0}{2}{0}Response:{0}{3}{0}",
						System.Environment.NewLine,
						ZDateTime.Now,
						screeningParty.ScreeningEntity.HumanReadableName,
						JsonConvert.SerializeObject(responseWithScreeningParty.Response, Formatting.Indented, new ProfileHeaderInfoJsonConverter()).Replace("\\r", string.Empty).Replace("\\n", System.Environment.NewLine)));

					if (partyScreened != null && partyScreened())
					{
						break;
					}
				}
				else
				{
					if (screeningParty.OrgCode == OrgHeader.UnmatchedOrganisationCode)
					{
						WriteMessage(() => string.Format(CultureInfo.CurrentCulture,
							(NoResString)"{0}No screening performed for {1} as it is system defined. The time is {2}{0}",
							System.Environment.NewLine,
							screeningParty.ScreeningEntity.HumanReadableName,
							ZDateTime.Now));
					}
					else
					{
						WriteMessage(() => string.Format(CultureInfo.CurrentCulture,
							"{0}No screening performed for {1} as current status is valid at {2}{0}",
							System.Environment.NewLine,
							screeningParty.ScreeningEntity.HumanReadableName,
							ZDateTime.Now));
					}

					responses.Add(new DpsResponseWithScreeningParty(screeningParty, null, null));
				}
			}

			return responses;
		}

		static async Task<(DpsResponse Response, DpsRequestHeaderWithAddressMatching RequestHeader)> GetScreeningResults(ScreeningParty screeningParty, IDpsServiceV4 service, DpsCandidateCreator candidateCreator)
		{
			(DpsResponse Response, DpsRequestHeaderWithAddressMatching RequestHeader) dpsResponseAndRequest = default;

			if (screeningParty.Header != null)
			{
				dpsResponseAndRequest = await ScreenOrgHeader(screeningParty.Header, service, candidateCreator);
			}
			else if (screeningParty.DocAddress != null)
			{
				if (screeningParty.DocAddress.E2_AddressOverride)
				{
					dpsResponseAndRequest = await ScreenDocAddress(screeningParty.DocAddress, service, candidateCreator);
				}
				else if (screeningParty.DocAddress.Address != null)
				{
					dpsResponseAndRequest = await ScreenOrgHeader(screeningParty.DocAddress.Address.Header, service, candidateCreator);
				}
			}
			else if (screeningParty.Vessel != null)
			{
				dpsResponseAndRequest = await ScreenVessel(screeningParty.Vessel, service, candidateCreator);
			}
			else if (screeningParty.NotLinkedVessel != null)
			{
				dpsResponseAndRequest = await ScreenNotLinkedVessel(screeningParty.NotLinkedVessel, service, candidateCreator);
			}
			else if (screeningParty.Country != null)
			{
				dpsResponseAndRequest = await ScreenRefCountry(screeningParty.Country, service, candidateCreator);
			}
			else if (screeningParty.NaturalPerson != null)
			{
				dpsResponseAndRequest = await ScreenNaturalPerson(screeningParty.NaturalPerson, service, candidateCreator);
			}

			return dpsResponseAndRequest;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It's only for diagnostic")]
		static async Task<(DpsResponse, DpsRequestHeaderWithAddressMatching)> ScreenNotLinkedVessel(IScreeningPartyForVessel notLinkedVessel, IDpsServiceV4 service, DpsCandidateCreator candidateCreator)
		{
			var requestHeader = candidateCreator.NewRequestHeader(notLinkedVessel);

			var notLinkedVesselBizO = (BusinessObject)notLinkedVessel;
			WriteRequestMessage("not linked vessel", notLinkedVesselBizO.HumanReadableName, notLinkedVesselBizO.PK, requestHeader);

			return (await GetDpsManager().Screen(requestHeader, service), requestHeader);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It's only for diagnostic")]
		static async Task<(DpsResponse, DpsRequestHeaderWithAddressMatching)> ScreenVessel(RefVessel vessel, IDpsServiceV4 service, DpsCandidateCreator candidateCreator)
		{
			var requestHeader = candidateCreator.NewRequestHeader(vessel);

			WriteRequestMessage("vessel", vessel.HumanReadableName, vessel.PK, requestHeader);

			return (await GetDpsManager().Screen(requestHeader, service), requestHeader);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It's only for diagnostic")]
		static async Task<(DpsResponse, DpsRequestHeaderWithAddressMatching)> ScreenDocAddress(JobDocAddress docAddress, IDpsServiceV4 service, DpsCandidateCreator candidateCreator)
		{
			var requestHeader = candidateCreator.NewRequestHeader(docAddress);

			WriteRequestMessage("address", docAddress.AddressAsASingleLineWithoutCompanyName, docAddress.PK, requestHeader);

			return (await GetDpsManager().Screen(requestHeader, service), requestHeader);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It's only for diagnostic")]
		static async Task<(DpsResponse, DpsRequestHeaderWithAddressMatching)> ScreenOrgHeader(OrgHeader header, IDpsServiceV4 service, DpsCandidateCreator candidateCreator)
		{
			var requestHeader = candidateCreator.NewRequestHeader(header);

			WriteRequestMessage("organization", header.OH_Code + " - " + header.OH_FullName, header.PK, requestHeader);

			return (await GetDpsManager().Screen(requestHeader, service), requestHeader);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It's only for diagnostic")]
		static async Task<(DpsResponse, DpsRequestHeaderWithAddressMatching)> ScreenRefCountry(RefCountry country, IDpsServiceV4 service, DpsCandidateCreator candidateCreator)
		{
			var requestHeader = candidateCreator.NewRequestHeader(country);

			WriteRequestMessage("country", country.RN_Code + " - " + country.RN_Desc, country.PK, requestHeader);

			return (await GetDpsManager().Screen(requestHeader, service), requestHeader);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It's only for diagnostic")]
		static async Task<(DpsResponse, DpsRequestHeaderWithAddressMatching)> ScreenNaturalPerson(NaturalPerson naturalPerson, IDpsServiceV4 service, DpsCandidateCreator candidateCreator)
		{
			var requestHeader = candidateCreator.NewRequestHeader(naturalPerson.Name, naturalPerson.Address1, naturalPerson.Address2, naturalPerson.City, naturalPerson.State, naturalPerson.PostCode, naturalPerson.Country, naturalPerson.AdditionalAddressLine, naturalPerson.IsConsideredAsOrganization);

			WriteRequestMessage("natural person", naturalPerson.Name, naturalPerson.BusinessEntityKey, requestHeader);

			return (await GetDpsManager().Screen(requestHeader, service), requestHeader);
		}

		static IDpsManager GetDpsManager() => ObjectFactory.Get<IDpsManager>();

		public static ScreeningParty[] MergeDuplicateParties(params ScreeningParty[] screeningParties)
		{
			var notLinkedVesselsToBeFilteredOut = screeningParties
				.Where(p => p.NotLinkedVessel != null)
				.GroupBy(p => p.NotLinkedVessel.Code)
				.Where(g => g.Count() > 1)
				.SelectMany(g => g.Skip(1).Where(p => p != g.First()))
				.ToList();

			if (notLinkedVesselsToBeFilteredOut.Count > 0)
			{
				screeningParties = screeningParties.Where(p => !notLinkedVesselsToBeFilteredOut.Contains(p)).ToArray();
			}

			var uniqueParties = new Dictionary<ZGuid, ScreeningParty>();
			var hashIdToKeyDictionary = new Dictionary<int, Guid>();

			foreach (var screeningParty in screeningParties)
			{
				if (screeningParty.NaturalPerson != null)
				{
					if (hashIdToKeyDictionary.TryGetValue(screeningParty.NaturalPerson.HashID, out var key))
					{
						MergeParty(uniqueParties, screeningParty, key);
					}
					else
					{
						var newKey = Guid.NewGuid();
						hashIdToKeyDictionary[screeningParty.NaturalPerson.HashID] = newKey;
						MarkAsUniqueParty(uniqueParties, screeningParty, newKey);
					}
				}
				else if (!screeningParty.Key.IsEmpty)
				{
					if (!uniqueParties.ContainsKey(screeningParty.Key))
					{
						MarkAsUniqueParty(uniqueParties, screeningParty, screeningParty.Key.ToGuid());
					}
					else
					{
						MergeParty(uniqueParties, screeningParty, screeningParty.Key.ToGuid());
					}
				}
			}

			return uniqueParties.Values.ToArray();
		}

		static void MarkAsUniqueParty(Dictionary<ZGuid, ScreeningParty> uniqueParties, ScreeningParty screeningParty, Guid key)
		{
			uniqueParties.Add(key, screeningParty);
		}

		static void MergeParty(Dictionary<ZGuid, ScreeningParty> uniqueParties, ScreeningParty screeningParty, Guid key)
		{
			ScreeningParty scrParty;
			uniqueParties.TryGetValue(key, out scrParty);
			if (!scrParty.Parents.Contains(screeningParty.Parent))
			{
				scrParty.Parents.Add(screeningParty.Parent);
				if (scrParty.ParentsDescription.Length > 0)
				{
					scrParty.ParentsDescription += "; ";
				}
				scrParty.ParentsDescription += screeningParty.ParentsDescription;
			}
		}

		public static ScreeningParty[] GetDeniedParties(params ScreeningParty[] screeningParties)
		{
			if (screeningParties == null)
			{
				return Array.Empty<ScreeningParty>();
			}

			var mergedParties = MergeDuplicateParties(screeningParties);
			var deniedParties = mergedParties.Where(p => p.CurrentScreeningStatus != ScreeningStatusesList.Codes.Clear
				&& p.CurrentScreeningStatus != ScreeningStatusesList.Codes.PermanentClear
				&& p.CurrentScreeningStatus != ScreeningStatusesList.Codes.JobCleared)
				.OrderBy(p => p.CurrentScreeningStatus).ToArray();
			return deniedParties;
		}

		public static bool HasExcludedList(BusinessObjectFactory factory) => DpsComplianceListHelper.HasExcludedList(factory);

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static IMessageWriter MessageWriter { get; set; }

		public static void WriteMessage(string message)
		{
			MessageWriter?.WriteMessage(message);
		}

		public static void WriteMessage(Func<string> getMessageFunc)
		{
			MessageWriter?.WriteMessage(getMessageFunc?.Invoke());
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It's only for diagnostic")]
		static void WriteRequestMessage(string bizOType, string entityCode, ZGuid pk, DpsRequestHeaderWithAddressMatching requestHeader)
		{
			WriteMessage(() => string.Format(CultureInfo.CurrentCulture,
				@"{0}REQUEST for {1} '{2}' ({3}) at {4}:{0}{5}{0}",
				System.Environment.NewLine,
				bizOType,
				entityCode,
				pk,
				ZDateTime.Now,
				JsonConvert.SerializeObject(requestHeader, Formatting.Indented)));
		}
	}

	class ProfileHeaderInfoJsonConverter : JsonConverter<ProfileHeaderInfo>
	{
		public override void WriteJson(JsonWriter writer, ProfileHeaderInfo value, JsonSerializer serializer)
		{
			JToken t = JToken.FromObject(value);
			JObject o = (JObject)t;
			o.Property("ProfileNotes").Value = Compressor.Unzip(value.ProfileNotes);
			o.WriteTo(writer);
		}

		public override ProfileHeaderInfo ReadJson(JsonReader reader, Type objectType, ProfileHeaderInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException("Unnecessary because ReadJson is not used.");
		}
	}
}

