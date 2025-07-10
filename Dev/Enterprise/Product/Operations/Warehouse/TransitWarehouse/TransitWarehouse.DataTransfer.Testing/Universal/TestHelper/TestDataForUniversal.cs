#region Test
#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class TestDataForUniversal
	{
		public TestDataForUniversal(UniversalObjectFactory factory, TestErrorLogger loggerWithTopLevelDataContext)
		{
			Factory = Argument.NotNull(factory, "factory");
			LoggerWithTopLevelDataContext = Argument.NotNull(loggerWithTopLevelDataContext, "loggerWithTopLevelDataContext");
		}

		readonly UniversalObjectFactory Factory;
		readonly TestErrorLogger LoggerWithTopLevelDataContext;

		#region CreateReceiveConsolAndConsignmentsInDB

		public WhsTransitReceiveConsol CreateReceiveConsolAndConsignmentsInDB(UniversalShipment consolDataObject)
		{
			var poke = Warehouse;
			Logger.TopLevelDataObject = consolDataObject;
			var receiveConsol = new WhsTransitReceiveConsolDataObjectReader(consolDataObject, Logger, Factory).ReadIntoBusinessObject();
			TransitUniversalTestCase.AssertNotNull(receiveConsol);

			var packageStatesQuery = new ZQuery(WhsItemPackageStateSchema.WPS_WRH_TransitReceiveHeader, receiveConsol.PK);
			var packagePKs = Factory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, packageStatesQuery).Select(d => d[WhsItemPackageStateSchema.Constants.WPS_KP_Package]);

			var packagesQuery = new ZQuery(PkgPackageSchema.PK, packagePKs);
			var allPackages = Factory.RowFactory.Load(PkgPackageSchema.Constants.TableName, packagesQuery);

			packagesQuery.AddToFilter(PkgPackageSchema.KP_KPH_PackageHeader, SQLComparisonOperator.NotEqual, null);
			var packagesWithIDs = Factory.RowFactory.Load(PkgPackageSchema.Constants.TableName, packagesQuery);

			var packageCount = consolDataObject.PackingLineCollection != null
				? consolDataObject.PackingLineCollection.Sum(p => p.PackQty.GetValueOrDefault())
				: 0;

			var packageIdPKs = packagesWithIDs.Select(p => p[PkgPackageSchema.Constants.KP_KPH_PackageHeader]);
			var packageIdQuery = new ZQuery(PkgPackageHeaderSchema.PK, packageIdPKs);
			var packageIDsFromDB = Factory.RowFactory.Load(PkgPackageHeaderSchema.Constants.TableName, packageIdQuery);

			var packageIDs = consolDataObject.PackingLineCollection != null
				? consolDataObject.PackingLineCollection.Select(p => p.ReferenceNumber.GetValueOrDefault()).Where(id => !id.IsEmpty)
				: Enumerable.Empty<ZString>();

			var subShipments = consolDataObject.SubShipmentCollection.Where(subShipment => !subShipment.IsFromGateBooking());
			var consignmentCount = subShipments != null ? subShipments.Count() : 0;

			TransitUniversalTestCase.AssertContainsExactElementsInAnyOrder("Precondition: Created correct Package Ids.", packageIDs, packageIDsFromDB.Select(p => p[PkgPackageHeaderSchema.Constants.KPH_PackageID]));
			TransitUniversalTestCase.AssertEquals("Precondition: Created correct amount of packages.", packageCount, allPackages.Length);
			TransitUniversalTestCase.AssertEquals("Precondition: Created correct amount of Consignments.", consignmentCount, receiveConsol.PopulatedConsignmentsForTesting.Length);

			Factory.SaveForTesting();
			return receiveConsol;
		}

		#endregion

		#region CreateReceiveConsignmentInDB

		public WhsItemReceiveConsignment CreateReceiveConsignmentInDB(UniversalShipment consignmentDataObject, bool checkPackageQty = true)
		{
			var poke = Warehouse;
			Logger.TopLevelDataObject = consignmentDataObject;
			var receiveConsignment = new WhsTransitReceiveConsignmentDataObjectReader(consignmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			TransitUniversalTestCase.AssertNotNull(receiveConsignment);

			var packageStatesQuery = new ZQuery(WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment.PK);
			var packagePKs = Factory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, packageStatesQuery).Select(d => d[WhsItemPackageStateSchema.Constants.WPS_KP_Package]);

			var packagesQuery = new ZQuery(PkgPackageSchema.PK, packagePKs);
			var allPackages = Factory.RowFactory.Load(PkgPackageSchema.Constants.TableName, packagesQuery);

			packagesQuery.AddToFilter(PkgPackageSchema.KP_KPH_PackageHeader, SQLComparisonOperator.NotEqual, null);
			var packagesWithIDs = Factory.RowFactory.Load(PkgPackageSchema.Constants.TableName, packagesQuery);

			var packageCount = consignmentDataObject.PackingLineCollection != null
				? consignmentDataObject.PackingLineCollection.Sum(p => p.PackQty.GetValueOrDefault())
				: 0;

			var packageIdPKs = packagesWithIDs.Select(p => p[PkgPackageSchema.Constants.KP_KPH_PackageHeader]);
			var packageIdQuery = new ZQuery(PkgPackageHeaderSchema.PK, packageIdPKs);
			var packageIDsFromDB = Factory.RowFactory.Load(PkgPackageHeaderSchema.Constants.TableName, packageIdQuery);

			var packageIDs = consignmentDataObject.PackingLineCollection != null
				? consignmentDataObject.PackingLineCollection.Select(p => p.ReferenceNumber.GetValueOrDefault()).Where(id => !id.IsEmpty)
				: Enumerable.Empty<ZString>();

			if (checkPackageQty)
			{
				TransitUniversalTestCase.AssertContainsExactElementsInAnyOrder("Precondition: Created correct Package Ids.", packageIDs, packageIDsFromDB.Select(p => p[PkgPackageHeaderSchema.Constants.KPH_PackageID]));
				TransitUniversalTestCase.AssertEquals("Precondition: Created correct amount of packages.", packageCount, allPackages.Sum(p => (int)p[PkgPackageSchema.Constants.KP_PackageQty]));
			}

			Factory.SaveForTesting();
			receiveConsignment.Reload();

			return receiveConsignment;
		}

		#endregion

		#region CreateShipmentWithPackages

		public UniversalShipment CreateShipmentWithPackages(string id, params string[] packageIDs)
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = id
			};

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Orgs.Consignor_CRAHOLSYD, Orgs.Consignor_PICKUPSYD, Orgs.Consignee_INTHEMSYD, Orgs.DeliveryAddress_CRAHOLSYD, Orgs.TransportCompany_INTHEMSYD, Orgs.Warehouse_WUFSHIJNB, Orgs.Warehouse_INTHEMSYD });
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(packageIDs.Select(packageID => new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, ReferenceNumber = packageID, PackType = new PackageType { Code = "PKG" } })));
			return SetupNewDataContextWithDataSource(shipment, shipmentNumber: id);
		}

		public UniversalShipment CreateShipmentWithDestinationPort(string id, string portOfDestination, params string[] packageIDs)
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = id,
				PortOfOrigin = new UNLOCO { Code = "ZAJNB" },
				PortOfDestination = new UNLOCO { Code = portOfDestination }
			};

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Orgs.Consignor_CRAHOLSYD, Orgs.Warehouse_WUFSHIJNB, Orgs.Warehouse_INTHEMSYD });
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.Outturn, "S1000000");
			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWR } } });
			shipment.DataContext = dataContext;

			shipment.SetAddInfoCollection(() =>
			{
				var addInfoCollection = new List<AddInfo>()
				{
					AddInfo.New("Consignee", "Test")
				};

				return addInfoCollection;
			});
			return shipment;
		}

		public UniversalShipment CreateShipmentWithWayBillAndPackages(string shipmentNumber, string wayBillNumber, params string[] packageIDs)
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = wayBillNumber
			};

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Orgs.Consignor_CRAHOLSYD, Orgs.Consignor_PICKUPSYD, Orgs.Consignee_INTHEMSYD, Orgs.Warehouse_WUFSHIJNB, Orgs.Warehouse_INTHEMSYD });
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(packageIDs.Select(packageID => new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, ReferenceNumber = packageID, PackType = new PackageType { Code = "PKG" } })));
			return SetupNewDataContextWithDataSource(shipment, shipmentNumber: shipmentNumber);
		}

		public UniversalShipment CreateTransitDataTargetWithPackages(string id, params string[] packageIDs)
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = id
			};

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Orgs.Consignor_CRAHOLSYD, Orgs.Consignor_PICKUPSYD, Orgs.Consignee_INTHEMSYD, Orgs.Warehouse_WUFSHIJNB, Orgs.Warehouse_INTHEMSYD });
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(packageIDs.Select(packageID => new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, ReferenceNumber = packageID, PackType = new PackageType { Code = "PKG" } })));
			return SetupNewDataContextWithDispatchDataTarget(shipment, consignmentNumber: id);
		}

		public UniversalShipment CreateShipmentWithPackagesWithoutIds(string id, params Tuple<string, int>[] typesAndQty)
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = id
			};
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Orgs.Consignor_CRAHOLSYD, Orgs.Consignor_PICKUPSYD, Orgs.Consignee_INTHEMSYD, Orgs.Warehouse_WUFSHIJNB, Orgs.Warehouse_INTHEMSYD });
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(typesAndQty.Select(typeAndQty => new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = typeAndQty.Item2, ReferenceNumber = "", PackType = new PackageType { Code = typeAndQty.Item1 } })));
			return SetupNewDataContextWithDataSource(shipment);
		}

		public UniversalShipment CreateShipmentWithCombinedPackages(string shipmentId, params (string type, int qty, string id)[] typeAndQtyAndIds)
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = shipmentId
			};
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Orgs.Consignor_CRAHOLSYD, Orgs.Consignor_PICKUPSYD, Orgs.Consignee_INTHEMSYD, Orgs.Warehouse_WUFSHIJNB, Orgs.Warehouse_INTHEMSYD });
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(typeAndQtyAndIds.Select(item => new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = item.qty, ReferenceNumber = item.id, PackType = new PackageType { Code = item.type } })));
			return SetupNewDataContextWithDataSource(shipment);
		}

		public UniversalShipment CreateShipmentWithPackagesAndContainerLinks(string id, params Tuple<int?, string>[] containerLinkAndPackageIDs)
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { WayBillNumber = id };
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Orgs.Consignor_CRAHOLSYD, Orgs.Consignor_PICKUPSYD, Orgs.Consignee_INTHEMSYD, Orgs.Warehouse_WUFSHIJNB, Orgs.Warehouse_INTHEMSYD });
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(containerLinkAndPackageIDs.Select(c => new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, ContainerLink = c.Item1, ReferenceNumber = c.Item2, PackType = new PackageType { Code = "PKG" } })));
			return SetupNewDataContextWithDataSource(shipment);
		}

		#endregion

		#region CreateContainer

		public Container CreateContainer(string containerNumber, int containerLink, string containerTypeCode = "40GP", string deliveryMode = "SEA")
		{
			return new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = containerNumber,
				ContainerType = new ContainerType { Code = containerTypeCode },
				Link = containerLink,
				DeliveryMode = deliveryMode
			};
		}

		#endregion

		#region DataObjects

		public UniversalShipment ShipmentDataObject
		{
			get
			{
				if (shipmentDataObject == null)
				{
					var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
					{
						PortOfOrigin = new UNLOCO { Code = "ZAJNB" },
						PortOfDestination = new UNLOCO { Code = "AUSYD" },
						WayBillNumber = DefaultConsignmentID
					};

					shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Orgs.Consignor_CRAHOLSYD, Orgs.Consignor_PICKUPSYD, Orgs.Consignee_INTHEMSYD, Orgs.Warehouse_WUFSHIJNB, Orgs.Warehouse_INTHEMSYD });
					shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { Pack }) { Content = CollectionContent.Complete });
					shipmentDataObject = SetupNewDataContextWithDataSource(shipment);
				}
				return shipmentDataObject;
			}
		}

		public UniversalShipment ShipmentDataObjectwithNewWarehouse
		{
			get
			{
				if (shipmentDataObjectwithNewWarehouse == null)
				{
					var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
					{
						PortOfOrigin = new UNLOCO { Code = "ZAJNB" },
						PortOfDestination = new UNLOCO { Code = "AUSYD" },
						WayBillNumber = "Waybill123"
					};

					shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Orgs.Consignor_CRAHOLSYD, Orgs.Consignee_INTHEMSYD, Orgs.Warehouse_CRAHOLSYD, Orgs.Warehouse_INTHEMSYD });
					shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { Pack }) { Content = CollectionContent.Complete });
					shipmentDataObjectwithNewWarehouse = SetupNewDataContextWithDataSource(shipment);
				}
				return shipmentDataObjectwithNewWarehouse;
			}
		}

		public UniversalShipment HeaderDataObject
		{
			get
			{
				if (headerDataObject == null)
				{
					var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
					{
						WayBillNumber = "WaybillParent",
					};
					shipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { Transport });
					shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { ShipmentDataObject });
					shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Orgs.Warehouse_WUFSHIJNB, Orgs.Warehouse_INTHEMSYD });
					headerDataObject = SetupNewDataContextWithDataSource(shipment, consolNumber: "C1000000");
				}

				return headerDataObject;
			}
		}

		public UniversalShipment HeaderDataObjectwithNewWarehouse
		{
			get
			{
				if (headerDataObjectwithNewWarehouse == null)
				{
					var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
					{
						WayBillNumber = "WaybillParent",
					};
					shipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { Transport });
					shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { ShipmentDataObjectwithNewWarehouse });
					shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Orgs.Warehouse_CRAHOLSYD, Orgs.Warehouse_INTHEMSYD });
					headerDataObjectwithNewWarehouse = SetupNewDataContextWithDataSource(shipment, consolNumber: "C1000000");
				}

				return headerDataObjectwithNewWarehouse;
			}
		}

		public UniversalShipment HeaderDataObjectWithDataTarget
		{
			get
			{
				if (headerDataObject == null)
				{
					var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
					{
						WayBillNumber = "WaybillParent",
					};
					shipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { Transport });
					shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { ShipmentDataObject });
					shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Orgs.Warehouse_WUFSHIJNB, Orgs.Warehouse_INTHEMSYD });
					headerDataObject = SetupNewDataContextWithDispatchDataTarget(shipment, isConsolDataTarget: true);
				}

				return headerDataObject;
			}
		}

		public UniversalShipment HeaderDataObjectWithoutChildShipment
		{
			get
			{
				if (headerDataObjectWithoutChildShipment == null)
				{
					headerDataObjectWithoutChildShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
					{
						WayBillNumber = "WaybillParent",
					};
					headerDataObjectWithoutChildShipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { Transport });
				}
				return headerDataObjectWithoutChildShipment;
			}
		}

		public UniversalShipment HeaderDataObjectForGateBooking_ATW
		{
			get
			{
				if (shipmentDataObjectForGateBooking_ATW == null)
				{
					var shipment = CreateHeaderObjectForGateBooking();
					shipmentDataObjectForGateBooking_ATW = SetupNewDataContextWithDataSource(shipment, gateBookingNumber: "GTB001", isArrival: true);
				}
				return shipmentDataObjectForGateBooking_ATW;
			}
		}

		public UniversalShipment CreateHeaderDataObjectForGateBooking_ATW_WithTransportCompany(OrganizationAddress transportaCompanyDocAddress)
		{
			var shipment = CreateHeaderObjectForGateBooking(transportaCompanyDocAddress: transportaCompanyDocAddress);
			return SetupNewDataContextWithDataSource(shipment, gateBookingNumber: "GTB001", isArrival: true);
		}

		public UniversalShipment CreateHeaderObjectForGateBooking(string vehicleRegistrationNumber = "DEF-023", string vehicleTypeCode = "RTRK", string driverName = "DRIVER1", OrganizationAddress transportaCompanyDocAddress = null)
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			SetupHeaderObjectForGateBooking(shipment, vehicleRegistrationNumber, vehicleTypeCode, driverName, transportaCompanyDocAddress);
			return shipment;
		}

		public UniversalShipment SetupHeaderObjectForGateBooking(UniversalShipment shipment, string vehicleRegistrationNumber = "DEF-023", string vehicleTypeCode = "RTRK", string driverName = "DRIVER1", OrganizationAddress transportaCompanyDocAddress = null)
		{
			if (shipment.OrganizationAddressCollection == null || shipment.OrganizationAddressCollection.Count == 0)
			{
				shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Orgs.Warehouse_WUFSHIJNB, Orgs.Warehouse_INTHEMSYD, transportaCompanyDocAddress ?? Orgs.TransportCompanyDocAddress_INTHEMSYD });
			}
			else
			{
				if (transportaCompanyDocAddress != null)
				{
					shipment.OrganizationAddressCollection.Add(transportaCompanyDocAddress);
				}
				else if (!shipment.OrganizationAddressCollection.Any(orgAddr => orgAddr.AddressType.HasValue && orgAddr.AddressType.Value == (ZString)nameof(DocAddressType.TransportCompanyDocumentaryAddress)))
				{
					shipment.OrganizationAddressCollection.Add(Orgs.TransportCompanyDocAddress_INTHEMSYD);
				}
				if (!shipment.OrganizationAddressCollection.Any(orgAddr => orgAddr.AddressType.HasValue && orgAddr.AddressType.Value == (ZString)nameof(DocAddressType.ArrivalCFSAddress)))
				{
					shipment.OrganizationAddressCollection.Add(Orgs.Warehouse_INTHEMSYD);
				}
				if (!shipment.OrganizationAddressCollection.Any(orgAddr => orgAddr.AddressType.HasValue && orgAddr.AddressType.Value == (ZString)nameof(DocAddressType.DepartureCFSAddress)))
				{
					shipment.OrganizationAddressCollection.Add(Orgs.Warehouse_WUFSHIJNB);
				}
			}

			shipment.SetPreCarriageShipmentCollection(() => new List<UniversalShipment>
			{
				new (DefaultDataObjectWriterStrategy.TestInstance)
				{
					VehicleRun = new VehicleRun()
					{
						Vehicle = new Vehicle()
						{
							Registration = new Registration() { Number = vehicleRegistrationNumber },
							VehicleType = new CodeDescriptionPair10Char() { Code = vehicleTypeCode }
						}
					}
				}
			});
			shipment.VehicleRun = new VehicleRun();
			shipment.VehicleRun.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.VehicleRun.SetCrewCollection(() =>
				new List<Crew>()
				{
					new () { FullName = driverName, CrewType = CrewType.Driver }
				}
			);
			return shipment;
		}

		public UniversalShipment CreateSubShipmentForGateBookingHeaderObject(ZString gateMovementBookingNumber, ZString bookingConfirmationReference, bool isPickup, ZString[] packageIDs = null, OrganizationAddress transportaCompanyDocAddress = null)
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Orgs.Warehouse_WUFSHIJNB, Orgs.Warehouse_INTHEMSYD, transportaCompanyDocAddress ?? Orgs.TransportCompanyDocAddress_INTHEMSYD });
			SetupGateBookingContextOnShipmentObject(shipment, gateMovementBookingNumber, isPickup);
			shipment.BookingConfirmationReference = bookingConfirmationReference;
			shipment.SetDateCollection(() =>
				new List<Date>()
				{
					new() { Type = DateType.Start, Value = new UXmlDateTime(new ZDateTime(2024, 10, 1, 9, 0, 0)) },
					new() { Type = DateType.End, Value = new UXmlDateTime(new ZDateTime(2024, 10, 1, 10, 0, 0)) }
				}
			);
			shipment.TransportBookingDirection = new TransportBookingDirection() { Code = isPickup ? "PIC" : "DLV" };

			if (packageIDs != null && packageIDs.Length > 0)
			{
				var packingLineCollection = new DataObjectList<PackingLine>
				(
					packageIDs.Select(packageID =>
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							PackQty = 1,
							ReferenceNumber = packageID,
							PackType = new PackageType { Code = "PKG" }
						})
				);
				shipment.SetPackingLineCollection(() => packingLineCollection);
			}
			return shipment;
		}

		#endregion

		#region DefaultConsignmentID

		public static ZString DefaultConsignmentID
		{
			get { return "Waybill123"; }
		}

		#endregion

		#region UniversalOrgs

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
				get { return cRAHOLSYD ?? (cRAHOLSYD = CreateOrgInDB(Consignor_CRAHOLSYD)); }
			}

			public OrgHeader INTHEMSYD
			{
				get { return iNTHEMSYD ?? (iNTHEMSYD = CreateOrgInDB(Consignee_INTHEMSYD)); }
			}

			public OrgHeader WUFSHIJNB
			{
				get { return wUFSHIJNB ?? (wUFSHIJNB = CreateOrgInDB(Warehouse_WUFSHIJNB)); }
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

			public OrganizationAddress Consignor_CRAHOLSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ConsignorDocumentaryAddress); }
			}

			public OrganizationAddress Consignor_PICKUPSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_WUFSHIJNB(DocAddressType.ConsignorPickupDeliveryAddress); }
			}

			public OrganizationAddress SendersLocalCLient_CRAHOLSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(AddressTypes.SendersLocalClient); }
			}

			public OrganizationAddress SendingForwarderAddress_CRAHOLSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(AddressTypes.SendingForwarderAddress); }
			}

			public OrganizationAddress BookingPartyAddress_CRAHOLSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.BookingPartyDocumentaryAddress); }
			}

			public OrganizationAddress ReceivingForwarderAddress_CRAHOLSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(AddressTypes.ReceivingForwarderAddress); }
			}

			public OrganizationAddress ConsigneePickupDeliveryAddress_CRAHOLSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneePickupDeliveryAddress); }
			}

			public OrganizationAddress Consignee_INTHEMSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.ConsigneeDocumentaryAddress); }
			}

			public OrganizationAddress DeliveryAddress_CRAHOLSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneePickupDeliveryAddress); }
			}

			public OrganizationAddress TransportCompany_INTHEMSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(AddressTypes.DeliveryLocalCartage); }
			}

			public OrganizationAddress TransportCompanyDocAddress_INTHEMSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.TransportCompanyDocumentaryAddress); }
			}

			public OrganizationAddress ArrivalCTOAddress_CRAHOLSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(AddressTypes.ArrivalCTOAddress); }
			}

			public OrganizationAddress DepartureCTOAddress_INTHEMSYD
			{
				get { return OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(AddressTypes.DepartureCTOAddress); }
			}

			public OrganizationAddress Warehouse_WUFSHIJNB
			{
				get
				{
					var result = OrganizationAddressTestHelper.GetNewAddressData_WUFSHIJNB(DocAddressType.DepartureCFSAddress);
					result.Port = new UNLOCO() { Code = "ZAJNB" };
					result.AddressShortCode = "SC1";
					return result;
				}
			}

			public OrganizationAddress Warehouse_INTHEMSYD
			{
				get
				{
					var result = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.ArrivalCFSAddress);
					result.Port = new UNLOCO() { Code = "AUSYD" };
					return result;
				}
			}

			public OrganizationAddress Warehouse_CRAHOLSYD
			{
				get
				{
					var result = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.DepartureCFSAddress);
					result.Port = new UNLOCO() { Code = "AUSYD" };
					return result;
				}
			}

			#endregion
		}

		#endregion

		#region Packages

		PackingLine Pack
		{
			get
			{
				return pack ??
				(
					pack = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						PackType = new PackageType { Code = "PKG" },
						PackQty = 2,
						Weight = 150,
						Volume = 2
					}
				);
			}
		}
		PackingLine pack;

		#endregion

		#region TransportLegs

		TransportLeg Transport
		{
			get
			{
				return transport ??
				(
					transport = new TransportLeg
					{
						LegType = LegType.Main,
						PortOfLoading = new UNLOCO { Code = "ZAJNB" },
						LCLCutOff = new ZDateTime(2015, 4, 14),
						EstimatedDeparture = new ZDateTime(2015, 4, 15),
						VoyageFlightNo = "VF1",
						VesselName = "VES1"
					}
				);
			}
		}

		TransportLeg transport;

		#endregion

		#region Warehouse

		public WhsWarehouse Warehouse
		{
			get
			{
				if (warehouse == null)
				{
					var warehouseAddress = Orgs.WUFSHIJNB.MainAddress;
					var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory.BOFactory);
					warehouse = (WhsWarehouse)helper.CreateWarehouse("Transit Warehouse", "TWH", "A");
					warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
					warehouse.WW_OA_WarehouseAddress = warehouseAddress.PK;
					warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
					Factory.SaveForTesting();
				}

				return warehouse;
			}
		}

		WhsWarehouse warehouse;

		#endregion

		#region WarehouseCRAHOLSYD

		public WhsWarehouse WarehouseCRAHOLSYD
		{
			get
			{
				if (warehouseCRAHOLSYD == null)
				{
					var warehouseAddress = Orgs.CRAHOLSYD.MainAddress;
					var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory.BOFactory);
					warehouseCRAHOLSYD = (WhsWarehouse)helper.CreateWarehouse("Transit Warehouse 3", "TW3", "Z");
					var newBranch = Factory.BOFactory.New<GlbBranch>();
					newBranch.GB_Code = "AVB";
					newBranch.GB_GC = GlbBranch.CurrentBranch.GB_GC; // to avoid constraint exception
					newBranch.GB_RL_NKHomePort = "AUSYD";
					warehouseCRAHOLSYD.WW_GB_RelatedCompanyBranch = newBranch.PK;
					warehouseCRAHOLSYD.WW_OA_WarehouseAddress = warehouseAddress.PK;
					warehouseCRAHOLSYD.WW_WarehouseType = WarehouseTypes.Codes.Transit;
					Factory.SaveForTesting();
				}

				return warehouseCRAHOLSYD;
			}
		}

		WhsWarehouse warehouseCRAHOLSYD;

		#endregion

		#region WarehouseINTHEMSYD

		public WhsWarehouse WarehouseINTHEMSYD
		{
			get
			{
				if (warehouseINTHEMSYD == null)
				{
					var warehouseAddress = Orgs.INTHEMSYD.MainAddress;
					var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory.BOFactory);
					warehouseINTHEMSYD = (WhsWarehouse)helper.CreateWarehouse("Transit Warehouse 2", "TW2", "A");
					var newBranch = Factory.BOFactory.New<GlbBranch>();
					newBranch.GB_GC = GlbBranch.CurrentBranch.GB_GC; // to avoid constraint exception
					newBranch.GB_RL_NKHomePort = "AUSYD";
					warehouseINTHEMSYD.WW_GB_RelatedCompanyBranch = newBranch.PK;
					warehouseINTHEMSYD.WW_OA_WarehouseAddress = warehouseAddress.PK;
					warehouseINTHEMSYD.WW_WarehouseType = WarehouseTypes.Codes.Transit;
					Factory.SaveForTesting();
				}

				return warehouseINTHEMSYD;
			}
		}

		WhsWarehouse warehouseINTHEMSYD;

		#endregion

		#region SetupForForwardingImport

		public void SetupForForwardingImport()
		{
			var poke1 = ShipmentDataObject;
			var poke2 = Orgs.CRAHOLSYD;
			var poke3 = Orgs.INTHEMSYD;
			var poke4 = Warehouse;
			var poke5 = WarehouseINTHEMSYD;
			var poke6 = WarehouseCRAHOLSYD;
			var poke7 = Orgs.WUFSHIJNB;
		}

		#endregion

		public UniversalShipment SetupNewDataContextWithDataSource(UniversalShipment shipment, string consolNumber = null, string runSheet = null, string shipmentNumber = "S1000000", string gateBookingNumber = null, bool isArrival = false, bool keepExistingDataContext = false)
		{
			var dataContext = keepExistingDataContext ? shipment.DataContext : DataContextFactory.New();
			if (consolNumber != null)
			{
				dataContext.AddDataSource(DataContextType.ForwardingConsol, consolNumber);
			}
			if (runSheet != null)
			{
				dataContext.AddDataSource(DataContextType.TransportConsignmentRunSheet, runSheet);
			}
			if (shipmentNumber != null)
			{
				dataContext.AddDataSource(DataContextType.ForwardingShipment, shipmentNumber);
			}
			if (gateBookingNumber != null && !shipment.IsFromDataSource(DataContextType.GateBooking))
			{
				dataContext.AddDataSource(DataContextType.GateBooking, gateBookingNumber);
			}

			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = isArrival ? RecipientRoleType.ATW : RecipientRoleType.DTW } } });

			shipment.DataContext = dataContext;
			LoggerWithTopLevelDataContext.TopLevelDataObject = shipment;

			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			return shipment;
		}
		
		public UniversalShipment SetupNewDataContextWithDispatchDataTarget(UniversalShipment shipment, bool isConsolDataTarget = false, string consignmentNumber = "")
		{
			var dataContext = DataContextFactory.New();
			if (isConsolDataTarget)
			{
				dataContext.AddDataTarget(DataContextType.TransitDispatchConsol, "");
			}
			if (consignmentNumber != null)
			{
				dataContext.AddDataTarget(DataContextType.TransitDispatch, consignmentNumber);
			}

			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW } } });

			shipment.DataContext = dataContext;
			LoggerWithTopLevelDataContext.TopLevelDataObject = shipment;

			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			return shipment;
		}

		public UniversalShipment SetupSeaCargoOutturnDataContextOnShipmentObject(UniversalShipment shipment, string reference)
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.SeaCargoOutturn, reference);
			shipment.DataContext = dataContext;

			return shipment;
		}

		public UniversalShipment SetupGateBookingContextOnShipmentObject(UniversalShipment shipment, string gateMovementBookingNumber, bool isArrival, RecipientRoleType recipientRoleType = RecipientRoleType.ATW)
		{
			if (shipment.DataContext == null)
			{
				shipment.DataContext = DataContextFactory.New();
			}

			shipment.DataContext.AddDataSource(DataContextType.GateMovementBooking, gateMovementBookingNumber);
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo
			{
				RecipientRoles = new[]
				{
					new RecipientRoleDetail
					{
						Type = recipientRoleType,
						ServiceCode = ServiceCodeType.GTB
					}
				}
			});

			shipment.DataContext.AddDataTarget(isArrival ? DataContextType.TransitReceiveConsol : DataContextType.TransitDispatchConsol, "", null);
			return shipment;
		}

		public TestErrorLogger Logger
		{
			get { return logger ?? (logger = new TestErrorLogger()); }
			set { logger = value; }
		}

		UniversalShipment shipmentDataObject;
		UniversalShipment shipmentDataObjectForGateBooking_ATW;
		UniversalShipment shipmentDataObjectwithNewWarehouse;
		UniversalShipment headerDataObject;
		UniversalShipment headerDataObjectwithNewWarehouse;
		UniversalShipment headerDataObjectWithoutChildShipment;
		TestErrorLogger logger;
	}
}
#endif
#endregion
