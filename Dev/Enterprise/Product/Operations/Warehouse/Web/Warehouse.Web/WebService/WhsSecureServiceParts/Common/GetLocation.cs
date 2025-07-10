using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetLocation

		/// <summary>
		/// This needs to return the location because we need to know that is not a Pallet ID,
		/// for example when the location is Void, it is not a valid location but it is also not a Pallet ID.
		/// </summary>
		/// <param name="locationBarcode"></param>
		/// <returns></returns>
		[WebMethod(Description = "Get Location by Barcode")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsLocationWebServiceResponse GetLocation(string locationBarcode)
		{
			return HandleWebServiceRequest<WhsLocationWebServiceResponse>(result => GetLocationFromBarcode(result, locationBarcode));
		}

		#region GetLocationFromBarcode

		void GetLocationFromBarcode(WhsLocationWebServiceResponse response, string locationBarcode)
		{
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			if (warehouse != null)
			{
				var location = WebServiceHelper.GetLocationByLocationString(Factory, warehouse, locationBarcode);

				if (location == null)
				{
					response.LogBusinessValidationError(Res.GetString("828f1535-ffb5-48ce-ac5d-853853541365",
						"Location {0} does not exist in warehouse {1}", locationBarcode,
						warehouse.WW_WarehouseNameMultilingual));
				}
				else if ((location.LocationType?.WLT_LocationClass ?? string.Empty) == LocationClasses.Codes.PST && location.WLV_LocationStatus != LocationStatus.Codes.Normal)
				{
					response.LogBusinessValidationError(Res.GetString("816fa2cf-136a-42c3-88da-297be4f5c9dd",
						"Location {0} is an invalid Packing Station Location in Warehouse {1}", location.WLV_LocationString,
						warehouse.WW_WarehouseNameMultilingual));
				}
				else
				{
					SetLocationResponseFields(Factory, response, location);
				}
			}
			else
			{
				response.LogBusinessValidationError(Res.GetString("73D3FFA7-9956-471A-96B5-ECC557D0112D", "Warehouse should not be null."));
			}
		}

		#endregion

		#region SetLocationResponseFields

		static void SetLocationResponseFields(BusinessObjectFactory factory, WhsLocationWebServiceResponse response, WhsLocation location)
		{
			response.Location = location.ToLocationString();
			response.LocationUserFriendly = location.WLV_LocationString_UserFriendly;
			response.LocationFormattedCheckDigit = location.FormattedCheckDigit;
			response.LocationPK = location.PK.ToGuid();
			response.IsVoidLocation = location.WLV_LocationStatus.EqualsIgnoringCase(LocationStatus.Codes.Void);
			if (location.WLV_MaxQuantity == 0)
			{
				response.QuantityLeftUntilFull = decimal.MaxValue;
			}
			else
			{
				response.QuantityLeftUntilFull = location.WLV_MaxQuantity - WebServiceHelper.GetConsumedCapacityForLocation(factory, location.PK);
				if (response.QuantityLeftUntilFull < 0)
				{
					response.QuantityLeftUntilFull = 0;
				}
			}

			var locationTypeValue = location.LocationType;
			response.IsFixed = locationTypeValue != null && locationTypeValue.WLT_LocationClass == LocationClasses.Codes.FIX;
			response.IsDockDoorLocation = locationTypeValue != null && locationTypeValue.WLT_LocationClass == LocationClasses.Codes.DDL;
			response.IsPackingStation = locationTypeValue != null && locationTypeValue.WLT_LocationClass == LocationClasses.Codes.PST;
			response.IsPackingConsolidation = locationTypeValue != null && locationTypeValue.WLT_LocationClass == LocationClasses.Codes.CON;
		}

		#endregion

		#endregion
	}
}
