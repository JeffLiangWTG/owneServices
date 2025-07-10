using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Test
{
	[TestedType(typeof(ConsolidatedDeclaration))]
	sealed class ConsolidatedDeclarationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetConsolidatedEntryMemberID() => CombineAssertions(() =>
		{
			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(new BusinessObjectFactory(), 3);
			var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			var leadDeclarationEntryHeader = leadDeclaration.CusEntryHeader;
			leadDeclarationEntryHeader.EntryNumber = "123";
			var otherDeclaration1 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
			var otherDeclaration1EntryHeader = otherDeclaration1.CusEntryHeader;
			var otherDeclaration2 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[2];
			var otherDeclaration2EntryHeader = otherDeclaration2.CusEntryHeader;
			AssertEquals("leadDeclarationEntryHeader ConsolidatedEntryMemberID initially 0", ZShort.Zero, leadDeclarationEntryHeader.CH_ConsolidatedEntryMemberID);
			AssertEquals("otherDeclaration1EntryHeader ConsolidatedEntryMemberID initially 0", ZShort.Zero, otherDeclaration1EntryHeader.CH_ConsolidatedEntryMemberID);
			AssertEquals("otherDeclaration2EntryHeader ConsolidatedEntryMemberID initially 0", ZShort.Zero, otherDeclaration2EntryHeader.CH_ConsolidatedEntryMemberID);

			var message = Factory.New<TSWMessage>();
			consolidatedDeclaration.Messages.Add(message);
			using (Env.SetTemporaryUserContext(new UserContext(User.ServiceUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				consolidatedDeclaration.SyncStatusAfterMessageProcessing(message);
				AssertEquals("leadDeclarationEntryHeader ConsolidatedEntryMemberID = 1", (ZShort)1, leadDeclarationEntryHeader.CH_ConsolidatedEntryMemberID);
				AssertEquals("otherDeclaration1EntryHeader ConsolidatedEntryMemberID = 2", (ZShort)2, otherDeclaration1EntryHeader.CH_ConsolidatedEntryMemberID);
				AssertEquals("otherDeclaration2EntryHeader ConsolidatedEntryMemberID = 3", (ZShort)3, otherDeclaration2EntryHeader.CH_ConsolidatedEntryMemberID);
			}
		});

		public void TestDefaultAttachDeclarationFilter_NonPeriodic()
		{
			var leadDeclaration = consolidatedDeclaration.LeadDeclaration;
			leadDeclaration.JE_MessageSubType = "ABC";
			leadDeclaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			leadDeclaration.JE_OH_Importer = new ZGuid("29B7C3B6-1578-498A-B107-1C546F0F5FB5");
			leadDeclaration.JE_VesselName = "CSCL Melbourne";
			leadDeclaration.JE_VoyageFlightNo = "0BOADN1MA";
			leadDeclaration.JE_DateOfArrival = new ZDateTime(2024, 12, 21);

			var defaults = consolidatedDeclaration.DefaultAttachDeclarationFilter.ToList<FilterBusinessObjectDefault>();
			CombineAssertions(() =>
			{
				var importerDefault = defaults.Single(x => x.FilterName == "Importer/Supplier");
				var transportModeDefault = defaults.Single(x => x.FilterName == "Transport Mode");
				var messageSubTypeDefault = defaults.Single(x => x.FilterName == "Shipment Sub-Type");
				var voyageVesselComparisonOperatorDefault = defaults.Single(x => x.Key == "Vessel and Flight/Voyage #:ComparisonOperator");
				var vesselNameDefault = defaults.Single(x => x.Key == "Vessel and Flight/Voyage #:NkProperty");
				var voyageNumberDefault = defaults.Single(x => x.Key == "Vessel and Flight/Voyage #:Property");
				var etaDischargePropertySearchDefault = defaults.Single(x => x.Key == "Arrival at Discharge Port:PropertySearch");
				var etaDischargeFromDateDefault = defaults.Single(x => x.Key == "Arrival at Discharge Port:Property1");
				var etaDischargeToDateDefault = defaults.Single(x => x.Key == "Arrival at Discharge Port:Property2");
				AssertEquals("Importer filter value", new ZGuid("29B7C3B6-1578-498A-B107-1C546F0F5FB5"), importerDefault.Value);
				AssertEquals("TransportMode filter value", JobTransportModeList.Codes.Sea, transportModeDefault.Value);
				AssertEquals("MessageSubType filter value", "ABC", messageSubTypeDefault.Value);
				AssertEquals("Voyage Vessel ComparisonOperator default", (ZString)ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact, voyageVesselComparisonOperatorDefault.Value);
				AssertEquals("VesselName filter value", "CSCL Melbourne", vesselNameDefault.Value);
				AssertEquals("VoyageNumber filter value", "0BOADN1MA", voyageNumberDefault.Value);
				AssertEquals("ETA of Discharge filter default", ModuleDateFilter.SpecifiedDateRange, etaDischargePropertySearchDefault.Value);
				AssertEquals("ETA of Discharge filter default", new ZDateTime(2024, 12, 21), etaDischargeFromDateDefault.Value);
				AssertEquals("ETA of Discharge filter default", new ZDateTime(2024, 12, 21), etaDischargeToDateDefault.Value);
			});
		}

		public void TestDefaultAttachDeclarationFilter_Periodic()
		{
			var leadDeclaration = consolidatedDeclaration.LeadDeclaration;
			leadDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
			leadDeclaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			leadDeclaration.JE_OH_Importer = new ZGuid("29B7C3B6-1578-498A-B107-1C546F0F5FB5");
			leadDeclaration.JE_VesselName = "CSCL Melbourne";
			leadDeclaration.JE_VoyageFlightNo = "0BOADN1MA";
			leadDeclaration.JE_DateOfArrival = new ZDateTime(2024, 12, 21);

			var defaults = consolidatedDeclaration.DefaultAttachDeclarationFilter.ToList<FilterBusinessObjectDefault>();
			CombineAssertions(() =>
			{
				var importerDefault = defaults.Single(x => x.FilterName == "Importer/Supplier");
				var transportModeDefault = defaults.Single(x => x.FilterName == "Transport Mode");
				var messageSubTypeDefault = defaults.Single(x => x.FilterName == "Shipment Sub-Type");
				var voyageVesselComparisonOperatorDefault = defaults.Single(x => x.Key == "Vessel and Flight/Voyage #:ComparisonOperator");
				var vesselNameDefault = defaults.Single(x => x.Key == "Vessel and Flight/Voyage #:NkProperty");
				var voyageNumberDefault = defaults.Single(x => x.Key == "Vessel and Flight/Voyage #:Property");
				AssertEquals("Importer filter value", new ZGuid("29B7C3B6-1578-498A-B107-1C546F0F5FB5"), importerDefault.Value);
				AssertEquals("TransportMode filter value", JobTransportModeList.Codes.Sea, transportModeDefault.Value);
				AssertEquals("MessageSubType filter value", "PER", messageSubTypeDefault.Value);
				AssertEquals("Voyage Vessel ComparisonOperator default", (ZString)ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact, voyageVesselComparisonOperatorDefault.Value);
				AssertEquals("VesselName filter value", "CSCL Melbourne", vesselNameDefault.Value);
				AssertEquals("VoyageNumber filter value", "0BOADN1MA", voyageNumberDefault.Value);
				AssertEquals("No Arrival at Discharge Port filter for periodic declaration", false, defaults.Any(x => x.FilterName == "Arrival at Discharge Port"));
			});
		}

		public void TestOnCreatedWithCustomsDeclarations()
		{
			var consolidatedDec = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 4);
			consolidatedDec.JobDeclarations[0].JE_EntryAuthorisationDate = new ZDateTime(2023, 09, 04);
			consolidatedDec.JobDeclarations[1].JE_EntryAuthorisationDate = new ZDateTime(2023, 11, 27);
			consolidatedDec.JobDeclarations[2].JE_EntryAuthorisationDate = new ZDateTime(2023, 02, 15);
			CombineAssertions(() =>
			{
				AssertEquals("Initially CRD_PeriodTo is empty", ZDateTime.Empty, consolidatedDec.CRD_PeriodTo);
				consolidatedDec.OnCreatedWithCustomsDeclarations();
				AssertEquals("CRD_PeriodTo is set to the max JE_EntryAuthorisationDate", new ZDateTime(2023, 11, 27), consolidatedDec.CRD_PeriodTo);
			});
		}

		public void TestEntryStyle()
		{
			CombineAssertions(() =>
			{
				var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
				consolidatedDeclaration.JobDeclarations[0].JE_MessageSubType = consolidatedDeclaration.JobDeclarations[1].JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				AssertEquals("Entry style equals to it of congruent declarations", JobMessageSubTypeList.Codes.Normal, consolidatedDeclaration.EntryStyle);

				consolidatedDeclaration.JobDeclarations[0].JE_MessageSubType = consolidatedDeclaration.JobDeclarations[1].JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
				AssertEquals("Entry style equals to it of congruent declarations", JobMessageSubTypeList.Codes.Periodic, consolidatedDeclaration.EntryStyle);
			});
		}

		public void TestVesselName()
		{
			CombineAssertions(() =>
			{
				consolidatedDeclaration.LeadDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				consolidatedDeclaration.LeadDeclaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
				consolidatedDeclaration.LeadDeclaration.JE_VesselName = "123";
				AssertEquals("Vessel name equals to it of lead declaration", "123", consolidatedDeclaration.VesselName);
				consolidatedDeclaration.LeadDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
				AssertEquals("Vessel name for PER entry", "123", consolidatedDeclaration.VesselName);
			});
		}

		public void TestVoyageFlightNO()
		{
			CombineAssertions(() =>
			{
				consolidatedDeclaration.LeadDeclaration.JE_TransportMode = JobTransportModeList.Codes.Air;
				consolidatedDeclaration.LeadDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				consolidatedDeclaration.LeadDeclaration.JE_VoyageFlightNo = "123";
				AssertEquals("Voyage/Flight equals to it of lead declaration", "123", consolidatedDeclaration.VoyageFlightNo);
				consolidatedDeclaration.LeadDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
				AssertEquals("Voyage/Flight for PER air entry", "123", consolidatedDeclaration.VoyageFlightNo);
				consolidatedDeclaration.LeadDeclaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
				consolidatedDeclaration.LeadDeclaration.JE_VoyageFlightNo = "123";
				AssertEquals("Voyage/Flight empty for PER sea entry", "123", consolidatedDeclaration.VoyageFlightNo);
			});
		}

		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CRD_ApplicationCode", Customs.Business.ConsolidatedDeclaration.ApplicationCodes.TSW, consolidatedDeclaration.CRD_ApplicationCode);
			});
		}

		public void TestCustomsStatusDescription_Caption()
		{
			var propertyInfo = consolidatedDeclaration.GetType().GetProperty(nameof(ConsolidatedDeclaration.CustomsStatusDescription));
			AssertEquals("Customs Status", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestCustomsStatusDescription()
		{
			CombineAssertions(() =>
			{
				consolidatedDeclaration.LeadDeclaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;
				AssertEquals("Display empty for AppliedToConsolidation", "", consolidatedDeclaration.CustomsStatusDescription);

				foreach (CodeDescriptionPair codeDescriptionPair in consolidatedDeclaration.LeadDeclaration.Lookups.EntryStatusList)
				{
					if (codeDescriptionPair.Code == ConsolidatedEntryStatusList.Codes.AppliedToConsolidation)
					{
						continue;
					}
					consolidatedDeclaration.LeadDeclaration.JE_EntryStatus = codeDescriptionPair.Code;
					AssertEquals("Display status as for individual declarations", consolidatedDeclaration.LeadDeclaration.JE_EntryStatusDescription, consolidatedDeclaration.CustomsStatusDescription);
				}
			});
		}

		public void TestAggregateDeclaration()
		{
			CombineAssertions(() =>
			{
				consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
				var declaration1 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[0];
				declaration1.JE_MessageType = consolidatedDeclaration.JobDeclarations[1].JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration1.JE_MasterBill = consolidatedDeclaration.JobDeclarations[1].JE_MasterBill = "MB000";
				declaration1.JE_HouseBill = consolidatedDeclaration.JobDeclarations[1].JE_HouseBill = "HB111";
				var leadDeclaration = consolidatedDeclaration.LeadDeclaration as JobDeclaration;
				leadDeclaration.JE_RL_NKPortOfDeliveryNotify = "12345";
				leadDeclaration.JE_VesselName = "__?__";
				leadDeclaration.JE_VoyageFlightNo = "__?__";
				var invoiceLineChanged = consolidatedDeclaration.JobDeclarations[1].Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
				invoiceLineChanged.JI_HadErrorInLastResponse = true;
				Factory.Save();
				var aggregateDeclaration = consolidatedDeclaration.BuildAggregateJobDeclaration() as JobDeclaration;
				AssertEquals("JE_RL_NKPortOfDeliveryNotify", "12345", aggregateDeclaration.JE_RL_NKPortOfDeliveryNotify);
				AssertEquals("JE_VesselName", consolidatedDeclaration.VesselName, aggregateDeclaration.JE_VesselName);
				AssertEquals("JE_VoyageFlightNo", consolidatedDeclaration.VoyageFlightNo, aggregateDeclaration.JE_VoyageFlightNo);
				AssertEquals("CH_BGMReference is set to CRD_JobReferenceNumber", consolidatedDeclaration.CRD_JobReferenceNumber, aggregateDeclaration.CusEntryHeader.CH_BGMReference);
				AssertContainsExactElementsInAnyOrder("Congruent MB", new[] { "MB000", "HB111", "HB111" }, aggregateDeclaration.Bills.Select(_ => _.CU_BillNum));

				var factoryCapturingChanges = new BusinessObjectFactory();
				consolidatedDeclaration.ImportAggregateDeclaration(aggregateDeclaration, factoryCapturingChanges);
				var changeSet = factoryCapturingChanges.GetChanges();
				AssertContainsExactElementsInAnyOrder("Importing without action does not make factory dirty but for CH_BGMReference", new string[] { "CH" }, changeSet.GetChangedObjects().Select(bizo => bizo.SessionInstance.PKSchemaColumn.ColumnPrefix).Distinct());

				aggregateDeclaration = consolidatedDeclaration.BuildAggregateJobDeclaration() as JobDeclaration;
				foreach (JobComInvoiceLine line in aggregateDeclaration.InvoiceLines)
				{
					line.JI_HadErrorInLastResponse = false;
				}
				aggregateDeclaration.CusEntryHeader.Messages.AddNew();
				factoryCapturingChanges = new BusinessObjectFactory();
				consolidatedDeclaration.ImportAggregateDeclaration(aggregateDeclaration, factoryCapturingChanges);
				AssertEquals("JI_HadErrorInLastResponse changed", false, factoryCapturingChanges.Load<JobComInvoiceLine>(invoiceLineChanged.PK).JI_HadErrorInLastResponse);
				AssertEquals("Log added", 2, factoryCapturingChanges.GetChanges().GetAddedObjects().Where(_ => ((_ as StmALog)?.SL_SE_NKEvent ?? "") == AutoEvents.CustomsCommencedCode).Count());

				factoryCapturingChanges.Save();
				factoryCapturingChanges = new BusinessObjectFactory();
				aggregateDeclaration = consolidatedDeclaration.BuildAggregateJobDeclaration() as JobDeclaration;
				consolidatedDeclaration.ImportAggregateDeclaration(aggregateDeclaration, factoryCapturingChanges);
				AssertEquals("Importing without action does not make factory dirty", 0, factoryCapturingChanges.LastChangeNumber);

				var declaration2 = (JobDeclaration)consolidatedDeclaration.JobDeclarations[1];
				var entryLine1 = declaration1.CusEntryHeader.AllEntryLines.AddNew();
				entryLine1.CL_LineNumber = 1;
				var entryLine2 = declaration1.CusEntryHeader.AllEntryLines.AddNew();
				entryLine2.CL_LineNumber = 2;
				var entryLine3 = declaration2.CusEntryHeader.AllEntryLines.AddNew();
				entryLine3.CL_LineNumber = 1;
				var entryLine4 = declaration2.CusEntryHeader.AllEntryLines.AddNew();
				entryLine4.CL_LineNumber = 2;
				Factory.Save();
				aggregateDeclaration = consolidatedDeclaration.BuildAggregateJobDeclaration() as JobDeclaration;
				AssertContainsExactElementsInExactOrder("Should reassign merged Entry Lines' number by", new short[] { 1, 2, 3, 4 }, aggregateDeclaration.CusEntryHeader.MergedLines.Select(line => (short)line.CL_LineNumber).ToArray());
			});
		}

		public void TestDocumentSupporter()
		{
			AssertType<ConsolidatedDeclarationDocumentSupporter>(consolidatedDeclaration.DocumentSupporter);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => consolidatedDeclaration;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(factory);
		}

		protected override BusinessObject GetNewBusinessObject() => consolidatedDeclaration;

		protected override void SetUp()
		{
			base.SetUp();
			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
		}
		ConsolidatedDeclaration consolidatedDeclaration;
	}
}
