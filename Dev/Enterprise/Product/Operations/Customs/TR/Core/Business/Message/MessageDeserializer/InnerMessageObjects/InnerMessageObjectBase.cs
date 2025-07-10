using System.Globalization;
using System.Xml;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.TR.Business
{
	public class InnerMessageObjectBase
	{
		[CodeAlive("Don't know why this is picked up by ReflectionTest.DeadCodeTest. Obviously it is used in many places.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Localised strings")]
		public static class Constants
		{
			public const string Guid = "GUID";
			public const string RefId = "RefID";
			public const string RegistrationNo = "TescilNo";
			public const string RegistrationTime = "TescilTarihi";
			public const string Result = "Sonuc";
			public const string ResultInfo = "SonucBilgisi";
			public const string Situation = "Durum";
			public const string DiffGramResultProcess = "Islem";
			public const string DefaultDateFormat = "dd/MM/yyyy";
			public const string DefaultDateTimeFormat = "dd/MM/yyyy HH:mm:ss";
			public const string DateTimeFormatUtc = "yyyy-MM-ddTHH:mm:ss";
			public const string DateTimeFormatUtcWithTimeZone = "yyyy-MM-ddTHH:mm:ss.fffzzz";

			public static (string ElementName, string NamespaceUrl)[] DiffGramResultPath => new (string, string)[]
			{
				(TRMessageConstants.Xml.DiffGramElementName, TRMessageConstants.Xml.DiffGramNamespace),
				(TRMessageConstants.Xml.ResultElementName, string.Empty),
			};
		}

		public InnerMessageObjectBase(XmlNode node) { }

		public InnerMessageObjectBase(string messageText) : this((XmlNode)null)
		{
			MessageText = messageText;
		}
		public string MessageText { get; }

		protected ZGuid TryGetGuid(string input) => ZGuid.TryParse(input, out var parsedGuid) ? parsedGuid : ZGuid.Invalid;

		protected ZInt TryGetInt(string input) => ZInt.TryParse(input, out var parsedInt) ? parsedInt : ZInt.Zero;

		protected ZDecimal TryGetDecimal(string input) => ZDecimal.TryParse(input, out var parsedDecimal) ? parsedDecimal : ZDecimal.Zero;

		protected ZDateTime TryGetDateTime(string input) =>
			ZDateTime.TryParseExact(input, out var parsedDateTime, Constants.DefaultDateTimeFormat) ? parsedDateTime :
			ZDateTime.TryParseExact(input, out var parsedDate, Constants.DefaultDateFormat) ? parsedDate :
			ZDateTime.TryParseExact(input, out var parsedDateTimeUtc, Constants.DateTimeFormatUtc) ? parsedDateTimeUtc :
			ZDateTime.TryParseIgnoreTimezone(input, CultureInfo.InvariantCulture, out var parsedDateTimeOther) ? parsedDateTimeOther :
			ZDateTime.Invalid;

		protected ZString TryGetInnerText(XmlNode node, string fieldName) => node[fieldName]?.InnerText ?? ZString.Empty;
	}
}
