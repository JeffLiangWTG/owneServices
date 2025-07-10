#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.CarrierConnect;

class RateGroup
{
	/// <summary>
	/// Fields that will are used to define the core grouping behavior of rate curation.
	/// These are used to determine the compatability of rates and whether they can be merged.
	/// </summary>
	readonly List<RateGroupingField> rateGroupingFields =
	[
		new RateGroupingField((NoResString)"RateSource",
			group => group.RateSource,
			(newGroup, entry, oldGroup) => newGroup.RateSource = entry is WiseEntry ? RateCurator.UrsRate : RateCurator.CargoWiseRate,
			entry => entry is WiseEntry ? RateCurator.UrsRate : RateCurator.CargoWiseRate
		),
		new RateGroupingField((NoResString)"CarrierServiceLevel",
			group => group.CarrierServiceLevel,
			(newGroup, entry, oldGroup) => newGroup.CarrierServiceLevel = GetMostSpecific(entry.TI_PL_NKCarrierServiceLevel, oldGroup?.CarrierServiceLevel),
			entry => NullIfEmpty(entry.TI_PL_NKCarrierServiceLevel)
		),
		new RateGroupingField((NoResString)"ServiceLevel",
			group => group.ServiceLevel,
			(newGroup, entry, oldGroup) => newGroup.ServiceLevel = GetMostSpecific(entry.TI_RS_NKServiceLevel_NI, oldGroup?.ServiceLevel),
			entry => NullIfEmpty(entry.TI_RS_NKServiceLevel_NI)
		),
		new RateGroupingField((NoResString)"UniversalServiceLevel",
			group => group.UniversalServiceLevel,
			(newGroup, entry, oldGroup) => newGroup.UniversalServiceLevel = GetMostSpecific((entry as WiseEntry)?.UniversalServiceLevel, oldGroup?.UniversalServiceLevel),
			entry => NullIfEmpty((entry as WiseEntry)?.UniversalServiceLevel)
		),
		new RateGroupingField((NoResString)"ContractNumber",
			group => group.ContractNumber,
			(newGroup, entry, oldGroup) => newGroup.ContractNumber = GetMostSpecific(entry.TI_ContractNumber, oldGroup?.ContractNumber),
			entry => NullIfEmpty(entry.TI_ContractNumber)
		),
		new RateGroupingField((NoResString)"Carrier",
			group => group.Carrier?.PK.ToString(),
			(newGroup, entry, oldGroup) => newGroup.Carrier = ToOrganisationDto(entry.TransportProvider) ?? oldGroup?.Carrier,
			entry => NullIfEmpty(entry.TransportProvider?.PK)
		),
		new RateGroupingField((NoResString)"ServiceProvider",
			group => group.ServiceProvider?.GetKey(),
			(newGroup, entry, oldGroup) => newGroup.ServiceProvider = ToUrsCarrier(entry) ?? oldGroup?.ServiceProvider,
			entry => ToUrsCarrier(entry)?.GetKey()
		),
		new RateGroupingField((NoResString)"Consignee",
			group => group.Consignee?.PK.ToString(),
			(newGroup, entry, oldGroup) => newGroup.Consignee = ToOrganisationDto(entry.Consignee) ?? oldGroup?.Consignee,
			entry => NullIfEmpty(entry.Consignee?.PK)
		),
		new RateGroupingField((NoResString)"Consignor",
			group => group.Consignor?.PK.ToString(),
			(newGroup, entry, oldGroup) => newGroup.Consignor = ToOrganisationDto(entry.Consignor) ?? oldGroup?.Consignor,
			entry => NullIfEmpty(entry.Consignor?.PK)
		),
		new RateGroupingField((NoResString)"ControllingCustomer",
			group => group.ControllingCustomer?.PK.ToString(),
			(newGroup, entry, oldGroup) => newGroup.ControllingCustomer = ToOrganisationDto(entry.ControllingCustomer) ?? oldGroup?.ControllingCustomer,
			entry => NullIfEmpty(entry.ControllingCustomer?.PK)
		),
		new RateGroupingField((NoResString)"Via",
			group => group.Via,
			(newGroup, entry, oldGroup) => newGroup.Via = GetMostSpecific(entry.TI_ViaLRC, oldGroup?.Via),
			entry => NullIfEmpty(entry.TI_ViaLRC)
		),
		new RateGroupingField((NoResString)"TransitTime",
			group => group.TransitTime,
			(newGroup, entry, oldGroup) => newGroup.TransitTime = GetMostSpecific(entry.TI_TransitTime, oldGroup?.TransitTime),
			entry => NullIfEmpty(entry.TI_TransitTime)
		),
		new RateGroupingField((NoResString)"AircraftType",
			group => group.AircraftType,
			(newGroup, entry, oldGroup) => newGroup.AircraftType = GetMostSpecific(entry.TI_AircraftType, oldGroup?.AircraftType),
			entry => NullIfEmpty(entry.TI_AircraftType)
		),
		new RateGroupingField((NoResString)"OriginPostCode",
			group => group.OriginPostCode,
			(newGroup, entry, oldGroup) => newGroup.OriginPostCode = GetMostSpecific(entry.TI_CartagePickupAddressPostCode, oldGroup?.OriginPostCode),
			entry => NullIfEmpty(entry.TI_CartagePickupAddressPostCode)
		),
		new RateGroupingField((NoResString)"DestinationPostCode",
			group => group.DestinationPostCode,
			(newGroup, entry, oldGroup) => newGroup.DestinationPostCode = GetMostSpecific(entry.TI_CartageDeliveryAddressPostCode, oldGroup?.DestinationPostCode),
			entry => NullIfEmpty(entry.TI_CartageDeliveryAddressPostCode)
		),
		new RateGroupingField((NoResString)"PortTransportAddress",
			group => group.PortTransportAddress?.ToString(),
			(newGroup, entry, oldGroup) => newGroup.PortTransportAddress = entry.TI_OA_CartagePickupAddressOverride.IsEmpty ? oldGroup?.PortTransportAddress : entry.TI_OA_CartagePickupAddressOverride.ToGuid(),
			entry => NullIfEmpty(entry.TI_OA_CartagePickupAddressOverride)
		),
		new RateGroupingField((NoResString)"Frequency",
			group => group.FrequencyUnit,
			(newGroup, entry, oldGroup) =>
			{
				if (!entry.TI_FrequencyUnit.IsEmpty)
				{
					newGroup.FrequencyUnit = entry.TI_FrequencyUnit;
					newGroup.Frequency = entry.TI_Frequency;
				}
				else
				{
					newGroup.FrequencyUnit = oldGroup?.FrequencyUnit;
					newGroup.Frequency = oldGroup?.Frequency;
				}
			},
			entry => NullIfEmpty(entry.TI_FrequencyUnit)
		),
		new RateGroupingField((NoResString)"IsNonOperatedReefer",
			group => group.IsNonOperatedReefer,
			(newGroup, entry, oldGroup) => newGroup.IsNonOperatedReefer = GetMostSpecific(entry.TI_IsNonOperatedReefer, oldGroup?.IsNonOperatedReefer),
			entry => NullIfEmpty(entry.TI_IsNonOperatedReefer)
		),
		new RateGroupingField((NoResString)"FirstLoad",
			group => group.FirstLoad,
			(newGroup, entry, oldGroup) => newGroup.FirstLoad = GetMostSpecific(entry.TI_FirstLoadLRC, oldGroup?.FirstLoad),
			entry => NullIfEmpty(entry.TI_FirstLoadLRC)
		),
		new RateGroupingField((NoResString)"LastDischarge",
			group => group.LastDischarge,
			(newGroup, entry, oldGroup) => newGroup.LastDischarge = GetMostSpecific(entry.TI_LastDischargeLRC, oldGroup?.LastDischarge),
			entry => NullIfEmpty(entry.TI_LastDischargeLRC)
		),
		new RateGroupingField((NoResString)"FirstRouteSetLoad",
			group => group.FirstRouteSetLoad,
			(newGroup, entry, oldGroup) => newGroup.FirstRouteSetLoad = GetMostSpecific(entry.TI_FirstRouteSetLoadPortLRC, oldGroup?.FirstRouteSetLoad),
			entry => NullIfEmpty(entry.TI_FirstRouteSetLoadPortLRC)
		),
		new RateGroupingField((NoResString)"LastRouteSetDischarge",
			group => group.LastRouteSetDischarge,
			(newGroup, entry, oldGroup) => newGroup.LastRouteSetDischarge = GetMostSpecific(entry.TI_LastRouteSetDischargePortLRC, oldGroup?.LastRouteSetDischarge),
			entry => NullIfEmpty(entry.TI_LastRouteSetDischargePortLRC)
		),
		new RateGroupingField((NoResString)"IsContainerMapped",
			group => group.IsContainerMapped,
			(newGroup, entry, oldGroup) => newGroup.IsContainerMapped = (entry as UrsRateEntry)?.UrsContainer?.IsMapped.ToString() ?? oldGroup?.IsContainerMapped,
			entry => (entry as UrsRateEntry)?.UrsContainer?.IsMapped.ToString()
		),

		// Air only grouping fields
		new RateGroupingField((NoResString)"Deck",
			group => group.Deck,
			(newGroup, entry, oldGroup) => newGroup.Deck = GetMostSpecific((entry as WiseEntry)?.Deck, oldGroup?.Deck),
			entry => NullIfEmpty((entry as WiseEntry)?.Deck),
			Core.Constants.TransportModes.Air
		),
		new RateGroupingField((NoResString)"ProductCode",
			group => group.ProductCode,
			(newGroup, entry, oldGroup) => newGroup.ProductCode = GetMostSpecific((entry as WiseEntry)?.ProductCode, oldGroup?.ProductCode),
			entry => NullIfEmpty((entry as WiseEntry)?.ProductCode),
			Core.Constants.TransportModes.Air
		),
		new RateGroupingField((NoResString)"UniversalCommodityGroup",
			group => group.UniversalCommodityGroup,
			(newGroup, entry, oldGroup) => newGroup.UniversalCommodityGroup = GetMostSpecific((entry as WiseEntry)?.CommodityGroup, oldGroup?.UniversalCommodityGroup),
			entry => NullIfEmpty((entry as WiseEntry)?.CommodityGroup),
			Core.Constants.TransportModes.Air
		),

		// Sea only grouping fields
		new RateGroupingField((NoResString)"Vessel",
			group => group.Vessel,
			(newGroup, entry, oldGroup) => newGroup.Vessel = GetMostSpecific((entry as WiseEntry)?.Vessel, oldGroup?.Vessel),
			entry => NullIfEmpty((entry as WiseEntry)?.Vessel),
			Core.Constants.TransportModes.Sea
		),
		new RateGroupingField((NoResString)"Tradelane",
			group => group.Tradelane,
			(newGroup, entry, oldGroup) => newGroup.Tradelane = GetMostSpecific((entry as WiseEntry)?.Tradelane, oldGroup?.Tradelane),
			entry => NullIfEmpty((entry as WiseEntry)?.Tradelane),
			Core.Constants.TransportModes.Sea
		),
		new RateGroupingField((NoResString)"ServiceString",
			group => group.ServiceString,
			(newGroup, entry, oldGroup) => newGroup.ServiceString = GetMostSpecific((entry as WiseEntry)?.ServiceString, oldGroup?.ServiceString),
			entry => NullIfEmpty((entry as WiseEntry)?.ServiceString),
			Core.Constants.TransportModes.Sea
		),
		new RateGroupingField((NoResString)"Schedule",
			group => group.ScheduleId,
			(newGroup, entry, oldGroup) => newGroup.ScheduleId = GetMostSpecific((entry as UrsRateEntry)?.BookingInfo?.UrsSchedule.ScheduleId, oldGroup?.ScheduleId),
			entry => (entry as UrsRateEntry)?.BookingInfo?.UrsSchedule?.ScheduleId,
			Core.Constants.TransportModes.Sea
		)
	];

