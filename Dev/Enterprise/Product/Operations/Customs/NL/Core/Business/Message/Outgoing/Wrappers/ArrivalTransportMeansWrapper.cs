using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.NL.Business;

public class ArrivalTransportMeansWrapper : IArrivalTransportMeans
{
	public ArrivalTransportMeansWrapper(JobDeclaration declaration)
	{
		this.declaration = declaration;
	}

	readonly JobDeclaration declaration;

	public string Id => IsImportConditionsChecked() ? declaration.JE_TransportIDInland : string.Empty;
	public string IdentificationTypeCode => IsImportConditionsChecked() ? declaration.JE_TransportMeans.ToString() : string.Empty;
	public string ModeCode => WrapperHelper.ConvertTransportMode(declaration.JE_TransportModeInland);

	bool IsImportConditionsChecked()
	{
		var requiredDeclarationTypes = new ZString[]
		{
			DeclarationTypeList.Codes.H1,
			DeclarationTypeList.Codes.H3,
			DeclarationTypeList.Codes.H4,
			DeclarationTypeList.Codes.H5,
			DeclarationTypeList.Codes.I1
		};
		var excludedTransportModes = new ZString[]
		{
			TransportModes.Mail,
			TransportModes.FixedTransportInstallations
		};
		bool hasRequiredDeclarationType = declaration.CustomsEntryInstructions.Any(entry => requiredDeclarationTypes.Contains(entry.CEI_Style));
		bool isExcludedTransportMode = excludedTransportModes.Contains(declaration.JE_TransportModeInland);

		return hasRequiredDeclarationType && !isExcludedTransportMode;
	}
}

