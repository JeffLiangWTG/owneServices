using System;
using CargoWise.ComponentModel;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Warehouse.DataTransfer.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing;

sealed class WhsDocketValueObjectDataUniversalAdapterTest : WhsTestCaseWithFactory
{
	#region Test Cases

	public void TestWhsOrdersImport()
	{
		var collection = new WhsDocketGenericCollection(Factory);
		var xsdDockets = new Xsd.WhsDockets();
		var xsdDocket1 = xsdDockets.WhsDocket.AddNew();
		var xsdDocket2 = xsdDockets.WhsDocket.AddNew();
		xsdDocket1.Identifier.Reference = "1";
		xsdDocket1.Identifier.DocketType = DocketTypes.Codes.WhsOrder;
		xsdDocket2.Identifier.Reference = "2";
		xsdDocket2.Identifier.DocketType = DocketTypes.Codes.WhsOrder;

		var context = new ValueObjectImportContext(Factory, Notifications);
		Adapter.ImportFromValueObject(collection, xsdDockets, context);

		AssertEquals("Collection should have new elements", 2, collection.Count);
		AssertEquals(typeof(WhsOrder), collection[0].GetType());
		AssertEquals(typeof(WhsOrder), collection[1].GetType());
	}

	public void TestWhsReceiveImport()
	{
		var collection = new WhsDocketGenericCollection(Factory);
		var xsdDockets = new Xsd.WhsDockets();
		var xsdDocket1 = xsdDockets.WhsDocket.AddNew();
		var xsdDocket2 = xsdDockets.WhsDocket.AddNew();
		xsdDocket1.Identifier.Reference = "1";
		xsdDocket1.Identifier.DocketType = DocketTypes.Codes.WhsASN;
		xsdDocket2.Identifier.Reference = "2";
		xsdDocket2.Identifier.DocketType = DocketTypes.Codes.WhsASN;

		var context = new ValueObjectImportContext(Factory, Notifications);
		Adapter.ImportFromValueObject(collection, xsdDockets, context);

		AssertEquals("Collection should have new elements", 2, collection.Count);
		AssertEquals(typeof(WhsReceive), collection[0].GetType());
		AssertEquals(typeof(WhsReceive), collection[1].GetType());
	}

	public void TestWhsOrdersAndWhsReceiveMixedImport()
	{
		var collection = new WhsDocketGenericCollection(Factory);
		var xsdDockets = new Xsd.WhsDockets();
		var xsdDocket1 = xsdDockets.WhsDocket.AddNew();
		var xsdDocket2 = xsdDockets.WhsDocket.AddNew();
		var xsdDocket3 = xsdDockets.WhsDocket.AddNew();
		var xsdDocket4 = xsdDockets.WhsDocket.AddNew();
		xsdDocket1.Identifier.DocketType = DocketTypes.Codes.WhsOrder;
		xsdDocket1.Identifier.Reference = "1";

		xsdDocket2.Identifier.DocketType = DocketTypes.Codes.WhsASN;
		xsdDocket2.Identifier.Reference = "2";

		xsdDocket3.Identifier.DocketType = DocketTypes.Codes.WhsOrder;
		xsdDocket3.Identifier.Reference = "3";

		xsdDocket4.Identifier.DocketType = DocketTypes.Codes.WhsASN;
		xsdDocket4.Identifier.Reference = "4";

		var context = new ValueObjectImportContext(Factory, Notifications);
		Adapter.ImportFromValueObject(collection, xsdDockets, context);

		AssertEquals("Collection should have a new element", 4, collection.Count);
		collection.ApplySort("WD_ExternalReference", System.ComponentModel.ListSortDirection.Ascending);
		AssertEquals(typeof(WhsOrder), collection[0].GetType());
		AssertEquals(typeof(WhsReceive), collection[1].GetType());
		AssertEquals(typeof(WhsOrder), collection[2].GetType());
		AssertEquals(typeof(WhsReceive), collection[3].GetType());
	}

