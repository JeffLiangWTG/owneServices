using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestsSubclassesOf(typeof(JobDeclarationValidation))]
	abstract class JobDeclarationValidationAbstractTest<T> : BusinessObjectValidationTestCase
		where T : JobDeclarationValidation
	{
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageType;
			validation = GetValidation();
		}
		protected JobDeclaration declaration;
		protected T validation;

		protected abstract string MessageType { get; }

		protected abstract T GetValidation();
	}
}
