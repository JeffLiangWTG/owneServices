using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CustomizedColumnOrderTest : TestCaseWithFactory
	{
		#region Properties

		ProcessTaskTemplate Template => template ?? (template = CreateTemplate());
		ProcessTaskTemplate template;

		DummyWithCustomFields Dummy => dummy ?? (dummy = Factory.New<DummyWithCustomFields>());
		DummyWithCustomFields dummy;

		#endregion

		#region Builders

		ProcessTaskTemplate CreateTemplate()
		{
			template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "Snakey";
			template.P0_ProcessType = "DUM";
			return template;
		}

		GenCustomColumnDefinition CreateDefinition(ProcessTaskTemplate template, string name, int sequence, string type = null)
		{
			var result = template.GenCustomColumnDefinitions.AddNew();
			result.XC_Name = name;
			result.XC_Type = type ?? AddOnColumnDataType.Codes.String;
			result.XC_DisplaySequence = sequence;

			return result;
		}

		#endregion

		public void TestCacheColumnDefinitions()
		{
			var columns = new List<GenCustomColumnDefinition>();
			int seq = 0;
			foreach (var type in new AddOnColumnDataType().GetAllCodes())
			{
				columns.Add(CreateDefinition(Template, "Aardvaark" + seq, sequence: seq++, type: type));
			}

			Factory.Save();

			var customBizos = new List<DummyWithCustomFields>();
			foreach (var i in Enumerable.Range(0, 10))
			{
				var d = Factory.New<DummyWithCustomFields>();

				customBizos.Add(d);
				d.GetCustomBusinessObject();
			}

			foreach (var column in columns)
			{
				if (column.XC_Type != AddOnColumnDataType.Codes.ComboBox)
				{
					ICustomColumnDefinitionExtensions.FlyweightPropertyManagedUserDefinedCustomProperty value;
					AssertEquals(true, Factory.TryGetValueFromCacheOnly((column.PK, "FlyweightCustomPropertyDefinition"), out value));
				}
				else
				{
					(ICustomColumnDefinitionExtensions.FlyweightPropertyManagedUserDefinedCustomProperty, ICustomColumnDefinitionExtensions.FlyweightPropertyManagedUserDefinedCustomProperty, List<OnSet>) value;
					AssertEquals(true, Factory.TryGetValueFromCacheOnly((column.PK, "ComboBoxDefinition"), out value));
				}
			}
		}

		public void TestDeletion()
		{
			var counter = 0;
			var definitions = new List<GenCustomColumnDefinition>();
			foreach (var type in new AddOnColumnDataType().GetAllCodes())
			{
				definitions.Add(CreateDefinition(Template, "Col" + ++counter, sequence: counter, type: type));
			}
			Factory.Save();

			Dummy.GetCustomBusinessObject().Validation.ValidateAll();
			var collection = new UserDefinedPropertyCollection(Dummy);
			collection.Add(new ProcessTaskTemplateMatches(template));

			foreach (var definition in definitions)
			{
				definition.Delete();
			}

			AssertNoExceptionThrown(() => Dummy.GetCustomBusinessObject().Validation.ValidateAll());

			foreach (var item in collection)
			{
				Assert(item.IsDeleted);
			}
		}

		public void TestPersisted()
		{
			var definition4 = CreateDefinition(Template, "Aardvaark", sequence: 2);
			var definition1 = CreateDefinition(Template, "Spooky1", sequence: 0);
			var definition2 = CreateDefinition(Template, "Spooky2", sequence: 0);
			var definition3 = CreateDefinition(Template, "Angle", sequence: 1);
			Factory.Save();

			var collection = new UserDefinedPropertyCollection(Dummy);
			collection.Add(new ProcessTaskTemplateMatches(template));

			AssertCustomFieldOrder(new[] { definition1, definition2, definition3, definition4 }, collection);
		}

		public void TestTypeOrder()
		{
			var definition5 = CreateDefinition(Template, "Spooky", sequence: 0, type: AddOnColumnDataType.Codes.Guid);
			var definition6 = CreateDefinition(Template, "Spooky", sequence: 0, type: AddOnColumnDataType.Codes.Integer);
			var definition7 = CreateDefinition(Template, "Spooky", sequence: 0, type: AddOnColumnDataType.Codes.Short);
			var definition8 = CreateDefinition(Template, "Spooky", sequence: 0, type: AddOnColumnDataType.Codes.String);
			var definition0 = CreateDefinition(Template, "Spooky", sequence: 0, type: AddOnColumnDataType.Codes.ComboBox);
			var definition1 = CreateDefinition(Template, "Spooky", sequence: 0, type: AddOnColumnDataType.Codes.Boolean);
			var definition2 = CreateDefinition(Template, "Spooky", sequence: 0, type: AddOnColumnDataType.Codes.Byte);
			var definition3 = CreateDefinition(Template, "Spooky", sequence: 0, type: AddOnColumnDataType.Codes.Datetime);
			var definition4 = CreateDefinition(Template, "Spooky", sequence: 0, type: AddOnColumnDataType.Codes.Decimal);

			Factory.Save();

			var collection = new UserDefinedPropertyCollection(Dummy);
			collection.Add(new ProcessTaskTemplateMatches(template));

			//extra two ZStrings are from ComboBox splitting logic in UserDefinedPropertyCollection.cs AddProperty.
			AssertCustomFieldOrder(new[] { definition1, definition2, definition3, definition4, definition5, definition6, definition7, definition8, definition8, definition8 }, collection);
		}

		void AssertCustomFieldOrder(GenCustomColumnDefinition[] genCustomColumnDefinition, UserDefinedPropertyCollection collection)
		{
			AssertCustomFieldOrder(string.Empty, genCustomColumnDefinition.Select(s => (string)s.XC_Name + AddOnColumnDataType.GetTypeFromCode(s.XC_Type).Name).ToArray(), collection);
		}

		void AssertCustomFieldOrder(string message, string[] expectedCollectionNames, UserDefinedPropertyCollection collection)
		{
			var actual = collection.Select(s => ((IDescription)s.Info.MetaData.First(d => d.Id == "Description").Value).GetDescription(0) + s.Info.Type.Name).ToArray();
			AssertArrayEqualsByElements(message, expectedCollectionNames, actual);
		}

		public void TestDeletedOrder()
		{
			var definition4 = CreateDefinition(Template, "Aardvaark", sequence: 2);
			var definition1 = CreateDefinition(Template, "Spooky1", sequence: 0);
			var definition2 = CreateDefinition(Template, "Spooky2", sequence: 0);
			var definition3 = CreateDefinition(Template, "Angle", sequence: 1);
			Factory.Save();

			var collection = new UserDefinedPropertyCollection(Dummy);
			collection.Add(new ProcessTaskTemplateMatches(template));

			foreach (var column in collection)
			{
				column.TrySetValue(Dummy, (ZString)"BING");
			}

			definition4.Delete();

			Factory.Save();

			collection = new UserDefinedPropertyCollection(Dummy);
			collection.Add(new ProcessTaskTemplateMatches(template));

			AssertCustomFieldOrder("New order with deleted items", new[] { "Aardvaark", "Spooky1", "Spooky2", "Angle", }.Select(s => s + "ZString").ToArray(), collection);
		}
	}
}
