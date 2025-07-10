using Enterprise.Customs.Common;

namespace Enterprise.Freight.Business
{
	public interface ISupportInspectionType
	{
		CusEntryNumber AdditionalInspectionType { get; }
		CusEntryNumber InspectionType { get; }
	}
}
