using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UpdateP8_DateForExchangeRateAction))]
	sealed class UpdateP8_DateForExchangeRateActionTest : NonPersistentBusinessObjectTestCase
	{
		#region Default Values

		public void TestDefaultValues()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_DateForExchangeRate = new ZDateTime(2022, 2, 2);
			var action = new UpdateP8_DateForExchangeRateAction(opportunity);
			AssertEquals(new ZDateTime(2022, 2, 2), action.Date);
		}

		#endregion

		#region Execute

		public void TestExecute()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_DateForExchangeRate = new ZDateTime(2022, 2, 2);
			var updater = new UpdateP8_DateForExchangeRateAction(opportunity);
			updater.Date = new ZDateTime(2022, 3, 3);

			updater.Execute();

			AssertEquals(new ZDateTime(2022, 3, 3), updater.Date);
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			return new UpdateP8_DateForExchangeRateAction(opportunity);
		}

		#endregion
	}
}
