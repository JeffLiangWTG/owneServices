using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ShipmentLocator<TShipment> where TShipment : CommonShipment
	{
		public ShipmentLocator(BusinessObjectFactory factory, ZQuery additionalQuery, CommonConsol consol = null, bool includeInactive = true)
		{
			Argument.NotNull(factory, "factory");

			this.factory = factory;
			this.consol = consol;
			this.additionalQuery = additionalQuery;
			this.includeInactive = includeInactive;
		}

		readonly BusinessObjectFactory factory;
		readonly CommonConsol consol;
		readonly ZQuery additionalQuery;
		readonly bool includeInactive;

		public TShipment Find(ZString shipmentNumber, ZString houseBill, ZString[] otherAgentReferences, ZDateTime etd, string origin = "", string destination = "")
		{
			TShipment result = null;

			if (ImportMatchingByNumber && !shipmentNumber.IsEmpty)
			{
				result = FindByShipmentNumber(shipmentNumber);
			}

			if (ImportMatchingByHouseBill && result == null && !houseBill.IsEmpty)
			{
				result = FindByHouseBill(houseBill, origin, destination, etd);
			}

			if (ImportMatchingByOtherAgentReferences && result == null)
			{
				result = FindByOtherAgentReferences(otherAgentReferences, shipmentNumber);
			}

			return result;
		}

		#region Implementation

		TShipment FindByShipmentNumber(ZString shipmentNumber)
		{
			ZQuery shipmentNumberQuery = GetCompleteQuery(JobShipmentSchema.JS_UniqueConsignRef, shipmentNumber);
			shipmentNumberQuery.IgnoreActiveFilter = includeInactive;

			return factory.LoadTop1<TShipment>(shipmentNumberQuery);
		}

		TShipment FindByHouseBill(ZString houseBill, ZString origin, ZString destination, ZDateTime etd)
		{
			ZQuery housebillQuery = GetCompleteQuery(JobShipmentSchema.JS_HouseBill, houseBill);

			var shipments = consol != null
								? factory.Load<TShipment>(housebillQuery).Where(shipment => consol.Shipments.Contains(shipment.PK))
								: factory.Load<TShipment>(housebillQuery);

			var result = new List<TShipment>(shipments);
			result.StableSort((s1, s2) => GetScore(s1, origin, destination, etd).CompareTo(GetScore(s2, origin, destination, etd)));

			return result.LastOrDefault();
		}

		int GetScore(TShipment shipment, ZString origin, ZString destination, ZDateTime etd)
		{
			int result = 0;

			if (!origin.IsEmpty && shipment.JS_RL_NKOrigin == origin)
			{
				result += 160;
			}
			else if (origin.IsEmpty && shipment.JS_RL_NKOrigin.IsEmpty)
			{
				result += 10;
			}
			else if (origin.IsEmpty || shipment.JS_RL_NKOrigin.IsEmpty)
			{
				result += 1;
			}

			if (!destination.IsEmpty && shipment.JS_RL_NKDestination == destination)
			{
				result += 80;
			}
			else if (destination.IsEmpty && shipment.JS_RL_NKDestination.IsEmpty)
			{
				result += 10;
			}
			else if (destination.IsEmpty || shipment.JS_RL_NKDestination.IsEmpty)
			{
				result += 1;
			}

			if (!etd.IsEmpty && shipment.JS_E_DEP == etd)
			{
				result += 40;
			}
			else if (etd.IsEmpty && shipment.JS_E_DEP.IsEmpty)
			{
				result += 10;
			}
			else if (etd.IsEmpty || shipment.JS_E_DEP.IsEmpty)
			{
				result += 1;
			}

			return result;
		}

		TShipment FindByOtherAgentReferences(ZString[] otherAgentReferences, ZString shipmentNumber)
		{
			TShipment result = null;

			if (otherAgentReferences != null && otherAgentReferences.Any())
			{
				ZQuery shipmentNumberQuery = GetCompleteQuery(JobShipmentSchema.JS_UniqueConsignRef, otherAgentReferences);

				result = consol != null
					? consol.Shipments.Find(shipmentNumberQuery).Cast<TShipment>().FirstOrDefault()
					: factory.LoadTop1<TShipment>(shipmentNumberQuery);
			}

			if (result == null && !shipmentNumber.IsEmpty && (consol == null || consol.IsInDatabase))
			{
				ZDBOnlySubQuery otherAgentReferencesSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				otherAgentReferencesSubQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, JobShipmentSchema.Constants.TableName);
				otherAgentReferencesSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.OtherAgentReference);
				otherAgentReferencesSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, shipmentNumber);

				ZDBOnlyQuery shipmentQuery = new ZDBOnlyQuery(typeof(TShipment));
				shipmentQuery.AddSubQuery(otherAgentReferencesSubQuery, JoinCondition.And);

				if (additionalQuery != null)
				{
					shipmentQuery.AddToFilter(additionalQuery);
				}

				if (consol != null)
				{
					ZDBOnlySubQuery consolLinkSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
					consolLinkSubQuery.AddToFilter(JobConShipLinkSchema.JN_JK, consol.PK);

					shipmentQuery.AddSubQuery(consolLinkSubQuery, JoinCondition.And);
				}

				result = factory.LoadTop1<TShipment>(shipmentQuery);
			}

			return result;
		}

		ZQuery GetCompleteQuery(SchemaColumn shipmentColumn, ZString value)
		{
			return GetCompleteQuery(shipmentColumn, new ZString[] { value });
		}

		ZQuery GetCompleteQuery(SchemaColumn shipmentColumn, ZString[] value)
		{
			ZQuery query = new ZQuery(shipmentColumn, value);
			if (additionalQuery != null)
			{
				query.AddToFilter(additionalQuery);
			}

			return query;
		}

		bool ImportMatchingByNumber
		{
			get { return ImportMatchingCriteria == Constants.ShipmentNumberImportTypes.Code.ShipmentNumber || ImportMatchingCriteria == Constants.ShipmentNumberImportTypes.Code.ShipmentThenHouseBill; }
		}

		bool ImportMatchingByHouseBill
		{
			get { return ImportMatchingCriteria == Constants.ShipmentNumberImportTypes.Code.HouseBill || ImportMatchingCriteria == Constants.ShipmentNumberImportTypes.Code.ShipmentThenHouseBill; }
		}

		bool ImportMatchingByOtherAgentReferences
		{
			get { return ImportMatchingCriteria == Constants.ShipmentNumberImportTypes.Code.HouseBill && SystemDataRegistry.Instance.AllowMatchingByOtherAgentReferencesOnImport.Value; }
		}

		ZString ImportMatchingCriteria
		{
			get { return SystemDataRegistry.Instance.ImportShipmentNoFromXml.Value; }
		}

		#endregion
	}
}
