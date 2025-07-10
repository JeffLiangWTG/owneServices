using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Recruiter.Business.RecruiterDataRegistry;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(AllowAnonymousCertificateUserRegistrationRegistryDataType))]
	sealed class AllowAnonymousCertificateUserRegistrationRegistryDataTypeTest : RegistryDataTypeTestCase<AllowAnonymousCertificateUserRegistrationRegistryDataType>
	{
		protected override AllowAnonymousCertificateUserRegistrationRegistryDataType GetNewDataType()
		{
			return new AllowAnonymousCertificateUserRegistrationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var result = false;
			var result2 = true;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(result, new AllowAnonymousCertificateUserRegistrationRegistryDataType().Serialise(result)),
				new ValidSampleAndBinaryValueInDB(result2, new AllowAnonymousCertificateUserRegistrationRegistryDataType().Serialise(result2))
			};
		}
	}
}
