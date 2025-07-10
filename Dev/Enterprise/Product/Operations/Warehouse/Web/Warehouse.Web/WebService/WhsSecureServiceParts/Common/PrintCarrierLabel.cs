using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Print Carrier Label")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public NewPackageWebServiceResponse PrintCarrierLabel(PackageInfo package, Guid printerPK)
		{
			return HandleWebServiceRequest<NewPackageWebServiceResponse>(r => PrintCarrierLabelCore(r, package, printerPK));
		}

		void PrintCarrierLabelCore(NewPackageWebServiceResponse response, PackageInfo packageInfo, Guid printerPK)
		{
			var package = GetPackage(response, packageInfo);
			if (package != null)
			{
				using (var carrierLabelPrintingProviderManager = new RFCarrierLabelPrintingProviderManager(this))
				{
					var result = package.PrintCarrierLabel(printerPK, carrierLabelPrintingProviderManager);
					if (package.HasChanges)
					{
						package.Factory.Save();
					}

					response.NewPackage = new PackageInfo { PackageID = package.KP_PackageID, PK = package.PK.ToGuid() };

					if (!result.Success)
					{
						response.LogError(ErrorTypes.BusinessValidationError, result.Message);
					}
				}
			}
		}

		PkgPackage GetPackage(WebServiceResponse response, PackageInfo packageInfo)
		{
			PkgPackage package = null;

			if (packageInfo == null)
			{
				response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("52bb5741-eebd-4c28-8158-032563298127", "No Package/Order information provided."));
			}
			else
			{
				package = Factory.Load<PkgPackage>(packageInfo.PK);
				if (package == null)
				{
					response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("e976fbaa-0933-4270-992b-67a9d9d804aa", "Package ID '{0}' does not exist.", packageInfo.PackageID.ToUpperInvariant()));
				}
			}

			return package;
		}

		#region CarrierLabelPrintingProvider

		FieldHolder<ICarrierLabelPrintingProvider> CarrierLabelPrintingProviderCache { get; } = New(GetCarrierLabelPrintingProvider, p => p.IsRemotePrintingConnectionDetailsProvided);

		ICarrierLabelPrintingProvider CarrierLabelPrintingProvider => CarrierLabelPrintingProviderCache.GetField(this);

		static ICarrierLabelPrintingProvider GetCarrierLabelPrintingProvider(WhsSecureService secureService)
		{
			var carrierLabelManager = ObjectFactory.New<ICarrierLabelManager>(GetActionInvoker(), null);
			var provider = ObjectFactory.Get<ICarrierLabelPrintingProvider>(nameof(ICarrierLabelPrintingProvider), GetActionInvoker(), carrierLabelManager, null);
			if (provider.IsRemotePrintingConnectionDetailsProvided)
			{
				secureService.AddDisposableToDisposeOnDispose(provider);
			}
			else
			{
				// Need to dispose this instance since it won't be disposed otherwise.
				// A new Manager will be created on the next Printing Provider creation.
				carrierLabelManager.Dispose();
			}

			return provider;

			Action<Exception> GetActionInvoker() => ex => secureService.CarrierLabelPrintingError?.Invoke(ex);
		}

		event Action<Exception> CarrierLabelPrintingError;

		IDisposable SubScribeCarrierLabelPrintingError(Action<Exception> onException)
		{
			return new DisposableAction(
					() => CarrierLabelPrintingError += onException,
					() => CarrierLabelPrintingError -= onException);
		}

		#endregion

		#region RFCarrierLabelPrintingProviderManager

		class RFCarrierLabelPrintingProviderManager : ICarrierLabelPrintingProviderManager
		{
			public RFCarrierLabelPrintingProviderManager(WhsSecureService service)
			{
				Service = service;

				IDisposable eventSubscriptionToDispose = null;
				GetEventSubscriptionToDispose = action =>
				{
					eventSubscriptionToDispose = Service.SubScribeCarrierLabelPrintingError(action);
					return EventSubscriptionToDispose.Value;
				};
				EventSubscriptionToDispose = new Lazy<IDisposable>(() => eventSubscriptionToDispose);
			}

			readonly Lazy<IDisposable> EventSubscriptionToDispose;
			readonly Func<Action<Exception>, IDisposable> GetEventSubscriptionToDispose;

			WhsSecureService Service { get; }

			public void Dispose()
			{
				if (EventSubscriptionToDispose.IsValueCreated)
				{
					EventSubscriptionToDispose.Value.Dispose();
				}
			}

			public ICarrierLabelPrintingProvider GetCarrierLabelPrintingProvider(Action<Exception> action)
			{
				if (!EventSubscriptionToDispose.IsValueCreated)
				{
					GetEventSubscriptionToDispose(action);
				}

				return Service.CarrierLabelPrintingProvider;
			}

			public bool IsParentJobValid(IPackingParent packingParent) => packingParent is WhsOrder || packingParent is PkgHandlingUnit;

			public string GetInvalidParentJobTypeErrorMessage(PkgPackage package)
				=> Res.GetString("374fe98d-2160-4766-8ca7-968bc3ef6ceb", "Package '{0}' must be a Packing Consolidation Handling Unit or attached to an Order.", package.KP_PackageID);
		}

		#endregion
	}
}
