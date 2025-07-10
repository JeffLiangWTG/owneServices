using System;
using System.ComponentModel;
using System.Web.Services;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	/// <summary>
	/// Summary description for WhsSecureService
	/// </summary>
	[WebService(Namespace = "http://cargowise.com/WarehouseRF/")]
	[ToolboxItem(false)]
	public partial class WhsSecureService : SecureService
	{
		// This file is left here to preserve TFS history and should remain empty.
		// Any new WhsSecureService methods should be added to the partial class in seperate files.

		#region ValidateWebServiceAndStaffAndWarehouse

#if DEBUG
		internal
#endif
		bool ValidateWebServiceAndStaffAndWarehouse(WebServiceResponse result, bool requireValidateConcurencyLogin = false)
		{
			var isOkToRun = AllowedToRunService(result, requireValidateConcurencyLogin);
			if (isOkToRun && WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode) == null)
			{
				result.Error = ErrorTypes.LoginFailed;
				result.ErrorMessage = Res.GetString("7f020eee-dca0-4421-9fa8-776f35c91f4f", "Please provide login credentials to use this service.");
			}

			return isOkToRun && result.Error == ErrorTypes.None;
		}

		#endregion

		protected T HandleWebServiceRequest_WithValidateWarehouseAndStaff<T>(Action<T> action)
			where T : WebServiceResponse, new()
		{
			return HandleWebServiceRequest(action, (T t) => ValidateWebServiceAndStaffAndWarehouse(t));
		}

		// Used to create Lazy Loaded fields that have access to other fields so that field initialisation
		// does not have to go into the constructor of this partial class but rather where it's being used
		class FieldHolder<T>
		{
			public FieldHolder(Func<WhsSecureService, T> getValue, Func<T, bool> shouldCache)
			{
				T value = default;
				GetValue = service => value = getValue(service);
				FieldCache = new Lazy<T>(() => value);
				ShouldCache = shouldCache ?? (_ => true);
			}

			readonly Lazy<T> FieldCache;
			readonly Func<WhsSecureService, T> GetValue;
			readonly Func<T, bool> ShouldCache;

			public T GetField(WhsSecureService webService)
			{
				T result;

				if (FieldCache.IsValueCreated)
				{
					result = FieldCache.Value;
				}
				else
				{
					var value = GetValue(webService);
					result = ShouldCache(value) ? FieldCache.Value : value;
				}

				return result;
			}
		}

		static FieldHolder<T> New<T>(Func<WhsSecureService, T> getValue) => New(getValue, null);
		static FieldHolder<T> New<T>(Func<WhsSecureService, T> getValue, Func<T, bool> shouldCache) => new FieldHolder<T>(getValue, shouldCache);
	}
}
