using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using BusinessContext = Enterprise.Integration.Accounting.BusinessContext;
using Constants = Enterprise.Core.Constants;
using OrgCodes = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeCodes;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocketTestCase<TDocket> : WhsBusinessObjectTestCase
		where TDocket : WhsDocket
	{
		#region INumberFountain

		public void TestINumberFountainConsumer()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Whs1");
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;
			docket.WD_WW_Whs = whs.PK;

			if (docket is INumberFountainConsumer)
			{
				docket.WD_DocketID = "W001";
				AssertEquals("ID refers to correct Field.", "W001", ((INumberFountainConsumer)docket).ID);

				((INumberFountainConsumer)docket).ID = "W002";
				AssertEquals("ID refers to correct Field.", "W002", docket.WD_DocketID);
			}
			else
			{
				Assert("Should implement ICustomizableNumberFountainConsumer interface.", docket is ICustomizableNumberFountainConsumer);

				docket.WD_DocketID = "W001";
				AssertEquals("ID refers to correct Field.", "W001", ((ICustomizableNumberFountainConsumer)docket).ID);

				((ICustomizableNumberFountainConsumer)docket).ID = "W002";
				AssertEquals("ID refers to correct Field.", "W002", docket.WD_DocketID);
			}
		}

		public void TestINumberFountainConsumer_SavingSetsJobID()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;
			docket.WD_WW_Whs = whs.PK;

			if (docket is INumberFountainConsumer)
			{
				AssertEquals("Precondition: Docket ID is empty.", "", docket.WD_DocketID);

				Factory.Save();
				AssertNotEquals("Docket ID should not be empty.", "", docket.WD_DocketID);
				AssertEquals("Docket ID was set correctly.", "W00000001", docket.WD_DocketID);
			}
			else
			{
				Assert("Should implement ICustomizableNumberFountainConsumer interface.", docket is ICustomizableNumberFountainConsumer);
			}
		}

		public void TestINumberFountainEntityWithID()
		{
			var docket = GetNewBusinessObject();

			var isDocketINumberFountainEntityWithID = docket is INumberFountainEntityWithID;
			Assert("Should implement INumberFountainEntityWithID interface.", isDocketINumberFountainEntityWithID);
		}

		#endregion

		#region Constructors

		#region TestConstructor

		public void TestConstructor()
		{
			AssertNotNull("NotificationManager has not been instantiated", Docket.NotificationManager);
		}

		#endregion

		#region TestConstructorSetsConcurrencyPolicy

		public void TestConstructorSetsConcurrencyPolicy()
		{
			AssertEquals(ConcurrencyPolicy.Strict, Docket.WD_WPInfo.ConcurrencyPolicy);
			AssertEquals(ConcurrencyPolicy.Ignore, Docket.WD_CriticalChangesVersionIDInfo.ConcurrencyPolicy);
		}

		#endregion

		#endregion

		#region Customs Stuff

		#region TestIsCustomsTransaction

		public void TestIsCustomsTransaction()
		{
			TestIsCustomsTransactionCore();
		}

		protected abstract void TestIsCustomsTransactionCore();

		#endregion

		#region TestIsBondedEntryKeyVisibleForCustomsTransactions

		public void TestIsBondedEntryKeyVisibleForCustomsTransactions()
		{
			var docket = GetNewBusinessObject();
			AssertEquals("BondedEntryKey visibility.", ExpectedIsBondedEntryKeyVisibleForCustomsTransactions, docket.IsBondedEntryKeyVisibleForCustomsTransactions);
		}

		protected virtual bool ExpectedIsBondedEntryKeyVisibleForCustomsTransactions => false;

		#endregion

		#region TestDefaultDocketSubTypeForCustomsTransaction

		public virtual void TestDefaultDocketSubTypeForCustomsTransaction()
		{
			AssertEquals("", GetNewBusinessObject().DefaultDocketSubTypeForCustomsTransaction);
		}

		#endregion

		#region TestIsWarehouseFreeStoreEnabled

		public void TestIsWarehouseFreeStoreEnabled()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("WHS1");
			Docket.WD_WW_Whs = whs.PK;
			Helper.EnableWarehouseForFreeStore(whs, true);
			Assert("Should be Freestore Whs", Docket.IsWarehouseFreeStoreEnabled);

			Docket.WD_WW_Whs = ZGuid.Empty;
			Assert("Should not be Freestore Whs", !Docket.IsWarehouseFreeStoreEnabled);
		}

		#endregion

		#region TestIsWarehouseBondEnabled

		public void TestIsWarehouseBondEnabled()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("WHS1");
			Docket.WD_WW_Whs = whs.PK;
			Helper.EnableWarehouseForBond(whs, true);
			Assert("Should be Bond Whs", Docket.IsWarehouseBondEnabled);

			Docket.WD_WW_Whs = ZGuid.Empty;
			Assert("Should not be Bond Whs", !Docket.IsWarehouseBondEnabled);
		}

		#endregion

		#region TestIsWarehouseExciseEnabled

		public void TestIsWarehouseExciseEnabled()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("WHS1");
			Docket.WD_WW_Whs = whs.PK;
			Helper.EnableWarehouseForExcise(whs, true);
			Assert("Should be Excise Whs", Docket.IsWarehouseExciseEnabled);

			Docket.WD_WW_Whs = ZGuid.Empty;
			Assert("Should not be Excise Whs", !Docket.IsWarehouseExciseEnabled);
		}

		#endregion

		#region TestSetCustomsExternalReference

		public void TestSetCustomsExternalReference()
		{
			OrgHeader org = Helper.CreateClient();
			WhsWarehouse whs = Helper.CreateWarehouse("1");

			WhsReceive receive1 = Helper.CreateWhsReceive(org, whs);
			receive1.WD_DocketType = CodeLists.DocketType.Codes.Receive;
			receive1.SetExternalReferenceFromEntryKey("TRAN1");
			AssertEquals("First Receive", "TRAN1-INW-01", receive1.WD_ExternalReference);

			WhsReceive receive2 = Helper.CreateWhsReceive(org, whs);
			receive2.WD_DocketType = CodeLists.DocketType.Codes.Receive;
			receive2.SetExternalReferenceFromEntryKey("TRAN1");
			AssertEquals("Second Increments", "TRAN1-INW-02", receive2.WD_ExternalReference);

			receive1.WD_ExternalReference = "TRAN1-INW-A";

			WhsReceive receive3 = Helper.CreateWhsReceive(org, whs);
			receive3.WD_DocketType = CodeLists.DocketType.Codes.Receive;
			receive3.SetExternalReferenceFromEntryKey("TRAN1");
			AssertEquals("Jump To 3", "TRAN1-INW-03", receive3.WD_ExternalReference);

			WhsAdjustment adjust1 = Helper.CreateWhsAdjustment(org, whs);
			adjust1.WD_DocketType = CodeLists.DocketType.Codes.Adjustment;
			adjust1.SetExternalReferenceFromEntryKey("TRAN1");
			AssertEquals("Second Receive", "TRAN1-AMD-01", adjust1.WD_ExternalReference);

			WhsAdjustment adjust2 = Helper.CreateWhsAdjustment(org, whs);
			adjust1.WD_DocketType = CodeLists.DocketType.Codes.Adjustment;
			adjust1.SetExternalReferenceFromEntryKey("TRAN1");
			AssertEquals("Second Receive", "TRAN1-AMD-02", adjust1.WD_ExternalReference);
		}

		#endregion

		//receive.WD_EXterna

		#region class InvoiceLinkTest

		public class InvoiceLinkTest : IInvoiceLink
		{
			public InvoiceLinkTest()
			{
				IsExWarehouseChanged = new IsExWarehouseChangedEvent(HandleEnabledChanged);
			}

			#region IInvoiceLink Members

			void HandleEnabledChanged() { }
			public void CreateOrUpdateInvoices(IWhsBondedWarehouseTransaction transaction) { }
			public ZString OwnersReference { get { return new ZString(); } }
			public ZBool IsEnabled { get { return new ZBool(); } }
			public ZGuid DeclarationPK { get { return new ZGuid(); } }
			public IOrgHeader Importer { get { return null; } }
			public event IsExWarehouseChangedEvent IsExWarehouseChanged;
			internal void NotifyFinalisingInvoices() { }
			internal void NotifyChangedToFromExWarehousing() { }
			public ZBool IsExWarehouse { get { return true; } }
			public void NotifyOfOrderCancellation() { }
			public void NotifyOfOrderCreation() { }
			public ZString EntryKeyTitle { get { return ZString.Empty; } }

			#endregion
		}

		#endregion

		#endregion

		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.WhsDocket);
			}
		}

		#endregion

		#region TestUniversalDataContext

		public void TestUniversalDataContext()
		{
			var attribute = typeof(TDocket).GetAttribute<UniversalDataContextAttribute>();

			if (ExpectedDataContextType == null)
			{
				AssertNull(attribute);
			}
			else
			{
				AssertNotNull(attribute);
				AssertEquals(ExpectedDataContextType, attribute.DataContextType);
			}
		}

		protected virtual DataContextType? ExpectedDataContextType => null;

		#endregion

		#region Business Object Overrides

		#region TestLightValidatonDisabled

		public virtual void TestLightValidatonDisabled()
		{
			AssertEquals(false, Docket.LightValidationEnabled);
		}

		#endregion

		#region TestClone

		public void TestClone() => TestClone(performRowCopyWithoutTriggeringValidationAndSetter: false);

		public void TestClone_RowCopy() => TestClone(performRowCopyWithoutTriggeringValidationAndSetter: true);

		void TestClone(bool performRowCopyWithoutTriggeringValidationAndSetter)
		{
			var warehouse = Helper.CreateWarehouse("W", "A", 2, 1);
			var client = Helper.CreateClient("C");
			Factory.Save();

			var parentDocket = GetNewBusinessObject();
			parentDocket.WD_OH_Client = client.PK;
			parentDocket.WD_WW_Whs = warehouse.PK;

			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = client.PK;
			docket.WD_WW_Whs = warehouse.PK;
			AssertEquals(true, docket.SupportsClone());

			docket.WD_DocketID = "W001";
			docket.WD_ExternalReference = "1";
			docket.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			docket.WD_WD_ParentDocket = parentDocket.PK;
			docket.WD_DocketStatus = DocketStatus.Codes.Putaway;
			docket.WD_BookingDate = ZDateTimeOffset.UtcNow.AddDays(-7);

			if (docket is IJobWithTransportCompany job)
			{
				job.TransportCoPK = Factory.New<OrgHeader>().PK;
			}
			AdditionalSetupForTestClone(docket);

			docket.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			docket.WD_CanceledTimeUtc = ZDateTime.UtcNow;
			docket.WD_GS_NKCanceledBy = "ABC";
			docket.WD_HoldPalletIDPutaway = true;
			docket.WD_FinalisedDate = ZDateTimeOffset.UtcNow;
			docket.WD_GS_NKFinalizedBy = "ABC";
			docket.WD_IsPickFaceReplenishment = true;
			docket.WD_IsInwardsProcessingJob = true;
			docket.WD_BookedWithCBADateTimeUtc = ZDateTime.UtcNow;
			docket.WD_CustomsParentReference = "CustomsRef";
			docket.WD_UnloadCompletedTime = ZDateTimeOffset.UtcNow;

			docket.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			docket.WD_WP_ParentPickForTransfer = ZGuid.BrettsGuid;
			docket.WD_IsPutawayTransfer = true;
			docket.WD_WD_Split = ZGuid.BrettsGuid;
			docket.WD_ExWhsJobGuid = ZGuid.BrettsGuid;
			docket.WD_ExternalReferenceSplit = new ZByte(1);

			docket.References.AddNew().WX_Reference = "1";
			docket.References.AddNew().WX_Reference = "2";
			docket.WD_BOLNo = "HOUSE";
			docket.Containers.AddNew().WC_ContainerNum = "1";
			docket.Containers.AddNew().WC_ContainerNum = "2";
			docket.Pallets.AddNew().W2_Quantity = 1;
			docket.Pallets.AddNew().W2_Quantity = 2;

			docket.WD_UnitsSent = 1m;
			docket.WD_PackagesSent = 2;
			docket.WD_PalletsSent = 3;
			docket.WD_WeightSent = 4m;
			docket.WD_WeightSentUserEntered = 5m;
			docket.WD_CubicSent = 6m;

			docket.WD_CriticalChangesVersionID = ZGuid.BrettsGuid;
			docket.WD_WP_PickBeingReplenished = ZGuid.BrettsGuid;
			docket.WD_TZ_TransportZone = ZGuid.BrettsGuid;
			docket.WD_WP_ParentPickForReceive = ZGuid.BrettsGuid;

			var cloneArgs = new BusinessObjectCloneArgs(Enumerable.Empty<string>(), performRowCopyWithoutTriggeringValidationAndSetter: performRowCopyWithoutTriggeringValidationAndSetter);
			var clone = (TDocket)docket.Clone(cloneArgs);
			AssertEquals(ZString.Empty, clone.WD_DocketID);
			AssertEquals("1", clone.WD_ExternalReference);
			AssertEquals("WD_DocketStatus is excluded for clone.", DocketStatus.Codes.New, clone.WD_DocketStatus);
			AssertEquals("WD_WD_ParentDocket", ZGuid.Empty, clone.WD_WD_ParentDocket);
			AssertEquals(2, clone.References.Count);
			AssertEquals(2, clone.Containers.Count);
			AssertEquals(2, clone.Pallets.Count);

			AssertEquals("WD_UnitsSent should have been excluded.", 0m, clone.WD_UnitsSent);
			AssertEquals("WD_PackagesSent should have been excluded.", 0, clone.WD_PackagesSent);
			AssertEquals("WD_PalletsSent should have been excluded.", ZShort.Zero, clone.WD_PalletsSent);
			AssertEquals("WD_WeightSent should have been excluded.", 0m, clone.WD_WeightSent);
			AssertEquals("WD_WeightSentUserEntered should have been excluded.", 0m, clone.WD_WeightSentUserEntered);
			AssertEquals("WD_CubicSent should have been excluded.", 0m, clone.WD_CubicSent);
			AssertEquals("WD_UnloadCompletedTime should have been excluded.", ZDateTimeOffset.Empty, clone.WD_UnloadCompletedTime);

			// don't copy across waybill number in case the copied order will be creating a Forwarding shipment
			AssertEquals("Should not copy waybill number because it is (kind of) unique per Shipment.", "", clone.WD_BOLNo);
			AssertEquals("1", clone.References[0].WX_Reference);
			AssertEquals("2", clone.References[1].WX_Reference);
			AssertEquals("1", clone.Containers[0].WC_ContainerNum);
			AssertEquals("2", clone.Containers[1].WC_ContainerNum);
			AssertEquals(1, clone.Pallets[0].W2_Quantity);
			AssertEquals(2, clone.Pallets[1].W2_Quantity);

			AssertEquals("WD_BookingDate for original", false, docket.WD_BookingDate.IsEmpty);
			AssertEquals("WD_BookingDate should be set after saving.", false, clone.WD_BookingDate.IsEmpty);
			AssertNotEquals("WD_BookingDate should be different.", clone.WD_BookingDate, docket.WD_BookingDate);

			AssertEquals("WD_StartedReceivingTimeUtc for original", false, docket.WD_StartedReceivingTimeUtc.IsEmpty);
			AssertEquals("WD_StartedReceivingTimeUtc for clone", true, clone.WD_StartedReceivingTimeUtc.IsEmpty);

			AssertEquals("WD_CanceledTimeUtc for original", false, docket.WD_CanceledTimeUtc.IsEmpty);
			AssertEquals("WD_CanceledTimeUtc for clone", true, clone.WD_CanceledTimeUtc.IsEmpty);

			AssertEquals("WD_GS_NKCanceledBy for original", "ABC", docket.WD_GS_NKCanceledBy);
			AssertEquals("WD_GS_NKCanceledBy for clone", string.Empty, clone.WD_GS_NKCanceledBy);

			AssertEquals("WD_HoldPalletIDPutaway for original", true, docket.WD_HoldPalletIDPutaway);
			AssertEquals("WD_HoldPalletIDPutaway for clone", false, clone.WD_HoldPalletIDPutaway);

			AssertEquals("WD_FinalisedDate for original", false, docket.WD_FinalisedDate.IsEmpty);
			AssertEquals("WD_FinalisedDate for clone", true, clone.WD_FinalisedDate.IsEmpty);

			AssertEquals("WD_GS_NKFinalizedBy for original", "ABC", docket.WD_GS_NKFinalizedBy);
			AssertEquals("WD_GS_NKFinalizedBy for clone", string.Empty, clone.WD_GS_NKFinalizedBy);

			AssertEquals("WD_WP_ParentPickForReceive for original", ZGuid.BrettsGuid, docket.WD_WP_ParentPickForReceive);
			AssertEquals("WD_WP_ParentPickForReceive for clone", ZGuid.Empty, clone.WD_WP_ParentPickForReceive);

			AssertEquals("WD_IsPickFaceReplenishment for original", true, docket.WD_IsPickFaceReplenishment);
			AssertEquals("WD_IsPickFaceReplenishment for clone", false, clone.WD_IsPickFaceReplenishment);

			AssertEquals("WD_CriticalChangesVersionID for original", ZGuid.BrettsGuid, docket.WD_CriticalChangesVersionID);
			AssertEquals("WD_CriticalChangesVersionID for clone", ZGuid.Empty, clone.WD_CriticalChangesVersionID);

			AssertEquals("WD_IsInwardsProcessingJob for original", true, docket.WD_IsInwardsProcessingJob);
			AssertEquals("WD_IsInwardsProcessingJob for clone", clone.WD_DocketType == DocketType.Codes.DynamicWorkOrder, clone.WD_IsInwardsProcessingJob);

			AssertEquals("WD_BookedWithCBADateTimeUtc for original", false, docket.WD_BookedWithCBADateTimeUtc.IsEmpty);
			AssertEquals("WD_BookedWithCBADateTimeUtc for clone", true, clone.WD_BookedWithCBADateTimeUtc.IsEmpty);

			AssertEquals("WD_CustomsParentReference for original", "CustomsRef", docket.WD_CustomsParentReference);
			AssertEquals("WD_CustomsParentReference for clone", string.Empty, clone.WD_CustomsParentReference);

			AssertEquals("WD_WP_PickBeingReplenished for original", ZGuid.BrettsGuid, docket.WD_WP_PickBeingReplenished);
			AssertEquals("WD_WP_PickBeingReplenished for clone", ZGuid.Empty, clone.WD_WP_PickBeingReplenished);

			AssertEquals("WD_TZ_TransportZone for original", ZGuid.BrettsGuid, docket.WD_TZ_TransportZone);
			AssertEquals("WD_TZ_TransportZone for clone", ZGuid.Empty, clone.WD_TZ_TransportZone);

			AssertEquals("WD_ScreeningStatus for original", ScreeningStatusesList.Codes.Matched, docket.WD_ScreeningStatus);
			AssertEquals("WD_ScreeningStatus for clone", ScreeningStatusesList.Codes.NotScreened, clone.WD_ScreeningStatus);

			AssertEquals("WD_WP_ParentPickForTransfer for original", ZGuid.BrettsGuid, docket.WD_WP_ParentPickForTransfer);
			AssertEquals("WD_WP_ParentPickForTransfer for clone", ZGuid.Empty, clone.WD_WP_ParentPickForTransfer);

			AssertEquals("WD_IsPutawayTransfer for original", true, docket.WD_IsPutawayTransfer);
			AssertEquals("WD_IsPutawayTransfer for clone", false, clone.WD_IsPutawayTransfer);

			AssertEquals("WD_WD_Split for original", ZGuid.BrettsGuid, docket.WD_WD_Split);
			AssertEquals("WD_WD_Split for clone", ZGuid.Empty, clone.WD_WD_Split);

			AssertEquals("WD_TaskPlanningStatus for original", TaskPlanningStatus.Codes.Ready, docket.WD_TaskPlanningStatus);
			AssertEquals("WD_TaskPlanningStatus for clone", string.Empty, clone.WD_TaskPlanningStatus);

			AssertEquals("WD_ExWhsJobGuid for original", ZGuid.BrettsGuid, docket.WD_ExWhsJobGuid);
			AssertEquals("WD_ExWhsJobGuid for clone", ZGuid.Empty, clone.WD_ExWhsJobGuid);

			AssertEquals("WD_ExternalReferenceSplit for original", new ZByte(1), docket.WD_ExternalReferenceSplit);
			AssertEquals("WD_ExternalReferenceSplit for clone", ZByte.Zero, clone.WD_ExternalReferenceSplit);

			TestCloneCore(docket, clone);
		}

		protected virtual void AdditionalSetupForTestClone(TDocket docket)
		{
		}

		protected virtual void TestCloneCore(TDocket originalDocket, TDocket clonedDocket)
		{
			if (originalDocket is IJobWithTransportCompany job)
			{
				AssertEquals("TransportCoPK", job.TransportCoPK, ((IJobWithTransportCompany)clonedDocket).TransportCoPK);
			}
		}

		#endregion

		#region TestSetDefaultValues

		[TestDate(2022, 02, 22)]
		public void TestSetDefaultValues()
		{
			Env.Registry.PackageVolumeUnit = Core.Constants.Volume.CubicDecimetres;
			Env.Registry.PackageWeightUnit = Core.Constants.Weight.Ounces;

			var docket = GetNewBusinessObject();
			AssertEquals("WD_DocketStatus must be 'NEW'", DocketStatus.Codes.New, docket.WD_DocketStatus);
			AssertEquals("Volume Unit:", Core.Constants.Volume.CubicDecimetres, docket.WD_TotalCubicUnit);
			AssertEquals("Weight Unit:", Core.Constants.Weight.Ounces, docket.WD_TotalWeightUnit);

			TestSetDefaultValuesCore(docket);
		}

		protected virtual void TestSetDefaultValuesCore(TDocket docket)
		{
		}

		#endregion

		#region TestDocketIDSequence

		public virtual void TestDocketIDSequence()
		{
			OrgHeader org = Helper.CreateClient();
			WhsWarehouse whs = Helper.CreateWarehouse("1");
			Docket.WD_OH_Client = org.PK;
			Docket.WD_WW_Whs = whs.PK;

			Docket.WD_ExternalReference = "1";
			Factory.Save();
			AssertEquals("WD_DocketID should be W00000001", "W00000001", Docket.WD_DocketID);

			Docket = GetNewBusinessObject();
			Docket.WD_OH_Client = org.PK;
			Docket.WD_WW_Whs = whs.PK;
			Docket.WD_ExternalReference = "2";
			Factory.Save();
			AssertEquals("WD_DocketID should be W00000002", "W00000002", Docket.WD_DocketID);

			Docket = GetNewBusinessObject();
			Docket.WD_OH_Client = org.PK;
			Docket.WD_WW_Whs = whs.PK;
			Docket.WD_ExternalReference = "3";
			Factory.Save();
			AssertEquals("WD_DocketID should be W00000003", "W00000003", Docket.WD_DocketID);
		}

		#endregion

		#region TestOnFactorySaving

		#region TestOnFactorySaving

		public virtual void TestOnFactorySaving()
		{
			OrgHeader org = Helper.CreateClient();
			WhsWarehouse whs = Helper.CreateWarehouse("1");
			Docket.WD_OH_Client = org.PK;
			Docket.WD_WW_Whs = whs.PK;
			Docket.WD_ExternalReference = "";

			AssertEquals("Precondition", false, Docket.IsInDatabase);
			AssertEquals("Precondition", "", Docket.WD_DocketID);
			AssertEquals("Precondition", "", Docket.WD_ExternalReference);
			AssertEquals("Precondition", DocketStatus.Codes.New, Docket.WD_DocketStatus);

			Factory.Save();

			AssertEquals("Docket not saved", true, Docket.IsInDatabase);
			AssertEquals("WD_DocketID is not set", "W00000001", Docket.WD_DocketID);
			AssertEquals("WD_DocketStatus must be set to 'ENT'", DocketStatus.Codes.Entered, Docket.WD_DocketStatus);
			AssertEquals("WD_ExternalReference was empty thus should be defaulted from DocketID.", "W00000001", Docket.WD_ExternalReference);
		}

		#endregion

		#region TestOnFactorySaving_PopulateWD_DocketIDIfRequired

		public void TestOnFactorySaving_PopulateWD_DocketIDIfRequired()
		{
			OrgHeader org = Helper.CreateClient();
			WhsWarehouse whs = Helper.CreateWarehouse("1");
			Factory.Save();

			var poke = Docket; // just touch Docket property (lame).
			bool factorySaveFailed = false;

			try
			{
				Factory.Save();
			}
			catch
			{
				factorySaveFailed = true;
			}

			Assert(factorySaveFailed);
			AssertEquals(false, Docket.IsInDatabase);
			AssertEquals(ZString.Empty, Docket.WD_DocketID);

			Docket.WD_OH_Client = org.PK;
			Docket.WD_WW_Whs = whs.PK;
			Factory.Save();

			AssertEquals(true, Docket.IsInDatabase);
			AssertEquals(false, Docket.WD_DocketID.IsEmpty);
			ZString docketID = Docket.WD_DocketID;
			Docket.WD_OH_Client = ZGuid.Empty;
			factorySaveFailed = false;

			try
			{
				Factory.Save();
			}
			catch
			{
				factorySaveFailed = true;
			}
			Assert(factorySaveFailed);
			AssertEquals(true, Docket.IsInDatabase);
			AssertEquals(docketID, Docket.WD_DocketID);
		}

		#endregion

		#region TestOnFactorySaving_GenerateNextDocketIdIfAlreadyUsed

		public void TestOnFactorySaving_GenerateNextDocketIdIfAlreadyUsed()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("1");
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;
			docket.WD_WW_Whs = whs.PK;
			Factory.Save();

			AssertEquals("Precondition", true, docket.IsInDatabase);
			AssertEquals("Precondition", false, docket.WD_DocketID.IsEmpty);
			AssertEquals("After save it should populate with first number ()", "W00000001", docket.WD_DocketID);

			docket.WD_DocketID = "W00000002"; // it make confilict with next fountain value
			Factory.Save();
			AssertEquals("It should not populate again and change the manual setting", "W00000002", docket.WD_DocketID);

			var newDocket = GetNewBusinessObject();
			newDocket.WD_OH_Client = org.PK;
			newDocket.WD_WW_Whs = whs.PK;

			bool saveFailed = false;
			try
			{
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				saveFailed = true;
				AssertEquals("Docket ID should not be populated if it failed to save, it will populate in the next save with the next fountain value.", "", newDocket.WD_DocketID);
				ZExceptionReporting.HandleSaveException(ex); // simulate the form handling the exception
				ErrorReporter.Clear();
			}

			newDocket.WorkflowItems.DeleteAll();

			AssertStartsWith("User should be notified about the unique index conflict.", @"While you were working, the automatically assigned record number was used by another user.
Saving again should automatically resolve this issue.
Number Fountain: WarehouseDocketID
Index: NR_UX__WD_DocketID", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Save should have failed", saveFailed);
			UnitTestUserNotification.Instance.ClearMessages();

			Factory.Save();
			AssertEquals("Next Docket ID should be given out.", "W00000003", newDocket.WD_DocketID);
			AssertEquals("There should be no error Message.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		#endregion

		#region TestOnFactorySaving_CreatesUniqueReferenceIfRequired

		public void TestOnFactorySaving_CreatesUniqueReferenceIfRequired()
		{
			OrgHeader client = Helper.CreateClient();
			WhsWarehouse whs = Helper.CreateWarehouse("1");

			// when no need to create Unique External Reference and Docket is not in DB.
			Docket.WD_OH_Client = client.PK;
			Docket.WD_WW_Whs = whs.PK;
			Docket.WD_ExternalReference = "R1";
			Docket.IsUniqueExternalReferenceCreatedOnSave = false;
			Factory.Save();
			AssertEquals("Docket save failed.", true, Docket.IsInDatabase);
			AssertEquals("R1", Docket.WD_ExternalReference);

			// when no need to create Unique External Reference and Docket already in DB.
			Docket.WD_ExternalReference = "HELLO";
			Factory.Save();
			AssertEquals("Reference incorrect", "HELLO", Docket.WD_ExternalReference);

			// when needed to create Unique External Reference and Docket already in DB.
			Docket.IsUniqueExternalReferenceCreatedOnSave = true;
			Docket.WD_ExternalReference = "R1";
			Factory.Save();
			AssertEquals("Reference incorrect", "R1", Docket.WD_ExternalReference);

			// when needed to create Unique External Reference ad Docket is not in DB.
			TestOnFactorySaving_CreatesUniqueReferenceIfRequiredCore(client, whs);
		}

		protected virtual void TestOnFactorySaving_CreatesUniqueReferenceIfRequiredCore(OrgHeader client, WhsWarehouse whs)
		{
			// for Empty External Reference
			var docket2 = CreateWhsDocket(client, whs, "", true);
			Factory.Save();
			AssertEquals("docket2 not saved", true, docket2.IsInDatabase);
			AssertEquals("Reference incorrect", "W00000002", docket2.WD_ExternalReference);

			// for not empty but already used External Reference
			var docket3 = CreateWhsDocket(client, whs, "R1", true);
			Factory.Save();
			AssertEquals("docket2 not saved", true, docket3.IsInDatabase);
			AssertEquals("Reference incorrect", "R1 W00000003", docket3.WD_ExternalReference);

			// for not empty and not used External Reference
			var docket4 = CreateWhsDocket(client, whs, "R2", true);
			Factory.Save();
			AssertEquals("docket2 not saved", true, docket4.IsInDatabase);
			AssertEquals("Reference incorrect", "R2 W00000004", docket4.WD_ExternalReference);
		}

		protected TDocket CreateWhsDocket(OrgHeader client, WhsWarehouse warehouse, ZString externalReference, bool isRefCreatedOnSave)
		{
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = client.PK;
			docket.WD_WW_Whs = warehouse.PK;
			docket.WD_ExternalReference = externalReference;
			docket.IsUniqueExternalReferenceCreatedOnSave = isRefCreatedOnSave;
			return docket;
		}

		#endregion

		#region TestOnFactorySaving_CreateOperationalStatusChangeEvents

		[TestDate(2013, 9, 26, 5, 10, 0)]
		public void TestOnFactorySaving_CreateOperationalStatusChangeEvents()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;
			docket.WD_WW_Whs = whs.PK;

			AssertEquals("Docket status should be set to new", docket.WD_DocketStatus, DocketStatus.Codes.New);

			Factory.Save();
			AssertEquals("Docket status should be set to entered", docket.WD_DocketStatus, DocketStatus.Codes.Entered);
			AssertEquals("Warehouse Job Entered event should be created", 1, docket.Logs.Find(Helper.GetLogFilter(Events.WarehouseJobEntered.Code)).Length);

			docket.WD_PalletsSent = 1;
			Factory.Save();
			AssertEquals("Only one Warehouse Job Entered event should be created", 1, docket.Logs.Find(Helper.GetLogFilter(Events.WarehouseJobEntered.Code)).Length);

			TestCreateOperationalStatusChangeEvents(docket);
		}

		public void TestOnFactorySaving_CreateOperationalStatusChangeEvents_OnlyIfStatusChanged()
		{
			var whs = Helper.CreateWarehouse("1");
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = Helper.CreateClient().PK;
			docket.WD_WW_Whs = whs.PK;

			CreateNewPick(docket, whs);
			SetFinsalisedDocketValuesForTest(docket);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			newFactory.Load(docket.GetType(), docket.PK);
			newFactory.Save();

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};
			AssertDbHits(expectedDBHits, newFactory); // should not hit logs on receives without changes
		}

		protected virtual WhsPick CreateNewPick(TDocket docket, WhsWarehouse warehouse) => null;

		protected virtual void SetFinsalisedDocketValuesForTest(TDocket docket)
		{
			docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
			docket.WD_FinalisedDate = ZDateTimeOffset.Now;
		}

		protected virtual void TestCreateOperationalStatusChangeEvents(TDocket docket)
		{
		}

		public void TestFinalisedEvent_AddedOnFinalisingDocket()
		{
			var docket = SetupForTestFinaliseDocket();
			Factory.Save();

			var finalisedLogsBeforeFinalisationInvoked = docket.Logs.Find(Helper.GetLogFilter(Events.ItemDocumentJobFinalised.Code));
			AssertEquals("Finalised Log should not exist.", 0, finalisedLogsBeforeFinalisationInvoked.Length);

			docket.FinaliseDocket();

			var finalisedLogsAfterFinalisationInvoked = docket.Logs.Find(Helper.GetLogFilter(Events.ItemDocumentJobFinalised.Code));
			AssertEquals("Finalised Log should be added.", 1, finalisedLogsAfterFinalisationInvoked.Length);

			Factory.Save();

			AssertIsFinalisedPrecondition(docket);
		}

		public void TestFinalisedEvent_NotAddedIfFinalisingDocketFails()
		{
			var whs = Helper.CreateWarehouse("1");
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = Helper.CreateClient().PK;
			docket.WD_WW_Whs = whs.PK;

			var finalisedLogsBeforeFinalisationInvoked = docket.Logs.Find(Helper.GetLogFilter(Events.ItemDocumentJobFinalised.Code));
			AssertEquals("Finalised Log should not exist.", 0, finalisedLogsBeforeFinalisationInvoked.Length);

			docket.FinaliseDocket();

			var finalisedLogsAfterFinalisationInvoked = docket.Logs.Find(Helper.GetLogFilter(Events.ItemDocumentJobFinalised.Code));
			AssertEquals("Docket should not be finalised.", false, docket.IsFinalised);
			AssertEquals("Finalised Log should not be added.", 0, finalisedLogsAfterFinalisationInvoked.Length);
		}

		#endregion

		#endregion

		#region TestNumberFountainsAreNotAccessedMoreThanOnceOnSaving

		public virtual void TestNumberFountainsAreNotAccessedMoreThanOnceOnSaving()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			OrgHeader org = Helper.CreateClient();
			WhsWarehouse whs = Helper.CreateWarehouse("1");
			Docket.WD_OH_Client = org.PK;
			Docket.WD_WW_Whs = whs.PK;

			Docket.WD_ExternalReference = "1";
			JobHeader job = Helper.AddJobToDocket(Docket);
			job.JH_JobNum = "1";

			AssertEquals("Precondition", ZString.Empty, Docket.WD_DocketID);
			Factory.Save();
			AssertEquals("WD_DocketID is not set", "W00000001", Docket.WD_DocketID);

			Docket = GetNewBusinessObject();
			Docket.WD_OH_Client = org.PK;
			Docket.WD_WW_Whs = whs.PK;
			Docket.WD_ExternalReference = "2";
			job = Helper.AddJobToDocket(Docket);
			job.JH_JobNum = "2";

			AssertEquals("Precondition", ZString.Empty, Docket.WD_DocketID);
			Factory.Save();
			AssertEquals("WD_DocketID is not set", "W00000002", Docket.WD_DocketID);
		}

		#endregion

		#region TestRunPreSaveValidationCore

		public virtual void TestRunPreSaveValidationCore()
		{
			AssertNoErrors(Docket.WD_WW_WhsInfo);
			Docket.RunPreSaveValidation();
			AssertHasError(Docket.WD_WW_WhsInfo, "Please enter a Warehouse.");
		}

		public void TestRunPreSaveValidationCore_SynchronisesOffset_InvalidWarehouseGuid()
		{
			var docket = SetupForTestFinaliseDocket();
			var dateTime = new ZDateTime(0024, 06, 12, 12, 30, 00);

			docket.WD_BookingDate = new ZDateTimeOffset(dateTime, TimeSpan.FromHours(0));
			docket.WD_WW_Whs = Guid.NewGuid();

			AssertNoExceptionThrown(() => docket.RunPreSaveValidation());
		}

		public void TestRunPreSaveValidationCore_SynchronisesOffset_BookingDate()
		{
			var docket = SetupForTestFinaliseDocket();
			var warehouse = docket.Warehouse;
			var dateTime = new ZDateTime(2024, 06, 12, 12, 30, 00);

			docket.WD_BookingDate = new ZDateTimeOffset(dateTime, TimeSpan.FromHours(0));
			docket.RunPreSaveValidation();

			var expectedOffset = warehouse.GetWarehouseBranchDateTimeOffset(dateTime);
			Assert("Docket should not be in error", !docket.HasErrors);
			AssertEquals(
				"Offsets should have same value, including offset component",
				expectedOffset.ToString("dd-MMM-yyyy hh:mm:ss zzz"),
				docket.WD_BookingDate.ToString("dd-MMM-yyyy hh:mm:ss zzz"));
		}

		public void TestRunPreSaveValidationCore_SynchronisesOffset_BookingDate_FailsIfNoWarehouse()
		{
			var docket = GetNewBusinessObject();
			var dateTime = new ZDateTime(2024, 06, 12, 12, 30, 00);

			docket.WD_WW_Whs = ZGuid.Empty;
			docket.WD_BookingDate = new ZDateTimeOffset(dateTime, TimeSpan.FromHours(0));
			docket.RunPreSaveValidation();

			Assert("Docket should be in error", docket.HasErrors);
			AssertEquals("Do not sychronise Offset if Warehouse is unpopulated.",
				dateTime.ToString("12-Jun-2024 12:30:00 +00:00"),
				docket.WD_BookingDate.ToString("dd-MMM-yyyy hh:mm:ss zzz"));
		}

		public void TestRunPreSaveValidationCore_SynchronisesOffset_BookingDate_FailsIfDocketInError()
		{
			var docket = SetupForTestFinaliseDocket();
			var dateTime = new ZDateTime(0024, 06, 12, 12, 30, 00);

			docket.WD_BookingDate = new ZDateTimeOffset(dateTime, TimeSpan.FromHours(0));

			docket.RunPreSaveValidation();

			Assert("WD_BookingDateInfo should be in error", docket.WD_BookingDateInfo.HasErrors());
			AssertEquals("Do not sychronise Offset if this field is in Error.",
				"12-Jun-0024 12:30 +00:00",
				docket.WD_BookingDate.ToString("dd-MMM-yyyy hh:mm zzz"));
		}

		public abstract void TestRunPreSaveValidationCore_SynchronisesOffset_UpdatesDocketLines();

		#endregion

		#region TestOnCreateAutoAdminLog

		public virtual void TestOnCreateAutoAdminLog()
		{
			// this also tests IsAutoLogged
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;
			docket.WD_WW_Whs = whs.PK;

			AssertEquals("Precondition", 0, docket.Logs.GetAllLogs().Count);
			Factory.Save();

			AssertEquals("Log Count unexpected", 2, docket.Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent != Events.WorkflowTemplateAppliedCode));
			AssertNotNull("Admin Log not created", docket.Logs.AutoCreatedLog);
		}

		#endregion

		#region TestFactoryLoadingWrongDocketType

		#region TestFactoryLoadWrongTypeFlag

		public void TestFactoryLoadWrongTypeFlag()
		{
			AssertNoExceptionThrown(() => TestFactoryLoadWrongTypeDocketSaveFactory(true));
		}

		#endregion

		#region TestFactoryLoadWrongTypeFlag_InMemory

		public void TestFactoryLoadWrongTypeFlag_InMemory()
		{
			AssertNoExceptionThrown(() => TestFactoryLoadWrongTypeDocketSaveFactory(false));
		}
		#endregion

		#region TestFactoryLoadWrongTypeFlag

		void TestFactoryLoadWrongTypeDocketSaveFactory(bool saveFactory = true)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			if (saveFactory)
			{
				Factory.Save();
			}

			var typesToCheck = new[] { typeof(WhsOrder), typeof(WhsReceive), typeof(WhsTransfer), typeof(WhsAdjustment), typeof(WhsWorkOrder) };
			Assert("Precondition", docket.GetType().IsAssignableFrom(typeof(TDocket)));

			foreach (var typeToCheck in typesToCheck)
			{
				if (!docket.GetType().IsAssignableFrom(typeToCheck) && !docket.GetType().IsSubclassOf(typeToCheck))
				{
					NUnit.Framework.Assert.That(delegate
					{
						Factory.Load(typeToCheck, docket.PK);
					}, CustomConstraints.InnermostExceptionThrown(typeof(NotSupportedException)));
				}
				else
				{
					AssertNoExceptionThrown("Should not throw exception", () => Factory.Load(typeToCheck, docket.PK));
				}
			}
		}

		#endregion

		#endregion

		#endregion

		#region Delete

		#region TestDelete

		public virtual void TestDelete()
		{
			WhsDocketReference reference = Docket.References.AddNew();
			WhsDocketContainer container = Docket.Containers.AddNew();
			WhsDocketPallet pallet = Docket.Pallets.AddNew();

			WhsDocketLine line = Docket.Lines.AddNew();

			AssertEquals("Precondition", 1, Docket.Lines.Count);
			AssertEquals("Precondition", 1, Docket.References.Count);
			AssertEquals("Precondition", 1, Docket.Containers.Count);
			AssertEquals("Precondition", 1, Docket.Pallets.Count);

			ProcessTask task = null;
			if (Docket.WorkflowItems != null)
			{
				task = Docket.WorkflowItems.AddNew();
				AssertEquals("Precondition", 1, Docket.WorkflowItems.Count);
			}

			Docket.Delete();

			AssertEquals(true, line.IsDeleted);
			AssertEquals(true, reference.IsDeleted);
			AssertEquals(true, container.IsDeleted);
			AssertEquals(true, pallet.IsDeleted);

			AssertEquals(0, Docket.Lines.Count);
			AssertEquals(0, Docket.References.Count);
			AssertEquals(0, Docket.Containers.Count);
			AssertEquals(0, Docket.Pallets.Count);

			if (Docket.WorkflowItems != null)
			{
				AssertEquals(true, task.IsDeleted);
				AssertEquals(0, Docket.WorkflowItems.Count);
			}
		}

		#endregion

		#region TestDelete_DeletesAllRelatedJobDocAddress

		public void TestDelete_DeletesAllRelatedJobDocAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var docket = GetNewBusinessObject();
			AdditionalSetupForDelete_DeletesAllRelatedJobDocAddress(docket, data);
			docket.SupplierDocAddress.OrganisationPK = data.Org1.PK;

			if (docket is IJobWithTransportCompany job)
			{
				job.TransportCoDocAddress.OrganisationPK = data.Org1.PK;
			}

			docket.TransportBillToDocAddress.OrganisationPK = data.Org1.PK;

			docket.Delete();
			Factory.Save();

			var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, docket.PK));
			AssertEquals("All JobDocAddresses should have been deleted on Docket.Delete().", 0, jobDocAddress.Length);
		}

		protected virtual void AdditionalSetupForDelete_DeletesAllRelatedJobDocAddress(TDocket docket, TestDataSimpleEnvironment data)
		{
		}

		#endregion

		#region TestDelete_DeletesAllRelatedJobHeaders

		public void TestDelete_DeletesAllRelatedJobHeaders()
		{
			var docket = GetNewBusinessObject();
			var jobHeader1 = Helper.CreateRatingJob(docket, "WD");
			var jobHeader2 = Helper.CreateRatingJob(docket, "WD");
			AssertEquals("Precondition", false, docket.IsDeleted);
			AssertEquals("Precondition", false, jobHeader1.IsDeleted);
			AssertEquals("Precondition", false, jobHeader2.IsDeleted);

			docket.Delete();
			AssertEquals(true, docket.IsDeleted);
			AssertEquals(true, jobHeader1.IsDeleted);
			AssertEquals(true, jobHeader2.IsDeleted);
		}

		public void TestDelete_WhatHappensIfJobIsInDb()
		{
			var expectedExceptionMessage = "You cannot delete Job in Database.";

			var docket = GetDocketForRating();
			AssertEquals("Precondition", false, docket.IsDeleted);
			Factory.Save();

			var jobHeader = Helper.CreateRatingJob(docket, "WD");
			AssertEquals("Precondition", false, jobHeader.IsDeleted);
			Factory.Save();

			var exception = AssertExceptionThrown<InvalidOperationException>(() => docket.Delete());

			AssertContains("Exception Message", expectedExceptionMessage, exception.Message);
			AssertEquals(false, docket.IsDeleted);
			AssertEquals(false, jobHeader.IsDeleted);
		}

		protected abstract TDocket GetDocketForRating();

		#endregion

		#region TestDelete_DeletesAllRelatedPivots

		public void TestDelete_DeletesAllRelatedPivots()
		{
			var docket = GetNewBusinessObject();
			var parent = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var pivot = Factory.New<WhsDocketJobPivot>();
			pivot.WV_ParentId = parent.PK;
			pivot.WV_ParentTableCode = parent.TablePrefix;
			pivot.WV_DocketType = docket.WD_DocketType;
			pivot.WV_WD_Docket = docket.PK;

			docket.Delete();
			AssertEquals("Precondition", true, docket.IsDeleted);
			AssertEquals("Deleting Docket should have deleted related pivots.", true, pivot.IsDeleted);
			AssertEquals("Deleting Docket should only delete related pivots.", false, parent.IsDeleted);
			Factory.Save();
		}

		#endregion

		#region TestDelete_DeletesAllLines

		public void TestDelete_DeletesAllLines()
		{
			var docket = GetNewBusinessObject();
			var line1 = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();
			line2.WE_IsOriginalInventory = false;

			docket.Delete();
			AssertEquals(true, docket.IsDeleted);
			AssertEquals(true, line1.IsDeleted);
			AssertEquals(true, line2.IsDeleted);
		}

		#endregion

		#endregion

		#region Fetch Strategy

		public void TestGetFetchStrategyIsCorrectType()
		{
			AssertEquals(FetchStrategyType, Docket.FetchStrategy.GetType());
		}

		protected virtual Type FetchStrategyType
		{
			get { return typeof(WhsDocketFetchStrategy); }
		}

		#endregion

		#region TestCheckExternalReferenceForDuplicates

		public void TestCheckExternalReferenceForDuplicates()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var client1 = Helper.CreateClient("Client1");
			var client2 = Helper.CreateClient("Client2");

			CreateWhsDocket(client1, whs, "REF1", false);
			CreateWhsDocket(client2, whs, "REF5", false);

			var docket3 = CreateWhsDocket(client1, whs, "REF7", false);
			docket3.WD_DocketType = "XXX";

			var docket4 = CreateWhsDocket(client1, whs, "REF9", false);
			docket4.WD_ExternalReferenceSplit = 1;

			// no docket exists with same client, docket type, ref and split-number
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = client1.PK;
			docket.WD_ExternalReference = "REFABC";
			AssertEquals(false, docket.CheckExternalReferenceForDuplicates());

			// docket exists with same client, docket type, ref and split-number
			docket.WD_ExternalReference = "REF1";
			AssertEquals(true, docket.CheckExternalReferenceForDuplicates());

			// other docket exists with same reference, docket type and slpit-number but different client
			docket.WD_ExternalReference = "REF5";
			AssertEquals(false, docket.CheckExternalReferenceForDuplicates());

			// other docket exists with same client, ref and split-number but different docket type 
			docket.WD_ExternalReference = "REF7";
			AssertEquals(false, docket.CheckExternalReferenceForDuplicates());

			// other docket exists with same client, reference and docket type but different split number
			docket.WD_ExternalReference = "REF9";
			AssertEquals(false, docket.CheckExternalReferenceForDuplicates());
		}

		public void TestCheckExternalReferenceForDuplicates_OnlyHitsDBIfNeeded_ReferenceChanged()
		{
			TestCheckExternalReferenceForDuplicates_OnlyHitsDBIfNeeded_Core(wd => wd.WD_ExternalReference, (wd, extRef) => wd.WD_ExternalReference = extRef, (ZString)"REFERENCE2");
		}

		public void TestCheckExternalReferenceForDuplicates_OnlyHitsDBIfNeeded_ClientChanged()
		{
			var client2 = Helper.CreateClient("Client2");
			TestCheckExternalReferenceForDuplicates_OnlyHitsDBIfNeeded_Core(wd => wd.WD_OH_Client, (wd, oh) => wd.WD_OH_Client = oh, client2.PK);
		}

		public void TestCheckExternalReferenceForDuplicates_OnlyHitsDBIfNeeded_ReferenceSplitChanged()
		{
			TestCheckExternalReferenceForDuplicates_OnlyHitsDBIfNeeded_Core(wd => wd.WD_ExternalReferenceSplit, (wd, split) => wd.WD_ExternalReferenceSplit = split, (ZByte)1);
		}

		public void TestCheckExternalReferenceForDuplicates_OnlyHitsDBIfNeeded_DocketTypeChanged()
		{
			var docketInOtherFactory = new BusinessObjectFactory().New<TDocket>();
			var docketType = docketInOtherFactory.WD_DocketType == DocketType.Codes.Order ? DocketType.Codes.Receive : DocketType.Codes.Order;
			var type = docketInOtherFactory.WD_DocketType == DocketType.Codes.Order ? typeof(WhsReceive) : typeof(WhsOrder);

			TestCheckExternalReferenceForDuplicates_OnlyHitsDBIfNeeded_Core(wd => wd.WD_DocketType, (wd, t) => wd.WD_DocketType = t, (ZString)docketType, type);
		}

		void TestCheckExternalReferenceForDuplicates_OnlyHitsDBIfNeeded_Core<T>(Func<WhsDocket, T> getFromColumnToChange, Action<WhsDocket, T> setToColumnToChange, T uniqueData, Type typeOverride = null)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket1 = CreateWhsDocket(data.Org1, data.Whs1, "REFERENCE1", false);
			Factory.Save();
			AssertEquals("Precondition.", true, docket1.IsInDatabase);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var docket2 = (WhsDocket)factory2.New(typeOverride ?? typeof(TDocket));
			docket2.WD_OH_Client = data.Org1.PK;
			docket2.WD_WW_Whs = data.Whs1.PK;
			docket2.WD_ExternalReference = "REFERENCE1";
			docket2.IsUniqueExternalReferenceCreatedOnSave = false;
			setToColumnToChange(docket2, uniqueData);
			factory2.Save();
			factory2.ResetDatabaseLoadCount();

			AssertEquals("Should find no duplicates.", false, docket2.CheckExternalReferenceForDuplicates());
			AssertEquals("Should not have hit database as no relevant data has changed and constraints will suffice.", 0, factory2.GetTableHitCount(WhsDocketSchema.Constants.TableName));

			var factory3 = new BusinessObjectFactory { RefreshEnabled = false };
			var docket2_InFactory3 = factory3.Load<WhsDocket>(docket2.PK);
			factory3.ResetDatabaseLoadCount();

			setToColumnToChange(docket2_InFactory3, getFromColumnToChange(docket1));
			AssertEquals("Should find duplicate.", true, docket2_InFactory3.CheckExternalReferenceForDuplicates());
			AssertEquals("Should have hit database as relevant data has changed.", 1, factory3.GetTableHitCount(WhsDocketSchema.Constants.TableName));
		}

		#endregion

		#region Related Entities

		#region Notes

		#region TestNoteContextsForRelatedNotes

		public void TestNoteContextsForRelatedNotes()
		{
			var docket = GetNewBusinessObject();
			TestNoteContextsForRelatedNotesSetup(docket);
			TestNoteContextsForRelatedNotesAssertions(docket);
		}

		protected virtual void TestNoteContextsForRelatedNotesSetup(TDocket docket)
		{
			var client = Helper.CreateClient();
			var warehouse = Helper.CreateWarehouse("Warehouse");
			CreateTestNoteCollection(client);
			CreateTestNoteCollection(warehouse);

			docket.WD_OH_Client = client.PK;
			docket.WD_WW_Whs = warehouse.PK;
		}

		protected virtual void TestNoteContextsForRelatedNotesAssertions(TDocket docket)
		{
		}

		protected void CreateTestNoteCollection(BusinessObject businessObject)
		{
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.A, StmNoteContextDirection.A, StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.A, StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.R, StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.R, StmNoteContextFreightMode.R);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.O, StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.O, StmNoteContextFreightMode.O);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.O, StmNoteContextFreightMode.R);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.O, StmNoteContextFreightMode.T);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.I, StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.I, StmNoteContextFreightMode.T);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.I, StmNoteContextFreightMode.D);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.I, StmNoteContextFreightMode.S);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.I, StmNoteContextFreightMode.P);

			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.A, StmNoteContextDirection.I, StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.A, StmNoteContextDirection.O, StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.A, StmNoteContextDirection.A, StmNoteContextFreightMode.S);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.A, StmNoteContextDirection.A, StmNoteContextFreightMode.R);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.F, StmNoteContextDirection.I, StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.F, StmNoteContextDirection.A, StmNoteContextFreightMode.P);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.F, StmNoteContextDirection.I, StmNoteContextFreightMode.P);
		}

		void GetStmNote(StmNote note, StmNoteContextModule module, StmNoteContextDirection direction, StmNoteContextFreightMode freightMode)
		{
			note.ST_NoteContextModule = module.ToString();
			note.ST_NoteContextDirection = direction.ToString();
			note.ST_NoteContextFreightMode = freightMode.ToString();
		}

		#endregion

		#region TestNoteTypes

		public void TestNoteTypes()
		{
			var expectedNoteTypes = ExpectedAdditionalNoteTypes.Concat(ExpectedAdditionalNoteTypesCore);

			if (SupportsUnmatchedOrgNoteType)
			{
				expectedNoteTypes = expectedNoteTypes.Concat(new[] { PredefinedNoteTypes.Instance.UnmatchedOrgDetails });
			}

			if (SupportsUnrecognisedAdditionalReferenceType)
			{
				expectedNoteTypes = expectedNoteTypes.Concat(new[] { PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes });
			}

			var docket = GetNewBusinessObject();
			AssertContainsExactElementsInAnyOrder(expectedNoteTypes, docket.NoteTypes);
		}

		PredefinedNoteType[] ExpectedAdditionalNoteTypes
		{
			get
			{
				return new[]
				{
					PredefinedNoteTypes.Instance.InternalWorkNotes,
					PredefinedNoteTypes.Instance.HandlingInstructions,
					PredefinedNoteTypes.Instance.ClientVisibleJobNotes,
					PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation,
					PredefinedNoteTypes.Instance.AutoRatingAuditLog,
				};
			}
		}

		protected virtual IEnumerable<PredefinedNoteType> ExpectedAdditionalNoteTypesCore => Enumerable.Empty<PredefinedNoteType>();

		protected virtual bool SupportsUnmatchedOrgNoteType => true;
		protected virtual bool SupportsUnrecognisedAdditionalReferenceType => true;

		#endregion

		#endregion

		#region TestDistributionCentreDocAddress

		public void TestDistributionCentreDocAddress()
		{
			var org = Helper.CreateClient("O1");
			var docket = GetNewBusinessObject();
			AssertNotNull("Distribution centre DocAddress should be set.", docket.DistributionCentreDocAddress);
			AssertEquals(ZGuid.Empty, docket.DistributionCentreDocAddress.OrganisationPK);

			docket.DistributionCentreDocAddress.OrganisationPK = org.PK;
			AssertEquals(org.MainAddress.PK, docket.DistributionCentreDocAddress.E2_OA_Address);

			docket.DistributionCentreDocAddress.E2_AddressOverride = true;
			AssertNotNull(docket.DistributionCentreDocAddress);
			AssertNotEquals(org.MainAddress.PK, docket.DistributionCentreDocAddress.E2_OA_Address);
			AssertEquals(ZGuid.Empty, docket.DistributionCentreDocAddress.OrganisationPK);
		}

		#endregion

		#region TestLines

		public void TestLines()
		{
			AssertType(ExpectedLineCollectionType, GetNewBusinessObject().Lines);
		}

		protected abstract Type ExpectedLineCollectionType { get; }

		#endregion

		#region TestParentDocket

		public void TestParentDocket()
		{
			AssertNull("Shouldn't blow up if no parent.", Docket.ParentDocket);

			var parent = GetNewBusinessObject();
			Docket.WD_WD_ParentDocket = parent.PK;
			AssertEquals(parent.PK, Docket.ParentDocket.PK);
		}

		#endregion

		#region TestRelatedJobs

		public void TestRelatedJobs()
		{
			var docket = GetNewBusinessObject();

			var validRelatedJobs = GetValidRelatedJobs(docket);
			var invalidRelatedJobs = GetInvalidRelatedJobs(docket);
			AssertCollectionNotContains("Should not self reference in RelatedJobs.", docket, docket.RelatedJobs);

			if (validRelatedJobs.Count == 0 && invalidRelatedJobs.Count == 0)
			{
				Assert("There are no related jobs for this Docket.", true);
			}
			else
			{
				foreach (var job in validRelatedJobs)
				{
					var errorMsg = string.Format("Related Jobs Collection should have contained job '{0} / {1} / {2} / {3}.", job.JobNumber, job.JobDescription, job.JobStatus, job.ControllerID);
					AssertCollectionContains(errorMsg, job, docket.RelatedJobs);
				}

				foreach (var job in invalidRelatedJobs)
				{
					var errorMsg = string.Format("Related Jobs Collection should NOT have contained job '{0} / {1} / {2} / {3}.", job.JobNumber, job.JobDescription, job.JobStatus, job.ControllerID);
					AssertCollectionNotContains(errorMsg, job, docket.RelatedJobs);
				}
			}
		}

		protected abstract List<IRelatedJob> GetValidRelatedJobs(TDocket docket);
		protected abstract List<IRelatedJob> GetInvalidRelatedJobs(TDocket docket);

		#endregion

		#region TestGetRelatedParents

		public void TestGetRelatedParents()
		{
			var docket = GetNewBusinessObject();
			var shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var pivot = Factory.New<WhsDocketJobPivot>();
			pivot.WV_ParentId = shipment.PK;
			pivot.WV_ParentTableCode = shipment.TablePrefix;
			pivot.WV_DocketType = docket.WD_DocketType;
			pivot.WV_WD_Docket = docket.PK;

			var cartageJob = (BusinessObject)Factory.New<ICommonCartage>();
			cartageJob.FillWithValidTestData();
			cartageJob[JobCartageSchema.JJ_ParentID] = docket.PK;
			cartageJob[JobCartageSchema.JJ_ParentTableCode] = docket.TablePrefix;
			cartageJob[JobCartageSchema.JJ_ConsignmentID] = docket.WD_DocketID;

			docket.WD_WD_ParentDocket = GetNewBusinessObject().PK;
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { shipment }, docket.GetRelatedParents());

			docket.Delete();
			AssertEquals("Should not load any parents when deleted", false, docket.GetRelatedParents().Any());
		}

		#endregion

		#region JobDocAddresses

		#region JobDocAddress Defaults

		#region TestPickUpDocAddressDefaultTypes

		public void TestPickUpDocAddressDefaultTypes()
		{
			var docket = GetNewBusinessObject();
			AssertDocAddressDefaultTypes(docket, DocAddressType.PickUpAddress, docket.PickUpDocAddressRequirement, ContactType.Consignee);
		}

		#endregion

		#region TestDropOffDocAddressDefaultTypes

		public void TestDropOffDocAddressDefaultTypes()
		{
			var docket = GetNewBusinessObject();
			AssertDocAddressDefaultTypes(docket, DocAddressType.DropOffAddress, docket.DropOffDocAddressRequirement, ContactType.All);
		}

		#endregion

		#region TestGoodsBillToDocAddressDefaultTypes

		public void TestGoodsBillToDocAddressDefaultTypes()
		{
			var docket = GetNewBusinessObject();
			AssertDocAddressDefaultTypes(docket, DocAddressType.GoodsBillToAddress, docket.GoodsBillToDocAddressRequirement, ContactType.Consignee);
		}

		#endregion

		#region TestSupplierDocAddressDefaultTypes

		public void TestSupplierDocAddressDefaultTypes()
		{
			var docket = GetNewBusinessObject();
			AssertDocAddressDefaultTypes(docket, DocAddressType.SupplierDocumentaryAddress, docket.SupplierDocAddressRequirement, ContactType.Consignor);
		}

		#endregion

		#region TestTransportCoDocAddressDefaultTypes

		public void TestTransportCoDocAddressDefaultTypes()
		{
			var docket = GetNewBusinessObject();
			if (docket is IJobWithTransportCompany job)
			{
				var requirement = GetJobDocAddressRequirement(job, DocAddressType.TransportCompanyDocumentaryAddress);
				AssertDocAddressDefaultTypes(docket, DocAddressType.TransportCompanyDocumentaryAddress, requirement, ContactType.TransportServices);
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual JobDocAddressRequirement GetJobDocAddressRequirement(IJobWithTransportCompany docket, DocAddressType addressType)
		{
			throw new NotImplementedException();
		}

		#endregion

		protected void AssertDocAddressDefaultTypes(TDocket docket, DocAddressType docAddressType, JobDocAddressRequirement jobDocAddressRequirement, ContactType contactType)
		{
			AssertNotNull(docAddressType.ToString() + " requirement should exist.", jobDocAddressRequirement);
			AssertEquals("Invalid " + docAddressType.ToString() + " requirement", docAddressType, jobDocAddressRequirement.DefaultDocAddressType);
			AssertEquals("Invalid " + docAddressType.ToString() + " requirement", contactType, jobDocAddressRequirement.DefaultContactType);

			var iDocket = (IDocAddresses)docket;
			AssertEquals("Interface should get " + docAddressType.ToString() + "Requirement", jobDocAddressRequirement, iDocket.GetDocAddressRequirement(docAddressType));
			AssertCodeInArray(iDocket.SupportedAddressTypes, docAddressType);
		}

		void AssertCodeInArray(IReadOnlyList<DocAddressType> readOnlyList, DocAddressType code)
		{
			AssertEquals("Code should have been found: " + DocAddressTypes.GetCode(Factory, code), true, readOnlyList.Contains(code));
		}

		#endregion

		#region Name Or PK Fields

		#region Properties

		#region TestTransportCoNameOrPK

		public void TestTransportCoNameOrPK()
		{
			var docket = GetNewBusinessObject();
			if (docket is IJobWithTransportCompany job)
			{
				var transportCo = Factory.New<OrgHeader>();
				transportCo.OH_Code = "TEST";
				transportCo.OH_FullName = "FULL NAME";

				job.TransportCoDocAddress.E2_AddressOverride = true;
				job.TransportCoDocAddress.E2_CompanyName = "TEST COMPANY";
				AssertEquals("TEST COMPANY", job.TransportCoNameOrPK);

				job.TransportCoDocAddress.E2_AddressOverride = false;
				job.TransportCoDocAddress.OrganisationPK = transportCo.PK;
				AssertEquals(transportCo.PK.ToString(), job.TransportCoNameOrPK);

				job.TransportCoDocAddress.OrganisationPK = ZGuid.Empty;
				AssertEquals(ZGuid.Empty.ToString(), job.TransportCoNameOrPK);

				job.TransportCoDocAddress.OrganisationPK = transportCo.PK;
				var code = RelatedBusinessObjectAttribute.GetCodeForGuid(job.TransportCoNameOrPKInfo);
				AssertEquals("Docket.TransportCoNameOrPK should be TransportCo.PK", transportCo.PK, new ZGuid(job.TransportCoNameOrPKInfo.Value));
				AssertEquals("Ensure that the RelatedBusinessObjectAttribute is correct(OrgHeader).", "TEST", code);
			}
			else
			{
				AssertExceptionThrown<NotSupportedException>(() => _ = docket.TransportCoNameOrPK);
				AssertExceptionThrown<NotSupportedException>(() => docket.TransportCoNameOrPK = "Test");
			}
		}

		public void TestTransportCoNameOrPK_InvalidGuid()
		{
			var docket = GetNewBusinessObject();
			if (docket is IJobWithTransportCompany job)
			{
				AssertNoExceptionThrown(() => job.TransportCoNameOrPK = "NOTAGUID");
				AssertEquals("Should still return a valid Value.", ZGuid.Empty.ToString(), job.TransportCoNameOrPK);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestTransportCoNameOrPK_ReadOnly()
		{
			var docket = GetNewBusinessObject();
			if (docket is IJobWithTransportCompany job)
			{
				AssertEquals("TransportCoNameOrPK readonly value should be set correctly", TransportIsReadOnly, job.TransportCoNameOrPKInfo.ReadOnly);
			}
			else
			{
				AssertExceptionThrown<NotSupportedException>(() => _ = docket.TransportCoNameOrPK);
			}
		}

		public void TestWD_TransportReference_ReadOnly()
		{
			var docket = GetNewBusinessObject();
			AssertEquals("WD_TransportReference readonly value should be set correctly", TransportIsReadOnly, docket.WD_TransportReferenceInfo.ReadOnly);
		}

		protected virtual bool TransportIsReadOnly => false;

		#endregion

		#endregion

		#region FieldType

		#region TestTransportCoFieldType

		public void TestTransportCoFieldType()
		{
			var docket = GetNewBusinessObject();
			if (docket is IJobWithTransportCompany job)
			{
				job.TransportCoDocAddress.E2_AddressOverride = true;
				AssertEquals(nameof(FieldType.Text), job.TransportCoFieldType);

				job.TransportCoDocAddress.E2_AddressOverride = false;
				AssertEquals(nameof(FieldType.Guid), job.TransportCoFieldType);
			}
			else
			{
				AssertExceptionThrown<ArgumentException>(() => _ = docket[nameof(IJobWithTransportCompany.TransportCoFieldType)]);
			}
		}

		#endregion

		#endregion

		#region MaxLength

		#region TestTransportCoNameOrPKMaxLength

		public void TestTransportCoNameOrPKMaxLength()
		{
			var docket = GetNewBusinessObject();
			if (docket is IJobWithTransportCompany job)
			{
				job.TransportCoDocAddress.E2_AddressOverride = true;
				AssertEquals(JobDocAddress.Schema.E2_CompanyNameTruncatedLength, job.TransportCoNameOrPKInfo.MaxLength);

				job.TransportCoDocAddress.E2_AddressOverride = false;
				AssertEquals(ZGuid.Empty.ToString().Length, job.TransportCoNameOrPKInfo.MaxLength);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestTransportCoNameOrPKHumanReadablename

		public void TestTransportCoNameOrPKHumanReadablename()
		{
			var docket = GetNewBusinessObject();
			if (docket is IJobWithTransportCompany job)
			{
				AssertEquals("Transport Company: Organization", job.TransportCoNameOrPKInfo.HumanReadableName);
			}
			else
			{
				AssertNull(docket.FindPropertyInfo(nameof(IJobWithTransportCompany.TransportCoNameOrPK)));
			}
		}

		#endregion

		#endregion

		#endregion

		#region DocAddresses

		#region TestSupplierDocAddress

		public void TestSupplierDocAddress()
		{
			AssertNotNull(Docket.SupplierDocAddress);
			AssertNull(Docket.Supplier);

			OrgHeader org = Factory.New<OrgHeader>();
			Docket.SupplierDocAddress.OrganisationPK = org.PK;
			AssertEquals(org, Docket.Supplier);

			Docket.SupplierDocAddress.E2_AddressOverride = true;
			AssertNull(Docket.Supplier);
		}

		public void TestSupplierDocAddress_ReadOnlyIsLazyTriggered()
		{
			var docket = GetNewBusinessObject();
			AssertNotNull("Poke & Precondition", docket.SupplierDocAddress);

			AssertPersistentPropertiesHitCount("Getting SupplierDocAddress should not trigger property hits.", 0, () => _ = docket.SupplierDocAddress);

			var hits = GetPersistentPropertiesHitCount(() => _ = docket.SupplierDocAddress.ReadOnly);
			AssertEquals("Poking at ReadOnly property should trigger state calculation.", true, hits > 0);
		}

		public void TestSupplierPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			AssertNotNull(docket.SupplierDocAddress);
			AssertNull(docket.Supplier);
			AssertEquals(ZGuid.Empty, docket.SupplierPK);

			var org = Factory.New<OrgHeader>();
			docket.SupplierDocAddress.OrganisationPK = org.PK;
			AssertEquals(org, docket.Supplier);
			AssertEquals(org.PK, docket.SupplierPK);

			docket.SupplierDocAddress.E2_AddressOverride = true;
			AssertNull(docket.Supplier);
			AssertEquals(ZGuid.Empty, docket.SupplierPK);
		}

		#endregion

		#region TestTransportCoDocAddress

		public void TestTransportCoDocAddress()
		{
			var docket = GetNewBusinessObject();
			if (docket is IJobWithTransportCompany job)
			{
				AssertNotNull("TransportCo DocAddress should be set.", job.TransportCoDocAddress);
				AssertEquals("TransportCo DocAddress Requirement should be correct.", GetJobDocAddressRequirement(job, TransportCoConstants.AddressType), job.TransportCoDocAddress.Requirement);
				AssertNull("TransportCo should be null", job.GetTransportCo());
				AssertEquals("TransportCoPK should be empty", ZGuid.Empty, job.TransportCoPK);

				var org = Factory.New<OrgHeader>();
				var address = org.MainAddress;

				job.TransportCoPK = org.PK;

				AssertEquals(org, job.GetTransportCo());
				AssertEquals("TransportCo DocAddress should indirectly point to new Org.", org.PK, job.TransportCoDocAddress.OrganisationPK);
				AssertEquals("TransportCo PK should should indirectly point to new Org.", org.PK, job.TransportCoPK);
				AssertEquals("TransportCo DocAddress should indirectly point to new Org Address.", address.PK, job.TransportCoDocAddress.E2_OA_Address);

				job.TransportCoDocAddress.E2_AddressOverride = true;
				AssertNull("TransportCo should be null", job.GetTransportCo());
				AssertEquals("TransportCo shoulbn't be real organisation", false, job.TransportCoDocAddress.HasRealOrganisation);
				AssertEquals("TransportCoPK should be empty", ZGuid.Empty, job.TransportCoPK);
			}
			else
			{
				AssertExceptionThrown<ArgumentException>(() => _ = docket[nameof(IJobWithTransportCompany.TransportCoDocAddress)]);
			}
		}

		public void TestGetTransportCoDocAddressReadOnly()
		{
			var docket = GetNewBusinessObject();
			if (docket is IJobWithTransportCompany job)
			{
				TestNonStandardReadOnly1(d => ((IJobWithTransportCompany)d).GetTransportCoDocAddressReadOnly(), d => nameof(IJobWithTransportCompany.GetTransportCoDocAddressReadOnly));
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestGoodsBillToDocAddress

		public virtual void TestGoodsBillToDocAddress()
		{
			AssertNotNull("GoodsBillTo DocAddress should be set.", Docket.GoodsBillToDocAddress);
			AssertNull("GoodsBillTo should be null", Docket.GoodsBillTo);
			AssertEquals("GoodsBillToPK should be empty", ZGuid.Empty, Docket.GoodsBillToPK);
			AssertEquals("GoodsBillToAddressPK should be empty", ZGuid.Empty, Docket.GoodsBillToAddressPK);

			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress address = org.MainAddress;//Factory.New<OrgAddress>();

			Docket.GoodsBillToPK = org.PK;

			AssertEquals(org, Docket.GoodsBillTo);
			AssertEquals("GoodsBillTo DocAddress should indirectly point to new Org.", org.PK, Docket.GoodsBillToDocAddress.OrganisationPK);
			AssertEquals("GoodsBillToPK should indirectly point to new Org.", org.PK, Docket.GoodsBillToPK);
			AssertEquals("GoodsBillTo DocAddress should indirectly point to new Org Address.", address.PK, Docket.GoodsBillToDocAddress.E2_OA_Address);
			AssertEquals("GoodsBillTo AddressPK should indirectly point to new Org Address.", address.PK, Docket.GoodsBillToAddressPK);

			Docket.GoodsBillToDocAddress.E2_AddressOverride = true;
			AssertNull("GoodsBillTo should be null", Docket.GoodsBillTo);
			AssertEquals("GoodsBillToPK should be empty", ZGuid.Empty, Docket.GoodsBillToPK);
			AssertEquals("GoodsBillToAddressPK should be empty", ZGuid.Empty, Docket.GoodsBillToAddressPK);
		}

		public void TestGoodsBillToDocAddress_ReadOnlyIsLazyTriggered()
		{
			var docket = GetNewBusinessObject();
			AssertNotNull("Poke & Precondition", docket.GoodsBillToDocAddress);

			AssertPersistentPropertiesHitCount("Getting GoodsBillToDocAddress should not trigger property hits.", 0, () => _ = docket.GoodsBillToDocAddress);

			var hits = GetPersistentPropertiesHitCount(() => _ = docket.GoodsBillToDocAddress.ReadOnly);
			AssertEquals("Poking at ReadOnly property should trigger state calculation (if there is one).", true, hits >= 0);
		}

		#endregion

		#region TestPickUpDocAddress

		public virtual void TestPickUpDocAddress()
		{
			AssertNotNull("PickUp DocAddress should be set.", Docket.PickUpDocAddress);
			AssertEquals("PickUpPK should be empty", ZGuid.Empty, Docket.PickUpPK);
			AssertEquals("PickUpAddressPK should be empty", ZGuid.Empty, Docket.PickUpAddressPK);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress address = org.MainAddress;

			//Address.OA_OH = Org.PK;
			Docket.PickUpPK = org.PK;

			AssertEquals("PickUp DocAddress should indirectly point to new Org.", org.PK, Docket.PickUpDocAddress.OrganisationPK);
			AssertEquals("PickUpPK should indirectly point to new Org.", org.PK, Docket.PickUpPK);
			AssertEquals("PickUp DocAddress should indirectly point to new Org Address.", address.PK, Docket.PickUpDocAddress.E2_OA_Address);
			AssertEquals("PickUp AddressPK should indirectly point to new Org Address.", address.PK, Docket.PickUpAddressPK);

			Docket.PickUpDocAddress.E2_AddressOverride = true;
			AssertEquals("PickUpPK should be empty", ZGuid.Empty, Docket.PickUpPK);
			AssertEquals("PickUpAddressPK should be empty", ZGuid.Empty, Docket.PickUpAddressPK);
		}

		public void TestPickUpDocAddress_ReadOnlyIsLazyTriggered()
		{
			var docket = GetNewBusinessObject();
			AssertNotNull("Poke & Precondition", docket.PickUpDocAddress);

			AssertPersistentPropertiesHitCount("Getting PickUpDocAddress should not trigger property hits.", 0, () => _ = docket.PickUpDocAddress);

			var hits = GetPersistentPropertiesHitCount(() => _ = docket.PickUpDocAddress.ReadOnly);
			AssertEquals("Poking at ReadOnly property should trigger state calculation.", true, hits > 0);
		}

		#endregion

		#region TestDropOffDocAddress

		public virtual void TestDropOffDocAddress()
		{
			AssertNotNull("DropOff DocAddress should be set.", Docket.DropOffDocAddress);
			AssertEquals("DropOffPK should be empty", ZGuid.Empty, Docket.DropOffPK);
			AssertEquals("DropOffAddressPK should be empty", ZGuid.Empty, Docket.DropOffAddressPK);

			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress address = Factory.New<OrgAddress>();

			address = org.Addresses.MainAddress;
			Docket.DropOffPK = org.PK;

			AssertEquals("DropOff DocAddress should indirectly point to new Org.", org.PK, Docket.DropOffDocAddress.OrganisationPK);
			AssertEquals("DropOffPK should indirectly point to new Org.", org.PK, Docket.DropOffPK);
			AssertEquals("DropOff DocAddress should indirectly point to new Org Address.", address.PK, Docket.DropOffDocAddress.E2_OA_Address);
			AssertEquals("DropOff AddressPK should indirectly point to new Org Address.", address.PK, Docket.DropOffAddressPK);

			Docket.DropOffDocAddress.E2_AddressOverride = true;
			AssertEquals("DropOffPK should be empty", ZGuid.Empty, Docket.DropOffPK);
			AssertEquals("DropOffAddressPK should be empty", ZGuid.Empty, Docket.DropOffAddressPK);
		}

		public void TestDropOffDocAddress_ReadOnlyIsLazyTriggered()
		{
			var docket = GetNewBusinessObject();
			AssertNotNull("Poke & Precondition", docket.DropOffDocAddress);

			AssertPersistentPropertiesHitCount("Getting DropOffDocAddress should not trigger property hits.", 0, () => _ = docket.DropOffDocAddress);

			var hits = GetPersistentPropertiesHitCount(() => _ = docket.DropOffDocAddress.ReadOnly);
			AssertEquals("Poking at ReadOnly property should trigger state calculation.", true, hits > 0);
		}

		#endregion

		#region TestTransportBillTo

		public void TestTransportBillTo()
		{
			var newDocket = Factory.NewWithValidTestData<TDocket>();
			AssertNotNull(newDocket.TransportBillToDocAddress);
			AssertEquals("Should not create a new DocAddress on each access.", newDocket.TransportBillToDocAddress, newDocket.TransportBillToDocAddress);
			AssertNull(newDocket.TransportBillTo);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			newDocket.TransportBillToDocAddress.OrganisationPK = org.PK;
			newDocket.TransportBillToDocAddress.Address.OA_Address1 = "address 1";
			AssertEquals(org, newDocket.TransportBillTo);
			Factory.Save();

			var loadedDocket = Factory.Load<TDocket>(newDocket.PK);
			AssertEquals("address 1", loadedDocket.TransportBillToDocAddress.Address.OA_Address1);
			AssertEquals(org, loadedDocket.TransportBillTo);

			loadedDocket.TransportBillToDocAddress.E2_AddressOverride = true;
			AssertNull(loadedDocket.TransportBillTo);
		}

		public void TestTransportBillToDocAddress_ReadOnlyIsLazyTriggered()
		{
			var docket = GetNewBusinessObject();
			AssertNotNull("Poke & Precondition", docket.TransportBillToDocAddress);

			AssertPersistentPropertiesHitCount("Getting TransportBillToDocAddress should not trigger property hits.", 0, () => _ = docket.TransportBillToDocAddress);

			var hits = GetPersistentPropertiesHitCount(() => _ = docket.TransportBillToDocAddress.ReadOnly);
			AssertEquals("Poking at ReadOnly property should trigger state calculation (if there is one).", true, hits >= 0);
		}

		#endregion

		#endregion

		#endregion

		#region TestAccTranLines

		public void TestAccTranLines()
		{
			JobHeader jobHeader = Helper.AddJobToDocket(Docket);

			AccTransactionLines line1 = Factory.New<AccTransactionLines>();
			AccTransactionLines line2 = Factory.New<AccTransactionLines>();
			line1.AL_JH = jobHeader.PK;
			line2.AL_JH = jobHeader.PK;
			line1.AL_GC = GlbCompany.CurrentCompany.PK;
			line2.AL_GC = GlbCompany.CurrentCompany.PK;

			AssertEquals(2, Docket.AccTranLines.Count);
			AssertCollectionContains(line1, Docket.AccTranLines);
			AssertCollectionContains(line2, Docket.AccTranLines);

			AccTransactionLines line3 = Factory.New<AccTransactionLines>();
			line3.AL_JH = jobHeader.PK;
			line3.AL_GC = GlbCompany.CurrentCompany.PK;
			AssertEquals("Should be cached", 2, Docket.AccTranLines.Count);

			Docket.ClearAccTranLinesCache();
			AssertEquals(3, Docket.AccTranLines.Count);
		}

		#endregion

		#region TestWarehouse

		public virtual void TestWarehouse()
		{
			AssertEquals("Precondition", null, Docket.Warehouse);

			WhsWarehouse whs = Helper.CreateWarehouse("1");
			Docket.WD_WW_Whs = whs.PK;

			AssertEquals(whs, Docket.Warehouse);
		}

		#endregion

		#region TestReferences

		public virtual void TestReferences()
		{
			var docket1 = GetNewBusinessObject();

			WhsDocketReference @ref = Factory.New<WhsDocketReference>();
			WhsDocketReference ref1 = Factory.New<WhsDocketReference>();

			@ref.WX_WD = Docket.PK;
			ref1.WX_WD = docket1.PK;

			AssertEquals(true, Docket.IsRegisteredEditableChildObject(Docket.References));
			AssertEquals(1, Docket.References.Count);
			AssertEquals(1, docket1.References.Count);
			AssertCollectionContains(@ref, Docket.References);
			AssertCollectionContains(ref1, docket1.References);
			AssertCollectionNotContains(ref1, Docket.References);
			AssertCollectionNotContains(@ref, docket1.References);
		}

		#endregion

		#region TestContainers

		public virtual void TestContainers()
		{
			var docket1 = GetNewBusinessObject();
			WhsDocketContainer con = Factory.New<WhsDocketContainer>();
			WhsDocketContainer con1 = Factory.New<WhsDocketContainer>();

			con.WC_WD = Docket.PK;
			con1.WC_WD = docket1.PK;

			AssertEquals(true, Docket.IsRegisteredEditableChildObject(Docket.Containers));
			AssertEquals(1, Docket.Containers.Count);
			AssertEquals(1, docket1.Containers.Count);
			AssertCollectionContains(con, Docket.Containers);
			AssertCollectionContains(con1, docket1.Containers);
			AssertCollectionNotContains(con1, Docket.Containers);
			AssertCollectionNotContains(con, docket1.Containers);
		}

		#endregion

		#region TestPallets

		public virtual void TestPallets()
		{
			var docket1 = GetNewBusinessObject();

			WhsDocketPallet pallet = Factory.New<WhsDocketPallet>();
			WhsDocketPallet pallet1 = Factory.New<WhsDocketPallet>();

			pallet.W2_WD = Docket.PK;
			pallet1.W2_WD = docket1.PK;

			AssertEquals(true, Docket.IsRegisteredEditableChildObject(Docket.Pallets));
			AssertEquals(1, Docket.Pallets.Count);
			AssertEquals(1, docket1.Pallets.Count);
			AssertCollectionContains(pallet, Docket.Pallets);
			AssertCollectionContains(pallet1, docket1.Pallets);
			AssertCollectionNotContains(pallet1, Docket.Pallets);
			AssertCollectionNotContains(pallet, docket1.Pallets);
		}

		#endregion

		#region TestJobHeader

		public virtual void TestJobHeader()
		{
			AssertEquals("Initially no job.", null, Docket.JobHeader);
			JobHeader job = Helper.AddJobToDocket(Docket);
			AssertEquals("JobHeader", job.PK, Docket.JobHeader.PK);
		}

		#endregion

		#region TestEventsProxyFromJobHeader

		public virtual void TestEventsProxyFromJobHeader()
		{
			JobHeader job = Helper.AddJobToDocket(Docket);
			ArrayList relatedLogObjects = new ArrayList(Docket.BusinessObjectsWithRelatedEvents);
			AssertEquals("RelatedBusinessObjectsWithLogs should container JobHeader", true, relatedLogObjects.Contains(Docket.JobHeader));
		}

		#endregion

		#region TestGetClientCode

		public void TestGetClientCode()
		{
			AssertEquals("", Docket.GetClientCode(OrgCusCode.CodeTypes.CustomsCPPermitCode));
			Docket.WD_OH_Client = Helper.CreateClient().PK;
			Docket.Client.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsCPPermitCode, "1234", GlbBranch.CurrentBranch.Country);
			AssertEquals("1234", Docket.GetClientCode(OrgCusCode.CodeTypes.CustomsCPPermitCode));
		}

		#endregion

		#region TestInventoryFilter

		public void TestInventoryFilter()
		{
			var inv = Factory.New<WhsInventoryView>();
			AssertEquals("Collection should not be loaded", 0, Docket.InventoryFilter.Count);
			Assert(!Docket.IsRegisteredEditableChildObject(Docket.InventoryFilter));
		}

		#endregion

		#region InventoryToPrintPalletLabelFor

		public void TestInventoryToPrintPalletLablelFor()
		{
			AssertNull(Docket.InventoryToPrintPalletLabelFor);

			Docket.InventoryToPrintPalletLabelFor = Factory.New<WhsInventoryView>();
			AssertNotNull(Docket.InventoryToPrintPalletLabelFor);
		}

		#endregion

		#region TestDocketDeviceStrategy

		public void TestDocketDeviceStrategy()
		{
			var docket1 = GetNewBusinessObject();
			AssertEquals("When run by Enterprise", typeof(WhsDocketRelatedEntityOperationsStrategy), docket1.DocketRelatedEntityOperationsStrategy.GetType());

			Globals.IsWeb = true;
			try
			{
				var docket2 = GetNewBusinessObject();
				AssertEquals("When run by WebTracker", typeof(WhsDocketRelatedEntityOperationsStrategy), docket2.DocketRelatedEntityOperationsStrategy.GetType());

				Globals.IsUserInteractive = false;
				var docket3 = GetNewBusinessObject();
				AssertEquals("When run by RF web service", typeof(WhsDocketRelatedEntityOperationsRFStrategy), docket3.DocketRelatedEntityOperationsStrategy.GetType());

				Globals.IsWeb = false;
				var docket4 = GetNewBusinessObject();
				AssertEquals("When run by Service Tasks", typeof(WhsDocketRelatedEntityOperationsStrategy), docket4.DocketRelatedEntityOperationsStrategy.GetType());
			}
			finally // clean up
			{
				Globals.IsWeb = false;
				Globals.IsUserInteractive = true;
			}
		}

		#endregion

		#endregion

		#region Validation

		public void TestGetNewValidation()
		{
			AssertNotNull(Docket.Validation);
			AssertEquals(GetExpectedValidationType(), Docket.Validation.GetType());
		}

		protected abstract Type GetExpectedValidationType();

		#endregion

		#region Lookups

		public void TestGetNewLookups()
		{
			AssertNotNull(Docket.Lookups);
			AssertEquals(GetExpectedLookupsType(), Docket.Lookups.GetType());
		}

		protected abstract Type GetExpectedLookupsType();

		#endregion

		#region Properties

		#region Total Order Value

		#region TestWD_TotalOrderValue

		public void TestWD_TotalOrderValue()
		{
			var docket = Factory.New<WhsOrder>();
			var line1 = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();

			SetLineOrderValue(line1, 20);
			SetLineOrderValue(line2, 20);
			AssertEquals("The Total Order Value is updated to the total on the Lines.", 40m, docket.WD_TotalOrderValue);

			SetLineOrderValue(line1, 40);
			SetLineOrderValue(line2, 30);
			AssertEquals("The Total Order Value is updated to the total on the Lines.", 70m, docket.WD_TotalOrderValue);

			docket.WD_TotalOrderValue = 20;
			SetLineOrderValue(line1, 20);
			SetLineOrderValue(line2, 20);
			AssertEquals("The Total Order Value is not Equal, so do not update Order Total.", 20m, docket.WD_TotalOrderValue);
		}

		void SetLineOrderValue(WhsDocketLine line, ZDecimal price)
		{
			line.WE_ExtendedLinePrice = price;
		}

		#endregion

		#region TestWD_TotalOrderCurr_Defaults

		public void TestWD_TotalOrderCurr_Defaults()
		{
			var docket = Factory.New<WhsOrder>();
			AssertEquals("Default WD_TotalOrderValue", 0m, docket.WD_TotalOrderValue);
			AssertEquals("Default WD_RX_NKTotalOrderCurrency", "", docket.WD_RX_NKTotalOrderCurrency);
		}

		#endregion

		#region TestWD_TotalOrderCurr_EnterTotalOrderValue

		public void TestWD_TotalOrderCurr_EnterTotalOrderValue()
		{
			var docket = Factory.New<WhsOrder>();
			docket.WD_TotalOrderValue = 10;
			AssertEquals("Set Cur to Company Cur", "AUD", docket.WD_RX_NKTotalOrderCurrency);

			docket.WD_RX_NKTotalOrderCurrency = "NZD";
			AssertEquals("Keep new cur", "NZD", docket.WD_RX_NKTotalOrderCurrency);
			AssertEquals("Keep existing value", 10m, docket.WD_TotalOrderValue);
		}

		#endregion

		#region TestWD_TotalOrderCurr_EnterLineOrderValue

		public void TestWD_TotalOrderCurr_EnterLineOrderValue()
		{
			var docket = Factory.New<WhsOrder>();
			var line = docket.Lines.AddNew();
			line.WE_ExtendedLinePrice = 10;
			AssertEquals("Line sets total on order", 10m, docket.WD_TotalOrderValue);
			AssertEquals("Setting total on order sets to common Cur on lines, because order cur is blank", "AUD", docket.WD_RX_NKTotalOrderCurrency);

			var line2 = docket.Lines.AddNew();
			line2.WE_ExtendedLinePrice = 10;
			AssertEquals("Line sets total on order", 20m, docket.WD_TotalOrderValue);
			AssertEquals("Setting total on order should not change the order cur", "AUD", docket.WD_RX_NKTotalOrderCurrency);

			docket.WD_RX_NKTotalOrderCurrency = "NZD";
			AssertEquals("Value should not be affected by changing the Cur", 20m, docket.WD_TotalOrderValue);
			AssertEquals("New cur entered", "NZD", docket.WD_RX_NKTotalOrderCurrency);

			line.WE_ExtendedLinePrice = 20;
			AssertEquals("Order Cur and Common Line Cur do not match, so don't update the order.", 20m, docket.WD_TotalOrderValue);
			AssertEquals("Cur not affected", "NZD", docket.WD_RX_NKTotalOrderCurrency);
		}

		#endregion

		#region TestWD_TotalOrderCurr_EnterLineOrderValue_OrderAUD_Line1NZD_Line2AUD

		public void TestWD_TotalOrderCurr_EnterLineOrderValue_OrderAUD_Line1NZD_Line2AUD()
		{
			var docket = Factory.New<WhsOrder>();
			var line = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();
			line.WE_ExtendedLinePrice = 10;
			line2.WE_ExtendedLinePrice = 10;
			AssertEquals("Updates Order", 20m, docket.WD_TotalOrderValue);
			AssertEquals("No cur, so sets to current company cur", "AUD", docket.WD_RX_NKTotalOrderCurrency);

			line.WE_RX_NKUnitPriceCurrency = "NZD";
			AssertEquals("No change", 20m, docket.WD_TotalOrderValue);
			AssertEquals("No change", "AUD", docket.WD_RX_NKTotalOrderCurrency);

			line2.WE_ExtendedLinePrice = 20;
			AssertEquals("No change", 20m, docket.WD_TotalOrderValue);
			AssertEquals("No change", "AUD", docket.WD_RX_NKTotalOrderCurrency);

			line.WE_ExtendedLinePrice = 20;
			AssertEquals("No change", 20m, docket.WD_TotalOrderValue);
			AssertEquals("No change", "AUD", docket.WD_RX_NKTotalOrderCurrency);
		}

		#endregion

		#region TestWD_TotalOrderCurr_ChangeLineCurWhenInSync

		public void TestWD_TotalOrderCurr_ChangeLineCurWhenInSync()
		{
			var docket = Factory.New<WhsOrder>();
			var line = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();
			line.WE_ExtendedLinePrice = 10;
			line2.WE_ExtendedLinePrice = 10;
			AssertEquals("Updates Order", 20m, docket.WD_TotalOrderValue);
			AssertEquals("No cur, so sets to current company cur", "AUD", docket.WD_RX_NKTotalOrderCurrency);

			docket.WD_RX_NKTotalOrderCurrency = "NZD";
			line.WE_RX_NKUnitPriceCurrency = "NZD";
			line2.WE_RX_NKUnitPriceCurrency = "NZD";
			AssertEquals("No change", 20m, docket.WD_TotalOrderValue);
			AssertEquals("Now NZD", "NZD", docket.WD_RX_NKTotalOrderCurrency);
			AssertEquals("Now NZD", "NZD", line.WE_RX_NKUnitPriceCurrency);
			AssertEquals("Now NZD", "NZD", line2.WE_RX_NKUnitPriceCurrency);

			line.WE_ExtendedLinePrice = 20;
			AssertEquals("10+20 = 30", 30m, docket.WD_TotalOrderValue);
			AssertEquals("No change", "NZD", docket.WD_RX_NKTotalOrderCurrency);
			AssertEquals("No change", "NZD", line.WE_RX_NKUnitPriceCurrency);
			AssertEquals("No change", "NZD", line2.WE_RX_NKUnitPriceCurrency);

			line.WE_RX_NKUnitPriceCurrency = "AUD";
			AssertEquals("No change", 30m, docket.WD_TotalOrderValue);
			AssertEquals("No change", "NZD", docket.WD_RX_NKTotalOrderCurrency);
			AssertEquals("Now AUD", "AUD", line.WE_RX_NKUnitPriceCurrency);
			AssertEquals("No change", "NZD", line2.WE_RX_NKUnitPriceCurrency);
		}

		#endregion

		#region TestGetCommonValidLineCurrency

		public void TestGetCommonValidLineCurrency_NoCurrency()
		{
			var docket = Docket;
			_ = docket.Lines.AddNew();
			_ = docket.Lines.AddNew();

			AssertEquals("Docket with no currencies should return empty string", string.Empty, docket.GetCommonValidLineCurrency());
		}

		public void TestGetCommonValidLineCurrency_OneCurrency()
		{
			var docket = Docket;
			_ = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();
			line2.WE_RX_NKUnitPriceCurrency = "AUD";

			AssertEquals("Docket with one currencies should return currency", "AUD", docket.GetCommonValidLineCurrency());
		}

		public void TestGetCommonValidLineCurrency_TwoCurrencies()
		{
			var docket = Docket;
			var line1 = docket.Lines.AddNew();
			line1.WE_RX_NKUnitPriceCurrency = "USD";

			var line2 = docket.Lines.AddNew();
			line2.WE_RX_NKUnitPriceCurrency = "AUD";

			AssertEquals("Docket with two or more currencies should return empty string", string.Empty, docket.GetCommonValidLineCurrency());
		}

		#endregion

		#endregion

		#region Carrier Service Level

		#region TestCarrierServiceLevel

		public void TestCarrierServiceLevel()
		{
			var docket = GetNewBusinessObject();
			AssertNull("Precondition", docket.CarrierServiceLevel);

			if (SupportsCarrierServiceLevel(docket))
			{
				var job = (IJobWithTransportCompany)docket;
				var transportCo = Factory.New<OrgHeader>();
				job.TransportCoPK = transportCo.PK;
				docket.WD_PL_NKCarrierServiceLevel = "XXX";
				AssertNull(docket.CarrierServiceLevel);

				var service = transportCo.MiscServ.CarrierServiceLevels.AddNew();
				service.PL_Code = "XXX";
				AssertEquals(service.PK, docket.CarrierServiceLevel.PK);

				service.PL_Code = "xxx";
				AssertEquals(service.PK, docket.CarrierServiceLevel.PK);

				docket.WD_PL_NKCarrierServiceLevel = "ZZZ";
				AssertNull(docket.CarrierServiceLevel);
			}
		}

		protected virtual bool SupportsCarrierServiceLevel(TDocket docket) => docket is IJobWithTransportCompany;

		#endregion

		#region TestCarrierServiceLevel_Standard

		public void TestCarrierServiceLevel_Standard()
		{
			var docket = GetNewBusinessObject();
			AssertNull("Precondition", docket.CarrierServiceLevel);

			if (SupportsCarrierServiceLevel(docket))
			{
				docket.FillWithValidTestData();

				var job = (IJobWithTransportCompany)docket;
				job.TransportCoPK = Factory.NewWithValidTestData<OrgHeader>().PK;
				AssertNull("No service level set", docket.CarrierServiceLevel);

				docket.WD_PL_NKCarrierServiceLevel = "STD";
				AssertNotNull(docket.CarrierServiceLevel);
				AssertEquals("Standard", docket.CarrierServiceLevel.PL_CarrierServiceLevelDescription);
				Factory.Save();

				var freshFactory = new BusinessObjectFactory();
				var docketInFreshFactory = freshFactory.Load<TDocket>(docket.PK);
				AssertNotNull(docketInFreshFactory.CarrierServiceLevel);
				AssertEquals("Standard", docketInFreshFactory.CarrierServiceLevel.PL_CarrierServiceLevelDescription);
			}
			else
			{
				docket.WD_PL_NKCarrierServiceLevel = "STD";
				AssertNull(docket.CarrierServiceLevel);
			}
		}

		#endregion

		#endregion

		#region TestServiceLevel

		public void TestServiceLevel()
		{
			AssertNull("Precondition", Docket.ServiceLevel);
			Docket.WD_RS_NKServiceLevel = "XXX";
			AssertNull(Docket.ServiceLevel);

			var serviceLevel = Docket.Lookups.ServiceLevels.AddNew();
			serviceLevel.RS_Code = "XXX";
			AssertEquals(serviceLevel.PK, Docket.ServiceLevel.PK);

			Docket.WD_RS_NKServiceLevel = "ZZZ";
			AssertNull(Docket.ServiceLevel);
		}

		#endregion

		#region WhsOrderFulfillmentRule

		public void TestWD_WhsOrderFulfillmentRule()
		{
			if (SupportsFullfillmentRule)
			{
				AssertEquals("Precondition", "", Docket.WD_WhsOrderFulfillmentRule);

				Docket.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
				AssertEquals("ALL", Docket.WD_WhsOrderFulfillmentRule);
			}
			else
			{
				bool noException = false;
				try
				{
					Docket.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
					noException = true;
				}
				catch (NotSupportedException e)
				{
					AssertEquals(string.Format("Docket type {0} does not support setting {1}.", Docket.GetType().Name, Docket.WD_WhsOrderFulfillmentRuleInfo.Name), e.Message);
				}
				if (noException)
				{
					Fail("Not Supported Exception should be thrown.");
				}
			}
		}

		protected virtual bool SupportsFullfillmentRule
		{
			get { return false; }
		}

		#endregion

		#region TestDescription

		public void TestDescription()
		{
			AssertEquals(ExpectedDescription, GetNewBusinessObject().Description);
		}

		protected abstract ZString ExpectedDescription { get; }

		#endregion

		#region TestIsPostFinalizeEditAllowed

		public virtual void TestIsPostFinalizeEditAllowed()
		{
			AssertEquals(false, Docket.IsPostFinalizeEditAllowed);
		}

		#endregion

		#region TestCalculateTotalsEnabled

		public void TestCalculateTotalsEnabled()
		{
			TestCalculateTotalsEnabledCore();
		}

		protected virtual void TestCalculateTotalsEnabledCore()
		{
			var docket = GetNewBusinessObject();
			AssertEquals(true, docket.CalculateTotalsEnabled);
			docket.CalculateTotalsEnabled = false;
			AssertEquals(false, docket.CalculateTotalsEnabled);
		}

		#endregion

		#region TestClientName

		public void TestClientName()
		{
			var org = Helper.CreateClient();
			org.OH_FullName = "NAME";
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;

			AssertEquals("NAME", docket.ClientName);
		}

		#endregion

		#region TestTransportZone

		public virtual void TestTransportZone()
		{
			// Sub-classes that use transport zone should override this
			Assert(true);
		}

		public virtual void TestTransportZoneName()
		{
			// Sub-classes that use transport zone should override this
			Assert(true);
		}

		#endregion

		#region TestTransportCo

		public void TestTransportCo()
		{
			var docket = GetNewBusinessObject();
			if (docket is IJobWithTransportCompany job)
			{
				var org = Factory.New<OrgHeader>();
				org.OH_FullName = "TRANSPORT CO";

				job.TransportCoPK = org.PK;
				AssertEquals(org, docket.TransportCo);

				job.TransportCoDocAddress.E2_AddressOverride = true;
				job.TransportCoDocAddress.E2_CompanyName = "OVERRIDEN TRANSPORT CO";
				AssertNull(docket.TransportCo);
			}
			else
			{
				AssertNull(docket.TransportCo);
			}
		}

		#endregion

		#region TestTransportCoName

		public void TestTransportCoName()
		{
			var docket = GetNewBusinessObject();
			if (docket is IJobWithTransportCompany job)
			{
				var org = Factory.New<OrgHeader>();
				org.OH_FullName = "TRANSPORT CO";

				job.TransportCoPK = org.PK;
				AssertEquals("TRANSPORT CO", docket.TransportCoName);

				job.TransportCoDocAddress.E2_AddressOverride = true;
				job.TransportCoDocAddress.E2_CompanyName = "OVERRIDEN TRANSPORT CO";
				AssertEquals("OVERRIDEN TRANSPORT CO", docket.TransportCoName);
			}
			else
			{
				AssertExceptionThrown<NotSupportedException>(() => _ = docket.TransportCoName);
			}
		}

		#endregion

		#region TestTransportCoPK

		public void TestTransportCoPK()
		{
			var docket = GetNewBusinessObject();
			if (docket is IJobWithTransportCompany job)
			{
				var transportCo = Factory.New<OrgHeader>();
				transportCo.OH_FullName = "TRANSPORT CO";

				int validationHitCount = 0;
				job.TransportCoDocAddress.OrganisationPKInfo.AdditionalValidation += () => validationHitCount++;
				job.TransportCoPK = transportCo.PK;
				AssertEquals("TRANSPORT CO", docket.TransportCoName);
				AssertEquals("Should have Validated OrganisationPK (setting OrganisationPK Validates both OrganisationPK and OrganisationNameOrPK).", 2, validationHitCount);

				using (docket.GetValidationSuspender())
				{
					job.TransportCoPK = Factory.New<OrgHeader>().PK;
					AssertEquals("Should not have Validated OrganisationPK while Validation is suspended on Docket.", 2, validationHitCount);
				}

				using (job.TransportCoDocAddress.GetValidationSuspender())
				{
					job.TransportCoPK = transportCo.PK;
					AssertEquals("Should not have Validated OrganisationPK while Validation is suspended on JobDocAddress.", 2, validationHitCount);
				}
			}
			else
			{
				AssertExceptionThrown<ArgumentException>(() => _ = docket[nameof(IJobWithTransportCompany.TransportCoPK)]);
			}
		}

		#endregion

		#region TestSupplierName

		public void TestSupplierName()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "SUPPLIER";
			var docket = GetNewBusinessObject();
			docket.SupplierDocAddress.OrganisationPK = org.PK;

			AssertEquals("SUPPLIER", docket.SupplierName);

			docket.SupplierDocAddress.E2_AddressOverride = true;
			docket.SupplierDocAddress.E2_CompanyName = "OVERRIDEN SUPPLIER";
			AssertEquals("OVERRIDEN SUPPLIER", docket.SupplierName);
		}

		#endregion

		#region TestContainerID

		public void TestContainerID()
		{
			var gP20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var gP40 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var docket = GetNewBusinessObject();
			var container1 = docket.Containers.AddNew();
			var container2 = docket.Containers.AddNew();

			container1.WC_RC = gP20.PK;
			container1.WC_ContainerNum = "C1";
			container2.WC_RC = gP40.PK;
			container2.WC_ContainerNum = "C2";
			AssertEquals("Many", docket.ContainerID);

			container2.WC_RC = gP20.PK;
			AssertEquals("C1, C2", docket.ContainerID);
		}

		#endregion

		#region TestContainerType

		public void TestContainerType()
		{
			var gP20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var gP40 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var docket = GetNewBusinessObject();
			var container1 = docket.Containers.AddNew();
			var container2 = docket.Containers.AddNew();

			container1.WC_RC = gP20.PK;
			container2.WC_RC = gP40.PK;
			AssertEquals("Many", docket.ContainerType);

			container2.WC_RC = gP20.PK;
			AssertEquals("20GP", docket.ContainerType);
		}

		#endregion

		#region TestContainerType_ContainerTypeEmpty

		[ExpectNoExceptions]
		public void TestContainerType_ContainerTypeEmpty()
		{
			var docket = GetNewBusinessObject();
			var container1 = docket.Containers.AddNew();
			AssertEquals("", docket.ContainerType);
		}

		#endregion

		#region TestWD_TotalUnitsFromLines_WhenLineRemoved

		public void TestWD_TotalUnitsFromLines_WhenLineRemoved()
		{
			if (IsWD_TotalUnitsFromLinesUpdatedOnLineRemove)
			{
				OrgHeader org = Helper.CreateClient();
				Docket.WD_OH_Client = org.PK;
				OrgSupplierPart part1 = Helper.CreateProduct(org, "PRODUCT1");

				WhsDocketLine line1 = Docket.Lines.AddNew();
				WhsDocketLine line2 = Docket.Lines.AddNew();
				WhsDocketLine line3 = Docket.Lines.AddNew();
				line1.WE_OP = part1.PK;
				line2.WE_OP = part1.PK;
				line3.WE_OP = part1.PK;
				line1.WE_TransactionQuantity = 2m;
				line2.WE_TransactionQuantity = 3m;
				line3.WE_TransactionQuantity = 4m;

				AssertEquals("Precondition", 9m, Docket.WD_TotalUnitsFromLines);

				Docket.Lines.Delete(line2);
				AssertEquals(6m, Docket.WD_TotalUnitsFromLines);

				Docket.Lines.RemoveFromRelationship(line1);
				AssertEquals(4m, Docket.WD_TotalUnitsFromLines);
			}
			else
			{
				Assert("Not supported.", true);
			}
		}

		protected virtual bool IsWD_TotalUnitsFromLinesUpdatedOnLineRemove
		{
			get { return true; }
		}

		#endregion

		#region Update Total Weight and Volume

		#region TestUpdateTotalWeightAndVolume

		public virtual void TestUpdateTotalWeightAndVolume()
		{
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1");

			part.OP_Weight = 2.0m;
			part.OP_Cubic = 0.02m;
			part.OP_WeightUQ = "KG";
			part.OP_CubicUQ = "M3";

			Docket.WD_TotalCubicUnit = Core.Constants.Volume.Litre;
			Docket.WD_TotalWeightUnit = Core.Constants.Weight.Pounds;
			var updateOn = Docket.ShouldUpdateWeightAndVolumeOnTheFly;

			Docket.UpdateTotalWeightAndVolume(part, 10m);
			AssertEquals("Total Weight is incorrect", updateOn ? 44.09m : 0m, Docket.WD_TotalWeight);
			AssertEquals("Total Volume is incorrect", updateOn ? 200m : 0m, Docket.WD_TotalCubic);

			Docket.UpdateTotalWeightAndVolume(part, 5m);
			AssertEquals("Total Weight is incorrect", updateOn ? 66.14m : 0m, Docket.WD_TotalWeight);
			AssertEquals("Total Volume is incorrect", updateOn ? 300m : 0m, Docket.WD_TotalCubic);

			Docket.UpdateTotalWeightAndVolume(part, 0m);
			AssertEquals("Should be no change because value is zero", updateOn ? 66.14m : 0m, Docket.WD_TotalWeight);
			AssertEquals("Should be no change because value is zero", updateOn ? 300m : 0m, Docket.WD_TotalCubic);

			Docket.UpdateTotalWeightAndVolume(part, -5m);
			AssertEquals("Total Weight is incorrect", updateOn ? 44.09m : 0m, Docket.WD_TotalWeight);
			AssertEquals("Total Volume is incorrect", updateOn ? 200m : 0m, Docket.WD_TotalCubic);

			Docket.UpdateTotalWeightAndVolume(null, -5m);
			AssertEquals("Should be no change because part is null", updateOn ? 44.09m : 0m, Docket.WD_TotalWeight);
			AssertEquals("Should be no change because part is null", updateOn ? 200m : 0m, Docket.WD_TotalCubic);

			Docket.WD_WeightVolSetFromImport = ZBool.True;

			Docket.UpdateTotalWeightAndVolume(part, -5m);
			AssertEquals("Should be no change because set from import", updateOn ? 44.09m : 0m, Docket.WD_TotalWeight);
			AssertEquals("Should be no change because set from import", updateOn ? 200m : 0m, Docket.WD_TotalCubic);
			Docket.UpdateTotalWeightAndVolume(part, 5m);
			AssertEquals("Should be no change because set from import", updateOn ? 44.09m : 0m, Docket.WD_TotalWeight);
			AssertEquals("Should be no change because set from import", updateOn ? 200m : 0m, Docket.WD_TotalCubic);

			Docket.WD_WeightVolSetFromImport = ZBool.False;

			Docket.UpdateTotalWeightAndVolume(part, -100m);
			AssertEquals("Should never go below zero", 0m, Docket.WD_TotalWeight);
			AssertEquals("Should never go below zero", 0m, Docket.WD_TotalCubic);
		}

		#endregion

		#region TestShouldUpdateWeightAndVolumeOnTheFly

		public virtual void TestShouldUpdateWeightAndVolumeOnTheFly()
		{
			Docket.WD_WeightVolSetFromImport = true;
			AssertEquals(false, Docket.ShouldUpdateWeightAndVolumeOnTheFly);

			Docket.WD_WeightVolSetFromImport = false;
			AssertEquals(true, Docket.ShouldUpdateWeightAndVolumeOnTheFly);

			using (new SemaphoreManager(Docket.UpdatingWeightAndVolumeSemaphore))
			{
				AssertEquals(false, Docket.ShouldUpdateWeightAndVolumeOnTheFly);
			}

			AssertEquals(true, Docket.ShouldUpdateWeightAndVolumeOnTheFly);
		}

		#endregion

		#region TestUpdateTotalWeightVolume_DecimalOverflowException

		public void TestUpdateTotalWeightVolume_DecimalOverflowException()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 1000m, Constants.Weight.Kilograms, 2000m, Constants.Volume.CubicMetres);

			var docket = CreateWhsDocket(data.Org1, data.Whs1, "R1", isRefCreatedOnSave: true);
			if (docket.ShouldUpdateWeightAndVolumeOnTheFly)
			{
				docket.WD_TotalWeightUnit = Constants.Weight.Kilograms;
				docket.WD_TotalCubicUnit = Constants.Volume.CubicMetres;
				docket.WD_TotalWeight = 10m;
				docket.WD_TotalCubic = 20m;
				AssertEquals("Precondition", 10m, docket.WD_TotalWeight);
				AssertEquals("Precondition", 20m, docket.WD_TotalCubic);

				AssertNoExceptionThrown(() => docket.UpdateTotalWeightAndVolume(data.Part1, decimal.MaxValue));
				AssertEquals("WD_TotalWeight should not be updated when over-flowing.", 10m, docket.WD_TotalWeight);
				AssertEquals("WD_TotalCubic should not be updated when over-flowing.", 20m, docket.WD_TotalCubic);
				AssertHasRowError(docket, "Attempt to overflow capacity of Weight. Please validate your setup and restart the process.");
				AssertHasRowError(docket, "Attempt to overflow capacity of Volume. Please validate your setup and restart the process.");
			}
			else
			{
				Assert("Total Weight / Volume is not calculated for the Docket Type.", true);
			}
		}

		#endregion

		#endregion

		#region TestCancelDocket

		public virtual void TestCancelDocket()
		{
			Docket = GetNewBusinessObject();

			CodeDescriptionPairList availableStatuses = new DocketStatus();
			foreach (CodeDescriptionPair status in availableStatuses)
			{
				Docket.WD_DocketStatus = DocketStatus.Codes.Entered;
				Docket.WD_DocketStatus = status.Code;
				Docket.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				Docket.CancelReactivateDocket();
				if (status.Code == DocketStatus.Codes.New || status.Code == DocketStatus.Codes.Entered || status.Code == DocketStatus.Codes.Held || status.Code == DocketStatus.Codes.Error)
				{
					AssertEquals("Should cancel docket with " + status.Description + " status", true, Docket.IsCancelled);
					AssertEquals("Should change status to Canceled for docket with " + status.Description + " status", DocketStatus.Codes.Cancelled, Docket.WD_DocketStatus);
					AssertEquals("Docket can be canceled with status: " + status.Code, true, WhsDocket.CanCancel(status.Code));
					AssertEquals("Should cancel docket with WD_GS_NKCanceledBy", GlbStaff.CurrentUser.GS_Code, Docket.WD_GS_NKCanceledBy);
					AssertEquals("Should cancel docket with WD_CanceledTimeUtc", true, Docket.WD_CanceledTimeUtc.IsValid);
					AssertEquals("Should re-active docket and clear WD_TaskPlanningStatus", string.Empty, Docket.WD_TaskPlanningStatus);
				}
				else if (status.Code == DocketStatus.Codes.Cancelled)
				{
					AssertEquals("Should re-active docket with " + status.Description + " status", false, Docket.IsCancelled);
					AssertEquals("Should change status to Entered for docket with " + status.Description + " status", DocketStatus.Codes.Entered, Docket.WD_DocketStatus);
					AssertEquals("Docket can be canceled with status: " + status.Code, true, WhsDocket.CanCancel(status.Code));
					AssertEquals("Should re-active docket and clear WD_GS_NKCanceledBy", true, Docket.WD_GS_NKCanceledBy.IsEmpty);
					AssertEquals("Should re-active docket and clear WD_CanceledTimeUtc", false, Docket.WD_CanceledTimeUtc.IsValid);
					AssertEquals("Should re-active docket and clear WD_TaskPlanningStatus", TaskPlanningStatus.Codes.Ready, Docket.WD_TaskPlanningStatus);
				}
				else
				{
					AssertEquals("Should not cancel docket with " + status.Description + " status", false, Docket.IsCancelled);
					AssertEquals("Should keep original status after attemp to cancel for docket with " + status.Description + " status", status.Code, Docket.WD_DocketStatus);
					AssertEquals("Docket cannot be canceled with status: " + status.Code, false, WhsDocket.CanCancel(status.Code));
					AssertEquals("Should not have WD_GS_NKCanceledBy", true, Docket.WD_GS_NKCanceledBy.IsEmpty);
					AssertEquals("Should not have WD_CanceledTimeUtc", false, Docket.WD_CanceledTimeUtc.IsValid);
					AssertEquals("Should re-active docket and clear WD_TaskPlanningStatus", TaskPlanningStatus.Codes.Ready, Docket.WD_TaskPlanningStatus);
				}
			}
		}

		public void TestUnCancelDocketShouldClearCanceledColumns()
		{
			var docket = GetNewBusinessObject();
			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals("Should cancel docket with WD_GS_NKCanceledBy", GlbStaff.CurrentUser.GS_Code, docket.WD_GS_NKCanceledBy);
			AssertEquals("Should cancel docket with WD_CanceledTimeUtc", true, docket.WD_CanceledTimeUtc.IsValid);
			docket.WD_DocketStatus = DocketStatus.Codes.New;
			AssertEquals("Should not have WD_GS_NKCanceledBy", true, Docket.WD_GS_NKCanceledBy.IsEmpty);
			AssertEquals("Should not have WD_CanceledTimeUtc", false, Docket.WD_CanceledTimeUtc.IsValid);
		}

		#region TestCancelDocketUpdateDocketLineStatus

		public void TestCancelDocketUpdateDocketLineStatus()
		{
			var docket = GetNewBusinessObject();
			var docketLine = docket.Lines.AddNew();
			var lineStatus = docketLine.WE_DocketLineStatus;
			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals("Cancelled status should be propagaded down to lines by default.", DocketLineStatus.Codes.Cancelled, docketLine.WE_DocketLineStatus);
			AssertEquals("Should cancel docket with WD_GS_NKCanceledBy", GlbStaff.CurrentUser.GS_Code, docket.WD_GS_NKCanceledBy);
			AssertEquals("Should cancel docket with WD_CanceledTimeUtc", true, docket.WD_CanceledTimeUtc.IsValid);

			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals("Original docketline status should be propagaded down to lines", lineStatus, docketLine.WE_DocketLineStatus);
		}

		#endregion

		#endregion

		#region TestCountryCode

		public void TestCountryCode()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("WHS");
			Docket.WD_WW_Whs = whs.PK;

			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Core.Constants.CountryCodes.Australia).RL_Code;
			AssertEquals(Core.Constants.CountryCodes.Australia, Docket.CountryCode);

			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Core.Constants.CountryCodes.UnitedStates).RL_Code;
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, Docket.CountryCode);

			Docket.WD_WW_Whs = ZGuid.Empty;
			AssertEquals(ZString.Empty, Docket.CountryCode);
		}

		#endregion

		#region TestWD_ExternalReference

		public void TestWD_ExternalReference_TrimsWhitespace()
		{
			var docket = GetNewBusinessObject();

			docket.WD_ExternalReference = "    SomeReference    ";
			AssertEquals("SomeReference", docket.WD_ExternalReference);
		}

		#endregion

		#region TestWD_DocketStatus

		public virtual void TestWD_DocketStatus()
		{
			OnDocketStatusChangedCalled = false;
			var docket = GetNewBusinessObject();
			docket.DocketStatusChanged += OnDocketStatusChangedTest;

			AssertEquals("Precondition", false, docket.WD_OH_ClientInfo.ReadOnly);
			AssertEquals("Precondition", DocketStatus.Codes.New, docket.WD_DocketStatus);

			docket.WD_FinalisedDate = ZDateTimeOffset.Now;
			docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
			AssertEquals(true, OnDocketStatusChangedCalled);

			// ensure OnReadOnlyChange() is called
			AssertEquals(true, docket.WD_OH_ClientInfo.ReadOnly);
		}

		#endregion

		#region TestWD_FinalisedDate_TruncatesMilliseconds

		[TestDate(2018, 7, 5, 8, 32, 17, 234)]
		public void TestWD_FinalisedDate_TruncatesMilliseconds()
		{
			var docket = SetupForTestFinaliseDocket();
			Factory.Save(); // need to save as some finalise dockets use a 2nd factory
			docket.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(docket);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var docketInNewFactory = newFactory.Load<TDocket>(docket.PK);
			AssertEquals("WD_FinalisedDate should contain seconds.",
				docket.Warehouse.GetWarehouseBranchDateTimeOffset(new ZDateTime(2018, 7, 5, 08, 32, 17, 000)),
				docketInNewFactory.WD_FinalisedDate);
		}

		#endregion

		#region TestWD_FinalisedDatePropagatesDown

		public void TestWD_FinalisedDatePropagatesDown()
		{
			var docket = GetNewBusinessObject();
			if (docket.PropagatesFinalisedDateAndStatusToLines)
			{
				var docketLine = docket.Lines.AddNew();
				docket.WD_FinalisedDate = ZDateTimeOffset.Now;
				AssertEquals("Finalised Date should be propagaded down to lines", docket.WD_FinalisedDate, docketLine.WE_FinalisedDate);

				docket.WD_FinalisedDate = ZDateTimeOffset.Empty;
				AssertEquals("Empty Finalised Date should be propagaded down to lines", ZDateTimeOffset.Empty, docketLine.WE_FinalisedDate);

				docketLine.Inventory.ForEach(i => i.ClearHasChanges());
				docketLine.ClearHasChanges();
				docket.WD_FinalisedDate = ZDateTimeOffset.Now;
				AssertEquals(docket.WD_FinalisedDate, docketLine.WE_FinalisedDate);
				AssertEquals("Should set HasChanges while propagating", true, docketLine.HasChanges);
			}
			else
			{
				var docketLine = docket.Lines.AddNew();
				docket.WD_FinalisedDate = ZDateTimeOffset.Now;
				AssertEquals("Docket line date should remain empty", ZDateTimeOffset.Empty, docketLine.WE_FinalisedDate);
			}
		}

		#endregion

		#region TestWD_DocketStatus_PropagatesDown

		public void TestWD_DocketStatus_PropagatesDown()
		{
			var docket = GetNewBusinessObject();
			if (docket.PropagatesFinalisedDateAndStatusToLines)
			{
				var docketLine = docket.Lines.AddNew();
				docket.WD_FinalisedDate = ZDateTimeOffset.UtcNow;
				docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
				AssertEquals("Finalised status should be propagaded down to lines", DocketLineStatus.Codes.Finalised, docketLine.WE_DocketLineStatus);

				docket.WD_FinalisedDate = ZDateTimeOffset.Empty;
				docket.WD_DocketStatus = DocketStatus.Codes.Entered;
				AssertEquals("Empty status should propagaded down to lines", "", docketLine.WE_DocketLineStatus);

				docketLine.Inventory.ForEach(i => i.ClearHasChanges());
				docketLine.ClearHasChanges();
				docket.WD_FinalisedDate = ZDateTimeOffset.UtcNow;
				docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
				AssertEquals(DocketLineStatus.Codes.Finalised, docketLine.WE_DocketLineStatus);
				AssertEquals("Should set HasChanges while propagating", true, docketLine.HasChanges);
			}
			else
			{
				var docketLine = docket.Lines.AddNew();
				var oldLineStatus = docketLine.WE_DocketLineStatus;
				docket.WD_FinalisedDate = ZDateTimeOffset.UtcNow;
				docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
				AssertEquals("Docket line status should remain the same", oldLineStatus, docketLine.WE_DocketLineStatus);
			}
		}

		public void TestWD_DocketStatus_PropagatesDownAtTheRightTime()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			if (docket.PropagatesFinalisedDateAndStatusToLines)
			{
				var line = docket.Lines.AddNew();
				line.WE_OP = data.Part1.PK;
				line.FillWithValidTestData();
				Factory.Save();

				var anotherFactory = new BusinessObjectFactory();
				var loadedDocket = anotherFactory.Load<TDocket>(docket.PK);

				loadedDocket.WD_FinalisedDate = ZDateTimeOffset.UtcNow;
				loadedDocket.WD_DocketStatus = DocketStatus.Codes.Finalised;
				AssertEquals("Finalised status be propagaded down to lines, because lines are loaded on the setter of the property", DocketLineStatus.Codes.Finalised, loadedDocket.Lines[0].WE_DocketLineStatus);
			}
			else
			{
				Assert("We don't care about this test for transfers", true);
			}
		}

		public void TestWD_DocketStatus_ClearingStatus_DoesNotPropagateToPutawayTransferReceives()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dockDoor = data.Whs1.DefaultOutboundDockDoorLocation;
			var location = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			var receiveLineA = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoor, "A");
			var receiveLineB = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoor, "B");
			receiveLineB.WE_DocketLineStatus = DocketLineStatus.Codes.Entered;
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoor, location, "A", 10m);
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();

			AssertNoExceptionThrown("No constraint failure throwing a ZSaveException should throw.", () => Factory.Save());
			AssertEquals($"ReceivelineA docketLine status should still be {DocketLineStatus.Codes.PickedForUnload}", DocketLineStatus.Codes.PickedForUnload, receiveLineA.WE_DocketLineStatus);
			AssertEquals($"ReceivelineB docketLine status should be empty ", "", receiveLineB.WE_DocketLineStatus);
		}

		#endregion

		#region TestWD_DocketStatus_IsLiteralOnly

		public void TestWD_DocketStatus_IsLiteralOnly()
		{
			Assert(WhsDocketSchema.WD_DocketStatus.IsLiteralOnly);
		}

		#endregion

		#region TestCanBeCancelled

		public void TestCanBeCancelled_SetCancelAgainShouldNotHaveIssue()
		{
			CreateDocketAndAssertCanBeCancelled(DocketStatus.Codes.Cancelled, DocketStatus.Codes.Cancelled);
		}

		public void TestCanBeCancelled_Entered()
		{
			CreateDocketAndAssertCanBeCancelled(DocketStatus.Codes.Entered, DocketStatus.Codes.Cancelled);
		}

		public void TestCanBeCancelled_Error()
		{
			CreateDocketAndAssertCanBeCancelled(DocketStatus.Codes.Error, DocketStatus.Codes.Cancelled);
		}

		public void TestCanBeCancelled_Held()
		{
			CreateDocketAndAssertCanBeCancelled(DocketStatus.Codes.Held, DocketStatus.Codes.Cancelled);
		}

		public void TestCanBeCancelled_Finalised()
		{
			CreateDocketAndAssertCanBeCancelled(DocketStatus.Codes.Finalised, DocketStatus.Codes.Finalised);
		}

		public void TestCanBeCancelled_New()
		{
			CreateDocketAndAssertCanBeCancelled(DocketStatus.Codes.New, DocketStatus.Codes.Cancelled);
		}

		public void TestCanBeCancelled_Picking()
		{
			CreateDocketAndAssertCanBeCancelled(DocketStatus.Codes.AttachedToPick, DocketStatus.Codes.AttachedToPick);
		}

		public void TestCanBeCancelled_Putaway()
		{
			CreateDocketAndAssertCanBeCancelled(DocketStatus.Codes.Putaway, DocketStatus.Codes.Putaway);
		}

		void CreateDocketAndAssertCanBeCancelled(string status, string expectedStatusAfterTryingToCancel)
		{
			var docket = GetNewBusinessObject();
			docket.WD_DocketStatus = status;
			AssertEquals("Should be able to set all status", status, docket.WD_DocketStatus);

			var expectedCanCancel = expectedStatusAfterTryingToCancel == DocketStatus.Codes.Cancelled;
			AssertEquals("Expect CanCancel return same result", expectedCanCancel, CanCancel(status));

			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals("Status was not expected.", expectedStatusAfterTryingToCancel, docket.WD_DocketStatus);
		}

		#endregion

		#region TestCancelFailedEvent

		public virtual void TestCancelFailedEvent()
		{
			Docket = GetNewBusinessObject();
			Docket.CancelFailed += OnCancelFailedTest;

			CodeDescriptionPairList availableStatuses = new DocketStatus();
			foreach (CodeDescriptionPair status in availableStatuses)
			{
				Docket.WD_DocketStatus = DocketStatus.Codes.Entered;
				Docket.WD_DocketStatus = status.Code;
				CancelFailedCalled = false;
				CancelFailedErrorMessage = ZString.Empty;
				Docket.CancelReactivateDocket();
				if (CanCancel(status.Code))
				{
					AssertEquals(false, CancelFailedCalled);
					AssertEquals(ZString.Empty, CancelFailedErrorMessage);
				}
				else
				{
					AssertEquals(true, CancelFailedCalled);
					AssertEquals(WhsDocket.CantCancelReasonMsg, CancelFailedErrorMessage);
				}
			}
		}

		#endregion

		#region TestWD_OH_Client

		public void TestWD_OH_Client()
		{
			var docket = GetNewBusinessObject();
			var onClientChangedCalled = false;
			docket.ClientChanged += (s, e) => onClientChangedCalled = true;

			var newClient = Factory.NewWithValidTestData<OrgHeader>();
			docket.WD_OH_Client = newClient.PK;
			AssertEquals(newClient.PK, docket.WD_OH_Client);
			AssertEquals(true, onClientChangedCalled);
		}

		#endregion

		#region TestWD_WW_Whs

		public virtual void TestWD_WW_Whs()
		{
			var docket = GetNewBusinessObject();
			var onWarehouseChangedCalled = false;
			docket.WarehouseChanged += (s, e) => onWarehouseChangedCalled = true;

			var whs = Factory.New<WhsWarehouse>();
			docket.WD_WW_Whs = whs.PK;
			AssertEquals(whs.PK, docket.WD_WW_Whs);
			AssertEquals(true, onWarehouseChangedCalled);

			TestWD_WW_Whs_ModifyLines(docket, whs);
		}

		protected virtual void TestWD_WW_Whs_ModifyLines(TDocket docket, WhsWarehouse whs)
		{
			var whs2 = Helper.CreateWarehouse("WH2");
			var line1 = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();
			AssertEquals(whs.PK, line1.WarehousePK);
			AssertEquals(whs.PK, line2.WarehousePK);

			docket.WD_WW_Whs = whs2.PK;
			AssertEquals(whs2.PK, line1.WarehousePK);
			AssertEquals(whs2.PK, line2.WarehousePK);
		}

		public virtual void TestWD_WW_Whs_AccessibilityToOperationalActions()
		{
			var actionFieldAttribute = ActionFieldAttribute.Get(typeof(TDocket).GetProperty(WhsDocketSchema.WD_WW_Whs.Name));
			AssertNull($"Action field attribute of {WhsDocketSchema.WD_WW_Whs.Name} should *not* exist as this property is only exposed to Operation Actions on WhsOrder.", actionFieldAttribute);
		}

		[TestDate(2023, 12, 14, 2, 10, 0)]
		public void TestWD_WW_Whs_PopulateWD_BookingDateIfRequired()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var company = Factory.Load<GlbCompany>(Env.CurrentCompanyPK);
			var branchCNNJI = company.Branches.AddNew();
			branchCNNJI.GB_Code = "NJ";
			branchCNNJI.GB_RL_NKHomePort = "CNNJI";
			whs.WW_GB_RelatedCompanyBranch = branchCNNJI.PK;

			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;

			AssertEquals("WD_BookingDate should be empty when newing.", ZDateTimeOffset.Empty, docket.WD_BookingDate);

			docket.WD_WW_Whs = whs.PK;
			AssertEquals(new ZDateTimeOffset(2023, 12, 14, 10, 10, 0, TimeSpan.FromHours(8)), docket.WD_BookingDate);
		}

		[TestDate(2023, 12, 14, 2, 10, 0)]
		public void TestWD_WW_Whs_PopulateWD_BookingDateIfRequired_OnlyIfNotInDBAndIsEmpty()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var company = Factory.Load<GlbCompany>(Env.CurrentCompanyPK);
			var branchCNNJI = company.Branches.AddNew();
			branchCNNJI.GB_Code = "NJ";
			branchCNNJI.GB_RL_NKHomePort = "CNNJI";
			whs.WW_GB_RelatedCompanyBranch = branchCNNJI.PK;

			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;
			var bookingDate = new ZDateTimeOffset(2012, 3, 4, 5, 0, 0, TimeSpan.FromHours(2));
			docket.WD_BookingDate = bookingDate;
			AssertEquals("WD_BookingDate is set.", bookingDate, docket.WD_BookingDate);

			docket.WD_WW_Whs = whs.PK;
			AssertEquals("WD_BookingDate won't change.", bookingDate, docket.WD_BookingDate);
		}

		#endregion

		#region TestWD_NotInvoiced

		public virtual void TestWD_NotInvoiced()
		{
			AssertEquals(true, Docket.WD_NotInvoiced);
		}

		#endregion

		#region TestWD_NotInvoicedInfo

		public virtual void TestWD_NotInvoicedInfo()
		{
			AssertEquals(WhsDocket.Schema.WD_NotInvoiced, Docket.WD_NotInvoicedInfo.Name);
		}

		#endregion

		#region TestDGContact

		public virtual void TestDGContact()
		{
			AssertEquals(ZString.Empty, Docket.DGContact);
			OrgHeader org = Helper.CreateClient();
			WhsWarehouse whs = Helper.CreateWarehouse("1");
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "A";
			org.MiscServ.OM_OC_EXDefaultDGContact = contact.PK;
			Docket.WD_OH_Client = org.PK;
			Docket.WD_WW_Whs = whs.PK;
			AssertEquals(contact.OC_ContactName, Docket.DGContact);
		}

		#endregion

		#region TestDGContactInfo

		public virtual void TestDGContactInfo()
		{
			AssertEquals("DGContact", Docket.DGContactInfo.Name);
			AssertEquals(true, Docket.DGContactInfo.ReadOnly);
		}

		#endregion

		#region TestWD_TotalUnitsFromLines

		public virtual void TestWD_TotalUnitsFromLines()
		{
			Docket.Lines.AddNew().WE_TransactionQuantity = 10m;
			Docket.Lines.AddNew().WE_TransactionQuantity = 20m;
			Docket.Lines.AddNew().WE_TransactionQuantity = 30.55m;

			AssertEquals(60.55m, Docket.WD_TotalUnitsFromLines);
		}

		#endregion

		#region TestTotalWeightUnits

		public virtual void TestTotalWeightUnits()
		{
			AssertEquals(OLookUpEditType.Weight, Docket.TotalWeightUnits.LookupEditType);
		}

		#endregion

		#region TestTotalCubicUnits

		public virtual void TestTotalCubicUnits()
		{
			AssertEquals(OLookUpEditType.Volume, Docket.TotalCubicUnits.LookupEditType);
		}

		#endregion

		#region TestSubTypeDesc

		public abstract void TestSubTypeDesc();

		#endregion

		#region TestSubTypeDescInfo

		public virtual void TestSubTypeDescInfo()
		{
			AssertEquals("SubTypeDesc", Docket.SubTypeDescInfo.Name);
		}

		#endregion

		#region TestStatuses

		public virtual void TestStatuses()
		{
			AssertNotNull(Docket.Statuses);
			AssertEquals(typeof(DocketStatus), Docket.Statuses.GetType());
		}

		#endregion

		#region TestWD_DocketStatusDescription

		public void TestWD_DocketStatusDescription()
		{
			TestWD_DocketStatusDescriptionCore();
		}

		protected virtual void TestWD_DocketStatusDescriptionCore()
		{
			var docket = GetNewBusinessObject();
			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals(DocketStatus.Descriptions.Entered, docket.WD_DocketStatusDescription);
		}

		#endregion

		#region TestWD_DocketStatusDescriptionInfo

		public void TestWD_DocketStatusDescriptionInfo()
		{
			AssertEquals("WD_DocketStatusDescription", Docket.WD_DocketStatusDescriptionInfo.Name);
		}

		#endregion

		#region TestIsDomesticFreight

		public virtual void TestIsDomesticFreight()
		{
			Assert(Docket.IsDomesticFreight);
		}

		#endregion

		#region TestRefreshProxyProperties

		public virtual void TestRefreshProxyProperties()
		{
			AssertEquals(false, Docket.RefreshProxyPropertiesWasRun);
			Docket.RefreshProxyProperties();
			AssertEquals(true, Docket.RefreshProxyPropertiesWasRun);
		}

		#endregion

		#region TestWD_BOL

		public virtual void TestWD_BOL()
		{
			AssertEquals(ZString.Empty, Docket.WD_BOLNo);

			Docket.References.AddNew();
			Docket.References[0].WX_RefType = "HSB";
			Docket.References[0].WX_Reference = "123";
			AssertEquals("123", Docket.WD_BOLNo);

			Docket.References[0].WX_RefType = "VHN";
			AssertEquals(ZString.Empty, Docket.WD_BOLNo);

			Docket.References[0].WX_RefType = "HSB";
			Docket.WD_BOLNo = "345";
			AssertEquals("345", Docket.References[0].WX_Reference);

			Docket.References.AddNew();
			Docket.References[1].WX_RefType = "XXX";
			Docket.WD_BOLNo = ZString.Empty;
			AssertEquals(1, Docket.References.Count);
			AssertEquals("XXX", Docket.References[0].WX_RefType);
		}

		#endregion

		#region TestWD_RX_NKTotalOrderCurrency

		public void TestWD_RX_NKTotalOrderCurrency_Defaults()
		{
			var docket = GetNewBusinessObject();
			AssertEquals("Default WD_TotalOrderValue", 0m, docket.WD_TotalOrderValue);
			AssertEquals("Default WD_RX_NKTotalOrderCurrency", "", docket.WD_RX_NKTotalOrderCurrency);
		}

		public void TestWD_RX_NKTotalOrderCurrency_EnterTotalOrderValue()
		{
			var docket = GetNewBusinessObject();
			docket.WD_TotalOrderValue = 10;
			AssertEquals("Set Cur to Company Cur", "AUD", docket.WD_RX_NKTotalOrderCurrency);

			docket.WD_RX_NKTotalOrderCurrency = "NZD";
			AssertEquals("Keep new cur", "NZD", docket.WD_RX_NKTotalOrderCurrency);
			AssertEquals("Keep existing value", 10m, docket.WD_TotalOrderValue);
		}

		public void TestWD_RX_NKTotalOrderCurrency_EnterLineOrderValue()
		{
			var docket = GetNewBusinessObject();
			var line = docket.Lines.AddNew();
			line.WE_ExtendedLinePrice = 10;
			AssertEquals("Line sets total on order", 10m, docket.WD_TotalOrderValue);
			AssertEquals("Setting total on order sets to common Cur on lines, because order cur is blank", "AUD", docket.WD_RX_NKTotalOrderCurrency);

			var line2 = docket.Lines.AddNew();
			line2.WE_ExtendedLinePrice = 10;
			AssertEquals("Line sets total on order", 20m, docket.WD_TotalOrderValue);
			AssertEquals("Setting total on order should not change the order cur", "AUD", docket.WD_RX_NKTotalOrderCurrency);

			docket.WD_RX_NKTotalOrderCurrency = "NZD";
			AssertEquals("Value should not be affected by changing the Cur", 20m, docket.WD_TotalOrderValue);
			AssertEquals("New cur entered", "NZD", docket.WD_RX_NKTotalOrderCurrency);

			line.WE_ExtendedLinePrice = 20;
			AssertEquals("Order Cur and Common Line Cur do not match, so don't update the order.", 20m, docket.WD_TotalOrderValue);
			AssertEquals("Cur not affected", "NZD", docket.WD_RX_NKTotalOrderCurrency);
		}

		public void TestWD_RX_NKTotalOrderCurrency_EnterLineOrderValue_OrderAUD_Line1NZD_Line2AUD()
		{
			var docket = GetNewBusinessObject();
			var line = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();
			line.WE_ExtendedLinePrice = 10;
			line2.WE_ExtendedLinePrice = 10;
			AssertEquals("Updates Order", 20m, docket.WD_TotalOrderValue);
			AssertEquals("No cur, so sets to current company cur", "AUD", docket.WD_RX_NKTotalOrderCurrency);

			line.WE_RX_NKUnitPriceCurrency = "NZD";
			AssertEquals("No change", 20m, docket.WD_TotalOrderValue);
			AssertEquals("No change", "AUD", docket.WD_RX_NKTotalOrderCurrency);

			line2.WE_ExtendedLinePrice = 20;
			AssertEquals("No change", 20m, docket.WD_TotalOrderValue);
			AssertEquals("No change", "AUD", docket.WD_RX_NKTotalOrderCurrency);

			line.WE_ExtendedLinePrice = 20;
			AssertEquals("No change", 20m, docket.WD_TotalOrderValue);
			AssertEquals("No change", "AUD", docket.WD_RX_NKTotalOrderCurrency);
		}

		public void TestWD_RX_NKTotalOrderCurrency_ChangeLineCurWhenInSync()
		{
			var docket = GetNewBusinessObject();
			var line = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();
			line.WE_ExtendedLinePrice = 10;
			line2.WE_ExtendedLinePrice = 10;
			AssertEquals("Updates Order", 20m, docket.WD_TotalOrderValue);
			AssertEquals("No cur, so sets to current company cur", "AUD", docket.WD_RX_NKTotalOrderCurrency);

			docket.WD_RX_NKTotalOrderCurrency = "NZD";
			line.WE_RX_NKUnitPriceCurrency = "NZD";
			line2.WE_RX_NKUnitPriceCurrency = "NZD";
			AssertEquals("No change", 20m, docket.WD_TotalOrderValue);
			AssertEquals("Now NZD", "NZD", docket.WD_RX_NKTotalOrderCurrency);
			AssertEquals("Now NZD", "NZD", line.WE_RX_NKUnitPriceCurrency);
			AssertEquals("Now NZD", "NZD", line2.WE_RX_NKUnitPriceCurrency);

			line.WE_ExtendedLinePrice = 20;
			AssertEquals("10+20 = 30", 30m, docket.WD_TotalOrderValue);
			AssertEquals("No change", "NZD", docket.WD_RX_NKTotalOrderCurrency);
			AssertEquals("No change", "NZD", line.WE_RX_NKUnitPriceCurrency);
			AssertEquals("No change", "NZD", line2.WE_RX_NKUnitPriceCurrency);

			line.WE_RX_NKUnitPriceCurrency = "AUD";
			AssertEquals("No change", 30m, docket.WD_TotalOrderValue);
			AssertEquals("No change", "NZD", docket.WD_RX_NKTotalOrderCurrency);
			AssertEquals("Now AUD", "AUD", line.WE_RX_NKUnitPriceCurrency);
			AssertEquals("No change", "NZD", line2.WE_RX_NKUnitPriceCurrency);
		}

		#endregion

		// TODO: Uncomment when doing (Reference Split or DocketID) for autorating WI.
		//#region TestWD_ExternalReference_UpdateReferenceOnCharges

		//public void TestWD_ExternalReference_UpdateReferenceOnCharges()
		//{
		//    var client = Helper.CreateClient("CLIENT");
		//    var whs = Helper.CreateWarehouse("WHS");

		//    Docket.WD_OH_Client = client.PK;
		//    Docket.WD_WW_Whs = whs.PK;

		//    Docket.WD_ExternalReference = "REF1";
		//    AssertNull("Precondition.", Docket.JobHeader);

		//    var job = Helper.CreateRatingJob(Docket, WhsDocketSchema.Constants.Prefix);
		//    var charge1 = Helper.CreateJobCharge(job, null, 0m);
		//    charge1.JobChargeAttributes.RemoveAndDeleteAll();
		//    Docket.WD_ExternalReference = "REF2";
		//    AssertEquals("Charge should have no DocketReference attributes added if it doesn't have any.", 0, charge1.JobChargeAttributes.Count);

		//    var chargeAttribute = Helper.CreateJobChargeAttrib(charge1, JobChargeAttribTypeList.Codes.DocketReference, "REF2");
		//    Docket.WD_ExternalReference = "REF3";
		//    AssertEquals("Charge DocketReference attribute should be updated with the update of Docket Reference.", "REF3", charge1.JobChargeAttrib_DocketReference);
		//}

		//#endregion

		#region Property Infos

		#region TestWD_ArrivalDateInfo

		public void TestWD_ArrivalDateInfo()
		{
			TestReadOnly(d => d.WD_ArrivalDateInfo, newStatus: false, entered: false, finalised: true, cancelled: true);
		}

		#endregion

		#region TestWD_BookingDateInfo

		public void TestWD_BookingDateInfo()
		{
			TestStandardReadOnly(d => d.WD_BookingDateInfo);
		}

		#endregion

		#region TestWD_CustomerReferenceInfo

		public void TestWD_CustomerReferenceInfo()
		{
			TestNonStandardReadOnly1(d => d.WD_CustomerReferenceInfo);
		}

		#endregion

		#region TestWD_DocketIDInfo

		public void TestWD_DocketIDInfo()
		{
			TestReadOnly(d => d.WD_DocketIDInfo, true, true, true, true);
		}

		#endregion

		#region TestWD_DocketStatusInfo

		public void TestWD_DocketStatusInfo()
		{
			TestReadOnly(d => d.WD_DocketStatusInfo, true, true, true, true);
		}

		#endregion

		#region TestWD_DocketSubTypeInfo

		public void TestWD_DocketSubTypeInfo()
		{
			TestHasLinesReadOnly(d => d.WD_DocketSubTypeInfo);
		}

		#endregion

		#region TestWD_ETAInfo

		public void TestWD_ETAInfo()
		{
			TestNonStandardReadOnly1(d => d.WD_ETAInfo);
		}

		#endregion

		#region TestWD_ETDInfo

		public void TestWD_ETDInfo()
		{
			TestNonStandardReadOnly1(d => d.WD_ETDInfo);
		}

		#endregion

		#region TestWD_ExternalReferenceInfo

		public void TestWD_ExternalReferenceInfo()
		{
			TestNonStandardReadOnly1(d => d.WD_ExternalReferenceInfo);
		}

		#endregion

		#region TestWD_ExternalReferenceSplitInfo

		public void TestWD_ExternalReferenceSplitInfo()
		{
			TestReadOnly(d => d.WD_ExternalReferenceSplitInfo, true, true, true, true);
		}

		#endregion

		#region TestWD_FinalisedDateInfo

		public void TestWD_FinalisedDateInfo()
		{
			TestReadOnly(d => d.WD_FinalisedDateInfo, true, true, true, true);
		}

		#endregion

		#region TestWD_OH_ClientInfo

		public void TestWD_OH_ClientInfo()
		{
			TestHasLinesReadOnly(d => d.WD_OH_ClientInfo);
		}

		#endregion

		#region TestWD_LocalCartInsuranceCostInfo

		public void TestWD_LocalCartInsuranceCostInfo()
		{
			TestNonStandardReadOnly1(d => d.WD_LocalCartInsuranceCostInfo);
		}

		#endregion

		#region TestWD_DropModeInfo

		public void TestWD_DropModeInfo()
		{
			TestNonStandardReadOnly1(d => d.WD_DropModeInfo);
		}

		#endregion

		#region TestWD_WhsOrderFulfillmentRuleInfo

		public void TestWD_WhsOrderFulfillmentRuleInfo()
		{
			TestStandardReadOnly(d => d.WD_WhsOrderFulfillmentRuleInfo);
		}

		#endregion

		#region TestWD_PickOptionInfo

		public virtual void TestWD_PickOptionInfo()
		{
			TestStandardReadOnly(d => d.WD_PickOptionInfo);
		}

		#endregion

		#region TestWD_RequiredDateInfo

		public void TestWD_RequiredDateInfo()
		{
			TestNonStandardReadOnly1(d => d.WD_RequiredDateInfo);
		}

		#endregion

		#region TestWD_PL_NKCarrierServiceLevelInfo

		public void TestWD_PL_NKCarrierServiceLevelInfo()
		{
			TestWD_PL_NKCarrierServiceLevelInfoCore();
		}

		protected virtual void TestWD_PL_NKCarrierServiceLevelInfoCore() => TestNonStandardReadOnly1(d => d.WD_PL_NKCarrierServiceLevelInfo);

		#endregion

		#region TestWD_RS_NKServiceLevelInfo

		public void TestWD_RS_NKServiceLevelInfo()
		{
			TestWD_RS_NKServiceLevelInfoCore();
		}

		protected virtual void TestWD_RS_NKServiceLevelInfoCore() => TestNonStandardReadOnly1(d => d.WD_RS_NKServiceLevelInfo);

		#endregion

		#region TestWD_TotalCubicUnitUpdatesTotalCubic

		public void TestWD_TotalCubicUnitUpdatesTotalCubic()
		{
			TestWD_TotalCubicUnitUpdatesTotalCubicCore();
		}

		protected virtual void TestWD_TotalCubicUnitUpdatesTotalCubicCore()
		{
			var org = Helper.CreateClient();
			var part1 = Helper.CreateProduct(org, "P1");
			Helper.SetProductWeightAndVolume(part1, 1m, Constants.Weight.Kilograms, 1m, Constants.Volume.CubicMetres);

			var part2 = Helper.CreateProduct(org, "P2");
			Helper.SetProductWeightAndVolume(part2, 2m, Constants.Weight.Pounds, 2m, Constants.Volume.CubicYards);

			var docket = GetNewBusinessObject();

			if (docket.ShouldUpdateWeightAndVolumeOnTheFly)
			{
				docket.WD_TotalCubicUnit = Constants.Volume.CubicYards;
				AssertEquals("Precondition:", 0m, docket.WD_TotalCubic);

				var docketLine1 = docket.Lines.AddNew();
				docketLine1.WE_OP = part1.PK;
				docketLine1.WE_TransactionQuantity = 10m;

				var docketLine2 = docket.Lines.AddNew();
				docketLine2.WE_OP = part2.PK;
				docketLine2.WE_TransactionQuantity = 10m;
				AssertEquals("Precondition: Total Volume is correct", 33.080m, docket.WD_TotalCubic);

				docket.WD_TotalCubicUnit = Constants.Volume.CubicMetres;
				AssertEquals("Total Volume is correct", 25.291m, docket.WD_TotalCubic);
			}
			else
			{
				Assert("Total Weight / Volume is not calculated for the Docket Type.", true);
			}
		}

		public void TestWD_TotalCubicUnitUpdatesTotalCubic_MultipleLinesWithTheSameProduct()
		{
			TestWD_TotalCubicUnitUpdatesTotalCubic_MultipleLinesWithTheSameProductCore();
		}

		protected virtual void TestWD_TotalCubicUnitUpdatesTotalCubic_MultipleLinesWithTheSameProductCore()
		{
			var docket = GetNewBusinessObject();
			if (docket.ShouldUpdateWeightAndVolumeOnTheFly)
			{
				var org = Helper.CreateClient();
				var part1 = Helper.CreateProduct(org, "P1");
				Helper.SetProductWeightAndVolume(part1, 1m, Constants.Weight.Kilograms, 1m, Constants.Volume.CubicMetres);

				var part2 = Helper.CreateProduct(org, "P2");
				Helper.SetProductWeightAndVolume(part2, 2m, Constants.Weight.Pounds, 2m, Constants.Volume.CubicYards);

				docket.WD_TotalCubicUnit = Constants.Volume.CubicYards;
				AssertEquals("Precondition:", 0m, docket.WD_TotalCubic);

				var docketLine1 = docket.Lines.AddNew();
				docketLine1.WE_OP = part1.PK;
				docketLine1.WE_TransactionQuantity = 10m;

				var docketLine2 = docket.Lines.AddNew();
				docketLine2.WE_OP = part2.PK;
				docketLine2.WE_TransactionQuantity = 5m;

				var docketLine3 = docket.Lines.AddNew();
				docketLine3.WE_OP = part2.PK;
				docketLine3.WE_TransactionQuantity = 5m;
				AssertEquals("Precondition: Total Volume is correct", 33.080m, docket.WD_TotalCubic);

				docket.WD_TotalCubicUnit = Constants.Volume.CubicMetres;
				AssertEquals("Total Volume is correct", 25.291m, docket.WD_TotalCubic);
			}
			else
			{
				Assert("Total Weight / Volume is not calculated for the Docket Type.", true);
			}
		}

		#endregion

		#region TestWD_TotalCubicInfo

		public void TestWD_TotalCubicInfo()
		{
			TestReadOnly(d => d.WD_TotalCubicInfo, false, false, false, false);
		}

		#endregion

		#region TestWD_TotalCubicUnitInfo

		public void TestWD_TotalCubicUnitInfo()
		{
			TestReadOnly(d => d.WD_TotalCubicUnitInfo, false, false, false, false);
		}

		#endregion

		#region TestWD_TotalWeightInfo

		public void TestWD_TotalWeightInfo()
		{
			TestReadOnly(d => d.WD_TotalWeightInfo, false, false, false, false);
		}

		#endregion

		#region TestWD_TotalWeightUnitUpdatesTotalWeight

		public void TestWD_TotalWeightUnitUpdatesTotalWeight()
		{
			TestWD_TotalWeightUnitUpdatesTotalWeightCore();
		}

		protected virtual void TestWD_TotalWeightUnitUpdatesTotalWeightCore()
		{
			var docket = GetNewBusinessObject();
			if (docket.ShouldUpdateWeightAndVolumeOnTheFly)
			{
				var org = Helper.CreateClient();
				var part1 = Helper.CreateProduct(org, "P1");
				Helper.SetProductWeightAndVolume(part1, 1m, Constants.Weight.Kilograms, 2000m, Constants.Volume.CubicCentimeters);
				var part2 = Helper.CreateProduct(org, "P2");
				Helper.SetProductWeightAndVolume(part2, 2m, Constants.Weight.Pounds, 2m, Constants.Volume.Litre);

				docket.WD_TotalWeightUnit = Constants.Weight.Pounds;
				AssertEquals("Precondition:", 0m, docket.WD_TotalWeight);

				var docketLine1 = docket.Lines.AddNew();
				docketLine1.WE_OP = part1.PK;
				docketLine1.WE_TransactionQuantity = 10m;

				var docketLine2 = docket.Lines.AddNew();
				docketLine2.WE_OP = part2.PK;
				docketLine2.WE_TransactionQuantity = 10m;
				AssertEquals("Precondition: Total Weight is correct", 42.05m, docket.WD_TotalWeight);

				docket.WD_TotalWeightUnit = Constants.Weight.Kilograms;
				AssertEquals("Total Volume is correct", 19.07m, docket.WD_TotalWeight);
			}
			else
			{
				Assert("Total Weight / Volume is not calculated for the Docket Type.", true);
			}
		}

		public void TestWD_TotalWeightUnitUpdatesTotalWeight_MultipleLinesWithTheSameProduct()
		{
			TestWD_TotalWeightUnitUpdatesTotalWeight_MultipleLinesWithTheSameProductCore();
		}

		protected virtual void TestWD_TotalWeightUnitUpdatesTotalWeight_MultipleLinesWithTheSameProductCore()
		{
			var docket = GetNewBusinessObject();
			if (docket.ShouldUpdateWeightAndVolumeOnTheFly)
			{
				var org = Helper.CreateClient();
				var part1 = Helper.CreateProduct(org, "P1");
				Helper.SetProductWeightAndVolume(part1, 1m, Constants.Weight.Kilograms, 2000m, Constants.Volume.CubicCentimeters);

				var part2 = Helper.CreateProduct(org, "P2");
				Helper.SetProductWeightAndVolume(part2, 2m, Constants.Weight.Pounds, 2m, Constants.Volume.Litre);

				docket.WD_TotalWeightUnit = Constants.Weight.Pounds;
				AssertEquals("Precondition:", 0m, docket.WD_TotalWeight);

				var docketLine1 = docket.Lines.AddNew();
				docketLine1.WE_OP = part1.PK;
				docketLine1.WE_TransactionQuantity = 10m;

				var docketLine2 = docket.Lines.AddNew();
				docketLine2.WE_OP = part2.PK;
				docketLine2.WE_TransactionQuantity = 5m;

				var docketLine3 = docket.Lines.AddNew();
				docketLine3.WE_OP = part2.PK;
				docketLine3.WE_TransactionQuantity = 5m;
				AssertEquals("Precondition: Total Weight is correct", 42.05m, docket.WD_TotalWeight);

				docket.WD_TotalWeightUnit = Constants.Weight.Kilograms;
				AssertEquals("Total Volume is correct", 19.07m, docket.WD_TotalWeight);
			}
			else
			{
				Assert("Total Weight / Volume is not calculated for the Docket Type.", true);
			}
		}

		#endregion

		#region TestTaskPlanningStatusPrompt

		public void TestTaskPlanningStatusPrompt()
		{
			var docket = GetNewBusinessObject();
			docket.WD_TaskPlanningStatus = string.Empty;
			AssertNullOrEmptyOrWhitespace("", docket.TaskPlanningStatusPrompt);

			docket.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			AssertNullOrEmptyOrWhitespace("", docket.TaskPlanningStatusPrompt);

			if (SupportsPlanningStatusPrompt)
			{
				docket.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				AssertEquals($"This {docket.Description} is read only because it is ready for planning.", docket.TaskPlanningStatusPrompt);

				docket.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
				AssertEquals($"This {docket.Description} is read only because it is planned.", docket.TaskPlanningStatusPrompt);
			}
			else
			{
				docket.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				AssertNullOrEmptyOrWhitespace("", docket.TaskPlanningStatusPrompt);

				docket.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
				AssertNullOrEmptyOrWhitespace("", docket.TaskPlanningStatusPrompt);
			}
		}

		protected virtual bool SupportsPlanningStatusPrompt => false;

		#endregion

		#region TestWD_TotalWeightUnitInfo

		public void TestWD_TotalWeightUnitInfo()
		{
			TestReadOnly(d => d.WD_TotalWeightUnitInfo, false, false, false, false);
		}

		#endregion

		#region TestWD_UnitsSentInfo

		public void TestWD_UnitsSentInfo()
		{
			TestNonStandardReadOnly1(d => d.WD_UnitsSentInfo);
		}

		#endregion

		#region TestWD_WP_ParentPickForTransferInfo

		public void TestWD_WP_ParentPickForTransferInfo()
		{
			var parentPickInfo = GetNewBusinessObject().WD_WP_ParentPickForTransferInfo;
			AssertEquals("ParentPickForTransfer should be readonly.", true, parentPickInfo.ReadOnly);

			var actionFieldAttribute = ActionFieldAttribute.Get(typeof(TDocket).GetProperty(parentPickInfo.Name));
			AssertNull($"Precondition: Action field attribute of {parentPickInfo.Name} should *not* exist as it is a guid field and excluded by default.", actionFieldAttribute);
		}

		#endregion

		#region TestWD_WW_WhsInfo

		public void TestWD_WW_WhsInfo()
		{
			TestWD_WW_WhsInfoCore();
		}

		protected virtual void TestWD_WW_WhsInfoCore()
		{
			TestHasLinesReadOnly(d => d.WD_WW_WhsInfo);
		}

		#endregion

		#region TestWD_BolNoInfo

		public void TestWD_BolNoInfo()
		{
			AssertEquals(25, Docket.WD_BOLNoInfo.MaxLength);
			TestNonStandardReadOnly1(d => d.WD_BOLNoInfo);
		}

		#endregion

		#region TestWD_CODPayMethodInfo

		public void TestWD_CODPayMethodInfo()
		{
			TestNonStandardReadOnly1(d => d.WD_CODPayMethodInfo);
		}

		#endregion

		#region TestWD_INCOInfo

		public void TestWD_INCOInfo()
		{
			TestNonStandardReadOnly1(d => d.WD_INCOInfo);
		}

		#endregion

		#region TestWD_TaskPlanningStatus

		public void TestWD_TaskPlanningStatus()
		{
			TestReadOnly(d => d.WD_TaskPlanningStatusInfo, true, true, true, true);
		}

		#endregion

		#region TestWD_P9_PackingTaskInfo

		public void TestWD_P9_PackingTaskInfo()
		{
			var packingTaskInfo = GetNewBusinessObject().WD_P9_PackingTaskInfo;
			AssertEquals("PackingTask should be readonly.", true, packingTaskInfo.ReadOnly);

			var actionFieldAttribute = ActionFieldAttribute.Get(typeof(TDocket).GetProperty(packingTaskInfo.Name));
			AssertEquals("Should be readonly.", true, actionFieldAttribute.ReadOnly);
		}

		#endregion

		#region TestWD_TransportModeInfo

		public void TestWD_TransportModeInfo()
		{
			TestNonStandardReadOnly1(d => d.WD_TransportModeInfo);
		}

		#endregion

		#region TestWD_TransportModeInfo

		public void TestWD_ContainerModeInfo()
		{
			TestNonStandardReadOnly1(d => d.WD_ContainerModeInfo);
		}

		#endregion

		#region TestWD_ShipperCODAmountInfo

		public void TestWD_ShipperCODAmountInfo()
		{
			TestNonStandardReadOnly1(d => d.WD_ShipperCODAmountInfo);
		}

		#endregion

		#region TestIsFinaliseAllowedInfo

		public void TestIsFinaliseAllowedInfo()
		{
			var docket = GetNewBusinessObject();
			AssertEquals("Is Finalize Allowed", docket.IsFinaliseAllowedInfo.HumanReadableName);
		}

		#endregion

		public void TestWD_OrderClassificationInfo()
		{
			TestReadOnly(d => d.WD_OrderClassificationInfo, true, true, true, true);
		}

		#region ReadOnly Helpers

		void TestHasLinesReadOnly(Func<TDocket, ZPropertyInfo> getInfo)
		{
			var docket = GetNewBusinessObject();
			var info = getInfo(docket);
			AssertEquals("Precondition: No lines.", 0, docket.Lines.Count);
			AssertEquals("Should *not* be ReadOnly if there are no lines.", false, info.ReadOnly);

			var line1 = docket.Lines.AddNew();
			AssertEquals("Should be ReadOnly if there are lines.", true, info.ReadOnly);

			line1.Delete();
			AssertEquals("Should *not* be ReadOnly if there are no lines.", false, info.ReadOnly);

			// Test we RefreshBinding on collection count changed
			var refreshCount = 0;
			info.ValueChanged += (s, e) => refreshCount++;

			var line2 = docket.Lines.AddNew();
			AssertEquals("Should have called refresh binding to ensure the read only is updated.", true, refreshCount >= 1);

			var addRefreshCount = refreshCount;
			line2.Delete();
			AssertEquals("Should have called refresh binding to ensure the read only is updated.", true, (refreshCount - addRefreshCount) >= 1);

			var addDeleteRefreshCount = refreshCount;
			docket.Lines.RefreshBinding();
			AssertEquals("Should *not* have called refresh binding.", addDeleteRefreshCount, refreshCount);

			// Should also use standard read only behaviour
			TestStandardReadOnly(getInfo, readOnlyWhenDocketHasLines: true);
		}

		public void TestIsReadyForPlanningOrPlanned()
		{
			var docket = GetNewBusinessObject();

			docket.WD_TaskPlanningStatus = string.Empty;
			AssertEquals($"IsReadyForPlanningOrPlanned should be false if task planning status of docket is empty", false, docket.IsReadyForPlanningOrPlanned);

			docket.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertEquals($"IsReadyForPlanningOrPlanned should be true if task planning status of docket is ready for planning", true, docket.IsReadyForPlanningOrPlanned);

			docket.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			AssertEquals($"IsReadyForPlanningOrPlanned should be false if task planning status of docket is not ready for planning", false, docket.IsReadyForPlanningOrPlanned);

			docket.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			AssertEquals($"IsReadyForPlanningOrPlanned should be true if task planning status of docket is planned", true, docket.IsReadyForPlanningOrPlanned);
		}

		protected void TestDocketPlanningStatusReadOnly(Func<TDocket, ZPropertyInfo> getInfo)
		{
			var docket = GetNewBusinessObject();
			var name = getInfo(docket).Name;

			docket.WD_TaskPlanningStatus = string.Empty;
			AssertEquals($"{name} should not be readonly if task planning status of docket is empty", false, getInfo(docket).ReadOnly);

			docket.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertEquals($"{name} should be readonly if task planning status of docket is ready for planning", true, getInfo(docket).ReadOnly);

			docket.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			AssertEquals($"{name} should not be readonly if task planning status of docket is not ready for planning", false, getInfo(docket).ReadOnly);

			docket.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			AssertEquals($"{name} should be readonly if task planning status of docket is planned", true, getInfo(docket).ReadOnly);
		}

		protected void TestStandardReadOnly(Func<TDocket, ZPropertyInfo> getInfo)
		{
			TestStandardReadOnly(getInfo, readOnlyWhenDocketHasLines: false);
		}

		protected void TestStandardReadOnly(Func<TDocket, ZPropertyInfo> getInfo, bool readOnlyWhenDocketHasLines)
		{
			TestStandardReadOnly(d => getInfo(d).ReadOnly, d => getInfo(d).Name, readOnlyWhenDocketHasLines);
		}

		protected void TestStandardReadOnly(Func<TDocket, bool> getReadOnly, Func<TDocket, string> getName, bool readOnlyWhenDocketHasLines)
		{
			TestReadOnly(
				getReadOnly,
				getName,
				false,
				false,
				true,
				true,
				(_, name, docket) => TestStandardReadOnlyCore(getReadOnly, name, docket, readOnlyWhenDocketHasLines)
			);
		}

		protected virtual void TestStandardReadOnlyCore(Func<TDocket, bool> getReadOnly, string name, TDocket docket, bool readOnlyWhenDocketHasLines)
		{
		}

		protected void TestNonStandardReadOnly1(Func<TDocket, ZPropertyInfo> getInfo)
		{
			TestNonStandardReadOnly1(d => getInfo(d).ReadOnly, d => getInfo(d).Name);
		}

		protected virtual void TestNonStandardReadOnly1(Func<TDocket, bool> getReadOnly, Func<TDocket, string> getName)
		{
			TestReadOnly(getReadOnly, getName, false, false, true, true);
		}

		protected void TestReadOnly(Func<TDocket, ZPropertyInfo> getInfo, bool newStatus, bool entered, bool finalised, bool cancelled)
		{
			TestReadOnly(d => getInfo(d).ReadOnly, d => getInfo(d).Name, newStatus, entered, finalised, cancelled);
		}

		protected void TestReadOnly(
			Func<TDocket, bool> getReadOnly,
			Func<TDocket, string> getName,
			bool newStatus,
			bool entered,
			bool finalised,
			bool cancelled,
			Action<Func<TDocket, bool>, string, TDocket> additionalAssertions = null)
		{
			Env.Security.WhsReceivePostFinaliseEdit.IsAllowed = false;

			var docket = GetNewBusinessObject();
			var name = getName(docket);

			docket.WD_DocketStatus = DocketStatus.Codes.New;
			AssertEquals($"{name} should {(newStatus ? "" : "not ")}be readonly if docket is New", newStatus, getReadOnly(docket));

			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals($"{name} should {(entered ? "" : "not ")}be readonly if docket is Entered", entered, getReadOnly(docket));

			docket.WD_FinalisedDate = ZDateTimeOffset.UtcNow;
			AssertEquals($"{name} should {(finalised ? "" : "not ")}be readonly if docket is Finalised", finalised, getReadOnly(docket));

			docket.WD_FinalisedDate = ZDateTimeOffset.Empty;  // clean up
			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			docket.CancelReactivateDocket();
			AssertEquals($"{name} should {(cancelled ? "" : "not ")}be readonly if docket is Cancelled", cancelled, getReadOnly(docket));

			docket.WD_DocketStatus = DocketStatus.Codes.Entered; // clean up

			if (additionalAssertions != null)
			{
				additionalAssertions(getReadOnly, name, docket);
			}
		}

		#endregion

		#endregion

		#region ProductCount

		public void TestProductCount()
		{
			TestProductCountCore();
		}

		protected virtual void TestProductCountCore()
		{
			var docket = GetNewBusinessObject();
			var productCount = 0;
			AssertExceptionThrown<NotSupportedException>("Docket with ProductCount not overridden should always throw NotSupportedException.", () => productCount = docket.ProductCount);
		}

		#endregion

		#region TestWD_GS_NKFinalizedBy

		public void TestWD_GS_NKFinalizedBy()
		{
			var org = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("1");
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;
			docket.WD_WW_Whs = whs.PK;
			Factory.Save();

			var today = ZDateTimeOffset.Today;
			docket.WD_FinalisedDate = today;
			AssertEquals("Precondition", today, docket.WD_FinalisedDate);
			AssertEquals("Finalised Date should be propagaded down to lines", GlbStaff.CurrentUser.GS_Code, docket.WD_GS_NKFinalizedBy);

			docket.WD_FinalisedDate = ZDateTimeOffset.Empty;
			AssertEquals("Precondition", ZDateTimeOffset.Empty, docket.WD_FinalisedDate);
			AssertEquals("FinalizedBy should not have value when is not finalized.", ZString.Empty, docket.WD_GS_NKFinalizedBy);

			var firstUserCode = GlbStaff.CurrentUser.GS_Code;
			var staff = Helper.CreateGlbStaff("ABC", "ABC");
			Factory.Save();
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertNotEquals("The user must be different from the previous user.", staff.GS_Code, firstUserCode);

				docket.WD_FinalisedDate = today;
				AssertEquals("Precondition", today, docket.WD_FinalisedDate);
				AssertEquals("FinalizedBy should change to new user.", "ABC", docket.WD_GS_NKFinalizedBy);
			}
		}

		#endregion

		#endregion

		#region Flags

		#region TestIsJobInError

		public void TestIsJobInError()
		{
			Docket = GetNewBusinessObject();
			AssertEquals(false, Docket.IsJobInError);

			Docket.WD_DocketType = ZString.Empty;
			AssertEquals(false, Docket.IsJobInError);

			if (!GetExpectedErrorDocketType().IsEmpty)
			{
				Docket.WD_DocketType = GetExpectedErrorDocketType();
				AssertEquals(true, Docket.IsJobInError);
			}
		}

		protected virtual ZString GetExpectedErrorDocketType()
		{
			return ZString.Empty;
		}

		#endregion

		#region TestIsUniqueExternalReferenceCreatedOnSave

		public void TestIsUniqueExternalReferenceCreatedOnSave()
		{
			// for unset value
			TestIsUniqueExternalReferenceCreatedOnSaveCore();

			// for set values
			Docket.IsUniqueExternalReferenceCreatedOnSave = true;
			AssertEquals(true, Docket.IsUniqueExternalReferenceCreatedOnSave);
			Docket.IsUniqueExternalReferenceCreatedOnSave = false;
			AssertEquals(false, Docket.IsUniqueExternalReferenceCreatedOnSave);
		}

		protected virtual void TestIsUniqueExternalReferenceCreatedOnSaveCore()
		{
			AssertEquals(false, Docket.IsUniqueExternalReferenceCreatedOnSave);
		}

		#endregion

		#region TestIsFinalising

		public void TestIsFinalising()
		{
			AssertEquals(false, Docket.IsFinalising);
			using (new SemaphoreManager(Docket.FinaliseDocketSemaphore))
			{
				AssertEquals(true, Docket.IsFinalising);
			}
			AssertEquals(false, Docket.IsFinalising);
		}

		#endregion

		#region TestIsFinaliseAllowed

		public void TestIsFinaliseAllowedAndAwaitingResponse()
		{
			if (CheckIsHeldByCustomsWhenFinalising)
			{
				var awaitingCustomsResponseMessage = "Awaiting Customs Response";
				var data = new TestDataSimpleEnvironment(Factory);
				var docket = GetNewBusinessObject();
				var previousDocketSubType = docket.WD_DocketSubType;
				docket.WD_OH_Client = data.Org1.PK;
				docket.WD_WW_Whs = data.Whs1.PK;
				docket.WD_DocketSubType = "CUS";
				AssertEquals(true, docket.IsFinaliseAllowed);
				AssertEquals("", docket.AwaitingCustomsResponseStatus);

				AddEventTimeAndSaveForPostedTime(docket, Events.WarehouseJobCanNowBeFinalised, 1);
				AssertEquals(true, docket.IsFinaliseAllowed);
				AssertEquals("", docket.AwaitingCustomsResponseStatus);

				Thread.Sleep(100);
				// simulate event time happening *before* previous event but posted time still occuring *after* previous event
				AddEventTimeAndSaveForPostedTime(docket, Events.HoldTheWarehouseOrder, 0);
				AssertEquals(false, docket.IsFinaliseAllowed);
				AssertEquals(awaitingCustomsResponseMessage, docket.AwaitingCustomsResponseStatus);

				docket.WD_WW_Whs = ZGuid.Empty;
				AssertEquals(true, docket.IsFinaliseAllowed);
				docket.WD_WW_Whs = data.Whs1.PK; // clean-up

				docket.Warehouse.WW_IsVirtualWarehouse = true;
				AssertEquals(true, docket.IsFinaliseAllowed);
				AssertEquals("", docket.AwaitingCustomsResponseStatus);

				docket.Warehouse.WW_IsVirtualWarehouse = false; // clean-up

				docket.WD_DocketSubType = previousDocketSubType;
				AssertEquals(true, docket.IsFinaliseAllowed);
				AssertEquals("", docket.AwaitingCustomsResponseStatus);

				docket.WD_DocketSubType = "CUS"; // clean-up

				Thread.Sleep(100);
				// simulate event time happening *before* previous event but posted time still occuring *after* previous event
				var acceptOrderLog = AddEventTimeAndSaveForPostedTime(docket, Events.WarehouseJobCanNowBeFinalised, -1);
				AssertEquals(true, docket.IsFinaliseAllowed);
				AssertEquals("", docket.AwaitingCustomsResponseStatus);

				acceptOrderLog.Cancel();
				Factory.Save();

				AssertEquals(false, docket.IsFinaliseAllowed);
				AssertEquals(awaitingCustomsResponseMessage, docket.AwaitingCustomsResponseStatus);
			}
			else
			{
				var docket = GetNewBusinessObject();
				AssertEquals("No status should be shown.", "", docket.AwaitingCustomsResponseStatus);
			}
		}

		StmALog AddEventTimeAndSaveForPostedTime(TDocket docket, Event eventToAdd, int daysToAddToCurrentEventTime)
		{
			var log = docket.Logs.AddNew(eventToAdd);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = log.SL_EventTime.AddDays(daysToAddToCurrentEventTime);
			}

			Factory.Save();
			return log;
		}

		protected virtual bool CheckIsHeldByCustomsWhenFinalising
		{
			get { return false; }
		}

		#endregion

		#region TestIsFinalised

		public void TestIsFinalised()
		{
			var docket = GetNewBusinessObject();
			docket.WD_DocketStatus = DocketStatus.Codes.New;
			AssertEquals(false, docket.IsFinalised);
			docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
			AssertEquals(false, docket.IsFinalised);

			docket.WD_FinalisedDate = ZDateTimeOffset.UtcNow;
			AssertEquals(true, docket.IsFinalised);
			docket.WD_FinalisedDate = ZDateTimeOffset.Empty;
			AssertEquals(false, docket.IsFinalised);
		}

		#endregion

		#region TestIsCancelled

		public void TestIsCancelled()
		{
			var docket = GetNewBusinessObject();
			docket.WD_DocketStatus = "";
			AssertEquals(false, docket.IsCancelled);

			foreach (CodeDescriptionPair status in new DocketStatus())
			{
				docket.WD_DocketStatus = DocketStatus.Codes.Entered;
				AssertEquals(false, docket.IsCancelled);

				AssertEquals("Should not have WD_GS_NKCanceledBy", true, docket.WD_GS_NKCanceledBy.IsEmpty);
				AssertEquals("Should not have WD_CanceledTimeUtc", false, docket.WD_CanceledTimeUtc.IsValid);

				docket.WD_DocketStatus = status.Code;
				AssertEquals(status.Code == DocketStatus.Codes.Cancelled, docket.IsCancelled);
				if (status.Code == DocketStatus.Codes.Cancelled)
				{
					AssertEquals("Should have WD_GS_NKCanceledBy", GlbStaff.CurrentUser.GS_Code, docket.WD_GS_NKCanceledBy);
					AssertEquals("Should have WD_CanceledTimeUtc", true, docket.WD_CanceledTimeUtc.IsValid);
				}
				else
				{
					AssertEquals("Should not have WD_GS_NKCanceledBy", true, docket.WD_GS_NKCanceledBy.IsEmpty);
					AssertEquals("Should not have WD_CanceledTimeUtc", false, docket.WD_CanceledTimeUtc.IsValid);
				}
			}
		}

		#endregion

		#region TestIsCancelled_ChecksCancelledViaCustomsLogWhenWarehouseIsVirtual

		public void TestIsCancelled_ChecksCancelledViaCustomsLogWhenWarehouseIsVirtual()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			AssertEquals("Precondition", false, docket.IsCancelled);

			docket.Logs.AddNew(Events.Cancelled);
			AssertEquals(false, docket.IsCancelled);

			data.Whs1.WW_IsVirtualWarehouse = true;
			AssertEquals(true, docket.IsCancelled);
			AssertNotEquals(DocketStatus.Codes.Cancelled, docket.WD_DocketStatus);
			AssertEquals("Should not have WD_GS_NKCanceledBy", true, docket.WD_GS_NKCanceledBy.IsEmpty);
			AssertEquals("Should not have WD_CanceledTimeUtc", false, docket.WD_CanceledTimeUtc.IsValid);
		}

		#endregion

		#region TestIsFinalisedOrCancelled

		public void TestIsFinalisedOrCancelled()
		{
			var docket = GetNewBusinessObject();
			AssertEquals("Precondition", false, docket.IsFinalisedOrCancelled);

			docket.WD_FinalisedDate = ZDateTimeOffset.UtcNow;
			AssertEquals(true, docket.IsFinalisedOrCancelled);

			docket.WD_FinalisedDate = ZDateTimeOffset.Empty;
			AssertEquals(false, docket.IsFinalisedOrCancelled);

			docket.WD_DocketStatus = DocketStatus.Codes.Held;
			AssertEquals(false, docket.IsFinalisedOrCancelled);

			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals(true, docket.IsFinalisedOrCancelled);

			TestIsFinalisedOrCancelledCore(docket);
		}

		protected virtual void TestIsFinalisedOrCancelledCore(TDocket docket) { }

		#endregion

		#region TestIsAtleastOneInvoiceLinePosted

		#region TestIsAtleastOneInvoiceLinePosted

		public void TestIsAtleastOneInvoiceLinePosted()
		{
			JobHeader jobHeader = Helper.AddJobToDocket(Docket);

			AccTransactionLines line1 = Factory.New<AccTransactionLines>();
			AccTransactionLines line2 = Factory.New<AccTransactionLines>();
			line1.AL_JH = jobHeader.PK;
			line2.AL_JH = jobHeader.PK;
			line1.AL_GC = GlbCompany.CurrentCompany.PK;
			line2.AL_GC = GlbCompany.CurrentCompany.PK;

			AssertEquals("Precondition", false, Docket.IsAtleastOneInvoiceLinePosted);

			line1.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			line2.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;

			AssertEquals("Precondition", false, Docket.IsAtleastOneInvoiceLinePosted);

			line2.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			AssertEquals(true, Docket.IsAtleastOneInvoiceLinePosted);
		}

		#endregion

		#region TestIsAtleastOneInvoiceLinePosted_DoesNotLoadAccTransactionLinesIfTheyWereNotLoadedPreviously

		/// <summary>
		/// Geoff -- This test addresses a performance problem discovered at WorldNet in which their batch processors were grinding to a halt.
		/// When AccTtansactionLines exist in the factory, Accounting does some expensive work on Factory.Save(). To avoid this we make sure
		/// we don't load the lines ourselves, and only use them if they were already loaded (i.e. by the Billing tab).
		/// </summary>
		public void TestIsAtleastOneInvoiceLinePosted_DoesNotLoadAccTransactionLinesIfTheyWereNotLoadedPreviously()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;

			Docket.FillWithValidTestData();
			JobHeader jobHeader = Helper.AddJobToDocket(Docket);
			jobHeader.JH_JobNum = "TESTJOB";

			// Testing with no AccTransactionLines in the DB.
			AssertEquals("Job has no AccTransactionLines, result should be false.", false, Docket.IsAtleastOneInvoiceLinePosted);

			// Create Line 1 - Not Posted
			var line_NotPosted1 = Factory.New<AccTransactionLines>();
			line_NotPosted1.AL_GE = department.PK;
			line_NotPosted1.AL_GB = branch.PK;
			line_NotPosted1.AL_JH = jobHeader.PK;
			line_NotPosted1.AL_LineType = TransactionLineTypes.WIP;
			line_NotPosted1.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			AssertEquals("Precondition - should have Line1 already loaded as it was created by this factory.", true, IsAccTransactionLineInFactory(Factory, line_NotPosted1));
			AssertEquals("Line1 was not posted, result should be false.", false, Docket.IsAtleastOneInvoiceLinePosted);

			// Testing with 1 Un-Posted AccTransactionLine in the DB.
			Factory.Save();
			var otherFactory1 = new BusinessObjectFactory();
			var docketInOtherFactory1 = otherFactory1.Load<TDocket>(Docket.PK);
			AssertEquals("Precondition - OtherFactory1 should not have Line1 loaded.", false, IsAccTransactionLineInFactory(otherFactory1, line_NotPosted1));
			AssertEquals("Line1 was not posted, result should be false.", false, docketInOtherFactory1.IsAtleastOneInvoiceLinePosted);
			AssertEquals("The IsAtLeastOneInvoiceLinePosted property should not have loaded any AccTransactionLines as none were loaded previously.", false, IsAccTransactionLineInFactory(otherFactory1, line_NotPosted1));

			// Create Line 2 - Posted
			var header = otherFactory1.New<AccTransactionHeader>();
			header.AH_TransactionNum = "10001002";
			header.AH_InvoiceDate = ZDateTime.Today;
			header.AH_GB = branch.PK;
			header.AH_GE = department.PK;
			// AccTransactionHeader is not valid with empty AH_Ledger or AH_TransactionType.
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.Invoice;

			var line_Posted = otherFactory1.New<AccTransactionLines>();
			line_Posted.AL_GE = department.PK;
			line_Posted.AL_GB = branch.PK;
			line_Posted.AL_AH = header.PK;
			line_Posted.AL_JH = jobHeader.PK;
			line_Posted.AL_LineType = TransactionLineTypes.Revenue;
			line_Posted.AL_AG = otherFactory1.NewWithValidTestData<AccGLHeader>().PK;

			var charge = otherFactory1.NewWithValidTestData<JobCharge>();
			charge.JR_JH = jobHeader.PK;
			charge.JR_AL_ARLine = line_Posted.PK;

			AssertEquals("Precondition - OtherFactory1 should not have Line1 loaded.", false, IsAccTransactionLineInFactory(otherFactory1, line_NotPosted1));
			AssertEquals("Precondition - OtherFactory1 should have Line2 already loaded as it was created by this factory.", true, IsAccTransactionLineInFactory(otherFactory1, line_Posted));
			AssertEquals("Line2 is Posted, result should be true.", true, docketInOtherFactory1.IsAtleastOneInvoiceLinePosted);
			AssertEquals("IsAtLeastOneInvoiceLinePosted should load all AccTransactionLines because at least one line was loaded previously.", true, IsAccTransactionLineInFactory(otherFactory1, line_NotPosted1));
			AssertEquals("IsAtLeastOneInvoiceLinePosted should load all AccTransactionLines because at least one line was loaded previously.", true, IsAccTransactionLineInFactory(otherFactory1, line_Posted));

			// Testing with 1 Un-Posted and 1 Posted AccTransactionLine in the DB.
			otherFactory1.Save();
			var otherFactory2 = new BusinessObjectFactory();
			var docketInOtherFactory2 = otherFactory2.Load<TDocket>(Docket.PK);

			AssertEquals("Precondition - OtherFactory2 should not have Line1 loaded.", false, IsAccTransactionLineInFactory(otherFactory2, line_NotPosted1));
			AssertEquals("Precondition - OtherFactory2 should not have Line2 loaded.", false, IsAccTransactionLineInFactory(otherFactory2, line_Posted));
			AssertEquals("Line2 is Posted, result should be true.", true, docketInOtherFactory2.IsAtleastOneInvoiceLinePosted);
			AssertEquals("IsAtLeastOneInvoiceLinePosted should not load any AccTransactionLines because none were loaded previously.", false, IsAccTransactionLineInFactory(otherFactory2, line_NotPosted1));
			AssertEquals("IsAtLeastOneInvoiceLinePosted should not load any AccTransactionLines because none were loaded previously.", false, IsAccTransactionLineInFactory(otherFactory2, line_Posted));

			// Create Line3 - Not Posted
			var line_NotPosted2 = otherFactory2.NewWithValidTestData<AccTransactionLines>();
			line_NotPosted2.AL_JH = jobHeader.PK;
			line_NotPosted2.AL_LineType = TransactionLineTypes.WIP;
			AssertEquals("Precondition - otherFactory2 should not have line1 loaded.", false, IsAccTransactionLineInFactory(otherFactory2, line_NotPosted1));
			AssertEquals("Precondition - otherFactory2 should not have line2 loaded.", false, IsAccTransactionLineInFactory(otherFactory2, line_Posted));
			AssertEquals("Precondition - otherFactory2 should have line3 loaded as it was created in this Factory.", true, IsAccTransactionLineInFactory(otherFactory2, line_NotPosted2));
			AssertEquals("Line2 is posted, so the property result should be true.", true, docketInOtherFactory2.IsAtleastOneInvoiceLinePosted);
			AssertEquals("IsAtLeastOneInvoiceLinePosted should load all AccTransactionLines because at least one line was loaded previously.", true, IsAccTransactionLineInFactory(otherFactory2, line_NotPosted1));
			AssertEquals("IsAtLeastOneInvoiceLinePosted should load all AccTransactionLines because at least one line was loaded previously.", true, IsAccTransactionLineInFactory(otherFactory2, line_Posted));
			AssertEquals("IsAtLeastOneInvoiceLinePosted should load all AccTransactionLines because at least one line was loaded previously.", true, IsAccTransactionLineInFactory(otherFactory2, line_NotPosted2));
		}

		bool IsAccTransactionLineInFactory(BusinessObjectFactory factory, AccTransactionLines lineToCheck)
		{
			var allBusinessObjectsInFactory = ((IBusinessObjectFactoryInternals)factory).AllBusinessObjects;

			foreach (BusinessObject line in allBusinessObjectsInFactory.Where(bizObj => bizObj is AccTransactionLines))
			{
				if (line != null && line.PK == lineToCheck.PK)
				{
					return true;
				}
			}
			return false;
		}

		public void TestIsAtleastOneInvoiceLinePostedDoesNotCheckOtherDocketCharges()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Factory.Save();

			var docket = GetNewBusinessObject();
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_ExternalReference = "2";
			Helper.CreateAccountingDataWithNoCharge(docket);

			Factory.Save();

			var jobStorage = Helper.CreateJobStorage(data.Whs1.PK, data.Org1.PK, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			var jobHeaderForStorage = Helper.CreateAccountingDataWithNoCharge(jobStorage);

			var postedChargeForJobStorage = Helper.AddChargeLine(jobStorage);
			var wipChargeForJobStorage = Helper.AddWIPChargeLine(jobStorage);
			var chargeForDocket = Helper.AddWIPChargeLine(docket);
			chargeForDocket.JR_OSCostAmt = 5m;
			chargeForDocket.JR_LocalSellAmt = 5m;

			Factory.Save();

			docket.WD_FinalisedDate = ZDateTimeOffset.Today;
			AssertEquals("Precondition: Docket IsAtleastOneInvoiceLinePosted is false as no charge for docket is posted.", false, docket.IsAtleastOneInvoiceLinePosted);

			chargeForDocket.ARLine.AL_LineType = TransactionLineTypes.Revenue; // post charge
			AssertEquals("Docket IsAtleastOneInvoiceLinePosted is true as charge is posted.", true, docket.IsAtleastOneInvoiceLinePosted);
		}

		#endregion

		#endregion

		#region TestIsUSBonded

		public void TestIsUSBonded()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			AssertEquals(false, docket.IsUSBonded);

			// ensure ReadOnly for US Bonded orders.
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			docket.WD_DocketSubType = "CUS";

			if (SupportsCustomsTransactions)
			{
				AssertEquals("Precondition", true, docket.IsCustomsTransaction);
				AssertEquals(false, docket.IsUSBonded);

				data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
				AssertEquals("Precondition", true, docket.IsCustomsTransaction);
				AssertEquals(true, docket.IsUSBonded);
			}
			else
			{
				AssertEquals(false, docket.IsCustomsTransaction);
			}
		}

		protected virtual bool SupportsCustomsTransactions
		{
			get { return true; }
		}

		#endregion

		#region TestCustomsDocketLine_CreatesCustomsDataOnPreSaveValidation

		public void TestCustomsDocketLine_CreatesCustomsDataOnPreSaveValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			Helper.EnableWarehouseForBond(data.Whs1, true);

			if (SupportsCustomsTransactions)
			{
				var defaultDocketSubTypeForCustomsTransaction = docket.DefaultDocketSubTypeForCustomsTransaction;
				if (defaultDocketSubTypeForCustomsTransaction.IsEmpty)
				{
					defaultDocketSubTypeForCustomsTransaction = ReceiveType.Codes.Customs;
				}
				docket.WD_DocketSubType = defaultDocketSubTypeForCustomsTransaction;
				var docketLine = docket.Lines.AddNew();
				docketLine.RunPreSaveValidation();

				var getRegisterChildren = ((IBusiness)docketLine).Children.ToArray(); // get children before pock the CustomsData
				AssertEquals("CustomsData should be one of the children to run in validaion.", 1, getRegisterChildren.Count(c => c.Equals(docketLine.CustomsData)));
			}
			else
			{
				AssertEquals(false, docket.IsCustomsTransaction);
			}
		}

		#endregion

		#region TestCanDeleteRelatedData

		public virtual void TestCanDeleteRelatedData()
		{
			var docket = GetNewBusinessObject();
			AssertEquals("Precondition", true, docket.CanDeleteRelatedData);

			docket.WD_FinalisedDate = ZDateTimeOffset.Today;
			AssertEquals(false, docket.CanDeleteRelatedData);

			docket.WD_FinalisedDate = ZDateTimeOffset.Empty;
			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals(false, docket.CanDeleteRelatedData);

			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals(true, docket.CanDeleteRelatedData);
		}

		#endregion

		#region TestIsImportingForChangeOfInventory

		public void TestIsImportingForChangeOfInventory()
		{
			AssertEquals("Precondition", false, Docket.IsImportingForChangeOfInventory);

			using (Docket.MarkAsImportingForChangeOfInventory())
			{
				AssertEquals(true, Docket.IsImportingForChangeOfInventory);
			}

			AssertEquals(false, Docket.IsImportingForChangeOfInventory);
		}

		#endregion

		#region TestIsLinesInitialised

		public void TestIsLinesInitialised()
		{
			var docketType = typeof(TDocket);
			var docket = (TDocket)Factory.New(docketType, Guid.NewGuid()); // just to not set default values
			AssertEquals("Docket lines are not loaded yet.", false, docket.IsLinesInitialised);
			_ = docket.Lines;
			AssertEquals("Docket lines are loaded after accessing Lines.", true, docket.IsLinesInitialised);
		}

		#endregion

		#region TestIsSalesChannelAllowed

		public void TestIsSalesChannelAllowed()
		{
			AssertEquals("Docket should have correct IsSalesChannelAllowed.", ExpectedIsSalesChannelAllowed, GetNewBusinessObject().IsSalesChannelAllowed);
		}

		protected virtual bool ExpectedIsSalesChannelAllowed => false;

		#endregion

		#region TestIsCreatedFromPickByBOM

		public void TestIsCreatedFromPickByBOM() => TestIsCreatedFromPickByBOMCore();

		protected virtual void TestIsCreatedFromPickByBOMCore() => AssertEquals(false, Docket.IsCreatedFromPickByBOM);

		#endregion

		#endregion

		#region Finalisation

		#region TestFinaliseDocket_AbortsAndNotifiesUserIfDocketAlreadyFinalised

		public virtual void TestFinaliseDocket_AbortsAndNotifiesUserIfDocketAlreadyFinalised()
		{
			var docket = GetNewBusinessObject();
			docket.WD_FinalisedDate = ZDateTimeOffset.Today;
			AssertIsFinalisedPrecondition(docket);

			docket.FinaliseDocket();

			var notify = (NotificationBuffer)docket.NotificationManager.Peek;
			AssertEquals(true, notify.ContainsNotificationType(WhsErrorTypes.JobIsFinalised));
		}

		#endregion

		#region TestFinaliseDocket_AbortsAndNotifiesUserIfRunRreFinaliseValidationFails

		public virtual void TestFinaliseDocket_AbortsAndNotifiesUserIfRunRreFinaliseValidationFails()
		{
			Docket.WD_OH_Client = Helper.CreateClient().PK;
			Docket.WD_WW_Whs = Helper.CreateWarehouse("1", "A", 2, 1).PK;
			AssertEquals("Precondition", false, Docket.IsFinalised);

			Docket.FinaliseDocket();

			AssertEquals(false, Docket.IsFinalised);
			AssertEquals(true, ((NotificationBuffer)Docket.NotificationManager.Peek).HasErrors);
		}

		#endregion

		#region TestFinaliseDocket

		[TestDate(2016, 1, 2)]
		public void TestFinaliseDocket()
		{
			var docket = SetupForTestFinaliseDocket();
			AssertEquals("Precondition", false, docket.IsFinalised);

			Factory.Save(); // need to save as some finalise dockets use a 2nd factory
			docket.FinaliseDocket();

			AssertIsFinalisedPrecondition(docket);
			AssertDateEquals("Finalised date incorrect", new ZDate(2016, 1, 2), docket.WD_FinalisedDate.Date);

			var notify = (NotificationBuffer)docket.NotificationManager.Peek;
			AssertEquals(false, notify.HasErrors);

			// force docket specific finalisation assertion
			AssertFinaliseDocket(docket);
		}

		protected virtual void AssertFinaliseDocket(TDocket docket)
		{
		}

		#endregion

		#region TestFinaliseDocket_StopsIfAbortFinaliseExceptionThrown

		public void TestFinaliseDocket_StopsIfAbortFinaliseExceptionThrown()
		{
			var docket = SetupForTestFinaliseDocket();
			AssertEquals("Precondition", false, docket.IsFinalised);

			Factory.Save(); // need to save as some finalise dockets use a 2nd factory
			docket.WD_OH_ClientInfo.AdditionalValidation += () => throw new AbortFinalizationException("STOP!!");
			docket.FinaliseDocket();

			var notify = (NotificationBuffer)docket.NotificationSubscriber;
			AssertEquals(true, notify.HasErrors);
			AssertEquals("STOP!!", notify.AsString.Trim());
			Helper.AssertZCannotSaveExceptionThrown(string.Format("Cannot save as an error occurred during Finalization. Please reload the {0}.", docket.Description), Factory.Save);
		}

		#endregion

		#region TestFinaliseDocket_PreventsSaveIfPartiallyFinalised

		public void TestFinaliseDocket_PreventsSaveIfPartiallyFinalised()
		{
			var docket = SetupForTestFinaliseDocket();
			AssertEquals("Precondition", false, docket.IsFinalised);
			Factory.Save();

			docket.WD_FinalisedDateInfo.ValueChanged += (s, e) => { throw new Exception(); };

			try
			{
				docket.FinaliseDocket();
			}
			catch
			{
				// Pretend exception caught on form, error shown to user, and user can continue editing the form with partially finalised data
			}

			Helper.AssertZCannotSaveExceptionThrown( string.Format("Cannot save as an error occurred during Finalization. Please reload the {0}.", docket.Description), Factory.Save);
		}

		#endregion

		#region TestFinaliseDocket_ShowsNotificationMessage

		public void TestFinaliseDocket_ShowsNotificationMessage()
		{
			if (ShowsConfirmationMessage)
			{
				var docket = SetupForTestFinaliseDocket();
				AssertEquals("Precondition", false, docket.IsFinalised);

				Factory.Save(); // need to save as some finalise dockets use a 2nd factory

				docket.FinaliseDocket();
				AssertEquals(true, docket.IsFinalised);
				AssertEquals("Finalisation process should show Finalization Confirmation message", "Finalization Confirmation", ((QueryUserMsgBoxEventArgs)Notify.LastQueryUserEventArgs).Caption);
			}
			else
			{
				Assert("Docket type does not support showing confirmation messages.", true);
			}
		}

		protected virtual bool ShowsConfirmationMessage
		{
			get { return true; }
		}

		#endregion

		#region TestFinaliseDocketWithoutUserConfirmation

		public void TestFinaliseDocketWithoutUserConfirmation()
		{
			var docket = SetupForTestFinaliseDocket();
			AssertEquals("Precondition", false, docket.IsFinalised);

			Factory.Save(); // need to save as some finalise dockets use a 2nd factory

			AssertNoUserConfirmation(docket, Notify);
		}

		protected virtual void AssertNoUserConfirmation(TDocket docket, TestNotificationBuffer notify)
		{
			notify.LastQueryUserEventArgs = null; // Clear any confirmations from setup

			docket.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, docket.IsFinalised);
			AssertNull("Finalisation process should proceed finalisation without calling for confirmation", notify.LastQueryUserEventArgs);
		}

		#endregion

		#region TestFinaliseDocket_DoesNotModifyWE_TransactionQuantity

		public void TestFinaliseDocket_DoesNotModifyWE_TransactionQuantity()
		{
			var docket = SetupForTestFinaliseDocket();
			var docketLine = docket.Lines[0];
			var part = docketLine.SupplierPart;
			Helper.CreateProductUnit(part, part.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 1560m);
			docketLine.WE_F3_NKPackType = Constants.PkgUnit.Pallet;
			docketLine.WE_TransactionQuantity = 864m;
			docketLine.RunPreSaveValidation(); // to commit inventory for Adjustment and Transfer lines.
			Factory.Save();
			AssertEquals("Precondition.", 864m, docketLine.WE_TransactionQuantity);

			docket.FinaliseDocket();
			AssertEquals("Docket should be finalised", true, docket.IsFinalised);
			AssertEquals("WE_TransactionQuantity should not have changed during finalise.", 864m, docketLine.WE_TransactionQuantity);
		}

		#endregion

		protected virtual TDocket SetupForTestFinaliseDocket()
		{
			return SetupForTestFinaliseDocket("1");
		}

		protected virtual TDocket SetupForTestFinaliseDocket(ZString whsName)
		{
			var warehouse = Helper.CreateWarehouse(whsName, "A", 2, 1);
			Factory.Save();
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = Helper.CreateClient().PK;
			docket.WD_WW_Whs = warehouse.PK;
			docket.WD_RequiredDate = ZDateTimeOffset.Now;
			docket.NotificationManager.Push(Notify);

			return docket;
		}

		#endregion

		#region GetProductParams

		public void TestGetProductParams()
		{
			var docket = GetNewBusinessObject();
			AssertNull(docket.GetProductParams(null));

			var client1 = Helper.CreateClient();
			var part1 = Helper.CreateProduct("P1", client1);
			var product1 = WhsProduct.GetWhsProduct(part1);
			AssertNull(docket.GetProductParams(product1));

			var warehouse1 = Helper.CreateWarehouse("WHS");
			var param1 = product1.ParamsByWhsAndClient.AddNew();
			param1.W3_OH = client1.PK;
			param1.W3_WW = warehouse1.PK;
			AssertNull(docket.GetProductParams(product1));

			docket.WD_OH_Client = client1.PK;
			docket.WD_WW_Whs = warehouse1.PK;
			AssertEquals(param1, docket.GetProductParams(product1));

			var warehouse2 = Helper.CreateWarehouse("ABC");
			var client2 = Helper.CreateClient();
			var part2 = Helper.CreateProduct("P2", client2);
			var product2 = WhsProduct.GetWhsProduct(part2);
			var param2 = product2.ParamsByWhsAndClient.AddNew();
			param2.W3_OH = client2.PK;
			param2.W3_WW = warehouse2.PK;

			docket.WD_OH_Client = client2.PK;
			docket.WD_WW_Whs = warehouse2.PK;
			AssertNull(docket.GetProductParams(product1));
			AssertEquals(param2, docket.GetProductParams(product2));
		}

		#endregion

		#region Notifications

		#region TestNotificationManager

		public void TestNotificationManager()
		{
			AssertNotNull(Docket.NotificationManager);
		}

		#endregion

		#region TestNotificationSubscriber

		public void TestNotificationSubscriber()
		{
			TestNotificationBuffer buffer = new TestNotificationBuffer();
			Docket.NotificationManager.Push(buffer);
			AssertEquals(buffer, Docket.NotificationSubscriber);
			Docket.NotificationManager.Pop();
			AssertNotNull(Docket.NotificationSubscriber);
		}

		#endregion

		#endregion

		#region TestOperationalActionsFieldToShowVisibility

		public void TestOperationalActionsFieldToShowVisibility()
		{
			AssertEquals(true, ActionFieldAttribute.Get(typeof(TDocket).GetProperty(WhsDocketSchema.WD_DocketType.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(TDocket).GetProperty(WhsDocketSchema.WD_DocketStatus.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(TDocket).GetProperty(WhsDocketSchema.WD_OH_Client.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(TDocket).GetProperty(WhsDocketSchema.WD_OH_Forwarder.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(TDocket).GetProperty(WhsDocketSchema.WD_DocketSubType.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(TDocket).GetProperty(WhsDocketSchema.WD_WhsOrderFulfillmentRule.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(TDocket).GetProperty(WhsDocketSchema.WD_ExternalReferenceSplit.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(TDocket).GetProperty(WhsDocketSchema.WD_DocketID.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(TDocket).GetProperty(WhsDocketSchema.WD_FinalisedDate.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(TDocket).GetProperty(WhsDocketSchema.WD_WeightSent.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(TDocket).GetProperty(WhsDocketSchema.WD_WeightVolSetFromImport.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(TDocket).GetProperty(WhsDocketSchema.WD_OrderClassification.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(TDocket).GetProperty(WhsDocketSchema.WD_IsInwardsProcessingJob.Name)).ReadOnly);
			AssertEquals(true, ActionFieldAttribute.Get(typeof(TDocket).GetProperty(WhsDocketSchema.WD_TaskPlanningStatus.Name)).ReadOnly);
		}

		#endregion

		#region Event Logs

		public void TestActiveInactiveEvents()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			var job = Helper.AddJobToDocket(docket); // The JobHeader will trigger an EDT event when the docket is deleted, which is part of the test.
			job.JH_JobNum = "1";
			Factory.Save();

			AssertLogs(new Logs(docket), expectedSetToActiveLogCount: 0, expectedSetToInactiveLogCount: 0);

			docket.CancelReactivateDocket();
			Factory.Save();
			AssertLogs(new Logs(docket), expectedSetToActiveLogCount: 0, expectedSetToInactiveLogCount: 1);

			docket.CancelReactivateDocket();
			Factory.Save();
			AssertLogs(new Logs(docket), expectedSetToActiveLogCount: 1, expectedSetToInactiveLogCount: 1);
		}

		void AssertLogs(Logs logs, int expectedSetToActiveLogCount, int expectedSetToInactiveLogCount)
		{
			AssertEquals("Count of SetToActive event.", expectedSetToActiveLogCount, logs.Find(Helper.GetLogFilter(Events.SetToActive.Code)).Length);
			AssertEquals("Count of SetToInactive event.", expectedSetToInactiveLogCount, logs.Find(Helper.GetLogFilter(Events.SetToInactive.Code)).Length);
		}

		public void TestEventTimeRemainCorrectTimeSequence()
		{
			var docket = SetupForTestFinaliseDocket();
			Factory.Save();

			var timeInFuture = ZDateTimeOffset.Now.AddDays(2);
			var eventInFuture = docket.Logs.AddNew(Events.WarehouseOrderPacking, "Test", timeInFuture);
			Factory.Save();

			docket.FinaliseDocket();
			AssertEquals(true, docket.IsFinalised);
			Factory.Save();

			var jobEnteredEvent = docket.Logs.Find(Helper.GetLogFilter(Events.WarehouseJobEntered.Code))[0];
			var testEvent = docket.Logs.Find(Helper.GetLogFilter(Events.WarehouseOrderPacking.Code))[0];
			var finalisedEvent = docket.Logs.Find(Helper.GetLogFilter(Events.ItemDocumentJobFinalised.Code))[0];

			Assert("Finalised event time should be after Job Entered event time.", finalisedEvent.SL_EventTime > jobEnteredEvent.SL_EventTime);
			Assert("Finalised event time should be earlier than Test event time.", finalisedEvent.SL_EventTime < testEvent.SL_EventTime);
		}

		#endregion

		// interfaces

		#region ICancellable Members

		#region TestCantCancelReasonMsg

		public void TestCantCancelReasonMsg()
		{
			AssertEquals("You can only cancel dockets with " + DocketStatus.Descriptions.Entered + " status", WhsDocket.CantCancelReasonMsg);
		}

		#endregion

		#region TestCantReactivateReasonMsg

		public void TestCantReactivateReasonMsg()
		{
			AssertEquals(Res.GetString("e1d1aeee-6368-48d4-9edd-c3d3acf3062f", "Only canceled dockets can be re-activated."), WhsDocket.CantReactivateReasonMsg);
		}

		#endregion

		#region TestICancellable_CanCancel

		public virtual void TestICancellable_CanCancel()
		{
			Docket = GetNewBusinessObject();
			var allowCancel = (ICancellable)Docket;

			CodeDescriptionPairList availableStatuses = new DocketStatus();
			foreach (CodeDescriptionPair status in availableStatuses)
			{
				Docket.WD_DocketStatus = DocketStatus.Codes.Entered;
				Docket.WD_DocketStatus = status.Code;
				if (CanCancel(status.Code))
				{
					AssertEquals(ZString.Empty, allowCancel.CanCancel());
				}
				else
				{
					AssertEquals(WhsDocket.CantCancelReasonMsg, allowCancel.CanCancel());
				}
			}
		}

		#endregion

		#region CanCancel

		bool CanCancel(string status)
		{
			return status == DocketStatus.Codes.New
				|| status == DocketStatus.Codes.Entered
				|| status == DocketStatus.Codes.Cancelled
				|| status == DocketStatus.Codes.Held
				|| status == DocketStatus.Codes.Error;
		}

		#endregion

		#region TestIAllowUserToCancelCanCancel_JobAttached

		public void TestIAllowUserToCancelCanCancel_JobAttached()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;
			docket.WD_WW_Whs = whs.PK;

			var job = Helper.AddJobToDocket(docket);
			job.JH_JobNum = "1";
			Factory.Save();
			AssertEquals("CanCancel", ZString.Empty, docket.CanCancel());

			Factory.Save();
			AssertEquals("CanCancel", ZString.Empty, docket.CanCancel());

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;

			charge.JR_OSCostAmt = 0m;
			charge.JR_OSSellAmt = 0m;
			Factory.Save();
			AssertEquals("CanCancel", ZString.Empty, docket.CanCancel());

			charge.JR_OSCostAmt = 5m;
			charge.JR_OSCostExRate = 1m;
			Factory.Save();

			var expectedMessage = $@"{docket.HumanReadableName} cannot be deactivated.
Job Invoicing Charge(s) have been saved against this Invoicing Job Header ({job.JH_JobNum}) in the company EDI.";
			AssertEquals("Cannot cancel: job with non-zero job charge", expectedMessage, docket.CanCancel());
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestIAllowUserToCancelCanCancel_ForeignJobAttached()
		{
			var foreignCompany = Factory.New<GlbCompany>();
			foreignCompany.GC_Code = "BLA";
			Factory.Save();

			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;
			docket.WD_WW_Whs = whs.PK;

			var foreignJob = Helper.AddJobToDocket(docket);
			foreignJob.JH_JobNum = "2";
			foreignJob.JH_GC = foreignCompany.PK;

			Factory.Save();
			AssertEquals("CanCancel", ZString.Empty, docket.CanCancel());

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = foreignJob.PK;

			charge.JR_OSCostAmt = 0m;
			charge.JR_OSSellAmt = 0m;
			Factory.Save();
			AssertEquals("CanCancel", ZString.Empty, docket.CanCancel());

			charge.JR_OSCostAmt = 5m;
			charge.JR_OSCostExRate = 1m;
			Factory.Save();

			var expectedMessage = $@"{docket.HumanReadableName} cannot be deactivated.
Job Invoicing Charge(s) have been saved against this Invoicing Job Header ({foreignJob.JH_JobNum}) in the company {foreignCompany.GC_Code}.";
			AssertEquals("Cannot cancel: foreign job with non-zero charge", expectedMessage, docket.CanCancel());
		}

		public void TestIAllowUserToCancelCanCancel_CanReActivateCancelledWithJobAttached()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;
			docket.WD_WW_Whs = whs.PK;

			var job = Helper.AddJobToDocket(docket);
			job.JH_JobNum = "1";
			Factory.Save();

			//Simluate Cancelled Docket with Job charge.
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_OSCostAmt = 5m;
			charge.JR_OSCostExRate = 1m;
			Factory.Save();

			var docketRow = ((IBusinessObjectInternals)docket).Row;
			docketRow[WhsDocketSchema.Constants.WD_DocketStatus] = DocketStatus.Codes.Cancelled;
			AssertEquals("Can Re-Activate Docket with charge.", ZString.Empty, docket.CanCancel());
		}

		#endregion

		#region TestICancellableIsCancelled

		public virtual void TestICancellableIsCancelled()
		{
			Docket = GetNewBusinessObject();
			var allowCancel = (ICancellable)Docket;

			CodeDescriptionPairList availableStatuses = new DocketStatus();
			foreach (CodeDescriptionPair status in availableStatuses)
			{
				Docket.WD_DocketStatus = DocketStatus.Codes.Entered;
				Docket.WD_DocketStatus = status.Code;
				allowCancel.IsCancelled = true;
				if (status.Code == DocketStatus.Codes.New || status.Code == DocketStatus.Codes.Entered || status.Code == DocketStatus.Codes.Held || status.Code == DocketStatus.Codes.Error)
				{
					AssertEquals("Should cancel docket with " + status.Description + " status", true, allowCancel.IsCancelled);
					AssertEquals("Shoud change status to Canceled for docket with " + status.Description + " status", DocketStatus.Codes.Cancelled, Docket.WD_DocketStatus);
				}
				else if (status.Code == DocketStatus.Codes.Cancelled)
				{
					AssertEquals("Should cancel docket with " + status.Description + " status", false, Docket.IsCancelled);
					AssertEquals("Should change status to Entered for docket with " + status.Description + " status", DocketStatus.Codes.Entered, Docket.WD_DocketStatus);
				}
				else
				{
					AssertEquals("Should not cancel docket with " + status.Description + " status", false, allowCancel.IsCancelled);
					AssertEquals("Should not change status to Cancelled after attemp to cancel for docket with " + status.Description + " status", status.Code, Docket.WD_DocketStatus);
				}
			}
		}

		#endregion

		#region TestICancellableCanReactivate

		public virtual void TestICancellableCanReactivate()
		{
			Docket = GetNewBusinessObject();
			var allowCancel = (ICancellable)Docket;

			CodeDescriptionPairList availableStatuses = new DocketStatus();
			foreach (CodeDescriptionPair status in availableStatuses)
			{
				Docket.WD_DocketStatus = DocketStatus.Codes.Entered;
				Docket.WD_DocketStatus = status.Code;
				if (status.Code == DocketStatus.Codes.Cancelled)
				{
					AssertEquals(null, allowCancel.CanReactivate());
				}
				else
				{
					AssertEquals(WhsDocket.CantReactivateReasonMsg, allowCancel.CanReactivate());
				}
			}
		}

		#endregion

		#endregion

		#region ICanDelete Members

		#region TestICanDelete_CanDelete

		public void TestICanDelete_CanDelete()
		{
			var iCanDelete = (ICanDelete)Docket;
			Docket.WD_DocketStatus = DocketStatus.Codes.New;
			Assert("Should be able to delete NEW Dockets", !CanDeleteOverride || iCanDelete.CanDelete);

			Docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			Assert("Should be able to delete ENTERED Dockets", !CanDeleteOverride || iCanDelete.CanDelete);

			Docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			Assert("Should not be able to delete CANCELLED Dockets", !CanDeleteOverride || !iCanDelete.CanDelete);

			Docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
			Assert("Should not be able to delete FINALIED Dockets", !CanDeleteOverride || !iCanDelete.CanDelete);

			Docket.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			Assert("Should not be able to delete ATTACHED TO PICK Dockets", !CanDeleteOverride || !iCanDelete.CanDelete);

			Docket.WD_DocketStatus = DocketStatus.Codes.Picking;
			Assert("Should not be able to delete PICKING Dockets", !CanDeleteOverride || !iCanDelete.CanDelete);

			Docket.WD_DocketStatus = DocketStatus.Codes.Putaway;
			Assert("Should not be able to delete PUTAWAY Dockets", !CanDeleteOverride || !iCanDelete.CanDelete);

			Docket.WD_DocketStatus = DocketStatus.Codes.Held;
			Assert("Should be able to delete HELD Dockets", !CanDeleteOverride || iCanDelete.CanDelete);
		}

		protected virtual bool CanDeleteOverride
		{
			get { return true; }
		}

		#endregion

		#region TestICanDelete_ReasonForNotAbleToDelete

		public void TestICanDelete_ReasonForNotAbleToDelete()
		{
			var iCanDelete = (ICanDelete)Docket;
			Docket.WD_DocketStatus = DocketStatus.Codes.New;
			if (CanDeleteOverride)
			{
				AssertNull(iCanDelete.ReasonForNotAbleToDelete);
			}

			Docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
			AssertEquals(CantDeleteReasonMsg, iCanDelete.ReasonForNotAbleToDelete);
		}

		protected virtual MultilingualString CantDeleteReasonMsg
		{
			get { return WhsDocket.CantDeleteReasonMsg; }
		}

		#endregion

		#endregion

		#region ICreditControlledDocumentDelivery Members

		#region TestDocumentLoginMessageBoxCallback

		public void TestDocumentLoginMessageBoxCallback()
		{
			ICreditControlledDocumentDelivery docket = GetNewBusinessObject();

			CustomMessageBoxCallback action = (message, caption) => ZDialogResult.Cancel;
			docket.DocumentLoginMessageBoxCallback = action;
			AssertEquals(action, docket.DocumentLoginMessageBoxCallback);
		}

		#endregion

		#region TestGetDocumentLoginAndRaiseOnGetDocumentLogin

		public void TestGetDocumentLoginAndRaiseOnGetDocumentLogin()
		{
			bool wasGetDocumentLoginCalled = false;
			ICreditControlledDocumentDelivery docket = GetNewBusinessObject();
			docket.GetDocumentLogin += (sender, e) => wasGetDocumentLoginCalled = true;
			AssertEquals(false, wasGetDocumentLoginCalled);

			docket.RaiseOnGetDocumentLogin(new SecurityLoginEventArgs((NoResString)string.Empty, (NoResString)string.Empty, null));
			AssertEquals(true, wasGetDocumentLoginCalled);
		}

		#endregion

		#region TestDescriptionOfOrganisationBeingCheckedForCredit

		public void TestDescriptionOfOrganisationBeingCheckedForCredit()
		{
			var docket = GetNewBusinessObject();
			ICreditControlledDocumentDelivery iCreditControlledDocumentDelivery = docket;
			AssertEquals("Warehouse Client", iCreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit);
		}

		#endregion

		public void TestOrganisationsForCreditChecks()
		{
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = ZGuid.Empty;
			AssertEquals(0, ((ICreditControlledDocumentDelivery)docket).OrganisationsForCreditChecks.Length);
			var client = Helper.CreateClient("WHSCLIENT");
			docket.WD_OH_Client = client.PK;
			AssertEquals(RequiresCreditCheck ? 1 : 0, ((ICreditControlledDocumentDelivery)docket).OrganisationsForCreditChecks.Length);
			if (RequiresCreditCheck)
			{
				AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
				AssertEquals(client, ((ICreditControlledDocumentDelivery)docket).OrganisationsForCreditChecks[0]);
				AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

				Action<string> setupOrganizationsEvaluatedForCreditControlRegistry = organizationType =>
				{
					var orgCreditControlCollection = new OrgsEvaluatedForCreditControlCollection();
					if (!string.IsNullOrEmpty(organizationType))
					{
						var orgCreditControl = orgCreditControlCollection.AddNew();
						orgCreditControl.JobType = docket.InvoicingSupporter.ConsumerType.Code;
						orgCreditControl.INCOTerm = AccountingMasterFilesConstants.INCOTermCodes.All;
						orgCreditControl.FreightPaymentTerm = AccountingMasterFilesConstants.FreightPaymentTermCodes.All;
						orgCreditControl.OrganizationType = organizationType;
					}
					AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgCreditControlCollection);
					Factory.ClearCachedValue<OrgsEvaluatedForCreditControlCollection>(AccountingMasterFilesRegistry.OrganizationsEvaluatedForCreditControlCacheKey());
				};

				setupOrganizationsEvaluatedForCreditControlRegistry(OrgCodes.AllDebtors);
				AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
				AssertEquals(1, ((ICreditControlledDocumentDelivery)docket).OrganisationsForCreditChecks.Length);
				AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);
			}
		}

		#region IsDPSFreightMovementRestricted

		public void TestIsDPSFreightMovementRestricted() => TestIsDPSFreightMovementRestrictedCore();

		protected virtual void TestIsDPSFreightMovementRestrictedCore()
		{
			ICreditControlledDocumentDelivery docket = GetNewBusinessObject();
			AssertEquals(false, docket.IsDPSFreightMovementRestricted);
		}

		public void TestIsDPSFreightMovementRestricted_RespectsDPSFreightMovementRestrictionsRegistry()
		{
			var docket = GetNewBusinessObject();
			AssertEquals("Precondition: default screening status.", ScreeningStatusesList.Codes.NotScreened, docket.WD_ScreeningStatus);

			var dpsFreightRestrictionOptionAndExpectedResult = new[]
			{
				(DPSFreightMovementRestrictionsOptions.Codes.All, TestIsDPSFreightMovementRestricted_All),
				(DPSFreightMovementRestrictionsOptions.Codes.Exp, false),
				(DPSFreightMovementRestrictionsOptions.Codes.No, false)
			};

			foreach (var (restrictionOption, expectedResult) in dpsFreightRestrictionOptionAndExpectedResult)
			{
				using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, restrictionOption))
				{
					AssertEquals($"IsDPSFreightMovementRestricted is correct with registry DPS restriction option '{restrictionOption}' should be {expectedResult}.",
						expectedResult,
						((ICreditControlledDocumentDelivery)docket).IsDPSFreightMovementRestricted);
				}
			}
		}

		protected virtual bool TestIsDPSFreightMovementRestricted_All => false;

		#endregion

		#region TestJobNumber

		public void TestJobNumber()
		{
			ICreditControlledDocumentDelivery docket = GetNewBusinessObject();
			AssertEquals(0, docket.JobNumber.Length);
		}

		#endregion

		protected virtual bool RequiresCreditCheck
		{
			get { return true; }
		}

		#endregion

		#region ICustomFieldProvider Members

		#region TestICustomFieldProvider_GetCustomBusinessObject

		public void TestICustomFieldProvider_GetCustomBusinessObject()
		{
			if (!string.IsNullOrEmpty(WorkflowDescriptorCode))
			{
				var template = Helper.CreateWorkflowTemplate("T1", WorkflowDescriptorCode);
				Helper.AddCustomField(template, "stringField", AddOnColumnDataType.Codes.String);
				Helper.AddCustomField(template, "intField", AddOnColumnDataType.Codes.Integer);
				Helper.AddCustomField(template, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
				Helper.AddCustomField(template, "boolField", AddOnColumnDataType.Codes.Boolean);

				Factory.Save();

				var docketProvider = (ICustomFieldProvider)GetNewBusinessObject();
				var docketCustomBizo = docketProvider.GetCustomBusinessObject();
				var docketDynamicBizo = (IDynamicBusinessObject)docketCustomBizo;

				AssertNotNull(docketDynamicBizo.GetProperty("__STRINGFIELD__prop__ZString"));
				AssertNotNull(docketDynamicBizo.GetProperty("__INTFIELD__prop__ZInt"));
				AssertNotNull(docketDynamicBizo.GetProperty("__DATETIMEFIELD__prop__ZDateTime"));
				AssertNotNull(docketDynamicBizo.GetProperty("__BOOLFIELD__prop__ZBool"));
			}
			else
			{
				Assert("Doesn't Support Workflow Custom Fields", true);
			}
		}

		protected virtual string WorkflowDescriptorCode
		{
			get { return ""; }
		}

		#endregion

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		#region TestConfigOrg

		public void TestConfigOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var docket = GetNewBusinessObject();

			docket.WD_OH_Client = org.PK;
			if (CustomFieldsSupported)
			{
				AssertEquals(org, ((ICustomLabelsConfigOrgProvider)docket).ConfigOrg);

				docket.Delete();
				AssertNull(((ICustomLabelsConfigOrgProvider)docket).ConfigOrg);
			}
			else
			{
				AssertNull(((ICustomLabelsConfigOrgProvider)docket).ConfigOrg);
			}
		}

		protected virtual bool CustomFieldsSupported => true;

		#endregion

		#region TestConfigOrgChanged

		public void TestConfigOrgChanged()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var docket = GetNewBusinessObject();

			var configOrgChangedCalledCount = 0;
			EventHandler configOrgChanged = (s, e) => configOrgChangedCalledCount++;
			((ICustomLabelsConfigOrgProvider)docket).ConfigOrgChanged += configOrgChanged;

			if (CustomFieldsSupported)
			{
				docket.WD_OH_Client = org1.PK;
				AssertEquals("Precondition: Changed the config org.", org1, ((ICustomLabelsConfigOrgProvider)docket).ConfigOrg);
				AssertEquals("Should have called config org changed.", 1, configOrgChangedCalledCount);

				((ICustomLabelsConfigOrgProvider)docket).ConfigOrgChanged -= configOrgChanged;
				docket.WD_OH_Client = org2.PK;
				AssertEquals("Precondition: Changed the config org.", org2, ((ICustomLabelsConfigOrgProvider)docket).ConfigOrg);
				AssertEquals("Should *not* have called config org changed.", 1, configOrgChangedCalledCount);
			}
			else
			{
				AssertNull("Precondition: Organisation is null.", ((ICustomLabelsConfigOrgProvider)docket).ConfigOrg);

				docket.WD_OH_Client = org1.PK;
				docket.WD_OH_Client = org2.PK;
				AssertEquals("Should *not* have called config org changed.", 0, configOrgChangedCalledCount);
			}
		}

		#endregion

		#region TestConfigOrgChanged_DeletedObject_Add

		public void TestConfigOrgChanged_DeletedObject_Add()
		{
			var docket = GetNewBusinessObject();

			docket.Delete();
			AssertEquals("Precondition: Deleted.", true, docket.IsDeleted);

			if (CustomFieldsSupported)
			{
				var configOrgChangedCalled = false;
				((ICustomLabelsConfigOrgProvider)docket).ConfigOrgChanged += (s, e) => configOrgChangedCalled = true;

				docket.WD_OH_ClientInfo.RefreshBinding();
				AssertEquals("Should *not* have called config org changed.", false, configOrgChangedCalled);
			}
		}

		#endregion

		#region TestConfigOrgChanged_DeletedObject_Remove

		public void TestConfigOrgChanged_DeletedObject_Remove()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var docket = GetNewBusinessObject();

			var configOrgChangedCalledCount = 0;
			EventHandler configOrgChanged = (s, e) => configOrgChangedCalledCount++;
			((ICustomLabelsConfigOrgProvider)docket).ConfigOrgChanged += configOrgChanged;

			docket.WD_OH_ClientInfo.RefreshBinding();
			if (CustomFieldsSupported)
			{
				AssertEquals("Should have called config org changed.", 1, configOrgChangedCalledCount);

				docket.Delete();
				AssertEquals("Precondition: Deleted.", true, docket.IsDeleted);

				((ICustomLabelsConfigOrgProvider)docket).ConfigOrgChanged -= configOrgChanged;
				docket.WD_OH_ClientInfo.RefreshBinding();
				AssertEquals("Should *not* have called config org changed.", 1, configOrgChangedCalledCount);
			}
			else
			{
				AssertEquals("Should *not* have called config org changed.", 0, configOrgChangedCalledCount);
			}
		}

		#endregion

		#endregion

		#region IPropertyChecker Members

		public void TestFieldUpdaterUpdate_WhenUpdateDocketStatus_ShouldFail()
		{
			var docket = GetNewBusinessObject();
			var propertyInfo = docket.GetType().GetProperty(nameof(docket.WD_DocketStatus));

			var outMessage = string.Empty;
			var checkResult = ((IPropertyChecker)docket).IsPropertyUpdatableViaXueAdditionalFields(propertyInfo, "NEW", out outMessage);

			AssertEquals("Can't set WD_DocketStatus", false, checkResult);
			AssertEquals("Should return error message", "Docket Status is read-only field, it can not be set.", outMessage);
		}

		#endregion

		#region ICustomLabelsProvider Members

		public void TestCustomLabelsProvider()
		{
			var docket = GetNewBusinessObject();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			docket.WD_OH_Client = org.PK;

			var provider = new WhsDocket.CustomLabelsProvider(docket);
			AssertEquals(docket, provider.ConfigOrgProvider);

			var list = provider.GetCustomFields(org, Factory);
			AssertEquals("the client", list.ConfigOrgLocatedAt);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
						WhsDocketSchema.WD_CustomAttrib1.Name,
						WhsDocketSchema.WD_CustomAttrib2.Name,
						WhsDocketSchema.WD_CustomAttrib3.Name,
						WhsDocketSchema.WD_CustomAttrib4.Name,
						WhsDocketSchema.WD_CustomAttrib5.Name,
						WhsDocketSchema.WD_CustomDate1.Name,
						WhsDocketSchema.WD_CustomDate2.Name,
						WhsDocketSchema.WD_CustomDecimal1.Name,
						WhsDocketSchema.WD_CustomDecimal2.Name,
						WhsDocketSchema.WD_CustomDecimal3.Name,
						WhsDocketSchema.WD_CustomDecimal4.Name,
						WhsDocketSchema.WD_CustomDecimal5.Name,
						WhsDocketSchema.WD_CustomFlag1.Name,
						WhsDocketSchema.WD_CustomFlag2.Name,
						WhsDocketSchema.WD_CustomFlag3.Name,
						WhsDocketSchema.WD_CustomFlag4.Name,
						WhsDocketSchema.WD_CustomFlag5.Name,
				}, list.Cast<CustomLabelInfo>().Select(i => i.PropertyName));
		}

		#endregion

		#region IDocketInternals Members

		[TestDate(2005, 1, 20)]
		public void TestIDocketInternalsFlagDocketAsFinalised()
		{
			TestIDocketInternalsFlagDocketAsFinalisedCore();
		}

		protected virtual void TestIDocketInternalsFlagDocketAsFinalisedCore()
		{
			var docket = GetNewBusinessObject();
			AssertEquals(DocketStatus.Codes.New, docket.WD_DocketStatus);
			AssertEquals(ZDateTimeOffset.Empty, docket.WD_FinalisedDate);

			((IDocketInternals)docket).FlagDocketAsFinalised();

			AssertEquals(DocketStatus.Codes.Finalised, docket.WD_DocketStatus);
			var now = docket.Warehouse.GetWarehouseBranchDateTimeOffset(ZDateTime.Now);
			now = new ZDateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 34); // ensure that seconds != 0.
			AssertNotEquals(now, docket.WD_FinalisedDate);

			now = new ZDateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0); // ensure that seconds = 0
			AssertEquals(now, docket.WD_FinalisedDate);
		}

		#endregion

		#region IDocManagerSupport Members

		public abstract void TestDocManagerInfo();

		#endregion

		#region IHaveServices Members

		#region TestServices

		public void TestServices()
		{
			WhsJobService service = Factory.New<WhsJobService>();
			service.ES_ParentID = Docket.PK;
			service.ES_ParentTableCode = Docket.TablePrefix;

			AssertCollectionContains("Collection was not loaded and/or the relationship filter is incorrect.", service, Docket.Services);
			AssertEquals(typeof(WhsJobService), docket.Services.TypeOfElements);
			AssertEquals(true, Docket.IsRegisteredEditableChildObject(Docket.Services));
		}

		#endregion

		#region TestIHaveServices

		public void TestIHaveServices()
		{
			IHaveServices iHaveServices = Docket;

			AssertEquals("", iHaveServices.ContainerMode);
			AssertEquals("", iHaveServices.TransportMode);
			AssertEquals(0, iHaveServices.DependentServiceParents.Length);
			AssertEquals(Docket, iHaveServices.ServiceParent);
			AssertEquals(Docket.TablePrefix, iHaveServices.TableCode);
		}

		public void TestServiceBranch()
		{
			var docket = Docket;
			var iHaveServices = (IHaveServices)docket;
			AssertNull("No service branch", iHaveServices.ServiceBranch);

			var warehouse = Helper.CreateWarehouse("Whs");
			AssertEquals(true, warehouse.WW_GB_RelatedCompanyBranch.IsValid);

			docket.WD_WW_Whs = warehouse.PK;
			AssertEquals("Service branch is warehouse branch", warehouse.WW_GB_RelatedCompanyBranch, iHaveServices.ServiceBranch.PK);
		}

		#endregion

		#endregion

		#region IWhsDocket

		public void TestIWhsDocketProperties()
		{
			var orgPK = ZGuid.NewZGuid();
			var pickPK = ZGuid.NewZGuid();
			var parentPickPK = ZGuid.NewZGuid();
			var arrivalDate = new ZDateTimeOffset(2019, 12, 5);
			var requiredDate = new ZDateTimeOffset(2019, 4, 27);
			var finalisedDate = new ZDateTimeOffset(2019, 8, 2);

			var data = new TestDataSimpleEnvironment(Factory);
			var docket = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			docket.WD_ArrivalDate = arrivalDate;
			docket.WD_RequiredDate = requiredDate;
			docket.WD_CustomerReference = "CustomerRefTest";
			docket.WD_DocketID = "D1";
			docket.WD_DocketSubType = ReceiveType.Codes.Customs;
			docket.WD_DocketType = "INW";
			docket.WD_ExternalReference = "REC434";
			docket.WD_FinalisedDate = finalisedDate;
			docket.WD_TotalUnits = 15m;
			docket.SupplierDocAddress.OrganisationPK = orgPK;
			docket.WD_WP = pickPK;
			docket.WD_RS_NKServiceLevel = "STD";
			docket.WD_WP_ParentPickForTransfer = parentPickPK;
			docket.WD_TransportReference = "Ted433";

			CombineAssertions(() =>
			{
				AssertEquals("Interface Arrival Date is correct", arrivalDate, ((IWhsDocket)docket).WD_ArrivalDate);
				AssertEquals("Interface Required Date is correct", requiredDate, ((IWhsDocket)docket).WD_RequiredDate);
				AssertEquals("Interface Customer Reference is correct", "CustomerRefTest", ((IWhsDocket)docket).WD_CustomerReference);
				AssertEquals("Interface DocketID is correct", "D1", ((IWhsDocket)docket).WD_DocketID);
				AssertEquals("Interface WD_DocketSubType is correct", ReceiveType.Codes.Customs, ((IWhsDocket)docket).WD_DocketSubType);
				AssertEquals("Interface WD_DocketType is correct", "INW", ((IWhsDocket)docket).WD_DocketType);
				AssertEquals("Interface WD_ExternalReference is correct", "REC434", ((IWhsDocket)docket).WD_ExternalReference);
				AssertEquals("Interface WD_FinalisedDate is correct", finalisedDate, ((IWhsDocket)docket).WD_FinalisedDate);
				AssertEquals("Interface WD_TotalUnits is correct", 15m, ((IWhsDocket)docket).WD_TotalUnits);
				AssertEquals("Interface SupplierPK is correct", orgPK, ((IWhsDocket)docket).SupplierPK);
				AssertEquals("Interface WD_WP is correct", pickPK, ((IWhsDocket)docket).WD_WP);
				AssertEquals("Interface Service Level is correct", "STD", ((IWhsDocket)docket).WD_RS_NKServiceLevel);
				AssertEquals("Interface WD_WP_ParentPickForTransfer is correct", parentPickPK, ((IWhsDocket)docket).WD_WP_ParentPickForTransfer);
				AssertEquals("Interface WD_TransportReference is correct", "Ted433", ((IWhsDocket)docket).WD_TransportReference);
			});
		}

		#endregion

		#region ISendEmailSource members

		public void TestGetTemplateCategory()
		{
			AssertEquals(ExpectedTemplateCategory, ((ISendEmailSource)Docket).TemplateCategory);
		}

		protected virtual string ExpectedTemplateCategory
		{
			get { return MailTemplateCategoryList.Codes.None; }
		}

		public void TestGetEmailSubject()
		{
			AssertEquals(Docket.Description + " - " + Docket.WD_DocketID, ((ISendEmailSource)Docket).EmailSubject);
		}

		public void TestGetAddressBookSelection()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.Contacts.AddNew();

			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = client.PK;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.Contacts.AddNew();
			SetupConsigneeForAddressBookSelection(docket, consignee);

			var goodsBillTo = Factory.NewWithValidTestData<OrgHeader>();
			goodsBillTo.Contacts.AddNew();
			docket.GoodsBillToPK = goodsBillTo.PK;

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.Contacts.AddNew();
			docket.SupplierDocAddress.OrganisationPK = supplier.PK;

			var transportCo = Factory.NewWithValidTestData<OrgHeader>();
			if (docket is IJobWithTransportCompany job)
			{
				transportCo.Contacts.AddNew();
				job.TransportCoPK = transportCo.PK;
			}

			var transportBillTo = Factory.NewWithValidTestData<OrgHeader>();
			transportBillTo.Contacts.AddNew();
			docket.TransportBillToDocAddress.OrganisationPK = transportBillTo.PK;

			var selection = ((ISendEmailSource)docket).GetAddressBookSelection();
			AssertEquals(ExpectedRecipientCount, selection.Recipients.Count);

			CheckForRecipients(client, consignee, goodsBillTo, supplier, transportCo, transportBillTo, selection);
		}

		protected virtual void SetupConsigneeForAddressBookSelection(TDocket docket, OrgHeader consignee)
		{
		}

		protected virtual void CheckForRecipients(OrgHeader client, OrgHeader consignee, OrgHeader goodsBillTo, OrgHeader supplier, OrgHeader transportCo, OrgHeader transportBillTo, AddressBookSelection selection)
		{
			AssertRecipient(client, selection);
		}

		protected void AssertRecipient(OrgHeader org, AddressBookSelection selection)
		{
			// Do not use AssertCollectionContains; it will not use AddressBookRecipientCollection's Contains
			Assert(selection.Recipients.Contains(org.Contacts[0]));
		}

		protected virtual ZInt ExpectedRecipientCount
		{
			get { return 1; }
		}

		#endregion

		#region IJobHeaderParent Members

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			var docket = (IJobHeaderParent)GetNewBusinessObject();
			Assert(docket.AllowInvoiceDeletion);
		}

		#endregion

		#region TestIJobInvoicingPlugIn Members

		public void TestIJobInvoicingPlugIn_JobNumber()
		{
			var docket = GetNewBusinessObject();
			var iDocket = ((IJobInvoicingPlugIn)docket);
			AssertEquals("Precondition", ZString.Empty, iDocket.JobNumber);

			docket.WD_DocketID = "W99999";
			AssertEquals("W99999", iDocket.JobNumber);
		}

		#endregion

		#region IJobInvoicingAdditionalData Members

		public void TestGetAdditionalProperties()
		{
			var client = Helper.CreateClient("Client");
			var part1 = Helper.CreateProduct(client, "P1");
			Helper.CreateProductUnit(part1, "KG", "UNT", 2m);
			Helper.CreateProductUnit(part1, "M3", "UNT", 3m);

			var transportCo = Helper.CreateClient("TransportCo");
			var otherDocket = GetNewBusinessObject();
			otherDocket.WD_DocketID = "Docket ID";
			otherDocket.WD_ExternalReference = "External Reference";
			otherDocket.WD_CustomerReference = "Customer Reference";

			var jobWithTransportCo = otherDocket as IJobWithTransportCompany;
			if (jobWithTransportCo != null)
			{
				jobWithTransportCo.TransportCoPK = transportCo.PK;
			}

			otherDocket.WD_PL_NKCarrierServiceLevel = "AAA";

			var job = Helper.CreateRatingJob(otherDocket);
			var charge1 = Helper.CreateJobCharge(job);
			var charge2 = Helper.CreateJobCharge(job);
			charge1.JobChargeAttributes.RemoveAndDeleteAll();
			charge2.JobChargeAttributes.RemoveAndDeleteAll();

			var additionalData = Docket as IJobInvoicingAdditionalData;
			AssertNotNull(additionalData);
			var additionalProperties = additionalData.GetAdditionalProperties();

			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.DocketID].GetValue(charge1));
			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.CustomerReference].GetValue(charge1));
			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.TransportCo].GetValue(charge1));
			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ServiceLevel].GetValue(charge1));
			AssertEquals(0m, additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.Weight].GetValue(charge1));
			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.WeightUQ].GetValue(charge1));
			AssertEquals(0m, additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.Volume].GetValue(charge1));
			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.VolumeUQ].GetValue(charge1));

			AddJobChargeAttrib(charge2, JobChargeAttribTypeList.Codes.DocketReference, "External Reference");
			AddJobChargeAttrib(charge2, JobChargeAttribTypeList.Codes.Product, "P1");
			AddJobChargeAttrib(charge2, JobChargeAttribTypeList.Codes.ItemsToRate, "10");
			AddJobChargeAttrib(charge2, JobChargeAttribTypeList.Codes.UnroundedItemsToRate, "10");

			AssertEquals(otherDocket.WD_DocketID, additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.DocketID].GetValue(charge2));
			AssertEquals(otherDocket.WD_CustomerReference, additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.CustomerReference].GetValue(charge2));

			if (jobWithTransportCo != null)
			{
				AssertEquals("TransportCo", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.TransportCo].GetValue(charge2));
			}

			AssertEquals("AAA", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ServiceLevel].GetValue(charge2));
			AssertEquals(20m, additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.Weight].GetValue(charge2));
			AssertEquals("KG", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.WeightUQ].GetValue(charge2));
			AssertEquals(30m, additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.Volume].GetValue(charge2));
			AssertEquals("M3", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.VolumeUQ].GetValue(charge2));

			TestGetAdditionalProperties_Core(otherDocket, charge2, additionalProperties);
		}

		protected virtual void TestGetAdditionalProperties_Core(TDocket docket, JobCharge charge, CustomPropertyContainer<JobCharge> properties)
		{
		}

		void AddJobChargeAttrib(JobCharge charge, string name, string value)
		{
			JobChargeAttrib attrib = charge.JobChargeAttributes.AddNew();
			attrib.EC_Name = name;
			attrib.EC_Value = value;
		}

		#endregion

		#region IRelatedJob Members

		public void TestIRelatedJob()
		{
			IRelatedJob job = Docket;
			AssertEquals(ExpectedControllerId, job.ControllerID);
			AssertEquals(Docket.WD_ExternalReference, job.JobDescription);
			AssertEquals(Docket.WD_DocketID, job.JobNumber);
			AssertEquals(Docket.WD_DocketStatus, job.JobStatus);
		}

		protected abstract ControllerID ExpectedControllerId { get; }

		#endregion

		#region ITaskPlanningJob

		protected virtual ZString HumanReadableNameWithoutID => null;

		protected virtual bool SupportsPlanningStatus(WhsDocket docket) => false;

		public void TestITaskPlanningJob_BasicProperties()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			var iNumberFountainEntityWithID = (INumberFountainEntityWithID)docket;
			iNumberFountainEntityWithID.ID = "WD00000001";

			var job = (ITaskPlanningJob)docket;
			CombineAssertions(() =>
			{
				AssertEquals(data.Whs1.PK, job.WarehousePK);
				AssertEquals(Factory, job.Factory);
				AssertEquals("WD00000001", job.JobID);
				AssertEquals(HumanReadableNameWithoutID, job.HumanReadableNameWithoutID);
			});
		}

		public void TestITaskPlanningJob_TaskPlanningStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			var job = (ITaskPlanningJob)docket;
			AssertEquals(string.Empty, job.TaskPlanningStatus);

			job.TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertEquals("RFP", job.TaskPlanningStatus);
		}

		public void TestITaskPlanningJob_GetCannotUpdateTaskPlanningStatusReason()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			Factory.Save();

			var job = (ITaskPlanningJob)docket;
			CombineAssertions(() =>
			{
				AssertEquals(true, job.IsInDatabase);
				AssertEquals(false, job.HasChanges);
				AssertEquals(false, job.IsFinalisedOrCancelled);
				AssertEquals(SupportsPlanningStatus(docket) ? string.Empty : "The job does not support Task Planning Status.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
				AssertEquals(SupportsPlanningStatus(docket) ? string.Empty : "The job does not support Task Planning Status.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
			});
		}

		public void TestITaskPlanningJob_GetCannotUpdateTaskPlanningStatusReason_NotInDatabase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			var job = (ITaskPlanningJob)docket;
			AssertEquals(false, job.IsInDatabase);
			AssertEquals(SupportsPlanningStatus(docket) ? string.Format("Cannot change Task Planning Status as the {0} is not saved.", job.HumanReadableNameWithoutID) : "The job does not support Task Planning Status.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
			AssertEquals(SupportsPlanningStatus(docket) ? string.Format("Cannot change Task Planning Status as the {0} is not saved.", job.HumanReadableNameWithoutID) : "The job does not support Task Planning Status.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
		}

		public void TestITaskPlanningJob_GetCannotUpdateTaskPlanningStatusReason_ValueChange()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			Factory.Save();

			var job = (ITaskPlanningJob)docket;
			AssertEquals(SupportsPlanningStatus(docket) ? string.Empty : "The job does not support Task Planning Status.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
			AssertEquals(SupportsPlanningStatus(docket) ? string.Empty : "The job does not support Task Planning Status.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));

			job.TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertEquals(true, job.HasChanges);
			AssertEquals(SupportsPlanningStatus(docket) ? string.Format("Cannot change Task Planning Status as the {0} is not saved.", job.HumanReadableNameWithoutID) : "The job does not support Task Planning Status.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
			AssertEquals(SupportsPlanningStatus(docket) ? string.Format("Cannot change Task Planning Status as the {0} is not saved.", job.HumanReadableNameWithoutID) : "The job does not support Task Planning Status.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
		}

		public void TestITaskPlanningJob_GetCannotUpdateTaskPlanningStatusReason_Cancelled()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			Factory.Save();

			var job = (ITaskPlanningJob)docket;
			AssertEquals(true, job.IsFinalisedOrCancelled);
			AssertEquals(SupportsPlanningStatus(docket) ? string.Format("Cannot change Task Planning Status as the {0} is finalized or canceled.", job.HumanReadableNameWithoutID) : "The job does not support Task Planning Status.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
			AssertEquals(SupportsPlanningStatus(docket) ? string.Format("Cannot change Task Planning Status as the {0} is finalized or canceled.", job.HumanReadableNameWithoutID) : "The job does not support Task Planning Status.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
		}

		public void TestITaskPlanningJob_GetCannotUpdateTaskPlanningStatusReason_Finalised()
		{
			var whs = Helper.CreateWarehouse("1");
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = Helper.CreateClient().PK;
			docket.WD_WW_Whs = whs.PK;

			CreateNewPick(docket, whs);
			SetFinsalisedDocketValuesForTest(docket);
			Factory.Save();

			var job = (ITaskPlanningJob)docket;
			AssertEquals(true, job.IsFinalisedOrCancelled);
			AssertEquals(SupportsPlanningStatus(docket) ? string.Format("Cannot change Task Planning Status as the {0} is finalized or canceled.", job.HumanReadableNameWithoutID) : "The job does not support Task Planning Status.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
			AssertEquals(SupportsPlanningStatus(docket) ? string.Format("Cannot change Task Planning Status as the {0} is finalized or canceled.", job.HumanReadableNameWithoutID) : "The job does not support Task Planning Status.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
		}

		#endregion

		#region IWhsLogEventParent

		public void TestEventFreeTextReference()
		{
			var docket = GetNewBusinessObject();
			var logParent = (IWhsLogEventParent)GetNewBusinessObject();
			AssertEquals(docket.WD_DocketID, logParent.EventFreeTextReference);
		}

		public void TestIWhsLogEventParentWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewBusinessObject();
			docket.WD_WW_Whs = data.Whs1.PK;
			IWhsLogEventParent logParent = docket;
			AssertEquals(data.Whs1, logParent.Warehouse);
		}

		public void TestEventReferenceParameterType()
		{
			var logParent = (IWhsLogEventParent)GetNewBusinessObject();
			AssertEquals(ExpectedEventReferenceParameterType, logParent.EventReferenceParameterType);
		}

		protected abstract string ExpectedEventReferenceParameterType { get; }

		#endregion

		#region TestICriticalChangesVersionID

		public void TestUpdateWhsDocketVersion_GenericDocketChange()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = Docket;
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			Factory.Save();

			if (docket is ICriticalChangesVersionID)
			{
				var docketVersion = docket.WD_CriticalChangesVersionID;
				UpdatedDocketVersionAndSubscribeToFactory(Factory, docket);
				Factory.Save();

				AssertEquals("Should update version.", true, docket.WD_CriticalChangesVersionID.IsValid);
				AssertNotEquals("Should update version.", docketVersion, docket.WD_CriticalChangesVersionID);
			}
			else
			{
				Assert(docket.WD_CriticalChangesVersionID.IsEmpty);
			}
		}

		public void TestUpdateWhsDocketVersion_AddDocketLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = Docket;
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			Factory.Save();

			if (docket is ICriticalChangesVersionID)
			{
				var docketVersion = docket.WD_CriticalChangesVersionID;
				PrepareValidDocketLineForICriticalChangesTest(Factory, docket, data.Part1, data.Whs1.FindLocation("A-1"));
				Factory.Save();

				AssertEquals("Should update version.", true, docket.WD_CriticalChangesVersionID.IsValid);
				AssertNotEquals("Should update version after adding line.", docketVersion, docket.WD_CriticalChangesVersionID);
			}
			else
			{
				Assert(docket.WD_CriticalChangesVersionID.IsEmpty);
			}
		}

		public void TestUpdateWhsDocketVersion_DeleteDocketLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = Docket;
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			Factory.Save();

			if (docket is ICriticalChangesVersionID)
			{
				var docketLine = PrepareValidDocketLineForICriticalChangesTest(Factory, docket, data.Part1, data.Whs1.FindLocation("A-1"));
				Factory.Save();

				var docketVersion = docket.WD_CriticalChangesVersionID;
				docketLine.Delete();
				Factory.Save();
				AssertEquals("Should update version.", true, docket.WD_CriticalChangesVersionID.IsValid);
				AssertNotEquals("Should update version after adding line.", docketVersion, docket.WD_CriticalChangesVersionID);
			}
			else
			{
				Assert(docket.WD_CriticalChangesVersionID.IsEmpty);
			}
		}

		public void TestUpdateWhsDocketVersion_ClearVersionAfterFinalise()
		{
			var docket = SetupForTestFinaliseDocket();
			Factory.Save();

			if (docket is ICriticalChangesVersionID)
			{
				var docketVersion = docket.WD_CriticalChangesVersionID;
				docket.FinaliseDocket();
				Factory.Save();

				AssertNotEquals("Should update version.", docketVersion, docket.WD_CriticalChangesVersionID);
				Assert("Version should be empty on finalise.", docket.WD_CriticalChangesVersionID.IsEmpty);
			}
			else
			{
				Assert(docket.WD_CriticalChangesVersionID.IsEmpty);
			}
		}

		public void TestUpdateWhsDocketVersion_UpdateOnce()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = Docket;
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			Factory.Save();

			if (docket is ICriticalChangesVersionID)
			{
				var changed = 0;
				docket.WD_CriticalChangesVersionIDInfo.ValueChanged += (object sender, EventArgs e) =>
				{
					changed++;
				};

				PrepareValidDocketLineForICriticalChangesTest(Factory, docket, data.Part1, data.Whs1.FindLocation("A-1"));
				PrepareValidDocketLineForICriticalChangesTest(Factory, docket, data.Part1, data.Whs1.FindLocation("A-2"));

				AssertEquals("Precondition.", 0, changed);
				Factory.Save();

				AssertEquals("Should only call once.", 1, changed);
			}
			else
			{
				Assert(docket.WD_CriticalChangesVersionID.IsEmpty);
			}
		}

		void UpdatedDocketVersionAndSubscribeToFactory(BusinessObjectFactory factory, WhsDocket docket)
			=> UpdatedDocketVersionAndSubscribeToFactoryCore(factory, docket);

		protected virtual void UpdatedDocketVersionAndSubscribeToFactoryCore(BusinessObjectFactory factory, WhsDocket docket)
		{
		}

		WhsDocketLine PrepareValidDocketLineForICriticalChangesTest(BusinessObjectFactory factory, WhsDocket docket, OrgSupplierPart product, WhsLocation location)
			=> PrepareValidDocketLineForICriticalChangesTestCore(factory, docket, product, location);

		protected virtual WhsDocketLine PrepareValidDocketLineForICriticalChangesTestCore(BusinessObjectFactory factory, WhsDocket docket, OrgSupplierPart product, WhsLocation location)
		{
			var newLine = docket.Lines.AddNew();
			newLine.WE_OP = product.PK;
			newLine.WE_TransactionQuantity = 1m;
			newLine.WE_WL = location.PK;
			return newLine;
		}

		#endregion

		#region Job Header Deactivation Tests

		public void TestJobHeaderIsNotDeactivatedWhenFactoryHasInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(true);
		}

		public void TestJobHeaderIsDeactivatedWhenFactoryDoesNotHaveInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(false);
		}

		void AssertJobHeaderDeactivation(bool hasInvoicingPlugInGUIContext)
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			Docket.WD_OH_Client = org.PK;
			Docket.WD_WW_Whs = whs.PK;
			var jobLoader = new JobHeader.Loader(Docket);
			var job = jobLoader.TryCreate();
			Factory.Save();

			Docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			if (hasInvoicingPlugInGUIContext)
			{
				Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			}

			var assertionMessage1 = string.Format("Docket {0} have InvoicingPluginGUI Business Context", hasInvoicingPlugInGUIContext ? "should" : "should not");
			var assertionMessage2 = string.Format("Job {0} be deactivated by OnSaving method", hasInvoicingPlugInGUIContext ? "should not" : "should");

			Assert("Deactivating Docket, IsCancelled flag should be set to true", Docket.IsCancelled);
			Assert("Deactivating Docket, WD_DocketStatusInfo should have changes", Docket.WD_DocketStatusInfo.HasChanges);
			AssertEquals(assertionMessage1, hasInvoicingPlugInGUIContext, Docket.HasContext(BusinessContext.InvoicingPlugInGUI));
			Assert("Job is not yet decativated", !job.IsCancelled);

			Factory.Save();

			AssertEquals(assertionMessage2, !hasInvoicingPlugInGUIContext, job.IsCancelled);
		}

		#endregion

		#region TestDocketCacheWasRefreshedProperlyAfterRevenueWasPosted

		public void TestDocketCacheWasRefreshedProperlyAfterRevenueWasPosted()
		{
			var docket = GetNewBusinessObject();

			AssertEquals("Precondition:", 0, docket.AccTranLines.Count);
			AssertEquals("Precondition:", false, docket.IsAtleastOneInvoiceLinePosted);

			bool wasRefreshBindingInvoked = false;
			((IBindingList)docket).ListChanged += delegate
			{ wasRefreshBindingInvoked = true; };

			// create Non-Posted charge line
			Helper.CreateAccountingDataWithWIPCharge(docket);
			AssertEquals("Docket has a Charge, but cache was not refreshed.", 0, docket.AccTranLines.Count);
			AssertEquals("There is not line was posted", false, docket.IsAtleastOneInvoiceLinePosted);
			AssertEquals("Refresh Binding should not have been called yet.", false, wasRefreshBindingInvoked);

			// create Posted charge line
			Helper.AddChargeLine(docket);
			AssertEquals("Docket has a Posted REV Charge, but the cache has not been refreshed.", 0, docket.AccTranLines.Count);
			AssertEquals("Docket has a Posted REV Charge, but the cache has not been refreshed.", false, docket.IsAtleastOneInvoiceLinePosted);
			AssertEquals("Refresh Binding should not have been called yet.", false, wasRefreshBindingInvoked);

			// mimic Accounting Posting Functionality
			docket.InvoicingSupporter.PostedStateChanged();
			AssertEquals("Docket now has a Charge Line with REV (means it's Posted), and cache refreshed, so there are two lines in cache.", 2, docket.AccTranLines.Count);
			AssertEquals("Docket has a Posted REV Charge, and cache was refreshed, a Charge Line with REV (means it's Posted)", true, docket.IsAtleastOneInvoiceLinePosted);
			AssertEquals("Refresh Binding should have been called in PostedStateChanged so the UI can refresh.", true, wasRefreshBindingInvoked);
		}

		#endregion

		#region TestDocketUniversalCopyIgnoreElement

		public void TestDocketUniversalCopyIgnoreElement()
		{
			var ignoreElements = new[] { "ParentDocket", "Split", "WhsInventoryView", "Whs" };

			var docketAllPropertiesShowsInUCTree = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(typeof(TDocket), includeInterfaces: true, useUniversalCopyIgnoreElementAttribute: false);
			AssertCollectionContains("Precondition: Should include elements that will be filtered.", docketAllPropertiesShowsInUCTree, (i) => ignoreElements.Contains(i.Name));

			var docketPropertiesShowsInUCTree = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(typeof(TDocket), includeInterfaces: true, useUniversalCopyIgnoreElementAttribute: true);
			AssertCollectionContains("UniversalCopyIgnoreElement should ignore elements.", docketPropertiesShowsInUCTree, (i) => !ignoreElements.Contains(i.Name));
		}

		#endregion

		#region TestDocketUpdatedByDataRefresh

		public void TestDocketUpdatedByDataRefresh_FinalisedInMemory()
		{
			TestDocketUpdatedByDataRefresh_FinalisedInMemoryCore();
		}

		protected abstract void TestDocketUpdatedByDataRefresh_FinalisedInMemoryCore();

		public void TestDocketUpdatedByDataRefresh_CriticalChangesVersionIDUpdate()
		{
			TestDocketUpdatedByDataRefresh_CriticalChangesVersionIDUpdateCore();
		}

		protected abstract void TestDocketUpdatedByDataRefresh_CriticalChangesVersionIDUpdateCore();

		#endregion

		#region Implementation

		protected virtual void AssertDocketLineEqualsInventory(WhsInventoryView inventory, WhsDocketLine line)
		{
			AssertEquals("WE_OP", inventory.WI_OP, line.WE_OP);
			AssertEquals("WE_F3_NKPackType", inventory.WI_F3_NKPackType, line.WE_F3_NKPackType);
			AssertEquals("WE_BondedEntryKey", inventory.WI_BondedEntryKey, line.WE_BondedEntryKey);

			AssertLocations(line, inventory);

			var entry = WhsBondedWarehouseAttribute.BreakUpKey(inventory.WI_BondedEntryKey);

			AssertEquals("WE_PackageGroupId", inventory.PackageGroupId, line.WE_PackageGroupId);
			AssertEquals("WE_PerPackageQty", inventory.PerPackageQty, line.WE_PerPackageQty);
			Helper.AssertAttributes(inventory, line);
			Helper.AssertDocketLineCustomAttributes(inventory, line.WE_CustomAttrib1, line.WE_CustomAttrib2, line.WE_CustomAttrib3, line.WE_CustomAttrib4, line.WE_CustomAttrib5, line.WE_CustomAttrib6,
				line.WE_CustomDecimal1, line.WE_CustomDecimal2, line.WE_CustomDecimal3, line.WE_CustomDecimal4, line.WE_CustomDecimal5, line.WE_CustomDate1, line.WE_CustomDate2, line.WE_CustomDate3,
				line.WE_CustomDate4, line.WE_CustomDate5, line.WE_CustomFlag1, line.WE_CustomFlag2, line.WE_CustomFlag3, line.WE_CustomFlag4, line.WE_CustomFlag5, line.WE_CustomTextBlob1);

			AssertEquals("CustomsData.WB_ParentID, Should copy from new docketline.", line.PK, line.CustomsData.WB_ParentID);
			AssertDocketLineEqualsInventory_CustomsData(inventory.CustomsData, line.CustomsData);
		}

		protected virtual void AssertDocketLineEqualsInventory_CustomsData(WhsBondedWarehouseAttribute inventoryCustomsData, WhsBondedWarehouseAttribute docketlineCustomsData)
		{
			AssertEquals("CustomsData.WB_EntryLineNo, Should always copied.", inventoryCustomsData.WB_EntryLineNo, docketlineCustomsData.WB_EntryLineNo);
			AssertEquals("CustomsData.WB_EntryKey, Should always copied.", inventoryCustomsData.WB_EntryKey, docketlineCustomsData.WB_EntryKey);
			AssertEquals("CustomsData.WB_AddInfo", ZString.Empty, docketlineCustomsData.WB_AddInfo);
			AssertEquals("CustomsData.WB_BondedWhsQty", 0m, docketlineCustomsData.WB_BondedWhsQty);
			AssertEquals("CustomsData.WB_BondedWhsUnitOfQty", ZString.Empty, docketlineCustomsData.WB_BondedWhsUnitOfQty);
			AssertEquals("CustomsData.WB_CustomsQty", 0m, docketlineCustomsData.WB_CustomsQty);
			AssertEquals("CustomsData.WB_CustomsUnitOfQty", ZString.Empty, docketlineCustomsData.WB_CustomsUnitOfQty);
			AssertEquals("CustomsData.WB_DeclarationReference", ZString.Empty, docketlineCustomsData.WB_DeclarationReference);
			AssertEquals("CustomsData.WB_CustomsDeadline", ZDate.Empty, docketlineCustomsData.WB_CustomsDeadline);
			AssertEquals("CustomsData.WB_InwardStyle", ZString.Empty, docketlineCustomsData.WB_InwardStyle);
			AssertEquals("CustomsData.WB_InwardProcedure", ZString.Empty, docketlineCustomsData.WB_InwardProcedure);
			AssertEquals("CustomsData.WB_EntryDate", ZDateTime.Empty, docketlineCustomsData.WB_EntryDate);
			AssertEquals("CustomsData.WB_IsActive", inventoryCustomsData.WB_IsActive, docketlineCustomsData.WB_IsActive);
			AssertEquals("CustomsData.WB_ParentTableCode, Should not copyed.", "WE", docketlineCustomsData.WB_ParentTableCode);
			AssertEquals("CustomsData.WB_RN_NKCountryOfOrigin", ZString.Empty, docketlineCustomsData.WB_RN_NKCountryOfOrigin);
			AssertEquals("CustomsData.WB_RX_NKTILVCurrency", ZString.Empty, docketlineCustomsData.WB_RX_NKTILVCurrency);
			AssertEquals("CustomsData.WB_TILV", 0m, docketlineCustomsData.WB_TILV);
			AssertEquals("CustomsData.WB_ValueForDuty", 0m, docketlineCustomsData.WB_ValueForDuty);
			AssertEquals("CustomsData.WB_WB_InwardsEntry", ZGuid.Empty, docketlineCustomsData.WB_WB_InwardsEntry);
			AssertEquals("CustomsData.WB_PrimaryPreference", ZString.Empty, docketlineCustomsData.WB_PrimaryPreference);
			AssertEquals("CustomsData.WB_CustomsSecondQuantity", 0m, docketlineCustomsData.WB_CustomsSecondQuantity);
			AssertEquals("CustomsData.WB_CustomsSecondUnitQty", ZString.Empty, docketlineCustomsData.WB_CustomsSecondUnitQty);
			AssertEquals("CustomsData.WB_CustomsThirdQuantity", 0m, docketlineCustomsData.WB_CustomsThirdQuantity);
			AssertEquals("CustomsData.WB_CustomsThirdUnitQty", ZString.Empty, docketlineCustomsData.WB_CustomsThirdUnitQty);
			AssertEquals("CustomsData.WB_Tariff", ZString.Empty, docketlineCustomsData.WB_Tariff);
			AssertEquals("CustomsData.WB_OA_ManufacturerAddress", ZGuid.Empty, docketlineCustomsData.WB_OA_ManufacturerAddress);
		}

		protected virtual void AssertLocations(WhsDocketLine line, WhsInventoryView inventory)
		{
			AssertEquals("WE_WL", inventory.WI_WL, line.WE_WL);
			AssertEquals("WE_PalletID", inventory.WI_PalletID, line.WE_PalletID);
		}

		protected void AssertContainsDeniedCandidate(string description, OrgHeader orgHeader, ScreeningParty[] deniedCandidates)
			=> AssertEquals(description, orgHeader, deniedCandidates.First(x => x.Description == description).Header);

		protected void AssertContainsDeniedCandidateAsJobAddress(string description, OrgHeader orgHeader, ScreeningParty[] deniedCandidates)
			=> AssertEquals(description, orgHeader, deniedCandidates.First(x => x.Description == description).DocAddress.Organisation);

		protected bool CompletedMilestonesExistForThisEvent(TDocket docket, Event eventType)
		{
			bool result = false;
			foreach (ProcessTask task in docket.WorkflowItems)
			{
				if (task.P9_SE_NKMilestoneEvent == eventType.Code && !task.P9_ActualDate.IsEmpty)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		protected int CountLogsForThisEvent(TDocket docket, Event eventType)
		{
			return docket.GetLogs().GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == eventType.Code);
		}

		protected int CountLogsForThisEvent(TDocket docket, Event eventType, bool isCancelled)
		{
			return docket.GetLogs().GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == eventType.Code && l.IsCancelled == isCancelled);
		}

		protected void OnDocketStatusChangedTest(object sender, EventArgs e)
		{
			OnDocketStatusChangedCalled = true;
		}

		protected void OnCancelFailedTest(object sender, TextEventArgs e)
		{
			CancelFailedCalled = true;
			CancelFailedErrorMessage = e.Message;
		}

		bool OnDocketStatusChangedCalled;
		bool CancelFailedCalled;
		ZString CancelFailedErrorMessage;

		protected TDocket Docket
		{
			get => docket ?? (docket = GetNewBusinessObject());
			set => docket = value;
		}

		TDocket docket;

		protected new TDocket GetNewBusinessObject()
		{
			return (TDocket)base.GetNewBusinessObject();
		}

		#endregion
	}
}
