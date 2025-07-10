using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	public class UniversalTestShipmentBuilder
	{
		readonly TestShipmentOptions options;

		public UniversalTestShipmentBuilder(TestShipmentOptions options)
		{
			this.options = options;
		}

		public static Shipment GetShipment(TestShipmentOptions options = null)
		{
			var shipmentBuilder = new UniversalTestShipmentBuilder(options ?? new TestShipmentOptions());
			return shipmentBuilder.Build();
		}

		Shipment Build()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			BuildDataContext(shipment);

			BuildOrganizationAddressCollection(shipment);

			BuildPreCarriageShipmentCollection(shipment);

			BuildSubShipmentCollection(shipment);

			BuildVehicleRun(shipment);

			BuildDateCollection(shipment);

			BuildWarehouseLocation(shipment);

			BuildRelatedShipmentCollection(shipment);

			return shipment;
		}

		void BuildDataContext(Shipment shipment)
		{
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipment.DataContext.DataProviderForCodeMapping = "AAABBBCCC";

			var dataSource = DataContextFactory.NewDataSource();
			if (!options.DataSourceIsNotFound)
			{
				if (options.IsDataSourceGateVehicleMovement)
				{
					dataSource.Key = "GV00001234";
					dataSource.Type = nameof(DataContextType.GateVehicleMovement);
				}
				else
				{
					dataSource.Key = options.UseUniqueSourceKey ? Guid.NewGuid().ToString() : "GB00001234";
					dataSource.Type = nameof(DataContextType.GateBooking);
				}
				shipment.DataContext.AddDataSource(dataSource);
			}
		}

		void BuildOrganizationAddressCollection(Shipment shipment)
		{
			var localCartageYard = new OrganizationAddress
			{
				AddressType = nameof(DocAddressType.LocalCartageYard),
				OrganizationCode = "WUFSHIJNB",
				Address1 = "Level 2, Building G",
				Address2 = "34 Dock Lane",
				AddressShortCode = "PST: 10 HUTCHESON STREET",
				City = "Johannesburg",
				CompanyName = "WUFU SHIPPING LINE",
				Country = new Country
				{
					Code = "ZA",
					Name = "South Africa"
				},
				Postcode = "4010"
			};

			var transportCompanyDocumentaryAddress = new OrganizationAddress
			{
				AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress),
				OrganizationCode = "WUFSHIJNB",
				Address1 = "Level 2, Building G",
				Address2 = "34 Dock Lane",
				AddressShortCode = "PST: 10 HUTCHESON STREET",
				City = "Johannesburg",
				CompanyName = "WUFU SHIPPING LINE",
				Country = new Country
				{
					Code = "ZA",
					Name = "South Africa"
				},
				Postcode = "4010"
			};

			if (options.YardIsNotFound)
			{
				shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					transportCompanyDocumentaryAddress,
				});
			}
			else if (options.BookingPartyIsNotFound)
			{
				shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					localCartageYard,
				});
			}
			else
			{
				shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					localCartageYard,
					transportCompanyDocumentaryAddress,
				});
			}
		}

		void BuildPreCarriageShipmentCollection(Shipment shipment)
		{
			if (!options.IsNeedToBuildPreCarriage)
			{
				return;
			}

			var vehicleNumbers = options.VehicleNumbers ?? new string[] { "REG1" };
			shipment.SetPreCarriageShipmentCollection(() => vehicleNumbers.Select(number => new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				VehicleRun = new VehicleRun
				{
					Vehicle = new Vehicle
					{
						Registration = new Registration
						{
							Number = number
						}
					}
				}
			}).ToList());
		}

		void BuildVehicleRun(Shipment shipment)
		{
			if (!options.IsNeedToBuildVehicleRun)
			{
				return;
			}

			var vehicleNumbers = options.VehicleNumbers ?? new string[] { "REG1" };
			shipment.VehicleRun = new VehicleRun
			{
				Vehicle = new Vehicle
				{
					Registration = new Registration
					{
						Number = vehicleNumbers.FirstOrDefault()
					}
				}
			};
		}

		void BuildWarehouseLocation(Shipment shipment)
		{
			if (options.WarehouseLocation.HasValue)
			{
				shipment.WarehouseLocation = options.WarehouseLocation;
			}
			else if (options.IsDataSourceGateVehicleMovement && options.IsIncoming)
			{
				shipment.WarehouseLocation = "DOCKDOOR";
			}
		}

		void BuildSubShipmentCollection(Shipment shipment)
		{
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>
			(
				(options.ContainerNumbers ?? ["GENL"])
					.Select((containerNumber, index) => BuildSubShipment(
						options.BookingConfirmationReference,
						options.IsForPickup,
						"VBS123123123",
						"TREF240419151008",
						containerNumber,
						options.ContainerTypeCode,
						options.DataSourceKeys?.ElementAtOrDefault(index)))
			));
		}

		Shipment BuildSubShipment(string bookingConfirmationReferenceNumber, bool isPickup, string bookingPartyReferenceNumber, string transportReferenceNumber, string containerNumber, string containerTypeCode, string dataSourceKey)
		{
			var subShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.BookingConfirmationReference = bookingConfirmationReferenceNumber;
			subShipment.TransportBookingDirection = isPickup
				? new TransportBookingDirection { Code = "PIC", Description = "Pickup" }
				: new TransportBookingDirection { Code = "DLV", Description = "Delivery" };
			var bookingPartyReference = new AdditionalReference
			{
				Type = new EntryType { Code = "BPR", Description = "BookingPartyReference" },
				ReferenceNumber = bookingPartyReferenceNumber
			};
			var transportReference = new AdditionalReference
			{
				Type = new EntryType { Code = "TRF", Description = "TransportReference" },
				ReferenceNumber = transportReferenceNumber
			};
			subShipment.SetAdditionalReferenceCollection(() =>
			{
				var list = new DataObjectList<AdditionalReference>();
				list.Add(bookingPartyReference);
				if (!options.TransportReferenceIsNotFound)
				{
					list.Add(transportReference);
				}
				return list;
			});

			var containerCollection = new DataObjectList<Container>();
			var container = new Container
			{
				ContainerType = new ContainerType
				{
					Code = containerTypeCode
				},
			};
			if (containerNumber is not null)
			{
				container.ContainerNumber = containerNumber;
			}
			containerCollection.Add(container);
			subShipment.SetContainerCollection(() => containerCollection);

			if (options.PopulateBookingSlotTime)
			{
				subShipment.SetDateCollection(() => new List<Date>
				{
					new ()
					{
						Type = DateType.Start,
						Value = ZDateTime.Today,
					},
					new ()
					{
						Type = DateType.End,
						Value = ZDateTime.Today.AddHours(1),
					}
				});
			}

			subShipment.DataContext = DataContextFactory.New();
			var dataSources = new List<IDataSourceDataObject>();
			if (options.IsDataSourceGateVehicleMovement)
			{
				var dataSource = DataContextFactory.NewDataSource();
				dataSource.Key = dataSourceKey ?? Guid.NewGuid().ToString();
				dataSource.Type = nameof(DataContextType.GateMovementBooking);
				dataSources.Add(dataSource);

				dataSource = DataContextFactory.NewDataSource();
				dataSource.Key = dataSourceKey ?? Guid.NewGuid().ToString();
				dataSource.Type = nameof(DataContextType.GateMovement);
				dataSources.Add(dataSource);

				dataSource = DataContextFactory.NewDataSource();
				dataSource.Key = "GB00001234";
				dataSource.Type = nameof(DataContextType.GateBooking);
				dataSources.Add(dataSource);
			}
			else
			{
				var dataSource = DataContextFactory.NewDataSource();
				dataSource.Key = dataSourceKey ?? Guid.NewGuid().ToString();
				dataSource.Type = nameof(DataContextType.GateMovementBooking);
				dataSources.Add(dataSource);
			}
			dataSources.ForEach(dataSource => subShipment.DataContext.AddDataSource(dataSource));

			return subShipment;
		}
		void BuildRelatedShipmentCollection(Shipment shipment)
		{
			if (options.IsDataSourceGateVehicleMovement && options.IsIncoming)
			{
				shipment.SetRelatedShipmentCollection(() => new List<Shipment>());
				shipment.RelatedShipmentCollection.Add(GetVehicleEntryShipmentForTest());
			}
		}

		void BuildDateCollection(Shipment shipment)
		{
			shipment.SetDateCollection(() => new List<Date>
			{
				new Date()
				{
					Type = DateType.Start,
					Value = options.GateInTime ?? new ZDateTimeOffset(new ZDateTime(2024, 7, 1, 00, 00, 00), DateTimeKind.Local),
				}
			});
		}

		Shipment GetVehicleEntryShipmentForTest()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.TotalWeight = 0;
			shipment.TotalWeightUnit = new UnitOfWeight() { Code = "KG" };

			shipment.SetAddInfoCollection(() => new List<AddInfo>()
			{
				new AddInfo() { Key = "IsIncoming", Value = options.IsIncoming ? "true" : "false" }
			});

			return shipment;
		}
	}
	public class TestShipmentOptions
	{
		public bool YardIsNotFound { get; set; }
		public bool BookingPartyIsNotFound { get; set; }
		public bool TransportReferenceIsNotFound { get; set; }
		public bool IsForPickup { get; set; }
		public bool IsNeedToBuildPreCarriage { get; set; } = true;
		public bool IsNeedToBuildVehicleRun { get; set; }
		public IReadOnlyList<string> VehicleNumbers { get; set; }
		public IReadOnlyList<string> ContainerNumbers { get; set; }
		public string ContainerTypeCode { get; set; } = "20GP";
		public string BookingConfirmationReference { get; set; } = "BKR01";
		public bool PopulateBookingSlotTime { get; set; } = true;
		public bool IsDataSourceGateVehicleMovement { get; set; }
		public bool IsIncoming { get; set; }
		public bool DataSourceIsNotFound { get; set; }
		public bool UseUniqueSourceKey { get; set; }
		public ZDateTimeOffset? GateInTime { get; set; }
		public ZString? WarehouseLocation { get; set; }
		public IReadOnlyList<string> DataSourceKeys { get; set; }
	}
}
