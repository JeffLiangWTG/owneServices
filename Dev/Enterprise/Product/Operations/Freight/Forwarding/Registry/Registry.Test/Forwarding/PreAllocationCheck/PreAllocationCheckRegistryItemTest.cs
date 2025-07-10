using System.Linq;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(PreAllocationCheckRegistryItem))]
	public class PreAllocationCheckRegistryItemTest : StronglyTypedRegistryItemTestCase<PreAllocationCheckCollection>
	{
		protected override StronglyTypedRegistryItem<PreAllocationCheckCollection, PreAllocationCheckCollection> GetNewRegistryItem()
		{
			return new PreAllocationCheckRegistryItem(string.Empty,
					null, null, null, RegistryStorageFlags.System, new PreAllocationCheckCollection());
		}

		public void TestTranslatablePreAllocationCheckItemMeasuresAndActions()
		{
			using (var mockCha = Res.GetLanguageInstance("ZH-CN").UseMockData())
			{
				var registryItem = new PreAllocationCheckRegistryItem("", null, null, null, RegistryStorageFlags.System, new PreAllocationCheckCollection());
				var collection = registryItem.Value;

				AddNewPreAllocationCheck(collection, PreAllocationCheck.Measures.Weight, "Test", PreAllocationCheck.Actions.None);
				var item = collection[0];
				var keyMeasureSource = ((ResourceString)item.MeasureMultilingualString).ResourceKey;
				mockCha.Put(keyMeasureSource, new ResourceStringData(keyMeasureSource, "测试"));

				AssertEquals("测试", item.MeasureMultilingualString.ToString(mockCha.Language));

				AssertActionTranslatable(mockCha, item, PreAllocationCheck.Actions.None, "无");
				AssertActionTranslatable(mockCha, item, PreAllocationCheck.Actions.Warning, "警告");
				AssertActionTranslatable(mockCha, item, PreAllocationCheck.Actions.Restriction, "限制");
			}
		}

		public void TestGetCaptions()
		{
			var registryItem = new PreAllocationCheckRegistryItem("", null, null, null, RegistryStorageFlags.System, new PreAllocationCheckCollection());
			var collection = registryItem.Value;

			AddNewPreAllocationCheck(collection, PreAllocationCheck.Measures.Weight, "Test1", PreAllocationCheck.Actions.None);
			AddNewPreAllocationCheck(collection, PreAllocationCheck.Measures.Volume, "Test2", PreAllocationCheck.Actions.None);
			AddNewPreAllocationCheck(collection, PreAllocationCheck.Measures.Chargeable, "Test3", PreAllocationCheck.Actions.None);
			AddNewPreAllocationCheck(collection, PreAllocationCheck.Measures.ShipmentCount, "Test4", PreAllocationCheck.Actions.None);

			var captions = registryItem.GetCaptions(collection);
			AssertEquals(4, captions.Count());
			AssertContainsExactElementsInAnyOrder(new[] { PreAllocationCheck.Measures.Weight, PreAllocationCheck.Measures.Volume, PreAllocationCheck.Measures.Chargeable, PreAllocationCheck.Measures.ShipmentCount }, captions);
		}

		public void TestDefaultStrings()
		{
			var collection = new PreAllocationCheckCollection();
			AddNewPreAllocationCheck(collection, PreAllocationCheck.Measures.Weight, "Test", PreAllocationCheck.Actions.None);
			var registryItem = new PreAllocationCheckRegistryItem("", null, null, null, RegistryStorageFlags.System, collection);

			AssertEquals("DefaultValue is correct.", PreAllocationCheck.Measures.Weight, registryItem.DefaultValue[0].MeasureMultilingualString.ToString());
			AssertEquals("DefaultString has 1 item.", 1, registryItem.DefaultStrings.Count());

			using (var mockCha = Res.GetLanguageInstance("ZH-CN").UseMockData())
			{
				var keyMeasureSource = ((ResourceString)registryItem.DefaultValue[0].MeasureMultilingualString).ResourceKey;
				mockCha.Put(keyMeasureSource, new ResourceStringData(keyMeasureSource, "测试"));

				AssertEquals("测试", registryItem.DefaultValue[0].MeasureMultilingualString.ToString(mockCha.Language));
			}
		}

		void AddNewPreAllocationCheck(PreAllocationCheckCollection collection, string measure, string measureMultilingualString, string action)
		{
			var item = collection.AddNew();
			item.Measure = measure;
			item.MeasureMultilingualString = ResString.GetMultilingualString(ZGuid.NewZGuid().ToString(), measureMultilingualString);
			item.Action = action;
		}

		void AssertActionTranslatable(IMockResourceStringCache mockData, PreAllocationCheck item, string actionCode, string multilingualString)
		{
			var multilingualDescription = item.ActionList.GetMultilingualDescriptionFromCode(actionCode);
			var keyNoneSource = ((ResourceString)multilingualDescription).ResourceKey;
			mockData.Put(keyNoneSource, new ResourceStringData(keyNoneSource, multilingualString));
			AssertEquals(multilingualString, multilingualDescription.ToString(mockData.Language));
		}
	}
}
