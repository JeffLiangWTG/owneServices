using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using WTG.RTUS.Interface;
using WTG.RTUS.Printing;

namespace Enterprise.Packing.DataTransfer
{
	public sealed class RTUSPrinterWithoutSignalR : IRTUSPrinter
	{
		public RTUSPrinterWithoutSignalR(Action<Exception> onError)
		{
			OnError = onError;
		}

		Action<Exception> OnError { get; }

		public void Dispose()
		{
			// Nothing to dispose
		}

		public bool Print(FileType fileType, byte[] binaryData, Guid printerPK)
		{
			bool result;

			var factory = new BusinessObjectFactory { RefreshEnabled = false, NameForDebugging = "Remote Print for RTUS" }; // Debug Name for Factory

			var deliveryGroup = factory.New<IStmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;

			var printJob = factory.New<IStmPrintJob>();
			printJob.SP_Copies = 1;
			printJob.SP_CustomProperties = binaryData;
			printJob.SP_EmailAttachments = "Label." + fileType.ToString();
			printJob.SP_JobType = "PRN";
			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;
			printJob.SP_SQ = printerPK;

			try
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
				result = true;
			}
			catch (Exception ex)
			{
				result = false;

				if (OnError != null)
				{
					OnError(ex);
				}
				else
				{
					throw;
				}
			}

			return result;
		}

		public bool Print(FileType fileType, byte[] binaryData, string printerName, string printerServer)
		{
			throw new NotSupportedException("Non-SignalR printer must use printer PK.");
		}
	}
}
