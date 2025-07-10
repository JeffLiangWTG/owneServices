using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(JobDeclarationLookups))]
	abstract class JobDeclarationLookupsAbstractTest : BusinessObjectLookupsTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageType;
			lookups = GetLookups();
		}
		protected JobDeclaration jobDeclaration;
		protected JobDeclarationLookups lookups;

		protected abstract string MessageType { get; }
		protected abstract JobDeclarationLookups GetLookups();
	}
}
