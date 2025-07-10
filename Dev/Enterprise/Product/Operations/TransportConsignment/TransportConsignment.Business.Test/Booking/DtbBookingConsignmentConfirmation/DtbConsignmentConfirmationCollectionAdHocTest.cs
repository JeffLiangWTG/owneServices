using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentConfirmationCollectionAdHoc))]
	public class DtbConsignmentConfirmationCollectionAdHocTest : ActiveBusinessObjectCollectionTestCase<DtbConsignmentConfirmationCollectionAdHoc>
	{
		#region TestAllowNew

		public void TestAllowNew()
		{
			AssertEquals(false, ((IBindingList)GetCollectionToTest()).AllowNew);
		}

		#endregion

		#region TestAllowRemove

		public void TestAllowRemove()
		{
			AssertEquals(false, ((IBindingList)new DtbConsignmentConfirmationCollectionAdHoc(Factory)).AllowRemove);
		}

		#endregion

		#region Implementation

		protected override DtbConsignmentConfirmationCollectionAdHoc GetCollectionToTest()
		{
			return new DtbConsignmentConfirmationCollectionAdHoc(Factory);
		}

		#endregion
	}
}
