using System;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsGroupedInventoryCollectionByAttributesWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region Properties

		public void TestGroupedInventoryInfoCollection()
		{
			var response = new WhsGroupedInventoryCollectionWebServiceResponse();
			AssertEquals("If no Collection was set, should have created an empty one.", 0, response.InventoryInfoCollection.InventoryLineInfos.Count);

			var collection = new WhsGroupedInventoryLineInfoCollection();
			response.InventoryInfoCollection = collection;
			AssertEquals(collection, response.InventoryInfoCollection);
		}

		public void TestCriteriaInfo()
		{
			var response = new WhsGroupedInventoryCollectionWebServiceResponse();
			AssertNotNull("If no CriteriaInfo was set, should have created an new one.", response.CriteriaInfo);

			var criteriaInfo = new WhsInventorySearchCriteriaInfo();
			AssertEquals(SearchJoinCondition.And, criteriaInfo.JoinCondition);
			AssertEquals("", criteriaInfo.ClientCode);
			AssertEquals(Guid.Empty, criteriaInfo.ProductPK);
			AssertEquals("", criteriaInfo.ProductCode);
			AssertEquals("", criteriaInfo.Location);
			AssertEquals("", criteriaInfo.PalletID);
			AssertEquals("", criteriaInfo.Attribute1);
			AssertEquals("", criteriaInfo.Attribute2);
			AssertEquals("", criteriaInfo.Attribute3);
			AssertEquals("", criteriaInfo.SerialNumber);
			AssertEquals(DateTime.MinValue, criteriaInfo.ExpiryDate);
			AssertEquals(DateTime.MinValue, criteriaInfo.PackingDate);
		}

		#endregion

		#region Implementation

		protected new WhsGroupedInventoryCollectionWebServiceResponse Response
		{
			get { return (WhsGroupedInventoryCollectionWebServiceResponse)base.Response; }
		}

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsGroupedInventoryCollectionWebServiceResponse();
		}

		#endregion
	}
}
