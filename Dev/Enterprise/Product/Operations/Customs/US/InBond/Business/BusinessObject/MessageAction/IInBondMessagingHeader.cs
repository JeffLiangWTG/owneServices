using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.InBond.Messaging.Interface;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	public interface IInBondMessagingHeader : IInBondQPHeader, IInBondArriveExportTOLHeader, IInBondQXHeader, IInBondWXHeader, WarehouseExtensions.IInBondWarehouseIntegrationSupporter
	{
		bool HasClearDepartureAdd { get; }

		ZDateTime ArrivalDate { get; set; }
		ZPropertyInfo ArrivalDateInfo { get; }

		ZString ArrivalPort { get; }
		ZPropertyInfo ArrivalPortInfo { get; }
		ZZRefCusCodeListCombinedCollection ArrivalPortList { get; }

		ZDateTime ExportDate { get; set; }
		ZPropertyInfo ExportDateInfo { get; }

		ZString ExportPort { get; }
		ZPropertyInfo ExportPortInfo { get; }
		ZZRefCusCodeListCombinedCollection ExportPortList { get; }

		ZString ExportLadenOn { get; set; }
		ZPropertyInfo ExportLadenOnInfo { get; }
		RefVesselCollection ExportLadenOnList { get; }

		ZString ExportTransportMode { get; set; }
		ZPropertyInfo ExportTransportModeInfo { get; }
		CodeDescriptionPairList ExportTransportModeList { get; }

		ZDateTime TOLDate { get; set; }
		ZPropertyInfo TOLDateInfo { get; }

		ZString TOLCarrierCode { get; set; }
		ZPropertyInfo TOLCarrierCodeInfo { get; }
		USCarrierCombinedCollection TOLCarrierCodeList { get; }

		ZString TOLCarrierID { get; set; }
		ZPropertyInfo TOLCarrierIDInfo { get; }

		ZString TOLCityName { get; set; }
		ZPropertyInfo TOLCityNameInfo { get; }

		ZString TOLStateCode { get; set; }
		ZPropertyInfo TOLStateCodeInfo { get; }
		CodeDescriptionPairList TOLStateCodeList { get; }

		ZString FIRMSCode { get; set; }
		ZPropertyInfo FIRMSCodeInfo { get; }

		ZBool ShouldSend { get; set; }

		bool IsMoveToFTZRequiredAndItIsEmpty { get; }

		ZString WarehouseAddressDetail { get; }
		bool HasAtLeastOneCommodityWithProduct { get; }
		bool HasAtLeastOneCommodityWithoutProduct { get; }
		bool HasAtLeastOneCommodityWithProductWithoutInvoiceQuantity { get; }
		bool HasAtLeastOneCommodityWithProductWithoutProperEntryDetails { get; }
		bool HasAtLeastOneDetail { get; }
		bool IsWarehouseAddressOutsideOfHeaderCountry { get; }
		string GetWarehouseShouldBeInsideHeaderCountryMessage();

		void ValidateAll();
		void AllocateInBondNumberIfNeeded();
		bool LockInBondNumberAllocationMutex();
		void UnLockInBondNumberAllocationMutex();
		bool InBondNumberAllocationMutexHasLock();
		string GetInBondNumberAllocationMutexLockInfo();
		void ReloadInBondNumber();

		ZBool IsPostDepartureMessageOnly { get; }

		OrgAddressCollection InBondCarriers { get; }
		ShippingProviderCollection ShippingProviders { get; }
		ZGuid InBondCarrierPK { get; }
	}
}
