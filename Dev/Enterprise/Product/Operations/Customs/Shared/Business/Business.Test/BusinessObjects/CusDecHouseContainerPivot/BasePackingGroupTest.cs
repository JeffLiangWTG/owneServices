using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BasePackingGroup))]
	public class BasePackingGroupTest : EnterpriseBusinessObjectTestCase
	{
		public void TestManualPopulateClusterKey()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_ClusterKey = 2;

			var container = declaration.CusContainers.AddNew();
			container.FillWithValidTestData();

			var bill = declaration.Bills.AddNew();
			bill.FillWithValidTestData();
			bill.CU_ClusterKey = 2;

			var packingGroup = declaration.PackingGroups.AddNew();
			AssertEquals("Should default to 0(changed on adding to collection).", 2, packingGroup.CR_ClusterKey);

			packingGroup.CR_CO_Container = container.PK;
			packingGroup.CR_CU_HouseBill = bill.PK;

			AssertEquals("Should manual populate the value from the parent bill.", 2, packingGroup.CR_ClusterKey);

			Factory.Save();

			var finallyKey = declaration.JE_ClusterKey;

			AssertEquals("Should populate the value from the parent declaration in OnSaving part.", finallyKey, bill.CU_ClusterKey);
			AssertEquals("Should populate the value from the parent bill in OnSaving part.", finallyKey, packingGroup.CR_ClusterKey);
		}

		public void TestUniqueIndexFailureHandler()
		{
			var basePackingGroup = GetNewBusinessObject();
			var handler = GetUniqueIndexFailureHandler(basePackingGroup);
			CombineAssertions(() =>
			{
				AssertType<BasePackingGroupUniqueIndexFailureHandler>("Correct Handler Type", handler);
				AssertSame("Cached", handler, GetUniqueIndexFailureHandler(basePackingGroup));
			});
		}

		public void TestCanDelete()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BasePackingGroup packGroup = declaration.PackingGroups.AddNew();
			AssertEquals(true, ((ICanDelete)packGroup).CanDelete);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertEquals(false, ((ICanDelete)packGroup).CanDelete);
			AssertEquals(Bill.ReasonForCannotDeleteWhenSynchronised, ((ICanDelete)packGroup).ReasonForNotAbleToDelete);

			declaration.JE_OverrideFreightDefaults = true;
			AssertEquals(true, ((ICanDelete)packGroup).CanDelete);
		}

		public void TestReportDeleteLog()
		{
			ErrorReporter.Clear();

			var declaration = Factory.New<BaseJobDeclaration>();
			var packGroup = declaration.PackingGroups.AddNew();
			var houseBill = packGroup.CR_CU_HouseBill;

			packGroup.Delete();
			AssertNoExceptionThrown(() => _ = packGroup.CR_CO_Container);

			var message = ErrorReporter.LastExceptionsReported().First(c => c.Contains(@"Message :Developer Error: Should not be accessing a property on a deleted business object"));
			CombineAssertions(() =>
			{
				AssertContains("Should contains the log of Delete.", "Delete Log:", message);
				AssertContains("Should contains the log of Delete.", $"CR_CU_HouseBill: {houseBill}", message);
				AssertContains("Should contains the log of Delete.", "at Enterprise.Customs.Business.BasePackingGroup.BuildDeleteLog() ", message);
				AssertContains("Should contains the log of Delete.", "at Enterprise.Customs.Business.BasePackingGroup.Delete() ", message);
			});

			ErrorReporter.Clear();
		}

		public void TestReportDeleteForDataRefreshLog()
		{
			ErrorReporter.Clear();

			var declaration = Factory.New<BaseJobDeclaration>();
			var packGroup = declaration.PackingGroups.AddNew();
			var houseBill = packGroup.CR_CU_HouseBill;

			((IBusiness)packGroup).DeleteForDataRefresh();
			AssertNoExceptionThrown(() => _ = packGroup.CR_CO_Container);

			var message = ErrorReporter.LastExceptionsReported().First(c => c.Contains(@"Message :Developer Error: Should not be accessing a property on a deleted business object"));
			CombineAssertions(() =>
			{
				AssertContains("Should contains the log of DeleteForDataRefresh.", "Delete Log:", message);
				AssertContains("Should contains the log of DeleteForDataRefresh.", $"CR_CU_HouseBill: {houseBill}", message);
				AssertContains("Should contains the log of DeleteForDataRefresh.", "at Enterprise.Customs.Business.BasePackingGroup.BuildDeleteLog() ", message);
				AssertContains("Should contains the log of DeleteForDataRefresh.", "at Enterprise.Customs.Business.BasePackingGroup.DeleteForDataRefresh() ", message);
			});

			ErrorReporter.Clear();
		}

		public void TestDeletingTwice()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var packGroup = declaration.PackingGroups.AddNew();

			AssertNoExceptionThrown("First Delete", () => packGroup.Delete());
			AssertNoExceptionThrown("Second Delete", () => packGroup.Delete());
		}

		public void TestDeletingForDataRefreshTwice()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			IBusiness packGroup = declaration.PackingGroups.AddNew();

			AssertNoExceptionThrown("First Delete", () => packGroup.DeleteForDataRefresh());
			AssertNoExceptionThrown("Second Delete", () => packGroup.DeleteForDataRefresh());
		}

		public void TestAddTotalOuterPackage()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BasePackingGroup packGroup = declaration.PackingGroups.AddNew();
			packGroup.AddTotalOuterPackageIfRequired(17);
			AssertEquals(17, packGroup.TotalPackageCount());
		}

		public virtual void TestRemovesContainerReferenceOnSavingWhenContainerDoesNotExistInJobDeclaration()
		{
			BaseCusContainer container = TestDec.CusContainers.AddNew();
			Bill bill = TestDec.Bills.AddNew();

			BasePackingGroup packingGroup = TestDec.PackingGroups.AddNew();
			packingGroup.CR_CO_Container = container.PK;
			packingGroup.CR_CU_HouseBill = bill.PK;

			AssertEquals("Container", container.PK, packingGroup.CR_CO_Container);

			TestDec.CusContainers.RemoveAndDeleteAll();
			Factory.Save();

			AssertEquals(true, packingGroup.IsDeleted);
		}

		public void TestPackingGroupDeletedWithoutExceptionWhenDeletingBill_Issue01095383()
		{
			var container = TestDec.CusContainers.AddNew();
			var bill = TestDec.Bills.AddNew();
			var packingGroup1 = bill.PackingGroups.AddNew();
			packingGroup1.CR_CO_Container = container.PK;
			var packingGroup2 = bill.PackingGroups.AddNew();
			packingGroup2.CR_CO_Container = container.PK;
			AssertNoExceptionThrown(() =>
			{
				bill.Delete();
			});
		}

		public void TestContainer()
		{
			var container = TestDec.CusContainers.AddNew();
			AssertEquals(null, PackingGroup.Container);
			AssertEquals(ZGuid.Empty, PackingGroup.CR_CO_Container);

			PackingGroup.CR_CO_Container = container.PK;
			AssertEquals(container, PackingGroup.Container);
			AssertEquals(container.PK, PackingGroup.CR_CO_Container);

			PackingGroup.CR_CO_Container = ZGuid.Empty;
			AssertEquals(null, PackingGroup.Container);
			AssertEquals(ZGuid.Empty, PackingGroup.CR_CO_Container);

			PackingGroup.CR_CO_Container = container.PK;
			AssertEquals(1, HouseBill.Containers.Count);
			AssertEquals(HouseBill.PK, PackingGroup.CR_CU_HouseBill);

			var bill2 = TestDec.Bills.AddNew();
			PackingGroup.CR_CU_HouseBill = bill2.PK;
			AssertEquals(0, HouseBill.Containers.Count);
			AssertEquals(1, bill2.Containers.Count);
		}

		public void TestEquipment()
		{
			AssertNull(PackingGroup.Equipment);

			var equipment = Factory.New<CusEquipment>();
			PackingGroup.CR_CEQ_Equipment = equipment.PK;
			AssertSame(equipment, PackingGroup.Equipment);
		}

		public void TestCR_CO_ContainerAndCR_CEQ_EquipmentWillEmptyEachOtherWhenSet()
		{
			var container = TestDec.CusContainers.AddNew();
			var equipment = TestDec.Equipments.AddNew();
			PackingGroup.CR_CO_Container = container.PK;
			PackingGroup.CR_CEQ_Equipment = equipment.PK;
			AssertEquals(ZGuid.Empty, PackingGroup.CR_CO_Container);
			AssertEquals(equipment.PK, PackingGroup.CR_CEQ_Equipment);

			PackingGroup.CR_CO_Container = container.PK;
			AssertEquals(ZGuid.Empty, PackingGroup.CR_CEQ_Equipment);
			AssertEquals(container.PK, PackingGroup.CR_CO_Container);
		}

		public void TestPackages()
		{
			BasePackage package = PackingGroup.Packages.AddNew();
			AssertNotNull(package);
		}

		public void TestTotalPackageCount()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var packGroup = declaration.PackingGroups.AddNew();
			var package1 = packGroup.Packages.AddNew();
			package1.CW_PackQty = 10;
			var package2 = packGroup.Packages.AddNew();
			package2.CW_PackQty = 2;
			var package3 = packGroup.Packages.AddNew();
			package3.CW_PackQty = 5;
			package3.CW_CW_Parent = package2.PK;
			var package4 = packGroup.Packages.AddNew();
			package4.CW_PackQty = 6;

			var packGroup2 = declaration.PackingGroups.AddNew();
			var package5 = packGroup2.Packages.AddNew();
			package5.CW_PackQty = 7;
			package5.CW_CW_Parent = package1.PK;

			var packGroup3 = declaration.PackingGroups.AddNew();
			var package6 = packGroup3.Packages.AddNew();
			package6.CW_PackQty = 8;
			declaration.PackingGroups.Remove(packGroup3);
			packGroup3.Declaration = null;

			var packGroup4 = declaration.PackingGroups.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("should sum CW_PackQty from: package3 package4", 11, packGroup.TotalPackageCount());
				AssertEquals("should sum CW_PackQty from: package5", 7, packGroup2.TotalPackageCount());
				AssertEquals("should be 0 when no declaration linked", 0, packGroup3.TotalPackageCount());
				AssertEquals("should be 0 when no packages linked", 0, packGroup4.TotalPackageCount());

				declaration.Packages.Delete(package3);
				declaration.Packages.Delete(package5);
				AssertEquals("should sum CW_PackQty from: package1 package2 package4", 18, packGroup.TotalPackageCount());

				var declaration2 = Factory.New<BaseJobDeclaration>();
				packGroup3.Declaration = declaration2;
				AssertEquals("should sum CW_PackQty from: package6 (parentPKs from declaration.Packages is empty)", 8, packGroup3.TotalPackageCount());
			});
		}

		public void TestDeleteRemovesChildPackages()
		{
			var package1 = PackingGroup.Packages.AddNew();
			var package2 = PackingGroup.Packages.AddNew();

			package1.CW_PackQty = 0;
			package2.CW_PackQty = 1;

			var type = package1.GetType();
			var property = type.GetProperty("ShouldDeleteIfPackQtyIsEmpty", BindingFlags.Instance | BindingFlags.NonPublic);

			var shouldDeleteIfPackQtyIsEmpty = (bool)property.GetValue(package1);

			var message = shouldDeleteIfPackQtyIsEmpty
				? "Package should be deleted as its CW_PackQty is 0, by ShouldDeleteIfPackQtyIsEmpty in BasePackage.OnFactorySaving()"
				: "Package should not be deleted even its CW_PackQty is 0, by ShouldDeleteIfPackQtyIsEmpty in BasePackage.OnFactorySaving()";

			Factory.Save();

			AssertEquals(message, shouldDeleteIfPackQtyIsEmpty, package1.IsDeleted);
			Assert("Package should not be deleted as its CW_PackQty is not 0, by BasePackage.OnFactorySaving()", !package2.IsDeleted);

			package1 = PackingGroup.Packages.AddNew();
			package1.CW_PackQty = 0;

			PackingGroup.Delete();

			Assert("Packing group is deleted", PackingGroup.IsDeleted);
			Assert("Package should be deleted, by either OnFactorySaving() or Delete()", package1.IsDeleted);
			Assert("Package should be deleted, by Delete()", package2.IsDeleted);
		}

		public void TestGetConvertedPackType()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TotalNoOfPacks = 10;
			declaration.JE_TotalNoOfPacksPackType = "BAG";
			declaration.JE_HouseBill = "HB1";
			var packingGroup = declaration.PrimaryHouseBill.PackingGroups[0];
			if (packingGroup != null)
			{
				var package = packingGroup.Packages[0];
				AssertEquals(10, package.CW_PackQty);
				AssertEquals("BAG", package.CW_PackType);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestBillType()
		{
			var grp = Factory.New<BasePackingGroup>();
			Assert(grp.BillType.IsEmpty);

			var dec = Factory.New<BaseJobDeclaration>();
			var bill = dec.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;

			grp.CR_CU_HouseBill = bill.PK;
			AssertEquals(BillTypeList.Codes.MasterBill, grp.BillType);

			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			AssertEquals(BillTypeList.Codes.HouseBill, grp.BillType);
		}

		public void TestNoExceptionWhenPackagesIsNull()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			declaration.Bills.AddNew();
			var packingGroup = declaration.PackingGroups.AddNew();
			declaration.PackingGroups.Remove(packingGroup);
			packingGroup.Declaration = null;

			var bizoToDelete = (IBusiness)packingGroup;
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(0, packingGroup.TotalPackageCount());
				packingGroup.DefaultPackTypeIfRequired("xxx");
				packingGroup.AddTotalOuterPackageIfRequired(0);
				bizoToDelete.DeleteForDataRefresh();
				Assert(packingGroup.IsDeleted);
			});
		}

		public void TestIsSavedByFactory()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var packGroup = declaration.PackingGroups.AddNew();

			CombineAssertions(() =>
			{
				Assert("can be saved properly", packGroup.IsSavedByFactory);
				declaration.MakeNonPersistent();
				Assert("should stop being saved if declaration not persistent", !packGroup.IsSavedByFactory);
				packGroup.Delete();
				Assert("can be saved properly for delete", packGroup.IsSavedByFactory);
			});
		}

		#region FieldsThatAreNotEffectiveOnMerge

		public void TestFieldsThatAreNotEffectiveOnMerge()
		{
			SettingMergeAffectingFieldsShouldNotThrowExceptionOrDeveloperError(CusDecHouseContainerPivotSchema.Constants.CR_CargoStatus, "RCV");
		}

		void SettingMergeAffectingFieldsShouldNotThrowExceptionOrDeveloperError(ZString fieldName, object value)
		{
			AssertNoExceptionThrown(() => SettingMergeAffectingFieldsWhenNoEntryHeader(fieldName, value));
			AssertNoExceptionThrown(() => SettingMergeAffectingFieldsWhenThereAreMergedEntries(fieldName, value));
		}

		void SettingMergeAffectingFieldsWhenNoEntryHeader(ZString fieldName, object value)
		{
			ErrorReporter.Clear();
			BaseJobDeclaration declaration = ImportJobDeclaration;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "HB1234";
			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "AAAA1234566";
			BasePackingGroup packGroup = declaration.PackingGroups.Count > 0 ? declaration.PackingGroups[0] : declaration.PackingGroups.AddNew();
			packGroup.CR_CU_HouseBill = houseBill.PK;
			packGroup.CR_CO_Container = container.PK;
			AssertEquals("No CusEntryHeaders expected", 0, declaration.CustomsEntryHeaders.Count);
			AssertNotEquals("Preconditions: New value should be different from the initial value", value, packGroup[fieldName]);
			packGroup[fieldName] = value;
			Factory.Save();
			AssertEquals("No Developer Error Expected when no CusEntryHeader", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		void SettingMergeAffectingFieldsWhenThereAreMergedEntries(ZString fieldName, object value)
		{
			ErrorReporter.Clear();
			BaseJobDeclaration declaration = ImportJobDeclaration;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "HB1234";
			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "AAAA1234566";
			BasePackingGroup packGroup = declaration.PackingGroups.Count > 0 ? declaration.PackingGroups[0] : declaration.PackingGroups.AddNew();
			packGroup.CR_CU_HouseBill = houseBill.PK;
			packGroup.CR_CO_Container = container.PK;
			BaseJobComInvoiceHeader header = declaration.Invoices.AddNew();
			header.JobComInvoiceLines.AddNew();
			declaration.DoMerge();
			Factory.Save();
			AssertEquals("Preconditions: Non-zero EntryHeaders expected on Declaration", true, declaration.CustomsEntryHeaders.Count > 0);
			AssertEquals("Precondtions: No Developer Error Expected", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			AssertNotEquals("Preconditions: New value should be different from the initial value", value, packGroup[fieldName]);
			packGroup[fieldName] = value;
			Factory.Save();
			AssertEquals("No Developer Error Expected when there is at least one CusEntryHeader", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		protected virtual BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				BaseJobDeclaration result = Factory.New<BaseJobDeclaration>();
				result.JE_MessageType = JobMessageTypeList.Codes.Import;
				result.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				return result;
			}
		}
		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return PackingGroup;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return PackingGroup;
		}

		BaseJobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = GetJobDeclaration();
					fTestDec.DisableDefaultPackingInformation = true;
				}
				return fTestDec;
			}
		}
		BaseJobDeclaration fTestDec;

		Bill HouseBill
		{
			get
			{
				if (fHouseBill == null)
				{
					fHouseBill = TestDec.Bills.AddNew();
				}
				return fHouseBill;
			}
		}
		Bill fHouseBill;

		BasePackingGroup PackingGroup
		{
			get
			{
				if (fPackingGroup == null)
				{
					fPackingGroup = HouseBill.PackingGroups.AddNew();
				}
				return fPackingGroup;
			}
		}
		BasePackingGroup fPackingGroup;

		protected virtual BaseJobDeclaration GetJobDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}

		#endregion

		IUniqueIndexFailureHandler GetUniqueIndexFailureHandler(BusinessObject basePackingGroup) => ((IEnumerable<IUniqueIndexFailureHandler>)typeof(BasePackingGroup).GetProperty("UniqueIndexFailureHandlers", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(basePackingGroup, null)).Single();
	}
}
