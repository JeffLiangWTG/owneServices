using System.ComponentModel;
using System.Web.Services;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	[WebService(Namespace = "http://cargowise.com/WarehouseRF/")]
	[ToolboxItem(false)]
	public class TestingDataService : SecureService
	{
		//[WebMethod(Description = "CreateData")]
		//[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		//public WebServiceResponse CreateData(BusinessActions[] actionsToDo)
		//{
		//    Db.Connection.BeginTransaction();
		//    CurrentTransactions.Add(Db.Connection);

		//    var licencesList = new Licences(EnvProxy.Instance.Registry.GetLicenceKey(EnvProxy.Instance.CurrentCompany.PK));
		//    licencesList.RFScannerManager.ExpiryDate = DateTime.MaxValue;
		//    licencesList.RFScannerManager.UserLimit = 100;
		//    licencesList.RFScannerManager.LicenceType = ModuleLicenceType.PUR;
		//    EnvProxy.Instance.Registry.SetLicenceKey(EnvProxy.Instance.CurrentCompany.PK, licencesList.ToEncryptedKeyString());

		//    var result = new WebServiceResponse();
		//    try
		//    {
		//        AllowedToRunService(result);

		//        TestingState.IsRunningTests = true;
		//        Globals.IsTest = true; // need to be after AllowedToRunService so License check will be run and CurrentBranchAndCompany set.

		//        Helper.CreateClient((string)actionsToDo[0].PropertiesToSet[0].PropertyValue);
		//        Factory.Save();
		//    }
		//    catch (Exception ex)
		//    {
		//        HandleException(ex);
		//    }
		//    return result;
		//}

		//[WebMethod(Description = "BeginTransaction")]
		//[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		//public void RollbackTransactions()
		//{
		//    foreach (var connection in CurrentTransactions)
		//    {
		//        connection.RollbackTransaction();
		//    }
		//    CurrentTransactions.Clear();
		//}

		//List<DbConnection> CurrentTransactions = new List<DbConnection>();

		//#region Helper

		//protected WhsTestHelperFunctions Helper
		//{
		//    get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		//}
		//WhsTestHelperFunctions helper;

		//#endregion

		//public enum ActionType
		//{
		//    CreateClient,
		//}

		//public class BusinessActions
		//{
		//    public ActionType DataType { get; set; }

		//    public BusinessProperties[] PropertiesToSet { get; set; }
		//}

		//public class BusinessProperties
		//{
		//    public string PropertyName { get; set; }
		//    public object PropertyValue { get; set; }
		//}
	}
}
