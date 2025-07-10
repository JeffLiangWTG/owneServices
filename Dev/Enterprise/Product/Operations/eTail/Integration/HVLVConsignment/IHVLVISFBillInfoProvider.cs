using Enterprise.Integration;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVISFBillInfoProvider : IHVLVConsignment
	{
		Customs.ICusEntryNumAdditionalReferenceCollection CustomsReferenceNumbers { get; }
	}
}
