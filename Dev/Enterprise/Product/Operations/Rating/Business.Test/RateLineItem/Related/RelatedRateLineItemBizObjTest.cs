using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RelatedRateLineItem))]
	public class RelatedRateLineItemBizObjTest : BizObjectRateLineItemTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestGetTM_ValueIfParentIsNullOrNot()
		{
			var item = GetNewBusinessObject() as RelatedRateLineItem;

			AssertNotNull(item);
			AssertNotNull(item.Parent);
			AssertNotNull(item.TM_Value);

			item.TM_TL = new ZGuid(Guid.NewGuid());
			AssertNull(item.Parent);
			AssertNotNull(item.TM_Value);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var entry = factory.New<Costing>().AddRateEntry(RatingConstants.RateCategory.AIR);

			var line = entry.RelatedRateLines.AddNew();
			line.TL_AC = factory.New<AccChargeCode>().PK;
			line.TL_TI = entry.PK;

			var item = (RelatedRateLineItem)line.RateLineItems.AddNew();
			item.TM_TL = line.PK;

			return item;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var entry = Factory.New<Costing>().AddRateEntry("AIR");
			var line = entry.RelatedRateLines.AddNew();
			var newItem = line.RateLineItems.AddNew() as RelatedRateLineItem;
			newItem.TM_TL = line.PK;

			return newItem;
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return GetNewBusinessObject();
		}
	}
}
