using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.eTail.DataTransfer.Universal
{
	class HVLVConsignmentEventParentFinder : EventParentFinder
	{
		public HVLVConsignmentEventParentFinder(HVLVConsignmentDataContextManager manager, BusinessObjectFactory factory, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			var eventContext = (xmlEvent as IXmlEventValueObject)?.Context;

			if (eventContext != null)
			{
				var hawbNumber = eventContext.HAWBNumber;
				var hbolNumber = eventContext.HBOLNumber.GetValueOrDefault();

				if (!hawbNumber.IsEmpty && !hbolNumber.IsEmpty && hawbNumber != hbolNumber)
				{
					return null;
				}

				var masterHouseBill = eventContext.MasterHouseBill.GetValueOrDefault();
				if (!hawbNumber.IsEmpty)
				{
					return MatchByWaybillAndMasterbill(hawbNumber, eventContext.MAWBNumber, masterHouseBill);
				}
				else if (!hbolNumber.IsEmpty)
				{
					return MatchByWaybillAndMasterbill(hbolNumber, eventContext.MBOLNumber.GetValueOrDefault(), masterHouseBill);
				}
			}

			return null;
		}

		HVLVConsignment[] MatchByWaybillAndMasterbill(ZString housebill, ZString masterbill, ZString masterHouseBill)
		{
			var query = new ZQuery(HVLVConsignmentSchema.HVC_WaybillNumber, housebill);
			query.AddToFilter(HVLVConsignmentSchema.HVC_IsActive, true);

			var matchedByWaybill = factory.Load<HVLVConsignment>(query);

			if (masterbill.IsEmpty)
			{
				if (matchedByWaybill.Length == 1)
				{
					return matchedByWaybill;
				}
			}
			else
			{
				masterbill = masterbill.Replace("-", "");
				bool isMatchedByConsolMasterbill(HVLVConsignment consignment)
				{
					var shipment = consignment.ManifestedOnShipment;

					return shipment != null
						&& (shipment.JS_HouseBill == masterHouseBill || masterHouseBill.IsEmpty)
						&& shipment.Consols
						.Any(t => t is ForwardingConsol consol && consol.JK_MasterBillNum == masterbill);
				}

				var result = matchedByWaybill
							.Where(consignment => isMatchedByConsolMasterbill(consignment))
							.Distinct()
							.ToArray();

				if (result.Length > 0)
				{
					return result;
				}
			}

			return null;
		}
	}
}