	public void TestInvalidDocketTypeMixedImport()
	{
		var collection = new WhsDocketGenericCollection(Factory);
		var xsdDockets = new Xsd.WhsDockets();
		var xsdDocket1 = xsdDockets.WhsDocket.AddNew();
		var xsdDocket2 = xsdDockets.WhsDocket.AddNew();
		var xsdDocket3 = xsdDockets.WhsDocket.AddNew();
		var xsdDocket4 = xsdDockets.WhsDocket.AddNew();
		xsdDocket1.Identifier.DocketType = DocketTypes.Codes.WhsOrder;
		xsdDocket1.Identifier.Reference = "1";

		xsdDocket2.Identifier.DocketType = DocketTypes.Codes.WhsASN;
		xsdDocket2.Identifier.Reference = "2";

		xsdDocket3.Identifier.DocketType = "UNK";
		xsdDocket3.Identifier.Reference = "3";

		xsdDocket4.Identifier.DocketType = ZString.Empty;
		xsdDocket4.Identifier.Reference = "4";

		var context = new ValueObjectImportContext(Factory, Notifications);
		Adapter.ImportFromValueObject(collection, xsdDockets, context);

		AssertEquals(2, collection.Count);
		AssertEquals(true, Notifications.Events.ContainsNotificationContaining("Error: ERROR MESSAGE: ('UNK' is not a valid docket type.)"));
		AssertEquals(true, Notifications.Events.ContainsNotificationContaining("Error: ERROR MESSAGE: ('' is not a valid docket type.)"));
	}

	public void TestInvalidDocketTypeImport()
	{
		var collection = new WhsDocketGenericCollection(Factory);
		var xsdDockets = new Xsd.WhsDockets();
		var xsdDocket = xsdDockets.WhsDocket.AddNew();
		xsdDocket.Identifier.DocketType = "UNK";

		var context = new ValueObjectImportContext(Factory, Notifications);
		Adapter.ImportFromValueObject(collection, xsdDockets, context);
		AssertEquals(0, collection.Count);

		var notification = Notifications.Events.GetFirst();
		AssertEquals(ErrorType.ImportingDataError, notification.Type);
		AssertEquals("Error: ERROR MESSAGE: ('UNK' is not a valid docket type.)", notification.Message);
		AssertNull(Adapter.CreateOrUpdateFromValueObject(xsdDocket, context));
	}

	#endregion

	#region Implementation

	GuiNotificationBuffer Notifications => notifications ?? (notifications = new GuiNotificationBuffer());
	GuiNotificationBuffer notifications;

	class GuiNotificationBuffer : NotificationBuffer
	{
		public IQueryUserEventArgs LastUserResponse;

		protected override void QueryUser(IQueryUserEventArgs e)
		{
			base.QueryUser(e);
			LastUserResponse = e;
		}
	}

	WhsDocketValueObjectDataUniversalAdapter Adapter => adapter ?? (adapter = new WhsDocketValueObjectDataUniversalAdapter());
	WhsDocketValueObjectDataUniversalAdapter adapter;

	protected override void SetUp()
	{
		base.SetUp();
		AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
	}

	#endregion
}

[TestedType(typeof(WhsDocketValueObjectDataUniversalAdapter))]
sealed class WhsDocketValueObjectDataUniversalAdapterForWhsReceiveTest : TestWhsDocketValueObjectDataUniversalAdapterCore
{
	#region Overrides

	protected override WhsDocket NewBusinessObject()
	{
		return Factory.New<WhsReceive>();
	}

	protected override ZString GetExpectedDocketType()
	{
		return DocketTypes.Codes.WhsASN;
	}

	protected override WhsDocket GetEmptyWhsDocket()
	{
		return Factory.New<WhsReceive>();
	}

