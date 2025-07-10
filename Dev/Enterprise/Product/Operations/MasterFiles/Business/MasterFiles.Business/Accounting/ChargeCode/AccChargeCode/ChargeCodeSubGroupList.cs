using System;
using CargoWise.Application;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	#region FreightServiceTypes

	public class FreightServiceTypes : CodeDescriptionPairList, Integration.IFreightServiceTypes
	{
		protected ZGuid CompanyPK;

		public FreightServiceTypes()
			: this(Env.CurrentCompany.PK)
		{
		}

		public FreightServiceTypes(ZGuid companyPK)
		{
			CompanyPK = companyPK;

			if (AddFreightServiceTypesOnConstruction)
			{
				AddFreightServiceTypes();
			}
		}

		#region Adding Freight Service Types

		protected void AddFreightServiceTypes()
		{
			AddServiceTypesFromRegistry(FreightDataRegistry.Instance.JobServices.GetFallBackValueAtAllLevels(CompanyPK.IsEmpty ? Guid.Empty : CompanyPK.ToGuid(), Guid.Empty, Guid.Empty).GetCodeDescriptionPairList());
		}

		void AddServiceTypesFromRegistry(ReadOnlyCodeDescriptionPairList registryList)
		{
			foreach (ICodeDescription pair in registryList)
			{
				AddPairIfNotExist(pair.Code, ((IMultilingualDescription)pair).MultilingualDescription);
			}
		}

		protected virtual bool AddFreightServiceTypesOnConstruction
		{
			get { return true; }
		}

		#endregion
	}

	#endregion

	#region ChargeCodeSubGroupList

	/// <summary>
	/// Provides a list of allowed Accounting Charge Code Sub-groups.
	/// </summary>
	public class ChargeCodeSubGroupList : FreightServiceTypes
	{
		#region Constants

		public const string Storage = "STG";
		public const string CarrierStorage = "STC";
		public const string Cod = "COD";
		public const string Labor = "LBR";
		public const string CartageDemurrageTotal = "DME";
		public const string CartageBeyondPostcode = "BPC";
		public const string ContainerDetention = "DTN";
		public const string MergedDemurrageDetention = "MDD";
		public const string PackingCharges = "PCK";
		public const string UnpackingCharges = "UNP";
		public const string PrincipalDetention = "PRC";
		public const string OverweightPenalty = "OWP";
		public const string OverweightSurcharge = "OWS";

		#endregion

		#region GetList

		public static ChargeCodeSubGroupList GetList(string chargeGroup)
		{
			return GetList(chargeGroup, Env.CurrentCompany.PK);
		}

		public static ChargeCodeSubGroupList GetList(string chargeGroup, ZGuid companyPK)
		{
			return new ChargeCodeSubGroupList(chargeGroup, companyPK);
		}

		ChargeCodeSubGroupList(ZString chargeGroup, ZGuid companyPK)
			: base(companyPK)
		{
			switch (chargeGroup)
			{
				case ChargeCodeGroupList.Codes.Transport:
					AddTransportSubGroups();
					break;

				case ChargeCodeGroupList.Codes.ShippingDisbursements:
					AddShippingDisbursementsSubGroups();
					break;

				case ChargeCodeGroupList.Codes.CFSLoadList:
					AddCFSLoadListSubGroups();
					break;

				case ChargeCodeGroupList.Codes.CFSShipment:
					AddCFSShipmentSubGroups();
					break;

				case ChargeCodeGroupList.Codes.WHSInwards:
				case ChargeCodeGroupList.Codes.WHSOutwards:
				case ChargeCodeGroupList.Codes.WHSAdHocServiceJob:
					AddProductWarehouseSubGroups();
					break;

				case ChargeCodeGroupList.Codes.TRWReceive:
				case ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit:
				case ChargeCodeGroupList.Codes.TRWDispatch:
				case ChargeCodeGroupList.Codes.TRWDispatchLoadList:
				case ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit:
					AddTransitWarehouseSubGroups();
					break;

				case ChargeCodeGroupList.Codes.ContainerStorage:
					AddContainerStorageSubGroups();
					break;

				case ChargeCodeGroupList.Codes.Origin:
				case ChargeCodeGroupList.Codes.OriginBrokerage:
				case ChargeCodeGroupList.Codes.OriginBrokerageOnly:
				case ChargeCodeGroupList.Codes.Destination:
				case ChargeCodeGroupList.Codes.Brokerage:
				case ChargeCodeGroupList.Codes.BrokerageOnly:
					AddBrokerageSubGroups(chargeGroup);
					break;

				case ChargeCodeGroupList.Codes.TransportBooking:
					AddTransportBookingSubGroups();
					break;
				default:
					break;
			}
		}

		#region AddTransportSubGroups

		void AddTransportSubGroups()
		{
			AddCartageDemurragePenalty();
			var consignmentListLoader = ObjectFactory.Get<IConsignmentListLoader>();
			AddRange(consignmentListLoader.GetTransportJobServices());
		}

		void AddCartageDemurragePenalty()
			=> AddPair(CartageDemurrageTotal, ResString.GetMultilingualString("aeb0f017-957f-443e-8658-652e95d55a3c", "Truck Wait Time (Penalty Type - TWT)"));

		void AddStoragePenalty()
		{
			AddPair(Storage, ResString.GetMultilingualString("28C46903-F75C-439C-8008-3ACAAC005CE4", "CTO Storage (Penalty Type - STO)"));
			AddPair(CarrierStorage, ResString.GetMultilingualString("17060905-385E-4C4C-9BF1-721A25C577CE", "Carrier Storage / Demurrage (Penalty Type - STO)"));
		}

		#endregion

		#region AddTransportBookingSubGroups

		void AddTransportBookingSubGroups()
		{
			var consignmentListLoader = ObjectFactory.Get<IConsignmentListLoader>();
			AddRange(consignmentListLoader.GetTransportBookingJobServices());
		}

		#endregion

		#region ShippingDisbursements

		void AddShippingDisbursementsSubGroups()
		{
			AddRange(LinerAgencyDataRegistry.Instance.DisbursementSubGroups.Value);
		}

		#endregion

		#region CFSLoadList

		void AddCFSLoadListSubGroups()
		{
			AddPair(PackingCharges, ResString.GetMultilingualString("ee07b261-4b8f-4058-89c3-40d3fd45799d", "Packing Charges"));
			AddPair(UnpackingCharges, ResString.GetMultilingualString("22631b3d-db2e-4dd8-9581-a062fca47378", "Unpacking Charges"));
			AddPair(Storage, ResString.GetMultilingualString("2f216ec7-a144-448b-a97c-c7bd3ecd4e02", "Storage"));
			AddFreightServiceTypes();
		}

		#endregion

		#region CFSShipment

		void AddCFSShipmentSubGroups()
		{
			AddPair(Storage, ResString.GetMultilingualString("2f216ec7-a144-448b-a97c-c7bd3ecd4e02", "Storage"));
			AddFreightServiceTypes();
		}

		#endregion

		#region ProductWarehouse

		void AddProductWarehouseSubGroups()
		{
			AddPair(Storage, ResString.GetMultilingualString("2f216ec7-a144-448b-a97c-c7bd3ecd4e02", "Storage"));
			AddFreightServiceTypes();
			var jobServicesCodePairs = WarehouseDataRegistry.Instance.JobServices.GetFallBackValueAtAllLevels(CompanyPK.IsEmpty ? Guid.Empty : CompanyPK.ToGuid(), Guid.Empty, Guid.Empty);
			foreach (SystemDefinableCodeDescriptionBool codeDescriptionBool in jobServicesCodePairs)
			{
				AddPairIfNotExist(codeDescriptionBool.Code, codeDescriptionBool.Description);
			}
		}

		#endregion

		#region TransitWarehouse

		void AddTransitWarehouseSubGroups()
		{
			AddFreightServiceTypes();
			var jobServicesCodePairs = WarehouseDataRegistry.Instance.JobServices.GetFallBackValueAtAllLevels(CompanyPK.IsEmpty ? Guid.Empty : CompanyPK.ToGuid(), Guid.Empty, Guid.Empty);
			foreach (SystemDefinableCodeDescriptionBool codeDescriptionBool in jobServicesCodePairs)
			{
				AddPairIfNotExist(codeDescriptionBool.Code, codeDescriptionBool.Description);
			}
		}

		#endregion

		#region ContainerStorage

		void AddContainerStorageSubGroups()
		{
			AddPair(Core.Constants.FreightServiceType.Codes.FCLContainerStorage, ResString.GetMultilingualString("3f8e5b94-9d1d-4eb6-9c2f-bcaa860e03c2", "Free Storage"));
			AddPair(Core.Constants.FreightServiceType.Codes.FCLUnderbondStorage, ResString.GetMultilingualString("38541ea7-666d-41f3-897c-697444a5f22d", "Bonded Storage"));
		}

		#endregion

		#region Brokerage

		void AddBrokerageSubGroups(ZString chargeGroup)
		{
			AddPair(Labor, ResString.GetMultilingualString("2b2c664c-fba1-4446-8aab-878124540a94", "Labor"));
			AddCartageDemurragePenalty();
			AddPair(CartageBeyondPostcode, ResString.GetMultilingualString("42415570-98b4-4cf9-bff5-533c90c0a309", "Port Transport Beyond Postcode"));

			if (chargeGroup == ChargeCodeGroupList.Codes.Destination ||
				chargeGroup == ChargeCodeGroupList.Codes.Brokerage ||
				chargeGroup == ChargeCodeGroupList.Codes.BrokerageOnly)
			{
				AddPair(Cod, ResString.GetMultilingualString("6e2456cd-ecdc-4a98-9a46-8d38111b6d3e", "COD Fee"));
			}

			if (chargeGroup == ChargeCodeGroupList.Codes.Destination ||
				chargeGroup == ChargeCodeGroupList.Codes.Brokerage ||
				chargeGroup == ChargeCodeGroupList.Codes.BrokerageOnly ||
				chargeGroup == ChargeCodeGroupList.Codes.Origin)
			{
				AddStoragePenalty();
				AddPair(ContainerDetention, ResString.GetMultilingualString("00bcc209-5c07-419f-b8d9-f73bcfb8681b", "Detention (Penalty Type - DET)"));
			}

			if (chargeGroup == ChargeCodeGroupList.Codes.Destination ||
				chargeGroup == ChargeCodeGroupList.Codes.Origin)
			{
				AddPair(MergedDemurrageDetention, ResString.GetMultilingualString("D9D4398C-F63D-4031-A347-F7C4CA277D63", "Merged Demurrage & Detention"));
				AddPair(PrincipalDetention, ResString.GetMultilingualString("54f253f3-fdd9-4675-84da-9b7b7d4a7aa5", "Principal Detention"));
				AddPair(OverweightPenalty, ResString.GetMultilingualString("e53822f1-3b92-45e4-a4b2-d67c0a493d50", "Overweight Penalty"));
				AddPair(OverweightSurcharge, ResString.GetMultilingualString("a7caef4d-485b-4bc9-97c7-7d9dc91f0e84", "Overweight Surcharge"));
			}

			AddFreightServiceTypes();
		}

		#endregion

		#endregion

		protected override bool AddFreightServiceTypesOnConstruction
		{
			get { return false; }
		}
	}

	#endregion
}
