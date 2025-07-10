using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(DeclarationLockConfigRegistryItemDataType))]
	sealed class DeclarationLockConfigRegistryItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DeclarationLockConfigRegistryItemDataType>
	{
		protected override DeclarationLockConfigRegistryItemDataType GetNewDataType()
		{
			return new DeclarationLockConfigRegistryItemDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "DeclarationLockConfigRegistryItemEditor"; }
		}

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject obj1, NonPersistentBusinessObject obj2)
		{
			base.AssertValuesEqual(message, obj1, obj2);

			var config1 = (DeclarationLockConfig)obj1;
			var config2 = (DeclarationLockConfig)obj2;

			AssertEquals("Events.Count", config1.EventInfos.Count, config2.EventInfos.Count);
			AssertEquals("Tabs.Count", config1.TabInfos.Count, config2.TabInfos.Count);

			for (var i = 0; i < config1.EventInfos.Count; ++i)
			{
				foreach (ZPropertyInfo propertyInfo in config1.EventInfos[i].ZPropertyInfoHash)
				{
					AssertEquals(config1.EventInfos[i][propertyInfo.Name], config2.EventInfos[i][propertyInfo.Name]);
				}
			}

			for (var i = 0; i < config1.TabInfos.Count; ++i)
			{
				foreach (ZPropertyInfo propertyInfo in config1.TabInfos[i].ZPropertyInfoHash)
				{
					AssertEquals(config1.TabInfos[i][propertyInfo.Name], config2.TabInfos[i][propertyInfo.Name]);
				}
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);

			var collection1 = new DeclarationLockConfigCollection(fallbackLevel, new BusinessObjectFactory());
			var config1 = collection1.AddNew();
			config1.DeclarationType = "IMP";
			var eventInfo1 = config1.EventInfos.AddNew();
			eventInfo1.EventType = "ARV";
			eventInfo1.EventReference = "DEP=AAA";
			eventInfo1.EventSource = "DEC";
			var tabInfo1 = config1.TabInfos.AddNew();
			tabInfo1.TabPage = "DEC";

			var collection2 = new DeclarationLockConfigCollection(fallbackLevel, new BusinessObjectFactory());
			var config2 = collection2.AddNew();
			config2.DeclarationType = "EXP";
			var eventInfo2 = config2.EventInfos.AddNew();
			eventInfo2.EventType = "DEP";
			eventInfo2.EventReference = "ARV=BB";
			eventInfo2.EventSource = "DEC";
			var tabInfo2 = config2.TabInfos.AddNew();
			tabInfo2.TabPage = "DEC";

			return
			[
				new ValidSampleAndBinaryValueInDB(collection1, new DeclarationLockConfigRegistryItemDataType().Serialise(collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, new DeclarationLockConfigRegistryItemDataType().Serialise(collection2))
			];
		}
	}
}
