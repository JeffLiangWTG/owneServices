using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Event = Enterprise.ZArchitecture.Business.Event;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalWarehouse = Enterprise.UniversalDataBuss.DataObjects.Universal.Warehouse;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	sealed class TestDataForUniversal
	{
		public TestDataForUniversal(UniversalObjectFactory factory, TestErrorLogger loggerWithTopLevelDataContext, DataContextType dataContextType)
		{
			Factory = Argument.NotNull(factory, "factory");
			LoggerWithTopLevelDataContext = Argument.NotNull(loggerWithTopLevelDataContext, "loggerWithTopLevelDataContext");
			DataContextType = dataContextType;
			LastUsedEntryLineNo = 1; // tests start with 2
		}

		readonly UniversalObjectFactory Factory;
		readonly TestErrorLogger LoggerWithTopLevelDataContext;
		readonly DataContextType DataContextType;

		public UniversalShipment ShipmentDataObject
		{
			get
			{
				if (shipmentDataObject == null)
				{
					shipmentDataObject = SetupDataContext(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Order = new Order(DefaultDataObjectWriterStrategy.TestInstance)
						{
							Warehouse = new UniversalWarehouse
							{
								Code = "WHS",
								Name = "Coolhouse"
							}
						}
					});

					shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { Orgs.ClientAddressDataObject_CRAHOLSYD });
				}
				return shipmentDataObject;
			}
		}

		public UniversalEvent GetEventDataObject(Event eventType, IEnumerable<RecipientRoleDetail> recipientRoles = null)
		{
			var eventDataObject = new UniversalEvent();
			eventDataObject.DataContext = DataContextFactory.New();
			SetupDataContextForCustomsImport(eventDataObject.DataContext, recipientRoles);
			eventDataObject.EventType = eventType.Code;

			// These properties are mandatory
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.ContextCollection = new List<Context> { new Context { Type = nameof(UniversalEvent.ContextTypes.WaybillNumber), Value = "123" } };

			return eventDataObject;
		}

		T SetupDataContext<T>(T topLevelDataObject) where T : ITopLevelDataObject
		{
			LoggerWithTopLevelDataContext.TopLevelDataObject = topLevelDataObject;
			topLevelDataObject.DataContext = DataContextFactory.New();
			return topLevelDataObject;
		}

		TestErrorLogger Logger
		{
			get { return logger ?? (logger = new TestErrorLogger()); }
		}

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory.BOFactory)); }
		}

		UniversalShipment shipmentDataObject;
		TestErrorLogger logger;
		WhsTestHelperFunctions helper;

		#region Custom Fields

		public void SetupCustomLabels(ICustomLabelsConfigOrgProvider customLabelsProvider)
		{
			customLabelsProvider.AddCustomLabel(Constants.CustomLabels.WhsDocket.CustomAttribute1, "What Makes You Happy?");
			customLabelsProvider.AddCustomLabel(Constants.CustomLabels.WhsDocket.CustomAttribute2, "Why this is so long?");
			customLabelsProvider.AddCustomLabel(Constants.CustomLabels.WhsDocket.CustomDate1, "The Date You Are Happy");
			customLabelsProvider.AddCustomLabel(Constants.CustomLabels.WhsDocket.CustomDecimal1, "The Happy Decimal");
			customLabelsProvider.AddCustomLabel(Constants.CustomLabels.WhsDocket.CustomFlag1, "Are you Happy?");
			customLabelsProvider.AddCustomLabel(Constants.CustomLabels.WhsDocketLine.CustomAttribute1, "What Makes You Happy?");
			customLabelsProvider.AddCustomLabel(Constants.CustomLabels.WhsDocketLine.CustomAttribute2, "Why this is so long?");
			customLabelsProvider.AddCustomLabel(Constants.CustomLabels.WhsDocketLine.CustomDate1, "The Date You Are Happy");
			customLabelsProvider.AddCustomLabel(Constants.CustomLabels.WhsDocketLine.CustomDecimal1, "The Happy Decimal");
			customLabelsProvider.AddCustomLabel(Constants.CustomLabels.WhsDocketLine.CustomFlag1, "Are you Happy?");
			customLabelsProvider.AddCustomLabel(Constants.CustomLabels.WhsDocketLine.CustomTextBlob1, "Some Happy Blob");
		}

		public void AddCustomFieldsToDataObject(ICustomizedFieldContainer customFieldContainer, bool addTextBlobToCollection = false)
		{
			customFieldContainer.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			customFieldContainer.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			customFieldContainer.CustomizedFieldCollection.Add("What Makes You Happy?", new ZString("Lots Of Ice"));
			customFieldContainer.CustomizedFieldCollection.Add("Why this is so long?", new ZString("1234567890123456"));
			customFieldContainer.CustomizedFieldCollection.Add("The Date You Are Happy", new ZDateTime(2013, 1, 1));
			customFieldContainer.CustomizedFieldCollection.Add("The Happy Decimal", new ZDecimal(7.7m));
			customFieldContainer.CustomizedFieldCollection.Add("Are you Happy?", new ZBool(true));

			if (addTextBlobToCollection)
			{
				customFieldContainer.CustomizedFieldCollection.Add("Some Happy Blob", new ZString("BLOBO"));
			}
		}

		public void AddCustomFieldsToBizO(ICustomLabelsProvider customLabelsProvider, BusinessObject parentBizo)
		{
			var customProperties = customLabelsProvider.GetCustomFields(customLabelsProvider.ConfigOrgProvider.ConfigOrg, customLabelsProvider.ConfigOrgProvider.Factory).Cast<CustomLabelInfoBase>().Where(o => o.IsEnabled);
			parentBizo[customProperties.FirstOrDefault(o => o.Caption == "What Makes You Happy?").PropertyName] = new ZString("Lots Of Ice");
			parentBizo[customProperties.FirstOrDefault(o => o.Caption == "Why this is so long?").PropertyName] = new ZString("123456789012345");
			parentBizo[customProperties.FirstOrDefault(o => o.Caption == "The Date You Are Happy").PropertyName] = new ZDateTime(2013, 1, 1);
			parentBizo[customProperties.FirstOrDefault(o => o.Caption == "The Happy Decimal").PropertyName] = new ZDecimal(7.7m);
			parentBizo[customProperties.FirstOrDefault(o => o.Caption == "Are you Happy?").PropertyName] = ZBool.True;

			var textBlobCustomLabel = customProperties.FirstOrDefault(o => o.Caption == "Some Happy Blob");
			if (textBlobCustomLabel != null)
			{
				parentBizo[textBlobCustomLabel.PropertyName] = new ZString("BLOBO");
			}
		}

		#endregion

		#region Orgs

		public UniversalOrgs Orgs
		{
			get { return orgs ?? (orgs = new UniversalOrgs(this)); }
		}

		UniversalOrgs orgs;

		public class UniversalOrgs
		{
			public UniversalOrgs(TestDataForUniversal data)
			{
				Data = data;
			}

			readonly TestDataForUniversal Data;

			#region Orgs

			public OrgHeader CRAHOLSYD
			{
				get
				{
					if (cRAHOLSYD == null)
					{
						cRAHOLSYD = CreateOrgInDB(ClientAddressDataObject_CRAHOLSYD);
						cRAHOLSYD.OH_IsWarehouseClient = true;
						Data.Helper.SetClientAttributeType(cRAHOLSYD, AttributeNumber.One, mandatoryAttributeType: false, attributeName: "Colour");
						Data.Helper.SetClientAttributeType(cRAHOLSYD, AttributeNumber.Two, mandatoryAttributeType: false, attributeName: "Size");
						Data.Helper.SetClientAttributeType(cRAHOLSYD, AttributeNumber.Three, mandatoryAttributeType: false, attributeName: "Serial");
						Data.Factory.SaveForTesting();
					}
					return cRAHOLSYD;
				}
			}

			public OrgHeader WUFSHIJNB
			{
				get
				{
					if (wUFSHIJNB == null)
					{
						wUFSHIJNB = CreateOrgInDB(NewCustomsOwnerAddressDataObject_WUFSHIJNB);
						wUFSHIJNB.OH_IsWarehouseClient = true;
						Data.Helper.SetClientAttributeType(wUFSHIJNB, AttributeNumber.One, mandatoryAttributeType: false, attributeName: "Height");
						Data.Helper.SetClientAttributeType(wUFSHIJNB, AttributeNumber.Two, mandatoryAttributeType: false, attributeName: "Length");
						Data.Helper.SetClientAttributeType(wUFSHIJNB, AttributeNumber.Three, mandatoryAttributeType: false, attributeName: "Width");
						Data.Factory.SaveForTesting();
					}
					return wUFSHIJNB;
				}
			}

			public OrgHeader INTHEMSYD
			{
				get { return iNTHEMSYD ?? (iNTHEMSYD = CreateOrgInDB(CustomsWarehouseAddressDataObject_INTHEMSYD)); }
			}

			OrgHeader CreateOrgInDB(OrganizationAddress addressDataObject)
			{
				var address = new OrganisationDataObjectReader(addressDataObject, Data.Logger, Data.Factory).GetMatchedOrNewForTesting();
				Data.Factory.SaveForTesting();

				return address.Header;
			}

			OrgHeader cRAHOLSYD;
			OrgHeader iNTHEMSYD;
			OrgHeader wUFSHIJNB;

			#endregion

			#region OrgDataObjects

			public OrganizationAddress VASOrderClientAddressDataObject_CRAHOLSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(OrganisationTypes.WarehouseClient)); }
			}

			public OrganizationAddress NewCustomsOwnerAddressDataObject_WUFSHIJNB
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_WUFSHIJNB(nameof(OrganisationTypes.WarehouseClient)); }
			}

			public OrganizationAddress ClientAddressDataObject_CRAHOLSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ConsignorDocumentaryAddress); }
			}

			public OrganizationAddress ConsigneeAddressDataObject_CRAHOLSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneeDocumentaryAddress); }
			}

			public OrganizationAddress ImporterAddressDataObject_CRAHOLSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ImporterDocumentaryAddress); }
			}

			public OrganizationAddress CustomsWarehouseAddressDataObject_INTHEMSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.CustomsWarehouseAddress); }
			}

			public OrganizationAddress WarehouseAddressDataObject_INTHEMSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.Warehouse); }
			}

			#endregion
		}

		#endregion

		#region Product

		public OrgSupplierPart CreateProduct(ZString productCode)
		{
			return CreateProduct(Orgs.CRAHOLSYD, productCode);
		}

		public OrgSupplierPart CreateProduct(OrgHeader org, ZString productCode)
		{
			return Helper.CreateProduct(org, productCode);
		}

		public OrgSupplierPart Product
		{
			get { return ProductCRAHOLSYD; }
		}

		public OrgSupplierPart ProductCRAHOLSYD
		{
			get
			{
				if (productCRAHOLSYD == null)
				{
					productCRAHOLSYD = new OrgSupplierPart.Loader(Factory.BOFactory).Load("P1", Orgs.CRAHOLSYD, null);

					if (productCRAHOLSYD == null)
					{
						productCRAHOLSYD = CreateProduct(Orgs.CRAHOLSYD, "P1");
						Helper.SetProductAllAttributeUse(Orgs.CRAHOLSYD, productCRAHOLSYD, true);
					}
				}

				return productCRAHOLSYD;
			}
		}

		OrgSupplierPart productCRAHOLSYD;

		public OrgSupplierPart ProductWUFSHIJNB
		{
			get
			{
				if (productWUFSHIJNB == null)
				{
					productWUFSHIJNB = new OrgSupplierPart.Loader(Factory.BOFactory).Load("W1", Orgs.WUFSHIJNB, null);

					if (productWUFSHIJNB == null)
					{
						productWUFSHIJNB = CreateProduct(Orgs.WUFSHIJNB, "W1");
						Helper.SetProductAllAttributeUse(Orgs.WUFSHIJNB, productWUFSHIJNB, true);
					}
				}

				return productWUFSHIJNB;
			}
		}

		OrgSupplierPart productWUFSHIJNB;

		#endregion

		#region LastUsedEntryLineNo, CustomsQtyOnOrder, CustomsCommInvLineNoOnOrder

		public ZShort LastUsedEntryLineNo
		{
			get;
			private set;
		}

		public ZDecimal CustomsQtyOnOrder
		{
			get { return 5m; }
		}

		public ZShort CustomsCommInvLineNoOnOrder
		{
			get { return 3; }
		}

		public ZShort CustomsCommInvLineBondedWarehouseOrderLineNo
		{
			get { return 3; }
		}

		#endregion

		#region CreateEmptyShipmentDataObject

		public void CreateEmptyShipmentDataObject()
		{
			shipmentDataObject = SetupDataContext(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance));
		}

		#endregion

		#region Create Orgs / Addresses

		public OrgHeader CreateClientOrgCRAHOLSYDInDB()
		{
			return Orgs.CRAHOLSYD;
		}

		public OrgHeader CreateWarehouseOrgINTHEMSYDInDB()
		{
			return Orgs.INTHEMSYD;
		}

		#endregion

		#region CreateOrderLine

		public OrderLine CreateOrderLine(OrgSupplierPart product, ZDecimal quantity)
		{
			return new OrderLine { OrderedQty = quantity, Product = new Product { Code = product.OP_PartNum } };
		}

		#endregion

		#region GetOrCreateWarehouseInDB

		public WhsWarehouse GetOrCreateWarehouseInDB(bool isWarehouseCreatedAsVirtual = false, bool isBondedWarehouse = false, bool createWarehouse = true, bool isFTZWarehouse = false)
		{
			warehouse = warehouse ?? Factory.LoadTop1<WhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, "WHS"));
			if (warehouse != null)
			{
				if (warehouse.WW_IsVirtualWarehouse != isWarehouseCreatedAsVirtual)
				{
					throw new InvalidOperationException("Cannot change from Virtual to Non-Virtual Whs or vice-versa.");
				}

				if (warehouse.WW_WarehouseType == WarehouseTypes.Codes.FreeTradeZone ? !isFTZWarehouse : isFTZWarehouse)
				{
					throw new InvalidOperationException("Cannot change from Free Trade Zone to Non Free Trade Zone Whs or vice-versa.");
				}
			}
			else
			{
				var warehouseAddress = CreateWarehouseOrgINTHEMSYDInDB().MainAddress;
				if (createWarehouse)
				{
					var locationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "RNO"));
					warehouse = Factory.New<WhsWarehouse>();
					warehouse.WW_WarehouseCode = "WHS";
					warehouse.WW_WarehouseName = "CoolHouse";
					warehouse.WW_IsVirtualWarehouse = isWarehouseCreatedAsVirtual;
					warehouse.WW_OA_WarehouseAddress = warehouseAddress.PK;
					warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
					warehouse.WW_WLT_DefaultLocationType = locationType.PK;
					if (isFTZWarehouse)
					{
						warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
					}

					var row = warehouse.Rows.AddNew();
					row.WR_Name = isWarehouseCreatedAsVirtual ? "BOND" : "A";
					row.WR_Columns = (short)(isWarehouseCreatedAsVirtual ? 1 : 10);
					row.WR_Levels = (short)(isWarehouseCreatedAsVirtual ? 1 : 8);
					row.WR_Trays = (short)(isWarehouseCreatedAsVirtual ? 1 : 5);

					var dockDoorLocationType = Helper.CreateLocationType("XYZ", Enterprise.Warehouse.Transactions.DataTransfer.ResString.GetMultilingualString("Test", "Test"), false, 0, LocationClasses.Codes.DDL);
					var warehouseDockDoorLocation = Helper.CreateRowAndGenerateLocations(warehouse, "DockA", 1, 1).Locations.Single();
					warehouseDockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

					var area = warehouse.Areas[0];
					area.WA_Name = "RARA";

					if (isWarehouseCreatedAsVirtual)
					{
						var inwardArea = Helper.CreateArea(warehouse, "INWARDS", AreaTypes.Codes.InwardProcessing);

						var inwardLocation = Helper.CreateRowAndGenerateLocations(warehouse, "INW", 1, 1).Locations.Single();
						inwardLocation.WLV_WA_PutawayArea = inwardArea.PK;
						inwardLocation.WLV_WA_PickingArea = inwardArea.PK;
					}

					if (isBondedWarehouse)
					{
						warehouse.WW_IsBondedWarehouse = true;
						area.WA_AreaType = AreaTypes.Codes.Bonded;
					}
				}

				Factory.SaveForTesting();
			}

			return warehouse;
		}

		WhsWarehouse warehouse;

		#endregion

		#region ServiceArea

		public WhsArea ServiceArea
		{
			get { return serviceArea ?? (serviceArea = Helper.CreateServiceAreaForVASOrder(GetOrCreateWarehouseInDB())); }
		}

		WhsArea serviceArea;

		#endregion

		#region SetupShipmentDataObjectAndEntitiesForVASOrderImportInDB

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		public void SetupShipmentDataObjectAndEntitiesForVASOrderImportInDB()
		{
			CreateClientOrgCRAHOLSYDInDB();

			ShipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Orgs.VASOrderClientAddressDataObject_CRAHOLSYD });
			ShipmentDataObject.Order.OrderNumber = "UPDATEME";
			ShipmentDataObject.Order.StagingArea = ServiceArea.WA_NameMultilingual;
			Factory.SaveForTesting();
		}

		#endregion

		#region SetupProductsAndShipmentDataObjectForCustomsImportInDB

		public void SetupProductsAndShipmentDataObjectForCustomsImportInDB(bool addWarehouseToXML = true, bool addImporterToXML = true, bool isWarehouseCreatedAsVirtual = false, bool createInventory = true, bool createInventoryAsInwardProcessing = false, bool createWarehouse = true, IEnumerable<RecipientRoleDetail> recipientRoles = null, bool isFTZWarehouse = false, bool isSerialNumberTest = false, bool setClearDpsScreeningStatus = false)
		{
			SetupDataContextForCustomsImport(ShipmentDataObject.DataContext, recipientRoles);

			// add branch
			ShipmentDataObject.Branch = Branch.New(GlbBranch.CurrentBranch);

			// for Previous Entry Number
			if (DataContextType == DataContextType.WarehouseOrder)
			{
				ShipmentDataObject.MessageType = new CodeDescriptionPair { Code = "EXW" }; // Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.ExWarehouse
			}

			// create whs or just insert warehouse address in DB
			var warehouse = GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual, isBondedWarehouse: true, createWarehouse: createWarehouse, isFTZWarehouse: isFTZWarehouse);

			// create customs importer (whs client)
			var importer = Orgs.CRAHOLSYD;
			var importerAddressDataObject = Orgs.ImporterAddressDataObject_CRAHOLSYD;

			// create new owner for change of inventory
			var isChangeOfOwner = recipientRoles?.Any(r => r.Type == RecipientRoleType.BCO) ?? false;
			if (isChangeOfOwner)
			{
				var newOwner = Orgs.WUFSHIJNB;
				if (isSerialNumberTest)
				{
					Helper.SetClientAttributeType(newOwner, AttributeNumber.Serial, true);
				}
			}

			// add client as importer to the XML
			if (addImporterToXML)
			{
				ShipmentDataObject.OrganizationAddressCollection.Add(importerAddressDataObject);
			}

			// create product with customs importer (whs client) as owner
			var productForInventory = Product;
			var productForImport = isChangeOfOwner ? ProductWUFSHIJNB : productForInventory;

			// bring in some stock
			if (createWarehouse && createInventory)
			{
				var receive = Helper.CreateWhsReceive(importer, warehouse, "R1");
				receive.WD_IsInwardsProcessingJob = createInventoryAsInwardProcessing && isWarehouseCreatedAsVirtual;
				if (isSerialNumberTest)
				{
					Helper.SetClientAttributeType(importer, AttributeNumber.Serial, true);
					var invLine = Helper.CreateWhsReceiveInventoryLine(receive, productForInventory, 1m, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
					invLine.WI_SerialNumber = "SNN";
				}
				else
				{
					Helper.CreateWhsReceiveInventoryLine(receive, productForInventory, 100m, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
				}

				receive.AllocateLocationsWithMock();
				using (((IBusinessObjectInternals)receive).ResumeValidationForAllDescendantsTemporarily())
				{
					receive.FinaliseDocketWithoutUserConfirmation();
				}
				Factory.SaveForTesting();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			}

			// add warehouse to the XML
			if (addWarehouseToXML)
			{
				ShipmentDataObject.OrganizationAddressCollection.Add(Orgs.CustomsWarehouseAddressDataObject_INTHEMSYD);
			}

			// add customs commercial invoice lines + matching entrylines
			SetupProductsAndShipmentDataObjectForCustomsImportInDB_CommercialInvoiceLine(productForImport, isSerialNumberTest);

			// no Order Data Object for Customs Import
			ShipmentDataObject.Order = null;

			if (setClearDpsScreeningStatus)
			{
				importer.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				warehouse.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			}
		}

		void SetupDataContextForCustomsImport(IDataContextDataObject dataContext, IEnumerable<RecipientRoleDetail> recipientRoles = null)
		{
			//  add customs as a datasource with System ID
			dataContext.AddDataSource(DataContextType.CustomsDeclaration, "B123");
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = recipientRoles ?? new[] { RecipientRoleType.BWR, RecipientRoleType.BWI }.ToRecipientRoleDetails() });

			dataContext.AddDataTarget(DataContextType, null); // add eg. Order or Receive DataTarget (will always be there for real Customs imports).
		}

		void SetupProductsAndShipmentDataObjectForCustomsImportInDB_CommercialInvoiceLine(OrgSupplierPart differentProduct, bool isSerialNumberTest)
		{
			// add a commercial invoice line (will become order or receive line)
			ShipmentDataObject.CommercialInfo = new CommercialInfo
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
				{
					new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddInfoCollection = new List<AddInfo>
						{
							new AddInfo { Key = "TILV4Warehouse", Value = "999" },
							new AddInfo { Key = "Moo", Value = "50" }
						},
					}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()))
				}
			};

			// add a matching entry line
			ShipmentDataObject.SetEntryHeaderCollection(() => new List<EntryHeader>
			{
				new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance)
				{
					EntryNumberCollection = new List<UniversalDataBuss.DataObjects.Universal.Customs.EntryNumber>
					{
						new UniversalDataBuss.DataObjects.Universal.Customs.EntryNumber
						{
							Number = "EntryNumber123",
							Type = new EntryType
							{
								Code = "CUS",
								Description = "Who cares."
							}
						}
					},
					EntryLineCollection = new List<EntryLine> { },
					Type = new EntryType
					{
						Code = "CUS",
						Description = "Who cares."
					},
					EntryInstructionLink = 1,
					BondValidToDate = new ZDateTime(2020, 6, 30),
				}
			});

			ShipmentDataObject.SetEntryInstructionCollection(() => new List<EntryInstruction>
			{
				new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Style = "CEI",
						Link = 1
					}
			});

			var qtyOnOrder = isSerialNumberTest ? (ZDecimal)1m : CustomsQtyOnOrder;

			AddCommercialInvoiceLineWithRelatedEntryLine(qtyOnOrder, CustomsCommInvLineNoOnOrder, differentProduct, isSerialNumberTest: isSerialNumberTest, orderLineNo: CustomsCommInvLineBondedWarehouseOrderLineNo);
		}

		#endregion

		#region AddCommercialInvoiceLineWithRelatedEntryLine

		public CommercialInvoiceLine AddCommercialInvoiceLineWithRelatedEntryLine(ZDecimal bondQty, ZShort commInvLineNo, OrgSupplierPart differentProduct, string inwardsEntryNo = "EntryNumber123", string outwardsEntryNo = "DummyOutward-1", bool isSerialNumberTest = false, short orderLineNo = 0)
		{
			CommercialInvoiceLine result;

			++LastUsedEntryLineNo;
			if (DataContextType == DataContextType.WarehouseOrder)
			{
				result = AddCommercialInvoiceLineWithRelatedEntryLine(bondQty, commInvLineNo, differentProduct, LastUsedEntryLineNo + 100, outwardsEntryNo, LastUsedEntryLineNo, inwardsEntryNo, isSerialNumberTest, orderLineNo);
			}
			else
			{
				result = AddCommercialInvoiceLineWithRelatedEntryLine(bondQty, commInvLineNo, differentProduct, LastUsedEntryLineNo, inwardsEntryNo, 0, "", isSerialNumberTest, orderLineNo);
			}

			return result;
		}

		public CommercialInvoiceLine AddCommercialInvoiceLineWithRelatedEntryLine(ZDecimal bondQty, ZShort commInvLineNo, OrgSupplierPart differentProduct, ZShort entryLineNo, ZString entryNo, ZShort previousEntryLineNo, ZString previousEntryNo, bool isSerialNumberTest = false, short orderLineNo = 0)
		{
			if (ShipmentDataObject.CommercialInfo == null)
			{
				throw new InvalidOperationException("Call SetupProductsAndShipmentDataObjectForCustomsImportInDB() before attempting to add more lines.");
			}

			var line = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddInfoCollection = new List<AddInfo>
				{
					new AddInfo { Key = "TILV4Warehouse", Value = "777" },
					new AddInfo { Key = "WRL", Value = previousEntryLineNo.ToString() },
					new AddInfo { Key = "WRN", Value = previousEntryNo },
				},

				BondedWHSOrderLineNumber = orderLineNo,
				BondedWarehouseQuantity = bondQty,
				BondedWarehouseQuantityUnit = new CodeDescriptionPair { Code = "BOX", Description = "Box" },
				CustomsQuantity = 6m,
				CustomsQuantityUnit = new CodeDescriptionPair6Char { Code = "PCE", Description = "Pieces" },
				CustomsValue = 888m,
				Commodity = new Commodity { Code = "CMM", Description = "Comm" },
				CountryOfOrigin = new Country { Code = "IT", Name = "Italy" },
				EntryLineNumber = entryLineNo,
				EntryNumber = entryNo,
				LineNo = commInvLineNo,
				LinePrice = 10.3,
				PartNo = differentProduct.OP_PartNum,

				CustomizedFieldCollection = new List<CustomizedField>(),

				CustomsSecondQuantity = 12m,
				CustomsSecondQuantityUnit = new CodeDescriptionPair6Char { Code = "BOX", Description = "Boxes" },
				HarmonisedCode = "T2",
				PrimaryPreference = "PP",

				CustomsThirdQuantity = 22m,
				CustomsThirdQuantityUnit = new CodeDescriptionPair6Char { Code = "BOX", Description = "Boxes" },
				// ManufacturerAddress not being tested yet, wait on Customs team to implement
				Procedure = "PRO",
				EntryInstructionLink = 1,
			};

			if (differentProduct.RelatedOrganisations[0].OU_UsePartAttrib1)
			{
				line.CustomizedFieldCollection.Add(CustomizedField.New("Colour", new ZString("Red")));
			}

			if (differentProduct.RelatedOrganisations[0].OU_UsePartAttrib2)
			{
				line.CustomizedFieldCollection.Add(CustomizedField.New("Size", new ZString("Medium")));
			}

			if (differentProduct.RelatedOrganisations[0].OU_UsePartAttrib3)
			{
				line.CustomizedFieldCollection.Add(CustomizedField.New("Serial", new ZString("S1234")));
			}

			if (isSerialNumberTest && differentProduct.RelatedOrganisations[0].OU_UseSerialNumber)
			{
				line.CustomizedFieldCollection.Add(CustomizedField.New("Serial Number", new ZString("SNN")));
			}

			// add the commercial invoice line
			ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection.Add(line);

			// add the related entry line if one does not exist (multiple comm inv lines can link to a single entry line)
			if (!ShipmentDataObject.EntryHeaderCollection[0].EntryLineCollection.Any(el => el.LineNumber == entryLineNo))
			{
				var entryLine = new EntryLine
				{
					LineNumber = entryLineNo,
				};
				ShipmentDataObject.EntryHeaderCollection[0].EntryLineCollection.Add(entryLine);
			}

			return line;
		}

		#endregion
	}
}
