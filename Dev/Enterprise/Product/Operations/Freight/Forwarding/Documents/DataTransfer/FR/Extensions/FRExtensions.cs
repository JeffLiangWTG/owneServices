using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using AdditionalService = Enterprise.UniversalDataBuss.DataObjects.Universal.AdditionalService;
using Container = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using PackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.FR
{
	static class FRExtensions
	{
		#region ToUXmlContainer

		public static Container ToUXmlContainer(this BookingContainer source, IDataObjectWriterStrategy writerStrategy)
		{
			if (source == null)
			{
				return null;
			}

			var container = new Container(writerStrategy);
			container.ContainerNumber = source.Number;
			container.ContainerCount = source.ContainerCount;
			container.IsShipperOwned = source.IsShipperOwned;
			container.IsEmptyContainer = source.IsEmpty;
			container.IsControlledAtmosphere = source.HasControlledAtmosphere;
			container.TempRecorderSerialNo = source.TemperatureRecorderSerialNumber;
			container.SetPointTemp = source.SetTemperature?.Value;
			container.SetPointTempUnit = source.SetTemperature?.Unit?.Code;
			container.HumidityPercent = Convert.ToByte((decimal)(source.Humidity?.Value ?? 0), CultureInfo.CurrentCulture);
			container.AirVentFlow = source.AirVentFlow?.Value;
			container.AirVentFlowRateUnit = source.AirVentFlow?.Unit?.ToUXmlCodeDescriptionPair();
			container.Seal = source.Seal;
			container.SealPartyType = source.SealPartyType.ToUXmlCodeDescriptionPair();
			container.SecondSeal = source.SecondSeal;
			container.SecondSealPartyType = source.SecondSealPartyType.ToUXmlCodeDescriptionPair();
			container.ThirdSeal = source.ThirdSeal;
			container.ThirdSealPartyType = source.ThirdSealPartyType.ToUXmlCodeDescriptionPair();
			container.GoodsWeight = source.GoodsWeight?.Value;
			container.TareWeight = source.TareWeight?.Value;
			container.DunnageWeight = source.Dunnage?.Value;
			container.GrossWeight = source.GrossWeight?.Value;
			container.WeightUnit = source.GoodsWeight?.Unit.ToUXmlUnitOfWeight();
			container.VolumeUnit = source.VolumeCapacity?.Unit.ToUXmlUnitOfVolume();
			container.GrossWeightVerificationDateTime = source.VerifiedDate;
			container.GrossWeightVerificationType = source.VerifiedMethod.ToUXmlCodeDescriptionPair();
			container.ContainerType = source.Type.ToUXmlContainerType();
			container.EmptyRequired = source.EmptyRequired;
			container.DepartureEstimatedPickup = source.DepartureEstimatedPickup;
			container.ArrivalDeliveryRequiredBy = source.ArrivalDeliveryRequiredBy;
			container.OverhangBack = source.OverhangBack?.Value;
			container.OverhangFront = source.OverhangFront?.Value;
			container.OverhangHeight = source.OverhangHeight?.Value;
			container.OverhangLeft = source.OverhangLeft?.Value;
			container.OverhangRight = source.OverhangRight?.Value;
			container.NonOperatingReefer = source.IsNonOperativeReefer;

			container.LengthUnit = new UnitOfLength()
			{
				Code = Core.Constants.Length.Feet,
				Description = Core.Constants.Length.GetDescription(Core.Constants.Length.Feet, Core.Constants.PluralState.Plural)
			};

			container.DepartureSlotDateTime = source.DepartureSlotDateTime;
			container.DeliveryMode = source.DeliveryMode;
			container.FCL_LCL_AIR = source.Mode != null ? new ContainerMode { Code = source.Mode.Code, Description = source.Mode.Description } : null;
			container.SetAdditionalServiceCollection(() => source.FumigationService != null ? new DataObjectList<AdditionalService> { source.FumigationService.ToUXmlAdditionalService(writerStrategy) } : null);

			container.SetAddInfoCollection(() =>
			{
				var addInfoCollection = CreateContainerAddInfos(source).ToList();
				return addInfoCollection.Any() ? addInfoCollection : null;
			});

			return container;
		}

		static IEnumerable<AddInfo> CreateContainerAddInfos(BookingContainer container)
		{
			yield return new AddInfo
			{
				Key = nameof(container.Fumigated),
				Value = container.Fumigated.ToString()
			};

			yield return new AddInfo
			{
				Key = nameof(container.OversizeContainer),
				Value = container.OversizeContainer.ToString()
			};

			yield return new AddInfo
			{
				Key = nameof(container.HazardousCargo),
				Value = container.HazardousCargo.ToString()
			};

			yield return new AddInfo
			{
				Key = nameof(container.MarinePollutant),
				Value = container.MarinePollutant.ToString()
			};

			if (!container.LDEReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = BookingContainer.LDEReferenceUXmlName,
					Value = container.LDEReference
				};
			}

			if (!container.LDEStatus.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.LDEStatus),
					Value = container.LDEStatus
				};
			}

			if (!container.DeliveryLocation.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.DeliveryLocation),
					Value = container.DeliveryLocation
				};
			}

			if (!container.DeliveryArea.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.DeliveryArea),
					Value = container.DeliveryArea
				};
			}

			if (!container.HandlingNotes.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.HandlingNotes),
					Value = container.HandlingNotes
				};
			}

			if (!container.DateOfArrival.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.DateOfArrival),
					Value = container.DateOfArrival.ToString()
				};
			}

			if (!(container.TransportMode?.Code ?? ZString.Empty).IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.TransportMode),
					Value = container.TransportMode.Code
				};
			}

			if (!container.VehicleRegistration.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.VehicleRegistration),
					Value = container.VehicleRegistration
				};
			}

			if (!container.PackingReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.PackingReference),
					Value = container.PackingReference
				};
			}

			if (!container.UnpackingReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.UnpackingReference),
					Value = container.UnpackingReference
				};
			}

			if (!container.AMQReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.AMQReference),
					Value = container.AMQReference
				};
			}

			if (!container.AMQAPPlusID.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.AMQAPPlusID),
					Value = container.AMQAPPlusID
				};
			}

			if (!(container.PortDuesPortCode?.Code ?? ZString.Empty).IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.PortDuesPortCode),
					Value = container.PortDuesPortCode.Code
				};
			}

			if (!container.PortDuesAmount.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.PortDuesAmount),
					Value = container.PortDuesAmount.ToString()
				};
			}

			if (!(container.PortDuesCurrency?.Code ?? ZString.Empty).IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.PortDuesCurrency),
					Value = container.PortDuesCurrency.Code
				};
			}

			if (!container.PortDuesPayingParty.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.PortDuesPayingParty),
					Value = container.PortDuesPayingParty
				};
			}

			if (!container.LPDStatus.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.LPDStatus),
					Value = container.LPDStatus
				};
			}

			if (!container.LPDReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.LPDReference),
					Value = container.LPDReference
				};
			}

			if (!container.UnpackingNotes.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.UnpackingNotes),
					Value = container.UnpackingNotes
				};
			}

			if (!container.ECTReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.ECTReference),
					Value = container.ECTReference
				};
			}

			yield return new AddInfo
			{
				Key = DocDataConstants.AddinfoTypes.FormVersion,
				Value = "1.0.0" // Programatic version number
			};
		}

		#endregion

		#region ToUXmlPackingLine

		public static PackingLine ToUXmlPackingLine(this BookingPackingLine source, IDataObjectWriterStrategy writerStrategy, Dictionary<ZGuid, int> packingLineContainerLinkMap = null)
		{
			if (source == null)
			{
				return null;
			}

			var packingLine = new PackingLine(writerStrategy);
			packingLine.PackingLineID = source.PackingLineID;
			packingLine.ContainerNumber = source.ContainerNumber;
			packingLine.GoodsDescription = source.GoodsDescription;
			packingLine.DetailedDescription = source.GoodsDescription;
			packingLine.PackQty = new ZLong(source.Quantity);
			packingLine.MarksAndNos = source.MarksAndNumbers;
			packingLine.HarmonisedCode = source.HarmonizedCode?.Code;
			packingLine.PackType = source.PackageType?.ToUXmlPackType();
			packingLine.Weight = source.Weight?.Value;
			packingLine.WeightUnit = source.Weight?.Unit.ToUXmlUnitOfWeight();
			packingLine.Volume = source.Volume?.Value;
			packingLine.VolumeUnit = source.Volume?.Unit.ToUXmlUnitOfVolume();
			packingLine.Height = source.Height?.Value;
			packingLine.Width = source.Width?.Value;
			packingLine.Length = source.Length?.Value;
			packingLine.LengthUnit = source.Length?.Unit.ToUXmlUnitOfLength();
			packingLine.ReferenceNumber = source.ReferenceNumber;
			packingLine.ImportReferenceNumber = source.ImportReferenceNumber;
			packingLine.ExportReferenceNumber = source.ExportReferenceNumber;
			packingLine.SetUNDGCollection(() => source.DangerousGoods?.Select(d => ToUXmlUNDG(d, writerStrategy)).ToList());
			packingLine.SetClassificationCollection(() => source.HarmonizedCodes != null ? new DataObjectList<Classification>(source.HarmonizedCodes.Select(c => c.ToUXmlClassification())) : null);

			packingLine.RequiresTemperatureControl = source.RequiresTemperatureControl;
			packingLine.RequiredTemperatureMinimum = source.TemperatureMinimum?.Value;
			packingLine.RequiredTemperatureMaximum = source.TemperatureMaximum?.Value;

			var requiredTemperatureUnit = source.TemperatureMinimum?.Unit;
			packingLine.RequiredTemperatureUnit = requiredTemperatureUnit == null
				? null
				: new CodeDescriptionPair1Char { Code = requiredTemperatureUnit.Code, Description = requiredTemperatureUnit.Description };

			if (packingLineContainerLinkMap != null && packingLineContainerLinkMap.TryGetValue((ZGuid)source.Identifier, out int containerLink))
			{
				packingLine.ContainerLink = containerLink;
			}

			return packingLine;
		}

		#endregion

		#region ToUXmlUNDG

		static UNDG ToUXmlUNDG(IDangerousGood source, IDataObjectWriterStrategy writerStrategy)
		{
			if (source == null)
			{
				return null;
			}

			var undg = new UNDG(writerStrategy);
			undg.UNDGCode = source.Unno;
			undg.Contact = source.Contact.ToUXmlContact();
			undg.ProperShippingName = source.ProperShippingName;
			undg.TechicalName = source.TechnicalName;
			undg.PackQty = source.Quantity;
			undg.PackedInLimitedQuantity = source.PackedInLimitedQuantity;
			undg.IMOClass = source.IMOClass;
			undg.PackingGroup = source.PackingGroup;
			undg.SubLabel1 = source.SubLabel1;
			undg.SubLabel2 = source.SubLabel2;
			undg.Weight = source.Weight?.Value;
			undg.State = new UNDGStateConverter().ToEnumValue(source.State);
			undg.WeightUQ = new UnitOfWeight { Code = source.Weight?.Unit?.Code, Description = source.Weight?.Unit?.Description };
			undg.Volume = source.Volume?.Value;
			undg.VolumeUQ = new UnitOfVolume { Code = source.Volume?.Unit?.Code, Description = source.Volume?.Unit?.Description };
			undg.PackType = new PackageType { Code = source.PackageType?.Code, Description = source.PackageType?.Description };
			undg.MarinePollutant = new UNDGMarinePollutant { Code = source.MarinePollutant?.Code, Description = source.MarinePollutant?.Description };

			if (source.FlashPoint != null)
			{
				undg.FlashPoint = source.FlashPoint?.Value.ToString();
			}

			return undg;
		}

		#endregion

		#region ToUXmlShipment

		public static UniversalShipment ToUXmlShipment(this GoodsDetail goodsDetail, Dictionary<ZGuid, int> packingLineContainerLinkMap, IDataObjectWriterStrategy writerStrategy)
		{
			if (goodsDetail == null)
			{
				return null;
			}

			var universalShipment = new UniversalShipment(writerStrategy);

			universalShipment.WayBillNumber = goodsDetail.HouseBillNumber;
			universalShipment.GoodsDescription = goodsDetail.GoodsDescription;
			universalShipment.PortOfOrigin = goodsDetail.PortOfOrigin.ToUXmlUnloco();
			universalShipment.RequiresTemperatureControl = goodsDetail.RequiresTemperatureControl;

			universalShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = CreateAGoodsDetailAddresses(goodsDetail, writerStrategy).ToList();
				return addresses.Any() ? addresses : null;
			});

			universalShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(goodsDetail.PackingLines.Select(x => x.ToUXmlPackingLine(writerStrategy, packingLineContainerLinkMap))));

			universalShipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = new DataObjectList<AdditionalReference>(CreateGoodsDetailAdditionalReferences(goodsDetail));
				return additionalReferences.Any() ? additionalReferences : null;
			});

			universalShipment.SetAddInfoCollection(() =>
			{
				var addInfoCollection = CreateGoodsDetailAddInfos(goodsDetail).ToList();
				return addInfoCollection.Any() ? addInfoCollection : null;
			});

			universalShipment.SetNoteCollection(() =>
			{
				var noteCollection = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Note>(CreateGoodsDetailNotes(goodsDetail));
				return noteCollection.Any() ? noteCollection : null;
			});

			return universalShipment;
		}

		static IEnumerable<OrganizationAddress> CreateAGoodsDetailAddresses(GoodsDetail goodsDetail, IDataObjectWriterStrategy writerStrategy)
		{
			if (!goodsDetail.Shipper.IsEmpty())
			{
				yield return goodsDetail.Shipper.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress), writerStrategy);
			}

			if (!goodsDetail.Consignee.IsEmpty())
			{
				yield return goodsDetail.Consignee.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress), writerStrategy);
			}

			if (!goodsDetail.NotifyParty.IsEmpty())
			{
				yield return goodsDetail.NotifyParty.ToUXmlOrganizationAddress(nameof(DocAddressType.NotifyParty), writerStrategy);
			}

			if (!goodsDetail.NotifyParty2.IsEmpty())
			{
				yield return goodsDetail.NotifyParty2.ToUXmlOrganizationAddress(nameof(DocAddressType.NotifyParty2), writerStrategy);
			}
		}

		static IEnumerable<AdditionalReference> CreateGoodsDetailAdditionalReferences(GoodsDetail goodsDetail)
		{
			if (!goodsDetail.ShipmentNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = goodsDetail.ShipmentNumber,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					}
				};
			}

			if (!goodsDetail.ConsignmentNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = goodsDetail.ConsignmentNumber,
					Type = new EntryType
					{
						Code = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC,
						Description = ForwardingPackingLineAdditionalReferenceNumberTypes.Descriptions.ERC
					}
				};
			}
		}

		static IEnumerable<AddInfo> CreateGoodsDetailAddInfos(GoodsDetail goodDetail)
		{
			yield return new AddInfo
			{
				Key = nameof(goodDetail.HazardousCargo),
				Value = goodDetail.HazardousCargo.ToString()
			};

			if (goodDetail.ShowAppliesToAllPacks)
			{
				yield return new AddInfo
				{
					Key = nameof(goodDetail.AppliesToAllPacks),
					Value = goodDetail.AppliesToAllPacks.ToString()
				};
			}

			if (!goodDetail.DeclaredPackCount.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(goodDetail.DeclaredPackCount),
					Value = goodDetail.DeclaredPackCount.ToString()
				};
			}

			if (goodDetail.DeclaredPackWeight != null && !goodDetail.DeclaredPackWeight.Value.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(goodDetail.DeclaredPackWeight),
					Value = goodDetail.DeclaredPackWeight.Value.ToString()
				};
			}

			if (!goodDetail.DeclarationReferenceNumber.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = GoodsDetail.DeclarationReferenceNumberUXmlName,
					Value = goodDetail.DeclarationReferenceNumber
				};
			}

			if (!goodDetail.CommodityLineRef.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(goodDetail.CommodityLineRef),
					Value = goodDetail.CommodityLineRef
				};
			}

			if (!goodDetail.CommodityReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(goodDetail.CommodityReference),
					Value = goodDetail.CommodityReference
				};
			}

			if (!goodDetail.TaxReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(goodDetail.TaxReference),
					Value = goodDetail.TaxReference
				};
			}

			if (goodDetail.TaxAmount != null)
			{
				yield return new AddInfo
				{
					Key = nameof(goodDetail.TaxAmount),
					Value = goodDetail.TaxAmount.Amount.ToString()
				};

				yield return new AddInfo
				{
					Key = "TaxCurrency",
					Value = goodDetail.TaxAmount.Currency?.Code
				};
			}

			if (!goodDetail.TaxStatus.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(goodDetail.TaxStatus),
					Value = goodDetail.TaxStatus
				};
			}

			yield return new AddInfo
			{
				Key = DocDataConstants.AddinfoTypes.FormVersion,
				Value = "1.0.0" // Programmatic version number
			};
		}

		static IEnumerable<UniversalDataBuss.DataObjects.Universal.Note> CreateGoodsDetailNotes(GoodsDetail goodDetail)
		{
			if (!goodDetail.MarksAndNumbers.IsEmpty)
			{
				yield return new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = PredefinedNoteTypes.Instance.MarksAndNumbers.MultilingualDescription.GetUnresolvedString(),
					NoteText = goodDetail.MarksAndNumbers
				};
			}

			if (!goodDetail.GoodsHandlingNotes.IsEmpty)
			{
				yield return new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = (NoResString)"Goods Handling Notes", // Programmatic constant
					NoteText = goodDetail.GoodsHandlingNotes
				};
			}
		}

		#endregion
	}
}
