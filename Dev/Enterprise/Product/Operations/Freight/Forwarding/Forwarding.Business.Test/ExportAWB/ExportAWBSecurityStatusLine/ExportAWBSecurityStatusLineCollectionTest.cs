using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ExportAWBSecurityStatusLineCollection))]
	sealed class ExportAWBSecurityStatusLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionLoadsElementProperly()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var header = consol.AWBHeader;
			var line1 = Factory.New<ExportAWBSecurityStatusLine>();
			line1.EAS_EH = header.PK;

			var lines = new ExportAWBSecurityStatusLineCollection(header);
			lines.Load();
			AssertEquals(1, lines.Count);
		}

		#region Implementation

		protected override CargoWise.EntityFramework.BusinessObjectCollection GetCollectionToTest()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var header = consol.AWBHeader;
			return new ExportAWBSecurityStatusLineCollection(header);
		}

		#endregion
	}
}
