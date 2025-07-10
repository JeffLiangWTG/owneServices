using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Recruitment.Module;
using Enterprise.Recruitment.Module.CandidateManagement;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Module
{
	[TestedType(typeof(CandidateModuleBusinessObject))]
	sealed class CandidateModuleBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
			=> new CandidateModuleBusinessObject(new CandidateBusinessObjectCollection(Factory));
	}
}
