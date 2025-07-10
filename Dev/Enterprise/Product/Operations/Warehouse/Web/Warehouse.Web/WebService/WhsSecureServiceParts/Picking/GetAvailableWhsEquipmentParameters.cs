using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetAvailableWhsEquipmentParameters

		[WebMethod(Description = "Get available warehouse pick methods, areas, groups & printers")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsEquipmentParametersWebServiceResponse GetAvailableWhsEquipmentParameters()
		{
			return HandleWebServiceRequest<WhsEquipmentParametersWebServiceResponse>(result => result.EquipmentParameters = LoadAvailableWhsEquipmentParameters(SecurityHeader.WarehouseCode));
		}

		#region LoadAvailableWhsEquipmentParameters

		WhsEquipmentParameters LoadAvailableWhsEquipmentParameters(string warehouseCode)
		{
			var pickMethods = GetPickMethods();
			var pickAreas = GetPickAreas(warehouseCode);
			var pickGroups = GetPickGroups();
			var printers = GetPrintersUtil(includeEmptyPrinter: true);
			var result = new WhsEquipmentParameters(pickMethods, pickAreas, pickGroups, printers);
			return result;
		}

		#endregion

		#region GetPickMethods

		WhsPickMethodInfoCollection GetPickMethods()
		{
			var result = new WhsPickMethodInfoCollection();
			var registryPickMethods = WarehouseDataRegistry.Instance.PickMethod.Value;
			result.Add(WhsPickMethodInfo.GetAnyCodePickMethod(registryPickMethods));

			foreach (var method in registryPickMethods.Cast<ICodeDescriptionBool>().Where(method => !WhsAreaAndPickMethodHelper.IsAnyCode(method.Code)))
			{
				var pickMethod = new WhsPickMethodInfo
				{
					Code = method.Code,
					Description = method.Description,
					IsDefault = method.Bool
				};
				result.Add(pickMethod);
			}
			return result;
		}

		#endregion

		#region GetPickAreas

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		WhsAreaInfoCollection GetPickAreas(string warehouseCode)
		{
			var result = new WhsAreaInfoCollection();
			var warehouse = WebServiceHelper.GetWarehouse(Factory, warehouseCode);
			if (warehouse != null)
			{
				var pickingAreas = warehouse.Areas.Where(area => area.WA_IsPickingArea);
				foreach (var pickingArea in pickingAreas)
				{
					result.Add(new WhsAreaInfo(pickingArea.WA_Name, pickingArea.WA_NameMultilingual, pickingArea.PK.ToGuid()));
				}
			}
			if (!result.Any(areaInfo => WhsAreaAndPickMethodHelper.IsAnyCode(areaInfo.Name)))
			{
				var areaInfo = new WhsAreaInfo(WhsAreaAndPickMethodHelper.AnyCode, Res.GetString("c8d033a4-ea5e-4a87-af43-fc859a6ed209", "ANY"), areaPK: Guid.Empty);
				result.Add(areaInfo);
			}
			return result;
		}

		#endregion

		#region GetPickGroups

		WhsPickGroupInfoCollection GetPickGroups()
		{
			var result = new WhsPickGroupInfoCollection { WhsPickGroupInfo.GetDefaultPickGroup() };
			WarehouseDataRegistry.Instance.PickGroups.Value.ForEach<PickGroup>(x => result.Add(new WhsPickGroupInfo(x)));
			return result;
		}

		#endregion

		#endregion
	}
}
