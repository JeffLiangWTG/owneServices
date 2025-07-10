using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	public class UnsentMessageChangeSupportTest : TransactionedTestCase
	{
		public void TestCheckWithNoMessageChanges()
		{
			var factory = new BusinessObjectFactory();
			var testMessageChangeSupport = new UnsentMessageChangeSupport();
			var actualResult = testMessageChangeSupport.CheckForHeldMessageChangesAndPerformUserAction(factory.New<JobDeclaration>(), null);
			AssertEquals("ContinueWithSave result with no dialogs", ContinueWithSave.Yes, actualResult);
		}
	}
}
