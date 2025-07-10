using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(AWBHeaderManager<ForwardingConsol>))]
	sealed class AWBHeaderManagerTestForForwardingConsol : ActiveBusinessObjectCollectionTestCase<AWBHeaderManager<ForwardingConsol>>
	{
		#region Implementation

		ForwardingConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.New<ForwardingConsol>();
					consol.JK_TransportMode = Constants.TransportModes.Air;
				}

				return consol;
			}
		}

		ForwardingConsol consol;

		protected override AWBHeaderManager<ForwardingConsol> GetCollectionToTest()
		{
			return new AWBHeaderManager<ForwardingConsol>(Consol);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var exportAWBHeader = Factory.New<ConsolExportAWBHeader>();
			exportAWBHeader.EH_ParentID = Consol.PK;

			return exportAWBHeader;
		}

		public void TestAWBHeaderOnlyReturnedForAirTransportModes()
		{
			var manager = new AWBHeaderManager<ForwardingConsol>(Consol);
			AssertNotNull("Manager should created an AWBHeader", manager.AWBHeader);

			Consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertNull("Should not return an AWBHeader for a non-air parent", manager.AWBHeader);

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertNotNull("Manager should created an AWBHeader", manager.AWBHeader);
		}

		#endregion

		public override void TestAdd()
		{
			int initialCount = Collection.Count;

			var bizO1 = (ConsolExportAWBHeader)GetNewElementToAddToTheCollection();
			AssertEquals("Collection count", initialCount + 1, Collection.Count);
			Assert("Contains element 1", Collection.Contains(bizO1));

			var bizO2 = (ConsolExportAWBHeader)GetNewElementToAddToTheCollection();
			AssertEquals("Collection count", 0, Collection.Count);
		}

		public override void TestDelete()
		{
			int initialCount = Collection.Count;
			var bizO = (ConsolExportAWBHeader)GetNewElementToAddToTheCollection();
			AssertEquals("Precondition : Collection count", initialCount + 1, Collection.Count);

			Collection.Delete(bizO);

			AssertEquals("Collection count", initialCount, Collection.Count);
			Assert("Doesn't contain element", !Collection.Contains(bizO));
			Assert("Element was removed, and deleted", bizO.IsDeleted);
		}
	}
}
