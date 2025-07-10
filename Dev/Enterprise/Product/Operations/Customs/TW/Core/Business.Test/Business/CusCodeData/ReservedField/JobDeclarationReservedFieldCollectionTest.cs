using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobDeclarationReservedFieldCollection))]
	sealed class JobDeclarationReservedFieldCollectionTest : ReservedFieldCollectionTest<JobDeclarationReservedField>
	{
		protected override CusCodeDataCollection<JobDeclarationReservedField> GetCusCodeDataCollection()
		{
			return new JobDeclarationReservedFieldCollection(JobDeclaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<JobDeclarationReservedField>();
			result.CY_ParentID = JobDeclaration.PK;
			result.CY_ParentTableCode = JobDeclaration.TablePrefix;
			return result;
		}

		JobDeclaration JobDeclaration
		{
			get
			{
				return jobDeclaration ?? (jobDeclaration = Factory.New<JobDeclaration>());
			}
		}

		JobDeclaration jobDeclaration;
	}
}
