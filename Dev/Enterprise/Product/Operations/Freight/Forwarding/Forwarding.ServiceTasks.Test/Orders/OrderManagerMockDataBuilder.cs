using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Testing.Orders
{
	class OrderManagerMockDataBuilder
	{
		readonly List<MockItem> items = new List<MockItem>();
		readonly BusinessObjectFactory factory;
		readonly IDictionary<string, string> defaultSetting;

		public OrderManagerMockDataBuilder(BusinessObjectFactory factory)
		{
			this.factory = factory;
			this.defaultSetting = new Dictionary<string, string>();
		}

		public OrderManagerMockDataBuilder(BusinessObjectFactory factory, IDictionary<string, string> defaultSetting)
		{
			this.factory = factory;
			this.defaultSetting = defaultSetting;
		}

		public ForwardingConsol BuildConsol(string key, IDictionary<string, string> dataObject = null)
		{
			return MatchOrCreate(key, () =>
			{
				var row = dataObject ?? new Dictionary<string, string>();
				var consol = factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_UniqueConsignRef = key;
				consol.JK_TransportMode = GetOrDefaultStringCell(row, nameof(consol.JK_TransportMode), Core.Constants.TransportModes.Sea);
				consol.JK_RL_NKLoadPort = GetOrDefaultStringCell(row, nameof(consol.JK_RL_NKLoadPort), "CNSHA");
				consol.JK_RL_NKDischargePort = GetOrDefaultStringCell(row, nameof(consol.JK_RL_NKDischargePort), "AUSYD");

				return consol;
			});
		}

		public ForwardingShipment BuildShipment(string key, string consolKey = null, IDictionary<string, string> dataObject = null)
		{
			return MatchOrCreate(key, () =>
			{
				var row = dataObject ?? new Dictionary<string, string>();
				var shipment = factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_UniqueConsignRef = key;
				if (consolKey != null)
				{
					shipment.Consols.Add(MatchExistedEntity<ForwardingConsol>(consolKey));
				}
				return shipment;
			});
		}

		public ForwardingContainer BuildContainer(string key, ZString consolKey, ZString supplierBookingKey, ZString containerLoadPlanKey, IDictionary<string, string> dataObject = null)
		{
			return MatchOrCreate(key, () =>
			{
				var row = dataObject ?? new Dictionary<string, string>();
				var consol = MatchExistedEntity<ForwardingConsol>(consolKey);
				var container = consol.Containers.AddNew();
				container.JC_JSB_SupplierBooking = !string.IsNullOrEmpty(supplierBookingKey) ? MatchExistedEntity<JobSupplierBooking>(supplierBookingKey).PK : ZGuid.Empty;
				container.JC_CLH_LoadListPlan = !string.IsNullOrEmpty(containerLoadPlanKey) ? MatchExistedEntity<CFSContainerLoadList>(containerLoadPlanKey).PK : ZGuid.Empty;
				container.JC_ContainerNum = key;

				return container;
			});
		}

		public ContainerLoadListLine BuildContainerLoadListLine(ZString key, ZString containerLoadListKey, ZString supplierBookingLineKey, ZString containerKey, IDictionary<string, string> dataObject = null)
		{
			return MatchOrCreate(key, () =>
			{
				var row = dataObject ?? new Dictionary<string, string>();
				var containerLoadList = (MatchExistedEntity<CYContainerLoadList>(containerLoadListKey) as CommonContainerLoadList) ?? MatchExistedEntity<CFSContainerLoadList>(containerLoadListKey);
				var containerLoadListLine = containerLoadList.LoadListLines.AddNew();
				containerLoadListLine.CLL_LoadMode = Core.Constants.ContainerLoadListHeaderLoadMode.ContainerYard;
				containerLoadListLine.CLL_JC_Container = MatchExistedEntity<ForwardingContainer>(containerKey)?.PK ?? ZGuid.Empty;
				containerLoadListLine.CLL_JSL_BookingLine = MatchExistedEntity<JobSupplierBookingLine>(supplierBookingLineKey).PK;
				containerLoadListLine.CLL_Volume = decimal.Parse(GetOrDefaultStringCell(row, nameof(containerLoadListLine.CLL_Volume), "0"));
				containerLoadListLine.CLL_VolumeUnit = GetOrDefaultStringCell(row, nameof(containerLoadListLine.CLL_VolumeUnit), Core.Constants.Volume.CubicMetres);
				containerLoadListLine.CLL_Weight = decimal.Parse(GetOrDefaultStringCell(row, nameof(containerLoadListLine.CLL_Weight), "0"));
				containerLoadListLine.CLL_WeightUnit = GetOrDefaultStringCell(row, nameof(containerLoadListLine.CLL_WeightUnit), Core.Constants.Weight.Kilograms);
				containerLoadListLine.CLL_PackedQuantity = decimal.Parse(GetOrDefaultStringCell(row, nameof(containerLoadListLine.CLL_PackedQuantity), "0"));
				containerLoadListLine.CLL_Packages = int.Parse(GetOrDefaultStringCell(row, nameof(containerLoadListLine.CLL_Packages), "0"));
				containerLoadListLine.CLL_F3_NKPackagesUnit = GetOrDefaultStringCell(row, nameof(containerLoadListLine.CLL_F3_NKPackagesUnit), Core.Constants.PkgUnit.Package);
				containerLoadListLine.CLL_LoadSequence = int.Parse(GetOrDefaultStringCell(row, nameof(containerLoadListLine.CLL_LoadSequence), "0"));
				return containerLoadListLine;
			});
		}

		public CYContainerLoadList BuildContainerLoadList(ZString key, ZString supplierBookingKey, string loadListPartyCode = "PartyOrg", string status = Core.Constants.ContainerLoadListHeaderStatus.Incomplete, IDictionary<string, string> dataObject = null)
		{
			return MatchOrCreate(key, () =>
			{
				var row = dataObject ?? new Dictionary<string, string>();
				var containerLoadList = factory.NewWithValidTestData<CYContainerLoadList>();
				containerLoadList.CLH_LoadListId = key;
				containerLoadList.CLH_Status = status;
				containerLoadList.CLH_JSB_Booking = MatchExistedEntity<JobSupplierBooking>(supplierBookingKey).PK;
				containerLoadList.CLH_OH_LoadListParty = GetOrCreateOrg(loadListPartyCode).PK;

				return containerLoadList;
			});
		}

		public CFSContainerLoadList BuildContainerLoadPlan(ZString key, string cfsOrgCode = "CFSOrg", string controllingCustomerOrgCode = "CUS001", string status = Core.Constants.ContainerLoadListHeaderStatus.Incomplete, IDictionary<string, string> dataObject = null)
		{
			return MatchOrCreate(key, () =>
			{
				var row = dataObject ?? new Dictionary<string, string>();
				var containerLoadList = factory.NewWithValidTestData<CFSContainerLoadList>();
				containerLoadList.CLH_LoadListId = key;
				containerLoadList.CLH_Status = status;
				containerLoadList.CLH_OA_CFSAddress = GetOrCreateOrg(cfsOrgCode).MainAddress.PK;
				CreateDocAddress(containerLoadList.ControllingCustomerAddress, row, nameof(containerLoadList.ControllingCustomerAddress));
				return containerLoadList;
			});
		}

		public JobSupplierBookingLine BuildSupplierBookingLine(ZString key, ZString supplierBookingKey, ZString supplierBookingLinegroup, ZString orderLineKey, IDictionary<string, string> dataObject = null)
		{
			return MatchOrCreate(key, () =>
			{
				var row = dataObject ?? new Dictionary<string, string>();
				var supplierBooking = MatchExistedEntity<JobSupplierBooking>(supplierBookingKey);
				var supplierBookingLine = supplierBooking.SupplierBookingLines.AddNew();
				supplierBookingLine.JSL_BookingLineId = key;
				supplierBookingLine.JSL_JO_OrderLine = MatchExistedEntity<OrderLine>(orderLineKey).PK;

				supplierBookingLine.JSL_BookedQuantity = decimal.Parse(GetOrDefaultStringCell(row, nameof(supplierBookingLine.JSL_BookedQuantity), "0"));
				supplierBookingLine.JSL_BookedPackages = decimal.Parse(GetOrDefaultStringCell(row, nameof(supplierBookingLine.JSL_BookedPackages), "0"));
				supplierBookingLine.JSL_GrossWeight = decimal.Parse(GetOrDefaultStringCell(row, nameof(supplierBookingLine.JSL_GrossWeight), "0"));
				supplierBookingLine.JSL_Volume = decimal.Parse(GetOrDefaultStringCell(row, nameof(supplierBookingLine.JSL_Volume), "0"));
				supplierBookingLine.JSL_BookedPackages = decimal.Parse(GetOrDefaultStringCell(row, nameof(supplierBookingLine.JSL_BookedPackages), "0"));
				supplierBookingLine.JSL_DispatchedVolume = decimal.Parse(GetOrDefaultStringCell(row, nameof(supplierBookingLine.JSL_DispatchedVolume), "0"));
				supplierBookingLine.JSL_VolumeUnit = GetOrDefaultStringCell(row, nameof(supplierBookingLine.JSL_VolumeUnit), Core.Constants.Volume.CubicMetres);
				supplierBookingLine.JSL_DispatchedWeight = decimal.Parse(GetOrDefaultStringCell(row, nameof(supplierBookingLine.JSL_DispatchedWeight), "0"));
				supplierBookingLine.JSL_GrossWeightUnit = GetOrDefaultStringCell(row, nameof(supplierBookingLine.JSL_GrossWeightUnit), Core.Constants.Weight.Kilograms);
				supplierBookingLine.JSL_DispatchedPackages = int.Parse(GetOrDefaultStringCell(row, nameof(supplierBookingLine.JSL_DispatchedPackages), "0"));
				supplierBookingLine.JSL_F3_NKBookedPackagesUnit = GetOrDefaultStringCell(row, nameof(supplierBookingLine.JSL_F3_NKBookedPackagesUnit), Core.Constants.PkgUnit.Package);
				supplierBookingLine.JSL_DispatchedQuantity = decimal.Parse(GetOrDefaultStringCell(row, nameof(supplierBookingLine.JSL_DispatchedQuantity), "0"));
				supplierBookingLine.JSL_RH_NKCommodityCode = GetOrDefaultStringCell(row, nameof(supplierBookingLine.JSL_RH_NKCommodityCode), "GEN");
				supplierBookingLine.JSL_MarksAndNumbers = GetOrDefaultStringCell(row, nameof(supplierBookingLine.JSL_MarksAndNumbers), "test marks & numbers");

				return supplierBookingLine;
			});
		}

		public JobSupplierBookingLine BuildSupplierBookingLine(ZString key, ZString supplierBookingKey, ZString supplierBookingLinegroup, ZString orderLineKey, decimal quantity, int package = 0, decimal volumne = 0, decimal weight = 0)
		{
			var bookingLine = BuildSupplierBookingLine(key, supplierBookingKey, supplierBookingLinegroup, orderLineKey);
			bookingLine.JSL_BookedQuantity = quantity;
			bookingLine.JSL_BookedPackages = package;
			bookingLine.JSL_Volume = volumne;
			bookingLine.JSL_GrossWeight = weight;

			return bookingLine;
		}

		public JobSupplierBooking BuildSupplierBooking(ZString key, string status = Core.Constants.OrderStatus.Incomplete, IDictionary<string, string> dataObject = null)
		{
			return MatchOrCreate(key, () =>
			{
				var row = dataObject ?? new Dictionary<string, string>();
				var supplierBooking = factory.NewWithValidTestData<JobSupplierBooking>();
				supplierBooking.JSB_BookingId = key;
				supplierBooking.JSB_TransportMode = GetOrDefaultStringCell(row, nameof(supplierBooking.JSB_TransportMode), Core.Constants.TransportModes.Sea);
				supplierBooking.JSB_LoadMode = GetOrDefaultStringCell(row, nameof(supplierBooking.JSB_LoadMode), Core.Constants.SupplierBookingLoadMode.ContainerYard);
				supplierBooking.JSB_Status = status;

				supplierBooking.JSB_IncoTerm = GetOrDefaultStringCell(row, JobSupplierBookingSchema.Constants.JSB_IncoTerm, Core.Constants.IncoTerms.ExWorks);
				supplierBooking.JSB_RL_NKLoadPort = GetOrDefaultStringCell(row, JobSupplierBookingSchema.Constants.JSB_RL_NKLoadPort, "CNSHA");
				supplierBooking.JSB_RL_NKDischargePort = GetOrDefaultStringCell(row, JobSupplierBookingSchema.Constants.JSB_RL_NKDischargePort, "AUSYD");
				supplierBooking.JSB_RL_NKOrigin = GetOrDefaultStringCell(row, JobSupplierBookingSchema.Constants.JSB_RL_NKOrigin, "CNSHA");
				supplierBooking.JSB_RL_NKDestination = GetOrDefaultStringCell(row, JobSupplierBookingSchema.Constants.JSB_RL_NKDestination, "AUSYD");
				supplierBooking.JSB_MarksAndNumbers = GetOrDefaultStringCell(row, JobSupplierBookingSchema.Constants.JSB_MarksAndNumbers, "");
				supplierBooking.JSB_GoodsDescription = GetOrDefaultStringCell(row, JobSupplierBookingSchema.Constants.JSB_GoodsDescription, "");
				supplierBooking.JSB_DetailedGoodsDescription = GetOrDefaultStringCell(row, nameof(supplierBooking.JSB_DetailedGoodsDescription), "");

				CreateDocAddress(supplierBooking.ControllingCustomerAddress, row, nameof(supplierBooking.ControllingCustomerAddress));
				CreateDocAddress(supplierBooking.SupplierAddress, row, nameof(supplierBooking.SupplierAddress));
				return supplierBooking;
			});
		}

		public OrderLine BuildOrderLine(ZString key, ZString orderKey, string lineStatus = Core.Constants.OrderStatus.Incomplete, IDictionary<string, string> dataObject = null)
		{
			return MatchOrCreate(key, () =>
			{
				var row = dataObject ?? new Dictionary<string, string>();
				var order = MatchExistedEntity<Order>(orderKey);
				var orderLine = order.OrderLines.AddNew();
				orderLine.JO_LineStatus = lineStatus;
				orderLine.JO_ItemPrice = decimal.Parse(GetOrDefaultStringCell(row, nameof(orderLine.JO_ItemPrice), "1"));
				orderLine.JO_Quantity = decimal.Parse(GetOrDefaultStringCell(row, nameof(orderLine.JO_Quantity), "1"));
				orderLine.JO_Partno = GetOrDefaultStringCell(row, nameof(orderLine.JO_Partno), "");
				orderLine.JO_F3_NKPackType = GetOrDefaultStringCell(row, nameof(orderLine.JO_F3_NKPackType), Core.Constants.PkgUnit.Package);
				orderLine.JO_Description = GetOrDefaultStringCell(row, nameof(orderLine.JO_Description), "");
				orderLine.JO_AdditionalInformation = GetOrDefaultStringCell(row, nameof(orderLine.JO_AdditionalInformation), "");

				return orderLine;
			});
		}

		public Order BuildOrder(ZString key, string orderStatus = Core.Constants.OrderStatus.Incomplete, IDictionary<string, string> dataObject = null)
		{
			return MatchOrCreate(key, () =>
			{
				var row = dataObject ?? new Dictionary<string, string>();
				var order = factory.NewWithValidTestData<Order>();
				order.JD_OrderNumber = key;
				order.JD_TransportMode = GetOrDefaultStringCell(row, nameof(order.JD_TransportMode), Core.Constants.TransportModes.Sea);
				order.JD_OrderStatus = orderStatus;
				var buyer = GetOrDefaultOrgCell(row, nameof(order.BuyerAddress));
				order.JD_OA_BuyerAddress = buyer.MainAddress.PK;
				order.JD_OC_BuyerContact = buyer.Contacts[0].PK;

				var supplier = GetOrDefaultOrgCell(row, nameof(order.SupplierAddress));
				order.JD_OA_SupplierAddress = supplier.MainAddress.PK;
				order.JD_OC_SupplierContact = supplier.Contacts[0].PK;

				CreateDocAddress(order.ControllingCustomerDocAddress, row, nameof(order.ControllingCustomerDocAddress));
				CreateDocAddress(order.NotifyPartyDocAddress, row, nameof(order.NotifyPartyDocAddress));
				CreateDocAddress(order.NotifyParty2DocAddress, row, nameof(order.NotifyParty2DocAddress));
				CreateDocAddress(order.NotifyParty3DocAddress, row, nameof(order.NotifyParty3DocAddress));
				CreateDocAddress(order.NotifyParty3DocAddress, row, nameof(order.NotifyParty3DocAddress));
				CreateDocAddress(order.GoodsDeliveredToAddress, row, nameof(order.GoodsDeliveredToAddress));
				CreateDocAddress(order.GoodsAvailableAtAddress, row, nameof(order.GoodsAvailableAtAddress));
				CreateDocAddress(order.ConsigneeDocumentaryAddress, row, nameof(order.ConsigneeDocumentaryAddress));
				CreateDocAddress(order.ControllingAgentDocAddress, row, nameof(order.ControllingAgentDocAddress));

				return order;
			});
		}

		T MatchOrCreate<T>(ZString key, Func<T> buildEntityFunc) where T : BusinessObject
		{
			var existedEntity = MatchExistedEntity<T>(key);
			if (existedEntity != null)
			{
				return existedEntity;
			}

			var newEntity = buildEntityFunc();
			items.Add(new MockItem { Entity = newEntity, EntityKey = key });
			return newEntity;
		}

		string GetOrDefaultStringCell(IDictionary<string, string> row, string fieldName, string defaultValue = "")
		{
			if (row.ContainsKey(fieldName))
			{
				return row[fieldName];
			}

			if (defaultSetting.TryGetValue(fieldName, out var valueFromDefaultSetting))
			{
				return valueFromDefaultSetting;
			}

			return defaultValue;
		}

		OrgHeader GetOrDefaultOrgCell(IDictionary<string, string> row, string fieldName)
		{
			if (row.ContainsKey(fieldName))
			{
				return GetOrCreateOrg(row[fieldName]);
			}

			return WrapWithContact(factory.NewWithValidTestData<OrgHeader>());
		}

		JobDocAddress CreateDocAddress(JobDocAddress docAddress, IDictionary<string, string> row, string fieldName)
		{
			var org = GetOrDefaultOrgCell(row, fieldName);
			docAddress.OrganisationPK = org.PK;
			docAddress.E2_OA_Address = org.MainAddress.PK;

			return docAddress;
		}

		OrgHeader GetOrCreateOrg(string orgCode)
		{
			var found = MatchExistedEntity<OrgHeader>(orgCode);
			if (found != null)
			{
				return found;
			}

			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = orgCode;
			return WrapWithContact(org);
		}

		OrgHeader WrapWithContact(OrgHeader org)
		{
			org.Contacts.AddNew().FillWithValidTestData();
			items.Add(new MockItem { Entity = org, EntityKey = org.OH_Code });
			return org;
		}

		public T MatchExistedEntity<T>(ZString key) where T : BusinessObject
		{
			return items.SingleOrDefault(item => item.Entity is T && item.EntityKey == key)?.Entity as T;
		}

		class MockItem
		{
			public ZString EntityKey { get; set; }
			public BusinessObject Entity { get; set; }
		}
	}
}
