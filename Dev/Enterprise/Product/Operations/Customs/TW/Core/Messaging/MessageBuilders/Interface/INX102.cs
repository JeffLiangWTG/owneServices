using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.TW.Messaging
{
	[CodeAlive("MessageBuilder will be created later")]
	public interface INX102
	{
		IAdditionalInformation AdditionalInformation { get; }

		ZString ErrorValidationCode { get; }

		ZString StatusNameCode { get; }

		INX102Declaration Declaration { get; }
	}

	public interface INX102Declaration
	{
		ZDateTime IssueDateTime { get; }

		IAdditionalInformation AdditionalInformation { get; }

		ZString ContactOffice { get; }

		IGoodsShipment GoodsShipment { get; }

		IPreviousDocument PreviousDocument { get; }

		IApplication Application { get; }
	}
}
