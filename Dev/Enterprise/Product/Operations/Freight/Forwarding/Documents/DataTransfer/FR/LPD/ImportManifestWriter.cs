using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.FR
{
	sealed class ImportManifestWriter : DataObjectWriter<ImportManifest, UniversalShipment>
	{
		public ImportManifestWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(ImportManifest importManifest)
		{
			var universalShipment = new UniversalShipment(writeManager.WriterStrategy);
			universalShipment.DataContext = importManifest.CreateUXmlDataContext();

			universalShipment.ContainerMode = importManifest.ContainerMode?.ToUXmlContainerMode();
			universalShipment.ShipmentType = importManifest.ShipmentType?.ToUXmlCodeDescriptionPair();
			universalShipment.PortOfDestination = importManifest.PortOfDestination.ToUXmlUnloco();
			universalShipment.PortOfOrigin = importManifest.PortOfOrigin.ToUXmlUnloco();
			universalShipment.PortFirstForeign = importManifest.PortOfTranshipment.ToUXmlUnloco();
			universalShipment.VesselName = importManifest.VesselName;
			universalShipment.VoyageFlightNo = importManifest.VoyageNumber;
			universalShipment.WayBillNumber = importManifest.BillOfLadingNumber;

			PopulateAddresses(importManifest, universalShipment);
			PopulateContainers(importManifest, universalShipment);
			PopulateShipments(importManifest, universalShipment);
			PopulateAddInfos(importManifest, universalShipment);
			PopulateAdditionalReferences(importManifest, universalShipment);
			PopulateDates(importManifest, universalShipment);

			return universalShipment;
		}

		#region PopulateAddresses

		void PopulateAddresses(ImportManifest importManifest, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!importManifest.SendingForwarder.IsEmpty())
				{
					addresses.Add(importManifest.SendingForwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.SendingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(importManifest.SendingForwarderCI5, importManifest.SendingForwarderSON)));
				}

				if (!importManifest.ReceivingForwarder.IsEmpty())
				{
					addresses.Add(importManifest.ReceivingForwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.ReceivingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(importManifest.ReceivingForwarderCI5, importManifest.ReceivingForwarderSON)));
				}

				if (!importManifest.Transporter.IsEmpty())
				{
					addresses.Add(importManifest.Transporter.ToUXmlOrganizationAddress(nameof(DocAddressType.ArrivalCFSLocalTransportAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(importManifest.TransporterCI5, importManifest.TransporterSON)));
				}

				if (!importManifest.Carrier.IsEmpty())
				{
					addresses.Add(importManifest.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.ShippingLineAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(importManifest.CarrierCI5, importManifest.CarrierSON)));
				}

				if (!importManifest.SendingParty.IsEmpty())
				{
					addresses.Add(importManifest.SendingParty.ToUXmlOrganizationAddress("CurrentUser", writeManager.WriterStrategy, CreateRegistrationNumbers(importManifest.SendingPartyCI5, importManifest.SendingPartySON)));
				}

				return addresses.Any() ? addresses : null;
			});
		}

		IEnumerable<RegistrationNumber> CreateRegistrationNumbers(params DocumentVisualizer.DocDataObjects.RegistrationNumber[] registrationNumbers)
		{
			foreach (var number in registrationNumbers.Where(x => x != null && !x.Value.IsEmpty))
			{
				yield return number.ToUXmlRegistrationNumber();
			}
		}

		#endregion

		#region PopulateAddInfos

		void PopulateAddInfos(ImportManifest importManifest, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();

				AddAddInfo(addInfos, nameof(importManifest.PortLocation), importManifest.PortLocation);
				AddAddInfo(addInfos, nameof(importManifest.PortArea), importManifest.PortArea);
				AddAddInfo(addInfos, nameof(importManifest.ATPReference), importManifest.ATPReference);
				AddAddInfo(addInfos, nameof(importManifest.OTCReference), importManifest.OTCReference);

				if (!importManifest.OperationalPort.IsEmpty())
				{
					addInfos.AddRange(importManifest.OperationalPort.ToUXmlAddInfos(nameof(importManifest.OperationalPort)));
				}

				if (addInfos.Any())
				{
					addInfos.Add(new AddInfo { Key = DocDataConstants.AddinfoTypes.FormVersion, Value = "1.0.0" }); // Programmatic version number
				}

				return addInfos.Any() ? addInfos : null;
			});
		}

		void AddAddInfo(List<AddInfo> addinfos, ZString key, ZString value)
		{
			if (!value.IsEmpty)
			{
				addinfos.Add(new AddInfo
				{
					Key = key,
					Value = value
				});
			}
		}

		#endregion

		#region PopulateAdditionalReferences

		void PopulateAdditionalReferences(ImportManifest importManifest, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = new DataObjectList<AdditionalReference>(CreateAdditionalReferences(importManifest));
				return additionalReferences.Any() ? additionalReferences : null;
			});
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(ImportManifest importManifest)
		{
			if (!importManifest.ConsolNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = importManifest.ConsolNumber,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					}
				};
			}
		}

		#endregion

		#region PopulateDates

		void PopulateDates(ImportManifest importManifest, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetDateCollection(() =>
			{
				var dates = CreateDates(importManifest).ToList();
				return dates.Any() ? dates : null;
			});
		}

		IEnumerable<Date> CreateDates(ImportManifest importManifest)
		{
			if (!importManifest.ETA.IsEmpty)
			{
				yield return new Date
				{
					Type = DateType.Arrival,
					Value = importManifest.ETA,
					IsEstimate = true
				};
			}
		}

		#endregion

		#region PopulateContainers

		void PopulateContainers(ImportManifest importManifest, UniversalShipment uxmlShipment)
		{
			var containerLink = 0;
			var uxmlContainers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();
			PackingLineContainerLinkMap = new Dictionary<ZGuid, int>();

			if (importManifest.Containers != null)
			{
				foreach (var container in importManifest.Containers)
				{
					var uxmlContainer = container.ToUXmlContainer(writeManager.WriterStrategy);
					uxmlContainer.Link = ++containerLink;

					foreach (BookingPackingLine packingLine in container.PackingLines)
					{
						PackingLineContainerLinkMap.Add((ZGuid)packingLine.Identifier, containerLink);
					}
					uxmlContainers.Add(uxmlContainer);
				}
			}

			uxmlShipment.SetContainerCollection(() => uxmlContainers);
		}

		#endregion

		#region PopulateShipments

		void PopulateShipments(ImportManifest importManifest, UniversalShipment uxmlShipment)
		{
			var uxmlSubShipments = importManifest.GoodsDetails?.OrderBy(x => x.ShipmentNumber).Select(x => x.ToUXmlShipment(PackingLineContainerLinkMap, writeManager.WriterStrategy)).ToList() ?? new List<UniversalShipment>();

			for (int i = 0; i < uxmlSubShipments.Count; i++)
			{
				var uxmlSubShipment = uxmlSubShipments[i];
				uxmlSubShipment.RequiredTemperatureMinimum = importManifest.TemperatureMinimum?.Value;
				uxmlSubShipment.RequiredTemperatureMaximum = importManifest.TemperatureMaximum?.Value;

				var requiredTemperatureUnit = importManifest.TemperatureMinimum?.Unit;
				uxmlSubShipment.RequiredTemperatureUnit = requiredTemperatureUnit == null
					? null
					: new CodeDescriptionPair1Char { Code = requiredTemperatureUnit.Code, Description = requiredTemperatureUnit.Description };

				var goodsInfo = importManifest.GoodsDetails.OrderBy(x => x.ShipmentNumber).ToList()[i];
				PopulateGoodsDetailAddresses(goodsInfo, uxmlSubShipment);
				PopulatePackingLine(goodsInfo, uxmlSubShipment);
			}

			uxmlShipment.SetSubShipmentCollection(() => uxmlSubShipments.Any() ? new DataObjectList<UniversalShipment>(uxmlSubShipments) : null);
		}

		void PopulateGoodsDetailAddresses(GoodsDetail goodsInfo, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!goodsInfo.Shipper.IsEmpty())
				{
					addresses.Add(goodsInfo.Shipper.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress), writeManager.WriterStrategy));
				}

				if (!goodsInfo.Consignee.IsEmpty())
				{
					addresses.Add(goodsInfo.Consignee.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress), writeManager.WriterStrategy));
				}

				if (!goodsInfo.NotifyParty.IsEmpty())
				{
					addresses.Add(goodsInfo.NotifyParty.ToUXmlOrganizationAddress(nameof(DocAddressType.NotifyParty), writeManager.WriterStrategy));
				}

				if (!goodsInfo.NotifyParty2.IsEmpty())
				{
					addresses.Add(goodsInfo.NotifyParty2.ToUXmlOrganizationAddress(nameof(DocAddressType.NotifyParty2), writeManager.WriterStrategy));
				}

				if (!goodsInfo.ThirdParty.IsEmpty())
				{
					addresses.Add(goodsInfo.ThirdParty.ToUXmlOrganizationAddress(nameof(DocAddressType.DeliveryAgent), writeManager.WriterStrategy, CreateRegistrationNumbers(goodsInfo.ThirdPartyCI5, goodsInfo.ThirdPartySON)));
				}

				return addresses.Any() ? addresses : null;
			});
		}

		int GetContainerLink(object identifier)
		{
			var res = 0;

			if (PackingLineContainerLinkMap != null && PackingLineContainerLinkMap.TryGetValue((ZGuid)identifier, out int containerLink))
			{
				res = containerLink;
			}

			return res;
		}

		void PopulatePackingLine(GoodsDetail goodsInfo, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetPackingLineCollection(() =>
			{
				var uxmlPackingLines = new DataObjectList<UniversalDataBuss.DataObjects.Universal.PackingLine>();
				var packlineGroup = goodsInfo.PackingLines.GroupBy(x => GetContainerLink(x.Identifier));
				foreach (var packline in packlineGroup)
				{
					var selectedPackline = packline.ToList();
					var firstPackline = selectedPackline.FirstOrDefault();

					var packingLine = new UniversalDataBuss.DataObjects.Universal.PackingLine(writeManager.WriterStrategy)
					{
						ContainerLink = packline.Key,
						ContainerNumber = firstPackline?.ContainerNumber,
						GoodsDescription = goodsInfo.GoodsDescription,
						DetailedDescription = goodsInfo.GoodsDescription,
						PackQty = new ZLong(selectedPackline?.Sum(x => x.Quantity)),
						MarksAndNos = goodsInfo.MarksAndNumbers,
						HarmonisedCode = firstPackline?.HarmonizedCode?.Code,
						PackType = firstPackline?.PackageType?.ToUXmlPackType(),
						Weight = selectedPackline?.Sum(x => x.Weight.Value),
						WeightUnit = firstPackline?.Weight?.Unit?.ToUXmlUnitOfWeight(),
						Volume = selectedPackline?.Sum(x => x.Volume.Value),
						VolumeUnit = firstPackline?.Volume?.Unit?.ToUXmlUnitOfVolume(),
						ReferenceNumber = firstPackline?.ReferenceNumber,
						ImportReferenceNumber = firstPackline?.ImportReferenceNumber,
						ExportReferenceNumber = firstPackline?.ExportReferenceNumber,
						RequiresTemperatureControl = firstPackline?.RequiresTemperatureControl,
						RequiredTemperatureMinimum = firstPackline?.TemperatureMinimum?.Value,
						RequiredTemperatureMaximum = firstPackline?.TemperatureMaximum?.Value
					};

					PopulateUNDGs(selectedPackline, packingLine);

					uxmlPackingLines.Add(packingLine);
				}

				return uxmlPackingLines.Any() ? uxmlPackingLines : null;
			});
		}

		void PopulateUNDGs(IEnumerable<BookingPackingLine> selectedPackline, UniversalDataBuss.DataObjects.Universal.PackingLine packingLine)
		{
			packingLine.SetUNDGCollection(() =>
			{
				var uxmlUNDGs = new List<UNDG>();
				var dangerousGoods = selectedPackline?.SelectMany(x => x.DangerousGoods)?.GroupBy(x => new { x.Unno, x.Contact, x.FlashPoint }).Select(x => x.FirstOrDefault());
				foreach (var dangerousGood in dangerousGoods)
				{
					var undg = new UNDG(writeManager.WriterStrategy)
					{
						UNDGCode = dangerousGood.Unno,
						Contact = dangerousGood.Contact.ToUXmlContact(),
						ProperShippingName = dangerousGood.ProperShippingName,
						TechicalName = dangerousGood.TechnicalName,
						PackQty = dangerousGood.Quantity,
						PackedInLimitedQuantity = dangerousGood.PackedInLimitedQuantity,
						IMOClass = dangerousGood.IMOClass,
						PackingGroup = dangerousGood.PackingGroup,
						SubLabel1 = dangerousGood.SubLabel1,
						SubLabel2 = dangerousGood.SubLabel2,
						Weight = dangerousGood.Weight?.Value,
						State = new UNDGStateConverter().ToEnumValue(dangerousGood.State),
						WeightUQ = new UnitOfWeight { Code = dangerousGood.Weight?.Unit?.Code, Description = dangerousGood.Weight?.Unit?.Description },
						Volume = dangerousGood.Volume?.Value,
						VolumeUQ = new UnitOfVolume { Code = dangerousGood.Volume?.Unit?.Code, Description = dangerousGood.Volume?.Unit?.Description },
						PackType = new PackageType { Code = dangerousGood.PackageType?.Code, Description = dangerousGood.PackageType?.Description },
						MarinePollutant = new UNDGMarinePollutant { Code = dangerousGood.MarinePollutant?.Code, Description = dangerousGood.MarinePollutant?.Description },
					};

					if (dangerousGood.FlashPoint != null)
					{
						undg.FlashPoint = dangerousGood.FlashPoint?.Value.ToString();
					}

					uxmlUNDGs.Add(undg);
				}

				return uxmlUNDGs.Any() ? uxmlUNDGs : null;
			});
		}

		Dictionary<ZGuid, int> PackingLineContainerLinkMap;

		#endregion
	}
}
