using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Web;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using WTG.RTUS.Interface;
using WTG.StaticAnalysis.Annotation;
using FileType = WTG.RTUS.Interface.FileType;

[assembly: UsesConstants(typeof(DataContentTypes))]

namespace Enterprise.Packing.DataTransfer
{
	public sealed class CarrierLabelsBatchPrintingProvider : ICarrierLabelsBatchPrintingProvider
	{
		CarrierLabelsBatchPrintingProvider(Action<Exception> onError, ICarrierLabelManager carrierLabelManager)
		{
			CarrierLabelManager = carrierLabelManager;
			OnError = onError;

			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		ICarrierLabelManager CarrierLabelManager { get; }
		public bool IsRemotePrintingConnectionDetailsProvided => true;
		Action<Exception> OnError { get; }

		public static ICarrierLabelsBatchPrintingProvider GetProvider(ICarrierLabelManager carrierLabelManager, Action<Exception> onError)
		{
			Argument.NotNull(carrierLabelManager, nameof(carrierLabelManager));

			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			var remotePrintingServerRaw = transportRegistry.RemotePrintServerURL.Value;
			var remotePrintingUserName = WebDataRegistry.Instance.WebServiceUsername.Value;
			var remotePrintingPassword = WebDataRegistry.Instance.WebServicePassword.Value;

			var config = RTUSRemotePrintingHelper.GetRemotePrintServerConfig(remotePrintingServerRaw, remotePrintingUserName, remotePrintingPassword);
			return config != null
				? new CarrierLabelsBatchPrintingProvider(onError, carrierLabelManager)
				: new EmptyCarrierLabelsBatchPrintingProvider(remotePrintingServerRaw, remotePrintingUserName, remotePrintingPassword);
		}

		ReturnResult ICarrierLabelsBatchPrintingProvider.PrintCarrierLabels(ICarrierLabelsBatchPrintingInfo carrierLabelsBatchPrintingInfo, RTUSCBA type, Uri url, ZGuid printerPK, CancellationToken cancellationToken)
		{
			Argument.NotNull(carrierLabelsBatchPrintingInfo, nameof(carrierLabelsBatchPrintingInfo));

			var errorMessage = string.Empty;
			var documentsToPrint = new List<(byte[], FileType)>();

			foreach (var request in carrierLabelsBatchPrintingInfo.PackagesBatchUniversalShipments)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var rtusBatchResponse = CarrierLabelManager.PushCarrierLabelRequest(RequestType.BatchBooking, request, type, url);
				if (rtusBatchResponse.IsSuccessful)
				{
					documentsToPrint.Add((rtusBatchResponse.BinaryData, rtusBatchResponse.FileType));
					errorMessage = ProcessBookingResponseAndAddCustomDocsToPrint(documentsToPrint, rtusBatchResponse, carrierLabelsBatchPrintingInfo.CustomSegments, carrierLabelsBatchPrintingInfo.PackageToPackingParentInfos);

					if (!errorMessage.IsNullOrEmpty())
					{
						break;
					}
				}
				else
				{
					errorMessage = rtusBatchResponse.ErrorMessageForFailure;
					break;
				}
			}

			var success = errorMessage.IsNullOrEmpty() && Print(documentsToPrint, printerPK);
			return new ReturnResult { Success = success, Message = errorMessage };
		}

