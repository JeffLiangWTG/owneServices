using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ShipmentEventParentFinderHelper<TShipment> : BaseShipmentEventParentFinderHelper
		where TShipment : CommonShipment
	{
		internal ShipmentEventParentFinderHelper(IXmlEventValueObject xmlEvent, BusinessObjectFactory factory, IXmlImportLogger logger, IUniversalFreightHelper helper)
			: base(factory, logger)
		{
			this.xmlEvent = xmlEvent;
			this.helper = Argument.NotNull(helper, "helper");

			var hawbNumber = xmlEvent.Context.HAWBNumber;
			if (!hawbNumber.IsEmpty)
			{
				HouseBill = hawbNumber;
				isAir = true;
			}

			var hbolNumber = xmlEvent.Context.HBOLNumber.GetValueOrDefault();
			if (!hbolNumber.IsEmpty)
			{
				HouseBill = hbolNumber;
			}
		}

		readonly IXmlEventValueObject xmlEvent;
		readonly IUniversalFreightHelper helper;
		readonly bool isAir;

		internal ZString HouseBill { get; }

		internal ZDBOnlyQuery BuildShipmentQueryIfHouseBillPresent()
		{
			var shipmentQuery = new ZDBOnlyQuery(typeof(TShipment));
			helper.AddShipmentParameters(shipmentQuery);
			shipmentQuery.OrderBy = JobShipmentSchema.JS_SystemCreateTimeUtc.Name + " desc";

			if (!HouseBill.IsEmpty)
			{
				shipmentQuery.AddToFilter(JobShipmentSchema.JS_HouseBill, HouseBill);

				if (isAir)
				{
					shipmentQuery.AddToFilter(JobShipmentSchema.JS_TransportMode, Constants.TransportModes.Air);
				}
				else
				{
					shipmentQuery.AddToFilter(JobShipmentSchema.JS_TransportMode, SQLComparisonOperator.NotEqual, Constants.TransportModes.Air);
				}

				return shipmentQuery;
			}

			return null;
		}

		internal BusinessObject[] GetLogParents()
		{
			var shipmentQuery = BuildShipmentQueryIfHouseBillPresent();
			if (shipmentQuery != null)
			{
				var shipment = factory.LoadTop1<TShipment>(shipmentQuery);
				if (shipment != null)
				{
					var logParents = new ShipmentLinker<TShipment>(factory, helper).GetLogParent(shipment, xmlEvent);
					if (logParents != null && logParents.Any())
					{
						return logParents;
					}
				}
			}

			return null;
		}

		internal BusinessObject[] GetLogParents(ZDBOnlySubQuery consolSubQuery, ZDBOnlyQuery shipmentQuery)
		{
			var consolLinkQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			consolLinkQuery.AddSubQuery(JobConShipLinkSchema.JN_JK, consolSubQuery, JoinCondition.And);

			shipmentQuery.AddSubQuery(consolLinkQuery, JoinCondition.And);

			var shipmentsMatchingQuery = factory.Load<TShipment>(shipmentQuery);
			if (shipmentsMatchingQuery.Length > 0)
			{
				var shipmentLinker = new ShipmentLinker<TShipment>(factory, helper);
				foreach (var shipment in shipmentsMatchingQuery)
				{
					var logParents = shipmentLinker.GetLogParent(shipment, xmlEvent);
					if (logParents != null && logParents.Any())
					{
						return logParents;
					}
				}
			}

			return null;
		}

		internal ShipmentReferences GetShipmentReferences()
		{
			var shipmentReferences = new ShipmentReferences();
			PopulateShipmentReference(xmlEvent, shipmentReferences, helper);
			return shipmentReferences;
		}
	}
}
