using NUnit.Framework;
using WF = CargoWise.Workflow;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class RulesFactoryTest : TestCase
	{
		public void TestNew()
		{
			RulesFactory factory = new RulesFactory();
			AssertEquals(typeof(CheckEnteredRule), factory.New(WF.CustomAddOnRuleTypes.CheckEntered).GetType());
			AssertEquals(typeof(InvalidCodeRule), factory.New(WF.CustomAddOnRuleTypes.InvalidCode).GetType());
			AssertEquals(typeof(DateTimeFormatRule), factory.New(WF.CustomAddOnRuleTypes.DateTimeFormat).GetType());
			AssertEquals(typeof(CreateEventRule), factory.New(WF.CustomAddOnRuleTypes.CreateEvent).GetType());
		}
	}
}
