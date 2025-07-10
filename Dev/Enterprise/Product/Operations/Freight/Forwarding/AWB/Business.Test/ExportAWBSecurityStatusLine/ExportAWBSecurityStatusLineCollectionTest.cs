using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(ExportAWBSecurityStatusLineCollection))]
	sealed class ExportAWBSecurityStatusLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionLoadsElementProperly()
		{
			var header = Factory.NewWithValidTestData<ExportAWBHeader>();
			var line1 = Factory.NewWithValidTestData<ExportAWBSecurityStatusLine>();
			line1.EAS_EH = header.PK;

			var lines = new ExportAWBSecurityStatusLineCollection(header);
			lines.Load();

			AssertEquals(1, lines.Count);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var master = Factory.New<ExportAWBHeader>();
			return new ExportAWBSecurityStatusLineCollection(master);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ExportAWBSecurityStatusLine>();
		}

		#endregion
	}
}
