using System;
using System.Linq;
using System.Reflection;
using System.Web.Services;
using CargoWise.EntityFramework.Testing;
using Enterprise.Tracking.Web.WebService;

namespace Enterprise.Tracking.Web.Testing
{
	public abstract class WebServiceWithFactoryTest<T> : TestCaseWithFactory
			where T : WebServiceWithFactory, new()
	{
		#region Test Cases

		public void TestAllPublicMethodsUseExecuteAndDisposeDbConnection()
		{
			var methods = WebService.GetType()
				.GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.Where(m => m.GetCustomAttributes<WebMethodAttribute>(true).Any());

			foreach (var method in methods)
			{
				var methodName = method.Name;
				var parameters = method.GetParameters().Select(p => p.HasDefaultValue ? p.DefaultValue : null).ToArray();
				WebService.ExecuteAndDisposeHasBeenCalled = false;

				try
				{
					method.Invoke(WebService, parameters);
				}
				catch (Exception) { }

				Assert($"Method {methodName} should be wrapped with ExecuteAndDispose", WebService.ExecuteAndDisposeHasBeenCalled);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			WebService = GetNewWebService();
		}

		protected virtual T GetNewWebService()
		{
			return new T();
		}

		protected T WebService { get; private set; }

		#endregion
	}
}
