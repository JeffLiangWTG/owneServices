using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public abstract class BaseShipmentMatcher<TShipment> : CombinationKeyMatcher<TShipment, ShipmentReferences>
		where TShipment : CommonShipment
	{
		public BaseShipmentMatcher(BusinessObjectFactory factory, ShipmentReferences references, IXmlImportLogger logger, IUniversalFreightHelper helper)
			: base(factory, references, logger)
		{
			this.helper = Argument.NotNull(helper, "helper");
		}

		protected readonly IUniversalFreightHelper helper;

		protected ZString reason;

		protected virtual ZString DefaultReason => Res.GetString("442a015b-0c81-4f88-88e6-b046700d5dc8", "No matching forwarding shipment.");

		public new TShipment GetBestMatch()
		{
			reason = ZString.Empty;
			return base.GetBestMatch();
		}

		public (TShipment shipment, ZString reason) GetBestMatchWithReason()
		{
			var shipment = GetBestMatch();
			if (shipment == null && reason.IsEmpty)
			{
				if (!referencesParent.HBOLNumber.IsEmpty && !referencesParent.ShipmentID.IsEmpty)
				{
					reason = Res.GetString("a17bcde1-ed54-4c25-ab38-20e94f2b8314", "Master Bill Number and Carrier Booking Number are Invalid.");
				}
				else if (!referencesParent.HBOLNumber.IsEmpty)
				{
					reason = Res.GetString("fcc5e5de-422f-4a5e-878d-d0fdb2e6b61d", "Master Bill Number is Invalid.");
				}
				else if (!referencesParent.ShipmentID.IsEmpty)
				{
					reason = Res.GetString("edd00af6-75fc-4658-a54d-bc1c6b5454a5", "Carrier Booking Number is Invalid.");
				}
				else
				{
					reason = DefaultReason;
				}
			}

			return (shipment, reason);
		}

		protected override ZQuery GetFullQuery(ZQuery initialMatchingQuery, ShipmentReferences references)
		{
			var result = new ZQuery();
			AddJobShipmentTypeFilter(result);
			result.AddToFilter(initialMatchingQuery);
			return result;
		}

		protected abstract void AddJobShipmentTypeFilter(ZQuery query);

		protected override bool CheckLatestParent(TShipment shipment, TShipment shipmentToCompare)
		{
			return shipment.JS_SystemCreateTimeUtc > shipmentToCompare.JS_SystemCreateTimeUtc;
		}

		protected sealed override void BuildMatchingQueryAndMatchDelegates(ShipmentReferences referencesParent)
		{
			if (referencesParent.IsVGM)
			{
				if (referencesParent.WayBillTypeCode != WayBillTypeList.Codes.House
					|| (referencesParent.MBOLNumber.IsEmpty && referencesParent.ShippersReference.IsEmpty))
				{
					reason = Res.GetString("f0b98bc5-5190-4549-90a8-6a0d4a184e60", "Master bill of lading number and shipper's reference are invalid.");
					return;
				}

				BuildVGMReferencesQueryAndMatchDelegate(referencesParent);
			}
			else if (referencesParent.IsNVOCC)
			{
				BuildNVOCCReferencesQueryAndMatchDelegate(referencesParent);
			}
			else
			{
				BuildHouseBillQueryAndMatchDelegate(referencesParent);

				AddPossibleMatch(JobShipmentSchema.JS_UniqueConsignRef, referencesParent.ShipmentID, shipment => GetMatchCount(shipment.JS_UniqueConsignRef, referencesParent.ShipmentID));

				AddPossibleMatch(JobShipmentSchema.JS_BookingReference, referencesParent.ShippersReference, shipment => GetMatchCount(shipment.JS_BookingReference, referencesParent.ShippersReference));

				if (helper.ShipmentHasOrders)
				{
					BuildOrderNumbersQueryAndMatchDelegate(referencesParent);
				}

				AddPossibleMatch(JobShipmentSchema.JS_InterimReceipt, referencesParent.InterimReceipt, shipment => GetMatchCount(shipment.JS_InterimReceipt, referencesParent.InterimReceipt));

				if (ShouldMatchCfsReference)
				{
					AddPossibleMatch(JobShipmentSchema.JS_CFSReference, referencesParent.CFSReference, shipment => GetMatchCount(shipment.JS_CFSReference, referencesParent.CFSReference));
				}

				if (helper.ShipmentHasAdditionalReferences)
				{
					BuildAdditionalReferencesQueryAndMatchDelegate(referencesParent);
				}
			}
		}

		protected virtual bool ShouldMatchCfsReference
		{
			get { return false; }
		}

		protected override void BuildFallbackMatchDelegates(ShipmentReferences referencesParent)
		{
			AddFallbackMatch(referencesParent.OriginUNLOCO, shipment => GetMatchCount(shipment.JS_RL_NKOrigin, referencesParent.OriginUNLOCO));
			AddFallbackMatch(referencesParent.DestinationUNLOCO, shipment => GetMatchCount(shipment.JS_RL_NKDestination, referencesParent.DestinationUNLOCO));
		}

		void BuildHouseBillQueryAndMatchDelegate(ShipmentReferences referencesParent)
		{
			BuildHouseBillQueryAndMatchDelegate(referencesParent.HAWBNumber, true);
			BuildHouseBillQueryAndMatchDelegate(referencesParent.HBOLNumber, false);
		}

		void BuildHouseBillQueryAndMatchDelegate(ZString houseBill, bool isAir)
		{
			if (!houseBill.IsEmpty)
			{
				var query = new ZQuery(JobShipmentSchema.JS_HouseBill, houseBill);

				if (isAir)
				{
					query.AddToFilter(JobShipmentSchema.JS_TransportMode, Constants.TransportModes.Air);
				}
				else
				{
					query.AddToFilter(JobShipmentSchema.JS_TransportMode, SQLComparisonOperator.NotEqual, Constants.TransportModes.Air);
				}

				AddPossibleMatch(query, shipment => GetMatchCount(shipment.JS_HouseBill, houseBill));
			}
		}

		void BuildOrderNumbersQueryAndMatchDelegate(ShipmentReferences referencesParent)
		{
			if (referencesParent.OrderNumbers != null && referencesParent.OrderNumbers.Count > 0)
			{
				var representativeShipment = GetRepresentativeShipmentMatchingOrderNumbers(referencesParent.OrderNumbers);
				AddFallbackMatch(representativeShipment, shipment => GetMatchCount(shipment.DocsAndCartage.OrderItems, JobOrderItemSchema.JT_OrderReference, referencesParent.OrderNumbers));
			}
		}

		TShipment GetRepresentativeShipmentMatchingOrderNumbers(List<ZString> orderNumbers)
		{
			var orderItemSubQuery = new ZDBOnlySubQuery(typeof(OrderItem), JobOrderItemSchema.JT_JP);
			orderItemSubQuery.AddToFilter(JobOrderItemSchema.JT_OrderReference, orderNumbers);

			var docsAndCartageSubQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
			docsAndCartageSubQuery.AddSubQuery(orderItemSubQuery, JoinCondition.And);

			var shipmentQuery = new ZDBOnlyQuery(typeof(TShipment));
			AddJobShipmentTypeFilter(shipmentQuery);
			shipmentQuery.AddSubQuery(docsAndCartageSubQuery, JoinCondition.And);
			shipmentQuery.MaximumRows = 1;

			return factory.LoadTop1<TShipment>(shipmentQuery);
		}

		void BuildAdditionalReferencesQueryAndMatchDelegate(ShipmentReferences referencesParent)
		{
			var helper = new AdditionalReferencesMatchingHelper<TShipment>(factory);
			var shipmentPKsMatchingAdditionalReferences = helper.GetParentsMatchingAdditionalReferences(referencesParent, AddJobShipmentTypeFilter);

			AddPossibleMatch(JobShipmentSchema.PK, shipmentPKsMatchingAdditionalReferences,
				shipment => GetMatchCount(shipment.Numbers, CusEntryNumSchema.CE_EntryType, CusEntryNumSchema.CE_EntryNum, referencesParent.AdditionalReferences));
		}

		public IEnumerable<(string KeyValue, string KeySource)> GetMatchingShipmentKeys(ShipmentReferences referencesParent)
		{
			return new AdditionalReferencesMatchingHelper<TShipment>(factory).GetKeysFromAdditionalReferences(referencesParent);
		}

		#region VGM

		void BuildVGMReferencesQueryAndMatchDelegate(ShipmentReferences referencesParent)
		{
			var shipmentQuery = new ZQuery();

			if (!referencesParent.MBOLNumber.IsEmpty)
			{
				shipmentQuery.AddToFilter(JobShipmentSchema.JS_HouseBill, referencesParent.MBOLNumber);
			}

			if (!referencesParent.ShippersReference.IsEmpty)
			{
				var bookingRefQuery = new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, referencesParent.ShippersReference);
				shipmentQuery.AddToFilter(bookingRefQuery, JoinCondition.Or);
			}

			AddPossibleMatch(shipmentQuery, CreateMatchDelegate(referencesParent));
		}

		MatchDelegate CreateMatchDelegate(ShipmentReferences referencesParent)
		{
			return shipment =>
			{
				var matchCount = 0;

				if (!referencesParent.SCAC.IsEmpty)
				{
					matchCount += shipment.Consols.Cast<CommonConsol>().Any(c => c.SendingForwarder.HasSCAC(referencesParent.SCAC)) ? 4 : 0;
				}

				if (!referencesParent.MBOLNumber.IsEmpty)
				{
					matchCount += shipment.JS_HouseBill == referencesParent.MBOLNumber ? 2 : 0;
				}

				if (!referencesParent.ShippersReference.IsEmpty)
				{
					matchCount += shipment.JS_UniqueConsignRef == referencesParent.ShippersReference ? 1 : 0;
				}

				return matchCount;
			};
		}

		#endregion

		protected virtual void BuildNVOCCReferencesQueryAndMatchDelegate(ShipmentReferences referencesParent)
		{
		}
	}
}
