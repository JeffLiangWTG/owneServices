using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
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
using static Enterprise.Integration.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	internal class DtbBookingConsignmentDataObjectReader : DtbTransportDataObjectReader<DtbBookingConsignment>
	{
		#region Constructors

		internal DtbBookingConsignmentDataObjectReader(UniversalShipment consignmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IColumnIndexer consolidation, UniversalShipment topLevelDataObject, Instruction instruction, DtbBookingConsignment consignment, IColumnIndexer pickUpInstructionToCopy = null, IColumnIndexer depotConfirmationToCopy = null, Instruction additionalInstruction = null)
			: this(consignmentDataObject, logger, factory, consolidation, topLevelDataObject, consignment)
		{
			if (instruction != null)
			{
				InstructionWithPackages = instruction;
				if (instruction.Type.Code.Value == InstructionTypes.Codes.PickUp)
				{
					PickUpInstruction = instruction;
				}
				else if (instruction.Type.Code.Value == InstructionTypes.Codes.Delivery)
				{
					DeliveryInstruction = instruction;
				}
			}

			if (additionalInstruction != null)
			{
				if (additionalInstruction.Type.Code.Value == InstructionTypes.Codes.PickUp)
				{
					PickUpInstruction = additionalInstruction;
				}
				else if (additionalInstruction.Type.Code.Value == InstructionTypes.Codes.Delivery)
				{
					DeliveryInstruction = additionalInstruction;
				}
			}

			if (PickUpInstruction == null)
			{
				PickUpInstruction = dataObject.InstructionCollection != null
					? dataObject.InstructionCollection.FirstOrDefault(i => i.Type.GetCodeAsUpperCase() == InstructionTypes.Codes.PickUp)
					: null;
			}

			if (InstructionWithPackages == null)
			{
				InstructionWithPackages = PickUpInstruction;
			}

			if (consignment != null && (pickUpInstructionToCopy != null || depotConfirmationToCopy != null))
			{
				throw new ArgumentException("We only want to copy the Pick Up Info to new Consignments.");
			}

			DepotConfirmationToCopy = depotConfirmationToCopy;
			PickUpInstructionToCopy = pickUpInstructionToCopy;
		}

		public DtbBookingConsignmentDataObjectReader(UniversalShipment consignmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IColumnIndexer consolidation, UniversalShipment topLevelDataObject, DtbBookingConsignment consignment)
			: this(consignmentDataObject, logger, factory, consolidation, topLevelDataObject)
		{
			Consignment = consignment;
		}

		DtbBookingConsignmentDataObjectReader(UniversalShipment consignmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IColumnIndexer consolidation, UniversalShipment topLevelDataObject)
			: base(consignmentDataObject, topLevelDataObject, logger, factory)
		{
			TopLevelDataObject = Argument.NotNull(topLevelDataObject, "UniversalShipment topLevelDataObject");
			Consolidation = Argument.NotNull(consolidation, "IColumnIndexer consolidation");
		}

		readonly DtbBookingConsignment Consignment;
		readonly IColumnIndexer Consolidation;
		readonly Instruction PickUpInstruction;
		readonly IColumnIndexer DepotConfirmationToCopy;
		readonly IColumnIndexer PickUpInstructionToCopy;
		readonly UniversalShipment TopLevelDataObject;
		readonly Instruction DeliveryInstruction;
		readonly Instruction InstructionWithPackages;

		#endregion

		#region Context

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransportConsignment; }
		}

		#endregion

		#region LogChildTopLevelObjectsOnImport

		protected override bool LogChildTopLevelObjectsOnImport
		{
			get { return Consignment == null || DeliveryInstruction != null; }
		}

		#endregion

		#region Matching Job

		protected override DtbBookingConsignment GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return Consignment;
		}

		protected override IMatchingBusinessEntityFinder<DtbBookingConsignment> GetCombinedReferenceMatcher()
		{
			// No references to match a consignment
			return null;
		}

		#endregion

		#region Create / Update

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(DtbBookingConsignment consignment)
		{
			PopulateData(consignment);
			PopulateRelatedEntities(consignment);
			PopulateConNoteNo(consignment);
		}

		void PopulateData(DtbBookingConsignment consignment)
		{
			var row = GetColumnIndexerFromRow(consignment);
			// Need to have the parent populated straight away
			SetValue(row, DtbBookingSchema.KM_KB_Booking, Consolidation.GetValue(DtbBookingConsolidationSchema.PK));
			SetValue(row, DtbBookingSchema.KM_KT_NKBookingTemplate, DtbBookingConsignment.TemplateCode);
			SetValue(row, DtbBookingSchema.KM_RS_NKServiceLevel, dataObject.ServiceLevel);
		}

		protected override void PopulateRelatedEntities(DtbBookingConsignment consignment)
		{
			base.PopulateRelatedEntities(consignment);

			if (PickUpInstruction == null)
			{
				var errorMessage = Res.GetString("49fac9ca-7250-4906-8b9a-a5b4eb2ab456", "The UXML can not be imported as there is no Pickup instruction.");
				if (dataObject.DataContext.GetMatchingDataSource(DataContextType.TransportBookingConsolidation) != null)
				{
					var errorMessageExtention = Res.GetString("cd18a542-0179-4960-a479-ba8152c60b46", "Possible reason: No matching sub-shipment ID is found.");
					throw new DataObjectReadFailureException(string.Format(CultureInfo.InvariantCulture, "{0} {1}", errorMessage, errorMessageExtention));
				}
			}

			PopulateLocalClient(consignment);
			PopulateAddresses(consignment);

			var packageJobReader = PopulatePackageJob(consignment);
			PopulateInstructions(consignment, packageJobReader);
		}

		void PopulateConNoteNo(DtbBookingConsignment consignment)
		{
			ZString conNoteNo = "";
			var row = GetColumnIndexerFromRow(consignment);

			// the DLV instruction conNoteNo should become the consignment's conNoteNo
			if (DeliveryInstruction != null)
			{
				var allConfirmations = DeliveryInstruction.GetConfirmations();
				var conNoteConfirmations = allConfirmations.Where(c => c.DateDescription.GetValueOrDefault().EqualsIgnoringCase(ConfirmationTypes.Codes.ConNoteNo));

				if (conNoteConfirmations.Count() == 1)
				{
					conNoteNo = conNoteConfirmations.First().Reference.GetValueOrDefault();
				}
			}
			// fallback to the booking-level Transport Ref
			if (conNoteNo.IsEmpty && dataObject.LocalProcessing != null)
			{
				conNoteNo = dataObject.LocalProcessing.ArrivalCartageRef.GetValueOrDefault();
			}

			SetValue(row, DtbBookingSchema.KM_TransportReference, conNoteNo);
		}

		#region PopulateAdditionalReferences

		protected override DataObjectList<AdditionalReference> GetAdditionalReferences(UniversalShipment dataObject)
		{
			return MergeSourceAndTargetDOAdditionalReferences(dataObject.AdditionalReferenceCollection, TopLevelDO.AdditionalReferenceCollection);
		}

		protected override ICusEntryNumAdditionalReferenceCollection GetAdditionalReferenceCollectionForWayBills(DtbBookingConsignment transport)
		{
			return transport.AdditionalReferenceNumbers;
		}

		DataObjectList<AdditionalReference> MergeSourceAndTargetDOAdditionalReferences(
			DataObjectList<AdditionalReference> dataObjectAdditionalReferences,
			DataObjectList<AdditionalReference> topLevelAdditionalReferences)
		{
			DataObjectList<AdditionalReference> result = dataObjectAdditionalReferences ?? topLevelAdditionalReferences;

			if (dataObjectAdditionalReferences != null && topLevelAdditionalReferences != null)
			{
				foreach (var additionalReference in topLevelAdditionalReferences)
				{
					if (!result.Any(a => a.Type.Code == additionalReference.Type.Code))
					{
						result.Add(additionalReference);
					}
				}
			}
			return result;
		}

		#endregion

		#endregion

		#region PopulateLocalClient

		void PopulateLocalClient(DtbBookingConsignment consignment)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var addressTypes = logger.TopLevelDataContext.IsFromSameSystem()
					? new ZString[] { nameof(DocAddressType.LocalClient), AddressTypes.SendersLocalClient }
					: new ZString[] { nameof(DocAddressType.LocalClient) };

				var localClientDataObject = dataObject.OrganizationAddressCollection.FirstOrDefault(addressTypes);
				if (localClientDataObject != null)
				{
					var localClientAddress = new OrganisationDataObjectReader(localClientDataObject, logger, factory).GetMatched();
					if (localClientAddress != null)
					{
						var localClientAddressRow = GetColumnIndexerFromRow(localClientAddress);
						var jobHeader = GetColumnIndexerFromRow(new JobHeader.Loader(consignment).TryLoadOrCreateWithMutex());
						SetValue(jobHeader, JobHeaderSchema.JH_OA_LocalChargesAddr, localClientAddressRow.GetValue(OrgAddressSchema.PK));
					}
				}
			}
		}

		#endregion

		#region PopulateAddresses

		void PopulateAddresses(DtbBookingConsignment consignment)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				foreach (var organisationAddressDataObject in dataObject.OrganizationAddressCollection)
				{
					new OrganisationDataObjectReader(organisationAddressDataObject, logger, factory).GetMatchedOrNew(consignment);
				}
			}
		}

		#endregion

		#region PopulatePackageJob

		PkgPackageJobDataObjectReader PopulatePackageJob(DtbBookingConsignment consignment)
		{
			PkgPackageJobDataObjectReader packageJobReader = null;

			var consignmentRow = GetColumnIndexerFromRow(consignment);
			var packageJob = PopulatePackageJobFromExistingPackages(consignmentRow);
			if (packageJob == null)
			{
				if (PickUpInstruction != null || DeliveryInstruction != null)
				{
					var pickupContainers = GetContainersForThisConsignment(PickUpInstruction);
					var deliveryContainers = GetContainersForThisConsignment(DeliveryInstruction);
					var hasContainersOnPickup = pickupContainers != null && pickupContainers.Count > 0;
					var containers = hasContainersOnPickup ? pickupContainers : deliveryContainers;
					if (containers != null)
					{
						containers = hasContainersOnPickup && deliveryContainers != null && deliveryContainers.Count > 0 ?
							new DataObjectList<Container>(containers.Intersect(deliveryContainers))
							: containers;
					}

					var pickupPackages = GetPackagesForThisConsignment(PickUpInstruction) ?? new List<PackingLine>();
					var deliveryPackages = GetPackagesForThisConsignment(DeliveryInstruction) ?? new List<PackingLine>();
					var packages = GetCommonPackages(pickupPackages, deliveryPackages);
					packages = CleanUpCommonPackages(packages, pickupPackages, deliveryPackages);
					var packageParentDataObject = new PackingSourceDataObject(containers, new DataObjectList<PackingLine>(packages) { Content = CollectionContent.Complete }, dataObject);
					packageJobReader = new PkgPackageJobDataObjectReader(packageParentDataObject, logger, factory, consignment);
					packageJob = packageJobReader.ReadIntoBusinessObject();
				}
			}

			if (packageJob != null)
			{
				SetIsHazardousAndRequiresRefrigeration(consignmentRow, GetColumnIndexerFromRow(packageJob));
			}

			return packageJobReader;
		}

		// Removes common packages where the inner is part of an already included outer
		// ensures that when pkgdivots are split, that the qty and dimms match
		// Ensures that inners of split packages qty matches the outt
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

		List<PackingLine> GetCommonPackages(List<PackingLine> pickupPackages, List<PackingLine> deliveryPackages)
		{
			var result = new List<PackingLine>();

			if (pickupPackages == null || !pickupPackages.Any())
			{
				result = deliveryPackages; // could be empty
			}
			else if (deliveryPackages == null || !deliveryPackages.Any())
			{
				result = pickupPackages;
			}
			else
			{
				result = new List<PackingLine>();

				AddPackagesToResult(pickupPackages, deliveryPackages, result);
				AddPackagesToResult(deliveryPackages, pickupPackages, result);
			}

			return result;
		}

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
			var query = new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignmentRow.GetValue(DtbBookingSchema.PK));
			return factory.LoadTop1<PkgPackageJob>(query);
		}

		#region ValidatePackageStructure

		void ValidatePackageStructure(IColumnIndexer packageJob)
		{
			var pickUpContainers = GetContainersForThisConsignment(PickUpInstruction) ?? Enumerable.Empty<Container>();
			var pickUpPackages = GetPackagesForThisConsignment(PickUpInstruction) ?? new List<PackingLine>();

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

			var deliveryContainers = GetContainersForThisConsignment(DeliveryInstruction);
			if (deliveryContainers != null && deliveryContainers.Any() && deliveryContainers.All(c => !c.ContainerNumber.GetValueOrDefault().IsEmpty))
			{
				result = result.Concat(deliveryContainers.Select(c => c.ContainerNumber.GetValueOrDefault()));
			}

			var deliveryPackages = GetPackagesForThisConsignment(DeliveryInstruction);
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

				SetValue(packageJob, PkgPackageJobSchema.KJ_ParentID, consignmentRow.GetValue(DtbBookingSchema.PK));
				SetValue(packageJob, PkgPackageJobSchema.KJ_ParentTableCode, DtbBookingSchema.Constants.Prefix);

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
			SetValue(consignment, DtbBookingSchema.KM_IsHazardous, undgs.Length > 0);
		}

		void SetRequiresRefrigerationIfAnyPackagesAreTemperatureControlled(IColumnIndexer consignment, ZQuery packagesQuery, object[] packagePKs)
		{
			packagesQuery.AddToFilter(PkgPackageSchema.KP_RequiresTemperatureControl, true);
			var refrigeratedPackageRows = factory.RowFactory.Load(PkgPackageSchema.Constants.TableName, packagesQuery);
			var containerQuery = new ZQuery();
			containerQuery.AddToFilter(PkgPackageContainerSchema.K0_KP_Package, packagePKs);
			containerQuery.AddToFilter(PkgPackageContainerSchema.K0_IsControlledAtmosphere, true);
			var refrigeratedContainerRows = factory.RowFactory.Load(PkgPackageContainerSchema.Constants.TableName, containerQuery);
			SetValue(consignment, DtbBookingSchema.KM_RequiresRefrigeration, refrigeratedPackageRows.Length > 0 || refrigeratedContainerRows.Length > 0);
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
							clone.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance); // not usually ok to use this strategy, ok here as it is in xml deserialisation
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

		#region PopulateInstructions

		void PopulateInstructions(DtbBookingConsignment consignment, PkgPackageJobDataObjectReader packageJobReader)
		{
			if (PickUpInstruction != null)
			{
				var deliveryInstruction = DeliveryInstruction ?? GetEmptyDeliveryConsigneeInstruction();
				var pickUpConfirmationToCopy = PickUpInstructionToCopy != null
					? factory.GetConfirmation(PickUpInstructionToCopy, ConfirmationTypes.Codes.PickUp) // we will be copying the Pick Up info for new Consignments
					: null;

				// if consignment exists already, we don't want to read in the pickup information
				var instructions = IsNewBO
					? new[] { GetPickUpInstructionDataObject(pickUpConfirmationToCopy), deliveryInstruction }
					: new[] { deliveryInstruction };

				var instructionCollectionReader = new InstructionDataObjectCollectionReader(this, consignment, instructions, packageJobReader);
				instructionCollectionReader.ReadIntoCollectionRetainingUnmatchedElements();
				consignment.UpdateDepotInstructions(); // TODO this needs to be refactored to be done in Universal/Row Factory code.

				var consignmentRow = GetColumnIndexerFromRow(consignment);
				if (IsNewBO)
				{
					// we want to copy allocation info from the existing Pick Up to the new Consignments.
					if (pickUpConfirmationToCopy != null)
					{
						CopyStatusAndRunSheetInformation(consignmentRow, pickUpConfirmationToCopy);
						UpdateConsignmentStatus(consignmentRow);
					}
				}
				else
				{
					// need to update the Divots for the Pick Up and Depot Instructions
					// as they would not have been updated by the Instruction Reader.
					UpdatePackageDivots(consignmentRow);

					// The lazy Loaded PackageJob BizO hooks package events so that the divots automatically update.
					// Since it is possible for the Packages to be split in Consignments on Factory Save, we need to 
					// have these events hooked for now.
					PokePackageJob(consignment);
				}
			}
		}

		static PkgPackageJob PokePackageJob(DtbBookingConsignment consignment)
		{
			return consignment.PackageJob;
		}

		Instruction GetEmptyDeliveryConsigneeInstruction()
		{
			var deliveryInstruction = new Instruction
			{
				Type = new CodeDescriptionPair { Code = InstructionTypes.Codes.Delivery },
				Address = new OrganizationAddress { AddressType = nameof(DocAddressType.LocalCartageImporter) }
			};

			return deliveryInstruction;
		}

		Instruction GetPickUpInstructionDataObject(IColumnIndexer pickUpConfirmationToCopy)
		{
			Instruction result;

			if (PickUpInstructionToCopy != null)
			{
				// setup data object to contain the Confirmation fields to copy from the existing Pick Up Consignment
				result = new Instruction(DefaultDataObjectWriterStrategy.Instance)
				{
					Address = PickUpInstruction.Address,
					DropMode = new DropMode { Code = PickUpInstructionToCopy.GetValue(DtbBookingInstructionSchema.KN_DropMode) },
					ServiceInstruction = PickUpInstructionToCopy.GetValue(DtbBookingInstructionSchema.KN_ServiceInstruction),
					Type = new CodeDescriptionPair { Code = PickUpInstructionToCopy.GetValue(DtbBookingInstructionSchema.KN_InstructionType) }
				};
				result.SetInstructionPackingLineLinkCollection(() => new List<InstructionPackingLineLink>
				{
					new InstructionPackingLineLink { ConfirmationCollection = new List<Confirmation>
					{
						new Confirmation
						{
							ActualDate = pickUpConfirmationToCopy.GetValue(DtbBookingConfirmationSchema.KK_Actual),
							DateDescription = pickUpConfirmationToCopy.GetValue(DtbBookingConfirmationSchema.KK_ConfirmationType),
							EstimatedDate = pickUpConfirmationToCopy.GetValue(DtbBookingConfirmationSchema.KK_Estimated),
							IsEmptyContainer = pickUpConfirmationToCopy.GetValue(DtbBookingConfirmationSchema.KK_IsEmptyContainer),
							ReceivedBy = pickUpConfirmationToCopy.GetValue(DtbBookingConfirmationSchema.KK_ReceivedBy),
							Reference = pickUpConfirmationToCopy.GetValue(DtbBookingConfirmationSchema.KK_ReferenceNum),
							RequiredFromDate = pickUpConfirmationToCopy.GetValue(DtbBookingConfirmationSchema.KK_RequiredFrom),
							RequiredToDate = pickUpConfirmationToCopy.GetValue(DtbBookingConfirmationSchema.KK_RequiredTo)
						}
					} }
				});
			}
			else
			{
				result = PickUpInstruction;
			}

			return result;
		}

		#region CopyStatusAndRunSheetInformation

		void CopyStatusAndRunSheetInformation(IColumnIndexer consignmentRow, IColumnIndexer pickUpConfirmationToCopy)
		{
			// copy across status, runsheet allocation and signature
			var newPickUpInstructionRow = factory.GetInstruction(consignmentRow, InstructionTypes.Codes.PickUp);
			SetValue(newPickUpInstructionRow, DtbBookingInstructionSchema.KN_Status, PickUpInstructionToCopy.GetValue(DtbBookingInstructionSchema.KN_Status));

			var newPickUpConfirmation = factory.GetConfirmation(newPickUpInstructionRow, ConfirmationTypes.Codes.PickUp);
			SetValue(newPickUpConfirmation, DtbBookingConfirmationSchema.KK_K1_RunSheetInstruction, pickUpConfirmationToCopy.GetValue(DtbBookingConfirmationSchema.KK_K1_RunSheetInstruction));
			SetValue(newPickUpConfirmation, DtbBookingConfirmationSchema.KK_ReceivedBySignature, pickUpConfirmationToCopy.GetValue(DtbBookingConfirmationSchema.KK_ReceivedBySignature));

			// copy runsheet allocation and signature from the depot confirmation + calculate correct status
			if (DepotConfirmationToCopy != null)
			{
				var newDepotInstructionRow = factory.GetInstruction(consignmentRow, InstructionTypes.Codes.Multi);
				var newDeliverToDepotConfirmation = factory.GetConfirmation(newDepotInstructionRow, ConfirmationTypes.Codes.Delivery);
				var runSheetInstructionPK = DepotConfirmationToCopy.GetValue(DtbBookingConfirmationSchema.KK_K1_RunSheetInstruction);
				SetValue(newDeliverToDepotConfirmation, DtbBookingConfirmationSchema.KK_K1_RunSheetInstruction, runSheetInstructionPK);
				SetValue(newDeliverToDepotConfirmation, DtbBookingConfirmationSchema.KK_ReceivedBySignature, DepotConfirmationToCopy.GetValue(DtbBookingConfirmationSchema.KK_ReceivedBySignature));
				SetStatusBasedOnRunSheetInstruction(newDepotInstructionRow, runSheetInstructionPK);
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
				SetValue(newDepotInstructionRow, DtbBookingInstructionSchema.KN_Status, depotInstructionStatus);
			}
		}

		#endregion

		#region UpdateConsignmentStatus

		void UpdateConsignmentStatus(IColumnIndexer consignmentRow)
		{
			var newPickUpInstructionRow = factory.GetInstruction(consignmentRow, InstructionTypes.Codes.PickUp);
			var status = newPickUpInstructionRow.GetValue(DtbBookingInstructionSchema.KN_Status);
			if (status == TransportStatuses.Codes.Allocated)
			{
				SetValue(consignmentRow, DtbBookingSchema.KM_Status, TransportStatuses.Codes.PickUpAllocated);
			}
			else if (status == TransportStatuses.Codes.PickedUp)
			{
				SetValue(consignmentRow, DtbBookingSchema.KM_Status, TransportStatuses.Codes.PickedUp);
			}
		}

		#endregion

		#region UpdatePackageDivots

		void UpdatePackageDivots(IColumnIndexer consignmentRow)
		{
			var pickUpInstructionRow = factory.GetInstruction(consignmentRow, InstructionTypes.Codes.PickUp);
			new InstructionDivotHelper(new Instruction(DefaultDataObjectWriterStrategy.Instance), logger, factory).PopulatePackageDivots(pickUpInstructionRow, consignmentRow);

			var multiInstructionRow = factory.GetInstruction(consignmentRow, InstructionTypes.Codes.Multi);
			new InstructionDivotHelper(new Instruction(DefaultDataObjectWriterStrategy.Instance), logger, factory).PopulatePackageDivots(multiInstructionRow, consignmentRow);
		}

		#endregion

		class InstructionDataObjectCollectionReader : DataObjectCollectionReader<Instruction, DtbConsignmentInstruction>
		{
			public InstructionDataObjectCollectionReader(DtbBookingConsignmentDataObjectReader reader, DtbBookingConsignment consignment,
				Instruction[] instructionDataObjects, PkgPackageJobDataObjectReader packageJobReader)
				: base(instructionDataObjects)
			{
				Consignment = consignment;
				Reader = reader;
				PackageJobReader = packageJobReader;
			}

			ZInt Sequence = 0;
			readonly DtbBookingConsignment Consignment;
			readonly DtbBookingConsignmentDataObjectReader Reader;
			readonly PkgPackageJobDataObjectReader PackageJobReader;

			protected override void AddToCollection(DtbConsignmentInstruction instruction)
			{
				var row = GetColumnIndexerFromRow(instruction);
				Reader.SetValue(row, DtbBookingInstructionSchema.KN_KM_BookingMovement, Consignment.GetValue(DtbBookingSchema.PK));
				Reader.SetValue(row, DtbBookingInstructionSchema.KN_Sequence, ++Sequence);
			}

			protected override DtbConsignmentInstruction[] BusinessObjects
			{
				get { return Reader.factory.Load<DtbConsignmentInstruction>(new ZQuery(DtbBookingInstructionSchema.KN_KM_BookingMovement, Consignment[DtbBookingSchema.Constants.PK])); }
			}

			protected override DtbConsignmentInstruction FindMatchingBusinessObject(Instruction dataObject)
			{
				return null;
			}

			protected override DtbConsignmentInstruction ReadIntoBusinessObject(Instruction instructionDataObject, DtbConsignmentInstruction businessObject)
			{
				return new DtbConsignmentInstructionDataObjectReader(instructionDataObject, Reader.logger, Reader.factory, Consignment, PackageJobReader).ReadIntoBusinessObject();
			}

			protected override void RemoveFromCollection(DtbConsignmentInstruction instruction)
			{
				throw new InvalidOperationException("We should not be removing Instructions from Existing Consignments.");
			}
		}

		#endregion

		#endregion
	}
}



