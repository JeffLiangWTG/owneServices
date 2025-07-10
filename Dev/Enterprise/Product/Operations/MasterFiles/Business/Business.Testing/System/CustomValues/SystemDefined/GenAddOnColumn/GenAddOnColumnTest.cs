using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	[TestedType(typeof(GenAddOnColumn))]
	sealed class GenAddOnColumnTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDeleteOfGenAddOnColumn()
		{
			var bizO = Factory.New<DynamicDummy>();
			bizO.SetSystemDefinedValue("Property", new ZString("hello"));
			Factory.Save();
			int count = Factory.GetDatabaseCount(typeof(GenAddOnColumn));
			bizO.Delete();
			Factory.Save();
			AssertEquals(count - 1, Factory.GetDatabaseCount(typeof(GenAddOnColumn)));
		}

		public void TestHandleInvalidZDateTime()
		{
			AssertContains("When XA_Type is 'DAT', then XA_Data must be in a range of 1900-01-01 00:00:00.000 to 2079-06-06 23:59:29.000, and in format 'yyyy-mm-dd hh:mi:ss.mmm'.  When XA_Type is 'DTE', then XA_Data must be in a range of 0001-01-01 to 9999-12-31, and in format 'yyyy-mm-dd'",
			AssertExceptionThrown<ZSaveException>(() =>
			{
				var bizO1 = Factory.New<DynamicDummy>();
				bizO1.SetSystemDefinedValue("Property", ZDateTime.Invalid);
				Factory.Save();
			}).Message);
		}

		public void TestZDateTimeValues()
		{
			var bizO1 = Factory.New<DynamicDummy>();
			bizO1.SetSystemDefinedValue("Property1", ZDateTime.MinSmallDateTimeValue);
			bizO1.SetSystemDefinedValue("Property2", ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var bizO1Loaded = factory2.Load<DynamicDummy>(bizO1.PK);
			AssertEquals(ZDateTime.MinSmallDateTimeValue, bizO1Loaded.GetSystemDefinedValue<ZDateTime>("Property1"));
			AssertEquals(ZDateTime.MaxSmallDateTimeValue, bizO1Loaded.GetSystemDefinedValue<ZDateTime>("Property2"));
		}

		public void TestHandleInvalidZDate()
		{
			AssertContains("When XA_Type is 'DAT', then XA_Data must be in a range of 1900-01-01 00:00:00.000 to 2079-06-06 23:59:29.000, and in format 'yyyy-mm-dd hh:mi:ss.mmm'.  When XA_Type is 'DTE', then XA_Data must be in a range of 0001-01-01 to 9999-12-31, and in format 'yyyy-mm-dd'",
			AssertExceptionThrown<ZSaveException>(() =>
			{
				var bizO1 = Factory.New<DynamicDummy>();
				bizO1.SetSystemDefinedValue("Property", ZDate.Invalid);
				Factory.Save();
			}).Message);
		}

		public void TestZDateValues()
		{
			var bizO1 = Factory.New<DynamicDummy>();
			bizO1.SetSystemDefinedValue("Property1", new ZDate(0001, 1, 2));
			bizO1.SetSystemDefinedValue("Property2", new ZDate(9999, 12, 31));
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var bizO1Loaded = factory2.Load<DynamicDummy>(bizO1.PK);
			AssertEquals(new ZDate(1, 1, 2), bizO1Loaded.GetSystemDefinedValue<ZDate>("Property1"));
			AssertEquals(new ZDate(9999, 12, 31), bizO1Loaded.GetSystemDefinedValue<ZDate>("Property2"));
		}

		public void TestDeleteItselfWhenSavingIfDataEmpty()
		{
			GenAddOnColumn genAddOnColumn = Factory.New<GenAddOnColumn>();

			genAddOnColumn.XA_ParentID = ZGuid.NewZGuid();
			genAddOnColumn.XA_ParentTableCode = "JE";
			genAddOnColumn.XA_Name = "XXX";
			genAddOnColumn.XA_Data = "DDD";
			Factory.Save();

			AssertEquals("Successfully saved", true, genAddOnColumn.IsInDatabase);
			AssertEquals("IsDeleted", false, genAddOnColumn.IsDeleted);

			genAddOnColumn.XA_Data = "";
			Factory.Save();
			AssertEquals("Deleted as there is no data", true, genAddOnColumn.IsDeleted);
		}

		public void TestTwoUsersSaveAddOnColumn_CS00181186()
		{
			var bizO1 = Factory.New<DynamicDummy>();
			var bizO2 = Factory.New<DynamicDummy>();
			var bizO3 = Factory.New<DynamicDummy>();
			Factory.Save();

			bizO1.SetSystemDefinedValue("Property", new ZString("hello"));
			bizO1.SetSystemDefinedValue("Property2", new ZString("ShouldntBeChanged1"));
			bizO3.SetSystemDefinedValue("Property", new ZString("ShouldntBeChanged2"));

			var factory2 = new BusinessObjectFactory();
			var bizO1Loaded = factory2.Load<DynamicDummy>(bizO1.PK);
			var bizO2Loaded = factory2.Load<DynamicDummy>(bizO2.PK);
			var bizO3Loaded = factory2.Load<DynamicDummy>(bizO3.PK);
			bizO1Loaded.SetSystemDefinedValue("Property", new ZString("hello2"));

			factory2.Save();
			Factory.Save();
			AssertEquals("hello", bizO1Loaded.GetSystemDefinedValue<ZString>("Property"));
			AssertEquals("ShouldntBeChanged1", bizO1Loaded.GetSystemDefinedValue<ZString>("Property2"));
			AssertEquals("ShouldntBeChanged2", bizO3Loaded.GetSystemDefinedValue<ZString>("Property"));

			// Ensure we invalidate the cache
			bizO2.SetSystemDefinedValue("Property", new ZString("hello3"));
			bizO2Loaded.SetSystemDefinedValue("Property", new ZString("hello4"));

			factory2.Save();
			Factory.Save();
			AssertEquals("hello3", bizO2Loaded.GetSystemDefinedValue<ZString>("Property"));
			AssertEquals("ShouldntBeChanged1", bizO1Loaded.GetSystemDefinedValue<ZString>("Property2"));
			AssertEquals("ShouldntBeChanged2", bizO3Loaded.GetSystemDefinedValue<ZString>("Property"));
		}

		[ExpectNoExceptions]
		public void TestTwoUsersSaveAddOnColumn_UniqueIndexFailureHandler()
		{
			var bizO = Factory.New<DynamicDummy>();
			Factory.Save();

			Factory.AddFetchHint(typeof(GenAddOnColumn), new ZQuery(GenAddOnColumnSchema.XA_ParentID, bizO.PK));
			AssertEquals("Store cache in Factory", "", bizO.GetSystemDefinedValue<ZString>("Property"));

			// Create a GenAddOnColumn in database, use another factory will clear the Cache
			((IDbConnected)Factory).Connection.ExecuteNonQuery($@"INSERT INTO dbo.GenAddOnColumn (XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID)
					VALUES (NEWID(), 'Property','STR','hello2','{bizO.TablePrefix}','{bizO.PK}')");

			AssertEquals("Cannot get the new GenAddOnColumn because of the Cache", "", bizO.GetSystemDefinedValue<ZString>("Property"));
			bizO.SetSystemDefinedValue("Property", new ZString("hello"));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			// Save the Factory and call UniqueIndexFailureHandler
			ZExceptionReporting.ProcessWithSaveExceptionHandling(() => Factory.Save(), () => { });

			AssertEquals("hello", bizO.GetSystemDefinedValue<ZString>("Property"));
			Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
			AssertEquals("While you were working, the DummyBizo was modified by another user. The system will now need to reload this information. Press OK to have this information loaded and then try saving again.",
				UnitTestUserNotification.Instance.LastMessage.Text);

			var factory2 = new BusinessObjectFactory();
			var bizOLoaded = factory2.Load<DynamicDummy>(bizO.PK);
			AssertEquals("hello", bizOLoaded.GetSystemDefinedValue<ZString>("Property"));
		}

		public void TestTwoUsersSaveAddOnColumn_CS00181186_MultipleInSameFactory()
		{
			var bizO1 = Factory.New<DynamicDummy>();
			Factory.Save();

			bizO1.SetSystemDefinedValue("Property", new ZString("hello"));
			var factory2 = new BusinessObjectFactory();
			var bizOLoaded = factory2.Load<DynamicDummy>(bizO1.PK);

			bizOLoaded.SetSystemDefinedValue("Property", new ZString("hello3"));
			var genAddOn = factory2.New<GenAddOnColumn>();
			genAddOn.XA_ParentID = bizOLoaded.PK;
			genAddOn.XA_ParentTableCode = bizOLoaded.TablePrefix;
			genAddOn.XA_Name = "Property";
			genAddOn.XA_Data = "hello2";
			genAddOn.XA_Type = "STR";

			AssertNoExceptionThrown(factory2.Save);
			Factory.Save();
			AssertEquals("hello", bizOLoaded.GetSystemDefinedValue<ZString>("Property"));
		}

		public void TestTwoUsersSaveAddOnColumn_CS00181186_WithManyBusinessObjectsAndCustomAddOnColumns()
		{
			const int numberOfBizOsToCreate = 100;
			var bizOs = new DynamicDummy[numberOfBizOsToCreate];

			for (int i = 0; i < numberOfBizOsToCreate; i++)
			{
				bizOs[i] = Factory.New<DynamicDummy>();
			}

			Factory.Save();

			for (int i = 0; i < numberOfBizOsToCreate; i++)
			{
				bizOs[i].SetSystemDefinedValue("Property", new ZString("hello"));
			}

			var factory2 = new BusinessObjectFactory();

			var bizOsLoaded = factory2.Load<DynamicDummy>(new ZQuery(DummyBizoSchema.PK, bizOs.Select(dummy => dummy.PK)));
			foreach (var bizOLoaded in bizOsLoaded)
			{
				bizOLoaded.SetSystemDefinedValue("Property", new ZString("hello2"));
			}

			var factory1CountPriorToSaving = Factory.GetTableHitCount(GenAddOnColumnSchema.Constants.TableName);
			var factory2CountPriorToSaving = factory2.GetTableHitCount(GenAddOnColumnSchema.Constants.TableName);
			factory2.Save();
			Factory.Save();

			foreach (var bizOLoaded in bizOsLoaded)
			{
				AssertEquals("hello", bizOLoaded.GetSystemDefinedValue<ZString>("Property"));
			}

			AssertEquals("Should have synchronised the property in a single hit.", factory1CountPriorToSaving + 5, Factory.GetTableHitCount(GenAddOnColumnSchema.Constants.TableName));
			AssertEquals("Should have synchronised the property in a single hit.", factory2CountPriorToSaving, factory2.GetTableHitCount(GenAddOnColumnSchema.Constants.TableName));
		}

		public void TestTwoUsersSaveAddOnColumn_CS00181186_WithLotsOfParametersToAdd()
		{
			const int numberOfBizOsToCreate = 640;
			var bizOs = new DynamicDummy[numberOfBizOsToCreate];

			for (int i = 0; i < numberOfBizOsToCreate; i++)
			{
				bizOs[i] = Factory.New<DynamicDummy>();
			}

			Factory.Save();

			for (int i = 0; i < numberOfBizOsToCreate; i++)
			{
				for (int j = 0; j < 5; j++)
				{
					bizOs[i].SetSystemDefinedValue(string.Format("Property{0}", j), new ZString("hello"));
				}
			}

			var factory2 = new BusinessObjectFactory();

			var bizOsLoaded = factory2.Load<DynamicDummy>(new ZQuery(DummyBizoSchema.PK, bizOs.Select(dummy => dummy.PK)));
			for (int i = 0; i < numberOfBizOsToCreate; i++)
			{
				for (int j = 0; j < 5; j++)
				{
					bizOsLoaded[i].SetSystemDefinedValue(string.Format("Property{0}", j), new ZString("hello2"));
				}
			}

			var factory1CountPriorToSaving = Factory.GetTableHitCount(GenAddOnColumnSchema.Constants.TableName);
			var factory2CountPriorToSaving = factory2.GetTableHitCount(GenAddOnColumnSchema.Constants.TableName);
			AssertNoExceptionThrown(factory2.Save);
			AssertNoExceptionThrown(Factory.Save);

			foreach (var bizOLoaded in bizOsLoaded)
			{
				for (int j = 0; j < 5; j++)
				{
					AssertEquals("hello", bizOLoaded.GetSystemDefinedValue<ZString>(string.Format("Property{0}", j)));
				}
			}

			AssertEquals("Should have synchronised the property in minimal hits.", factory1CountPriorToSaving + 13 + 133, Factory.GetTableHitCount(GenAddOnColumnSchema.Constants.TableName));
			AssertEquals("Should have synchronised the property in minimal hits.", factory2CountPriorToSaving, factory2.GetTableHitCount(GenAddOnColumnSchema.Constants.TableName));
		}

		public void TestParent()
		{
			var bizO = Factory.New<DynamicDummy>();
			var genAddOnColumn = Factory.New<GenAddOnColumn>();

			genAddOnColumn.XA_ParentID = bizO.PK;
			genAddOnColumn.XA_ParentTableCode = bizO.TablePrefix;
			genAddOnColumn.XA_Name = "XXX";
			genAddOnColumn.XA_Data = "DDD";
			AssertNull(genAddOnColumn.Parent);

			genAddOnColumn.Parent = bizO;
			AssertEquals(bizO, genAddOnColumn.Parent);
		}

		public void TestConcurrencyWithoutDelete()
		{
			var column = Factory.New<GenAddOnColumn>();
			column.XA_Data = "one";
			Factory.Save();
			var columnReloaded = new BusinessObjectFactory() { RefreshEnabled = false }.Load<GenAddOnColumn>(column.PK);
			columnReloaded.XA_Data = "changed";
			columnReloaded.Factory.Save();

			column.XA_Data = "two";
			AssertNoExceptionThrown(() => ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, null, true));
			AssertEquals("Merge completed with database value", "changed", column.XA_Data);
		}

		public void TestConcurrencyWithDelete()
		{
			var column = Factory.New<GenAddOnColumn>();
			column.XA_Data = "one";
			Factory.Save();
			var columnReloaded = new BusinessObjectFactory() { RefreshEnabled = false }.Load<GenAddOnColumn>(column.PK);
			columnReloaded.Delete();
			columnReloaded.Factory.Save();

			column.XA_Data = "two";
			AssertNoExceptionThrown(() => ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, null, true));
			Assert("Delete completed by ConcurrencyResolver", column.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			GenAddOnColumn result = factory.New<GenAddOnColumn>();
			result.XA_Data = "DDD";
			return result;
		}
	}
}
