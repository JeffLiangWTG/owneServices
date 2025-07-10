using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions.Customs;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitReceiveTransportationUnitDataObjectWriter : TopLevelDataObjectWriter<WhsItemReceiveTransportationUnit, UniversalShipment>
	{
		public WhsTransitReceiveTransportationUnitDataObjectWriter(IDataWritingManager manager, bool shouldPopulateConsignments = true)
			: base(manager)
		{
			this.shouldPopulateConsignments = shouldPopulateConsignments;
		}

		readonly bool shouldPopulateConsignments;

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.TransitReceiveHeader;
		}

		protected override void PopulateDataObject(WhsItemReceiveTransportationUnit receiveTransportationUnit, UniversalShipment dataObject)
		{
			dataObject.WarehouseLocation = receiveTransportationUnit.Location?.ToLocationString();
			dataObject.VesselName = receiveTransportationUnit.WRH_VehicleReference;

			PopulatePackages(receiveTransportationUnit, dataObject);
			PopulateConsignments(receiveTransportationUnit, dataObject);
			PopulateAddresses(receiveTransportationUnit, dataObject);
			PopulateDates(receiveTransportationUnit, dataObject);
			PopulateNotes(receiveTransportationUnit, dataObject);
			PopulateReferences(receiveTransportationUnit, dataObject);
			PopulateVehicleRun(receiveTransportationUnit, dataObject);
			PopulateSeaCargoOutturnDataForMatching(receiveTransportationUnit, dataObject);
		}

		void PopulateSeaCargoOutturnDataForMatching(WhsItemReceiveTransportationUnit receiveTransportationUnit, UniversalShipment dataObject)
		{
			var asnFromPivotAndPackageStates = (receiveTransportationUnit.ReceiveASNs ?? Enumerable.Empty<WhsItemReceiveASN>()).Union(receiveTransportationUnit.ReceiveASNsFromPackageStates ?? Enumerable.Empty<WhsItemReceiveASN>());
			var asnFromSeaCargoOutturn = asnFromPivotAndPackageStates
				.Where(a => a != null && !a.VesselLloydsNumber.IsEmpty)
				.OrderByDescending(a => a.WRP_SystemCreateTimeUtc)
				.FirstOrDefault();

			if (asnFromSeaCargoOutturn != null)
			{
				if (string.IsNullOrEmpty(dataObject.VesselName))
				{
					dataObject.VesselName = asnFromSeaCargoOutturn.VesselName;
				}

				dataObject.LloydsIMO = asnFromSeaCargoOutturn.VesselLloydsNumber;
				dataObject.VoyageFlightNo = asnFromSeaCargoOutturn.VoyageNumber;
				dataObject.TransportMode = new CodeDescriptionPair
				{
					Code = TransportModes.Sea,
					Description = TransportModeDescriptions.Sea
				};

				// for sea cargo outturn overwrite the existing container collection
				dataObject.SetContainerCollection(() =>
				new DataObjectList<Container>
				{
					new Container()
					{
						ContainerNumber = receiveTransportationUnit.WRH_VehicleReference,
						IsSealOk = receiveTransportationUnit.WRH_IsVehicleSecure,
						LCLUnpack = receiveTransportationUnit.WRH_UnloadCompleteTime.ToZDateTime()
					}
				});

				var additionalReferenceWithPremiseID = new AdditionalReference
				{
					Type = new EntryType
					{
						Code = CustomsAdditionalReferenceTypes.EntryType.Codes.ControlledPremiseID,
						Description = CustomsConstants.AdditionalReference.EntryType.Descriptions.ControlledPremiseID
					},
					ContextInformation = receiveTransportationUnit.Warehouse.WarehouseAddress.Country.RN_Code,
					ReferenceNumber = TransitWarehouseHelper.GetPremiseIDFortWarehouse(receiveTransportationUnit.Warehouse)
				};

				if (dataObject.AdditionalReferenceCollection != null)
				{
					dataObject.AdditionalReferenceCollection.Add(additionalReferenceWithPremiseID);
				}
				else
				{
					dataObject.SetAdditionalReferenceCollection(() =>
						new DataObjectList<AdditionalReference>
						{
							additionalReferenceWithPremiseID
						});
				}
			}
		}

		void PopulatePackages(WhsItemReceiveTransportationUnit receiveTransportationUnit, UniversalShipment dataObject)
		{
			var packages = receiveTransportationUnit.PackageJob.Packages;
			var packagesExcludingTrucks = packages.Where(p => p.IsContainer || !p.PackageExtensions.Any());
			var helper = new PkgPackageJobDataObjectWriterHelper(writeManager, packagesExcludingTrucks, receiveTransportationUnit);
			helper.PopulateDataObject(dataObject);
			if (dataObject.PackingLineCollection == null)
			{
				dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { Content = CollectionContent.Complete });
			}
		}

		void PopulateConsignments(WhsItemReceiveTransportationUnit receiveTransportationUnit, UniversalShipment dataObject)
		{
			if (shouldPopulateConsignments)
			{
				var receiveConsignmentsFromASN = receiveTransportationUnit.Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRH_TransitReceiveTransportationUnit, receiveTransportationUnit.PK))
				.SelectMany(r => r.ReceiveASN.ReceiveConsignments)
				.Where(r => r != null)
				.Distinct();

				var receiveConsignmentFromRTU = receiveTransportationUnit.PackageStates
					.Select(ps => ps.ReceiveConsignment)
					.Where(r => r != null)
					.Distinct();

				var receiveConsignments = receiveConsignmentsFromASN.Union(receiveConsignmentFromRTU);
				var consignmentWriter = new WhsTransitReceiveConsignmentDataObjectWriter(writeManager, receiveTransportationUnit, dataObject.ContainerCollection?.FirstOrDefault());

				if (receiveConsignments.Any())
				{
					dataObject.SetSubShipmentCollection(() =>
					{
						var result = new DataObjectList<UniversalShipment>();
						foreach (var receiveConsignment in receiveConsignments)
						{
							var consignmentDataObject = consignmentWriter.GetDataObject(receiveConsignment);
							result.Add(consignmentDataObject);
						}
						return result;
					});
				}
			}
		}

		void PopulateDates(WhsItemReceiveTransportationUnit receiveTransportationUnit, UniversalShipment dataObject)
		{
			if (receiveTransportationUnit.WRH_UnloadCompleteTime.IsValid)
			{
				dataObject.SetDateCollection(() => new List<Date>(new[] { Date.New(DateType.Unpack, ZBool.False, receiveTransportationUnit.WRH_UnloadCompleteTime.ToZDateTime()) }));
			}
		}

		void PopulateAddresses(WhsItemReceiveTransportationUnit receiveTransportationUnit, UniversalShipment dataObject)
		{
			var writer = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.LocalCartageCFS));
			var warehouseOrganiastionAddress = writer.GetDataObject(receiveTransportationUnit.Warehouse.WarehouseAddress);

			dataObject.SetOrganizationAddressCollection(() => ProcessCollection(receiveTransportationUnit.DocAddresses, new JobDocAddressDataObjectWriter(writeManager)));
			if (dataObject.OrganizationAddressCollection != null)
			{
				dataObject.OrganizationAddressCollection.Add(warehouseOrganiastionAddress);
			}
			else
			{
				dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { warehouseOrganiastionAddress });
			}
		}

		void PopulateNotes(WhsItemReceiveTransportationUnit receiveTransportationUnit, UniversalShipment dataObject)
		{
			dataObject.SetNoteCollection(() =>
			{
				var notes = receiveTransportationUnit.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
				return ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial);
			});
		}

		void PopulateReferences(WhsItemReceiveTransportationUnit receiveTransportationUnit, UniversalShipment dataObject)
		{
			dataObject.SetAdditionalReferenceCollection(() => ProcessCollection(receiveTransportationUnit.AdditionalReferenceNumbers, new AdditionalReferenceDataObjectWriter(writeManager), CollectionContent.Partial));
		}

		void PopulateVehicleRun(WhsItemReceiveTransportationUnit receiveTransportationUnit, UniversalShipment dataObject)
		{
			var signedBy = "";
			if (receiveTransportationUnit.WRH_UnitType == TransportUnitTypes.ULD || (receiveTransportationUnit.WRH_UnitType == TransportUnitTypes.Container && receiveTransportationUnit.WRH_SignedBy.IsEmpty))
			{
				signedBy = receiveTransportationUnit.ContainerizedPackageState?.ReceiveTransportationUnit?.WRH_SignedBy ?? ZString.Empty;
			}
			else
			{
				signedBy = receiveTransportationUnit.WRH_SignedBy;
			}

			if (string.IsNullOrWhiteSpace(signedBy))
			{
				return;
			}

			if (dataObject.VehicleRun is null)
			{
				dataObject.VehicleRun = new VehicleRun();
			}

			var vehicleRun = dataObject.VehicleRun;

			var driver = new Crew
			{
				CrewType = CrewType.Driver,
				FullName = signedBy,
			};

			if (vehicleRun.CrewCollection is null)
			{
				vehicleRun.SetWriterStrategy(writeManager.WriterStrategy);
				vehicleRun.SetCrewCollection(() => new List<Crew> { driver, });
			}
			else
			{
				vehicleRun.CrewCollection.Add(driver);
			}
		}
	}
}
