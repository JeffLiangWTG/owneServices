using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.OceanCarrier.ReeferSetting;

[TestedType(typeof(Business.ReeferSetting))]
sealed class ReeferSettingTest : EnterpriseBusinessObjectTestCase
{
	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

	protected override BusinessObject GetNewBusinessObject()
	{
		return CreateNewReeferSetting();
	}

	public void TestShouldCreateReeferDetailsUpdatedEventForInsertedRecord()
	{
		var parent = CreateParent();
		Factory.Save();

		CreateNewReeferSetting(parent);
		Factory.Save();

		var logs = (parent as IStmALogParent)?.Logs.GetAllLogs();
		AssertEquals(1, logs?.Count);
		AssertEquals(EventCodes.ReeferDetailsUpdated, logs?[0].SL_SE_NKEvent);
	}

	public void TestShouldCreateReeferDetailsUpdatedEventForModifiedRecord()
	{
		var parent = CreateParent();
		Factory.Save();

		var reeferSetting = CreateNewReeferSetting(parent);
		Factory.Save();

		var logs = (parent as IStmALogParent)?.Logs.GetAllLogs();
		AssertEquals(1, logs?.Count);

		reeferSetting.RFS_RequiresHumidityControl = ZBool.True;
		reeferSetting.RFS_HumidityPercentMinimum = new ZDecimal(5.4m);
		reeferSetting.RFS_HumidityPercentMaximum = new ZDecimal(25.4m);
		reeferSetting.RFS_HumidityPercentSetPoint = new ZDecimal(20m);

		Factory.Save();

		logs = (parent as IStmALogParent)?.Logs.GetAllLogs();
		AssertEquals(2, logs?.Count);
		AssertEquals(EventCodes.ReeferDetailsUpdated, logs?[1].SL_SE_NKEvent);
	}

	public void TestShouldCreateReeferDetailsUpdatedEventForDeletedRecord()
	{
		var parent = CreateParent();
		Factory.Save();

		var reeferSetting = CreateNewReeferSetting(parent);
		Factory.Save();

		var logs = (parent as IStmALogParent)?.Logs.GetAllLogs();
		AssertEquals(1, logs?.Count);

		reeferSetting.Delete();
		Factory.Save();

		logs = (parent as IStmALogParent)?.Logs.GetAllLogs();
		AssertEquals(2, logs?.Count);
		AssertEquals(EventCodes.ReeferDetailsUpdated, logs?[1].SL_SE_NKEvent);
	}

	public void TestShouldCreateOneReeferDetailsUpdatedEventWhenMultipleReeferSettingsWereChangedForTheSameParentEntityInTheSameTransaction()
	{
		var parent = CreateParent();
		Factory.Save();

		var reeferSettingOne = CreateNewReeferSetting(parent);
		var reeferSettingTwo = CreateNewReeferSetting(parent);
		Factory.Save();

		var logs = (parent as IStmALogParent)?.Logs.GetAllLogs();
		AssertEquals(1, logs?.Count);
		AssertEquals(EventCodes.ReeferDetailsUpdated, logs?[0].SL_SE_NKEvent);
	}

	public void TestShouldCreateMultipleReeferDetailsUpdatedEventWhenMultipleReeferSettingsWereChangedForMultipleParentEntitiesInTheSameTransaction()
	{
		var parentOne = CreateParent();
		var parentTwo = CreateParent();
		Factory.Save();

		CreateNewReeferSetting(parentOne);
		CreateNewReeferSetting(parentTwo);

		Factory.Save();

		var logs = (parentOne as IStmALogParent)?.Logs.GetAllLogs();
		AssertEquals(1, logs?.Count);
		AssertEquals(EventCodes.ReeferDetailsUpdated, logs?[0].SL_SE_NKEvent);

		logs = (parentTwo as IStmALogParent)?.Logs.GetAllLogs();
		AssertEquals(1, logs?.Count);
		AssertEquals(EventCodes.ReeferDetailsUpdated, logs?[0].SL_SE_NKEvent);
	}

	public void TestShouldCreateMultipleReeferDetailsUpdatedEvents_WhenReeferSettingsWereChangedForTheSameParentEntityInMultipleTransactions()
	{
		var parent = CreateParent();
		Factory.Save();

		CreateNewReeferSetting(parent);
		Factory.Save();

		CreateNewReeferSetting(parent);
		Factory.Save();

		var logs = (parent as IStmALogParent)?.Logs.GetAllLogs();
		AssertEquals(2, logs?.Count);
		AssertEquals(EventCodes.ReeferDetailsUpdated, logs?[0].SL_SE_NKEvent);
		AssertEquals(EventCodes.ReeferDetailsUpdated, logs?[1].SL_SE_NKEvent);
	}

	BusinessObject CreateParent()
	{
		var type = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(CarrierShipmentCargoSchema.Constants.Prefix);
		AssertNotNull(type);

		return Factory.NewWithValidTestData(type);
	}

	Business.ReeferSetting CreateNewReeferSetting(BusinessObject parent = null)
	{
		var reeferSetting = Factory.New<Business.ReeferSetting>();
		reeferSetting.RFS_ValidFrom = ZDateTimeOffset.Now;
		reeferSetting.RFS_ParentId = parent?.PK ?? ZGuid.NewZGuid();
		reeferSetting.RFS_ParentTableCode = parent?.TablePrefix ?? CarrierShipmentCargoSchema.Constants.Prefix;

		return reeferSetting;
	}
}
