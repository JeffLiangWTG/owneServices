using CargoWise.EntityFramework.Testing;
using CargoWise.Workflow;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class TriggerConditionTypeConverterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAllCodesHaveAnEnum()
		{
			foreach (var key in new EventReferenceConditionList().GetAllCodes())
			{
				AssertNotEquals(TriggerConditionType.None, TriggerConditionTypeConverter.Convert(key));
			}
		}
	}
}
