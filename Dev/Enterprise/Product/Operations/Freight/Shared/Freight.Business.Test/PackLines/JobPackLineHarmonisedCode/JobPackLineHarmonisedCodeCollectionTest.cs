using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobPackLineHarmonisedCodeCollection))]
	sealed class JobPackLineHarmonisedCodeCollectionTest : ActiveBusinessObjectCollectionTestCase<JobPackLineHarmonisedCodeCollection>
	{
		#region Binding Auto Added Items

		public void TestBindingItem()
		{
			var packline = Factory.New<PackLine>();
			AssertEquals(0, packline.HarmonisedCodes.Count);

			packline.HarmonisedCodes.FirstItemForBinding[0].JLH_RN_NKCountry = "CN";
			AssertEquals(1, packline.HarmonisedCodes.Count);
			AssertEquals(false, packline.HarmonisedCodes[0].IsAutoAddedItem);
			AssertEquals(packline.HarmonisedCodes[0], packline.HarmonisedCodes.FirstItemForBinding[0]);

			packline.HarmonisedCodes.DeleteAll();
			AssertEquals(0, packline.HarmonisedCodes.Count);
			AssertEquals(1, packline.HarmonisedCodes.FirstItemForBinding.Count);
			AssertEquals(true, packline.HarmonisedCodes.FirstItemForBinding[0].IsAutoAddedItem);
			AssertEquals(false, packline.HarmonisedCodes.FirstItemForBinding[0].IsSavedByFactory);

			var item1 = packline.HarmonisedCodes.AddNew();
			var item2 = packline.HarmonisedCodes.AddNew();

			AssertEquals(2, packline.HarmonisedCodes.Count);
			AssertEquals(1, packline.HarmonisedCodes.FirstItemForBinding.Count);
			AssertEquals(false, packline.HarmonisedCodes.FirstItemForBinding[0].IsAutoAddedItem);

			AssertEquals(false, item2.IsDeleted);
			item2.JLH_RN_NKCountry = "BE";
			AssertEquals(false, item2.IsDeleted);
		}

		public void TestBindingItems_ShouldNotAddNewItemTwice()
		{
			var packline = Factory.New<PackLine>();
			packline.HarmonisedCodes.DeleteAll();
			CombineAssertions("Precondition", () =>
			{
				AssertEquals(0, packline.HarmonisedCodes.Count);
				AssertEquals(1, packline.HarmonisedCodes.FirstItemForBinding.Count);
				Assert("IsAutoAddedItem", packline.HarmonisedCodes.FirstItemForBinding[0].IsAutoAddedItem);
				Assert("IsSavedByFactory", !packline.HarmonisedCodes.FirstItemForBinding[0].IsSavedByFactory);
			});

			packline.HarmonisedCodes.FirstItemForBinding[0].JLH_RN_NKCountry = "CN";
			AssertEquals("HarmonisedCodes.Count", 1, packline.HarmonisedCodes.Count);
			AssertEquals("HarmonisedCodes.FirstItemForBinding.Count", 1, packline.HarmonisedCodes.FirstItemForBinding.Count);
			AssertEquals(packline.HarmonisedCodes[0], packline.HarmonisedCodes.FirstItemForBinding[0]);
			Assert("IsAutoAddedItem", !packline.HarmonisedCodes.FirstItemForBinding[0].IsAutoAddedItem);
			Assert("IsSavedByFactory", packline.HarmonisedCodes.FirstItemForBinding[0].IsSavedByFactory);
		}

		public void TestBindingItemReadonly()
		{
			var packLine = Factory.New<PackLine>();

			packLine.HarmonisedCodes.SetReadOnlyIncludingChildren(true);

			AssertEquals(0, packLine.HarmonisedCodes.Count);
			AssertReadOnly(packLine.HarmonisedCodes, true);

			packLine.HarmonisedCodes.SetReadOnlyIncludingChildren(false);

			AssertEquals(0, packLine.HarmonisedCodes.Count);
			AssertReadOnly(packLine.HarmonisedCodes, false);

			packLine.HarmonisedCodes.SetReadOnlyIncludingChildren(true);

			AssertEquals(0, packLine.HarmonisedCodes.Count);
			AssertReadOnly(packLine.HarmonisedCodes, true);

			packLine.HarmonisedCodes.FirstItemForBinding[0].JLH_RN_NKCountry = "ZA";

			AssertEquals(1, packLine.HarmonisedCodes.Count);
			AssertReadOnly(packLine.HarmonisedCodes, true);

			packLine.HarmonisedCodes.SetReadOnlyIncludingChildren(false);

			AssertEquals(1, packLine.HarmonisedCodes.Count);
			AssertReadOnly(packLine.HarmonisedCodes, false);

			packLine.HarmonisedCodes.SetReadOnlyIncludingChildren(true);

			AssertEquals(1, packLine.HarmonisedCodes.Count);
			AssertReadOnly(packLine.HarmonisedCodes, true);
		}

		void AssertReadOnly(JobPackLineHarmonisedCodeCollection harmonisedCodes, bool readOnly)
		{
			AssertEquals(readOnly, harmonisedCodes.FirstItemForBinding.ReadOnly);
			AssertEquals(readOnly, harmonisedCodes.FirstItemForBinding[0].JLH_RN_NKCountryInfo.ReadOnly);
			AssertEquals(readOnly, harmonisedCodes.FirstItemForBinding[0].JLH_CodeInfo.ReadOnly);
			AssertEquals(readOnly, harmonisedCodes.HSCountryManager.ReadOnly);
			AssertEquals(readOnly, harmonisedCodes.HSCountryManager.ValueInfo.ReadOnly);
			AssertEquals(readOnly, harmonisedCodes.HSCodeManager.ReadOnly);
			AssertEquals(readOnly, harmonisedCodes.HSCodeManager.ValueInfo.ReadOnly);
		}

		public void TestItemDeletedWhenCountryAndCodeAreBlanked()
		{
			var packline = Factory.New<PackLine>();
			AssertEquals(0, packline.HarmonisedCodes.Count);

			packline.HarmonisedCodes.FirstItemForBinding[0].JLH_RN_NKCountry = "DE";
			AssertEquals(1, packline.HarmonisedCodes.Count);
			AssertEquals(false, packline.HarmonisedCodes[0].IsAutoAddedItem);
			AssertEquals(1, packline.HarmonisedCodes.FirstItemForBinding.Count);

			packline.HarmonisedCodes.FirstItemForBinding[0].JLH_RN_NKCountry = "";
			packline.HarmonisedCodes.FirstItemForBinding[0].JLH_Code = "";
			AssertEquals(0, packline.HarmonisedCodes.Count);
			AssertEquals(true, packline.HarmonisedCodes.FirstItemForBinding[0].IsAutoAddedItem);
			AssertEquals(1, packline.HarmonisedCodes.FirstItemForBinding.Count);

			packline.HarmonisedCodes.FirstItemForBinding[0].JLH_Code = "1.1";
			AssertEquals(1, packline.HarmonisedCodes.Count);
			AssertEquals(false, packline.HarmonisedCodes[0].IsAutoAddedItem);
			AssertEquals(1, packline.HarmonisedCodes.FirstItemForBinding.Count);

			packline.HarmonisedCodes.FirstItemForBinding[0].JLH_Code = "";
			AssertEquals(0, packline.HarmonisedCodes.Count);
			AssertEquals(true, packline.HarmonisedCodes.FirstItemForBinding[0].IsAutoAddedItem);
			AssertEquals(1, packline.HarmonisedCodes.FirstItemForBinding.Count);
		}

		public void TestDoNotSaveNonPersistedData()
		{
			var packline = Factory.NewWithValidTestData<PackLine>();

			Factory.Save();

			packline.JL_PackageCount = 10;
			((ILightValidationInternals)packline).IsValid = true;
			packline.HasChanges = false;

			JobPackLineHarmonisedCode code = null;
			((IBindingList)packline).ListChanged += (s, e) =>
			{
				code = packline.HarmonisedCodes.FirstItemForBinding[0];
			};

			Factory.Save();

			AssertNull("code should be null due to bizObj.SuspendListChanged()", code);
		}

		#endregion

		#region Default Values

		public void TestDefaultValues()
		{
			var packline = Factory.New<PackLine>();
			var item = packline.HarmonisedCodes.AddNew();
			AssertEquals(packline.PK, item.JLH_JL);
		}

		#endregion

		#region Implementation

		[TestedType(typeof(JobPackLineHarmonisedCodeCollection.PackLineHarmonisedCodeStandAloneCollection))]
		class HarmonisedCodeStandAloneCollectionTest : ActiveBusinessObjectCollectionTestCase<JobPackLineHarmonisedCodeCollection.PackLineHarmonisedCodeStandAloneCollection>
		{
		}

		protected override JobPackLineHarmonisedCodeCollection GetCollectionToTest()
		{
			var packline = Factory.NewWithValidTestData<PackLine>();
			var collection = packline.HarmonisedCodes;
			var code = collection.AddNew();
			code.JLH_RN_NKCountry = "BE";
			code.JLH_Code = "1111";

			Factory.Save();

			return collection;
		}

		#endregion
	}
}
