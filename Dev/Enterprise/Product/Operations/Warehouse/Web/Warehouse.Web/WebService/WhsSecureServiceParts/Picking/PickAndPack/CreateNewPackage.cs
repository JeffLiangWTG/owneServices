using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region CreateNewPackage

		[WebMethod(Description = "Create New Package on Order")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public NewPackageWebServiceResponse CreateNewPackage(string orderDocketID, PackageInfo newPackage)
		{
			return HandleWebServiceRequest<NewPackageWebServiceResponse>(r => CreateNewPackage(r, orderDocketID, newPackage));
		}

		void CreateNewPackage(NewPackageWebServiceResponse response, string orderDocketID, PackageInfo newPackage)
		{
			var order = WebServiceHelper.GetOrderByDocketID(Factory, response, SecurityHeader.WarehouseCode, orderDocketID);
			if (order != null)
			{
				var packageJob = order.PackageJob;
				var package = packageJob.Packages.AddNew(newPackage.PackType);
				if (string.IsNullOrEmpty(newPackage.PackageID))
				{
					GenerateAndAssignPackageId(package, response);
				}
				else
				{
					AssignPackageId(package, newPackage.PackageID, response);
				}
			}
		}

		void GenerateAndAssignPackageId(PkgPackage package, NewPackageWebServiceResponse response)
		{
			try
			{
				((ISupportPackageIDGeneration)package).ShouldGenerateIDOnSaving = true;
				Factory.Save();
				response.NewPackage = new PackageInfo(package);
			}
			catch (ZSaveException saveEx)
			{
				response.LogBusinessValidationError(
					Res.GetString("8810b42d-6171-40bf-8a01-bdf11d656451", "Was unable to Generate ID for new Package:\r\n{0}", string.Join("\r\n", saveEx.InnerException.FriendlyMessage)));
			}
			catch (ZCannotSaveException ex)
			{
				response.LogBusinessValidationError(ex.Message);
			}
			catch (InvalidOperationException ex)
			{
				ErrorReporter.ReportOnce("InvalidOperationException thrown during PackageID generation" + ex.Message, ex);
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("862657D3-8A75-4788-88C4-C0AEFB1CF5B1", "Was unable to Generate ID for new Package due to invalid operation:\r\n{0}",
					string.Join("\r\n", ex.Message, ex.InnerException?.Message ?? string.Empty));
			}
		}

		void AssignPackageId(PkgPackage package, string packageId, NewPackageWebServiceResponse response)
		{
			package.KP_PackageID = packageId.ToUpper(Culture.Invariant);
			if (package.HasErrors)
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("26849C65-9602-4FA1-B598-80768EB2AAB4", "Unable to create a package with '{0}' package id.", packageId);
			}
			else
			{
				Factory.Save();
				response.NewPackage = new PackageInfo(package);
			}
		}

		#endregion
	}
}
