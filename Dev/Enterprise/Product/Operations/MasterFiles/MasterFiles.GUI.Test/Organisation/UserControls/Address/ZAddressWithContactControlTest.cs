using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Organisation.UserControls.Address.Tests
{
	public class ZAddressWithContactControlTest : TransactionedTestCase
	{
		public void TestTabVisiblePropertiesBehavesWell()
		{
			using (var control = new ZAddressWithContactControl())
			{
				AssertEquals("ContactInfoTabVisible is true by default", true, control.ContactInfoTabVisible);
				control.ContactInfoTabVisible = false;
				AssertEquals("Get/set harmony", false, control.ContactInfoTabVisible);
				control.ContactInfoTabVisible = true;
				AssertEquals("Get/set harmony", true, control.ContactInfoTabVisible);
			}
		}
	}
}
