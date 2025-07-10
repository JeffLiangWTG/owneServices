using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Web;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using WTG.StaticAnalysis.Annotation;
using Res = Enterprise.Warehouse.Transactions.DataTransfer.Res;

[assembly: UsesConstants(typeof(DataContentTypes))]

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PrintCarrierLabelsProcessor : IProcessor
	{
		public PrintCarrierLabelsProcessor(WhsPick pick, ZGuid printerPK)
		{
			Pick = Argument.NotNull(pick, nameof(pick));
			PrinterPK = printerPK;
			docPackDocumentsToPrintByPackageCache = new Lazy<Dictionary<ZGuid, byte[]>>(GetDocPackCommandByPackage);
		}

		readonly WhsPick Pick;
		readonly ZGuid PrinterPK;

		BusinessObjectFactory Factory => Pick.Factory;

		string FailedToPrintCarrierLabelReason => Res.GetString("0289f173-1b6e-42c8-a7e7-f9b682a65b04", "Failed to print carrier label");

		static string DocumentsWithCarrierLabelMenuName => (NoResString)"Documents with Carrier Label";

		static string OtherDocumentsToPrintWithTheCarrierLabelContentType => DataContentTypes.Pdf;

		public event EventHandler PackageCarrierLabelsPrinted;

		void OnPackageCarrierLabelsPrinted()
		{
			PackageCarrierLabelsPrinted?.Invoke(this, EventArgs.Empty);
		}

		DocumentUtility DocumentUtilityHelper => documentUtilityHelper ?? (documentUtilityHelper = new DocumentUtility(Factory, requiresWebUser: false));
		DocumentUtility documentUtilityHelper;

		#region RTUS Single Processing

		public event EventHandler PackageCarrierLabelPrinted;
		public event EventHandler PackageCarrierLabelPrintFail;
		public event EventHandler PackageCarrierLabelPrintingStarted;

		void OnPackageCarrierLabelPrinted()
		{
			PackageCarrierLabelPrinted?.Invoke(this, EventArgs.Empty);
		}

		void OnPackageCarrierLabelPrintFail()
		{
			PackageCarrierLabelPrintFail?.Invoke(this, EventArgs.Empty);
		}

		void OnPackageCarrierLabelPrintingStarted()
		{
			PackageCarrierLabelPrintingStarted?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		public void Process(INotifications notifications, CancellationToken cancellationToken
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			var printer = (IStmPrintQueue)WhsCommonLookups.GetPrintersList(Factory).FindByPK(PrinterPK);
			if (printer != null)
			{
				if (CarrierLabelDocPackHasChildDocument() || !ObjectFactory.Get<ITransportRegistry>().EnableRTUSBatchProcessing.Value)
				{
					SingleProcessingPrintCarrierLabel(printer, cancellationToken);
				}
				else
				{
					BatchProcessingPrintCarrierLabels(cancellationToken);
				}
			}
			else
			{
				CreateErrorLog(Pick, Res.GetString("0a07f7ed-fd0d-4c60-bb31-98fc24cdfb77", "No printer is selected to print carrier labels."));
			}
		}

		DocumentCommand GetDocPackCommand(PkgPackage package)
		{
			var documentCommand = DocumentCommand.GetDocumentCommand(Factory, package, DocumentsWithCarrierLabelMenuName);
			if (documentCommand != null)
			{
				documentCommand.Parent = package;
			}
			return documentCommand;
		}

		public bool CarrierLabelDocPackHasChildDocument() => docPackDocumentsToPrintByPackageCache.Value.Count > 0;

		readonly Lazy<Dictionary<ZGuid, byte[]>> docPackDocumentsToPrintByPackageCache;

		Dictionary<ZGuid, byte[]> GetDocPackCommandByPackage()
		{
			var result = new Dictionary<ZGuid, byte[]>();
			foreach (var package in Pick.OuterPackages)
			{
				var documentCommand = GetDocPackCommand(package);
				if (documentCommand != null)
				{
					using (var printTask = new PrintTask(documentCommand))
					{
						var loader = new PrintTaskDocumentPackLoader(printTask, documentCommand, null);
						loader.LoadAll();

						var childDocuments = PackageLabelsHelper.GetChildDocumentsInDocPack(printTask, documentCommand);
						var hasChildDocuments = childDocuments.Any();
						if (hasChildDocuments)
						{
							var docToPrintInBytes = DocumentUtilityHelper.GetDocument(printTask.GetFirstDocumentPack(), null, OtherDocumentsToPrintWithTheCarrierLabelContentType);
							result[package.PK] = docToPrintInBytes;
						}
					}
				}
			}

			return result;
		}

		void BatchProcessingPrintCarrierLabels(CancellationToken cancellationToken)
		{
			var rtusOption = GetPackageRTUSOption();
			if (rtusOption != null)
			{
				PrintCarrierLabels(rtusOption, cancellationToken);
			}
		}

		IOrganisationRTUSOption GetPackageRTUSOption()
		{
			var carrierBookingAgent = GetCommonCarrierBookingAgent();
			IOrganisationRTUSOption rtusOption = null;

			if (carrierBookingAgent != null)
			{
				rtusOption = ObjectFactory.Get<ITransportRegistry>().GetRTUSOption(carrierBookingAgent.PK.ToGuid());
				if (rtusOption == null)
				{
					CreateErrorLog(Pick, Res.GetString("6212df62-b3ca-4e46-947a-e725c5368ed3", "No RTUS URL provided for Organization '{0}'.", carrierBookingAgent.OH_Code));
				}
			}

			return rtusOption;
		}

		OrgHeader GetCommonCarrierBookingAgent()
		{
			var packages = Pick.OuterPackages;
			OrgHeader referenceCarrierBookingAgent = null;

			var packageWithNoOrder = packages.FirstOrDefault(package => package.PackageJob?.ParentJob == null);
			if (packageWithNoOrder != null)
			{
				CreateErrorLog(Pick, Res.GetString("c550f05b-299d-4724-b779-24ec4c493cb9", "No warehouse order found for package '{0}'", packageWithNoOrder.KP_PackageID));
			}
			else
			{
				var carrierBookingAgents = packages.Select(package => package.PackageJob.ParentJob.CarrierBookingAgent);
				if (carrierBookingAgents.Any(cba => cba == null))
				{
					CreateErrorLog(Pick, Res.GetString("72b2840a-3f9f-4dc2-918a-e8cb2f0aa4b3", "Not all orders has a Carrier Booking Agent."));
				}
				else
				{
					var distinctCarrierBookingAgents = carrierBookingAgents.Distinct();
					if (distinctCarrierBookingAgents.IsCountMoreThan(1))
					{
						CreateErrorLog(Pick, Res.GetString("592749f7-f687-46e4-af0d-f1374097f738", "Not all orders have the same carrier booking agents."));
					}
					else
					{
						referenceCarrierBookingAgent = distinctCarrierBookingAgents.Single();
					}
				}
			}

			return referenceCarrierBookingAgent;
		}

		void PrintCarrierLabels(IOrganisationRTUSOption rtusOption, CancellationToken cancellationToken)
		{
			var errors = new HashSet<string>();
			OnError = ex => errors.Add(ex.Message);

			using (var carrierLabelManager = ObjectFactory.New<ICarrierLabelManager>(null, null))
			using (var carrierLabelsBatchPrintingProvider = ObjectFactory.Get<ICarrierLabelsBatchPrintingProvider>(nameof(ICarrierLabelsBatchPrintingProvider), carrierLabelManager, OnError))
			{
				Argument.NotNull(carrierLabelsBatchPrintingProvider, nameof(carrierLabelsBatchPrintingProvider));

				if (carrierLabelsBatchPrintingProvider.IsRemotePrintingConnectionDetailsProvided)
				{
					PrintCarrierLabelsCore(carrierLabelsBatchPrintingProvider, rtusOption, cancellationToken);
				}
				else
				{
					CreateErrorLog(Pick, carrierLabelsBatchPrintingProvider.PrintCarrierLabels(WhsCarrierLabelsBatchPrintingInfo.GetEmptyWhsCarrierLabelsBatchPrintingInfo(), rtusOption.RTUSCBA, null, ZGuid.Empty, cancellationToken).Message);
				}
			}

			foreach (var error in errors)
			{
				CreateErrorLog(Pick, error);
			}
		}

#if DEBUG
		public
#endif
		Action<Exception> OnError;

		void PrintCarrierLabelsCore(ICarrierLabelsBatchPrintingProvider carrierLabelsBatchPrintingProvider, IOrganisationRTUSOption rtusOption, CancellationToken cancellationToken)
		{
			var sorter = new PackageComparerForLabelPrinting(Pick);
			var orderedPackages = Pick.OuterPackages.OrderBy(p => p, sorter);

			var (labelSeparatorsItemNumbers, labelSeparatorToPrintInfos) = GetSeparatorLabelToPrintInfos(sorter, orderedPackages);
			var packageJobToOrderMatch = GetPackageJobToPackingParentMatches(orderedPackages);
			var packingParentToPackageItemNumbers = GetOrderToPackageItemNumbers(orderedPackages, packageJobToOrderMatch);

			var subscriber = ObjectFactory.Get<IWhsCarrierLabelsBatchPrintingSubscriber>(nameof(IWhsCarrierLabelsBatchPrintingSubscriber));
			var subscribeResult = subscriber.SubscribePackages(Pick, packingParentToPackageItemNumbers, labelSeparatorsItemNumbers);
			if (subscribeResult.Success)
			{
				var packageToPackingParentInfos = orderedPackages.Select(package => new PackageToPackingParentInfo(packageJobToOrderMatch[package.KP_KJ_ParentPackageJob], package, ((WhsOrder)packageJobToOrderMatch[package.KP_KJ_ParentPackageJob]).WD_DocketID));
				var whsCarrierLabelsBatchPrintingInfo = new WhsCarrierLabelsBatchPrintingInfo(subscriber.GetCarrierLabelsBatchRequests, labelSeparatorToPrintInfos, packageToPackingParentInfos);
				var result = carrierLabelsBatchPrintingProvider.PrintCarrierLabels(whsCarrierLabelsBatchPrintingInfo, rtusOption.RTUSCBA, rtusOption.WrappedUrl, PrinterPK, cancellationToken);
				if (result.Success)
				{
					OnPackageCarrierLabelsPrinted();
				}
				else
				{
					CreateErrorLog(Pick, result.Message);
				}
			}
			else
			{
				CreateErrorLog(Pick, Res.GetString("60d0ab4b-f71f-40b0-b31a-ce369dbdc126", "Failed to subscribe packages to RTUS batch booking due to: '{0}'.", subscribeResult.Message));
			}
		}

		static Dictionary<ZGuid, IPackingParent> GetPackageJobToPackingParentMatches(IEnumerable<PkgPackage> packages)
		{
			var result = new Dictionary<ZGuid, IPackingParent>();
			foreach (var package in packages)
			{
				if (!result.ContainsKey(package.KP_KJ_ParentPackageJob))
				{
					var packageParentOrder = package.PackageJob?.ParentJob;
					if (packageParentOrder != null)
					{
						result.Add(package.KP_KJ_ParentPackageJob, packageParentOrder);
					}
				}
			}

			return result;
		}

		static (IReadOnlyCollection<int> labelSeparatorItemNumbers, IEnumerable<KeyValuePair<ZGuid, DocumentCommandCollection>> labelSeparatorInfos) GetSeparatorLabelToPrintInfos(PackageComparerForLabelPrinting sorter, IEnumerable<PkgPackage> packages)
		{
			var labelSeparatorInfos = new List<KeyValuePair<ZGuid, DocumentCommandCollection>>();
			var labelSeparatorItemNumbers = new List<int>();

			var packageList = packages.ToList();
			var numberOfPackages = packageList.Count;
			var packageCounter = 0;

			for (var index = 0; index < numberOfPackages; index++)
			{
				packageCounter++;
				var package = packageList[index];
				var nextPackage = index + 1 < numberOfPackages ? packageList[index + 1] : null;
				var delimeterType = sorter.GetDelimeterTypeBetweenPackages(package, nextPackage);

				if (!delimeterType.Equals(PackageComparerForLabelPrinting.DocDelimeterType.None))
				{
					package.PackageJob.Selected.UpdateSelectedPackages(new[] { package });
					var splitterDocList = new DocumentCommandCollection(package);
					PackageLabelsHelper.AddDelimeterDocumentsIfNeeded(delimeterType, splitterDocList);

					labelSeparatorInfos.Add(new KeyValuePair<ZGuid, DocumentCommandCollection>(package.PK, splitterDocList));
					if (numberOfPackages != packageCounter)
					{
						labelSeparatorItemNumbers.Add(packageCounter);
					}
				}
			}

			return (labelSeparatorItemNumbers, labelSeparatorInfos);
		}

		static IEnumerable<WhsOrderToPackageItemNumbers> GetOrderToPackageItemNumbers(IEnumerable<PkgPackage> packages, Dictionary<ZGuid, IPackingParent> packageJobToOrderMatch)
		{
			var orderToPackageItemNumbers = new List<WhsOrderToPackageItemNumbers>();
			var packageList = packages.ToList();
			var numberOfPackages = packageList.Count;
			var packageCounter = 0;

			var enumerator = packages.GetEnumerator();
			while (enumerator.MoveNext())
			{
				packageCounter++;
				var package = enumerator.Current;

				var order = packageJobToOrderMatch[package.KP_KJ_ParentPackageJob];
				var orderToPackageItemNumber = orderToPackageItemNumbers.FirstOrDefault(pkgItemNumber => pkgItemNumber.Order.JobNo == order.JobNo);
				if (orderToPackageItemNumber == null)
				{
					orderToPackageItemNumber = new WhsOrderToPackageItemNumbers(order);
					orderToPackageItemNumbers.Add(orderToPackageItemNumber);
				}
				orderToPackageItemNumber.PackageItemNumbers.Add(new KeyValuePair<PkgPackage, int>(package, packageCounter));
			}

			return orderToPackageItemNumbers;
		}

		void CreateErrorLog(WhsPick pick, string reference)
		{
			var typeParameter = new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.BulkCarrierLabelBooking);
			var reasonParameter = new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, FailedToPrintCarrierLabelReason);
			pick.Logs.CreateOrRecreateEventLog(Events.ErrorReport, EstimateActual.Actual, ZDateTimeOffset.Now, reference, typeParameter, reasonParameter);
		}

		#region RTUS Single Processing

		Dictionary<ZGuid, IOrganisationRTUSOption> GetPackageRTUSOptions(IEnumerable<PkgPackage> packages)
		{
			var parentJobRTUSOptionsCache = new Dictionary<ZGuid, IOrganisationRTUSOption>();
			var packageRTUSOptionsDictionary = new Dictionary<ZGuid, IOrganisationRTUSOption>();

			foreach (var package in packages)
			{
				var parentJob = package.PackageJob?.ParentJob;
				if (parentJob != null)
				{
					if (!parentJobRTUSOptionsCache.TryGetValue(parentJob.PK, out var rtusOption))
					{
						rtusOption = GetRTUSOptionFromParentJob(parentJob);
						parentJobRTUSOptionsCache.Add(parentJob.PK, rtusOption);
					}

					if (rtusOption != null)
					{
						packageRTUSOptionsDictionary.Add(package.PK, rtusOption);
					}
				}
				else
				{
					CreateErrorLog(Pick, Res.GetString("c550f05b-299d-4724-b779-24ec4c493cb9", "No warehouse order found for package '{0}'", package.KP_PackageID));
				}
			}

			return packageRTUSOptionsDictionary;
		}

		IOrganisationRTUSOption GetRTUSOptionFromParentJob(IPackingParent parentJob)
		{
			IOrganisationRTUSOption rtusOption = null;

			var carrierBookingAgent = parentJob.CarrierBookingAgent;
			if (carrierBookingAgent != null)
			{
				rtusOption = ObjectFactory.Get<ITransportRegistry>().GetRTUSOption(carrierBookingAgent.PK.ToGuid());
				if (rtusOption == null)
				{
					CreateErrorLog(Pick, Res.GetString("afe45a80-7794-4c2a-adf0-9dcb435730d3", "Order '{0}' - No RTUS URL provided for Organization '{1}'.", parentJob.JobNo, carrierBookingAgent.OH_Code));
				}
			}
			else
			{
				CreateErrorLog(Pick, Res.GetString("02f8ceb5-a736-422d-ad0a-0cb7f1fe4065", "Order '{0}' has no Carrier Booking Agent.", parentJob.JobNo));
			}

			return rtusOption;
		}

		void SingleProcessingPrintCarrierLabel(IStmPrintQueue printer, CancellationToken cancellationToken)
		{
			var packages = Pick.OuterPackages;
			var rtusOptionsDictionary = GetPackageRTUSOptions(packages);

			if (rtusOptionsDictionary.Count == packages.Count)
			{
				PrintCarrierLabels(packages, printer, rtusOptionsDictionary, cancellationToken);
			}
		}

		void PrintCarrierLabels(
			IEnumerable<PkgPackage> packages,
			IStmPrintQueue printer,
			Dictionary<ZGuid, IOrganisationRTUSOption> packageRTUSOptionsDictionary,
			CancellationToken cancellationToken)
		{
			var errors = new HashSet<string>();
			OnError = ex => errors.Add(ex.Message);

			using (var carrierLabelManager = ObjectFactory.New<ICarrierLabelManager>(null, null))
			using (var carrierLabelProvider = ObjectFactory.Get<ICarrierLabelPrintingProvider>(nameof(ICarrierLabelPrintingProvider), OnError, carrierLabelManager, null))
			{
				Argument.NotNull(carrierLabelProvider, nameof(carrierLabelProvider));

				if (carrierLabelProvider.IsRemotePrintingConnectionDetailsProvided)
				{
					PrintCarrierLabelsCore(packages, printer, packageRTUSOptionsDictionary, carrierLabelProvider, cancellationToken);
				}
				else
				{
					CreateErrorLog(Pick, carrierLabelProvider.PrintCarrierLabel(null, 0, null, null).Message);
				}
			}

			foreach (var error in errors)
			{
				CreateErrorLog(Pick, error);
			}
		}

		void PrintCarrierLabelsCore(
			IEnumerable<PkgPackage> packages,
			IStmPrintQueue printer,
			Dictionary<ZGuid, IOrganisationRTUSOption> packageRTUSOptionsDictionary,
			ICarrierLabelPrintingProvider carrierLabelProvider,
			CancellationToken cancellationToken)
		{
			OnPackageCarrierLabelPrintingStarted();
			var notAllPackageLabelsArePrinted = false;
			var sortedPackageLabelsPrinted = 0;
			var sorter = new PackageComparerForLabelPrinting(Pick);
			var packagesQueue = new Queue<PkgPackage>(packages.Where(p => p.PackedItems.Count > 0).OrderBy(p => p, sorter));

			while (packagesQueue.Count > 0)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var package = packagesQueue.Dequeue();
				var rtusOption = packageRTUSOptionsDictionary[package.PK];

				var result = carrierLabelProvider.PrintCarrierLabel(package, rtusOption.RTUSCBA, rtusOption.WrappedUrl, printer);
				if (result.Success)
				{
					PrintDocPack(carrierLabelProvider, printer, package);
					sortedPackageLabelsPrinted++;
					OnPackageCarrierLabelPrinted();
				}
				else
				{
					var errorMessage = string.IsNullOrEmpty(result.Message)
						? Res.GetString("1cf1aec9-40ff-4582-91d9-87d4f8f3aaf7", "Print Request for Package '{0}' failed.", package.KP_PackageID)
						: result.Message;

					AddCarrierLabelPrintingFailureNote(package, errorMessage);
					notAllPackageLabelsArePrinted = true;
					OnPackageCarrierLabelPrintFail();
				}

				if (sortedPackageLabelsPrinted > 0)
				{
					var nextPackage = packagesQueue.Count > 0 ? packagesQueue.Peek() : null;
					var delimeterType = sorter.GetDelimeterTypeBetweenPackages(package, nextPackage);

					if (PrintLabelBreaks(carrierLabelProvider, printer, delimeterType, package))
					{
						sortedPackageLabelsPrinted = 0;
					}
				}
			}

			if (notAllPackageLabelsArePrinted)
			{
				CreateErrorLog(Pick, Res.GetString("5fc92d21-88ab-4f7a-b3ec-fef6e413674c", "Not all Packages were successfully booked with a Carrier and had their Label Printed, see Notes on the relevant Orders for details."));
			}
		}

		void PrintDocPack(ICarrierLabelPrintingProvider provider, IStmPrintQueue printer, PkgPackage package)
		{
			if (docPackDocumentsToPrintByPackageCache.Value.TryGetValue(package.PK, out var docToPrintInBytes))
			{
				if (!PrintDocument(provider, printer, docToPrintInBytes))
				{
					CreateErrorLog(Pick, Res.GetString("3f7e3f6f-6c89-4360-9bc0-7cba62a82966", "Failed to print documents from the '{0}' document pack for package '{1}'.", DocumentsWithCarrierLabelMenuName, package.KP_PackageID));
				}
			}
		}

		bool PrintLabelBreaks(ICarrierLabelPrintingProvider provider, IStmPrintQueue printer, PackageComparerForLabelPrinting.DocDelimeterType delimeterType, PkgPackage package)
		{
			package.PackageJob.Selected.UpdateSelectedPackages([package]); // required so the correct document wrapper will be created, copied from PackageLabelsHelper

			var splitterDocList = new DocumentCommandCollection(package);
			PackageLabelsHelper.AddDelimeterDocumentsIfNeeded(delimeterType, splitterDocList);

			foreach (var doc in splitterDocList.Cast<DocumentCommand>())
			{
				if (!PrintDocument(provider, printer, DocumentUtilityHelper.GetDocument(package, doc.SU_MenuName, null, OtherDocumentsToPrintWithTheCarrierLabelContentType)))
				{
					CreateErrorLog(Pick, Res.GetString("c646f95f-779c-427e-9cc6-aa3024ee57d0", "Failed to print label break '{0}' after package '{1}'.", doc.SU_MenuName, package.KP_PackageID));
				}
			}

			return splitterDocList.Count > 0;
		}

		static bool PrintDocument(ICarrierLabelPrintingProvider provider, IStmPrintQueue printer, byte[] docToPrintInBytes)
			=> docToPrintInBytes.Length > 0 && provider.Print(WTG.RTUS.Interface.FileType.PDF, docToPrintInBytes, printer);

		static void AddCarrierLabelPrintingFailureNote(PkgPackage package, string errorMessage)
		{
			var noteParent = (IStmNoteParent)package.PackageJob.ParentJob;
			var noteText = Res.GetString("a24f4c0d-0f92-41de-ae90-5d5bd54fb4c4", "Carrier label printing failed for package '{0}'. Error: {1}", package.KP_PackageID, errorMessage);
			noteParent.Notes.AddNew(false, PredefinedNoteTypes.Instance.RTUSRequestLog.Description, noteText);
		}

		#endregion
	}
}
