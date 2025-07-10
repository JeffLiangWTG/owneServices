using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestsSubclassesOf(typeof(ShipmentCustomsEntryNumber))]
	public abstract class ShipmentCustomsEntryNumberTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewShipmentCustomsEntryNumber();
		}

		public abstract ShipmentCustomsEntryNumber GetNewShipmentCustomsEntryNumber();
		public abstract void TestGetCusEntryNumber();
		public abstract void TestEntryType();
		public abstract void TestEntryNumber();
		public abstract void TestProxiedEntryNumberIsReadOnly();
		public abstract void TestIssueDate();
		public abstract void TestExpiryDate();
		public abstract void TestValidation();
	}
}
