using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.Warehouse;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService
{
	/// <summary>
	/// Summary description for SecureService
	/// </summary>
	[WebService(Namespace = "http://cargowise.com/WarehouseRF/")]
	[ToolboxItem(false)]
	public class SecureService : BaseService
	{
		public SecureService()
			: base()
		{
			DisposableList = new DisposableList(2) { Db.DisposableActionForDbConnection() };
			Factory = new BusinessObjectFactory();

			FactoryService = new Lazy<IFactoryService>(PrepareFactoryService);
		}

		DisposableList DisposableList { get; }

		protected void AddDisposableToDisposeOnDispose(IDisposable disposable) => DisposableList.Add(disposable);

		IFactoryService PrepareFactoryService()
		{
			var factoryService = ObjectFactory.Get<IFactoryService>();
			factoryService.RegisterFactory(() => new BusinessObjectFactory());
			return factoryService;
		}

		#region ValidateLogin

		[WebMethod(Description = "Check whether login credentials work")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WarehouseUserLoginResponse ValidateLogin()
		{
			var result = new WarehouseUserLoginResponse();
			try
			{
				if (AllowedToRunService(result))
				{
					var whs = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
					var taskManagementEnabledForWarehouse = whs?.WW_GG_ReleaseGroup.IsValid ?? false;
					if (!SecurityHeader.IsAndroidDevice && taskManagementEnabledForWarehouse)
					{
						result.LogBusinessValidationError(Res.GetString("46d888b0-4e72-405c-a44e-831f65911493", "Warehouses using Task Management are not supported on Windows CE devices."));
					}
					else
					{
						var currentCompany = GlbCompany.CurrentCompany;
						result.CurrentCompany = currentCompany.CompanyName;
						result.CurrentCompanyCode = currentCompany.GC_Code;
						result.CurrentCompanyCountryCode = currentCompany.GC_RN_NKCountryCode;
						result.ClientEnterpriseCode = currentCompany.LicenceEnterpriseCode;
						result.ClientLicenceServerID = currentCompany.LicenceServerID;
						result.DefaultScanAll = whs?.WW_ScanAll ?? false;
						result.RFVolcamEnabled = WarehouseDataRegistry.Instance.EnableRFVolcam.Value;
						result.WarehouseDateTime = Env.Time.CurrentUtcDateTime;

						var enabledFeatures = new[]
						{
							WhsNonExposedFeatureName.SchemaRedesignChanges.IsEnabled(),       // #1
							taskManagementEnabledForWarehouse, // #2
						};
						result.EnabledFeatureFlags = enabledFeatures.Select((value, i) => Convert.ToInt32(value) * (i + 1)).ToArray();
						result.RFInactivityLimit = WarehouseDataRegistry.Instance.RFInactivityPeriod.Value;
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex);
			}
			return result;
		}

		#region AllowedToRunService

		protected override void AllowedToRunServiceCore(WebServiceResponse response, bool requireValidateConcurencyLogin = true)
		{
			#region Testing Purposes Only
#if DEBUG
			AllowedToRunServiceHasBeenCalled = true;
#endif
			#endregion

			base.AllowedToRunServiceCore(response, requireValidateConcurencyLogin);
			if (response.Error == ErrorTypes.None)
			{
				LoginHelper.Validate(response, SecurityHeader, requireValidateConcurencyLogin);
			}
		}

		#endregion

		#endregion

		#region Logout

		[WebMethod(Description = "Logout a user from a device.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse Logout(bool isRemoteLogout)
		{
			var result = new WebServiceResponse();
			try
			{
				LoginHelper.LogoutUser(SecurityHeader, isRemoteLogout);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleException(ex);
			}
			return result;
		}

		#endregion

		#region Implementation

#if DEBUG
		public bool AllowedToRunServiceHasBeenCalled;

		internal
#else
		protected
#endif
		readonly BusinessObjectFactory Factory;

		protected readonly Lazy<IFactoryService> FactoryService;

		protected override void Dispose(bool disposing)
		{
			DisposableList.Dispose();
			base.Dispose(disposing);
		}

		#endregion
	}
}
