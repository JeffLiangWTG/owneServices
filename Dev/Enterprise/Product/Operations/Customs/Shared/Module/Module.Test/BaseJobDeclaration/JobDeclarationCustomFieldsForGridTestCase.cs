using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module.Testing
{
	sealed class JobDeclarationCustomFieldsForGridTestCase : TestCaseWithFactory
	{
		public void TestNothingVisibleWhenRegistryNotSet()
		{
			using (var formToTest = new ZForm())
			{
				var parentControlToTest = GetNewParentControlToTest();
				formToTest.Controls.Add(parentControlToTest);
				formToTest.Show();

				var controlsToTestForCaptions = GetColumnsHashTable(fGridToTest);
				foreach (var customFieldKey in CustomFieldKeys)
				{
					AssertControlInVisibleForCustomFieldKey(customFieldKey, controlsToTestForCaptions);
				}
			}
		}

		public void TestVisibilityOfCustomFields()
		{
			foreach (var customFieldKey in CustomFieldKeys)
			{
				using (var formToTest = new ZForm())
				{
					var parentControlToTest = GetNewParentControlToTest();
					formToTest.Controls.Add(parentControlToTest);
					SetupRegistryItemClearingOthers(customFieldKey, TestCaption, TestHint);
					formToTest.Show();
					var controlsToTestForCaptions = GetColumnsHashTable(fGridToTest);
					var controlsToTestForBinding = GetColumnsHashTable(fGridToTest);
					AssertEquals("Column should exist in Hash table", true, controlsToTestForBinding.Contains(customFieldKey));
					AssertEquals("Column should exist in Hash table", true, controlsToTestForCaptions.Contains(customFieldKey));
					AssertVisibilityofControlForCustomFieldKey(customFieldKey, controlsToTestForCaptions);
				}
			}
		}

		void AssertVisibilityofControlForCustomFieldKey(string customFieldKeyToTest, Hashtable objectsToTestForCaptions)
		{
			foreach (var customFieldKey in CustomFieldKeys)
			{
				if (customFieldKey == customFieldKeyToTest)
				{
					AssertControlVisibleForCustomFieldKey(customFieldKey, objectsToTestForCaptions);
				}
				else
				{
					AssertControlInVisibleForCustomFieldKey(customFieldKey, objectsToTestForCaptions);
				}
			}
		}

		void AssertControlInVisibleForCustomFieldKey(string customFieldKeyToTest, Hashtable objectsToTestForCaptions)
		{
			AssertNull("Column should be null", objectsToTestForCaptions[customFieldKeyToTest]);
		}

		void AssertControlVisibleForCustomFieldKey(string customFieldKeyToTest, Hashtable objectsToTestForCaptions)
		{
			var column = (ZGridColumnInfo)objectsToTestForCaptions[customFieldKeyToTest];
			AssertNotNull("Column reference has to be an instance of an existing column", column);
			AssertEquals("Column is visible", true, column.IsVisible);
		}

		Control GetNewParentControlToTest()
		{
			var control = GetNewParentControlWithGridToTest();
			fGridToTest = ((JobDeclarationFilterStripControl)control).FilteredGrid;
			return control;
		}

		Control GetNewParentControlWithGridToTest()
		{
			var declarations = new BaseJobDeclarationCollection(Factory);
			var filterBo = new JobDeclarationFilterBusinessObject();
			return new JobDeclarationFilterStripControl(null, declarations, filterBo);
		}

		Hashtable GetColumnsHashTable(ZGrid grid)
		{
			var key = "String1";
			var objectsToTestForCaptions = new Hashtable { { key, GetColumnWithCaption(grid, (string)BindToHash[key]) } };
			key = "String2";
			objectsToTestForCaptions.Add(key, GetColumnWithCaption(grid, (string)BindToHash[key]));
			key = "Date1";
			objectsToTestForCaptions.Add(key, GetColumnWithCaption(grid, (string)BindToHash[key]));
			key = "Date2";
			objectsToTestForCaptions.Add(key, GetColumnWithCaption(grid, (string)BindToHash[key]));
			key = "Decimal1";
			objectsToTestForCaptions.Add(key, GetColumnWithCaption(grid, (string)BindToHash[key]));
			key = "Decimal2";
			objectsToTestForCaptions.Add(key, GetColumnWithCaption(grid, (string)BindToHash[key]));
			key = "Flag1";
			objectsToTestForCaptions.Add(key, GetColumnWithCaption(grid, (string)BindToHash[key]));
			key = "Flag2";
			objectsToTestForCaptions.Add(key, GetColumnWithCaption(grid, (string)BindToHash[key]));
			return objectsToTestForCaptions;
		}

		ZGridColumnInfo GetColumnWithCaption(ZGrid grid, string bindToToFind)
		{
			foreach (ZGridColumnInfo column in grid.ColumnStyles)
			{
				if (column.ColumnName == bindToToFind)
				{
					return column;
				}
			}
			return null;
		}

		ZGrid fGridToTest;

		void SetupRegistryItemClearingOthers(string customFieldKey, string caption, string hint)
		{
			foreach (CaptionAndHintRegistryItem item in RegistryHash.Values)
			{
				item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("", ""));
			}
			((CaptionAndHintRegistryItem)RegistryHash[customFieldKey]).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint(caption, hint));
		}

		Hashtable RegistryHash => fRegistries ?? (fRegistries = new Hashtable
		{
			{ "String1", FreightRegistry.ShipmentCustomText1 },
			{ "String2", FreightRegistry.ShipmentCustomText2 },
			{ "Date1", FreightRegistry.ShipmentCustomDate1 },
			{ "Date2", FreightRegistry.ShipmentCustomDate2 },
			{ "Decimal1", FreightRegistry.ShipmentCustomDecimalNo1 },
			{ "Decimal2", FreightRegistry.ShipmentCustomDecimalNo2 },
			{ "Flag1", FreightRegistry.ShipmentCustomFlag1 },
			{ "Flag2", FreightRegistry.ShipmentCustomFlag2 }
		});

		Hashtable BindToHash => fBindTos ?? (fBindTos = new Hashtable
		{
			{ "String1", "DocsAndCartage+JP_CustomAttrib1" },
			{ "String2", "DocsAndCartage+JP_CustomAttrib2" },
			{ "Date1", "DocsAndCartage+JP_CustomDate1" },
			{ "Date2", "DocsAndCartage+JP_CustomDate2" },
			{ "Decimal1", "DocsAndCartage+JP_CustomDecimal1" },
			{ "Decimal2", "DocsAndCartage+JP_CustomDecimal2" },
			{ "Flag1", "DocsAndCartage+JP_CustomFlag1" },
			{ "Flag2", "DocsAndCartage+JP_CustomFlag2" }
		});

		Hashtable fBindTos;
		Hashtable fRegistries;

		string[] CustomFieldKeys => fCustomFieldKeys ?? (fCustomFieldKeys = new[]
												  { "String1", "String2", "Date1", "Date2", "Decimal1", "Decimal2", "Flag1", "Flag2" });

		string[] fCustomFieldKeys;

		const string TestCaption = "TestCaption";
		const string TestHint = "TestHint";

		FreightDataRegistry FreightRegistry => FreightDataRegistry.Instance;
	}
}
