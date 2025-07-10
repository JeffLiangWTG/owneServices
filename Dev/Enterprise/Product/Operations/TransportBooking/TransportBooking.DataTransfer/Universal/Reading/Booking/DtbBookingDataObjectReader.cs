using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.DataTransfer.Universal;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using ICusEntryNumAdditionalReferenceCollection = Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	abstract class DtbBookingDataObjectReader : ShipmentDataObjectReader<DtbBooking>
	{
		protected DtbBookingDataObjectReader(UniversalShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory, DtbBookingConsolidation consolidation, UniversalShipment topLevelDO, UniversalShipment sourceDO = null)
			: base(shipment, logger, factory)
		{
			TopLevelDO = topLevelDO;
			SourceDO = sourceDO;
			this.consolidation = Argument.NotNull(consolidation, "DtbBookingConsolidation consolidation");
		}

		protected readonly UniversalShipment TopLevelDO;
		protected readonly UniversalShipment SourceDO;

		protected DtbBookingConsolidation Consolidation
		{
			get { return consolidation; }
		}

		readonly DtbBookingConsolidation consolidation;

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransportBooking; }
		}

		protected override IMatchingBusinessEntityFinder<DtbBooking> GetCombinedReferenceMatcher()
		{
			var dataContext = TopLevelDO != null ? TopLevelDO.DataContext : null;
			var sendingParty = dataContext != null ? dataContext.GetSourceOrganisation(logger, factory.BOFactory) : null;

			var bookingReferences = new TransportBookingReferences();
			if (sendingParty != null)
			{
				bookingReferences.SendingPartyOrganizationPK = sendingParty.PK;
				bookingReferences.SendingPartyOrganizationCode = sendingParty.OH_Code;
			}
			bookingReferences.SendingPartyExternalTransportBookingNumber = GetExternalTransportBookingNumberIfSenderIsTestCba();

			if (bookingReferences.SendingPartyExternalTransportBookingNumber.IsEmpty)
			{
				bookingReferences.SendingPartyTransportBookingNumber = GetSendingPartyBookingPartyReferenceNumber().GetValueOrDefault();
			}

			return new TransportBookingMatcher(factory.BOFactory, bookingReferences, logger);
		}

		class TransportBookingReferences : IReferencesParent
		{
			public ZGuid SendingPartyOrganizationPK { get; set; }
			public ZString SendingPartyOrganizationCode { get; set; }
			public ZString SendingPartyTransportBookingNumber { get; set; }
			public ZString SendingPartyExternalTransportBookingNumber { get; set; }
		}

		class TransportBookingMatcher : CombinationKeyMatcher<DtbBooking, TransportBookingReferences>
		{
			public TransportBookingMatcher(BusinessObjectFactory factory, TransportBookingReferences references, IXmlImportLogger logger)
				: base(factory, references, logger)
			{ }

			protected override bool CheckLatestParent(DtbBooking parent, DtbBooking parentToCompare)
			{
				return parent.Logs.AddedLog.SL_EventTime > parentToCompare.Logs.AddedLog.SL_EventTime;
			}

			protected override void BuildMatchingQueryAndMatchDelegates(TransportBookingReferences referencesParent)
			{
				if (!referencesParent.SendingPartyOrganizationPK.IsEmpty)
				{
					if (!referencesParent.SendingPartyExternalTransportBookingNumber.IsEmpty)
					{
						logger.Log(Integration.LogType.Information, FormattableString.Invariant($"Searching for Booking, attempting to match Transport Booking Job ID to External Transport Booking number {referencesParent.SendingPartyExternalTransportBookingNumber} from sending system"));

						var bookingQuery = new ZDBOnlyQuery(typeof(DtbBooking));
						bookingQuery.AddToFilter(DtbBookingSchema.KM_JobID, referencesParent.SendingPartyExternalTransportBookingNumber);

						var matchDelegates = new List<MatchDelegate>
						{
							b => GetMatchCount(b.TransportBookingPartyReference, referencesParent.SendingPartyExternalTransportBookingNumber)
						};
						AddPossibleMatch(bookingQuery, booking => { return matchDelegates.TrueForAll(m => m(booking) == 1) ? 1 : 0; });
					}

					if (!referencesParent.SendingPartyTransportBookingNumber.IsEmpty)
					{
						logger.Log(Integration.LogType.Information, FormattableString.Invariant($"Searching for Booking, attempting to match External Transport Booking numbers to Transport Booking number {referencesParent.SendingPartyTransportBookingNumber} from sending system and match Address to Sending Party {referencesParent.SendingPartyOrganizationCode}"));

						var bookingQuery = new ZDBOnlyQuery(typeof(DtbBooking));
						bookingQuery.AddSubQuery(GetSendingPartyTransportBookingNumberSubQuery(referencesParent.SendingPartyTransportBookingNumber), JoinCondition.And);
						bookingQuery.AddSubQuery(GetSendingPartySubQuery(referencesParent.SendingPartyOrganizationPK), JoinCondition.And);

						var matchDelegates = new List<MatchDelegate>
						{
							b => GetMatchCount(b.ConsolidationSingleJob != null ? b.ConsolidationSingleJob.BookedByOrganisationPK : ZGuid.Empty, referencesParent.SendingPartyOrganizationPK),
							b => GetMatchCount(b.TransportBookingPartyReference, referencesParent.SendingPartyTransportBookingNumber)
						};

						AddPossibleMatch(bookingQuery, booking => { return matchDelegates.TrueForAll(m => m(booking) == 1) ? 1 : 0; });
					}
				}
			}

			ZDBOnlySubQuery GetSendingPartySubQuery(ZGuid sendingPartyOrganisationPK)
			{
				var jobDocAddressQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(sendingPartyOrganisationPK);

				var consolidationQuery = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);
				consolidationQuery.AddSubQuery(jobDocAddressQuery, JoinCondition.And);
				return consolidationQuery;
			}

			ZDBOnlySubQuery GetSendingPartyTransportBookingNumberSubQuery(ZString sendingPartyTransportBookingNumber)
			{
				var additionalReferencesQuery = new ZDBOnlySubQuery(typeof(ICusEntryNumber), CusEntryNumSchema.CE_ParentID);
				additionalReferencesQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, Enterprise.TransportCommon.Shared.TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber);
				additionalReferencesQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, sendingPartyTransportBookingNumber);
				return additionalReferencesQuery;
			}

			protected override void BuildFallbackMatchDelegates(TransportBookingReferences referencesParent)
			{
			}
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(DtbBooking targetBO)
		{
			var result = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
			if (result.IsEmpty && targetBO != null && targetBO.HasServiceCommencedLogs)
			{
				result = Res.GetString("63b42c1f-d72a-430f-b8fe-6a695e369d10", "{0} has already been commenced and could not be overridden.", targetBO.HumanReadableName);
			}
			return result;
		}

		protected sealed override void PopulateBusinessObject(DtbBooking booking)
		{
			((ISupportDataImporting)booking).IsImportingData = true; // do not set back to false ... for the life of the object in this factory it should be importing including SAVE

			BeforePopulateBusinessObject(booking);
			PopulateTemplate(booking);
			AddAdditionalContainerYardInstructions(booking);
			SetParentContainerLinksAndAddresses(booking);
			PopulateRelatedEntities(booking);
			PopulateOtherDetails(booking);
		}

		(List<Container> matchingContainersOnCYDInstructions, List<Container> nonMatchingContainersOnCYDInstructions) ContainersOnCYDInstructions { get; set; }

		void SetParentContainerLinksAndAddresses(DtbBooking booking)
		{
			if (SourceDO != null && SourceDO.ContainerCollection != null)
			{
				var instructionTypeForDirection = GetInstructionTypeForBookingDirection(booking);

				if (instructionTypeForDirection != (ZString?)null)
				{
					var existingContainerYardInstructionsForDirection = booking.Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CYD && i.KN_InstructionType == (ZString)instructionTypeForDirection);

					SetParentContainerLinksAndAddresses(ContainersOnCYDInstructions.matchingContainersOnCYDInstructions, booking);
					SetParentContainerLinksAndAddresses(ContainersOnCYDInstructions.nonMatchingContainersOnCYDInstructions, booking);
				}
			}
		}

		void SetParentContainerLinksAndAddresses(List<Container> containers, DtbBooking booking)
		{
			if (containers != null)
			{
				var containerYardAddressTypeForDirection = GetContainerYardAddressTypeForBookingDirection(booking);

				foreach (var container in containers)
				{
					if (container != null)
					{
						var addressCodesAndTypes = new List<(ZString? addressCode, ZString? addressType)>();
						var shouldFallbackToShipmentContainerYard = false;
						if (container.OrganizationAddressCollection != null && container.OrganizationAddressCollection.Any(a => a != null) && container.OrganizationAddressCollection.Select(a => a?.AddressType).Contains(containerYardAddressTypeForDirection))
						{
							foreach (var address in container.OrganizationAddressCollection)
							{
								if (address != null)
								{
									addressCodesAndTypes.Add((addressCode: address.AddressShortCode, addressType: address.AddressType));
								}
							}
						}
						else if (container.OrganizationAddressCollection == null || !container.OrganizationAddressCollection.Any(a => a != null && a.AddressType == containerYardAddressTypeForDirection))
						{
							if (SourceDO.OrganizationAddressCollection != null && SourceDO.OrganizationAddressCollection.Any(a => a != null && a.AddressType == (ZString?)nameof(DocAddressType.CustomsContainerYardAddress)))
							{
								shouldFallbackToShipmentContainerYard = true;
								foreach (var address in SourceDO.OrganizationAddressCollection)
								{
									if (address != null)
									{
										addressCodesAndTypes.Add((addressCode: address.AddressShortCode, addressType: address.AddressType));
									}
								}
							}
						}

						if (addressCodesAndTypes.Count == 0)
						{
							addressCodesAndTypes.Add((addressCode: null, addressType: containerYardAddressTypeForDirection));
						}

						if (booking.ParentContainerLinksAndAddresses == null)
						{
							booking.ParentContainerLinksAndAddresses = new List<(ZInt?, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>();
						}

						if (addressCodesAndTypes.Count != 0)
						{
							booking.ParentContainerLinksAndAddresses.Add((link: container.Link, addressCodesAndTypes, shouldFallbackToShipmentContainerYard));
						}
					}
				}
			}
		}

		(IEnumerable<Container> matchingContainersOnCYDInstructions, IEnumerable<Container> nonMatchingContainersOnCYDInstructions) GetContainersOnCYDInstructions(DtbBooking booking, IEnumerable<DtbBookingInstruction> existingContainerYardInstructionsForDirection)
		{
			var containerYardAddressTypeForDirection = GetContainerYardAddressTypeForBookingDirection(booking);

			var nonMatchingContainersOnCYDInstructions = new List<Container>();
			var matchingContainersOnCYDInstructions = new List<Container>();
			var containerCollection = GetContainerCollectionForDtbBooking();

			if (existingContainerYardInstructionsForDirection.Any())
			{
				foreach (var container in containerCollection)
				{
					var matchingInstructionExistsForContainer = MatchingInstructionExistsForContainer(container, existingContainerYardInstructionsForDirection, containerYardAddressTypeForDirection);

					if (matchingInstructionExistsForContainer)
					{
						matchingContainersOnCYDInstructions.Add(container);
					}
					else
					{
						nonMatchingContainersOnCYDInstructions.Add(container);
					}
				}
			}

			return (matchingContainersOnCYDInstructions, nonMatchingContainersOnCYDInstructions);
		}

		IEnumerable<Container> GetContainerCollectionForDtbBooking()
		{
			if (dataObject == null || dataObject.ContainerCollection == null || SourceDO == null || SourceDO.ContainerCollection == null)
			{
				return Enumerable.Empty<Container>();
			}

			if (dataObject.ContainerCollection.Count == SourceDO.ContainerCollection.Count)
			{
				return SourceDO.ContainerCollection;
			}
			else
			{
				return SourceDO.ContainerCollection.Where(sc => dataObject.ContainerCollection.Any(dc => dc.Link == sc.Link));
			}
		}

		bool MatchingInstructionExistsForContainer(Container container, IEnumerable<DtbBookingInstruction> existingContainerYardInstructionsForDirection, ZString? containerYardAddressTypeForDirection)
		{
			var result = false;

			var shouldAddNewInstructionForContainerAddress = false;
			var instructionAlreadyExistsForContainerAddress = false;

			var containerOrgCollectionIsNotNull = container.OrganizationAddressCollection != null;
			var containerHasAtLeastOneNonNullAddress = containerOrgCollectionIsNotNull && container.OrganizationAddressCollection.Any(a => a != null);
			var addressCodesOnExistingInstructions = existingContainerYardInstructionsForDirection?.Select(i => i.Address?.RealAddress?.AddressCode);
			var containerHasAddressOfCorrectType = containerHasAtLeastOneNonNullAddress && container.OrganizationAddressCollection.Any(a => a != null && a.AddressType == containerYardAddressTypeForDirection);

			if (containerHasAddressOfCorrectType)
			{
				var codeOfContainerYardAddressOnContainer = container.OrganizationAddressCollection.Where(a => a != null && a.AddressType == containerYardAddressTypeForDirection).FirstOrDefault()?.AddressShortCode;
				instructionAlreadyExistsForContainerAddress = addressCodesOnExistingInstructions.Contains(codeOfContainerYardAddressOnContainer);
				shouldAddNewInstructionForContainerAddress = !instructionAlreadyExistsForContainerAddress;
			}

			var shouldAddNewInstructionForShipmentAddress = false;

			var shipmentOrgAddressCollectionIsNotNull = SourceDO.OrganizationAddressCollection != null;
			var shipmentHasAtLeastOneContainerYardAddress = shipmentOrgAddressCollectionIsNotNull && SourceDO.OrganizationAddressCollection.Any(a => a != null && a.AddressType == (ZString?)nameof(DocAddressType.CustomsContainerYardAddress));

			if (!shouldAddNewInstructionForContainerAddress && !instructionAlreadyExistsForContainerAddress && shipmentHasAtLeastOneContainerYardAddress)
			{
				var shipmentContainerYardAddressCode = SourceDO.OrganizationAddressCollection.Where(a => a != null && a.AddressType == (ZString?)nameof(DocAddressType.CustomsContainerYardAddress)).FirstOrDefault()?.AddressShortCode;
				instructionAlreadyExistsForContainerAddress = addressCodesOnExistingInstructions.Contains(shipmentContainerYardAddressCode);

				var containerDoesNotHaveAddressToMatch = !containerHasAtLeastOneNonNullAddress || !container.OrganizationAddressCollection.Any(a => a != null && a.AddressType == containerYardAddressTypeForDirection);

				shouldAddNewInstructionForShipmentAddress = containerDoesNotHaveAddressToMatch && !instructionAlreadyExistsForContainerAddress;
			}

			if (!containerHasAddressOfCorrectType && !shipmentHasAtLeastOneContainerYardAddress)
			{
				instructionAlreadyExistsForContainerAddress = existingContainerYardInstructionsForDirection.Any(i => i != null && (i.Address == null || i.Address.RealAddress == null));
				shouldAddNewInstructionForContainerAddress = !instructionAlreadyExistsForContainerAddress;
			}

			if (shouldAddNewInstructionForContainerAddress || shouldAddNewInstructionForShipmentAddress)
			{
				result = false;
			}
			else if (instructionAlreadyExistsForContainerAddress)
			{
				result = true;
			}

			return result;
		}

		ZString? GetInstructionTypeForBookingDirection(DtbBooking booking)
		{
			ZString? instructionTypeForDirection = null;

			if (SourceDO != null && SourceDO.TransportBookingDirection != null && SourceDO.TransportBookingDirection.Code != (ZString?)null)
			{
				instructionTypeForDirection = SourceDO.TransportBookingDirection.Code;
			}
			else if (booking.KM_Direction == "ORG" || booking.KM_Direction == "EXP")
			{
				instructionTypeForDirection = InstructionTypes.Codes.PickUp;
			}
			else if (booking.KM_Direction == "DST" || booking.KM_Direction == "IMP")
			{
				instructionTypeForDirection = InstructionTypes.Codes.Delivery;
			}

			return instructionTypeForDirection;
		}

		ZString? GetContainerYardAddressTypeForBookingDirection(DtbBooking booking)
		{
			ZString? addressTypeForDirection = null;

			if (SourceDO != null && SourceDO.TransportBookingDirection != null && SourceDO.TransportBookingDirection.Code != (ZString?)null)
			{
				if (SourceDO.TransportBookingDirection.Code == (ZString?)"PIC")
				{
					addressTypeForDirection = AddressTypes.ContainerYardEmptyPickupAddress;
				}
				else if (SourceDO.TransportBookingDirection.Code == (ZString?)"DLV")
				{
					addressTypeForDirection = AddressTypes.ContainerYardEmptyReturnAddress;
				}
			}
			else if (booking.KM_Direction == "ORG" || booking.KM_Direction == "EXP")
			{
				addressTypeForDirection = AddressTypes.ContainerYardEmptyPickupAddress;
			}
			else if (booking.KM_Direction == "DST" || booking.KM_Direction == "IMP")
			{
				addressTypeForDirection = AddressTypes.ContainerYardEmptyReturnAddress;
			}

			return addressTypeForDirection;
		}

		void AddAdditionalContainerYardInstructions(DtbBooking booking)
		{
			var instructionTypeForDirection = GetInstructionTypeForBookingDirection(booking);

			if (instructionTypeForDirection != (ZString?)null)
			{
				var existingContainerYardInstructionsForDirection = booking.Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CYD && i.KN_InstructionType == (ZString)instructionTypeForDirection);
				var containersOnCYDInstructions = GetContainersOnCYDInstructions(booking, existingContainerYardInstructionsForDirection);

				if (containersOnCYDInstructions.nonMatchingContainersOnCYDInstructions != null && containersOnCYDInstructions.matchingContainersOnCYDInstructions != null)
				{
					ContainersOnCYDInstructions = (matchingContainersOnCYDInstructions: containersOnCYDInstructions.matchingContainersOnCYDInstructions.ToList(), nonMatchingContainersOnCYDInstructions: containersOnCYDInstructions.nonMatchingContainersOnCYDInstructions.ToList());

					var firstExistingCYDInstructionForDirection = existingContainerYardInstructionsForDirection.FirstOrDefault(i => i.KN_InstructionType == (ZString)instructionTypeForDirection);
					if (firstExistingCYDInstructionForDirection != null)
					{
						var newInstructionSequence = firstExistingCYDInstructionForDirection.KN_Sequence + 1;

						var containerLinksWithMatchingInstruction = new List<ZInt?>();
						containerLinksWithMatchingInstruction.AddRange(ContainersOnCYDInstructions.matchingContainersOnCYDInstructions.Select(c => c.Link));

						foreach (var container in ContainersOnCYDInstructions.nonMatchingContainersOnCYDInstructions)
						{
							var matchingInstructionExistsForContainer = MatchingInstructionExistsForContainer(container, existingContainerYardInstructionsForDirection, GetContainerYardAddressTypeForBookingDirection(booking));

							if (!matchingInstructionExistsForContainer)
							{
								OrganizationAddress matchingOrgAddressOnContainerOrShipment = null;
								if (container.OrganizationAddressCollection != null && container.OrganizationAddressCollection.Any(a => a != null) && container.OrganizationAddressCollection.Select(a => a?.AddressType).Contains(GetContainerYardAddressTypeForBookingDirection(booking)))
								{
									container.OrganizationAddressCollection.RemoveAll(orgaddress => orgaddress == null);
									matchingOrgAddressOnContainerOrShipment = container.OrganizationAddressCollection.Where(a => a.AddressType == GetContainerYardAddressTypeForBookingDirection(booking))?.First();
								}
								else if (SourceDO.OrganizationAddressCollection != null && SourceDO.OrganizationAddressCollection.Any(a => a != null) && SourceDO.OrganizationAddressCollection.Select(a => a?.AddressType).Contains((ZString?)nameof(DocAddressType.CustomsContainerYardAddress)))
								{
									SourceDO.OrganizationAddressCollection.RemoveAll(orgaddress => orgaddress == null);
									matchingOrgAddressOnContainerOrShipment = SourceDO.OrganizationAddressCollection.Where(a => a.AddressType == (ZString?)nameof(DocAddressType.CustomsContainerYardAddress))?.First();
								}

								if (matchingOrgAddressOnContainerOrShipment == null || !existingContainerYardInstructionsForDirection.Select(i => i.Address?.RealAddress?.AddressCode).Contains(matchingOrgAddressOnContainerOrShipment.AddressShortCode))
								{
									if (booking.Instructions.Any(i => i.KN_Sequence == newInstructionSequence))
									{
										foreach (var instruction in booking.Instructions.Where(i => i.KN_Sequence >= newInstructionSequence))
										{
											instruction.KN_Sequence++;
										}
									}

									var newInstruction = booking.Instructions.AddNew();
									newInstruction.KN_Sequence = newInstructionSequence;
									newInstruction.KN_InstructionType = firstExistingCYDInstructionForDirection.KN_InstructionType;
									newInstruction.PackageCategory = firstExistingCYDInstructionForDirection.PackageCategory;
									newInstruction.KN_IsContainerRateable = firstExistingCYDInstructionForDirection.KN_IsContainerRateable;
									newInstruction.KN_DropMode = firstExistingCYDInstructionForDirection.KN_DropMode;
									newInstruction.OrganisationType = firstExistingCYDInstructionForDirection.OrganisationType;

									if (matchingOrgAddressOnContainerOrShipment != null)
									{
										var addressCode = matchingOrgAddressOnContainerOrShipment.AddressShortCode;
										var addressOrgCode = (ZString)matchingOrgAddressOnContainerOrShipment.OrganizationCode;

										var orgAddressQuery = new ZDBOnlyQuery(typeof(OrgAddress));
										orgAddressQuery.AddToFilter(OrgAddressSchema.OA_Code, addressCode);

										var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
										orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_Code, addressOrgCode);
										orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgHeaderSubQuery, JoinCondition.And);

										var boFactory = new ReadOnlyBusinessObjectFactory();
										var newInstructionOrgAddress = boFactory.Load<OrgAddress>(orgAddressQuery)[0];

										newInstruction.Address.E2_OA_Address = newInstructionOrgAddress.PK;
									}
									else
									{
										newInstruction.Address.E2_OA_Address = ZGuid.Empty;
									}

									containerLinksWithMatchingInstruction.Add(container.Link);
								}
							}
						}
					}
				}
			}
		}

		protected virtual void BeforePopulateBusinessObject(DtbBooking booking)
		{
		}

		protected void PopulateAdditionalReferences(DtbBooking booking)
		{
			var dataObjectWithReferences = SourceDO ?? dataObject;
			var additionalReferenceCollection = GetAdditionalReferences(dataObjectWithReferences);
			RemoveUnnecessaryAdditionalReferences(ref additionalReferenceCollection, booking);
			if (additionalReferenceCollection != null)
			{
				var additionalReferenceCollectionReader = new TransportAdditionalReferenceCollectionReader<DtbBooking>(additionalReferenceCollection, logger, factory, booking);
				additionalReferenceCollectionReader.ReadIntoCollection();
			}

			PopulateWayBills(GetAdditionalReferenceCollectionForWayBills(booking));
		}

		DataObjectList<AdditionalReference> GetAdditionalReferences(UniversalShipment dataObject)
		{
			return dataObject.AdditionalReferenceCollection;
		}

		void RemoveUnnecessaryAdditionalReferences(ref DataObjectList<AdditionalReference> additionalReferenceCollection, DtbBooking booking)
		{
			if (booking != null && booking.ConsolidationSingleJob != null && additionalReferenceCollection != null)
			{
				foreach (var reference in additionalReferenceCollection.ToArray())
				{
					foreach (ICusEntryNumber consolReference in booking.ConsolidationSingleJob.AdditionalReferenceNumbers)
					{
						if (consolReference?.CE_EntryNum == reference?.ReferenceNumber && consolReference?.CE_EntryType == reference?.Type.Code)
						{
							additionalReferenceCollection.Remove(reference);
						}
					}
				}
			}
		}

		ICusEntryNumAdditionalReferenceCollection GetAdditionalReferenceCollectionForWayBills(DtbBooking booking)
		{
			return booking.ConsolidationSingleJob != null ? booking.ConsolidationSingleJob.AdditionalReferenceNumbers : null;
		}

		void PopulateTemplate(DtbBooking booking)
		{
			ZString? templateCode = dataObject.LocalTransportJobType.GetNullableCodeAsUpperCase();

			using (booking.SuspendSettingPackages())
			{
				SetTemplateCode(booking, templateCode);
			}
		}

		protected virtual void SetTemplateCode(DtbBooking booking, ZString? templateCode)
		{
			SetValue(booking, DtbBookingSchema.KM_KT_NKBookingTemplate, templateCode);
		}

		void PopulateRelatedEntities(DtbBooking booking)
		{
			PopulateAdditionalReferences(booking);
			PopulateNotes(booking);
			PopulateAddresses(booking);
			PopulateRelatedEntitiesCore(booking);
			booking.Instructions.Where(i => i.IsEmptyYard).SelectMany(i => i.Confirmations).ForEach(c => c.SetIsEmptyContainer());
			AssignAuthorisedToLeave(booking);
		}

		void PopulateAddresses(DtbBooking booking)
		{
			var organizationAddressCollection = new List<OrganizationAddress>();
			if (dataObject.OrganizationAddressCollection != null)
			{
				organizationAddressCollection.AddRange(dataObject.OrganizationAddressCollection);
			}

			if (((IDtbBookingParent)booking.ConsolidationSingleJob?.ParentBO)?.JobType is ZString jobType &&
				(jobType == JobInvoicingConsumerTypes.Shipment.Code || jobType == JobInvoicingConsumerTypes.Consol.Code))
			{
				var shippingLineAddress = TopLevelDO.OrganizationAddressCollection?.Find(a => a.AddressType is ZString addressType && addressType == nameof(DocAddressType.ShippingLineAddress));
				if (shippingLineAddress != null)
				{
					organizationAddressCollection.Add(shippingLineAddress);
				}
			}

			OldTransportCompanyOrgAddressValue = booking.Address.E2_OA_Address;

			foreach (var orgAddressDataObject in organizationAddressCollection)
			{
				var addressType = orgAddressDataObject.AddressType.GetValueOrDefault();
				if (addressType != nameof(DocAddressType.CarrierBookingAgent) || consolidation.KB_ParentID.IsEmpty)
				{
					var jobDocAddress = addressType == nameof(DocAddressType.TransportCompanyDocumentaryAddress)
						? new OrganisationDataObjectReader(orgAddressDataObject, logger, factory).GetMatchedOrNew(booking, OrganisationTypes.Carrier)
						: new OrganisationDataObjectReader(orgAddressDataObject, logger, factory).GetMatchedOrNew(booking, null, true);

					if (jobDocAddress != null)
					{
						booking.DocAddresses.Add(jobDocAddress);

						(var interchangeType, var sender) = DtbDataObjectReaderHelper.GetInterchangeTypeAndSenderFromLogger(logger);
						var isCbaBilled = ((TopLevelDataObject)logger.TopLevelDataObject).HasRecipientRoleAndService(RecipientRoleType.TPC, ServiceCodeType.CBA);
						var isSenderAuthorised = interchangeType == EDIInterchangeTransportTypeList.Codes.eHub && DtbAgentBooking.IsAuthorisedCarrierBookingAgent(sender);

						if ((isCbaBilled && isSenderAuthorised || DtbDataObjectReaderHelper.MessageIsFromTestCba(sender, logger.TopLevelDataObject as TopLevelDataObject))
							&& jobDocAddress.DocAddressType == DocAddressType.TransportCompanyDocumentaryAddress
							&& OldTransportCompanyOrgAddressValue == ZGuid.Empty
							&& orgAddressDataObject != null)
						{
							booking.GetLogs().AddNew(Events.BookingConfirmed, ZString.Format(@"FAC={0}|NAM={1}", "Transport Company", orgAddressDataObject.CompanyName));
						}
					}
				}
			}
		}

		ZGuid OldTransportCompanyOrgAddressValue { get; set; }

		void AssignAuthorisedToLeave(DtbBooking booking)
		{
			if (dataObject.IsAuthorizedToLeave.HasValue)
			{
				foreach (var bookingInstruction in booking.Instructions.Where(instruction => instruction.IsAuthorisedToLeaveAvailable))
				{
					bookingInstruction.KN_IsAuthorisedToLeave = dataObject.IsAuthorizedToLeave.Value;
				}
			}
		}

		protected abstract void PopulateRelatedEntitiesCore(DtbBooking booking);

		void PopulateOtherDetails(DtbBooking booking)
		{
			SetValue(booking, DtbBookingSchema.KM_RS_NKServiceLevel, dataObject.ServiceLevel);
			SetValue(booking, DtbBookingSchema.KM_IsHazardous, dataObject.IsHazardous);

			if (dataObject.GetMatchingDataSource(DataContextType.TransportBooking) != null) // direction is used as Consolidation Direction and Booking Direction, normally we set direction via the Template, the only time we would set it here is TB to TB
			{
				SetValue(booking, DtbBookingSchema.KM_Direction, dataObject.TransportBookingDirection);
			}

			SetCarrierServiceLevel(booking);
			SetValue(booking, DtbBookingSchema.KM_RatingFreightMode, dataObject.RatingTransportMode);
			if (dataObject.BookingTransportMode != null && dataObject.BookingTransportMode.Code.HasValue)
			{
				SetValue(booking, DtbBookingSchema.KM_TransportMode, dataObject.BookingTransportMode.Code);
			}
			SetValue(booking, DtbBookingSchema.KM_RequiresRefrigeration, dataObject.RequiresRefrigeration);

			IDtbParentInfo parentInfo = new DtbParentInfo(Consolidation, new HasConsolidationParentDataObjectStrategy(dataObject));
			SetValue(booking, DtbBookingSchema.KM_TransportReference, parentInfo.TransportReference); // fallback to Schedule for Routing
			parentInfo.UpdateAddress(booking.Address, true, null, null);

			PopulateBranch(booking);

			PopulateWorkflowCustomFields(booking, dataObject);

			var bookingPartyReference = GetSendingPartyBookingPartyReferenceNumber().GetValueOrDefault();
			if (!bookingPartyReference.IsEmpty)
			{
				CreateOrUpdateReference(Enterprise.TransportCommon.Shared.TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber, bookingPartyReference, booking.AdditionalReferenceNumbers);
			}

			var carrierAccount = GetCarrierAccount(dataObject.CarrierAccount, booking);
			SetValue(booking, DtbBookingSchema.KM_OAN_CarrierAccount, carrierAccount == null ? ZGuid.Empty : carrierAccount.PK);

			var dataSourceIsTransportBooking = dataObject.DataContext != null && dataObject.DataContext.DataSourceCollection != null && dataObject.DataContext.DataSourceCollection.Select(g => g.Type).Contains(nameof(DataContextType.TransportBooking));
			SetValue(booking, DtbBookingSchema.KM_Chargeable, dataObject.ActualChargeable.HasValue && dataSourceIsTransportBooking ? Core.Constants.Weight.ConvertSafe(dataObject.ActualChargeable.Value, dataObject.TotalWeightUnit.Code, booking.ChargeableUnits) : 0);
			SetValue(booking, DtbBookingSchema.KM_OverrideChargeable, dataObject.ActualChargeable.HasValue && dataSourceIsTransportBooking);

			PopulateBookingOfTransportRequestedDate(booking);
		}

		void PopulateBookingOfTransportRequestedDate(DtbBooking booking)
		{
			var shipment = SourceDO ?? dataObject;
			var requestedDate = shipment?.DateCollection?.FirstOrDefault(date =>
				date.Type == DateType.TransportBookingRequested
				&& !date.IsEstimate.GetValueOrDefault());
			if (requestedDate != null)
			{
				SetValue(booking, DtbBookingSchema.KM_BookingOfTransportRequestedDate, requestedDate.Value);
			}
		}

		void PopulateBranch(DtbBooking booking)
		{
			if (IsNewBO || booking.KM_GB_Branch.IsEmpty)
			{
				var branch = GetDefaultBranch(booking);
				if (branch != null)
				{
					SetValue(booking, DtbBookingSchema.KM_GB_Branch, branch.PK);
				}
			}
		}

		void PopulateNotes(DtbBooking booking)
		{
			if (dataObject.NoteCollection != null)
			{
				var supportedNoteTypes = booking.NoteTypes.Cast<PredefinedNoteType>();
				var notesToImport = new DataObjectList<Note>();
				foreach (var note in dataObject.NoteCollection)
				{
					var isNoteReadonly = PredefinedNoteTypes.Instance.NoteTypeByDescription(note.Description)?.IsReadOnlyAfterAdd ?? false;
					if (!isNoteReadonly || supportedNoteTypes.Any(t => t.Description.Equals(note.Description)))
					{
						notesToImport.Add(note);
					}
				}

				new NotesCollectionReader(notesToImport, logger, factory, booking).ReadIntoCollection();
			}
		}

		/// <summary>
		/// Shipment	(int) -> TB		Default to Logged In Branch		Occurs when the User clicks Documents > Cartage Advice or Actions > Create Transport Booking.
		/// TB			(ext) -> TB		Carrier Branch					Occurs via Service Task. Use the carriers org proxy branch, fallback to controlling branch, fallback to Logged in Branch.
		/// </summary>
		GlbBranch GetDefaultBranch(DtbBooking booking)
		{
			var result = GlbBranch.CurrentBranch;

			var isFromExternalSystem = !logger.IsInternalImport(); // the service task is likely to be importing this Job if external import, therefore we cannot use the service task logged in branch.
			if (isFromExternalSystem)
			{
				var carrier = booking.Address?.Organisation;
				if (carrier != null)
				{
					var carriersOrgProxyBranch = factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, carrier.PK));
					if (carriersOrgProxyBranch != null)
					{
						result = carriersOrgProxyBranch;
					}
					else
					{
						var carriersControllingBranch = carrier.CompanyData?.ControllingBranch;
						if (carriersControllingBranch != null)
						{
							result = carriersControllingBranch;
						}
					}
				}
			}

			return result;
		}

		OrgCarrierAccount GetCarrierAccount(CarrierAccount account, DtbBooking booking)
		{
			OrgCarrierAccount result = null;
			var carrier = booking.Address?.Organisation;
			if (carrier != null && account != null && !string.IsNullOrEmpty(account.AccountNumber))
			{
				var query = new ZDBOnlyQuery(typeof(OrgCarrierAccount));
				query.AddToFilter(OrgCarrierAccountSchema.OAN_OH_Carrier, carrier.PK);
				query.AddToFilter(OrgCarrierAccountSchema.OAN_AccountNumber, account.AccountNumber);

				result = factory.LoadTop1<OrgCarrierAccount>(query);
			}
			return result;
		}

		void CreateOrUpdateReference(string referenceType, ZString number, ICusEntryNumAdditionalReferenceCollection additionalReferenceCollection)
		{
			var referenceNumber = additionalReferenceCollection.Cast<ICusEntryNumber>().FirstOrDefault(r => r.CE_EntryType == referenceType);

			if (referenceNumber == null)
			{
				referenceNumber = additionalReferenceCollection.AddNew();
				referenceNumber.CE_EntryType = referenceType;
			}

			referenceNumber.CE_EntryNum = number;
		}

		void SetCarrierServiceLevel(DtbBooking booking)
		{
			var carrierServiceLevel = dataObject.CarrierServiceLevel;
			if (carrierServiceLevel.GetCodeAsUpperCase().IsEmpty)
			{
				carrierServiceLevel = DtbDataObjectExtractor.GetCarrierServiceLevelFromRouting(dataObject, booking.IsPickupDirection); // agency
			}

			SetValue(booking, DtbBookingSchema.KM_PL_NKCarrierServiceLevel, carrierServiceLevel);
		}

		ZString? GetSendingPartyBookingPartyReferenceNumber()
		{
			ZString? result = null;

			var dataSourceForDataObject = dataObject.GetMatchingDataSource(DataContextType.TransportBooking);
			if (dataSourceForDataObject != null)
			{
				result = dataSourceForDataObject.Key;
			}

			return result;
		}

		ZString GetExternalTransportBookingNumberIfSenderIsTestCba()
		{
			(_, var sender) = DtbDataObjectReaderHelper.GetInterchangeTypeAndSenderFromLogger(logger);
			if (DtbDataObjectReaderHelper.MessageIsFromTestCba(sender, TopLevelDO) && dataObject.AdditionalReferenceCollection != null)
			{
				return dataObject
					.AdditionalReferenceCollection
					.Where(r => r.Type.Code.HasValue && r.Type.Code.Value == (ZString)TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber && r.ReferenceNumber.HasValue && !r.ReferenceNumber.Value.IsEmpty)
					.Select(r => r.ReferenceNumber.Value)
					.FirstOrDefault();
			}
			else
			{
				return ZString.Empty;
			}
		}

		internal static IEnumerable<ZString> GetContainerIDs(UniversalShipment bookingDO)
		{
			var containers = bookingDO.ContainerCollection;
			if (containers != null)
			{
				return containers
					.Where(c => c.ContainerNumber.HasValue && !c.ContainerNumber.Value.IsEmpty)
					.Select(c => c.ContainerNumber.Value)
					.ToArray();
			}
			else
			{
				return bookingDO.PackingLineCollection.GetDistinctContainerNumbers();
			}
		}

		protected override bool LogChildTopLevelObjectsOnImport
		{
			get { return true; }
		}

		void PopulateWayBills(ICusEntryNumAdditionalReferenceCollection additionalReferenceNumbers)
		{
			if (additionalReferenceNumbers != null)
			{
				var waybills = new List<(string WayBillType, ZString BillNumber)>();
				UpdateWayBillsFromShipment(waybills, dataObject);

				if (SourceDO != null)
				{
					UpdateWayBillsFromShipment(waybills, SourceDO);
				}

				UpdateWayBillsFromShipment(waybills, TopLevelDO);

				if (DtbDataObjectReaderHelper.HasSourceContext(dataObject.DataContext, DataContextType.ForwardingShipment)
					&& dataObject.ParentShipmentCollection?.FirstOrDefault() is UniversalShipment firstParentShipment
					&& DtbDataObjectReaderHelper.HasSourceContext(firstParentShipment.DataContext, DataContextType.ForwardingConsol))
				{
					UpdateWayBillsFromShipment(waybills, dataObject.ParentShipmentCollection?.FirstOrDefault());
				}

				if (waybills.Count > 0)
				{
					RemoveWaybillReferences(additionalReferenceNumbers);

					foreach (var waybill in waybills)
					{
						PopulateWayBill(waybill.WayBillType, waybill.BillNumber, additionalReferenceNumbers);
					}
				}
			}
		}

		void UpdateWayBillsFromShipment(List<(string, ZString)> currentWayBills, UniversalShipment shipment)
		{
			if (shipment.WayBillType != null && shipment.WayBillNumber.HasValue)
			{
				AddWayBillIfAppropriate(currentWayBills, shipment.WayBillType.Code, shipment.WayBillNumber);
			}

			if (shipment.AdditionalBillCollection != null)
			{
				foreach (var additionalBill in shipment.AdditionalBillCollection)
				{
					var billNumber = additionalBill.BillNumber.GetValueOrDefault();
					if (!billNumber.IsEmpty)
					{
						var additionalRefType = GetAdditionalReferenceTypeFromWayBillType(additionalBill.BillType);
						AddWayBillIfAppropriate(currentWayBills, additionalRefType, billNumber);
					}
				}
			}
		}

		void AddWayBillIfAppropriate(List<(string WayBillType, ZString BillNumber)> result, string wayBillType, string wayBill)
		{
			var houseBillFound = result.Any(w => w.WayBillType == AdditionalReferenceTypes.Codes.HouseBill);
			if (!houseBillFound && HouseBillTypes.Contains(wayBillType))
			{
				result.Add((AdditionalReferenceTypes.Codes.HouseBill, wayBill));
			}
			else if (MasterBillTypes.Contains(wayBillType))
			{
				result.Add((AdditionalReferenceTypes.Codes.MasterBill, wayBill));
			}
		}

		IEnumerable<string> HouseBillTypes => new[] { WayBillTypeList.Codes.House, AdditionalReferenceTypes.Codes.HouseBill };
		IEnumerable<string> MasterBillTypes => new[] { WayBillTypeList.Codes.Master, AdditionalReferenceTypes.Codes.MasterBill };

		void RemoveWaybillReferences(ICusEntryNumAdditionalReferenceCollection additionalReferenceNumbers)
		{
			foreach (ICusEntryNumber reference in additionalReferenceNumbers.ToArray())
			{
				if (reference.CE_EntryType == AdditionalReferenceTypes.Codes.HouseBill || reference.CE_EntryType == AdditionalReferenceTypes.Codes.MasterBill)
				{
					additionalReferenceNumbers.RemoveAndDelete(reference);
				}
			}
		}

		void PopulateWayBill(string wayBillType, ZString? billNumber, ICusEntryNumAdditionalReferenceCollection additionalReferenceNumbers)
		{
			var value = billNumber.Value.Trim();
			if (wayBillType == AdditionalReferenceTypes.Codes.MasterBill)
			{
				ZString masterBillSeparator = "-";
				var masterBillNum = FormatMasterBillNum(value, masterBillSeparator);
				AddOrUpdate(additionalReferenceNumbers, wayBillType, masterBillNum, ignoreChar: masterBillSeparator);
			}
			else
			{
				additionalReferenceNumbers.AddNewIfNotExist(wayBillType, value);
			}
		}

		ZString FormatMasterBillNum(ZString value, ZString masterBillSeparator)
		{
			var masterBillNum = value;
			var isAir = IsAir;

			var hasSeparator = value.Contains(masterBillSeparator, StringComparison.OrdinalIgnoreCase);
			if (isAir && !hasSeparator)
			{
				masterBillNum = value.FormatAirMAWB();
			}
			if (!isAir && hasSeparator)
			{
				masterBillNum = value.Replace(masterBillSeparator, "");
			}

			return masterBillNum;
		}

		void AddOrUpdate(ICusEntryNumAdditionalReferenceCollection additionalReferenceNumbers, ZString type, ZString number, ZString ignoreChar)
		{
			if (!type.IsEmpty && !number.IsEmpty)
			{
				var numberSubstringSafe = number.SubstringSafe(0, CusEntryNumSchema.CE_EntryNum.MaxLength);
				var existingItem = additionalReferenceNumbers.Cast<ICusEntryNumber>().FirstOrDefault(n => n.CE_EntryType == type && n.CE_EntryNum.Replace(ignoreChar, "") == numberSubstringSafe.Replace(ignoreChar, ""));
				if (existingItem == null)
				{
					additionalReferenceNumbers.AddNewIfNotExist(type, numberSubstringSafe);
				}
				else if (existingItem.CE_EntryNum != numberSubstringSafe)
				{
					SetValue((IColumnIndexer)existingItem, CusEntryNumSchema.CE_EntryNum, numberSubstringSafe);
				}
			}
		}

		bool IsAir
		{
			get { return dataObject.TransportMode.GetCodeAsUpperCase() == Core.Constants.TransportModes.Air; }
		}

		string GetAdditionalReferenceTypeFromWayBillType(WayBillType type)
		{
			// Sub House, Master House or "Waybills" become HouseBills
			return (type != null && type.Code.GetValueOrDefault() == WayBillTypeList.Codes.Master)
					? TransportCommonAdditionalReferenceTypes.Codes.MasterBill : TransportCommonAdditionalReferenceTypes.Codes.HouseBill;
		}
	}
}
