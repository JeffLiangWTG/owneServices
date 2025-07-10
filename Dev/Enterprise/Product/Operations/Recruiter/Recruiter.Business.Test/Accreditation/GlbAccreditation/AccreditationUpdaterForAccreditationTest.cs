using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Moq;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(AccreditationUpdaterForAccreditation))]
	sealed class AccreditationUpdaterForAccreditationTest : AccreditationUpdaterTest
	{
		public void TestRun_Accreditation()
		{
			var accred = PrepareData(out var applicant);

			var updater = new AccreditationUpdaterForAccreditation(accred);
			updater.Run();

			applicant.Person.AccreditationAttemptCollection.Reload();

			applicant.Certificates.RefreshFromDb();
			AssertEquals(0, applicant.Certificates.Count);
		}

		public void TestRun_AccreditationZSaveExceptionCatching()
		{
			var accred = PrepareData(out var _);

			var mockUpdater = new Mock<AccreditationUpdaterForAccreditation>(accred);
			mockUpdater.CallBase = true;
			mockUpdater.Setup(x => x.DeleteAttemptsAndCertificates(null)).Throws(new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), null, Db.Connection), Factory));

			AssertExceptionThrown<ZSaveConcurrencyException>(() => mockUpdater.Object.Run());
			mockUpdater.Verify(mock => mock.DeleteAttemptsAndCertificates(null), Times.Exactly(5));
		}

		[TestDate(2018, 1, 1)]
		public void TestDeleteAttemptsAndCertificatesForAccreditation()
		{
			var accred = PrepareData(out var applicant);

			var updater = new AccreditationUpdaterForAccreditation(accred);
			updater.Run();

			applicant.Person.AccreditationAttemptCollection.Reload();
			applicant.Certificates.RefreshFromDb();

			AssertEquals(0, applicant.Certificates.Count);

			var certificate2 = applicant.Certificates.AddNew();
			certificate2.XZ_RefNumber = "A0000002";
			certificate2.XZ_Comment = "bla";
			Factory.Save();

			updater.DeleteExistingCertificates = true;
			updater.DeleteAttemptsAndCertificates();

			applicant = new BusinessObjectFactory().Load<HRJobApplicant>(applicant.PK);
			AssertEquals(1, applicant.Certificates.Count);
			AssertEquals(certificate2.PK, applicant.Certificates[0].PK);
			AssertEquals(0, applicant.Person.AccreditationAttemptCollection.Count);
		}

		protected override AccreditationUpdater GetUpdater()
		{
			var accred = Factory.New<GlbAccreditation>();
			return new AccreditationUpdaterForAccreditation(accred);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetUpdater();
		}
	}
}
