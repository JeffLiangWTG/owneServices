using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	public class DataModelHelperTest : TestCaseWithFactory
	{
		public void TestPopulateDataModelForBOThatIsInDataBase()
		{
			var dataModelSupporter = new DataModelSupporter();
			dataModelSupporter.PopulateDataModelIfNeeded();
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, dataModelSupporter.DataModel);

			var childDataModelSupporter = new DataModelSupporter();
			childDataModelSupporter.PopulateDataModelFromParentIfNeeded(dataModelSupporter);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, childDataModelSupporter.DataModel);
		}

		class DataModelSupporter : NonPersistentBusinessObject, IDataModelSupporter
		{
			public DataModelSupporter()
			{
			}

			public override bool IsInDatabase => true;

			public ZString DataModel { get; set; }

			public void PopulateDataModelIfNeeded() => this.PopulateDataModelFromCountryCodeIfNeeded(Core.Constants.CountryCodes.UnitedStates);
		}
	}
}
