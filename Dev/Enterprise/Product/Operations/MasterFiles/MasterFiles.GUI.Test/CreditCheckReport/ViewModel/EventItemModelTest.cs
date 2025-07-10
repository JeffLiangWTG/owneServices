using System;
using System.Globalization;
using CargoWise.EntityFramework.Testing;
using WTG.ROPE.Model;
using static Enterprise.MasterFiles.GUI.Test.ImageBitmapTestHelper;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class EventItemModelTest : TestCaseWithFactory
	{
		public void TestAlertsBannerItemModel()
		{
			var model = new CreditEvent() { Type = CreditEventType.CollectionChange, EventDate = new DateTime(2020, 4, 1) };
			var eventItemModel = new EventItemModel(model);

			CombineAssertions(() =>
			{
				AssertImageBitsEquals(ImageBitmapHelper.GetCreditEventIconInfo(CreditEventType.CollectionChange).Icon, eventItemModel.EventIcon);
				AssertEquals(new DateTime(2020, 4, 1).ToString("dd MMMM yyyy", CultureInfo.CurrentCulture), eventItemModel.EventDate);
				AssertEquals(ResourceStringHelper.GetCreditEventDescription(CreditEventType.CollectionChange), eventItemModel.EventDescription);
			});
		}
	}
}
