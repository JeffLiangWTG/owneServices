using System;
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
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Update User Preferred Settings")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse UpdateUserPreferredSettings(string clientCode, string uomType, Guid pickAreaPK, string pickMethod, short pickGroupSequence, Guid printerPK, bool enableErrorAudio)
		{
			return HandleWebServiceRequest<WebServiceResponse>(result => UpdateUserPreferredSettingsCore(result, clientCode, uomType, pickAreaPK, pickMethod, pickGroupSequence, printerPK, enableErrorAudio));
		}

		void UpdateUserPreferredSettingsCore(WebServiceResponse response, string clientCode, string uomType, Guid pickAreaPK, string pickMethod, short pickGroupSequence, Guid printerPK, bool enableErrorAudio)
		{
			if (response.NoError())
			{
				var user = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
				var registryQuery = new ZQuery(WhsRFRegistrySchema.WRR_GS_NKAssignedTo, user.GS_Code);
				var registry = Factory.LoadTop1<WhsRFRegistry>(registryQuery);

				if (registry == null)
				{
					var errorMessage = Res.GetString(
						"2fb2c70b-d814-48ee-be49-d3db6c10cdd3",
						"RF Registry Configuration for user '{0}' could not be found. Please log out and back in to recreate.",
						user.GS_Code);
					response.LogBusinessValidationError(errorMessage);
				}

				OrgHeader client = null;
				WhsArea pickArea = null;
				StmPrintQueue printer = null;
				if (response.NoError() && !string.IsNullOrEmpty(clientCode))
				{
					client = LoadClientFromCode(response, clientCode);
				}

				if (response.NoError())
				{
					ValidateUOMPackType(response, uomType);
				}

				if (response.NoError() && pickAreaPK != Guid.Empty)
				{
					pickArea = LoadAreaFromPK(response, pickAreaPK);
				}

				if (response.NoError() && !WhsAreaAndPickMethodHelper.IsAnyCode(pickMethod))
				{
					ValidatePickMethod(response, pickMethod);
				}

				if (response.NoError() && pickGroupSequence != 0)
				{
					ValidatePickGroupSequence(response, pickGroupSequence);
				}

				if (response.NoError() && printerPK != Guid.Empty)
				{
					printer = LoadPrinterFromPK(response, printerPK);
				}

				if (response.NoError())
				{
					registry.WRR_OH_Client = client?.PK ?? ZGuid.Empty;
					registry.WRR_UOMPackType = uomType;
					registry.WRR_WA_PickingArea = pickArea?.PK ?? ZGuid.Empty;
					registry.WRR_PickMethodCode = pickMethod;
					registry.WRR_PickGroupSequence = pickGroupSequence;
					registry.WRR_SQ_Printer = printer?.PK ?? ZGuid.Empty;
					registry.WRR_EnableWarehouseErrorAudio = enableErrorAudio;

					var concurrencyErrorMessage = Res.GetString("ff1168c3-91dd-4ba4-a7ee-7c138cbba5ad", "Unable to save user registry as another user made changes. Please refresh and try again.");
					WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
				}
			}

			OrgHeader LoadClientFromCode(WebServiceResponse response, string clientCode)
			{
				var client = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, clientCode));
				if (client == null)
				{
					var errorMessage = Res.GetString(
						"172c559a-4669-42b0-b957-90d4e2eb5811",
						"Specified Client Code '{0}' is not valid.",
						clientCode);
					response.LogBusinessValidationError(errorMessage);
				}
				return client;
			}

			void ValidateUOMPackType(WebServiceResponse response, string uomPackType)
			{
				var packTypes = new UOMPackTypesList();
				if (!packTypes.ContainsCode(uomPackType) && uomPackType != WhsRFRegistry.DefaultUOMPackType)
				{
					var errorMessage = Res.GetString(
						"c3c3bc89-e233-471b-9529-5021db343b87",
						"Specified UOM Type '{0}' is not valid.",
						uomPackType);
					response.LogBusinessValidationError(errorMessage);
				}
			}

			WhsArea LoadAreaFromPK(WebServiceResponse response, Guid pickAreaPK)
			{
				var area = Factory.Load<WhsArea>(pickAreaPK);
				if (area == null)
				{
					var errorMessage = Res.GetString(
						"b626f082-44fa-4800-8376-b63e8f6f6dcd",
						"Specified Pick Area is not valid.");
					response.LogBusinessValidationError(errorMessage);
				}
				return area;
			}

			void ValidatePickMethod(WebServiceResponse response, string pickMethodCode)
			{
				var registryPickMethods = WarehouseDataRegistry.Instance.PickMethod.Value;
				var pickMethod = registryPickMethods.Cast<ICodeDescriptionBool>()
					.FirstOrDefault(method => method.Code == pickMethodCode);

				if (pickMethod == null)
				{
					var errorMessage = Res.GetString(
						"2eac99ad-e634-4fa7-ac37-e7a2bf68bedb",
						"Specified Pick Method is not valid.");
					response.LogBusinessValidationError(errorMessage);
				}
			}

			void ValidatePickGroupSequence(WebServiceResponse response, short pickGroupSequence)
			{
				var registryPickGroups = WarehouseDataRegistry.Instance.PickGroups.Value.Cast<PickGroup>();
				var pickGroup = registryPickGroups.FirstOrDefault(pg => pg.PickSequence == pickGroupSequence);

				if (pickGroup == null)
				{
					var errorMessage = Res.GetString(
						"c83ef22d-f422-4be6-b223-93bf3ec873ac",
						"Specified Pick Group is not valid.");
					response.LogBusinessValidationError(errorMessage);
				}
			}

			StmPrintQueue LoadPrinterFromPK(WebServiceResponse response, Guid printerPK)
			{
				var printer = Factory.Load<StmPrintQueue>(printerPK);
				if (printer == null)
				{
					var errorMessage = Res.GetString(
						"94d1d3b3-6a19-40c6-83cd-5c6c22cf4a7f",
						"Specified Printer is not valid.");
					response.LogBusinessValidationError(errorMessage);
				}
				return printer;
			}
		}
	}
}
