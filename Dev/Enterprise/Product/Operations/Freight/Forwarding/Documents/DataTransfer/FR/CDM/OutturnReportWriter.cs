using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.FR
{
	sealed class OutturnReportWriter : DataObjectWriter<OutturnReport, UniversalShipment>
	{
		public OutturnReportWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(OutturnReport outturnReport)
		{
			var universalShipment = new UniversalShipment(writeManager.WriterStrategy);
			universalShipment.DataContext = outturnReport.CreateUXmlDataContext();

			universalShipment.ContainerMode = outturnReport.ContainerMode?.ToUXmlContainerMode();
			universalShipment.ShipmentType = outturnReport.ShipmentType?.ToUXmlCodeDescriptionPair();
			universalShipment.PortOfDestination = outturnReport.PortOfDestination.ToUXmlUnloco();
			universalShipment.PortOfOrigin = outturnReport.PortOfOrigin.ToUXmlUnloco();
			universalShipment.VesselName = outturnReport.VesselName;
			universalShipment.VoyageFlightNo = outturnReport.VoyageFlightNo;
			universalShipment.WayBillNumber = outturnReport.BillOfLading;

			PopulateAddresses(outturnReport, universalShipment);
			PopulateContainers(outturnReport, universalShipment);
			PopulateShipments(outturnReport, universalShipment);
			PopulateAddInfos(outturnReport, universalShipment);
			PopulateAdditionalReferences(outturnReport, universalShipment);

			return universalShipment;
		}

		#region PopulateAddresses

		void PopulateAddresses(OutturnReport outturnReport, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!outturnReport.SendingForwarder.IsEmpty())
				{
					addresses.Add(outturnReport.SendingForwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.SendingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(outturnReport.SendingForwarderCI5, outturnReport.SendingForwarderSON)));
				}

				if (!outturnReport.ReceivingForwarder.IsEmpty())
				{
					addresses.Add(outturnReport.ReceivingForwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.ReceivingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(outturnReport.ReceivingForwarderCI5, outturnReport.ReceivingForwarderSON)));
				}

				if (!outturnReport.CFS.IsEmpty())
				{
					addresses.Add(outturnReport.CFS.ToUXmlOrganizationAddress(nameof(DocAddressType.ArrivalCFSAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(outturnReport.CFSCI5, outturnReport.CFSSON, outturnReport.CFSSOW, outturnReport.CFSSOA)));
				}

				if (!outturnReport.SendingParty.IsEmpty())
				{
					addresses.Add(outturnReport.SendingParty.ToUXmlOrganizationAddress(nameof(DocAddressType.BookingPartyDocumentaryAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(outturnReport.SendingPartyCI5, outturnReport.SendingPartySON, outturnReport.SendingPartySOW, outturnReport.SendingPartySOA)));
				}

				if (!outturnReport.CurrentUser.IsEmpty())
				{
					addresses.Add(outturnReport.CurrentUser.ToUXmlOrganizationAddress("CurrentUser", writeManager.WriterStrategy, CreateRegistrationNumbers(outturnReport.CurrentUserCI5, outturnReport.CurrentUserSON, outturnReport.CurrentUserSOW, outturnReport.CurrentUserSOA)));
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

		void PopulateAddInfos(OutturnReport outturnReport, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();

				AddAddInfo(addInfos, nameof(outturnReport.ATPReference), outturnReport.ATPReference);

				if (!outturnReport.OperationalPort.IsEmpty())
				{
					addInfos.AddRange(outturnReport.OperationalPort.ToUXmlAddInfos(nameof(outturnReport.OperationalPort)));
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

		void PopulateAdditionalReferences(OutturnReport outturnReport, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = new DataObjectList<AdditionalReference>(CreateAdditionalReferences(outturnReport));
				return additionalReferences.Any() ? additionalReferences : null;
			});
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(OutturnReport outturnReport)
		{
			if (!outturnReport.ConsolNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = outturnReport.ConsolNumber,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					}
				};
			}
		}

		#endregion

		#region PopulateContainers

		void PopulateContainers(OutturnReport outturnReport, UniversalShipment uxmlShipment)
		{
			var containerLink = 0;
			var uxmlContainers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();

			if (outturnReport.Containers != null)
			{
				foreach (var container in outturnReport.Containers)
				{
					var uxmlContainer = ToUXmlContainer(container, writeManager.WriterStrategy);
					uxmlContainer.Link = ++containerLink;

					uxmlContainers.Add(uxmlContainer);
				}
			}

			uxmlShipment.SetContainerCollection(() => uxmlContainers);
		}

		UniversalDataBuss.DataObjects.Universal.Container ToUXmlContainer(BookingContainer source, IDataObjectWriterStrategy writerStrategy)
		{
			if (source == null)
			{
				return null;
			}

			var container = new UniversalDataBuss.DataObjects.Universal.Container(writerStrategy);
			container.ContainerNumber = source.Number;
			container.GrossWeight = source.GrossWeight?.Value;
			container.WeightUnit = source.GoodsWeight?.Unit.ToUXmlUnitOfWeight();
			container.ImportDepotCustomsReference = source.ImportDepotCustomsReference;
			container.NonOperatingReefer = source.IsNonOperativeReefer;

			container.SetAddInfoCollection(() =>
			{
				var addInfoCollection = CreateContainerAddInfos(source).ToList();
				return addInfoCollection.Any() ? addInfoCollection : null;
			});

			return container;
		}

		IEnumerable<AddInfo> CreateContainerAddInfos(BookingContainer container)
		{
			if (!container.LPDReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.LPDReference),
					Value = container.LPDReference
				};
			}

			if (!container.ImportDepotCustomsReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = "ICTReference",
					Value = container.ImportDepotCustomsReference
				};
			}

			if (!container.UnpackLocation.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.UnpackLocation),
					Value = container.UnpackLocation
				};
			}

			if (!container.UnpackArea.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(container.UnpackArea),
					Value = container.UnpackArea
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

			yield return new AddInfo
			{
				Key = DocDataConstants.AddinfoTypes.FormVersion,
				Value = "1.0.0" // Programatic version number
			};
		}

		#endregion

		#region PopulateShipments

		void PopulateShipments(OutturnReport outturnReport, UniversalShipment uxmlShipment)
		{
			var uxmlSubShipments = outturnReport.GoodsDetails?.Select(x => ToUXmlShipment(outturnReport, x, writeManager.WriterStrategy)).ToList()
							?? new List<UniversalShipment>();

			uxmlShipment.SetSubShipmentCollection(() => uxmlSubShipments.Any() ? new DataObjectList<UniversalShipment>(uxmlSubShipments) : null);
		}

		UniversalShipment ToUXmlShipment(OutturnReport outturnReport, GoodsDetail goodsDetail, IDataObjectWriterStrategy writerStrategy)
		{
			if (goodsDetail == null)
			{
				return null;
			}

			var universalShipment = new UniversalShipment(writerStrategy);

			universalShipment.WayBillNumber = goodsDetail.HouseBillNumber;

			universalShipment.SetPackingLineCollection(() =>
			{
				var uxmlPackingLines = CreateGoodsDetailsPackingLines(outturnReport, goodsDetail, writerStrategy);
				return uxmlPackingLines.Any() ? uxmlPackingLines : null;
			});

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

		DataObjectList<UniversalDataBuss.DataObjects.Universal.PackingLine> CreateGoodsDetailsPackingLines(OutturnReport outturnReport, GoodsDetail goodDetail, IDataObjectWriterStrategy writerStrategy)
		{
			var containerLink = 0;
			var uxmlPackingLines = new DataObjectList<UniversalDataBuss.DataObjects.Universal.PackingLine>();

			if (outturnReport.Containers != null)
			{
				foreach (var container in outturnReport.Containers)
				{
					containerLink++;
					foreach (var packingSummary in container.PackingSummaries.Where(p => p.ShipmentNumber == goodDetail.ShipmentNumber))
					{
						uxmlPackingLines.Add(ToUXmlPackingLine(packingSummary, writerStrategy, goodDetail.MarksAndNumbers, containerLink, container.Number));
					}
				}
			}

			return uxmlPackingLines;
		}

		UniversalDataBuss.DataObjects.Universal.PackingLine ToUXmlPackingLine(BookingPackingSummary summary, IDataObjectWriterStrategy writerStrategy, ZString marksAndNumbers, ZInt containerLink, ZString containerNumber)
		{
			if (summary == null)
			{
				return null;
			}

			var packingLine = new UniversalDataBuss.DataObjects.Universal.PackingLine(writerStrategy);
			packingLine.ContainerNumber = containerNumber;
			packingLine.ContainerLink = containerLink;

			packingLine.PackQty = new ZLong(summary.TotalPackages);
			packingLine.PackType = new PackageType
			{
				Code = summary.TotalPackagesUnit
			};
			packingLine.MarksAndNos = marksAndNumbers;
			packingLine.Weight = summary.TotalWeight?.Value;
			packingLine.WeightUnit = summary.TotalWeight?.Unit.ToUXmlUnitOfWeight();
			packingLine.Volume = summary.TotalVolume?.Value;
			packingLine.VolumeUnit = summary.TotalVolume?.Unit.ToUXmlUnitOfVolume();

			packingLine.OutturnQty = summary.TotalOutturnedPackages;
			packingLine.OutturnedWeight = summary.TotalOutturnedWeight?.Value;
			packingLine.OutturnedVolume = summary.TotalOutturnedVolume?.Value;
			packingLine.ImportReferenceNumber = summary.ICVReference;

			packingLine.SetAddInfoCollection(() =>
			{
				var addInfoCollection = CreatePackingSummaryAddInfo(summary).ToList();
				return addInfoCollection.Any() ? addInfoCollection : null;
			});

			return packingLine;
		}

		IEnumerable<AddInfo> CreatePackingSummaryAddInfo(BookingPackingSummary summary)
		{
			if (!summary.UnpackedReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(summary.UnpackedReference),
					Value = summary.UnpackedReference
				};
			}

			yield return new AddInfo
			{
				Key = nameof(summary.SurplusIndicator),
				Value = summary.SurplusIndicator.ToString()
			};

			yield return new AddInfo
			{
				Key = nameof(summary.UnpackingIndicator),
				Value = summary.UnpackingIndicator.ToString()
			};

			yield return new AddInfo
			{
				Key = nameof(summary.ReserveIndicator),
				Value = summary.ReserveIndicator.ToString()
			};

			yield return new AddInfo
			{
				Key = DocDataConstants.AddinfoTypes.FormVersion,
				Value = "1.0.0" // Programmatic version number
			};
		}

		IEnumerable<AddInfo> CreateGoodsDetailAddInfos(GoodsDetail goodDetail)
		{
			if (!goodDetail.CommodityReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(goodDetail.CommodityReference),
					Value = goodDetail.CommodityReference
				};
			}

			if (!goodDetail.MarksAndNumbers.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(goodDetail.MarksAndNumbers),
					Value = goodDetail.MarksAndNumbers
				};
			}

			yield return new AddInfo
			{
				Key = DocDataConstants.AddinfoTypes.FormVersion,
				Value = "1.0.0" // Programmatic version number
			};
		}

		IEnumerable<AdditionalReference> CreateGoodsDetailAdditionalReferences(GoodsDetail goodsDetail)
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
		}

		IEnumerable<UniversalDataBuss.DataObjects.Universal.Note> CreateGoodsDetailNotes(GoodsDetail goodDetail)
		{
			if (!goodDetail.MarksAndNumbers.IsEmpty)
			{
				yield return new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = PredefinedNoteTypes.Instance.MarksAndNumbers.MultilingualDescription.GetUnresolvedString(),
					NoteText = goodDetail.MarksAndNumbers
				};
			}
		}

		#endregion
	}
}