	static string? NullIfEmpty(IZType? value) => value == null || value.IsEmpty ? null : value.ToString();

	static string? GetMostSpecific(ZString? entryValue, string? previousValue) => NullIfEmpty(entryValue) ?? previousValue;

	static OrganisationDto? ToOrganisationDto(OrgHeader? header) => header != null ? new OrganisationDto(header) : null;

	static UrsCarrier? ToUrsCarrier(IRateEntry entry) => entry switch
	{
		UrsRateEntry ursEntry => ursEntry.UrsRatingHeader.ServiceProvider,
		_ => entry?.ParentRatingHeader?.Header is null ? null : UrsCarrier.FromOrg(entry?.ParentRatingHeader?.Header!)
	};

	string TransportMode { get; init; }

	public string RateSource { get; set; }

	public string? CarrierServiceLevel { get; set; }

	public string? ServiceLevel { get; set; }

	public string? UniversalServiceLevel { get; set; }

	public string? ContractNumber { get; set; }

	public OrganisationDto? Carrier { get; set; }

	public UrsCarrier? ServiceProvider { get; set; }

	public OrganisationDto? Consignee { get; set; }

	public OrganisationDto? Consignor { get; set; }

	public OrganisationDto? ControllingCustomer { get; set; }

	public string? Via { get; set; }

