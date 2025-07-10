using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Freight.Integration.CFS;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(SailingRelatedJobCollection))]
	sealed class SailingRelatedJobCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var shipment = (CommonShipment)Factory.New<ICFSShipment>();
			shipment.FillWithValidTestData();
			shipment.JS_JX = sailing.PK;
			shipment.JS_UniqueConsignRef = "SRJC0001";

			Factory.Save();

			var collection = new SailingRelatedJobCollection(sailing);
			return collection;
		}

		#endregion
	}
}
