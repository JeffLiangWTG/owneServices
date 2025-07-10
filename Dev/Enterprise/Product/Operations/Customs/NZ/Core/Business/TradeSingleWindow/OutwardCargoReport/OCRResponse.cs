using Enterprise.Customs.Common;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class OCRResponse : CargoReportResponse
	{
		public OCRResponse(BaseTSWResponse response)
			: base(response)
		{
		}

		public INZManifestHeader SendingBusinessObject => (INZManifestHeader)LinkedObject;

		public bool IsLinkedToManifestHeader => SendingBusinessObject.JobName == "AsycudaManifestHeader";

		#region Overrides

		protected override bool GetIsImportEntry() => false;

		protected override string GetJobID()
		{
			return SendingBusinessObject.JobNumber;
		}

		protected override string GetJobName()
		{
			var result = "";
			if (SendingBusinessObject != null)
			{
				if (IsLinkedToManifestHeader)
				{
					result = "Global Manifest";
				}
				else
				{
					result = SendingBusinessObject.JobName;
				}
			}

			return result;
		}

		protected override string GetEnterpriseStatus()
		{
			if (IsCancellation)
			{
				return OutwardReportStatusList.Codes.Cancelled;
			}
			else
			{
				switch (Status)
				{
					case StatusList.Codes.EntryHeldInstructionsAsSpecified:
						return OutwardReportStatusList.Codes.CustomsInstuctionReceived;
					case StatusList.Codes.AdjustmentAccepted:
						return OutwardReportStatusList.Codes.Cleared;
					case StatusList.Codes.EciOutwardReportRejectedErrorReportAttached:
						return OutwardReportStatusList.Codes.Rejected;
					case StatusList.Codes.OutwardReportAccepted:
					case StatusList.Codes.ExportGoodsClearedPortNotification:
						return OutwardReportStatusList.Codes.Cleared;
					case StatusList.Codes.Acknowledgement:
						return OutwardReportStatusList.Codes.Acknowledgement;
					default:
						return "";
				}
			}
		}

		protected override string GetEnterpriseStatusDescription()
		{
			switch (EnterpriseStatus)
			{
				case OutwardReportStatusList.Codes.Cancelled:
					return "Outward Report Cancelled";
				case OutwardReportStatusList.Codes.Cleared:
					return "Outward Report Accepted";
				case OutwardReportStatusList.Codes.CustomsInstuctionReceived:
					return "Customs Instuctions as specified";
				case OutwardReportStatusList.Codes.Rejected:
					return "Outward Report Rejected";
				case OutwardReportStatusList.Codes.Acknowledgement:
					return "OCR Acknowledged";
				default:
					return "";
			}
		}

		public string GetEventReference()
		{
			switch (Status)
			{
				case StatusList.Codes.EntryRestored:
				case StatusList.Codes.AdjustmentAccepted:
				case StatusList.Codes.OutwardReportAccepted:
					return "ACC";
				case StatusList.Codes.EciOutwardReportRejectedErrorReportAttached:
				case StatusList.Codes.CustomsProcessingError:
					return "FAL";
				case StatusList.Codes.EntryCancelled:
					return "CAN";
				default:
					return "";
			}
		}

		#endregion // Overrides
	}
}