	public string? TransitTime { get; set; }

	public string? AircraftType { get; set; }

	public string? OriginPostCode { get; set; }

	public string? DestinationPostCode { get; set; }

	public Guid? PortTransportAddress { get; set; }

	public int? Frequency { get; set; }

	public string? FrequencyUnit { get; set; }

	public string? IsNonOperatedReefer { get; set; }

	public string? Deck { get; set; }

	public string? ProductCode { get; set; }

	public string? UniversalCommodityGroup { get; set; }

	public string? Vessel { get; set; }

	public string? Tradelane { get; set; }

	public string? ServiceString { get; set; }

	public string? RateId { get; set; }

	public string? FirstLoad { get; set; }

	public string? LastDischarge { get; set; }

	public string? FirstRouteSetLoad { get; set; }

	public string? LastRouteSetDischarge { get; set; }

	public string? ScheduleId { get; set; }

	public string? ContainerQuality { get; set; }

	public string? IsContainerMapped { get; set; }

	public List<IRateEntry> RateEntries { get; set; } = [];

	public RateGroup(string transportMode, IRateEntry entry, RateGroup? oldGroup = null)
	{
		TransportMode = transportMode;
		RateSource = entry is WiseEntry ? RateCurator.UrsRate : RateCurator.CargoWiseRate;

		RateEntries = oldGroup?.RateEntries.ToList() ?? [];
		RateEntries.Add(entry);

		if (entry is WiseEntry ursEntry)
		{
			RateId = oldGroup?.RateId ?? ursEntry.RateId;
			ContainerQuality = oldGroup?.ContainerQuality ??
				(!string.IsNullOrWhiteSpace(ursEntry.ContainerQuality) ? ursEntry.ContainerQuality : null);
		}

		// Set grouping fields
		rateGroupingFields
			.Where(field => field.TransportMode == null || field.TransportMode == TransportMode)
			.ForEach(field => field.Combine(this, entry, oldGroup));
	}

