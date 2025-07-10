using System;
using System.Collections;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IOrgHeader : IOrganisationData, IBusiness
	{
		#region Most OH_ Properties from Auto Bizo

		ZString OH_Code { get; set; }
		ZString OH_FullName { get; set; }
		ZBool OH_IsActive { get; set; }
		ZBool OH_IsAirCTO { get; set; }
		ZBool OH_IsAirLine { get; set; }
		ZBool OH_IsAirWholesaler { get; set; }
		ZBool OH_IsBroker { get; set; }
		ZBool OH_IsCompetitor { get; set; }
		ZBool OH_IsConsignee { get; set; }
		ZBool OH_IsConsignor { get; set; }
		ZBool OH_IsContainerYard { get; set; }
		ZBool OH_IsUserFlag1 { get; set; }
		ZBool OH_IsUserFlag2 { get; set; }
		ZBool OH_IsUserFlag3 { get; set; }
		ZBool OH_IsUserFlag4 { get; set; }
		ZBool OH_IsUserFlag5 { get; set; }
		ZBool OH_IsUserFlag6 { get; set; }
		ZBool OH_IsUserFlag7 { get; set; }
		ZBool OH_IsUserFlag8 { get; set; }
		ZBool OH_IsUserFlag9 { get; set; }
		ZBool OH_IsUserFlag10 { get; set; }
		ZBool OH_IsUserFlag11 { get; set; }
		ZBool OH_IsUserFlag12 { get; set; }
		ZBool OH_IsUserFlag13 { get; set; }
		ZBool OH_IsUserFlag14 { get; set; }
		ZBool OH_IsUserFlag15 { get; set; }
		ZBool OH_IsUserFlag16 { get; set; }
		ZBool OH_IsUserFlag17 { get; set; }
		ZBool OH_IsUserFlag18 { get; set; }
		ZBool OH_IsUserFlag19 { get; set; }
		ZBool OH_IsUserFlag20 { get; set; }
		ZBool OH_IsUserFlag21 { get; set; }
		ZBool OH_IsUserFlag22 { get; set; }
		ZBool OH_IsUserFlag23 { get; set; }
		ZBool OH_IsUserFlag24 { get; set; }
		ZBool OH_IsUserFlag25 { get; set; }
		ZBool OH_IsUserFlag26 { get; set; }
		ZBool OH_IsUserFlag27 { get; set; }
		ZBool OH_IsUserFlag28 { get; set; }
		ZBool OH_IsUserFlag29 { get; set; }
		ZBool OH_IsUserFlag30 { get; set; }
		ZBool OH_IsUserFlag31 { get; set; }
		ZBool OH_IsUserFlag32 { get; set; }
		ZBool OH_IsForwarder { get; set; }
		ZBool OH_IsFumigationContractor { get; set; }
		ZBool OH_IsGlobalAccount { get; set; }
		ZBool OH_IsInlandWaterwayProvider { get; set; }
		ZBool OH_IsLineHaulProvider { get; set; }
		ZBool OH_IsLocalTransport { get; set; }
		ZBool OH_IsMiscFreightServices { get; set; }
		ZBool OH_IsNationalAccount { get; set; }
		ZBool OH_IsPackDepot { get; set; }
		ZBool OH_IsPersonalEffectsAccount { get; set; }
		ZBool OH_IsRailHead { get; set; }
		ZBool OH_IsRailProvider { get; set; }
		ZBool OH_IsRoadFreightDepot { get; set; }
		ZBool OH_IsSalesLead { get; set; }
		ZBool OH_IsSeaCTO { get; set; }
		ZBool OH_IsSeaWholesaler { get; set; }
		ZBool OH_IsShippingConsortium { get; set; }
		ZBool OH_IsShippingLine { get; set; }
		ZBool OH_IsShippingProvider { get; set; }
		ZBool OH_IsTempAccount { get; set; }
		ZBool OH_IsTransportClient { get; set; }
		ZBool OH_IsUnpackDepot { get; set; }
		ZBool OH_IsWarehouseClient { get; set; }
		ZString OH_Language { get; set; }
		ZBool OH_OverrideAdditionalAddressInformation { get; set; }
		ZString OH_RL_NKClosestPort { get; set; }
		ZString OH_ScreeningStatus { get; set; }
		ZDateTime OH_SystemCreateTimeUtc { get; set; }
		ZString OH_SystemCreateUser { get; set; }
		ZDateTime OH_SystemLastEditTimeUtc { get; set; }
		ZString OH_SystemLastEditUser { get; set; }

		#endregion

		ZBool OH_IsDebtor { get; set; }
		ZBool OH_IsCreditor { get; set; }

		ZString Phone_Formatted { get; set; }
		ZString Fax_Formatted { get; set; }
		ZString Mobile_Formatted { get; set; }

		[SuppressWeaklyTypedCollectionMessage]
		IList Address_List { get; }

		ISimilarOrganisationsFinder SimilarOrgFinder { get; }

		event EventHandler AddressChanged;

		IOrgAddress MainAddress { get; }

		IOrgAddress CustomsAddress { get; }

		IOrgCompanyData CompanyData { get; }
	}
}
