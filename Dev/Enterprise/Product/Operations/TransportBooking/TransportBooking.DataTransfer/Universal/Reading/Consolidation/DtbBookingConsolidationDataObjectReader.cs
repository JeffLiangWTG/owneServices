using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public sealed class DtbBookingConsolidationDataObjectReader : ShipmentDataObjectReader<DtbBookingConsolidation>
	{
		public DtbBookingConsolidationDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, DtbBooking bookingTarget = null)
				: base(dataObject, logger, factory)
		{
			Booking = bookingTarget;
		}

		readonly DtbBooking Booking;
		readonly ZString[] ShipmentTypesToIgnore = new ZString[]
		{
			Constants.ShipmentTypes.CoLoadMaster,
			Constants.ShipmentTypes.BlindCoLoadMaster,
			Constants.ShipmentTypes.AssemblyMaster,
		};

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransportBookingConsolidation; }
		}

		protected override DtbBookingConsolidation GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return Booking != null ? Booking.ConsolidationSingleJob : null;
		}

		/// <summary>
		/// Find Consolidation that has a matching Booking Party and a Booking with Booking TB Number
		/// But only if Booking Target wasn't specified
		/// </summary>
		/// <returns></returns>
		protected override IMatchingBusinessEntityFinder<DtbBookingConsolidation> GetCombinedReferenceMatcher()
		{
			var dataContext = dataObject != null ? dataObject.DataContext : null;
			var sendingParty = dataContext != null ? dataContext.GetSourceOrganisation(logger, factory.BOFactory) : null;

			var bookingConsolidationReferences = new TransportBookingConsolidationReferences();
			if (sendingParty != null)
			{
				bookingConsolidationReferences.SendingPartyOrganizationPK = sendingParty.PK;
				bookingConsolidationReferences.SendingPartyOrganizationCode = sendingParty.OH_Code;
			}
			(_, var sender) = DtbDataObjectReaderHelper.GetInterchangeTypeAndSenderFromLogger(logger);
			if (DtbDataObjectReaderHelper.MessageIsFromTestCba(sender, dataObject))
			{
				bookingConsolidationReferences.SendingPartyExternalTransportBookingNumbers = GetExternalTransportBookingNumbers();
			}
			if (bookingConsolidationReferences.SendingPartyExternalTransportBookingNumbers == null || !bookingConsolidationReferences.SendingPartyExternalTransportBookingNumbers.Any())
			{
				bookingConsolidationReferences.SendingPartyTransportBookingNumbers = GetSendingPartyTransportBookingNumbers();
			}

			return new TransportBookingConsolidationMatcher(factory.BOFactory, bookingConsolidationReferences, logger);
		}

		IEnumerable<ZString> GetSendingPartyTransportBookingNumbers()
		{
			var result = new List<ZString>();

			var dataSourcesForDataObject = dataObject.GetMatchingDataSources(DataContextType.TransportBooking);
			if (dataSourcesForDataObject != null)
			{
				result.AddRange(dataSourcesForDataObject.Where(d => d.Key.HasValue).Select(d => d.Key.Value));
			}

			return result;
		}

		IEnumerable<ZString> GetExternalTransportBookingNumbers()
		{
			var result = new List<ZString>();
			result.AddRange(GetExternalTransportBookingNumbersFromCollection(dataObject.AdditionalReferenceCollection));
			foreach (var subShipment in dataObject.SubShipmentCollection)
			{
				result.AddRange(GetExternalTransportBookingNumbersFromCollection(subShipment.AdditionalReferenceCollection));
			}

			return result;
		}

		IEnumerable<ZString> GetExternalTransportBookingNumbersFromCollection(DataObjectList<AdditionalReference> additionalReferences) =>
			additionalReferences == null
				? Enumerable.Empty<ZString>()
				: additionalReferences
					.Where(r => r.Type.Code.HasValue && r.Type.Code.Value == (ZString)TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber && r.ReferenceNumber.HasValue && !r.ReferenceNumber.Value.IsEmpty)
					.Select(r => r.ReferenceNumber.Value)
					.ToList();

		class TransportBookingConsolidationReferences : IReferencesParent
		{
			public ZGuid SendingPartyOrganizationPK { get; set; }
			public ZString SendingPartyOrganizationCode { get; set; }
			public IEnumerable<ZString> SendingPartyTransportBookingNumbers { get; set; }
			public IEnumerable<ZString> SendingPartyExternalTransportBookingNumbers { get; set; }
		}

		class TransportBookingConsolidationMatcher : CombinationKeyMatcher<DtbBookingConsolidation, TransportBookingConsolidationReferences>
		{
			public TransportBookingConsolidationMatcher(BusinessObjectFactory factory, TransportBookingConsolidationReferences references, IXmlImportLogger logger)
					: base(factory, references, logger)
			{ }

			protected override bool CheckLatestParent(DtbBookingConsolidation parent, DtbBookingConsolidation parentToCompare)
			{
				return parent.Logs.AddedLog.SL_EventTime > parent.Logs.AddedLog.SL_EventTime;
			}

			protected override void BuildMatchingQueryAndMatchDelegates(TransportBookingConsolidationReferences referencesParent)
			{
				if (!referencesParent.SendingPartyOrganizationPK.IsEmpty)
				{
					if (referencesParent.SendingPartyExternalTransportBookingNumbers?.Any() ?? false)
					{
						logger.Log(Integration.LogType.Information, FormattableString.Invariant($"Searching for Booking Consolidation, attempting to match Transport Booking Job ID to External Transport Booking number(s) {string.Join(",", referencesParent.SendingPartyExternalTransportBookingNumbers)} from sending system"));

						var consolidationQuery = new ZDBOnlyQuery(typeof(DtbBookingConsolidation));
						consolidationQuery.AddSubQuery(GetTransportBookingNumberSubQuery(referencesParent.SendingPartyExternalTransportBookingNumbers), JoinCondition.And);

						var matchDelegates = new List<MatchDelegate>
						{
							c => { return c.Bookings.Any(b => referencesParent.SendingPartyExternalTransportBookingNumbers.Contains(b.KM_JobID)) ? 1 : 0; }
						};

						AddPossibleMatch(consolidationQuery, consolidation => { return matchDelegates.TrueForAll(m => m(consolidation) == 1) ? 1 : 0; });
					}

					if (referencesParent.SendingPartyTransportBookingNumbers?.Any() ?? false)
					{
						logger.Log(Integration.LogType.Information, FormattableString.Invariant($"Searching for Booking Consolidation, attempting to match External Transport Booking numbers to Transport Booking number(s) {string.Join(",", referencesParent.SendingPartyTransportBookingNumbers)} from sending system and match Address to Sending Party {referencesParent.SendingPartyOrganizationCode}"));

						var consolidationQuery = new ZDBOnlyQuery(typeof(DtbBookingConsolidation));
						consolidationQuery.AddSubQuery(GetSendingPartyTransportBookingNumberSubQuery(referencesParent.SendingPartyTransportBookingNumbers), JoinCondition.And);
						consolidationQuery.AddSubQuery(GetSendingPartySubQuery(referencesParent.SendingPartyOrganizationPK), JoinCondition.And);

						var matchDelegates = new List<MatchDelegate>
						{
							c => GetMatchCount(c.BookedByOrganisationPK, referencesParent.SendingPartyOrganizationPK),
							c => { return c.TransportBookingPartyReferences.Any(r => referencesParent.SendingPartyTransportBookingNumbers.Contains(r)) ? 1 : 0; }
						};

						AddPossibleMatch(consolidationQuery, consolidation => { return matchDelegates.TrueForAll(m => m(consolidation) == 1) ? 1 : 0; });
					}
				}
			}

			ZDBOnlySubQuery GetSendingPartySubQuery(ZGuid sendingPartyOrganisationPK)
			{
				return JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(sendingPartyOrganisationPK);
			}

			ZDBOnlySubQuery GetSendingPartyTransportBookingNumberSubQuery(IEnumerable<ZString> sendingPartyTransportBookingNumbers)
			{
				var additionalReferencesQuery = new ZDBOnlySubQuery(typeof(Integration.Customs.ICusEntryNumber), CusEntryNumSchema.CE_ParentID);
				additionalReferencesQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber);
				additionalReferencesQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, sendingPartyTransportBookingNumbers);

				var bookingQuery = new ZDBOnlySubQuery(typeof(DtbBooking), DtbBookingSchema.KM_KB_Booking);
				bookingQuery.AddSubQuery(additionalReferencesQuery, JoinCondition.And);

				return bookingQuery;
			}

			ZDBOnlySubQuery GetTransportBookingNumberSubQuery(IEnumerable<ZString> transportBookingNumbers)
			{
				var bookingQuery = new ZDBOnlySubQuery(typeof(DtbBooking), DtbBookingSchema.KM_KB_Booking);
				bookingQuery.AddToFilter(DtbBookingSchema.KM_JobID, transportBookingNumbers);

				return bookingQuery;
			}

			protected override void BuildFallbackMatchDelegates(TransportBookingConsolidationReferences referencesParent)
			{
			}
		}

		protected override void PopulateBusinessObject(DtbBookingConsolidation consolidation)
		{
			var matchingDO = GetDataObject(dataObject);
			var consolDO = DtbParentInfoLoader.GetConsolDO(dataObject, GetDirection(matchingDO.TransportBookingDirection));
			var parentInfo = new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(matchingDO, consolDO));
			consolidation.SetParentInfo(parentInfo);
			if (consolidation.KB_JobType != TransportConsolidationJobTypes.Codes.Booking && consolidation.Bookings.Count > 0)
			{
				throw new DataObjectReadFailureException(DtbBookingConsolidationSchema.Constants.KB_JobType + " cannot be changed from " + consolidation.KB_JobType + " if the Consolidation already has Bookings.");
			}
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;
			PopulateJobDirection(consolidation, matchingDO.TransportBookingDirection?.Code);
			SetValue(consolidation, DtbBookingConsolidationSchema.KB_GoodsDescription, matchingDO.GoodsDescription);

			PopulateSendingParty(consolidation, dataObject);

			// existing

			var hasCartageAgentRecipientRole = HasCartageAgentRecipientRole;

			// This needs to be done before reading in packages
			var existingBookingInfos = hasCartageAgentRecipientRole
					? DtbBookingConsolidationReaderHelper.GetExistingBookingInfos(consolidation)
					: Enumerable.Empty<ExistingBookingInfo>();

			var targetPackagePks = Enumerable.Empty<ZGuid>();

			var source = consolDO?.DataContext?.DataSourceCollection?.FirstOrDefault();
			IDtbBookingConsolidationExtender extender = null;
			if (source != null)
			{
				var sourceType = source.Type.ToString();
				var extenders = ObjectFactory.Get<Hashtable>("DtbBookingConsolidationExtenders");
				if (extenders.ContainsKey(sourceType))
				{
					extender = (IDtbBookingConsolidationExtender)((ObjectHandle)extenders[sourceType]).GetObject();
				}
			}

			if (Booking == null || dataObject != matchingDO)
			{
				PopulateAdditionalReferences(consolidation, matchingDO);
			}

			if (Booking != null && matchingDO.InstructionCollection != null && matchingDO.InstructionCollection.Content != CollectionContent.Partial)
			{
				targetPackagePks = Booking.Instructions.SelectMany(instruction => instruction.DivotsWithPackages.Typed.Select(divot => divot.KD_KP_Package));
				Booking.Instructions.DeleteAll();
			}
			else if (Booking == null)
			{
				if (extender != null)
				{
					extender.GetExistingDtbBookings(consolidation, source.Key, consolidation.Factory).ForEach(idtbBooking => ((DtbBooking)idtbBooking).Instructions.DeleteAll());
				}
				else
				{
					consolidation.Bookings.ToList().ForEach(booking => booking.Instructions.DeleteAll());
				}
			}

			// packing job

			var packingParent = GetPackageParentDataObject(consolDO, matchingDO);

			var isAssignedByPassedPackage = extender?.IsCreatingBookingByPassedPackages ?? false;

			var importOption = Booking != null ? ImportOption.PartialMatch : isAssignedByPassedPackage ? ImportOption.KeepUnmatchedPackages | ImportOption.MatchOnPackingLineID : ImportOption.Default;

			var packageJobReader = new PkgPackageJobDataObjectReader(packingParent, logger, factory, consolidation, targetPackagePks: targetPackagePks, importOptions: importOption);
			packageJobReader.ReadIntoBusinessObject();

			LinkParent(consolidation, matchingDO);

			// transport bookings

			if (hasCartageAgentRecipientRole)
			{
				var assignedPackages = isAssignedByPassedPackage ? packageJobReader.PackageLinks.Values.Where(p => p.IsOuter) : null;
				DtbBookingConsolidationReaderHelper.ReadInCartageAdvice(consolidation, matchingDO, existingBookingInfos, dataObject, logger, factory, packageJobReader.PackageContainerLinks, assignedPackages);
			}
			else if (Booking != null)
			{
				CheckValidInstructionAndPackageData(matchingDO.InstructionCollection, packingParent.ContainerCollection, packingParent.PackingLineCollection);

				var bookingReader = new DtbBookingDataObjectReaderFromDtbBooking(matchingDO, logger, factory, consolidation, dataObject, Booking, packageJobReader);
				var readBooking = bookingReader.ReadIntoBusinessObject();
				using (readBooking.SuspendSettingServiceLevel())
				{
					consolidation.Bookings.Add(readBooking);
				}
			}
			else
			{
				ReadInBookings(consolidation, packageJobReader, dataObject);
			}

			if (!logger.IsInternalImport())
			{
				SchedulesReader.ReadIntoCollection(consolDO, logger, factory, consolidation);
			}

			PopulateNotes(consolidation);
		}

		void PopulateJobDirection(DtbBookingConsolidation consolidation, ZString? transportBookingDirectionCode)
		{
			var jobDirection = ZString.Empty;
			switch (transportBookingDirectionCode)
			{
				case null:
					return;

				case "ORG":
				case "EXP":
					jobDirection = "PIC";
					break;

				case "DST":
				case "IMP":
					jobDirection = "DLV";
					break;

				default:
					jobDirection = transportBookingDirectionCode.Value;
					break;
			}

			SetValue(consolidation, DtbBookingConsolidationSchema.KB_JobDirection, jobDirection);
		}

		static void CheckValidInstructionAndPackageData(DataObjectList<Instruction> instructions, DataObjectList<Container> containers, DataObjectList<PackingLine> packingLines)
		{
			var hasInstructionsSpecified = instructions != null;
			var hasPackagesSpecified = containers != null || packingLines != null;
			if ((hasPackagesSpecified && !hasInstructionsSpecified) || (hasInstructionsSpecified && !hasPackagesSpecified))
			{
				var errorMessage = Res.GetString("05EBD4E7-BB71-40E6-B199-3F813C5626E4", "Cannot import Transport Booking, ensure <ContainerCollection>/<PackingLineCollection> and <InstructionCollection> are specified.");
				throw new DataObjectReadFailureException(errorMessage);
			}
		}

		void PopulateAdditionalReferences(DtbBookingConsolidation consolidation, UniversalShipment dataObject)
		{
			AdditionalReferenceCollectionReaderHelper.ReadAdditionalReferences(consolidation, dataObject, logger, factory);
			if (DtbDataObjectReaderHelper.HasSourceContext(dataObject.DataContext, DataContextType.ForwardingShipment)
				&& dataObject.ParentShipmentCollection?.FirstOrDefault() is UniversalShipment firstParentShipment
				&& DtbDataObjectReaderHelper.HasSourceContext(firstParentShipment.DataContext, DataContextType.ForwardingConsol))
			{
				AdditionalReferenceCollectionReaderHelper.ReadNewAdditionalReferences(consolidation, dataObject.ParentShipmentCollection?.FirstOrDefault(), logger, factory);
			}
			PopulateBookingConfirmationReference(consolidation);
		}

		void PopulateBookingConfirmationReference(DtbBookingConsolidation consolidation)
		{
			if (!string.IsNullOrEmpty(dataObject.BookingConfirmationReference))
			{
				PopulateBookingConfirmationReferenceCore(consolidation, (ZString)dataObject.BookingConfirmationReference);
			}
			else if (DtbDataObjectReaderHelper.HasSourceContext(dataObject.DataContext, DataContextType.ForwardingShipment)
				&& dataObject.ParentShipmentCollection?.FirstOrDefault() is UniversalShipment firstParentShipment
				&& DtbDataObjectReaderHelper.HasSourceContext(firstParentShipment.DataContext, DataContextType.ForwardingConsol)
				&& !string.IsNullOrEmpty(firstParentShipment.BookingConfirmationReference))
			{
				PopulateBookingConfirmationReferenceCore(consolidation, (ZString)dataObject.ParentShipmentCollection?.FirstOrDefault()?.BookingConfirmationReference);
			}
		}

		void PopulateBookingConfirmationReferenceCore(DtbBookingConsolidation consolidation, ZString referenceNumber)
		{
			var reference = consolidation.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportAdditionalReferenceTypes.Codes.CarrierBookingReference);
			if (reference == null)
			{
				reference = consolidation.AdditionalReferenceNumbers.AddNew();
				reference.CE_EntryType = TransportAdditionalReferenceTypes.Codes.CarrierBookingReference;
			}
			reference.CE_EntryNum = referenceNumber;
		}

		void PopulateNotes(DtbBookingConsolidation consolidation)
		{
			if (dataObject.NoteCollection != null)
			{
				var supportedNoteTypes = consolidation.NoteTypes.Cast<PredefinedNoteType>();
				var notesToImport = new DataObjectList<Note>();
				foreach (var note in dataObject.NoteCollection)
				{
					var isNoteReadonly = PredefinedNoteTypes.Instance.NoteTypeByDescription(note.Description)?.IsReadOnlyAfterAdd ?? false;
					if (!isNoteReadonly || supportedNoteTypes.Any(t => t.Description.Equals(note.Description)))
					{
						notesToImport.Add(note);
					}
				}

				new NotesCollectionReader(notesToImport, logger, factory, consolidation).ReadIntoCollection();
			}
		}

		void LinkParent(DtbBookingConsolidation consolidation, UniversalShipment sourceDO)
		{
			if (logger.IsInternalImport())
			{
				var dataSource = sourceDO.DataContext.DataSourceCollection.First();
				var parent = dataSource.GetLoadedJobFromDataContextType(logger.TopLevelDataObject, factory.BOFactory);
				if (parent != null && !(parent is DtbBookingConsolidation) && consolidation.KB_ParentTableCode.IsEmpty && consolidation.KB_ParentID.IsEmpty)
				{
					consolidation.KB_ParentTableCode = parent.TablePrefix;
					consolidation.KB_ParentID = parent.PK;
					logger.LogLinkCreated(factory, consolidation.GetUniversalDataContextManager(), parent.GetUniversalDataContextManager());
				}
			}
		}

		DtbBookingDirection GetDirection(TransportBookingDirection doDirection)
		{
			if (!Enum.TryParse<DtbBookingDirection>(doDirection.GetCodeAsUpperCase(), out var direction))
			{
				direction = DtbBookingDirection.LOC;
			}

			return direction;
		}

		IPackageParentDataObject GetPackageParentDataObject(UniversalShipment consolDO, UniversalShipment sourceDO)
		{
			var packingLines = GetPackingLines(sourceDO);

			if (packingLines == null && consolDO != null)
			{
				packingLines = consolDO.PackingLineCollection;
			}

			var containers = sourceDO.ContainerCollection;
			var containersHaveLinks = HasContainerLinks(containers) && HasContainerLinks(consolDO.ContainerCollection);

			if (packingLines == null && consolDO == sourceDO && (containersHaveLinks || (containers == null && GetPackingLinesFromSubShipments(consolDO).Count != 0)))
			{
				var packingLinesFromSubShipments = GetPackingLinesFromSubShipments(consolDO);
				packingLines = packingLinesFromSubShipments.Count != 0 ? packingLinesFromSubShipments : packingLines;
			}
			else if (consolDO != sourceDO && containersHaveLinks)
			{
				var consolContainerLinks = consolDO.ContainerCollection.Select(l => l.Link.Value);
				containers = new DataObjectList<Container>(containers.Where(c => consolContainerLinks.Contains(c.Link.Value)));
			}
			else if (packingLines == null && sourceDO.Order?.OrderLineCollection != null)
			{
				packingLines = PopulatePackingLinesFromOrderLines(sourceDO);
			}
			else if (containers == null && consolDO != null)
			{
				if (packingLines != null)
				{
					containers = new DataObjectList<Container>(consolDO.ContainerCollection.GetContainersReferencedBy(packingLines));
				}
				else
				{
					containers = consolDO.ContainerCollection;
				}
			}

			var parentPackingLines = GetParentPackingLines(sourceDO);
			FixParentPackingLineLinks(parentPackingLines);

			var combinedPackingLines = CombinePackingLines(parentPackingLines, packingLines);

			var packageParentDataObject = new PackingSourceDataObject(containers, combinedPackingLines, sourceDO);
			return packageParentDataObject;
		}

		DataObjectList<PackingLine> PopulatePackingLinesFromOrderLines(UniversalShipment shipment)
		{
			DataObjectList<PackingLine> packingLines = new DataObjectList<PackingLine>();

			if (DtbDataObjectReaderHelper.HasSourceContext(shipment.DataContext, DataContextType.WarehouseOrder))
			{
				foreach (var orderLine in shipment.Order.OrderLineCollection)
				{
					var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.Instance);

					packingLine.PackQty = (int)orderLine.OrderedQty;
					packingLine.PackType = shipment.OuterPacksPackageType;
					packingLine.SetUNDGCollection(() => orderLine.UNDGCollection);

					packingLines.Add(packingLine);
				}
			}

			return packingLines;
		}

		DataObjectList<PackingLine> GetPackingLines(UniversalShipment shipment)
		{
			DataObjectList<PackingLine> packingLines = null;

			if (DtbDataObjectReaderHelper.HasSourceContext(shipment.DataContext, DataContextType.HVLVBookingHeader))
			{
				packingLines = GetPackingLinesFromSubShipments(shipment);
			}
			else
			{
				packingLines = shipment.PackingLineCollection;
			}

			return packingLines;
		}

		DataObjectList<PackingLine> GetPackingLinesFromSubShipments(UniversalShipment shipment)
		{
			var result = new DataObjectList<PackingLine>();

			if (shipment != null && shipment.SubShipmentCollection != null)
			{
				foreach (var subShipment in shipment.SubShipmentCollection)
				{
					if (subShipment != null && subShipment.PackingLineCollection != null)
					{
						result.AddRange(subShipment.PackingLineCollection);
						if (!IgnoreSubShipmentPackLines(subShipment))
						{
							result.AddRange(GetPackingLinesFromSubShipments(subShipment));
						}
					}
				}
			}

			return result;
		}

		bool IgnoreSubShipmentPackLines(UniversalShipment shipment)
		{
			var code = shipment?.ShipmentType?.Code;
			return (code.HasValue && ShipmentTypesToIgnore.Contains(code.Value));
		}

		bool HasContainerLinks(IEnumerable<Container> containers)
		{
			return containers != null && containers.Any() && containers.All(c => c.Link.HasValue);
		}

		DataObjectList<PackingLine> GetParentPackingLines(UniversalShipment shipment)
		{
			return shipment.ParentPackingLineCollection;
		}

		DataObjectList<PackingLine> CombinePackingLines(DataObjectList<PackingLine> parentPackingLines, DataObjectList<PackingLine> childPackingLines)
		{
			if (parentPackingLines == null && childPackingLines == null)
			{
				return null;
			}

			var combinedPackingLines = new DataObjectList<PackingLine>() { Content = childPackingLines.Content };

			if (parentPackingLines != null && childPackingLines != null)
			{
				foreach (var child in childPackingLines.ToArray())
				{
					foreach (var parent in parentPackingLines)
					{
						if (child.ParentPackingLineLink.HasValue && parent.Link.HasValue && ConvertLinkValue(child.ParentPackingLineLink) == parent.Link.Value)
						{
							if (parent.PackingLineCollection == null)
							{
								parent.SetWriterStrategy(DefaultDataObjectWriterStrategy.Instance);
								parent.SetPackingLineCollection(() =>
								{
									return new List<PackingLine>();
								});
							}
							parent.PackingLineCollection.Add(child);
							childPackingLines.Remove(child);
							break;
						}
					}
				}
			}
			combinedPackingLines.AddRange(parentPackingLines);
			combinedPackingLines.AddRange(childPackingLines);

			return combinedPackingLines;
		}

		void FixParentPackingLineLinks(IEnumerable<PackingLine> parentPackingLines)
		{
			if (parentPackingLines != null)
			{
				foreach (var parent in parentPackingLines)
				{
					parent.Link = ConvertLinkValue(parent.Link);
				}
			}
		}

		ZInt? ConvertLinkValue(ZInt? original)
		{
			return original * -1 - 1;
		}

		void ReadInBookings(DtbBookingConsolidation consolidation, PkgPackageJobDataObjectReader packageJobReader, UniversalShipment topLevelDO)
		{
			var bookings = GetBookingDataObjects();
			var bookingCollecionReader = new DtbBookingDataObjectCollectionReader(this, consolidation, packageJobReader, bookings, topLevelDO);
			bookingCollecionReader.ReadIntoCollectionRetainingUnmatchedElements();
		}

		UniversalShipment[] GetBookingDataObjects()
		{
			var result = Array.Empty<UniversalShipment>();

			var dataObjectIsConsolidation = SchemaVersionManager.Current != UniversalXmlSchema.Version_2012_11_DO_NOT_USE || dataObject.ParentShipmentCollection == null;
			if (dataObjectIsConsolidation && dataObject.SubShipmentCollection != null)
			{
				// tested in DtbBookingConsolidationDataContextManagerTest.TestReadFromTestFile_TransportBooking_DataTarget_TB
				var bookings = dataObject.SubShipmentCollection.Where(s => DtbDataObjectReaderHelper.HasSourceContext(s.DataContext, DataContextType.TransportBooking) || HasTargetContext(s.DataContext, DataContextType.TransportBooking));
				if (bookings.Any())
				{
					result = bookings.ToArray();
				}
			}

			return result.Any() ? result : new[] { dataObject };
		}

		ZBool HasTargetContext(IDataContextDataObject dataContext, DataContextType contextType)
		{
			var dataTargets = dataContext != null ? dataContext.DataTargetCollection : null;
			return dataTargets != null && dataTargets.Any(ds => ds.Type.GetValueOrDefault() == contextType.ToString());
		}

		class DtbBookingDataObjectCollectionReader : DataObjectCollectionReader<UniversalShipment, DtbBooking>
		{
			public DtbBookingDataObjectCollectionReader(DtbBookingConsolidationDataObjectReader reader, DtbBookingConsolidation consolidation, PkgPackageJobDataObjectReader packageJobReader, UniversalShipment[] bookings, UniversalShipment topLevelDO)
					: base(bookings)
			{
				Reader = reader;
				Consolidation = consolidation;
				PackageJobReader = packageJobReader;
				TopLevelDO = topLevelDO;
			}

			readonly DtbBookingConsolidationDataObjectReader Reader;
			readonly DtbBookingConsolidation Consolidation;
			readonly PkgPackageJobDataObjectReader PackageJobReader;
			readonly UniversalShipment TopLevelDO;

			protected override void AddToCollection(DtbBooking booking)
			{
				using (booking.SuspendSettingServiceLevel())
				{
					Consolidation.Bookings.Add(booking);
				}
			}

			protected override DtbBooking[] BusinessObjects
			{
				get { return Consolidation.Bookings.ToArray(); }
			}

			protected override DtbBooking FindMatchingBusinessObject(UniversalShipment dataObject)
			{
				return null;
			}

			protected override DtbBooking ReadIntoBusinessObject(UniversalShipment dataObject, DtbBooking businessObject)
			{
				return new DtbBookingDataObjectReaderFromDtbBooking(dataObject, Reader.logger, Reader.factory, Consolidation, TopLevelDO, packageJobReader: PackageJobReader).ReadIntoBusinessObject();
			}

			protected override void RemoveFromCollection(DtbBooking booking)
			{
				Consolidation.Bookings.Delete(booking);
			}
		}

		bool HasCartageAgentRecipientRole
		{
			get
			{
				return logger.TopLevelDataContext != null
						&& logger.TopLevelDataContext.RecipientRoleCollection != null
						&& logger.TopLevelDataContext.RecipientRoleCollection.Any(r => r.Code == RecipientRoleType.CTG);
			}
		}

		void PopulateSendingParty(DtbBookingConsolidation consolidation, UniversalShipment topLevelDO)
		{
			var dataContext = topLevelDO.DataContext;
			var sendingParty = dataContext != null ? dataContext.GetSourceOrganisation(logger, factory.BOFactory) : null;
			if (sendingParty != null && sendingParty.MainAddress != null)
			{
				consolidation.BookedByAddress.E2_OA_Address = sendingParty.MainAddress.PK;
			}
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(DtbBookingConsolidation targetBO)
		{
			var result = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);

			if (result.IsEmpty && targetBO != null)
			{
				// if no active bookings, change overridden state so we can successfully 
				var matchingDO = GetDataObject(base.dataObject);

				var isOverridden = targetBO.KB_IsOverridden;
				if (isOverridden)
				{
					var dataContext = dataObject.DataContext;
					var sendingParty = dataContext?.GetSourceOrganisation(logger, factory.BOFactory);

					if (sendingParty != null
						&& targetBO.Bookings != null
						&& targetBO.Bookings.Any(bkg => sendingParty.PK == bkg?.Address?.OrganisationPK || sendingParty.PK == bkg?.CarrierBookingAgent?.PK))
					{
						isOverridden = false;
					}
				}

				if (isOverridden && targetBO.ActiveBookings.Count > 0 &&
								(matchingDO.JobCosting == null
								|| matchingDO.JobCosting.ChargeLineCollection == null
								|| matchingDO.JobCosting.ChargeLineCollection.Count == 0))
				{
					result = Res.GetString("62c828b0-31bb-47b3-92af-50d4a8e50129", "The Transport Booking Consolidation cannot be updated as it has been overridden.");
				}
				else if (AddFetchHintsForLogs(targetBO.Bookings) && targetBO.ActiveBookings.Any(b => b.HasServiceCommencedLogs))
				{
					result = Res.GetString("cbbeacec-c7d9-4c70-a179-320acd9daa30", "Bookings on this Consolidation have already been commenced, cannot update the Consolidation.");
				}
			}

			return result;
		}

		bool AddFetchHintsForLogs(IEnumerable<DtbBooking> bookings)
		{
			bookings.ToList().ForEach(b => factory.BOFactory.AddFetchHint(StmALogSchema.SL_Parent, b.PK));
			return true;
		}

		UniversalShipment GetDataObject(UniversalShipment topLevelDO)
		{
			return GetTargetDataObject(topLevelDO) ?? UniversalShipment.GetSourceDataObject(topLevelDO);
		}

		static UniversalShipment GetTargetDataObject(UniversalShipment topLevelDataObject)
		{
			UniversalShipment result = null;
			var dataTarget = topLevelDataObject.DataContext?.DataTargetCollection?.LastOrDefault();
			if (dataTarget != null)
			{
				result = FindRelatedTargetShipment(topLevelDataObject.SubShipmentCollection, dataTarget);
			}

			return result;
		}

		static UniversalShipment FindRelatedTargetShipment(IEnumerable<UniversalShipment> subShipments, IDataTargetDataObject dataTarget)
		{
			UniversalShipment result = null;

			if (subShipments != null)
			{
				foreach (var subShipment in subShipments)
				{
					if (subShipment.DataContext?.DataTargetCollection != null)
					{
						if (subShipment.DataContext.DataTargetCollection.Any(o => o.Type == dataTarget.Type && o.Key == dataTarget.Key))
						{
							result = subShipment;
						}
						else
						{
							result = FindRelatedTargetShipment(subShipment.SubShipmentCollection, dataTarget);
						}

						if (result != null)
						{
							break;
						}
					}
				}
			}

			return result;
		}

		protected override bool IsImportConsolCostsAllowed(DtbBookingConsolidation targetBO)
		{
			return true;
		}
	}
}
