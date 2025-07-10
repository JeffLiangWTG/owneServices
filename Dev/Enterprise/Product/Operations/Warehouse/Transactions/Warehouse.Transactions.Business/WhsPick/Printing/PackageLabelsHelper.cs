using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PackageLabelsHelper : IDocumentEventsForMenu
	{
		#region Constructor

		public PackageLabelsHelper(WhsPick pick)
		{
			Pick = Argument.NotNull(pick, nameof(pick));
		}

		WhsPick Pick { get; }

		#endregion

		#region GetDocCommandInNewReadOnlyFactory

		public static DocumentCommand GetDocCommandInNewReadOnlyFactory()
		{
			var newFactory = new ReadOnlyBusinessObjectFactory { NameForDebugging = "Warehouse Pick All Package Labels Factory" };
			var docCommand = newFactory.New<DocumentCommand>();
			docCommand.SU_IsDocPack = true;
			docCommand.SU_MenuName = ResString.GetMultilingualString("6B1F4A39-8AD7-48B5-BC2E-F7B52FF81B09", "All Package Labels for a Pick.");
			return docCommand;
		}

		#endregion

		#region PrintAllDocument

		public void PrintAllDocument()
		{
			var docCommand = GetDocCommandInNewReadOnlyFactory();
			docCommand.Parent = Pick;
			docCommand.ParentDocumentSupporter.Initialise(this);

			var docRunner = new DocumentRunner(documentEventsForMenu: this);
			docRunner.Run(docCommand);
		}

		#endregion

		#region GetDocumentPack

		public static DocumentPack GetDocumentPack(WhsPick pick, DocumentCommand docCommand)
		{
			Argument.NotNull(pick, nameof(pick));
			Argument.NotNull(docCommand, nameof(docCommand));

			var docPack = new DocumentPack(docCommand);
			var uomTypeSettings = WarehouseDataRegistry.Instance.UOMPackType.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var labelsForUOMType = uomTypeSettings.Cast<UOMPackType>().ToDictionary(x => x.Code, x => (short)x.NumberOfLabels);

			var sorter = new PackageComparerForLabelPrinting(pick);
			var packagesQueue = new Queue<PkgPackage>(pick.OuterPackages.Where(p => p.PackedItems.Count > 0).OrderBy(p => p, sorter)); // queue is used here for its Peek method (and to increase skill of developers who will read this code) - DRD
			while (packagesQueue.Count > 0)
			{
				var package = packagesQueue.Dequeue();
				var autoPrinter = new PackageLabelAutoPrinter(package.PackageJob);
				var documentToPrint = autoPrinter.GetDocumentToPrintWithRegistryFallback(package);

				var uomType = package.PackType?.F3_UOMType;
				if (string.IsNullOrEmpty(uomType))
				{
					uomType = UOMPackTypesList.Codes.SplitCase;
				}

				var docList = new DocumentCommandCollection(package);
				if (documentToPrint != null)
				{
					docList.AddFromDatabase(documentToPrint.PK);
				}

				package.PackageJob.Selected.UpdateSelectedPackages(new[] { package }); // this is required so the correct document wrapper will be created 
				var numberOfCopies = labelsForUOMType[uomType.Value];
				foreach (DocumentCommand doc in docList)
				{
					var childDocList = new DocumentCommandCollection(package);
					for (int i = 0; i < numberOfCopies; i++)
					{
						if (!doc.SU_IsDocPack)
						{
							docPack.AddReportsToPack(doc, null, package, null);
						}

						if (childDocList.Count == 0)
						{
							AddChildDocuments(package, doc, childDocList);
						}
						foreach (DocumentCommand childDoc in childDocList)
						{
							docPack.AddReportsToPack(childDoc, null, package, null);
						}
					}
				}
				if (!pick.IsPrintingWithoutSeparatorLabels)
				{
					var splitterDocList = new DocumentCommandCollection(package);
					var nextPackage = packagesQueue.Count > 0 ? packagesQueue.Peek() : null;
					var delimeterType = sorter.GetDelimeterTypeBetweenPackages(package, nextPackage);
					AddDelimeterDocumentsIfNeeded(delimeterType, splitterDocList);

					foreach (DocumentCommand doc in splitterDocList)
					{
						docPack.AddReportsToPack(doc, null, package, null);
					}
				}
			}

			return docPack;
		}

		static void AddChildDocuments(PkgPackage package, DocumentCommand parentDocument, DocumentCommandCollection childDocList)
		{
			using (var printTask = new PrintTask(parentDocument))
			{
				var loader = new PrintTaskDocumentPackLoader(printTask, parentDocument, null);
				loader.LoadAll();

				IEnumerable<StmMenuItem> childDocuments;
				if (parentDocument.SU_IsDocPack)
				{
					childDocuments = GetChildDocumentsInDocPack(printTask, parentDocument);
				}
				else
				{
					childDocuments = printTask.GetDocumentPacks().Where(pack => pack.StmMenuCommand != parentDocument).Select(p => p.StmMenuCommand);
				}

				if (childDocuments != null && childDocuments.Any())
				{
					foreach (var childDocument in childDocuments)
					{
						var childDocumentCommand = DocumentCommand.GetDocumentCommand(package.Factory, package, childDocument.SU_MenuName);
						if (childDocumentCommand != null)
						{
							childDocList.Add(childDocumentCommand);
						}
					}
				}
			}
		}

		public static IEnumerable<StmMenuItem> GetChildDocumentsInDocPack(PrintTask printTask, DocumentCommand parentDocument)
		{
			return printTask.GetDocumentPacks().SingleOrDefault(pack => pack.StmMenuCommand == parentDocument)?.Cast<Report>()
				.Where(r => r.MenuItem.PK != parentDocument.PK).Select(r => r.MenuItem) ?? Enumerable.Empty<StmMenuItem>();
		}

		public static void AddDelimeterDocumentsIfNeeded(PackageComparerForLabelPrinting.DocDelimeterType delimeterType, DocumentCommandCollection docList)
		{
			if (delimeterType.HasFlag(PackageComparerForLabelPrinting.DocDelimeterType.EndOfPickGroup))
			{
				docList.AddFromDatabase(EndOfPickGroupLabelPK);
			}
			if (delimeterType.HasFlag(PackageComparerForLabelPrinting.DocDelimeterType.EndOfPallet))
			{
				docList.AddFromDatabase(EndOfPalletLabelPK);
			}
			if (delimeterType.HasFlag(PackageComparerForLabelPrinting.DocDelimeterType.EndOfCase))
			{
				docList.AddFromDatabase(EndOfCaseLabelPK);
			}
			if (delimeterType.HasFlag(PackageComparerForLabelPrinting.DocDelimeterType.EndOfSplitCase))
			{
				docList.AddFromDatabase(EndOfSplitCaseLabelPK);
			}
			if (delimeterType.HasFlag(PackageComparerForLabelPrinting.DocDelimeterType.EndOfArea))
			{
				docList.AddFromDatabase(EndOfAreaLabelPK);
			}
		}

		#region IDocumentEventsForMenu Members

		event DocumentCancelEventHandler IDocumentEvents.DocumentPrintRequested
		{
			add { documentPrintRequested += value; }
			remove { documentPrintRequested -= value; }
		}

		event DocumentPrintedEventHandler IDocumentEvents.DocumentPrinted
		{
			add { documentPrinted += value; }
			remove { documentPrinted -= value; }
		}

		event DocumentPrintedEventHandler IDocumentEvents.DocumentPrePreviewed
		{
			add { documentPrePreviewed += value; }
			remove { documentPrePreviewed -= value; }
		}

		event DocumentPrintedEventHandler IDocumentEvents.DocumentPrePrinted
		{
			add { documentPrePrinted += value; }
			remove { documentPrePrinted -= value; }
		}

		event DocumentCancelEventHandler documentPrintRequested;
		event DocumentPrintedEventHandler documentPrinted;
		event DocumentPrintedEventHandler documentPrePreviewed;
		event DocumentPrintedEventHandler documentPrePrinted;

		bool IDocumentEventsForMenu.CancelPrintRequest
		{
			get { return cancelPrintRequest; }
		}
		bool cancelPrintRequest;

		void IDocumentEventsForMenu.NotifyDocumentPrePreviewed(DocumentPrintedEventArgs e)
		{
			documentPrePreviewed?.Invoke(this, e);
		}

		void IDocumentEventsForMenu.NotifyDocumentPrePrinted(DocumentPrintedEventArgs e)
		{
			documentPrePrinted?.Invoke(this, e);
		}

		void IDocumentEventsForMenu.NotifyDocumentPrintRequested(IStmMenuItem menuItem)
		{
			cancelPrintRequest = false;
			var args = new DocumentCancelEventArgs(menuItem);
			if (documentPrintRequested != null)
			{
				cancelPrintRequest = args.Cancel;
			}
		}

		public void NotifyDocumentPrinted(DocumentPrintedEventArgs e)
		{
			documentPrinted?.Invoke(this, e);
		}

		#endregion

		internal static readonly ZGuid EndOfAreaLabelPK = new ZGuid("174bbaf6-6f87-4299-9fe1-243335bc4199");
		internal static readonly ZGuid EndOfPalletLabelPK = new ZGuid("494e1b73-2ea5-4fca-b673-ebca174b4b2d");
		internal static readonly ZGuid EndOfCaseLabelPK = new ZGuid("33307ec0-bd70-4ccd-8267-58f761e6278f");
		internal static readonly ZGuid EndOfSplitCaseLabelPK = new ZGuid("06da3ea6-117e-4d01-be66-345d94474456");
		internal static readonly ZGuid EndOfPickGroupLabelPK = new ZGuid("cf051758-df29-4d11-b0a9-388c0bfc337b");

		#endregion
	}
}
