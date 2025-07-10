using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestsSubclassesOf(typeof(JobDeclarationLookups))]
abstract class JobDeclarationLookupsAbstractTest<T> : BusinessObjectLookupsTestCase where T : JobDeclarationLookups
{
	protected abstract string MessageType { get; }

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageType;
		lookups = declaration.Lookups as T;
	}

	protected JobDeclaration declaration;
	protected T lookups;
}
