using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class AgencyShipmentMatcher<T> : CombinationKeyMatcher<T, AgencyShipmentReferences> where T : AgencyShipment
	{
		public AgencyShipmentMatcher(BusinessObjectFactory factory, AgencyShipmentReferences references, IXmlImportLogger logger, IXmlEventValueObject xmlEvent = null)
			: base(factory, references, logger)
		{
			this.xmlEvent = xmlEvent;
		}

		public AgencyShipmentMatcher(BusinessObjectFactory factory, AgencyShipmentReferences references, IXmlImportLogger logger, DataContextType dataContextType)
			: base(factory, references, logger)
		{
			this.dataContextType = dataContextType;
		}

		public new AgencyShipment GetBestMatch()
		{
			reason = ZString.Empty;
			return base.GetBestMatch();
		}

		public (AgencyShipment shipment, ZString reason) GetBestMatchWithReason()
		{
			var shipment = GetBestMatch();
			if (shipment == null && reason.IsEmpty)
			{
				if (!referencesParent.OceanBillNumber.IsEmpty && !referencesParent.ShipmentID.IsEmpty)
				{
					reason = Res.GetString("50f2b877-4b7c-44c9-86ce-e52d21267784", "Master Bill Number and Carrier Booking Number are Invalid.");
				}
				else if (!referencesParent.OceanBillNumber.IsEmpty)
				{
					reason = Res.GetString("ae2a454d-5620-439b-b106-0b1207bccd40", "Master Bill Number is Invalid.");
				}
				else if (!referencesParent.ShipmentID.IsEmpty)
				{
					reason = Res.GetString("4e03b649-07b5-4177-829b-c0dbf1ae8e7c", "Carrier Booking Number is Invalid.");
				}
				else
				{
					reason = Res.GetString("e0139fbe-f88c-4db7-a7e1-c7c0c7d75d88", "No matching agency shipment.");
				}
			}

			return (shipment, reason);
		}

		protected ZString reason;

		protected override ZQuery GetFullQuery(ZQuery initialMatchingQuery, AgencyShipmentReferences references)
		{
			ZQuery query = base.GetFullQuery(initialMatchingQuery, references);
			query.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
			query.AddToFilter(JobShipmentSchema.JS_IsCancelled, false);

			return query;
		}

		protected override T[] GetBusinessObjectUsingModuleSpecificBusinessRules(AgencyShipmentReferences referencesParent)
		{
			var result = base.GetBusinessObjectUsingModuleSpecificBusinessRules(referencesParent);

			if (dataContextType == DataContextType.AgencyBooking)
			{
				var shipperRef = referencesParent.AgentsReference;
				var bookingRef = referencesParent.CarriersBookingReference;

				var filteredBOs = result.Where(x => !string.IsNullOrEmpty(x.JS_CFSReference) || !string.IsNullOrEmpty(x.JS_BookingReference)).ToArray();

				result = (!shipperRef.IsEmpty && !bookingRef.IsEmpty)
					? filteredBOs.Where(x => x.JS_CFSReference == bookingRef && (x.JS_BookingReference.IsEmpty || x.JS_BookingReference == shipperRef)).ToArray()
					: filteredBOs;
			}

			return result;
		}

		#region MatchingQueryAndMatchDelegates

		protected override void BuildMatchingQueryAndMatchDelegates(AgencyShipmentReferences referencesParent)
		{
			if (referencesParent.IsCarrierVGM && AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value)
			{
				AddPossibleMatchForCarrierVGM(referencesParent);
			}
			else
			{
				var allReferencesAreNotEmpty = !referencesParent.CarriersBookingReference.IsEmpty && !referencesParent.AgentsReference.IsEmpty;
				var atLeastOneReferenceIsNotEmpty = !referencesParent.CarriersBookingReference.IsEmpty || !referencesParent.AgentsReference.IsEmpty;

				if ((AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value
						&& ((referencesParent.PurposeCode == MessagePurposes.Codes.Original && allReferencesAreNotEmpty)
							|| (referencesParent.PurposeCode != MessagePurposes.Codes.Original && atLeastOneReferenceIsNotEmpty)))
					|| (!AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value && atLeastOneReferenceIsNotEmpty)
					|| (IncludeOceanBillNumber && !referencesParent.OceanBillNumber.IsEmpty)
				)
				{
					AddPossibleMatchForAgencyShipment(referencesParent);
				}
			}
		}

		void AddPossibleMatchForCarrierVGM(AgencyShipmentReferences referencesParent)
		{
			var matchingQuery = new ZDBOnlyQuery(typeof(AgencyShipment));

			var shipmentReferenceQuery = new ZDBOnlyQuery(typeof(AgencyShipment));

			if (IncludeOceanBillNumber && !referencesParent.OceanBillNumber.IsEmpty)
			{
				shipmentReferenceQuery.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_HouseBill, referencesParent.OceanBillNumber);
			}

			if (!referencesParent.CarriersBookingReference.IsEmpty)
			{
				shipmentReferenceQuery.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_CFSReference, referencesParent.CarriersBookingReference);
			}

			if (!referencesParent.ContainerNumber.IsEmpty && !shipmentReferenceQuery.IsEmpty)
			{
				var shipmentReferenceAndContainerQuery = new ZDBOnlyQuery(typeof(AgencyShipment));
				shipmentReferenceAndContainerQuery.AddToFilter(shipmentReferenceQuery);

				var subContainerQuery = new ZDBOnlySubQuery(typeof(AgencyShipmentContainer), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
				subContainerQuery.AddToFilter(JobContainerSchema.JC_ContainerNum, referencesParent.ContainerNumber);

				if (dataContextType == DataContextType.BillOfLading)
				{
					subContainerQuery.AddToFilter(JobContainerSchema.JC_Purpose, ContainerBookedStatus.Codes.Real);
				}

				shipmentReferenceAndContainerQuery.AddSubQuery(JobShipmentSchema.PK, subContainerQuery, JoinCondition.And);
				matchingQuery.AddToFilter(shipmentReferenceAndContainerQuery, JoinCondition.Or);
			}

			matchingQuery.AddToFilter(shipmentReferenceQuery, JoinCondition.Or);

			if (dataContextType == DataContextType.BillOfLading)
			{
				matchingQuery.AddToFilter(JobShipmentSchema.JS_ShipmentStatus, ShipmentStatusHelperMethods.GetBillOfLadingStageStatus());
			}
			else if (dataContextType == DataContextType.AgencyBooking)
			{
				matchingQuery.AddToFilter(JobShipmentSchema.JS_ShipmentStatus, ShipmentStatusHelperMethods.GetBookingStageStatus());
			}

			AddPossibleMatch(matchingQuery, CreateMatchDelegate(referencesParent));
		}

		void AddPossibleMatchForAgencyShipment(AgencyShipmentReferences referencesParent)
		{
			var bookingRef = referencesParent.CarriersBookingReference;
			var shipperRef = referencesParent.AgentsReference;
			var shipmentID = referencesParent.ShipmentID;
			var oceanBillNumber = IncludeOceanBillNumber ? referencesParent.OceanBillNumber : ZString.Empty;
			var bookingPartyPk = !shipperRef.IsEmpty ? referencesParent.BookingPartyPK : ZGuid.Empty;
			var bookingPartyName = !shipperRef.IsEmpty ? referencesParent.BookingPartyName.ToUpper() : ZString.Empty;

			var referenceInfos = new List<AgencyShipmentReferenceInfo>();

			if (IncludeOceanBillNumber && !oceanBillNumber.IsEmpty)
			{
				var oceanBillRefInfo = new AgencyShipmentReferenceInfo(
					query => query.AddToFilter(JobShipmentSchema.JS_HouseBill, oceanBillNumber),
					query => query.AddToFilter(JobShipmentSchema.JS_HouseBill, ZString.Empty));
				referenceInfos.Add(oceanBillRefInfo);
			}

			if (!bookingRef.IsEmpty)
			{
				var bookingRefInfo = new AgencyShipmentReferenceInfo(
					query => query.AddToFilter(JobShipmentSchema.JS_CFSReference, bookingRef),
					query => query.AddToFilter(JobShipmentSchema.JS_CFSReference, ZString.Empty));
				referenceInfos.Add(bookingRefInfo);
			}

			var matchingQuery = GetMatchingShipmentQuery(referenceInfos);

			if (!shipmentID.IsEmpty)
			{
				matchingQuery.AddToFilter(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, shipmentID), JoinCondition.Or);
			}

			if (!shipperRef.IsEmpty)
			{
				var shipperRefQuery = new ZQuery();
				shipperRefQuery.AddToFilter(JobShipmentSchema.JS_BookingReference, ZString.Empty);

				if (bookingRef.IsEmpty)
				{
					shipperRefQuery.AddToFilter(JobShipmentSchema.JS_CFSReference, SQLComparisonOperator.Equal, ZString.Empty);
				}

				var shipperRefSubQuery = new ZQuery();
				shipperRefSubQuery.AddToFilter(JobShipmentSchema.JS_BookingReference, shipperRef);

				var bookingPartyQuery = new ZQuery();
				if (!bookingPartyPk.IsEmpty)
				{
					var bookingPartyAddressQuery = GetBookingPartyAddressQuery(bookingPartyPk);
					bookingPartyQuery.AddToFilter(bookingPartyAddressQuery, JoinCondition.Or);
				}

				if (!bookingPartyName.IsEmpty)
				{
					var bookingPartyNameQuery = GetBookingPartyNameQuery(bookingPartyName);
					bookingPartyQuery.AddToFilter(bookingPartyNameQuery, JoinCondition.Or);
				}

				shipperRefSubQuery.AddToFilter(bookingPartyQuery, JoinCondition.And);
				shipperRefQuery.AddToFilter(shipperRefSubQuery, JoinCondition.Or);
				matchingQuery.AddToFilter(shipperRefQuery, JoinCondition.And);
			}

			if (referencesParent.IsTargettedToBothAgentModules || referencesParent.IsVGM)
			{
				if (dataContextType == DataContextType.BillOfLading)
				{
					matchingQuery.AddToFilter(JobShipmentSchema.JS_ShipmentStatus, ShipmentStatusHelperMethods.GetBillOfLadingStageStatus());
				}
				else if (dataContextType == DataContextType.AgencyBooking)
				{
					matchingQuery.AddToFilter(JobShipmentSchema.JS_ShipmentStatus, ShipmentStatusHelperMethods.GetBookingStageStatus());
				}
			}

			AddPossibleMatch(matchingQuery, CreateMatchDelegate(referencesParent));
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		MatchDelegate CreateMatchDelegate(AgencyShipmentReferences referencesParent)
		{
			var vesselNames = referencesParent.SailingReferences
				.Select(sailingReference => sailingReference.VesselName)
				.Where(vesselName => !vesselName.IsEmpty)
				.ToArray();

			var lloydsNumbers = referencesParent.SailingReferences
				.Select(sailingReference => sailingReference.LloydsNumber)
				.Where(lloydsNumber => !lloydsNumber.IsEmpty)
				.ToArray();

			return agencyShipment =>
			{
				var matchCount = 0;

				if (referencesParent.IsCarrierVGM && !referencesParent.ContainerNumber.IsEmpty)
				{
					if (agencyShipment.RealContainers.OfType<AgencyShipmentContainer>().Any(c => c.JC_ContainerNum == referencesParent.ContainerNumber))
					{
						matchCount += 128;
					}

					if (!agencyShipment.IsBillOfLadingStage && agencyShipment.BookedContainers.OfType<AgencyShipmentContainer>().Any(c => c.JC_ContainerNum == referencesParent.ContainerNumber))
					{
						matchCount += 64;
					}
				}

				if (!referencesParent.OceanBillNumber.IsEmpty && agencyShipment.JS_HouseBill == referencesParent.OceanBillNumber)
				{
					matchCount += hasMasterBillNumberSubscription ? 64 : 32;
				}

				if (!referencesParent.ShipmentID.IsEmpty && agencyShipment.JS_UniqueConsignRef == referencesParent.ShipmentID)
				{
					matchCount += hasMasterBillNumberSubscription ? 32 : 64;
				}

				if ((!referencesParent.CarriersBookingReference.IsEmpty || dataContextType == DataContextType.AgencyBooking) && agencyShipment.JS_CFSReference == referencesParent.CarriersBookingReference)
				{
					matchCount += 16;
				}

				if ((!referencesParent.AgentsReference.IsEmpty || dataContextType == DataContextType.AgencyBooking) && agencyShipment.JS_BookingReference == referencesParent.AgentsReference)
				{
					matchCount += 8;

					if (!referencesParent.BookingPartyPK.IsEmpty && (agencyShipment.BookingParty?.PK ?? ZGuid.Empty) == referencesParent.BookingPartyPK)
					{
						matchCount += 4;
					}
					else if (!referencesParent.BookingPartyName.IsEmpty && agencyShipment.BookingPartyDocumentaryAddress.E2_AddressOverride
								&& agencyShipment.BookingPartyDocumentaryAddress.E2_CompanyName == referencesParent.BookingPartyName)
					{
						matchCount += 4;
					}
				}

				if (agencyShipment.Sailing != null && agencyShipment.Sailing.Vessel != null)
				{
					if (lloydsNumbers.Contains(agencyShipment.Sailing.Vessel.RV_LloydsNumber))
					{
						matchCount += 2;
					}

					if (vesselNames.Contains(agencyShipment.Sailing.Vessel.RV_Name))
					{
						matchCount += 1;
					}
				}

				return matchCount;
			};
		}

		ZDBOnlyQuery GetBookingPartyAddressQuery(ZGuid bookingPartyPk)
		{
			var result = new ZDBOnlyQuery(typeof(AgencyShipment));
			var jobDocAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.BookingPartyDocumentaryAddress);
			orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, bookingPartyPk);

			jobDocAddressQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);
			result.AddSubQuery(jobDocAddressQuery, JoinCondition.And);

			return result;
		}

		ZDBOnlyQuery GetBookingPartyNameQuery(ZString bookingPartyName)
		{
			var result = new ZDBOnlyQuery(typeof(AgencyShipment));
			var jobDocAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);

			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.BookingPartyDocumentaryAddress);
			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressOverride, true);
			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_CompanyName, bookingPartyName);
			result.AddSubQuery(jobDocAddressQuery, JoinCondition.And);

			return result;
		}

		readonly IXmlEventValueObject xmlEvent;

		bool hasMasterBillNumberSubscription => xmlEvent != null && xmlEvent.Context.SubscriptionType == "MasterBillNumber";

		#endregion

		protected override void BuildFallbackMatchDelegates(AgencyShipmentReferences referencesParent)
		{
			AddFallbackMatch(referencesParent.OriginUNLOCO, shipment => GetMatchCount(shipment.JS_RL_NKOrigin, referencesParent.OriginUNLOCO));
			AddFallbackMatch(referencesParent.DestinationUNLOCO, shipment => GetMatchCount(shipment.JS_RL_NKDestination, referencesParent.DestinationUNLOCO));
		}

		protected override bool CheckLatestParent(T parent, T parentToCompare)
		{
			return parent.JS_SystemCreateTimeUtc > parentToCompare.JS_SystemCreateTimeUtc;
		}

		bool IncludeOceanBillNumber
		{
			get { return dataContextType != DataContextType.AgencyBooking; }
		}
		readonly DataContextType dataContextType;

		ZQuery GetMatchingShipmentQuery(List<AgencyShipmentReferenceInfo> referenceInfos, int index = 0)
		{
			var query = new ZQuery();
			if (referenceInfos.Any())
			{
				var referenceInfo = referenceInfos[index];
				referenceInfo.AddIsMatching(query);

				if (index < referenceInfos.Count - 1)
				{
					var subQuery = new ZQuery();
					referenceInfo.AddIsEmpty(subQuery);
					subQuery.AddToFilter(GetMatchingShipmentQuery(referenceInfos, index + 1), JoinCondition.And);
					query.AddToFilter(subQuery, JoinCondition.Or);
				}
			}
			return query;
		}

		class AgencyShipmentReferenceInfo
		{
			public AgencyShipmentReferenceInfo(Action<ZQuery> addIsMatching, Action<ZQuery> addIsEmpty)
			{
				AddIsMatching = addIsMatching;
				AddIsEmpty = addIsEmpty;
			}

			public Action<ZQuery> AddIsMatching { get; }
			public Action<ZQuery> AddIsEmpty { get; }
		}
	}
}
