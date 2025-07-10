
namespace Enterprise.Customs.SG.V4.Business
{
	public interface ICusMessage
	{
		string MessageText { get; }
		string MessageType { get; }
		string MessageSubType { get; }
	}
}
