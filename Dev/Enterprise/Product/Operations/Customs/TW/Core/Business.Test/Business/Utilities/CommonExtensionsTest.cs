using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CommonExtensionsTest : TestCaseWithDummy
	{
		[ExpectNoExceptions]
		public void TestRemoveAndDeleteAllIfReadOnly_BusinessObjectCollection()
		{
			CombineAssertions(() =>
			{
				var collection = new DummyBusinessObjectCollection(Factory);

				collection.AddNew();
				NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(1), "a new element is added to the collection");

				collection.RemoveAndDeleteAllIfReadOnly();
				NUnit.Framework.Assert.That(collection.ReadOnly, NUnit.Framework.Is.EqualTo(false), "collection is not readonly");
				NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(1), "not cleared because collection is not readonly");

				collection.SetReadOnlyIncludingChildren(true);
				collection.RemoveAndDeleteAllIfReadOnly();
				NUnit.Framework.Assert.That(collection.ReadOnly, NUnit.Framework.Is.EqualTo(true), "collection is readonly");
				NUnit.Framework.Assert.That(collection.Count, NUnit.Framework.Is.EqualTo(0), "cleared because collection is readonly");
			});
		}

		[ExpectNoExceptions]
		public void TestClearValueIfReadOnly_ZPropertyInfo()
		{
			CombineAssertions(() =>
			{
				var info = Dummy.Z0_DescriptionInfo;
				info.Value = new ZString("123");
				NUnit.Framework.Assert.That(info.Value, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison), "value is updated");

				info.ClearValueIfReadOnly();
				NUnit.Framework.Assert.That(info.ReadOnly, NUnit.Framework.Is.EqualTo(false), "property is not readonly");
				NUnit.Framework.Assert.That(info.Value.IsEmpty, NUnit.Framework.Is.EqualTo(false), "cleared because property is not readonly");

				((IBusinessObjectState)Dummy).IncrementReadOnlyIncludingChildren();
				info.ClearValueIfReadOnly();
				NUnit.Framework.Assert.That(info.ReadOnly, NUnit.Framework.Is.EqualTo(true), "property is readonly");
				NUnit.Framework.Assert.That(info.Value.IsEmpty, NUnit.Framework.Is.EqualTo(true), "cleared because property is readonly");
			});
		}
	}
}
