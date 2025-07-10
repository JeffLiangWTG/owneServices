using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BasePackingGroupUniqueIndexFailureHandlerTest : TestCaseWithFactory
	{
		public void TestConflictResolution_Container()
		{
			Factory.RefreshEnabled = false;
			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = true;

			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "!US";
			usCompany.GC_Name = "!US Name";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			usCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "#US";
			usBranch.GB_BranchName = "#US Name";
			usBranch.GB_RL_NKHomePort = "USLAX";
			usBranch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			usBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var usDeclaration = (BaseJobDeclaration)Factory.New<Integration.Customs.US.IJobDeclaration>();
			usDeclaration.JE_GB = usBranch.PK;
			usDeclaration.JE_MasterBill = "MB1";
			usDeclaration.JE_HouseBill = "HB1";
			var usHouseBill = usDeclaration.PrimaryHouseBill;
			var usContainer = usDeclaration.CusContainers.AddNew();
			usContainer.CO_ContainerNumber = "CONT1";
			usDeclaration.PackingGroups.DeleteAll();
			var usPackGroup = usDeclaration.PackingGroups.AddNew();
			usPackGroup.CR_CU_HouseBill = usHouseBill.PK;
			usPackGroup.CR_CO_Container = usContainer.PK;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MasterBill = "MB1234";
				var masterBill = declaration.PrimaryMasterBill;
				var container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = "AAAA1234566";
				declaration.PackingGroups.DeleteAll();
				Factory.Save();

				var packGroup = declaration.PackingGroups.AddNew();
				packGroup.CR_CU_HouseBill = masterBill.PK;
				packGroup.CR_CO_Container = container.PK;
				var package = packGroup.Packages.AddNew();
				package.CW_PackQty = 1;

				var declaration2 = anotherFactory.Load<BaseJobDeclaration>(declaration.PK);
				var packGroup2 = declaration2.PackingGroups.AddNew();
				packGroup2.CR_CU_HouseBill = masterBill.PK;
				packGroup2.CR_CO_Container = container.PK;
				var package2 = packGroup2.Packages.AddNew();
				var packages2 = declaration2.Packages;
				packages2.Add(package2);

				Factory.Save();
				((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				CombineAssertions(() =>
				{
					try
					{
						anotherFactory.Save();
						Fail("First save should not have succeeded.");
					}
					catch (Exception ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}

					AssertEquals("User should have been notified", false, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertEquals("Message to notify user", $@"Container Number (AAAA1234566) has already been linked to Bill Number (MB1234) by another user ({declaration.JE_SystemLastEditUser} @ {declaration.JE_SystemLastEditTimeUtc.ToSmallDateTimeFloor()}). Duplicate packing details have been deleted, please review your changes and save again.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("packGroup 2 should be deleted", true, packGroup2.IsDeleted);
					var declaration2PackingGroups = declaration2.PackingGroups;
					AssertEquals("declaration2PackingGroups.Count", 1, declaration2PackingGroups.Count);
					AssertEquals("declaration2PackingGroups should have loaded from database", packGroup.PK, declaration2PackingGroups[0].PK);
					AssertEquals("packages2.Count", 1, packages2.Count);
					AssertEquals("packages2 should have loaded from database", package.PK, packages2[0].PK);

					var packingGroups2 = declaration2.PrimaryMasterBill.PackingGroups;
					AssertEquals(1, packingGroups2.Count);
					AssertEquals("should have loaded from database", packGroup.PK, packingGroups2[0].PK);

					anotherFactory.Save();
					AssertEquals("Should be using the existing packGroup now", packGroup.PK, declaration.PackingGroups[0].PK);
				});
			}
		}

		public void TestConflictResolution_Equipment()
		{
			Factory.RefreshEnabled = false;
			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = true;

			var declaration = Factory.New<JobDeclarationForTestingEquipments>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MB1234";
			var masterBill = declaration.PrimaryMasterBill;
			var equipment = declaration.Equipments.AddNew();
			equipment.CEQ_IdentificationNumber = "AAAA1234566";
			declaration.PackingGroups.DeleteAll();
			Factory.Save();

			var packGroup = declaration.PackingGroups.AddNew();
			packGroup.CR_CU_HouseBill = masterBill.PK;
			packGroup.CR_CEQ_Equipment = equipment.PK;
			var package = packGroup.Packages.AddNew();
			package.CW_PackQty = 1;

			var declaration2 = anotherFactory.Load<BaseJobDeclaration>(declaration.PK);
			var packGroup2 = declaration2.PackingGroups.AddNew();
			packGroup2.CR_CU_HouseBill = masterBill.PK;
			packGroup2.CR_CEQ_Equipment = equipment.PK;
			var package2 = packGroup2.Packages.AddNew();
			var packages2 = declaration2.Packages;
			packages2.Add(package2);

			Factory.Save();
			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = false;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			CombineAssertions(() =>
			{
				try
				{
					anotherFactory.Save();
					Fail("First save should not have succeeded.");
				}
				catch (Exception ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}

				AssertEquals("User should have been notified", false, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Message to notify user", $@"Equipment (AAAA1234566) has already been linked to Bill Number (MB1234) by another user ({declaration.JE_SystemLastEditUser} @ {declaration.JE_SystemLastEditTimeUtc.ToSmallDateTimeFloor()}). Duplicate packing details have been deleted, please review your changes and save again.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("packGroup 2 should be deleted", true, packGroup2.IsDeleted);
				var declaration2PackingGroups = declaration2.PackingGroups;
				AssertEquals("declaration2PackingGroups.Count", 1, declaration2PackingGroups.Count);
				AssertEquals("declaration2PackingGroups should have loaded from database", packGroup.PK, declaration2PackingGroups[0].PK);
				AssertEquals("packages2.Count", 1, packages2.Count);
				AssertEquals("packages2 should have loaded from database", package.PK, packages2[0].PK);

				var packingGroups2 = declaration2.PrimaryMasterBill.PackingGroups;
				AssertEquals(1, packingGroups2.Count);
				AssertEquals("should have loaded from database", packGroup.PK, packingGroups2[0].PK);

				anotherFactory.Save();
				AssertEquals("Should be using the existing packGroup now", packGroup.PK, declaration.PackingGroups[0].PK);
			});
		}

		public void TestConflictResolution_NoContainerOrEquipment()
		{
			Factory.RefreshEnabled = false;
			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = true;

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MB1234";
			var masterBill = declaration.PrimaryMasterBill;
			declaration.PackingGroups.DeleteAll();
			Factory.Save();

			var packGroup = declaration.PackingGroups.AddNew();
			packGroup.CR_CU_HouseBill = masterBill.PK;
			var package = packGroup.Packages.AddNew();
			package.CW_PackQty = 1;

			var declaration2 = anotherFactory.Load<BaseJobDeclaration>(declaration.PK);
			var packGroup2 = declaration2.PackingGroups.AddNew();
			packGroup2.CR_CU_HouseBill = masterBill.PK;
			var package2 = packGroup2.Packages.AddNew();
			var packages2 = declaration2.Packages;
			packages2.Add(package2);

			Factory.Save();
			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = false;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			CombineAssertions(() =>
			{
				try
				{
					anotherFactory.Save();
					Fail("First save should not have succeeded.");
				}
				catch (Exception ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}

				AssertEquals("User should have been notified", false, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Message to notify user", $@"Another user ({declaration.JE_SystemLastEditUser} @ {declaration.JE_SystemLastEditTimeUtc.ToSmallDateTimeFloor()}) has already linked to Bill Number (MB1234) a record without Container or Equipment details. Duplicate packing details have been deleted, please review your changes and save again.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("packGroup 2 should be deleted", true, packGroup2.IsDeleted);
				var declaration2PackingGroups = declaration2.PackingGroups;
				AssertEquals("declaration2PackingGroups.Count", 1, declaration2PackingGroups.Count);
				AssertEquals("declaration2PackingGroups should have loaded from database", packGroup.PK, declaration2PackingGroups[0].PK);
				AssertEquals("packages2.Count", 1, packages2.Count);
				AssertEquals("packages2 should have loaded from database", package.PK, packages2[0].PK);

				var packingGroups2 = declaration2.PrimaryMasterBill.PackingGroups;
				AssertEquals(1, packingGroups2.Count);
				AssertEquals("should have loaded from database", packGroup.PK, packingGroups2[0].PK);

				anotherFactory.Save();
				AssertEquals("Should be using the existing packGroup now", packGroup.PK, declaration.PackingGroups[0].PK);
			});
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new InvoiceLinePackagePivotUniqueIndexFailureHandler(null));
		}
	}
}
