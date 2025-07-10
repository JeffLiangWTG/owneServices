using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D96B.Segments;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public class ResendableResponseInformation
	{
		public ResendableResponseInformation(FTXSegment ftx, BusinessObjectFactory factory)
		{
			fTX = ftx;
			this.factory = factory;
		}

		readonly FTXSegment fTX;
		readonly BusinessObjectFactory factory;

		public bool HasValidVersion => !string.IsNullOrEmpty(fTX.TextLiteral.FreeText2.Trim());
		public ZString Version => fTX.TextLiteral.FreeText2.Substring(fTX.TextLiteral.FreeText2.IndexOf("=", StringComparison.Ordinal) + 1);
		public ZString Status => fTX.TextLiteral.FreeText4.Substring(fTX.TextLiteral.FreeText4.IndexOf("=", StringComparison.Ordinal) + 1);
		public ZString Recipient => fTX.TextLiteral.FreeText5.Substring(fTX.TextLiteral.FreeText5.IndexOf("=", StringComparison.Ordinal) + 1);
		public ZDateTime ProcessedDate
		{
			get
			{
				var start = fTX.TextLiteral.FreeText3.LastIndexOf("=", StringComparison.Ordinal) + 1;
				var date = fTX.TextLiteral.FreeText3.Substring(start, Math.Min(14, fTX.TextLiteral.FreeText3.Length - start));
				var result = ZDateTime.Empty;
				ZDateTime.TryParseExact(date, out result, "yyyyMMddHHmmss");
				return result;
			}
		}
		public string DisplayName
		{
			get
			{
				var statusDescription = ZARefCusCodeListTypes.GetCustomsStatusList(factory).GetDescriptionFromCode(Status);
				return new ZStringBuilder("Status ")
					.Append(Status)
					.Append(" - ")
					.Append(statusDescription ?? "")
					.Append(" (")
					.Append(ProcessedDate.ToString())
					.Append(")").ToString();
			}
		}
	}
}
