using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Integration
{
	public interface IProcessTaskNotification : ITriggerAction
	{
		IBaseTrigger Parent { get; }
		ZString PQ_EmailAddr { get; set; }
		ZString PQ_EmailText { get; set; }
		ZString PQ_FieldName { get; set; }
		ZString PQ_FieldNameTrimmed { get; }
		ZString PQ_FieldValue { get; set; }
		ZString PQ_MessagePurpose { get; set; }
		ZGuid PQ_P9 { get; set; }
		ZGuid PQ_SQ { get; set; }
		ZGuid PQ_SU_Document { get; set; }
		ZString PQ_TriggerParty { get; set; }
		ZString PQ_TriggerType { get; set; }
		string GetDiagnosticLogInfo();
	}
}
