using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Integration;
using Enterprise.DocumentEngine.Shared;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Packing.Business
{
	public class PackageLabelAutoPrinter
	{
		public PackageLabelAutoPrinter(PkgPackageJob packageJob)
		{
			PackageJob = Argument.NotNull(packageJob, "packageJob");
		}

		readonly PkgPackageJob PackageJob;

		#region PrintDocument

		public void PrintDocument(PkgPackage package, Guid printerPK, int numberOfLabelsToPrint, IStmMenuItem documentToPrint = null)
		{
			// ensure package is ok
			Argument.NotNull(package, "package");

			if (package.Factory.Load<IStmPrintQueue>(printerPK) != null)
			{
				DocumentToPrint = documentToPrint;
				if (HasDocumentsToPrint(package))
				{
					AutoPrint(package, DocumentToPrint, printerPK, numberOfLabelsToPrint);
				}
			}
			else
			{
				OnPrintFailed(Res.GetString("9b1b71a8-15e2-4c96-886c-1fcf766fe201", "No valid Printer provided."), canContinueWithManualPrint: false);
			}
		}

		public void PrintDocument(PkgPackage package)
		{
			if (HasDocumentsToPrint(package))
			{
				OnAutoPrinting(package);
				PrintDocuments(package);
				OnAutoPrinted();
			}
		}

		bool HasDocumentsToPrint(PkgPackage package)
		{
			// ensure package is ok
			Argument.NotNull(package, "package");
			if (package.KP_KJ_ParentPackageJob != PackageJob.PK)
			{
				throw new ArgumentException("Package is not a child of the PackageLabelAutoPrinter's PackageJob.");
			}

			// printing
			if (DocumentToPrint == null)  // cached for performance. if the user changes the doc selection this cache will be old (they would need to reopen the form).
			{
				DocumentToPrint = GetDocumentToPrintWithRegistryFallback(package);
			}

			bool result = DocumentToPrint == null;
			if (result)
			{
				OnPrintFailed(NoDocumentToPrintError, canContinueWithManualPrint: false);
			}

			return !result;
		}

		void PrintDocuments(PkgPackage package)
		{
			var defaultPrinter = StmDefaultPrinter.LoadDefaultPrinter(PackageJob.Factory, GlbStaff.CurrentUser, DocumentToPrint);
			if (defaultPrinter != null && PackageJob.Factory.Load<IStmPrintQueue>(defaultPrinter.SDP_SQ_Printer) != null) // in case user deletes printer in ediEnterprise
			{
				AutoPrint(package, DocumentToPrint, defaultPrinter.SDP_SQ_Printer, defaultPrinter.SDP_NumberOfCopies);
			}
			else
			{
				var message = "\r\n" + Res.GetString("b851994f-674b-439a-859c-822844339c42",
					"There is no Default Printer defined for the Document '{0}'.\r\nSelect a Default Printer for this Document on the Next Screen.", DocumentToPrint.SU_MenuNameMultilingual);

				OnPrintFailed(message, canContinueWithManualPrint: true);
				RunPrintTask(package);
			}
		}

		static void AutoPrint(IDocumentSupportable documentSupportable, IStmMenuItem document, ZGuid printerPK, int numberOfLabelsToPrint)
		{
			var showNotification = false;
			var printer = ObjectFactory.New<IDocumentPrinter>(showNotification);
			printer.Print(document.PK, printerPK, documentSupportable, numberOfLabelsToPrint);
		}

		void RunPrintTask(PkgPackage package)
		{
			var printRunner = ObjectFactory.New<IPrintTaskRunner>();
			if (DocumentToPrint.SU_IsDocPack)
			{
				printRunner.RunPrintTaskIncludingChildMenus(DocumentToPrint, package);
			}
			else
			{
				printRunner.RunPrintTask(DocumentToPrint, package);
			}
		}

		IStmMenuItem DocumentToPrint;

		#endregion

		#region NoDocumentToPrintError

		public static string NoDocumentToPrintError
		{
			get { return Res.GetString("88f72cbc-5b40-4cdc-9706-e37898ab05ad", "The default document to print has 'Prevent Auto Delivery' checked."); }
		}

		#endregion

		#region GetDocumentToPrint

		public StmMenuItem GetDocumentToPrintWithRegistryFallback(IDocumentSupportable documentSupportable)
		{
			var documentToPrint = GetDocumentToPrint(documentSupportable);
			if (documentToPrint == null)
			{
				var defaultDocument = PackageJob.Factory.Load<StmMenuItem>(PackingRegistry.Instance.DefaultDocumentToPrintOnClosePackage.Value);
				if (!defaultDocument.SU_PreventAutoDelivery)
				{
					documentToPrint = defaultDocument;
				}
			}

			return documentToPrint;
		}

		public StmMenuItem GetDocumentToPrint(IDocumentSupportable documentSupportable)
		{
			var documentToPrint = GetDocumentsForPackageCloseCommand(documentSupportable);
			if (IsDocumentPackEmpty(documentToPrint))
			{
				documentToPrint = null;
			}

			return documentToPrint;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Name")]
		DocumentCommand GetDocumentsForPackageCloseCommand(IDocumentSupportable documentSupportable)
		{
			var documentCommand = DocumentCommand.GetDocumentCommand(PackageJob.Factory, documentSupportable, "Documents for Package Close");

			if (documentCommand != null)
			{
				documentCommand.Parent = documentSupportable;
			}

			return documentCommand;
		}

		bool IsDocumentPackEmpty(DocumentCommand documentCommand)
		{
			var isDocPackEmpty = true;
			if (documentCommand != null)
			{
				using (var printTask = new PrintTask(documentCommand))
				{
					var loader = new PrintTaskDocumentPackLoader(printTask, documentCommand, null);
					loader.LoadAll();

					var docPackForPkgClose = printTask.GetDocumentPacks().SingleOrDefault(docPack => docPack.StmMenuCommand == documentCommand);
					if (docPackForPkgClose != null && docPackForPkgClose.Count > 0)
					{
						isDocPackEmpty = false;
					}
				}
			}

			return isDocPackEmpty;
		}

		#endregion

		#region AutoPrinting

		public event EventHandler<AutoPrintingEventArgs> AutoPrinting;

		void OnAutoPrinting(PkgPackage package)
		{
			if (AutoPrinting != null)
			{
				AutoPrinting(this, new AutoPrintingEventArgs(package));
			}
		}

		#endregion

		#region AutoPrinted

		public event EventHandler AutoPrinted;

		void OnAutoPrinted()
		{
			if (AutoPrinted != null)
			{
				AutoPrinted(this, EventArgs.Empty);
			}
		}

		#endregion

		#region AutoPrintFailed

		public event EventHandler<AutoPrintFailedEventArgs> AutoPrintFailed;

		void OnPrintFailed(ZString message, bool canContinueWithManualPrint)
		{
			if (AutoPrintFailed != null)
			{
				AutoPrintFailed(this, new AutoPrintFailedEventArgs(message, canContinueWithManualPrint));
			}
		}

		#endregion
	}
}
