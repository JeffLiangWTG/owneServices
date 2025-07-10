using NUnit.Framework;

namespace Enterprise.Warehouse.Integration.Testing
{
	public class NotificationCollectionTest : TestCase
	{
		public void TestErrors()
		{
			NotificationCollection errors = new NotificationCollection();
			AssertEquals(0, errors.ErrorList.Count);
			AssertEquals(false, errors.HasErrors);
			errors.ErrorList.Add("Error1");
			AssertEquals(1, errors.ErrorList.Count);
			AssertEquals(true, errors.HasErrors);
			errors.ErrorList.Add("Error2");
			errors.ErrorList.Add("Error3");
			AssertEquals(3, errors.ErrorList.Count);
			AssertCollectionContains("Error1", "Error1", errors.ErrorList);
			AssertCollectionContains("Error2", "Error2", errors.ErrorList);
			AssertCollectionContains("Error2", "Error2", errors.ErrorList);
			AssertEquals(true, errors.HasErrors);
			errors.ErrorList.Clear();
			AssertEquals(0, errors.ErrorList.Count);
			AssertEquals(false, errors.HasErrors);
		}

		public void TestWarnings()
		{
			NotificationCollection errors = new NotificationCollection();
			AssertEquals(0, errors.WarningList.Count);
			AssertEquals(false, errors.HasWarnings);
			errors.WarningList.Add("Warning1");
			AssertEquals(1, errors.WarningList.Count);
			AssertEquals(true, errors.HasWarnings);
			errors.WarningList.Add("Warning2");
			errors.WarningList.Add("Warning3");
			AssertEquals(3, errors.WarningList.Count);
			AssertCollectionContains("Warning1", "Warning1", errors.WarningList);
			AssertCollectionContains("Warning2", "Warning2", errors.WarningList);
			AssertCollectionContains("Warning2", "Warning2", errors.WarningList);
			AssertEquals(true, errors.HasWarnings);
			errors.WarningList.Clear();
			AssertEquals(0, errors.WarningList.Count);
		}

		public void TestHasWarnings()
		{
			NotificationCollection errors = new NotificationCollection();
			Assert("HasWarnings", !errors.HasWarnings);
			errors.WarningList.Add("Warning1");
			Assert("HasWarnings", errors.HasWarnings);
			errors.WarningList.Clear();
			Assert("HasWarnings", !errors.HasWarnings);
		}

		public void TestHasErrors()
		{
			NotificationCollection errors = new NotificationCollection();
			Assert("HasErrors", !errors.HasErrors);
			errors.ErrorList.Add("Errors1");
			Assert("HasErrors", errors.HasErrors);
			errors.ErrorList.Clear();
			Assert("HasErrors", !errors.HasErrors);
			AssertEquals(false, errors.HasWarnings);
		}
	}
}