using System;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(CertificationCodeMappingDataType))]
	sealed class CertificationCodeMappingDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CertificationCodeMappingDataType>
	{
		protected override CertificationCodeMappingDataType GetNewDataType()
		{
			return new CertificationCodeMappingDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "CertificationCodeMappingRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			CertificationCodeMappingCollection collection1 = new CertificationCodeMappingCollection();
			CertificationCodeMapping mapping1 = collection1.AddNew();
			mapping1.MainCode = "CUS";
			mapping1.SpecialisationCode = "XXX";

			CertificationCodeMappingCollection collection2 = new CertificationCodeMappingCollection();
			CertificationCodeMapping mapping2 = collection2.AddNew();
			mapping2.MainCode = "ZZZ";
			mapping2.SpecialisationCode = "YYY";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, DataType.Serialise(collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, DataType.Serialise(collection2))
			};
		}

		protected override void SetUp()
		{
			base.SetUp();
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("XXX", (NoResString)"XXX");
			certCodes.Add("YYY", (NoResString)"YYY");
			certCodes.Add("ZZZ", (NoResString)"ZZZ");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);
		}
	}
}
