using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusContainer))]
	sealed class BaseCusContainerBaseOnlyTest : BaseCusContainerTest<BaseCusContainer, BaseJobDeclaration>
	{
		public void TestNotFireDeleteLinkedJobContainer_WhenCusContainerAlreadyDeleted()
		{
			var jobContainer = Factory.New<ForwardingContainer>();
			var cusContainer = Factory.New<BaseCusContainer>();
			cusContainer.CO_JC = jobContainer.PK;
			cusContainer.CO_ContainerNumber = "TESTDELETE";

			((IBusinessObjectInternals)cusContainer).MarkAsDeleted();
			cusContainer.Delete();
			AssertEquals("Not Fire Delete LinkedJobContainer When CusContainer is Already Deleted.", false, jobContainer.IsDeleted);
		}

		public void TestDeletingContainerDoesNotClearCO_JC()
		{
			var co_JCCalledCount = 0;
			void CO_JCInfo_ValueChanged(object sender, System.EventArgs e)
			{
				co_JCCalledCount++;
			}
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();
			var declaration = Factory.New<BaseJobDeclaration>();
			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_JC = container.PK;
			cusContainer.CO_JCInfo.ValueChanged += CO_JCInfo_ValueChanged;
			cusContainer.Delete();
			AssertEquals("co_JCCalledCount", 0, co_JCCalledCount);
			cusContainer.CO_JCInfo.ValueChanged -= CO_JCInfo_ValueChanged;
		}

		public void TestContainersAndEquipmentsOnDeclaration_ListWhenCO_ContainerNumberChanged()
		{
			var mockDec = Factory.NewMoq<BaseJobDeclaration>();
			var declaration = mockDec.Object;
			mockDec.Setup(m => m.ContainersRequired).Returns(false);
			mockDec.Setup(m => m.EquipmentsRequired).Returns(true);
			_ = declaration.ContainersAndEquipmentsOnDeclaration_List;
			var containers = new BaseCusContainerCollection<BaseCusContainer>(declaration, Factory);
			var container = containers.AddNew();
			container.CO_ContainerNumber = "C1";
			_ = declaration.ContainersAndEquipmentsOnDeclaration_List;
			var equipments = new CusEquipmentCollection<CusEquipment>(declaration);
			var equipment1 = equipments.AddNew();
			equipment1.CEQ_IdentificationNumber = "E1";
			var equipment2 = equipments.AddNew();
			equipment2.CEQ_IdentificationNumber = "E2";

			container.CO_ContainerNumber = "C2";
			AssertEquals("When ContainersRequired is false and EquipmentsRequired is true", "E1, E2", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);

			mockDec.Setup(m => m.ContainersRequired).Returns(true);
			container.CO_ContainerNumber = "C1";
			AssertEquals("When ContainersRequired and EquipmentsRequired are both true", "C1, E1, E2", declaration.ContainersAndEquipmentsOnDeclaration_List.CodesAsString);
		}

		public void TestManualPopulateClusterKey()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_ClusterKey = 2;

			var container = declaration.CusContainers.AddNew();
			container.FillWithValidTestData();

			AssertEquals("Should manual populate the value from the parent declaration.", 2, container.CO_ClusterKey);
			Factory.Save();

			AssertEquals("Should populate the value from the parent declaration in OnSaving part.", declaration.JE_ClusterKey, container.CO_ClusterKey);
		}

		public void TestDeleteIncludeAllInvoiceLinePivots()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ClusterKey = 11;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var container = declaration.CusContainers.AddNew();
			container.CO_ClusterKey = 11;
			var pivot1 = invoiceLine.ContainersPivot.AddNew();
			pivot1.C2_CO = container.PK;
			pivot1.C2_ClusterKey = 11;
			Factory.Save();
			var pivot2 = invoiceLine.ContainersPivot.AddNew();
			pivot2.C2_CO = container.PK;
			pivot2.C2_ClusterKey = 0;

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Only pivot in InvoiceLinePivotCollection", new[] { pivot1 }, container.InvoiceLinePivotCollection);

				container.Delete();
				AssertEquals("pivot1 deleted", true, pivot1.IsDeleted);
				AssertEquals("pivot2 deleted", true, pivot2.IsDeleted);
			});
		}

		public void TestPivotToEntryInstructionIsDeleted()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			declaration.SupportContainerEntryInstructionPivotCoreForTesting = true;
			var container = declaration.CusContainers.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var pivot = Factory.New<CusContainerEntryInstructionPivot>();
			pivot.CEP_CEI_EntryInstruction = instruction.PK;
			pivot.CEP_CO_Container = container.PK;
			Factory.Save();

			container.Delete();
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals("pivot.IsDeleted", true, pivot.IsDeleted);
		}

		public void TestPivotToEntryIsDeleted()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.SupportContainerEntryHeaderPivot).Returns(true);
			var declaration = declarationMock.Object;
			var container = declaration.CusContainers.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var pivot = Factory.New<CusContainerEntryHeaderPivot>();
			pivot.CCE_CH_EntryHeader = entry.PK;
			pivot.CCE_CO_Container = container.PK;
			Factory.Save();

			container.Delete();
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals("pivot.IsDeleted", true, pivot.IsDeleted);
		}

		public void TestPivotsToEntriesIsNotLoad()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var pivot = Factory.New<CusContainerEntryHeaderPivot>();
			pivot.CCE_CH_EntryHeader = entry.PK;
			pivot.CCE_CO_Container = container.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDecMock = newFactory.LoadMoq<BaseJobDeclaration>(declaration.PK);
			loadedDecMock.Setup(m => m.SupportContainerEntryHeaderPivot).Returns(false);
			declaration = loadedDecMock.Object;
			container = declaration.CusContainers.Single();
			container.LoadChildEditableObjects();
			AssertEquals("No db hits", 0, newFactory.GetTableHitCount(CusContainerEntryHeaderPivot.Schema.TableName));
			AssertEquals(0, container.PivotsToEntries.Count);
			AssertEquals("No db hits", 0, newFactory.GetTableHitCount(CusContainerEntryHeaderPivot.Schema.TableName));

			newFactory = new BusinessObjectFactory();
			loadedDecMock = newFactory.LoadMoq<BaseJobDeclaration>(declaration.PK);
			loadedDecMock.Setup(m => m.SupportContainerEntryHeaderPivot).Returns(true);
			declaration = loadedDecMock.Object;
			container = declaration.CusContainers.Single();
			container.LoadChildEditableObjects();
			AssertEquals("db hits", 1, newFactory.GetTableHitCount(CusContainerEntryHeaderPivot.Schema.TableName));
			AssertEquals(1, container.PivotsToEntries.Count);
		}

		[TestDate(2022, 8, 1)]
		public void TestFindContainerOnShipmentByContainerNumber_ReloadWhenTimeout()
		{
			var port1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var port2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, port1.RL_RN_NKCountryCode }));
			var port3 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, port1.RL_RN_NKCountryCode, port2.RL_RN_NKCountryCode }));
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = port1.RL_Code;
			shipment.JS_RL_NKDestination = port3.RL_Code;

			var consol1 = (ForwardingConsol)shipment.Consols.AddNew(typeof(ForwardingConsol));
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = port2.RL_Code;
			consol1.JK_RL_NKDischargePort = localPort.RL_Code;
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1234567";

			var line1 = shipment.OuterPackLines.AddNew();
			container1.PackLines.Add(line1);
			line1.JL_ActualWeight = 100m;
			line1.JL_ActualWeightUQ = "LB";
			Factory.Save();

			Db.Connection.ExecuteNonQuery($"UPDATE dbo.JobContainer SET JC_ContainerNum = 'CONT8888888',JC_SystemLastEditTimeUtc=GETUTCDATE(),JC_SystemLastEditUser='~BP' WHERE JC_PK = '{container1.PK}'");

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(31);
			var declaration = GetJobDeclaration();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals(1, declaration.CusContainers.Count);
			var cusContainer = declaration.CusContainers[0];
			AssertEquals("Should reload consol.Containers", "CONT8888888", cusContainer.CO_ContainerNumber);
		}

		public void TestFindContainerOnShipmentByContainerNumber_WhenCO_JCIsInvalid()
		{
			var port1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var port2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, port1.RL_RN_NKCountryCode }));
			var port3 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, port1.RL_RN_NKCountryCode, port2.RL_RN_NKCountryCode }));
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = port1.RL_Code;
			shipment.JS_RL_NKDestination = port3.RL_Code;

			var consol1 = (ForwardingConsol)shipment.Consols.AddNew(typeof(ForwardingConsol));
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = port2.RL_Code;
			consol1.JK_RL_NKDischargePort = localPort.RL_Code;
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1234567";

			var line1 = shipment.OuterPackLines.AddNew();
			container1.PackLines.Add(line1);
			line1.JL_ActualWeight = 100m;
			line1.JL_ActualWeightUQ = "LB";
			Factory.Save();

			var declaration = GetJobDeclaration();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ShipmentSynchroniser.Synchronise(true);
			Factory.Save();

			AssertEquals(1, declaration.CusContainers.Count);
			declaration.CusContainers[0].CO_JC = ZGuid.Invalid; // It's unlikely to happen, just fix in theory
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestContainerLinkingWithJobContainerInClone()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "C1";

			var shipment1 = consol.Shipments.AddNew();
			var line1 = shipment1.OuterPackLines.AddNew();
			container1.PackLines.Add(line1);

			var declaration = GetJobDeclaration();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			cusContainer.CO_ContainerNumber = "C1";
			cusContainer.CO_JC = ZGuid.Empty;
			declaration.JE_JS = shipment1.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration);

			AssertEquals("JobContainer linked", container1, cusContainer.JobContainer);
			AssertEquals("GUID", container1.PK, cusContainer.CO_JC);

			declaration.JE_OverrideFreightDefaults = true;
			cusContainer.CO_ContainerNumber = "C2";

			AssertEquals("JobContainer linked", container1, cusContainer.JobContainer);
			AssertEquals("GUID", container1.PK, cusContainer.CO_JC);

			var clonedContainer = (BaseCusContainer)new CustomsBusinessObjectCloneStrategy(cusContainer, CloneType.TemplateCopy, Factory).Clone();
			AssertEquals("ContainerNumber", "C2", clonedContainer.CO_ContainerNumber);
			AssertNotEquals("Not linked. Is a clone by CloneInternal", container1.PK, clonedContainer.CO_JC);

			var clonedContainerDeep = (BaseCusContainer)new CustomsBusinessObjectCloneStrategy(cusContainer, CloneType.DeepTemplateCopy, Factory).Clone();
			AssertEquals("ContainerNumber", "C2", clonedContainerDeep.CO_ContainerNumber);
			AssertNotEquals("Not linked. Is a clone by CloneInternal", container1.PK, clonedContainerDeep.CO_JC);

			var clonedContainerCountryToCountry = (BaseCusContainer)new CustomsBusinessObjectCloneStrategy(cusContainer, CloneType.CountryToCountryCopy, Factory).Clone();
			AssertEquals("ContainerNumber", "C2", clonedContainerCountryToCountry.CO_ContainerNumber);
			AssertEquals("Not linked. Excluded from copy in CustomsBusinessObjectCloneArgs", ZGuid.Empty, clonedContainerCountryToCountry.CO_JC);

			var clonedContainerWithinShipment = (BaseCusContainer)new CustomsBusinessObjectCloneStrategy(cusContainer, CloneType.CountryToCountryCopyWithinShipment, Factory).Clone();
			AssertEquals("ContainerNumber", "C2", clonedContainerWithinShipment.CO_ContainerNumber);
			AssertEquals("Linked", container1.PK, clonedContainerWithinShipment.CO_JC);
			AssertEquals("JobContainer remains linked.", container1, clonedContainerWithinShipment.JobContainer);
		}

		public void TestITypeDeciderContext()
		{
			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_Code = "CNZ";
			nzCompany.GC_Name = "NZ Company";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_Code = "BNZ";

			CombineAssertions(() =>
			{
				AssertEquals("From CurrentCompany", "ER", (Factory.New<BaseCusContainer>() as ITypeDeciderContext).Country);

				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.NZ.IJobDeclaration>();
				declaration.JE_GB = nzBranch.PK;
				var container = declaration.CusContainers.AddNew();
				AssertEquals("From Declaration", "NZ", (container as ITypeDeciderContext).Country);
			});
		}

		public void TestCO_DataModel_ReportErrorWhenSetToEmpty() => DataModelTestHelper.RunDataModelTest_ReportErrorWhenSetToEmpty<BaseCusContainer>(Factory);

		public void TestCO_DataModel_ReportErrorWhenUpdated() => DataModelTestHelper.RunDataModelTest_ReportErrorWhenUpdated<BaseCusContainer>(Factory);

		public void TestCO_DataModel_CanSaveTwice() => DataModelTestHelper.RunDataModelTest_CanSaveTwice<BaseCusContainer>(Factory);
	}
}
