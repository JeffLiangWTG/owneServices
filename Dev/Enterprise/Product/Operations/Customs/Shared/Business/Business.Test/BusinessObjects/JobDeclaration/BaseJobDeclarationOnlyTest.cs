using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	/// <summary>
	/// This is the test class that tests the base behaviours that are supposed to be overriden by Inherited classes
	/// </summary>
	[TestedType(typeof(BaseJobDeclaration))]
	sealed class BaseJobDeclarationOnlyTest : BaseJobDeclarationTest<BaseJobDeclaration>
	{
		public void TestJE_VoyageFlightNoCaption()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_TransportMode = Core.Constants.TransportModes.Road;
			var info = dec.JE_VoyageFlightNoInfo;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Road", "Registration", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Registration", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Sea", "Voyage", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Air", "Flight/Folio", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Flight/Folio", mediumCaption: "Flight No.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Rail", "Voyage / Flight No", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage / Flight No", shortCaption: "Voyage/Flight", fullDescription: "A unique reference assigned by a carrier to identify a specific journey of an aircraft or vessel.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
		}

		public void TestICustomFieldProvider_GetCustomBusinessObject_RespectShouldRefresh() => CombineAssertions(() =>
		{
			ICustomFieldProvider declaration = Factory.New<BaseJobDeclaration>();
			AssertSame("Not refresh", declaration.GetCustomBusinessObject(false), declaration.GetCustomBusinessObject(false));
			AssertNotSame("refresh", declaration.GetCustomBusinessObject(true), declaration.GetCustomBusinessObject(true));
		});

		public void TestGetDataModelAndPopulateDataModelIfNeeded()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(ZString.Empty, declaration.JE_DataModel);
			AssertEquals("ER", declaration.GetDataModelAndPopulateDataModelIfNeeded());
			AssertEquals("ER", declaration.JE_DataModel);
		}

		public void TestICommonInvoiceDataProviderMembers()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			ICommonInvoiceDataProvider provider = declaration;
			AssertSame("Persistent - CustomsFileParent", declaration, provider.CustomsFileParent);
			declaration.MakeNonPersistent();
			AssertSame("Non Persistent - CustomsFileParent", invoice1, provider.CustomsFileParent);
		}

		public void TestIDataModelSupporter()
		{
			IDataModelSupporter cusSupportingInfoParent = Factory.New<BaseJobDeclaration>();
			AssertEquals(ZString.Empty, cusSupportingInfoParent.DataModel);
			cusSupportingInfoParent.PopulateDataModelIfNeeded();
			AssertEquals("ER", cusSupportingInfoParent.DataModel);
		}

		public void TestUnlockDoMergeMutexWhenSaveFailed()
		{
			var mock = Factory.NewMoq<BaseJobDeclaration>();
			mock.Setup(x => x.OnSaving()).Callback(() => { throw new InvalidOperationException(); });
			var declaration = mock.Object;
			using (var mutex = declaration.DoMergeMutex)
			{
				Assert(mutex.Lock());
				AssertEquals("Is locked", true, declaration.DoMergeMutex.IsLocked);
				try
				{
					Factory.Save();
				}
				catch (InvalidOperationException)
				{
					AssertEquals("Lock is released even when saving failed", false, declaration.DoMergeMutex.IsLocked);
				}
			}
		}

		public void TestSetJE_OH_ImporterBaseValueOnly()
		{
			var declaration = Factory.New<BaseJobDeclarationForMiscellaneousTesting>();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "!@1";
			org.OH_RL_NKClosestPort = "ERASA";
			declaration.JE_MessageType = ZString.Empty;
			declaration.SetJE_OH_ImporterBaseValueOnlyForTesting(org.PK);
			AssertEquals("declaration.JE_OH_Importer", org.PK, declaration.JE_OH_Importer);
			AssertEquals("declaration.JE_MessageType", ZString.Empty, declaration.JE_MessageType);
		}

		public void TestSetJE_OH_SupplierBaseValueOnly()
		{
			var declaration = Factory.New<BaseJobDeclarationForMiscellaneousTesting>();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "!@1";
			org.OH_RL_NKClosestPort = "ERASA";
			declaration.JE_MessageType = ZString.Empty;
			declaration.SetJE_OH_SupplierBaseValueOnlyForTesting(org.PK);
			AssertEquals("declaration.JE_OH_Supplier", org.PK, declaration.JE_OH_Supplier);
			AssertEquals("declaration.JE_MessageType", ZString.Empty, declaration.JE_MessageType);
		}

		public void TestGetInterfaceSubmissionType_Default()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", ZString.Empty, Factory.New<BaseJobDeclaration>().GetInterfaceSubmissionType());
				var customsInterface = new LocalCountryCustomsInterface();
				customsInterface.RecipientID = "RecipientID";
				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;
				using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("BuiltIn", DeclarationApplicationCodeList.Codes.Builtin, Factory.New<BaseJobDeclaration>().GetInterfaceSubmissionType());
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
				using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("BothBuiltInDefaulted", DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted, Factory.New<BaseJobDeclaration>().GetInterfaceSubmissionType());
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
				using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("Interfaced", DeclarationApplicationCodeList.Codes.Interfaced, Factory.New<BaseJobDeclaration>().GetInterfaceSubmissionType());
				}

				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
				using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("BothInterfaceDefaulted", DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted, Factory.New<BaseJobDeclaration>().GetInterfaceSubmissionType());
				}
			});
		}

		public void TestCaptureLoginDetailsOnCastingException()
		{
			var frCompany = Factory.New<GlbCompany>();
			frCompany.GC_Code = "FR$";
			frCompany.GC_Name = "FR Company";
			frCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			frCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var frBranch = frCompany.Branches.AddNew();
			frBranch.GB_Code = "FR#";
			frBranch.GB_BranchName = "FR Branch";
			frBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GB = frBranch.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var mockDec = newFactory.LoadMoq<BaseJobDeclaration>(declaration.PK);
			var mockDecProtected = mockDec.Protected();
			mockDecProtected.Setup("OnApportioned")
				.Throws(() => new InvalidCastException("TEST DATA"));
			var dec = mockDec.Object;
			dec.ApportionmentDirty = true;
			AssertExceptionThrown<InvalidCastException>("Exception should include login details", $"Declaration (Type:{dec.GetType().FullName}, Country:FR), Login Country (ER).", newFactory.Save);
		}

		public void TestDefaultJE_ApplicationCode_BuiltinOnly_NotConfigureLocalCountryCustomsInterface()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("EXP - When LocalCountryCustomsInterface registry item is no configured and country is only BuiltIn, Default JE_ApplicationCode should be", ZString.Empty, declaration.JE_ApplicationCode);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("IMP - When LocalCountryCustomsInterface registry item is no configured and country is only BuiltIn, Default JE_ApplicationCode should be", "CMR", declaration.JE_ApplicationCode);
			}
		}

		public void TestDefaultJE_ApplicationCode_IsABMInterfaceActivated_NotConfigureLocalCountryCustomsInterface()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				DataRegistry.Business.CustomsDataRegistry.Instance.CustomsWareCompany.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "TEST");
				AssertEquals("When CustomsWareCompany registry item is set to TEST, Default JE_ApplicationCode should be", DeclarationApplicationCodeList.Codes.Interfaced, Factory.New<BaseJobDeclaration>().JE_ApplicationCode);
				DataRegistry.Business.CustomsDataRegistry.Instance.CustomsWareCompany.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "");
			}
		}

		public void TestDefaultJE_ApplicationCode_NotBuiltinOnly_NotHasBuiltInDeclaration_NotConfigureLocalCountryCustomsInterface()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Burundi))
			{
				AssertEquals("When LocalCountryCustomsInterface registry item is no configured and country is not an only BuiltIn, Default JE_ApplicationCode should be", DeclarationApplicationCodeList.Codes.Interfaced, Factory.New<BaseJobDeclaration>().JE_ApplicationCode);
			}
		}

		public void TestDefaultJE_ApplicationCode_NotBuiltinOnly_HasBuiltInDeclaration_NotConfigureLocalCountryCustomsInterface()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				AssertEquals(DeclarationApplicationCodeList.Codes.Builtin, Factory.New<BaseJobDeclaration>().JE_ApplicationCode);
			}
		}

		public void TestDefaultJE_ApplicationCode_ConfigureLocalCountryCustomsInterface()
		{
			CombineAssertions(() =>
			{
				var customsInterface = new LocalCountryCustomsInterface();
				customsInterface.RecipientID = "RecipientID";
				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;
				using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("When LocalCountryCustomsInterface registry item is set to BuiltIn, Default JE_ApplicationCode should be", DeclarationApplicationCodeList.Codes.Builtin, Factory.New<BaseJobDeclaration>().JE_ApplicationCode);
				}
				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
				using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("When LocalCountryCustomsInterface registry item is set to BothBuiltInDefaulted, Default JE_ApplicationCode should be", DeclarationApplicationCodeList.Codes.Builtin, Factory.New<BaseJobDeclaration>().JE_ApplicationCode);
				}
				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
				using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("When LocalCountryCustomsInterface registry item is set to Interfaced, Default JE_ApplicationCode should be", DeclarationApplicationCodeList.Codes.Interfaced, Factory.New<BaseJobDeclaration>().JE_ApplicationCode);
				}
				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
				using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("When LocalCountryCustomsInterface registry item is set to BothInterfaceDefaulted, Default JE_ApplicationCode should be", DeclarationApplicationCodeList.Codes.Interfaced, Factory.New<BaseJobDeclaration>().JE_ApplicationCode);
				}
			});
		}

		public void TestOnIsDeclarationIntegratedChanged()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var called = false;
			declaration.OnIsDeclarationIntegratedChanged += (sender, e) =>
			{
				called = true;
			};
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			Assert("Setting JE_ApplicationCode should trigger OnIsDeclarationIntegratedChanged", called);
		}

		[TestDate(2023, 01, 29)]
		public void TestCustomsRule()
		{
			var todaty = ZDateTime.Today.Date;

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";

			var exporter = Factory.New<OrgHeader>();
			exporter.OH_Code = "EXPORTER";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "TS1";

			var rule01 = Factory.New<CustomsRule>();
			var rule02 = Factory.New<CustomsRule>();
			var rule03 = Factory.New<CustomsRule>();
			var rule04 = Factory.New<CustomsRule>();

			rule01.CPH_OH_PermitHolder = importer.PK;
			rule01.CPH_StartDate = todaty.AddMonths(-3);
			rule01.CPH_EndDate = todaty.AddMonths(-2);
			rule01.CPH_GC_Company = company.PK;

			rule02.CPH_OH_PermitHolder = importer.PK;
			rule02.CPH_StartDate = todaty.AddMonths(-1);
			rule02.CPH_EndDate = todaty.AddMonths(1);
			rule02.CPH_GC_Company = company.PK;

			rule03.CPH_StartDate = todaty.AddMonths(-3);
			rule03.CPH_EndDate = todaty.AddMonths(-2);
			rule03.CPH_GC_Company = company.PK;

			rule04.CPH_StartDate = todaty.AddMonths(-1);
			rule04.CPH_EndDate = todaty.AddMonths(1);
			rule04.CPH_GC_Company = company.PK;

			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(false, declaration.IsBrokerToPay);
			AssertEquals(false, declaration.IsImporterToPay);
			AssertEquals(0m, declaration.DisbursementAmount);
			AssertEquals(0m, declaration.TotalDutyAmount);
			AssertNull(declaration.CustomsRule);

			declaration.JE_MessageType = DefaultImportMessageType;
			declaration.JE_DateOfFirstArrival = todaty.AddMonths(-1);
			AssertEquals(rule04.PK, declaration.CustomsRule.PK);

			declaration.JE_DateOfFirstArrival = todaty.AddMonths(-2);
			AssertEquals(rule03.PK, declaration.CustomsRule.PK);

			declaration.JE_OH_Importer = importer.PK;
			AssertEquals(rule01.PK, declaration.CustomsRule.PK);

			declaration.JE_DateOfFirstArrival = todaty.AddMonths(-1);
			AssertEquals(rule02.PK, declaration.CustomsRule.PK);

			declaration.JE_OH_Importer = exporter.PK;
			AssertEquals(rule04.PK, declaration.CustomsRule.PK);
		}

		public void TestTypeOfBills()
		{
			AssertType<BillCollection<Bill, BaseJobDeclaration>>(Factory.New<BaseJobDeclaration>().Bills);
		}

		public void TestJE_DeclarationLanguage_MaxLength()
		{
			AssertEquals("JE_DeclarationLanguage MaxLength", 2, Factory.New<TestDeclaration>().JE_DeclarationLanguageInfo.MaxLength);
		}

		public void TestJE_DeclarationLanguage_List()
		{
			AssertEquals("Lookups.DeclarationLanguageList", Factory.New<TestDeclaration>().JE_DeclarationLanguageInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestJE_DeclarationLanguage_Caption()
		{
			AssertEquals("Language", Factory.New<TestDeclaration>().JE_DeclarationLanguageInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestGetInventoryAutomationAction()
		{
			var declaration = Factory.New<JobDeclarationForTestingIWarehouseIntegrationSupporter>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Inward", InventoryAutomationAction.Inward, declaration.GetInventoryAutomationAction());
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("Outward", InventoryAutomationAction.Outward, declaration.GetInventoryAutomationAction());
			declaration.IsChangeOfOwnershipBondedWarehousingEnabledForTesting = true;
			AssertEquals("ChangeOfOwnership", InventoryAutomationAction.ChangeOfOwnership, declaration.GetInventoryAutomationAction());
			declaration.IsChangeOfRegimeWarehousingEnabledForTesting = true;
			AssertEquals("ChangeOfRegime", InventoryAutomationAction.ChangeOfRegime, declaration.GetInventoryAutomationAction());
		}

		public void TestIsChangeOfOwnershipBondedWarehousingEnabled()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			IWarehouseIntegrationSupporter supporter = declaration;
			AssertEquals("IsChangeOfOwnershipBondedWarehousingEnabled", false, supporter.IsChangeOfOwnershipBondedWarehousingEnabled);
		}

		public void TestIsChangeOfRegimeWarehousingEnabled()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			IWarehouseIntegrationSupporter supporter = declaration;
			AssertEquals("IsChangeOfRegimeWarehousingEnabled", false, supporter.IsChangeOfRegimeWarehousingEnabled);
		}

		public void TestHasInvoiceLineWithoutEntryInstruction()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("Entry Instruction is not enabled", false, declaration.HasInvoiceLineWithoutEntryInstruction);
			declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("HasInvoiceLineWithoutEntryInstruction", true, declaration.HasInvoiceLineWithoutEntryInstruction);
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			AssertEquals("HasInvoiceLineWithoutEntryInstruction", false, declaration.HasInvoiceLineWithoutEntryInstruction);
		}

		public void TestJE_DataModel_SetOnSaving()
		{
			AssertJE_DataModel(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public void TestJE_DataModel_Jurisdiction()
		{
			AssertJE_DataModel(Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.Australia);
			AssertJE_DataModel(Core.Constants.CountryCodes.Switzerland, Core.Constants.CountryCodes.Switzerland);
			AssertJE_DataModel(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates);
			AssertJE_DataModel(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.France);

			AssertJE_DataModel(Core.Constants.CountryCodes.PuertoRico, Core.Constants.CountryCodes.UnitedStates);
			AssertJE_DataModel(Core.Constants.CountryCodes.Liechtenstein, Core.Constants.CountryCodes.Switzerland);
			AssertJE_DataModel(Core.Constants.CountryCodes.FrenchGuyana, Core.Constants.CountryCodes.France);
			AssertJE_DataModel(Core.Constants.CountryCodes.Guadeloupe, Core.Constants.CountryCodes.France);
			AssertJE_DataModel(Core.Constants.CountryCodes.Martinique, Core.Constants.CountryCodes.France);
			AssertJE_DataModel(Core.Constants.CountryCodes.Mayotte, Core.Constants.CountryCodes.France);
			AssertJE_DataModel(Core.Constants.CountryCodes.Reunion, Core.Constants.CountryCodes.France);
			AssertJE_DataModel(Core.Constants.CountryCodes.SaintMartin, Core.Constants.CountryCodes.France);
			AssertJE_DataModel(Core.Constants.CountryCodes.SaintBarthelemy, Core.Constants.CountryCodes.France);
		}

		void AssertJE_DataModel(string currentCountry, string expectedJE_DataModel)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(currentCountry))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				AssertEquals("Not set", ZString.Empty, declaration.JE_DataModel);
				Factory.Save();
				AssertEquals("OnSaving", expectedJE_DataModel, declaration.JE_DataModel);
			}
		}

		public void TestJE_DataModel_ReportErrorWhenUpdated() =>
			DataModelTestHelper.RunDataModelTest_ReportErrorWhenUpdated<BaseJobDeclaration>(Factory);

		public void TestJE_DataModel_CanSaveTwice() =>
			DataModelTestHelper.RunDataModelTest_CanSaveTwice<BaseJobDeclaration>(Factory);

		public void TestDeferWeightAllocation()
		{
			var declaration = Factory.New<TestDeclaration>();
			Factory.Save();

			var declarationInNewFactory = new BusinessObjectFactory().Load<TestDeclaration>(declaration.PK);
			_ = declarationInNewFactory.InvoiceLines;

			AssertEquals("Weight Apportionment should be deferred during InvoiceLineComplete collection load.", true, declarationInNewFactory.isWeightApportionmentDeferred);
		}

		public void TestGetScreeningPartyies_WhenOrgHeaderWasDeleted()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "TO1";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "TO2";
			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.OH_Code = "TO3";
			var orgHeader4 = Factory.New<OrgHeader>();
			orgHeader4.OH_Code = "TO4";
			var orgHeader5 = Factory.New<OrgHeader>();
			orgHeader5.OH_Code = "TO5";

			var declaration = Factory.New<TestDeclaration>();
			declaration.JE_OH_Supplier = orgHeader1.PK;
			declaration.JE_OH_Importer = orgHeader2.PK;
			declaration.JE_OH_Forwarder = orgHeader3.PK;
			declaration.JE_OA_DeliveryOrPickupCartageCoAddr = orgHeader4.PK;
			declaration.JE_OH_ShippingLine = orgHeader5.PK;

			CombineAssertions(() =>
			{
				new[] { null, orgHeader1, orgHeader2, orgHeader3, orgHeader4, orgHeader5 }.
					ForEach(header => DeleteHeaderThenAssertScreeningParty(declaration, header));
			});
		}

		void DeleteHeaderThenAssertScreeningParty(TestDeclaration declaration, OrgHeader header)
		{
			header?.Delete();
			AssertNoExceptionThrown(() =>
			{
				_ = declaration.ScreeningParties;
			});
		}

		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			Assert($"Covered by {nameof(FetchStrategies.Testing.BaseJobDeclarationFetchStrategyTest)}.", true);
		}

		public void TestEquipments()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var equipments = declaration.Equipments;
			AssertEquals(equipments.AdditionalFilter, ZQuery.NoResultQuery);
			AssertEquals(false, declaration.IsRegisteredEditableChildObject(equipments));

			var declaration2 = Factory.New<JobDeclarationForTestingEquipments>();
			var equipments2 = declaration2.Equipments;
			AssertNotEquals(equipments2.AdditionalFilter, ZQuery.NoResultQuery);
			AssertEquals(true, declaration2.IsRegisteredEditableChildObject(equipments2));
		}

		public void TestSupportEquipments()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(false, declaration.SupportEquipments);
		}

		public void TestEquipmentsRequired()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(false, declaration.EquipmentsRequired);
		}

		public void TestContainerOrEquipmentCaption()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.ContainersRequired).Returns(true);
			declarationMock.Setup(m => m.EquipmentsRequired).Returns(true);
			var declaration = declarationMock.Object;
			AssertEquals("When ContainersRequired and EquipmentsRequired are both true", "Container/Equipment", declaration.ContainerOrEquipmentCaption);

			declarationMock.Setup(m => m.ContainersRequired).Returns(false);
			AssertEquals("When ContainersRequired is false and EquipmentsRequired is true", "Equipment", declaration.ContainerOrEquipmentCaption);

			declarationMock.Setup(m => m.EquipmentsRequired).Returns(false);
			AssertEquals("When EquipmentsRequired is false", "Container No", declaration.ContainerOrEquipmentCaption);
		}

		public void TestContainerEquipmentList()
		{
			var mockDec = Factory.NewMoq<JobDeclarationForTestingEquipments>();
			var declaration = mockDec.Object;
			mockDec.Setup(x => x.ContainersRequired).Returns(true);
			mockDec.Setup(x => x.EquipmentsRequired).Returns(true);
			var containers = declaration.CusContainers;
			var container1 = containers.AddNew();
			container1.CO_ContainerNumber = "C1";
			var container2 = containers.AddNew();
			container2.CO_ContainerNumber = "C2";
			var equipments = declaration.Equipments;
			var equipment1 = equipments.AddNew();
			equipment1.CEQ_IdentificationNumber = "E1";
			var equipment2 = equipments.AddNew();
			equipment2.CEQ_IdentificationNumber = "E2";
			AssertEquals("When ContainersRequired and EquipmentsRequired are both true", "C1, C2, E1, E2", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);

			mockDec.Setup(x => x.ContainersRequired).Returns(false);
			declaration.ResetContainersAndEquipmentsOnDeclaration_List();
			AssertEquals("When ContainersRequired is false and EquipmentsRequired is true", "E1, E2", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);

			mockDec.Setup(x => x.EquipmentsRequired).Returns(false);
			declaration.ResetContainersAndEquipmentsOnDeclaration_List();
			AssertEquals("When ContainersRequired and EquipmentsRequired are both false", "", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);

			mockDec.Setup(x => x.ContainersRequired).Returns(true);
			declaration.ResetContainersAndEquipmentsOnDeclaration_List();
			AssertEquals("When ContainersRequired is true and EquipmentsRequired is false", "C1, C2", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);
		}

		public void TestAddToAndRemoveFromContainersAndEquipmentsOnDeclaration_ListIfNeeded()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);

			var container = Factory.New<BaseCusContainer>();
			container.CO_ContainerNumber = "C1";
			var equipment = Factory.New<CusEquipment>();
			equipment.CEQ_IdentificationNumber = "E1";
			declaration.AddToContainersAndEquipmentsOnDeclaration_ListIfNeeded(equipment.CEQ_IdentificationNumberInfo, true);
			declaration.AddToContainersAndEquipmentsOnDeclaration_ListIfNeeded(container.CO_ContainerNumberInfo, true);
			CombineAssertions("Add To", () =>
			{
				AssertEquals("C1, E1", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);
				AssertEquals("C1", declaration.ContainersAndEquipmentsOnDeclaration_List.GetDescriptionFromCode("C1"));
				AssertEquals("Equipment:E1", declaration.ContainersAndEquipmentsOnDeclaration_List.GetDescriptionFromCode("E1"));
			});

			declaration.RemoveFromContainersAndEquipmentsOnDeclaration_ListIfNeeded(equipment);
			AssertEquals("Remove From", "C1", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);
		}

		public void TestResetContainersAndEquipmentsOnDeclaration_List()
		{
			var mockDec = Factory.NewMoq<BaseJobDeclaration>();
			var declaration = mockDec.Object;
			mockDec.Setup(m => m.ContainersRequired).Returns(true);
			var containers = declaration.CusContainers;
			var container1 = containers.AddNew();
			container1.CO_ContainerNumber = "C1";
			var container2 = containers.AddNew();
			container2.CO_ContainerNumber = "C2";
			AssertEquals("Initial value", "C1, C2", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);

			declaration.RemoveFromContainersAndEquipmentsOnDeclaration_ListIfNeeded(container1);
			AssertEquals("Remove C1", "C2", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);

			declaration.ResetContainersAndEquipmentsOnDeclaration_List();
			AssertEquals("Reset directly", "C1, C2", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);

			declaration.RemoveFromContainersAndEquipmentsOnDeclaration_ListIfNeeded(container1);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Reset when JE_MessageType changes", "C1, C2", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);

			declaration.RemoveFromContainersAndEquipmentsOnDeclaration_ListIfNeeded(container1);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Reset when JE_TransportMode changes", "C1, C2", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);

			declaration.RemoveFromContainersAndEquipmentsOnDeclaration_ListIfNeeded(container1);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("Reset when JE_ContainerMode changes", "C1, C2", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);
		}

		public void TestUpdateContainersAndEquipmentsOnDeclaration_ListWhenCusContainersChanged()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "C1";
			var equipment = declaration.Equipments.AddNew();
			equipment.CEQ_IdentificationNumber = "E1";
			AssertEquals("C1", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C2";
			AssertEquals("C1, C2", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);

			container.CO_ContainerNumber = "C3";
			AssertEquals("C2, C3", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);

			declaration.CusContainers.Delete(container);
			AssertEquals("C2", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);
		}

		public void TestUpdateContainersAndEquipmentsOnDeclaration_ListWhenEquipmentsChanged()
		{
			var declaration = Factory.New<JobDeclarationForTestingEquipments>();
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "C1";
			var equipment = declaration.Equipments.AddNew();
			equipment.CEQ_IdentificationNumber = "E1";
			AssertEquals("C1, E1", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);

			var equipment2 = declaration.Equipments.AddNew();
			equipment2.CEQ_IdentificationNumber = "E2";
			AssertEquals("C1, E1, E2", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);

			equipment.CEQ_IdentificationNumber = "E3";
			AssertEquals("C1, E2, E3", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);

			declaration.Equipments.Delete(equipment);
			AssertEquals("C1, E2", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);
		}

		public void TestSynchronizationBetweenJE_GCAndJE_GB()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			var dec = Factory.New<BaseJobDeclaration>();
			var currentBranch = GlbBranch.CurrentBranch;
			AssertEquals("set as default", currentBranch.PK, dec.JE_GB);
			AssertEquals("set as default", currentBranch.GB_GC, dec.JE_GC);

			dec.JE_GB = branch.PK;
			AssertEquals("JE_GC is synchronized by JE_GB setting", company.PK, dec.JE_GC);

			dec.JE_GB = ZGuid.Empty;
			AssertEquals("JE_GC keep oldvalue if JE_GB is not valid Branch PK", company.PK, dec.JE_GC);
			dec.JE_GC = ZGuid.Empty;
			dec.JE_GC = company.PK;
			AssertEquals("JE_GB is synchronized by JE_GC setting", branch.PK, dec.JE_GB);

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = ZGuid.Empty;
			dec.JE_GB = branch2.PK;
			AssertEquals("JE_GC is set as branch.GB_GC", ZGuid.Empty, dec.JE_GC);

			dec.JE_GC = company.PK;
			AssertEquals("JE_GB is synchronized by JE_GC setting", branch.PK, dec.JE_GB);
		}

		public void TestShowApportionmentMenuItem_IsIntegratedCountry_IsInterface()
		{
			using (TemporarilySetInterfacedCountry(Core.Constants.CountryCodes.Switzerland))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				AssertEquals(false, declaration.ShowApportionmentMenuItem);
			}
		}

		public void TestShowApportionmentMenuItem_IsIntegratedCountry_NotIsInterface()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				AssertEquals(true, declaration.ShowApportionmentMenuItem);
			}
		}

		public void TestShowApportionmentMenuItem_NotIsIntegratedCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				AssertEquals(true, declaration.ShowApportionmentMenuItem);
			}
		}

		public void TestIsDeclarationIntegrated_BuiltinOnly()
		{
			var builtinOnlyCountryCodes = new[]
			{
				Core.Constants.CountryCodes.Australia,
				Core.Constants.CountryCodes.UnitedStates,
				Core.Constants.CountryCodes.Canada,
				Core.Constants.CountryCodes.PuertoRico
			};

			foreach (var builtinOnlyCountryCode in builtinOnlyCountryCodes)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(builtinOnlyCountryCode))
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					AssertEquals(builtinOnlyCountryCode, false, declaration.IsDeclarationIntegrated);
				}
			}
		}

		public void TestIsDeclarationIntegrated_HasBuiltInDeclaration()
		{
			CombineAssertions(() =>
			{
				foreach (var hasBuiltInDeclarationCountryCode in IntegratedCountryHelper.HasBuiltInDeclarationCountryCodes)
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(hasBuiltInDeclarationCountryCode))
					{
						var declaration = Factory.New<BaseJobDeclaration>();
						if (hasBuiltInDeclarationCountryCode == Core.Constants.CountryCodes.Japan || hasBuiltInDeclarationCountryCode == Core.Constants.CountryCodes.Mexico)
						{
							AssertEquals(hasBuiltInDeclarationCountryCode + "Application Code is default to ITF because of its registry item DeclarationApplicationCode ", true, declaration.IsDeclarationIntegrated);
						}
						else
						{
							AssertEquals(hasBuiltInDeclarationCountryCode, false, declaration.IsDeclarationIntegrated);
						}
					}
				}
			});
		}

		public void TestIsDeclarationIntegrated_NotHasBuiltInDeclaration()
		{
			CombineAssertions(() =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Russia))
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					AssertEquals("Interfaced", true, declaration.IsDeclarationIntegrated);
					declaration.JE_ApplicationCode = ZString.Empty;
					AssertEquals("Empty", true, declaration.IsDeclarationIntegrated);
				}
			});
		}

		public void TestOnExchangeRateHolderDeleted()
		{
			int count = 0;

			var declaration = Factory.New<BaseJobDeclaration>();

			var eventHandler = new EventHandler(delegate
			{ count++; });
			((ILandedCostHeader)declaration).OnExchangeRateHolderDeleted += eventHandler;

			var invoice = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoice3 = declaration.Invoices.AddNew();

			invoice.Delete();
			AssertEquals("event handler triggered", 1, count);

			invoice2.Delete();
			AssertEquals("event handler triggered", 2, count);

			((ILandedCostHeader)declaration).OnExchangeRateHolderDeleted -= eventHandler;
			invoice3.Delete();
			AssertEquals("event handler not triggered as it is removed", 2, count);
		}

		public void TestIDocsAndCartageParentUniqueConsignRef()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			IShipmentWithDocsAndCartage decAsIDocsAndCartageParent = dec;
			AssertNotNull("Precondition", decAsIDocsAndCartageParent);
			AssertEquals("IDocsAndCartageParent.UniqueConsignRef should be empty", 0, decAsIDocsAndCartageParent.UniqueConsignRef.Trim().Length);

			dec.JE_DeclarationReference = "B00000001";
			AssertEquals("IDocsAndCartageParent.UniqueConsignRef:", "B00000001", decAsIDocsAndCartageParent.UniqueConsignRef);
		}

		public void TestFetchStrategy()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			AssertEquals("JobDeclaration FetchStrategy type", typeof(FetchStrategies.BaseJobDeclarationFetchStrategy), dec.FetchStrategy.GetType());
		}

		public void TestHasLinesForInwardBondedWarehousing()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.SetSupportsBondedWarehousingForTesting(true);
			AssertEquals("Empty collection", false, declaration.HasLinesForInwardBondedWarehousing);
			BaseJobComInvoiceLine line = declaration.FilteredInvoiceLines.AddNew();
			line.SetDeclarationForTesting(declaration);

			declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(false, declaration.HasLinesForInwardBondedWarehousing);

			line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			AssertEquals(true, declaration.HasLinesForInwardBondedWarehousing);
		}

		public void TestEntriesExistWithExBondAutomationAndAllHaveEntryNumbers()
		{
			var helper = new WhsDataTestHelper(Factory);
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.SetSupportsBondedWarehousingForTesting(true);
			declaration.WarehouseDocAddress.E2_OA_Address = helper.WhsWarehouse.WW_OA_WarehouseAddress;
			AssertEquals("Empty collection", false, declaration.EntriesExistWithExBondAutomationAndAllHaveEntryNumbers);
			BaseJobComInvoiceLine line = declaration.FilteredInvoiceLines.AddNew();
			line.SetDeclarationForTesting(declaration);

			declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(false, declaration.EntriesExistWithExBondAutomationAndAllHaveEntryNumbers);

			line.SetUseBondedWarehouseAutomationForTesting(true);
			AssertEquals(false, declaration.EntriesExistWithExBondAutomationAndAllHaveEntryNumbers);

			declaration.CustomsEntryHeaders[0].EntryNumber = "Entry1";
			AssertEquals("One entry with entry no and going into bond", true, declaration.EntriesExistWithExBondAutomationAndAllHaveEntryNumbers);

			declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(false, declaration.EntriesExistWithExBondAutomationAndAllHaveEntryNumbers);
		}

		public void TestGetDefaultContainerisedContainerMode()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			AssertEquals(Enterprise.Core.Constants.ContainerModes.Containerised, declaration.GetDefaultContainerisedContainerMode());
		}

		public void TestContainerModeVisible_IsNonTransportDeclarationType()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.IsNonTransportDeclarationType).Returns(true);
			var declaration = declarationMock.Object;
			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				AssertEquals("Mail", false, declaration.ContainerModeVisible);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Sea", false, declaration.ContainerModeVisible);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Air", false, declaration.ContainerModeVisible);
			});
		}

		public void TestContainerModeVisible_NotIsNonTransportDeclarationType()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.IsNonTransportDeclarationType).Returns(false);
			var declaration = declarationMock.Object;
			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				AssertEquals("Mail", true, declaration.ContainerModeVisible);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Sea", true, declaration.ContainerModeVisible);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Air", false, declaration.ContainerModeVisible);
			});
		}

		public void TestDoesJobInvoicingHaveOverseasFreightAmount()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			AssertEquals("PreCondition:DoesJobInvoicingHaveFreightAmount", false, testDec.DoesJobInvoicingHaveFreightAmount);

			JobCharge charge = Factory.New<JobCharge>();
			charge.FillWithValidTestData();
			charge.JR_AC = Env.Registry.FreightChargeCode;

			var jobInvoicing = Factory.NewJobForTesting<JobHeader>();
			jobInvoicing.FillWithValidTestData();
			charge.JR_JH = jobInvoicing.PK;
			jobInvoicing[JobHeaderSchema.JH_ParentID.Name] = testDec.PK;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var testDecLoaded = factory2.Load<BaseJobDeclaration>(testDec.PK);
			AssertEquals("DoesJobInvoicingHaveFreightAmount", true, testDecLoaded.DoesJobInvoicingHaveFreightAmount);
		}

		public void TestSaveFailureRevertsDeclarationReferenceObtainedFromNumberFountain()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			testDec.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			bool saveFailed = false;
			try
			{
				Factory.Save();
			}
			catch
			{
				saveFailed = true;
			}
			Assert(!saveFailed);
			Assert("Declaration reference should not be blank", !testDec.JE_DeclarationReference.IsEmpty);
			testDec.JE_DeclarationReference = ZString.Empty;
			testDec.ForceExceptionAfterDeclarationReferenceAllocated = true;
			saveFailed = false;
			try
			{
				Factory.Save();
			}
			catch
			{
				saveFailed = true;
			}
			Assert(saveFailed);
			AssertEquals("Declaration reference should be blank", "", testDec.JE_DeclarationReference);
		}

		public void TestSaveFailureRevertsDeclarationReferenceObtainedFromShipment()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var declaration1 = BaseJobDeclaration.New(Factory);
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration1.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			declaration1.JE_JS = shipment1.PK;
			var invoicingJob = Factory.NewJobForTesting<JobHeader>();
			invoicingJob[JobHeaderSchema.JH_ParentID.Name] = shipment1.PK;
			invoicingJob[JobHeaderSchema.JH_ParentTableCode.Name] = JobDeclarationSchema.Constants.Prefix;
			invoicingJob[JobHeaderSchema.JH_GB.Name] = GlbBranch.CurrentBranch.PK;
			invoicingJob[JobHeaderSchema.JH_GC.Name] = GlbCompany.CurrentCompany.PK;
			invoicingJob[JobHeaderSchema.JH_GE.Name] = GlbDepartment.CurrentDepartment.PK;
			invoicingJob[JobHeaderSchema.JH_JobNum.Name] = "X";

			declaration1.ForceExceptionAfterDeclarationReferenceAllocated = true;
			bool saveFailed = false;
			try
			{
				Factory.Save();
			}
			catch
			{
				saveFailed = true;
			}
			Assert(saveFailed);

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var shipment2 = factory2.New<ForwardingShipment>();
			var declaration2 = BaseJobDeclaration.New(factory2);
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration2.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			declaration2.JE_JS = shipment2.PK;
			saveFailed = false;
			try
			{
				factory2.Save();
			}
			catch
			{
				saveFailed = true;
			}
			Assert(!saveFailed);

			shipment1.HasChanges = true;
			declaration1.HasChanges = true;
			declaration1.ForceExceptionAfterDeclarationReferenceAllocated = false;
			saveFailed = false;
			try
			{
				Factory.Save();
			}
			catch
			{
				saveFailed = true;
			}
			Assert(!saveFailed);
			Assert("Declaration reference should not be blank", !declaration1.JE_DeclarationReference.IsEmpty);
			AssertEquals("Declaration reference should be the shipment number", shipment1.JS_UniqueConsignRef, declaration1.JE_DeclarationReference);
			AssertNotEquals("Declaration reference should be different to declaration2", declaration2.JE_DeclarationReference, declaration1.JE_DeclarationReference);
		}

		public void TestJE_MessageStatusWhenSavingFails()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			declaration.JE_MessageStatus = "AAA";
			declaration.JE_ConsolidatedCargoStatus = "GGG";
			declaration.ShouldThrowExceptionOnSaving = true;

			try
			{
				Factory.Save();
			}
			catch
			{
				AssertEquals("JE_MessageStatus should have been reverted as saving fails and messages that might have been generated are going to deleted", "", declaration.JE_MessageStatus);
				AssertEquals("JE_ConsolidatedCargoStatus should have been reverted as saving fails and messages that might have been generated are going to deleted", "", declaration.JE_ConsolidatedCargoStatus);
			}

			declaration.ShouldThrowExceptionOnSaving = false;
			declaration.JE_MessageStatus = "AAA";
			declaration.JE_ConsolidatedCargoStatus = "GGG";
			Factory.Save();

			declaration.JE_MessageStatus = "BBB";
			declaration.JE_ConsolidatedCargoStatus = "HHH";
			declaration.ShouldThrowExceptionOnSaving = true;

			try
			{
				Factory.Save();
			}
			catch
			{
				AssertEquals("JE_MessageStatus should have been reverted as saving fails and messages that might have been generated are going to deleted", "AAA", declaration.JE_MessageStatus);
				AssertEquals("JE_ConsolidatedCargoStatus should have been reverted as saving fails and messages that might have been generated are going to deleted", "GGG", declaration.JE_ConsolidatedCargoStatus);
			}
		}

		public void TestJE_ConsolidationStatus()
		{
			var declaration = Factory.New<TestDeclaration>();
			declaration.JE_EntryStatus = ZString.Empty;
			AssertEquals(ZString.Empty, declaration.JE_ConsolidationStatus);

			declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			AssertEquals(ConsolidatedEntryStatusList.Codes.ReadyForConsolidation, declaration.JE_ConsolidationStatus);

			declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;
			AssertEquals(ConsolidatedEntryStatusList.Codes.AppliedToConsolidation, declaration.JE_ConsolidationStatus);

			declaration.JE_EntryStatus = "ZZZ";
			AssertEquals("Invalid Consolidation Status code is ignored", ZString.Empty, declaration.JE_ConsolidationStatus);
		}

		public void TestDefaultMessageTypeIsDisabledWhenReadonly()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageType_ReadOnly = true;
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_RL_NKClosestPort = "ERASA";                 // Eritrea is our test system - so supplier will make it export if failure
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("IMP", declaration.JE_MessageType);
		}

		public void TestInvoicesOverrideDeclarationSupporter()
		{
			var declaration1 = Factory.New<JobDeclarationSupportAdditionalInvoices>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var declaration3 = Factory.New<JobDeclarationSupportAdditionalInvoices>();
			BaseJobComInvoiceGroupHeader groupHeader1 = declaration1.JobComInvoiceGroupHeaders[0];
			var invoice1 = declaration1.Invoices.AddNew();
			var line1 = invoice1.InvoiceLines.AddNew();
			BaseJobComInvoiceGroupHeader groupHeader2 = Factory.New<JobComInvoiceGroupHeaderSupportAdditionalDeclarations>();
			groupHeader2.JZ_JE = declaration2.PK;
			groupHeader2.AttachToAdditionalDeclaration(declaration1);
			BaseJobComInvoiceHeader invoice2 = Factory.New<JobComInvoiceHeaderSupportAdditionalDeclarations>();
			invoice2.JZ_JE = declaration2.PK;
			invoice2.AttachToAdditionalDeclaration(declaration1);
			var line2 = invoice2.InvoiceLines.AddNew();

			invoice1 = (BaseJobComInvoiceHeader)declaration1.Invoices.FindByPK(invoice1.PK);
			invoice2 = (BaseJobComInvoiceHeader)declaration1.Invoices.FindByPK(invoice2.PK);
			groupHeader1 = (BaseJobComInvoiceGroupHeader)declaration1.JobComInvoiceGroupHeaders.FindByPK(groupHeader1.PK);
			groupHeader2 = (BaseJobComInvoiceGroupHeader)declaration1.JobComInvoiceGroupHeaders.FindByPK(groupHeader2.PK);
			line1 = (BaseJobComInvoiceLine)declaration1.InvoiceLines.FindByPK(line1.PK);
			line2 = (BaseJobComInvoiceLine)declaration1.InvoiceLines.FindByPK(line2.PK);

			AssertEquals("invoice1.JobDeclaration should be declaration1", declaration1.PK, invoice1.JobDeclaration.PK);
			AssertEquals("invoice2.JobDeclaration should be declaration2", declaration2.PK, invoice2.JobDeclaration.PK);
			AssertEquals("groupHeader1.JobDeclaration should be declaration1", declaration1.PK, groupHeader1.JobDeclaration.PK);
			AssertEquals("groupHeader2.JobDeclaration should be declaration2", declaration2.PK, groupHeader2.JobDeclaration.PK);
			AssertEquals("line1.Declaration should be declaration1", declaration1.PK, line1.Declaration.PK);
			AssertEquals("line2.Declaration should be declaration2", declaration2.PK, line2.Declaration.PK);

			using (new BaseJobDeclaration.InvoicesOverrideDeclarationSupporter(declaration1))
			{
				AssertEquals("invoice1.JobDeclaration should be overriden to declaration1", declaration1.PK, invoice1.JobDeclaration.PK);
				AssertEquals("invoice2.JobDeclaration should be overriden to declaration1", declaration1.PK, invoice2.JobDeclaration.PK);
				AssertEquals("groupHeader1.JobDeclaration should be overriden to declaration1", declaration1.PK, groupHeader1.JobDeclaration.PK);
				AssertEquals("groupHeader2.JobDeclaration should be overriden to declaration1", declaration1.PK, groupHeader2.JobDeclaration.PK);
				AssertEquals("line1.Declaration should be declaration1", declaration1.PK, line1.Declaration.PK);
				AssertEquals("line2.Declaration should be declaration1", declaration1.PK, line2.Declaration.PK);

				using (new BaseJobDeclaration.InvoicesOverrideDeclarationSupporter(declaration1))
				{
					AssertEquals("invoice1.JobDeclaration should be overriden to declaration1", declaration1.PK, invoice1.JobDeclaration.PK);
					AssertEquals("invoice2.JobDeclaration should be overriden to declaration1", declaration1.PK, invoice2.JobDeclaration.PK);
					AssertEquals("groupHeader1.JobDeclaration should be overriden to declaration1", declaration1.PK, groupHeader1.JobDeclaration.PK);
					AssertEquals("groupHeader2.JobDeclaration should be overriden to declaration1", declaration1.PK, groupHeader2.JobDeclaration.PK);
					AssertEquals("line1.Declaration should be declaration1", declaration1.PK, line1.Declaration.PK);
					AssertEquals("line2.Declaration should be declaration1", declaration1.PK, line2.Declaration.PK);
				}

				AssertEquals("invoice1.JobDeclaration should still be overriden to declaration1", declaration1.PK, invoice1.JobDeclaration.PK);
				AssertEquals("invoice2.JobDeclaration should still be overriden to declaration1", declaration1.PK, invoice2.JobDeclaration.PK);
				AssertEquals("groupHeader1.JobDeclaration should still be overriden to declaration1", declaration1.PK, groupHeader1.JobDeclaration.PK);
				AssertEquals("groupHeader2.JobDeclaration should still be overriden to declaration1", declaration1.PK, groupHeader2.JobDeclaration.PK);
				AssertEquals("line1.Declaration should still be declaration1", declaration1.PK, line1.Declaration.PK);
				AssertEquals("line2.Declaration should still be declaration1", declaration1.PK, line2.Declaration.PK);

				AssertExceptionThrown<DeveloperNotificationException>(() => { declaration1.Invoices.SetOverrideDeclaration(declaration3); });
			}

			AssertEquals("invoice1.JobDeclaration should be reset", declaration1.PK, invoice1.JobDeclaration.PK);
			AssertEquals("invoice2.JobDeclaration should be reset", declaration2.PK, invoice2.JobDeclaration.PK);
			AssertEquals("groupHeader1.JobDeclaration should be reset", declaration1.PK, groupHeader1.JobDeclaration.PK);
			AssertEquals("groupHeader2.JobDeclaration should be reset", declaration2.PK, groupHeader2.JobDeclaration.PK);
			AssertEquals("line1.Declaration should be declaration1", declaration1.PK, line1.Declaration.PK);
			AssertEquals("line2.Declaration should be declaration2", declaration2.PK, line2.Declaration.PK);
		}

		public void TestIsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader_SupportAdditionalGroupHeaders()
		{
			var declaration1 = Factory.New<JobDeclarationSupportAdditionalInvoices>();
			var invoice2 = Factory.New<JobComInvoiceHeaderSupportAdditionalDeclarations>();
			invoice2.AttachToAdditionalDeclaration(declaration1);
			AssertEquals("IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader", false, declaration1.IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader);

			invoice2.JobComInvoiceLines.AddNew();
			AssertEquals("IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader", true, declaration1.IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader);

			var invoice3 = Factory.New<JobComInvoiceHeaderSupportAdditionalDeclarations>();
			invoice3.AttachToAdditionalDeclaration(declaration1);
			AssertEquals("IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader", false, declaration1.IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader);

			invoice3.JobComInvoiceLines.AddNew();
			AssertEquals("IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader", true, declaration1.IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader);
		}

		public void TestUniversalCopy_DocsAndCartage()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2016, 6, 7);

			var elementType = typeof(BaseJobDeclaration);
			var interfaceType = GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(elementType, true);
			var copyTemplateTree = new CopyTemplateTree(interfaceType, elementType, BusinessObjectCopyManager.CopyTreeConfiguration);

			var docsAndCartageDetailsNode = (RelatedEntityCopyTemplateNode)((EntityCopyTemplateNode)copyTemplateTree.InnerNode).Nodes.FirstOrDefault(n => n.Name == "DocsAndCartageDetails");
			AssertNull(docsAndCartageDetailsNode);

			var docsAndCartageNode = (RelatedEntityCopyTemplateNode)((EntityCopyTemplateNode)copyTemplateTree.InnerNode).Nodes.First(n => n.Name == "DocsAndCartage");
			docsAndCartageNode.CopyMethod = RelatedEntityCopyMethod.Copy;
			var estimatedDeliveryNode = (PropertyCopyTemplateNode)((EntityCopyTemplateNode)docsAndCartageNode.InnerNode).Nodes.First(n => n.Name == "JP_EstimatedDelivery");
			estimatedDeliveryNode.CopyMethod = CopyMethod.Copy;
			var copyManager = new BusinessObjectCopyManager();
			var copiedDeclaration = (BaseJobDeclaration)copyManager.Copy(declaration, copyTemplateTree).Object;
			Assert(!((EntityCopyTemplateNode)docsAndCartageNode.InnerNode).Nodes.Any(n => n.Name == "JP_ParentID"));
			Assert(!((EntityCopyTemplateNode)docsAndCartageNode.InnerNode).Nodes.Any(n => n.Name == "JP_ParentTableCode"));
			Assert("JP_OrderItemsAsString is pieced together by OrderItems, so it doesn't need to be copied.", !((EntityCopyTemplateNode)docsAndCartageNode.InnerNode).Nodes.Any(n => n.Name == "JP_OrderItemsAsString"));
			AssertEquals("DocsAndCartage.JP_EstimatedDelivery should be copied", new ZDateTime(2016, 6, 7), copiedDeclaration.DocsAndCartage.JP_EstimatedDelivery);
			AssertEquals("Copied DocsAndCartage should be linked to the new declaration", copiedDeclaration.PK, copiedDeclaration.DocsAndCartage.JP_ParentID);
			AssertEquals("Copied DocsAndCartage should be linked to the new declaration", "JE", copiedDeclaration.DocsAndCartage.JP_ParentTableCode);
		}

		public void TestUniversalCopy_Services()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var service = declaration.Services.AddNew();
			service.ES_ServiceCode = "FUM";

			var elementType = typeof(BaseJobDeclaration);
			var interfaceType = GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(elementType, true);
			var copyTemplateTree = new CopyTemplateTree(interfaceType, elementType, BusinessObjectCopyManager.CopyTreeConfiguration);
			var servicesNode = (CollectionCopyTemplateNode)((EntityCopyTemplateNode)copyTemplateTree.InnerNode).Nodes.First(n => n.Name == "Services");
			servicesNode.CopyMethod = CollectionCopyMethod.All;
			var serviceCodeNode = (PropertyCopyTemplateNode)((EntityCopyTemplateNode)servicesNode.InnerNode).Nodes.First(n => n.Name == "ES_ServiceCode");
			serviceCodeNode.CopyMethod = CopyMethod.Copy;
			var copyManager = new BusinessObjectCopyManager();
			var copiedDeclaration = (BaseJobDeclaration)copyManager.Copy(declaration, copyTemplateTree).Object;

			AssertEquals("Services should be copied", "FUM", copiedDeclaration.Services[0].ES_ServiceCode);
		}

		public void TestUniversalCopyEntityCopyNode()
		{
			var elementType = typeof(BaseJobDeclaration);
			var copyTemplateTree = new CopyTemplateTree(elementType, elementType, BusinessObjectCopyManager.CopyTreeConfiguration);
			var innerNode = copyTemplateTree.InnerNode as EntityCopyTemplateNode;

			var additionalReferenceNumbersNode = innerNode.Nodes.FirstOrDefault(n => n.Name == "AdditionalReferenceNumbers") as CollectionCopyTemplateNode;
			AssertNotNull("AdditionalReferenceNumbers is expected as proof that Universal Copy is functioning", additionalReferenceNumbersNode);
		}

		public void TestUniversalCopyMappingKeysAttribute()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JZ_GroupInvoiceFK = declaration.JobComInvoiceGroupHeaders[0].PK;

			var elementType = typeof(BaseJobDeclaration);
			var interfaceType = GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(elementType, true);
			var copyTemplateTree = new CopyTemplateTree(interfaceType, elementType, BusinessObjectCopyManager.CopyTreeConfiguration);
			var invoicesNode = (CollectionCopyTemplateNode)((EntityCopyTemplateNode)copyTemplateTree.InnerNode).Nodes.First(n => n.Name == "JobComInvoiceHeaders");
			invoicesNode.CopyMethod = CollectionCopyMethod.All;
			var invoiceNumber = (PropertyCopyTemplateNode)((EntityCopyTemplateNode)invoicesNode.InnerNode).Nodes.First(n => n.Name == "JZ_InvoiceNumber");
			invoiceNumber.CopyMethod = CopyMethod.Copy;
			var copyManager = new BusinessObjectCopyManager();
			var copiedDeclaration = (BaseJobDeclaration)copyManager.Copy(declaration, copyTemplateTree).Object;

			AssertEquals("JZ_JZ_GroupHeaderFK is copied", copiedDeclaration.JobComInvoiceGroupHeaders[0].PK, copiedDeclaration.Invoices[0].JZ_JZ_GroupInvoiceFK);
		}

		public void TestUniversalCopyWithDuplicateTopGroupInvoices()
		{
			var elementType = typeof(BaseJobDeclaration);
			var interfaceType = GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(elementType, true);
			var copyTemplateTree = new CopyTemplateTree(interfaceType, elementType, BusinessObjectCopyManager.CopyTreeConfiguration);
			var invoicesNode = (CollectionCopyTemplateNode)((EntityCopyTemplateNode)copyTemplateTree.InnerNode).Nodes.First(n => n.Name == "JobComInvoiceHeaders");
			invoicesNode.CopyMethod = CollectionCopyMethod.All;
			var copyManager = new BusinessObjectCopyManager();

			CombineAssertions("Link the duplicated top group invoice to the new TopGroupInvoice", () =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var duplicateTopGroupInvoice = declaration.AllGroupHeaders.AddNew();
				ErrorReporter.Clear();
				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_JZ_GroupInvoiceFK = declaration.TopGroupInvoice.PK;
				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_JZ_GroupInvoiceFK = duplicateTopGroupInvoice.PK;
				var copiedDeclaration = (BaseJobDeclaration)copyManager.Copy(declaration, copyTemplateTree).Object;

				AssertEquals(ZString.Empty, ErrorReporter.LastMessageReported);
				AssertEquals(2, copiedDeclaration.AllGroupHeaders.Count);
				AssertEquals(copiedDeclaration.TopGroupInvoice, copiedDeclaration.AllGroupHeaders[0]);
				AssertEquals(copiedDeclaration.TopGroupInvoice, copiedDeclaration.AllGroupHeaders[1].GroupHeader);
				AssertEquals(copiedDeclaration.TopGroupInvoice, copiedDeclaration.Invoices[0].GroupHeader);
				AssertEquals(copiedDeclaration.AllGroupHeaders[1], copiedDeclaration.Invoices[1].GroupHeader);
			});

			CombineAssertions("Skip the duplicated top group invoice and move its invoices to the new TopGroupInvoice", () =>
			{
				var declaration = Factory.New<BaseJobDeclarationForUniversalCopyTesting>();
				var duplicateTopGroupInvoice = declaration.AllGroupHeaders.AddNew();
				ErrorReporter.Clear();
				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_JZ_GroupInvoiceFK = declaration.TopGroupInvoice.PK;
				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_JZ_GroupInvoiceFK = duplicateTopGroupInvoice.PK;
				var copiedDeclaration = (BaseJobDeclaration)copyManager.Copy(declaration, copyTemplateTree).Object;

				AssertEquals(ZString.Empty, ErrorReporter.LastMessageReported);
				AssertEquals(1, copiedDeclaration.AllGroupHeaders.Count);
				AssertEquals(copiedDeclaration.TopGroupInvoice, copiedDeclaration.AllGroupHeaders[0]);
				AssertEquals(copiedDeclaration.TopGroupInvoice, copiedDeclaration.Invoices[0].GroupHeader);
				AssertEquals(copiedDeclaration.TopGroupInvoice, copiedDeclaration.Invoices[1].GroupHeader);
			});
		}

		public void TestRelatedDeclarationAtAnyLevelIsLinkedToUltimateParent()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.FillWithValidTestData();
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "TSTORG01";
			var addr1 = org1.MainAddress;

			declaration.JE_OH_Supplier = org1.PK;

			var jobInvoicing = Factory.NewJobForTesting<JobHeader>();
			jobInvoicing.FillWithValidTestData();
			jobInvoicing[JobHeaderSchema.JH_ParentID.Name] = declaration.PK;
			Factory.Save();

			declaration.JE_HouseBill = "HB0001";
			declaration.JE_MasterBill = "MB0002";
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(4);
			declaration.JE_DeclarationReference = "one";

			using (CustomsDataRegistry.Instance.EnableInheritanceOfLinkedDeclarationsJobHeader.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var relatedDeclaration = declaration.GetNewRelatedDeclaration(Factory);
				Factory.Save();

				new JobHeader.Loader(relatedDeclaration).TryLoadOrCreateWithoutMutexForTestOnly();
				AssertEquals("relatedDeclaration JH_JH_ParentJob should be equal to first declaration Job PK", declaration.Job.PK, relatedDeclaration.Job.JH_JH_ParentJob);

				var relatedDeclaration2 = relatedDeclaration.GetNewRelatedDeclaration(Factory);
				Factory.Save();
				var jobForShipment = new JobHeader.Loader(relatedDeclaration2).TryLoadOrCreateWithoutMutexForTestOnly();
				AssertEquals("relatedDeclaration2 JH_JH_ParentJob should be equal to first declaration Job PK", declaration.Job.PK, relatedDeclaration2.Job.JH_JH_ParentJob);

				var relatedDeclaration3 = relatedDeclaration2.GetNewRelatedDeclaration(Factory);
				Factory.Save();
				new JobHeader.Loader(relatedDeclaration3).TryLoadOrCreateWithoutMutexForTestOnly();
				AssertEquals("relatedDeclaration3 JH_JH_ParentJob should be equal to first declaration Job PK", declaration.Job.PK, relatedDeclaration3.Job.JH_JH_ParentJob);

				var relatedDeclaration4 = relatedDeclaration3.GetNewRelatedDeclaration(Factory);
				relatedDeclaration3.Job.JH_JH_ParentJob = ZGuid.Empty;
				Factory.Save();
				new JobHeader.Loader(relatedDeclaration4).TryLoadOrCreateWithoutMutexForTestOnly();
				AssertEquals("relatedDeclaration4 JH_JH_ParentJob should be empty as it has a grandparent, but its parent is not attached to its grandparent.", ZGuid.Empty, relatedDeclaration4.Job.JH_JH_ParentJob);

				var relatedDeclaration5 = relatedDeclaration2.GetNewRelatedDeclaration(Factory);
				Factory.Save();
				new JobHeader.Loader(relatedDeclaration5).TryLoadOrCreateWithoutMutexForTestOnly();
				AssertEquals("relatedDeclaration5 JH_JH_ParentJob should be empty as any of its sisters (relatedDeclaration3) is not attached to parent", ZGuid.Empty, relatedDeclaration5.Job.JH_JH_ParentJob);
			}

			using (CustomsDataRegistry.Instance.EnableInheritanceOfLinkedDeclarationsJobHeader.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var relatedDeclaration2 = declaration.GetNewRelatedDeclaration(Factory);
				Factory.Save();
				var jobForShipment = new JobHeader.Loader(relatedDeclaration2).TryLoadOrCreateWithoutMutexForTestOnly();
				AssertEquals("relatedDeclaration2 JH_JH_ParentJob should be empty as EnableInheritanceOfLinkedDeclarationsJobHeader is disable", ZGuid.Empty, relatedDeclaration2.Job.JH_JH_ParentJob);
			}
		}

		public void TestJE_ApplicationCode_ReadOnly()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Lesotho))
				{
					Factory.InvalidateCachedProperties();
					AssertEquals("No LocalCountryCustomsInterface - No Built-In", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
					{
						Factory.InvalidateCachedProperties();
						AssertEquals("No LocalCountryCustomsInterface - Built-In - Used to be only Built-In", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
					}
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
					{
						Factory.InvalidateCachedProperties();
						AssertEquals("No LocalCountryCustomsInterface - Built-In - Not used to be only Built-In", false, declaration.JE_ApplicationCodeInfo.ReadOnly);
					}

					var customsInterface = new LocalCountryCustomsInterface();
					customsInterface.RecipientID = "RecipientID";
					customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;
					using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
					{
						Factory.InvalidateCachedProperties();
						AssertEquals("Has LocalCountryCustomsInterface with SubmissionType BLT", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
					}

					customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
					using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
					{
						Factory.InvalidateCachedProperties();
						AssertEquals("Has LocalCountryCustomsInterface with SubmissionType ITF", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
					}

					customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
					using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
					{
						Factory.InvalidateCachedProperties();
						AssertEquals("Has LocalCountryCustomsInterface with SubmissionType BIT", false, declaration.JE_ApplicationCodeInfo.ReadOnly);
					}

					customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
					using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
					{
						Factory.InvalidateCachedProperties();
						AssertEquals("Has LocalCountryCustomsInterface with SubmissionType BTH", false, declaration.JE_ApplicationCodeInfo.ReadOnly);
					}

					using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
					using (DataRegistry.Business.CustomsDataRegistry.Instance.CustomsWareCompany.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "TEST"))
					{
						Factory.InvalidateCachedProperties();
						AssertEquals("Has ABMInterface activated and LocalCountryCustomsInterface with SubmissionType BTH", false, declaration.JE_ApplicationCodeInfo.ReadOnly);
					}
				}
			});
		}

		public void TestJE_ApplicationCode_ReadOnly_SupportMultipleBuiltInTypes()
		{
			CombineAssertions(() =>
			{
				var declarationMoq = Factory.NewMoq<BaseJobDeclaration>();
				var declarationMoqProtected = declarationMoq.Protected();
				declarationMoqProtected.Setup<bool>("SupportMultipleBuiltInTypes").Returns(true);

				var customsInterface = new LocalCountryCustomsInterface();
				customsInterface.RecipientID = "RecipientID";
				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;

				var declaration = declarationMoq.Object;
				var entry = declaration.ActiveEntryHeaders.AddNew();
				using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertEquals("Should allow selection of Built-in Types", false, declaration.JE_ApplicationCodeInfo.ReadOnly);
					var message = entry.Messages.AddNew();
					AssertEquals("Should be read only when message started", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
					message.Delete();
					AssertEquals("Should allow selection of Built-in Types if no message", false, declaration.JE_ApplicationCodeInfo.ReadOnly);

					Factory.InvalidateCachedProperties();
					declarationMoqProtected.Setup<bool>("SupportMultipleBuiltInTypes").Returns(false);
					AssertEquals("Should be readonly when registry is set as BLT", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
				}
			});
		}

		public void TestJE_ApplicationCode_ReadOnly_EmptyConfiguration()
		{
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			((IRegistryItemInternals)DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface).DeleteValue(companyPK, Guid.Empty, Guid.Empty);
			CombineAssertions("Not Declaration In Development - AZ", () =>
			{
				AssertEquals("PreCondition:CountryHasBuiltInDeclaration(AZ)", false, IntegratedCountryHelper.CountryHasBuiltInDeclaration(Core.Constants.CountryCodes.Azerbaijan));
				AssertEquals("PreCondition:CountryHasDeclarationInDevelopment(AZ)", false, IntegratedCountryHelper.CountryHasDeclarationInDevelopment(Core.Constants.CountryCodes.Azerbaijan));
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Azerbaijan))
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					AssertEquals("JE_ApplicationCode", DeclarationApplicationCodeList.Codes.Interfaced, declaration.JE_ApplicationCode);
					AssertEquals("JE_ApplicationCodeInfo.ReadOnly", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
				}
			});

			CombineAssertions("Declaration In Development - IN", () =>
			{
				AssertEquals("PreCondition:CountryHasBuiltInDeclaration(IN)", false, IntegratedCountryHelper.CountryHasBuiltInDeclaration(Core.Constants.CountryCodes.India));
				AssertEquals("PreCondition:CountryHasDeclarationInDevelopment(IN)", true, IntegratedCountryHelper.CountryHasDeclarationInDevelopment(Core.Constants.CountryCodes.India));
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					AssertEquals("JE_ApplicationCode", DeclarationApplicationCodeList.Codes.Interfaced, declaration.JE_ApplicationCode);
					AssertEquals("JE_ApplicationCodeInfo.ReadOnly", false, declaration.JE_ApplicationCodeInfo.ReadOnly);
				}
			});
		}

		public void TestCloningDeclarationWithPivotsDoesNotClonePivotsInUnsupportedDeclaration()
		{
			PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot dec = Factory.New<PivotBetweenCWandJITest.BaseJobDeclarationWhichSupportsPackagesPivot>();

			dec.JE_TransportMode = "AIR";
			dec.JE_MasterBill = "123456789012";
			dec.JE_TotalNoOfPieces = 1;
			var pack1 = dec.Bills[0].PackingGroups[0].Packages[0];
			pack1.CW_PackQty = 100001;

			var invoice = dec.Invoices.AddNew();

			var invLine1 = dec.InvoiceLines.AddNew();
			invLine1.JI_JZ = invoice.PK;
			invoice.InvoiceLines.Add(invLine1);
			invLine1.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;

			var clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(dec, CloneType.CountryToCountryCopyWithinShipment, GlbBranch.CurrentBranch.PK).Clone();

			AssertEquals("Cloned declaration should not have any package pivots", false, clonedDeclaration.Invoices[0].InvoiceLines[0].PackagesPivot.Any());
			Factory.Save();
			clonedDeclaration.Packages[0].Delete();
			Factory.Save();
		}

		//This test should only be run in base customs - not for any extending countries
		public void TestETAGetsDelayedByPortDeliveryTimeIfIsImport()
		{
			var currentDay = ZDateTime.Today;
			var defaultDelay = Factory.New<GlbPortDeliveryTime>();
			defaultDelay.G1_FreightMode = nameof(FreightMode.LCL);
			defaultDelay.G1_RL_NKDischargePort = "USNYC";
			defaultDelay.G1_RL_NKDestinationPort = "USAAA";
			defaultDelay.G1_DaysDelayFromArrivalToDeliver = 3;

			var defaultDelay2 = Factory.New<GlbPortDeliveryTime>();
			defaultDelay2.G1_FreightMode = nameof(FreightMode.LCL);
			defaultDelay2.G1_RL_NKDischargePort = "USLAX";
			defaultDelay2.G1_RL_NKDestinationPort = "USAAA";
			defaultDelay2.G1_DaysDelayFromArrivalToDeliver = 5;

			var defaultDelay3 = Factory.New<GlbPortDeliveryTime>();
			defaultDelay3.G1_FreightMode = nameof(FreightMode.SEA);
			defaultDelay3.G1_RL_NKDischargePort = "USNYC";
			defaultDelay3.G1_RL_NKDestinationPort = "USSEA";
			defaultDelay3.G1_DaysDelayFromArrivalToDeliver = 10;

			var uSOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "USLAX"));
			AssertNotNull("US Organisation should be found", uSOrg);
			AssertEquals("USOrg must be in US", Core.Constants.CountryCodes.UnitedStates, uSOrg.UNLOCO.RL_RN_NKCountryCode);

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = GetJobDeclaration();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				declaration.JE_OH_Importer = uSOrg.PK;
				declaration.JE_RL_NKOrigin = Enterprise.Core.Constants.CountryCodes.France;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.LCL;
				declaration.JE_RL_NKPortOfArrival = "USLAX";
				declaration.JE_RL_NKFinalDestination = "USAAA";
				declaration.JE_DateOfArrival = currentDay;
				AssertEquals("ETA should be delayed by 5 days", currentDay.AddDays(5), declaration.JE_DateAtFinalDestination);

				declaration.JE_RL_NKPortOfArrival = "USNYC";
				AssertEquals("ETA should be delayed by 3 days", currentDay.AddDays(3), declaration.JE_DateAtFinalDestination);

				declaration.JE_RL_NKFinalDestination = "USSEA";
				AssertEquals("ETA should be delayed by 10 days", currentDay.AddDays(10), declaration.JE_DateAtFinalDestination);

				declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
				declaration.JE_DateAtFinalDestination = currentDay.AddDays(1);
				AssertEquals("Updating ETA should not affect ATA when ATA wasn't empty", currentDay, declaration.JE_DateOfArrival);
			}
		}

		//This test should only be run in base customs - not for any extending countries
		public void TestETDeliveryIsDelayed()
		{
			var currentDay = new ZDateTime(2005, 4, 14);
			CreateDefaultDelay(nameof(FreightMode.SEA), "USLAX", "USAAA", 1, 3);
			CreateDefaultDelay(nameof(FreightMode.AIR), "USLAX", "USAAA", 2, 4);
			CreateDefaultDelay(nameof(FreightMode.AIR), "USLAX", "USSEA", 5, 7);
			CreateDefaultDelay(nameof(FreightMode.LCL), "USLAX", "USSEA", 6, 8);

			var uSOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "USLAX"));
			AssertNotNull("US Organisation should be found", uSOrg);
			AssertEquals("USOrg must be in US", "US", uSOrg.UNLOCO.RL_RN_NKCountryCode);
			var systemCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = GetJobDeclaration();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				declaration.JE_OH_Importer = uSOrg.PK;
				declaration.JE_RL_NKOrigin = Enterprise.Core.Constants.CountryCodes.France;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.NonContainerised;
				declaration.JE_RL_NKPortOfArrival = "USLAX";
				declaration.JE_RL_NKFinalDestination = "USAAA";
				declaration.JE_DateOfArrival = currentDay;
				AssertEquals("ET Delivery should be delayed by 4 days", currentDay.AddDays(4), declaration.DocsAndCartage.JP_EstimatedDelivery);

				declaration.JE_EstimatedDeliveryOrPickup = ZDateTime.Empty;
				declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
				AssertEquals("ET Delivery should not be delayed ", ZDateTime.Empty, declaration.DocsAndCartage.JP_EstimatedDelivery);

				declaration.JE_EstimatedDeliveryOrPickup = ZDateTime.Empty;
				declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
				AssertEquals("ET Delivery be delayed by 6 days ", currentDay.AddDays(6), declaration.DocsAndCartage.JP_EstimatedDelivery);

				declaration.JE_EstimatedDeliveryOrPickup = ZDateTime.Empty;
				declaration.JE_RL_NKFinalDestination = "USSEA";
				AssertEquals("ET Delivery should NOT be defaulted as no match found", currentDay.AddDays(12), declaration.DocsAndCartage.JP_EstimatedDelivery);

				declaration.JE_EstimatedDeliveryOrPickup = ZDateTime.Today;
				declaration.JE_RL_NKFinalDestination = "USAAA";
				AssertEquals("ET Delivery be delayed by 6 days ", currentDay.AddDays(6), declaration.DocsAndCartage.JP_EstimatedDelivery);
				//Check what happens if ata updated when est. deliv. already set.
			}
		}

		[ExpectNoExceptions]
		public void TestRequiresOrderNumbersOnDocs()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_FullName = "Consignee";
			consignee.MiscServ.OM_IMImporterRequiresOrderNumbersOnDocs = true;
			consignee.MiscServ.Delete();
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_OH_Importer = consignee.PK;
			Assert(!((IShipmentWithDocsAndCartage)declaration).RequiresOrderNumbersOnDocs());

			declaration.JE_MessageType = "EXP";
			declaration.JE_OH_Exporter = consignee.PK;
			Assert(!((IShipmentWithDocsAndCartage)declaration).RequiresOrderNumbersOnDocs());
		}

		[ExpectNoExceptions]
		public void TestRequiresOrderTrackLink()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_FullName = "Consignee";
			consignee.MiscServ.OM_IMJobRequireOrderTrackLink = true;
			consignee.MiscServ.Delete();
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_OH_Importer = consignee.PK;
			Assert(!((IShipmentWithDocsAndCartage)declaration).RequiresOrderTrackLink());

			declaration.JE_MessageType = "EXP";
			declaration.JE_OH_Exporter = consignee.PK;
			Assert(!((IShipmentWithDocsAndCartage)declaration).RequiresOrderTrackLink());
		}

		public void TestJE_DateAtFinalDestinationShouldReportErrorWhenInvalidSmallDateTimeValueIsSetByService()
		{
			using (Env.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Precondition: IsBatchProcessor", true, Env.CurrentUser.IsBatchProcessor);
				AsserttInvalidSmallDateWasSetError();
			}
		}

		public void TestJE_DateAtFinalDestinationShouldReportErrorWhenInvalidSmallDateTimeValueIsSetByDataImport()
		{
			using (Env.SetTemporaryUserContext(User.InterchangeUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Precondition: CWAutoDataImport", User.InterchangeUserCode, GlbStaff.CurrentUser.GS_Code);
				AsserttInvalidSmallDateWasSetError();
			}
		}

		public void TestJE_DateAtFinalDestinationShouldNotReportErrorWhenInvalidSmallDateTimeValueIsSetByUser()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_DateAtFinalDestination = ZDateTime.MinSmallDateTimeValue.AddDays(-1);
				AssertEquals("LastMessageReported", string.Empty, ErrorReporter.LastMessageReported);
				AssertHasErrors("HasErrors", declaration.JE_DateAtFinalDestinationInfo);
			});
		}

		public void TestNeedsServiceEvents()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Assert("NeedsServiceEvents should be true", ((IServicesParent)declaration).NeedsServiceEvents);
			Assert("NeedsReferenceNumber should be true", ((IServicesParent)declaration).NeedsReferenceNumber);
		}

		#region ICustomsCustomLabelsConfigOrgProvider

		public void TestICustomsCustomLabelsConfigOrgProvider()
		{
			var customsCustomLabelsConfigOrgProvider = Factory.New<BaseJobDeclaration>() as ICustomsCustomLabelsConfigOrgProvider;
			AssertEquals(JobComInvoiceLineSchema.JI_PartAttrib1.Name, customsCustomLabelsConfigOrgProvider.PartAttribute1);
			AssertEquals(JobComInvoiceLineSchema.JI_PartAttrib2.Name, customsCustomLabelsConfigOrgProvider.PartAttribute2);
			AssertEquals(JobComInvoiceLineSchema.JI_PartAttrib3.Name, customsCustomLabelsConfigOrgProvider.PartAttribute3);
			AssertEquals(JobComInvoiceLineSchema.JI_SerialNumber.Name, customsCustomLabelsConfigOrgProvider.SerialNumber);
		}

		#endregion

		public void TestJobDeclarationCreateOrUpdateServiceEvent_OnSave()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var doc = declaration.DocsAndCartage;

			var now = ZDateTime.Now;

			var service = doc.Services.AddNew();
			service.ES_Booked = now;
			service.ES_Completed = now.AddDays(10);
			service.ES_ServiceCode = "FUM";
			service.ES_References = "REF123";

			Factory.Save();

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, new[] { "SVC", "SVR" });
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);

			var logs = ((IStmALogParent)declaration).Logs.Find(query);
			var svrLog = logs.First(c => c.SL_SE_NKEvent == "SVR");
			var svcLog = logs.First(c => c.SL_SE_NKEvent == "SVC");

			AssertNotNull("Should exist at least one log with SVR event", svrLog);
			AssertNotNull("Should exist at least one log with SVC event", svcLog);

			AssertEquals(now, svrLog.SL_EventTime);
			AssertEquals(now.AddDays(10), svcLog.SL_EventTime);

			AssertEquals("|RFN=REF123|TYP=FUM", svrLog.SL_Reference);
			AssertEquals("|RFN=REF123|TYP=FUM", svcLog.SL_Reference);

			service.ES_Booked = now.AddDays(1);
			Factory.Save();

			logs = ((IStmALogParent)declaration).Logs.Find(query);
			svrLog = logs.First(c => c.SL_SE_NKEvent == "SVR");
			AssertEquals("Should update the event date on the SVR log", now.AddDays(1), svrLog.SL_EventTime);

			service.ES_Completed = now.AddDays(15);
			Factory.Save();

			logs = ((IStmALogParent)declaration).Logs.Find(query);
			svcLog = logs.First(c => c.SL_SE_NKEvent == "SVC");
			AssertEquals("Should update the event date on the SVC log", now.AddDays(15), svcLog.SL_EventTime);

			service.ES_ServiceCode = "CLN";
			Factory.Save();

			logs = ((IStmALogParent)declaration).Logs.Find(query);
			AssertEquals(2, logs.Length);

			svrLog = logs.First(c => c.SL_SE_NKEvent == "SVR");
			svcLog = logs.First(c => c.SL_SE_NKEvent == "SVC");
			AssertEquals("Should create a new SVR log with the CLN Type Parameter", "|RFN=REF123|TYP=CLN", svrLog.SL_Reference);
			AssertEquals("Should create a new SVC log with the CLN Type Parameter", "|RFN=REF123|TYP=CLN", svcLog.SL_Reference);

			service.ES_References = "REF456";
			Factory.Save();

			logs = ((IStmALogParent)declaration).Logs.Find(query);
			AssertEquals(2, logs.Length);

			svrLog = logs.First(c => c.SL_SE_NKEvent == "SVR");
			svcLog = logs.First(c => c.SL_SE_NKEvent == "SVC");
			AssertEquals("Should create a new SVR log with the CLN Type Parameter", "|RFN=REF456|TYP=CLN", svrLog.SL_Reference);
			AssertEquals("Should create a new SVC log with the CLN Type Parameter", "|RFN=REF456|TYP=CLN", svcLog.SL_Reference);
		}

		public void TestRefVessel_LoadUsingNaturalKey_NullIfDuplicatesFound()
		{
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "DUPLICATE VESSEL";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_TransportMode = "SEA";
			declaration.JE_VesselName = vessel1.RV_Name;
			AssertNotNull("Vessel should return", declaration.Vessel);
			AssertSame("Vessel", declaration.Vessel, vessel1);

			CombineAssertions(() =>
			{
				var vessel2 = Factory.NewWithValidTestData<RefVessel>();
				vessel2.RV_Name = "DUPLICATE VESSEL";
				declaration.JE_VesselName = "DUPLICATE VESSEL";
				AssertNull("Duplicate vessel should return as null", declaration.Vessel);
			});
		}

		public void TestVesselCaching()
		{
			SetupTestVessels();
			var testDec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			testDec.JE_VesselName = "VESSEL NO LLOYDS";
			AssertEquals(testVessel3.PK, testDec.Vessel.PK);

			testDec.JE_VesselName = testVessel1.RV_Name;
			AssertEquals("Declaration.Vessel cached value should have been updated.", testVessel1.PK, testDec.Vessel.PK);

			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_Name = "TestVessel";
			testDec.JE_VesselName = testVessel.RV_Name;
			AssertEquals("Declaration.Vessel cached value should have been updated.", testVessel.PK, testDec.Vessel.PK);

			testVessel.RV_LloydsNumber = "4984731";
			AssertEquals("Declaration.Vessel cached value should have been updated.", "4984731", testDec.Vessel.RV_LloydsNumber);
		}

		public void TestRefVessel_LoadUsingNaturalKey_LoadByVesselNameIfNoLloydIMO()
		{
			SetupTestVessels();
			var testDec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			testDec.JE_VesselName = "VESSEL NO LLOYDS";
			AssertNotNull("RefVessel sans Lloyds number should still have been loaded", testDec.Vessel);
		}

		public void TestRefVessel_GetsInactiveVessels()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "INACTIVE";
			vessel.RV_IsActive = false;

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_VesselName = vessel.RV_Name;
			AssertEquals("Vessel is inactive", false, declaration.Vessel.RV_IsActive);
		}

		public void TestCheckJE_VesselName_MessageErrorOnDuplicate()
		{
			const string expectedMessageError = "Duplicate Vessels exist for this Vessel Name.\r\nUse the <F4> key to show all vessels with this name for appropriate selection of the required vessel.";
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "DUPLICATE VESSEL";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_TransportMode = "SEA";
			declaration.JE_VesselName = vessel1.RV_Name;

			CombineAssertions(() =>
			{
				AssertNoMessageError("No duplicate", declaration.JE_VesselNameInfo, expectedMessageError);

				var vessel2 = Factory.NewWithValidTestData<RefVessel>();
				vessel2.RV_Name = "DUPLICATE VESSEL";
				declaration.Validation.ValidateJE_VesselName();
				AssertHasMessageError("Has Duplicate Vessel Name", declaration.JE_VesselNameInfo, expectedMessageError);
			});
		}

		public void TestJE_VesselName_DefaultsLloydsIMOForOneMatch()
		{
			SetupTestVessels();
			var testDec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			AssertEquals("Pre-condition", "", testDec.JE_LloydsIMO);

			testDec.JE_VesselName = "HYOGO MARU";
			AssertEquals("JE_LloydsIMO should have populated from unique Vessel", "4567123", testDec.JE_LloydsIMO);

			testDec.JE_VesselName = "HYOGO KENU";
			AssertEquals("JE_LloydsIMO should have re-populated from the current unique Vessel", "6712345", testDec.JE_LloydsIMO);

			testDec.JE_VesselName = "VESSEL NO LLOYDS";
			AssertEquals("JE_LloydsIMO should have been cleared out for this Vessel", "", testDec.JE_LloydsIMO);

			testDec.JE_VesselName = "HYOGO MARU";
			AssertEquals("JE_LloydsIMO should have populated from Vessel", "4567123", testDec.JE_LloydsIMO);

			testDec.JE_VesselName = "Invalid Vessel";
			AssertEquals("JE_LloydsIMO should be cleared out if the vessel is not found", "", testDec.JE_LloydsIMO);
		}

		public void TestJE_VesselName_ClearsLloydsIMOIfEmptyOrDuplicate()
		{
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "DUPLICATE VESSEL";
			vessel1.RV_LloydsNumber = "4592837";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_TransportMode = "SEA";
			declaration.JE_VesselName = "ABEL SEAMAN";
			declaration.JE_LloydsIMO = "123456";
			declaration.JE_VesselName = "";
			AssertEquals("Lloyds number has been cleared out as blank vessel entered", "", declaration.JE_LloydsIMO);

			declaration.JE_VesselName = vessel1.RV_Name;

			CombineAssertions(() =>
			{
				AssertNotNull("No duplicate", declaration.Vessel);
				AssertEquals("Lloyds number has defaulted", "4592837", declaration.JE_LloydsIMO);

				var vessel2 = Factory.NewWithValidTestData<RefVessel>();
				vessel2.RV_Name = "DUPLICATE VESSEL";
				vessel2.RV_LloydsNumber = "7840021";

				declaration.JE_LloydsIMO = "";
				declaration.JE_VesselName = "DUPLICATE VESSEL";
				AssertNull("Duplicate vessel with the same name exists", declaration.Vessel);
				AssertEquals("Lloyds number has been cleared out until specific vessel is chosen", "", declaration.JE_LloydsIMO);

				declaration.JE_VesselName = "DUPLICATE VESSEL";
				declaration.JE_LloydsIMO = "7840021";
				AssertNotNull("Vessel should now be selected as LLoyds number is provided", declaration.Vessel);
				AssertSame("Selected Vessel is the second vessel with the same name", declaration.Vessel, vessel2);
			});
		}

		public void TestIWarehouseIntegrationSupporter_SupportModificationState()
		{
			AssertEquals(false, ((IWarehouseIntegrationSupporter)Factory.New<BaseJobDeclaration>()).SupportModificationState);
		}

		public void TestProcessTaskCollection_InheritFromGeneric()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			AssertEquals("WorkflowItems's type should inherit from ProcessTaskCollection<BaseJobDeclarationProcessTask, BaseJobDeclaration>", typeof(ProcessTaskCollection<BaseJobDeclarationProcessTask<BaseJobDeclaration>, BaseJobDeclaration>), ((IWorkflowProvider)dec).WorkflowItems.GetType().BaseType);
		}

		public void TestDeclarationAlreadyExistsForShipment_ThrowExceptionForDeclarationCreatedInAnotherFactory()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HB12312";
			Factory.Save();

			var company = GlbCompany.CurrentCompany;
			var branch = GlbBranch.CurrentBranch;
			var existedDec = (BaseJobDeclaration)shipment.GetDeclaration();
			AssertEquals(null, existedDec);
			var mutex = DeclarationBeingCreatedForShipmentMutexCreator.Create(shipment.PK);
			var createDeclarationHelper = new CreateDeclarationHelper();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var dec = newFactory.New<BaseJobDeclaration>();
			dec.JE_JS = shipment.PK;

			CombineAssertions(() =>
			{
				var dec1 = createDeclarationHelper.CreateDeclaration(shipment, mutex, () =>
				{
					existedDec = (BaseJobDeclaration)shipment.GetDeclaration();
					AssertEquals(null, existedDec);
					AssertEquals("no error", 0, ExceptionReporterTestListener.Instance.Count);

					newFactory.Save();
					return existedDec;
				});
				mutex.Unlock();

				AssertEquals("1 error", 1, ExceptionReporterTestListener.Instance.Count);
				AssertContains(FormattableString.Invariant(
$@"Can't create the declaration '{dec1.PK}' with CurrentBranchPK={branch.PK}, BranchCode={branch.GB_Code}, BranchName={branch.GB_BranchName}, CompanyCode={company.GC_Code}, CompanyName={company.GC_Name}.
There already exists declarations for the shipment '{dec.JE_JS}'.
Declaration '{dec.PK}' created by User '{dec.JE_SystemCreateUser}' at {dec.JE_SystemCreateTimeUtc.ToSmallDateTimeFloor()} with BranchPK={branch.PK}, BranchCode={branch.GB_Code}, BranchName={branch.GB_BranchName}, CompanyCode={company.GC_Code}, CompanyName={company.GC_Name}.")
					, ExceptionReporterTestListener.Instance[0].ToString());

				ExceptionReporterTestListener.Instance.Clear();
			});
		}

		public void TestRelatedTransportBookingsJobNumbers()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = "IMP";

			var bookingConsolidation1 = Factory.New<IDtbBookingConsolidation>();
			bookingConsolidation1.KB_ParentID = dec.PK;
			bookingConsolidation1.KB_ParentTableCode = "JE";
			bookingConsolidation1.KB_JobDirection = "DLV";

			var bookingConsolidation2 = Factory.New<IDtbBookingConsolidation>();
			bookingConsolidation2.KB_ParentID = dec.PK;
			bookingConsolidation2.KB_ParentTableCode = "JE";
			bookingConsolidation2.KB_JobDirection = "PIC";

			var booking1 = Factory.New<IDtbBooking>();
			booking1.KM_JobID = "TB001";
			booking1.KM_KB_Booking = bookingConsolidation1.PK;

			var booking2 = Factory.New<IDtbBooking>();
			booking2.KM_JobID = "TB002";
			booking2.KM_KB_Booking = bookingConsolidation2.PK;
			Factory.Save();

			AssertEquals("TB001", dec.RelatedTransportBookingsJobNumbers);

			dec.JE_MessageType = "EXP";
			AssertEquals("TB002", dec.RelatedTransportBookingsJobNumbers);

			var shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			Factory.Save();

			var bookingConsolidation3 = Factory.New<IDtbBookingConsolidation>();
			bookingConsolidation3.KB_ParentID = shipment.PK;
			bookingConsolidation3.KB_ParentTableCode = "JS";
			bookingConsolidation3.KB_JobDirection = "PIC";

			var booking3 = Factory.New<IDtbBooking>();
			booking3.KM_JobID = "TB003";
			booking3.KM_KB_Booking = bookingConsolidation3.PK;
			Factory.Save();

			AssertEquals("TB002,TB003", dec.RelatedTransportBookingsJobNumbers);
		}

		public void TestSupplierOrganizationAndAddress()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "TSTORG01";
			var addr1 = org1.MainAddress;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "TSTORG02";
			var addr2 = org2.MainAddress;
			var declaration = Factory.New<BaseJobDeclaration>();

			declaration.JE_OH_Supplier = org1.PK;
			AssertEquals(ZGuid.Empty, declaration.JE_OA_SupplierAddress);

			declaration.JE_OA_SupplierAddress = addr2.PK;
			AssertEquals("JE_OH_Supplier will not changed since UseSupplierAddress is false", org1.PK, declaration.JE_OH_Supplier);

			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertEquals("JE_OA_SupplierAddress will not changed since UseSupplierAddress is false", addr2.PK, declaration.JE_OA_SupplierAddress);

			var declarationUseSupplierAddress = Factory.New<BaseJobDeclarationForTesting>();
			Assert(declarationUseSupplierAddress.UseSupplierAddress);

			declarationUseSupplierAddress.JE_OH_Supplier = org1.PK;
			AssertEquals(ZGuid.Empty, declarationUseSupplierAddress.JE_OA_SupplierAddress);

			declarationUseSupplierAddress.SupplierAddressOrgPK = org2.PK;
			declarationUseSupplierAddress.JE_OA_SupplierAddress = addr2.PK;
			AssertEquals(org2.PK, declarationUseSupplierAddress.JE_OH_Supplier);

			declarationUseSupplierAddress.SupplierAddressOrgPK = ZGuid.Empty;
			declarationUseSupplierAddress.JE_OA_SupplierAddress = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, declarationUseSupplierAddress.JE_OH_Supplier);

			declarationUseSupplierAddress.SupplierAddressOrgPK = org1.PK;
			declarationUseSupplierAddress.JE_OA_SupplierAddress = addr1.PK;
			AssertEquals(org1.PK, declarationUseSupplierAddress.JE_OH_Supplier);

			declarationUseSupplierAddress.JE_OH_Supplier = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, declarationUseSupplierAddress.JE_OA_SupplierAddress);
		}

		public void TestResetToDefaultIfNotUsed()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "SQUID";
			var address1 = org1.MainAddress;
			address1.OA_Address1 = "Squid Address1";
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_OA_ImporterAddress = address1.PK;

			declaration.JE_OH_Supplier = org1.PK;
			declaration.JE_OA_SupplierAddress = address1.PK;

			AssertEquals(false, declaration.UseImporterAddress);
			AssertEquals(false, declaration.UseSupplierAddress);
			Factory.Save();

			AssertEquals("Set to default value on SupplierAddress", declaration.JE_OA_SupplierAddressInfo.DefaultValue, declaration.JE_OA_SupplierAddress);
			AssertEquals("Set to default value on ImpoterAddress", declaration.JE_OA_ImporterAddressInfo.DefaultValue, declaration.JE_OA_ImporterAddress);
		}

		public void TestImporterOrganizationAndAddress()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "TSTORG01";
			var addr1 = org1.MainAddress;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "TSTORG02";
			var addr2 = org2.MainAddress;
			var declaration = Factory.New<BaseJobDeclaration>();

			declaration.JE_OH_Importer = org1.PK;
			AssertEquals(ZGuid.Empty, declaration.JE_OA_ImporterAddress);

			declaration.JE_OA_ImporterAddress = addr2.PK;
			AssertEquals("JE_OH_Importer will not changed since UseImporterAddress is false", org1.PK, declaration.JE_OH_Importer);

			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals("JE_OA_ImporterAddress will not changed since UseImporterAddress is false", addr2.PK, declaration.JE_OA_ImporterAddress);

			var declarationUseImporterAddress = Factory.New<BaseJobDeclarationForTesting>();
			Assert(declarationUseImporterAddress.UseImporterAddress);

			declarationUseImporterAddress.JE_OH_Importer = org1.PK;
			AssertEquals(ZGuid.Empty, declarationUseImporterAddress.JE_OA_ImporterAddress);

			declarationUseImporterAddress.ImporterAddressOrgPK = org2.PK;
			declarationUseImporterAddress.JE_OA_ImporterAddress = addr2.PK;
			AssertEquals(org2.PK, declarationUseImporterAddress.JE_OH_Importer);

			declarationUseImporterAddress.ImporterAddressOrgPK = ZGuid.Empty;
			declarationUseImporterAddress.JE_OA_ImporterAddress = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, declarationUseImporterAddress.JE_OH_Importer);

			declarationUseImporterAddress.ImporterAddressOrgPK = org1.PK;
			declarationUseImporterAddress.JE_OA_ImporterAddress = addr1.PK;
			AssertEquals(org1.PK, declarationUseImporterAddress.JE_OH_Importer);

			declarationUseImporterAddress.JE_OH_Importer = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, declarationUseImporterAddress.JE_OA_ImporterAddress);
		}

		public void TestCurrentSupplierPickupAddressE2_OA_Address()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var addressPK = org.MainAddress.PK;
			var dec = Factory.New<BaseJobDeclaration>();
			dec.SupplierPickupAddress.E2_OA_Address = addressPK;
			AssertEquals(addressPK, dec.CurrentSupplierPickupAddressE2_OA_Address);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var dec1 = newFactory.Load<BaseJobDeclaration>(dec.PK);
			_ = dec1.SupplierPickupAddress;
			AssertEquals(addressPK, dec1.CurrentSupplierPickupAddressE2_OA_Address);
		}

		public void TestShouldAutoRatingForExternalBroker()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Assert(declaration.ShouldAutoRatingForExternalBroker);
		}

		public void TestIsContainerShouldLinkToOneInvoiceLineDefaultValue()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(true, declaration.IsContainerInvoiceLinkRelevant);
		}

		public void TestInvalidCartageDataIsClearedWhenMessageTypeIsChanged()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.IsNonTransportDeclarationType).Returns(false);
			var declaration = declarationMock.Object;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("IsExportOrNonTransport", true, declaration.IsExportOrNonTransport);
			var docsAndCartage = declaration.DocsAndCartage;

			var importData = new CartageData()
			{
				EstimatedInfo = docsAndCartage.JP_EstimatedDeliveryInfo,
				RequiredByInfo = docsAndCartage.JP_DeliveryRequiredByInfo,
				CartageAdvisedInfo = docsAndCartage.JP_DeliveryCartageAdvisedInfo,
				CartageCompletedInfo = docsAndCartage.JP_DeliveryCartageCompletedInfo,
				LabourTimeInfo = docsAndCartage.JP_DeliveryLabourTimeInfo,
				LabourChargeInfo = docsAndCartage.JP_DeliveryLabourChargeInfo,
				DemurrageOnTimeInfo = docsAndCartage.JP_DeliveryTruckWaitTimeInfo,
				DemurrageOnChargeInfo = docsAndCartage.JP_DeliveryTruckWaitChargeInfo,
				OA_CartageCoAddrInfo = docsAndCartage.JP_OA_DeliveryCartageCoAddrInfo,
				CartageCoPKInfo = docsAndCartage.DeliveryCartageCoPKInfo
			};
			SetupCartageData(importData);
			var exportData = new CartageData()
			{
				EstimatedInfo = docsAndCartage.JP_EstimatedPickupInfo,
				RequiredByInfo = docsAndCartage.JP_PickupRequiredByInfo,
				CartageAdvisedInfo = docsAndCartage.JP_PickupCartageAdvisedInfo,
				CartageCompletedInfo = docsAndCartage.JP_PickupCartageCompletedInfo,
				LabourTimeInfo = docsAndCartage.JP_PickupLabourTimeInfo,
				LabourChargeInfo = docsAndCartage.JP_PickupLabourChargeInfo,
				DemurrageOnTimeInfo = docsAndCartage.JP_PickupTruckWaitTimeInfo,
				DemurrageOnChargeInfo = docsAndCartage.JP_PickupTruckWaitChargeInfo,
				OA_CartageCoAddrInfo = docsAndCartage.JP_OA_PickupCartageCoAddrInfo,
				CartageCoPKInfo = docsAndCartage.PickupCartageCoPKInfo
			};
			SetupCartageData(exportData);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IsExportOrNonTransport", false, declaration.IsExportOrNonTransport);
			AssertCartageData(importData);
			AssertCartageDataIsCleared(exportData);
			SetupCartageData(exportData);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
			AssertEquals("IsExportOrNonTransport", false, declaration.IsExportOrNonTransport);
			AssertCartageData(importData);
			AssertCartageData(exportData);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("IsExportOrNonTransport", true, declaration.IsExportOrNonTransport);
			AssertCartageDataIsCleared(importData);
			AssertCartageData(exportData);
			SetupCartageData(importData);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExportDeclarationByExternalBroker;
			AssertEquals("IsExportOrNonTransport", true, declaration.IsExportOrNonTransport);
			AssertCartageData(importData);
			AssertCartageData(exportData);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
			AssertEquals("IsExportOrNonTransport", false, declaration.IsExportOrNonTransport);
			AssertCartageData(importData);
			AssertCartageDataIsCleared(exportData);
			SetupCartageData(exportData);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExportDeclarationByExternalBroker;
			AssertEquals("IsExportOrNonTransport", true, declaration.IsExportOrNonTransport);
			AssertCartageDataIsCleared(importData);
			AssertCartageData(exportData);
			SetupCartageData(importData);
			declarationMock.Setup(m => m.IsNonTransportDeclarationType).Returns(true);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IsExportOrNonTransport", true, declaration.IsExportOrNonTransport);
			AssertCartageData(importData);
			AssertCartageData(exportData);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("IsExportOrNonTransport", true, declaration.IsExportOrNonTransport);
			AssertCartageData(importData);
			AssertCartageData(exportData);
		}

		public void TestGetWorstScreeningStatusUnlessManuallyCleared()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			var result = (declaration as IScreeningPartyProvider).GetWorstScreeningStatusUnlessManuallyCleared();
			CombineAssertions(() =>
			{
				AssertNotEquals(ScreeningStatusesList.Codes.JobCleared, result);
				AssertEquals(ScreeningStatusesList.Codes.PermanentClear, result);
			});

			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			result = (declaration as IScreeningPartyProvider).GetWorstScreeningStatusUnlessManuallyCleared();
			AssertEquals("Declaration doesn't have its own status when linked with shipment.", ScreeningStatusesList.Codes.PermanentClear, result);
		}

		public void TestSetShouldUpdateScreeningStatus()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var orgHeader = Factory.New<OrgHeader>();
			AssertSetShouldUpdateScreeningStatus((headerPK) => { declaration.JE_OH_Importer = headerPK; });
			AssertSetShouldUpdateScreeningStatus((headerPK) => { declaration.JE_OH_Supplier = headerPK; });
			AssertSetShouldUpdateScreeningStatus((headerPK) => { declaration.JE_OH_ShippingLine = headerPK; });
			AssertSetShouldUpdateScreeningStatus((headerPK) => { declaration.JE_OH_Forwarder = headerPK; });

			void AssertSetShouldUpdateScreeningStatus(Action<ZGuid> setDeclarationValue)
			{
				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
				((IShouldUpdateScreeningStatus)declaration).ShouldUpdateScreeningStatus = false;
				orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				AssertEquals("Precondition", false, ((IShouldUpdateScreeningStatus)declaration).ShouldUpdateScreeningStatus);

				setDeclarationValue(orgHeader.PK);
				AssertEquals(false, ((IShouldUpdateScreeningStatus)declaration).ShouldUpdateScreeningStatus);

				setDeclarationValue(ZGuid.Empty);
				AssertEquals(false, ((IShouldUpdateScreeningStatus)declaration).ShouldUpdateScreeningStatus);

				orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				setDeclarationValue(orgHeader.PK);
				AssertEquals(true, ((IShouldUpdateScreeningStatus)declaration).ShouldUpdateScreeningStatus);

				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				((IShouldUpdateScreeningStatus)declaration).ShouldUpdateScreeningStatus = false;
				setDeclarationValue(ZGuid.Empty);
				AssertEquals(true, ((IShouldUpdateScreeningStatus)declaration).ShouldUpdateScreeningStatus);
			}
		}

		public void TestNoStackOverflowForLoadingCollection()
		{
			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.US.IJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice1 = declaration.Invoices.AddNew();
			var invoice1Line = invoice1.JobComInvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoice2Line = invoice2.JobComInvoiceLines.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var mockDec = newFactory.LoadMoq<BaseJobDeclaration>(declaration.PK);
			var dec = mockDec.Object;
			mockDec.Protected().Setup<IBillCollection<Bill, BaseJobDeclaration>>("CreateNewBillCollection")
				.Returns(new BillCollection<Bill, BaseJobDeclaration>(dec, newFactory))
				.Callback(() => _ = dec.Bills);
			mockDec.Protected().Setup<InvoiceHeaderActiveCollection>("CreateNewInvoiceHeaderCollection")
				.Returns(new InvoiceHeaderActiveCollection(dec))
				.Callback(() => _ = dec.Invoices);
			mockDec.Protected().Setup<InvoiceLineCompleteCollection>("GetNewInvoiceLineCompleteCollection")
				.Returns(new InvoiceLineCompleteCollection(dec))
				.Callback(() => _ = dec.InvoiceLines);

			CombineAssertions(() =>
			{
				_ = dec.Bills;
				AssertEquals("Loading Bills", "Bills is being called while being created; this will cause stack overflow.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
				_ = dec.Invoices;
				AssertEquals("Loading Invoices", "Invoices is being called while being created; this will cause stack overflow.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
				_ = dec.InvoiceLines;
				AssertEquals("Loading InvoiceLines", "InvoiceLines is being called while being created; this will cause stack overflow.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}

		public void TestAfterUniversalCopy()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("Should only contains a default top group invoice header.", 1, declaration.AllGroupHeaders.Count);

			var topGroup = declaration.TopGroupInvoice;

			var type = declaration.GetType();

			var extendedEntitiesAttribute = type
				.GetCustomAttributes<UniversalCopyWithExtendedEntitiesAttribute>(true)
				.First();

			var group = Factory.New<BaseJobComInvoiceGroupHeader>();
			group.JZ_JE = declaration.PK;
			group.JZ_JZ_GroupInvoiceFK = topGroup.PK;

			AssertCollectionNotContains("Should not contains it at now.", group, topGroup.JobComInvoiceGroupHeaders);

			var method = extendedEntitiesAttribute.FinishCopyMethod;
			AssertEquals("Precondition.", "AfterUniversalCopy", method);
			AssertNoExceptionThrown(() => type.InvokeMember(method, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, declaration, null));

			AssertEquals("Should only contains a default top group invoice header.", 2, declaration.AllGroupHeaders.Count);
			AssertCollectionContains("Should contains it as the AllGroupHeaders is reloaded.", group, topGroup.JobComInvoiceGroupHeaders);
		}

		public void TestSuspendSettingOfSetterSuspender()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ContainerMode = "CNT";

			var setterSuspender = declaration.SetterSuspender;

			using (setterSuspender.SuspendSetting(AutoJobDeclaration.Schema.JE_ContainerMode))
			{
				declaration.JE_ContainerMode = "LCL";

				Assert("Should is suspended.", declaration.SetterSuspender.IsSetterSuspended(AutoJobDeclaration.Schema.JE_ContainerMode));
				AssertEquals("Container Mode setter should be suspended", "CNT", declaration.JE_ContainerMode);

				using (setterSuspender.SuspendSetting(AutoJobDeclaration.Schema.JE_ContainerMode))
				{
					declaration.JE_ContainerMode = "LCL";

					Assert("Should is suspended.", declaration.SetterSuspender.IsSetterSuspended(AutoJobDeclaration.Schema.JE_ContainerMode));
					AssertEquals("Container Mode setter should still be suspended", "CNT", declaration.JE_ContainerMode);
				}

				declaration.JE_ContainerMode = "LCL";

				Assert("Should is suspended.", declaration.SetterSuspender.IsSetterSuspended(AutoJobDeclaration.Schema.JE_ContainerMode));
				AssertEquals("Container Mode setter should still be suspended", "CNT", declaration.JE_ContainerMode);
			}

			declaration.JE_ContainerMode = "LCL";

			Assert("Should is not suspended.", !declaration.SetterSuspender.IsSetterSuspended(AutoJobDeclaration.Schema.JE_ContainerMode));
			AssertEquals("Container Mode setter should not be suspended anymore", "LCL", declaration.JE_ContainerMode);
		}

		public void TestResumeSettingOfSetterSuspender()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ContainerMode = "CNT";

			var setterSuspender = declaration.SetterSuspender;

			using (setterSuspender.SuspendSetting(AutoJobDeclaration.Schema.JE_ContainerMode))
			{
				declaration.JE_ContainerMode = "LCL";

				Assert("Should is suspended.", declaration.SetterSuspender.IsSetterSuspended(AutoJobDeclaration.Schema.JE_ContainerMode));
				AssertEquals("Container Mode setter should be suspended", "CNT", declaration.JE_ContainerMode);

				using (setterSuspender.ResumeSetting(AutoJobDeclaration.Schema.JE_ContainerMode))
				{
					declaration.JE_ContainerMode = "LCL";

					Assert("Should is not suspended from the method - ResumeSetting.", !declaration.SetterSuspender.IsSetterSuspended(AutoJobDeclaration.Schema.JE_ContainerMode));
					AssertEquals("Container Mode setter suspension should be overriden", "LCL", declaration.JE_ContainerMode);
				}

				declaration.JE_ContainerMode = "";

				Assert("Should is suspended.", declaration.SetterSuspender.IsSetterSuspended(AutoJobDeclaration.Schema.JE_ContainerMode));
				AssertEquals("Container Mode setter should still be suspended", "LCL", declaration.JE_ContainerMode);
			}

			declaration.JE_ContainerMode = "";

			Assert("Should is not suspended.", !declaration.SetterSuspender.IsSetterSuspended(AutoJobDeclaration.Schema.JE_ContainerMode));
			AssertEquals("Container Mode setter should not be suspended anymore", "", declaration.JE_ContainerMode);
		}

		public void TestReloadMessages()
		{
			var declaration = Factory.New<BaseJobDeclarationForTesting>();
			var message1 = Factory.New<EDIMessage>();
			message1.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			message1.EM_LinkUniqueID = declaration.PK;
			message1.EM_ApplicationCode = "XXX";

			var message2 = Factory.New<EDIMessage>();
			message2.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			message2.EM_LinkUniqueID = declaration.PK;
			message2.EM_ApplicationCode = "YYY";
			AssertEquals(2, declaration.Messages.Count);

			declaration.Messages.Reload(true);
			AssertEquals(2, declaration.Messages.Count);
		}

		public void TestEventsThatCannotBeAddedOfLogs()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var logs = declaration.Logs;

			var expectedEventCodes = new[] { AutoEvents.LockForEditCode, AutoEvents.UnlockForEditCode };
			var actualEventCodes = logs.EventsThatCannotBeAdded.Cast<Event>().Select(c => c.Code);

			AssertContainsExactElementsInAnyOrder(expectedEventCodes, actualEventCodes);
		}

		[ExpectNoExceptions]
		public void TestUnhookFromMiscServUpdatedByDataRefresh_NullMiscServ()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var importer1 = OrgHeader.New(Factory);
			importer1.FillWithValidTestData();
			importer1.CompanyData.OB_IMUsedBondedWhs = true;
			declaration.JE_OH_Importer = importer1.PK;

			var importer2 = OrgHeader.New(Factory);
			importer2.FillWithValidTestData();
			importer2.CompanyData.OB_IMUsedBondedWhs = true;
			importer1.MiscServ.Delete();
			declaration.JE_OH_Importer = importer2.PK;
		}

		public void TestGlobalManifestTransferChanges()
		{
			var mockDec = Factory.NewMoq<BaseJobDeclaration>();
			mockDec.Setup(m => m.SupportTransferFromCustomsToManifestEvent).Returns(true);
			mockDec
				.Protected()
				.Setup<bool>("IsClearedEntryStatus", ItExpr.IsAny<ZString>())
				.Returns((ZString status) => status == "CLR");
			var dec = mockDec.Object;
			var log = dec.Logs.AddNew(Events.TransferFromManifestToCustoms, "MK322423");
			AssertEquals("IsGlobalManifestIntegrationEnabled", true, dec.IsGlobalManifestIntegrationEnabled);
			AssertEquals("GlobalManifestReference", "MK322423", dec.GlobalManifestReference);
			dec.JE_JS = ZGuid.Invalid;
			AssertEquals("IsGlobalManifestIntegrationEnabled", false, dec.IsGlobalManifestIntegrationEnabled);
			AssertEquals("GlobalManifestReference", "MK322423", dec.GlobalManifestReference);
			dec.JE_JS = ZGuid.Empty;
			AssertEquals("IsGlobalManifestIntegrationEnabled", true, dec.IsGlobalManifestIntegrationEnabled);
			AssertEquals("GlobalManifestReference", "MK322423", dec.GlobalManifestReference);
			AssertEquals("dec.JE_MasterBillInfo.ReadOnly", true, dec.JE_MasterBillInfo.ReadOnly);
			AssertEquals("dec.JE_HouseBillInfo.ReadOnly", true, dec.JE_HouseBillInfo.ReadOnly);

			dec.JE_EntryStatus = "ERR";
			AssertNull(dec.Logs.MostRecentLogByEventTime(Events.TransferFromCustomsToManifest));

			dec.JE_EntryStatus = "CLR";
			var tcmLog = dec.Logs.MostRecentLogByEventTime(Events.TransferFromCustomsToManifest);
			AssertNotNull(tcmLog);

			tcmLog.Delete();
			dec.JE_EntryStatus = "ERR";
			AssertNull(dec.Logs.MostRecentLogByEventTime(Events.TransferFromCustomsToManifest));
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = ZString.Empty;
			}
			AssertEquals("IsGlobalManifestIntegrationEnabled", false, dec.IsGlobalManifestIntegrationEnabled);
			AssertEquals("GlobalManifestReference", ZString.Empty, dec.GlobalManifestReference);
			dec.JE_EntryStatus = "CLR";
			AssertNull(dec.Logs.MostRecentLogByEventTime(Events.TransferFromCustomsToManifest));
		}

		public void TestShouldUpdateOutwardLinesWithInventoryDetails()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("ShouldUpdateOutwardLinesWithInventoryDetails", false, dec.ShouldUpdateOutwardLinesWithInventoryDetails);
			dec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("ShouldUpdateOutwardLinesWithInventoryDetails", true, dec.ShouldUpdateOutwardLinesWithInventoryDetails);
		}

		public void TestUpdateOutwardLinesWithInventoryDetailsFIFO()
		{
			var helper = new WhsDataTestHelper(Factory);
			Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				// Add the first lots to the warehouse
				var arrivalDate = new DateTime(2018, 2, 3);
				AddItemsToWareHouse(helper, "B00002132", "ENT324", 12m, arrivalDate);

				// Add the second lots to the warehouse
				arrivalDate = new DateTime(2018, 2, 5);
				AddItemsToWareHouse(helper, "B00002133", "ENT325", 25m, arrivalDate);

				// Add the third lots to the warehouse
				arrivalDate = new DateTime(2018, 2, 7);
				AddItemsToWareHouse(helper, "B00002134", "ENT326", 20m, arrivalDate);

				// Retrieve 10 items from the warehouse
				var outwardInvoiceLine = RetrieveItemsFromWarehouse(helper, "BEXW000001", "EN012312", 10m);
				Assert("Use the ENT3254 entry because it has 12 stocks and its arrival date is the earliest ", outwardInvoiceLine.JI_AddInfo.Contains("WRN=ENT324"));

				// Retrieve 15 items
				outwardInvoiceLine = RetrieveItemsFromWarehouse(helper, "BEXW000002", "EN012313", 15m);
				Assert("Use the ENT325 entry because despite ENT324 has an earlier arrrival date, it dosen't have all the demanding items", outwardInvoiceLine.JI_AddInfo.Contains("WRN=ENT325"));
			}
		}

		public void TestBondedWarehousingFlags()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Protected().Setup<bool>("SupportsBondedWarehousingCore").Returns(true);
			var declaration = declarationMock.Object;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var helper = new WhsDataTestHelper(Factory);
			AssertEquals("ClientIsBondedWarehousing", false, declaration.ClientIsBondedWarehousing);
			AssertEquals("ClientIsInwardProcessing", false, declaration.ClientIsInwardProcessing);
			AssertEquals("ClientIsOutwardProcessing", false, declaration.ClientIsOutwardProcessing);
			AssertEquals("ClientIsInventoryManagementOn", false, declaration.ClientIsInventoryManagementOn);
			AssertEquals("WarehouseAddressIsBondedWarehousing", false, declaration.WarehouseAddressIsBondedWarehousing);
			AssertEquals("HasABondedWarehousingEntryOnMultipleWarehouseEntrySystem", false, declaration.HasABondedWarehousingEntryOnMultipleWarehouseEntrySystem);
			AssertEquals("SupportsBondedWarehousingForSingleOrMultipleEntry", false, declaration.SupportsBondedWarehousingForSingleOrMultipleEntry);
			AssertEquals("SupportsBondedWarehousing", false, declaration.SupportsBondedWarehousing);

			helper.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			declaration.JE_OH_Importer = helper.Importer.PK;
			AssertEquals("ClientIsBondedWarehousing", true, declaration.ClientIsBondedWarehousing);
			AssertEquals("ClientIsInwardProcessing", false, declaration.ClientIsInwardProcessing);
			AssertEquals("ClientIsOutwardProcessing", false, declaration.ClientIsOutwardProcessing);
			AssertEquals("ClientIsInventoryManagementOn", true, declaration.ClientIsInventoryManagementOn);
			AssertEquals("WarehouseAddressIsBondedWarehousing", false, declaration.WarehouseAddressIsBondedWarehousing);
			AssertEquals("HasABondedWarehousingEntryOnMultipleWarehouseEntrySystem", false, declaration.HasABondedWarehousingEntryOnMultipleWarehouseEntrySystem);
			AssertEquals("SupportsBondedWarehousingForSingleOrMultipleEntry", true, declaration.SupportsBondedWarehousingForSingleOrMultipleEntry);
			AssertEquals("SupportsBondedWarehousing", true, declaration.SupportsBondedWarehousing);

			helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			declaration.WarehouseDocAddress.E2_OA_Address = helper.Warehouse.MainAddress.PK;
			AssertEquals("ClientIsBondedWarehousing", true, declaration.ClientIsBondedWarehousing);
			AssertEquals("ClientIsInwardProcessing", false, declaration.ClientIsInwardProcessing);
			AssertEquals("ClientIsOutwardProcessing", false, declaration.ClientIsOutwardProcessing);
			AssertEquals("ClientIsInventoryManagementOn", true, declaration.ClientIsInventoryManagementOn);
			AssertEquals("WarehouseAddressIsBondedWarehousing", true, declaration.WarehouseAddressIsBondedWarehousing);
			AssertEquals("HasABondedWarehousingEntryOnMultipleWarehouseEntrySystem", false, declaration.HasABondedWarehousingEntryOnMultipleWarehouseEntrySystem);
			AssertEquals("SupportsBondedWarehousingForSingleOrMultipleEntry", true, declaration.SupportsBondedWarehousingForSingleOrMultipleEntry);
			AssertEquals("SupportsBondedWarehousing", true, declaration.SupportsBondedWarehousing);

			helper.Importer.CompanyData.OB_IMUsedBondedWhs = false;
			helper.Importer.CompanyData.OB_CusInventoryForInwardProcessing = true;
			AssertEquals("ClientIsBondedWarehousing", false, declaration.ClientIsBondedWarehousing);
			AssertEquals("ClientIsInwardProcessing", true, declaration.ClientIsInwardProcessing);
			AssertEquals("ClientIsOutwardProcessing", false, declaration.ClientIsOutwardProcessing);
			AssertEquals("ClientIsInventoryManagementOn", true, declaration.ClientIsInventoryManagementOn);

			helper.Importer.CompanyData.OB_CusInventoryForInwardProcessing = false;
			helper.Importer.CompanyData.OB_CusInventoryForOutwardProcessing = true;
			AssertEquals("ClientIsBondedWarehousing", false, declaration.ClientIsBondedWarehousing);
			AssertEquals("ClientIsInwardProcessing", false, declaration.ClientIsInwardProcessing);
			AssertEquals("ClientIsOutwardProcessing", false, declaration.ClientIsOutwardProcessing);
			AssertEquals("ClientIsInventoryManagementOn", false, declaration.ClientIsInventoryManagementOn);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
			var entryLine = entry.MergedLines.AddNew();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			entry.CH_CEI_Instruction = instruction.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			AssertEquals("HasABondedWarehousingEntryOnMultipleWarehouseEntrySystem", false, declaration.HasABondedWarehousingEntryOnMultipleWarehouseEntrySystem);
			AssertEquals("SupportsBondedWarehousingForSingleOrMultipleEntry", true, declaration.SupportsBondedWarehousingForSingleOrMultipleEntry);
			AssertEquals("SupportsBondedWarehousing", true, declaration.SupportsBondedWarehousing);

			declarationMock.Protected().Setup<bool>("SupportMultipleWarehouseEntryCore").Returns(true);
			Factory.InvalidateCachedProperties();
			AssertEquals("HasABondedWarehousingEntryOnMultipleWarehouseEntrySystem", true, declaration.HasABondedWarehousingEntryOnMultipleWarehouseEntrySystem);
			AssertEquals("SupportsBondedWarehousingForSingleOrMultipleEntry", true, declaration.SupportsBondedWarehousingForSingleOrMultipleEntry);
			AssertEquals("SupportsBondedWarehousing", false, declaration.SupportsBondedWarehousing);
		}

		public void TestClientIsBondedWarehousingWhenCompanyDataDeleted()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			var declaration = declarationMock.Object;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var helper = new WhsDataTestHelper(Factory);
			helper.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			declaration.JE_OH_Importer = helper.Importer.PK;
			helper.Importer.Delete();
			AssertNoExceptionThrown(() =>
			{
				AssertEquals("ClientIsBondedWarehousing", false, declaration.ClientIsBondedWarehousing);
			});
		}

		public void TestDeclarationRefsIsNotLoad()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var ref1 = Factory.New<JobDecRefs>();
			ref1.J3_JE = declaration.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDec = newFactory.Load<BaseJobDeclaration>(declaration.PK);
			loadedDec.LoadChildEditableObjects();
			AssertEquals("No db hits", 0, newFactory.GetTableHitCount(JobDecRefs.Schema.TableName));
			AssertEquals(0, loadedDec.DeclarationRefs.Count);
			AssertEquals("No db hits", 0, newFactory.GetTableHitCount(JobDecRefs.Schema.TableName));
		}

		public void TestCurrentUserHasBondedWarehouseSecurityAccess()
		{
			Env.Security.ImportEditBondedWarehouse.IsAllowed = true;
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(true, declaration.CurrentUserHasBondedWarehouseSecurityAccess);

			Env.Security.ImportEditBondedWarehouse.IsAllowed = false;
			AssertEquals(false, declaration.CurrentUserHasBondedWarehouseSecurityAccess);
		}

		public void TestProcessHandlingInfoGetter_ReturnProcessHandlingInfoForDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			AssertEquals("Type of ProcessHandlingInfo", typeof(BaseJobDeclarationProcessHandlingInfo), declaration.ProcessHandlingInfo.GetType());
		}

		public void TestJobDatesProvider()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertType<DeclarationJobDatesProvider>(declaration.RatingAdapter.JobDatesProvider);
		}

		public void TestConcurrencyPolicyOnJE_ApplicationCode()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = "AAA";
			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var dec2 = factory2.Load<BaseJobDeclaration>(declaration.PK);

			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };
			var dec3 = factory3.Load<BaseJobDeclaration>(declaration.PK);
			dec3.JE_ApplicationCode = "BBB";
			factory3.Save();

			dec2.JE_ApplicationCode = "BBB";
			bool saveException = false;
			try
			{
				factory2.Save();
			}
			catch (ZSaveException)
			{
				saveException = true;
			}
			Assert("Concurrency Policy on JE_ApplicationCode should stop users from changing JE_ApplicationCode even if they change to the same value", saveException);
		}

		public void TestJE_PaidByOnJE_ApplicationCode()
		{
			CombineAssertions(() =>
			{
				var customsInterface = new LocalCountryCustomsInterface();
				customsInterface.RecipientID = "RecipientID";
				customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
				using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					var declaration = Factory.New<BaseJobDeclaration>();

					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
					AssertEquals("JE_Paidby should be defaulted to 'BRK' for interfaced declarations", PaidByCodeList.Codes.BRK, declaration.JE_PaidBy);

					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					AssertEquals("JE_Paidby should not be defaulted for non-interfaced declarations", ZString.Empty, declaration.JE_PaidBy);
				}
			});
		}

		public void TestConcurrencyPolicyOnJE_InvisibleTabsXML()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_InvisibleTabsXML = "<InvisibleTabs>AAA</InvisibleTabs>";
			Factory.Save();

			declaration.JE_RL_NKOrigin = "SGSIN";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			Factory.Save();

			declaration.JE_InvisibleTabsXML = "<InvisibleTabs>BBB</InvisibleTabs>";
			AssertNoExceptionThrown("No ZSaveException exception should be thrown", Factory.Save);
		}

		public void TestLocalCurrency()
		{
			var koreaCompany = Factory.New<GlbCompany>();
			koreaCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			koreaCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var koreaBranch = koreaCompany.Branches.AddNew();
			koreaBranch.GB_RL_NKHomePort = "NZAKL";

			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea);
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("Local currecny for Eritrea should be ERN", Core.Constants.CurrencyCodes.Eritrea, declaration.LocalCurrencyCode);
			declaration.JE_GB = koreaBranch.PK;
			AssertEquals("Local currecny for Korea", Core.Constants.CurrencyCodes.KoreaRepublicOf, declaration.LocalCurrencyCode);
			AssertEquals("Static method Local currecny for Korea", Core.Constants.CurrencyCodes.KoreaRepublicOf, BaseJobDeclaration.GetLocalCurrencyCodeFor(declaration));
			declaration.JE_GB = ZGuid.Empty;
			AssertEquals("Local currecny reverts to logged in company counhtry currency", Core.Constants.CurrencyCodes.Eritrea, declaration.LocalCurrencyCode);
			AssertEquals("Null declaration local currency reverts to logged in company counhtry currency", Core.Constants.CurrencyCodes.Eritrea, BaseJobDeclaration.GetLocalCurrencyCodeFor(null));
		}

		public void TestLandedCostHeaderIsRelatedObjectInDocManager()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var landedCostHeader = Factory.New<LandedCosting.ILandedCostHeader>() as BusinessObject;
			landedCostHeader[LandedCostHeaderSchema.LT_ParentID.Name] = declaration.PK;
			landedCostHeader[LandedCostHeaderSchema.LT_ParentTableCode.Name] = "JE";

			Factory.Save();

			var relatedObjects = declaration.DocManagerInfo.RelatedObjects;
			Assert(relatedObjects.Contains(landedCostHeader));

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			relatedObjects = shipment.DocManagerInfo.RelatedObjects;
			Assert(relatedObjects.Contains(landedCostHeader));
		}

		public void TestImportBroker()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy, declaration.RatingAdapter.ImportBroker);
		}

		public void TestExportBroker()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy, declaration.RatingAdapter.ExportBroker);
		}

		public void TestApplyWorkflowTemplatesToShipmentWhenDeclarationIsAttachedToAShipment_ByLogwalker()
		{
			CreateTestTemplate();
			var shipment = CreateShipment();

			shipment.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			shipment.CreateTasksAndMilestonesFromTemplateInvokedOnShipment = 0;
			var log = MasterFilesTestHelper.RunLogWalker();
			AssertContains("A Declaration was created.", log);
			AssertEquals("CreateTasksAndMilestonesFromTemplate on shipment won't be invoked when declaration created by Logwalker", 0, shipment.CreateTasksAndMilestonesFromTemplateInvokedOnShipment);

			var shipmentReload = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);
			AssertTriggerAddedOnshipment(shipmentReload);
		}

		public void TestApplyWorkflowTemplatesToShipmentWhenDeclarationIsAttachedToAShipment_ByForm()
		{
			CreateTestTemplate();
			var shipment = CreateShipment();

			var declaration = Factory.New<JobDeclarationForTestCreateTaskFromWorkflowTemplate>();
			declaration.JE_JS = shipment.PK;

			Factory.Save();

			AssertTriggerAddedOnshipment(shipment);
		}

		public void TestCreateTasksAndMilestonesFromTemplateOnShipmentInvokedOnlyOnce()
		{
			CombineAssertions(() =>
			{
				var shipment = Factory.New<ForwardingShipmentForTestCreateTaskFromWorkflowTemplate>();
				AssertEquals("Precondition", true, shipment.JS_IsForwardRegistered);
				var declaration = Factory.New<JobDeclarationForTestCreateTaskFromWorkflowTemplate>();
				declaration.JE_JS = shipment.PK;
				Factory.Save();
				AssertEquals("CreateTasksAndMilestonesFromTemplateInvokedOnShipment not Invoked when shipment first created without any change", 0, shipment.CreateTasksAndMilestonesFromTemplateInvokedOnShipment);
				AssertEquals("CreateTasksAndMilestonesFromTemplateInvokedOnDeclaration Invoked when only declaration change", 1, declaration.CreateTasksAndMilestonesFromTemplateInvokedOnDeclaration);

				shipment.CreateTasksAndMilestonesFromTemplateInvokedOnShipment = 0;
				declaration.CreateTasksAndMilestonesFromTemplateInvokedOnDeclaration = 0;
				Factory.Save();
				AssertEquals("CreateTasksAndMilestonesFromTemplateInvokedOnShipment Not Invoked when save without any change", 0, shipment.CreateTasksAndMilestonesFromTemplateInvokedOnShipment);
				AssertEquals("CreateTasksAndMilestonesFromTemplateInvokedOnDeclaration Not Invoked when save without any change", 0, declaration.CreateTasksAndMilestonesFromTemplateInvokedOnDeclaration);

				shipment.CreateTasksAndMilestonesFromTemplateInvokedOnShipment = 0;
				declaration.CreateTasksAndMilestonesFromTemplateInvokedOnDeclaration = 0;
				shipment.JS_HouseBill = "S0002";
				Factory.Save();
				AssertEquals("CreateTasksAndMilestonesFromTemplateInvokedOnShipment Invoked when shipment change", 1, shipment.CreateTasksAndMilestonesFromTemplateInvokedOnShipment);
				AssertEquals("CreateTasksAndMilestonesFromTemplateInvokedOnDeclaration Not Invoked when shipment change but declaration not change", 0, declaration.CreateTasksAndMilestonesFromTemplateInvokedOnDeclaration);

				shipment.CreateTasksAndMilestonesFromTemplateInvokedOnShipment = 0;
				declaration.CreateTasksAndMilestonesFromTemplateInvokedOnDeclaration = 0;
				shipment.JS_HouseBill = "S0001";
				declaration.JE_HouseBill = "H123";
				Factory.Save();
				AssertEquals("CreateTasksAndMilestonesFromTemplateInvokedOnShipment Invoked when shipment change", 1, shipment.CreateTasksAndMilestonesFromTemplateInvokedOnShipment);
				AssertEquals("CreateTasksAndMilestonesFromTemplateInvokedOnDeclaration Not Invoked when both shipment and declaration change", 0, declaration.CreateTasksAndMilestonesFromTemplateInvokedOnDeclaration);
			});
		}

		public void TestDoNotApplyWorkflowTemplatesWhenDeclarationIsAttachedToAShipment()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "BRK";
			var templateTask1 = template.WorkflowItems.Tasks.AddNew();
			templateTask1.P9_Description = "Danks Panajamos";

			Factory.Save();

			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declarationDeliveryAddress = declaration1.ImporterDeliveryAddress;
			var declarationPickupAddress = declaration1.SupplierPickupAddress;
			var shipment = Factory.New<ForwardingShipment>();
			declaration1.JE_JS = shipment.PK;

			var declaration2 = Factory.New<BaseJobDeclaration>();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(declaration2, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(declaration1, TemplateApplicationParameters.ApplyIgnoreHasChanges());

			AssertEquals("Precondition: Making sure that I know how to apply templates", 1, declaration2.WorkflowItems.Tasks.Count);
			AssertEquals("We do not expect tasks, because this declaration should proxy its workflow to the shipment, always :)", 0, declaration1.WorkflowItems.Tasks.Count);
		}

		public void TestDoNotApplyWorkflowTemplatesWhenABunchOfOtherRandomCustomsBusinessLogic()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "BRK";
			var templateTask1 = template.WorkflowItems.Tasks.AddNew();
			templateTask1.P9_Description = "Field";

			Factory.Save();

			var declaration1 = Factory.New<BaseJobDeclaration>();

			_ = declaration1.ImporterDeliveryAddress;
			_ = declaration1.SupplierPickupAddress;
			declaration1.MakeNonPersistent();

			var declaration2 = Factory.New<BaseJobDeclaration>();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(declaration2, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(declaration1, TemplateApplicationParameters.ApplyIgnoreHasChanges());

			AssertEquals("Precondition: Making sure that I know how to apply templates", 1, declaration2.WorkflowItems.Tasks.Count);
			AssertEquals("We do not expect tasks, because this declaration should proxy its workflow to the shipment, always :)", 0, declaration1.WorkflowItems.Tasks.Count);
		}

		public void TestDoNotApplyWorkflowTemplatesWhenInDatabaseAndNotChanged()
		{
			var existingTemplates = Factory.Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode));
			existingTemplates.ForEach(t => t.P0_IsActive = false);

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode;
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "General task";

			Factory.Save();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("The template should have been applied because the declaration was new. SAD!", new[] { "General task" }, declaration.WorkflowItems.Tasks.Cast<ProcessTask>().Select(x => x.P9_Description));

			declaration.WorkflowItems.RemoveAndDeleteAll();
			declaration.HasChanges = false;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(@"The job happens to be open in the same factory that saved, but it shouldn't have its templates reapplied since it doesn't actually have any changes.
This improves performance and avoids random service tasks and processes applying templates from the wrong company. SAD!", Array.Empty<string>(), declaration.WorkflowItems.Tasks.Cast<ProcessTask>().Select(x => x.P9_Description));

			declaration.HasChanges = true;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Templates should have been applied because the factory was saved AND the job has changes. SAD!", new[] { "General task" }, declaration.WorkflowItems.Tasks.Cast<ProcessTask>().Select(x => x.P9_Description));
		}

		public void TestWorkflowTemplateSetInAnotherCompany()
		{
			var existingTemplates = Factory.Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, JobInvoicingConsumerTypes.Brokerage.Code));
			existingTemplates.ForEach(t => t.P0_IsActive = false);
			Factory.Save();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "09234890342";
			orgHeader.OH_IsConsignee = true;

			var koreaCompany = Factory.New<GlbCompany>();
			koreaCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			koreaCompany.GC_Code = "~KR";
			var koreaBranch = koreaCompany.Branches.AddNew();
			koreaBranch.GB_RL_NKHomePort = "KRSEL";
			koreaBranch.GB_Code = "~KR";

			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			nzCompany.GC_Code = "~NZ";
			var nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_RL_NKHomePort = "NZAKL";
			nzBranch.GB_Code = "~NZ";

			var nzTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			nzTemplate.P0_ProcessType = JobInvoicingConsumerTypes.Brokerage.Code;
			nzTemplate.P0_SubType2 = JobMessageTypeList.Codes.Import;
			nzTemplate.P0_GC = nzCompany.PK;
			nzTemplate.WorkflowItems.Milestones.AddNew().P9_Description = "NZ Milestone";
			nzTemplate.P0_OH_Client = orgHeader.PK;

			var krTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			krTemplate.P0_ProcessType = JobInvoicingConsumerTypes.Brokerage.Code;
			krTemplate.P0_SubType2 = JobMessageTypeList.Codes.Import;
			krTemplate.P0_GC = koreaCompany.PK;
			var krMilestone = krTemplate.WorkflowItems.Milestones.AddNew();
			krMilestone.P9_Description = "KR Milestone";
			krMilestone.TriggerConditions.TriggerEventCode = "ATH";
			krTemplate.P0_OH_Client = orgHeader.PK;
			Factory.Save();

			BaseJobDeclaration nzDec = null;
			using (DisposableEnvironment.ForBranch(nzBranch.PK.ToGuid()))
			{
				nzDec = Factory.New<BaseJobDeclaration>();
				nzDec.JE_OH_Importer = orgHeader.PK;
				nzDec.JE_MessageType = JobMessageTypeList.Codes.Import;
				Factory.Save();
				AssertEquals(1, nzDec.WorkflowItems.Milestones.Count);
				AssertEquals("NZ Milestone", nzDec.WorkflowItems.Milestones[0].P9_Description);
			}

			using (DisposableEnvironment.ForBranch(koreaBranch.PK.ToGuid()))
			{
				var factory2 = new BusinessObjectFactory();
				var nzDecLoaded = factory2.Load<BaseJobDeclaration>(nzDec.PK);
				nzDecLoaded.HasChanges = true;
				factory2.Save();
				AssertEquals("system should not add workflows that belong to another company", 1, nzDecLoaded.WorkflowItems.Milestones.Count);
				AssertEquals("system should not add workflows that belong to another company", "NZ Milestone", nzDecLoaded.WorkflowItems.Milestones[0].P9_Description);
			}
		}

		public void TestNoAIDEventLoggingOnBaseJobDeclarationWithCountryRequiredDocuments()
		{
			var testClasses = new RefCountryRequiredDocumentCollectionTest();
			testClasses.CreateRequiredDocumentsForAustraliaAndOriginSingapore();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "SGSIN";
			declaration.JE_RL_NKFinalDestination = "AUBNE";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var requiredDocument = declaration.DocsAndCartage.RequiredDocuments.AddNew();

			requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.BeneficiaryCertificate;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Import;
			requiredDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = DateTime.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0001";

			Factory.Save();

			AssertNull("AID event should not be logged", declaration.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));
		}

		public void TestAIDEventLoggingOnBaseJobDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "SGSIN";
			declaration.JE_RL_NKFinalDestination = "AUBNE";

			var requiredDocument = declaration.DocsAndCartage.RequiredDocuments.AddNew();

			requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.BeneficiaryCertificate;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Import;
			requiredDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = DateTime.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0001";

			Factory.Save();

			AssertNotNull("AID event should be logged", declaration.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));
		}

		public void TestNoAEDEventLoggingOnBaseJobDeclarationWithCountryRequiredDocuments()
		{
			var testClasses = new RefCountryRequiredDocumentCollectionTest();
			testClasses.CreateRequiredDocumentsForAustraliaAndOriginSingapore();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "SGSIN";
			declaration.JE_RL_NKFinalDestination = "AUBNE";

			var requiredDocument = declaration.DocsAndCartage.RequiredDocuments.AddNew();

			requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.BeneficiaryCertificate;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Export;
			requiredDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = DateTime.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0001";

			Factory.Save();

			AssertNull("AED event should not be logged", declaration.Logs.MostRecentLogByEventTime(Events.AllExportDocumentsReceived));
		}

		public void TestAEDEventLoggingOnBaseJobDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "SGSIN";
			declaration.JE_RL_NKFinalDestination = "AUBNE";

			var requiredDocument = declaration.DocsAndCartage.RequiredDocuments.AddNew();

			requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.BeneficiaryCertificate;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Export;
			requiredDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = DateTime.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0001";

			Factory.Save();

			AssertNotNull("AED event should be logged", declaration.Logs.MostRecentLogByEventTime(Events.AllExportDocumentsReceived));
		}

		public void TestIsInwardBondedWarehousingEnabled()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclarationWithEntryInstructions>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
			declarationMock.Protected().Setup<bool>("SupportsBondedWarehousingCore").Returns(true);
			declarationMock.Protected().Setup<bool>("SupportMultipleWarehouseEntryCore").Returns(false);
			var declaration = declarationMock.Object;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var helper = new WhsDataTestHelper(Factory);
			declaration.JE_OH_Importer = helper.Importer.PK;
			AssertEquals(false, declaration.IsInwardBondedWarehousingEnabled);
			AssertEquals(false, declaration.IsInwardBondedWarehousingEnabledForSingleOrMultipleEntry);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			AssertEquals(true, declaration.IsInwardBondedWarehousingEnabled);
			AssertEquals(true, declaration.IsInwardBondedWarehousingEnabledForSingleOrMultipleEntry);
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals(false, declaration.IsInwardBondedWarehousingEnabled);
			AssertEquals(false, declaration.IsInwardBondedWarehousingEnabledForSingleOrMultipleEntry);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Importer = helper.Importer.PK;
			AssertEquals(false, declaration.IsInwardBondedWarehousingEnabled);
			AssertEquals(false, declaration.IsInwardBondedWarehousingEnabledForSingleOrMultipleEntry);

			declarationMock.Protected().Setup<bool>("SupportMultipleWarehouseEntryCore").Returns(true);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JobComInvoiceLines.RemoveAndDeleteAll();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = helper.InwardCusProcedure.ZZ6_ProcedureCode;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			AssertEquals(false, declaration.IsInwardBondedWarehousingEnabled);
			AssertEquals(false, declaration.IsInwardBondedWarehousingEnabledForSingleOrMultipleEntry);

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
			AssertEquals(false, declaration.IsInwardBondedWarehousingEnabled);
			AssertEquals(true, declaration.IsInwardBondedWarehousingEnabledForSingleOrMultipleEntry);

			invoiceLine.JI_Procedure = ZString.Empty;
			AssertEquals(false, declaration.IsInwardBondedWarehousingEnabled);
			AssertEquals(false, declaration.IsInwardBondedWarehousingEnabledForSingleOrMultipleEntry);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = helper.Importer.PK;
			invoiceLine.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
			AssertEquals(false, declaration.IsInwardBondedWarehousingEnabled);
			AssertEquals(true, declaration.IsInwardBondedWarehousingEnabledForSingleOrMultipleEntry);
		}

		public void TestIsOutwardBondedWarehousingEnabled()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclarationWithEntryInstructions>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
			declarationMock.Protected().Setup<bool>("SupportsBondedWarehousingCore").Returns(true);
			declarationMock.Protected().Setup<bool>("SupportMultipleWarehouseEntryCore").Returns(false);
			var declaration = declarationMock.Object;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var helper = new WhsDataTestHelper(Factory);
			declaration.JE_OH_Importer = helper.Importer.PK;
			AssertEquals(false, declaration.IsOutwardBondedWarehousingEnabled);
			AssertEquals(false, declaration.IsOutwardBondedWarehousingEnabledForSingleOrMultipleEntry);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals(true, declaration.IsOutwardBondedWarehousingEnabled);
			AssertEquals(true, declaration.IsOutwardBondedWarehousingEnabledForSingleOrMultipleEntry);

			declarationMock.Protected().Setup<bool>("SupportMultipleWarehouseEntryCore").Returns(true);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = helper.OutwardCusProcedure.ZZ6_ProcedureCode;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			AssertEquals(false, declaration.IsOutwardBondedWarehousingEnabled);
			AssertEquals(false, declaration.IsOutwardBondedWarehousingEnabledForSingleOrMultipleEntry);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = helper.OutwardCusProcedure.ZZ6_ProcedureCode + helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode;
			AssertEquals(false, declaration.IsOutwardBondedWarehousingEnabled);
			AssertEquals(true, declaration.IsOutwardBondedWarehousingEnabledForSingleOrMultipleEntry);

			invoiceLine.JI_Procedure = ZString.Empty;
			AssertEquals(false, declaration.IsOutwardBondedWarehousingEnabled);
			AssertEquals(false, declaration.IsOutwardBondedWarehousingEnabledForSingleOrMultipleEntry);
		}

		public void TestGetMessageErrorOfRequiredFieldsForBondedWarehousing()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "O234";
			importer.MainAddress.OA_Address1 = "1";
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var importer2 = Factory.New<OrgHeader>();
			importer2.OH_Code = "O236";
			importer2.MainAddress.OA_Address1 = "1";

			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "O235";
			warehouse.MainAddress.OA_Address1 = "1";
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
			declarationMock.Protected().Setup<bool>("IsInvoiceQuantityRequiredForBondedWarehouse").Returns(true);
			declarationMock.Protected().Setup<bool>("IsBondedWhsQuantityRequiredForBondedWarehouse").Returns(true);
			var declaration = declarationMock.Object;
			var helperMock = new Mock<BondedWarehousingHelper>(declaration) { CallBase = true };
			helperMock
				.Protected()
				.Setup<bool>("HasBondedWarehouseEntryDetailsCore", ItExpr.IsAny<BaseJobComInvoiceLine>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>())
				.Returns(false);
			helperMock
				.Protected()
				.Setup<bool>("IsMarkedForBondedWarehousingCore", ItExpr.IsAny<BaseJobComInvoiceLine>())
				.Returns((BaseJobComInvoiceLine targetInvoiceLine) => targetInvoiceLine.Declaration.IsExWarehouse ? targetInvoiceLine.UseBondedWarehouseAutomation : targetInvoiceLine.IsGoingIntoBondedWarehouse);
			declarationMock.Protected().Setup<BondedWarehousingHelper>("GetNewBondedWarehousingHelper").Returns(helperMock.Object);
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.Delete();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.SetSupportsBondedWarehousingForTesting(true);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);

			var whsInventory = Factory.New<IWhsInventoryView>();
			whsInventory.WI_AllocationKey = "WI123";
			var inventory = Factory.New<JobComInvLineComponentInventory>();
			inventory.JIV_AllocationKey = whsInventory.WI_AllocationKey;
			invoiceLine.ComponentInventoryCollection.Add(inventory);

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "JD4345";
			part.RelatedOrganisations.AddOwner(importer);

			var classification = Factory.New<BaseCusClassification>();
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			classification.CC_TariffNum = "10101010";

			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_CC = classification.PK;
			pivot.CI_OP = part.PK;

			invoice.JZ_OH_Buyer = importer2.PK;

			var bondedWarehouseMessage = BaseJobDeclaration.BondedWarehouseIsRequiredForBondedWarehousing("Inventory Management", "O234");
			var bondedWarehouseNotInCountryMessage = BaseJobDeclaration.BondedWarehouseAddressShouldBeInsideDeclarationCountry("Inventory Management", "O234", GlbCompany.CurrentCompany.Country.RN_DescMultilingual);
			var errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
			AssertContains(bondedWarehouseMessage, errorMessage);
			AssertNotContains(bondedWarehouseNotInCountryMessage, errorMessage);
			AssertContains(declaration.ImporterDocumentaryAddressIsRequiredForBondedWarehousing, errorMessage);
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), errorMessage);
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), errorMessage);
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresACountableQuantityAndUnit("Inventory Management"), errorMessage);
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresEntryDetails("Inventory Management"), errorMessage);
			AssertContains(BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentImporterMessage("Inventory Management"), errorMessage);

			declarationMock.Setup(m => m.IsInvoiceQuantityRequiredForBondedWarehouse).Returns(false);
			errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
			AssertContains(bondedWarehouseMessage, errorMessage);
			AssertNotContains(bondedWarehouseNotInCountryMessage, errorMessage);
			AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), errorMessage);
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresACountableQuantityAndUnit("Inventory Management"), errorMessage);

			declarationMock.Setup(m => m.IsBondedWhsQuantityRequiredForBondedWarehouse).Returns(false);
			errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
			AssertContains(bondedWarehouseMessage, errorMessage);
			AssertNotContains(bondedWarehouseNotInCountryMessage, errorMessage);
			AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), errorMessage);
			AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresACountableQuantityAndUnit("Inventory Management"), errorMessage);

			declarationMock.Setup(m => m.IsBondedWhsQuantityRequiredForBondedWarehouse).Returns(true);
			errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
			AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), errorMessage);
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresACountableQuantityAndUnit("Inventory Management"), errorMessage);

			invoiceLine.JI_BondedWhsQuantity = 1m;
			invoiceLine.JI_BondedWhsUnitQty = "PK";
			errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
			AssertContains(bondedWarehouseMessage, errorMessage);
			AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), errorMessage);
			AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresACountableQuantityAndUnit("Inventory Management"), errorMessage);

			declarationMock.Setup(m => m.IsAllocatedQuantityRequiredForBondedWarehouse).Returns(false);
			errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
			AssertNotContains("No error when IsAllocatedQuantityRequiredForBondedWarehouse is false", BaseJobDeclaration.InvoiceLineMarkedForAllocatedInventoryRequiresACountableQuantity("Inventory Management"), errorMessage);

			declarationMock.Setup(m => m.IsAllocatedQuantityRequiredForBondedWarehouse).Returns(true);
			invoiceLine.JI_PartNo = part.OP_PartNum;
			inventory.JIV_QuantityToDraw = 0;
			errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
			AssertContains("Error when JIV_QuantityToDraw is less than or equal to 0", BaseJobDeclaration.InvoiceLineMarkedForAllocatedInventoryRequiresACountableQuantity("Inventory Management"), errorMessage);

			inventory.JIV_QuantityToDraw = 1;
			errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
			AssertNotContains("No error when JIV_QuantityToDraw is greater than 0", BaseJobDeclaration.InvoiceLineMarkedForAllocatedInventoryRequiresACountableQuantity("Inventory Management"), errorMessage);

			invoiceLine.JI_PartNo = ZString.Empty;
			declarationMock.Setup(m => m.IsInvoiceQuantityRequiredForBondedWarehouse).Returns(true);
			declarationMock.Setup(m => m.IsBondedWhsQuantityRequiredForBondedWarehouse).Returns(false);
			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
			AssertEquals("No error when there is no inward lines", "", declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
			AssertContains(bondedWarehouseMessage, errorMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("No error when warehouse is not applicable", "", declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());

			declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
			AssertContains(bondedWarehouseMessage, errorMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExportDeclarationByExternalBroker;
			AssertEquals("No error when warehouse is not applicable", "", declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());

			declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
			errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
			AssertContains(bondedWarehouseMessage, errorMessage);

			declaration.WarehouseTransactionStatus = ZString.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing(checkProduct: false);
			AssertContains(bondedWarehouseMessage, errorMessage);
			AssertNotContains(bondedWarehouseNotInCountryMessage, errorMessage);
			AssertContains(declaration.ImporterDocumentaryAddressIsRequiredForBondedWarehousing, errorMessage);
			AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), errorMessage);
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), errorMessage);
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresEntryDetails("Inventory Management"), errorMessage);
			AssertContains(BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentImporterMessage("Inventory Management"), errorMessage);
			errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing(checkQuantity: false);
			AssertContains(bondedWarehouseMessage, errorMessage);
			AssertNotContains(bondedWarehouseNotInCountryMessage, errorMessage);
			AssertContains(declaration.ImporterDocumentaryAddressIsRequiredForBondedWarehousing, errorMessage);
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), errorMessage);
			AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), errorMessage);
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresEntryDetails("Inventory Management"), errorMessage);
			AssertContains(BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentImporterMessage("Inventory Management"), errorMessage);
			errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing(checkEntryDetails: false);
			AssertContains(bondedWarehouseMessage, errorMessage);
			AssertNotContains(bondedWarehouseNotInCountryMessage, errorMessage);
			AssertContains(declaration.ImporterDocumentaryAddressIsRequiredForBondedWarehousing, errorMessage);
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), errorMessage);
			AssertContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), errorMessage);
			AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresEntryDetails("Inventory Management"), errorMessage);
			AssertContains(BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentImporterMessage("Inventory Management"), errorMessage);
			declaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
			errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
			AssertNotContains(bondedWarehouseMessage, errorMessage);
			AssertContains(bondedWarehouseNotInCountryMessage, errorMessage);

			var port = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			warehouse.OH_RL_NKClosestPort = port.RL_Code;
			declaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
			errorMessage = declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing();
			AssertNotContains(bondedWarehouseMessage, errorMessage);
			AssertNotContains(bondedWarehouseNotInCountryMessage, errorMessage);

			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			AssertNotContains(declaration.ImporterDocumentaryAddressIsRequiredForBondedWarehousing, declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
			invoiceLine.JI_PartNo = part.OP_PartNum;
			AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAProduct("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
			invoiceLine.JI_InvoiceQuantity = 1m;
			AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
			AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
			helperMock
				.Protected()
				.Setup<bool>("HasBondedWarehouseEntryDetailsCore", ItExpr.IsAny<BaseJobComInvoiceLine>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>())
				.Returns(true);
			Factory.InvalidateCachedProperties();
			AssertNotContains(BaseJobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresEntryDetails("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
			invoice.JZ_OH_Buyer = ZGuid.Empty;
			AssertNotContains(BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentImporterMessage("Inventory Management"), declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing());
		}

		public void TestWarehouseAddressValidationForBondedWarehousing()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				DataRegistry.Business.CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "O234";
				importer.MainAddress.OA_Address1 = "1";
				importer.CompanyData.OB_IMUsedBondedWhs = true;
				var warehouse = Factory.New<OrgHeader>();
				warehouse.OH_Code = "O235";
				warehouse.MainAddress.OA_Address1 = "1";
				var port = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
				warehouse.OH_RL_NKClosestPort = port.RL_Code;
				var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
				var declaration = declarationMock.Object;
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.SetSupportsBondedWarehousingForTesting(true);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				declaration.WarehouseDocAddress.OrganisationPK = warehouse.PK;

				var bondedWarehouseMessage = BaseJobDeclaration.BondedWarehouseIsRequiredForBondedWarehousing("Inventory Management", "O234");
				AssertNoMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, bondedWarehouseMessage);
				var bondedWarehouseNotInCountryMessage = BaseJobDeclaration.BondedWarehouseAddressShouldBeInsideDeclarationCountry("Inventory Management", "O234", GlbCompany.CurrentCompany.Country.RN_DescMultilingual);
				AssertNoMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, bondedWarehouseNotInCountryMessage);
				warehouse.OH_RL_NKClosestPort = ZString.Empty;
				declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, bondedWarehouseNotInCountryMessage);
				declarationMock.Protected().Setup<bool>("IsWarehouseDocAddressRequiredForWarehouseValidation").Returns(false);
				declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, bondedWarehouseNotInCountryMessage);
				declaration.WarehouseDocAddress.OrganisationPK = ZGuid.Empty;
				AssertNoMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, bondedWarehouseMessage);
				AssertNoMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, bondedWarehouseNotInCountryMessage);
				declarationMock.Protected().Setup<bool>("IsWarehouseDocAddressRequiredForWarehouseValidation");
				declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, bondedWarehouseMessage);

				declaration.SetSupportsBondedWarehousingForTesting(false);
				declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, bondedWarehouseMessage);

				declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-1);
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.SetSupportsBondedWarehousingForTesting(true);
				declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo, bondedWarehouseMessage);
			}
		}

		public void TestImporterDocumentaryAddressIsRequiredForBondedWarehousing()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				DataRegistry.Business.CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "O234";
				importer.MainAddress.OA_Address1 = "1";
				importer.CompanyData.OB_IMUsedBondedWhs = true;
				var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
				var declaration = declarationMock.Object;
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.SetSupportsBondedWarehousingForTesting(true);
				declarationMock.Protected().Setup<bool>("IsImporterDocumentaryAddressRequiredForWarehouseValidationCore").Returns(false);
				declaration.ImporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				AssertNoMessageError(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, declaration.ImporterDocumentaryAddressIsRequiredForBondedWarehousing);
				declarationMock.Protected().Setup<bool>("IsImporterDocumentaryAddressRequiredForWarehouseValidationCore");
				declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, declaration.ImporterDocumentaryAddressIsRequiredForBondedWarehousing);

				declaration.ImporterDocumentaryAddress.OrganisationPK = importer.PK;
				AssertNoMessageError(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, declaration.ImporterDocumentaryAddressIsRequiredForBondedWarehousing);

				declaration.SetSupportsBondedWarehousingForTesting(false);
				declaration.ImporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				AssertNoMessageError(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, declaration.ImporterDocumentaryAddressIsRequiredForBondedWarehousing);

				declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-1);
				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.SetSupportsBondedWarehousingForTesting(true);
				declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, declaration.ImporterDocumentaryAddressIsRequiredForBondedWarehousing);
			}
		}

		public void TestImporterDocumentaryAddressIsRequiredForBondedWarehousing_MessageErrorForShipment()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			var declaration = declarationMock.Object;
			CombineAssertions(() =>
			{
				AssertEquals("ImporterDocumentaryAddressIsRequiredForBondedWarehousing standalone", "Importer Documentary Address is required for Inventory Management integration.", declaration.ImporterDocumentaryAddressIsRequiredForBondedWarehousing);
				AssertEquals("SupplierDocumentaryAddressIsRequiredForBondedWarehousing standalone", "Supplier Documentary Address is required for Inventory Management integration.", declaration.SupplierDocumentaryAddressIsRequiredForBondedWarehousing);

				declarationMock.Setup(m => m.JE_JS).Returns(ZGuid.BrettsGuid);
				AssertEquals("ImporterDocumentaryAddressIsRequiredForBondedWarehousing not standalone", "Consignee Documentary Address is required for Inventory Management integration.", declaration.ImporterDocumentaryAddressIsRequiredForBondedWarehousing);
				AssertEquals("SupplierDocumentaryAddressIsRequiredForBondedWarehousing not standalone", "Consignor Documentary Address is required for Inventory Management integration.", declaration.SupplierDocumentaryAddressIsRequiredForBondedWarehousing);
			});
		}

		public void TestCannotChangeWarehouseAddressOrganisationPKWhenThereIsWHSTransaction()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "O234";
			org1.MainAddress.OA_Address1 = "1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "O235";
			org2.MainAddress.OA_Address1 = "1";
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
			var declaration = declarationMock.Object;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.WarehouseDocAddress.OrganisationPK = org1.PK;
			_ = declaration.CustomsEntryHeaders.AddNew();
			declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
			var errorMessage = "There is an Inventory transaction created against this job.\r\nPlease cancel it before changing this value.";
			AssertEquals("HasWHSTransaction", true, declaration.HasWHSTransaction);
			AssertNoError(declaration.WarehouseDocAddress.OrganisationPKInfo, errorMessage);

			declaration.WarehouseDocAddress.OrganisationPK = org2.PK;
			AssertNoError(declaration.WarehouseDocAddress.OrganisationPKInfo, errorMessage);

			declaration.WarehouseDocAddress.OrganisationPK = org1.PK;
			AssertNoError(declaration.WarehouseDocAddress.OrganisationPKInfo, errorMessage);
			Factory.Save();
			AssertNoError(declaration.WarehouseDocAddress.OrganisationPKInfo, errorMessage);

			declaration.WarehouseDocAddress.OrganisationPK = org2.PK;
			AssertHasError(declaration.WarehouseDocAddress.OrganisationPKInfo, errorMessage);

			declaration.WarehouseDocAddress.OrganisationPK = org1.PK;
			AssertNoError(declaration.WarehouseDocAddress.OrganisationPKInfo, errorMessage);
		}

		public void TestCannotChangeWarehouseAddressPKWhenThereIsWHSTransaction()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "O234";
			org1.MainAddress.OA_Address1 = "1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "O235";
			org2.MainAddress.OA_Address1 = "1";
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
			var declaration = declarationMock.Object;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.WarehouseDocAddress.E2_OA_Address = org1.MainAddress.PK;
			_ = declaration.CustomsEntryHeaders.AddNew();
			declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
			var errorMessage = "There is an Inventory transaction created against this job.\r\nPlease cancel it before changing this value.";
			AssertEquals("HasWHSTransaction", true, declaration.HasWHSTransaction);
			AssertNoError(declaration.WarehouseDocAddress.E2_OA_AddressInfo, errorMessage);

			declaration.WarehouseDocAddress.E2_OA_Address = org2.MainAddress.PK;
			AssertNoError(declaration.WarehouseDocAddress.E2_OA_AddressInfo, errorMessage);

			declaration.WarehouseDocAddress.E2_OA_Address = org1.MainAddress.PK;
			AssertNoError(declaration.WarehouseDocAddress.E2_OA_AddressInfo, errorMessage);
			Factory.Save();
			AssertNoError(declaration.WarehouseDocAddress.E2_OA_AddressInfo, errorMessage);

			declaration.WarehouseDocAddress.E2_OA_Address = org2.MainAddress.PK;
			AssertHasError(declaration.WarehouseDocAddress.E2_OA_AddressInfo, errorMessage);

			declaration.WarehouseDocAddress.E2_OA_Address = org1.MainAddress.PK;
			AssertNoError(declaration.WarehouseDocAddress.E2_OA_AddressInfo, errorMessage);
		}

		public void TestHasWHSTransaction()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
			var declaration = declarationMock.Object;
			declaration.WarehouseTransactionStatus = ZString.Empty;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(false, declaration.HasWHSTransaction);
			foreach (var code in new[]
			{
				WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal,
				WarehouseTransactionStatusList.Codes.InwardCreated,
				WarehouseTransactionStatusList.Codes.InwardCreatedPending,
				WarehouseTransactionStatusList.Codes.InwardUpdated,
				WarehouseTransactionStatusList.Codes.InwardUpdatedPending,
				WarehouseTransactionStatusList.Codes.OutwardCreated,
				WarehouseTransactionStatusList.Codes.OutwardCreatedPending,
				WarehouseTransactionStatusList.Codes.OutwardUpdated,
				WarehouseTransactionStatusList.Codes.OutwardUpdatedPending,
				WarehouseTransactionStatusList.Codes.OutwardCanceledPendingWithdrawal,
				WarehouseTransactionStatusList.Codes.OutwardHolding
			})
			{
				declaration.WarehouseTransactionStatus = code;
				AssertEquals(code, true, declaration.HasWHSTransaction);
			}

			foreach (var code in new[] { WarehouseTransactionStatusList.Codes.InwardCanceled, WarehouseTransactionStatusList.Codes.OutwardCanceled })
			{
				declaration.WarehouseTransactionStatus = code;
				AssertEquals(code, false, declaration.HasWHSTransaction);
			}
		}

		public void TestIsWHSUniversalXMLActive()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				DataRegistry.Business.CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_SystemCreateTimeUtc = ZDateTime.Today;
				AssertEquals(true, declaration.IsWHSUniversalXMLActive);
				declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddYears(-1);
				AssertEquals(false, declaration.IsWHSUniversalXMLActive);
			}
		}

		[TestDate(2021, 4, 9)]
		public void TestIsIntoTemporaryImportEnabled()
		{
			CombineAssertions(() =>
			{
				var dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Import);
				dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryImports = true;
				AssertEquals("not enabled", false, dec.IsIntoTemporaryImportEnabled);

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.TMPIMP, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, true))
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Import);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryImports = true;
					AssertEquals("IMP and OB_CusInventoryForTemporaryImports", true, dec.IsIntoTemporaryImportEnabled);

					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Import);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryImports = false;
					AssertEquals("IMP and not OB_CusInventoryForTemporaryImports", false, dec.IsIntoTemporaryImportEnabled);

					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Export);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryImports = true;
					AssertEquals("not IMP and OB_CusInventoryForTemporaryImports", false, dec.IsIntoTemporaryImportEnabled);
				}
			});
		}

		[TestDate(2021, 4, 9)]
		public void TestIsOutOfTemporaryImportEnabled()
		{
			CombineAssertions(() =>
			{
				var dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Export);
				dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryImports = true;
				AssertEquals("not enabled", false, dec.IsOutOfTemporaryImportEnabled);

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.TMPIMP, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, true))
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Export);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryImports = true;
					AssertEquals("EXP and OB_CusInventoryForTemporaryImports", true, dec.IsOutOfTemporaryImportEnabled);

					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Export);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryImports = false;
					AssertEquals("EXP and not OB_CusInventoryForTemporaryImports", false, dec.IsOutOfTemporaryImportEnabled);

					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Import);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryImports = true;
					AssertEquals("not EXP and OB_CusInventoryForTemporaryImports", false, dec.IsOutOfTemporaryImportEnabled);
				}
			});
		}

		[TestDate(2021, 4, 9)]
		public void TestIsIntoTemporaryExportEnabled()
		{
			CombineAssertions(() =>
			{
				var dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Import);
				dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryExports = true;
				AssertEquals("not enabled", false, dec.IsIntoTemporaryExportEnabled);

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.TMPEXP, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, true))
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Import);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryExports = true;
					AssertEquals("IMP and OB_CusInventoryForTemporaryExports", true, dec.IsIntoTemporaryExportEnabled);

					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Import);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryExports = false;
					AssertEquals("IMP and not OB_CusInventoryForTemporaryExports", false, dec.IsIntoTemporaryExportEnabled);

					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Export);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryExports = true;
					AssertEquals("not IMP and OB_CusInventoryForTemporaryExports", false, dec.IsIntoTemporaryExportEnabled);
				}
			});
		}

		[TestDate(2021, 4, 9)]
		public void TestIsOutOfTemporaryExportEnabled()
		{
			CombineAssertions(() =>
			{
				var dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Export);
				dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryExports = true;
				AssertEquals("not enabled", false, dec.IsOutOfTemporaryExportEnabled);

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.TMPEXP, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, true))
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Export);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryExports = true;
					AssertEquals("EXP and OB_CusInventoryForTemporaryExports", true, dec.IsOutOfTemporaryExportEnabled);

					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Export);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryExports = false;
					AssertEquals("EXP and not OB_CusInventoryForTemporaryExports", false, dec.IsOutOfTemporaryExportEnabled);

					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Import);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryExports = true;
					AssertEquals("not EXP and OB_CusInventoryForTemporaryExports", false, dec.IsOutOfTemporaryExportEnabled);
				}
			});
		}

		[TestDate(2021, 4, 9)]
		public void TestIsIntoInwardProcessingEnabled()
		{
			CombineAssertions(() =>
			{
				var dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Import);
				dec.WarehouseClient.CompanyData.OB_CusInventoryForInwardProcessing = true;
				AssertEquals("not enabled", false, dec.IsIntoInwardProcessingEnabled);

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.IWDPROC, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, true))
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Import);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForInwardProcessing = true;
					AssertEquals("IMP and OB_CusInventoryForInwardProcessing", true, dec.IsIntoInwardProcessingEnabled);

					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Import);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForInwardProcessing = false;
					AssertEquals("IMP and not OB_CusInventoryForInwardProcessing", false, dec.IsIntoInwardProcessingEnabled);

					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Export);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForInwardProcessing = true;
					AssertEquals("not IMP and OB_CusInventoryForInwardProcessing", false, dec.IsIntoInwardProcessingEnabled);
				}
			});
		}

		[TestDate(2021, 4, 9)]
		public void TestIsOutOfInwardProcessingEnabled()
		{
			CombineAssertions(() =>
			{
				var dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Import);
				dec.WarehouseClient.CompanyData.OB_CusInventoryForInwardProcessing = true;
				AssertEquals("not enabled", false, dec.IsOutOfInwardProcessingEnabled);

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.IWDPROC, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, true))
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Import);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForInwardProcessing = true;
					AssertEquals("IMP and OB_CusInventoryForInwardProcessing", true, dec.IsOutOfInwardProcessingEnabled);

					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Export);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForInwardProcessing = true;
					AssertEquals("EXP and OB_CusInventoryForInwardProcessing", true, dec.IsOutOfInwardProcessingEnabled);

					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Export);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForInwardProcessing = false;
					AssertEquals("EXP and not OB_CusInventoryForInwardProcessing", false, dec.IsOutOfInwardProcessingEnabled);

					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.MiscellaneousCustoms);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForInwardProcessing = true;
					AssertEquals("not IMP or EXP and OB_CusInventoryForInwardProcessing", false, dec.IsOutOfInwardProcessingEnabled);
				}
			});
		}

		[TestDate(2021, 4, 9)]
		public void TestIsIntoOutwardProcessingEnabled()
		{
			CombineAssertions(() =>
			{
				var dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Export);
				dec.WarehouseClient.CompanyData.OB_CusInventoryForOutwardProcessing = true;
				AssertEquals("not enabled", false, dec.IsIntoOutwardProcessingEnabled);

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.OWDPROC, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, true))
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Export);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForOutwardProcessing = true;
					AssertEquals("EXP and OB_CusInventoryForOutwardProcessing", false, dec.IsIntoOutwardProcessingEnabled);

					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Export);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForOutwardProcessing = false;
					AssertEquals("EXP and not OB_CusInventoryForOutwardProcessing", false, dec.IsIntoOutwardProcessingEnabled);

					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Import);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForOutwardProcessing = true;
					AssertEquals("not EXP and OB_CusInventoryForOutwardProcessing", false, dec.IsIntoOutwardProcessingEnabled);
				}
			});
		}

		[TestDate(2021, 4, 9)]
		public void TestIsOutOfOutwardProcessingEnabled()
		{
			CombineAssertions(() =>
			{
				var dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Import);
				dec.WarehouseClient.CompanyData.OB_CusInventoryForOutwardProcessing = true;
				AssertEquals("not enabled", false, dec.IsOutOfOutwardProcessingEnabled);

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.OWDPROC, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, true))
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Import);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForOutwardProcessing = true;
					AssertEquals("IMP and OB_CusInventoryForOutwardProcessing", false, dec.IsOutOfOutwardProcessingEnabled);

					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Import);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForOutwardProcessing = false;
					AssertEquals("IMP and not OB_CusInventoryForOutwardProcessing", false, dec.IsOutOfOutwardProcessingEnabled);

					dec = CreateBaseJobDeclarationForTest(JobMessageTypeList.Codes.Export);
					dec.WarehouseClient.CompanyData.OB_CusInventoryForOutwardProcessing = true;
					AssertEquals("not IMP and OB_CusInventoryForOutwardProcessing", false, dec.IsOutOfOutwardProcessingEnabled);
				}
			});
		}

		public void TestIsAmendmentDetectionSuspended()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			AssertEquals(false, dec.IsAmendmentDetectionSuspended);
			using (dec.SuspendAmendmentDetection())
			{
				AssertEquals(true, dec.IsAmendmentDetectionSuspended);
				using (dec.SuspendAmendmentDetection())
				{
					AssertEquals(true, dec.IsAmendmentDetectionSuspended);
				}
				AssertEquals("Should be suspended", true, dec.IsAmendmentDetectionSuspended);
			}
			AssertEquals(false, dec.IsAmendmentDetectionSuspended);
		}

		public void TestHasALineWithExBondAutomation()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.SetUseBondedWarehouseAutomationForTesting(false);
			AssertEquals(false, invoiceLine.UseBondedWarehouseAutomation);
			Factory.InvalidateCachedProperties();
			AssertEquals(false, declaration.HasALineWithExBondAutomation);
			invoiceLine.SetUseBondedWarehouseAutomationForTesting(true);
			AssertEquals(true, invoiceLine.UseBondedWarehouseAutomation);
			Factory.InvalidateCachedProperties();
			AssertEquals(true, declaration.HasALineWithExBondAutomation);
			invoiceLine.Delete();
			AssertEquals(false, declaration.HasALineWithExBondAutomation);
		}

		public void TestIsExBondAutomationEnabled()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				DataRegistry.Business.CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-2);
				declaration.SetSupportsBondedWarehousingForTesting(false);
				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.SetUseBondedWarehouseAutomationForTesting(true);
				AssertEquals(false, declaration.IsExBondAutomationEnabled);
				declaration.SetSupportsBondedWarehousingForTesting(true);
				Factory.InvalidateCachedProperties();
				AssertEquals(true, declaration.IsExBondAutomationEnabled);
				declaration.JE_SystemCreateTimeUtc = ZDateTime.Today;
				Factory.InvalidateCachedProperties();
				AssertEquals(false, declaration.IsExBondAutomationEnabled);
				var org = Factory.New<OrgHeader>();
				declaration.WarehouseDocAddress.E2_OA_Address = org.MainAddress.PK;
				AssertEquals(false, declaration.IsExBondAutomationEnabled);
				var warehouse = Factory.New<IWhsWarehouse>();
				warehouse.WW_OA_WarehouseAddress = org.MainAddress.PK;
				AssertEquals(true, declaration.IsExBondAutomationEnabled);
			}
		}

		public void TestIsMergeDone()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var declaration2 = factory2.Load<BaseJobDeclaration>(declaration.PK);
			Assert("New declaration should not appear merged immediately after save", !declaration2.IsMergeDone);
			declaration2.CustomsEntryHeaders.AddNew();
			Assert("Declaration with entry", declaration2.IsMergeDone);
		}

		public void TestJobDocAddressInitialised()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(false, declaration.IsImporterDeliveryAddressInitialised);
			AssertEquals(false, declaration.IsSupplierPickupAddressInitialised);
			var throwAway = declaration.ImporterDeliveryAddress;
			AssertEquals(true, declaration.IsImporterDeliveryAddressInitialised);
			AssertEquals(false, declaration.IsSupplierPickupAddressInitialised);
			throwAway = declaration.SupplierPickupAddress;
			AssertEquals(true, declaration.IsSupplierPickupAddressInitialised);
		}

		public void TestJE_JSDeletesOldJobDocAddresses()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var declarationDeliveryAddress = declaration.ImporterDeliveryAddress;
			var declarationPickupAddress = declaration.SupplierPickupAddress;
			declarationDeliveryAddress.E2_AddressOverride = true;
			declarationPickupAddress.E2_AddressOverride = true;
			declarationDeliveryAddress.E2_Address1 = "1";
			declarationPickupAddress.E2_Address1 = "2";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipment.ConsignorPickupAddress.E2_AddressOverride = true;
			shipment.ConsigneeDeliveryAddress.E2_Address1 = "3";
			shipment.ConsignorPickupAddress.E2_Address1 = "4";
			AssertEquals(declarationDeliveryAddress.PK, declaration.ImporterDeliveryAddress.PK);
			AssertEquals(declarationPickupAddress.PK, declaration.SupplierPickupAddress.PK);
			declaration.JE_JS = shipment.PK;
			AssertEquals(shipment.ConsigneeDeliveryAddress.PK, declaration.ImporterDeliveryAddress.PK);
			AssertEquals(shipment.ConsignorPickupAddress.PK, declaration.SupplierPickupAddress.PK);
			declaration.ResumeApportionment();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertEquals("Two docs addresses, not four, are saved...", 2, newFactory.GetDatabaseCount(typeof(JobDocAddress), new ZQuery()));
			AssertEquals("... and they all belong to the shipment, not declaration", 2, newFactory.GetDatabaseCount(typeof(JobDocAddress), new ZQuery(JobDocAddressSchema.E2_ParentID, shipment.PK)));
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		public void TestDefaultMessageTypeWhenSupplierAndImporterAreUnderTheSameCountryOfJurisdiction()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
			var declaration = Factory.New<BaseJobDeclaration>();
			var usUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates));
			var prUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.PuertoRico));
			var importer = Factory.New<OrgHeader>();
			importer.OH_RL_NKClosestPort = usUNLOCO.RL_Code;
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_RL_NKClosestPort = prUNLOCO.RL_Code;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
		}

		public void TestDefaultMessageTypeWhenSupplierAndImporterAreUnderDifferentCountry()
		{
			var country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry("CA");
			GlbDepartment.CurrentDepartment.GE_Import = ZBool.True;
			var declaration = Factory.New<BaseJobDeclaration>();
			var usUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates));
			var prUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.PuertoRico));
			var importer = Factory.New<OrgHeader>();
			importer.OH_RL_NKClosestPort = usUNLOCO.RL_Code;
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_RL_NKClosestPort = prUNLOCO.RL_Code;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			GlbDepartment.CurrentDepartment.GE_Import = ZBool.False;
			var importer2 = Factory.New<OrgHeader>();
			importer2.OH_RL_NKClosestPort = usUNLOCO.RL_Code;
			declaration.JE_OH_Importer = importer2.PK;
			AssertEquals(JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			GlbCompany.CurrentCompany.SetCountry(country);
		}

		public void TestDefaultMessageTypeFromOriginOrDestinationWhenDDPIncoTerm()
		{
			var consignor = OrgHeader.New(Factory);
			consignor.OH_RL_NKClosestPort = "USLAX";
			consignor.OH_Code = "TTG1";
			var consignee = OrgHeader.New(Factory);
			consignee.OH_RL_NKClosestPort = "CABLO";
			consignee.OH_Code = "TTG2";
			Factory.Save();

			var country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry("US");
			GlbDepartment.CurrentDepartment.GE_Import = ZBool.True;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_INCO = Core.Constants.IncoTerms.DeliveredDutyPaid;
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKOrigin = "USCHI";
			declaration.ShipmentSynchroniser.Synchronise(true);

			AssertEquals("USCHI", declaration.JE_RL_NKOrigin);
			AssertEquals("AUSYD", declaration.JE_RL_NKFinalDestination);
			AssertEquals(JobMessageTypeList.Codes.Export, declaration.JE_MessageType);

			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_INCO = Core.Constants.IncoTerms.DeliveredDutyPaid;
			shipment.JS_RL_NKOrigin = "AUSYD";

			AssertEquals("AUSYD", declaration.JE_RL_NKOrigin);
			AssertEquals("USCHI", declaration.JE_RL_NKFinalDestination);
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
		}

		public void TestNeedsAdditionalLinkBetweenInvoiceLineAndEntryLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Philippines))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				AssertEquals(false, declaration.NeedsAdditionalLinkBetweenInvoiceLineAndEntryLine);
				foreach (var countryCode in new[] {
					Core.Constants.CountryCodes.UnitedStates,
					Core.Constants.CountryCodes.PuertoRico,
					Core.Constants.CountryCodes.Canada
				})
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
					{
						AssertEquals(countryCode, true, declaration.NeedsAdditionalLinkBetweenInvoiceLineAndEntryLine);
					}
				}
			}
		}

		public void TestDefaultCartageCompany()
		{
			var airCartageLTT = Factory.NewWithValidTestData<OrgHeader>();
			airCartageLTT.OH_IsShippingProvider = true;
			airCartageLTT.OH_IsLocalTransport = true;
			var lclCartageLTT = Factory.NewWithValidTestData<OrgHeader>();
			lclCartageLTT.OH_IsShippingProvider = true;
			lclCartageLTT.OH_IsLocalTransport = true;
			var fclCartageLTT = Factory.NewWithValidTestData<OrgHeader>();
			fclCartageLTT.OH_IsShippingProvider = true;
			fclCartageLTT.OH_IsLocalTransport = true;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var delivAddr1 = importer.Addresses.AddNew();
			delivAddr1.OA_Address1 = "UNIT 7, 229 BOTANY RD.";
			delivAddr1.OA_City = "MASCOT";
			importer.AddRelatedParty(airCartageLTT.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty, GlbCompany.CurrentCompany);
			var relatedParty1 = importer.AllRelatedParties[0];
			relatedParty1.PR_OA = delivAddr1.PK;

			importer.AddRelatedParty(lclCartageLTT.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL, GlbCompany.CurrentCompany);
			importer.AddRelatedParty(fclCartageLTT.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL, GlbCompany.CurrentCompany);

			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("No defaulting of Cartage Company for default export job", ZGuid.Empty, declaration.DocsAndCartage.PickupCartageCoPK);
			AssertEquals("No defaulting of Cartage Company for default export job", ZGuid.Empty, declaration.DocsAndCartage.DeliveryCartageCoPK);

			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("No defaulting when no transport mode as only specific LTT set-up", ZGuid.Empty, declaration.DocsAndCartage.DeliveryCartageCoPK);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("AIR Cartage Company should default", airCartageLTT.PK, declaration.DocsAndCartage.DeliveryCartageCoPK);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("FCL Cartage Company", fclCartageLTT.PK, declaration.DocsAndCartage.DeliveryCartageCoPK);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("LCL Cartage Company", lclCartageLTT.PK, declaration.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestDefaultDeliveryCartageCompanyForSea_Import_BreakBulk()
		{
			var declaration = CreateImportDeclarationWithImporter(Core.Constants.TransportModes.Sea);
			var expectedCartage = declaration.Importer.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.BreakBulk);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;

			AssertEquals("Precondition.", false, declaration.ShouldDefaultFCLCartageCo);
			AssertEquals("Should default the DeliveryCartageCoPK from the related party of importer which container mode is BBK.", expectedCartage.PK, declaration.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestDefaultDeliveryCartageCompanyForSea_Import_FCL()
		{
			var declaration = CreateImportDeclarationWithImporter(Core.Constants.TransportModes.Sea);
			var expectedCartage = declaration.Importer.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;

			AssertEquals("Precondition.", true, declaration.ShouldDefaultFCLCartageCo);
			AssertEquals("Should default the DeliveryCartageCoPK from the related party of importer which container mode is FCL.", expectedCartage.PK, declaration.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestDefaultDeliveryCartageCompanyForSea_Import_FCLMixedShipper()
		{
			var declaration = CreateImportDeclarationWithImporter(Core.Constants.TransportModes.Sea);
			var expectedCartage = declaration.Importer.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCLMixedShipper;

			AssertEquals("We need to default the cartage company from FCL even the container mode is FCX.", true, declaration.ShouldDefaultFCLCartageCo);
			AssertEquals("Should default the DeliveryCartageCoPK from the related party of importer which container mode is FCL.", expectedCartage.PK, declaration.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestDefaultDeliveryCartageCompanyForSea_Import_Container()
		{
			var declaration = CreateImportDeclarationWithImporter(Core.Constants.TransportModes.Sea);
			var expectedCartage = declaration.Importer.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL);

			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;

			AssertEquals("Precondition.", true, declaration.ShouldDefaultFCLCartageCo);
			AssertEquals("Should default the DeliveryCartageCoPK from the related party of importer which container mode is FCL.", expectedCartage.PK, declaration.DocsAndCartage.DeliveryCartageCoPK);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCLMixedShipper;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Groupage;

			AssertEquals("Precondition.", true, declaration.ShouldDefaultFCLCartageCo);
			AssertEquals("Should default the DeliveryCartageCoPK from the related party of importer which container mode is FCL.", expectedCartage.PK, declaration.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestDefaultDeliveryCartageCompanyForAir_Import_BreakBulk()
		{
			var declaration = CreateImportDeclarationWithImporter(Core.Constants.TransportModes.Air);
			var expectedCartage = declaration.Importer.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.BreakBulk);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;

			AssertEquals("Precondition.", false, declaration.ShouldDefaultFCLCartageCo);
			AssertEquals("Should default the DeliveryCartageCoPK from the related party of importer which container mode is BBK.", expectedCartage.PK, declaration.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestDefaultDeliveryCartageCompanyForAir_Import_FCL()
		{
			var declaration = CreateImportDeclarationWithImporter(Core.Constants.TransportModes.Air);
			var expectedCartage = declaration.Importer.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.FCL);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;

			AssertEquals("Precondition.", false, declaration.ShouldDefaultFCLCartageCo);
			AssertEquals("Should default the DeliveryCartageCoPK from the related party of importer which container mode is FCL.", expectedCartage.PK, declaration.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestDefaultDeliveryCartageCompanyForAir_Import_FCLMixedShipper()
		{
			var declaration = CreateImportDeclarationWithImporter(Core.Constants.TransportModes.Air);
			var expectedCartage = declaration.Importer.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.FCLMixedShipper);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCLMixedShipper;

			AssertEquals("Precondition.", false, declaration.ShouldDefaultFCLCartageCo);
			AssertEquals("Should default the DeliveryCartageCoPK from the related party of importer which container mode is FCX.", expectedCartage.PK, declaration.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestDefaultDeliveryCartageCompanyForAir_Import_Container()
		{
			var declaration = CreateImportDeclarationWithImporter(Core.Constants.TransportModes.Air);
			var expectedCartage = declaration.Importer.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.BreakBulk);

			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;

			AssertEquals("Precondition.", true, declaration.ShouldDefaultFCLCartageCo);
			AssertEquals("Should default the DeliveryCartageCoPK from the related party of importer which container mode is BBK as the tranport mode is Air.", expectedCartage.PK, declaration.DocsAndCartage.DeliveryCartageCoPK);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCLMixedShipper;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Groupage;

			expectedCartage = declaration.Importer.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.Groupage);

			AssertEquals("Precondition.", true, declaration.ShouldDefaultFCLCartageCo);
			AssertEquals("Should default the DeliveryCartageCoPK from the related party of importer which container mode is GRP as the tranport mode is Air.", expectedCartage.PK, declaration.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestDefaultDeliveryCartageCompanyForAir_Export()
		{
			var declaration = CreateImportDeclarationWithImporter(Core.Constants.TransportModes.Air);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("Precondition.", true, declaration.IsExport);
			AssertEquals("Should not default the DeliveryCartageCoPK when the declaration is export.", ZGuid.Empty, declaration.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestDefaultDeliveryCartageCompanyForSea_Export()
		{
			var declaration = CreateImportDeclarationWithImporter(Core.Constants.TransportModes.Sea);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("Precondition.", true, declaration.IsExport);
			AssertEquals("Should not default the DeliveryCartageCoPK when the declaration is export.", ZGuid.Empty, declaration.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestCartageCompanyIsDefaultCorrectlyForImportAndThereIsNoErrorIfAlreadySaved()
		{
			var lclCartageLTT = Factory.NewWithValidTestData<OrgHeader>();
			lclCartageLTT.OH_IsShippingProvider = false;
			lclCartageLTT.OH_IsLocalTransport = false;
			var fclCartageLTT = Factory.NewWithValidTestData<OrgHeader>();
			fclCartageLTT.OH_IsShippingProvider = true;
			fclCartageLTT.OH_IsLocalTransport = true;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.AddRelatedParty(lclCartageLTT.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL, GlbCompany.CurrentCompany);
			importer.AddRelatedParty(fclCartageLTT.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL, GlbCompany.CurrentCompany);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(fclCartageLTT.PK, declaration.DocsAndCartage.DeliveryCartageCoPK);
			AssertNoErrors(declaration.DocsAndCartage.DeliveryCartageCoPKInfo);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(ZGuid.Empty, declaration.DocsAndCartage.DeliveryCartageCoPK);
			AssertNoErrors(declaration.DocsAndCartage.DeliveryCartageCoPKInfo);

			((IBuyerSupplierRelationshipConsumer)declaration).DeliveryCartageCoPK = lclCartageLTT.PK;
			AssertEquals(ZGuid.Empty, declaration.DocsAndCartage.DeliveryCartageCoPK);
			AssertNoErrors(declaration.DocsAndCartage.DeliveryCartageCoPKInfo);

			declaration.DocsAndCartage.DeliveryCartageCoPK = lclCartageLTT.PK;
			AssertHasErrors(declaration.DocsAndCartage.DeliveryCartageCoPKInfo);

			declaration.DocsAndCartage.Validation.ValidateAll();
			AssertHasErrors(declaration.DocsAndCartage.DeliveryCartageCoPKInfo);
			AssertNoWarnings(declaration.DocsAndCartage.DeliveryCartageCoPKInfo);

			Factory.Save();
			declaration.DocsAndCartage.Validation.ValidateAll();
			AssertNoErrors(declaration.DocsAndCartage.DeliveryCartageCoPKInfo);
			AssertHasWarnings(declaration.DocsAndCartage.DeliveryCartageCoPKInfo);
		}

		public void TestDefaultCartageCompany_HasShipment()
		{
			var fclCartageLTT = Factory.NewWithValidTestData<OrgHeader>();
			fclCartageLTT.OH_IsShippingProvider = true;
			fclCartageLTT.OH_IsLocalTransport = true;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.AddRelatedParty(fclCartageLTT.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL, GlbCompany.CurrentCompany);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			AssertEquals("Should not default the DeliveryCartageCoPK when the declaration is attached to shipment", ZGuid.Empty, declaration.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestDefaultPickupCartageCompanyForSea_Export_BreakBulk()
		{
			var declaration = CreateExportDeclarationWithSupplier(Core.Constants.TransportModes.Sea);
			var expectedCartage = declaration.Supplier.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.BreakBulk);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;

			AssertEquals("Precondition.", false, declaration.ShouldDefaultFCLCartageCo);
			AssertEquals("Should default the PickupCartageCoPK from the related party of supplier which container mode is BBK.", expectedCartage.PK, declaration.DocsAndCartage.PickupCartageCoPK);
		}

		public void TestDefaultPickupCartageCompanyForSea_Export_FCL()
		{
			var declaration = CreateExportDeclarationWithSupplier(Core.Constants.TransportModes.Sea);
			var expectedCartage = declaration.Supplier.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Precondition.", false, declaration.ShouldDefaultFCLCartageCo);
			AssertEquals("Should default the PickupCartageCoPK from the related party of supplier which container mode is FCL.", expectedCartage.PK, declaration.DocsAndCartage.PickupCartageCoPK);
		}

		public void TestDefaultPickupCartageCompanyForSea_Export_FCLMixedShipper()
		{
			var declaration = CreateExportDeclarationWithSupplier(Core.Constants.TransportModes.Sea);
			var expectedCartage = declaration.Supplier.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCLMixedShipper);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCLMixedShipper;
			AssertEquals("Precondition.", false, declaration.ShouldDefaultFCLCartageCo);
			AssertEquals("Should default the PickupCartageCoPK from the related party of supplier which container mode is FCX.", expectedCartage.PK, declaration.DocsAndCartage.PickupCartageCoPK);
		}

		public void TestDefaultPickupCartageCompanyForSea_Export_Containers()
		{
			var declaration = CreateExportDeclarationWithSupplier(Core.Constants.TransportModes.Sea);

			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("Precondition.", true, declaration.ShouldDefaultFCLCartageCo);

			var expectedCartage = declaration.Supplier.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL);
			AssertEquals("Should default the PickupCartageCoPK from the related party of supplier which container mode is FCL as the container is marked as FCL.", expectedCartage.PK, declaration.DocsAndCartage.PickupCartageCoPK);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCLMixedShipper;

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Groupage;
			AssertEquals("Precondition.", false, declaration.ShouldDefaultFCLCartageCo);

			expectedCartage = declaration.Supplier.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.Groupage);
			AssertEquals("Should default the PickupCartageCoPK from the related party of supplier which container mode is GRP.", expectedCartage.PK, declaration.DocsAndCartage.PickupCartageCoPK);
		}

		public void TestDefaultPickupCartageCompanyForAir_Export_BreakBulk()
		{
			var declaration = CreateExportDeclarationWithSupplier(Core.Constants.TransportModes.Air);
			var expectedCartage = declaration.Supplier.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.BreakBulk);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;

			AssertEquals("Precondition.", false, declaration.ShouldDefaultFCLCartageCo);
			AssertEquals("Should default the PickupCartageCoPK from the related party of supplier which container mode is BBK.", expectedCartage.PK, declaration.DocsAndCartage.PickupCartageCoPK);

			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			declaration.JE_ContainerMode = string.Empty;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;

			AssertEquals("Precondition.", true, declaration.ShouldDefaultFCLCartageCo);
			AssertEquals("Should default the PickupCartageCoPK from the related party of supplier which container mode is BBK as the tranport mode is Air.", expectedCartage.PK, declaration.DocsAndCartage.PickupCartageCoPK);
		}

		public void TestDefaultPickupCartageCompanyForAir_Export_FCL()
		{
			var declaration = CreateExportDeclarationWithSupplier(Core.Constants.TransportModes.Air);
			var expectedCartage = declaration.Supplier.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.FCL);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Precondition.", false, declaration.ShouldDefaultFCLCartageCo);
			AssertEquals("Should default the PickupCartageCoPK from the related party of supplier which container mode is FCL.", expectedCartage.PK, declaration.DocsAndCartage.PickupCartageCoPK);
		}

		public void TestDefaultPickupCartageCompanyForAir_Export_FCLMixedShipper()
		{
			var declaration = CreateExportDeclarationWithSupplier(Core.Constants.TransportModes.Air);
			var expectedCartage = declaration.Supplier.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.FCLMixedShipper);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCLMixedShipper;
			AssertEquals("Precondition.", false, declaration.ShouldDefaultFCLCartageCo);
			AssertEquals("Should default the PickupCartageCoPK from the related party of supplier which container mode is FCX as the tranport mode is Air.", expectedCartage.PK, declaration.DocsAndCartage.PickupCartageCoPK);
		}

		public void TestDefaultPickupCartageCompanyForAir_Import()
		{
			var declaration = CreateExportDeclarationWithSupplier(Core.Constants.TransportModes.Air);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("Precondition.", true, declaration.IsImport);
			AssertEquals("Should not default the PickupCartageCoPK when the declaration is import.", ZGuid.Empty, declaration.DocsAndCartage.PickupCartageCoPK);
		}

		public void TestDefaultPickupCartageCompanyForSea_Import()
		{
			var declaration = CreateExportDeclarationWithSupplier(Core.Constants.TransportModes.Sea);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("Precondition.", true, declaration.IsImport);
			AssertEquals("Should not default the PickupCartageCoPK when the declaration is import.", ZGuid.Empty, declaration.DocsAndCartage.PickupCartageCoPK);
		}

		public void TestCartageCompanyIsDefaultCorrectlyForExportAndThereIsNoErrorIfAlreadySaved()
		{
			var lclCartageLTT = Factory.NewWithValidTestData<OrgHeader>();
			lclCartageLTT.OH_IsShippingProvider = false;
			lclCartageLTT.OH_IsLocalTransport = false;
			var fclCartageLTT = Factory.NewWithValidTestData<OrgHeader>();
			fclCartageLTT.OH_IsShippingProvider = true;
			fclCartageLTT.OH_IsLocalTransport = true;

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.AddRelatedParty(lclCartageLTT.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL, GlbCompany.CurrentCompany);
			supplier.AddRelatedParty(fclCartageLTT.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL, GlbCompany.CurrentCompany);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(fclCartageLTT.PK, declaration.DocsAndCartage.PickupCartageCoPK);
			AssertNoErrors(declaration.DocsAndCartage.PickupCartageCoPKInfo);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(ZGuid.Empty, declaration.DocsAndCartage.PickupCartageCoPK);
			AssertNoErrors(declaration.DocsAndCartage.PickupCartageCoPKInfo);

			((IBuyerSupplierRelationshipConsumer)declaration).PickupCartageCoPK = lclCartageLTT.PK;
			AssertEquals(ZGuid.Empty, declaration.DocsAndCartage.PickupCartageCoPK);
			AssertNoErrors(declaration.DocsAndCartage.PickupCartageCoPKInfo);

			declaration.DocsAndCartage.PickupCartageCoPK = lclCartageLTT.PK;
			AssertHasErrors(declaration.DocsAndCartage.PickupCartageCoPKInfo);

			declaration.DocsAndCartage.Validation.ValidateAll();
			AssertHasErrors(declaration.DocsAndCartage.PickupCartageCoPKInfo);
			AssertNoWarnings(declaration.DocsAndCartage.PickupCartageCoPKInfo);

			Factory.Save();
			declaration.DocsAndCartage.Validation.ValidateAll();
			AssertNoErrors(declaration.DocsAndCartage.PickupCartageCoPKInfo);
			AssertHasWarnings(declaration.DocsAndCartage.PickupCartageCoPKInfo);
		}

		public void TestSetCorrectMessageTypeForUSSupplierImporterWithPRCompany()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.PuertoRico);

			var declaration = Factory.New<BaseJobDeclaration>();

			var usOrg = Factory.New<OrgHeader>();
			usOrg.OH_RL_NKClosestPort = "USXXX";
			var prOrg = Factory.New<OrgHeader>();
			prOrg.OH_RL_NKClosestPort = "PRPNU";

			declaration.JE_OH_Supplier = usOrg.PK;
			AssertEquals("Set default to message type to export", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);

			declaration.JE_MessageType = ZString.Empty;
			declaration.JE_OH_Supplier = prOrg.PK;
			AssertEquals("Set default to message type to export", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);

			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Importer = usOrg.PK;
			AssertEquals("Set default to message type to import", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);

			declaration.JE_MessageType = ZString.Empty;
			declaration.JE_OH_Importer = prOrg.PK;
			AssertEquals("Set default to message type to import", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
		}

		public void TestDeclaration_OnSaving_LogEventIfDeclarationHasMessageErrors_StandAloneAndTrue() => TestDeclaration_OnSaving_LogEventIfDeclarationHasMessageErrors(true, true);

		public void TestDeclaration_OnSaving_LogEventIfDeclarationHasMessageErrors_StandAloneAndFalse() => TestDeclaration_OnSaving_LogEventIfDeclarationHasMessageErrors(true, false);

		public void TestDeclaration_OnSaving_LogEventIfDeclarationHasMessageErrors_ShipmentAndTrue() => TestDeclaration_OnSaving_LogEventIfDeclarationHasMessageErrors(false, true);

		public void TestDeclaration_OnSaving_LogEventIfDeclarationHasMessageErrors_ShipmentAndFalse() => TestDeclaration_OnSaving_LogEventIfDeclarationHasMessageErrors(false, false);

		public void TestDeclaration_OnSaving_LogEventIfDeclarationHasMessageErrors(bool isStandAloneDeclaration, bool registryValue)
		{
			using (CustomsDataRegistry.Instance.RaiseEventDCEWhenSavingErrorMSG.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				Factory.Save();

				var decEvent1 = declaration.Logs.Find(l => l.SL_SE_NKEvent == Events.DeclarationHasErrorsCode).FirstOrDefault();
				AssertNull("Declaration Log 1", decEvent1);

				var shippingLine = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_MasterBill = "12345678";
				declaration.JE_OH_ShippingLine = shippingLine.PK;

				if (!isStandAloneDeclaration)
				{
					var consol = Factory.New<ForwardingConsol>();
					var forwardingContainer = consol.Containers.AddNew();
					forwardingContainer.JC_ContainerNum = "ContainerNum111";
					var shipment = consol.Shipments.AddNew();
					declaration.JE_JS = shipment.PK;
				}
				Factory.Save();

				var decEvent2 = declaration.Logs.Find(l => l.SL_SE_NKEvent == Events.DeclarationHasErrorsCode).FirstOrDefault();
				if (registryValue)
				{
					AssertNotNull("Declaration Log 2", decEvent2);
				}
				else
				{
					AssertNull("Declaration Log 2", decEvent2);
				}
			}
		}

		public void TestIMessageSenderSupporterMembers()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			dec.HasChanges = true;
			IMessageSenderSupporter supporter = dec;
			AssertEquals("HasChanges", true, supporter.HasChanges);
			AssertEquals("HasErrors", false, supporter.HasErrors);
			AssertEquals("MessageInitiator", dec.MessageInitiator, supporter.MessageInitiator);
			AssertEquals("Factory", Factory, supporter.Factory);
			AssertEquals("BusinessObjectForNotifications", dec, supporter.BusinessObjectForNotifications);

			dec.JE_GB = ZGuid.Empty;
			dec.HasChanges = false;
			AssertEquals("HasChanges", false, supporter.HasChanges);
			AssertEquals("HasErrors", true, supporter.HasErrors);
			AssertEquals("MessageInitiator", dec.MessageInitiator, supporter.MessageInitiator);
			AssertEquals("Factory", Factory, supporter.Factory);
			AssertEquals("BusinessObjectForNotifications", dec, supporter.BusinessObjectForNotifications);

			dec.JE_GB = GlbBranch.CurrentBranch.PK;
			dec.HasChanges = false;
			AssertEquals("HasChanges", false, supporter.HasChanges);
			AssertEquals("HasErrors", false, supporter.HasErrors);
			AssertEquals("MessageInitiator", dec.MessageInitiator, supporter.MessageInitiator);
			AssertEquals("Factory", Factory, supporter.Factory);
			AssertEquals("BusinessObjectForNotifications", dec, supporter.BusinessObjectForNotifications);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ReleaseType = "!";
			AssertEquals(true, shipment.HasErrors);
			dec.JE_JS = shipment.PK;
			AssertEquals("HasChanges", true, supporter.HasChanges);
			AssertEquals("HasErrors", true, supporter.HasErrors);
			AssertEquals("MessageInitiator", dec.MessageInitiator, supporter.MessageInitiator);
			AssertEquals("Factory", Factory, supporter.Factory);
			AssertEquals("BusinessObjectForNotifications", dec, supporter.BusinessObjectForNotifications);

			shipment.JS_ReleaseType = ZString.Empty;
			shipment.HasChanges = false;
			AssertEquals("HasChanges", false, supporter.HasChanges);
			AssertEquals("HasErrors", false, supporter.HasErrors);
			AssertEquals("MessageInitiator", dec.MessageInitiator, supporter.MessageInitiator);
			AssertEquals("Factory", Factory, supporter.Factory);
			AssertEquals("BusinessObjectForNotifications", dec, supporter.BusinessObjectForNotifications);
		}

		public void TestTransportModeIsNotDefaultedWhenJobIsNonTransportMode()
		{
			var declaration = Factory.New<DummyBaseJobDeclaration_TestTransportModeIsNotDefaultedWhenJobIsNonTransportMode>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "Supplier~";
			supplier.OH_RL_NKClosestPort = "AUSYD";

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer~";
			importer.OH_RL_NKClosestPort = "USLAX";

			var link = importer.SupplierLinks.AddNew(supplier);
			var linkTrnMode = link.OrgSupBuyLinkTrnModes[0];
			linkTrnMode.PF_OL = link.PK;
			linkTrnMode.PF_TransportMode = Core.Constants.TransportModes.Sea;

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;

			AssertEquals("JE_TransportMode should be blank when job is non transport mode regardless of importer and supplier relationship", ZString.Empty, declaration.JE_TransportMode);
		}

		public void TestCanClearDefaultData()
		{
			// Restoration suspender from buyer and supllier relationship

			var declaration = Factory.New<BaseJobDeclarationForTesting>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "Supplier~";
			supplier.OH_RL_NKClosestPort = "AUSYD";

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer~";
			importer.OH_RL_NKClosestPort = "USLAX";

			var link = importer.SupplierLinks.AddNew(supplier);
			var linkTrnMode = link.OrgSupBuyLinkTrnModes[0];
			linkTrnMode.PF_OL = link.PK;
			linkTrnMode.PF_TransportMode = Core.Constants.TransportModes.Sea;
			linkTrnMode.PF_ContainerMode = Core.Constants.ContainerModes.FCL;

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;

			AssertEquals("Precondition-Transport Mode is defaulted from importer and supplier relationship as epxected", Core.Constants.TransportModes.Sea, declaration.JE_TransportMode);
			declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("Transport Mode is blank as intentionally and left as it is regardless of importer and supplier realationship", ZString.Empty, declaration.JE_TransportMode);
			AssertEquals("Precondition-Container Mode is defaulted from importer and supplier relationship", Core.Constants.ContainerModes.FCL, declaration.JE_ContainerMode);
			declaration.JE_ContainerMode = ZString.Empty;
			AssertEquals("Transport Mode is still blank", ZString.Empty, declaration.JE_TransportMode);
		}

		public void TestShippingLinePKIsNotSavedToANewRelationship()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();

			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_OH_Supplier = supplier.PK;
			dec.JE_OH_ShippingLine = shippingLine.PK;
			Factory.Save();

			dec.BuyerSupplierLinksHelper.AddNewBuyerSupplierLink();

			AssertEquals(ZGuid.Empty, dec.SupplierImporterLink.OrgSupBuyLinkTrnModes[0].PF_OH_CarrierLine);
		}

		public void TestIControllerIDProviderMembers()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			IControllerIDProvider provider = dec;
			AssertEquals("ControllerID", ControllerIDs.Customs.JobDeclaration, provider.ControllerID);
			AssertEquals("BusinessObjectPK", dec.PK.ToGuid(), provider.BusinessObjectPK);

			var shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			AssertEquals("ControllerID", ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, provider.ControllerID);
			AssertEquals("BusinessObjectPK", dec.PK.ToGuid(), provider.BusinessObjectPK);
		}

		public void TestParentReleatedDeclaration()
		{
			var sourceDeclaration = Factory.New<BaseJobDeclaration>();
			AssertNull(sourceDeclaration.ParentRelatedDeclaration);
			var related = sourceDeclaration.RelatedDeclarations.AddNew();
			Factory.Save();
			AssertEquals(sourceDeclaration, related.ParentRelatedDeclaration);
			var newFactory = new BusinessObjectFactory();
			var sourceDeclarationInNewFactory = newFactory.Load<BaseJobDeclaration>(sourceDeclaration.PK);
			var relatedInNewFactory = newFactory.Load<BaseJobDeclaration>(related.PK);
			AssertEquals(sourceDeclarationInNewFactory, relatedInNewFactory.ParentRelatedDeclaration);
		}

		public void TestParentRelatedDeclaration_IncludeRelationType()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Factory.ResetDatabaseLoadCount();
			using (Factory.EnableTableHitQueryCollection(new[] { JobDeclarationSchema.Constants.TableName }))
			{
				_ = declaration.ParentRelatedDeclaration;
			}
			var tableSelect = Factory.TableSelects.Single(x => x.TableName.Equals(JobDeclarationSchema.Constants.TableName));
			var tableSelectQuery = tableSelect.Queries.Single();
			AssertContains(nameof(tableSelectQuery.Query), $"{GenPivotSchema.Constants.XX_RelationType} = ''", tableSelectQuery.Query);
		}

		public void TestGetNewRelatedDeclaration_InvalidRelationshipType()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.GetNewRelatedDeclaration(Factory, "ABC");
			AssertEquals("Invalid relationship type: ABC", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			AssertNoExceptionThrown(() => declaration.GetNewRelatedDeclaration(Factory));
			AssertNoExceptionThrown(() => declaration.GetNewRelatedDeclaration(Factory, ""));
		}

		public void TestSupplierBuyerLinkDefaultRefreshingWhenDestinationChanges()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "Supplier~";
			supplier.OH_RL_NKClosestPort = "AUSYD";

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer~";
			importer.OH_RL_NKClosestPort = "USLAX";

			var link = importer.SupplierLinks.AddNew(supplier);
			link.OL_RN_NKImporterCountry = "US";
			var linkTrnMode = link.OrgSupBuyLinkTrnModes[0];
			linkTrnMode.PF_OL = link.PK;
			linkTrnMode.PF_TransportMode = Core.Constants.TransportModes.Sea;
			linkTrnMode.PF_RL_NKDischargePort = "USCHI";

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;

			AssertEquals("USCHI", declaration.JE_RL_NKPortOfArrival);

			declaration.JE_RL_NKPortOfArrival = "USLAX";
			declaration.JE_RL_NKFinalDestination = "USNYC";
			AssertEquals("should remain as overriden", "USLAX", declaration.JE_RL_NKPortOfArrival);
		}

		public void TestDefaultImporterAddressIsBasedOnDestination()
		{
			var header = Factory.New<OrgHeader>();
			var mainAddress = header.MainAddress;
			mainAddress.OA_RL_NKRelatedPortCode = "USLAX";

			var address1 = header.Addresses.AddNew();
			address1.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			address1.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office);
			address1.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery);
			address1.OA_RL_NKRelatedPortCode = "AUSYD";

			var address2 = header.Addresses.AddNew();
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office);
			address2.OA_RL_NKRelatedPortCode = "AUMEL";

			var declaration = BaseJobDeclaration.New(Factory);
			declaration.ImporterDeliveryAddress.OrganisationPK = header.PK;
			declaration.ImporterDocumentaryAddress.OrganisationPK = header.PK;

			declaration.JE_RL_NKFinalDestination = "AUMEL";
			AssertEquals(declaration.ImporterDeliveryAddress.E2_OA_Address, address2.PK);
			AssertEquals(declaration.ImporterDocumentaryAddress.E2_OA_Address, address2.PK);

			declaration.JE_RL_NKFinalDestination = "AUSYD";
			AssertEquals(declaration.ImporterDeliveryAddress.E2_OA_Address, address1.PK);
			AssertEquals(declaration.ImporterDocumentaryAddress.E2_OA_Address, address1.PK);
		}

		public void TestShouldNotUpdateImporterDocumentaryAddressAfterJE_RL_NKFinalDestinationChangesDuringShipmentSynchronization()
		{
			var consignee = OrgHeader.New(Factory);
			consignee.OH_Code = "VWG";
			var address1 = consignee.Addresses.AddNew();
			address1.Address1 = "Address 1";
			address1.OA_RL_NKRelatedPortCode = "AUBNE";
			address1.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office);
			var address2 = consignee.Addresses.AddNew();
			address2.OA_Address1 = "Address 2";
			address2.OA_RL_NKRelatedPortCode = "AUSYD";
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office);
			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = address2.PK;
			using (shipment.SuspendSettingConsigneeFromDestination())
			{
				shipment.JS_RL_NKDestination = "AUBNE";
			}

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);
			CombineAssertions(() =>
			{
				AssertNotEquals("AUBNE", address1.PK, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
				AssertEquals("AUSYD", address2.PK, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
			});
		}

		public void TestShouldNotUpdateImporterDeliveryAddressAfterJE_RL_NKFinalDestinationChangesDuringShipmentSynchronization()
		{
			var consignee = OrgHeader.New(Factory);
			consignee.OH_Code = "VWG";
			var address1 = consignee.Addresses.AddNew();
			address1.Address1 = "Address 1";
			address1.OA_RL_NKRelatedPortCode = "AUBNE";
			address1.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			var address2 = consignee.Addresses.AddNew();
			address2.OA_Address1 = "Address 2";
			address2.OA_RL_NKRelatedPortCode = "AUSYD";
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = address2.PK;

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);
			CombineAssertions(() =>
			{
				AssertNotEquals("AUBNE", address1.PK, shipment.ConsigneeDeliveryAddress.E2_OA_Address);
				AssertEquals("AUSYD", address2.PK, shipment.ConsigneeDeliveryAddress.E2_OA_Address);
			});
		}

		public void TestDestinationDefaultFromBuyerSupplierCanBeOverriden()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "Supplier~";
			supplier.OH_RL_NKClosestPort = "AUSYD";

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer~";
			importer.OH_RL_NKClosestPort = "USLAX";

			var link = importer.SupplierLinks.AddNew(supplier);
			var linkTrnMode = link.OrgSupBuyLinkTrnModes[0];
			linkTrnMode.PF_OL = link.PK;
			linkTrnMode.PF_TransportMode = Core.Constants.TransportModes.Sea;
			linkTrnMode.PF_RL_NKPlaceOfDeliveryPort = "NZWLG";

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;

			AssertEquals("NZWLG", declaration.JE_RL_NKFinalDestination);
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			Factory.Save();
			AssertEquals("Should not change", "AUSYD", declaration.JE_RL_NKFinalDestination);
		}

		public void TestMasterBillHouseBillReadOnly()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Assert(!declaration.JE_MasterBillInfo.ReadOnly);
			Assert(!declaration.JE_HouseBillInfo.ReadOnly);

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			Assert(declaration.JE_MasterBillInfo.ReadOnly);
			Assert(declaration.JE_HouseBillInfo.ReadOnly);

			declaration.JE_OverrideFreightDefaults = true;
			Assert(!declaration.JE_MasterBillInfo.ReadOnly);
			Assert(!declaration.JE_HouseBillInfo.ReadOnly);
		}

		public void TestPortDefaultingFromBuyerConsignorRelationship()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OH_Code = "AUSBUYER";
			buyer.OH_RL_NKClosestPort = "AUSYD";
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "NZSUPPLIER";
			supplier.OH_RL_NKClosestPort = "NZAKL";

			BaseJobDeclaration dec1 = Factory.New<BaseJobDeclaration>();
			dec1.JE_OH_Importer = buyer.PK;
			dec1.JE_OH_Supplier = supplier.PK;

			AssertEquals("No Supplier/Buyer Link: Load should default to supplier's UNLOCO", "NZAKL", dec1.JE_RL_NKPortOfLoading);
			AssertEquals("No Supplier/Buyer Link: Discharge should default to buyer's UNLOCO", "AUSYD", dec1.JE_RL_NKPortOfArrival);

			OrgSupplierBuyerLink link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = buyer.PK;
			link.OL_OH_Supplier = supplier.PK;
			OrgSupBuyLinkTrnMode linkTrnMode = link.OrgSupBuyLinkTrnModes[0];
			linkTrnMode.PF_OL = link.PK;
			linkTrnMode.PF_TransportMode = Core.Constants.TransportModes.Sea;
			linkTrnMode.PF_ContainerMode = Core.Constants.ContainerModes.FCL;
			linkTrnMode.PF_RL_NKLoadPort = "NZWLG";
			linkTrnMode.PF_RL_NKDischargePort = "AUBNE";

			Factory.Save();

			BaseJobDeclaration dec2 = Factory.New<BaseJobDeclaration>();
			dec2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec2.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			dec2.JE_OH_Importer = buyer.PK;
			dec2.JE_OH_Supplier = supplier.PK;

			AssertEquals("Load should default from SupplierBuyerLink", "NZWLG", dec2.JE_RL_NKPortOfLoading);
			AssertEquals("Discharge should default from SupplierBuyerLink", "AUBNE", dec2.JE_RL_NKPortOfArrival);
		}

		public void TestIWorkflowTriggerEventSource_Members()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			IWorkflowTriggerEventSource source = dec;
			AssertEquals(dec.Company, source.JobHeaderCompany);
			AssertEquals(0, source.ParentWorkflowProviders.Count);

			var shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			var prov = source.ParentWorkflowProviders;
			AssertEquals(1, prov.Count);
			AssertEquals(shipment, prov[0]);
		}

		public void TestIWorkflowTriggerFieldChangeSource_Members()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			var prov = (dec as IWorkflowTriggerFieldChangeSource).ParentWorkflowProviders;
			AssertEquals(1, prov.Count);
			AssertEquals(shipment, prov[0]);
		}

		public void TestGetReasonForNotAbleToUpdate()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			AssertEquals("", dec.GetReasonForNotAbleToUpdate());
			dec.Messages.AddNew();
			dec.JE_DeclarationReference = "TESTDEC";
			AssertEquals(dec.JobNumber, dec.JE_DeclarationReference);
			AssertEquals("Message has been sent for this Declaration. Job Number: TESTDEC", dec.GetReasonForNotAbleToUpdate());
		}

		public void TestIAdditionalReferenceNumberTypeProvider()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var list = declaration.Lookups.MessageTypeList;
			IAdditionalReferenceNumberTypeProvider provider = declaration;
			AssertEquals(list, provider.GetAdditionalReferenceNumberTypeList(ZString.Empty, ZString.Empty));
			AssertEquals(list, provider.GetAdditionalReferenceNumberTypeList(CusEntryNumber.Categories.CustomsPermitClearanceNumber, Core.Constants.CountryCodes.UnitedStates));

			var additionalReferenceNumberTypes = CusEntryNumLookups.GetAdditionalReferenceNumberTypes(Factory, Core.Constants.CountryCodes.UnitedStates, false);
			AssertNotEquals(list, additionalReferenceNumberTypes);
			AssertEquals(additionalReferenceNumberTypes, provider.GetAdditionalReferenceNumberTypeList(CusEntryNumber.Categories.AdditionalReferenceNumber, Core.Constants.CountryCodes.UnitedStates));
		}

		public void TestIStmALogParentProviderMembers()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			IStmALogParentProvider provider = dec;
			AssertEquals(dec, provider.LogParent);
			var shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			AssertEquals(shipment, provider.LogParent);
		}

		[RunInExtraTransaction]
		public void TestIFountainResolver()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			dec.PopulateJE_DeclarationReferenceIfNeeded();
			ZString refNum = dec.JE_DeclarationReference;
			Assert(!refNum.IsEmpty);
			int num1 = Convert.ToInt32(refNum.Right(refNum.Length - 1));

			dec.JE_DeclarationReference = "";
			(dec as IFountainResolver).TryToResolve();
			dec.PopulateJE_DeclarationReferenceIfNeeded();
			ZString refNum2 = dec.JE_DeclarationReference;
			Assert(!refNum2.IsEmpty);
			int num2 = Convert.ToInt32(refNum2.Right(refNum2.Length - 1));
			AssertEquals("Should be increased once for PopulateJE_DeclarationReferenceIfNeeded and once for TryToResolve", num1 + 2, num2);
		}

		public void TestChangingTransportModeFromSeaToRail()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = "BUNGA DELIMA";
			declaration.JE_VoyageFlightNo = "4389";
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_ExportDate = new ZDateTime(2009, 1, 2);
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_DateOfArrival = new ZDateTime(2009, 1, 3);

			declaration.Transports.RemoveAndDeleteAll();

			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Rail;
			AssertEquals(ZString.Empty, declaration.JE_VesselName);
			AssertEquals(ZString.Empty, declaration.JE_VoyageFlightNo);

			AssertEquals("JE_TransportMode should not cause creating routing records. Otherwise Routing system clears out some key fields which causes validation errors", 0, declaration.Transports.Count);
		}

		public void TestDefaultWhenPortofLoadingEntered()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ExportDate = ZDateTime.BrettsBirthday;

			declaration.JE_RL_NKOrigin = "HKHKG";
			AssertEquals("Port of Loading defaulted because it was empty", "HKHKG", declaration.JE_RL_NKPortOfLoading);

			AssertEquals("As two fields have the same ports, dates should be copied", ZDateTime.BrettsBirthday, declaration.JE_DateAtOrigin);

			declaration.JE_DateAtOrigin = ZDateTime.BrettsBirthday.AddDays(1);
			AssertEquals("LoadingDate should not change by DateAtOrigin", ZDateTime.BrettsBirthday, declaration.JE_ExportDate);
			AssertHasWarning(declaration.JE_DateAtOriginInfo, BaseJobDeclarationValidation.OriginDateIsLaterThanLoadingDate);
			declaration.JE_DateAtOrigin = ZDateTime.BrettsBirthday.AddDays(-1);
			AssertEquals("Export Date remains the same", ZDateTime.BrettsBirthday, declaration.JE_ExportDate);

			declaration.JE_RL_NKOrigin = "SGSIN";
			AssertEquals("Port of Loading should not be defaulted from origin now", "HKHKG", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Export date remains the same", ZDateTime.BrettsBirthday, declaration.JE_ExportDate);
		}

		public void TestDefaultDateOfFirstArrival()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_RL_NKPortOfArrival = "AUSYD";

				AssertEquals("Date of first arrival defaulted", "AUSYD", declaration.JE_RL_NKPortOfFirstArrival);

				declaration.JE_DateOfArrival = ZDateTime.BrettsBirthday;
				AssertEquals("Date at first arrival should be defaulted", ZDateTime.BrettsBirthday, declaration.JE_DateOfFirstArrival);

				declaration.JE_RL_NKPortOfFirstArrival = "AUMEL";
				declaration.JE_DateOfFirstArrival = ZDateTime.BrettsBirthday.AddDays(1);
				AssertEquals("Date at arrival should not change, but validate", ZDateTime.BrettsBirthday, declaration.JE_DateOfArrival);

				declaration.JE_DateOfFirstArrival = ZDateTime.BrettsBirthday.AddDays(-1);
				AssertEquals("Date at arrival remains the same", ZDateTime.BrettsBirthday, declaration.JE_DateOfArrival);

				AssertEquals("Date at first arrival should remain", ZDateTime.BrettsBirthday.AddDays(-1), declaration.JE_DateOfFirstArrival);
			}
		}

		public void TestDefaultWhenPortOfDischargeEntered()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var declaration = Factory.New<BaseJobDeclaration>();

				var testDate = new ZDateTime(2012, 05, 25, 16, 0, 0);
				declaration.JE_DateOfArrival = testDate;
				declaration.JE_RL_NKPortOfArrival = "AUSYD";
				AssertEquals("Destintaion defaulted", "AUSYD", declaration.JE_RL_NKFinalDestination);
				AssertEquals("First Arrival defaulted", "AUSYD", declaration.JE_RL_NKPortOfFirstArrival);

				AssertEquals("Dates are defaulted", testDate, declaration.JE_DateAtFinalDestination);
				AssertEquals("Dates are defaulted", testDate, declaration.JE_DateOfFirstArrival);

				declaration.JE_DateAtFinalDestination = testDate.AddDays(-1);
				AssertEquals("JE_DateOfArrival should not change by dateAtDestination, but validate", testDate, declaration.JE_DateOfArrival);
				AssertHasWarning(declaration.JE_DateAtFinalDestinationInfo, BaseJobDeclarationValidation.DestinationDateIsEarlierThanDischargeDate);

				declaration.JE_DateAtFinalDestination = testDate.AddMinutes(-20);
				AssertNoWarning(declaration.JE_DateAtFinalDestinationInfo, BaseJobDeclarationValidation.DestinationDateIsEarlierThanDischargeDate);
				AssertEquals("Dates are defaulted", testDate, declaration.JE_DateOfFirstArrival);
			}
		}

		public void TestDefaultCarrierFromSchedule_BTH()
		{
			AssertDefaultCarrierFromSchedule(RelatedPartyDefaultingTypeList.Codes.Both, true);
		}

		public void TestDefaultCarrierFromSchedule_EXP()
		{
			AssertDefaultCarrierFromSchedule(RelatedPartyDefaultingTypeList.Codes.Export, true);
		}

		public void TestDefaultCarrierFromSchedule_IMP()
		{
			AssertDefaultCarrierFromSchedule(RelatedPartyDefaultingTypeList.Codes.Import, false);
		}

		public void TestDefaultCarrierFromSchedule_DIS()
		{
			AssertDefaultCarrierFromSchedule(RelatedPartyDefaultingTypeList.Codes.Disabled, false);
		}

		public void TestDefaultCarrierFromScheduleForSeaJob()
		{
			var voyage = InitialiseSailings();
			AssertNotNull(voyage.Line);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = "My Vessel";
			declaration.JE_VoyageFlightNo = "V123";

			AssertNotNull("The job has voyage", declaration.Voyage);
			AssertEquals("ShippingLine is not defaulted when transportMode is sea", Guid.Empty, declaration.JE_OH_ShippingLine);
		}

		public void TestIJobInvoicingSupporterDatesImplementation()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DateAtOrigin = new ZDateTime(2009, 1, 1);
			declaration.JE_ExportDate = new ZDateTime(2009, 1, 2);

			declaration.JE_DateOfFirstArrival = new ZDateTime(2009, 1, 14);
			declaration.JE_DateOfArrival = new ZDateTime(2009, 1, 15);
			declaration.JE_DateAtFinalDestination = new ZDateTime(2009, 1, 20);
			declaration.JE_EstimatedDeliveryOrPickup = new ZDateTime(2009, 1, 25);
			declaration.JE_CartageCompleted = new ZDateTime(2009, 1, 30);

			IJobInvoicingSupporter supporter = ((IJobInvoicingPlugIn)declaration).InvoicingSupporter;
			AssertEquals("ATA: Actual Arrival", new ZDateTime(2009, 1, 15), supporter.ATA);
			AssertEquals("ATD: Actual Departure", new ZDateTime(2009, 1, 2), supporter.ATD);
			AssertEquals("ETA: Estimated Arrival", new ZDateTime(2009, 1, 20), supporter.ETA);
			AssertEquals("ETD: Estimated Departure", new ZDateTime(2009, 1, 1), supporter.ETD);
			AssertEquals("ESP", new ZDateTime(2009, 1, 25), supporter.ESP);
			AssertEquals("ESD", new ZDateTime(2009, 1, 25), supporter.ESD);
			AssertEquals("ActualPickupDate", new ZDateTime(2009, 1, 30), supporter.ActualPickupDate);
			AssertEquals("ActualDeliveryDate", new ZDateTime(2009, 1, 30), supporter.ActualDeliveryDate);
		}

		public void TestSynchronisatioFromConsolLevel_CS00122264()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_JS = shipment.PK;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var consol = newFactory.New<ForwardingConsol>();
			var newshipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			consol.Shipments.Add(newshipment);
			TestHelper.MakeConsolRelevantToDeclaration(consol, dec);
			consol.JK_MasterBillNum = "MB1";
			newshipment.JS_HouseBill = "HB1";
			newshipment.JS_ActualWeight = 10m;
			newFactory.Save();
			var declarationForDocuments = (BaseJobDeclaration)newshipment.DeclarationForDocuments;
			AssertEquals(dec.PK, declarationForDocuments.PK);
			AssertEquals(10m, declarationForDocuments.JE_TotalWeight);
			AssertEquals("MB1", declarationForDocuments.JE_MasterBill);
			AssertEquals("HB1", declarationForDocuments.JE_HouseBill);

			consol.JK_MasterBillNum = "MB2";
			newshipment.JS_HouseBill = "HB2";
			newshipment.JS_ActualWeight = 19m;
			declarationForDocuments = (BaseJobDeclaration)newshipment.DeclarationForDocuments;
			declarationForDocuments.ShipmentSynchroniser.Synchronise(true);
			AssertEquals(dec.PK, declarationForDocuments.PK);
			AssertEquals(19m, declarationForDocuments.JE_TotalWeight);
			AssertEquals("MB2", declarationForDocuments.JE_MasterBill);
			AssertEquals("HB2", declarationForDocuments.JE_HouseBill);

			newshipment.Consols.Remove(consol);
			declarationForDocuments = (BaseJobDeclaration)newshipment.DeclarationForDocuments;
			declarationForDocuments.ShipmentSynchroniser.Synchronise(true);
			AssertEquals(19m, declarationForDocuments.JE_TotalWeight);
			AssertEquals("No consol exists", "", declarationForDocuments.JE_MasterBill);
			AssertEquals("HB2", declarationForDocuments.JE_HouseBill);

			consol.JK_MasterBillNum = "MB3";
			newFactory.Save();
			declarationForDocuments = (BaseJobDeclaration)newshipment.DeclarationForDocuments;
			declarationForDocuments.ShipmentSynchroniser.Synchronise(true);
			AssertEquals(19m, declarationForDocuments.JE_TotalWeight);
			AssertEquals("", declarationForDocuments.JE_MasterBill);
			AssertEquals("HB2", declarationForDocuments.JE_HouseBill);

			var newFactory2 = new BusinessObjectFactory();
			shipment = newFactory2.Load<ForwardingShipment>(shipment.PK);
			dec = newFactory2.Load<BaseJobDeclaration>(dec.PK);
			dec.ShipmentSynchroniser.Synchronise(true);
			AssertEquals(dec, shipment.DeclarationForDocuments);
			AssertEquals(19m, dec.JE_TotalWeight);
			AssertEquals("", dec.JE_MasterBill);
			AssertEquals("HB2", dec.JE_HouseBill);

			shipment.JS_ActualWeight = 25m;
			shipment.JS_HouseBill = "HB3";
			AssertEquals(25m, dec.JE_TotalWeight);
			AssertEquals("", dec.JE_MasterBill);
			AssertEquals("HB3", dec.JE_HouseBill);
			newFactory2.Save();

			newshipment.Consols.Add(consol);
			newshipment.JS_ActualWeight = 35m;
			newshipment.JS_HouseBill = "HB4";
			AssertEquals(35m, declarationForDocuments.JE_TotalWeight);
			AssertEquals("Synched", "MB3", declarationForDocuments.JE_MasterBill);
			AssertEquals("HB4", declarationForDocuments.JE_HouseBill);
			declarationForDocuments = (BaseJobDeclaration)newshipment.DeclarationForDocuments;
			declarationForDocuments.ShipmentSynchroniser.Synchronise(true);
			AssertEquals(dec.PK, declarationForDocuments.PK);
			AssertEquals(35m, declarationForDocuments.JE_TotalWeight);
			AssertEquals("Should have the new value", "MB3", declarationForDocuments.JE_MasterBill);
			AssertEquals("HB4", declarationForDocuments.JE_HouseBill);
		}

		public void TestUpdateAllRelatedDeclarationsETDAndETA()
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();

			var declarationWithRelatedVoyage = Factory.New<BaseJobDeclaration>();
			declarationWithRelatedVoyage.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declarationWithRelatedVoyage.JE_TransportMode = TransportTypeList.Codes.Sea;
			declarationWithRelatedVoyage.JE_VesselName = "BUNGA DELIMA";
			declarationWithRelatedVoyage.JE_VoyageFlightNo = "4389";
			declarationWithRelatedVoyage.JE_OH_ShippingLine = shippingLine.PK;
			declarationWithRelatedVoyage.JE_RL_NKPortOfLoading = "NZAKL";
			declarationWithRelatedVoyage.JE_RL_NKOrigin = "NZWEL";
			declarationWithRelatedVoyage.JE_RL_NKPortOfArrival = "AUPER";
			declarationWithRelatedVoyage.JE_RL_NKFinalDestination = "AUADL";
			declarationWithRelatedVoyage.JE_VoyageFlightNo = "Voyage";
			declarationWithRelatedVoyage.JE_ExportDate = new ZDateTime(2010, 1, 2);
			declarationWithRelatedVoyage.JE_DateOfArrival = new ZDateTime(2010, 1, 3);
			declarationWithRelatedVoyage.JE_DateAtOrigin = new ZDateTime(2010, 1, 1);
			declarationWithRelatedVoyage.JE_DateAtFinalDestination = new ZDateTime(2010, 1, 4);
			declarationWithRelatedVoyage.JE_DeclarationReference = "DEC1";
			AssertEquals("Precondition: declaration.Transports.Count", 1, declarationWithRelatedVoyage.Transports.Count);
			Transport transport = declarationWithRelatedVoyage.Transports[0];
			AssertEquals("Precondition: transport.JW_ETD", new ZDateTime(2010, 1, 2), transport.JW_ETD);
			AssertEquals("Precondition: transport.JW_ETA", new ZDateTime(2010, 1, 3), transport.JW_ETA);
			transport.JW_IsLinked = true;
			JobSailing sailing = transport.Sailing;

			var messagingDeclarationWithRelatedVoyage = Factory.New<BaseJobDeclaration>();
			messagingDeclarationWithRelatedVoyage.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			messagingDeclarationWithRelatedVoyage.JE_TransportMode = TransportTypeList.Codes.Sea;
			messagingDeclarationWithRelatedVoyage.JE_VesselName = "BUNGA DELIMA";
			messagingDeclarationWithRelatedVoyage.JE_VoyageFlightNo = "4389";
			messagingDeclarationWithRelatedVoyage.JE_OH_ShippingLine = shippingLine.PK;
			messagingDeclarationWithRelatedVoyage.JE_RL_NKPortOfLoading = "NZAKL";
			messagingDeclarationWithRelatedVoyage.JE_RL_NKOrigin = "NZWEL";
			messagingDeclarationWithRelatedVoyage.JE_RL_NKPortOfArrival = "AUPER";
			messagingDeclarationWithRelatedVoyage.JE_RL_NKFinalDestination = "AUADL";
			messagingDeclarationWithRelatedVoyage.JE_VoyageFlightNo = "Voyage";
			messagingDeclarationWithRelatedVoyage.JE_ExportDate = new ZDateTime(2010, 1, 2);
			messagingDeclarationWithRelatedVoyage.JE_DateOfArrival = new ZDateTime(2010, 1, 3);
			messagingDeclarationWithRelatedVoyage.JE_DateAtOrigin = new ZDateTime(2010, 1, 1);
			messagingDeclarationWithRelatedVoyage.JE_DateAtFinalDestination = new ZDateTime(2010, 1, 4);
			messagingDeclarationWithRelatedVoyage.JE_DeclarationReference = "DEC2";

			EDIMessage message = messagingDeclarationWithRelatedVoyage.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S00001000";
			var nonSynchronisedShipmentDeclarationWithRelatedVoyage = Factory.New<BaseJobDeclaration>();
			nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_JS = shipment1.PK;
			nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_OverrideFreightDefaults = true;
			nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_TransportMode = TransportTypeList.Codes.Sea;
			nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_VesselName = "BUNGA DELIMA";
			nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_VoyageFlightNo = "4389";
			nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_OH_ShippingLine = shippingLine.PK;
			nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_RL_NKPortOfLoading = "NZAKL";
			nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_RL_NKOrigin = "NZWEL";
			nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_RL_NKPortOfArrival = "AUPER";
			nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_RL_NKFinalDestination = "AUADL";
			nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_VoyageFlightNo = "Voyage";
			nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_ExportDate = new ZDateTime(2010, 1, 2);
			nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_DateOfArrival = new ZDateTime(2010, 1, 3);
			nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_DateAtOrigin = new ZDateTime(2010, 1, 1);
			nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_DateAtFinalDestination = new ZDateTime(2010, 1, 4);
			nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_DeclarationReference = "DEC1";

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S00001001";
			var synchronisedShipmentDeclarationWithRelatedVoyage = Factory.New<BaseJobDeclaration>();
			synchronisedShipmentDeclarationWithRelatedVoyage.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			synchronisedShipmentDeclarationWithRelatedVoyage.JE_JS = shipment2.PK;
			synchronisedShipmentDeclarationWithRelatedVoyage.JE_OverrideFreightDefaults = false;
			synchronisedShipmentDeclarationWithRelatedVoyage.JE_TransportMode = TransportTypeList.Codes.Sea;
			synchronisedShipmentDeclarationWithRelatedVoyage.JE_VesselName = "BUNGA DELIMA";
			synchronisedShipmentDeclarationWithRelatedVoyage.JE_VoyageFlightNo = "4389";
			synchronisedShipmentDeclarationWithRelatedVoyage.JE_OH_ShippingLine = shippingLine.PK;
			synchronisedShipmentDeclarationWithRelatedVoyage.JE_RL_NKPortOfLoading = "NZAKL";
			synchronisedShipmentDeclarationWithRelatedVoyage.JE_RL_NKOrigin = "NZWEL";
			synchronisedShipmentDeclarationWithRelatedVoyage.JE_RL_NKPortOfArrival = "AUPER";
			synchronisedShipmentDeclarationWithRelatedVoyage.JE_RL_NKFinalDestination = "AUADL";
			synchronisedShipmentDeclarationWithRelatedVoyage.JE_VoyageFlightNo = "Voyage";
			synchronisedShipmentDeclarationWithRelatedVoyage.JE_ExportDate = new ZDateTime(2010, 1, 2);
			synchronisedShipmentDeclarationWithRelatedVoyage.JE_DateOfArrival = new ZDateTime(2010, 1, 3);
			synchronisedShipmentDeclarationWithRelatedVoyage.JE_DateAtOrigin = new ZDateTime(2010, 1, 1);
			synchronisedShipmentDeclarationWithRelatedVoyage.JE_DateAtFinalDestination = new ZDateTime(2010, 1, 4);
			synchronisedShipmentDeclarationWithRelatedVoyage.JE_DeclarationReference = "DEC1";

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			declarationWithRelatedVoyage.JE_ExportDate = new ZDateTime(2010, 1, 12);
			AssertEquals(SortedText("The following declarations were processed:\r\nDEC1 ETD updated\r\nDEC2 has not been updated as messages have been sent\r\nS00001000 ETD updated\r\nS00001001 has not been updated as it is synchronized with shipment information"), SortedText(UnitTestUserNotification.Instance.LastMessage.Text));
			AssertEquals("ETD updated", new ZDateTime(2010, 1, 11), declarationWithRelatedVoyage.JE_DateAtOrigin);
			AssertEquals("Export date not updated", new ZDateTime(2010, 1, 2), messagingDeclarationWithRelatedVoyage.JE_ExportDate);
			AssertEquals("ETD not updated because messages exist", new ZDateTime(2010, 1, 1), messagingDeclarationWithRelatedVoyage.JE_DateAtOrigin);
			AssertEquals("Export date not updated", new ZDateTime(2010, 1, 2), nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_ExportDate);
			AssertEquals("ETD updated", new ZDateTime(2010, 1, 11), nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_DateAtOrigin);
			AssertEquals("Export date not updated", new ZDateTime(2010, 1, 2), synchronisedShipmentDeclarationWithRelatedVoyage.JE_ExportDate);
			AssertEquals("ETD not updated because synchronised with shipment", new ZDateTime(2010, 1, 1), synchronisedShipmentDeclarationWithRelatedVoyage.JE_DateAtOrigin);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			declarationWithRelatedVoyage.JE_DateOfArrival = new ZDateTime(2010, 1, 13);
			AssertEquals(SortedText("The following declarations were processed:\r\nDEC1 ETA updated\r\nDEC2 has not been updated as messages have been sent\r\nS00001000 ETA updated\r\nS00001001 has not been updated as it is synchronized with shipment information"), SortedText(UnitTestUserNotification.Instance.LastMessage.Text));
			AssertEquals("ETA updated", new ZDateTime(2010, 1, 14), declarationWithRelatedVoyage.JE_DateAtFinalDestination);
			AssertEquals("Arrival date not updated", new ZDateTime(2010, 1, 3), messagingDeclarationWithRelatedVoyage.JE_DateOfArrival);
			AssertEquals("ETA not updated because messages exist", new ZDateTime(2010, 1, 4), messagingDeclarationWithRelatedVoyage.JE_DateAtFinalDestination);
			AssertEquals("Arrival date not updated", new ZDateTime(2010, 1, 3), nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_DateOfArrival);
			AssertEquals("ETA updated", new ZDateTime(2010, 1, 14), nonSynchronisedShipmentDeclarationWithRelatedVoyage.JE_DateAtFinalDestination);
			AssertEquals("Arrival date not updatedt", new ZDateTime(2010, 1, 3), synchronisedShipmentDeclarationWithRelatedVoyage.JE_DateOfArrival);
			AssertEquals("ETA not updated because synchronised with shipment", new ZDateTime(2010, 1, 4), synchronisedShipmentDeclarationWithRelatedVoyage.JE_DateAtFinalDestination);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertNoExceptionThrown(delegate
			{
				declarationWithRelatedVoyage.JE_DateAtOrigin = ZDateTime.Invalid;
				declarationWithRelatedVoyage.JE_ExportDate = ZDateTime.Now;

				declarationWithRelatedVoyage.JE_DateAtFinalDestination = ZDateTime.Invalid;
				declarationWithRelatedVoyage.JE_DateOfArrival = ZDateTime.Now;
			});
		}

		public void TestSaveBuyerSupplierRelationships()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "Supplier~";
			supplier.OH_RL_NKClosestPort = "AUSYD";
			var deliveryAddress = supplier.Addresses.AddNew();
			deliveryAddress.OA_Address1 = "Supp addr 1";
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer~";
			importer.OH_RL_NKClosestPort = "USLAX";
			var pickupAddress = importer.Addresses.AddNew();
			pickupAddress.OA_Address1 = "Imp Addr 1";
			pickupAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;

			AssertEquals("Prompt to save relationship", true, declaration.BuyerSupplierLinksHelper.ShouldPromptToSaveSupplierBuyerRelationship);

			Factory.Save();
			AssertEquals("Save relationship prompt should be false, added after save", false, declaration.BuyerSupplierLinksHelper.ShouldPromptToSaveSupplierBuyerRelationship);
		}

		public void TestSaveBuyerSupplierRelationshipsDoesNotPromptWhenRegistryOff()
		{
			Env.Registry.PromptToSaveBuyerSupplier = false;

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "Supplier~";
			supplier.OH_RL_NKClosestPort = "AUSYD";
			var deliveryAddress = supplier.Addresses.AddNew();
			deliveryAddress.OA_Address1 = "Supp addr 1";
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer~";
			importer.OH_RL_NKClosestPort = "USLAX";
			var pickupAddress = importer.Addresses.AddNew();
			pickupAddress.OA_Address1 = "Imp Addr 1";
			pickupAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;

			AssertEquals("Prompt to save relationship", false, declaration.BuyerSupplierLinksHelper.ShouldPromptToSaveSupplierBuyerRelationship);

			Env.Registry.PromptToSaveBuyerSupplier = true;
			AssertEquals("Should prompt to save relationship when registry turned on", true, declaration.BuyerSupplierLinksHelper.ShouldPromptToSaveSupplierBuyerRelationship);
		}

		public void TestConsigneeDefaultsBasedOnRegistry()
		{
			Env.Registry.UseBuyerSupplierRelationships = false;

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer~";
			importer.OH_RL_NKClosestPort = "USLAX";
			var pickupAddress = importer.Addresses.AddNew();
			pickupAddress.OA_Address1 = "Imp Addr 1";
			pickupAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "Supplier~";
			supplier.OH_RL_NKClosestPort = "AUSYD";
			var deliveryAddress = supplier.Addresses.AddNew();
			deliveryAddress.OA_Address1 = "Supp addr 1";
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			supplier.BuyerLinks.AddNew(importer);

			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("Importer should not default from org links even if only 1 link when UseBuyerSupplierRelationships is not on", ZGuid.Empty, declaration.JE_OH_Importer);

			Env.Registry.UseBuyerSupplierRelationships = true;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("Importer should default from org links when only 1 link when UseBuyerSupplierRelationships is on", importer.PK, declaration.JE_OH_Importer);
		}

		public void TestIRegistryAccessingSupporterMembers()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			var declaration = Factory.New<BaseJobDeclaration>();
			IRegistryAccessingSupporter supporter = declaration;

			declaration.JE_GB = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), supporter.RegistryBranchPK);

			declaration.JE_GB = branch.PK;
			AssertEquals(company.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(branch.PK.ToGuid(), supporter.RegistryBranchPK);

			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), supporter.RegistryBranchPK);
		}

		public void TestIsJE_MessageTypeChangedSinceLoading()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Assert(!declaration.IsJE_MessageTypeChangedSinceLoading);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(!declaration.IsJE_MessageTypeChangedSinceLoading);

			Factory.Save();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(!declaration.IsJE_MessageTypeChangedSinceLoading);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert(declaration.IsJE_MessageTypeChangedSinceLoading);
		}

		public void TestAddressesCopyInTemplateCopy()
		{
			var importerName = "LA LA LE LIE LA";
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = importerName;
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			AssertNotNull("Precondition: declaration.ImporterDeliveryAddress", declaration.ImporterDeliveryAddress);
			AssertEquals("Precondition: declaration.ImporterDeliveryAddress.E2_CompanyName", importerName, declaration.ImporterDeliveryAddress.E2_CompanyName);

			var copiedDeclaration = (BaseJobDeclaration)declaration.TemplateCopy();
			AssertNotNull("copiedDeclaration.ImporterDeliveryAddress", copiedDeclaration.ImporterDeliveryAddress);
			AssertEquals("copiedDeclaration.ImporterDeliveryAddress.E2_CompanyName", importerName, copiedDeclaration.ImporterDeliveryAddress.E2_CompanyName);
		}

		public void TestIInvoicesProvider()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			IInvoicesProvider invoicesProvider = declaration;
			AssertEquals("Lookups", declaration.Lookups.GetType(), invoicesProvider.Lookups.GetType());
			AssertSame("Invoices", declaration.Invoices, invoicesProvider.Invoices);
		}

		public void TestCopyNonBillDecDoesNotCreateDummy()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TotalNoOfPacks = 10;
			declaration.JE_TotalNoOfPacksPackType = "NO";
			var clonedDec = (BaseJobDeclaration)declaration.TemplateCopy();
			AssertEquals(0, clonedDec.Bills.Count);
			AssertEquals(10, clonedDec.JE_TotalNoOfPacks);
			AssertEquals("NO", clonedDec.JE_TotalNoOfPacksPackType);
			clonedDec = (BaseJobDeclaration)declaration.Clone();
			AssertEquals(0, clonedDec.Bills.Count);
			AssertEquals(10, clonedDec.JE_TotalNoOfPacks);
			AssertEquals("NO", clonedDec.JE_TotalNoOfPacksPackType);
		}

		public void TestPackDetailsDefault()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MasterBill = ZString.Empty;
			declaration.JE_HouseBill = ZString.Empty;
			declaration.JE_TotalNoOfPacks = 10;
			declaration.JE_TotalNoOfPacksPackType = "NO";
			AssertEquals(0, declaration.Bills.Count);
			AssertEquals(0, declaration.PackingGroups.Count);
			declaration.JE_HouseBill = "HB1";
			AssertEquals(1, declaration.Bills.Count);
			AssertEquals(declaration.PrimaryHouseBill, declaration.Bills[0]);
			AssertEquals(1, declaration.PrimaryHouseBill.PackingGroups.Count);
			var packingGroup = declaration.PrimaryHouseBill.PackingGroups[0];
			AssertEquals(1, packingGroup.Packages.Count);
			var package = packingGroup.Packages[0];
			AssertEquals(10, package.CW_PackQty);
			AssertEquals("NO", package.CW_PackType);
			declaration.JE_TotalNoOfPacks = 13;
			AssertEquals(10, package.CW_PackQty);
			AssertEquals("NO", package.CW_PackType);
			package.CW_PackQty = 0;
			declaration.JE_TotalNoOfPacks = 15;
			AssertEquals(15, package.CW_PackQty);
			AssertEquals("NO", package.CW_PackType);
			declaration.JE_TotalNoOfPacksPackType = "KG";
			AssertEquals(15, package.CW_PackQty);
			AssertEquals("KG", package.CW_PackType);
			declaration.JE_MasterBill = "MB1";
			AssertEquals(2, declaration.Bills.Count);
			var masterBill = declaration.PrimaryMasterBill;
			AssertCollectionContains(masterBill, declaration.Bills);
			AssertEquals(0, masterBill.PackingGroups.Count);
			AssertEquals(15, package.CW_PackQty);
			AssertEquals("KG", package.CW_PackType);
			declaration.PrimaryHouseBill.Delete();
			AssertEquals(true, packingGroup.IsDeleted);
			AssertEquals(true, package.IsDeleted);
			declaration.JE_MasterBill = "MB2";
			AssertEquals(1, declaration.Bills.Count);
			masterBill = declaration.PrimaryMasterBill;
			AssertEquals(masterBill, declaration.Bills[0]);
			AssertEquals(1, masterBill.PackingGroups.Count);
			var packingGroup2 = masterBill.PackingGroups[0];
			AssertNotEquals(packingGroup2, packingGroup);
			AssertEquals(1, packingGroup2.Packages.Count);
			var package2 = packingGroup2.Packages[0];
			AssertNotEquals(package2, package);
			AssertEquals(15, package2.CW_PackQty);
			AssertEquals("KG", package2.CW_PackType);
		}

		public void TestCloningShipmentAndDeclaration_CS00140036()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_VoyageFlightNo = "84234";
			declaration.JE_VesselName = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Code;
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_RL_NKPortOfArrival = "AUSYD";

			Factory.Save();

			var shipmentCopied = (ForwardingShipment)shipment.TemplateCopy();
			var declarationCopied = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_JS, shipmentCopied.PK));

			AssertEquals("There should not be any transport records against plugged-in declarations", 0, declarationCopied.Transports.Count);
		}

		public void TestIWeightHolderMembers()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TotalWeight = 120m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Pounds;
			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			IWeightHolder weightHolder = declaration;
			AssertEquals("TotalWeight", new ZWeight(120m, Core.Constants.Weight.Pounds), weightHolder.TotalWeight);
			AssertEquals("AllApportionees", 2, weightHolder.AllApportionees.Length);
			AssertCollectionContains(invoice1, weightHolder.AllApportionees);
			AssertCollectionContains(invoice2, weightHolder.AllApportionees);
		}

		public void TestUnmatchedClassificationSaving()
		{
			var dec = Factory.New<JobDeclarationWithExposed_IsUnmatchedProductClassification>();
			var decSupplier = OrgHeader.New(Factory);
			decSupplier.OH_Code = "SUPPLIER";
			decSupplier.OH_IsConsignor = true;
			decSupplier.MainAddress.OA_Address1 = "Add1";
			dec.JE_OH_Supplier = decSupplier.PK;

			var invHeader = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invLine1 = invHeader.InvoiceLines.AddNew();
			invLine1.JI_Tariff = "123";

			Assert("Part is null, therefore it's not possible for it to be unmatched", !dec.IsUnmatchedProductClassification_Exposed(invLine1));

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "SomePart";
			part.OP_Desc = "Description of product";
			part.OP_StockKeepingUnit = Core.Constants.PkgUnit.Bag;
			var relOrg = part.RelatedOrganisations.AddNew();
			relOrg.OU_OH = dec.Supplier.PK;
			relOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			invLine1.JI_PartNo = part.OP_PartNum;

			Assert("Part exists without a classification, therefore it is an unmatched Product", dec.IsUnmatchedProductClassification_Exposed(invLine1));

			invLine1.JI_Tariff = ZString.Empty;
			Assert("Invoice Line does not have a tarriff or Classification, therefore it is not an unmatched Product", !dec.IsUnmatchedProductClassification_Exposed(invLine1));

			invLine1.JI_CC = ZGuid.NewZGuid();
			Assert("Invoice Line has a Classification and Part exists without a classification, therefore it is not an unmatched Product", dec.IsUnmatchedProductClassification_Exposed(invLine1));

			invLine1.JI_CC = ZGuid.Empty;
			invLine1.JI_Tariff = "123";

			var classification1 = Factory.New<BaseCusClassification>();
			classification1.CC_Description = "CUCKOO SQUEAKERS";
			classification1.CC_LookupCode = "CKSQKS";
			classification1.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			classification1.CC_TariffNum = "456";

			var classification2 = Factory.New<BaseCusClassification>();
			classification2.CC_Description = "DECKOO SQUEAKERS";
			classification2.CC_LookupCode = "DESQKS";
			classification2.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;

			var classification3 = Factory.New<BaseCusClassification>();
			classification2.CC_Description = "EECKOO SQUEAKERS";
			classification2.CC_LookupCode = "EESQKS";
			classification2.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;

			var classPartPivot1 = Factory.New<BaseCusClassPartPivot>();
			classPartPivot1.CI_OP = part.PK;
			classPartPivot1.CI_ChildType = ClassificationTypeList.Codes.HTB;
			classPartPivot1.CI_TariffNum = "123";

			var classPartPivot2 = Factory.New<BaseCusClassPartPivot>();
			classPartPivot2.CI_OP = part.PK;
			classPartPivot2.CI_ChildType = ClassificationTypeList.Codes.HTB;
			classPartPivot2.CI_TariffNum = "456";
			classPartPivot2.CI_CC = classification1.PK;

			var classPartPivot3 = Factory.New<BaseCusClassPartPivot>();
			classPartPivot3.CI_OP = part.PK;
			classPartPivot3.CI_ChildType = ClassificationTypeList.Codes.HTB;
			classPartPivot3.CI_TariffNum = ZString.Empty;
			classPartPivot3.CI_CC = classification2.PK;

			Assert("Part exists with a PartPivot, therefore it is not an unmatched Product", !dec.IsUnmatchedProductClassification_Exposed(invLine1));

			var invLine2 = invHeader.InvoiceLines.AddNew();
			Assert(!dec.IsUnmatchedProductClassification_Exposed(invLine2));
			Assert("Neither of the invoice lines are unmatched", !dec.InvoicesMentionProductsWithoutMatchingClassification);

			invLine1.JI_Tariff = "456";
			Assert("There is no PartPivot in the Part with the corresponding tariff and Classification, therefore it is an unmatched Product", dec.IsUnmatchedProductClassification_Exposed(invLine1));

			invLine1.JI_CC = classification1.PK;
			Assert("There exists a PartPivot in the Part with the corresponding tariff and Classification, therefore it is not an unmatched Product", !dec.IsUnmatchedProductClassification_Exposed(invLine1));

			invLine1.JI_Tariff = ZString.Empty;
			invLine1.JI_CC = classification3.PK;
			Assert("There is no PartPivot in the Part with the corresponding tariff and Classification, therefore it is an unmatched Product", dec.IsUnmatchedProductClassification_Exposed(invLine1));

			invLine1.JI_CC = classification2.PK;
			Assert("There exists a PartPivot in the Part with the corresponding tariff and Classification, therefore it is not an unmatched Product", !dec.IsUnmatchedProductClassification_Exposed(invLine1));

			classPartPivot3.CI_OP = ZGuid.Empty;
			Assert(dec.IsUnmatchedProductClassification_Exposed(invLine1));
			Assert("Atleast one of the invoice lines are matched", dec.InvoicesMentionProductsWithoutMatchingClassification);
		}

		public void TestDefaultingDateOfFirstArrivalWhenNoSailingSchedule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var testDec = Factory.New<BaseJobDeclaration>();

				testDec.JE_RL_NKPortOfArrival = "USLAX";
				testDec.JE_DateOfArrival = new ZDateTime(2001, 12, 30);
				testDec.JE_RL_NKPortOfFirstArrival = "USLAX";
				AssertEquals("Date of first arrival should be defaulted", new ZDateTime(2001, 12, 30), testDec.JE_DateOfFirstArrival);

				testDec.JE_DateOfFirstArrival = new ZDateTime(2006, 1, 12);
				testDec.JE_RL_NKPortOfFirstArrival = "";
				testDec.JE_RL_NKPortOfFirstArrival = "USLAX";
				AssertEquals("Date of first arrival should not be defaulted", new ZDateTime(2006, 1, 12), testDec.JE_DateOfFirstArrival);
			}
		}

		public void TestDefaultingDateOfFirstArrivalWhenSailingSchedule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var yoyage = InitialiseSailings();
				var testDec = Factory.New<BaseJobDeclaration>();
				testDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
				testDec.JE_VesselName = yoyage.JV_RV_NKVessel;
				testDec.JE_VoyageFlightNo = yoyage.JV_VoyageFlight;

				AssertEquals(true, testDec.JE_DateOfFirstArrival.IsEmpty);
				testDec.JE_RL_NKPortOfFirstArrival = "AUPER";
				AssertEquals(new ZDateTime(2005, 12, 24), testDec.JE_DateOfFirstArrival);

				testDec.JE_DateOfFirstArrival = new ZDateTime(2006, 01, 17);
				testDec.JE_RL_NKPortOfFirstArrival = "";
				testDec.JE_RL_NKPortOfFirstArrival = "AUPER";
				AssertEquals(new ZDateTime(2005, 12, 24), testDec.JE_DateOfFirstArrival);

				//testDec.JE_DateOfFirstArrival = ZDateTime.Empty; It should default even when this date has a valid value.
				testDec.JE_RL_NKPortOfFirstArrival = "";
				testDec.JE_RL_NKPortOfFirstArrival = "AUPER";
				AssertEquals(new ZDateTime(2005, 12, 24), testDec.JE_DateOfFirstArrival);
			}
		}

		public void TestDefaultPortsFromImporterSupplierAndThenDatesFromSailing()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var yoyage = InitialiseSailings();

				var importer = Factory.NewWithValidTestData<OrgHeader>();
				importer.OH_RL_NKClosestPort = "AUSYD";

				var supplier = Factory.NewWithValidTestData<OrgHeader>();
				supplier.OH_RL_NKClosestPort = "IRABD";

				var testDec = Factory.New<BaseJobDeclaration>();
				testDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
				testDec.JE_VesselName = yoyage.JV_RV_NKVessel;
				testDec.JE_VoyageFlightNo = yoyage.JV_VoyageFlight;

				testDec.JE_OH_Supplier = supplier.PK;
				AssertEquals("Loading & origin ports are defaulted from supplier", "IRABD", testDec.JE_RL_NKPortOfLoading);
				AssertEquals("Loading & origin ports are defaulted from supplier", "IRABD", testDec.JE_RL_NKOrigin);
				AssertEquals("JE_DateAtOrigin", new ZDateTime(2005, 12, 20), testDec.JE_DateAtOrigin);
				AssertEquals("Export Date", new ZDateTime(2005, 12, 20), testDec.JE_ExportDate);

				testDec.JE_OH_Importer = importer.PK;
				AssertEquals("Arrival & Destination Ports from importer", "AUSYD", testDec.JE_RL_NKPortOfArrival);
				AssertEquals("Arrival & Destination Ports from importer", "AUSYD", testDec.JE_RL_NKFinalDestination);

				AssertEquals("JE_DateAtFinalDestination", new ZDateTime(2005, 12, 22), testDec.JE_DateAtFinalDestination);
				AssertEquals("Arrival Date", new ZDateTime(2005, 12, 22), testDec.JE_DateOfArrival);
				AssertEquals("First Arrival Date", new ZDateTime(2005, 12, 22), testDec.JE_DateOfFirstArrival);

				testDec.JE_RL_NKPortOfArrival = "AUPER";
				AssertEquals("Arrival Date", new ZDateTime(2005, 12, 24), testDec.JE_DateOfArrival);

				testDec.JE_RL_NKFinalDestination = "AUPER";
				AssertEquals("Destination Date", new ZDateTime(2005, 12, 24), testDec.JE_DateOfArrival);
			}
		}

		public void TestPortOfFirstArrivalIsSetToFinalDestination()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var testDec = Factory.New<BaseJobDeclaration>();
				testDec.JE_RL_NKOrigin = "NZAKL";
				testDec.JE_DateAtOrigin = new ZDateTime(2004, 10, 30);
				testDec.JE_RL_NKFinalDestination = "AUSYD";
				testDec.JE_DateAtFinalDestination = new ZDateTime(2004, 11, 1);

				AssertEquals("Discharge", testDec.JE_RL_NKFinalDestination, testDec.JE_RL_NKPortOfFirstArrival);
			}
		}

		[TestDate(2007, 02, 05)]
		[RunInExtraTransaction]
		public void TestPopulateDeclarationReferenceUsingCustomisation()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = OverseasPort.Code;
			declaration.JE_RL_NKFinalDestination = HomePort.Code;

			var nextJobNumber = Env.NumberFountains.CustomsJobNo.PeekPreliminaryFormatted(Factory);
			declaration.OnSaving();
			AssertEquals("JE_DeclarationReference", nextJobNumber, declaration.JE_DeclarationReference);

			declaration.OnSaving();
			AssertEquals("JE_DeclarationReference", nextJobNumber, declaration.JE_DeclarationReference);

			var newCustomisation = new BillOfLadingNumberCustomisation();
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 1).Fountain = true;
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter, 2).Fountain = true;
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");
			CustomsDataRegistry.Instance.DeclarationNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation);

			declaration.JE_DeclarationReference = "";
			declaration.OnSaving();
			AssertEquals("JE_DeclarationReference", "B7B001", declaration.JE_DeclarationReference);

			declaration.OnSaving();
			AssertEquals("JE_DeclarationReference", "B7B001", declaration.JE_DeclarationReference);

			declaration.JE_DeclarationReference = "";
			declaration.OnSaving();
			AssertEquals("JE_DeclarationReference", "B7B002", declaration.JE_DeclarationReference);
			declaration.OnSaving();

			AssertEquals("JE_DeclarationReference", "B7B002", declaration.JE_DeclarationReference);

			declaration.JE_DeclarationReference = "";
			(declaration as IFountainResolver).TryToResolve();
			declaration.OnSaving();
			AssertEquals("JE_DeclarationReference", "B7B004", declaration.JE_DeclarationReference);

			declaration.JE_DeclarationReference = "";
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "BR1";
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			var branchCustomisation = new BillOfLadingNumberCustomisation();
			SetElement(branchCustomisation, BillOfLadingNumberCustomisationElement.Keys.BranchCode, 1).Fountain = false;
			CustomsDataRegistry.Instance.DeclarationNumberCustomisation.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, branchCustomisation);
			declaration.JE_GB = branch1.PK;
			declaration.OnSaving();
			AssertContains("JE_DeclarationReference", "BBR1", declaration.JE_DeclarationReference);

			declaration.JE_GB = ZGuid.Empty;
			declaration.JE_DeclarationReference = "";
			CustomsDataRegistry.Instance.DeclarationNumberCustomisation.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, branchCustomisation);
			CustomsDataRegistry.Instance.DeclarationNumberCustomisation.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, branchCustomisation);
			declaration.OnSaving();
			AssertContains("JE_DeclarationReference", "B" + GlbBranch.CurrentBranch.GB_Code, declaration.JE_DeclarationReference);
		}

		public void TestIBillGenerationSupportMembers()
		{
			var org = Factory.New<OrgHeader>();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_ShippingLine = org.PK;
			declaration.JE_RL_NKOrigin = HomePort.Code;
			declaration.JE_RL_NKFinalDestination = OverseasPort.Code;

			IBillGenerationSupport billGenerationSupport = declaration;
			AssertEquals("CarrierPrincipal", org, billGenerationSupport.CarrierPrincipal);
			AssertEquals("Origin", HomePort, billGenerationSupport.Origin);
			AssertEquals("Destination", OverseasPort, billGenerationSupport.Destination);
			AssertEquals("Factory", Factory, billGenerationSupport.Factory);
			AssertNull("Load should be NULL", billGenerationSupport.Load);
			AssertNull("Discharge should be NULL", billGenerationSupport.Discharge);

			AssertEquals("", billGenerationSupport.TranshipmentIndicator);

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			AssertEquals("TransportMode", Core.Constants.TransportModes.Air, billGenerationSupport.TransportMode);

			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			AssertEquals("TransportMode", Core.Constants.TransportModes.Sea, billGenerationSupport.TransportMode);

			declaration.JE_TransportMode = declaration.TransportModeRailCodeForTesting;
			AssertEquals("TransportMode", Core.Constants.TransportModes.Rail, billGenerationSupport.TransportMode);

			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
			AssertEquals("TransportMode", Core.Constants.TransportModes.Road, billGenerationSupport.TransportMode);

			declaration.JE_TransportMode = declaration.TransportModeMailCodeForTesting;
			AssertEquals("TransportMode", Core.Constants.TransportModes.Other, billGenerationSupport.TransportMode);
		}

		public void TestDisableResultApportionmentWithRealInvoice()
		{
			using (DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var testDec = Factory.New<BaseJobDeclaration>();

				var invoice = testDec.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 3581.21m;
				invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice.JZ_IncoTerm = "FOB";

				var invDIS = invoice.Charges.AddNew();
				invDIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				invDIS.J7_Percentage = 0.75m;

				var invFIFT = invoice.Charges.AddNew();
				invFIFT.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
				invFIFT.J7_Amount = 55.85m;
				invFIFT.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
				invFIFT.J7_IsIncludedInITOT = false;

				var line1 = invoice.JobComInvoiceLines.AddNew();
				line1.JI_LinePrice = 360;

				var line2 = invoice.JobComInvoiceLines.AddNew();
				line2.JI_LinePrice = 2880m;

				var line3 = invoice.JobComInvoiceLines.AddNew();
				line3.JI_LinePrice = 312m;

				testDec.ResumeApportionment();

				AssertEquals("Line1 FIFT", 5.66m, line1.ApportionedCharges.GetCharge(invFIFT.ChargeKey).Amount, 0.01m);
				AssertEquals("Line2 FIFT", 45.28m, line2.ApportionedCharges.GetCharge(invFIFT.ChargeKey).Amount, 0.01m);
				AssertEquals("Line3 FIFT", 4.91m, line3.ApportionedCharges.GetCharge(invFIFT.ChargeKey).Amount, 0.01m);

				AssertEquals("Line1 DIS", 2.70m, line1.ApportionedCharges.GetCharge(invDIS.ChargeKey).Amount, 0.01m);
				AssertEquals("Line2 DIS", 21.60m, line2.ApportionedCharges.GetCharge(invDIS.ChargeKey).Amount, 0.01m);
				AssertEquals("Line3 DIS", 2.34m, line3.ApportionedCharges.GetCharge(invDIS.ChargeKey).Amount, 0.01m);
			}
		}

		public void TestDisableResultApportionmentWithRealInvoice3()
		{
			using (DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var testDec = Factory.New<BaseJobDeclaration>();

				var invoice = testDec.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 3581.21m;
				invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice.JZ_IncoTerm = "FOB";

				var invFIFT = invoice.Charges.AddNew();
				invFIFT.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
				invFIFT.J7_Amount = 55.85m;
				invFIFT.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
				invFIFT.J7_IsIncludedInITOT = false;

				var line1 = invoice.JobComInvoiceLines.AddNew();
				line1.JI_LinePrice = 360;
				var line1DIS = line1.Charges.AddNew();
				line1DIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				line1DIS.J7_Percentage = 0.75m;

				var line2 = invoice.JobComInvoiceLines.AddNew();
				line2.JI_LinePrice = 2880m;
				var line2DIS = line2.Charges.AddNew();
				line2DIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				line2DIS.J7_Percentage = 0.75m;

				var line3 = invoice.JobComInvoiceLines.AddNew();
				line3.JI_LinePrice = 312m;
				var line3DIS = line3.Charges.AddNew();
				line3DIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				line3DIS.J7_Percentage = 0.75m;

				testDec.ResumeApportionment();

				AssertEquals("Line1 FIFT", 5.66m, line1.ApportionedCharges.GetCharge(invFIFT.ChargeKey).Amount, 0.01m);
				AssertEquals("Line2 FIFT", 45.28m, line2.ApportionedCharges.GetCharge(invFIFT.ChargeKey).Amount, 0.01m);
				AssertEquals("Line3 FIFT", 4.91m, line3.ApportionedCharges.GetCharge(invFIFT.ChargeKey).Amount, 0.01m);
				AssertEquals("Invoice DIS", 26.64m, invoice.GroupCharges.GetCharge(line1DIS.ChargeKey).Amount, 0.01m);
			}
		}

		public void TestDefaultPackQuantityAndPackTypeToBill()
		{
			var declaration = Factory.New<DummyBaseJobDeclaration_TestDefaultPackQuantityAndPackTypeToBill>();
			declaration.JE_TotalNoOfPacks = 27;
			declaration.JE_TotalNoOfPacksPackType = "T";
			declaration.JE_MasterBill = "MasterBill";

			AssertEquals("Packages.Count", 1, declaration.Packages.Count);
			AssertEquals("CW_PackQty", declaration.JE_TotalNoOfPacks, declaration.Packages[0].CW_PackQty);
			AssertEquals("CW_PackType", declaration.JE_TotalNoOfPacksPackType, declaration.Packages[0].CW_PackType);
			AssertEquals("CW_HouseBill", "MB:MasterBill", declaration.Packages[0].CW_HouseBill);

			declaration.JE_HouseBill = "HouseBill";

			AssertEquals("Packages.Count", 1, declaration.Packages.Count);
			AssertEquals("CW_PackQty", declaration.JE_TotalNoOfPacks, declaration.Packages[0].CW_PackQty);
			AssertEquals("CW_PackType", declaration.JE_TotalNoOfPacksPackType, declaration.Packages[0].CW_PackType);
			AssertEquals("CW_HouseBill", "HB:HouseBill (MB:MasterBill)", declaration.Packages[0].CW_HouseBill);
		}

		public void TestDefaultFreightAmountFromShipmentChargeableAmount()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ActualChargeable = 50m;
			shipment.JS_UnitFreightRate = 1000m;
			shipment.JS_RX_NKFrtRateCurrency = "USD";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var consol = AddConsol(shipment, "USNYC", "DEAAC");
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "11111";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_JC = container1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "22222";

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = container2.PK;
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.DefaultFreightAmountFromShipmentChargeableAmount();
			AssertEquals("Freight should have been defaulted into group charges", 1, declaration.JobComInvoiceGroupHeaders[0].Charges.Count);

			var freightDefaultedForFCL = declaration.JobComInvoiceGroupHeaders[0].Charges[0];
			AssertEquals(2000m, freightDefaultedForFCL.J7_Amount);
			AssertEquals("USD", freightDefaultedForFCL.J7_RX_NKCurrency);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			declaration.DefaultFreightAmountFromShipmentChargeableAmount();

			var freightDefaultedForLCL = declaration.JobComInvoiceGroupHeaders[0].Charges[1];
			AssertEquals(50000m, freightDefaultedForLCL.J7_Amount);
			AssertEquals("USD", freightDefaultedForLCL.J7_RX_NKCurrency);
		}

		public void TestIsEntryClear()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_EntryStatus = "CLR";
			AssertEquals("IsEntryClear:", true, declaration.IsEntryClear);

			declaration.JE_EntryStatus = "WOF";
			AssertEquals("IsEntryClear:", false, declaration.IsEntryClear);
		}

		public void TestSettingJE_HouseBillThenJE_MasterBill()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_HouseBill = "1";
			AssertNotNull(declaration.PrimaryHouseBill);
			AssertNull(declaration.PrimaryHouseBill.ParentBill);

			declaration.JE_MasterBill = "2";
			AssertNotNull(declaration.PrimaryMasterBill);
			AssertEquals("Setting JE_MasterBill sets ParentBill", declaration.PrimaryMasterBill, declaration.PrimaryHouseBill.ParentBill);
		}

		public void TestJE_GoodsDescriptionWithExceedMaxLength()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var maxDescription = new string('X', declaration.JE_GoodsDescriptionInfo.MaxLength);

			declaration.JE_GoodsDescription = maxDescription + "XXX";
			AssertEquals("Declaration.JE_GoodsDescription", maxDescription, declaration.JE_GoodsDescription);
		}

		public void TestJE_GoodsDescriptionDetailedFromDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GoodsDescription = "English Bowlers Eat Cows.";
			AssertEquals("declaration.JE_GoodsDescriptionDetailed", "English Bowlers Eat Cows.", declaration.JE_GoodsDescriptionDetailed);
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "Zubin Has Mad Cows Disease.");
			AssertEquals("declaration.JE_GoodsDescriptionDetailed", "Zubin Has Mad Cows Disease.", declaration.JE_GoodsDescriptionDetailed);
		}

		public void TestJE_GoodsDescriptionDetailedFromShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_GoodsDescription = "Dogs Have Red Crayons.";
			AssertEquals("declaration.JE_GoodsDescriptionDetailed", "Dogs Have Red Crayons.", declaration.JE_GoodsDescriptionDetailed);
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "So Clinty Tells Us.");
			AssertEquals("declaration.JE_GoodsDescriptionDetailed", "So Clinty Tells Us.", declaration.JE_GoodsDescriptionDetailed);
		}

		public void TestIHaveInternalCartageGetPackLines()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var line = declaration.Packages.AddNew();
			IPackLineInfo[] packs = ((IHaveInternalCartage)declaration).GetPackLines();
			AssertEquals("GetPackLines", 1, packs.Length);
			AssertSame(line, packs[0]);
		}

		public void TestBuyerDocumentaryAddress()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var buyerAddress = declaration.BuyerDocAddress;
			AssertNotNull("docAddresses.GetDocAddress(DocAddressType.BuyerDocumentaryAddress)", buyerAddress);
			AssertEquals(DocAddressType.BuyerDocumentaryAddress, buyerAddress.DocAddressType);
			AssertEquals(ContactType.Consignee, buyerAddress.DefaultContactType);
		}

		public void TestIDocAddressesInterfaceImplementsBuyerDocumentaryAddress()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			IDocAddresses docAddresses = declaration;
			var supportedAddressTypes = new List<DocAddressType>(docAddresses.SupportedAddressTypes);
			AssertEquals("supportedAddressTypes.Contains(DocAddressType.BuyerDocumentaryAddress)", true, supportedAddressTypes.Contains(DocAddressType.BuyerDocumentaryAddress));
			AssertEquals("docAddresses.GetDocAddressRequirement(DocAddressType.BuyerDocumentaryAddress)", declaration.BuyerDocAddress.DocAddressType, docAddresses.GetDocAddressRequirement(DocAddressType.BuyerDocumentaryAddress).DefaultDocAddressType);
		}

		public void TestInsuredByDocumentaryAddress()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var insuredByAddress = declaration.InsuredByDocAddress;
			AssertNotNull("docAddresses.GetDocAddress(DocAddressType.InsuredByDocumentaryAddress)", insuredByAddress);
			AssertEquals(DocAddressType.InsuredByDocumentaryAddress, insuredByAddress.DocAddressType);
			AssertEquals(ContactType.Consignee, insuredByAddress.DefaultContactType);
		}

		public void TestIDocAddressesInterfaceImplementsInsuredByDocumentaryAddress()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			IDocAddresses docAddresses = declaration;
			var supportedAddressTypes = new List<DocAddressType>(docAddresses.SupportedAddressTypes);
			AssertEquals("supportedAddressTypes.Contains(DocAddressType.InsuredByDocumentaryAddress)", true, supportedAddressTypes.Contains(DocAddressType.InsuredByDocumentaryAddress));
			AssertEquals("docAddresses.GetDocAddress(DocAddressType.InsuredByDocumentaryAddress)", declaration.InsuredByDocAddress.DocAddressType, docAddresses.GetDocAddressRequirement(DocAddressType.InsuredByDocumentaryAddress).DefaultDocAddressType);
		}

		public void TestAssuredPartyDocumentaryAddress()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var assuredPartyAddress = declaration.AssuredPartyDocAddress;
			AssertNotNull("docAddresses.GetDocAddress(DocAddressType.AssuredPartyDocumentaryAddress)", assuredPartyAddress);
			AssertEquals(DocAddressType.AssuredPartyDocumentaryAddress, assuredPartyAddress.DocAddressType);
			AssertEquals(ContactType.Consignee, assuredPartyAddress.DefaultContactType);
		}

		public void TestIDocAddressesInterfaceImplementsAssuredPartyDocumentaryAddress()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			IDocAddresses docAddresses = declaration;
			var supportedAddressTypes = new List<DocAddressType>(docAddresses.SupportedAddressTypes);
			AssertEquals("supportedAddressTypes.Contains(DocAddressType.AssuredPartyDocumentaryAddress)", true, supportedAddressTypes.Contains(DocAddressType.AssuredPartyDocumentaryAddress));
			AssertEquals("docAddresses.GetDocAddress(DocAddressType.AssuredPartyDocumentaryAddress) = declaration.AssuredPartyDocumentaryAddress", declaration.AssuredPartyDocAddress.DocAddressType, docAddresses.GetDocAddressRequirement(DocAddressType.AssuredPartyDocumentaryAddress).DefaultDocAddressType);
		}

		public void TestClaimsPayableByDocumentaryAddress()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var claimsPayableByAddress = declaration.ClaimsPayableByDocAddress;
			AssertNotNull("docAddresses.GetDocAddress(DocAddressType.ClaimsPayableByDocumentaryAddress)", claimsPayableByAddress);
			AssertEquals(DocAddressType.ClaimsPayableByDocumentaryAddress, claimsPayableByAddress.DocAddressType);
			AssertEquals(ContactType.Consignee, claimsPayableByAddress.DefaultContactType);
		}

		public void TestIDocAddressesInterfaceImplementsClaimsPayableByDocumentaryAddress()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			IDocAddresses docAddresses = declaration;
			var supportedAddressTypes = new List<DocAddressType>(docAddresses.SupportedAddressTypes);
			AssertEquals("supportedAddressTypes.Contains(DocAddressType.ClaimsPayableByDocumentaryAddress)", true, supportedAddressTypes.Contains(DocAddressType.ClaimsPayableByDocumentaryAddress));
			AssertEquals("docAddresses.GetDocAddress(DocAddressType.ClaimsPayableByDocumentaryAddress) = declaration.ClaimsPayableByDocumentaryAddress", declaration.ClaimsPayableByDocAddress.DocAddressType, docAddresses.GetDocAddressRequirement(DocAddressType.ClaimsPayableByDocumentaryAddress).DefaultDocAddressType);
		}

		public void TestSurveyReportPartyDocumentaryAddress()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var surveyReportPartyAddress = declaration.SurveyReportPartyDocAddress;
			AssertNotNull("docAddresses.GetDocAddress(DocAddressType.SurveyReportPartyDocumentaryAddress)", surveyReportPartyAddress);
			AssertEquals(DocAddressType.SurveyReportPartyDocumentaryAddress, surveyReportPartyAddress.DocAddressType);
			AssertEquals(ContactType.Consignee, surveyReportPartyAddress.DefaultContactType);
		}

		public void TestContainerTerminalOperatorPartyDocumentaryAddress()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var containerTerminalOperatorPartyAddress = declaration.ContainerTerminalOperatorDocAddress;
			AssertNotNull("docAddresses.GetDocAddress(DocAddressType.ContainerTerminalOperatorPartyDocumentaryAddress)", containerTerminalOperatorPartyAddress);
			AssertEquals(DocAddressType.CustomsContainerTerminalOperatorAddress, containerTerminalOperatorPartyAddress.DocAddressType);
			AssertEquals(ContactType.Administration, containerTerminalOperatorPartyAddress.DefaultContactType);
		}

		public void TestIDocAddressesInterfaceImplementsSurveyReportPartyDocumentaryAddress()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			IDocAddresses docAddresses = declaration;
			var supportedAddressTypes = new List<DocAddressType>(docAddresses.SupportedAddressTypes);
			AssertEquals("supportedAddressTypes.Contains(DocAddressType.SurveyReportPartyDocumentaryAddress)", true, supportedAddressTypes.Contains(DocAddressType.SurveyReportPartyDocumentaryAddress));
			AssertEquals("docAddresses.GetDocAddress(DocAddressType.SurveyReportPartyDocumentaryAddress) = declaration.SurveyReportPartyDocumentaryAddress", declaration.SurveyReportPartyDocAddress.DocAddressType, docAddresses.GetDocAddressRequirement(DocAddressType.SurveyReportPartyDocumentaryAddress).DefaultDocAddressType);
		}

		public void TestNotifyPartyDocumentaryAddress()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var notifyPartyAddress = declaration.NotifyPartyDocumentaryAddress;
			AssertNotNull("docAddresses.GetDocAddress(DocAddressType.NotifyPartyDocumentaryAddress)", notifyPartyAddress);
			AssertEquals(DocAddressType.NotifyParty, notifyPartyAddress.DocAddressType);
			AssertEquals(ContactType.Consignee, notifyPartyAddress.DefaultContactType);
		}

		public void TestIDocAddressesInterfaceImplementsNotifyPartyDocumentaryAddress()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			IDocAddresses docAddresses = declaration;
			var supportedAddressTypes = new List<DocAddressType>(docAddresses.SupportedAddressTypes);
			AssertEquals("supportedAddressTypes.Contains(DocAddressType.NotifyPartyDocumentaryAddress)", true, supportedAddressTypes.Contains(DocAddressType.NotifyParty));
			AssertEquals("docAddresses.GetDocAddress(DocAddressType.NotifyPartyDocumentaryAddress) = declaration.NotifyPartyDocumentaryAddress", declaration.NotifyPartyDocumentaryAddress.DocAddressType, docAddresses.GetDocAddressRequirement(DocAddressType.NotifyParty).DefaultDocAddressType);
		}

		public void TestNotifyParty2DocumentaryAddress()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var notifyParty2Address = declaration.NotifyParty2DocumentaryAddress;
			AssertNotNull("docAddresses.GetDocAddress(DocAddressType.NotifyParty2DocumentaryAddress)", notifyParty2Address);
			AssertEquals(DocAddressType.NotifyParty2, notifyParty2Address.DocAddressType);
			AssertEquals(ContactType.NotifyParty, notifyParty2Address.DefaultContactType);
		}

		public void TestNotifyParty3DocumentaryAddress()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var notifyParty3Address = declaration.NotifyParty3DocumentaryAddress;
			AssertNotNull("docAddresses.GetDocAddress(DocAddressType.NotifyParty2DocumentaryAddress)", notifyParty3Address);
			AssertEquals(DocAddressType.NotifyParty3, notifyParty3Address.DocAddressType);
			AssertEquals(ContactType.NotifyParty, notifyParty3Address.DefaultContactType);
		}

		public void TestPackagesActualPackageCount()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var packGroup = declaration.PackingGroups.AddNew();
			var package1 = packGroup.Packages.AddNew();
			package1.CW_PackQty = 10;
			AssertEquals("Declaration.PackagesActualPackageCount", 10, (int)declaration.PackagesActualPackageCount);

			var package2 = packGroup.Packages.AddNew();
			package2.CW_PackQty = 23;
			AssertEquals("Declaration.PackagesActualPackageCount", 33, (int)declaration.PackagesActualPackageCount);
		}

		public void TestSettingEntryStatusDoesNotRemergeDeclaration()
		{
			SettingMergeAffectingFieldsShouldNotThrowExceptionOrDeveloperError(JobDeclarationSchema.Constants.JE_EntrySubmittedDate, ZDateTime.Now.AddDays(20));
			SettingMergeAffectingFieldsShouldNotThrowExceptionOrDeveloperError(JobDeclarationSchema.Constants.JE_EntryStatus, "AAA");
			SettingMergeAffectingFieldsShouldNotThrowExceptionOrDeveloperError(JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc, ZDateTime.Now.AddDays(20));
			SettingMergeAffectingFieldsShouldNotThrowExceptionOrDeveloperError(JobDeclarationSchema.Constants.JE_SystemCreateUser, "RG");
			SettingMergeAffectingFieldsShouldNotThrowExceptionOrDeveloperError(JobDeclarationSchema.Constants.JE_SystemLastEditTimeUtc, ZDateTime.Now.AddDays(10));
			SettingMergeAffectingFieldsShouldNotThrowExceptionOrDeveloperError(JobDeclarationSchema.Constants.JE_MessageStatus, "OK");
			SettingMergeAffectingFieldsShouldNotThrowExceptionOrDeveloperError(JobDeclarationSchema.Constants.JE_ConsolidatedCargoStatus, "CLR");
			SettingMergeAffectingFieldsShouldNotThrowExceptionOrDeveloperError(JobDeclarationSchema.Constants.JE_OperationalStatus, "WRK");
			SettingMergeAffectingFieldsShouldNotThrowExceptionOrDeveloperError(JobDeclarationSchema.Constants.JE_ConsolidatedCargoStatus, "WRK");
			SettingMergeAffectingFieldsShouldNotThrowExceptionOrDeveloperError(JobDeclarationSchema.Constants.JE_OverrideFreightDefaults, true);
			SettingMergeAffectingFieldsShouldNotThrowExceptionOrDeveloperError(JobDeclarationSchema.Constants.JE_RS_NKServiceLevel, "AAA");
		}

		[ExpectNoExceptions]
		void SettingMergeAffectingFieldsShouldNotThrowExceptionOrDeveloperError(ZString fieldName, object value)
		{
			SettingMergeAffectingFieldsWhenNoEntryHeader(fieldName, value);
			SettingMergeAffectingFieldsWhenThereAreMergedEntries(fieldName, value);
		}

		void SettingMergeAffectingFieldsWhenNoEntryHeader(ZString fieldName, object value)
		{
			ErrorReporter.Clear();
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			AssertEquals("No CusEntryHeaders expected", 0, declaration.CustomsEntryHeaders.Count);
			AssertNotEquals("Preconditions: New value should be different from the initial value", value, declaration[fieldName]);
			declaration[fieldName] = value;
			Factory.Save();
			AssertEquals("No Developer Error Expected when no CusEntryHeader", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		void SettingMergeAffectingFieldsWhenThereAreMergedEntries(ZString fieldName, object value)
		{
			ErrorReporter.Clear();
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var header = declaration.Invoices.AddNew();
			header.JobComInvoiceLines.AddNew();
			declaration.DoMerge();
			Factory.Save();
			AssertEquals("Preconditions: Non-zero EntryHeaders expected on Declaration", true, declaration.CustomsEntryHeaders.Count > 0);
			AssertEquals("Precondtions: No Developer Error Expected", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			AssertNotEquals("Preconditions: New value should be different from the initial value", value, declaration[fieldName]);
			declaration[fieldName] = value;
			Factory.Save();
			AssertEquals("No Developer Error Expected when there is at least one CusEntryHeader", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		public void TestSetMessageParentToJobDeclaration()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_ConsolidatedCargoStatus = "HLD";
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var discardedMessage1 = entryHeader.Messages.AddNew(typeof(EDIMessage));
			discardedMessage1.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + EDIMessage.SendersReferencePlaceHolder;
			var discardedMessage2 = entryHeader.Messages.AddNew(typeof(EDIMessage));
			discardedMessage2.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + EDIMessage.SendersReferencePlaceHolder;
			AssertEquals("2 entry header messages before method called", 2, entryHeader.Messages.Count);
			AssertEquals("No declaration messages before method called", 0, testDec.Messages.Count);
			Assert("Consolidated cargo status is not empty", !testDec.JE_ConsolidatedCargoStatus.IsEmpty);
			Assert("EM_LinkTable", discardedMessage1.EM_LinkTable == AutoCusEntryHeader.Schema.TableName);
			Assert("EM_LinkUniqueID", discardedMessage1.EM_LinkUniqueID == entryHeader.PK);
			Assert("EM_Status", discardedMessage1.EM_Status != EDIMessage.Status.Discarded);
			Assert("EM_LinkTable", discardedMessage2.EM_LinkTable == AutoCusEntryHeader.Schema.TableName);
			Assert("EM_LinkUniqueID", discardedMessage2.EM_LinkUniqueID == entryHeader.PK);
			Assert("EM_Status", discardedMessage2.EM_Status != EDIMessage.Status.Discarded);
			var packGroup = testDec.PackingGroups.AddNew();
			packGroup.CR_CargoStatus = "CLR";
			var pack = testDec.Packages.AddNew();
			pack.CW_CR_HouseContainer = packGroup.PK;
			var discardedMessage3 = packGroup.Messages.AddNew(typeof(EDIMessage));
			discardedMessage3.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + EDIMessage.SendersReferencePlaceHolder;
			Assert("EM_LinkTable", discardedMessage3.EM_LinkTable == AutoCusDecHouseContainerPivot.Schema.TableName);
			Assert("EM_LinkUniqueID", discardedMessage3.EM_LinkUniqueID == packGroup.PK);
			Assert("EM_Status", discardedMessage3.EM_Status != EDIMessage.Status.Discarded);
			Assert("CR_CargoStatus is not empty", !packGroup.CR_CargoStatus.IsEmpty);

			testDec.SetMessageParentToJobDeclaration();

			Assert("Consolidated cargo status is empty", testDec.JE_ConsolidatedCargoStatus.IsEmpty);
			AssertEquals("No entry header messages after method called", 0, entryHeader.Messages.Count);
			AssertEquals("No pack group messages after method called", 0, packGroup.Messages.Count);
			AssertEquals("3 declaration messages after method called", 3, testDec.Messages.Count);
			Assert("EM_LinkTable", discardedMessage1.EM_LinkTable == AutoJobDeclaration.Schema.TableName);
			Assert("EM_LinkUniqueID", discardedMessage1.EM_LinkUniqueID == testDec.PK);
			Assert("EM_Status", discardedMessage1.EM_Status == EDIMessage.Status.Discarded);
			Assert("EM_LinkTable", discardedMessage2.EM_LinkTable == AutoJobDeclaration.Schema.TableName);
			Assert("EM_LinkUniqueID", discardedMessage2.EM_LinkUniqueID == testDec.PK);
			Assert("EM_Status", discardedMessage2.EM_Status == EDIMessage.Status.Discarded);
			Assert("CR_CargoStatus is empty", packGroup.CR_CargoStatus.IsEmpty);
			Assert("EM_LinkTable", discardedMessage3.EM_LinkTable == AutoJobDeclaration.Schema.TableName);
			Assert("EM_LinkUniqueID", discardedMessage3.EM_LinkUniqueID == testDec.PK);
			Assert("EM_Status", discardedMessage3.EM_Status == EDIMessage.Status.Discarded);
		}

		public void TestTemplateCopyForStandAlone()
		{
			using (DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var testDec = BaseJobDeclaration.New(Factory);
				testDec.DisableDefaultPackingInformation = true;
				testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				testDec.AutoCreateChargesBasedOnIncoTerm = false;
				var container = testDec.CusContainers.AddNew();
				container.CO_ContainerNumber = "CRUX123456";

				var bill = testDec.Bills.AddNew();
				bill.CU_BillType = BillTypeList.Codes.HouseBill;
				bill.CU_HouseBill = "123Test";

				var packingGroup = bill.PackingGroups.AddNew();
				packingGroup.CR_CO_Container = container.PK;

				var package = packingGroup.Packages.AddNew();
				package.CW_PackQty = 10;
				package.CW_PackType = "PT";

				var groupHeader = testDec.JobComInvoiceGroupHeaders[0];
				var oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500m, testDec.LocalCurrencyCode);

				var invoice = testDec.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "1111";
				invoice.JZ_InvoiceAmount = 2000m;
				invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice.JZ_IncoTerm = "FOB";

				var oTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, testDec.LocalCurrencyCode);
				oTH.J7_IsIncludedInITOT = true;

				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 2000m;
				invoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 50m, testDec.LocalCurrencyCode);

				var decCopied = testDec.TemplateCopy() as BaseJobDeclaration;
				AssertEquals("Copied container", 0, decCopied.CusContainers.Count);
				AssertEquals("Copied house bill", 0, decCopied.Bills.Count);

				AssertEquals("Copied Invoice", 1, decCopied.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);
				AssertEquals("Copied invoice lines", 1, decCopied.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.Count);
				AssertEquals("Invoice charge copied", 1, decCopied.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].Charges.Count);
				AssertEquals("Invoice charge copied is OTH", CustomsChargeTypeList.Codes.OtherCharges, decCopied.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].Charges[0].J7_ChargeType);
				AssertEquals("Invoice line charge copied is EXW", CustomsChargeTypeList.Codes.ExWorks, decCopied.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].Charges[0].J7_ChargeType);

				AssertEquals("Invoices collection of jobdeclaration", 1, decCopied.Invoices.Count);
				AssertEquals("Invoice lines collection", 1, decCopied.InvoiceLines.Count);
				AssertEquals("Invoice lines collection", 1, decCopied.FilteredInvoiceLines.Count);
				AssertEquals("No apportioned charges", 0, decCopied.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].GroupCharges.Count);
				AssertEquals("No apportioned charges", 0, decCopied.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].ApportionedCharges.Count);

				decCopied.ResumeApportionment();
				AssertEquals("Invoice has apportioned charges from Group", 500m, decCopied.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].GroupCharges.GetCharge(oFT.ChargeKey).Amount);
				AssertEquals("Invoice line has an apportioned charge from group", 500m, decCopied.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].ApportionedCharges.GetCharge(oFT.ChargeKey).Amount);
				AssertEquals("Invoice line has an apportioned charge from Invoice", 100m, decCopied.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].ApportionedCharges.GetCharge(oTH.ChargeKey).Amount);
			}
		}

		public void TestTemplateCopyForPlugIn()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var testDec = BaseJobDeclaration.New(Factory);
			testDec.DisableDefaultPackingInformation = true;
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			testDec.JE_JS = shipment.PK;

			var shipmentCopied = shipment.TemplateCopy() as BusinessObject;
			var decCopied = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_JS, shipmentCopied.PK));

			AssertNotNull("Shipment copy -> Declaration copied", decCopied);
		}

		public void TestReDefaultTransportCompanyWhenTransportModeChanges()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var transportCo1 = Factory.NewWithValidTestData<OrgHeader>();
			var transportCo2 = Factory.NewWithValidTestData<OrgHeader>();
			var transportCo3 = Factory.NewWithValidTestData<OrgHeader>();

			importer.AllRelatedParties.SetRelatedParty(importer.MainAddress.PK, transportCo1, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.All, ZString.Empty);
			OrgRelatedPartyCompanySpecificCollection parties = importer.AllRelatedParties;

			var relatedParty2 = parties.AddNew();
			relatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			relatedParty2.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			relatedParty2.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			relatedParty2.PR_OH_RelatedParty = transportCo2.PK;
			relatedParty2.PR_OA = importer.Addresses[0].PK;

			var relatedParty3 = parties.AddNew();
			relatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			relatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			relatedParty3.PR_FreightTransportMode = Core.Constants.TransportModes.Air;
			relatedParty3.PR_OH_RelatedParty = transportCo3.PK;
			relatedParty3.PR_OA = importer.Addresses[0].PK;

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDeliveryAddress.E2_OA_Address = importer.MainAddress.PK;
			AssertEquals("Transport Provider should default related party for ALL as fallback", transportCo1.Addresses[0].PK, declaration.JE_OA_DeliveryOrPickupCartageCoAddr);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Should now re-default AIR relationship T/P", transportCo3.Addresses[0].PK, declaration.JE_OA_DeliveryOrPickupCartageCoAddr);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Should now re-default SEA relationship T/P", transportCo2.Addresses[0].PK, declaration.JE_OA_DeliveryOrPickupCartageCoAddr);
		}

		public void TestSettingSupplierOrImporterWillPopulateOriginOrDestinationInSubClass()
		{
			GlbCompany.CurrentCompany.SetCountry("US");
			var dec = Factory.New<BaseJobDeclaration>();
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_RL_NKClosestPort = "AUSYD";
			var importer = Factory.New<OrgHeader>();
			importer.OH_RL_NKClosestPort = "USLAX";
			dec.JE_OH_Supplier = supplier.PK;
			dec.JE_OH_Importer = importer.PK;
			AssertEquals("AUSYD", dec.JE_RL_NKOrigin);
			AssertEquals("USLAX", dec.JE_RL_NKFinalDestination);
		}

		public void TestInvoicesMentionNewOrInactiveProducts()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			testDec.Invoices.AddNew();
			testDec.InvoiceLines.AddNew();
			Assert("No products for save must be found", !testDec.InvoicesMentionNewOrInactiveProducts);
			var line = Factory.New<BaseJobComInvoiceLine>();
			line.JI_PartNo = "123";
			line.JI_CC = ZGuid.NewZGuid();
			line.JI_Description = "123";
			line.JI_InvoiceUQ = "KG";
			testDec.InvoiceLines.Add(line);
			Assert("Product for save must be found", testDec.InvoicesMentionNewOrInactiveProducts);

			// see also BaseInvoiceLineCompleteCollectionTest.TestAddNewPartsOrActivateInactiveOnes_MakeNew()
		}

		public void TestSaveNewProductsorActivateInactiveOnes()
		{
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "NEWCLASS";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			classification.CC_IsActive = true;
			Factory.Save();

			var testDec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			testDec.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = testDec.Invoices.AddNew();
			header.JZ_InvoiceNumber = "666";

			var line = testDec.InvoiceLines.AddNew();
			line.JI_JZ = header.PK;
			line.JI_PartNo = "NEWPART1";
			line.JI_CC = classification.PK;
			line.JI_Description = "666";
			line.JI_InvoiceUQ = "KG";

			var duplicates = testDec.SaveNewProductsorActivateInactiveOnes();
			AssertEquals(0, duplicates.Count);
			Factory.Save();

			Assert("Part must be created", Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "NEWPART1")) != null);

			line = testDec.InvoiceLines.AddNew();
			line.JI_JZ = header.PK;
			line.JI_PartNo = "NEWPART2";
			line.JI_Tariff = "84314905";
			line.JI_Description = "666";
			line.JI_InvoiceUQ = "KG";

			duplicates = testDec.SaveNewProductsorActivateInactiveOnes();
			AssertEquals(0, duplicates.Count);
			Factory.Save();

			Assert("Part must be created", Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "NEWPART2")) != null);

			// See also BaseInvoiceLineCompleteCollectionTest.TestAddNewPartsOrActivateInactiveOnes_ActivatesTheInactive()
		}

		public void TestSaveNewProductsorActivateInactiveOnes_WithDuplicate()
		{
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "NEWCLASS";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			classification.CC_IsActive = true;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "TESTORG";

			Factory.Save();

			var testDec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			testDec.JE_OH_Importer = importer.PK;
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = testDec.Invoices.AddNew();
			header.JZ_InvoiceNumber = "123";

			var line = testDec.InvoiceLines.AddNew();
			line.JI_JZ = header.PK;
			line.JI_PartNo = "DUPTEST";
			line.JI_CC = classification.PK;
			line.JI_Description = "123";
			line.JI_InvoiceUQ = "KG";

			// in this scenario another user creates product with same code and owner
			var existingProductPK = ZGuid.NewZGuid();
			var existingRelationPK = ZGuid.NewZGuid();
			TestConnection.ExecuteNonQuery($@"
				insert into dbo.OrgSupplierPart (OP_PK, OP_PartNum, OP_Desc)
				values ('{existingProductPK}', 'DUPTEST', 'Existing Product');
				insert into dbo.OrgPartRelation (OU_PK, OU_OP, OU_OH, OU_Relationship, OU_SystemCreateTimeUtc, OU_SystemCreateUser, OU_SystemLastEditTimeUtc, OU_SystemLastEditUser)
				values ('{existingRelationPK}', '{existingProductPK}', '{importer.PK}', 'OWN', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");

			var duplicates = testDec.SaveNewProductsorActivateInactiveOnes();
			AssertEquals("DUPTEST (Owner = TESTORG, Supplier = )", string.Join(", ", duplicates));

			AssertEquals(
				"existing product sholud now be visible to declaration factory, and no duplicate product should be created",
				"DUPTEST (Existing Product)",
				string.Join(", ", testDec.Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "DUPTEST")).Select(p => $"{p.OP_PartNum} ({p.OP_Desc})"))
			);
		}

		public void TestTopGroupInvoiceIsCreated()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(1, declaration.JobComInvoiceGroupHeaders.Count);
			AssertEquals("All Invoices", declaration.JobComInvoiceGroupHeaders[0].JZ_InvoiceNumber);
			AssertEquals("No changes", false, declaration.HasChanges);

			Factory.Save();
			AssertEquals("top group invoice is saved", true, declaration.JobComInvoiceGroupHeaders[0].IsInDatabase);

			var factory2 = new BusinessObjectFactory();
			var topGroupInvoiceLoaded = factory2.Load<BaseJobComInvoiceGroupHeader>(declaration.JobComInvoiceGroupHeaders[0].PK);
			AssertNotNull(topGroupInvoiceLoaded);
		}

		public void TestTopGroupInvoice_SupportAdditionalInvoices()
		{
			var declaration = Factory.New<JobDeclarationSupportAdditionalInvoices>();
			AssertEquals(1, declaration.JobComInvoiceGroupHeaders.Count);
			var topGroupInvoice = declaration.JobComInvoiceGroupHeaders[0];

			AssertEquals("TopGroupInvoice", topGroupInvoice, declaration.TopGroupInvoice);
			var groupHeader = Factory.New<JobComInvoiceGroupHeaderSupportAdditionalDeclarations>();
			groupHeader.AttachToAdditionalDeclaration(declaration);
			AssertEquals(2, declaration.JobComInvoiceGroupHeaders.Count);

			AssertEquals("TopGroupInvoice", topGroupInvoice, declaration.TopGroupInvoice);
		}

		public void TestIJobInvoicingPlugInDefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = Factory.New<BaseJobDeclaration>();
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent baseJobDeclaration = Factory.New<BaseJobDeclaration>();
			Assert(baseJobDeclaration.AllowInvoiceDeletion);
		}

		public void TestScreeningVesselWhenInactiveOnDeclarationPage_ShouldExists()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "MAERKS";
			vessel.RV_IsActive = false;

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = vessel.RV_Code;

			Factory.Save();

			var party = ((IScreeningPartyProvider)declaration).ScreeningParties;
			AssertEquals(vessel, party.Where(x => x.Description == "Vessel").FirstOrDefault().ScreeningEntity);
		}

		public void TestScreeningVesselWhenInactiveOnRoutingPage_ShouldExists()
		{
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Code = "MAERKS1";
			vessel1.RV_IsActive = false;

			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Code = "MAERKS2";
			vessel2.RV_IsActive = false;

			var vessel3 = Factory.NewWithValidTestData<RefVessel>();
			vessel3.RV_Code = "MAERKS3";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = vessel3.RV_Code;

			declaration.Transports.AddNew();
			declaration.Transports.AddNew();
			declaration.Transports.AddNew();

			declaration.Transports[0].JW_Vessel = vessel3.RV_Code;
			declaration.Transports[1].JW_Vessel = vessel1.RV_Code;
			declaration.Transports[2].JW_Vessel = vessel2.RV_Code;

			Factory.Save();

			var parties = ((IScreeningPartyProvider)declaration).ScreeningParties;

			AssertEquals(vessel1, parties.Where(x => x.Code == vessel1.RV_Code && x.Description == "Vessel").FirstOrDefault().ScreeningEntity);
			AssertEquals(vessel2, parties.Where(x => x.Code == vessel2.RV_Code && x.Description == "Vessel").FirstOrDefault().ScreeningEntity);
			AssertEquals(vessel3, parties.Where(x => x.Code == vessel3.RV_Code && x.Description == "Vessel").FirstOrDefault().ScreeningEntity);
		}

		public void TestIScreeningPartyProvider()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RN_NKCountryCode = "AU";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_GB = branch.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNotNull(declaration);

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryPickup = Factory.NewWithValidTestData<OrgHeader>();

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "hello";
			job.JH_ParentID = declaration.PK;
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Forwarder = forwarder.PK;
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			declaration.JE_OA_DeliveryOrPickupCartageCoAddr = deliveryPickup.MainAddress.PK;

			var shippingCompany = Factory.NewWithValidTestData<OrgHeader>();
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "hello";
			vessel.RV_OH = shippingCompany.PK;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = vessel.RV_Code;
			declaration.JE_VoyageFlightNo = "AA001";

			Factory.Save();

			var deniedCandidates = ((IScreeningPartyProvider)declaration).ScreeningParties;
			AssertContainsDeniedCandidate("AU - Local Client", localClient, deniedCandidates);
			AssertContainsDeniedCandidate("AU - Supplier", supplier, deniedCandidates);
			AssertContainsDeniedCandidate("AU - Importer", importer, deniedCandidates);
			AssertContainsDeniedCandidate("AU - Forwarder", forwarder, deniedCandidates);
			AssertContainsDeniedCandidate("AU - Carrier (Shipping Line)", shippingLine, deniedCandidates);
			AssertContainsDeniedCandidate("AU - Delivery/Pickup Port Transport Company", deliveryPickup, deniedCandidates);
			AssertEquals("Vessel", vessel, Array.Find(deniedCandidates, x => x.Description == "Vessel").Vessel);
			AssertContainsDeniedCandidate("Shipping Provider", shippingCompany, deniedCandidates);
			Assert("Routing Carrier", Array.FindAll(deniedCandidates, x => x.Description == "AU - Routing Carrier").Length > 0);
		}

		public void TestVesselWhenEmptyNameOnRouting_ShouldNotIncludedOnScreeningProcess()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var provider = declaration as IScreeningPartyProvider;
			var transport = declaration.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_Vessel = "";

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Pre-condition Declaration screening status", ScreeningStatusesList.Codes.NotScreened, declaration.JE_ScreeningStatus);
				AssertNull("Blank vessels name are excluded in screening parties", provider.ScreeningParties.SingleOrDefault(x => x.Description == "Vessel"));

				declaration.JE_ScreeningStatus = provider.GetWorstScreeningStatusUnlessManuallyCleared();
				AssertEquals("Declaration screening status should be CLR", ScreeningStatusesList.Codes.Clear, declaration.JE_ScreeningStatus);
			});
		}

		public void TestIRelatedOrgDeniedPartyScreenable()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RN_NKCountryCode = "AU";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_GB = branch.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryPickup = Factory.NewWithValidTestData<OrgHeader>();

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "hello";
			job.JH_ParentID = declaration.PK;
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Forwarder = forwarder.PK;
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			declaration.JE_OA_DeliveryOrPickupCartageCoAddr = deliveryPickup.MainAddress.PK;

			var shippingCompany = Factory.NewWithValidTestData<OrgHeader>();
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "hello";
			vessel.RV_OH = shippingCompany.PK;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = vessel.RV_Code;
			declaration.JE_VoyageFlightNo = "AA001";

			var statusLocalClient = CreateRelatedOrgPartyScreeningStatus(localClient.PK);
			var statusSupplier = CreateRelatedOrgPartyScreeningStatus(supplier.PK);
			var statusImporter = CreateRelatedOrgPartyScreeningStatus(importer.PK);
			var statusForwarder = CreateRelatedOrgPartyScreeningStatus(forwarder.PK);
			var statusShippingLine = CreateRelatedOrgPartyScreeningStatus(shippingLine.PK);
			var statusdeliveryPickup = CreateRelatedOrgPartyScreeningStatus(deliveryPickup.PK);
			var statusShippingProvider = CreateRelatedOrgPartyScreeningStatus(shippingCompany.PK);
			var statusRoutingCarrier = CreateRelatedOrgPartyScreeningStatus(declaration.Transports[0].Carrier.PK);
			var statusDeclaration = CreateRelatedOrgPartyScreeningStatus(declaration.PK, JobDeclarationSchema.Constants.Prefix);

			var relatedScreeningStatus = declaration.RelatedOrgPartyScreeningStatusCollection.ToList<IRelatedOrgPartyScreeningStatus>();

			CombineAssertions(() =>
			{
				AssertEquals(9, relatedScreeningStatus.Count);
				AssertRelatedScreeningStatusResult(localClient, statusLocalClient.PK, "AU - Local Client", relatedScreeningStatus);
				AssertRelatedScreeningStatusResult(supplier, statusSupplier.PK, "AU - Supplier Documentary Address|AU - Supplier Pickup/Delivery Address|AU - Supplier", relatedScreeningStatus);
				AssertRelatedScreeningStatusResult(importer, statusImporter.PK, "AU - Importer Documentary Address|AU - Importer Pickup/Delivery Address|AU - Importer", relatedScreeningStatus);
				AssertRelatedScreeningStatusResult(forwarder, statusForwarder.PK, "AU - Forwarder", relatedScreeningStatus);
				AssertRelatedScreeningStatusResult(shippingLine, statusShippingLine.PK, "AU - Carrier (Shipping Line)|AU - Routing Carrier", relatedScreeningStatus);
				AssertRelatedScreeningStatusResult(deliveryPickup, statusdeliveryPickup.PK, "AU - Delivery/Pickup Port Transport Company", relatedScreeningStatus);
				AssertRelatedScreeningStatusResult(shippingCompany, statusShippingProvider.PK, "Shipping Provider", relatedScreeningStatus);
				AssertRelatedScreeningStatusResult(declaration.Transports[0].Carrier, statusRoutingCarrier.PK, "AU - Carrier (Shipping Line)|AU - Routing Carrier", relatedScreeningStatus);
				AssertEquals(true, relatedScreeningStatus.Any(u => u.PJ_ParentID == declaration.PK && u.PK == statusDeclaration.PK));
			});
		}

		public void TestAddRemoveOrInsertPartyScreeningLogWhenSavingDeclarations()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RN_NKCountryCode = "AU";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Org1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "Org2";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_GB = branch.PK;
			Factory.Save();

			declaration.JE_OH_Forwarder = org1.PK;
			Factory.Save();

			var screeningStatus = declaration.RelatedOrgPartyScreeningStatusCollection.ToList<IRelatedOrgPartyScreeningStatus>();
			CombineAssertions(() =>
			{
				AssertEquals(1, screeningStatus.Count);
				AssertEquals(true, screeningStatus.Any(u => u.PJ_Status == "PAA" && u.PJ_ClearedReason == "Parties Info:Org1(AU - Forwarder)"));
			});

			declaration.JE_OH_Forwarder = org2.PK;
			Factory.Save();

			screeningStatus = declaration.RelatedOrgPartyScreeningStatusCollection.ToList<IRelatedOrgPartyScreeningStatus>();
			CombineAssertions(() =>
			{
				AssertEquals(true, screeningStatus.Any(u => u.PJ_Status == "PAA" && u.PJ_ClearedReason == "Parties Info:Org2(AU - Forwarder)"));
				AssertEquals(true, screeningStatus.Any(u => u.PJ_Status == "PAR" && u.PJ_ClearedReason == "Parties Info:Org1(AU - Forwarder)"));
			});
		}

		public void TestScreeningStatusUpdatedWithReplacedParty()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			var docAddressOrg = Factory.NewWithValidTestData<OrgHeader>();
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryPickup = Factory.NewWithValidTestData<OrgHeader>();

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_AddressOverride = false;
			docAddress.E2_ParentID = declaration.PK;
			docAddress.E2_ParentTableCode = "JE";
			docAddress.E2_OA_Address = docAddressOrg.MainAddress.PK;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentID = declaration.PK;
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			job.JH_JobNum = "ABC123";
			job.Parent = declaration;

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			declaration.JE_OH_Forwarder = forwarder.PK;
			declaration.JE_OA_DeliveryOrPickupCartageCoAddr = deliveryPickup.MainAddress.PK;

			Factory.Save();
			AssertEquals("Declaration's Screening Status", ScreeningStatusesList.Codes.NotScreened, declaration.JE_ScreeningStatus);

			var docAddressOrgNew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			docAddressOrgNew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			docAddress.E2_OA_Address = docAddressOrgNew.MainAddress.PK;
			Factory.Save();
			AssertEquals("Declaration's Screening Status", ScreeningStatusesList.Codes.Matched, declaration.JE_ScreeningStatus);
			docAddress.E2_OA_Address = docAddressOrg.MainAddress.PK;
			Factory.Save();
			AssertEquals("Declaration's Screening Status", ScreeningStatusesList.Codes.NotScreened, declaration.JE_ScreeningStatus);

			var localClientNew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			localClientNew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			job.JH_OA_LocalChargesAddr = localClientNew.MainAddress.PK;
			Factory.Save();
			AssertEquals("Declaration's Screening Status", ScreeningStatusesList.Codes.Matched, declaration.JE_ScreeningStatus);
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			Factory.Save();
			AssertEquals("Declaration's Screening Status", ScreeningStatusesList.Codes.NotScreened, declaration.JE_ScreeningStatus);

			var supplierNew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			supplierNew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			declaration.JE_OH_Supplier = supplierNew.PK;
			Factory.Save();
			AssertEquals("Declaration's Screening Status", ScreeningStatusesList.Codes.Matched, declaration.JE_ScreeningStatus);
			declaration.JE_OH_Supplier = supplier.PK;
			Factory.Save();
			AssertEquals("Declaration's Screening Status", ScreeningStatusesList.Codes.NotScreened, declaration.JE_ScreeningStatus);

			var importerNew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			importerNew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			declaration.JE_OH_Importer = importerNew.PK;
			Factory.Save();
			AssertEquals("Declaration's Screening Status", ScreeningStatusesList.Codes.Matched, declaration.JE_ScreeningStatus);
			declaration.JE_OH_Importer = importer.PK;
			Factory.Save();
			AssertEquals("Declaration's Screening Status", ScreeningStatusesList.Codes.NotScreened, declaration.JE_ScreeningStatus);

			var forwarderNew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			forwarderNew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			declaration.JE_OH_Forwarder = forwarderNew.PK;
			Factory.Save();
			AssertEquals("Declaration's Screening Status", ScreeningStatusesList.Codes.Matched, declaration.JE_ScreeningStatus);
			declaration.JE_OH_Forwarder = forwarder.PK;
			Factory.Save();
			AssertEquals("Declaration's Screening Status", ScreeningStatusesList.Codes.NotScreened, declaration.JE_ScreeningStatus);

			var shippingLineNew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			shippingLineNew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			declaration.JE_OH_ShippingLine = shippingLineNew.PK;
			Factory.Save();
			AssertEquals("Declaration's Screening Status", ScreeningStatusesList.Codes.Matched, declaration.JE_ScreeningStatus);
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			Factory.Save();
			AssertEquals("Declaration's Screening Status", ScreeningStatusesList.Codes.NotScreened, declaration.JE_ScreeningStatus);

			var deliveryPickupNew = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			deliveryPickupNew.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			declaration.JE_OA_DeliveryOrPickupCartageCoAddr = deliveryPickupNew.MainAddress.PK;
			Factory.Save();
			AssertEquals("Declaration's Screening Status", ScreeningStatusesList.Codes.Matched, declaration.JE_ScreeningStatus);
			declaration.JE_OA_DeliveryOrPickupCartageCoAddr = deliveryPickup.MainAddress.PK;
			Factory.Save();
			AssertEquals("Declaration's Screening Status", ScreeningStatusesList.Codes.NotScreened, declaration.JE_ScreeningStatus);
		}

		public void TestJE_ScreeningStatus()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			AssertEquals("JE_ScreeningStatus must be readonly", true, declaration.JE_ScreeningStatusInfo.ReadOnly);
		}

		public void TestJE_ContainerCountIsReadOnly()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			AssertEquals("JE_ContainerCount must be readonly", true, declaration.JE_ContainerCountInfo.ReadOnly);
		}

		public void TestNotes()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.MakeNonPersistent();
			declaration.SetShouldOverrideNotes(true);
			declaration.Notes.AddNew(true, "Customs Description", "note text");
			var notesFromDeclaration = declaration.Notes.FindByDescription("Customs Description");
			AssertEquals("No Invoices, Master for notes should be declaration", typeof(BaseJobDeclaration), notesFromDeclaration[0].Master.GetType());

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var fake = new FakeDeclarationCreatorForInvoice(invoice);
			var fakeDeclaration = (BaseJobDeclaration)fake.HeaderData;
			fakeDeclaration.Notes.AddNew(true, "Comm Invoice Custom Note", "some text");
			var notesFromInvoice = fakeDeclaration.Notes.FindByDescription("Comm Invoice Custom Note");
			AssertEquals("Master for notes should be Commercial Invoice", typeof(BaseJobComInvoiceHeader), notesFromInvoice[0].Master.GetType());

			var ordinaryDeclaration = Factory.New<BaseJobDeclaration>();
			ordinaryDeclaration.Notes.AddNew(true, "Customs Description", "note text test");
			ordinaryDeclaration.Notes.AddNew(true, "Customs Description 2", "note text test 2");
			notesFromDeclaration = ordinaryDeclaration.Notes.FindByDescription("Customs Description");
			AssertEquals("Persistent Declaration (not fake), Master for notes should be declaration", typeof(BaseJobDeclaration), notesFromDeclaration[0].Master.GetType());
		}

		public void TestOrganizationNotesWorksForContainerType()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var note = importer.Notes.AddNew();
			note.ST_NoteContext = "AAF";
			note.ST_Description = "Import Delivery Instructions";
			note.ST_NoteText = "Whoever is doing the review please enjoy and have a nice day!";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = "FCL";
			Factory.Save();

			AssertEquals("Declaration should have 1 note from importer showing.", 1, declaration.Notes.VisibleNotes.Count);
		}

		public void TestRelevantConsolItaly()
		{
			var (shipment, _, consolITtoDE) = SetupRelevantConsolTest();

			CombineAssertions(() =>
			{
				using (SetNewBranchAsTemporaryContext())
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					declaration.JE_JS = shipment.PK;
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertEquals("IT Import Relevant Consol", consolITtoDE, declaration.RelevantConsol);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertEquals("IT Export Relevant Consol: ITAAC - DEAAC", consolITtoDE, declaration.RelevantConsol);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Refund;
					AssertNull("No Relevant Consol for Misc declaration if more than one consol", declaration.RelevantConsol);
				}
			});
		}

		public void TestRelevantConsolGermany()
		{
			var (shipment, consolDEtoUS, consolITtoDE) = SetupRelevantConsolTest();

			CombineAssertions(() =>
			{
				using (SetNewBranchAsTemporaryContext())
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					declaration.JE_JS = shipment.PK;
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertEquals("DE Import Relevant Consol: ITAAC - DEAAC", consolITtoDE, declaration.RelevantConsol);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertEquals("DE Export Relevant Consol: DEAAA - USNYC", consolDEtoUS, declaration.RelevantConsol);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Refund;
					AssertNull("No Relevant Consol for Misc declaration if more than one consol", declaration.RelevantConsol);
				}
			});
		}

		public void TestRelevantConsolUnitedStates()
		{
			var (shipment, consolDEtoUS, _) = SetupRelevantConsolTest();

			CombineAssertions(() =>
			{
				using (SetNewBranchAsTemporaryContext())
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					declaration.JE_JS = shipment.PK;
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertEquals("US Import Relevant Consol: DEAAA - USNYC", consolDEtoUS, declaration.RelevantConsol);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertNull("US Export Relevant Consol", declaration.RelevantConsol);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Refund;
					AssertNull("No Relevant Consol for Misc declaration if more than one consol", declaration.RelevantConsol);

					shipment.Consols.RemoveAndDeleteAll();
					var consolUStoUS = AddConsol(shipment, "USMIA", "USCHI");
					AssertEquals("Relevant Consol for Misc declaration if one consol only", consolUStoUS, declaration.RelevantConsol);

					AddConsol(shipment, "DEAAA", "DEAAC");
					AssertNull("No Relevant Consol for Misc declaration if more than one consol", declaration.RelevantConsol);

					var consolUStoDE = AddConsol(shipment, "USNYC", "DEAAA");
					AddConsol(shipment, "USCHI", "USNYC");
					AddConsol(shipment, "DEAAC", "ITAAC");

					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertNull("US Import Relevant Consol", declaration.RelevantConsol);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertEquals("US Export Relevant Consol: USNYC - DEAAA", consolUStoDE, declaration.RelevantConsol);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Refund;
					AssertNull("No Relevant Consol for Misc declaration if more than one consol", declaration.RelevantConsol);
				}
			});
		}

		public void TestRelevantConsolGermanyUpdated()
		{
			var (shipment, _, _) = SetupRelevantConsolTest();

			CombineAssertions(() =>
			{
				using (SetNewBranchAsTemporaryContext())
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
				{
					shipment.Consols.RemoveAndDeleteAll();
					AddConsol(shipment, "USMIA", "USCHI");
					AddConsol(shipment, "DEAAA", "DEAAC");
					var consolUStoDE = AddConsol(shipment, "USNYC", "DEAAA");
					AddConsol(shipment, "USCHI", "USNYC");
					var consolDEtoIT = AddConsol(shipment, "DEAAC", "ITAAC");

					var declaration = Factory.New<BaseJobDeclaration>();
					declaration.JE_JS = shipment.PK;
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertEquals("DE Import Relevant Consol: USNYC - DEAAA", consolUStoDE, declaration.RelevantConsol);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertEquals("DE Export Relevant Consol: DEAAC - ITAAC", consolDEtoIT, declaration.RelevantConsol);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Refund;
					AssertNull("No Relevant Consol for Misc declaration if more than one consol", declaration.RelevantConsol);
				}
			});
		}

		(ForwardingShipment shipment, ForwardingConsol consolDEtoUS, ForwardingConsol consolITtoDE) SetupRelevantConsolTest()
		{
			var shipment = Factory.New<ForwardingShipment>();

			AddConsol(shipment, "USNYC", "USCHI");
			AddConsol(shipment, "DEAAC", "DEAAA");
			var consolDEtoUS = AddConsol(shipment, "DEAAA", "USNYC");
			var consolITtoDE = AddConsol(shipment, "ITAAC", "DEAAC");
			AddConsol(shipment, "USCHI", "USMIA");

			return (shipment, consolDEtoUS, consolITtoDE);
		}

		public void TestRelevantConsolAreMatchedCorrectlyBetweenUSAndPR()
		{
			const string port1 = "AUSYD";
			const string port2 = "USCHI";
			const string port3 = "PRBAS";

			var declaration = Factory.New<BaseJobDeclaration>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				declaration.JE_JS = shipment.PK;
				var consol = AddConsol(shipment, port1, port3);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("US Import Relevant Consol: AUSYD - PRBAS", consol, declaration.RelevantConsol);

				consol.JK_RL_NKLoadPort = port2;
				AssertEquals("US Export Relevant Consol: USCHI - PRBAS", consol, declaration.RelevantConsol);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				var shipment = Factory.New<ForwardingShipment>();
				declaration.JE_JS = shipment.PK;
				var consol = AddConsol(shipment, port1, port2);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("US Import Relevant Consol: AUSYD - USCHI", consol, declaration.RelevantConsol);

				consol.JK_RL_NKLoadPort = port3;
				AssertEquals("US Export Relevant Consol: PRBAS - USCHI", consol, declaration.RelevantConsol);
			}
		}

		public void TestServiceLevelIsDefaultedFromBuyerSupplierChanges()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_IsConsignor = true;
			supplier.OH_RL_NKClosestPort = "AUSYD";

			var importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			importer.OH_RL_NKClosestPort = "USLAX";
			importer.MiscServ.OM_RS_NKIMDefaultServiceLevel = "OBC";

			var supplierLink = importer.SupplierLinks.AddNew(supplier);
			supplierLink.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Canada;
			var supplierLinkTrnModeSEABKK = supplierLink.OrgSupBuyLinkTrnModes.AddNew();
			supplierLinkTrnModeSEABKK.PF_TransportMode = Core.Constants.TransportModes.Sea;
			supplierLinkTrnModeSEABKK.PF_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			supplierLinkTrnModeSEABKK.PF_RS_NKDefaultServiceLevel = "PRN";

			var supplierLinkTrnModeSEACNT = supplierLink.OrgSupBuyLinkTrnModes.AddNew();
			supplierLinkTrnModeSEACNT.PF_TransportMode = Core.Constants.TransportModes.Sea;
			supplierLinkTrnModeSEACNT.PF_ContainerMode = Core.Constants.ContainerModes.Containerised;
			supplierLinkTrnModeSEACNT.PF_RS_NKDefaultServiceLevel = "CTD";

			var supplierLinkTrnModeAIRCNT = supplierLink.OrgSupBuyLinkTrnModes.AddNew();
			supplierLinkTrnModeAIRCNT.PF_TransportMode = Core.Constants.TransportModes.Air;
			supplierLinkTrnModeAIRCNT.PF_ContainerMode = Core.Constants.ContainerModes.Containerised;
			supplierLinkTrnModeAIRCNT.PF_RS_NKDefaultServiceLevel = "ALS";

			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("STD", declaration.JE_RS_NKServiceLevel);
			AssertEquals(JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			AssertEquals(ZString.Empty, declaration.JE_TransportMode);
			AssertEquals(ZString.Empty, declaration.JE_ContainerMode);
			AssertEquals(ZString.Empty, declaration.JE_RL_NKFinalDestination);

			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("STD", declaration.JE_RS_NKServiceLevel);
			AssertEquals(ZString.Empty, declaration.JE_TransportMode);
			AssertEquals(ZString.Empty, declaration.JE_ContainerMode);
			AssertEquals(ZString.Empty, declaration.JE_RL_NKFinalDestination);

			declaration.JE_OH_Importer = importer.PK;
			AssertEquals(JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			AssertEquals("OBC", declaration.JE_RS_NKServiceLevel);
			AssertEquals(ZString.Empty, declaration.JE_TransportMode);
			AssertEquals(ZString.Empty, declaration.JE_ContainerMode);
			AssertEquals("USLAX", declaration.JE_RL_NKFinalDestination);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			AssertEquals("OBC", declaration.JE_RS_NKServiceLevel);
			AssertEquals(Core.Constants.TransportModes.Sea, declaration.JE_TransportMode);
			AssertEquals(ZString.Empty, declaration.JE_ContainerMode);
			AssertEquals("USLAX", declaration.JE_RL_NKFinalDestination);

			GlbCompany.CurrentCompany.SetCountry("US");
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("OBC", declaration.JE_RS_NKServiceLevel);
			AssertEquals(Core.Constants.TransportModes.Sea, declaration.JE_TransportMode);
			AssertEquals(ZString.Empty, declaration.JE_ContainerMode);
			AssertEquals("USLAX", declaration.JE_RL_NKFinalDestination);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("OBC", declaration.JE_RS_NKServiceLevel);
			AssertEquals(Core.Constants.TransportModes.Sea, declaration.JE_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.BreakBulk, declaration.JE_ContainerMode);
			AssertEquals("USLAX", declaration.JE_RL_NKFinalDestination);

			declaration.JE_RL_NKFinalDestination = "CATOR";
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("PRN", declaration.JE_RS_NKServiceLevel);
			AssertEquals(Core.Constants.TransportModes.Sea, declaration.JE_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.BreakBulk, declaration.JE_ContainerMode);
			AssertEquals("CATOR", declaration.JE_RL_NKFinalDestination);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("CTD", declaration.JE_RS_NKServiceLevel);
			AssertEquals(Core.Constants.TransportModes.Sea, declaration.JE_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.JE_ContainerMode);
			AssertEquals("CATOR", declaration.JE_RL_NKFinalDestination);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("ALS", declaration.JE_RS_NKServiceLevel);
			AssertEquals(Core.Constants.TransportModes.Air, declaration.JE_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.JE_ContainerMode);
			AssertEquals("CATOR", declaration.JE_RL_NKFinalDestination);
		}

		public void TestDeclarationCDArchiveInfo()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			var helper = new TestHelper();
			declaration.JE_OH_Importer = helper.Buyer.PK;
			declaration.JE_OH_Supplier = helper.Supplier.PK;

			var customsContainer = declaration.CusContainers.AddNew();
			customsContainer.CO_ContainerNumber = "123";

			var customsContainer2 = declaration.CusContainers.AddNew();
			customsContainer2.CO_ContainerNumber = "APLU123001";

			declaration.JE_RL_NKFinalDestination = "52000";
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-1);
			declaration.JE_VoyageFlightNo = "001Y";
			declaration.JE_HouseBill = "HouseBill1";
			declaration.JE_DeclarationReference = "B0001645";
			declaration.JE_MasterBill = "MasterBill1";
			declaration.JE_RL_NKOrigin = "NZAKL";
			declaration.JE_VesselName = "BUNGA DELIMA";

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			declaration.JE_OwnerRef = "Test Reference";
			declaration.DocsAndCartage.JP_OrderItemsAsString = "Order1, Order2";

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = "AAA";
			entry1.EntryNumber = "10000012";

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = "BBB";
			entry2.EntryNumber = "10000042";

			var job = new JobHeader.Loader(declaration).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var invoiceConsignee = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceConsignee.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoiceConsignee.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			invoiceConsignee.AH_OH = declaration.Importer.PK;
			invoiceConsignee.AH_TransactionNum = "00001001";
			invoiceConsignee.AH_RX_NKTransactionCurrency = declaration.LocalCurrencyCode;
			invoiceConsignee.AH_GB = GlbBranch.CurrentBranch.PK;
			invoiceConsignee.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			invoiceConsignee.AH_TransactionReference = "00001001";
			invoiceConsignee.AH_JH = job.PK;

			var declarationInfo = ((ICDArchive)declaration).CDArchiveInfo;
			AssertEquals("Consignee", helper.Buyer.OH_Code, declarationInfo.ConsigneeCode);
			AssertEquals("Consignor", helper.Supplier.OH_Code, declarationInfo.ConsignorCode);
			AssertEquals("Containers", "123, APLU123001", declarationInfo.ContainerNumbers);
			AssertEquals("Destination", "52000", declarationInfo.Destination);
			AssertEquals("EntryNumbers", "10000012, 10000012,10000042, 10000042", declarationInfo.EntryNumber);
			AssertEquals("ETA", ZDateTime.Today, declarationInfo.ETA);
			AssertEquals("ETD", ZDateTime.Today.AddDays(-1), declarationInfo.ETD);
			AssertEquals("HouseBill", "HouseBill1", declarationInfo.HouseBill);
			AssertEquals("InvoiceNumbers", "B0001645", declarationInfo.InvoiceNumbers);
			AssertEquals("JobNumber", "B0001645", declarationInfo.JobNumber);
			AssertEquals("MasterBill", "MasterBill1", declarationInfo.MasterBill);
			AssertEquals("OrderNumbers", "Order1, Order2, Test Reference", declarationInfo.OrderNumbers);
			AssertEquals("Origin", "NZAKL", declarationInfo.Origin);
			AssertEquals("Vessel", "BUNGA DELIMA", declarationInfo.Vessel);
			AssertEquals("VoyageFlight", "001Y", declarationInfo.VoyageFlight);
		}

		public void TestDeclarationMessagesHaveBeenSent_HasMessage()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var message = declaration.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertEquals(true, declaration.DeclarationMessagesHaveBeenSent());
		}

		public void TestDeclarationMessagesHaveBeenSent_HasMessage_IsDeclarationIntegrated()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
			using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var message = declaration.Messages.AddNew();
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				AssertEquals(false, declaration.DeclarationMessagesHaveBeenSent());
			}
		}

		public void TestDeclarationMessagesHaveBeenSent_NoMessage_NotBulitinCountry_IsDeclarationIntegrated()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
			using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				AssertEquals(false, declaration.DeclarationMessagesHaveBeenSent());
			}
		}

		public void TestDeclarationMessagesHaveBeenSent_NoMessage_NotBulitinCountry_NotIsDeclarationIntegrated()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Switzerland))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				AssertEquals(false, declaration.DeclarationMessagesHaveBeenSent());
			}
		}

		public void TestDeclarationMessagesHaveBeenSent_NoMessage_BulitinCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				AssertEquals(false, declaration.DeclarationMessagesHaveBeenSent());
			}
		}

		public void TestDeclarationMessagesHaveBeenSent_ReloadMessagesInDB()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var message = declaration.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			AssertEquals(true, declaration.DeclarationMessagesHaveBeenSent(true));
			AssertEquals(1, ((IBusinessObjectInternals)message).ParentCollections.Length);
		}

		public void TestActiveGroupHeaderWhenBaseJobComInvoiceGroupHeaderCollectionIsEmpty()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(1, declaration.JobComInvoiceGroupHeaders.Count);

			declaration.JobComInvoiceGroupHeaders.RemoveAndDeleteAll();
			AssertEquals(0, declaration.JobComInvoiceGroupHeaders.Count);

			AssertNotNull(declaration.ActiveGroupHeader);
		}

		public void TestMessageStatusForShouldCombineMessageStatusForHeaders()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				Assert(!declaration.IsDeclarationIntegrated);
				Assert(!DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(declaration.CountryCode, declaration.JE_GC));

				declaration.JE_MessageStatus = "CDR";
				AssertEquals("DEPREC Replaced", declaration.JE_MessageStatusDescription);

				var entryHeader0 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader0.CH_Status = "CDW";
				AssertEquals("DEPREC Replaced", declaration.JE_MessageStatusDescription);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				Assert(DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(declaration.CountryCode, declaration.JE_GC));

				declaration.JE_MessageStatus = ZAMessageStatusList.Codes.AwaitingResponse;
				AssertEquals("Awaiting Response", declaration.JE_MessageStatusDescription);
				declaration.JE_MessageStatus = ZAMessageStatusList.Codes.Acknowledged;
				AssertEquals("Acknowledged", declaration.JE_MessageStatusDescription);

				var entryHeader0 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader0.CH_Status = ZAMessageStatusList.Codes.AwaitingResponse;
				AssertEquals("Awaiting Response", declaration.JE_MessageStatusDescription);
				AssertEquals("AWA", declaration.JE_MessageStatus);

				var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader1.CH_Status = ZAMessageStatusList.Codes.AwaitingResponse;
				AssertEquals("Awaiting Response", declaration.JE_MessageStatusDescription);
				AssertEquals("AWA", declaration.JE_MessageStatus);

				entryHeader1.CH_Status = ZAMessageStatusList.Codes.Acknowledged;
				AssertEquals(BaseJobDeclaration.MultipleMessageStatusWithSameCategory("Awaiting"), declaration.JE_MessageStatusDescription);
				AssertEquals(CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.JE_MessageStatus);

				entryHeader1.CH_Status = ZAMessageStatusList.Codes.Error;
				AssertEquals(BaseJobDeclaration.MultipleMessageStatusWithSameCategory("Awaiting"), declaration.JE_MessageStatusDescription);
				AssertEquals(CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.JE_MessageStatus);

				entryHeader0.CH_Status = ZAMessageStatusList.Codes.Acknowledged;
				AssertEquals(BaseJobDeclaration.MultipleMessageStatusWithSameCategory("Error"), declaration.JE_MessageStatusDescription);
				AssertEquals(CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.JE_MessageStatus);

				entryHeader1.CH_Status = "SNT";
				AssertEquals(CommonEntryStatusList.Descriptions.MultipleEntryStatus, declaration.JE_MessageStatusDescription);
				AssertEquals(CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.JE_MessageStatus);

				entryHeader0.CH_Status = ZAMessageStatusList.Codes.NotSent;
				entryHeader1.CH_Status = ZAMessageStatusList.Codes.NotSent;
				AssertEquals("Acknowledged", declaration.JE_MessageStatusDescription);
				AssertEquals("ACK", declaration.JE_MessageStatus);
			}
		}

		public void TestDeclarationNumberCaptions() => CombineAssertions(() =>
			AssertEntity<BaseJobDeclaration>()
				.HasProperty(x => x.DeclarationNumber)
				.WithCaption("Entry Number"));

		public void TestEntryStatusDescriptionCaptions() => CombineAssertions(() =>
			AssertEntity<BaseJobDeclaration>()
				.HasProperty(x => x.JE_EntryStatusDescription)
				.WithCaption("Status"));

		public void TestEntryStatusForShouldReadEntryStatusFromHeaders()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_EntryStatus = "WTC";
				AssertEquals("Awaiting Response for Create", declaration.JE_EntryStatusDescription);

				var entryHeader0 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader0.CH_EntryStatus = "FLC";
				AssertEquals("Awaiting Response for Create", declaration.JE_EntryStatusDescription);
				AssertEquals("WTC", declaration.JE_EntryStatus);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var universalReferenceTestHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

				universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CustomsStatus");
				var za6 = universalReferenceTestHelper.CreateCusCodeList("ZA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "6", "Reject To Clearer", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				universalReferenceTestHelper.CreateCusCodeListAttribute(za6.PK, "CustomsRejected", "true");

				var za1 = universalReferenceTestHelper.CreateCusCodeList("ZA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "1", "Release", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				universalReferenceTestHelper.CreateCusCodeListAttribute(za1.PK, "CustomsCleared", "true");

				var za2 = universalReferenceTestHelper.CreateCusCodeList("ZA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "2", "Stop/Detain", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				universalReferenceTestHelper.CreateCusCodeListAttribute(za2.PK, "IAllowCancel", "true");

				Factory.Save();

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_EntryStatus = "6";
				AssertEquals("Reject To Clearer", declaration.JE_EntryStatusDescription);
				declaration.JE_EntryStatus = "1";
				AssertEquals("Release", declaration.JE_EntryStatusDescription);

				var entryHeader0 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader0.CH_EntryStatus = "2";
				AssertEquals("Stop/Detain", declaration.JE_EntryStatusDescription);
				AssertEquals("2", declaration.JE_EntryStatus);

				var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader1.CH_EntryStatus = "6";
				AssertEquals(CommonEntryStatusList.Descriptions.MultipleEntryStatus, declaration.JE_EntryStatusDescription);
				AssertEquals(CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.JE_EntryStatus);

				entryHeader1.CH_EntryStatus = ZString.Empty;
				AssertEquals(CommonEntryStatusList.Descriptions.MultipleEntryStatus, declaration.JE_EntryStatusDescription);
				AssertEquals(CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.JE_EntryStatus);

				entryHeader0.CH_EntryStatus = ZString.Empty;
				entryHeader1.CH_EntryStatus = ZString.Empty;
				AssertEquals("Release", declaration.JE_EntryStatusDescription);
			}
		}

		public void TestEntryStatusForIntegratedCountry()
		{
			using (TemporarilySetInterfacedCountry(Core.Constants.CountryCodes.China))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_EntryStatus = "ACK";
				AssertEquals("Acknowledged", declaration.JE_EntryStatusDescription);
				declaration.JE_EntryStatus = "SUB";
				AssertEquals("Submitted", declaration.JE_EntryStatusDescription);
				var entryHeader0 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader0.CH_EntryStatus = "DUT";
				AssertEquals("Duty Calculated", declaration.JE_EntryStatusDescription);

				var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader1.CH_EntryStatus = "CSN";
				AssertEquals("Multiple - See Entries", declaration.JE_EntryStatusDescription);

				entryHeader0.CH_EntryStatus = ZString.Empty;
				entryHeader1.CH_EntryStatus = ZString.Empty;
				AssertEquals("Submitted", declaration.JE_EntryStatusDescription);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_EntryStatus = "ACK";
				AssertEquals("Unknown", declaration.JE_EntryStatusDescription);
				declaration.JE_EntryStatus = "SUB";
				AssertEquals("Unknown", declaration.JE_EntryStatusDescription);
			}

			using (TemporarilySetInterfacedCountry(Core.Constants.CountryCodes.Belgium))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_EntryStatus = "ACK";
				AssertEquals("Acknowledged", declaration.JE_EntryStatusDescription);
				var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
				DataRegistry.Business.CustomsDataRegistry.Instance.CustomsWareCompany.SetValue(companyPK, Guid.Empty, Guid.Empty, "TST");
				var customsWareRegistry = ObjectFactory.Get<Integration.Customs.CustomsWare.ICustomsWareRegistry>();
				customsWareRegistry.CustomsWareSiteID.SetValue(companyPK, Guid.Empty, Guid.Empty, "TSTSITE");
				customsWareRegistry.UserName.SetValue(companyPK, Guid.Empty, Guid.Empty, "TSTUN");
				customsWareRegistry.Password.SetValue(companyPK, Guid.Empty, Guid.Empty, "TSTPW");
				AssertEquals("Message confirmed received by party responsible for delivering message to final destination.", declaration.JE_EntryStatusDescription);
				declaration.JE_EntryStatus = "SUB";
				AssertEquals("Submitted to CustomsWare", declaration.JE_EntryStatusDescription);
			}
		}

		public void TestSaving_OnSaving_StandAloneDeclaration_AddContainer_TriggersContainerSubscription()
		{
			// Arrange
			using (FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var eventReference = StmALog.GenerateEventReference("", new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("TYP", "Container Tracking") });
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var shippingLine = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_MasterBill = "12345678";
				declaration.JE_OH_ShippingLine = shippingLine.PK;

				// Act
				var customsContainer = declaration.CusContainers.AddNew();
				customsContainer.CO_ContainerNumber = "AAAA1111113";
				Factory.Save();

				// Assert
				var declarationLog1 = declaration.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				var commonContainer = customsContainer.JobContainer;
				var forwardingContainerLog1 = commonContainer.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				var cusContainerLog1 = customsContainer.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertNotNull("Declaration Log 1", declarationLog1);
				AssertNull("Forwarding Container Log 1", forwardingContainerLog1);
				AssertNull("CusContainer Log", cusContainerLog1);

				// Act
				declaration.JE_MasterBill = "23456789";
				Factory.Save();

				// Assert
				var declarationLog2 = declaration.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertNotNull("Declaration Log 2", declarationLog2);
				Assert(declarationLog2.SL_EventTime > declarationLog1.SL_EventTime);

				// Act
				customsContainer.CO_ContainerNumber = "AAAA2222220";
				Factory.Save();

				// Assert
				var declarationLog3 = declaration.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertNotNull("Declaration Log 3", declarationLog3);
				Assert(declarationLog3.SL_EventTime > declarationLog2.SL_EventTime);
			}
		}

		public void TestSaving_OnSaving_StandAloneDeclaration_DeleteContainer_TriggersContainerSubscription()
		{
			// Arrange
			using (FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var eventReference = StmALog.GenerateEventReference("", new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("TYP", "Container Tracking") });
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var shippingLine = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_MasterBill = "12345678";
				declaration.JE_OH_ShippingLine = shippingLine.PK;
				var customsContainer = declaration.CusContainers.AddNew();
				customsContainer.CO_ContainerNumber = "AAAA1111113";
				Factory.Save();

				var declarationLog1 = declaration.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertNotNull("Declaration Log 1", declarationLog1);

				// Act
				customsContainer.Delete();
				Factory.Save();

				// Assert
				var declarationLog2 = declaration.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertNotNull("Declaration Log 2", declarationLog2);
				Assert(declarationLog2.SL_EventTime > declarationLog1.SL_EventTime);
			}
		}

		public void TestSaving_OnSaving_ShipmentDeclaration_AddContainer_NoContainerSubscription()
		{
			// Arrange
			using (FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var eventReference = StmALog.GenerateEventReference("", new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("TYP", "Container Tracking") });

				var consol = Factory.New<ForwardingConsol>();
				var forwardingContainer = consol.Containers.AddNew();
				forwardingContainer.JC_ContainerNum = "ContainerNum111";
				var shipment = consol.Shipments.AddNew();

				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var shippingLine = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "12345", Core.Constants.CountryCodes.UnitedStates);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_MasterBill = "1111";
				declaration.JE_OH_ShippingLine = shippingLine.PK;

				// Act
				var customsContainer = declaration.CusContainers.AddNew();
				customsContainer.CO_ContainerNumber = "ContainerNum111";

				declaration.JE_JS = shipment.PK;
				customsContainer.CO_JC = forwardingContainer.PK;
				Factory.Save();

				var cusContainerLog = customsContainer.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertNull("CustomsContainer Log", cusContainerLog);
				var forwardingContainerLog = forwardingContainer.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertNull("ForwardingContainer Log", forwardingContainerLog);

				// Assert
				var declarationLog1 = declaration.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertNull("Declaration Log 1", declarationLog1);
			}
		}

		public void TestSaving_OnSaving_ShipmentDeclaration_DeleteContainer_NoContainerSubscription()
		{
			// Arrange
			using (FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var eventReference = StmALog.GenerateEventReference("", new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("TYP", "Container Tracking") });

				var consol = Factory.New<ForwardingConsol>();
				var forwardingContainer = consol.Containers.AddNew();
				forwardingContainer.JC_ContainerNum = "ContainerNum111";
				var shipment = consol.Shipments.AddNew();

				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var shippingLine = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "12345", Core.Constants.CountryCodes.UnitedStates);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_MasterBill = "1111";
				declaration.JE_OH_ShippingLine = shippingLine.PK;
				var customsContainer = declaration.CusContainers.AddNew();
				customsContainer.CO_ContainerNumber = "ContainerNum111";

				declaration.JE_JS = shipment.PK;
				customsContainer.CO_JC = forwardingContainer.PK;
				Factory.Save();

				var declarationLog1 = declaration.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertNull("Declaration Log 1", declarationLog1);

				// Act
				customsContainer.Delete();
				Factory.Save();

				// Assert
				var declarationLog2 = declaration.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertNull("Declaration Log 2", declarationLog2);
			}
		}

		public void TestCreateSBRWhenRoutingIsUpdated()
		{
			using (FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var shippingLine = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_MasterBill = "12345678";
				declaration.JE_OH_ShippingLine = shippingLine.PK;

				var transport = declaration.Transports.AddNew("AUSYD", "SGSIN");
				Factory.Save();
				AssertEventNumber("1 uncancelled SBR event on custom declaration, 0 cancelled SBR event on custom declaration", declaration, 1, 0);

				declaration.JE_CustomsLoadPort = "AUMEL";
				transport.JW_RL_NKLoadPort = "AUMEL";
				transport.JW_RL_NKDiscPort = "NZAKL";
				Factory.Save();
				AssertEventNumber("1 uncancelled SBR event on custom declaration, 1 cancelled SBR event on custom declaration", declaration, 1, 1);

				var factory = new BusinessObjectFactory();
				var declaration2 = factory.Load<BaseJobDeclaration>(declaration.PK);
				declaration2.JE_CustomsLoadPort = "AUBNE";
				declaration2.Transports.RemoveAll();
				factory.Save();
				AssertEventNumber("1 uncancelled SBR event on custom declaration, 2 cancelled SBR events on custom declaration", declaration2, 1, 2);
			}
		}

		static void AssertEventNumber(string message, BaseJobDeclaration declaration, int expectedUncancelledEventNumber, int expectedCancelledEventNumber)
		{
			AssertEquals(
				message,
				expectedUncancelledEventNumber,
				declaration.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code && !x.IsCancelled));
			AssertEquals(
				message,
				expectedCancelledEventNumber,
				declaration.Logs.GetAllLogs().Cast<StmALog>()
					.Count(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code && x.IsCancelled));
		}

		public void TestIContainerTrackingProvider_ArrivalDepartureDates()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var containerTrackingProvider = declaration as IContainerTrackingProvider;

			var transport1 = declaration.Transports.AddNew();
			transport1.JW_ETA = ZDateTime.Today.AddDays(-10);

			declaration.JE_DateAtFinalDestination = ZDateTime.Today.AddDays(-5);
			AssertEquals(ZDateTime.Today.AddDays(-5), containerTrackingProvider.GetLastArrivalDate());

			declaration.JE_DateAtFinalDestination = ZDateTime.Empty;
			AssertEquals(ZDateTime.Today.AddDays(-10), containerTrackingProvider.GetLastArrivalDate());

			var transport2 = declaration.Transports.AddNew();
			transport2.JW_ETA = ZDateTime.Today.AddDays(-7);
			AssertEquals(ZDateTime.Today.AddDays(-7), containerTrackingProvider.GetLastArrivalDate());

			declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			containerTrackingProvider = declaration;

			transport1 = declaration.Transports.AddNew();
			transport1.JW_ETD = ZDateTime.Today.AddDays(-10);

			declaration.JE_DateAtOrigin = ZDateTime.Today.AddDays(-5);
			AssertEquals(ZDateTime.Today.AddDays(-10), containerTrackingProvider.GetFirstDepartureDate());

			transport1.JW_ETD = ZDateTime.Empty;
			AssertEquals(ZDateTime.Today.AddDays(-5), containerTrackingProvider.GetFirstDepartureDate());

			transport1.JW_ETD = ZDateTime.Today.AddDays(-10);

			transport2 = declaration.Transports.AddNew();
			transport2.JW_ETD = ZDateTime.Today.AddDays(-7);
			AssertEquals(ZDateTime.Today.AddDays(-10), containerTrackingProvider.GetFirstDepartureDate());
		}

		public void TestIContainerTrackingProvider_RoutingLegsHaveChanges()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var containerTrackingProvider = declaration as IContainerTrackingProvider;
			AssertEquals(false, containerTrackingProvider.RoutingLegsHaveChanges);

			var transport = declaration.Transports.AddNew("AUSYD", "SGSIN");
			AssertEquals(true, containerTrackingProvider.RoutingLegsHaveChanges);
			Factory.Save();
			transport.JW_RL_NKLoadPort = "SGSIN";
			AssertEquals(true, containerTrackingProvider.RoutingLegsHaveChanges);
			Factory.Save();
			transport.JW_RL_NKDiscPort = "AUMEL";
			AssertEquals(true, containerTrackingProvider.RoutingLegsHaveChanges);
			Factory.Save();
			transport.JW_VoyageFlight = "122";
			AssertEquals(true, containerTrackingProvider.RoutingLegsHaveChanges);
			Factory.Save();
			transport.JW_VoyageFlight = "122S";
			AssertEquals(true, containerTrackingProvider.RoutingLegsHaveChanges);
			Factory.Save();
			transport.JW_Vessel = "VESSEL1";
			AssertEquals(true, containerTrackingProvider.RoutingLegsHaveChanges);
			Factory.Save();
			declaration.Transports.Remove(transport);
			AssertEquals(true, containerTrackingProvider.RoutingLegsHaveChanges);
		}

		public void TestGetDiscardedMessagesFilter()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var message1 = Factory.New<EDIMessage>();
			message1.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			message1.EM_LinkUniqueID = declaration.PK;
			AssertEquals(1, declaration.DiscardedMessages.Count);

			var declaration2 = Factory.New<BaseJobDeclarationForTesting>();
			var message2 = Factory.New<EDIMessage>();
			message2.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			message2.EM_LinkUniqueID = declaration2.PK;
			var message3 = Factory.New<EDIMessage>();
			message3.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			message3.EM_LinkUniqueID = declaration2.PK;
			message3.EM_ApplicationCode = "ZZZ";
			AssertEquals(1, declaration2.DiscardedMessages.Count);
			AssertEquals(message3.PK, declaration2.DiscardedMessages.OfType<EDIMessage>().FirstOrDefault()?.PK);
		}

		public void TestUpdateDeliverDates_DCFEventAdded()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMP";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUP";
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			Factory.Save();

			declaration.GetLogs().AddNew(Events.DeliveryCartageCompleteFinalised, new ZDateTimeOffset(2016, 12, 9), true);
			Factory.Save();
			AssertEquals(new ZDateTime(2016, 12, 9), declaration.JE_EstimatedDeliveryOrPickup);

			declaration.GetLogs().AddNew(Events.DeliveryCartageCompleteFinalised, new ZDateTimeOffset(2016, 12, 10), false);
			Factory.Save();
			AssertEquals(new ZDateTime(2016, 12, 10), declaration.JE_CartageCompleted);
		}

		public void TestUpdatePickupDates_PCFEventAdded()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMP";
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUP";
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			Factory.Save();

			declaration.GetLogs().AddNew(Events.PickupCartageCompleteFinalised, new ZDateTimeOffset(2016, 12, 9), true);
			Factory.Save();
			AssertEquals(new ZDateTime(2016, 12, 9), declaration.JE_EstimatedDeliveryOrPickup);

			declaration.GetLogs().AddNew(Events.PickupCartageCompleteFinalised, new ZDateTimeOffset(2016, 12, 10), false);
			Factory.Save();
			AssertEquals(new ZDateTime(2016, 12, 10), declaration.JE_CartageCompleted);
		}

		public void TestDPSFreightMovementRestricted()
		{
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				AssertEquals("Not Restricted", false, ((ICreditControlledDocumentDelivery)declaration).IsDPSFreightMovementRestricted);

				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				AssertEquals("Restricted", true, ((ICreditControlledDocumentDelivery)declaration).IsDPSFreightMovementRestricted);

				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
				AssertEquals("Restricted", true, ((ICreditControlledDocumentDelivery)declaration).IsDPSFreightMovementRestricted);

				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				AssertEquals("Restricted", true, ((ICreditControlledDocumentDelivery)declaration).IsDPSFreightMovementRestricted);
			}

			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.Exp))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Prereq - Must be Import", true, declaration.IsImport);

				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				AssertEquals("Not Restricted", false, ((ICreditControlledDocumentDelivery)declaration).IsDPSFreightMovementRestricted);

				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				AssertEquals("Not Restricted", false, ((ICreditControlledDocumentDelivery)declaration).IsDPSFreightMovementRestricted);

				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
				AssertEquals("Not Restricted", false, ((ICreditControlledDocumentDelivery)declaration).IsDPSFreightMovementRestricted);

				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				AssertEquals("Not Restricted", false, ((ICreditControlledDocumentDelivery)declaration).IsDPSFreightMovementRestricted);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Prereq - Must be Export", true, declaration.IsExport);

				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				AssertEquals("Not Restricted", false, ((ICreditControlledDocumentDelivery)declaration).IsDPSFreightMovementRestricted);

				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				AssertEquals("Restricted", true, ((ICreditControlledDocumentDelivery)declaration).IsDPSFreightMovementRestricted);

				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
				AssertEquals("Restricted", true, ((ICreditControlledDocumentDelivery)declaration).IsDPSFreightMovementRestricted);

				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				AssertEquals("Restricted", true, ((ICreditControlledDocumentDelivery)declaration).IsDPSFreightMovementRestricted);
			}
		}

		public void TestJobHeaderIsNotDeactivatedWhenFactoryHasInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(true);
		}

		public void TestJobHeaderIsDeactivatedWhenFactoryDoesNotHaveInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(false);
		}

		public void TestAddRulesToNotes()
		{
			CreateRuleNotes("AU", "DE", "This is a client visible Notes", true);
			CreateRuleNotes("AU", "DE", "This is an internal Notes", false);

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "AUBNE";
			declaration.JE_RL_NKFinalDestination = "DEHAM";
			Factory.Save();

			Assert(declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRules.Description).FirstOrDefault().ST_NoteText.Contains("This is a client visible Notes"));
			Assert(declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesInternal.Description).FirstOrDefault().ST_NoteText.Contains("This is an internal Notes"));
		}

		public void TestRuleNotesShouldNotBeAddedWhenNotesAreNotVisible()
		{
			CreateRuleNotes("AU", "DE", "This is a client visible Notes", true);
			CreateRuleNotes("AU", "DE", "This is an internal Notes", false);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "AUBNE";
			declaration.JE_RL_NKFinalDestination = "DEHAM";
			declaration.JE_JS = shipment.PK;
			Factory.Save();

			AssertNull(declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRules.Description).FirstOrDefault());
			AssertNull(declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesInternal.Description).FirstOrDefault());
		}

		public void TestJE_GBChange()
		{
			var comapny = Factory.NewWithValidTestData<GlbCompany>();
			var branch = comapny.Branches.AddNew();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			AssertNotEquals(comapny.PK, declaration.JE_GC);

			declaration.JE_GB = branch.PK;
			AssertEquals(comapny.PK, declaration.JE_GC);
		}

		public void TestJE_MessageType_DefaultExternalBroker()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var orgRelatedParty = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateOrgRelatedParty(Factory, organisation, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, "USLAX");
			var declaration = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateDeclarationForRelatedParty(Factory, ZString.Empty, "AUSYD", "USLAX", organisation.PK, organisation.PK);
			AssertMessageTypeDefaultsRelatedParty(CustomsDataRegistry.Instance.RelatedPartyDefaultingExternalBroker, declaration, organisation, declaration.JE_OH_ExternalBrokerInfo, orgRelatedParty.PK);
		}

		public void TestJE_MessageType_DefaultDepot()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var orgRelatedParty = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateOrgRelatedParty(Factory, organisation, RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Delivery, "USLAX");
			var declaration = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateDeclarationForRelatedParty(Factory, ZString.Empty, "AUSYD", "USLAX", organisation.PK, organisation.PK);
			AssertMessageTypeDefaultsRelatedParty(CustomsDataRegistry.Instance.RelatedPartyDefaultingDepot, declaration, organisation, declaration.DepotDocAddress.OrganisationPKInfo, orgRelatedParty.PK);
		}

		public void TestJE_OH_Importer_DefaultExternalBroker()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var orgRelatedParty = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateOrgRelatedParty(Factory, organisation, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, "USLAX");
			var declaration = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Import, "AUSYD", "USLAX", ZGuid.Empty, ZGuid.Empty);
			AssertImporterDefaultsRelatedParty(CustomsDataRegistry.Instance.RelatedPartyDefaultingExternalBroker, declaration, organisation, declaration.JE_OH_ExternalBrokerInfo, orgRelatedParty.PK);
		}

		public void TestJE_OH_Importer_DefaultDepot()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var orgRelatedParty = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateOrgRelatedParty(Factory, organisation, RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Delivery, "USLAX");
			var declaration = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Import, "AUSYD", "USLAX", ZGuid.Empty, ZGuid.Empty);
			AssertImporterDefaultsRelatedParty(CustomsDataRegistry.Instance.RelatedPartyDefaultingDepot, declaration, organisation, declaration.DepotDocAddress.OrganisationPKInfo, orgRelatedParty.PK);
		}

		public void TestJE_OH_Supplier_DefaultExternalBroker()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var orgRelatedParty = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateOrgRelatedParty(Factory, supplier, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, "AUSYD");
			var declaration = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Export, "AUSYD", "USLAX", ZGuid.Empty, ZGuid.Empty);
			AssertSupplierDefaultsRelatedParty(CustomsDataRegistry.Instance.RelatedPartyDefaultingExternalBroker, declaration, supplier, declaration.JE_OH_ExternalBrokerInfo, orgRelatedParty.PK);
		}

		public void TestJE_OH_Supplier_DefaultDepot()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var orgRelatedParty = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateOrgRelatedParty(Factory, organisation, RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Pickup, "AUSYD");
			var declaration = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Export, "AUSYD", "USLAX", ZGuid.Empty, ZGuid.Empty);
			AssertSupplierDefaultsRelatedParty(CustomsDataRegistry.Instance.RelatedPartyDefaultingDepot, declaration, organisation, declaration.DepotDocAddress.OrganisationPKInfo, orgRelatedParty.PK);
		}

		public void TestJE_TransportMode_DefaultExternalBroker()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var orgRelatedParty = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateOrgRelatedParty(Factory, organisation, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, "USLAX");
			var declaration = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Import, "AUSYD", "USLAX", organisation.PK, organisation.PK);
			AssertTransportModeDefaultsRelatedParty(CustomsDataRegistry.Instance.RelatedPartyDefaultingExternalBroker, declaration, organisation, declaration.JE_OH_ExternalBrokerInfo, orgRelatedParty.PK);
		}

		public void TestJE_TransportMode_DefaultDepot()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var orgRelatedParty = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateOrgRelatedParty(Factory, organisation, RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Delivery, "USLAX");
			var declaration = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Import, "AUSYD", "USLAX", organisation.PK, organisation.PK);
			AssertTransportModeDefaultsRelatedParty(CustomsDataRegistry.Instance.RelatedPartyDefaultingDepot, declaration, organisation, declaration.DepotDocAddress.OrganisationPKInfo, orgRelatedParty.PK);
		}

		public void TestJE_ContainerMode_DefaultExternalBroker()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var orgRelatedParty = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateOrgRelatedParty(Factory, organisation, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, "USLAX");
			var declaration = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Import, "AUSYD", "USLAX", organisation.PK, organisation.PK);
			AssertContainerModeDefaultsRelatedParty(CustomsDataRegistry.Instance.RelatedPartyDefaultingExternalBroker, declaration, organisation, declaration.JE_OH_ExternalBrokerInfo, orgRelatedParty.PK);
		}

		public void TestJE_ContainerMode_DefaultDepot()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var orgRelatedParty = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateOrgRelatedParty(Factory, organisation, RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Delivery, "USLAX");
			var declaration = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Import, "AUSYD", "USLAX", organisation.PK, organisation.PK);
			AssertContainerModeDefaultsRelatedParty(CustomsDataRegistry.Instance.RelatedPartyDefaultingDepot, declaration, organisation, declaration.DepotDocAddress.OrganisationPKInfo, orgRelatedParty.PK);
		}

		public void TestJE_RL_NKFinalDestination_DefaultExternalBroker()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var orgRelatedParty = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateOrgRelatedParty(Factory, importer, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, "USLAX");
			var declaration = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Import, "AUSYD", ZString.Empty, importer.PK, ZGuid.Empty);
			AssertFinalDestinationDefaultsRelatedParty(CustomsDataRegistry.Instance.RelatedPartyDefaultingExternalBroker, declaration, importer, declaration.JE_OH_ExternalBrokerInfo, orgRelatedParty.PK);
		}

		public void TestJE_RL_NKFinalDestination_DefaultDepot()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var orgRelatedParty = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateOrgRelatedParty(Factory, importer, RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Delivery, "USLAX");
			var declaration = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Import, "AUSYD", ZString.Empty, importer.PK, ZGuid.Empty);
			AssertFinalDestinationDefaultsRelatedParty(CustomsDataRegistry.Instance.RelatedPartyDefaultingDepot, declaration, importer, declaration.DepotDocAddress.OrganisationPKInfo, orgRelatedParty.PK);
		}

		public void TestJE_RL_NKOrigin_DefaultExternalBroker()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var orgRelatedParty = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateOrgRelatedParty(Factory, supplier, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, "AUSYD");
			var declaration = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Export, ZString.Empty, "USLAX", ZGuid.Empty, supplier.PK);
			AssertOriginDefaultsRelatedParty(CustomsDataRegistry.Instance.RelatedPartyDefaultingExternalBroker, declaration, supplier, declaration.JE_OH_ExternalBrokerInfo, orgRelatedParty.PK);
		}

		public void TestJE_RL_NKOrigin_DefaultDepot()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var orgRelatedParty = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateOrgRelatedParty(Factory, supplier, RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Pickup, "AUSYD");
			var declaration = BaseJobDeclarationPartyDefaultingExtensionsTest.CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Export, ZString.Empty, "USLAX", ZGuid.Empty, supplier.PK);
			AssertOriginDefaultsRelatedParty(CustomsDataRegistry.Instance.RelatedPartyDefaultingDepot, declaration, supplier, declaration.DepotDocAddress.OrganisationPKInfo, orgRelatedParty.PK);
		}

		public void TestFilteredInvoiceLines()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			AssertType<InvoiceLineViewCollection<BaseJobComInvoiceLine>>(declaration.FilteredInvoiceLines);
		}

		public void TestIsMail()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("Default", false, declaration.IsMail);
				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				AssertEquals("Not 'MAI'", false, declaration.IsMail);
				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Mail;
				AssertEquals("'MAI'", true, declaration.IsMail);
			});
		}

		public void TestIsWaterwayTransports()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("Default", false, declaration.IsWaterwayTransport);
				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				AssertEquals("Not 'IWT'", false, declaration.IsWaterwayTransport);
				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.InlandWaterwayTransport;
				AssertEquals("'IWT'", true, declaration.IsWaterwayTransport);
			});
		}

		public void TestIsOwnPropulsion()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("Default", false, declaration.IsOwnPropulsion);
				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				AssertEquals("Not 'OWN'", false, declaration.IsOwnPropulsion);
				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.OwnPropulsion;
				AssertEquals("'OWN'", true, declaration.IsOwnPropulsion);
			});
		}
		public void TestIsRoadInland()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("Default", false, declaration.IsRoadInland);
				declaration.JE_TransportModeInland = Enterprise.Core.Constants.TransportModes.Air;
				AssertEquals("Not 'ROA'", false, declaration.IsRoadInland);
				declaration.JE_TransportModeInland = Enterprise.Core.Constants.TransportModes.Road;
				AssertEquals("'ROA'", true, declaration.IsRoadInland);
			});
		}
		public void TestIsSeaInland()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("Default", false, declaration.IsSeaInland);
				declaration.JE_TransportModeInland = Enterprise.Core.Constants.TransportModes.Air;
				AssertEquals("Not 'SEA'", false, declaration.IsSeaInland);
				declaration.JE_TransportModeInland = Enterprise.Core.Constants.TransportModes.Sea;
				AssertEquals("'SEA'", true, declaration.IsSeaInland);
			});
		}
		public void TestIsAirInland()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("Default", false, declaration.IsAirInland);
				declaration.JE_TransportModeInland = Enterprise.Core.Constants.TransportModes.Sea;
				AssertEquals("Not 'AIR'", false, declaration.IsAirInland);
				declaration.JE_TransportModeInland = Enterprise.Core.Constants.TransportModes.Air;
				AssertEquals("'AIR'", true, declaration.IsAirInland);
			});
		}
		public void TestIsInlandWaterwayTransportsInland()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("Default", false, declaration.IsWaterwayTransportsInland);
				declaration.JE_TransportModeInland = Enterprise.Core.Constants.TransportModes.Sea;
				AssertEquals("Not 'IWT'", false, declaration.IsWaterwayTransportsInland);
				declaration.JE_TransportModeInland = Enterprise.Core.Constants.TransportModes.InlandWaterwayTransport;
				AssertEquals("'IWT'", true, declaration.IsWaterwayTransportsInland);
			});
		}
		public void TestIsRailInland()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("Default", false, declaration.IsRailInland);
				declaration.JE_TransportModeInland = Enterprise.Core.Constants.TransportModes.Sea;
				AssertEquals("Not 'RAI'", false, declaration.IsRailInland);
				declaration.JE_TransportModeInland = Enterprise.Core.Constants.TransportModes.Rail;
				AssertEquals("'RAI'", true, declaration.IsRailInland);
			});
		}
		public void TestIsOwnPropulsionInland()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("Default", false, declaration.IsOwnPropulsionInland);
				declaration.JE_TransportModeInland = Enterprise.Core.Constants.TransportModes.Sea;
				AssertEquals("Not 'OWN'", false, declaration.IsOwnPropulsionInland);
				declaration.JE_TransportModeInland = Enterprise.Core.Constants.TransportModes.OwnPropulsion;
				AssertEquals("'OWN'", true, declaration.IsOwnPropulsionInland);
			});
		}

		public void TestIsMailInland()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("Default", false, declaration.IsMailInland);
				declaration.JE_TransportModeInland = Enterprise.Core.Constants.TransportModes.Sea;
				AssertEquals("Not 'MAI'", false, declaration.IsMailInland);
				declaration.JE_TransportModeInland = Enterprise.Core.Constants.TransportModes.Mail;
				AssertEquals("'MAI'", true, declaration.IsMailInland);
			});
		}
		public void TestIsFixedInstallationInland()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("Default", false, declaration.IsFixedInstallationInland);
				declaration.JE_TransportModeInland = Enterprise.Core.Constants.TransportModes.Sea;
				AssertEquals("Not 'FIX'", false, declaration.IsFixedInstallationInland);
				declaration.JE_TransportModeInland = Enterprise.Core.Constants.TransportModes.FixedTransportInstallations;
				AssertEquals("'FIX'", true, declaration.IsFixedInstallationInland);
			});
		}

		public void TestJE_MarksAndNumbersShortCaption()
		{
			AssertEquals("Marks & Numbers", Factory.New<BaseJobDeclaration>().JE_MarksAndNumbersShortInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestJE_RN_NKTransportNationalityInland()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var resourceStrings = DataBoundResourceStrings.GetDataForProperty(declaration.JE_RN_NKTransportNationalityInlandInfo);
				AssertEquals("Caption", "Nationality", resourceStrings.Caption);
				AssertEquals("MediumCaption", "Nation", resourceStrings.MediumCaption);
				AssertEquals("ShortCaption", "Nat.", resourceStrings.ShortCaption);
			});
		}

		public void TestJE_RN_NKTransportNationality()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var resourceStrings = DataBoundResourceStrings.GetDataForProperty(declaration.JE_RN_NKTransportNationalityInfo);
				AssertEquals("Caption", "Nationality", resourceStrings.Caption);
				AssertEquals("MediumCaption", "Nation", resourceStrings.MediumCaption);
				AssertEquals("ShortCaption", "Nat.", resourceStrings.ShortCaption);
			});
		}

		public void TestJE_RN_NKTrailer1Nationality()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var resourceStrings = DataBoundResourceStrings.GetDataForProperty(declaration.JE_RN_NKTrailer1NationalityInfo);
				AssertEquals("Caption", "Nationality", resourceStrings.Caption);
				AssertEquals("MediumCaption", "Nation", resourceStrings.MediumCaption);
				AssertEquals("ShortCaption", "Nat.", resourceStrings.ShortCaption);
			});
		}

		public void TestJE_RN_NKTrailer2Nationality()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var resourceStrings = DataBoundResourceStrings.GetDataForProperty(declaration.JE_RN_NKTrailer2NationalityInfo);
				AssertEquals("Caption", "Nationality", resourceStrings.Caption);
				AssertEquals("MediumCaption", "Nation", resourceStrings.MediumCaption);
				AssertEquals("ShortCaption", "Nat.", resourceStrings.ShortCaption);
			});
		}

		public void TestJE_AircraftRegistrationInland()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var resourceStrings = DataBoundResourceStrings.GetDataForProperty(declaration.JE_AircraftRegistrationInlandInfo);
				AssertEquals("Caption", "Aircraft ID", resourceStrings.Caption);
				AssertEquals("MediumCaption", "Plane ID", resourceStrings.MediumCaption);
				AssertEquals("ShortCaption", "ID", resourceStrings.ShortCaption);
			});
		}

		public void TestJE_DateOfFirstArrival()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var resourceStrings = DataBoundResourceStrings.GetDataForProperty(declaration.JE_DateOfFirstArrivalInfo);
				AssertEquals("Caption", "Arrival Date at First Port of Arrival", resourceStrings.Caption);
				AssertEquals("ShortCaption", "Arr.", resourceStrings.ShortCaption);
				AssertEquals("MediumCaption", "Arrival", resourceStrings.MediumCaption);
				AssertEquals("FullDescription", "The Date of Importation at the First Port of Arrival.", resourceStrings.FullDescription);
			});
		}

		public void TestJE_ExportDate()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var resourceStrings = DataBoundResourceStrings.GetDataForProperty(declaration.JE_ExportDateInfo);
				AssertEquals("Caption", "Date of Export", resourceStrings.Caption);
				AssertEquals("ShortCaption", "Dep.", resourceStrings.ShortCaption);
				AssertEquals("MediumCaption", "Departure", resourceStrings.MediumCaption);
			});
		}

		public void TestJE_TransportMeans()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var resourceStrings = DataBoundResourceStrings.GetDataForProperty(declaration.JE_TransportMeansInfo);
				AssertEquals("Caption", "Type of ID", resourceStrings.Caption);
				AssertEquals("MediumCaption", "ID Type", resourceStrings.MediumCaption);
			});
		}

		public void TestShouldInitialiseInvoiceLineData()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			CombineAssertions(() =>
			{
				Assert("Should InitialiseInvoiceLineData before using SuspendInitialiseInvoiceLineData()", declaration.ShouldInitialiseInvoiceLineData);
				using (declaration.SuspendInvoiceLineDataInitialization())
				{
					Assert("InitialiseInvoiceLineData suspended", !declaration.ShouldInitialiseInvoiceLineData);
					using (declaration.SuspendInvoiceLineDataInitialization())
					{
						Assert("Nested initialiseInvoiceLineData suspended", !declaration.ShouldInitialiseInvoiceLineData);
					}
					Assert("InitialiseInvoiceLineData still suspended", !declaration.ShouldInitialiseInvoiceLineData);
				}

				Assert("Should InitialiseInvoiceLineData after using SuspendInitialiseInvoiceLineData()", declaration.ShouldInitialiseInvoiceLineData);

				var company = Factory.NewWithValidTestData<GlbCompany>();
				declaration.JE_GC = company.PK;
				Assert("Should not InitialiseInvoiceLineData when JE_GC of declaration is different from current company", !declaration.ShouldInitialiseInvoiceLineData);
			});
		}

		public void TestICusGoodsLocationTypeSupporter()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(typeof(CusGoodsLocation), (declaration as ICusGoodsLocationTypeSupporter).GoodsLocationType);
		}

		public void TestIsValidatingCustomsMessaging()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(false, declaration.IsValidatingCustomsMessaging);

			using (declaration.MarkDeclarationIsValidatingCustomsMessaging())
			{
				AssertEquals(true, declaration.IsValidatingCustomsMessaging);
			}
		}

		public void TestAccIntegrationWhenValidateCustomsMessaging()
		{
			var creator = new TestObjectCreator(Factory);
			var testHelper = new InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();

			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			DataRegistry.Business.CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
			Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK).Staff[0].GS_EmailAddress = "test@cargowise.com";

			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			DataRegistry.Business.CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);

			var declaration = Factory.New<BaseJobDeclarationWithValidateCustomsMessagingForTesting>();
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "12345";
			entry.Charges.AddNew("VAT", 100);

			var job = creator.CreateJob(declaration, false);
			Factory.Save();

			var charge = creator.CreateCharge(job, testHelper.DisbursementChargeCode, null, null);
			charge.JR_ChargeType = Enterprise.Core.Constants.ChargeType.Disbursement;
			charge.JR_OSSellAmt = 10m;
			charge.JR_DisplaySequence = 1;

			((Integration.Customs.IBaseJobDeclaration)declaration).GetCreditCheckMessage();

			var charges = new BusinessObjectFactory().Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("Factory saved during integration and no new charge is created", 1, charges.Length);
			AssertEquals("Charge amount updated", 100m, charges[0].JR_LocalCostAmt);
			AssertEquals("No new charge code created", charge.PK, charges[0].PK);
		}

		public void TestPopulateOwnerRefWithOrderNumbersWithoutConsignee()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();

			using (CustomsDataRegistry.Instance.PopulateOwnersRef.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				declaration.DocsAndCartage.JP_OrderItemsAsString = "OrderNumber1, OrderNumber2";
				AssertEquals("Owner Ref should polulate with Order Number", "OrderNumber1, OrderNumber2", declaration.JE_OwnerRef);
			}

			using (CustomsDataRegistry.Instance.PopulateOwnersRef.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				declaration.DocsAndCartage.JP_OrderItemsAsString = "OrderNumber3, OrderNumber4";
				AssertEquals("Owner Ref should not polulate with new Order Number", "OrderNumber1, OrderNumber2", declaration.JE_OwnerRef);
			}

			using (CustomsDataRegistry.Instance.PopulateOwnersRef.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orderItem1 = declaration.AttachedOrders.AddNew();
				orderItem1.JD_OrderNumber = "OrderItem1";
				var orderItem2 = declaration.AttachedOrders.AddNew();
				orderItem2.JD_OrderNumber = "OrderItem2";
				AssertEquals("Owner Ref is truncated to 35 if the value goes over max length", "OrderItem1, OrderItem2, OrderNumber", declaration.JE_OwnerRef);

				orderItem1.JD_OrderNumber = "OrderItem3";
				AssertEquals("Owner Ref should update when an orderItem number is updated", "OrderItem2, OrderItem3, OrderNumber", declaration.JE_OwnerRef);
				orderItem2.JD_OrderNumber = "OrderItem4";
				AssertEquals("Owner Ref should update when an orderItem number is updated", "OrderItem3, OrderItem4, OrderNumber", declaration.JE_OwnerRef);

				declaration.AttachedOrders.Delete(orderItem1);
				AssertEquals("Owner Ref should remove the deleted orderItem1", "OrderItem4, OrderNumber3, OrderNumb", declaration.JE_OwnerRef);

				declaration.AttachedOrders.Delete(orderItem2);
				AssertEquals("Owner Ref should remove the deleted orderItem2", "OrderNumber3, OrderNumber4", declaration.JE_OwnerRef);

				var orderItem3 = declaration.AttachedOrders.AddNew();
				orderItem3.JD_OrderNumber = "OrderItem3";
				declaration.AttachedOrders.Delete(orderItem3);
				AssertEquals("Owner Ref should be not affect after adding an order item and remove it", "OrderNumber3, OrderNumber4", declaration.JE_OwnerRef);
			}
		}

		public void TestPopulateOwnerRefWithOrderNumbersWithConsigneeOrgSetting()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.MiscServ.OM_IMAutoPopulateOwnerRefWithOrderNums = PopulateOwnerRefList.Codes.Default;

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = consignee.PK;
			Factory.Save();

			using (CustomsDataRegistry.Instance.PopulateOwnersRef.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				declaration.DocsAndCartage.JP_OrderItemsAsString = "OrderNumber1, OrderNumber2";
				AssertEquals("Owner Ref should polulate with Order Number with registry true value", "OrderNumber1, OrderNumber2", declaration.JE_OwnerRef);
			}

			using (CustomsDataRegistry.Instance.PopulateOwnersRef.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				declaration.DocsAndCartage.JP_OrderItemsAsString = "OrderNumber3, OrderNumber4";
				AssertEquals("Owner Ref should not polulate with new Order Number with registry false value", "OrderNumber1, OrderNumber2", declaration.JE_OwnerRef);
			}

			consignee.MiscServ.OM_IMAutoPopulateOwnerRefWithOrderNums = PopulateOwnerRefList.Codes.Yes;
			consignee.MiscServ.OM_IMAutoImpJobRefered = true;
			declaration.DocsAndCartage.JP_OrderItemsAsString = "OrderNumber3, OrderNumber4";
			AssertEquals("Owner Ref should not polulate with Order Number if AutoAssignImporterRef is true", "OrderNumber1, OrderNumber2", declaration.JE_OwnerRef);
			consignee.MiscServ.OM_IMAutoImpJobRefered = false;
			declaration.DocsAndCartage.JP_OrderItemsAsString = "OrderNumber5, OrderNumber6";
			AssertEquals("Owner Ref should polulate with Order Number with consignee consignee auto populate OwnerRef YES setting", "OrderNumber5, OrderNumber6", declaration.JE_OwnerRef);

			consignee.MiscServ.OM_IMAutoPopulateOwnerRefWithOrderNums = PopulateOwnerRefList.Codes.No;
			declaration.DocsAndCartage.JP_OrderItemsAsString = "OrderNumber1, OrderNumber2";
			AssertEquals("Owner Ref should not polulate with Order Number with consignee auto populate OwnerRef No setting", "OrderNumber5, OrderNumber6", declaration.JE_OwnerRef);
		}

		public void TestPopulateOwnerRefWithOrderNumbersWithCorrectOrder()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();

			using (CustomsDataRegistry.Instance.PopulateOwnersRef.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				declaration.DocsAndCartage.JP_OrderItemsAsString = "OrderINV1";
				AssertEquals("Owner Ref should polulate in correct order", "OrderINV1", declaration.JE_OwnerRef);

				var orderItem1 = declaration.AttachedOrders.AddNew();
				orderItem1.JD_OrderNumber = "Order1";
				AssertEquals("Owner Ref should polulate in correct order", "Order1, OrderINV1", declaration.JE_OwnerRef);

				var orderItem2 = declaration.AttachedOrders.AddNew();
				orderItem2.JD_OrderNumber = "Order2";
				AssertEquals("Owner Ref should polulate in correct order", "Order1, Order2, OrderINV1", declaration.JE_OwnerRef);
			}
		}

		public void TestValidateOrderRefsWithAttachedOrdersDuplicateNumber()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();

			using (CustomsDataRegistry.Instance.PopulateOwnersRef.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expectedError = "There are duplicate Order Refs. You must either delete the duplicate number from the Order Refs field or unlink the linked Order before saving.";
				var cartage = declaration.DocsAndCartage;
				cartage.JP_OrderItemsAsString = "Order1,Order1";
				AssertHasError(cartage.JP_OrderItemsAsStringInfo, expectedError);

				cartage.JP_OrderItemsAsString = "Order1,Order2";
				var orderItem1 = declaration.AttachedOrders.AddNew();
				orderItem1.JD_OrderNumber = "Order1";
				declaration.AttachedOrders.Add(orderItem1);

				AssertHasError(cartage.JP_OrderItemsAsStringInfo, expectedError);

				cartage.JP_OrderItemsAsString = "Order3";
				AssertNoError(cartage.JP_OrderItemsAsStringInfo, expectedError);

				var orderItem2 = declaration.AttachedOrders.AddNew();
				orderItem2.JD_OrderNumber = "Order3";
				declaration.AttachedOrders.Add(orderItem2);
				AssertHasError(cartage.JP_OrderItemsAsStringInfo, expectedError);
			}
		}

		[TestDate(2024, 3, 14)]
		public void TestReleaseDate()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryheader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryheader2 = declaration.CustomsEntryHeaders.AddNew();
			var entryheader3 = declaration.CustomsEntryHeaders.AddNew();

			AssertEquals("ReleaseDate should be empty because there is no entry header.", ZString.Empty, declaration.EntryReleaseDate);

			entryheader1.CH_EntryReleaseDate = ZDateTime.Today;
			Factory.Save();
			AssertEquals("ReleaseDate should map entry entryheader1 CH_EntryReleaseDate.", ZDateTime.Today.ToString(DateTimeFormatStrings.ShortDateFormat, CultureInfo.CurrentCulture), declaration.EntryReleaseDate);

			entryheader2.CH_EntryReleaseDate = ZDateTime.Today;
			Factory.Save();
			AssertEquals("ReleaseDate should map entry entryheader1/entryheader2 CH_EntryReleaseDate.", ZDateTime.Today.ToString(DateTimeFormatStrings.ShortDateFormat, CultureInfo.CurrentCulture), declaration.EntryReleaseDate);

			entryheader3.CH_EntryReleaseDate = ZDateTime.Today.AddDays(-1);
			Factory.Save();
			AssertEquals("ReleaseDate should map String 'Many'.", "Many", declaration.EntryReleaseDate);
		}

		static IDisposable TemporarilySetInterfacedCountry(string countryCode)
		{
			IDisposable temporarilySetCountryToChina = null;
			IDisposable setLocalCountryCustomsInterface = null;
			return new DisposableAction(() =>
			{
				temporarilySetCountryToChina = GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode);
				var customsInterface = new LocalCountryCustomsInterface
				{
					RecipientID = "RecipientID",
					SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced
				};
				setLocalCountryCustomsInterface = DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			}, () =>
			{
				setLocalCountryCustomsInterface.Dispose();
				temporarilySetCountryToChina.Dispose();
			});
		}

		void CreateRuleNotes(string origin, string destination, string notes, bool isClientVisible)
		{
			var rule = Factory.NewWithValidTestData<RefCountryRules>();
			rule.R7_RN_NKOrigin = origin;
			rule.R7_RN_NKDestination = destination;
			rule.R7_Notes = notes;
			rule.R7_IsClientVisible = isClientVisible;
			Factory.Save();
		}

		string SortedText(string text)
		{
			string[] lines = text.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			Array.Sort(lines);

			StringBuilder builder = new StringBuilder();
			foreach (string line in lines)
			{
				builder.AppendLine(line);
			}

			return builder.ToString();
		}

		void AssertMessageTypeDefaultsRelatedParty(CodePairRegistryItem registryItem, BaseJobDeclaration declaration, OrgHeader organisation, ZPropertyInfo relatedPartyInfo, ZGuid expectedValue)
		{
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Both))
			{
				var relatedParty = organisation.AllRelatedParties[0];
				CombineAssertions(() =>
				{
					relatedPartyInfo.Value = ZGuid.Empty;
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertEquals("Import", expectedValue, relatedPartyInfo.Value);
					relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
					relatedParty.PR_Location = "AUSYD";
					relatedPartyInfo.Value = ZGuid.Empty;
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertEquals("Export", expectedValue, relatedPartyInfo.Value);
					relatedPartyInfo.Value = ZGuid.Empty;
					declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
					AssertEquals("Miscellaneous", ZGuid.Empty, relatedPartyInfo.Value);
				});
			}
		}

		void AssertImporterDefaultsRelatedParty(CodePairRegistryItem registryItem, BaseJobDeclaration declaration, OrgHeader organisation, ZPropertyInfo relatedPartyInfo, ZGuid expectedValue)
		{
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Both))
			{
				relatedPartyInfo.Value = ZGuid.Empty;
				declaration.JE_OH_Importer = organisation.PK;
				AssertEquals(expectedValue, relatedPartyInfo.Value);
			}
		}

		void AssertSupplierDefaultsRelatedParty(CodePairRegistryItem registryItem, BaseJobDeclaration declaration, OrgHeader organisation, ZPropertyInfo relatedPartyInfo, ZGuid expectedValue)
		{
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Both))
			{
				relatedPartyInfo.Value = ZGuid.Empty;
				declaration.JE_OH_Supplier = organisation.PK;
				AssertEquals(expectedValue, relatedPartyInfo.Value);
			}
		}

		void AssertTransportModeDefaultsRelatedParty(CodePairRegistryItem registryItem, BaseJobDeclaration declaration, OrgHeader organisation, ZPropertyInfo relatedPartyInfo, ZGuid expectedValue)
		{
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Both))
			{
				var relatedParty = organisation.AllRelatedParties[0];
				CombineAssertions(() =>
				{
					declaration.JE_TransportMode = ZString.Empty;
					relatedPartyInfo.Value = ZGuid.Empty;
					declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
					AssertEquals("Import", expectedValue, relatedPartyInfo.Value);
					relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
					relatedParty.PR_Location = "AUSYD";
					declaration.JE_TransportMode = ZString.Empty;
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					relatedPartyInfo.Value = ZGuid.Empty;
					declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
					AssertEquals("Export", expectedValue, relatedPartyInfo.Value);
				});
			}
		}

		void AssertContainerModeDefaultsRelatedParty(CodePairRegistryItem registryItem, BaseJobDeclaration declaration, OrgHeader organisation, ZPropertyInfo relatedPartyInfo, ZGuid expectedValue)
		{
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Both))
			{
				var relatedParty = organisation.AllRelatedParties[0];
				CombineAssertions(() =>
				{
					declaration.JE_ContainerMode = ZString.Empty;
					relatedPartyInfo.Value = ZGuid.Empty;
					declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
					AssertEquals("Import", expectedValue, relatedPartyInfo.Value);
					relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
					relatedParty.PR_Location = "AUSYD";
					declaration.JE_ContainerMode = ZString.Empty;
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					relatedPartyInfo.Value = ZGuid.Empty;
					declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
					AssertEquals("Export", expectedValue, relatedPartyInfo.Value);
				});
			}
		}

		void AssertFinalDestinationDefaultsRelatedParty(CodePairRegistryItem registryItem, BaseJobDeclaration declaration, OrgHeader organisation, ZPropertyInfo relatedPartyInfo, ZGuid expectedValue)
		{
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Both))
			{
				relatedPartyInfo.Value = ZGuid.Empty;
				declaration.JE_RL_NKFinalDestination = "USLAX";
				AssertEquals(expectedValue, relatedPartyInfo.Value);
			}
		}

		void AssertOriginDefaultsRelatedParty(CodePairRegistryItem registryItem, BaseJobDeclaration declaration, OrgHeader organisation, ZPropertyInfo relatedPartyInfo, ZGuid expectedValue)
		{
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Both))
			{
				relatedPartyInfo.Value = ZGuid.Empty;
				declaration.JE_RL_NKOrigin = "AUSYD";
				AssertEquals(expectedValue, relatedPartyInfo.Value);
			}
		}

		void CreateDefaultDelay(string freightMode, string dischargePort, string destinationPort, ZByte daysDelayFromArrivalToDeliver, ZByte daysFromDestinationArrivalToClientDelivery)
		{
			var delay = Factory.New<GlbPortDeliveryTime>();
			delay.G1_FreightMode = freightMode;
			delay.G1_RL_NKDischargePort = dischargePort;
			delay.G1_RL_NKDestinationPort = destinationPort;
			delay.G1_DaysDelayFromArrivalToDeliver = daysDelayFromArrivalToDeliver;
			delay.G1_DaysFromDestinationArrivalToClientDelivery = daysFromDestinationArrivalToClientDelivery;
		}

		void AssertJobHeaderDeactivation(bool hasInvoicingPlugInGUIContext)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var jobLoader = new JobHeader.Loader(declaration);
			var job = jobLoader.TryCreate();
			Factory.Save();

			declaration.IsCancelled = true;
			if (hasInvoicingPlugInGUIContext)
			{
				Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			}

			var assertionMessage1 = string.Format("declaration {0} have InvoicingPluginGUI Business Context", hasInvoicingPlugInGUIContext ? "should" : "should not");
			var assertionMessage2 = string.Format("Job {0} be deleted by OnSaving method", hasInvoicingPlugInGUIContext ? "should not" : "should");

			Assert("Deactivating declaration, IsCancelled flag should be set to true", declaration.IsCancelled);
			Assert("Deactivating declaration, IsCancelledInfo should have changes", declaration.IsCancelledHasChanged);
			AssertEquals(assertionMessage1, hasInvoicingPlugInGUIContext, declaration.HasContext(BusinessContext.InvoicingPlugInGUI));
			Assert("Job is not yet deactivated", !job.IsCancelled);

			Factory.Save();

			AssertEquals(assertionMessage2, !hasInvoicingPlugInGUIContext, job.IsCancelled);
		}

		void AsserttInvalidSmallDateWasSetError()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_DateAtFinalDestination = ZDateTime.Invalid;
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

				declaration.JE_DateAtFinalDestination = ZDateTime.MinSmallDateTimeValue.AddDays(-1);
				AssertEquals("LastMessageReported", "The value '1899-12-31T00:00:00' setting to JE_DateAtFinalDestination is out range of smalldatetime 01-Jan-1900 ~ 06-Jun-2079.", ErrorReporter.LastMessageReported);
				AssertEquals("LastKeyReported", "Invalid small date time was set to declaration DateAtFinalDestination", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			});
		}

		void AssertContainsDeniedCandidate(string description, OrgHeader orgHeader, ScreeningParty[] deniedCandidates)
		{
			AssertEquals(description, orgHeader, Array.Find(deniedCandidates, x => x.Description == description).Header);
		}

		void AssertRelatedScreeningStatusResult(OrgHeader org, ZGuid statusPK, ZString relatedOrganizationStr, List<IRelatedOrgPartyScreeningStatus> relatedScreeningStatus)
		{
			AssertEquals($"{org.OH_Code}({relatedOrganizationStr})", relatedScreeningStatus.Single(u => u.PJ_ParentID == org.PK && u.PK == statusPK).RelatedOrganization);
		}

		IRelatedOrgPartyScreeningStatus CreateRelatedOrgPartyScreeningStatus(ZGuid parentPK, string tableCode = OrgHeaderSchema.Constants.Prefix)
		{
			var status = Factory.New<IRelatedOrgPartyScreeningStatus>();
			status.PJ_ParentID = parentPK;
			status.PJ_ParentTableCode = tableCode;
			status.PJ_SystemCreateTimeUtc = ZDateTime.UtcNow;

			return status;
		}

		BaseJobComInvoiceLine RetrieveItemsFromWarehouse(WhsDataTestHelper helper, ZString jobNumber, ZString entryNumber, ZDecimal quantity)
		{
			var exWhDec = helper.GetNewDeclaration(JobMessageTypeList.Codes.ExWarehouse, jobNumber, entryNumber, quantity);
			var outwardInvoiceLine = exWhDec.InvoiceLines[0];
			outwardInvoiceLine.SetUseBondedWarehouseAutomationForTesting(true);
			outwardInvoiceLine.JI_PartNo = helper.Part.OP_PartNum;
			Factory.Save();
			exWhDec.UpdateOutwardLinesWithInventoryDetails();
			return outwardInvoiceLine;
		}

		void AddItemsToWareHouse(WhsDataTestHelper helper, ZString jobNumber, ZString entryNumber, ZDecimal quantity, DateTime arrivalDate)
		{
			var inWhDec = helper.GetNewDeclaration(JobMessageTypeList.Codes.Import, jobNumber, entryNumber, quantity);
			Factory.Save();
			var decResult = inWhDec.PublishShipmentForWHSInward(false);
			var whsReceive = (IWhsReceive)decResult.FindJobIfExists();
			whsReceive.WD_ArrivalDate = arrivalDate;
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			whsReceive.Factory.Save();
		}

		void CreateTestTemplate()
		{
			var existingTemplates = Factory.Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode));
			existingTemplates.ForEach(t => t.P0_IsActive = false);

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			template.P0_SubType1 = Core.Constants.TransportModes.Air;
			var templateTrigger1 = template.WorkflowItems.Triggers.AddNew();
			templateTrigger1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			templateTrigger1.P9_Description = "SHP Trigger1";
			var action1 = templateTrigger1.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.CreateBrokerageOnShipment;
			var templateTrigger2 = template.WorkflowItems.Triggers.AddNew();
			templateTrigger2.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			templateTrigger2.P9_Description = "SHP Trigger2";
			templateTrigger2.TemplateConditions.TemplateCondition1 = JobShipmentWorkflowCondition1CodeList.Codes.BrokerageAttached;
			Factory.Save();
		}

		ForwardingShipmentForTestCreateTaskFromWorkflowTemplate CreateShipment()
		{
			var shipment = Factory.New<ForwardingShipmentForTestCreateTaskFromWorkflowTemplate>();
			shipment.JS_HouseBill = "S0001";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Factory.Save();

			var shipmentTriggers = shipment.WorkflowItems.Triggers;
			AssertEquals("Precondition: one Trigger added from template", 1, shipmentTriggers.Count);
			AssertEquals("Precondition: templateTrigger1 added to shipment", "SHP Trigger1", shipmentTriggers[0].P9_Description);

			return shipment;
		}

		static ForwardingConsol AddConsol(ForwardingShipment shipment, string loadPort, string dischargePort)
		{
			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = loadPort;
			consol.JK_RL_NKDischargePort = dischargePort;
			return consol;
		}

		IDisposable SetNewBranchAsTemporaryContext()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";
			Factory.NewWithValidTestData<GlbCompany>().Branches.Add(branch);
			Factory.Save();
			return branch.SetAsTemporaryContext();
		}

		void AssertDefaultCarrierFromSchedule(string registryValue, bool expected)
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingCarrier.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var voyage = InitialiseSailings();

				AssertNotNull(voyage.Line);
				voyage.JV_FlightDate = new ZDateTime(2005, 12, 21);
				voyage.JV_IsChartered = false;
				voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_ExportDate = new ZDateTime(2005, 12, 21);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_VesselName = "My Vessel";
				declaration.JE_VoyageFlightNo = "V123";
				AssertEquals("ShippingLine defaulting", expected, voyage.JV_OH_Line == declaration.JE_OH_ShippingLine);

				declaration.JE_RL_NKPortOfArrival = "AUSYD";
				AssertEquals("CTO is defaulted", voyage.Destinations[0].JB_OA_ArrivalCTOAddress, declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address);
			}
		}

		void AssertTriggerAddedOnshipment(ForwardingShipment shipment)
		{
			CombineAssertions(() =>
			{
				var shipmentTriggers = shipment.WorkflowItems.Triggers;
				AssertEquals("Two Triggers added from template", 2, shipmentTriggers.Count);
				AssertEquals("templateTrigger1 already in shipment", "SHP Trigger1", shipmentTriggers[0].P9_Description);
				AssertEquals("templateTrigger2 added to shipment", "SHP Trigger2", shipmentTriggers[1].P9_Description);
				AssertEquals("Declaration attached to shipment", 1, shipment.Declarations.Length);
			});
		}

		void AssertCartageDataIsCleared(CartageData data)
		{
			CombineAssertions(() =>
			{
				AssertEquals(data.EstimatedInfo.Name, ZDateTime.Empty, data.EstimatedInfo.Value);
				AssertEquals(data.RequiredByInfo.Name, ZDateTime.Empty, data.RequiredByInfo.Value);
				AssertEquals(data.CartageAdvisedInfo.Name, ZDateTime.Empty, data.CartageAdvisedInfo.Value);
				AssertEquals(data.CartageCompletedInfo.Name, ZDateTime.Empty, data.CartageCompletedInfo.Value);
				AssertEquals(data.LabourTimeInfo.Name, ZDateTime.Empty, data.LabourTimeInfo.Value);
				AssertEquals(data.LabourChargeInfo.Name, ZDecimal.Zero, data.LabourChargeInfo.Value);
				AssertEquals(data.DemurrageOnTimeInfo.Name, ZDateTime.Empty, data.DemurrageOnTimeInfo.Value);
				AssertEquals(data.DemurrageOnChargeInfo.Name, ZDecimal.Zero, data.DemurrageOnChargeInfo.Value);
				AssertEquals(data.CartageCoPKInfo.Name, ZGuid.Empty, data.CartageCoPKInfo.Value);
				AssertEquals(data.OA_CartageCoAddrInfo.Name, ZGuid.Empty, data.OA_CartageCoAddrInfo.Value);
			});
		}

		void AssertCartageData(CartageData data)
		{
			CombineAssertions(() =>
			{
				AssertEquals(data.EstimatedInfo.Name, new ZDateTime(2019, 10, 1), data.EstimatedInfo.Value);
				AssertEquals(data.RequiredByInfo.Name, new ZDateTime(2019, 10, 2), data.RequiredByInfo.Value);
				AssertEquals(data.CartageAdvisedInfo.Name, new ZDateTime(2019, 10, 3), data.CartageAdvisedInfo.Value);
				AssertEquals(data.CartageCompletedInfo.Name, new ZDateTime(2019, 10, 4), data.CartageCompletedInfo.Value);
				AssertEquals(data.LabourTimeInfo.Name, new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 10, 5), data.LabourTimeInfo.Value);
				AssertEquals(data.LabourChargeInfo.Name, new ZDecimal(10.50m), data.LabourChargeInfo.Value);
				AssertEquals(data.DemurrageOnTimeInfo.Name, new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 10, 6), data.DemurrageOnTimeInfo.Value);
				AssertEquals(data.DemurrageOnChargeInfo.Name, new ZDecimal(20.79m), data.DemurrageOnChargeInfo.Value);
				AssertEquals(data.CartageCoPKInfo.Name, GlbCompany.CurrentCompany.GC_OH_OrgProxy, data.CartageCoPKInfo.Value);
				AssertEquals(data.OA_CartageCoAddrInfo.Name, GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK, data.OA_CartageCoAddrInfo.Value);
			});
		}

		public void TestUseDeclarationContainersIfNoneFoundOnEntry()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(true, declaration.UseDeclarationContainersIfNoneFoundOnEntry);
		}

		public void TestUseDeclarationContainersForSingleContainerWhenMultipleEntriesExist()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(true, declaration.UseDeclarationContainersForSingleContainerWhenMultipleEntriesExist);
		}

		public void TestDeleteAnyNewMessages_DoNotLoadMessagesDuringDelete()
		{
			var declaration = Factory.New<Integration.Customs.US.IJobDeclaration>() as BaseJobDeclaration;
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = "RCV";
			declaration.Messages.Add(message);
			Factory.Save();
			var expected = new Dictionary<string, int>
			{
				{ EDIMessage.Schema.TableName, 1 }
			};
			AssertEquals(1, declaration.Messages.Count);
			declaration.Messages.Reload(true);
			AssertDbHits(expected, Factory, true);

			var newFactory = new BusinessObjectFactory();
			var newDec = newFactory.Load<Integration.Customs.US.IJobDeclaration>(declaration.PK) as BaseJobDeclaration;
			newDec.DeleteAnyNewMessages();
			expected[EDIMessage.Schema.TableName] = 0;
			AssertDbHits(expected, newFactory, true);
		}

		public void TestJE_AddInfoSet()
		{
			var bo = Factory.New<BaseJobDeclaration>();

			AssertEquals(ZString.Empty, bo.JE_AddInfo);
			AssertEquals(ZString.Empty, bo.JE_NAddInfo);

			bo.JE_AddInfo = "String=abc*NString=def";

			AssertEquals("String=abc*NString=def", bo.JE_AddInfo);
			AssertEquals(ZString.Empty, bo.JE_NAddInfo);
		}

		public void TestJE_AddInfoSet_WithBaseAddInfo()
		{
			var bo = Factory.New<BaseJobDeclarationWithBaseAddInfoForTesting>();

			AssertEquals(ZString.Empty, bo.JE_AddInfo);
			AssertEquals(ZString.Empty, bo.JE_NAddInfo);

			bo.JE_AddInfo = "String=abc*NString=def";

			AssertEquals("String=abc*NString=def", bo.JE_AddInfo);
			AssertEquals("NString=def", bo.JE_NAddInfo);
		}

		public void TestJE_AddInfoSet_WithAddInfoWrapper()
		{
			var bo = Factory.New<BaseJobDeclarationWithAddInfoWrapperForTesting>();

			AssertEquals(ZString.Empty, bo.JE_AddInfo);
			AssertEquals(ZString.Empty, bo.JE_NAddInfo);
			AssertEquals(ZString.Empty, bo.JE_String);
			AssertEquals(ZString.Empty, bo.JE_NString);

			bo.JE_AddInfo = "String=abc*NString=def";

			AssertEquals("String=abc*NString=def", bo.JE_AddInfo);
			AssertEquals("", bo.JE_NAddInfo);
			AssertEquals("abc", bo.JE_String);
			AssertEquals("", bo.JE_NString);

			bo.JE_NAddInfo = "String=ghi*NString=jkl";

			AssertEquals("String=abc*NString=def", bo.JE_AddInfo);
			AssertEquals("String=ghi*NString=jkl", bo.JE_NAddInfo);
			AssertEquals("abc", bo.JE_String);
			AssertEquals("jkl", bo.JE_NString);
		}

		void SetupCartageData(CartageData data)
		{
			data.EstimatedInfo.Value = new ZDateTime(2019, 10, 1);
			data.RequiredByInfo.Value = new ZDateTime(2019, 10, 2);
			data.CartageAdvisedInfo.Value = new ZDateTime(2019, 10, 3);
			data.CartageCompletedInfo.Value = new ZDateTime(2019, 10, 4);
			data.LabourTimeInfo.Value = new ZDateTime(2019, 10, 5);
			data.LabourChargeInfo.Value = new ZDecimal(10.50m);
			data.DemurrageOnTimeInfo.Value = new ZDateTime(2019, 10, 6);
			data.DemurrageOnChargeInfo.Value = new ZDecimal(20.79m);
			data.OA_CartageCoAddrInfo.Value = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
		}

		struct CartageData
		{
			public ZPropertyInfo EstimatedInfo;
			public ZPropertyInfo RequiredByInfo;
			public ZPropertyInfo CartageAdvisedInfo;
			public ZPropertyInfo CartageCompletedInfo;
			public ZPropertyInfo LabourTimeInfo;
			public ZPropertyInfo LabourChargeInfo;
			public ZPropertyInfo DemurrageOnTimeInfo;
			public ZPropertyInfo DemurrageOnChargeInfo;
			public ZPropertyInfo OA_CartageCoAddrInfo;
			public ZPropertyInfo CartageCoPKInfo;
		}

		BaseJobDeclaration CreateBaseJobDeclarationForTest(string messageType)
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			dec.JE_OH_Supplier = Factory.New<OrgHeader>().PK;
			dec.JE_MessageType = messageType;
			return dec;
		}

		BaseJobDeclaration CreateImportDeclarationWithImporter(string transportMode)
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			OrgHeader AddRelatedParty(string containerMode)
			{
				var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
				relatedParty.OH_IsShippingProvider = true;
				relatedParty.OH_IsLocalTransport = true;

				importer.AddRelatedParty(relatedParty.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, transportMode, containerMode, GlbCompany.CurrentCompany);

				return relatedParty;
			}

			AddRelatedParty(Core.Constants.ContainerModes.BreakBulk);
			AddRelatedParty(Core.Constants.ContainerModes.FCLMixedShipper);
			AddRelatedParty(Core.Constants.ContainerModes.FCL);
			AddRelatedParty(Core.Constants.ContainerModes.Groupage);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = transportMode;

			declaration.JE_OH_Importer = importer.PK;

			return declaration;
		}

		BaseJobDeclaration CreateExportDeclarationWithSupplier(string transportMode)
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			OrgHeader AddRelatedParty(string containerMode)
			{
				var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
				relatedParty.OH_IsShippingProvider = true;
				relatedParty.OH_IsLocalTransport = true;

				supplier.AddRelatedParty(relatedParty.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, transportMode, containerMode, GlbCompany.CurrentCompany);

				return relatedParty;
			}

			AddRelatedParty(Core.Constants.ContainerModes.BreakBulk);
			AddRelatedParty(Core.Constants.ContainerModes.FCLMixedShipper);
			AddRelatedParty(Core.Constants.ContainerModes.FCL);
			AddRelatedParty(Core.Constants.ContainerModes.Groupage);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = transportMode;

			declaration.JE_OH_Supplier = supplier.PK;

			return declaration;
		}

		JobVoyage InitialiseSailings()
		{
			var vessel = RefVessel.New(Factory);
			vessel.RV_Code = "My Vessel";

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			vessel.RV_OH = shippingLine.PK;

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "V123";
			voyage.JV_OH_Line = shippingLine.PK;

			var origin = voyage.Origins.AddNew();
			origin.JA_A_DEP = new ZDateTime(2005, 12, 20);
			origin.JA_RL_NKPortOfLoading = "IRABD";

			var arrivalCTO = Factory.NewWithValidTestData<OrgHeader>();

			var destination = voyage.Destinations.AddNew();
			destination.JB_E_ARV = new ZDateTime(2005, 12, 22);
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			destination.JB_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;

			AssertEquals(1, voyage.Sailings.Count);
			var sailing = voyage.Sailings[0];
			sailing.Destination.JB_AvailabilityDate = new ZDateTime(2005, 12, 23);
			sailing.Destination.JB_StorageDate = new ZDateTime(2005, 12, 25);
			sailing.JX_DepotAvailabilityDate = new ZDateTime(2005, 12, 23);
			sailing.JX_DepotStorageDate = new ZDateTime(2005, 12, 24);

			var firstArrival = voyage.Destinations.AddNew();
			firstArrival.JB_A_ARV = new ZDateTime(2005, 12, 24);
			firstArrival.JB_E_ARV = new ZDateTime(2005, 12, 25);
			firstArrival.JB_RL_NKPortOfDischarge = "AUPER";

			return voyage;
		}

		BillOfLadingNumberCustomisationElement SetElement(BillOfLadingNumberCustomisation customisation, string key, byte order)
		{
			var element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			return element;
		}

		BillOfLadingNumberCustomisationElement SetElement(BillOfLadingNumberCustomisation customisation, string key, byte order, string detail)
		{
			var element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			element.Detail = detail;
			return element;
		}

		RefUNLOCO HomePort
		{
			get
			{
				if (!GlbBranch.CurrentBranch.GB_RL_NKHomePort.IsEmpty)
				{
					return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, GlbBranch.CurrentBranch.GB_RL_NKHomePort);
				}
				return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			}
		}

		RefUNLOCO OverseasPort
		{
			get
			{
				if (GlbBranch.CurrentBranch.Country.Code != Core.Constants.CountryCodes.Australia)
				{
					return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
				}
				else
				{
					return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
				}
			}
		}

		void SetupTestVessels()
		{
			testVessel1 = Factory.NewWithValidTestData<RefVessel>();
			testVessel1.RV_Name = "HYOGO MARU";
			testVessel1.RV_LloydsNumber = "4567123";

			testVessel2 = Factory.NewWithValidTestData<RefVessel>();
			testVessel2.RV_Name = "HYOGO KENU";
			testVessel2.RV_LloydsNumber = "6712345";

			testVessel3 = Factory.NewWithValidTestData<RefVessel>();
			testVessel3.RV_Name = "VESSEL NO LLOYDS";
			testVessel3.RV_LloydsNumber = "";

			testVessel4 = Factory.NewWithValidTestData<RefVessel>();
			testVessel4.RV_Name = "DUP_VESS";
			testVessel4.RV_LloydsNumber = "4984731";

			// TODO: implement when unique key constraint is removed:

			//TestVessel5 = Factory.NewWithValidTestData<RefVessel>();
			//TestVessel5.RV_Name = "DUP_VESS";
			//TestVessel5.RV_LloydsNumber = "";

			//TestVessel6 = Factory.NewWithValidTestData<RefVessel>();
			//TestVessel6.RV_Name = "DUP_VESS";
			//TestVessel6.RV_LloydsNumber = "5738219";

			Factory.Save();
		}

		RefVessel testVessel1;
		RefVessel testVessel2;
		RefVessel testVessel3;
		RefVessel testVessel4;
		//RefVessel TestVessel5;
		//RefVessel TestVessel6;

		class TestDeclaration : BaseJobDeclaration
		{
			public TestDeclaration(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool ShouldThrowExceptionOnSaving;
			public override void OnSaving()
			{
				base.OnSaving();
				if (ShouldThrowExceptionOnSaving)
				{
					throw new Exception("Testing");
				}
			}

			internal bool isWeightApportionmentDeferred;

			protected override InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection()
			{
				var mockInvoiceLineCompleteCollection = new Mock<InvoiceLineCompleteCollection>(this);
				mockInvoiceLineCompleteCollection.CallBase = true;
				mockInvoiceLineCompleteCollection.Protected().Setup("OnLoaded").Callback(() =>
				{
					isWeightApportionmentDeferred = WeightApportionManager.IsWeightApportionmentDeffered;
				});
				return mockInvoiceLineCompleteCollection.Object;
			}

			public ScreeningParty[] ScreeningParties => base.GetScreeningPartiesCore();
		}

		sealed class JobDeclarationForTestCreateTaskFromWorkflowTemplate : BaseJobDeclaration
		{
			public JobDeclarationForTestCreateTaskFromWorkflowTemplate(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			protected override void OnFactorySavingBeforeTransactionCore()
			{
				if (!IsInDatabase || HasChanges && Shipment != null && !Shipment.HasChanges)
				{
					CreateTasksAndMilestonesFromTemplateInvokedOnDeclaration++;
				}
				base.OnFactorySavingBeforeTransactionCore();
			}

			public int CreateTasksAndMilestonesFromTemplateInvokedOnDeclaration { get; set; }
		}

		sealed class ForwardingShipmentForTestCreateTaskFromWorkflowTemplate : ForwardingShipment
		{
			public ForwardingShipmentForTestCreateTaskFromWorkflowTemplate(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			protected override void OnFactorySavingBeforeTransactionCore()
			{
				if (HasChanges)
				{
					CreateTasksAndMilestonesFromTemplateInvokedOnShipment++;
				}
				base.OnFactorySavingBeforeTransactionCore();
			}

			public int CreateTasksAndMilestonesFromTemplateInvokedOnShipment { get; set; }
		}

		sealed class DummyBaseJobDeclaration_TestTransportModeIsNotDefaultedWhenJobIsNonTransportMode : BaseJobDeclaration
		{
			public DummyBaseJobDeclaration_TestTransportModeIsNotDefaultedWhenJobIsNonTransportMode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override bool IsNonTransportDeclarationType => true;
		}

		sealed class DummyBaseJobDeclaration_TestDefaultPackQuantityAndPackTypeToBill : BaseJobDeclaration
		{
			public DummyBaseJobDeclaration_TestDefaultPackQuantityAndPackTypeToBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool IsPackingInformationRelevantCore => true;
		}
	}

	public class JobDeclarationForTestingEquipments : BaseJobDeclaration
	{
		public JobDeclarationForTestingEquipments(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override bool SupportEquipmentsCore => true;
	}

	sealed class JobDeclarationForTestingEquipmentsOnly : BaseJobDeclaration
	{
		public JobDeclarationForTestingEquipmentsOnly(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZBool ContainersRequired => false;

		protected override bool SupportEquipmentsCore => true;
	}

	sealed class JobDeclarationForTestingContainersOnly : BaseJobDeclaration
	{
		public JobDeclarationForTestingContainersOnly(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZBool ContainersRequired => true;

		protected override bool SupportEquipmentsCore => false;
	}

	sealed class JobDeclarationForTestingNonContainersAndEquipments : BaseJobDeclaration
	{
		public JobDeclarationForTestingNonContainersAndEquipments(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZBool ContainersRequired => false;

		protected override bool SupportEquipmentsCore => false;
	}

	sealed class JobDeclarationForTestingIWarehouseIntegrationSupporter : BaseJobDeclaration, IWarehouseIntegrationSupporter
	{
		public JobDeclarationForTestingIWarehouseIntegrationSupporter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		bool IWarehouseIntegrationSupporter.IsChangeOfOwnershipBondedWarehousingEnabled => IsChangeOfOwnershipBondedWarehousingEnabledForTesting;

		public bool IsChangeOfOwnershipBondedWarehousingEnabledForTesting;

		bool IWarehouseIntegrationSupporter.IsChangeOfRegimeWarehousingEnabled => IsChangeOfRegimeWarehousingEnabledForTesting;

		public bool IsChangeOfRegimeWarehousingEnabledForTesting;
	}

	sealed class BaseJobDeclarationWithBaseAddInfoForTesting : BaseJobDeclaration, IAddInfoManager, INAddInfoSupporter
	{
		public BaseJobDeclarationWithBaseAddInfoForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public TestAddInfo AddInfo => addInfo ?? (addInfo = new TestAddInfo(this));
		TestAddInfo addInfo;

		IAddInfo IAddInfoManager.AddInfo => AddInfo;
		public ZPropertyInfoString NAddInfoProperty => JE_NAddInfoInfo as ZPropertyInfoString;
	}

	sealed class BaseJobDeclarationWithAddInfoWrapperForTesting : BaseJobDeclaration, IAddInfoManagerWithSchema
	{
		public BaseJobDeclarationWithAddInfoWrapperForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			addInfo = new AddInfoWrapper<BaseJobDeclarationWithAddInfoWrapperForTesting>(
				this,
				Schema.JE_AddInfo,
				() => AddInfoNamesMapping,
				Schema.JE_NAddInfo,
				() => NAddInfoNamesMapping
			);
		}

		public ZString JE_String
		{
			get => JE_StringData.Value;
			set
			{
				SetNonPersistentPropertyValue(JE_StringInfo, ref JE_StringData.Value, value);
			}
		}
		public ZPropertyInfo JE_StringInfo => GetZPropertyInfo(nameof(JE_String));
		AddInfoPropertyData<ZString> JE_StringData => je_String ?? (je_String = new AddInfoPropertyData<ZString>(nameof(JE_String)));
		AddInfoPropertyData<ZString> je_String;

		public ZString JE_NString
		{
			get => JE_NStringData.Value;
			set
			{
				SetNonPersistentPropertyValue(JE_NStringInfo, ref JE_NStringData.Value, value);
			}
		}
		public ZPropertyInfo JE_NStringInfo => GetZPropertyInfo(nameof(JE_NString));
		AddInfoPropertyData<ZString> JE_NStringData => je_NString ?? (je_NString = new AddInfoPropertyData<ZString>(nameof(JE_NString)));
		AddInfoPropertyData<ZString> je_NString;

		IDictionary<string, IAddInfoPropertyData> AddInfoNamesMapping => addInfoNamesMapping ?? (addInfoNamesMapping = new Dictionary<string, IAddInfoPropertyData> { { "String", JE_StringData } });
		IDictionary<string, IAddInfoPropertyData> addInfoNamesMapping;

		IDictionary<string, IAddInfoPropertyData> NAddInfoNamesMapping => nAddInfoNamesMapping ?? (nAddInfoNamesMapping = new Dictionary<string, IAddInfoPropertyData> { { "NString", JE_NStringData } });
		IDictionary<string, IAddInfoPropertyData> nAddInfoNamesMapping;

		IAddInfo IAddInfoManager.AddInfo => addInfo;
		readonly IAddInfo addInfo;

		ITableSchema IAddInfoManagerWithSchema.AddInfoSchema => TWJobDeclarationSchema.Instance;
	}

	sealed class BaseJobDeclarationWithValidateCustomsMessagingForTesting : BaseJobDeclaration
	{
		public BaseJobDeclarationWithValidateCustomsMessagingForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZBool SupportValidateCustomsMessagingCore => true;

		protected internal override bool IsIntegrationWithAccountingSupported => true;
	}

	sealed class BaseJobDeclarationForUniversalCopyTesting : BaseJobDeclaration
	{
		public BaseJobDeclarationForUniversalCopyTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected internal override bool SkipDuplicateTopGroupInvoiceOnUniversalCopy => true;
	}

	sealed class BaseJobDeclarationForMiscellaneousTesting : BaseJobDeclaration
	{
		public BaseJobDeclarationForMiscellaneousTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void SetJE_OH_ImporterBaseValueOnlyForTesting(ZGuid value) => SetJE_OH_ImporterBaseValueOnly(value);

		public void SetJE_OH_SupplierBaseValueOnlyForTesting(ZGuid value) => SetJE_OH_SupplierBaseValueOnly(value);
	}
}
