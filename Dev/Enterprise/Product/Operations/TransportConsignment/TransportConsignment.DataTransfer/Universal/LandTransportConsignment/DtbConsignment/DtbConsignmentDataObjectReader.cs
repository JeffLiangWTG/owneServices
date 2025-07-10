using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.TransportCommon.DataTransfer.Universal;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	public class DtbConsignmentDataObjectReader : ShipmentDataObjectReader<DtbConsignment>
	{
		public DtbConsignmentDataObjectReader(UniversalShipment consignmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, UniversalShipment topLevelDataObject, Instruction instruction, DtbConsignment consignment, IColumnIndexer pickUpInstructionToCopy = null, IColumnIndexer depotConfirmationToCopy = null, Instruction additionalInstruction = null)
			: this(consignmentDataObject, logger, factory, consignment, topLevelDataObject, initialPickupDeliveryAddress: false)
		{
			if (instruction != null)
			{
				InstructionWithPackages = instruction;
				if (instruction.Type.Code.Value == ConsignmentAddressTypes.Codes.PickUp)
				{
					PickUpAddress = instruction;
				}
				else if (instruction.Type.Code.Value == ConsignmentAddressTypes.Codes.Delivery)
				{
					DeliveryAddress = instruction;
				}
			}

			if (additionalInstruction != null)
			{
				if (additionalInstruction.Type.Code.Value == ConsignmentAddressTypes.Codes.PickUp)
				{
					PickUpAddress = additionalInstruction;
				}
				else if (additionalInstruction.Type.Code.Value == ConsignmentAddressTypes.Codes.Delivery)
				{
					DeliveryAddress = additionalInstruction;
				}
			}

			if (PickUpAddress == null)
			{
				PickUpAddress = dataObject.InstructionCollection != null
					? dataObject.InstructionCollection.FirstOrDefault(i => i.Type.GetCodeAsUpperCase() == ConsignmentAddressTypes.Codes.PickUp)
					: null;
			}

			if (InstructionWithPackages == null)
			{
				InstructionWithPackages = PickUpAddress;
			}

			if (consignment != null && (pickUpInstructionToCopy != null || depotConfirmationToCopy != null))
			{
				throw new ArgumentException("We only want to copy the Pick Up Info to new Consignments.");
			}

			DepotActionToCopy = depotConfirmationToCopy;
			PickUpAddressToCopy = pickUpInstructionToCopy;
		}

		public DtbConsignmentDataObjectReader(UniversalShipment consignmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, DtbConsignment consignment, UniversalShipment topLevelDataObject, bool initialPickupDeliveryAddress = true)
			: base(consignmentDataObject, logger, factory)
		{
			Consignment = consignment;
			if (initialPickupDeliveryAddress)
			{
				PickUpAddress = consignmentDataObject.InstructionCollection?.FirstOrDefault(i => i.Type.GetCodeAsUpperCase() == ConsignmentAddressTypes.Codes.PickUp);
				DeliveryAddress = consignmentDataObject.InstructionCollection?.FirstOrDefault(i => i.Type.GetCodeAsUpperCase() == ConsignmentAddressTypes.Codes.Delivery);
			}			
			TopLevelDataObject = Argument.NotNull(topLevelDataObject, nameof(topLevelDataObject));
		}

		readonly Instruction PickUpAddress;
		readonly IColumnIndexer DepotActionToCopy;
		readonly IColumnIndexer PickUpAddressToCopy;
		readonly UniversalShipment TopLevelDataObject;
		readonly Instruction DeliveryAddress;
		readonly Instruction InstructionWithPackages;

		readonly DtbConsignment Consignment;
		Dictionary<ZInt, PkgPackage> packageLinksDictionary;
		Dictionary<ZInt, PkgPackage> containerLinksDictionary;

		#region DataContextType

		public override DataContextType DataContextType
		{
			get { return DataContextType.LandTransportConsignment; }
		}

		#endregion

		#region LogChildTopLevelObjectsOnImport

		protected override bool LogChildTopLevelObjectsOnImport
		{
			get { return Consignment == null || DeliveryAddress != null; }
		}

		#endregion

		#region GetCombinedReferenceMatcher

		protected override IMatchingBusinessEntityFinder<DtbConsignment> GetCombinedReferenceMatcher()
		{
			// No references to match a consignment
			return null;
		}

		#endregion

		#region GetExistingBusinessObjectUsingModuleSpecificBusinessRules

		protected override DtbConsignment GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return Consignment;
		}

		#endregion

		#region IsImportJobCostingAllowed

		protected override bool IsImportJobCostingAllowed(DtbConsignment targetBO) => false;

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(DtbConsignment targetBO)
		{
			PopulateData(targetBO);
			PopulateConNoteNo(targetBO);
			PopulateGoodsAndInsuranceValue(targetBO);
			PopulateRelatedEntities(targetBO);
		}

		#region PopulateData

		void PopulateData(DtbConsignment consignment)
		{
			var row = GetColumnIndexerFromRow(consignment);
			SetValue(row, DtbConsignmentSchema.LTC_RS_NKServiceLevel, dataObject.ServiceLevel);
			SetValue(row, DtbConsignmentSchema.LTC_Direction, dataObject.Direction ?? Constants.CartageDirection.Local);
			SetValue(row, DtbConsignmentSchema.LTC_JobType, dataObject.LocalTransportJobType?.Code ?? ZString.Empty);
			SetValue(row, DtbConsignmentSchema.LTC_Incoterm, dataObject.ShipmentIncoTerm?.Code ?? ZString.Empty);
			SetValue(row, DtbConsignmentSchema.LTC_AdditionalTerms, dataObject.AdditionalTerms ?? ZString.Empty);
		}

		#endregion

		#region PopulateConNoteNo

		void PopulateConNoteNo(DtbConsignment consignment)
		{
			ZString conNoteNo = "";
			var row = GetColumnIndexerFromRow(consignment);
			conNoteNo = dataObject.ConsignmentNote ?? ZString.Empty;

			// the DLV instruction conNoteNo should become the consignment's conNoteNo
			if (conNoteNo.IsEmpty && DeliveryAddress != null)
			{
				var allConfirmations = DeliveryAddress.GetConfirmations();
				var conNoteActions = allConfirmations.Where(c => c.DateDescription.GetValueOrDefault().EqualsIgnoringCase(ActionTypes.Codes.ConNoteNo)).ToArray();

				if (conNoteActions.Length == 1)
				{
					conNoteNo = conNoteActions.Single().Reference.GetValueOrDefault();
				}
			}
			// fallback to the booking-level Transport Ref
			if (conNoteNo.IsEmpty && dataObject.LocalProcessing != null)
			{
				conNoteNo = dataObject.LocalProcessing.ArrivalCartageRef.GetValueOrDefault();
			}

			SetValue(row, DtbConsignmentSchema.LTC_ConnoteNumber, conNoteNo);
		}

		#endregion

		void PopulateGoodsAndInsuranceValue(DtbConsignment consignment)
		{
			var row = GetColumnIndexerFromRow(consignment);

			UniversalShipment universalShipmentToCalcFrom = dataObject;

			if (dataObject.GoodsValue == null)
			{
				if (SchemaVersionManager.Current == UniversalXmlSchema.Version_2012_11_DO_NOT_USE)
				{
					var bookingConsolidationDO = TopLevelDataObject.ParentShipmentCollection?.FirstOrDefault();
					if (bookingConsolidationDO != null)
					{
						if (bookingConsolidationDO.TransportBookingDirection?.Code is ZString direction)
						{
							if (direction == RelatedPartyDirectionList.Codes.Delivery)
							{
								if (bookingConsolidationDO.PreCarriageShipmentCollection?.FirstOrDefault() is UniversalShipment parentShipment)
								{
									universalShipmentToCalcFrom = parentShipment;
								}
							}
							else if (direction == RelatedPartyDirectionList.Codes.Pickup)
							{
								if (bookingConsolidationDO.PostCarriageShipmentCollection?.FirstOrDefault() is UniversalShipment parentShipment)
								{
									universalShipmentToCalcFrom = parentShipment;
								}
							}
						}
					}
				}
				else
				{
					if (((TopLevelDataObject.SubShipmentCollection?.Count ?? 0) > 1) &&
						(TopLevelDataObject.SubShipmentCollection.SingleOrDefault(e => e.DataContext.GetDataSourceKey() == dataObject.DataContext.GetDataSourceKey()) != null) &&
						(TopLevelDataObject.SubShipmentCollection.FirstOrDefault(e => e.DataContext.GetDataSourceKey() != dataObject.DataContext.GetDataSourceKey()) is UniversalShipment shipment))
					{
						universalShipmentToCalcFrom = shipment;
					}
				}
			}

			SetValue(row, DtbConsignmentSchema.LTC_GoodsValue, universalShipmentToCalcFrom.GoodsValue ?? ZDecimal.Zero);
			SetValue(row, DtbConsignmentSchema.LTC_RX_NKGoodsValueCurrency, universalShipmentToCalcFrom.GoodsValueCurrency?.Code ?? ZString.Empty);
			SetValue(row, DtbConsignmentSchema.LTC_InsuranceValue, universalShipmentToCalcFrom.InsuranceValue ?? ZDecimal.Zero);
			SetValue(row, DtbConsignmentSchema.LTC_RX_NKInsuranceValueCurrency, universalShipmentToCalcFrom.InsuranceValueCurrency?.Code ?? ZString.Empty);
		}

		#region PopulateRelatedEntities

		void PopulateRelatedEntities(DtbConsignment consignment)
		{
			PopulateAdditionalReferences(consignment);
			PopulateBookingJobId(consignment); //test in BookingToTransportJobCommonCreator.cs
			PopulateNotes(consignment);

			if (PickUpAddress == null)
			{
				var errorMessage = Res.GetString("dfd650dd-4c95-423e-bd7a-d90e3c48c3d5", "The UXML can not be imported as there is no Pickup address.");
				if (dataObject.DataContext.GetMatchingDataSource(DataContextType.TransportBookingConsolidation) != null)
				{
					var errorMessageExtention = Res.GetString("cd18a542-0179-4960-a479-ba8152c60b46", "Possible reason: No matching sub-shipment ID is found.");
					throw new DataObjectReadFailureException(string.Format(CultureInfo.InvariantCulture, "{0} {1}", errorMessage, errorMessageExtention));
				}
			}

			PopulateLocalClient(consignment);
			PopulateOrgAddresses(consignment);

			PopulatePackageJob(consignment);
			PopulateConsignmentAddress(consignment);
			PopulateConsignmentActionPackageDivots(consignment);
		}

		void PopulateBookingJobId(DtbConsignment consignment)
		{
			var bookingId = dataObject?.DataContext?.GetMatchingDataSource(DataContextType.TransportBooking)?.Key;
			if (bookingId.HasValue)
			{
				var bookingDataSource = logger.TopLevelDataContext.GetMatchingDataSource(DataContextType.TransportBooking);
				var parentBooking = bookingDataSource.GetLoadedJobFromDataContextType(logger.TopLevelDataObject, factory.BOFactory);
				if (parentBooking != null)
				{
					var parentBookingRow = GetColumnIndexerFromRow(parentBooking);
					consignment.LTC_KM_Booking = parentBookingRow.GetValue(DtbBookingSchema.PK);
				}
			}
		}

		#endregion

		#region PopulateAdditionalReferences

		void PopulateAdditionalReferences(DtbConsignment consignment)
		{
			if (dataObject.AdditionalReferenceCollection != null)
			{
				var additionalReferenceCollectionReader = new TransportAdditionalReferenceCollectionReader<DtbConsignment>(GetAdditionalReferences(), logger, factory, consignment);
				additionalReferenceCollectionReader.ReadIntoCollection();
			}
		}

		DataObjectList<AdditionalReference> GetAdditionalReferences()
		{
			var result = new DataObjectList<AdditionalReference>();

			var collectionWithReferences = dataObject.AdditionalReferenceCollection ?? TopLevelDataObject.AdditionalReferenceCollection;
			if (collectionWithReferences != null)
			{
				result.AddRange(collectionWithReferences);

				if (dataObject.AdditionalReferenceCollection != null && TopLevelDataObject.AdditionalReferenceCollection != null)
				{
					foreach (var additionalReference in TopLevelDataObject.AdditionalReferenceCollection)
					{
						if (!result.Any(a => a.Type.Code == additionalReference.Type.Code))
						{
							result.Add(additionalReference);
						}
					}
				}
			}

			return result;
		}

		#endregion

		#region PopulateNotes

		void PopulateNotes(DtbConsignment consignment)
		{
			if (dataObject.NoteCollection != null)
			{
				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, consignment).ReadIntoCollection();
			}
		}

		#endregion

		#region PopulateLocalClient

		void PopulateLocalClient(DtbConsignment consignment)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var addressTypes = logger.TopLevelDataContext.IsFromSameSystem()
					? new ZString[] { nameof(DocAddressType.LocalClient), AddressTypes.SendersLocalClient }
					: new ZString[] { nameof(DocAddressType.LocalClient) };

				var localClientDataObject = dataObject.OrganizationAddressCollection.FirstOrDefault(addressTypes);
				if (localClientDataObject != null)
				{
					var reader = new OrganisationDataObjectReader(localClientDataObject, logger, factory);
					var localClientAddress = reader.GetMatched();
					if (localClientAddress != null)
					{
						var localClientAddressRow = GetColumnIndexerFromRow(localClientAddress);
						var jobHeader = GetColumnIndexerFromRow(new JobHeader.Loader(consignment).TryLoadOrCreateWithMutex());
						SetValue(jobHeader, JobHeaderSchema.JH_OA_LocalChargesAddr, localClientAddressRow.GetValue(OrgAddressSchema.PK));
						reader.GetMatchedOrNew(consignment, DocAddressType.ClientRequestedBillingParty);
					}
				}
			}
		}

		#endregion

		#region PopulateOrgAddresses

		void PopulateOrgAddresses(DtbConsignment consignment)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				PopulateOrgAddressesCore(consignment, dataObject.OrganizationAddressCollection);
			}

			if (TopLevelDataObject.OrganizationAddressCollection != null)
			{
				PopulateOrgAddressesCore(consignment, TopLevelDataObject.OrganizationAddressCollection);
			}
		}

		void PopulateOrgAddressesCore(DtbConsignment consignment, List<OrganizationAddress> addresses)
		{
			foreach (var organisationAddressDataObject in addresses)
			{
				new OrganisationDataObjectReader(organisationAddressDataObject, logger, factory).GetMatchedOrNew(consignment);
			}
		}

		#endregion

		#region PopulatePackageJob

		PkgPackageJobDataObjectReader PopulatePackageJob(DtbConsignment consignment)
		{
			PkgPackageJobDataObjectReader packageJobReader = null;

			var consignmentRow = GetColumnIndexerFromRow(consignment);
			var packageJob = PopulatePackageJobFromExistingPackages(consignmentRow);
			if (packageJob == null)
			{
				if (PickUpAddress != null || DeliveryAddress != null)
				{
					var pickupContainers = GetContainersForThisConsignment(PickUpAddress) ?? new DataObjectList<Container>();
					var deliveryContainers = GetContainersForThisConsignment(DeliveryAddress) ?? new DataObjectList<Container>();
					var containers = DeliveryAddress != null ? new DataObjectList<Container>(pickupContainers.Intersect(deliveryContainers)) : pickupContainers;
					var pickupPackages = GetPackagesForThisConsignment(PickUpAddress) ?? new List<PackingLine>();
					var deliveryPackages = GetPackagesForThisConsignment(DeliveryAddress) ?? new List<PackingLine>();
					var packages = GetCommonPackages(pickupPackages, deliveryPackages);

					packages = CleanUpCommonPackages(packages, pickupPackages, deliveryPackages);
					if (!packages.Any())
					{
						packages.AddRange(GetActionPackages() ?? new List<PackingLine>());
					}
					if (!containers.Any())
					{
						containers.AddRange(GetActionContainers() ?? new List<Container>());
					}
					if (!packages.Any() && !containers.Any())
					{
						throw new DataObjectReadFailureException(Res.GetString("DBF0504E-4234-455A-98E8-E708C4E8F50D", "The packages are not linked correctly."));
					}
					var packageParentDataObject = new PackingSourceDataObject(containers, new DataObjectList<PackingLine>(packages) { Content = CollectionContent.Complete }, dataObject);
					packageJobReader = new PkgPackageJobDataObjectReader(packageParentDataObject, logger, factory, consignment);
					packageJob = packageJobReader.ReadIntoBusinessObject();
					packageLinksDictionary = packageJobReader.PackageLinks;
					containerLinksDictionary = packageJobReader.PackageContainerLinks;
				}
			}

			if (packageJob != null)
			{
				SetIsHazardousAndRequiresRefrigeration(consignmentRow, GetColumnIndexerFromRow(packageJob));
			}

			return packageJobReader;
		}

		IEnumerable<PackingLink> GetPackingLinks()
		{
			var packingLinks = dataObject.InstructionCollection?.Where(instruction => instruction.InstructionPackingLineLinkCollection != null)
				.SelectMany(instruction => instruction.InstructionPackingLineLinkCollection).Where(link => link.ConfirmationCollection != null)
				.SelectMany(link => link.ConfirmationCollection).Where(action => action.PackingLinkCollection != null)
				.SelectMany(action => action.PackingLinkCollection).Where(link => link.PackingLineLink.HasValue);

			return packingLinks;
		}

		List<PackingLine> GetActionPackages()
		{
			var actionPackageLinks = GetPackingLinks().Where(link => !link.IsContainer ?? true).Select(link => link.PackingLineLink).Distinct().ToList() ?? new List<ZInt?>();

			return dataObject.PackingLineCollection?.Where(pack => actionPackageLinks.Contains(pack.Link)).ToList() ?? new List<PackingLine>();
		}

		List<Container> GetActionContainers()
		{
			var actionContainerLinks = GetPackingLinks().Where(link => link.IsContainer ?? false).Select(link => link.PackingLineLink).Distinct().ToList() ?? new List<ZInt?>();

			return dataObject.ContainerCollection?.Where(container => actionContainerLinks.Contains(container.Link)).ToList() ?? new List<Container>();
		}

		#region CleanUpCommonPackages

		List<PackingLine> CleanUpCommonPackages(List<PackingLine> packages, List<PackingLine> pickupPackages, List<PackingLine> deliveryPackages)
		{
			List<PackingLine> result = null;
			if (packages != null && pickupPackages.Any() && deliveryPackages.Any())
			{
				foreach (var package in packages.ToArray())
				{
					// We need to remove inners where the outer is already included.
					if (IsAnInnerOfIncludedOuter(package, packages))
					{
						packages.Remove(package);
					}
				}

				result = new List<PackingLine>();
				var currentDepthLevel = 0;
				var maxDepthLevel = 0;
				while (packages.Any())
				{
					var packageAdded = false;
					foreach (var packageWeAreChecking in packages.OrderBy(p => p.Link))
					{
						var picLevel = 0;
						var dlvLevel = 0;
						var deepestPicPackLevel = FindDepestPackLevel(pickupPackages, 0);
						var deepestDlvPackLevel = FindDepestPackLevel(deliveryPackages, 0);
						var localMax = Math.Max(deepestDlvPackLevel, deepestPicPackLevel);
						maxDepthLevel = Math.Max(maxDepthLevel, localMax);
						PackingLine picPackage = null;
						PackingLine dlvPackage = null;
						while (picLevel <= currentDepthLevel && dlvLevel <= currentDepthLevel && (picPackage == null && picLevel <= deepestPicPackLevel || dlvPackage == null && dlvLevel <= deepestDlvPackLevel))
						{
							picPackage = picPackage ?? FindPackageAtLvl(packageWeAreChecking, pickupPackages, picLevel);
							if (picPackage == null)
							{
								picLevel++;
							}

							dlvPackage = dlvPackage ?? FindPackageAtLvl(packageWeAreChecking, deliveryPackages, dlvLevel);
							if (dlvPackage == null)
							{
								dlvLevel++;
							}
						}

						if (AddPackageToPackages(packages, result, packageWeAreChecking, picLevel, dlvLevel, picPackage, dlvPackage))
						{
							packageAdded = true;
							break;
						}
					}

					if (!packageAdded)
					{
						currentDepthLevel++;
					}

					if (currentDepthLevel > maxDepthLevel) // situation where package is added to commonPackages that doesn't actually exist in both instructions.
					{
						break;
					}
				}
			}
			return result ?? packages;
		}

		bool AddPackageToPackages(List<PackingLine> packages, List<PackingLine> result, PackingLine packageWeAreChecking, int picLevel, int dlvLevel, PackingLine picPackage, PackingLine dlvPackage)
		{
			var packageAdded = false;
			if (picPackage != null && dlvPackage != null)
			{
				if (!IsPackgeAnInnerOfPackageInCollection(result, packageWeAreChecking, Math.Max(picLevel, dlvLevel)))
				{
					var packageWithCorrectDetails = picPackage.PackQty.GetValueOrDefault(0) < dlvPackage.PackQty.GetValueOrDefault(0) ? picPackage : dlvPackage;
					packageWeAreChecking.SetPackingLineCollection(() => ChangeRatioOfInners(picPackage, dlvPackage));

					result.Add(packageWithCorrectDetails);
					packages.Remove(packageWeAreChecking);
					packageAdded = true;
				}
			}
			return packageAdded;
		}

		bool IsPackgeAnInnerOfPackageInCollection(List<PackingLine> packLines, PackingLine innerPackage, int depth)
		{
			return packLines.Any(p => FindPackageAtLvl(innerPackage, p.PackingLineCollection, depth) != null);
		}

		bool IsAnInnerOfIncludedOuter(PackingLine package, List<PackingLine> includedPackages)
		{
			var result = includedPackages.Any(p => p.PackingLineCollection != null && p.PackingLineCollection.Any(innerPackage => innerPackage.Link == package.Link));
			if (!result)
			{
				foreach (var pack in includedPackages)
				{
					if (pack.PackingLineCollection != null)
					{
						result = IsAnInnerOfIncludedOuter(package, pack.PackingLineCollection);
					}
				}
			}

			return result;
		}

		PackingLine FindPackageAtLvl(PackingLine package, List<PackingLine> packages, int level)
		{
			PackingLine result = null;
			if (packages != null)
			{
				foreach (var pack in packages)
				{
					if (level != 0)
					{
						if (pack.PackingLineCollection != null)
						{
							result = FindPackageAtLvl(package, pack.PackingLineCollection.ToList(), level--);
						}
					}
					if (pack.Link == package.Link)
					{
						result = pack;
					}
					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}

		List<PackingLine> ChangeRatioOfInners(PackingLine picPackage, PackingLine dlvPackage)
		{
			List<PackingLine> result = null;
			if (picPackage.PackingLineCollection != null && picPackage.PackQty != null) // they're the same package so we only need to check one, and if the pack qty is null this is moot.
			{
				result = new List<PackingLine>();
				var ratio = Math.Min(picPackage.PackQty.GetValueOrDefault(0), dlvPackage.PackQty.GetValueOrDefault(0)) / (double)Math.Max(picPackage.PackQty.GetValueOrDefault(1), dlvPackage.PackQty.GetValueOrDefault(1)); // 0 / 1 (default if null)
				foreach (var package in picPackage.PackingLineCollection)
				{
					var correspondingDlvPackage = dlvPackage.PackingLineCollection.First(p => p.Link == package.Link);
					package.SetPackingLineCollection(() => ChangeRatioOfInners(package, correspondingDlvPackage));
					package.PackQty = (int)((double)package.PackQty * ratio);
					result.Add(package);
				}
			}
			return result;
		}

		int FindDepestPackLevel(List<PackingLine> packages, int level)
		{
			if (packages != null)
			{
				foreach (var package in packages)
				{
					if (package.PackingLineCollection != null)
					{
						var newLevel = FindDepestPackLevel(package.PackingLineCollection, level++);
						if (newLevel > level)
						{
							level = newLevel;
						}
					}
				}
			}

			return level;
		}

		#endregion

		#region GetCommonPackages

		List<PackingLine> GetCommonPackages(List<PackingLine> pickupPackages, List<PackingLine> deliveryPackages)
		{
			if (DeliveryAddress == null)
			{
				return pickupPackages;
			}
			else if (!pickupPackages.Any() || !deliveryPackages.Any())
			{
				return new List<PackingLine>();
			}
			else
			{
				var result = new List<PackingLine>();

				AddPackagesToResult(pickupPackages, deliveryPackages, result);
				AddPackagesToResult(deliveryPackages, pickupPackages, result);
				return result;
			}
		}

		#region AddPackagesToResult

		void AddPackagesToResult(List<PackingLine> primaryPackageList, List<PackingLine> secondaryPackageList, List<PackingLine> result)
		{
			foreach (var primaryPackage in primaryPackageList)
			{
				if (secondaryPackageList.Any(p => p.Link == primaryPackage.Link))
				{
					if (!result.Any(r => r.Link == primaryPackage.Link))
					{
						result.Add(primaryPackage);
					}
				}
				else
				{
					foreach (var secondaryPackage in secondaryPackageList)
					{
						if (secondaryPackage.PackingLineCollection != null)
						{
							var commonPackages = GetCommonPackages(primaryPackageList, secondaryPackage.PackingLineCollection);
							foreach (var package in commonPackages)
							{
								if (!result.Any(r => r.Link == package.Link))
								{
									result.Add(package);
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#endregion

		#region PopulatePackageJobFromExistingPackages

		PkgPackageJob PopulatePackageJobFromExistingPackages(IColumnIndexer consignmentRow)
		{
			PkgPackageJob result = null;

			if (!IsNewBO)
			{
				var packageJob = GetPackageJob(consignmentRow);
				var packageJobRow = GetColumnIndexerFromRow(packageJob);
				ValidatePackageStructure(packageJobRow);

				// If no Package IDs are supplied it is not possible to keep the original Packages as we have 
				// nothing to match with. If no Packages are re-wired we simply rebuild the Package Tree later.
				bool successfullyRewiredPackages = RewirePackages(packageJobRow);
				if (successfullyRewiredPackages)
				{
					result = packageJob;
				}
			}
			else
			{
				var query = new ZQuery(PkgPackageSchema.KP_KJ_ParentPackageJob, ZGuid.Empty);
				var detachedPackages = factory.RowFactory.Load(PkgPackageSchema.Constants.TableName, query);
				if (detachedPackages.Length > 0)
				{
					result = MoveDetachedPackagesOnToConsignment(consignmentRow);
				}
			}

			return result;
		}

		PkgPackageJob GetPackageJob(IColumnIndexer consignmentRow)
		{
			var query = new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignmentRow.GetValue(DtbConsignmentSchema.PK));
			return factory.LoadTop1<PkgPackageJob>(query);
		}

		#region ValidatePackageStructure

		void ValidatePackageStructure(IColumnIndexer packageJob)
		{
			var pickUpContainers = GetContainersForThisConsignment(PickUpAddress) ?? Enumerable.Empty<Container>();
			var pickUpPackages = GetPackagesForThisConsignment(PickUpAddress) ?? new List<PackingLine>();

			if (packageJob != null)
			{
				if (!PackageJobStructureComparer.IsPackageJobSameStructureAsXMLPackageStructure(factory, pickUpContainers, pickUpPackages, packageJob))
				{
					var errorMessage = Res.GetString("f9ddf433-982c-4d38-a705-f254bc01628b", "Cannot Import as Existing Pick Up Consignment Packages do not match the Booking Pick Up Packages.");
					throw new DataObjectReadFailureException(errorMessage);
				}
			}
			else if (pickUpContainers.Any() || pickUpPackages.Any())
			{
				var errorMessage = Res.GetString("f9ddf433-982c-4d38-a705-f254bc01628b", "Cannot Import as Existing Pick Up Consignment Packages do not match the Booking Pick Up Packages.");
				throw new DataObjectReadFailureException(errorMessage);
			}
		}

		#endregion

		#region RewirePackages

		bool RewirePackages(IColumnIndexer packageJob)
		{
			bool result = false;

			if (packageJob != null)
			{
				var deliveryPackageIDs = GetDeliveryPackageIDs().ToArray();
				if (deliveryPackageIDs.Any())
				{
					var existingPackages = GetExistingPackages(deliveryPackageIDs, packageJob.GetValue(PkgPackageJobSchema.PK));
					MakeInnersOutersAndCopyScanEvents(existingPackages, null);
					DetachRemainingPackages(packageJob, existingPackages);
					result = true;
				}
			}

			return result;
		}

		IEnumerable<ZString> GetDeliveryPackageIDs()
		{
			var result = Enumerable.Empty<ZString>();

			var deliveryContainers = GetContainersForThisConsignment(DeliveryAddress);
			if (deliveryContainers != null && deliveryContainers.Any() && deliveryContainers.All(c => !c.ContainerNumber.GetValueOrDefault().IsEmpty))
			{
				result = result.Concat(deliveryContainers.Select(c => c.ContainerNumber.GetValueOrDefault()));
			}

			var deliveryPackages = GetPackagesForThisConsignment(DeliveryAddress);
			if (deliveryPackages != null && deliveryPackages.Any() && deliveryPackages.All(p => !p.ReferenceNumber.GetValueOrDefault().IsEmpty))
			{
				result = result.Concat(deliveryPackages.Select(p => p.ReferenceNumber.GetValueOrDefault()));
			}

			return result;
		}

		IColumnIndexer[] GetExistingPackages(ZString[] deliveryPackageIDs, ZGuid packageJobPK)
		{
			var existingPackagesOnJob = factory.RowFactory.Load(PkgPackageSchema.Constants.TableName, new ZQuery(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJobPK));
			var existingPackageIDPKs = existingPackagesOnJob.Select(p => GetColumnIndexerFromRow(p).GetValue(PkgPackageSchema.KP_KPH_PackageHeader));

			var packageIdQuery = new ZQuery(PkgPackageHeaderSchema.PK, existingPackageIDPKs);
			packageIdQuery.AddToFilter(PkgPackageHeaderSchema.KPH_PackageID, deliveryPackageIDs);
			var matchingPackageIDs = factory.RowFactory.Load(PkgPackageHeaderSchema.Constants.TableName, packageIdQuery);
			var matchingPackageIDPKs = new HashSet<ZGuid>(matchingPackageIDs.Select(i => GetColumnIndexerFromRow(i).GetValue(PkgPackageHeaderSchema.PK)));

			var existingPackages = existingPackagesOnJob.Where(p => matchingPackageIDPKs.Contains(GetColumnIndexerFromRow(p).GetValue(PkgPackageSchema.KP_KPH_PackageHeader))).ToArray();
			if (existingPackages.Length != deliveryPackageIDs.Length)
			{
				var message = Res.GetString("25e4f31d-175d-459d-a7ef-acd2509f9ee2", "Package IDs were specified for the Packages to Deliver but they cannot be found on the Existing Consignment.");
				throw new DataObjectReadFailureException(message);
			}

			return Array.ConvertAll(existingPackages, GetColumnIndexerFromRow);
		}

		#region MakeInnersOutersAndCopyScanEvents

		void MakeInnersOutersAndCopyScanEvents(IColumnIndexer[] deliveryPackages, ZGuid? packageJobPKToSet)
		{
			// make the delivery packages Outers if they are Inners.
			foreach (var package in deliveryPackages)
			{
				// need to set has changes since we will be mixing changes in the rows and the bizOs
				factory.Load<PkgPackage>(package.GetValue(PkgPackageSchema.PK)).HasChanges = true;

				if (package.GetValue(PkgPackageSchema.KP_KP_ParentPackage).IsValid)
				{
					var parentOuter = GetParentOuter(package);
					SetValue(package, PkgPackageSchema.KP_KP_ParentPackage, ZGuid.Empty);

					CopyLogs(package, parentOuter); // scan events only need to be copied if they were not an outer.
				}

				SetValue(package, PkgPackageSchema.KP_KJ_ParentPackageJob, packageJobPKToSet);
			}
		}

		IColumnIndexer GetParentOuter(IColumnIndexer package)
		{
			IColumnIndexer result = null;
			var currentPackage = package;

			while (currentPackage != null)
			{
				result = currentPackage;

				var parentPackagePK = currentPackage.GetValue(PkgPackageSchema.KP_KP_ParentPackage);
				var parentPackage = parentPackagePK.IsValid ? factory.RowFactory.LoadFromPK(PkgPackageSchema.Constants.TableName, parentPackagePK) : null;
				currentPackage = GetColumnIndexerFromRow(parentPackage);
			}

			return result;
		}

		void CopyLogs(IColumnIndexer packageRow, IColumnIndexer outerParent)
		{
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, outerParent.GetValue(PkgPackageSchema.PK));
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);

			var eventsOnOuter = factory.RowFactory.Load(StmALogSchema.Constants.TableName, query);
			foreach (var scanEvent in eventsOnOuter)
			{
				var scanEventRow = GetColumnIndexerFromRow(scanEvent);
				var newEvent = factory.RowFactory.NewRowWithPK(StmALogSchema.Instance);

				SetValue(newEvent, StmALogSchema.SL_EventTime, scanEventRow.GetValue(StmALogSchema.SL_EventTime));
				SetValue(newEvent, StmALogSchema.SL_FireWorkflow, scanEventRow.GetValue(StmALogSchema.SL_FireWorkflow));
				SetValue(newEvent, StmALogSchema.SL_GB_NKBranch, scanEventRow.GetValue(StmALogSchema.SL_GB_NKBranch));
				SetValue(newEvent, StmALogSchema.SL_GE_NKDepartment, scanEventRow.GetValue(StmALogSchema.SL_GE_NKDepartment));
				SetValue(newEvent, StmALogSchema.SL_GS_NKUser, scanEventRow.GetValue(StmALogSchema.SL_GS_NKUser));
				SetValue(newEvent, StmALogSchema.SL_Parent, packageRow.GetValue(PkgPackageSchema.PK));
				SetValue(newEvent, StmALogSchema.SL_PostedTimeUtc, scanEventRow.GetValue(StmALogSchema.SL_PostedTimeUtc));
				SetValue(newEvent, StmALogSchema.SL_Reference, scanEventRow.GetValue(StmALogSchema.SL_Reference));
				SetValue(newEvent, StmALogSchema.SL_SE_NKEvent, scanEventRow.GetValue(StmALogSchema.SL_SE_NKEvent));
				SetValue(newEvent, StmALogSchema.SL_Table, PkgPackageSchema.Constants.TableName);
			}
		}

		#endregion

		#region DetachRemainingPackages

		void DetachRemainingPackages(IColumnIndexer packageJob, IColumnIndexer[] existingPackages)
		{
			var query = new ZQuery();
			query.AddToFilter(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJob.GetValue(PkgPackageJobSchema.PK));
			query.AddToFilter(PkgPackageSchema.PK, SQLComparisonOperator.NotEqual, existingPackages.Select(c => c.GetValue(PkgPackageSchema.PK)));

			// the rest of the packages no longer belong to this consignment so we null out the Package Job FK.
			var remainingPackages = factory.RowFactory.Load(PkgPackageSchema.Constants.TableName, query);
			foreach (var package in remainingPackages)
			{
				var packageRow = GetColumnIndexerFromRow(package);
				SetValue(packageRow, PkgPackageSchema.KP_KJ_ParentPackageJob, ZGuid.Empty);
			}
		}

		#endregion

		#endregion

		#region MoveDetachedPackagesOnToConsignment

		PkgPackageJob MoveDetachedPackagesOnToConsignment(IColumnIndexer consignmentRow)
		{
			PkgPackageJob result = null;

			var deliveryPackageIDs = GetDeliveryPackageIDs().ToArray();
			if (deliveryPackageIDs.Any())
			{
				// find packages not currently assigned to a Consignment that match Package IDs.
				var existingPackages = GetExistingPackages(deliveryPackageIDs, ZGuid.Empty);

				// create Package Job for the New Consignment if it has not already been
				// loaded in Business Code.
				var packageJob = GetColumnIndexerFromRow(GetPackageJob(consignmentRow))
					?? factory.RowFactory.NewRowWithPK(PkgPackageJobSchema.Instance);

				SetValue(packageJob, PkgPackageJobSchema.KJ_ParentID, consignmentRow.GetValue(DtbConsignmentSchema.PK));
				SetValue(packageJob, PkgPackageJobSchema.KJ_ParentTableCode, DtbConsignmentSchema.Constants.Prefix);

				// Attach the matching Packages to this Consignment.
				MakeInnersOutersAndCopyScanEvents(existingPackages, packageJob.GetValue(PkgPackageJobSchema.PK));
				result = factory.Load<PkgPackageJob>(packageJob.GetValue(PkgPackageJobSchema.PK));
			}

			return result;
		}

		#endregion

		#endregion

		#region SetIsHazardousAndRequiresRefrigeration

		void SetIsHazardousAndRequiresRefrigeration(IColumnIndexer consignment, IColumnIndexer packageJob)
		{
			var packagesQuery = new ZQuery(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJob.GetValue(PkgPackageJobSchema.PK));
			var packageRows = factory.RowFactory.Load(PkgPackageSchema.Constants.TableName, packagesQuery);
			var packagePKs = packageRows.Select(r => r[PkgPackageSchema.Constants.PK]).ToArray();
			SetIsHazardousIfThereAreAnyDangerousGoods(consignment, packagePKs);
			SetRequiresRefrigerationIfAnyPackagesAreTemperatureControlled(consignment, packagesQuery, packagePKs);
		}

		void SetIsHazardousIfThereAreAnyDangerousGoods(IColumnIndexer consignment, object[] packagePKs)
		{
			var undgsQuery = new ZQuery(UNDGDataItemSchema.DI_ParentID, packagePKs);
			var undgs = factory.RowFactory.Load(UNDGDataItemSchema.Constants.TableName, undgsQuery);
			SetValue(consignment, DtbConsignmentSchema.LTC_IsHazardous, undgs.Length > 0);
		}

		void SetRequiresRefrigerationIfAnyPackagesAreTemperatureControlled(IColumnIndexer consignment, ZQuery packagesQuery, object[] packagePKs)
		{
			packagesQuery.AddToFilter(PkgPackageSchema.KP_RequiresTemperatureControl, true);
			var refrigeratedPackageRows = factory.RowFactory.Load(PkgPackageSchema.Constants.TableName, packagesQuery);
			var containerQuery = new ZQuery();
			containerQuery.AddToFilter(PkgPackageContainerSchema.K0_KP_Package, packagePKs);
			containerQuery.AddToFilter(PkgPackageContainerSchema.K0_IsControlledAtmosphere, true);
			var refrigeratedContainerRows = factory.RowFactory.Load(PkgPackageContainerSchema.Constants.TableName, containerQuery);
			SetValue(consignment, DtbConsignmentSchema.LTC_RequiresRefrigeration, refrigeratedPackageRows.Length > 0 || refrigeratedContainerRows.Length > 0);
		}

		#endregion

		#region GetPackages

		DataObjectList<Container> GetContainersForThisConsignment(Instruction instructionWithPacks)
		{
			DataObjectList<Container> result = null;

			if (TopLevelDataObject.ContainerCollection != null && instructionWithPacks != null && instructionWithPacks.InstructionContainerLinkCollection != null)
			{
				result = new DataObjectList<Container>();
				result.Content = CollectionContent.Complete;

				foreach (var container in TopLevelDataObject.ContainerCollection)
				{
					if (instructionWithPacks.InstructionContainerLinkCollection.Any(l => l.ContainerLink.HasValue && l.ContainerLink == container.Link))
					{
						result.Add(container);
					}
				}
			}

			return result;
		}

		List<PackingLine> GetPackagesForThisConsignment(Instruction instructionWithPacks)
		{
			List<PackingLine> result = null;

			if (instructionWithPacks != null && instructionWithPacks.InstructionPackingLineLinkCollection != null)
			{
				result = GetPackagesForThisConsignment(instructionWithPacks, TopLevelDataObject.PackingLineCollection?.ToList());
			}

			return result;
		}

		List<PackingLine> GetPackagesForThisConsignment(Instruction instructionWithPacks, List<PackingLine> packageDataObjects)
		{
			List<PackingLine> result = null;

			if (packageDataObjects != null)
			{
				result = new List<PackingLine>();

				foreach (var package in packageDataObjects)
				{
					var packingLink = instructionWithPacks.InstructionPackingLineLinkCollection.SingleOrDefault(l => l.PackingLineLink.HasValue && l.PackingLineLink == package.Link);
					if (packingLink != null)
					{
						var packQty = packingLink.Quantity.GetValueOrDefault();
						if (packQty > 0)
						{
							var originalPackageQty = package.PackQty.GetValueOrDefault();
							var originalPackageVolume = package.Volume.GetValueOrDefault();
							var originalPackageWeight = package.Weight.GetValueOrDefault();
							var proportion = originalPackageQty > 0 ? (ZDecimal)packQty / originalPackageQty : 0m;

							var clone = (PackingLine)package.Clone();
							clone.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance); // normally not OK to use this strategy but okay here because this has come from xml
							clone.PackQty = new ZLong(packQty);
							clone.Volume = originalPackageVolume * proportion;
							clone.Weight = originalPackageWeight * proportion;

							result.Add(clone);
						}
						else
						{
							result.Add(package);
						}

						continue; // no need to enumerate Children as child packs will be read in by the PackageJobReader
					}

					var packs = GetPackagesForThisConsignment(instructionWithPacks, package.PackingLineCollection);
					if (packs != null)
					{
						result.AddRange(packs);
					}
				}
			}

			return result;
		}

		#endregion

		#endregion

		#region PopulateConsignmentAddress

		void PopulateConsignmentAddress(DtbConsignment consignment)
		{
			if (PickUpAddress != null)
			{
				var deliveryAddress = DeliveryAddress ?? GetEmptyDeliveryConsigneeInstruction();
				var pickUpActionToCopy = PickUpAddressToCopy != null
					? GetAction(PickUpAddressToCopy, ConsignmentAddressTypes.Codes.PickUp) // we will be copying the Pick Up info for new Consignments
					: null;

				// if consignment exists already, we don't want to read in the pickup information
				var depotInstruction = GetDepotInstructionDataObject(DepotActionToCopy);
				List<Instruction> instructions;
				if (IsNewBO)// new means this is the copy!
				{
					instructions = new List<Instruction>() { GetPickUpInstructionDataObject(pickUpActionToCopy), deliveryAddress };

					if (depotInstruction != null)
					{
						instructions.Add(depotInstruction);
					}
					else
					{
						var depotInstructions = dataObject.InstructionCollection?.Where(i => i.Type.GetCodeAsUpperCase() == ConsignmentAddressTypes.Codes.Multi);
						if (depotInstructions?.Any() ?? false)
						{
							instructions.AddRange(depotInstructions);
						}
					}
				}
				else
				{
					instructions = new List<Instruction>() { deliveryAddress };
				}

				var addressCollectionReader = new ConsignmentAddressDataObjectCollectionReader(this, consignment, instructions.ToArray(), packageLinksDictionary, containerLinksDictionary);
				addressCollectionReader.ReadIntoCollectionRetainingUnmatchedElements();

				PopuldateConsignmentLegs(consignment);

				var consignmentRow = GetColumnIndexerFromRow(consignment);
				if (IsNewBO)
				{
					// we want to copy allocation info from the existing Pick Up to the new Consignments.
					if (pickUpActionToCopy != null)
					{
						CopyStatusAndRunSheetInformation(consignmentRow, pickUpActionToCopy);
					}
				}
			}
		}

		void PopuldateConsignmentLegs(DtbConsignment consignment)
		{
			var consignmentAddresses = consignment.Addresses.OrderBy(ca => ca.LTS_Sequence);
			OrchestrateConsignmentLegs(consignment, consignmentAddresses);
		}

		void OrchestrateConsignmentLegs(DtbConsignment consignment, IEnumerable<DtbConsignmentAddress> consignmentAddresses)
		{
			for (var i = 0; i < consignmentAddresses.Count() - 1; i++)
			{
				var currentAddress = consignmentAddresses.ElementAt(i);
				var pickupActions = currentAddress.Actions.Where(action => action.IsPickUp);
				foreach (var pickupAction in pickupActions)
				{
					var nextAddressDeliveryActions = consignmentAddresses.Where(address => address.LTS_Sequence > currentAddress.LTS_Sequence).SelectMany(address => address.Actions).Where(ac => ac.IsDelivery);
					var matchingDeliveryAction = nextAddressDeliveryActions.FirstOrDefault(deliveryAction => CheckActionsHaveSameActionPackageDivots(pickupAction, deliveryAction));

					if (matchingDeliveryAction != null)
					{
						CreateNewConsignmentLeg(consignment, currentAddress, pickupAction, matchingDeliveryAction);
					}
				}
			}
		}

		bool CheckActionsHaveSameActionPackageDivots(DtbConsignmentAction action1, DtbConsignmentAction action2)
		{
			var actionPackageDivots1 = action1.PackageDivots;
			var actionPackageDivots2 = action2.PackageDivots;
			return actionPackageDivots1.Count == actionPackageDivots2.Count
				&& actionPackageDivots1.All(actionPackageDivot1 => actionPackageDivots2.Any(actionPackageDivot2 => actionPackageDivot1.LTP_KP_Package == actionPackageDivot2.LTP_KP_Package));
		}

		void CreateNewConsignmentLeg(DtbConsignment consignment, DtbConsignmentAddress address, DtbConsignmentAction pickupAction, DtbConsignmentAction deliveryAction)
		{
			var consignmentLeg = factory.RowFactory.NewRowWithPK(DtbConsignmentLegSchema.Instance);
			var consignmentPK = GetColumnIndexerFromRow(consignment).GetValue(DtbConsignmentSchema.PK);
			var pickupActionPK = GetColumnIndexerFromRow(pickupAction).GetValue(DtbConsignmentActionSchema.PK);
			var deliveryActionPK = GetColumnIndexerFromRow(deliveryAction).GetValue(DtbConsignmentActionSchema.PK);
			var sequence = GetColumnIndexerFromRow(address).GetValue(DtbConsignmentAddressSchema.LTS_Sequence);
			var utcNow = ZDateTime.UtcNow;

			SetValue(consignmentLeg, DtbConsignmentLegSchema.LTG_LTC_Consignment, consignmentPK);
			SetValue(consignmentLeg, DtbConsignmentLegSchema.LTG_LTA_Pickup, pickupActionPK);
			SetValue(consignmentLeg, DtbConsignmentLegSchema.LTG_LTA_Delivery, deliveryActionPK);
			SetValue(consignmentLeg, DtbConsignmentLegSchema.LTG_Sequence, sequence);
			SetValue(consignmentLeg, DtbConsignmentLegSchema.LTG_SystemCreateTimeUtc, utcNow);
			SetValue(consignmentLeg, DtbConsignmentLegSchema.LTG_SystemCreateUser, GlbStaff.CurrentUser.GS_Code);
			SetValue(consignmentLeg, DtbConsignmentLegSchema.LTG_SystemLastEditTimeUtc, utcNow);
			SetValue(consignmentLeg, DtbConsignmentLegSchema.LTG_SystemLastEditUser, GlbStaff.CurrentUser.GS_Code);
		}

		void PopulateConsignmentActionPackageDivots(DtbConsignment consignment)
		{
			if (consignment.AllActions.Any(action => action.PackageDivots.Any()))
			{
				return;
			}
			var packageJob = GetPackageJob(consignment);

			if (packageJob == null)
			{
				return;
			}

			var packages = packageJob.Packages;
			var consignmentAddresses = consignment.Addresses.OrderBy(ca => ca.LTS_Sequence);

			foreach (var address in consignmentAddresses)
			{
				PopulateConsignmentActionPackageDivotsFromAddresses(address.Actions, packages);
			}
		}

		void PopulateConsignmentActionPackageDivotsFromAddresses(DtbConsignmentActionCollection actions, PkgPackageCollection packages)
		{
			foreach (var action in actions)
			{
				PopulateConsignmentActionPackageDivotsFromAction(action, packages);
			}
		}

		void PopulateConsignmentActionPackageDivotsFromAction(DtbConsignmentAction action, PkgPackageCollection packages)
		{
			foreach (var package in packages)
			{
				var consignmentActionPackageDivotRow = factory.RowFactory.NewRowWithPK(DtbConsignmentActionPackageDivotSchema.Instance);
				var actionPK = GetColumnIndexerFromRow(action).GetValue(DtbConsignmentActionSchema.PK);
				SetValue(consignmentActionPackageDivotRow, DtbConsignmentActionPackageDivotSchema.LTP_LTA_ConsignmentAction, actionPK);
				SetValue(consignmentActionPackageDivotRow, DtbConsignmentActionPackageDivotSchema.LTP_KP_Package, package.PK);
				SetValue(consignmentActionPackageDivotRow, DtbConsignmentActionPackageDivotSchema.LTP_PackageQuantity, package.KP_PackageQty);
				var operationTime = ZDateTime.UtcNow;
				SetValue(consignmentActionPackageDivotRow, DtbConsignmentActionPackageDivotSchema.LTP_SystemCreateTimeUtc, operationTime);
				SetValue(consignmentActionPackageDivotRow, DtbConsignmentActionPackageDivotSchema.LTP_SystemCreateUser, GlbStaff.CurrentUser.GS_Code);
				SetValue(consignmentActionPackageDivotRow, DtbConsignmentActionPackageDivotSchema.LTP_SystemLastEditTimeUtc, operationTime);
				SetValue(consignmentActionPackageDivotRow, DtbConsignmentActionPackageDivotSchema.LTP_SystemLastEditUser, GlbStaff.CurrentUser.GS_Code);
			}
		}

		#region GetEmptyDeliveryConsigneeInstruction

		Instruction GetEmptyDeliveryConsigneeInstruction()
		{
			var deliveryInstruction = new Instruction(DefaultDataObjectWriterStrategy.Instance)
			{
				Type = new CodeDescriptionPair { Code = ActionTypes.Codes.Delivery },
				Address = new OrganizationAddress { AddressType = nameof(DocAddressType.LocalCartageImporter) }
			};

			return deliveryInstruction;
		}

		#endregion

		#region GetPickUpDepotInstructionDataObject

		Instruction GetDepotInstructionDataObject(IColumnIndexer depotActionToCopy)
		{
			Instruction result = null;

			var addressPk = depotActionToCopy != null ? depotActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_LTS_ConsignmentAddress) : ZGuid.Empty;
			if (addressPk.IsValid)
			{
				var depotAddressBO = factory.Load<DtbConsignmentAddress>(addressPk);
				var depotAddress = GetColumnIndexerFromRow(depotAddressBO);
				if (depotAddress != null)
				{
					var actionInfo = new ActionInfo(RecipientRoleType.TPC, depotAddressBO);

					// setup data object to contain the Action fields to copy from the existing Pick Up Consignment
					var depotOrgAddress = depotAddressBO.Address.Address;
					result = new Instruction(DefaultDataObjectWriterStrategy.Instance)
					{
						Address = depotOrgAddress != null
							? new OrganizationDataObjectWriter(new DataWritingManager(actionInfo), AddressTypes.DepotAddress).GetDataObject(depotOrgAddress)
							: new OrganizationAddress { AddressType = nameof(DocAddressType.LocalCartageCFS) },
						DropMode = new DropMode { Code = depotAddress.GetValue(DtbConsignmentAddressSchema.LTS_DropMode) },
						ServiceInstruction = depotAddress.GetValue(DtbConsignmentAddressSchema.LTS_Notes),
						Type = new CodeDescriptionPair { Code = depotAddress.GetValue(DtbConsignmentAddressSchema.LTS_InstructionType) }
					};
					result.SetInstructionPackingLineLinkCollection(() => new List<InstructionPackingLineLink>
					{
						new InstructionPackingLineLink { ConfirmationCollection = new List<Confirmation>
						{
							new Confirmation
							{
								ActualDate = depotActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_ActualTime).ToLocalZDateTime(),
								DateDescription = depotActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_ActionType),
								EstimatedDate = depotActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_EstimatedTime).ToLocalZDateTime(),
								ReceivedBy = depotActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_SignedBy),
								Reference = depotActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_ReferenceNumber),
								RequiredFromDate = depotActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_RequiredFrom).ToLocalZDateTime(),
								RequiredToDate = depotActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_RequiredTo).ToLocalZDateTime()
							}
						} }
					});
				}
			}

			return result;
		}

		Instruction GetPickUpInstructionDataObject(IColumnIndexer pickUpActionToCopy)
		{
			Instruction result;

			if (PickUpAddressToCopy != null)
			{
				// setup data object to contain the Action fields to copy from the existing Pick Up Consignment
				result = new Instruction(DefaultDataObjectWriterStrategy.Instance)
				{
					Address = PickUpAddress.Address,
					DropMode = new DropMode { Code = PickUpAddressToCopy.GetValue(DtbConsignmentAddressSchema.LTS_DropMode) },
					ServiceInstruction = PickUpAddressToCopy.GetValue(DtbConsignmentAddressSchema.LTS_Notes),
					Type = new CodeDescriptionPair { Code = PickUpAddressToCopy.GetValue(DtbConsignmentAddressSchema.LTS_InstructionType) }
				};
				result.SetInstructionPackingLineLinkCollection(() => new List<InstructionPackingLineLink>
				{
					new InstructionPackingLineLink { ConfirmationCollection = new List<Confirmation>
					{
						new Confirmation
						{
							ActualDate = pickUpActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_ActualTime).ToLocalZDateTime(),
							DateDescription = pickUpActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_ActionType),
							EstimatedDate = pickUpActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_EstimatedTime).ToLocalZDateTime(),
							ReceivedBy = pickUpActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_SignedBy),
							Reference = pickUpActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_ReferenceNumber),
							RequiredFromDate = pickUpActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_RequiredFrom).ToLocalZDateTime(),
							RequiredToDate = pickUpActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_RequiredTo).ToLocalZDateTime()
						}
					} }
				});
			}
			else
			{
				result = PickUpAddress;
			}

			return result;
		}

		#endregion

		#region CopyStatusAndRunSheetInformation

		void CopyStatusAndRunSheetInformation(IColumnIndexer consignmentRow, IColumnIndexer pickUpActionToCopy)
		{
			// copy across status, runsheet allocation and signature
			var newPickUpAddressRow = GetAddress(consignmentRow, ConsignmentAddressTypes.Codes.PickUp);
			SetValue(newPickUpAddressRow, DtbConsignmentAddressSchema.LTS_Status, PickUpAddressToCopy.GetValue(DtbConsignmentAddressSchema.LTS_Status));

			var newPickUpAction = GetAction(newPickUpAddressRow, ActionTypes.Codes.PickUp);
			SetValue(newPickUpAction, DtbConsignmentActionSchema.LTA_K1_RunSheetInstruction, pickUpActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_K1_RunSheetInstruction));
			SetValue(newPickUpAction, DtbConsignmentActionSchema.LTA_SignedBySignature, pickUpActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_SignedBySignature));

			// copy runsheet allocation and signature from the depot confirmation + calculate correct status
			if (DepotActionToCopy != null)
			{
				var newDepotAddressRow = GetAddress(consignmentRow, ConsignmentAddressTypes.Codes.Multi);
				var newDeliverToDepotAction = GetAction(newDepotAddressRow, ActionTypes.Codes.Delivery);
				var runSheetInstructionPK = DepotActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_K1_RunSheetInstruction);
				SetValue(newDeliverToDepotAction, DtbConsignmentActionSchema.LTA_K1_RunSheetInstruction, runSheetInstructionPK);
				SetValue(newDeliverToDepotAction, DtbConsignmentActionSchema.LTA_SignedBySignature, DepotActionToCopy.GetValue(DtbConsignmentActionSchema.LTA_SignedBySignature));
				SetStatusBasedOnRunSheetInstruction(newDepotAddressRow, runSheetInstructionPK);
			}
		}

		void SetStatusBasedOnRunSheetInstruction(IColumnIndexer newDepotInstructionRow, ZGuid runSheetInstructionPK)
		{
			var runSheetInstruction = factory.RowFactory.LoadFromPK(DtbConsignmentRunSheetInstructionSchema.Constants.TableName, runSheetInstructionPK);
			if (runSheetInstruction != null)
			{
				var runSheetInstructionRow = GetColumnIndexerFromRow(runSheetInstruction);

				bool isTimeInEmpty = runSheetInstructionRow.GetValue(DtbConsignmentRunSheetInstructionSchema.K1_TimeIn).IsEmpty;
				bool isTimeOutEmpty = runSheetInstructionRow.GetValue(DtbConsignmentRunSheetInstructionSchema.K1_TimeOut).IsEmpty;
				var depotInstructionStatus = isTimeInEmpty && isTimeOutEmpty ? TransportStatuses.Codes.DeliveryAllocated : TransportStatuses.Codes.Delivered;
				SetValue(newDepotInstructionRow, DtbConsignmentAddressSchema.LTS_Status, depotInstructionStatus);
			}
		}

		#endregion

		#region GetActions

		IColumnIndexer GetAction(IColumnIndexer addressRow, string actionType)
		{
			var query = new ZQuery();
			query.AddToFilter(DtbConsignmentActionSchema.LTA_LTS_ConsignmentAddress, addressRow.GetValue(DtbConsignmentAddressSchema.PK));
			query.AddToFilter(DtbConsignmentActionSchema.LTA_ActionType, actionType);

			var action = factory.RowFactory.Load(DtbConsignmentActionSchema.Constants.TableName, query).Single(); // only one Action of each type should exist.
			return GetColumnIndexerFromRow(action);
		}

		#endregion

		#region GetAddresses

		IColumnIndexer GetAddress(IColumnIndexer consignmentRow, string instructionType)
		{
			var query = new ZQuery();
			query.AddToFilter(DtbConsignmentAddressSchema.LTS_LTC_Consignment, consignmentRow.GetValue(DtbConsignmentSchema.PK));
			query.AddToFilter(DtbConsignmentAddressSchema.LTS_InstructionType, instructionType);

			return GetColumnIndexerFromRow(factory.RowFactory.Load(DtbConsignmentAddressSchema.Constants.TableName, query).Single()); // only one Address of each type should exist.
		}

		#endregion

		#endregion

		#region ConsignmentAddressDataObjectCollectionReader

		class ConsignmentAddressDataObjectCollectionReader : DataObjectCollectionReader<Instruction, DtbConsignmentAddress>
		{
			public ConsignmentAddressDataObjectCollectionReader(DtbConsignmentDataObjectReader reader, DtbConsignment consignment, Instruction[] instructionDataObjects, Dictionary<ZInt, PkgPackage> packageLinksDictionary, Dictionary<ZInt, PkgPackage> containerLinksDictionary)
				: base(instructionDataObjects)
			{
				Consignment = consignment;
				this.packageLinksDictionary = packageLinksDictionary;
				this.containerLinksDictionary = containerLinksDictionary;
				Reader = reader;
			}

			ZInt Sequence = 0;
			readonly DtbConsignment Consignment;
			readonly Dictionary<ZInt, PkgPackage> packageLinksDictionary;
			readonly Dictionary<ZInt, PkgPackage> containerLinksDictionary;
			readonly DtbConsignmentDataObjectReader Reader;

			protected override void AddToCollection(DtbConsignmentAddress address)
			{
				var row = GetColumnIndexerFromRow(address);
				var currentAddressSequence = row.GetValue(DtbConsignmentAddressSchema.LTS_Sequence);

				++Sequence;
				Reader.SetValue(row, DtbConsignmentAddressSchema.LTS_LTC_Consignment, Consignment.GetValue(DtbConsignmentSchema.PK));
				if (currentAddressSequence <= 0)
				{
					Reader.SetValue(row, DtbConsignmentAddressSchema.LTS_Sequence, Sequence);
				}
			}

			protected override DtbConsignmentAddress[] BusinessObjects
			{
				get { return Reader.factory.Load<DtbConsignmentAddress>(new ZQuery(DtbConsignmentAddressSchema.LTS_LTC_Consignment, Consignment[DtbConsignmentSchema.Constants.PK])); }
			}

			protected override DtbConsignmentAddress FindMatchingBusinessObject(Instruction dataObject)
			{
				return null;
			}

			protected override DtbConsignmentAddress ReadIntoBusinessObject(Instruction instructionDataObject, DtbConsignmentAddress businessObject)
			{
				return new DtbConsignmentAddressDataObjectReader(instructionDataObject, Reader.logger, Reader.factory, Consignment, packageLinksDictionary, containerLinksDictionary).ReadIntoBusinessObject();
			}

			protected override void RemoveFromCollection(DtbConsignmentAddress address)
			{
				throw new InvalidOperationException("We should not be removing Address from Existing Consignments.");
			}
		}

		#endregion

		#endregion
	}
}



