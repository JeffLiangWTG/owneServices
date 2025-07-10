using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using LegType = Enterprise.UniversalDataBuss.DataObjects.Universal.LegType;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ConsolDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestNamespace2012ShipmentsExportOtherParentConsolsEvenWhenTheyAreChildrenOfAConsol()
		{
			var consol1BO = SetupConsolWithOneShipment();
			consol1BO.JK_UniqueConsignRef = "C00002001";

			var shipmentBO = consol1BO.Shipments[0];
			shipmentBO.JS_UniqueConsignRef = "S00002022";

			var consol2BO = shipmentBO.Consols.AddNew();
			consol2BO.JK_UniqueConsignRef = "C00002012";
			consol2BO.JK_AgentType = "CLD";
			consol2BO.JK_TransportMode = "SEA";
			consol2BO.JK_ConsolMode = "LCL";
			consol2BO.JK_AWBServiceLevel = "STD";
			consol2BO.JK_DatePortOfFirstArrival = new ZDateTime(2010, 4, 1);
			consol2BO.JK_IsCFS = true;
			consol2BO.JK_IsForwarding = true;
			consol2BO.JK_IsNeutralMaster = true;
			consol2BO.JK_MasterBillNum = "M333M444";
			consol2BO.JK_MasterBillIssueDate = new ZDateTime(2010, 5, 2);
			consol2BO.JK_RL_NKDischargePort = "FJSUV";
			consol2BO.JK_RL_NKLoadPort = "NZAKL";
			consol2BO.JK_PrepaidCollect = "PPD";

			UniversalShipment consolDataObject;
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol1BO)));
				consolDataObject = writer.GetDataObject(consol1BO);
			}

			CombineAssertions(() =>
			{
				AssertEquals("consolDataObject.DataContext.GetDataSources()", "ForwardingConsol [C00002001]", consolDataObject.DataContext.GetDataSources());
				AssertNotNull("consolDataObject.SubShipmentCollection", consolDataObject.SubShipmentCollection);
				AssertEquals("consolDataObject.SubShipmentCollection.Count", 1, consolDataObject.SubShipmentCollection.Count);
			});

			var shipmentDataObject = consolDataObject.SubShipmentCollection[0];
			CombineAssertions(() =>
			{
				AssertEquals("shipmentDataObject.DataContext.GetDataSources()", "ForwardingShipment [S00002022]", shipmentDataObject.DataContext.GetDataSources());
				AssertNotNull("shipmentDataObject.ParentShipmentCollection", shipmentDataObject.ParentShipmentCollection);
				AssertEquals("shipmentDataObject.ParentShipmentCollection.Count", 1, shipmentDataObject.ParentShipmentCollection.Count);
			});

			var otherConsolDataObject = shipmentDataObject.ParentShipmentCollection[0];
			AssertEquals("otherConsolDataObject.DataContext.GetDataSources()", "ForwardingConsol [C00002012]", otherConsolDataObject.DataContext.GetDataSources());
		}

		public void TestWorkflowCustomFieldsOnConsolAreExported()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();

			consolBO.SetUserDefinedValue("Are you Happy?", ZBool.True);
			consolBO.SetUserDefinedValue("What Makes You Happy?", new ZString("Lots Of Ice"));
			consolBO.SetUserDefinedValue("The Happy Number", new ZInt(42));
			consolBO.SetUserDefinedValue("The Happy Decimal", new ZDecimal(7.7));
			consolBO.SetUserDefinedValue("The Date You Are Happy", ZDateTime.BrettsBirthday);

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var shipmentData = writer.GetDataObject(consolBO);
			AssertNotNull("Precondition: shipmentData", shipmentData);

			var customFields = shipmentData.CustomizedFieldCollection;
			AssertNotNull(customFields);

			CombineAssertions(delegate
			{
				AssertEquals("customFields.Count", 5, customFields.Count);
				customFields.AssertCustomFieldWasExported(DataType.Boolean, "Are you Happy?", "true");
				customFields.AssertCustomFieldWasExported(DataType.String, "What Makes You Happy?", "Lots Of Ice");
				customFields.AssertCustomFieldWasExported(DataType.Integer, "The Happy Number", "42");
				customFields.AssertCustomFieldWasExported(DataType.Decimal, "The Happy Decimal", "7.7");
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "The Date You Are Happy", ZDateTime.BrettsBirthday.ToISO8601String());
			});
		}

		public void TestAdditionalReferences()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
			var additionalReferenceBO = AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(Factory);

			consolBO.Numbers.Add(additionalReferenceBO);

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);
			AssertNotNull("Precondition: consolDataObject", consolDataObject);
			AssertNotNull("Precondition: consolDataObject.AdditionalReferenceCollection", consolDataObject.AdditionalReferenceCollection);

			CombineAssertions(delegate
			{
				AssertEquals("consolDataObject.AdditionalReferenceCollection.Count", 1, consolDataObject.AdditionalReferenceCollection.Count);
				AdditionalReferenceDataObjectWriterTest.AssertContents(consolDataObject.AdditionalReferenceCollection.FirstOrDefault());
			});
		}

		public void TestAdditionalReferences_CarrierContractNumber()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
			consolBO.JK_CarrierContractNumber = "CAR456789";

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);
			AssertNotNull("Precondition: consolDataObject", consolDataObject);
			AssertNotNull("Precondition: consolDataObject.AdditionalReferenceCollection", consolDataObject.AdditionalReferenceCollection);

			CombineAssertions(delegate
			{
				AssertEquals("consolDataObject.AdditionalReferenceCollection.Count", 1, consolDataObject.AdditionalReferenceCollection.Count);

				var reference = consolDataObject.AdditionalReferenceCollection.FirstOrDefault();
				Assertion.AssertEquals("additionalReferenceDataObject.ReferenceNumber", "CAR456789", reference.ReferenceNumber);
				Assertion.AssertEquals("additionalReferenceDataObject.Type.Code", "CON", reference.Type.Code);
				Assertion.AssertEquals("additionalReferenceDataObject.Type.Description", "Carrier Contract Number", reference.Type.Description);
			});
		}

		public void TestMAWBHandlingInformationExtraText()
		{
			using (FreightDataRegistry.Instance.MAWBHandlingInformationExtraText.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "<BillNumber.Find(\"\"{First}\"\" == \"\"1\"\").First()>"))
			{
				var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
				consolBO.JK_TransportMode = TransportModes.Air;
				var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
				AssertExceptionThrown<DataObjectValidationException>("Should not throw ExportAWBHeaderReplaceMacrosException", () => writer.GetDataObject(consolBO));
			}
		}

		public void TestEntryNumbers()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
			var cusEntryNumber = EntryNumberDataObjectWriterTest.SetupCusEntryNumber(consolBO);

			consolBO.CusEntryNums.Add(cusEntryNumber);

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);
			AssertNotNull("Precondition: consolDataObject", consolDataObject);
			AssertNotNull("Precondition: consolDataObject.EntryNumberCollection", consolDataObject.EntryNumberCollection);

			CombineAssertions(delegate
			{
				AssertEquals("consolDataObject.EntryNumberCollection.Count", 1, consolDataObject.EntryNumberCollection.Count);

				var entryNumberDataObject = consolDataObject.EntryNumberCollection[0];
				EntryNumberDataObjectWriterTest.AssertContents(entryNumberDataObject);
			});
		}

		public void TestCarrierBookingOffice()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
			consolBO.JK_TransportMode = "SEA";
			consolBO.JK_RL_NKCarrierBookingOffice = "SYD";

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertEquals("SYD", consolDataObject.CarrierBookingOffice.Code);

			consolBO.JK_TransportMode = "AIR";
			consolDataObject = writer.GetDataObject(consolBO);

			AssertNullOrEmpty(consolDataObject.CarrierBookingOffice?.Code);
		}

		public void TestCarrierBookingLatest_StatusAndDate()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
			consolBO.JK_TransportMode = "SEA";

			var documentData = Factory.NewWithValidTestData<DummyAutoJobDocumentData>();
			documentData[JobDocumentDataSchema.JDD_ParentTableCode] = consolBO.TablePrefix;
			documentData[JobDocumentDataSchema.JDD_ParentID] = consolBO.PK;
			documentData[JobDocumentDataSchema.JDD_Name] = ConsolDocumentDataStoreNames.SeaBookingRequest2;

			var shippingInstructionEventParameter = new[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, ConsolDocumentNames.ShippingInstruction)
			};
			var baseZDateTimeOffset = new ZDateTimeOffset(2022, 02, 28);
			AddLog(documentData, Events.MessageSent, baseZDateTimeOffset, shippingInstructionEventParameter);

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertEquals(Freight.Business.FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Sent, consolDataObject.CarrierBookingLatestStatus.Code);
			AssertEquals(Freight.Business.FreightConstants.CarrierBookingStatus.Description.ShippingInstruction.Sent, consolDataObject.CarrierBookingLatestStatus.Description);
			var postedTime = documentData.Logs.MostRecentLogByPostedDate.SL_PostedTimeUtc;
			AssertEquals(postedTime, consolDataObject.CarrierBookingLatestDate);

			consolBO.JK_TransportMode = "AIR";
			consolDataObject = writer.GetDataObject(consolBO);

			AssertNull(consolDataObject.CarrierBookingLatestStatus);
			AssertNull(consolDataObject.CarrierBookingLatestDate);
		}

		void AddLog(DummyAutoJobDocumentData documentData, ZArchitecture.Business.Event @event, ZDateTimeOffset dateTimeOffset, KeyValuePair<string, string>[] eventParameters)
		{
			documentData.Logs.CreateOrRecreateEventLog(
				@event,
				EstimateActual.Actual,
				dateTimeOffset,
				ZString.Empty,
				eventParameters);

			Factory.Save();
		}

		class DummyAutoJobDocumentData : AutoJobDocumentData
		{
			public DummyAutoJobDocumentData(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}

		public void TestAirConsolHasHyphenatedMAWBOnExport()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
			consolBO.JK_TransportMode = "AIR";
			consolBO.JK_MasterBillNum = "1112222222";

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertNotNull(consolDataObject);
			AssertEquals("consolDataObject.WayBillNumber", "111-2222222", consolDataObject.WayBillNumber);
		}

		public void TestShouldIncludeConsolCostsWhenSendingToOrgProxyOnly()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
			var cost1 = (BusinessObject)Factory.New<IJobConsolCost>();
			cost1[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			cost1.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				cost1[JobConsolCostSchema.E6_ParentID] = consolBO.PK;
				cost1[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				cost1.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			cost1[JobConsolCostSchema.E6_AC_ChargeCode] = Factory.NewWithValidTestData<AccChargeCode>().PK;
			cost1[JobConsolCostSchema.E6_LocalCostAmount] = 100m;
			cost1[JobConsolCostSchema.E6_RX_NKCurrency] = "UGX";
			cost1[JobConsolCostSchema.E6_ExchangeRate] = 2m;

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);
			AssertEquals("Should include ConsolCosts when recipient is OrgProxy", 100m, consolDataObject.ConsolCosts.ConsolCostLineCollection[0].CostLocalAmount);

			eAdaptorRegistry.Instance.UniversalXMLAlwaysIncludeJobCostingInUniversalShipment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.NFP, consolBO)));
			consolDataObject = writer.GetDataObject(consolBO);
			AssertNull("Should not include ConsolCosts when recipient is not OrgProxy and registry is set to only export ConsolCosts to OrgProxy", consolDataObject.ConsolCosts);

			eAdaptorRegistry.Instance.UniversalXMLAlwaysIncludeJobCostingInUniversalShipment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			consolDataObject = writer.GetDataObject(consolBO);
			AssertEquals("Should include ConsolCosts when registry is set to always include ConsolCosts", 100m, consolDataObject.ConsolCosts.ConsolCostLineCollection[0].CostLocalAmount);
		}

		public void TestSubShipmentWithSameContainerAsParentConsolDoesNotExportSameContainer()
		{
			var consolBO = SetupConsolWithOneShipment();
			var containerBO = consolBO.Containers.AddNew();
			containerBO.JC_ContainerNum = "OOCL0000006";
			var shipmentBO = consolBO.Shipments[0];
			var packLine = shipmentBO.OuterPackLines.AddNew();
			packLine.JL_JC = containerBO.PK;
			packLine.JL_PackageCount = 12;
			packLine.JL_F3_NKPackType = Constants.PkgUnit.Package;

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertNotEquals("Precondition: consolDataObject.ContainerCollection", null, consolDataObject.ContainerCollection);
			AssertEquals("Precondition: consolDataObject.ContainerCollection.Count", 1, consolDataObject.ContainerCollection.Count);
			AssertEquals("Precondition: consolDataObject.SubShipmentCollection.Count", 1, consolDataObject.SubShipmentCollection.Count);

			var shipmentDataObject = consolDataObject.SubShipmentCollection[0];
			AssertEquals("shipmentDataObject.ContainerCollection", null, shipmentDataObject.ContainerCollection);
		}

		public void TestSubShipmentWithSameTransportLegsAsParentConsolDoesNotExportSameTransportLeg()
		{
			var consolBO = SetupConsolWithOneShipment();
			var legBO = consolBO.Transports[0];
			var shipmentBO = consolBO.Shipments[0];

			var shipmentLegBO = shipmentBO.Transports.AddNew();
			shipmentLegBO.JW_Vessel = "StinkyWind";
			shipmentLegBO.JW_VoyageFlight = "F4RT";

			Factory.Save();

			var factory = new BusinessObjectFactory();

			var reloadedConsolBO = factory.Load<ForwardingConsol>(consolBO.PK);

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(reloadedConsolBO);

			AssertNotEquals("Precondition: consolDataObject.TransportLegCollection", null, consolDataObject.TransportLegCollection);
			AssertEquals("Precondition: consolDataObject.TransportLegCollection.Count", 1, consolDataObject.TransportLegCollection.Count);

			var consolTransportData = consolDataObject.TransportLegCollection[0];
			AssertEquals("HighWind", consolTransportData.VesselName);
			AssertEquals("SSS111222", consolTransportData.VoyageFlightNo);

			AssertEquals("Precondition: consolDataObject.SubShipmentCollection.Count", 1, consolDataObject.SubShipmentCollection.Count);

			var shipmentDataObject = consolDataObject.SubShipmentCollection[0];
			AssertNotEquals("shipmentDataObject.TransportLegCollection", null, shipmentDataObject.TransportLegCollection);

			var shipmentTransportData = shipmentDataObject.TransportLegCollection[0];
			AssertEquals("StinkyWind", shipmentTransportData.VesselName);
			AssertEquals("F4RT", shipmentTransportData.VoyageFlightNo);
		}

		public void TestSubShipmentsAppearOnTheCorrectLevel()
		{
			var consolBO = SetupConsolWithOneShipment();

			var shipmentBO1 = consolBO.Shipments[0];
			shipmentBO1.JS_HouseBill = "S1";

			var shipmentBO2 = consolBO.Shipments.AddNew();
			shipmentBO2.JS_HouseBill = "S2";

			var subShipmentBO1 = shipmentBO1.CoLoadShipments.AddNew();
			subShipmentBO1.JS_HouseBill = "SS1";

			var subShipmentBO2 = shipmentBO1.CoLoadShipments.AddNew();
			subShipmentBO2.JS_HouseBill = "SS2";

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertNotNull("consolDataObject", consolDataObject);
			AssertNotNull("consolDataObject.SubShipmentCollection", consolDataObject.SubShipmentCollection);
			AssertEquals("consolDataObject.SubShipmentCollection.Count", 2, consolDataObject.SubShipmentCollection.Count);

			var shipmentDataObject1 = consolDataObject.SubShipmentCollection[0];

			AssertEquals("shipmentDataObject1.WayBillNumber", "S1", shipmentDataObject1.WayBillNumber);
			AssertNotNull("shipmentDataObject1.SubShipmentCollection", shipmentDataObject1.SubShipmentCollection);
			AssertEquals("shipmentDataObject1.SubShipmentCollection.Count", 2, shipmentDataObject1.SubShipmentCollection.Count);

			var subShipmentDataObject1 = shipmentDataObject1.SubShipmentCollection[0];

			AssertNull("subShipmentDataObject1.SubShipmentCollection", subShipmentDataObject1.SubShipmentCollection);
			AssertEquals("subShipmentDataObject1.WayBillNumber", "SS1", subShipmentDataObject1.WayBillNumber);

			var subShipmentDataObject2 = shipmentDataObject1.SubShipmentCollection[1];

			AssertNull("subShipmentDataObject2.SubShipmentCollection", subShipmentDataObject2.SubShipmentCollection);
			AssertEquals("subShipmentDataObject2.WayBillNumber", "SS2", subShipmentDataObject2.WayBillNumber);

			var shipmentDataObject2 = consolDataObject.SubShipmentCollection[1];

			AssertNull("shipmentDataObject2.SubShipmentCollection", shipmentDataObject2.SubShipmentCollection);
			AssertEquals("shipmentDataObject2.WayBillNumber", "S2", shipmentDataObject2.WayBillNumber);
		}

		public void TestBasicConsolLevelMappings()
		{
			var consolBO = SetupConsolWithOneShipment();

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertNotNull(consolDataObject);

			CombineAssertions(delegate
			{
				AssertContents(consolDataObject);
			});
		}

		public void TestBasicConsolLevelMappings_ForGatewayCoLoadConsol()
		{
			var consolBO = SetupConsolWithOneShipment();

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertNotNull(consolDataObject);

			CombineAssertions(delegate
			{
				AssertContents(consolDataObject, Constants.AgentType.CoLoad);
			});
		}

		public void TestWithDangerousGoods()
		{
			var consolBO = SetupConsolWithOneShipment();
			consolBO.JK_IsHazardous = true;
			var dangerousGood1 = consolBO.ConsolDGRestrictionCollection.AddNew();
			dangerousGood1.JKD_Class = "2.1";
			dangerousGood1.JKD_UNNO = "1001";

			var dangerousGood2 = consolBO.ConsolDGRestrictionCollection.AddNew();
			dangerousGood2.JKD_Class = "2.2";
			dangerousGood2.JKD_UNNO = "1002";

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertNotNull(consolDataObject);
			AssertNotNull("consolDataObject.PreallocatedUNDGCollection", consolDataObject.PreallocatedUNDGCollection);
			AssertEquals("consolDataObject.PreallocatedUNDGCollection.Count", 2, consolDataObject.PreallocatedUNDGCollection.Count);

			CombineAssertions(delegate
			{
				AssertEquals("consolDataObject.IsHazardous", true, consolDataObject.IsHazardous);
				AssertEquals("consolDataObject.PreallocatedUNDGCollection[0].IMOClass", "2.1", consolDataObject.PreallocatedUNDGCollection[0].IMOClass);
				AssertEquals("consolDataObject.PreallocatedUNDGCollection[0].UNDGCode", "1001", consolDataObject.PreallocatedUNDGCollection[0].UNDGCode);
				AssertEquals("consolDataObject.PreallocatedUNDGCollection[1].IMOClass", "2.2", consolDataObject.PreallocatedUNDGCollection[1].IMOClass);
				AssertEquals("consolDataObject.PreallocatedUNDGCollection[1].UNDGCode", "1002", consolDataObject.PreallocatedUNDGCollection[1].UNDGCode);
			});
		}

		public void TestWithNotes()
		{
			var consolBO = SetupConsolWithOneShipment();
			var shipmentBO = consolBO.Shipments[0];

			var noteBO1 = consolBO.Notes.AddNew(true, "CAT EATER!!", "Feee-lix the cat, what a wonderful-wonderful cat.");
			noteBO1.ST_NoteType = nameof(CargoWise.Definitions.StmNoteVisibility.PUB);
			var noteBO2 = consolBO.Notes.AddNew(false, "Internal Work Notes", "Flintstones, meet the Flintstones.");
			noteBO2.ST_NoteContext = "DEB";

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertNotNull(consolDataObject);
			AssertNotNull("consolDataObject.NoteCollection", consolDataObject.NoteCollection);
			AssertEquals("consolDataObject.NoteCollection.Count", 2, consolDataObject.NoteCollection.Count);

			var note1 = consolDataObject.NoteCollection[0];

			#region Check Contents of Consol Data Object

			CombineAssertions(delegate
			{
				AssertContents(consolDataObject);
				AssertEquals("note1.Description", "CAT EATER!!", note1.Description);
				AssertEquals("note1.IsCustomDescription", ZBool.True, note1.IsCustomDescription);
				AssertEquals("note1.NoteText", "Feee-lix the cat, what a wonderful-wonderful cat.", note1.NoteText);
				AssertEquals("note1.NoteContext.Code", "AAA", note1.NoteContext.Code);
				AssertEquals("note1.NoteContext.Description", "Module: A - All, Direction: A - All, Freight: A - All", note1.NoteContext.Description);
				AssertEquals("note1.Visibility.Code", "PUB", note1.Visibility.Code);
				AssertEquals("note1.Visibility.Description", "CLIENT-VISIBLE", note1.Visibility.Description);
			});

			var note2 = consolDataObject.NoteCollection[1];

			CombineAssertions(delegate
			{
				AssertEquals("note2.Description", "Internal Work Notes", note2.Description);
				AssertEquals("note2.IsCustomDescription", ZBool.False, note2.IsCustomDescription);
				AssertEquals("note2.NoteText", "Flintstones, meet the Flintstones.", note2.NoteText);
				AssertEquals("note2.NoteContext.Code", "DEB", note2.NoteContext.Code);
				AssertEquals("note2.NoteContext.Description", "Module: D - Customs/Declarations, Direction: E - Export, Freight: B - Air and Sea", note2.NoteContext.Description);
				AssertEquals("note2.Visibility.Code", "INT", note2.Visibility.Code);
				AssertEquals("note2.Visibility.Description", "INTERNAL", note2.Visibility.Description);
			});

			#endregion
		}

		public void TestWithLegs()
		{
			var consolBO = SetupConsolWithOneShipment();
			var shipmentBO = consolBO.Shipments[0];

			shipmentBO.JS_RL_NKOrigin = "NZDUD";
			shipmentBO.JS_RL_NKDestination = "AUBDG";

			var leg = consolBO.Transports[0];

			leg.JW_RL_NKLoadPort = "NZCHC";
			leg.JW_ETD = new ZDateTime(2011, 3, 4);
			leg.JW_ATD = new ZDateTime(2011, 3, 5);

			leg.JW_RL_NKDiscPort = "AUSYD";
			leg.JW_ETA = new ZDateTime(2011, 3, 6);
			leg.JW_ATA = new ZDateTime(2011, 3, 7);
			leg.JW_CarrierBookingReference = "BOOKMEUP";

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertNotNull(consolDataObject);

			#region Check Contents of Consol Data Object

			var legData1 = consolDataObject.TransportLegCollection[0];
			CombineAssertions(delegate
			{
				AssertContents(consolDataObject);
				AssertEquals("legData1.LegOrder", new ZByte(1), legData1.LegOrder);
				AssertEquals("legData1.TransportMode", TransportMode.Sea, legData1.TransportMode);
				AssertEquals("legData1.ActualArrival", new ZDateTime(2011, 3, 7), legData1.ActualArrival);
				AssertEquals("legData1.ActualDeparture", new ZDateTime(2011, 3, 5), legData1.ActualDeparture);
				AssertEquals("legData1.CarrierBookingReference", "BOOKMEUP", legData1.CarrierBookingReference);
				AssertEquals("legData1.EstimatedArrival", new ZDateTime(2011, 3, 6), legData1.EstimatedArrival);
				AssertEquals("legData1.EstimatedDeparture", new ZDateTime(2011, 3, 4), legData1.EstimatedDeparture);
				AssertEquals("legData1.LegType", LegType.Main, legData1.LegType);
				AssertEquals("legData1.PortOfDischarge.Code", "AUSYD", legData1.PortOfDischarge.Code);
				AssertEquals("legData1.PortOfDischarge.Name", "Sydney", legData1.PortOfDischarge.Name);
				AssertEquals("legData1.PortOfLoading.Code", "NZCHC", legData1.PortOfLoading.Code);
				AssertEquals("legData1.PortOfLoading.Name", "Christchurch", legData1.PortOfLoading.Name);
				AssertEquals("legData1.VesselName", "HighWind", legData1.VesselName);
				AssertEquals("legData1.VesselLloydsNumber", "9174622", legData1.VesselLloydsIMO);
				AssertEquals("legData1.VoyageFlightNo", "SSS111222", legData1.VoyageFlightNo);
			});

			#endregion
		}

		public void TestGreenhouseGasEmission()
		{
			var consolBO = SetupConsolWithOneShipment();
			consolBO.SetTotalCO2e(31210m);
			consolBO.SetCO2eStatus(CO2eStatusList.Codes.Current);

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = writer.GetDataObject(consolBO);

			AssertNotNull("consolData.GreenhouseGasEmission", consolData.GreenhouseGasEmission);
			AssertEquals(31210m, consolData.GreenhouseGasEmission.CO2e);
			CombineAssertions("GreenhouseGasEmission.CO2eUnit", () =>
			{
				AssertEquals("Code", "KG", consolData.GreenhouseGasEmission.CO2eUnit.Code);
				AssertEquals("Description", "Kilograms", consolData.GreenhouseGasEmission.CO2eUnit.Description);
			});
			AssertNull("Should not write into GreenhouseGasEmission.CO2eStatus", consolData.GreenhouseGasEmission.CO2eStatus);
			CombineAssertions("GreenhouseGasEmission.CO2eDescriptiveStatus", () =>
			{
				AssertEquals("Code", CO2eStatusList.Codes.Current, consolData.GreenhouseGasEmission.CO2eDescriptiveStatus.Code);
				AssertEquals("Description", CO2eHelper.GetCO2eStatusShortDescription(CO2eStatusList.Codes.Current), consolData.GreenhouseGasEmission.CO2eDescriptiveStatus.Description);
			});
		}

		public void TestConsolAdditionalAddressInfo()
		{
			var factory = Factory;
			var consolBO = factory.New<ForwardingConsol>();

			consolBO.JK_OA_PackDepotAddress = factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			consolBO.JK_OA_UnpackDepotAddress = factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			consolBO.CFSDepartureByTransportMode = "ROA";
			consolBO.CFSArrivalByTransportMode = "RAI";

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolData = writer.GetDataObject(consolBO);

			AssertNotNull("consolData.AdditionalAddressInfoCollection is not null", consolData.AdditionalAddressInfoCollection);
			AssertEquals("consolData.AdditionalAddressInfoCollection.Count", 2, consolData.AdditionalAddressInfoCollection.Count);

			var additionalAddressInfo1 = consolData.AdditionalAddressInfoCollection.FirstOrDefault(a => a.AddressType.ToString() == nameof(DocAddressType.DepartureCFSAddress));
			AssertNotNull("additionalAddressInfo1 is not null", additionalAddressInfo1);
			AssertEquals("additionalAddressInfo1.TransportMode.Code", "ROA", additionalAddressInfo1.TransportMode.Code);

			var additionalAddressInfo2 = consolData.AdditionalAddressInfoCollection.FirstOrDefault(a => a.AddressType.ToString() == nameof(DocAddressType.ArrivalCFSAddress));
			AssertNotNull("additionalAddressInfo2 is not null", additionalAddressInfo2);
			AssertEquals("additionalAddressInfo2.TransportMode.Code", "RAI", additionalAddressInfo2.TransportMode.Code);
		}

		public void TestWithCommodity()
		{
			var consolBO = SetupConsolWithOneShipment();
			consolBO.JK_RH_NKConsolCommodity = "WOOD";

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertEquals("WOOD", consolDataObject.ConsolCommodity.Code);
		}

		[TestDate(2017, 07, 03, 13, 01, 01)]
		public void TestWithContainers()
		{
			var unlimitedFreeDaysOptions = new ContainerPenaltyFreeDaysOptions { UnlimitedFreeDays = true };
			using (Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForExport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			using (Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			{
				var consolBO = SetupConsolWithOneShipment();

				var containerCommodity = ShipmentDataObjectWriterTest.GetCommodity(Factory, "FGFG");
				var consolCommodity = ShipmentDataObjectWriterTest.GetCommodity(Factory, "XYXY");

				var container1 = consolBO.Containers.AddNew();
				ContainerDataObjectWriterTest.PopulateContainer1(container1, containerCommodity, consolCommodity, Factory);

				AssertEquals("Precondition: container1.GoodsWeightForBinding", 68806272.028m, container1.GoodsWeightForBinding);
				AssertEquals("Precondition: container1.WeightUnitForBinding", "LB", container1.WeightUnitForBinding);

				var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
				var consolDataObject = writer.GetDataObject(consolBO);
				AssertNotNull("Precondition: consolDataObject", consolDataObject);

				AssertNotNull("consolDataObject.ContainerCollection", consolDataObject.ContainerCollection);
				AssertEquals("consolDataObject.ContainerCollection.Count", 1, consolDataObject.ContainerCollection.Count);
				CombineAssertions(delegate
				{
					AssertEquals("containerData1.GrossWeight", 68806418.928m, consolDataObject.ContainerCollection[0].GrossWeight);
					AssertEquals("containerData1.GoodsWeight", 68806272.028m, consolDataObject.ContainerCollection[0].GoodsWeight);
					ContainerDataObjectWriterTest.AssertContainer1(consolDataObject.ContainerCollection[0], containerCommodity.RH_Code, consolCommodity.RH_Code);
				});
			}
		}

		public void TestPackLineToContainerLink_PackLinesHasContainers_LinkShouldPointToContainer()
		{
			var consolBO = SetupConsolWithOneShipment();
			var shipmentBO = consolBO.Shipments[0];

			var commodity1 = ShipmentDataObjectWriterTest.GetCommodity(Factory, "HAM", "McLaren - CHEMPION!");
			var commodity2 = ShipmentDataObjectWriterTest.GetCommodity(Factory, "BUT", "McLaren - CHEMPION!");

			var container1 = consolBO.Containers.AddNew();
			var container2 = consolBO.Containers.AddNew();

			ContainerDataObjectWriterTest.PopulateContainer1(container1, commodity1, Factory);
			ContainerDataObjectWriterTest.PopulateContainer1(container2, commodity2, Factory);

			var packLine1 = shipmentBO.OuterPackLines.AddNew();
			var packLine2 = shipmentBO.OuterPackLines.AddNew();
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine1);
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine2);

			packLine1.SetContainer(container1.PK);
			packLine1.JL_RH_NKCommodityCode = commodity1.RH_Code;

			packLine2.SetContainer(container2.PK);
			packLine2.JL_RH_NKCommodityCode = commodity2.RH_Code;

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);
			var packLineCollection = consolDataObject.SubShipmentCollection[0].PackingLineCollection;
			var hamPackLine = packLineCollection.FirstOrDefault(c => c.Commodity.Code.Value == "HAM");
			var butPackLine = packLineCollection.FirstOrDefault(c => c.Commodity.Code.Value == "BUT");

			AssertNotNull("shipmentData.ContainerCollection", consolDataObject.ContainerCollection);
			AssertEquals("shipmentData.ContainerCollection.Count", 2, consolDataObject.ContainerCollection.Count);
			AssertEquals("Container1 has link", true, consolDataObject.ContainerCollection[0].Link.HasValue);
			AssertEquals("Container2 has link", true, consolDataObject.ContainerCollection[1].Link.HasValue);
			AssertNotNull("shipmentData.PackingLineCollection", packLineCollection);
			AssertEquals("shipmentData.PackingLineCollection.Count", 3, packLineCollection.Count);
			AssertEquals("PackLine1 has link to container", true, hamPackLine.ContainerLink.HasValue);
			AssertEquals("PackLine2 has link to container", true, butPackLine.ContainerLink.HasValue);
			AssertEquals("PackLine1 is linked to appropriate container", consolDataObject.ContainerCollection.SingleOrDefault(c => c.Commodity.Code.Value == "HAM").Link, hamPackLine.ContainerLink);
			AssertEquals("PackLine2 is linked to appropriate container", consolDataObject.ContainerCollection.SingleOrDefault(c => c.Commodity.Code.Value == "BUT").Link, butPackLine.ContainerLink);
		}

		public void TestConsolsWithOrgAddresses()
		{
			var organizationBO1 = Factory.New<OrgHeader>();
			organizationBO1.OH_FullName = "NZ COMPANY";
			organizationBO1.OH_RL_NKClosestPort = "NZAKL";
			organizationBO1.OH_Code = "NZCOMPAKL";

			var mainAddressBO1 = organizationBO1.MainAddress;
			mainAddressBO1.OA_Address1 = "123 Main Address Road";
			mainAddressBO1.OA_City = "Auckland";
			mainAddressBO1.OA_PostCode = "1234";

			var organizationBO2 = Factory.New<OrgHeader>();
			organizationBO2.OH_FullName = "NZ ORGANIZATION";
			organizationBO2.OH_RL_NKClosestPort = "NZAKL";
			organizationBO2.OH_Code = "NZORGAKL";

			var mainAddressBO2 = organizationBO2.MainAddress;
			mainAddressBO2.OA_Address1 = "456 Main Address Road";
			mainAddressBO2.OA_City = "Auckland";
			mainAddressBO2.OA_PostCode = "1234";

			var organizationBO3 = Factory.New<OrgHeader>();
			organizationBO3.OH_FullName = "HK ORGANIZATION";
			organizationBO3.OH_RL_NKClosestPort = "HKHKG";
			organizationBO3.OH_Code = "HKORGHKG";

			var mainAddressBO3 = organizationBO3.MainAddress;
			mainAddressBO3.OA_Address1 = "789 Main Address Road";
			mainAddressBO3.OA_City = "HongKong";
			mainAddressBO3.OA_PostCode = "5678";

			Factory.Save();

			var consolBO = SetupConsolWithOneShipment();
			consolBO.JK_OA_ArrivalCTOAddress = mainAddressBO1.PK;
			consolBO.JK_OA_ShippingLineAddress = mainAddressBO2.PK;
			consolBO.JK_OA_CreditorAddress = mainAddressBO3.PK;

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertNotNull(consolDataObject);

			#region Check Contents of Consol Data Object

			CombineAssertions(delegate
			{
				var addressData1 = consolDataObject.OrganizationAddressCollection.Find(a => a.OrganizationCode.Value == "NZCOMPAKL");
				AssertEquals("Address1", "123 Main Address Road", addressData1.Address1);
				AssertEquals("City", "Auckland", addressData1.City);
				AssertEquals("Postcode", "1234", addressData1.Postcode);
				AssertEquals("AddressType", "ArrivalCTOAddress", addressData1.AddressType);

				var addressData2 = consolDataObject.OrganizationAddressCollection.Find(a => a.OrganizationCode.Value == "NZORGAKL");
				AssertEquals("Address1", "456 Main Address Road", addressData2.Address1);
				AssertEquals("City", "Auckland", addressData2.City);
				AssertEquals("Postcode", "1234", addressData2.Postcode);
				AssertEquals("AddressType", "ShippingLineAddress", addressData2.AddressType);

				var addressData3 = consolDataObject.OrganizationAddressCollection.Find(a => a.AddressType.Value == "Creditor");
				AssertNotNull(addressData3);
				AssertEquals("Address1", "789 Main Address Road", addressData3.Address1);
				AssertEquals("City", "HongKong", addressData3.City);
				AssertEquals("Postcode", "5678", addressData3.Postcode);
				AssertEquals("OrganizationCode", "HKORGHKG", addressData3.OrganizationCode.Value);

				var addressData4 = consolDataObject.OrganizationAddressCollection.Find(a => a.AddressType.Value == "CoLoadWith");
				AssertNotNull(addressData4);
				AssertEquals("Address1", "789 Main Address Road", addressData4.Address1);
				AssertEquals("City", "HongKong", addressData4.City);
				AssertEquals("Postcode", "5678", addressData4.Postcode);
				AssertEquals("OrganizationCode", "HKORGHKG", addressData3.OrganizationCode.Value);

				AssertContents(consolDataObject);
			});

			#endregion
		}

		public void TestConsolForeignPorts()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
			consolBO.JK_RL_NKFirstForeignPort = "CNSHA";
			consolBO.JK_RL_NKLastForeignPort = "MXCUU";
			Factory.Save();

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);
			AssertNotNull("Precondition: consolDataObject was created", consolDataObject);
			AssertNotNull("consolDataObject.PortFirstForeign", consolDataObject.PortFirstForeign);
			AssertNotNull("consolDataObject.PortLastForeign", consolDataObject.PortLastForeign);

			AssertEquals("consolDataObject.JK_RL_NKFirstForeignPort", "CNSHA", consolDataObject.PortFirstForeign.Code);
			AssertEquals("consolDataObject.JK_RL_NKFirstForeignPort", "Shanghai Hongqiao International Apt", consolDataObject.PortFirstForeign.Name);
			AssertEquals("consolDataObject.JK_RL_NKLastForeignPort", "MXCUU", consolDataObject.PortLastForeign.Code);
			AssertEquals("consolDataObject.JK_RL_NKLastForeignPort", "Chihuahua", consolDataObject.PortLastForeign.Name);
		}

		public void TestIContainerParentDataObjectWriter()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
			consolBO.JK_RL_NKFirstForeignPort = "CNSHA";
			consolBO.JK_RL_NKLastForeignPort = "MXCUU";

			var container = consolBO.Containers.AddNew();
			container.JC_ContainerNum = "CONT123";
			var container2 = consolBO.Containers.AddNew();
			container2.JC_ContainerNum = "CONT456";

			Factory.Save();

			var writer = new ContainerTopLevelDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, container)));
			var data = writer.GetDataObject(container);
			AssertEquals("ContainerCollection.Count", 1, data.ContainerCollection.Count);
			AssertEquals("Should contain container", "CONT123", data.ContainerCollection[0].ContainerNumber);
			AssertEquals("Content should be Partial", CollectionContent.Partial, data.ContainerCollection.Content);
		}

		public void TestMultiAWBMasterConsol_AWBCoload()
		{
			AssertMultiAWBMasterConsol(Constants.AgentType.AWBCoload);
		}

		public void TestMultiAWBMasterConsol_Direct()
		{
			AssertMultiAWBMasterConsol(Constants.AgentType.Direct);
		}

		void AssertMultiAWBMasterConsol(string agentType)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00002001";
			consol.JK_AgentType = Constants.AgentType.AWBMaster;

			var subConsol1 = consol.ColoadConsols.AddNew();
			subConsol1.JK_UniqueConsignRef = "C00002002";
			subConsol1.JK_AgentType = agentType;

			var subConsol2 = consol.ColoadConsols.AddNew();
			subConsol2.JK_UniqueConsignRef = "C00002003";
			subConsol2.JK_AgentType = agentType;

			var shipment1 = subConsol1.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00002001";

			Factory.Save();

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)));
			var consolDataObject = writer.GetDataObject(consol);

			AssertEquals("2 sub-consols", 2, consolDataObject.SubShipmentCollection.Count);
			AssertEquals("1 sub-shipment on C00002002", 1, consolDataObject.SubShipmentCollection[0].SubShipmentCollection.Count);
			AssertNull("0 sub-shipments on C00002003", consolDataObject.SubShipmentCollection[1].SubShipmentCollection);

			AssertEquals("C00002001", "ForwardingConsol [C00002001]", consolDataObject.DataContext.GetDataSources());
			AssertEquals("C00002002", "ForwardingConsol [C00002002]", consolDataObject.SubShipmentCollection[0].DataContext.GetDataSources());
			AssertEquals("C00002003", "ForwardingConsol [C00002003]", consolDataObject.SubShipmentCollection[1].DataContext.GetDataSources());
			AssertEquals("S00002001", "ForwardingShipment [S00002001]", consolDataObject.SubShipmentCollection[0].SubShipmentCollection[0].DataContext.GetDataSources());
		}

		public void TestAWBMasterWithNoColoadConsols_SubshipmentsIsNotSet()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00002001";
			consol.JK_AgentType = Constants.AgentType.AWBMaster;

			Factory.Save();

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)));

			AssertNoExceptionThrown(() =>
			{
				var consolDataObject = writer.GetDataObject(consol);
				AssertNull("SubShipmentCollection should not be set", consolDataObject.SubShipmentCollection);
			});
		}

		public void TestDataWriteOptions_IncludeSubShipments()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)));
			var consolDataObject = writer.GetDataObject(consol);
			AssertEquals(1, consolDataObject.SubShipmentCollection.Count);

			var dataWriterOptions = new DataWriterOptions()
			{
				IncludeSubShipments = false
			};

			writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)), null, dataWriterOptions);
			consolDataObject = writer.GetDataObject(consol);
			AssertNull(consolDataObject.SubShipmentCollection);
		}

		public void TestDataWriteOptions_IncludeContainers()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT123";

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)));
			var consolDataObject = writer.GetDataObject(consol);
			AssertEquals(1, consolDataObject.ContainerCollection.Count);

			var dataWriterOptions = new DataWriterOptions()
			{
				IncludeContainers = false
			};

			writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)), null, dataWriterOptions);
			consolDataObject = writer.GetDataObject(consol);
			AssertNull(consolDataObject.ContainerCollection);
		}

		public void TestJK_SendingForwaderHandlingType_Export()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
			consolBO.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertEquals("Sending Type has been exported", AgentStatusList.Codes.GatewayAgent, consolDataObject.SendingForwarderHandlingType.Code);
		}

		public void TestJK_ReceivingForwaderHandlingType_UpdatesOnConsolidation()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
			consolBO.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertEquals("Sending Type has been exported", AgentStatusList.Codes.GatewayAgent, consolDataObject.ReceivingForwarderHandlingType.Code);
		}

		public void TestHandlingTypes_UpdatesOnConsolidation()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
			consolBO.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consolBO.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertEquals("Sending Type has been exported", AgentStatusList.Codes.GatewayAgent, consolDataObject.SendingForwarderHandlingType.Code);
			AssertEquals("Receiving Type has been exported", AgentStatusList.Codes.GatewayAgent, consolDataObject.ReceivingForwarderHandlingType.Code);
		}

		public void TestHandlingTypesAreEmpty_ConsolidationIsUnchanged()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();

			AssertEquals("Precondition: Handling types are empty", ZString.Empty, consolBO.JK_SendingForwarderHandlingType);
			AssertEquals("Precondition: Handling types are empty", ZString.Empty, consolBO.JK_ReceivingForwarderHandlingType);

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertEquals("Sending Type has been exported", ZString.Empty, consolDataObject.SendingForwarderHandlingType.Code);
			AssertEquals("Receiving Type has been exported", ZString.Empty, consolDataObject.ReceivingForwarderHandlingType.Code);
		}

		public void TestSpecialHandlingCodesExported()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_TransportMode = Constants.TransportModes.Air;

			var sh1 = consolBO.AWBSpecialHandlingItems.AddNew();
			sh1.JKH_Code = "ACT";
			var sh2 = consolBO.AWBSpecialHandlingItems.AddNew();
			sh2.JKH_Code = "PER";

			Factory.Save();

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertEquals("ACT", consolDataObject.SpecialHandlingCollection[0].Code);
			AssertEquals("PER", consolDataObject.SpecialHandlingCollection[1].Code);
		}
		public void TestExcessiveOuterPackCount()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_OuterPacks = 999999999;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_OuterPacks = 999999999;
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_OuterPacks = 999999999;

			AssertEquals("Precondition: JK_TotalShipmentQuantity", 2999999997m, consol.JK_TotalShipmentQuantity);
			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)));
			AssertExceptionThrown<DataObjectValidationException>("JK_TotalShipmentQuantity", () => writer.GetDataObject(consol));
		}

		public void TestExcessiveInnerPackCount()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TotalPackageCount = 999999999;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TotalPackageCount = 999999999;
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_TotalPackageCount = 999999999;

			AssertEquals("Precondition: JK_TotalShipmentPackageCount", 2999999997m, consol.JK_TotalShipmentPackageCount);
			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)));
			AssertExceptionThrown<DataObjectValidationException>("JK_TotalShipmentPackageCount", () => writer.GetDataObject(consol));
		}

		public void TestGatewayServiceLevel()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_RS_NKGatewayServiceLevel = "DIR";

			Factory.Save();

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertEquals("DIR", consolDataObject.GatewayServiceLevel.Code);
		}

		public void TestTotalLoadingMeters()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_LoadingMeters = 5;

			AssertEquals("Precondition: JK_TotalShipmentLoadingMeters is populated", 5m, consol.JK_TotalShipmentLoadingMeters);

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)));
			var shipmentData = writer.GetDataObject(consol);

			AssertEquals("TotalLoadingMeters is populated", 5m, shipmentData.TotalLoadingMeters);
		}

		public void TestTotalLoadingMeters_NoValue()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			AssertEquals("Precondition: JK_TotalShipmentLoadingMeters is zero", 0m, consol.JK_TotalShipmentLoadingMeters);

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)));
			var shipmentData = writer.GetDataObject(consol);

			AssertNull("TotalLoadingMeters is nll", shipmentData.TotalLoadingMeters);
		}

		#region Test

		public void TestNoVessel()
		{
			var consolBO = Factory.New<TestForwardingConsol>();
			consolBO.JK_TransportMode = Constants.TransportModes.Air;

			var transportCollection = consolBO.Transports;

			Assert(!((TestConsolTransportCollection)transportCollection).IsUpdatingByDataRefreshBusExposed);

			transportCollection.AddNew();
			AssertEquals(2, transportCollection.Count);

			transportCollection.RemoveAll();
			AssertEquals("created by transportCollection.AddInitialTransportIfEmpty()", 1, transportCollection.Count);

			transportCollection.AddNew();
			AssertEquals(2, transportCollection.Count);

			((TestConsolTransportCollection)transportCollection).IsUpdatingByDataRefreshBusExposed = true;
			Assert(((TestConsolTransportCollection)transportCollection).IsUpdatingByDataRefreshBusExposed);

			transportCollection.RemoveAll();
			AssertEquals(0, transportCollection.Count);

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			AssertNoExceptionThrown(() =>
			{
				var consolDataObject = writer.GetDataObject(consolBO);
				AssertEquals("LloydsIMO is empty.", ZString.Empty, consolDataObject.LloydsIMO);
			});
		}

		class TestForwardingConsol : ForwardingConsol
		{
			public TestForwardingConsol(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[ChildEditable(false)]
			public override ConsolTransportCollection Transports
			{
				get
				{
					if (fTransports == null)
					{
						using (SuspendSettingHasChanges())
						{
							using (GetValidationSuspender())
							{
								fTransports = new TestConsolTransportCollection(this);
								fTransports.Load();
								RegisterEditableChildObject(fTransports, "Routing");
								fTransports.DepartureTransport.JW_TerminalCutOffInfo.ValueChanged += JW_TerminalCutOffInfo_ValueChanged;
								fTransports.DepartureTransport.JW_DepotCutOffInfo.ValueChanged += JW_DepotCutOffInfo_ValueChanged;
								fTransports.CountChanged += OnTransports_CountChanged;
							}
						}
					}

					return fTransports;
				}
			}

			void OnTransports_CountChanged(object sender, CollectionCountChangedEventArgs e)
			{
				(from CommonContainer container in Containers
				 where !container.IsDeleted
				 select container).ForEach(delegate(CommonContainer x)
				 {
					 x.Validation.ValidateJC_GrossWeightVerificationType();
				 });
			}

			void JW_DepotCutOffInfo_ValueChanged(object sender, EventArgs e)
			{
				if (JK_MasterBillIssueDate.IsEmpty && FreightConfigurationRegistry.Instance.AWBIssueDate.Value == FreightConfigurationRegistry.Instance.AWBIssueDateIsCutOffDate && JK_ConsolMode != "FCL")
				{
					JK_MasterBillIssueDate = Transports.DepartureTransport.JW_DepotCutOff;
				}
			}

			void JW_TerminalCutOffInfo_ValueChanged(object sender, EventArgs e)
			{
				if (JK_MasterBillIssueDate.IsEmpty && FreightConfigurationRegistry.Instance.AWBIssueDate.Value == FreightConfigurationRegistry.Instance.AWBIssueDateIsCutOffDate && JK_ConsolMode == "FCL")
				{
					JK_MasterBillIssueDate = Transports.DepartureTransport.JW_TerminalCutOff;
				}
			}

			ConsolTransportCollection fTransports;
		}

		class TestConsolTransportCollection : ConsolTransportCollection
		{
			public TestConsolTransportCollection(CommonConsol parent) : base(parent)
			{ }

			public bool IsUpdatingByDataRefreshBusExposed
			{
				get { return IsUpdatingByDataRefreshBus; }
				set { IsUpdatingByDataRefreshBus = value; }
			}
		}

		#endregion

		public void TestBasicConsolLevelMappingsWithMultipleShipments()
		{
			var consolBO = SetupConsolWithMultipleShipment();

			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertNotNull(consolDataObject);

			CombineAssertions(delegate
			{
				AssertEquals("consolDataObject.OuterPacks", 4, consolDataObject.OuterPacks);
				AssertEquals("consolDataObject.TotalNoOfPacks", 6, consolDataObject.TotalNoOfPacks);
			});
		}

		public void TestEventBranchHomePort()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
			var consolDataObject = writer.GetDataObject(consolBO);

			AssertEquals(GlbBranch.CurrentBranch.HomePort.Code, consolDataObject.EventBranchHomePort.Code);
			AssertEquals(GlbBranch.CurrentBranch.HomePort.RL_PortName, consolDataObject.EventBranchHomePort.Name);
		}

		#region Implementation

		static void AssertContents(UniversalShipment consolDataObject, string agentType = Constants.AgentType.CoLoad)
		{
			AssertEquals("consolDataObject.ContainerCount", 0, consolDataObject.ContainerCount);
			AssertEquals("consolDataObject.AgentsReference", "Agent Bob", consolDataObject.AgentsReference);
			AssertEquals("consolDataObject.ShipmentType.Code", agentType, consolDataObject.ShipmentType.Code);
			AssertEquals("consolDataObject.ShipmentType.Description", agentType == Constants.AgentType.CoLoad ? Constants.AgentTypeDescriptions.CoLoad : Constants.AgentTypeDescriptions.CoLoad, consolDataObject.ShipmentType.Description);
			AssertEquals("consolDataObject.AWBServiceLevel.Code", "STD", consolDataObject.AWBServiceLevel.Code);
			AssertEquals("consolDataObject.AWBServiceLevel.Description", "Standard", consolDataObject.AWBServiceLevel.Description);
			AssertEquals("consolDataObject.BookingConfirmationReference", "Booking please", consolDataObject.BookingConfirmationReference);
			AssertEquals("consolDataObject.CarrierContractNumber", "CCA69420", consolDataObject.CarrierContractNumber);
			AssertEquals("consolDataObject.ContainerMode.Code", "LCL", consolDataObject.ContainerMode.Code);
			AssertEquals("consolDataObject.ContainerMode.Description", "Less Container Load", consolDataObject.ContainerMode.Description);
			AssertEquals("consolDataObject.DocumentedChargeable", 34320m, consolDataObject.DocumentedChargeable);
			AssertEquals("consolDataObject.DocumentedVolume", 12.12m, consolDataObject.DocumentedVolume);
			AssertEquals("consolDataObject.DocumentedWeight", 34320000m, consolDataObject.DocumentedWeight);
			AssertEquals("consolDataObject.FreightRate", 8m, consolDataObject.FreightRate);
			AssertEquals("consolDataObject.FreightRateCurrency.Code", "AUD", consolDataObject.FreightRateCurrency.Code);
			AssertEquals("consolDataObject.FreightRateCurrency.Description", "Australian Dollar", consolDataObject.FreightRateCurrency.Description);
			AssertEquals("consolDataObject.ManifestedChargeable", 10020m, consolDataObject.ManifestedChargeable);
			AssertEquals("consolDataObject.ManifestedVolume", 32.16m, consolDataObject.ManifestedVolume);
			AssertEquals("consolDataObject.ManifestedWeight", 10020000m, consolDataObject.ManifestedWeight);
			AssertEquals("consolDataObject.TotalVolume", 12.43m, consolDataObject.TotalVolume);
			AssertEquals("consolDataObject.TotalWeight", 31210000m, consolDataObject.TotalWeight);
			AssertEquals("consolDataObject.TotalWeightUnit.Code", "KG", consolDataObject.TotalWeightUnit.Code);
			AssertEquals("consolDataObject.TotalWeightUnit.Description", "Kilograms", consolDataObject.TotalWeightUnit.Description);
			AssertEquals("consolDataObject.TotalVolumeUnit.Code", "CF", consolDataObject.TotalVolumeUnit.Code);
			AssertEquals("consolDataObject.TotalVolumeUnit.Description", "Cubic Feet", consolDataObject.TotalVolumeUnit.Description);
			AssertEquals("consolDataObject.OuterPacks", 3, consolDataObject.OuterPacks);
			consolDataObject.DateCollection.AssertDateExists(DateType.FirstArrivalInCountry, ZBool.False, new ZDateTime(2010, 1, 1));
			consolDataObject.DateCollection.AssertDateExists(DateType.ShippedOnBoard, ZBool.False, new ZDateTime(2010, 3, 3));
			AssertEquals("consolDataObject.IsDirectBooking", false, consolDataObject.IsDirectBooking);
			AssertEquals("consolDataObject.IsCFSRegistered", true, consolDataObject.IsCFSRegistered);
			AssertEquals("consolDataObject.IsForwardRegistered", true, consolDataObject.IsForwardRegistered);
			AssertEquals("consolDataObject.IsNeutralMaster", true, consolDataObject.IsNeutralMaster.Value);
			AssertEquals("consolDataObject.WayBillNumber", "M111M222", consolDataObject.WayBillNumber);
			AssertEquals("consolDataObject.WayBillType.Code", "MWB", consolDataObject.WayBillType.Code);
			AssertEquals("consolDataObject.Description", "Master Waybill", consolDataObject.WayBillType.Description);
			consolDataObject.DateCollection.AssertDateExists(DateType.BillIssued, ZBool.False, new ZDateTime(2010, 2, 2));
			AssertEquals("consolDataObject.NoCopyBills", new ZByte(3), consolDataObject.NoCopyBills);
			AssertEquals("consolDataObject.NoOriginalBills", new ZByte(4), consolDataObject.NoOriginalBills);
			AssertEquals("consolDataObject.ReleaseType.Code", "CAD", consolDataObject.ReleaseType.Code);
			AssertEquals("consolDataObject.ReleaseType.Description", "Cash Against Documents", consolDataObject.ReleaseType.Description);
			AssertEquals("consolDataObject.TotalNoOfPacks", 4, consolDataObject.TotalNoOfPacks);
			AssertEquals("consolDataObject.TotalNoOfPacksPackageType.Code", Constants.PkgUnit.Package, consolDataObject.TotalNoOfPacksPackageType.Code);
			AssertEquals("consolDataObject.TotalNoOfPacksPackageType.Description", "Package", consolDataObject.TotalNoOfPacksPackageType.Description);
			AssertEquals("consolDataObject.PaymentMethod.Code", "PPD", consolDataObject.PaymentMethod.Code);
			AssertEquals("consolDataObject.PaymentMethod.Description", "Prepaid", consolDataObject.PaymentMethod.Description);
			AssertEquals("consolDataObject.PortOfDischarge.Code", "NZAKL", consolDataObject.PortOfDischarge.Code);
			AssertEquals("consolDataObject.PortOfDischarge.Name", "Auckland", consolDataObject.PortOfDischarge.Name);
			AssertEquals("consolDataObject.PlaceOfDelivery.Code", "NZAKL", consolDataObject.PlaceOfDelivery.Code);
			AssertEquals("consolDataObject.PlaceOfDelivery.Name", "Auckland", consolDataObject.PlaceOfDelivery.Name);
			AssertEquals("consolDataObject.PlaceOfIssue.Code", "NZAKL", consolDataObject.PlaceOfIssue.Code);
			AssertEquals("consolDataObject.PlaceOfIssue.Name", "Auckland", consolDataObject.PlaceOfIssue.Name);
			AssertEquals("consolDataObject.PortOfLoading.Code", "AUMEL", consolDataObject.PortOfLoading.Code);
			AssertEquals("consolDataObject.PortOfLoading.Name", "Melbourne", consolDataObject.PortOfLoading.Name);
			AssertEquals("consolDataObject.PlaceOfReceipt.Code", "AUMEL", consolDataObject.PlaceOfReceipt.Code);
			AssertEquals("consolDataObject.PlaceOfReceipt.Name", "Melbourne", consolDataObject.PlaceOfReceipt.Name);
			AssertEquals("consolDataObject.PortOfFirstArrival.Code", "AUSYD", consolDataObject.PortOfFirstArrival.Code);
			AssertEquals("consolDataObject.PortOfFirstArrival.Name", "Sydney", consolDataObject.PortOfFirstArrival.Name);
			AssertEquals("consolDataObject.PortFirstForeign.Name", "CNSHA", consolDataObject.PortFirstForeign.Code);
			AssertEquals("consolDataObject.PortFirstForeign.Code", "Shanghai Hongqiao International Apt", consolDataObject.PortFirstForeign.Name);
			AssertEquals("consolDataObject.PortLastForeign.Name", "USCHI", consolDataObject.PortLastForeign.Code);
			AssertEquals("consolDataObject.PortLastForeign.Code", "Chicago", consolDataObject.PortLastForeign.Name);
			consolDataObject.DateCollection.AssertDateExists(DateType.FirstForeignArrival, ZBool.False, new ZDateTime(2010, 4, 4));
			consolDataObject.DateCollection.AssertDateExists(DateType.LastForeignDeparture, ZBool.False, new ZDateTime(2010, 5, 5));
			consolDataObject.DateCollection.AssertDateExists(DateType.DepartureReceiptRequested, ZBool.False, new ZDateTime(2017, 8, 9));
			consolDataObject.DateCollection.AssertDateExists(DateType.ArrivalReceiptRequested, ZBool.False, new ZDateTime(2017, 8, 11));
			consolDataObject.DateCollection.AssertDateExists(DateType.DepartureDispatchRequested, ZBool.False, new ZDateTime(2017, 8, 13));
			consolDataObject.DateCollection.AssertDateExists(DateType.ArrivalDispatchRequested, ZBool.False, new ZDateTime(2017, 8, 15));
			AssertEquals("consolDataObject.ScreeningStatus.Code", "UNK", consolDataObject.ScreeningStatus.Code);
			AssertEquals("consolDataObject.ScreeningStatus.Description", "Unknown", consolDataObject.ScreeningStatus.Description);
			AssertEquals("consolDataObject.VesselName", "HighWind", consolDataObject.VesselName);
			AssertEquals("consolDataObject.VesselLloydsNumber", "9174622", consolDataObject.LloydsIMO);
			AssertEquals("consolDataObject.VoyageFlightNo", "SSS111222", consolDataObject.VoyageFlightNo);
			AssertEquals("consolDataObject.TransportMode.Code", "SEA", consolDataObject.TransportMode.Code);
			AssertEquals("consolDataObject.TransportMode.Description", "Sea Freight", consolDataObject.TransportMode.Description);
			AssertEquals("consolDataObject.TotalPreallocatedWeight", 1m, consolDataObject.TotalPreallocatedWeight);
			AssertEquals("consolDataObject.TotalPreallocatedWeightUnit", Constants.Weight.Milligrams, consolDataObject.TotalPreallocatedWeightUnit.Code);
			AssertEquals("consolDataObject.TotalPreallocatedVolume", 2m, consolDataObject.TotalPreallocatedVolume);
			AssertEquals("consolDataObject.TotalPreallocatedVolumeUnit", Constants.Volume.CubicCentimeters, consolDataObject.TotalPreallocatedVolumeUnit.Code);
			AssertEquals("consolDataObject.TotalPreallocatedChargeable", 3m, consolDataObject.TotalPreallocatedChargeable);
			consolDataObject.DateCollection.AssertDateExists(DateType.CutOffDate, ZBool.False, new ZDateTime(1953, 03, 05));
			AssertEquals("consolDataObject.CorrectedWeight", 5m, consolDataObject.CarrierCorrectedWeight);
			AssertEquals("consolDataObject.CorrectedWeightUnit", Constants.Weight.Kilograms, consolDataObject.CarrierCorrectedWeightUnit.Code);
			AssertEquals("consolDataObject.CorrectedVolume", 6m, consolDataObject.CarrierCorrectedVolume);
			AssertEquals("consolDataObject.CorrectedVolumeUnit", Constants.Volume.CubicMetres, consolDataObject.CarrierCorrectedVolumeUnit.Code);
			AssertEquals("consolDataObject.CorrectedChargeable", 7m, consolDataObject.CarrierCorrectedChargeable);
			AssertEquals("consolDataObject.ChargeableRate", 8m, consolDataObject.ChargeableRate);
			AssertEquals("consolDataObject.CoLoadMasterBillNumber", "COLOADMASTERBILL1", consolDataObject.CoLoadMasterBillNumber);
			AssertEquals("consolDataObject.CoLoadBookingConfirmationReference", "COLOADREF1", consolDataObject.CoLoadBookingConfirmationReference);
			AssertEquals("consolDataObject.IsHazardous", false, consolDataObject.IsHazardous);
			AssertEquals("consolDataObject.RequiresTemperatureControl", true, consolDataObject.RequiresTemperatureControl);
			AssertEquals("consolDataObject.RequiredTemperatureMinimum", (ZDecimal)1.0, consolDataObject.RequiredTemperatureMinimum);
			AssertEquals("consolDataObject.RequiredTemperatureMaximum", (ZDecimal)100.0, consolDataObject.RequiredTemperatureMaximum);
			AssertEquals("consolDataObject.RequiredTemperatureUnit", Core.Constants.Temperature.Centigrade, consolDataObject.RequiredTemperatureUnit.Code);
			AssertEquals("consolDataObject.MaximumAllowablePackageLength", (ZDecimal)100.0, consolDataObject.MaximumAllowablePackageLength);
			AssertEquals("consolDataObject.MaximumAllowablePackageWidth", (ZDecimal)100.0, consolDataObject.MaximumAllowablePackageWidth);
			AssertEquals("consolDataObject.MaximumAllowablePackageHeight", (ZDecimal)100.0, consolDataObject.MaximumAllowablePackageHeight);
			AssertEquals("consolDataObject.MaximumAllowablePackageLengthUnit", Constants.Length.Centimetres, consolDataObject.MaximumAllowablePackageLengthUnit.Code);
			AssertEquals("consolDataObject.ElectronicBillOfLadingReference", "TestData_ElectronicBillOfLadingReference", consolDataObject.ElectronicBillOfLadingReference);
		}

		ForwardingConsol SetupConsolWithOneShipment()
		{
			var consolBO = Factory.New<ForwardingConsol>();

			consolBO.JK_AgentType = Constants.AgentType.CoLoad;
			consolBO.JK_TransportMode = "SEA";
			consolBO.JK_ConsolMode = "LCL";
			consolBO.JK_AWBServiceLevel = "STD";
			consolBO.JK_AgentsReference = "Agent Bob";
			consolBO.JK_BookingReference = "Booking please";
			consolBO.JK_CarrierContractNumber = "CCA69420";
			consolBO.JK_DatePortOfFirstArrival = new ZDateTime(2010, 1, 1);
			consolBO.JK_IsCFS = true;
			consolBO.JK_IsForwarding = true;
			consolBO.JK_IsNeutralMaster = true;
			consolBO.JK_MasterBillNum = "M111M222";
			consolBO.JK_MasterBillIssueDate = new ZDateTime(2010, 2, 2);
			consolBO.JK_ReleaseType = "CAD";
			consolBO.JK_RL_NKDischargePort = "NZAKL";
			consolBO.JK_RL_NKLoadPort = "AUMEL";
			consolBO.JK_RL_NKPortOfFirstArrival = "AUSYD";
			consolBO.JK_RL_NKMasterBillIssuePlace = "NZAKL";
			consolBO.JK_PrepaidCollect = "PPD";
			consolBO.JK_ScreeningStatus = "UNK";
			consolBO.JK_ShippedOnBoardDate = new ZDateTime(2010, 3, 3);
			consolBO.JK_NoCopyBills = 3;
			consolBO.JK_NoOriginalBills = 4;
			consolBO.JK_TotalShipmentActWeightCheck = 1m;
			consolBO.JK_OverrideConsolChargeable = true;
			consolBO.WeightVerificationUnit = Constants.Weight.Milligrams;
			consolBO.JK_TotalShipmentActVolumeCheck = 2m;
			consolBO.VolumeVerificationUnit = Constants.Volume.CubicCentimeters;
			consolBO.JK_TotalShipmentChargableCheck = 3m;
			consolBO.JK_ConsolCutOffDate = new ZDateTime(1953, 03, 05);
			consolBO.JK_CorrectedConsolWeight = 5m;
			consolBO.JK_CorrectedConsolWeightUnit = Constants.Weight.Kilograms;
			consolBO.JK_CorrectedConsolVolume = 6m;
			consolBO.JK_CorrectedConsolVolumeUnit = Constants.Volume.CubicMetres;
			consolBO.JK_ConsolChargeable = 7m;
			consolBO.JK_ConsolChargeableRate = 8m;
			consolBO.JK_OverrideConsolChargeable = true;
			consolBO.JK_RL_NKFirstForeignPort = "CNSHA";
			consolBO.JK_RL_NKLastForeignPort = "USCHI";
			consolBO.JK_DateFirstForeignPort = new ZDateTime(2010, 4, 4);
			consolBO.JK_DateLastForeignPort = new ZDateTime(2010, 5, 5);
			consolBO.JK_CoLoadBookingReference = "COLOADREF1";
			consolBO.JK_CoLoadMasterBill = "COLOADMASTERBILL1";
			consolBO.JK_RequiresTemperatureControl = true;
			consolBO.JK_RequiredTemperatureMinimum = 1.0;
			consolBO.JK_RequiredTemperatureMaximum = 100.0;
			consolBO.JK_RequiredTemperatureUnit = Core.Constants.Temperature.Centigrade;
			consolBO.JK_MaximumAllowablePackageLength = 100;
			consolBO.JK_MaximumAllowablePackageWidth = 100;
			consolBO.JK_MaximumAllowablePackageHeight = 100;
			consolBO.JK_MaximumAllowablePackageUnit = Constants.Length.Centimetres;
			consolBO.JK_PackDepotReceiptRequested = new ZDateTime(2017, 8, 9);
			consolBO.JK_UnpackDepotReceiptRequested = new ZDateTime(2017, 8, 11);
			consolBO.JK_PackDepotDispatchRequested = new ZDateTime(2017, 8, 13);
			consolBO.JK_UnpackDepotDispatchRequested = new ZDateTime(2017, 8, 15);
			consolBO.JK_ElectronicBillOfLadingReference = "TestData_ElectronicBillOfLadingReference";

			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_DocumentedChargeable = 23.45m;
			shipmentBO.JS_DocumentedVolume = 12.12m;
			shipmentBO.JS_DocumentedWeight = 34.32m;
			shipmentBO.JS_ManifestedChargeable = 13.32m;
			shipmentBO.JS_ManifestedVolume = 32.16m;
			shipmentBO.JS_ManifestedWeight = 10.02m;
			shipmentBO.JS_UnitOfVolume = "CF";
			shipmentBO.JS_ActualVolume = 12.43m;
			shipmentBO.JS_UnitOfWeight = "KT";
			shipmentBO.JS_ActualWeight = 31.21m;
			shipmentBO.JS_TotalPackageCount = 4;
			shipmentBO.JS_OuterPacks = 3;
			shipmentBO.JS_TransportMode = "SEA";
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD";

			consolBO.Shipments.Add(shipmentBO);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "HighWind";
			vessel.RV_LloydsNumber = "9174622";

			var transportBO = consolBO.Transports[0];
			transportBO.JW_Vessel = vessel.RV_FK;
			transportBO.JW_VoyageFlight = "SSS111222";

			return consolBO;
		}

		ForwardingConsol SetupConsolWithMultipleShipment()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_AgentType = Constants.AgentType.CoLoad;
			consolBO.JK_TransportMode = "SEA";
			consolBO.JK_ConsolMode = "LCL";
			consolBO.JK_AWBServiceLevel = "STD";
			consolBO.JK_AgentsReference = "Agent Bob";
			consolBO.JK_BookingReference = "Booking please";
			consolBO.JK_CarrierContractNumber = "CCA69420";
			consolBO.JK_DatePortOfFirstArrival = new ZDateTime(2010, 1, 1);
			consolBO.JK_IsCFS = true;
			consolBO.JK_IsForwarding = true;
			consolBO.JK_IsNeutralMaster = true;
			consolBO.JK_MasterBillNum = "M111M222";
			consolBO.JK_MasterBillIssueDate = new ZDateTime(2010, 2, 2);
			consolBO.JK_ReleaseType = "CAD";
			consolBO.JK_RL_NKDischargePort = "NZAKL";
			consolBO.JK_RL_NKLoadPort = "AUMEL";
			consolBO.JK_RL_NKPortOfFirstArrival = "AUSYD";
			consolBO.JK_RL_NKMasterBillIssuePlace = "NZAKL";
			consolBO.JK_PrepaidCollect = "PPD";
			consolBO.JK_ScreeningStatus = "UNK";
			consolBO.JK_ShippedOnBoardDate = new ZDateTime(2010, 3, 3);
			consolBO.JK_NoCopyBills = 3;
			consolBO.JK_NoOriginalBills = 4;
			consolBO.JK_TotalShipmentActWeightCheck = 1m;
			consolBO.JK_OverrideConsolChargeable = true;
			consolBO.WeightVerificationUnit = Constants.Weight.Milligrams;
			consolBO.JK_TotalShipmentActVolumeCheck = 2m;
			consolBO.VolumeVerificationUnit = Constants.Volume.CubicCentimeters;
			consolBO.JK_TotalShipmentChargableCheck = 3m;
			consolBO.JK_ConsolCutOffDate = new ZDateTime(1953, 03, 05);
			consolBO.JK_CorrectedConsolWeight = 5m;
			consolBO.JK_CorrectedConsolWeightUnit = Constants.Weight.Kilograms;
			consolBO.JK_CorrectedConsolVolume = 6m;
			consolBO.JK_CorrectedConsolVolumeUnit = Constants.Volume.CubicMetres;
			consolBO.JK_ConsolChargeable = 7m;
			consolBO.JK_ConsolChargeableRate = 8m;
			consolBO.JK_OverrideConsolChargeable = true;
			consolBO.JK_RL_NKFirstForeignPort = "CNSHA";
			consolBO.JK_RL_NKLastForeignPort = "USCHI";
			consolBO.JK_DateFirstForeignPort = new ZDateTime(2010, 4, 4);
			consolBO.JK_DateLastForeignPort = new ZDateTime(2010, 5, 5);
			consolBO.JK_CoLoadBookingReference = "COLOADREF1";
			consolBO.JK_CoLoadMasterBill = "COLOADMASTERBILL1";
			consolBO.JK_RequiresTemperatureControl = true;
			consolBO.JK_RequiredTemperatureMinimum = 1.0;
			consolBO.JK_RequiredTemperatureMaximum = 100.0;
			consolBO.JK_RequiredTemperatureUnit = Core.Constants.Temperature.Centigrade;
			consolBO.JK_MaximumAllowablePackageLength = 100;
			consolBO.JK_MaximumAllowablePackageWidth = 100;
			consolBO.JK_MaximumAllowablePackageHeight = 100;
			consolBO.JK_MaximumAllowablePackageUnit = Constants.Length.Centimetres;
			consolBO.JK_PackDepotReceiptRequested = new ZDateTime(2017, 8, 9);
			consolBO.JK_UnpackDepotReceiptRequested = new ZDateTime(2017, 8, 11);
			consolBO.JK_PackDepotDispatchRequested = new ZDateTime(2017, 8, 13);
			consolBO.JK_UnpackDepotDispatchRequested = new ZDateTime(2017, 8, 15);
			consolBO.JK_ElectronicBillOfLadingReference = "TestData_ElectronicBillOfLadingReference";

			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_DocumentedChargeable = 23.45m;
			shipmentBO.JS_DocumentedVolume = 12.12m;
			shipmentBO.JS_DocumentedWeight = 34.32m;
			shipmentBO.JS_ManifestedChargeable = 13.32m;
			shipmentBO.JS_ManifestedVolume = 32.16m;
			shipmentBO.JS_ManifestedWeight = 10.02m;
			shipmentBO.JS_UnitOfVolume = "CF";
			shipmentBO.JS_ActualVolume = 12.43m;
			shipmentBO.JS_UnitOfWeight = "KT";
			shipmentBO.JS_ActualWeight = 31.21m;
			shipmentBO.JS_TotalPackageCount = 4;
			shipmentBO.JS_OuterPacks = 3;
			shipmentBO.JS_TransportMode = "SEA";
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD";

			var shipmentBO2 = Factory.New<ForwardingShipment>();
			shipmentBO2.JS_DocumentedChargeable = 23.45m;
			shipmentBO2.JS_DocumentedVolume = 12.12m;
			shipmentBO2.JS_DocumentedWeight = 34.32m;
			shipmentBO2.JS_ManifestedChargeable = 13.32m;
			shipmentBO2.JS_ManifestedVolume = 32.16m;
			shipmentBO2.JS_ManifestedWeight = 10.02m;
			shipmentBO2.JS_UnitOfVolume = "CF";
			shipmentBO2.JS_ActualVolume = 12.43m;
			shipmentBO2.JS_UnitOfWeight = "KT";
			shipmentBO2.JS_ActualWeight = 31.21m;
			shipmentBO2.JS_TotalPackageCount = 2;
			shipmentBO2.JS_OuterPacks = 1;
			shipmentBO2.JS_TransportMode = "SEA";
			shipmentBO2.JS_PackingMode = "LCL";
			shipmentBO2.JS_ShipmentType = "STD";

			consolBO.Shipments.Add(shipmentBO);
			consolBO.Shipments.Add(shipmentBO2);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "HighWind";
			vessel.RV_LloydsNumber = "9174622";

			var transportBO = consolBO.Transports[0];
			transportBO.JW_Vessel = vessel.RV_FK;
			transportBO.JW_VoyageFlight = "SSS111222";

			return consolBO;
		}

		#endregion
	}
}
