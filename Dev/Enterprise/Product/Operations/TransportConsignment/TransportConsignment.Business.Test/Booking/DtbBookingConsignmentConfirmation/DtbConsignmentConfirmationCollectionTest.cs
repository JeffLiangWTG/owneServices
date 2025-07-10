using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.TransportCommon.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentConfirmationCollection))]
	public class DtbConsignmentConfirmationCollectionTest : DtbTransportConfirmationCollectionTest<DtbConsignmentConfirmation, DtbConsignmentConfirmationCollection>
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
			AssertEquals(false, ((IBindingList)GetCollectionToTest()).AllowRemove);
		}

		#endregion

		#region Implementation

		protected override DtbConsignmentConfirmationCollection GetCollectionToTest()
		{
			return new DtbConsignmentConfirmationCollection(Factory, new ZQuery());
		}

		#endregion
	}
}
