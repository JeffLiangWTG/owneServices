using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgContactNameChangeServiceTest : TestCaseWithFactory
	{
		public void TestContactNameChangedEvent()
		{
			var contactNameChangedEventFired = new List<OrgContact>();

			OrgContactNameChangeService.GetInstance(Factory).ContactNameChanged += (sender, e) =>
				{
					contactNameChangedEventFired.Add((OrgContact)sender);
				};

			var contact1 = Factory.New<OrgContact>();
			contact1.OC_ContactName = "Banana";
			AssertContainsExactElementsInAnyOrder(new[] { contact1 }, contactNameChangedEventFired);

			contactNameChangedEventFired.Clear();
			contact1.OC_ContactName = "Apple";
			AssertContainsExactElementsInAnyOrder(new[] { contact1 }, contactNameChangedEventFired);

			contactNameChangedEventFired.Clear();
			var contact2 = Factory.New<OrgContact>();
			contact2.OC_ContactName = "Apple";
			AssertContainsExactElementsInAnyOrder(new[] { contact2 }, contactNameChangedEventFired);
		}
	}
}
