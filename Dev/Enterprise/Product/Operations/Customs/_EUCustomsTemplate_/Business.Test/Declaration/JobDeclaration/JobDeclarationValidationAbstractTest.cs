using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs._EUCustomsTemplate_.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(JobDeclarationValidation))]
	abstract class JobDeclarationValidationAbstractTest<T> : BusinessObjectValidationTestCase
		where T : JobDeclarationValidation
	{
		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageType;
			validation = GetValidation();
		}
		protected JobDeclaration jobDeclaration;
		protected T validation;

		protected abstract string MessageType { get; }

		protected abstract T GetValidation();
	}
}
