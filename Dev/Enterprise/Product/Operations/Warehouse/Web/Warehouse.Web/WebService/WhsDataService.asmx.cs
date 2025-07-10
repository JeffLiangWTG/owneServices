using System;
using System.ComponentModel;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	/// <summary>
	/// Summary description for WhsDataService
	/// </summary>
	[WebService(Namespace = "http://cargowise.com/WarehouseRF/")]
	[ToolboxItem(false)]
	public class WhsDataService : DataService
	{
		[WebMethod(Description = "Retrieve available warehouses")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WarehouseInfoWebServiceResponse GetAvailableWarehouses()
		{
			return HandleWebServiceRequest<WarehouseInfoWebServiceResponse>(r => LoadAvailableWarehouses(r));
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void LoadAvailableWarehouses(WarehouseInfoWebServiceResponse response)
		{
			var virtualWarehousesEnabledUntil = WarehouseDataRegistry.Instance.VirtualWarehouseOnRFEnabledUntil.Value;
			var virtualWarehousesAllowed = virtualWarehousesEnabledUntil != DateTime.MinValue && virtualWarehousesEnabledUntil > ZDateTime.UtcNow;

			var sql = $@"
SELECT
	WW_WarehouseName, WW_WarehouseCode, GB_Code, GB_RN_NKCountryCode
FROM
	dbo.WhsWarehouse JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
WHERE
	WW_IsActive = 1
	AND WW_WarehouseType IN ('PRW', 'FTZ'){(virtualWarehousesAllowed ? string.Empty : " AND WW_IsVirtualWarehouse <> 1")}
ORDER BY
	WW_WarehouseName";

			var result = new WarehouseInfoCollection();
			using (Db.DisposableActionForDbConnection())
			using (var reader = Db.Connection.Command(sql).ExecuteReader()) // It is cheaper and quicker to bypass using a BusinessObjectFactory here
			{
				while (reader.Read())
				{
					var warehouse = new WarehouseInfo();
					warehouse.Code = (string)reader[WhsWarehouseSchema.WW_WarehouseCode.Name];
					warehouse.Name = (string)reader[WhsWarehouseSchema.WW_WarehouseName.Name];
					warehouse.BranchCode = (string)reader[GlbBranchSchema.GB_Code.Name];
					warehouse.CountryCode = (string)reader[GlbBranchSchema.GB_RN_NKCountryCode.Name];
					result.Add(warehouse);
				}
			}

			response.WarehouseInfos = result;
		}

		#endregion

		#region VerifyEquipment

		[WebMethod(Description = "Verifies if the Equipment Registration Code exists")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse VerifyEquipmentRegistrationCode(string equipmentRegistrationCode, bool hasUOMType)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => CheckEquipmentRegistrationCode(equipmentRegistrationCode, hasUOMType, r));
		}

		void CheckEquipmentRegistrationCode(string equipmentRegistrationCode, bool hasUOMType, WebServiceResponse response)
		{
			if (string.IsNullOrEmpty(equipmentRegistrationCode))
			{
				response.Error = ErrorTypes.None;
			}
			else
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factory = new BusinessObjectFactory();
					var refEquipment = factory.LoadTop1<RefEquipment>(new ZQuery(RefEquipmentSchema.RQ_Registration, equipmentRegistrationCode));
					if (refEquipment == null)
					{
						response.Error = ErrorTypes.BusinessValidationError;
						response.ErrorMessage = Res.GetString("658c63e9-43fa-41ca-918a-a05f55e02658",
							"A valid Equipment Registration Number could not be found on the specified web server, please check the Equipment Registration Number.");
					}
					else if (hasUOMType && !refEquipment.RQ_F3_NKPackType.IsEmpty)
					{
						response.Error = ErrorTypes.BusinessValidationError;
						response.ErrorMessage = Res.GetString("e3b24851-44da-48b4-a840-826fbf4b9330",
							"Equipment Reference '{0}' has a pack type constraint. Therefore cannot be used with a UOM Type.", equipmentRegistrationCode);
					}
				}
			}
		}

		#endregion
	}
}
