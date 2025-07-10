using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class BorderTransportMeansWrapper : IBorderTransportMeans
{
	public BorderTransportMeansWrapper(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}
	readonly JobDeclaration declaration;

	public string RegistrationNationalityCode => declaration.TransportNationality?.Code;

	public string ModeCode => WrapperHelper.ConvertTransportMode(declaration.TransportMode);

	public string Id => IdentificationType switch
	{
		TransportTypeIdList.Codes._10 => declaration.Vessel?.RV_LloydsNumber,
		TransportTypeIdList.Codes._11 or
		TransportTypeIdList.Codes._21 or
		TransportTypeIdList.Codes._30 or
		TransportTypeIdList.Codes._80 or
		TransportTypeIdList.Codes._81 => declaration.JE_VesselName,
		TransportTypeIdList.Codes._40 or
		TransportTypeIdList.Codes._41 => declaration.JE_VoyageFlightNo,
		_ => string.Empty
	};

	public string IdentificationType => declaration.ZG_BorderTransportMeans;
}
