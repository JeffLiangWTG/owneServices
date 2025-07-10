using System;
using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.Customs.US.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class FilingInfoTypeProvider : IFilingInfoType
	{
		readonly USExportAsycudaManifestHeader header;

		public FilingInfoTypeProvider(USExportAsycudaBill bill, string messageType)
		{
			this.header = Argument.NotNull(bill.Header, "bill.Header cannot be null");
			MessageType = new ManifestStringTypeProvider(messageType);
		}

		public const string AirSenderID = "WTGWTG";
		public const string SeaSenderID = "8CAR";
		public IManifestStringType SenderId => header.IsAir ? new ManifestStringTypeProvider(AirSenderID) : (header.IsSea ? new ManifestStringTypeProvider(SeaSenderID) : new ManifestStringTypeProvider(GlbCompany.CurrentCompany.GetSenderIDFromOrgProxy(header.IsAir)));

		public IManifestStringType ReceiverId => new ManifestStringTypeProvider("CUSTOMS");

		public IManifestStringType MessageDateTime
		{
			get
			{
				var easternStandardTime = TimeZoneInfo.ConvertTimeFromUtc(ZDateTime.UtcNow.ToDateTime(), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
				return new ManifestStringTypeProvider(easternStandardTime.ToString("yyyyMMdd hhmmss"));
			}
		}

		public IManifestStringType MessageType { get; }

		public IManifestStringType MessageControlNumber => new ManifestStringTypeProvider(header.AMA_JobReference);

		public IManifestStringType MessageReferenceNumber => new ManifestStringTypeProvider(UEMEDIMessage.UEMMessageNumberPlaceHolder);

		public Collection<IErrorType> ResponseMessage => new Collection<IErrorType>(Array.Empty<ErrorTypeProvider>());
	}
}
