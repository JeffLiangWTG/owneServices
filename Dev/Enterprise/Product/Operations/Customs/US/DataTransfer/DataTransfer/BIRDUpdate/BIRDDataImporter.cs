using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.DataTransfer
{
	public class BIRDDataImporter : DataImporter
	{
		public BIRDDataImporter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		internal virtual IBIRDProcessor DefaultProcessor => new ACSBIRDProcessor(FactoryProvider);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			var documentFactories = new List<ITransactionParticipant>();
			IBIRDProcessor processor = null;
			ZString lineOneBlock = dataReader.ReadLine();
			ZString lineTwoBlock = dataReader.ReadLine();

			if (!lineOneBlock.IsEmpty && lineOneBlock.StartsWith("A", System.StringComparison.OrdinalIgnoreCase) && !lineTwoBlock.IsEmpty && lineTwoBlock.StartsWith("B", System.StringComparison.OrdinalIgnoreCase))
			{
				var applicationID = lineTwoBlock.SubstringSafe(10, 2).Trim();
				if (applicationID.EqualsIgnoringCase(ACEApplicationIdentifierCodeList.Codes.EntrySummary) || applicationID.EqualsIgnoringCase(ACEApplicationIdentifierCodeList.Codes.CargoRelease))
				{
					processor = new ACEBIRDProcessor(FactoryProvider);
				}
			}

			processor = processor ?? DefaultProcessor;
			var result = false;
			try
			{
				result = processor.Process(dataReader, attachmentFileName, notifications, documentFactories, lineOneBlock, lineTwoBlock);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				notifications.Notify(new ErrorNotification(ErrorType.Error, ex.Message + System.Environment.NewLine + ex.StackTrace));
			}
			additionalTransactionActions = documentFactories.ToArray();
			return result;
		}
	}
}