	public bool IsRateInGroup(IRateEntry entry) => RateEntries.Any(rate => rate.PK == entry.PK);

	public bool IsCompatibleWith(IRateEntry entry, bool isForRateSearch)
	{
		if (entry is WiseEntry && !RateEntries.Any(r => r.RateId == entry.RateId))
		{
			if (RateEntries.Any(r => IsSameContainerDetails(r, entry)))
			{
				return false;
			}
		}

		if (isForRateSearch && entry is WiseEntry ursEntry)
		{
			var containerQuality = string.IsNullOrWhiteSpace(ursEntry.ContainerQuality) ? null : ursEntry.ContainerQuality;
			if (containerQuality != ContainerQuality) {
				return false;
			}
		}

		return rateGroupingFields.All(field => field.IsCompatible(this, entry));
	}

	static bool IsSameContainerDetails(IRateEntry entry1, IRateEntry entry2) =>
		entry1.Container?.RC_Code == entry2.Container?.RC_Code &&
		entry1.TI_RH_NKCommodityCode == entry2.TI_RH_NKCommodityCode &&
		(entry1 as WiseEntry)?.ContainerQuality == (entry2 as WiseEntry)?.ContainerQuality;

	public bool IsMoreSpecificThan(IRateEntry entry) => rateGroupingFields.All(field => field.IsMoreSpecific(this, entry));

	public string GetKey() => string.Join("|", rateGroupingFields.Select(g => g.GetGroupValue(this))) + $"|{RateId}";

	public override int GetHashCode() => GetKey().GetHashCode();

	public override bool Equals(object? obj) => (obj is RateGroup) && GetHashCode() == obj.GetHashCode();

	internal class RateGroupingField
	{
		public string FieldName { get; }
		public string? TransportMode { get; }
		readonly Func<RateGroup, string?> getGroupValue;
		readonly Action<RateGroup, IRateEntry, RateGroup?> setGroupValue;
		readonly Func<IRateEntry, string?> getEntryValue;

		public RateGroupingField(string fieldName, Func<RateGroup, string?> getter, Action<RateGroup, IRateEntry, RateGroup?> setter, Func<IRateEntry, string?> entryGetter, string? transportMode = null)
		{
			FieldName = fieldName;
			TransportMode = transportMode;
			getGroupValue = getter;
			setGroupValue = setter;
			getEntryValue = entryGetter;
		}

		public void Combine(RateGroup newGroup, IRateEntry entry, RateGroup? oldGroup) => setGroupValue(newGroup, entry, oldGroup);

		public bool IsCompatible(RateGroup group, IRateEntry entry)
		{
			var groupValue = getGroupValue(group);
			var entryValue = getEntryValue(entry);
			return groupValue == null || entryValue == null || Equals(groupValue, entryValue);
		}

		public string? GetGroupValue(RateGroup group) => getGroupValue(group);

		public bool IsMoreSpecific(RateGroup group, IRateEntry entry) => getGroupValue(group) != null || getEntryValue(entry) == null;
	}
}
