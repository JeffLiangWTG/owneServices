using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	abstract class CustomsDataSourceHelperTest<TCustomsHelper, TDocket> : WhsTestCaseWithFactory, IDisposable
		where TCustomsHelper : CustomsDataSourceHelper<TDocket>
		where TDocket : WhsDocket
	{
		#region TestDocketType

		public void TestDocketType()
		{
			AssertEquals(ExpectedDocketType, ((IDocketType)GetNewCustomsHelper(ShipmentDataObject, ShipmentDataObject.DataContext)).DocketType);
		}

		protected abstract string ExpectedDocketType { get; }

		#endregion

		#region TestConstructorDoesNotAcceptNullTopLevelDataObject

		public void TestConstructorDoesNotAcceptNullTopLevelDataObject()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => GetNewCustomsHelper(null, DataContextFactory.New()));
		}

		#endregion

		#region TestConstructorDoesNotAcceptNullTopLevelDataContext

		public void TestConstructorDoesNotAcceptNullTopLevelDataContext()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => GetNewCustomsHelper(ShipmentDataObject, null));
		}

		#endregion

		#region TestCustomsJobNo

		public void TestCustomsJobNo()
		{
			AssertEquals(null, GetNewCustomsHelper(ShipmentDataObject, ShipmentDataObject.DataContext).CustomsJobNo);

			ShipmentDataObject.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "B123");
			ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo() { RecipientRoles = new[] { new RecipientRoleDetail() { Type = GetRecipientRoleType() } } });
			AssertEquals("B123", GetNewCustomsHelper(ShipmentDataObject, ShipmentDataObject.DataContext).CustomsJobNo);
		}

		#endregion

		#region TestIsDataSourceCustoms

		public void TestIsDataSourceCustoms()
		{
			var shipmentDO = new UniversalShipment();
			shipmentDO.DataContext = DataContextFactory.New();
			AssertEquals(false, GetNewCustomsHelper(shipmentDO, shipmentDO.DataContext).IsDataSourceCustoms);
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo() { RecipientRoles = new[] { new RecipientRoleDetail() { Type = GetRecipientRoleType() } } });

			shipmentDO.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "B123");
			AssertEquals(true, GetNewCustomsHelper(shipmentDO, shipmentDO.DataContext).IsDataSourceCustoms);

			shipmentDO = new UniversalShipment();
			shipmentDO.DataContext = DataContextFactory.New();
			shipmentDO.DataContext.AddDataSource(DataContextType.WarehouseInBond, "B123");
			AssertEquals(true, GetNewCustomsHelper(shipmentDO, shipmentDO.DataContext).IsDataSourceCustoms);

			shipmentDO = new UniversalShipment();
			shipmentDO.DataContext = DataContextFactory.New();
			shipmentDO.DataContext.AddDataSource(DataContextType.WarehouseCustomsEntry, "B123");
			AssertEquals(true, GetNewCustomsHelper(shipmentDO, shipmentDO.DataContext).IsDataSourceCustoms);

			shipmentDO = new UniversalShipment();
			shipmentDO.DataContext = DataContextFactory.New();
			shipmentDO.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "B123");
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.BCO } } });
			AssertEquals(true, GetNewCustomsHelper(shipmentDO, shipmentDO.DataContext).IsDataSourceCustoms);

			shipmentDO = new UniversalShipment();
			shipmentDO.DataContext = DataContextFactory.New();
			shipmentDO.DataContext.AddDataSource(DataContextType.NctsHeader, "B123");
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.BCO } } });
			AssertEquals(true, GetNewCustomsHelper(shipmentDO, shipmentDO.DataContext).IsDataSourceCustoms);
		}

		public void TestIsDataSourceCustoms_BondedWarehouseRecipientRole()
		{
			var shipmentDO = new UniversalShipment();
			shipmentDO.DataContext = DataContextFactory.New();
			AssertEquals(false, GetNewCustomsHelper(shipmentDO, shipmentDO.DataContext).IsDataSourceCustoms);
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo() { RecipientRoles = new[] { new RecipientRoleDetail() { Type = GetRecipientRoleType() } } });

			shipmentDO.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "B123");
			AssertEquals(true, GetNewCustomsHelper(shipmentDO, shipmentDO.DataContext).IsDataSourceCustoms);

			shipmentDO = new UniversalShipment();
			shipmentDO.DataContext = DataContextFactory.New();
			shipmentDO.DataContext.AddDataSource(DataContextType.WarehouseInBond, "B123");
			AssertEquals(true, GetNewCustomsHelper(shipmentDO, shipmentDO.DataContext).IsDataSourceCustoms);
		}

		#endregion

		#region IsWarehouseBondedChangeOfOwnership

		public void TestIsWarehouseBondedChangeOfOwnership()
		{
			var shipmentDO = new UniversalShipment();
			shipmentDO.DataContext = DataContextFactory.New();
			AssertEquals(false, GetNewCustomsHelper(shipmentDO, shipmentDO.DataContext).IsWarehouseBondedChangeOfOwnership);

			shipmentDO.DataContext.AddDataTarget(DataContextType.WarehouseBondedChangeOfInventory, "B123");
			AssertEquals("Does not use ContextType.", false, GetNewCustomsHelper(shipmentDO, shipmentDO.DataContext).IsWarehouseBondedChangeOfOwnership);

			shipmentDO.DataContext = DataContextFactory.New();
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR, RecipientRoleType.BWI }.ToRecipientRoleDetails() });
			AssertEquals(false, GetNewCustomsHelper(shipmentDO, shipmentDO.DataContext).IsWarehouseBondedChangeOfOwnership);

			shipmentDO.DataContext = DataContextFactory.New();
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR, RecipientRoleType.BCO }.ToRecipientRoleDetails() });
			AssertEquals(true, GetNewCustomsHelper(shipmentDO, shipmentDO.DataContext).IsWarehouseBondedChangeOfOwnership);
		}

		#endregion

		#region IsWarehouseBondedChangeOfInventory

		public void TestIsWarehouseBondedChangeOfInventory()
		{
			var shipmentDO = new UniversalShipment();
			shipmentDO.DataContext = DataContextFactory.New();
			AssertEquals(false, GetNewCustomsHelper(shipmentDO, shipmentDO.DataContext).IsWarehouseBondedChangeOfInventory);

			shipmentDO.DataContext.AddDataTarget(DataContextType.WarehouseBondedChangeOfInventory, "B123");
			AssertEquals("Does not use ContextType.", false, GetNewCustomsHelper(shipmentDO, shipmentDO.DataContext).IsWarehouseBondedChangeOfInventory);

			shipmentDO.DataContext = DataContextFactory.New();
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR, RecipientRoleType.BWI }.ToRecipientRoleDetails() });
			AssertEquals(false, GetNewCustomsHelper(shipmentDO, shipmentDO.DataContext).IsWarehouseBondedChangeOfInventory);

			shipmentDO.DataContext = DataContextFactory.New();
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR, RecipientRoleType.BCO }.ToRecipientRoleDetails() });
			AssertEquals(true, GetNewCustomsHelper(shipmentDO, shipmentDO.DataContext).IsWarehouseBondedChangeOfInventory);

			shipmentDO.DataContext = DataContextFactory.New();
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { RecipientRoleType.BWR, RecipientRoleType.BCR }.ToRecipientRoleDetails() });
			AssertEquals(true, GetNewCustomsHelper(shipmentDO, shipmentDO.DataContext).IsWarehouseBondedChangeOfInventory);
		}

		#endregion

		#region TestCustomsParentReferenceToMatchForDocket

		public void TestCustomsParentReferenceToMatchForDocket()
		{
			AssertEquals("-", GetNewCustomsHelper(ShipmentDataObject, ShipmentDataObject.DataContext).CustomsParentReferenceToMatchForDocket);

			ShipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo() { RecipientRoles = new[] { new RecipientRoleDetail() { Type = GetRecipientRoleType() } } });
			AssertEquals("-EDIDATEDI", GetNewCustomsHelper(ShipmentDataObject, ShipmentDataObject.DataContext).CustomsParentReferenceToMatchForDocket);

			ShipmentDataObject.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "B123");
			AssertEquals("B123-EDIDATEDI", GetNewCustomsHelper(ShipmentDataObject, ShipmentDataObject.DataContext).CustomsParentReferenceToMatchForDocket);
		}

		#endregion

		#region TestCancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed_SuccessfulAmendment

		public void TestCancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed_SuccessfulAmendment()
		{
			var helper = GetNewCustomsHelper(ShipmentDataObject, ShipmentDataObject.DataContext);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			data.Whs1.WW_IsBondedWarehouse = true;
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.EnableWarehouseForFreeStore(data.Whs1, false);
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON").PK;

			var docket = GetFinalisedDocket(data);
			docket.WD_DocketSubType = "CUS";

			var amendedInventory = helper.CancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed(docket);
			AssertEquals("Should cancel amended Docket, because in a virtual whs it has been replaced with a new docket.", true, docket.IsCancelled);
			AssertEquals(1, docket.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).Count());
			AssertCancelledDocket(docket, amendedInventory);
			AssertCorrectInventoryBalanceAfterCancel(data, amendedInventory.Sum(i => i.WI_TotalUnits));
		}

		void AssertCorrectInventoryBalanceAfterCancel(TestDataSimpleEnvironment data, ZDecimal expectedTotalUnits)
		{
			var query = new ZQuery();
			query.AddToFilter(WhsInventoryViewSchema.WI_OP, data.Part1.PK);
			query.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, data.Org1.PK);
			query.AddToFilter(WhsInventoryViewSchema.WI_WL, data.Whs1.DefaultLocation.PK);

			var inventory = Factory.Load<WhsInventoryView>(query).Single();
			AssertEquals(expectedTotalUnits, inventory.WI_TotalUnits);
		}

		protected abstract void AssertCancelledDocket(TDocket docket, IEnumerable<WhsInventoryView> amendedInventory);

		#endregion

		#region TestCancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed_FailedAmendment

		public void TestCancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed_FailedAmendment()
		{
			ShipmentDataObject.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "B123");
			ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo() { RecipientRoles = new[] { new RecipientRoleDetail() { Type = GetRecipientRoleType() } } });

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			data.Whs1.WW_IsBondedWarehouse = true;
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON").PK;
			var docket = GetFinalisedDocket(data);
			docket.WD_DocketSubType = "CUS";
			Factory.Save();

			TestCancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed_FailedAmendmentCore(docket);
		}

		protected abstract void TestCancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed_FailedAmendmentCore(TDocket docket);

		#endregion

		#region Implementation

		protected abstract TDocket GetFinalisedDocket(TestDataSimpleEnvironment data);

		protected abstract TCustomsHelper GetNewCustomsHelper(UniversalShipment topLevelDataObject, IDataContextDataObject topLevelDataContext);

		protected abstract RecipientRoleType GetRecipientRoleType();

		protected UniversalShipment ShipmentDataObject
		{
			get
			{
				if (shipmentDataObject == null)
				{
					shipmentDataObject = new UniversalShipment();
					shipmentDataObject.DataContext = DataContextFactory.New();
				}

				return shipmentDataObject;
			}
		}

		UniversalShipment shipmentDataObject;

		#endregion

		#region IDisposable Support

		bool disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					this.shipmentDataObject?.Dispose();
				}

				disposed = true;
			}
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
