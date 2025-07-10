using CargoWise.Types;
using Enterprise.Edifact.D16A.Elements;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CALINF
{
	public interface ICALINFCallInformation : ILOC_LocationInformation, IDTM_DateTimeInformation
	{
	}

	public interface ILOC_LocationInformation
	{
		ZString CallLocation { get; }

		ZString LocationCodeListIdentificationCode { get; }

		CodeListResponsibleAgencyCodeList LocationCodeListResponsibleAgencyCode { get; }
	}

	public interface IDTM_DateTimeInformation
	{
		ZDateTime CallDateTime { get; }
	}
}
