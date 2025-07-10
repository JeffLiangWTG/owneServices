using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using EZC = Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer
{
	sealed class AirBookingRequestDataObjectWriter : DataObjectWriter<AirBookingRequest, UniversalShipment>
	{
		public AirBookingRequestDataObjectWriter(IDataWritingManager writeManager, MessageType messageType)
			: base(writeManager)
		{
			this.messageType = messageType;
		}

		readonly MessageType messageType;

		protected override UniversalShipment PopulateDataObject(AirBookingRequest airBookingRequest)
		{
			var uxmlShipment = new UniversalShipment(writeManager.WriterStrategy);

			var dataContext = airBookingRequest
				.CreateUXmlDataContext()
				.AddDataProvider()
				.AddUserBranchAndDepartment();

			dataContext.DocumentaryOverride = new DocumentaryOverride
			{
				DocumentName = "AirBooking"
			};

			uxmlShipment.DataContext = dataContext;

			uxmlShipment.BookingConfirmationReference = airBookingRequest.BookingReferenceNumber;

			uxmlShipment.WayBillNumber = airBookingRequest.MasterAirWaybillNumber;
			uxmlShipment.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, new WayBillTypeList());

			if (airBookingRequest.OriginAirport != null)
			{
				uxmlShipment.PortOfOrigin = new UNLOCO
				{
					Code = airBookingRequest.OriginAirport.IATACode,
					Name = airBookingRequest.OriginAirport.Name
				};
			}

			if (airBookingRequest.DestinationAirport != null)
			{
				uxmlShipment.PortOfDestination = new UNLOCO
				{
					Code = airBookingRequest.DestinationAirport.IATACode,
					Name = airBookingRequest.DestinationAirport.Name
				};
			}

			if (airBookingRequest.SpecialHandlingItems?.Count > 0)
			{
				uxmlShipment.SetSpecialHandlingCollection(() => airBookingRequest
					.SpecialHandlingItems
					.Select(sph => sph.ToUXmlCodeDescriptionPair())
					.ToList());
			}

			uxmlShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress(writeManager.WriterStrategy)
				{
					AddressType = (EZC.NoResString)"Airline", // programmatic constant
					CompanyName = airBookingRequest.Carrier?.Description,
					OrganizationCode = airBookingRequest.Carrier?.Code
				},
			});

			var address = new OrganizationAddress(writeManager.WriterStrategy)
			{
				AddressType = (EZC.NoResString)"Agent", // programmatic constant
				CompanyName = airBookingRequest.Agent,
			};

			address.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
			{
				new RegistrationNumber
				{
					Type = new RegistrationNumberType
					{
						Code = "CAS", // programmatic constant
						Description = (EZC.NoResString)"IATA CASS Number" // programmatic constant
					},
					Value = airBookingRequest.CASS // programmatic constant
				}
			});

			uxmlShipment.RequiresTemperatureControl = airBookingRequest.RequiresTemperatureControl;
			uxmlShipment.RequiredTemperatureMinimum = airBookingRequest.TemperatureMinimum?.Value;
			uxmlShipment.RequiredTemperatureMaximum = airBookingRequest.TemperatureMaximum?.Value;

			var requiredTemperatureUnit = airBookingRequest.TemperatureMinimum?.Unit;
			uxmlShipment.RequiredTemperatureUnit = requiredTemperatureUnit == null
				? null
				: new CodeDescriptionPair1Char { Code = requiredTemperatureUnit.Code, Description = requiredTemperatureUnit.Description };

			uxmlShipment.OrganizationAddressCollection.Add(address);
			PopulateTransportLegs(airBookingRequest, uxmlShipment);
			PopulateGoodsDetails(airBookingRequest, uxmlShipment);
			PopulateAdditionalDetails(airBookingRequest, uxmlShipment);
			PopulateAdditionalReferences(airBookingRequest, uxmlShipment);

			return uxmlShipment;
		}

		#region Transports

		void PopulateTransportLegs(AirBookingRequest airBookingRequest, UniversalShipment uxmlShipment)
		{
			if (airBookingRequest.FlightDetails == null
				|| airBookingRequest.FlightDetails.Count == 0)
			{
				return;
			}
			uxmlShipment.SetTransportLegCollection(() =>
			{
				var transports = new DataObjectList<TransportLeg>();

				var legOrder = 0;

				foreach (var flightDetail in airBookingRequest.FlightDetails)
				{
					legOrder = legOrder + 1;

					var transportLeg = new TransportLeg(writeManager.WriterStrategy)
					{
						LegOrder = (ZByte)legOrder,
						TransportMode = TransportMode.Air,
						LegType = new LegTypeConverter().ToEnumValue(flightDetail.TransportType?.Code ?? ZString.Empty),
						VoyageFlightNo = flightDetail.FlightNumber,
						BookingStatus = new CodeDescriptionPair
						{
							Code = messageType == MessageType.Withdrawal
								? Core.Constants.TransportStatus.CancellationRequested
								: Core.Constants.TransportStatus.Requested,
							Description = messageType == MessageType.Withdrawal
								? Core.Constants.TransportStatusDescriptions.CancellationRequested
								: Core.Constants.TransportStatusDescriptions.Requested
						},
						PortOfLoading = new UNLOCO
						{
							Code = flightDetail.PortOfLoading.IATACode,
							Name = flightDetail.PortOfLoading.Name
						},
						PortOfDischarge = new UNLOCO
						{
							Code = flightDetail.PortOfDischarge.IATACode,
							Name = flightDetail.PortOfDischarge.Name
						},
						EstimatedDeparture = flightDetail.ETD,
						EstimatedArrival = flightDetail.ETA
					};

					transportLeg.SetCustomizedFieldCollection(() => new List<CustomizedField>
					{
						new CustomizedField
						{
							DataType = DataType.String,
							Key = nameof(flightDetail.AllotmentId),
							Value = flightDetail.AllotmentId
						}
					});

					transports.Add(transportLeg);
				}

				return transports;
			});
		}

		#endregion Transports

		#region GoodsDetails

		void PopulateGoodsDetails(AirBookingRequest airBookingRequest, UniversalShipment uxmlShipment)
		{
			PopulateTotals(airBookingRequest, uxmlShipment);
			PopulateGoodDescription(airBookingRequest, uxmlShipment);
			PopulateAddInfoCollection(airBookingRequest, uxmlShipment);
			PopulateDimensions(airBookingRequest, uxmlShipment);
			PopulateULDs(airBookingRequest, uxmlShipment);
		}

		void PopulateTotals(AirBookingRequest airBookingRequest, UniversalShipment uxmlShipment)
		{
			uxmlShipment.TotalNoOfPacks = airBookingRequest.TotalPieces;

			uxmlShipment.TotalWeight = airBookingRequest.TotalWeight?.Value;
			uxmlShipment.TotalWeightUnit = new UnitOfWeight
			{
				Code = airBookingRequest.TotalWeight?.Unit?.Code,
				Description = airBookingRequest.TotalWeight?.Unit?.Description
			};

			uxmlShipment.TotalVolume = airBookingRequest.TotalVolume?.Value;
			uxmlShipment.TotalVolumeUnit = new UnitOfVolume
			{
				Code = airBookingRequest.TotalVolume?.Unit?.Code,
				Description = airBookingRequest.TotalVolume?.Unit?.Description
			};
		}

		void PopulateGoodDescription(AirBookingRequest airBookingRequest, UniversalShipment uxmlShipment)
		{
			uxmlShipment.GoodsDescription = airBookingRequest.GoodsDescription;
		}

		void PopulateAddInfoCollection(AirBookingRequest airBookingRequest, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();

				if (!airBookingRequest.Commodity.IsEmpty)
				{
					addInfos.Add(new AddInfo
					{
						Key = DocDataConstants.AddinfoTypes.CommodityCode,
						Value = airBookingRequest.Commodity
					});
				}

				if (airBookingRequest.ProductList != null)
				{
					var selectedProduct = airBookingRequest.ProductList
						.Cast<AirlineConfigProduct>()
						.FirstOrDefault(product => product.Description.Equals(airBookingRequest.Product));

					if (selectedProduct != null)
					{
						addInfos.Add(new AddInfo
						{
							Key = DocDataConstants.AddinfoTypes.ProductCode,
							Value = selectedProduct.Code
						});
					}
				}

				if (!airBookingRequest.CarrierBookingReference.IsEmpty)
				{
					addInfos.Add(new AddInfo
					{
						Key = DocDataConstants.AddinfoTypes.CarrierBookingReference,
						Value = airBookingRequest.CarrierBookingReference
					});
				}

				return addInfos.Any() ? addInfos : null;
			});
		}

		void PopulateDimensions(AirBookingRequest airBookingRequest, UniversalShipment uxmlShipment)
		{
			if (airBookingRequest.Dimensions == null
				|| airBookingRequest.Dimensions.Count == 0)
			{
				return;
			}
			uxmlShipment.SetPackingLineCollection(() =>
			{
				var packingLines = new DataObjectList<UniversalDataBuss.DataObjects.Universal.PackingLine>();

				foreach (var dimension in airBookingRequest.Dimensions)
				{
					var packingLine = new UniversalDataBuss.DataObjects.Universal.PackingLine
					{
						PackQty = new ZLong(dimension.Quantity),
						Length = dimension.Length.Value,
						LengthUnit = new UnitOfLength
						{
							Code = dimension.Length.Unit.Code,
							Description = dimension.Length.Unit.Description
						},
						Width = dimension.Width.Value,
						Height = dimension.Height.Value,
						Weight = dimension.Weight.Value,
						WeightUnit = new UnitOfWeight
						{
							Code = dimension.Weight.Unit.Code,
							Description = dimension.Weight.Unit.Description
						}
					};

					packingLines.Add(packingLine);
				}

				return packingLines;
			});
		}

		void PopulateULDs(AirBookingRequest airBookingRequest, UniversalShipment uxmlShipment)
		{
			if (airBookingRequest.Ulds == null
				|| airBookingRequest.Ulds.Count == 0)
			{
				return;
			}
			uxmlShipment.SetContainerCollection(() =>
			{
				var containers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();

				foreach (var uld in airBookingRequest.Ulds)
				{
					var container = new UniversalDataBuss.DataObjects.Universal.Container
					{
						ContainerCount = uld.ContainerCount,
						ContainerType = new UniversalDataBuss.DataObjects.Universal.ContainerType
						{
							Code = uld.Type.Code,
							Description = uld.Type.Description
						},
						ContainerNumber = uld.Number,
						TareWeight = uld.TareWeight.Value,
						GoodsWeight = uld.GoodsWeight.Value,
						GrossWeight = uld.GrossWeight.Value,
						WeightUnit = new UnitOfWeight
						{
							Code = uld.GrossWeight.Unit.Code,
							Description = uld.GrossWeight.Unit.Description
						},
						NonOperatingReefer = uld.IsNonOperativeReefer
					};

					containers.Add(container);
				}

				return containers;
			});
		}

		#endregion

		#region AdditionalDetails

		void PopulateAdditionalDetails(AirBookingRequest airBookingRequest, UniversalShipment uxmlShipment)
		{
			const string specialServiceRequestNoteType = "SpecialServiceRequest"; // Note Type description
			const string otherServiceInformationNoteType = "OtherServiceInformation"; // Note Type description

			uxmlShipment.SetNoteCollection(() =>
			{
				var notes = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Note>();

				var specialServiceRequest = new ZStringBuilder();
				specialServiceRequest.AppendIfNotEmpty(airBookingRequest.SpecialInstructions);
				specialServiceRequest.AppendIfNotEmpty(airBookingRequest.DangerousGoodsHandlingInformation);

				if (!specialServiceRequest.IsEmpty)
				{
					notes.Add(new UniversalDataBuss.DataObjects.Universal.Note
					{
						Description = specialServiceRequestNoteType,
						NoteText = specialServiceRequest.ToStringWithNewLineBetweenAppends()
					});
				}

				if (!airBookingRequest.GoodsHandlingInstructions.IsEmpty)
				{
					notes.Add(new UniversalDataBuss.DataObjects.Universal.Note
					{
						Description = otherServiceInformationNoteType,
						NoteText = airBookingRequest.GoodsHandlingInstructions
					});
				}

				if (notes.Count > 0)
				{
					return notes;
				}

				return uxmlShipment.NoteCollection;
			});
		}

		#endregion

		#region PopulateAdditionalReferences

		void PopulateAdditionalReferences(AirBookingRequest airBookingRequest, UniversalShipment uxmlShipment)
		{
			if (airBookingRequest.CarrierContractNumbers == null
				|| airBookingRequest.CarrierContractNumbers.Count == 0)
			{
				return;
			}
			uxmlShipment.SetAdditionalReferenceCollection(() =>
			{
				var numbers = new DataObjectList<AdditionalReference>();

				foreach (var number in airBookingRequest.CarrierContractNumbers)
				{
					numbers.Add(number.ToUXmlAdditionalReference());
				}

				return numbers;
			});
		}

		#endregion
	}
}
