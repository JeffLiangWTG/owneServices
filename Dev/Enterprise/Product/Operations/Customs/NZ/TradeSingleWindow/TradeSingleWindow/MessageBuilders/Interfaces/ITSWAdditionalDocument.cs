
namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	using CargoWise.Types;

	/// <summary>
	/// AdditionalDocument
	///		identification:	Additional document reference number	an..35
	///		type			Additional document type, coded			an..3
	///		category		Additional document category, coded		an..3
	///		image			Document Image							an..256
	/// </summary>
	public interface ITSWAdditionalDocument
	{
		ZString AdditionalDocumentNumber { get; }
		ZString AdditionalDocumentType { get; }
		ZString AttachmentType { get; }
		ZString Attachment { get; }
	}
}
