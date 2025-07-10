using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(DateAndReferenceRegistryItem))]
	class DateAndReferencesRegistryItemTest : StronglyTypedRegistryItemTestCase<DateAndReferenceCollection>
	{
		protected override StronglyTypedRegistryItem<DateAndReferenceCollection, DateAndReferenceCollection> GetNewRegistryItem()
		{
			return new DateAndReferenceRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, new DateAndReferenceCollection());
		}

		#region TestTranslatable

		public void TestTranslatable()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var registryItem = new DateAndReferenceRegistryItem("", null, null, null, RegistryStorageFlags.System, new DateAndReferenceCollection());
				var drCollection = registryItem.Value;
				AddNewDateAndReference(drCollection, "TST", "Wassup", "From", "To");

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, drCollection);

				var keyDescription = ((ResourceString)registryItem.Value[0].Description).ResourceKey;
				mockChs.Put(keyDescription, new ResourceStringData(keyDescription, "废话"));
				AssertEquals("废话", registryItem.Value[0].Description.ToString(Core.SharedConstants.Languages.ChineseSimplified));

				var keyFromLabel = ((ResourceString)registryItem.Value[0].AllowRequiredFromLabel).ResourceKey;
				mockChs.Put(keyFromLabel, new ResourceStringData(keyFromLabel, "自"));
				AssertEquals("自", registryItem.Value[0].AllowRequiredFromLabel.ToString(Core.SharedConstants.Languages.ChineseSimplified));

				var keyToLabel = ((ResourceString)registryItem.Value[0].AllowRequiredToLabel).ResourceKey;
				mockChs.Put(keyToLabel, new ResourceStringData(keyToLabel, "至"));
				AssertEquals("至", registryItem.Value[0].AllowRequiredToLabel.ToString(Core.SharedConstants.Languages.ChineseSimplified));

				var captions = new List<string>(registryItem.GetCaptions(drCollection));
				int numOfCaptions = captions.Count;
				AddNewDateAndReference(drCollection, "STS", "Whatever", " ", "");
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, drCollection);
				captions = new List<string>(registryItem.GetCaptions(drCollection));
				AssertEquals(numOfCaptions + 1, captions.Count);
			}
		}

		void AddNewDateAndReference(DateAndReferenceCollection drCollection, string code, string englishDescription, string englishRequireFrom, string englishRequiredTo)
		{
			var reference = drCollection.AddNew();
			reference.Code = code;
			reference.Description = ResString.GetMultilingualString(ZGuid.NewZGuid().ToString(), englishDescription);
			reference.BookingDirection = "ANY";
			reference.InstructionType = "ANY";
			reference.OrganisationType = "ANY";
			reference.ContainerMode = "ANY";
			reference.AllowActualDate = true;
			reference.AllowEstimatedDate = true;
			reference.AllowRequiredFromDate = true;
			reference.AllowRequiredToDate = true;
			reference.AllowReference = true;
			reference.AllowReceivedBy = true;

			reference.AllowRequiredFromLabel = ResString.GetMultilingualString(ZGuid.NewZGuid().ToString(), englishRequireFrom);
			reference.AllowRequiredToLabel = ResString.GetMultilingualString(ZGuid.NewZGuid().ToString(), englishRequiredTo);
		}

		#endregion

		#region TestGetCaptions_NoDuplicates

		public void TestGetCaptions_NoDuplicates()
		{
			var registryItem = new DateAndReferenceRegistryItem("", null, null, null, RegistryStorageFlags.System, new DateAndReferenceCollection());
			var drCollection = registryItem.Value;
			AddNewDateAndReference(drCollection, "TTT", "Description", "From", "To");
			AddNewDateAndReference(drCollection, "SSS", "Description", "From", "To");
			AddNewDateAndReference(drCollection, "RRR", "Description", "From", "To");

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, drCollection);

			var captions = registryItem.GetCaptions(drCollection);
			AssertEquals(3, captions.Count());
			AssertContainsExactElementsInAnyOrder(new[] { "Description", "From", "To" }, captions);
		}

		#endregion

		#region TestDefaultStrings

		public void TestDefaultStrings_TranslatesCorrectly()
		{
			var testDefaults = new DateAndReferenceCollection();
			AddNewDateAndReference(testDefaults, "TST", "Description", "From", "To");
			var registryItem = new DateAndReferenceRegistryItem("", null, null, null, RegistryStorageFlags.System, testDefaults);

			AssertEquals("Precondtion: DefaultValue is correct.", "From", registryItem.DefaultValue[0].AllowRequiredFromLabel.ToString());

			AssertEquals("DefaultString has 3 items.", 3, registryItem.DefaultStrings.Count());
			using (var mockChs = Res.GetLanguageInstance("CHS").UseMockData())
			{
				var description = registryItem.DefaultStrings.First().ResourceKey;
				mockChs.Put(description, new ResourceStringData(description, "废话")); //probably no correct chinese translation
				AssertEquals("废话", registryItem.DefaultStrings.First().ToString("CHS"));
			}
		}

		public void TestDefaultStrings_IsInCorrectOrder()
		{
			var testDefaults = new DateAndReferenceCollection();
			AddNewDateAndReference(testDefaults, "TST", "Description", "From", "To");
			AddNewDateAndReference(testDefaults, "NXT", "SecondDescription", "Originating", "Concluding");
			var registryItem = new DateAndReferenceRegistryItem("", null, null, null, RegistryStorageFlags.System, testDefaults);

			AssertEquals("DefaultString has 6 items.", 6, registryItem.DefaultStrings.Count());
			var correctOrder = new string[] { "Description", "SecondDescription", "From", "Originating", "To", "Concluding" };

			var index = 0;
			CombineAssertions(() =>
			{
				foreach (var str in registryItem.DefaultStrings)
				{
					var correctString = correctOrder[index];
					AssertEquals($"{correctString} is at index:{index} matched", correctString, str.ToString());
					index++;
				}
			});
		}

		#endregion

	}
}
