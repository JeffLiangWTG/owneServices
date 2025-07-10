using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class VietnamVATCodeValidatorTest : TestCaseWithFactory
	{
		string CodeType => OrgCusCode.CodeTypes.VATCode;
		string CountryCode => Core.Constants.CountryCodes.VietNam;

		readonly ZString[] validCodes = new ZString[] { "0100233488", "0100233488-999", "8122585590" };

		readonly ZString[] invalidCodes = new ZString[] { "123456789", "12345678901", "1234567890-12", "1234567890-000", "1234567890-1234", "1234567890_123", "0034567890", "6534567890", "0100000001", "0104128565-000", "0100233481-999" };

		const string partialErrorMessage = @"It should be in format 'NNNNNNNNNN' or 'NNNNNNNNNN-NNN' and complies with check digit validation.";

		public void TestVATCodeValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation_VNVAT(CountryCode, CodeType, invalidCodes, validCodes, partialErrorMessage, OrganisationRegistry.RegistrationNumberFormatFields.VNVAT);
		}
	}
}