		string ProcessBookingResponseAndAddCustomDocsToPrint(List<(byte[], FileType)> docsToPrint, IBatchBookingRTUSResponse rtusBatchResponse, IEnumerable<KeyValuePair<ZGuid, DocumentCommandCollection>> customSegments, IEnumerable<PackageToPackingParentInfo> packageToPackingParentInfos)
		{
			var errorMessage = packageToPackingParentInfos != null && rtusBatchResponse.ReferenceNumbers.Any()
				? string.Empty
				: PackageToPackingParentMismatchError;

			if (errorMessage.IsNullOrEmpty())
			{
				var packageBookedTime = ZDateTime.Now;
				foreach (var referenceNumbers in rtusBatchResponse.ReferenceNumbers)
				{
					var packageToPackingParentInfo = packageToPackingParentInfos
						.Where(packageToPackingParentInfo => packageToPackingParentInfo.Package.KP_PackageID.EqualsIgnoringCase(referenceNumbers.RequestPackageID))
						.FirstOrDefault(packageToPackingParentInfo => packageToPackingParentInfo.PackingParentId.EqualsIgnoringCase(referenceNumbers.ParentID));

					if (packageToPackingParentInfo != null)
					{
						var package = packageToPackingParentInfo.Package;
						var packingParent = packageToPackingParentInfo.PackingParent;
						if (RTUSRemotePrintingHelper.IsValidRTUSResponse(package.PackageJob, package.KP_PackageID, packingParent.TransportReference, referenceNumbers))
						{
							RTUSRemotePrintingHelper.UpdatePackageWithRTUSResponse(package, packingParent, referenceNumbers.TrackingNumber, referenceNumbers.TransportReference, packageBookedTime, referenceNumbers.OrderType);
							docsToPrint.AddRange(GetCustomSegmentsToPrint(package, GetCustomSegmentsToPrintForPackage(customSegments, package)));
						}
						else
						{
							errorMessage = RTUSRemotePrintingHelper.PackageJobFinalizedError;
							break;
						}
					}
					else
					{
						errorMessage = PackageToPackingParentMismatchError;
						break;
					}
				}
			}

			return errorMessage;
		}

		static string PackageToPackingParentMismatchError => Res.GetString("7592f358-e0e4-4e4b-a46f-a6e886ecbfeb", "Package and Packing Parent information mismatch between RTUS response and request.");

		static DocumentCommandCollection GetCustomSegmentsToPrintForPackage(IEnumerable<KeyValuePair<ZGuid, DocumentCommandCollection>> customSegments, PkgPackage referencePackage)
		{
			DocumentCommandCollection documentCommandCollection = null;
			if (customSegments != null)
			{
				var customSegmentInfos = customSegments.Where(customSegment => customSegment.Key == referencePackage.PK);
				if (customSegmentInfos.Any())
				{
					documentCommandCollection = customSegmentInfos.Single().Value;
				}
			}

			return documentCommandCollection ?? new DocumentCommandCollection(referencePackage);
		}

		static IEnumerable<(byte[], FileType)> GetCustomSegmentsToPrint(PkgPackage package, DocumentCommandCollection documentCommands)
		{
			var factory = package.Factory;
			var documentUtility = new DocumentUtility(factory, requiresWebUser: false);

			foreach (DocumentCommand doc in documentCommands)
			{
				yield return (documentUtility.GetDocument(package, doc.SU_MenuName, null, DataContentTypes.Pdf), FileType.PDF);
			}
		}

		bool Print(IEnumerable<(byte[] BinaryData, FileType FileType)> docsToPrint, ZGuid printerPK)
		{
			var result = true;
			if (docsToPrint.Any())
			{
				var factory = new BusinessObjectFactory();
				var deliveryGroup = factory.New<IStmDeliveryGroup>();
				deliveryGroup.SB_IsProcessed = true;
				var sequence = 1;

				foreach (var docToPrint in docsToPrint)
				{
					var printJob = factory.New<StmPrintJob>();
					printJob.SP_Copies = 1;
					printJob.SP_CustomProperties = docToPrint.BinaryData;
					printJob.SP_EmailAttachments = $"Label{sequence}.{docToPrint.FileType}"; // File name
					printJob.SP_JobType = "PRN";
					printJob.SP_RunDateTime = ZDateTime.UtcNow;
					printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;
					printJob.SP_SQ = printerPK;
					printJob.SP_Sequence = sequence;
					sequence++;
				}

				try
				{
					ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
				}
				catch (Exception ex)
				{
					OnError(ex);
					result = false;
				}
			}

			return result;
		}

		public void Dispose()
		{
			CarrierLabelManager.Dispose();
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}
	}
}
