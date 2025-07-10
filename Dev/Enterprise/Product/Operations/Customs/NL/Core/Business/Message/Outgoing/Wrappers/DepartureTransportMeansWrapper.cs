using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class DepartureTransportMeansWrapper : IDepartureTransportMeans
{
	DepartureTransportMeansWrapper(JobDeclaration jobDeclaration, int sequenceNumeric, ZString id, ZString identificationTypeCode, ZString registrationNationalityCode)
	{
		declaration = Argument.NotNull(jobDeclaration, nameof(jobDeclaration));
		SequenceNumeric = sequenceNumeric;
		Id = id;
		IdentificationTypeCode = identificationTypeCode;
		RegistrationNationalityCode = registrationNationalityCode;
	}
	readonly JobDeclaration declaration;

	public int SequenceNumeric { get; }

	public string Id { get; }

	public string IdentificationTypeCode { get; }

	public string RegistrationNationalityCode { get; }

	public string ModeCode => WrapperHelper.ConvertTransportMode(declaration.JE_TransportModeInland);

	public string Nationality => string.Empty;

	public static DepartureTransportMeansWrapper New(JobDeclaration jobDeclaration, int sequenceNumeric, ZString id, ZString identificationTypeCode, ZString registrationNationalityCode)
	{
		var isInvalid = sequenceNumeric > 0
			&& id.IsEmpty
			&& identificationTypeCode.IsEmpty
			&& registrationNationalityCode.IsEmpty
			&& WrapperHelper.ConvertTransportMode(jobDeclaration?.JE_TransportModeInland).IsNullOrEmpty();

		return isInvalid ? null : new DepartureTransportMeansWrapper(jobDeclaration, sequenceNumeric, id, identificationTypeCode, registrationNationalityCode);
	}
}
