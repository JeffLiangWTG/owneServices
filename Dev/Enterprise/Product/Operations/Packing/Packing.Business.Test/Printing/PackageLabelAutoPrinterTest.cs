using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business.Testing
{
	public class PackageLabelAutoPrinterTest : PackingTestCaseWithFactory
	{
		#region TestAutoPrintDoesNotLoadEveryDocumentIfDocumentSupporterHasNoRelatedConsignee

		public void TestAutoPrintDoesNotLoadEveryDocumentIfDocumentSupporterHasNoRelatedConsignee()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package = packageJob.Packages.AddNew();

			AssertNoExceptionThrown(() => packageJob.PrintLabel(package));
		}

		#endregion

		#region TestAutoPrinting

		public void TestAutoPrinting()
		{
			bool autoPrintingFired = false;

			Data.PackageJob.AutoPrinting += delegate
			{ autoPrintingFired = true; };
			AssertEquals("Precondition", false, autoPrintingFired);

			var package = Data.PackageJob.Packages.AddNew();
			Data.Printer.PrintDocument(package);
			AssertEquals(true, autoPrintingFired);
		}

		#endregion

		#region TestAutoPrinted

		public void TestAutoPrinted()
		{
			bool autoPrintedFired = false;

			Data.PackageJob.AutoPrinted += delegate
			{ autoPrintedFired = true; };
			AssertEquals("Precondition", false, autoPrintedFired);

			var package = Data.PackageJob.Packages.AddNew();
			Data.Printer.PrintDocument(package);
			AssertEquals(true, autoPrintedFired);
		}

		#endregion

		#region TestGetDocumentToPrint

		public void TestGetDocumentToPrint()
		{
			IDocumentSupportable package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");
			var docCommand = DocumentCommand.GetDocumentCommand(Factory, package, "Documents for Package Close");
			var documentMenuPK = new Guid("8241245e-3ad7-494a-96d4-7e2e31935813");

			// link document to doc pack
			var stmMenuMenuPivot = Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot.SF_SU_Outward = documentMenuPK;
			stmMenuMenuPivot.SF_SU_Inward = docCommand.PK;
			Factory.Save();

			var documentToPrint = Data.Printer.GetDocumentToPrint(package);
			AssertEquals("Document pack 'Document for Package Close' is returned.", docCommand.PK, documentToPrint.PK);
		}

		public void TestGetDocumentToPrint_EmptyDocumentPack()
		{
			IDocumentSupportable package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");
			var docCommand = DocumentCommand.GetDocumentCommand(Factory, package, "Documents for Package Close");
			var documentMenuPK = new Guid("8241245e-3ad7-494a-96d4-7e2e31935813");

			using (PackingRegistry.Instance.DefaultDocumentToPrintOnClosePackage.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, documentMenuPK))
			using (var printTask = new PrintTask(docCommand))
			{
				printTask.DeliveryInstructionsDefaultPK = docCommand.PK;
				var loader = new PrintTaskDocumentPackLoader(printTask, docCommand, null);
				loader.LoadAll();
				AssertEquals("No document to print on document pack.", 0, printTask.GetDocumentPacks().Count());

				Factory.Save();
				var documentToPrint = Data.Printer.GetDocumentToPrint(package);
				AssertNull("No document to print.", documentToPrint);
			}
		}

		public void TestGetDocumentToPrintWithRegistryFallback()
		{
			IDocumentSupportable package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");
			var documentMenuPK = new Guid("8241245e-3ad7-494a-96d4-7e2e31935813");
			var docCommand = DocumentCommand.GetDocumentCommand(Factory, package, "Documents for Package Close");
			docCommand.Parent = package;

			// link document to doc pack
			var stmMenuMenuPivot = Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot.SF_SU_Outward = documentMenuPK;
			stmMenuMenuPivot.SF_SU_Inward = docCommand.PK;
			Factory.Save();

			using (PackingRegistry.Instance.DefaultDocumentToPrintOnClosePackage.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, documentMenuPK))
			using (var printTask = new PrintTask(docCommand))
			{
				printTask.DeliveryInstructionsDefaultPK = docCommand.PK;
				var loader = new PrintTaskDocumentPackLoader(printTask, docCommand, null);
				loader.LoadAll();

				AssertEquals(1, printTask.GetDocumentPacks().Count());
				var docPack = printTask.GetFirstDocumentPack();
				AssertEquals("Precondition: Document pack is not empty.", true, docPack.Any());

				Factory.Save();
				var documentToPrint = Data.Printer.GetDocumentToPrintWithRegistryFallback(package);
				AssertEquals("Document pack 'Document for Package Close' is returned.", docCommand.PK, documentToPrint.PK);
			}
		}

		public void TestGetDocumentToPrintWithRegistryFallback_EmptyDocumentPack()
		{
			IDocumentSupportable package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");
			var docCommand = DocumentCommand.GetDocumentCommand(Factory, package, "Documents for Package Close");
			var documentMenuPK = new Guid("8241245e-3ad7-494a-96d4-7e2e31935813");

			using (PackingRegistry.Instance.DefaultDocumentToPrintOnClosePackage.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, documentMenuPK))
			using (var printTask = new PrintTask(docCommand))
			{
				printTask.DeliveryInstructionsDefaultPK = docCommand.PK;
				var loader = new PrintTaskDocumentPackLoader(printTask, docCommand, null);
				loader.LoadAll();
				AssertEquals("No document to print on document pack.", 0, printTask.GetDocumentPacks().Count());

				Factory.Save();
				var documentToPrint = Data.Printer.GetDocumentToPrintWithRegistryFallback(package);
				AssertEquals("Configured registry default is returned.", documentMenuPK, documentToPrint.PK);
			}
		}

		#endregion

		#region TestPrintDocumentConstructor

		public void TestPrintDocumentConstructor()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => { new PackageLabelAutoPrinter(null); });
		}

		#endregion

		#region TestNonAutoPrintingDataContexts

		public void TestNonAutoPrintingDataContexts()
		{
			var enums = Enum.GetValues(typeof(Constants.DataContext));
			var regex = new System.Text.RegularExpressions.Regex("Generic[A-Za-z0-9_]+LabelAll");
			foreach (var value in enums)
			{
				if (regex.IsMatch(value.ToString()))
				{
					AssertCollectionContains("Collection did not contain [" + value.ToString() + "]",
						value, RegistryConstants.Packing.NonAutoPrintingDataContexts);
				}
			}
		}

		#endregion

		#region TestPrintDocument

		#region TestPrintDocument_DoesNotAcceptNullPackage

		public void TestPrintDocument_DoesNotAcceptNullPackage()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => { Data.Printer.PrintDocument(null); });
		}

		#endregion

		#region TestPrintDocument_DoesNotAcceptPackageFromAnotherPackageJob

		public void TestPrintDocument_DoesNotAcceptPackageFromAnotherPackageJob()
		{
			AssertExceptionThrown(typeof(ArgumentException),
				"Package is not a child of the PackageLabelAutoPrinter's PackageJob.", () => { Data.Printer.PrintDocument(Factory.New<PkgPackage>()); });
		}

		#endregion

		#region TestPrintDocument_DBHits

		public void TestPrintDocument_DBHits()
		{
			var package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");

			// Basic Label
			var newDocumentMenuPK = new Guid("8241245e-3ad7-494a-96d4-7e2e31935813");

			// Delivery Label
			var anotherDocumentMenuPK = new Guid("fcbc917b-20e1-428a-8649-8c8d308b3485");

			// Setup document pack to have linked documents
			var docPackForPackageClose = DocumentCommand.GetDocumentCommand(Factory, package, "Documents for Package Close");
			docPackForPackageClose.Parent = package;

			var stmMenuMenuPivot1 = Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot1.SF_SU_Outward = newDocumentMenuPK;
			stmMenuMenuPivot1.SF_SU_Inward = docPackForPackageClose.PK;

			var stmMenuMenuPivot2 = Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot2.SF_SU_Outward = anotherDocumentMenuPK;
			stmMenuMenuPivot2.SF_SU_Inward = docPackForPackageClose.PK;

			var stmMenuMenuPivot3 = Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot3.SF_SU_Outward = PackingRegistry.DefaultTargetLabelDocument;
			stmMenuMenuPivot3.SF_SU_Inward = docPackForPackageClose.PK;
			Factory.Save();

			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocument, Printer.PK, 1);
			Helper.CreateDefaultPrinter(docPackForPackageClose.PK, Printer.PK, 1);
			Factory.Save();

			Data.Dummy.NewFactory = new BusinessObjectFactory(); // Different factory to test DB Hits;
			var packageJob = Data.Dummy.NewFactory.Load<PkgPackageJob>(Data.PackageJob.PK);

			var autoPrint = new PackageLabelAutoPrinter(packageJob);
			autoPrint.PrintDocument(package);
			AssertEquals("Should have 2x hits - 1 for DocPack and 1 for Child Documents.", 2, Data.Dummy.NewFactory.GetTableHitCount("StmMenuItem"));
			AssertEquals("Should have 2x hits - 1 for DocPack and 1 for Child Documents.", 2, Data.Dummy.NewFactory.GetTableHitCount("StmMenuTemplatePivot"));
		}

		#endregion

		#region TestPrintDocument_WithAutomaticPrintingButNoPrinterSet

		public void TestPrintDocument_WithAutomaticPrintingButNoPrinterSet()
		{
			var package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");

			// print automatically, will popup GUI because printer is not set
			Data.Printer.PrintDocument(package);

			var printJobs = new BusinessObjectFactory().Load<IStmPrintJob>(new ZQuery());
			AssertEquals(0, printJobs.Length);
			AssertEquals(true, CanContinueWithPrint);
			AssertEquals("\r\nThere is no Default Printer defined for the Document 'Product/Delivery (Selected Only)'.\r\nSelect a Default Printer for this Document on the Next Screen.", Message);
		}

		#endregion

		#region TestPrintDocument_WithAutomaticPrintingAndPrinterSet

		public void TestPrintDocument_WithAutomaticPrintingAndPrinterSet()
		{
			var package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");

			// set Printer for this user and document, default document should be printed
			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocument, Printer.PK, 1);
			Factory.Save();
			Data.Printer.PrintDocument(package);

			var printJobs = new BusinessObjectFactory().Load<IStmPrintJob>(new ZQuery());
			AssertEquals(1, printJobs.Length);
			var printJob = printJobs[0];
			AssertPrintJob(printJob, "Product/Delivery");
			AssertNull(Message);
		}

		#endregion

		#region TestPrintDocument_WithEmptyDocumentPack

		public void TestPrintDocument_WithEmptyDocumentPack()
		{
			var package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");
			Factory.Save();

			//No setup for document pack
			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocument, Printer.PK, 1);
			Factory.Save();
			// Will print default document since job does not contain random Org
			Data.Printer.PrintDocument(package);

			var printJobs = new BusinessObjectFactory().Load<IStmPrintJob>(new ZQuery());
			AssertEquals(1, printJobs.Length);
			var printJob = printJobs[0];
			AssertPrintJob(printJob, "Product/Delivery");
			AssertNull(Message);
		}

		#endregion

		#region TestPrintDocument_WithNotEmptyDocumentPack

		public void TestPrintDocument_WithNotEmptyDocumentPack()
		{
			var package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");

			// Basic Label
			var newDocumentMenuPK = new Guid("8241245e-3ad7-494a-96d4-7e2e31935813");

			// Setup document pack to have linked document
			var docCommand = DocumentCommand.GetDocumentCommand(Factory, package, "Documents for Package Close");
			docCommand.Parent = package;

			var stmMenuMenuPivot = Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot.SF_SU_Outward = newDocumentMenuPK;
			stmMenuMenuPivot.SF_SU_Inward = docCommand.PK;
			Factory.Save();

			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocument, Printer.PK, 1);
			Helper.CreateDefaultPrinter(docCommand.PK, Printer.PK, 1);

			Factory.Save();

			// Should print document pack
			Data.Printer.PrintDocument(package);

			var printJobs = new BusinessObjectFactory().Load<IStmPrintJob>(new ZQuery());
			AssertEquals(1, printJobs.Length);
			var printJob = printJobs[0];
			AssertPrintJob(printJob, "Basic Label");
			AssertNull(Message);
		}

		#endregion

		#region TestPrintDocument_WithNotEmptyDocumentPackButPrinterNotSet

		public void TestPrintDocument_WithNotEmptyDocumentPackButPrinterNotSet()
		{
			var package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");

			// Basic Label
			var newDocumentMenuPK = new Guid("8241245e-3ad7-494a-96d4-7e2e31935813");

			// Setup document pack to have linked document
			var docCommand = DocumentCommand.GetDocumentCommand(Factory, package, "Documents for Package Close");
			docCommand.Parent = package;

			var stmMenuMenuPivot = Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot.SF_SU_Outward = newDocumentMenuPK;
			stmMenuMenuPivot.SF_SU_Inward = docCommand.PK;
			Factory.Save();

			// Should not print the document pack
			Data.Printer.PrintDocument(package);

			var printJobs = new BusinessObjectFactory().Load<IStmPrintJob>(new ZQuery());
			AssertEquals(0, printJobs.Length);
			AssertEquals("\r\nThere is no Default Printer defined for the Document 'Documents for Package Close'.\r\nSelect a Default Printer for this Document on the Next Screen.", Message);
		}

		#endregion

		#region TestPrintDocument_WithDocumentPackNotEmptyButPreventAutoDeliveryOn

		public void TestPrintDocument_WithDocumentPackNotEmptyButPreventAutoDeliveryOn()
		{
			var package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");

			// Basic Label
			var newDocumentMenuPK = new Guid("8241245e-3ad7-494a-96d4-7e2e31935813");
			var newDocument = Factory.Load<IStmMenuItem>(newDocumentMenuPK);
			newDocument.SU_PreventAutoDelivery = true;

			// Setup document pack to have linked document
			var docCommand = DocumentCommand.GetDocumentCommand(Factory, package, "Documents for Package Close");
			docCommand.Parent = package;

			var stmMenuMenuPivot = Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot.SF_SU_Outward = newDocumentMenuPK;
			stmMenuMenuPivot.SF_SU_Inward = docCommand.PK;
			Factory.Save();

			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocument, Printer.PK, 1);
			Helper.CreateDefaultPrinter(docCommand.PK, Printer.PK, 1);

			Factory.Save();

			// Should still print the document pack, PreventAutoDelivery is not checked anymore
			Data.Printer.PrintDocument(package);

			var printJobs = new BusinessObjectFactory().Load<IStmPrintJob>(new ZQuery());
			AssertEquals(1, printJobs.Length);
			var printJob = printJobs[0];
			AssertPrintJob(printJob, "Basic Label");
			AssertNull(Message);
		}

		#endregion

		#region TestPrintDocument_WithDocumentPackNotEmptyButDoNotPrintInDocumentPackOn

		public void TestPrintDocument_WithDocumentPackNotEmptyButDoNotPrintInDocumentPackOn()
		{
			var package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");

			// Basic Label
			var newDocumentMenuPK = new Guid("8241245e-3ad7-494a-96d4-7e2e31935813");

			// Setup document pack to have linked document
			var docCommand = DocumentCommand.GetDocumentCommand(Factory, package, "Documents for Package Close");
			docCommand.Parent = package;

			var stmMenuMenuPivot = Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot.SF_SU_Outward = newDocumentMenuPK;
			stmMenuMenuPivot.SF_SU_Inward = docCommand.PK;
			Factory.Save();

			// Set up a document to not print in document pack on
			var existingPivot = Factory.LoadTop1<StmMenuTemplatePivot>(new ZQuery(StmMenuTemplatePivotSchema.SI_SU, newDocumentMenuPK));
			var stmMenuDocumentConfig = Factory.LoadTop1<StmMenuDocumentConfig>(new ZQuery(StmMenuDocumentConfigSchema.S3_SI, existingPivot.PK));
			stmMenuDocumentConfig.S3_ExcludedFromDocPack = true;

			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocument, Printer.PK, 1);
			Helper.CreateDefaultPrinter(docCommand.PK, Printer.PK, 1);

			Factory.Save();

			// Should print default document as multi-label documents are ignored
			Data.Printer.PrintDocument(package);

			var printJobs = new BusinessObjectFactory().Load<IStmPrintJob>(new ZQuery());
			AssertEquals(1, printJobs.Length);
			var printJob = printJobs[0];
			AssertPrintJob(printJob, "Product/Delivery");
			AssertNull(Message);
		}

		#endregion

		#region TestPrintDocument_WithMultipleDocumentsLinkedToDocumentPack

		public void TestPrintDocument_WithMultipleDocumentsLinkedToDocumentPack()
		{
			var package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");

			// Basic Label
			var newDocumentMenuPK = new Guid("8241245e-3ad7-494a-96d4-7e2e31935813");

			// Setup document pack to have linked documents
			var docCommand = DocumentCommand.GetDocumentCommand(Factory, package, "Documents for Package Close");
			docCommand.Parent = package;

			var stmMenuMenuPivot1 = Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot1.SF_SU_Outward = newDocumentMenuPK;
			stmMenuMenuPivot1.SF_SU_Inward = docCommand.PK;

			var stmMenuMenuPivot2 = Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot2.SF_SU_Outward = PackingRegistry.DefaultTargetLabelDocument;
			stmMenuMenuPivot2.SF_SU_Inward = docCommand.PK;
			Factory.Save();

			Helper.CreateDefaultPrinter(docCommand.PK, Printer.PK, 1);
			Factory.Save();
			// Should print both documents
			Data.Printer.PrintDocument(package);

			var printJobs = new BusinessObjectFactory().Load<IStmPrintJob>(new ZQuery());
			AssertEquals(2, printJobs.Length);
			var printJob1 = printJobs.First(p => ((BusinessObject)p)[StmPrintJobSchema.SP_DocumentName].Equals("Basic Label" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling));
			var printJob2 = printJobs.First(p => ((BusinessObject)p)[StmPrintJobSchema.SP_DocumentName].Equals("Product/Delivery" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling));

			AssertPrintJob(printJob1, "Basic Label");
			AssertPrintJob(printJob2, "Product/Delivery");
			AssertNull(Message);
		}

		#endregion

		#region TestPrintDocument_WithInvalidPrinterSet

		public void TestPrintDocument_WithInvalidPrinterSet()
		{
			var package = Data.PackageJob.Packages.AddNew();

			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocument, Printer.PK, 1);
			Factory.Save();

			// Should print no document

			((BusinessObject)Printer).Delete();
			Data.Printer.PrintDocument(package);

			var printJobs = new BusinessObjectFactory().Load<IStmPrintJob>(new ZQuery());
			AssertEquals(0, printJobs.Length);
			AssertEquals(true, CanContinueWithPrint);
			AssertEquals("\r\nThere is no Default Printer defined for the Document 'Product/Delivery (Selected Only)'.\r\nSelect a Default Printer for this Document on the Next Screen.", Message);
		}

		#endregion

		#region TestPrintDocument_WithDifferentDefaultDocumentSet

		public void TestPrintDocument_WithDifferentDefaultDocumentSet()
		{
			var package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");

			// Basic Label and Set as Default
			var newDocumentMenuPK = new Guid("8241245e-3ad7-494a-96d4-7e2e31935813");
			PackingRegistry.Instance.DefaultDocumentToPrintOnClosePackage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, newDocumentMenuPK);

			// set Printer for this user and the usual and new default documents, new default document should be printed
			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocument, Printer.PK, 1);
			Helper.CreateDefaultPrinter(newDocumentMenuPK, Printer.PK, 1);
			Factory.Save();

			Data.Printer.PrintDocument(package);

			var printJobs = new BusinessObjectFactory().Load<IStmPrintJob>(new ZQuery());
			AssertEquals(1, printJobs.Length);
			var printJob = printJobs[0];
			AssertPrintJob(printJob, "Basic Label");
			AssertNull(Message);
		}

		#endregion

		#region TestPrintDocument_WithDefaultDocumentHavingPreventAutoDeliveryOn

		public void TestPrintDocument_WithDefaultDocumentHavingPreventAutoDeliveryOn()
		{
			var package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");

			var targetLabelMenu = Factory.Load<IStmMenuItem>(PackingRegistry.DefaultTargetLabelDocument);
			targetLabelMenu.SU_PreventAutoDelivery = true;

			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocument, Printer.PK, 1);
			Factory.Save();

			Data.Printer.PrintDocument(package);

			var printJobs = new BusinessObjectFactory().Load<IStmPrintJob>(new ZQuery());
			AssertEquals(0, printJobs.Length);
			AssertEquals(false, CanContinueWithPrint);
			AssertEquals("The default document to print has 'Prevent Auto Delivery' checked.", Message);
		}

		#endregion

		#region TestPrintDocument_WithPrinterDetails

		public void TestPrintDocument_WithPrinterDetails()
		{
			var package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");

			// Basic Label and Set as Default
			var newDocumentMenuPK = new Guid("8241245e-3ad7-494a-96d4-7e2e31935813");
			PackingRegistry.Instance.DefaultDocumentToPrintOnClosePackage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, newDocumentMenuPK);

			// set Printer for documents
			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocument, Printer.PK, 1);
			Helper.CreateDefaultPrinter(newDocumentMenuPK, Printer.PK, 1);
			Factory.Save();

			Data.Printer.PrintDocument(package, Guid.Empty, 2);
			AssertEquals("There should be and Error Message when invalid printer is provided.", "No valid Printer provided.", Message);
			Message = null; // clean-up

			Data.Printer.PrintDocument(package, Printer.PK.ToGuid(), 2);
			var printJob = Factory.LoadTop1<IStmPrintJob>(new ZQuery());
			AssertPrintJob(printJob, "Basic Label", numberOfCopies: 2);
			AssertNull(Message);
		}

		#endregion

		#region TestPrintDocument_WithNoPrinterDetails

		public void TestPrintDocument_WithNoPrinterDetails_NotEmptyDocumentPack()
		{
			var package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");

			// Basic Label
			var newDocumentMenuPK = new Guid("8241245e-3ad7-494a-96d4-7e2e31935813");

			// Setup document pack to have linked document
			var docCommand = DocumentCommand.GetDocumentCommand(Factory, package, "Documents for Package Close");
			docCommand.Parent = package;

			var stmMenuMenuPivot = Factory.New<StmMenuMenuPivot>();
			stmMenuMenuPivot.SF_SU_Outward = newDocumentMenuPK;
			stmMenuMenuPivot.SF_SU_Inward = docCommand.PK;
			Factory.Save();

			var printTaskRunnerMock = new PrintTaskRunnerMock();
			printTaskRunnerMock.RunActionBeforeRunPrintTask += (stmMenuItem, methodName) =>
			{
				AssertEquals("Document to print is 'Documents for Package Close'.", docCommand, stmMenuItem);
				AssertEquals("Document to print is a document pack.", true, stmMenuItem.SU_IsDocPack);
				AssertEquals("Print task runner invoked 'RunPrintTaskIncludingChildMenus'.", "RunPrintTaskIncludingChildMenus", methodName);
			};

			using (ObjectFactory.Substitute(printTaskRunnerMock))
			{
				Data.Printer.PrintDocument(package);
				AssertEquals("\r\nThere is no Default Printer defined for the Document 'Documents for Package Close'.\r\nSelect a Default Printer for this Document on the Next Screen.", Message);
			}
		}

		public void TestPrintDocument_WithNoPrinterDetails_EmptyDocumentPack()
		{
			var package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");
			var defaultDocumentToPrint = Factory.Load<IStmMenuItem>(PackingRegistry.DefaultTargetLabelDocument);

			var printTaskRunnerMock = new PrintTaskRunnerMock();
			printTaskRunnerMock.RunActionBeforeRunPrintTask += (stmMenuItem, methodName) =>
			{
				AssertEquals("Document to print is registry default document.", defaultDocumentToPrint, stmMenuItem);
				AssertEquals("Document to print is not a document pack.", false, stmMenuItem.SU_IsDocPack);
				AssertEquals("Print task runner invoked 'RunPrintTask'.", "RunPrintTask", methodName);
			};

			using (ObjectFactory.Substitute(printTaskRunnerMock))
			{
				Data.Printer.PrintDocument(package);
				AssertEquals("\r\nThere is no Default Printer defined for the Document 'Product/Delivery (Selected Only)'.\r\nSelect a Default Printer for this Document on the Next Screen.", Message);
			}
		}

		#endregion

		#region TestPrintDocument_WithDocumentSpecified

		public void TestPrintDocument_WithDocumentSpecified()
		{
			var package = Helper.CreatePackage(Data.PackageJob, "Test1", 1, "BOX");

			// Basic Label and Set as Default
			var newDocumentMenuPK = new Guid("8241245e-3ad7-494a-96d4-7e2e31935813");
			PackingRegistry.Instance.DefaultDocumentToPrintOnClosePackage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, newDocumentMenuPK);

			// set Printer for documents. This will be ignored when document specified
			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocument, Printer.PK, 1);
			Helper.CreateDefaultPrinter(newDocumentMenuPK, Printer.PK, 1);
			Factory.Save();

			// load a document
			var labelQuery = new DocumentZQuery(CargoWise.Definitions.BusinessContext.Package, "Package Manifest");
			var manifestDocument = Factory.LoadTop1<StmMenuItem>(labelQuery);

			Data.Printer.PrintDocument(package, Guid.Empty, 2, manifestDocument);
			AssertEquals("There should be and Error Message when invalid printer is provided.", "No valid Printer provided.", Message);
			Message = null; // clean-up

			Data.Printer.PrintDocument(package, Printer.PK.ToGuid(), 2, manifestDocument);
			var printJob = Factory.LoadTop1<IStmPrintJob>(new ZQuery());
			AssertPrintJob(printJob, "Package Manifest", numberOfCopies: 2);
			AssertNull(Message);
		}

		#endregion

		#endregion

		#region Implementation

		void AssertPrintJob(IStmPrintJob printJob, ZString documentName, short numberOfCopies = 1)
		{
			AssertPrintJob(printJob, documentName, Printer.PK, numberOfCopies);
		}

		void AssertPrintJob(IStmPrintJob printJob, ZString documentName, ZGuid printerPK, short numberOfCopies = 1)
		{
			var printJobAsBizo = (BusinessObject)printJob;
			AssertEquals(documentName + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling, printJobAsBizo[StmPrintJobSchema.SP_DocumentName]);
			AssertEquals(string.Format("EAGLE DATAMATION INTERNATIONAL - BN - AUBNE - {0}", documentName), printJobAsBizo[StmPrintJobSchema.SP_EmailSubjectLine]);
			AssertEquals("E", printJobAsBizo[StmPrintJobSchema.SP_GS_NKJobSubmittedBy]);
			AssertEquals("PRN", printJobAsBizo[StmPrintJobSchema.SP_JobType]);
			AssertEquals("PPL", printJobAsBizo[StmPrintJobSchema.SP_DocumentType]);
			AssertEquals(printerPK, printJobAsBizo[StmPrintJobSchema.SP_SQ]);
			AssertEquals(numberOfCopies, printJobAsBizo[StmPrintJobSchema.SP_Copies]);
		}

		protected override void SetUp()
		{
			base.SetUp();

			Data.CreatePackingData();
			Data.PackageJob.AutoPrintFailed += (sender, e) =>
			{
				CanContinueWithPrint = e.CanContinueWithManualPrint;
				Message = e.Message;
			};

			Printer = Factory.New<IStmPrintQueue>();
			Printer.QueueName = "ZDesigner LP 2844";
		}

		IStmPrintQueue Printer;
		bool CanContinueWithPrint;
		string Message;

		#endregion
	}

	public class PrintTaskRunnerMock : IPrintTaskRunner
	{
		void IPrintTaskRunner.RunPrintTask(IStmMenuItem menuItem, IDocumentSupportable documentSupportable)
		{
			RunActionBeforeRunPrintTask?.Invoke(menuItem, "RunPrintTask");
		}

		void IPrintTaskRunner.RunPrintTaskIncludingChildMenus(IStmMenuItem menuItem, BusinessObject businessObject)
		{
			RunActionBeforeRunPrintTask?.Invoke(menuItem, "RunPrintTaskIncludingChildMenus");
		}

		public Action<IStmMenuItem, string> RunActionBeforeRunPrintTask;
	}
}
