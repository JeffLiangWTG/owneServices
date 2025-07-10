using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class NZCCPValidator : EstablishmentCodeValidator
	{
		public NZCCPValidator(ZPropertyInfo propertyInfo)
			: base()
		{
			this.PropertyInfo = propertyInfo;
		}

		public static string InvalidLocationCodeFormat
		{
			get { return Res.GetString("AB492952-AD76-4CDE-86DA-C6D876924C70", "Invalid Location Code format. The format must be NNNNA or NNNNNA ('N' indicates numeric and 'A' means alphabetical)."); }
		}

		public void ValidateCCPAddError()
		{
			ZString establishmentCode = (ZString)PropertyInfo.Value;
			if (!establishmentCode.IsEmpty)
			{
				if (!Regex.IsMatch(establishmentCode, @"^[0-9]{4,5}[A-Z]{1}$"))
				{
					PropertyInfo.AddErrorIfEnforced(InvalidLocationCodeFormat, OrganisationRegistry.RegistrationNumberFormatFields.NZCCP);
				}
			}
		}

		readonly ZPropertyInfo PropertyInfo;
	}
}
