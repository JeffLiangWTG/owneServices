using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.TransportCommon.DataTransfer.Universal.Test
{
	public class ConfirmationComparerTest : TestCaseWithFactory
	{
		#region TestAllConfirmationPropertiesAreCompared

		public void TestAllConfirmationPropertiesAreCompared()
		{
			var confirmation = new Confirmation();
			AssertEquals("All confirmation properties except Quantity should be compared. This is used to find instruction level confirmations during universal import.",
				21, confirmation.GetType().GetProperties().Where(p => p.Name != "Quantity").Count());
		}

		#endregion
	}
}
