using System;
using System.Net;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Schema;
using WTG.RTUS.Interface;
using WTG.RTUS.Printing;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class RTUSLabelPrinting : ILabelPrintingService
	{
		public RTUSLabelPrinting(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public bool TryPrintLabel(byte[] binaryData, string fileType, Guid printerPK, out string errorMessage)
		{
			var result = false;
			errorMessage = string.Empty;
			ETailRTUSPrinter rtusPrinter = null;

			var printer = GetPrinter(printerPK);
			if (printer != null)
			{
				try
				{
					rtusPrinter = GetRTUSPrinter();
					if (!Enum.TryParse(fileType, out FileType rtusFileType))
					{
						rtusFileType = FileType.Unknown;
					}

					result = rtusPrinter.Print(rtusFileType, binaryData, printer.QueueName, printer.SQ_ServerName);
				}
				catch (WebException wex)
				{
					var errorResponseContent = string.Empty;
					var errorResponse = wex.Response?.GetResponseStream()?.ReadFully();
					if (errorResponse != null && errorResponse.Length > 0)
					{
						errorResponseContent = Encoding.Default.GetString(wex.Response?.GetResponseStream()?.ReadFully());
					}

					errorMessage = Res.GetString(
						"8ecdf585-a207-434a-864a-bcd4fa6d1fdf",
						"Failed to connect to remote printer due to network error: {0}, connection status: {1}, response: {2}",
						wex.Message,
						wex.Status,
						errorResponseContent);
					return false;
				}
				catch (UriFormatException uex)
				{
					errorMessage = Res.GetString("0f6319ea-e9cd-4bd4-933e-b5de12826184", "{0}\r\nPlease ensure the URL in 'Registry > Transport > RTUS > Remote Print Server URL for RTUS' is valid.", uex.Message);
					return false;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					#region SuppressResourceStringsCheckRegion
					var detailsForDebug = FormattableString.Invariant($@"Unexpected exception while printing: 
File size: {binaryData?.Length ?? -1}
File type: {fileType}
Printer PK: {printerPK}
Printer name: {printer.QueueName}
Printer host: {printer.SQ_ServerName}
Remote printing server: {rtusPrinter.PrintingServiceUrl.AbsoluteUri}
Last action: {rtusPrinter.LastAction}
{(rtusPrinter.IsReconnecting ? "Printer was trying to reconnect" : string.Empty)}
{(rtusPrinter.Reconnected ? "Printer was reconneted" : string.Empty)}
Last error message: {rtusPrinter.LastErrorMessage}");
					#endregion
					ErrorReporter.ReportOnce("eTailLabelPrinting|ExceptionMessage", detailsForDebug, ex);
				}

				if (!result)
				{
					errorMessage = Res.GetString("85b86121-6c37-4b32-bb8e-4db64a2dbfb4",
						"Error occurred while printing label.\r\n{0}\r\nPrinter: {1}@{2}",
						rtusPrinter.LastErrorMessage,
						printer.QueueName,
						printer.SQ_ServerName);
				}
			}
			else
			{
				errorMessage = Res.GetString("8ecb8f13-3fa0-4952-aee0-fefd58a493f5", "Cannot find printer [{0}]", printerPK);
			}

			return result;
		}

		#region ILabelPrinting implementation

		IStmPrintQueue GetPrinter(ZGuid printerPK)
		{
			var query = new ZQuery(StmPrintQueueSchema.PK, printerPK);
			query.AddToFilter(StmPrintQueueSchema.SQ_AllowPrinting, true);
			query.AddToFilter(StmPrintQueueSchema.SQ_QueueDeleted, null);

			var printers = (BusinessObjectCollection)ObjectFactory.Get<IStmPrintQueueCollection>("IStmPrintQueueCollection", factory, query);
			printers.Load();
			return (IStmPrintQueue)printers.FindByPK(printerPK);
		}

#if DEBUG
		protected virtual
#endif
		ETailRTUSPrinter GetRTUSPrinter()
		{
			var remotePrintingServerRaw = ObjectFactory.Get<ITransportRegistry>().RemotePrintServerURL.Value;
			var remotePrintingUserName = WebDataRegistry.Instance.WebServiceUsername.Value;
			var remotePrintingPassword = WebDataRegistry.Instance.WebServicePassword.Value;

			var config = new RemotePrintServerConfig(new Uri(remotePrintingServerRaw, UriKind.Absolute), remotePrintingUserName, remotePrintingPassword);
			var printer = new ETailRTUSPrinter(config);
			return printer;
		}

		#endregion
	}

	public class ETailRTUSPrinter : IRTUSPrinter
	{
		public ETailRTUSPrinter(RemotePrintServerConfig config)
		{
			inner = new RTUSPrinter(
				config,
				() => Closed = true,
				r => LastAction = r,
				err => LastErrorMessage = err.GetInnermostException().Message,
				() =>
				{
					IsReconnecting = true;
					Reconnected = false;
				},
				() =>
				{
					IsReconnecting = false;
					Reconnected = true;
				});
			PrintingServiceUrl = config.Url;
		}

		public bool Closed { get; private set; }
		public bool IsReconnecting { get; private set; }
		public bool Reconnected { get; private set; }
		public string LastAction { get; private set; }
		public string LastErrorMessage { get; private set; }

		public Uri PrintingServiceUrl { get; }

		readonly RTUSPrinter inner;
		public bool Print(FileType fileType, byte[] binaryData, string printerName, string printerServer)
		{
			return PrintCore(fileType, binaryData, printerName, printerServer);
		}

		protected virtual bool PrintCore(FileType fileType, byte[] binaryData, string printerName, string printerServer)
		{
			return inner.Print(fileType, binaryData, printerName, printerServer);
		}

		public bool Print(FileType fileType, byte[] binaryData, Guid printerPK)
		{
			return ((IRTUSPrinter)inner).Print(fileType, binaryData, printerPK);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				inner.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
