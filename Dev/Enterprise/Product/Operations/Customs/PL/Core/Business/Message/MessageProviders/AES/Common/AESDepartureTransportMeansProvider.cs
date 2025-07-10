using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class AESDepartureTransportMeansProvider : IDepartureTransportMeans
{
	public AESDepartureTransportMeansProvider(JobDeclaration declaration, int sequenceNumber)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
		this.sequenceNumber = sequenceNumber;
	}

	readonly JobDeclaration declaration;
	readonly int sequenceNumber;

	public int SequenceNumber => sequenceNumber;

	public string TypeOfIdentification => declaration.JE_TransportMeans;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber, () => MessageProviderHelper.GetLabelingOfTransportAtArrival(declaration));
	CachedValue<string> identificationNumber;

	public string Nationality => CachedValueHelper.GetValue(ref nationality, GetNationality);
	CachedValue<string> nationality;

	string GetNationality()
	{
		var result = declaration.JE_RN_NKTransportNationalityInland;

		if ((TypeOfIdentification == ExportBorderTransportMeansList.Codes._10
			|| TypeOfIdentification == ExportBorderTransportMeansList.Codes._11)
			&& declaration.VesselInland is RefVessel vessel
			&& !vessel.RV_RN_NKCountryOfReg.IsEmpty)
		{
			result = vessel.RV_RN_NKCountryOfReg;
		}

		return result;
	}
}
