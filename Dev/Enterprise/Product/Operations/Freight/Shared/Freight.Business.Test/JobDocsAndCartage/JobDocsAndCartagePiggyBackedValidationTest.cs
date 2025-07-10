using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	public class JobDocsAndCartagePiggyBackedValidationTest : TestCaseWithFactory
	{
		public void TestPiggyBackedValidationCanBeHookedUpToThisClass()
		{
			JobDocsAndCartage jobDocs = JobDocsAndCartage.New(GetNewParent());
			Assert(jobDocs.Validation.ContainsPiggybackedValidation(typeof(PiggyBackedValidationForTest)));
		}

		#region Implementation

		protected virtual Type GetJobDocsAndCartageType()
		{
			return typeof(JobDocsAndCartage);
		}

		protected virtual ParentForTest GetNewParent()
		{
			return new ParentForTest(Factory);
		}

		protected class ParentForTest : MockJobDocsAndCartageParent, IShipmentWithDocsAndCartage
		{
			public ParentForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			JobDocsAndCartageValidation IShipmentWithDocsAndCartage.PiggyBackedValidation
			{
				get { return new PiggyBackedValidationForTest(DocsAndCartage); }
			}

			ZString IHaveInternalCartage.OwnerRef
			{
				get { return ZString.Empty; }
			}

			bool IHaveInternalCartage.IsAllowedToUpdateAdviseDates
			{
				get { return false; }
			}

			public new JobDocsAndCartage DocsAndCartage
			{
				get { return JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(this); }
			}
		}

		class PiggyBackedValidationForTest : JobDocsAndCartageValidation
		{
			public PiggyBackedValidationForTest(JobDocsAndCartage parent)
				: base(parent)
			{
			}
		}

		#endregion
	}
}
