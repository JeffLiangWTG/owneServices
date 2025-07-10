using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AUCCPValidator : EstablishmentCodeValidator
	{
		public AUCCPValidator(ZPropertyInfo propertyInfo) : base()
		{
			this.PropertyInfo = propertyInfo;
		}

		public static string InvalidEstablishmentCode
		{
			get { return Res.GetString("9067efe8-3b93-4ac5-b7b2-bd4e46b87228", "Invalid Establishment Code, expected checksum is:") + " "; }
		}
		public static string InvalidEstablishmentCodeFormat
		{
			get { return Res.GetString("7c35a250-ee4a-4403-807c-b42c8fa0c229", "Invalid establishment code format. The format could be any of NNNNA, ANNNA or AANNA ('A' means alphabetical and 'N' indicates numeric)."); }
		}

		public void ValidateCCPAddError()
		{
			ZString establishmentCode = (ZString)PropertyInfo.Value;
			if (establishmentCode != "")
			{
				if (CodeIsNNNNA(establishmentCode) || CodeIsANNNA(establishmentCode) || CodeIsAANNA(establishmentCode))
				{
					char checkSum;
					if ((CodeIsNNNNA(establishmentCode) && !EstablishmentCodeValidNNNNA(establishmentCode, out checkSum)) ||
						(CodeIsANNNA(establishmentCode) && !EstablishmentCodeValidANNNA(establishmentCode, out checkSum)) ||
						(CodeIsAANNA(establishmentCode) && !EstablishmentCodeValidAANNA(establishmentCode, out checkSum)))
					{
						PropertyInfo.AddWarning(InvalidEstablishmentCode + checkSum);
					}
				}
				else
				{
					PropertyInfo.AddErrorIfEnforced(InvalidEstablishmentCodeFormat, OrganisationRegistry.RegistrationNumberFormatFields.AUCCP);
				}
			}
		}

		readonly ZPropertyInfo PropertyInfo;
	}
}