	protected override WhsDocket GetFullyPopulatedWhsDocket()
	{
		var client = Helper.CreateClient();
		var warehouse = Helper.CreateWarehouse("TST WHS");
		return Helper.CreateWhsReceive(client, warehouse);
	}

	#endregion
}

[TestedType(typeof(WhsDocketValueObjectDataUniversalAdapter))]
sealed class WhsDocketValueObjectDataUniversalAdapterForWhsOrderTest : TestWhsDocketValueObjectDataUniversalAdapterCore
{
	#region Overrides

	protected override WhsDocket NewBusinessObject()
	{
		return Factory.New<WhsOrder>();
	}

	protected override ZString GetExpectedDocketType()
	{
		return DocketTypes.Codes.WhsOrder;
	}

	protected override WhsDocket GetEmptyWhsDocket()
	{
		return Factory.New<WhsOrder>();
	}

	protected override WhsDocket GetFullyPopulatedWhsDocket()
	{
		var client = Helper.CreateClient();
		var warehouse = Helper.CreateWarehouse("TST WHS");
		return Helper.CreateWhsOrder(client, warehouse);
	}

	#endregion
}

abstract class TestWhsDocketValueObjectDataUniversalAdapterCore : ValueObjectDataAdapterTest<WhsDocket, Xsd.WhsDocket>
{
	protected override void SetUp()
	{
		base.SetUp();
		AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}

	protected override void TearDown()
	{
		base.TearDown();
		if (resourceRetriever.IsValueCreated)
		{
			resourceRetriever.Value.Dispose();
		}
	}

	protected override void PopulateValueObjectWithLongStrings(IValueObject value, ValueObjectPropertyNavigator navigator)
	{
		base.PopulateValueObjectWithLongStrings(value, navigator);
		if (value is Xsd.WhsDocket)
		{
			var docket = (Xsd.WhsDocket)value;
			var whs = Helper.CreateWarehouse("WAREHOUSE");
			whs.WW_WarehouseCode = "WHS";
			docket.DocketDetail.WarehouseCode = whs.WW_WarehouseCode;
			docket.Identifier.DocketType = GetExpectedDocketType();
		}
	}

	protected override IValueObject PopulateValueObject(Type valueType, int fieldPopulateDepth)
	{
		var result = (Xsd.WhsDocket)base.PopulateValueObject(valueType, fieldPopulateDepth);
		result.Identifier.DocketType = GetExpectedDocketType();
		result.Identifier.Reference = "123456789012345678901234567890123456789";
		return result;
	}

	protected override bool IsExportToValueObjectSupported => false;

	protected override string ExpectedRootCollectionElementName => "WhsDockets";

	protected override string ExpectedRootElementName => "WhsDocket";

	protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
	{
		var emptyWhsDocketPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.EmptyWhsDocket.xml");
		return new BusinessObjectAndExpectedOutputFileName(GetEmptyWhsDocket(), emptyWhsDocketPath, ValidationKind.None, "Empty WhsDocket");
	}

	protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
	{
		var populatedWhsDocketPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.GUI.Testing.TestFiles.PopulatedWhsDocket.xml");
		return new BusinessObjectAndExpectedOutputFileName(GetFullyPopulatedWhsDocket(), populatedWhsDocketPath, ValidationKind.None, "Populated WhsDocket");
	}

	protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
	{
		return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
	}

	protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
	{
		return GetEmptyBizObjSample();
	}

	protected override ValueObjectDataAdapter<WhsDocket, Xsd.WhsDocket> GetNewBizObjXmlDataAdapter()
	{
		var result = new WhsDocketValueObjectDataUniversalAdapter();
		result.FileName = "DocketFile";
		return result;
	}

	protected abstract ZString GetExpectedDocketType();

	protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
	WhsTestHelperFunctions helper;

	protected abstract WhsDocket GetEmptyWhsDocket();

	protected abstract WhsDocket GetFullyPopulatedWhsDocket();

	Lazy<EmbeddedResourceRetriever> resourceRetriever;
}
