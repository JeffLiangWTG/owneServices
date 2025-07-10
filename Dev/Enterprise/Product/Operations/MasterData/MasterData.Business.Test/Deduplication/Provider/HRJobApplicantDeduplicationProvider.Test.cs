using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	public class HRJobApplicantDeduplicationProviderTest : TestCaseWithFactory
	{
		public void TestDeduplicationProvider()
		{
			var provider = new HRJobApplicantDeduplicationProvider();

			CombineAssertions(() =>
			{
				AssertEquals(DeduplicationDisplayMode.Detailed, provider.DisplayModeForType);
				AssertEquals(DeduplicationProvider.Constants.Applicant, provider.GroupNameForType);
				AssertEquals(typeof(IHRJobApplicant), provider.GlowType);
				AssertEquals(typeof(Integration.Recruiter.IHRJobApplicant), provider.BusinessObjectType);
				AssertEquals(HRJobApplicantSchema.Constants.Prefix, provider.TablePrefix);
			});
		}

		public void TestGetComparisonSource()
		{
			var provider = new HRJobApplicantDeduplicationProvider();
			var personInDB = Factory.NewWithValidTestData<GlbPerson>();
			var applicantInDB = GetJobApplicant();
			applicantInDB.HA_PER = personInDB.PK;

			Factory.Save();
			var dedupPerson = new MasterDataProvider().GetDeduplicationGlbPerson(personInDB) as DeduplicationGlbPerson;
			var source = provider.GetComparisonSource(dedupPerson, applicantInDB.PK.ToGuid(), null);

			AssertNotNull("Source is not null", source);
			AssertEquals(applicantInDB.PK, ((DeduplicationHRJobApplicant)source).HA_PK);

			var newPerson = Factory.NewWithValidTestData<GlbPerson>();
			var newApplicant = GetJobApplicant();
			newApplicant.HA_PER = newPerson.PK;
			var dedupPersonNew = newPerson.CreateIGlbPerson() as DeduplicationGlbPerson;
			source = provider.GetComparisonSource(dedupPersonNew, newApplicant.PK.ToGuid(), null);

			AssertNotNull("Source is not null", source);
			AssertEquals(newApplicant.PK, ((DeduplicationHRJobApplicant)source).HA_PK);

			source = provider.GetComparisonSource(new[] { dedupPersonNew }, newApplicant.PK.ToGuid(), null);
			AssertNotNull("Source is not null", source);
			AssertEquals(newApplicant.PK, ((DeduplicationHRJobApplicant)source).HA_PK);
		}

		public void TestGetHeading_WhenSourceIsKnown()
		{
			var provider = new HRJobApplicantDeduplicationProvider();
			var applicant = GetJobApplicant();
			applicant.HA_FullName = "ABC";

			var dedupApplicant = new DeduplicationHRJobApplicant(applicant, null);
			var header = provider.GetHeading(dedupApplicant);

			AssertEquals("ABC", header);
		}

		public void TestGetHeading_WhenSourceIsEitherNewOrExisting()
		{
			var provider = new HRJobApplicantDeduplicationProvider();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var applicant = GetJobApplicant();
			applicant.HA_PER = person.PK;
			Factory.Save();
			applicant.HA_FullName = "Capitol Hill";
			Factory.Save();

			var dedupPerson = new List<IGlbPerson>() { person.CreateIGlbPerson() };
			var shortHeader = provider.GetHeading(dedupPerson, applicant.PK.ToGuid(), HeaderType.Short);
			var longHeader = provider.GetHeading(dedupPerson, applicant.PK.ToGuid(), HeaderType.Long);

			AssertEquals("Capitol Hill", shortHeader);
			AssertEquals("Capitol Hill", longHeader);
		}

		public void TestGetChildObject()
		{
			var provider = new HRJobApplicantDeduplicationProvider();
			var applicant = GetJobApplicant();
			var person = GlbPerson.CreateFromApplicant(Factory, applicant);
			var dedupPerson = person.CreateIGlbPerson();

			CombineAssertions(() =>
			{
				AssertNull(provider.GetChildObject(new object() as IDeduplicationMaster, applicant.PK.ToGuid()));
				AssertNull(provider.GetChildObject(null, applicant.PK.ToGuid()));
				AssertNull(provider.GetChildObject((IDeduplicationMaster)dedupPerson, Guid.NewGuid()));
				AssertNotNull(provider.GetChildObject((IDeduplicationMaster)dedupPerson, applicant.PK.ToGuid()));
			});
		}

		Integration.Recruiter.IHRJobApplicant GetJobApplicant()
		{
			var applicant = Factory.New<Integration.Recruiter.IHRJobApplicant>();
			((BusinessObject)applicant)[HRJobApplicantSchema.Constants.HA_EmailAddress] = "test@abcd.com";

			return applicant;
		}
	}
}
