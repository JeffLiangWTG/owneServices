using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	public static class UniversalTestHelper
	{
		public static WhsWarehouse CreateWarehouse(OrgAddress warehouseAddress, UniversalObjectFactory factory)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory.BOFactory);
			var warehouse = (WhsWarehouse)helper.CreateWarehouse("Container Yard", "CYD", "A");
			warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			warehouse.WW_OA_WarehouseAddress = warehouseAddress.PK;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			factory.SaveForTesting();
			return warehouse;
		}

		public static WhsWarehouse GetWarehouse(OrgAddress warehouseAddress, UniversalObjectFactory factory)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory.BOFactory);
			var query = new ZQuery(WhsWarehouseSchema.WW_WarehouseName, "Container Yard");
			var warehouse = factory.LoadTop1<WhsWarehouse>(query);
			query.AddToFilter(WhsWarehouseSchema.WW_WarehouseCode, "CYD");
			query.AddToFilter(WhsWarehouseSchema.WW_GB_RelatedCompanyBranch, GlbBranch.CurrentBranch.PK);
			query.AddToFilter(WhsWarehouseSchema.WW_OA_WarehouseAddress, warehouseAddress.PK);
			query.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, WarehouseTypes.Codes.ContainerYard);
			return warehouse;
		}

		public static CYDReceiveAdvice CreateReceiveAdvice(UniversalObjectFactory factory, WhsWarehouse yard, string acceptanceNumber, ZDate fromDate, ZDate toDate)
		{
			var receiveAdvice = factory.NewWithValidTestData<CYDReceiveAdvice>();
			receiveAdvice.YRA_AcceptanceNumber = acceptanceNumber;
			receiveAdvice.YRA_WW_Yard = yard.PK;
			receiveAdvice.YRA_FromDate = fromDate;
			receiveAdvice.YRA_ToDate = toDate;
			factory.SaveForTesting();
			return receiveAdvice;
		}

		public static CYDReceiveAdviceLine CreateReceiveAdviceLine(UniversalObjectFactory factory, CYDReceiveAdvice receiveAdvice, string containerTypeCode)
		{
			var refContainer = factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, containerTypeCode));
			var receiveAdviceLine = factory.NewWithValidTestData<CYDReceiveAdviceLine>();
			receiveAdviceLine.YRL_YRA_ReceiveAdvice = receiveAdvice.PK;

			var unitLineItem = factory.New<CYDUnitLineItem>();
			unitLineItem.YLI_RC_ContainerType = refContainer.PK;
			unitLineItem.YLI_Quantity = 1;
			unitLineItem.YLI_Type = "CNT";
			unitLineItem.YLI_IsEmpty = true;
			receiveAdviceLine.YRL_YLI_UnitLineItem = unitLineItem.PK;

			factory.SaveForTesting();
			return receiveAdviceLine;
		}

		public static CYDReleaseAdvice CreateReleaseAdvice(UniversalObjectFactory factory, WhsWarehouse yard, string releaseNumber, ZDate fromDate, ZDate toDate)
		{
			var releaseAdvice = factory.NewWithValidTestData<CYDReleaseAdvice>();
			releaseAdvice.YRE_ReleaseNumber = releaseNumber;
			releaseAdvice.YRE_WW_Yard = yard.PK;
			releaseAdvice.YRE_FromDate = fromDate;
			releaseAdvice.YRE_ToDate = toDate;
			factory.SaveForTesting();
			return releaseAdvice;
		}

		public static CYDReleaseAdviceLine CreateReleaseAdviceLine(UniversalObjectFactory factory, CYDReleaseAdvice releaseAdvice, string containerTypeCode, short quantity = 1, bool isPreAdvice = false)
		{
			var refContainer = factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, containerTypeCode));
			var releaseAdviceLine = factory.NewWithValidTestData<CYDReleaseAdviceLine>();
			releaseAdviceLine.YEL_YRE_ReleaseAdvice = releaseAdvice.PK;

			var unitLineItem = factory.New<CYDUnitLineItem>();
			unitLineItem.YLI_RC_ContainerType = refContainer.PK;
			unitLineItem.YLI_Quantity = quantity;
			unitLineItem.YLI_Type = YardUnitType.Codes.Container;
			unitLineItem.YLI_IsPreAdvice = isPreAdvice;
			releaseAdviceLine.YEL_YLI_UnitLineItem = unitLineItem.PK;

			factory.SaveForTesting();
			return releaseAdviceLine;
		}

		public static CYDYardUnitState CreateYardUnitState(UniversalObjectFactory factory, WhsWarehouse yard, CYDReceiveAdviceLine receiveAdviceLine, string containerNumber, CYDReleaseAdviceLine releaseAdviceLine = null)
		{
			var yardUnit = factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnit.YUS_WW_CurrentYard = yard.PK;
			if (receiveAdviceLine != null)
			{
				yardUnit.YUS_YRL_ReceiveLine = receiveAdviceLine.PK;
			}
			yardUnit.YUS_UnitID = containerNumber;
			if (releaseAdviceLine != null)
			{
				yardUnit.YUS_YEL_ReleaseLine = releaseAdviceLine.PK;
			}
			factory.SaveForTesting();
			return yardUnit;
		}

		public static CYDTransportationUnit CreateTransportationUnit(UniversalObjectFactory factory, WhsWarehouse yard, string transportationReference)
		{
			var transportationUnit = factory.NewWithValidTestData<CYDTransportationUnit>();
			transportationUnit.YTU_WW_Yard = yard.PK;
			transportationUnit.YTU_TransportationReference = transportationReference;
			factory.SaveForTesting();
			return transportationUnit;
		}

		public static CYDUnitLineItem CreateUnitLineItem(UniversalObjectFactory factory, RefContainer containerType, string type, short quantity, bool isEmpty = true, string sealNumber = null)
		{
			var yardUnitLineItem = factory.New<CYDUnitLineItem>();
			yardUnitLineItem.YLI_RC_ContainerType = containerType.PK;
			yardUnitLineItem.YLI_Quantity = quantity;
			yardUnitLineItem.YLI_Type = type;
			yardUnitLineItem.YLI_IsEmpty = isEmpty;
			yardUnitLineItem.YLI_SealNumber = sealNumber;
			return yardUnitLineItem;
		}

		public static CYDDelivery CreateDelivery(UniversalObjectFactory factory, CYDReceiveAdviceLine receiveAdviceLine, CYDTransportationUnit transportationUnit, CYDYardUnitState yardUnit, string containerTypeCode, short quantity = 1, string transportReference = "")
		{
			var refContainer = factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, containerTypeCode));
			var delivery = factory.NewWithValidTestData<CYDDelivery>();
			var unitLine = factory.New<CYDUnitLineItem>();
			delivery.YDL_YRL_ReceiveAdviceLine = receiveAdviceLine.PK;
			delivery.YDL_YTU_DeliveryTransportationUnit = transportationUnit.PK;
			unitLine.YLI_Type = "CNT";
			unitLine.YLI_RC_ContainerType = refContainer.PK;
			unitLine.YLI_Quantity = quantity;
			delivery.YDL_YLI_UnitLineItem = unitLine.PK;
			delivery.YDL_TransportReference = transportReference;
			yardUnit.YUS_YDL_Delivery = delivery.PK;
			yardUnit.YUS_YTU_ReceiveTransportationUnit = transportationUnit.PK;
			factory.SaveForTesting();
			return delivery;
		}

		public static CYDDeliveryHeader CreateDeliveryHeader(UniversalObjectFactory factory, WhsWarehouse yard, CYDDelivery[] deliveries)
		{
			var deliveryHeader = factory.NewWithValidTestData<CYDDeliveryHeader>();
			deliveryHeader.YDH_WW_Yard = yard.PK;
			factory.SaveForTesting();
			foreach (var delivery in deliveries)
			{
				delivery.YDL_YDH_DeliveryHeader = deliveryHeader.PK;
			}
			factory.SaveForTesting();
			return deliveryHeader;
		}

		public static CYDPickup CreatePickup(UniversalObjectFactory factory, CYDReleaseAdviceLine releaseAdviceLine, CYDTransportationUnit transportationUnit, CYDYardUnitState yardUnit, string containerTypeCode, short quantity = 1, string transportReference = "")
		{
			var refContainer = factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, containerTypeCode));
			var pickup = factory.NewWithValidTestData<CYDPickup>();
			if (releaseAdviceLine != null)
			{
				pickup.YPL_YEL_ReleaseAdviceLine = releaseAdviceLine.PK;
				yardUnit.YUS_YEL_ReleaseLine = releaseAdviceLine.PK;
			}
			pickup.YPL_YTU_PickupTransportationUnit = transportationUnit.PK;
			var unitLine = factory.New<CYDUnitLineItem>();
			unitLine.YLI_Type = "CNT";
			unitLine.YLI_RC_ContainerType = refContainer.PK;
			unitLine.YLI_Quantity = quantity;
			pickup.YPL_YLI_UnitLineItem = unitLine.PK;
			pickup.YPL_TransportReference = transportReference;
			yardUnit.YUS_YPL_Pickup = pickup.PK;
			yardUnit.YUS_YTU_DispatchTransportationUnit = transportationUnit.PK;
			factory.SaveForTesting();
			return pickup;
		}

		public static CYDPickupHeader CreatePickupHeader(UniversalObjectFactory factory, WhsWarehouse yard, CYDPickup[] pickups)
		{
			var pickupHeader = factory.NewWithValidTestData<CYDPickupHeader>();
			pickupHeader.YPH_WW_Yard = yard.PK;
			factory.SaveForTesting();
			foreach (var pickup in pickups)
			{
				pickup.YPL_YPH_PickupHeader = pickupHeader.PK;
			}
			factory.SaveForTesting();
			return pickupHeader;
		}

		public static JobDocAddress CreateJobDocAddress(UniversalObjectFactory factory, OrgAddress address, string addressType, BusinessObject parentObject)
		{
			var jobDocAddress = factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = address.PK;
			jobDocAddress.E2_ParentID = parentObject.PK;
			jobDocAddress.E2_ParentTableCode = parentObject.TablePrefix;
			jobDocAddress.E2_AddressType = addressType;
			factory.SaveForTesting();
			return jobDocAddress;
		}

		public static OrgAddress CreateOrganization(UniversalObjectFactory factory, string name, string address, string fullName = "")
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = name;
			org.OH_FullName = fullName;
			var orgAddress = factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = org.PK;
			orgAddress.OA_Code = address;
			factory.SaveForTesting();
			return orgAddress;
		}

		public static OrgAddress GetOrganization(UniversalObjectFactory factory, string name, string address, string fullName = "")
		{
			var org = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, name));
			if (org == null)
			{
				return null;
			}
			var query = new ZQuery(OrgAddressSchema.OA_OH, org.PK);
			var orgAddress = factory.LoadTop1<OrgAddress>(query);
			query.AddToFilter(OrgAddressSchema.OA_Code, address);
			return orgAddress;
		}

		public static Container CreateContainer(string containerNumber, string containerType, bool isEmpty, int containerCount = 3, string sealNumber = "seal number")
		{
			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = containerNumber,
				ContainerType = new ContainerType
				{
					Code = containerType
				},
				ContainerCount = containerCount,
				IsEmptyContainer = isEmpty
			};
			container.SetAdditionalSealNumberCollection(() => new List<SealNumber>
			{
				new SealNumber
				{
					Number = sealNumber
				}
			});
			return container;
		}

		public static StmUniversalJobLink CreateStmUniversalJobLink(UniversalObjectFactory factory, ZGuid parentId, string sourceKey, string sourceType)
		{
			var universalJobLink = factory.New<StmUniversalJobLink>();
			universalJobLink.UCL_CompanyCode = "EDI";
			universalJobLink.UCL_EnterpriseCode = "EDI";
			universalJobLink.UCL_ServerCode = "DAT";
			universalJobLink.UCL_ParentID = parentId;
			universalJobLink.UCL_ParentTableCode = CYDTransportationUnitSchema.Constants.Prefix;
			universalJobLink.UCL_SourceType = sourceType;
			universalJobLink.UCL_SourceKey = sourceKey;
			factory.SaveForTesting();
			return universalJobLink;
		}

		public static string BuildEventXMLWithEventReference(string eventType, string dataSourceName = null, string dataSourceKey = null, string eventReference = null, string dataTargetName = null, string dataTargetKey = null, string recipientRole = "CYD")
		{
			var dataSourceElement = dataSourceName != null && dataSourceKey != null ? new XElement("DataSourceCollection", new XElement("DataSource", new[] { new XElement("Key", dataSourceKey), new XElement("Type", dataSourceName) })).ToString() : "";
			var dataTargetElement = dataTargetName != null && dataTargetKey != null ? new XElement("DataTargetCollection", new XElement("DataTarget", new[] { new XElement("Key", dataTargetKey), new XElement("Type", dataTargetName) })).ToString() : "";
			var eventReferenceElement = new XElement("EventReference", eventReference);

			var xml =
				@"
				<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
					<Event>
						<DataContext>
							<DocumentaryOverride>
								<DocumentName>Goods Received(CRESA)</DocumentName>
							</DocumentaryOverride>" + dataSourceElement + dataTargetElement +
					$@"
							<Company>
								<Code>EDI</Code>
								<Name>Eagle Datamation International</Name>
							</Company>
							<DataProvider>EDIDATEDI</DataProvider>
							<EnterpriseID>EDI</EnterpriseID>
							<ServerID>DAT</ServerID>
							<EventType>
								<Code>{eventType}</Code>
							</EventType>" + eventReferenceElement +
					$@"
							<RecipientRoleCollection>
								<RecipientRole>
									<Code>{recipientRole}</Code>
								</RecipientRole>
							</RecipientRoleCollection>
						</DataContext>
						<EventTime>2021-05-28T10:29:00</EventTime>
						<EventType>{eventType}</EventType>" + eventReferenceElement +
				@"
					</Event>
				</UniversalEvent>";

			return GetFormattedXMLText(xml);
		}

		public static string GetFormattedXMLText(string inputXml)
		{
			XmlDocument document = new XmlDocument();
			document.Load(new StringReader(inputXml));

			StringBuilder builder = new StringBuilder();
			using (XmlTextWriter writer = new XmlTextWriter(new StringWriter(builder)))
			{
				writer.Formatting = Formatting.Indented;
				document.Save(writer);
			}
			return builder.ToString();
		}

		public static DateTimeOffset TrimSeconds(DateTimeOffset target) => new(target.Year, target.Month, target.Day, target.Hour, target.Minute, 0, 0, target.Offset);
	}
}
