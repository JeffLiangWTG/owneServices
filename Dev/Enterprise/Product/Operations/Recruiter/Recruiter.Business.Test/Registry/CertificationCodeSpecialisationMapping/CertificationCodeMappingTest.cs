using System;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(CertificationCodeMapping))]
	sealed class CertificationCodeMappingTest : RegistryBusinessObjectTest
	{
		public void TestDuplicatePair()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("XXX", (NoResString)"XXX");
			certCodes.Add("ZZZ", (NoResString)"ZZZ");
			certCodes.Add("YYY", (NoResString)"YYY");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var collection = new CertificationCodeMappingCollection();
			CertificationCodeMapping status = collection.AddPair("XXX", "YYY");
			status.ValidateMainCode();
			AssertNoErrors(status.MainCodeInfo);

			status = collection.AddPair("ZZZ", "YYY");
			status.ValidateMainCode();

			AssertNoErrors(status.MainCodeInfo);

			status = collection.AddPair("XXX", "YYY");
			status.ValidateMainCode();

			AssertHasError(status.MainCodeInfo, "The pair of codes has duplicates");
		}

		public void TestSameCode()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("XXX", (NoResString)"XXX");
			certCodes.Add("ZZZ", (NoResString)"ZZZ");
			certCodes.Add("YYY", (NoResString)"YYY");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var collection = new CertificationCodeMappingCollection();
			CertificationCodeMapping status = collection.AddPair("XXX", "XXX");
			status.ValidateMainCode();

			AssertHasError(status.MainCodeInfo, "You cannot have the same code in both cells");
		}

		public void TestOnlyOneLevel()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("XXX", (NoResString)"XXX");
			certCodes.Add("ZZZ", (NoResString)"ZZZ");
			certCodes.Add("YYY", (NoResString)"YYY");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var collection = new CertificationCodeMappingCollection();
			CertificationCodeMapping status = collection.AddPair("XXX", "YYY");
			status.ValidateMainCode();
			AssertNoErrors(status.MainCodeInfo);

			status = collection.AddPair("YYY", "ZZZ");
			status.ValidateMainCode();

			AssertHasError(status.MainCodeInfo, "You cannot have multiple levels of related codes, i.e a single code cannot appear as both Main and Specialization code");
		}

		public void TestValidationMainCode()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("XXX", (NoResString)"XXX");
			certCodes.Add("ZZZ", (NoResString)"ZZZ");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var collection = new CertificationCodeMappingCollection();
			CertificationCodeMapping status = collection.AddPair("AAA", "YYY");
			status.ValidateMainCode();

			AssertHasErrors(status.MainCodeInfo);
		}

		public void TestValidationSpecCode()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("XXX", (NoResString)"XXX");
			certCodes.Add("ZZZ", (NoResString)"ZZZ");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var collection = new CertificationCodeMappingCollection();
			CertificationCodeMapping status = collection.AddPair("XXX", "AAA");
			status.ValidateMainCode();

			AssertHasErrors(status.SpecialisationCodeInfo);
		}

		public void TestCloneProperties()
		{
			CertificationCodeMapping status = new CertificationCodeMapping();
			status.MainCode = "MEH";
			status.SpecialisationCode = "BAA";

			CertificationCodeMapping clonedStatus = (CertificationCodeMapping)status.Clone(status.CurrentFallbackLevel, status.Factory);
			AssertEquals("MEH", clonedStatus.MainCode);
			AssertEquals("BAA", clonedStatus.SpecialisationCode);
		}

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

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone1)
		{
			var clone = clone1 as CertificationCodeMapping;
			AssertEquals("MainCode", "AAA", clone.MainCode);
			AssertEquals("SpecialisationCode", "BBB", clone.SpecialisationCode);
		}

		protected override bool IsCodeMandatory => false;

		#endregion
	}
}
