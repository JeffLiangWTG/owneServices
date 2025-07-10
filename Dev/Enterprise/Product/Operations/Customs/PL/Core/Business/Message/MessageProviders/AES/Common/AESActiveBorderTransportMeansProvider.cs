using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class AESActiveBorderTransportMeansProvider : IActiveBorderTransportMeans
{
	public AESActiveBorderTransportMeansProvider(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}

	readonly JobDeclaration declaration;

	public string TypeOfIdentification => declaration.ZG_BorderTransportMeans;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber,
		() => !CheckR0076() ? IdentificationNumberValue : IdentificationNumberValue.ToUpper());
	CachedValue<string> identificationNumber;

	public string Nationality => CachedValueHelper.GetValue(ref nationality, GetNationality);
	CachedValue<string> nationality;

	string IdentificationNumberValue
	{
		get
		{
			var identification = TypeOfIdentification;
			switch (identification)
			{
				case ExportBorderTransportMeansList.Codes._10:
					return declaration.IsSea
						? declaration.JE_LloydsIMO
						: declaration.JE_VesselName;
				case ExportBorderTransportMeansList.Codes._40:
				case ExportBorderTransportMeansList.Codes._41:
					return declaration.JE_VoyageFlightNo;
				case ExportBorderTransportMeansList.Codes._11:
				case ExportBorderTransportMeansList.Codes._21:
				case ExportBorderTransportMeansList.Codes._30:
				case ExportBorderTransportMeansList.Codes._80:
				case ExportBorderTransportMeansList.Codes._81:
				default:
					return declaration.JE_VesselName;
			}
		}
	}

	string GetNationality()
	{
		var result = declaration.JE_RN_NKTransportNationality;

		if ((TypeOfIdentification == ExportBorderTransportMeansList.Codes._10
			|| TypeOfIdentification == ExportBorderTransportMeansList.Codes._11)
			&& declaration.Vessel is RefVessel vessel
			&& !vessel.RV_RN_NKCountryOfReg.IsEmpty)
		{
			result = vessel.RV_RN_NKCountryOfReg;
		}

		return result;
	}

	bool CheckR0076()
	{
		var identification = TypeOfIdentification;
		return identification == ExportBorderTransportMeansList.Codes._10
				|| identification == ExportBorderTransportMeansList.Codes._21
				|| identification == ExportBorderTransportMeansList.Codes._30
				|| identification == ExportBorderTransportMeansList.Codes._40
				|| identification == ExportBorderTransportMeansList.Codes._41
				|| identification == ExportBorderTransportMeansList.Codes._80;
	}
}
