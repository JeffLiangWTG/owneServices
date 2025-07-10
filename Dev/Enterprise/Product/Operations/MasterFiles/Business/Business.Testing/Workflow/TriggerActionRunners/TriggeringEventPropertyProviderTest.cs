using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TriggeringEventPropertyProvider<StmALog>))]
	sealed class TriggeringEventPropertyProviderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new TriggeringEventPropertyProvider<StmALog>(Factory.New<StmALog>());
	}
}
