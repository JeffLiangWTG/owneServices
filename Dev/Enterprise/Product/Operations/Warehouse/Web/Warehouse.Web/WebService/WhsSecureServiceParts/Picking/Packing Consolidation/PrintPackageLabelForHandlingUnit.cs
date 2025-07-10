using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Print Package Label For HandlingUnit")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse PrintPackageLabelForHandlingUnit(Guid packagePK, Guid printerPK, int numberOfLabelsToPrint)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => PrintPackageLabelForHandlingUnit(r, packagePK, printerPK, numberOfLabelsToPrint));
		}

		void PrintPackageLabelForHandlingUnit(WebServiceResponse response, Guid packagePK, Guid printerPK, int numberOfLabelsToPrint)
		{
			var package = Factory.Load<PkgPackage>(packagePK);
			if (package != null)
			{
				var documentToPrint = LoadHandlingUnitBasicLabel(Factory);

				EventHandler<AutoPrintFailedEventArgs> autoPrintFailed = (sender, e) => response.LogBusinessValidationError(e.Message);

				var packageJob = package.PackageJob;
				packageJob.AutoPrintFailed += autoPrintFailed;

				try
				{
					package.PrintDocument(printerPK, numberOfLabelsToPrint, documentToPrint);
				}
				finally
				{
					packageJob.AutoPrintFailed -= autoPrintFailed;
				}
			}
			else
			{
				response.LogBusinessValidationError(Res.GetString("3ca98381-7997-4c52-a65b-51d78e30d8b7", "Handling Unit was not found."));
			}
		}

		static DocumentCommand LoadHandlingUnitBasicLabel(BusinessObjectFactory factory)
		{
			var basicLabelQuery = new DocumentZQuery(BusinessContext.PackageHandlingUnit, (NoResString)"Basic Label"); // Hard-coded constant
			return factory.LoadTop1<DocumentCommand>(basicLabelQuery);
		}
	}
}
