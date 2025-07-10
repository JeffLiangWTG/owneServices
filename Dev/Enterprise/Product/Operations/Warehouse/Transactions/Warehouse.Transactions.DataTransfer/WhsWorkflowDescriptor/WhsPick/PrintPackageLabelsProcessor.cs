using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;

namespace Enterprise.Warehouse.Transactions.Business
{
	class PrintPackageLabelsProcessor : IProcessor
	{
		public PrintPackageLabelsProcessor(WhsPick pick, ZGuid printerPK)
		{
			Pick = Argument.NotNull(pick, nameof(pick));
			PrinterPK = printerPK;
		}
		readonly WhsPick Pick;
		readonly ZGuid PrinterPK;

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			var deliveryJob = new PackageLabelsDocumentDeliveryJob(Pick, PrinterPK);
			deliveryJob.DeliverWithoutSettingAutoDeliveryFlag(notifications);
		}
	}

	#region PackageLabelsDocumentDeliveryJob

	[Serializable]
	class PackageLabelsDocumentDeliveryJob : AutoDocumentDeliveryJob
	{
		public PackageLabelsDocumentDeliveryJob(WhsPick pick, ZGuid printerPK)
			: base(pick, ZGuid.Empty, printerPK, true)
		{
			Pick = Argument.NotNull(pick, nameof(pick));
		}
		readonly WhsPick Pick;

		#region DeliverWithoutSettingAutoDeliveryFlag

		public void DeliverWithoutSettingAutoDeliveryFlag(INotifications notifications)
		{
			// The IsAutoDocumentDelivery flag is used to prevent duplicate documented being automatically printed
			// With "Print all Package Labels" we want duplicates to be printed so do not want to set this flag
			DeliverCore(notifications, shouldSetIsAutoDocumentDeliveryFlag: false);
		}

		#endregion

		#region GetNewDocumentPrintSetForDelivery

		protected override DocumentPrintSet GetNewDocumentPrintSetForDelivery()
		{
			var docPack = PackageLabelsHelper.GetDocumentPack(Pick, DocCommand);
			var documentPrintSet = new DocumentPrintSet(DocCommand);
			documentPrintSet.Add(docPack);

			return documentPrintSet;
		}

		#endregion

		#region DocCommand

		protected override DocumentCommand GetDocumentCommandCore()
		{
			return DocCommand;
		}

		DocumentCommand DocCommand
		{
			get { return docCommand ?? (docCommand = PackageLabelsHelper.GetDocCommandInNewReadOnlyFactory()); }
		}
		DocumentCommand docCommand;

		#endregion
	}

	#endregion
}
