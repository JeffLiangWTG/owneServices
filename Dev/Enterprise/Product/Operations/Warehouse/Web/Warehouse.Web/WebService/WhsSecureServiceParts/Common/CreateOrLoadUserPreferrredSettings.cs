using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Create Or Load User Preferred Settings")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WarehouseRFRegistryInfoResponse CreateOrLoadUserPreferredSettings(string equipmentRegistrationNumber)
		{
			return HandleWebServiceRequest<WarehouseRFRegistryInfoResponse>(result => CreateOrLoadUserPreferredSettingsCore(result, equipmentRegistrationNumber));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		void CreateOrLoadUserPreferredSettingsCore(WarehouseRFRegistryInfoResponse response, string equipmentRegistrationNumber)
		{
			RefEquipment equipment = null;
			if (!string.IsNullOrEmpty(equipmentRegistrationNumber))
			{
				equipment = Factory.Load<RefEquipment>(new ZQuery(RefEquipmentSchema.RQ_Registration, equipmentRegistrationNumber)).SingleOrDefault();
				if (equipment == null)
				{
					response.LogBusinessValidationError(Res.GetString("8c9551c9-67e5-41ae-b2aa-2fe642c437ef", "Equipment '{0}' could not be found.", equipmentRegistrationNumber));
				}
			}

			if (response.NoError())
			{
				var user = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
				var registryQuery = new ZQuery(WhsRFRegistrySchema.WRR_GS_NKAssignedTo, user.GS_Code);
				var registry = Factory.LoadTop1<WhsRFRegistry>(registryQuery) ?? CreateDefaultWhsRFRegistry(user);
				registry.WRR_RQ_LastUsedEquipment = equipment?.PK ?? ZGuid.Empty;

				var concurrencyErrorMessage = Res.GetString("9d79567e-b84b-4e34-80db-2ba60af65719", "Unable to save user registry as another user made changes. Please refresh and try again.");
				WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);

				if (response.NoError())
				{
					var pickArea = registry.PickingArea;
					var pickAreaInfo = pickArea != null
						? new WhsAreaInfo(pickArea.WA_Name, pickArea.WA_NameMultilingual, pickArea.PK.ToGuid())
						: new WhsAreaInfo();

					var printerInfo = registry.WRR_SQ_Printer.IsValid
						? new PrinterInfo(Factory.Load<StmPrintQueue>(registry.WRR_SQ_Printer))
						: new PrinterInfo();

					response.RFRegistryInfo = new WhsRFRegistryInfo
					{
						ClientCode = registry.Client?.OH_Code,
						UOMType = registry.WRR_UOMPackType,
						EnableErrorAudio = registry.WRR_EnableWarehouseErrorAudio,
						Language = user.Language,
						PickGroup = GetPickGroupInfoForRegistry(registry),
						PickMethod = GetPickMethodInfoForRegistry(registry),
						PickArea = pickAreaInfo,
						Printer = printerInfo,
						EquipmentRegistrationNumber = registry.LastUsedEquipment?.RQ_Registration,
					};
				}
			}

			WhsRFRegistry CreateDefaultWhsRFRegistry(GlbStaff user)
			{
				var whs = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);

				var registry = Factory.New<WhsRFRegistry>();
				registry.WRR_GS_NKAssignedTo = user.GS_Code;
				registry.WRR_WW_Whs = whs.PK;

				return registry;
			}

			WhsPickGroupInfo GetPickGroupInfoForRegistry(WhsRFRegistry registry)
			{
				WhsPickGroupInfo pickGroupInfo = null;
				if (registry.WRR_PickGroupSequence != 0)
				{
					var registryPickGroups = WarehouseDataRegistry.Instance.PickGroups.Value.Cast<PickGroup>();
					var pickGroup = registryPickGroups.FirstOrDefault(pg => pg.PickSequence == registry.WRR_PickGroupSequence);

					if (pickGroup != null)
					{
						pickGroupInfo = new WhsPickGroupInfo(pickGroup);
					}
				}

				return pickGroupInfo ?? WhsPickGroupInfo.GetDefaultPickGroup();
			}

			WhsPickMethodInfo GetPickMethodInfoForRegistry(WhsRFRegistry registry)
			{
				WhsPickMethodInfo pickMethodInfo = null;

				var registryPickMethods = WarehouseDataRegistry.Instance.PickMethod.Value;
				if (!WhsAreaAndPickMethodHelper.IsAnyCode(registry.WRR_PickMethodCode))
				{
					var pickMethod = registryPickMethods.Cast<ICodeDescriptionBool>()
						.FirstOrDefault(method => method.Code == registry.WRR_PickMethodCode);

					if (pickMethod != null)
					{
						pickMethodInfo = new WhsPickMethodInfo(pickMethod.Code, pickMethod.Description, pickMethod.Bool);
					}
				}

				return pickMethodInfo ?? WhsPickMethodInfo.GetAnyCodePickMethod(registryPickMethods);
			}
		}
	}
}
