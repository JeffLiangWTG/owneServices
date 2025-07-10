using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ShippingOrderNumberValidationHelper
	{
		public static ForwardingShipment[] GetDuplicatedShipmentsByShippingOrderNumber(string sld, ZGuid currentShipmentPK, BusinessObjectFactory factoty)
		{
			return factoty.Load<ForwardingShipment>(GetDuplicateNumQuery(sld, currentShipmentPK));
		}

		static ZQuery GetDuplicateNumQuery(string sld, ZGuid currentShipmentPK)
		{
			var countryCodes = new ZString[] { Core.Constants.CountryCodes.China, Core.Constants.CountryCodes.Taiwan, Core.Constants.CountryCodes.HongKong };

			var shippingOrderNumberFilter = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.JL_JS);
			shippingOrderNumberFilter.AddToFilter(JobPackLinesSchema.JL_ExportRefNumber, sld);

			var additionalDetailsReferenceNumberFilter = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			additionalDetailsReferenceNumberFilter.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.AdditionalReferenceNumber);
			additionalDetailsReferenceNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber);
			additionalDetailsReferenceNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryNum, sld);

			var connQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
			connQuery.AddSubQuery(shippingOrderNumberFilter, JoinCondition.Or);
			connQuery.AddSubQuery(additionalDetailsReferenceNumberFilter, JoinCondition.Or);

			var query = new ZQuery();
			query.AddToFilter(JobShipmentSchema.PK, SQLComparisonOperator.NotEqual, currentShipmentPK);
			query.AddToFilter(JobShipmentSchema.JS_TransportMode, Core.Constants.TransportModes.Sea);
			query.AddToFilter(JobShipmentSchema.JS_RL_NKOrigin, SQLComparisonOperator.StartsWith, countryCodes);
			query.AddToFilter(connQuery);

			return query;
		}
	}
}
