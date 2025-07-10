using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Core;
using WTG.RTUS.Interface;
using WTG.RTUS.Printing;

namespace Enterprise.Packing.DataTransfer
{
	public sealed class CarrierLabelPrintingProvider : ICarrierLabelPrintingProvider
	{
		delegate bool PrintDelegate(IRTUSPrinter printer, FileType fileType, byte[] binaryData, IStmPrintQueue printQueue);

		CarrierLabelPrintingProvider(ICarrierLabelManager carrierLabelManager, IRTUSPrinter printer, PrintDelegate printMethod, IHubConnectionFactory hubConnectionFactory, RemotePrintServerConfig config, Action<Exception> onError)
		{
			CarrierLabelManager = carrierLabelManager;
			Printer = printer ?? GetPrinter(hubConnectionFactory, config, onError);
			PrintMethod = printMethod ?? DefaultPrintMethod;
			RTUSPackageSubscriber = new RTUSPackageSubscriber();

			DisposableLeakListener.Instance.RegisterDisposable(this);

			bool DefaultPrintMethod(IRTUSPrinter p, FileType fileType, byte[] binaryData, IStmPrintQueue printQueue) => p.Print(fileType, binaryData, printQueue.QueueName, printQueue.SQ_ServerName);
		}

		static IRTUSPrinter GetPrinter(IHubConnectionFactory hubConnectionFactory, RemotePrintServerConfig config, Action<Exception> onError)
		{
			return hubConnectionFactory != null
				? ObjectFactory.Get<IRTUSPrinter>(nameof(IRTUSPrinter), hubConnectionFactory, config, null, null, onError, null, null)
				: ObjectFactory.Get<IRTUSPrinter>(nameof(IRTUSPrinter), config, null, null, onError, null, null);
		}

		ICarrierLabelManager CarrierLabelManager { get; }
		IRTUSPrinter Printer { get; }
		PrintDelegate PrintMethod { get; }
		RTUSPackageSubscriber RTUSPackageSubscriber { get; }

		public static ICarrierLabelPrintingProvider GetProvider(Action<Exception> onException, ICarrierLabelManager carrierLabelManager, IHubConnectionFactory hubConnectionFactory)
		{
			Argument.NotNull(carrierLabelManager, nameof(carrierLabelManager));

			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			var remotePrintingServerRaw = transportRegistry.RemotePrintServerURL.Value;
			var remotePrintingUserName = WebDataRegistry.Instance.WebServiceUsername.Value;
			var remotePrintingPassword = WebDataRegistry.Instance.WebServicePassword.Value;

			var config = RTUSRemotePrintingHelper.GetRemotePrintServerConfig(remotePrintingServerRaw, remotePrintingUserName, remotePrintingPassword);

			var useStandardRemotePrinting = transportRegistry.UseStandardRemotePrintingForRTUS.Value;
			var printer = useStandardRemotePrinting ? new RTUSPrinterWithoutSignalR(onException) : null;
			var printMethod = useStandardRemotePrinting ? Print : default(PrintDelegate);

			return config != null
				? new CarrierLabelPrintingProvider(carrierLabelManager, printer, printMethod, hubConnectionFactory, config, onException)
				: new EmptyCarrierLabelPrintingProvider(remotePrintingServerRaw, remotePrintingUserName, remotePrintingPassword);

			bool Print(IRTUSPrinter p, FileType fileType, byte[] binaryData, IStmPrintQueue printQueue) => p.Print(fileType, binaryData, printQueue.PK.ToGuid());
		}

		bool ICarrierLabelPrintingProvider.IsRemotePrintingConnectionDetailsProvided => true;

		ReturnResult ICarrierLabelPrintingProvider.PrintCarrierLabel(PkgPackage package, RTUSCBA type, Uri url, IStmPrintQueue printer)
		{
			Argument.NotNull(package, nameof(package));
			Argument.NotNull(printer, nameof(printer));

			var result = RTUSPackageSubscriber.SubscribePackage(package, RequestType.Booking);

			if (result.Success)
			{
				var requestXML = RTUSPackageSubscriber.GetRequestXML(package);
				var response = CarrierLabelManager.PushCarrierLabelRequest(RequestType.Booking, requestXML.ShipmentDataObject, type, url);

				var packageJob = package.PackageJob;
				var parentJob = packageJob.ParentJob;

				var isValidResponse = RTUSRemotePrintingHelper.IsValidRTUSResponse(packageJob, package.KP_PackageID, parentJob.TransportReference, response);
				if (response.IsSuccessful && isValidResponse)
				{
					RTUSRemotePrintingHelper.UpdatePackageWithRTUSResponse(package, parentJob, response.TrackingNumber, response.TransportReference, ZDateTime.Now, response.OrderType);
				}

				var success = response.IsSuccessful && isValidResponse && PrintMethod(Printer, response.FileType, response.BinaryData, printer);

				if (success)
				{
					package.RTUSLabelPrinterPK = printer.PK;
				}

				var message = isValidResponse
					? response.ErrorMessageForFailure
					: RTUSRemotePrintingHelper.PackageJobFinalizedError;
				result = new ReturnResult { Success = success, Message = message };
			}

			return result;
		}

		bool ICarrierLabelPrintingProvider.Print(FileType fileType, byte[] binaryData, IStmPrintQueue printer)
		{
			return PrintMethod(Printer, fileType, binaryData, printer);
		}

		public void Dispose()
		{
			CarrierLabelManager.Dispose();
			Printer.Dispose();

			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}
	}
}
