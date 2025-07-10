using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class DocAddressesValidationStatusFilterModuleStrategyTest : TestCaseWithFactory
	{
		public void TestNewFilterAdded()
		{
			CreateWhsOrder(out var whsOrder, out _);
			var filters = new ModuleFilterCollection();
			var validationStatusStrategy = new DocAddressesValidationStatusFilterModuleStrategy();
			validationStatusStrategy.RunOnModuleFiltersCreated(filters, whsOrder.GetType(), Factory);
			AssertNotNull(filters["Address Validation Status" + FilterModuleStrategy.UniqueSuffix]);

			filters = new ModuleFilterCollection();
			validationStatusStrategy.RunOnModuleFiltersCreated(filters, typeof(DummyBusinessObject), Factory);
			AssertNull(filters["Address Validation Status" + FilterModuleStrategy.UniqueSuffix]);
		}

		public void TestFilterList()
		{
			CreateWhsOrder(out var whsOrder, out _);
			var filters = new ModuleFilterCollection();
			var validationStatusStrategy = new DocAddressesValidationStatusFilterModuleStrategy();
			validationStatusStrategy.RunOnModuleFiltersCreated(filters, whsOrder.GetType(), Factory);
			var filter = filters["Address Validation Status" + FilterModuleStrategy.UniqueSuffix] as ModuleTextFilter;
			AssertNotNull(filter);
			AssertContainsExactElementsInAnyOrder(AddressValidationStatusList.AddressValidationStatuses.GetAllCodes(), (filter.List as CodeDescriptionPairList).GetAllCodes());
		}

		public void TestFilterQuery()
		{
			#region Prepare Test Data

			CreateWhsOrder(out var whsOrder, out var orgHeader);
			var jobDocAddressOverride = Factory.New<JobDocAddress>();
			jobDocAddressOverride.E2_AddressType = DocAddressTypes.Codes.Carrier;
			jobDocAddressOverride.E2_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			jobDocAddressOverride.E2_AddressOverride = true;
			jobDocAddressOverride.E2_Address1 = "WOO HA HA HA";
			jobDocAddressOverride.E2_ParentID = whsOrder.PK;
			jobDocAddressOverride.E2_ValidationStatus = AddressValidationStatus.CountryNotAvailable;

			var jobDocAddressNotOverride = Factory.New<JobDocAddress>();
			jobDocAddressNotOverride.E2_AddressType = DocAddressTypes.Codes.NotifyParty;
			jobDocAddressNotOverride.E2_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			jobDocAddressNotOverride.E2_AddressOverride = false;
			jobDocAddressNotOverride.E2_Address1 = "WOO HA HA HA";
			jobDocAddressNotOverride.E2_ParentID = whsOrder.PK;
			jobDocAddressNotOverride.E2_OA_Address = orgHeader.MainAddress.PK;

			orgHeader.MainAddress.OA_ValidationStatus = AddressValidationStatus.Verified;

			Factory.Save();

			#endregion

			AssertEquals("When Address is not override, E2_ValidationStatus matches linked address OA_ValidationStatus", orgHeader.MainAddress.OA_ValidationStatus, jobDocAddressNotOverride.E2_ValidationStatus);

			var filters = new ModuleFilterCollection();
			var validationStatusStrategy = new DocAddressesValidationStatusFilterModuleStrategy();
			validationStatusStrategy.RunOnModuleFiltersCreated(filters, whsOrder.GetType(), Factory);
			var addressStatusFilter = filters["Address Validation Status" + FilterModuleStrategy.UniqueSuffix] as ModuleTextFilter;
			addressStatusFilter.Property = AddressValidationStatus.Verified;

			var referenceFilter = filters.AddTextFilter("Reference", WhsDocketSchema.WD_ExternalReference);
			referenceFilter.Property = "W000002345";
			referenceFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			var query = filters.GetFilterQuery(new[] { addressStatusFilter, referenceFilter });
			var result = Factory.Load<IWhsDocket>(query);
			AssertEquals(1, result.Length);
			AssertEquals(whsOrder.PK, result[0].PK);

			addressStatusFilter.Property = AddressValidationStatus.NotRequired;
			query = filters.GetFilterQuery(new[] { addressStatusFilter, referenceFilter });
			result = Factory.Load<IWhsDocket>(query);
			AssertEquals(0, result.Length);

			addressStatusFilter.Property = AddressValidationStatus.CountryNotAvailable;
			query = filters.GetFilterQuery(new[] { addressStatusFilter, referenceFilter });
			result = Factory.Load<IWhsDocket>(query);
			AssertEquals(1, result.Length);
			AssertEquals(whsOrder.PK, result[0].PK);
		}

		void CreateWhsOrder(out IWhsDocket whsOrder, out OrgHeader orgHeader)
		{
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_Address1 = "ABC TEST";
			Factory.Save();

			var warehousePK = ZGuid.NewZGuid();
			var whsDocketPK = ZGuid.NewZGuid();

			Db.Connection.ExecuteNonQuery($@"INSERT dbo.WhsWarehouse (
	WW_PK,
	WW_WarehouseCode, 
	WW_WLT_DefaultLocationType,
	WW_OA_WarehouseAddress,
	WW_GB_RelatedCompanyBranch,
	WW_IsVirtualWarehouse,
	WW_SystemCreateTimeUtc,
	WW_SystemCreateUser,
	WW_SystemLastEditTimeUtc,
	WW_SystemLastEditUser
) VALUES (
	'{warehousePK}', 
	'WH1', 
	'16C9FD62-730A-42ED-A20E-699606FFF360',
	'{orgHeader.MainAddress.PK}',
	'{Env.CurrentBranch.PK}',
	1,
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP'
)");

			Db.Connection.ExecuteNonQuery($@"INSERT dbo.WhsDocket (
	WD_PK,
	WD_DocketStatus,
	WD_WW_Whs,
	WD_OH_Client,
	WD_DocketType,
	WD_DocketSubType,
	WD_BookingDate,
	WD_DocketID,
	WD_ExternalReference,
	WD_SystemCreateTimeUtc,
	WD_SystemCreateUser,
	WD_SystemLastEditTimeUtc,
	WD_SystemLastEditUser
) VALUES (
	'{whsDocketPK}',
	'ENT',
	'{warehousePK}',
	'{orgHeader.PK}',
	'ORD',
	'ORD',
	GETDATE(),
	'W000001234',
	'W000002345',
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP'
)");
			whsOrder = Factory.Load<IWhsDocket>(whsDocketPK);
			AssertNotNull("Precondition", whsOrder);
			AssertEquals("Precondition", ControllerIDs.WhsOrder.Name, whsOrder.GetType().Name);
			AssertEquals("Precondition", true, typeof(IWhsDocket).IsAssignableFrom(whsOrder.GetType()));
		}
	}
}
