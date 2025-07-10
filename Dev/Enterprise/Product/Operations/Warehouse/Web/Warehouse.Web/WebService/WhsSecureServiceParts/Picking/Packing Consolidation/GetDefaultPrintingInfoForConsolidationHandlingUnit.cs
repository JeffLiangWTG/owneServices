using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Shared;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetDefaultPrintingInfoForConsolidationHandlingUnit

		[WebMethod(Description = "Get Default Printing Information")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public DefaultPrintingInfoWebServiceResponse GetDefaultPrintingInfoForConsolidationHandlingUnit(PackageForPrintingInfo packageInfo)
		{
			return HandleWebServiceRequest<DefaultPrintingInfoWebServiceResponse>(r => GetDefaultPrintingInfoForConsolidationHandlingUnit(r, packageInfo));
		}

		void GetDefaultPrintingInfoForConsolidationHandlingUnit(DefaultPrintingInfoWebServiceResponse response, PackageForPrintingInfo packageInfo)
		{
			if (packageInfo == null)
			{
				response.LogBusinessValidationError(Res.GetString("4060b302-d91a-4762-89f2-9ba3070a7f65", "Invalid Request for Printing Information."));
			}
			else
			{
				var package = Factory.Load<PkgPackage>(packageInfo.PK);
				if (package != null)
				{
					GetDefaultPrintingInfoForConsolidationHandlingUnitCore(response, package, packageInfo.SupportsCarrierLabelIntegration);
				}
				else
				{
					response.LogBusinessValidationError(Res.GetString("3ca98381-7997-4c52-a65b-51d78e30d8b7", "Handling Unit was not found."));
				}
			}
		}

		void GetDefaultPrintingInfoForConsolidationHandlingUnitCore(DefaultPrintingInfoWebServiceResponse response, PkgPackage package, bool supportsCarrierLabelIntegration)
		{
			SetAvailablePrinters(response, includeEmptyPrinter: false);

			if (response.Printers.Length > 0)
			{
				response.DefaultPrinter = GetPrinterFromPackageLocationOrWarehouse(package.PK);

				if (response.DefaultPrinter == Guid.Empty && !supportsCarrierLabelIntegration)
				{
					var documentToPrint = LoadHandlingUnitBasicLabel(Factory);
					if (documentToPrint != null)
					{
						var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
						var defaultPrinterForUser = StmDefaultPrinter.LoadDefaultPrinter(Factory, staff, documentToPrint);
						if (defaultPrinterForUser != null && defaultPrinterForUser.SDP_SQ_Printer.IsValid)
						{
							response.DefaultPrinter = defaultPrinterForUser.SDP_SQ_Printer.ToGuid();
						}
					}
				}
			}
			else
			{
				response.LogBusinessValidationError(Res.GetString("1d322520-fa7e-480f-8a4c-0361a3d23b8e", "No printers exist in the system."));
			}
		}

		Guid GetPrinterFromPackageLocationOrWarehouse(ZGuid packagePK)
		{
			var pkgPackageLocation = Factory.LoadTop1<WhsPackageLocationView>(new ZQuery(WhsPackageLocationViewSchema.WPK_KP_Package, packagePK));

			var ddlArea = pkgPackageLocation.Location.PutawayArea;
			var result = ddlArea?.RFPickPackPrinterPK ?? ZGuid.Empty;
			if (!result.IsValid)
			{
				result = pkgPackageLocation.Warehouse.RFPickPackPrinterPK;
			}

			return result.IsValid ? result.ToGuid() : Guid.Empty;
		}

		#endregion
	}
}
