using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	internal static class SoapMessageTextHelper
	{
		public static ZString GetResponseGuid(string messageText) => TRMessageHelper.GetNodeValue(messageText, "//x:Root/Response/Guid", "http://schemas.microsoft.com/BizTalk/2003/Any");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Localised strings")]
		public static ZString GetOutputMessageGuid(string messageText) => TRMessageHelper.GetNodeValue(messageText, new ZString[] { "Envelope", "Body", "OutputMessage", "Record", "Sonuc" }, "GUID");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Localised strings")]
		public static ZString GetOutputMessageStatus(string messageText) => TRMessageHelper.GetNodeValue(messageText, new ZString[] { "Envelope", "Body", "OutputMessage", "Record", "Sonuc" }, "Durum");

		public static class Constants
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html elements")]
			public static class Html
			{
				public const string DefaultTableClass = "table";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Localised strings")]
			public static class OutputMessage
			{
				public const string SigningCardAndUserInfoIncompatible = "Elektronik İmza ile kullanıcı kodu üzerindeki bilgiler uyumsuz.";
				public const string ProcessHasStarted = "İşleminiz başlamıştır.Teşekkür ederiz.";
				public const string TextSentBackEmpty = "The message text sent back from Customs is empty.";
				public const string NoRegistrationInfo = "No Registration Information in the message content sent back from customs.";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Localised strings")]
			public static class DocumentCodes
			{
				public const string Code7780Desc = "DIKKAT!!! Beyanname tescil edilecek ve otomatik olarak hat bildirimi (onay) islemi yapilacaktir. Bu asamadan sonra beyannamede düzeltme yapilamaz! Düzeltme yapmak istiyormusunuz? (E/H)";
				public const string Code5178Desc = "Bu ithalat/ihracat elektronik ticaret (eticaret) midir ?";
				public const string Code5063Desc = "Beyan ettiginiz esyaniz Bazi Tarim Ürünlerinin Ihracatinda ve Ithalatinda Ticari Kalite Denetimi Tebligi kapsaminda ya da bu Teblig'de yer alan sabit TAREKS referans numarasi beyanina tabi midir?";
			}
		}

		public static string[] RowHeaders_Labelvalue => new[]
		{
			Res.GetString("7E04F563-15EE-48D7-A929-1575EFD5D4E7", "Label"),
			Res.GetString("4B27B9B8-4BC1-4800-A318-AE828A9F5445", "Value")
		};

		public static string[] RowHeaders_ErrorCodeAndDescription => new[]
		{
			Res.GetString("95660BAD-4A50-44B3-8247-160241F7E2FF", "Error Code"),
			Res.GetString("ECBDD463-7EF2-4270-AEC4-E8464696BD8E", "Error Description")
		};

		public static string TableHeaderWithErrorMessage => Res.GetString("612626AC-A9E0-4D59-B3BD-D6F18FA50357", "Error Message");

		public static string[] TableHeaderSoapMessage => new[]
		{
			Res.GetString("B37F4C20-A8D5-495A-83F9-0115D320DF1C", "SOAP Message")
		};

		public static string RowHeader_RegistrationNumber => Res.GetString("E12F079B-40E6-4D79-9F09-EC890AC6E4BE", "Registration Number:");

		public static string RowHeader_IssueDate => Res.GetString("03ED4705-7569-4652-AB5D-14DCE93FFAD0", "Issue Date:");

		public static string[] Row_EmptyResponse => new[] {
			Res.GetString("D6529EC2-422C-4681-ADEC-85D690DAFF6E", "Error Message:"),
			Res.GetString("B53BB70D-6984-42A8-B11B-77667F614DED", "The message text sent back from Customs is empty, please try to resend original message")
		};

		public static string[] TableHeaderWithQuestionsFields => new[]
		{
			Res.GetString("F9D9A4D3-34E4-4427-BFF8-5B45EC62970D", "Code"),
			Res.GetString("2D21001E-E331-4050-8D0F-E548A9B635F2", "Description"),
			Res.GetString("2F58247C-4346-491C-9715-A85BDA391861", "Line Number"),
			Res.GetString("87A9FF0D-889F-4644-BA86-2319DA79138C", "Type")
		};

		public static string[] TableHeaderWithDocumentsFields => new[]
		{
			Res.GetString("01CDFEA6-F57A-4CE9-93C7-BFE3D180A518", "Line Number"),
			Res.GetString("CD6763E9-C07B-43C2-8A36-FBB61D6AA3E0", "Code"),
			Res.GetString("33D610BB-8059-4B68-916C-28ECF0ACC700", "Description"),
		};

		public static string[] TableHeaderWithTaxesFields => new[]
		{
			Res.GetString("AAD306F0-41FC-4D29-B261-F11E920ADC62", "Line Number"),
			Res.GetString("E1150FAB-6B56-401A-9E6B-6BB81D56C630", "Code"),
			Res.GetString("0898608E-D319-4B5D-AE74-676813C10939", "Description"),
			Res.GetString("71FE92D3-084D-461B-8A8C-F10523629F97", "Amount"),
			Res.GetString("04D4DDAB-DE01-4338-BE32-88FB6199C451", "Rate"),
			Res.GetString("D994DD78-1938-4ADA-8431-BAB7CAD75FF5", "Payment Type"),
			Res.GetString("E21934EA-87C1-4913-BEEF-8B4071EC59FA", "Tax Assessment"),
		};

		public static string[] TableHeaderTRNRecieveFields => new string[]
		{
			ResString.GetMultilingualString("7E04F563-15EE-48D7-A929-1575EFD5D4E7", "Label"),
			ResString.GetMultilingualString("4B27B9B8-4BC1-4800-A318-AE828A9F5445", "Value"),
		};

		public static string[] TableHeaderT1NRecieveDescFields => new string[]
		{
			ResString.GetMultilingualString("B7BB5C93-A819-4B8E-847C-DD945892B8E5", "Message Code"),
			ResString.GetMultilingualString("9D63C09B-924A-467A-81F0-2B548E496F58", "Description"),
		};

		public static string[] TableHeaderT1NRecieveIndexFields => new string[]
		{
			ResString.GetMultilingualString("68BF3AA9-C69D-45D1-B468-F68072442A86", "Message Code"),
			ResString.GetMultilingualString("42610245-BA14-42B5-B97F-4E6E1911EFE5", "Description"),
			ResString.GetMultilingualString("48866292-C95E-40E1-AC6F-B9226B3F7D01", "Index No")
		};

		public static string[] TableHeaderT1NRecieveMessageFields => new string[]
		{
			ResString.GetMultilingualString("5B41188E-2467-4C63-A46C-1D9D8E89FC6D", "Response Message")
		};

		public static string[] TableHeaderT2NRecieveFields => new string[]
		{
			ResString.GetMultilingualString("D203293E-F472-41FC-84D4-6B043B6012C7", "Message Code"),
			ResString.GetMultilingualString("A730DC38-4808-49F3-8A5F-3371CCC88717", "Description"),
			ResString.GetMultilingualString("6A9ADA44-4878-4295-A7C2-2021778730B5", "Response Message")
		};
	}
}
