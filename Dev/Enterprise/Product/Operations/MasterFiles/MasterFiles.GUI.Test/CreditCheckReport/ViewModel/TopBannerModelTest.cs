using System;
using System.Collections.Generic;
using System.Drawing;
using Enterprise.MasterFiles.GUI.Test;
using NUnit.Framework;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class TopBannerModelTest : TestCase
	{
		public void TestNoEvent()
		{
			var model = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "WiseTech Global Limited");
			CombineAssertions(() =>
			{
				AssertEquals(ResourceStringHelper.StatusHeaderNoEvent, model.StatusHeader);
				AssertEquals(ResourceStringHelper.StatusDetailsNoEvent, model.StatusDetails);
				AssertEquals(string.Empty, model.StatusDetailsEventsInfo);
				AssertEquals(null, model.StatusImage);
				Assert(!model.StatusImageAndDetailEventInfoVisible);
			});
		}

		public void TestUpToDate()
		{
			var model = new TopBannerModel(CreditReportStatusType.UpToDate, 0, "WiseTech Global Limited");
			CombineAssertions(() =>
			{
				AssertEquals(ResourceStringHelper.StatusHeaderUpToDate, model.StatusHeader);
				AssertEquals(ResourceStringHelper.StatusDetailsUpToDate("WiseTech Global Limited"), model.StatusDetails);
				AssertEquals(ResourceStringHelper.GetEventsInfo(0), model.StatusDetailsEventsInfo);
				AssertColorEquals(Color.Green, model.StatusDetailsEventsInfoColor);
				ImageBitmapTestHelper.AssertImageBitsEquals(Properties.Resources.TickGreen, model.StatusImage);
				Assert(model.StatusImageAndDetailEventInfoVisible);
			});
		}

		public void TestWarning()
		{
			var model = new TopBannerModel(CreditReportStatusType.Warning, 10, "WiseTech Global Limited");
			CombineAssertions(() =>
			{
				AssertEquals(ResourceStringHelper.StatusHeaderWarning, model.StatusHeader);
				AssertEquals(ResourceStringHelper.StatusDetailsWarning("WiseTech Global Limited"), model.StatusDetails);
				AssertEquals(ResourceStringHelper.GetEventsInfo(10), model.StatusDetailsEventsInfo);
				AssertColorEquals(Color.Red, model.StatusDetailsEventsInfoColor);
				ImageBitmapTestHelper.AssertImageBitsEquals(Properties.Resources.WarningRed, model.StatusImage);
				Assert(model.StatusImageAndDetailEventInfoVisible);
			});
		}

		public void TestUpdateStatusTypeAndEventsCount()
		{
			var model = new TopBannerModel(CreditReportStatusType.Warning, 10, "WiseTech Global Limited");
			CombineAssertions(() =>
			{
				AssertEquals(ResourceStringHelper.StatusHeaderWarning, model.StatusHeader);
				AssertEquals(ResourceStringHelper.StatusDetailsWarning("WiseTech Global Limited"), model.StatusDetails);
				AssertEquals(ResourceStringHelper.GetEventsInfo(10), model.StatusDetailsEventsInfo);
				AssertColorEquals(Color.Red, model.StatusDetailsEventsInfoColor);
				ImageBitmapTestHelper.AssertImageBitsEquals(Properties.Resources.WarningRed, model.StatusImage);
				Assert(model.StatusImageAndDetailEventInfoVisible);
			});

			var propertyChanged = 0;
			model.PropertyChanged += Model_PropertyChanged;
			model.UpdateStatusTypeAndEventsCount(CreditReportStatusType.UpToDate, 0);

			CombineAssertions(() =>
			{
				AssertEquals(ResourceStringHelper.StatusHeaderUpToDate, model.StatusHeader);
				AssertEquals(ResourceStringHelper.StatusDetailsUpToDate("WiseTech Global Limited"), model.StatusDetails);
				AssertEquals(ResourceStringHelper.GetEventsInfo(0), model.StatusDetailsEventsInfo);
				AssertColorEquals(Color.Green, model.StatusDetailsEventsInfoColor);
				ImageBitmapTestHelper.AssertImageBitsEquals(Properties.Resources.TickGreen, model.StatusImage);
				Assert(model.StatusImageAndDetailEventInfoVisible);
				AssertEquals(4, propertyChanged);
			});

			void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
			{
				propertyChanged++;
			}
		}

		public void TestUpdateStatusTypeAndEventsCountWithEvents()
		{
			var events = new List<CreditEvent>();
			var model = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "WiseTech Global Limited");
			model.UpdateStatusTypeAndEventsCount(events);
			AssertEquals(ResourceStringHelper.StatusHeaderUpToDate, model.StatusHeader);

			events.Add(new CreditEvent() { EventDate = DateTime.Today, Type = CreditEventType.StatusChange });
			model.UpdateStatusTypeAndEventsCount(events);
			AssertEquals(ResourceStringHelper.StatusHeaderWarning, model.StatusHeader);
		}
	}
}
