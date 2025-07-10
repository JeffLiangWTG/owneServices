using System;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(CertificationCodeMapping))]
	sealed class CertificationCodeMappingTemplateTest : RegistryBusinessObjectTemplateTestCase<CertificationCodeMapping>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		CertificationCodeMapping NewPopulatedBusinessObject()
		{
			CertificationCodeMapping result = new CertificationCodeMapping(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty));
			result.MainCode = "AAA";
			result.SpecialisationCode = "BBB";
			return result;
		}

		protected override CertificationCodeMapping GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override CertificationCodeMapping GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		#endregion
	}
}
