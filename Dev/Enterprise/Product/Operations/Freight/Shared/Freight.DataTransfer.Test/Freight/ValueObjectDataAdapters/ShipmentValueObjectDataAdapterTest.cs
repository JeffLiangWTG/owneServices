using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.DataAdapters.Testing;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.SDF;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using SystemDataRegistry = Enterprise.Registry.Business.SystemDataRegistry;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	public abstract class ShipmentValueObjectDataAdapterTest<TBusinessObject, TConsol> : ValueObjectDataAdapterTest<TBusinessObject, Xsd.Shipment>
			where TBusinessObject : CommonShipment
			where TConsol : CommonConsol
	{
		public void TestGetNewOrganisationValueObjectDataAdapter()
		{
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			OrganisationValueObjectDataAdapter orgAdapter = adapter.GetNewOrganisationValueObjectDataAdapter(shipment);
			AssertNotNull(orgAdapter);
			AssertEquals(shipment, orgAdapter.DocAddressesParent);
		}

		#region TestImportGoodsDescription

		public void TestImportGoodsDescription_ShortOnly()
		{
			GenericImportGoodsDescriptionTest(
				"Short Description 01234567890123456",
				"Short Description 0123456789012345678901",
				"Short Description 0123456789012345678901",
				""
			);
		}

		public void TestImportGoodsDescription_LongOnly()
		{
			GenericImportGoodsDescriptionTest(
				"Long Description 012345678901234567",
				"Long Description 0123456789012345678901",
				"",
				"Long Description 0123456789012345678901"
			);
		}

		public void TestImportGoodsDescription_Both()
		{
			GenericImportGoodsDescriptionTest(
				"Short Description 01234567890123456",
				"Long Description 0123456789012345678901",
				"Short Description 0123456789012345678901",
				"Long Description 0123456789012345678901"
			);
		}

		public void TestImportGoodsDescription_Neither()
		{
			GenericImportGoodsDescriptionTest("", "", "", "");
		}

		public void TestImportGoodsDescription_DoesntCreateUnnecessaryNotes()
		{
			#region Setup

			const string longDescription = "This is longer than the maximum allowed by JS_GoodsDescription";
			const string shortDescription = "This is short enough to fit";
			var longDescriptionTruncated = longDescription.Substring(0, JobShipmentSchema.JS_GoodsDescription.MaxLength);

			var xsdShipmentLongDesc = new Xsd.Shipment();
			xsdShipmentLongDesc.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			xsdShipmentLongDesc.ShipmentDetails.PortOfOrigin.Port.Value = "SGSIN";
			xsdShipmentLongDesc.ShipmentDetails.PortofDestination.Port.Value = "AUBNE";

			var xsdShipmentShortDesc = new Xsd.Shipment();
			xsdShipmentShortDesc.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			xsdShipmentShortDesc.ShipmentDetails.PortOfOrigin.Port.Value = "SGSIN";
			xsdShipmentShortDesc.ShipmentDetails.PortofDestination.Port.Value = "AUBNE";

			var importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var valueAdapter = GetNewShipmentValueObjectDataAdapter();

			#endregion

			var shipmentBOLongDesc = Factory.New<TBusinessObject>();
			xsdShipmentLongDesc.ShipmentDetails.GoodsDescription = longDescription;
			valueAdapter.ImportFromValueObject(shipmentBOLongDesc, xsdShipmentLongDesc, importContext);
			AssertEquals("Long Description was truncated", longDescriptionTruncated, shipmentBOLongDesc.JS_GoodsDescription);
			AssertEquals("Shipment Has Notes", true, shipmentBOLongDesc.Notes.HasNotes);
			AssertEquals("Detailed Goods Description Note is not truncated", longDescription, shipmentBOLongDesc.DetailedGoodsDescriptionNoteText);

			var shipmentBOShortDesc = Factory.New<TBusinessObject>();
			xsdShipmentShortDesc.ShipmentDetails.GoodsDescription = shortDescription;
			valueAdapter.ImportFromValueObject(shipmentBOShortDesc, xsdShipmentShortDesc, importContext);
			AssertEquals("Short Description was NOT truncated", shortDescription, shipmentBOShortDesc.JS_GoodsDescription);
			AssertEquals("Shipment Doesn't have Notes", false, shipmentBOShortDesc.Notes.HasNotes);
			AssertEquals("Detailed Goods Description is empty", "", shipmentBOShortDesc.DetailedGoodsDescriptionNoteText);
		}

		void GenericImportGoodsDescriptionTest(
			ZString expectedShortDescription, ZString expectedLongDescription,
			ZString providedShortDescription, ZString providedLongDescription)
		{
			Xsd.Shipment shipment = new Xsd.Shipment();
			shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			shipment.ShipmentDetails.PortOfOrigin.Port.Value = "SGSIN";
			shipment.ShipmentDetails.PortofDestination.Port.Value = "AUBNE";
			shipment.ShipmentDetails.GoodsDescription = providedShortDescription;

			if (!providedLongDescription.IsEmpty)
			{
				Xsd.NotesNote note = shipment.Notes.AddNew();
				note.NoteType = Xsd.NotesNoteNoteType.DetailedGoodsDescription;
				note.NoteData = providedLongDescription;
			}

			TBusinessObject shipmentBO = Factory.New<TBusinessObject>();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			adapter.ImportFromValueObject(shipmentBO, shipment, context);

			AssertEquals("Short", expectedShortDescription, shipmentBO.JS_GoodsDescription);
			AssertEquals("Long", expectedLongDescription, shipmentBO.DetailedGoodsDescriptionNoteText);
		}

		#endregion

		public void TestWarningsReturnedIfUpdateInactiveShipment()
		{
			TestWarningsReturnedIfUpdateInactiveShipmentCore();
		}

		protected virtual void TestWarningsReturnedIfUpdateInactiveShipmentCore()
		{
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			var existingShipment = Factory.NewWithValidTestData<TBusinessObject>(TestBusinessObjectKind.MinimumRequiredToSave);
			existingShipment.JS_HouseBill = "HouseBill";
			existingShipment.JS_IsCancelled = true;
			existingShipment.JS_GoodsDescription = "";

			Factory.Save();

			//registry is set to update existing shipment with shipment no in XML
			SystemRegistry.ImportShipmentNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.ShipmentNumber);

			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetails.AgentReference = existingShipment.JobNumber;
			xmlShipment.ShipmentDetailsSpecified = true;
			xmlShipment.ShipmentDetails.GoodsDescription = "CANDY";
			Xsd.ShipmentIdentifier identifier = xmlShipment.ShipmentIdentifier.AddNew();
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			identifier.Value = "HouseBill";

			var buffer = new NotificationBuffer();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			AssertEquals("notification has no warning", false, buffer.HasWarnings);
			var result = adapter.CreateOrUpdateFromValueObject(xmlShipment, context);
			AssertEquals("shipment is not updated", "", existingShipment.JS_GoodsDescription);
			AssertEquals("notification has warnings", true, buffer.HasWarnings);
			ZString expectedWarningMesg = String.Format("Cannot update Shipment {0} as it is flagged as inactive.", existingShipment.JS_UniqueConsignRef);
			INotification warning = buffer.Events.FirstOrDefault(x => x.Message.Contains(expectedWarningMesg));
			AssertNotNull("Expected Warning message should returned", warning);

			buffer.Clear();

			//registry is set to update existing shipment with shipment no fallback to housebill
			SystemRegistry.ImportShipmentNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.ShipmentThenHouseBill);
			AssertEquals("notification has no warning", false, buffer.HasWarnings);
			result = adapter.CreateOrUpdateFromValueObject(xmlShipment, context);
			AssertEquals("shipment is not updated", "", existingShipment.JS_GoodsDescription);
			AssertEquals("notification has warnings", true, buffer.HasWarnings);
			warning = buffer.Events.FirstOrDefault(x => x.Message.Contains(expectedWarningMesg));
			AssertNotNull("Expected Warning message should returned", warning);

			buffer.Clear();
			existingShipment.JS_IsCancelled = false;
			Factory.Save();
			result = adapter.CreateOrUpdateFromValueObject(xmlShipment, context);
			AssertEquals("shipment is updated", "CANDY", existingShipment.JS_GoodsDescription);
			AssertEquals("notification has warnings", false, buffer.HasWarnings);
		}

		[ExpectNoExceptions]
		public void TestExporterStatementAndShipmentWithDocData()
		{
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			shipment.DocsAndCartage.JP_ExportStatement = "XXX";

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			NotificationBuffer notify = new NotificationBuffer();

			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetails.AgentReference = "AgentReference";
			xmlShipment.ShipmentDetailsSpecified = true;
			adapter.ExportToValueObject(shipment, xmlShipment, new ValueObjectExportContext(notify));
			AssertEquals("XXX", xmlShipment.ShipmentDetails.ExporterStatement);

			TBusinessObject newShipment = Factory.New<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(newShipment, xmlShipment, context);
			AssertEquals("XXX", newShipment.DocsAndCartage.JP_ExportStatement);
		}

		public void TestOnlyOperationalEventsAreImported()
		{
			TBusinessObject importedShipment = Factory.New<TBusinessObject>();
			StmALog log = importedShipment.Logs.MostRecentLogByEventTime(Events.DataExport);
			AssertNull("Precondition: no DEX event", log);

			Xsd.Shipment xsdShipment = new Xsd.Shipment();
			Xsd.ShipmentIdentifier identifier = xsdShipment.ShipmentIdentifier.AddNew();
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			identifier.Value = "HOUSEBILL";

			// Create events (2 operational and 2 admin)
			Xsd.Events xsdEvents = new Xsd.Events();

			// DataExport Event
			Xsd.Event xsdEvent = xsdEvents.Event.AddNew();
			xsdEvent.Code = Events.DataExport.Code;
			xsdEvent.DateTime = ZDateTime.Now;
			// OrderShipped Event
			Xsd.Event bookedEvent = xsdEvents.Event.AddNew();
			bookedEvent.Code = Events.Booked.Code;
			bookedEvent.DateTime = ZDateTime.Now;

			// Add Event
			Xsd.Event addEvent = xsdEvents.Event.AddNew();
			addEvent.Code = Events.AddedARecordToTheSystem.Code;
			addEvent.DateTime = ZDateTime.Now;
			// Edit Event
			Xsd.Event editEvent = xsdEvents.Event.AddNew();
			editEvent.Code = Events.EditedARecord.Code;
			editEvent.DateTime = ZDateTime.Now;

			xsdShipment.Events = xsdEvents;

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(importedShipment, xsdShipment, context);

			StmALog[] dataExportLog = importedShipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("DataExport Event should have been imported", 1, dataExportLog.Length);
			StmALog[] bookedLog = importedShipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Booked.Code));
			AssertEquals("Booked Event should have been imported", 1, bookedLog.Length);

			StmALog[] addLog = importedShipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystem.Code));
			AssertEquals("Add Event should NOT have been imported", 0, addLog.Length);
			StmALog[] editLog = importedShipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code));
			AssertEquals("Edit Event should NOT have been imported", 0, editLog.Length);
		}

		public void TestErrorsReturnedIfNoAgentReference()
		{
			var adapter = GetNewShipmentValueObjectDataAdapter();
			Factory.Save();

			SystemRegistry.ImportShipmentNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.ShipmentNumber);

			var xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetails.AgentReference = string.Empty;

			var buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, buffer);

			AssertEquals("notification has no error", false, buffer.HasErrors);

			adapter.CreateOrUpdateFromValueObject(xmlShipment, context);
			AssertEquals("notification has errors", true, buffer.HasErrors);

			var expectedWarningMesg = "Registry 'System -> Data Import Settings -> Shipment -> Shipment Matching Criteria' is set to match on Agent's Reference however no Agent's Reference (Shipment Number) is provided in the XML file.";
			var expectedEvent = buffer.Events.FirstOrDefault(x => x.Message.Contains(expectedWarningMesg));
			AssertNotNull("Expected event message should returned", expectedEvent);
		}

		public void TestImportAgentReferencePutInShipmentRef()
		{
			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.ShipmentNumber);

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			NotificationBuffer notify = new NotificationBuffer();

			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetails.AgentReference = "AgentReference";

			TBusinessObject shipment = NewBusinessObject();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals("Should populate the unique shipment ref with agent reference", "AGENTREFERENCE", shipment.JS_UniqueConsignRef);
		}

		public void TestCreateOrUpdateFromValueObject_ConsolIsSpecified_UpdateShipmentIfAttachedToSameConsol()
		{
			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.ShipmentNumber);

			var xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetailsSpecified = true;
			xmlShipment.ShipmentDetails.AgentReference = "MCLAREN";
			xmlShipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;

			var shipment = Factory.New<TBusinessObject>();
			shipment.JS_UniqueConsignRef = "MCLAREN";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var shipmentConsol = shipment.Consols.AddNew();
			shipmentConsol.JK_UniqueConsignRef = "ManUtd";

			Factory.Save();

			// Shipment attached to same consol
			var notificationsLogger = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notificationsLogger);
			var adapter = new ShipmentValueObjectDataAdapter<TBusinessObject>(shipmentConsol);
			adapter.CreateOrUpdateFromValueObject(xmlShipment, context);
			AssertEquals("Shipment should have been updated", true, shipment.JS_TransportMode == Core.Constants.TransportModes.Sea);
			AssertEquals("Warning should have been generated", false, notificationsLogger.HasWarnings);

			// Shipment attached to another consol
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "ROONEY";

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			Factory.Save();

			adapter = new ShipmentValueObjectDataAdapter<TBusinessObject>(consol);
			adapter.CreateOrUpdateFromValueObject(xmlShipment, context);
			AssertEquals("Shipment should have been updated", false, shipment.JS_TransportMode == Core.Constants.TransportModes.Sea);
			AssertEquals("Warning should have been generated", true, notificationsLogger.HasWarnings);

			// Shipment is not attached to consol
			shipment.Consols.RemoveAll();

			Factory.Save();

			adapter.CreateOrUpdateFromValueObject(xmlShipment, context);
			AssertEquals("Shipment should have been updated", false, shipment.JS_TransportMode == Core.Constants.TransportModes.Sea);
			AssertEquals("Warning should have been generated", true, notificationsLogger.HasWarnings);
		}

		public void TestImportShipment_AgentReferenceHasNormalLength_ShouldUpdateTheAgentReference()
		{
			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.ShipmentNumber);

			var consol = Factory.New<TConsol>();
			var xmlShipment = CreateXsdShipmentForImportWithMinimumData();
			xmlShipment.ShipmentDetails.AgentReference = "McLaren";

			var adapterToTest = new ShipmentValueObjectDataAdapter<TBusinessObject>(consol);
			var notify = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notify);

			adapterToTest.CreateOrUpdateFromValueObject(xmlShipment, context);

			AssertEquals("Should not has errors", false, notify.HasErrors);
			AssertEquals("Should not update the agent reference", "MCLAREN", consol.Shipments[0].JS_UniqueConsignRef);
		}

		public void TestImportShipment_AgentReferenceIsTooLong_NotifyError()
		{
			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.ShipmentNumber);

			var consol = Factory.New<TConsol>();
			var xmlShipment = CreateXsdShipmentForImportWithMinimumData();
			xmlShipment.ShipmentDetails.AgentReference = "McLaren - CHEMPION!!!!!!!!!!!!!!!";

			var adapterToTest = new ShipmentValueObjectDataAdapter<TBusinessObject>(consol);
			var notify = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notify);

			adapterToTest.CreateOrUpdateFromValueObject(xmlShipment, context);

			var dataErrorPreventSaveError = from notification in notify.Events
											let errorNotification = notification as ErrorNotification
											where errorNotification != null && errorNotification.ErrorType == ErrorType.DataErrorPreventSave
											select errorNotification;

			AssertEquals("Should has errors", true, notify.HasErrors);
			AssertNotNull("Should has DataErrorPreventSave", dataErrorPreventSaveError);
		}

		public void TestImportCusHAWBIfIsSACNonAustralia()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			OrgHeader consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, "US"));

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			TBusinessObject importToShipment = Factory.New<TBusinessObject>();
			importToShipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			importToShipment.ConsigneePK = consignee.PK;

			Xsd.Shipment shipmentValueObject = new Xsd.Shipment();
			shipmentValueObject.ShipmentDetails.DeclarationStyle = "SAC";

			adapter.ImportFromValueObject(importToShipment, shipmentValueObject, context);

			ZQuery filter = new ZQuery(CusHAWBSchema.CS_JS, importToShipment.PK);
			BusinessObject cusHAWB = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Customs.AU.ICusHAWB>(filter);
			AssertNull(cusHAWB);
		}

		public void TestImport_BlankDocAddressValues()
		{
			OrgHeader consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			TBusinessObject importToShipment = Factory.New<TBusinessObject>();
			importToShipment.ConsigneePK = consignee.PK;

			Xsd.Shipment shipmentValueObject = new Xsd.Shipment();
			shipmentValueObject.ShipmentDetailsSpecified = true;

			adapter.ImportFromValueObject(importToShipment, shipmentValueObject, context);
			AssertEquals(consignee.PK, importToShipment.ConsigneePK);

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			adapter.ImportFromValueObject(importToShipment, shipmentValueObject, context);
			AssertEquals(ZGuid.Empty, importToShipment.ConsigneePK);
		}

		public void TestImport_BlankPackingMode()
		{
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			TBusinessObject importToShipment = Factory.New<TBusinessObject>();
			importToShipment.JS_PackingMode = Constants.ContainerModes.Loose;

			Xsd.Shipment shipmentValueObject = new Xsd.Shipment();
			shipmentValueObject.ShipmentDetailsSpecified = true;

			adapter.ImportFromValueObject(importToShipment, shipmentValueObject, context);
			AssertEquals(Constants.ContainerModes.Loose, importToShipment.JS_PackingMode);

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			adapter.ImportFromValueObject(importToShipment, shipmentValueObject, context);
			AssertEquals(Constants.ContainerModes.LCL, importToShipment.JS_PackingMode);
		}

		public void TestImport_BlankTransportMode()
		{
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			TBusinessObject importToShipment = Factory.New<TBusinessObject>();
			importToShipment.JS_TransportMode = Constants.TransportModes.Rail;

			Xsd.Shipment shipmentValueObject = new Xsd.Shipment();
			shipmentValueObject.ShipmentDetailsSpecified = true;

			adapter.ImportFromValueObject(importToShipment, shipmentValueObject, context);
			AssertEquals(Constants.TransportModes.Rail, importToShipment.JS_TransportMode);

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			adapter.ImportFromValueObject(importToShipment, shipmentValueObject, context);
			AssertEquals(Constants.TransportModes.Sea, importToShipment.JS_TransportMode);
		}

		public void TestImport_BlankPackages()
		{
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			TBusinessObject importToShipment = Factory.New<TBusinessObject>();
			importToShipment.JS_OuterPacks = 33;
			importToShipment.JS_F3_NKPackType = Constants.PkgUnit.Box;
			importToShipment.JS_TotalPackageCount = 44;
			importToShipment.JS_F3_NKTotalCountPackType = Constants.PkgUnit.Carton;

			Xsd.Shipment shipmentValueObject = new Xsd.Shipment();
			shipmentValueObject.ShipmentDetailsSpecified = true;

			adapter.ImportFromValueObject(importToShipment, shipmentValueObject, context);
			AssertEquals(1, importToShipment.OuterPackLines.Count);
			AssertEquals(1, importToShipment.InnerPackLines.Count);
			AssertEquals(33, importToShipment.JS_OuterPacks);
			AssertEquals(Constants.PkgUnit.Box, importToShipment.JS_F3_NKPackType);
			AssertEquals(44, importToShipment.JS_TotalPackageCount);
			AssertEquals(Constants.PkgUnit.Carton, importToShipment.JS_F3_NKTotalCountPackType);

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			adapter.ImportFromValueObject(importToShipment, shipmentValueObject, context);
			AssertEquals(0, importToShipment.OuterPackLines.Count);
			AssertEquals(0, importToShipment.InnerPackLines.Count);
			AssertEquals(0, importToShipment.JS_OuterPacks);
			AssertEquals("", importToShipment.JS_F3_NKPackType);
			AssertEquals(0, importToShipment.JS_TotalPackageCount);
			AssertEquals("", importToShipment.JS_F3_NKTotalCountPackType);
		}

		public void TestImport_PackagesWillNotBeImportedForColoadAssembly()
		{
			Action<TBusinessObject, bool> assertPackagesImport = (shipment, packagesShouldBeImported) =>
				{
					Xsd.Shipment shipmentValue = CreateXsdShipmentForImportWithMinimumData();

					shipmentValue.ShipmentDetails.Packages = new Xsd.PackageCollection();
					shipmentValue.ShipmentDetails.Packages.AddNew();

					shipmentValue.ShipmentDetails.InnerPackages = new Xsd.PackageBaseCollection();
					shipmentValue.ShipmentDetails.InnerPackages.AddNew();

					AssertEquals("Precondition: no packlines in factory", 0, shipment.Factory.Load<PackLine>(new ZQuery()).Length);

					var adapter = GetNewShipmentValueObjectDataAdapter();
					var context = new ValueObjectImportContext(shipment.Factory, new NotificationBuffer());

					adapter.ImportFromValueObject(shipment, shipmentValue, context);
					AssertEquals(packagesShouldBeImported ? 2 : 0, shipment.Factory.Load<PackLine>(new ZQuery()).Length);
				};

			var standardShipment = new BusinessObjectFactory().New<TBusinessObject>();
			standardShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			AssertEquals("Precondition", true, standardShipment.CanHaveOwnPackLines);
			assertPackagesImport(standardShipment, true);

			var coloadMaster = new BusinessObjectFactory().New<TBusinessObject>();
			coloadMaster.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			AssertEquals("Precondition", true, coloadMaster.CanHaveOwnPackLines);
			assertPackagesImport(coloadMaster, true);

			coloadMaster = new BusinessObjectFactory().New<TBusinessObject>();
			coloadMaster.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			coloadMaster.CoLoadShipments.AddNew();

			AssertEquals("Precondition", false, coloadMaster.CanHaveOwnPackLines);
			assertPackagesImport(coloadMaster, false);

			var highVolumeLowValueLegacy = new BusinessObjectFactory().New<TBusinessObject>();
			highVolumeLowValueLegacy.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			AssertEquals("Precondition", true, highVolumeLowValueLegacy.CanHaveOwnPackLines);
			assertPackagesImport(highVolumeLowValueLegacy, true);
		}

		[ExpectNoExceptions]
		public void TestImportEmptyPackageTypes()
		{
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			TBusinessObject importToShipment = Factory.New<TBusinessObject>();

			Xsd.Shipment shipmentValueObject = new Xsd.Shipment();
			var package = shipmentValueObject.ShipmentDetails.Packages.AddNew();

			AssertEquals("Prequisite", string.Empty, package.PackType);

			adapter.ImportFromValueObject(importToShipment, shipmentValueObject, context);
		}

		public void TestAllowBillingImportIntoShipment()
		{
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.Billing = new Xsd.Billing();
			xmlShipment.Billing.ChargeLines.AddNew().ChargeCode = "FRT";

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			TBusinessObject importedShipment = Factory.New<TBusinessObject>();
			JobHeader jobHeader = new JobHeader.Loader(importedShipment).TryCreate();

			SystemDataRegistry.Instance.AllowBillingImportIntoShipment.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			adapter.ImportFromValueObject(importedShipment, xmlShipment, context);
			JobCharge[] charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));

			AssertEquals("Should not import billing charges", 0, charges.Length);

			SystemDataRegistry.Instance.AllowBillingImportIntoShipment.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			adapter.ImportFromValueObject(importedShipment, xmlShipment, context);
			charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobHeader.PK));

			AssertEquals("Should import billing charges", 1, charges.Length);
			AssertEquals("Should import billing charges", "FRT", charges[0].ChargeCode.AC_Code);
		}

		#region TestForwardingShipmentType

		public void TestImportForwardingShipmentType()
		{
			var xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetailsSpecified = true;
			xmlShipment.ShipmentDetails.ForwardingShipmentType = Xsd.ForwardingShipmentType.CLD;
			var identifier = xmlShipment.ShipmentIdentifier.AddNew();
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			identifier.Value = "HOUSEBILL";

			var importedShipment = Factory.New<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			Adapter.ImportFromValueObject(importedShipment, xmlShipment, context);
			AssertEquals("shipment's type", Core.Constants.ShipmentTypes.CoLoadMaster, importedShipment.JS_ShipmentType);
		}

		public void TestExportForwardingShipmentType()
		{
			var shipment = Factory.NewWithValidTestData<TBusinessObject>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;

			Xsd.Shipment xmlShipment = Adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("forwarding shipment type", Xsd.ForwardingShipmentType.BCN, xmlShipment.ShipmentDetails.ForwardingShipmentType);
		}

		#endregion

		public void TestImportAddressCompanyName()
		{
			Xsd.Shipment xmlShipment = new Xsd.Shipment();

			Xsd.OrgAddress pickupAddress = new Xsd.OrgAddress();
			pickupAddress.CompanyName = "PICKUP ORG";
			pickupAddress.AddressLine1 = "PICKUP1";
			pickupAddress.AddressLine2 = "PICKUP2";
			pickupAddress.CityOrSuburb = "ALEXANDRIA";
			pickupAddress.StateOrProvince = "NSW";
			pickupAddress.PostCode = "2015";

			xmlShipment.ShipmentDetails.Pickup.Address = pickupAddress;

			Xsd.OrgAddress deliveryAddress = new Xsd.OrgAddress();
			deliveryAddress.CompanyName = "DELIVERY ORG";
			deliveryAddress.AddressLine1 = "DELIVERY1";
			deliveryAddress.AddressLine2 = "DELIVERY2";
			deliveryAddress.CityOrSuburb = "BRISBANE";
			deliveryAddress.StateOrProvince = "QLD";
			deliveryAddress.PostCode = "3000";

			xmlShipment.ShipmentDetails.Deliver.Address = deliveryAddress;

			AssertNull("Prerequisite", xmlShipment.ShipmentDetails.DocAddresses.DocAddress.GetAddressByType(Xsd.DocAddressAddressType.CRG));
			Assert("Prerequisite", xmlShipment.ShipmentDetails.Pickup.Address.IsSpecified);

			AssertNull("Prerequisite", xmlShipment.ShipmentDetails.DocAddresses.DocAddress.GetAddressByType(Xsd.DocAddressAddressType.CEG));
			Assert("Prerequisite", xmlShipment.ShipmentDetails.Deliver.Address.IsSpecified);

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			TBusinessObject shipment = Factory.New<TBusinessObject>();
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals("PICKUP ORG", shipment.ConsignorPickupAddress.E2_CompanyName);
			AssertEquals("PICKUP1", shipment.ConsignorPickupAddress.E2_Address1);
			AssertEquals("PICKUP2", shipment.ConsignorPickupAddress.E2_Address2);
			AssertEquals("ALEXANDRIA", shipment.ConsignorPickupAddress.E2_City);
			AssertEquals("NSW", shipment.ConsignorPickupAddress.E2_State);
			AssertEquals("2015", shipment.ConsignorPickupAddress.E2_Postcode);

			AssertEquals("DELIVERY ORG", shipment.ConsigneeDeliveryAddress.E2_CompanyName);
			AssertEquals("DELIVERY1", shipment.ConsigneeDeliveryAddress.E2_Address1);
			AssertEquals("DELIVERY2", shipment.ConsigneeDeliveryAddress.E2_Address2);
			AssertEquals("BRISBANE", shipment.ConsigneeDeliveryAddress.E2_City);
			AssertEquals("QLD", shipment.ConsigneeDeliveryAddress.E2_State);
			AssertEquals("3000", shipment.ConsigneeDeliveryAddress.E2_Postcode);
		}

		public void TestExportAddressCompanyName()
		{
			TBusinessObject shipment = Factory.New<TBusinessObject>();

			OrgAddress deliveryAddress = Factory.New<OrgAddress>();
			deliveryAddress.OA_Address1 = "ADDRESS1";
			deliveryAddress.OA_Address2 = "ADDRESS2";
			deliveryAddress.OA_City = "ALEXANDRIA";
			deliveryAddress.OA_State = "NSW";
			deliveryAddress.OA_PostCode = "2015";
			deliveryAddress.AddressCapability.SetCapabilityEnabled(nameof(AddressType.DLV));

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "GOOGLE";
			org.Addresses.Add(deliveryAddress);

			shipment.ConsigneePK = org.PK;

			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			adapter.ExportToValueObject(shipment, xmlShipment, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(ZString.Empty, xmlShipment.ShipmentDetails.Deliver.Address.CompanyName);
			AssertEquals("ADDRESS1", xmlShipment.ShipmentDetails.Deliver.Address.AddressLine1);
			AssertEquals("ADDRESS2", xmlShipment.ShipmentDetails.Deliver.Address.AddressLine2);
			AssertEquals("ALEXANDRIA", xmlShipment.ShipmentDetails.Deliver.Address.CityOrSuburb);
			AssertEquals("NSW", xmlShipment.ShipmentDetails.Deliver.Address.StateOrProvince);
			AssertEquals("2015", xmlShipment.ShipmentDetails.Deliver.Address.PostCode);

			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;

			xmlShipment = new Xsd.Shipment();
			adapter = GetNewShipmentValueObjectDataAdapter();
			adapter.ExportToValueObject(shipment, xmlShipment, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("GOOGLE", xmlShipment.ShipmentDetails.Deliver.Address.CompanyName);
			AssertEquals("ADDRESS1", xmlShipment.ShipmentDetails.Deliver.Address.AddressLine1);
			AssertEquals("ADDRESS2", xmlShipment.ShipmentDetails.Deliver.Address.AddressLine2);
			AssertEquals("ALEXANDRIA", xmlShipment.ShipmentDetails.Deliver.Address.CityOrSuburb);
			AssertEquals("NSW", xmlShipment.ShipmentDetails.Deliver.Address.StateOrProvince);
			AssertEquals("2015", xmlShipment.ShipmentDetails.Deliver.Address.PostCode);

			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = false;
			deliveryAddress.OA_CompanyNameOverride = "YAHOO!";

			xmlShipment = new Xsd.Shipment();
			adapter = GetNewShipmentValueObjectDataAdapter();
			adapter.ExportToValueObject(shipment, xmlShipment, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("YAHOO!", xmlShipment.ShipmentDetails.Deliver.Address.CompanyName);
			AssertEquals("ADDRESS1", xmlShipment.ShipmentDetails.Deliver.Address.AddressLine1);
			AssertEquals("ADDRESS2", xmlShipment.ShipmentDetails.Deliver.Address.AddressLine2);
			AssertEquals("ALEXANDRIA", xmlShipment.ShipmentDetails.Deliver.Address.CityOrSuburb);
			AssertEquals("NSW", xmlShipment.ShipmentDetails.Deliver.Address.StateOrProvince);
			AssertEquals("2015", xmlShipment.ShipmentDetails.Deliver.Address.PostCode);
		}

		#region TestCoLoadMaster

		public void TestImportCoLoadMaster()
		{
			Xsd.Shipment shipmentValue = new Xsd.Shipment();
			shipmentValue.ShipmentIdentifier.AddNew(Xsd.ShipmentIdentifierType.CoLoadMaster, "MASTERHOUSE");

			var candidateColoadMaster = Factory.New<TBusinessObject>();
			candidateColoadMaster.JS_HouseBill = "MASTERHOUSE";

			var importToShipment = Factory.New<TBusinessObject>();

			var adapter = GetNewShipmentValueObjectDataAdapter();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(importToShipment, shipmentValue, context);
			Assert("No coload master found", importToShipment.JS_JS_ColoadMasterShipment.IsEmpty);

			var highVolumeLowValueLegacy = Factory.New<TBusinessObject>();
			highVolumeLowValueLegacy.JS_HouseBill = "MASTERHOUSE";
			highVolumeLowValueLegacy.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			adapter.ImportFromValueObject(importToShipment, shipmentValue, context);
			Assert("No coload master found", importToShipment.JS_JS_ColoadMasterShipment.IsEmpty);

			var highVolumeLowValue = Factory.New<TBusinessObject>();
			highVolumeLowValue.JS_HouseBill = "MASTERHOUSE";
			highVolumeLowValue.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			adapter.ImportFromValueObject(importToShipment, shipmentValue, context);
			Assert("No coload master found", importToShipment.JS_JS_ColoadMasterShipment.IsEmpty);

			var coloadMaster = Factory.New<TBusinessObject>();
			coloadMaster.JS_HouseBill = "MASTERHOUSE";
			coloadMaster.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			adapter.ImportFromValueObject(importToShipment, shipmentValue, context);
			AssertEquals("Coload master found and set", coloadMaster.PK, importToShipment.JS_JS_ColoadMasterShipment);

			var anotherColoadMaster = Factory.New<TBusinessObject>();
			anotherColoadMaster.JS_HouseBill = "MASTERHOUSE";
			anotherColoadMaster.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			importToShipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			adapter.ImportFromValueObject(importToShipment, shipmentValue, context);
			Assert("Could not match coload master", importToShipment.JS_JS_ColoadMasterShipment.IsEmpty);
			AssertEquals(true, ((NotificationBuffer)context.Notifications).AsString.Contains("Unable to identify Coload Master"));
		}

		public void TestImportCoLoadMasterWillDeleteExistingPacklinesOnMaster()
		{
			Xsd.Shipment shipmentValue = new Xsd.Shipment();
			shipmentValue.ShipmentIdentifier.AddNew(Xsd.ShipmentIdentifierType.CoLoadMaster, "MASTERHOUSE");

			var coloadMaster = Factory.New<TBusinessObject>();
			coloadMaster.JS_HouseBill = "MASTERHOUSE";
			coloadMaster.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var packline1 = coloadMaster.OuterPackLines.AddNew();
			var packline2 = coloadMaster.OuterPackLines.AddNew();
			AssertEquals("Precondition", 2, coloadMaster.OuterPackLines.Count);

			var shipment = Factory.New<TBusinessObject>();
			var adapter = GetNewShipmentValueObjectDataAdapter();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			adapter.ImportCoLoadMaster(shipment, shipmentValue, context);
			AssertEquals("Coload master found and set", coloadMaster.PK, shipment.JS_JS_ColoadMasterShipment);
			AssertEquals("Master's packlines have been deleted", 0, coloadMaster.OuterPackLines.Count);
			AssertEquals(true, packline1.IsDeleted);
			AssertEquals(true, packline2.IsDeleted);
		}

		public void TestExportCoLoadMaster()
		{
			var exportFromShipment = Factory.New<TBusinessObject>();
			exportFromShipment.JS_HouseBill = "HOUSEBILL";

			var coLoadShipment = Factory.New<TBusinessObject>();
			coLoadShipment.JS_HouseBill = "MASTERHOUSE";
			coLoadShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			exportFromShipment.JS_JS_ColoadMasterShipment = coLoadShipment.PK;

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			Xsd.Shipment shipmentValueObject = adapter.ExportToValueObject(exportFromShipment, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("CoLoadMater is exported as it has the different house bill.", "MASTERHOUSE", shipmentValueObject.ShipmentIdentifier.FindFirst(Xsd.ShipmentIdentifierType.CoLoadMaster).Value);

			coLoadShipment.JS_HouseBill = "HOUSEBILL";

			shipmentValueObject = adapter.ExportToValueObject(exportFromShipment, new ValueObjectExportContext(new NotificationBuffer()));
			AssertNull("CoLoadMater is not exported as it has the same house bill.", shipmentValueObject.ShipmentIdentifier.FindFirst(Xsd.ShipmentIdentifierType.CoLoadMaster));
		}

		#endregion

		#region Test correct registry defaults used for testing

		public void TestValueOfRegistryDefaultForImporting()
		{
			try
			{
				SystemRegistry.UpdateConsolShipmentsDuringAutomaticImportOther.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				ShipmentValueObjectDataAdapter<TBusinessObject> shipmentAdapter = GetNewShipmentValueObjectDataAdapter();
				PropertyInfo registryDefaultForImportingProperty = shipmentAdapter.GetType().GetProperty("RegistryDefaultForImporting", BindingFlags.NonPublic | BindingFlags.Instance);

				AssertEquals("Adapter should be using shipment registry item which is currently true", true, registryDefaultForImportingProperty.GetValue(shipmentAdapter, null));

				SystemRegistry.UpdateConsolShipmentsDuringAutomaticImportOther.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				AssertEquals("Adapter should be using shipment registry item which is currently true", false, registryDefaultForImportingProperty.GetValue(shipmentAdapter, null));
			}
			finally
			{
				((IRegistryItemInternals)SystemRegistry.UpdateConsolShipmentsDuringAutomaticImportOther).ClearCache();
			}
		}

		SystemDataRegistry SystemRegistry
		{
			get { return SystemDataRegistry.Instance; }
		}

		#endregion

		public void TestExportShipmentNotifyParty()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			OrgContact contact = Consignee.Contacts.AddNew();
			contact.OC_ContactName = "SecondContact";

			Factory.Save();

			Xsd.Shipment xsdShipment = new Xsd.Shipment();
			TBusinessObject exportedShipment = Factory.New<TBusinessObject>();
			exportedShipment.NotifyPartyDocumentaryAddress.OrganisationPK = Consignee.PK;

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			adapter.ExportToValueObject(exportedShipment, xsdShipment, new ValueObjectExportContext(new NotificationBuffer()));

			Xsd.ContactReference contactReference = xsdShipment.ShipmentDetails.NotifyParty;
			AssertNotNull("NotifyParty", contactReference);
			AssertEquals(Consignee.OH_FullName, contactReference.Organisation.OrganisationDetails.Name);
			AssertEquals("Contacts.Count", 1, contactReference.Organisation.OrganisationDetails.Contacts.Count);
			AssertEquals("ContactSequenceRef", 1, contactReference.ContactSequenceRef);

			exportedShipment.NotifyContact = NotifyPartyContact.OC_ContactName;
			xsdShipment = new Xsd.Shipment();
			adapter.ExportToValueObject(exportedShipment, xsdShipment, new ValueObjectExportContext(new NotificationBuffer()));

			contactReference = xsdShipment.ShipmentDetails.NotifyParty;
			AssertNotNull("NotifyParty", contactReference);
			AssertEquals("Contacts.Count", 2, contactReference.Organisation.OrganisationDetails.Contacts.Count);
			Assert("ContactSequenceRef", contactReference.ContactSequenceRef > 0);

			TBusinessObject importedShipment = Factory.New<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(importedShipment, xsdShipment, context);

			AssertNotNull("NotifyParty", importedShipment.NotifyParty);
			AssertEquals("NotifyParty PK", Consignee.PK, importedShipment.NotifyParty.PK);
			AssertEquals("NotifyParty Contact", "Wally", importedShipment.NotifyContact);
		}

		public void TestImportShipmentNotifyParty()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OrganisationTypes = OrganisationTypes.Consignee;
			org.OH_Code = "CONSIGNEE";
			org.OH_FullName = "Consignee";
			org.MainAddress.OA_Address1 = "address";
			Factory.Save();

			TBusinessObject importedShipment = Factory.New<TBusinessObject>();
			Xsd.Shipment xsdShipment = new Xsd.Shipment();
			Xsd.ShipmentIdentifier identifier = xsdShipment.ShipmentIdentifier.AddNew();
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			identifier.Value = "HOUSEBILL";

			Xsd.ContactReference reference = new Xsd.ContactReference();

			Xsd.Organisation xsdConsignee = new Xsd.Organisation();
			xsdConsignee.OrganisationDetails.Addresses.GetOrCreateMainAddress().AddressLine1 = "address";
			xsdConsignee.OwnerCode = "CONSIGNEE";
			xsdConsignee.OrganisationDetails.Name = "Consignee";
			Xsd.OrgContactCollection contactCollection = xsdConsignee.OrganisationDetails.Contacts;
			Xsd.OrgContact contact1 = contactCollection.AddNew();
			contact1.Name = "Contact1";
			contact1.Sequence = 4;
			Xsd.OrgContact contact2 = contactCollection.AddNew();
			contact2.Name = "Contact2";
			contact2.Sequence = 2;

			reference.Organisation = xsdConsignee;
			reference.ContactSequenceRef = 2;
			xsdShipment.ShipmentDetails.NotifyParty = reference;

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(importedShipment, xsdShipment, context);

			AssertNotNull("NotifyParty", importedShipment.NotifyParty);
			AssertEquals("NotifyParty Name", org.PK, importedShipment.NotifyParty.PK);
			AssertEquals("NotifyParty Contact", "Contact2", importedShipment.NotifyContact);
		}

		public void TestShipment_Export_GoodsDescription()
		{
			TBusinessObject shipmentBizObj = Factory.New<TBusinessObject>();
			shipmentBizObj.DetailedGoodsDescriptionNoteText = "Detailed Goods Description";
			shipmentBizObj.JS_GoodsDescription = "Short Goods Description";

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			Xsd.Shipment xsdShipment = new Xsd.Shipment();
			adapter.ExportToValueObject(shipmentBizObj, xsdShipment, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("Short Goods Description", xsdShipment.ShipmentDetails.GoodsDescription);
			AssertEquals(1, xsdShipment.Notes.Count);
			AssertEquals("Detailed Goods Description", xsdShipment.Notes[0].NoteData);
		}

		public void TestShipment_Export_CreatedDate()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			NotificationBuffer notify = new NotificationBuffer();

			TBusinessObject shipment = Factory.New<TBusinessObject>();
			shipment.FillWithValidTestData();

			Xsd.Shipment xmlShipment = new Xsd.Shipment();

			adapter.ExportToValueObject(shipment, xmlShipment, new ValueObjectExportContext(notify));
			AssertEquals(DateTime.MinValue, xmlShipment.ShipmentDetails.DateCreated);

			Factory.Save();

			Assert("PreCondition: Shipment.Logs.CreatedDate should not be empty", !shipment.Logs.CreatedDateUtc.IsEmpty);

			Xsd.Shipment xmlShipment1 = new Xsd.Shipment();
			adapter.ExportToValueObject(shipment, xmlShipment1, new ValueObjectExportContext(notify));
			AssertEquals(shipment.Logs.CreatedDateUtc.ToDateTime(), xmlShipment1.ShipmentDetails.DateCreated);
		}

		public void TestShipment_Export_CustomsEntryNumber()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			NotificationBuffer notify = new NotificationBuffer();

			TBusinessObject shipment = Factory.New<TBusinessObject>();
			CusEntryNumber cen1 = shipment.CusEntryNumbers.AddNew();
			cen1.FillWithValidTestData();
			cen1.CE_ParentID = shipment.PK;
			CusEntryNumber cen2 = shipment.CusEntryNumbers.AddNew();
			cen2.FillWithValidTestData();
			cen2.CE_ParentID = shipment.PK;
			cen2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.AmericanSamoa;

			Xsd.Shipment xmlShipment = new Xsd.Shipment();

			adapter.ExportToValueObject(shipment, xmlShipment, new ValueObjectExportContext(notify));
			AssertEquals(2, xmlShipment.ShipmentDetails.CustomsEntryNumbers.Count);
		}

		public void TestShipment_Export_AgentReference()
		{
			TConsol consol = Factory.New<TConsol>();
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter(consol);
			NotificationBuffer notify = new NotificationBuffer();

			TBusinessObject shipment = Factory.New<TBusinessObject>();
			shipment.FillWithValidTestData();

			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			adapter.ExportToValueObject(shipment, xmlShipment, new ValueObjectExportContext(notify));
			AssertEquals(shipment.JS_UniqueConsignRef, xmlShipment.ShipmentDetails.AgentReference);
		}

		public void TestShipment_Export_JobHeaderAdditionals()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			Shipment.FillWithValidTestData();

			GlbStaff salesRep = Factory.New<GlbStaff>();
			salesRep.GS_Code = "JA";
			salesRep.GS_FullName = "JESSICA ALLEN";
			salesRep.GS_IsSalesRep = true;

			OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "LOCALCLIENT";
			localClient.OH_RL_NKClosestPort = "AUSYD";
			OrgAddress address = localClient.Addresses.MainAddress;
			address.OA_Address1 = "X435E3R6PK226SDFZ7TTNXEH7GX0EM2HRJ13NDJK423980";

			JobHeader.Loader loader = new JobHeader.Loader(Shipment);
			JobHeader jobHeader = loader.TryCreate();
			jobHeader.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			jobHeader.JH_GS_NKRepSales = salesRep.GS_Code;
			jobHeader.JH_ParentID = Shipment.PK;
			Adapter.ExportToValueObject(Shipment, xmlShipment, new ValueObjectExportContext(Notify));
			AssertEquals("JA", xmlShipment.ShipmentDetails.SalesRep);
			AssertEquals("LOCALCLIENT", xmlShipment.ShipmentDetails.LocalClient.EDICode);
		}

		public void TestShipment_Import_LocalClient()
		{
			SystemDataRegistry.Instance.AllowBillingImportIntoShipment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			Xsd.Shipment shipmentValue = CreateXsdShipmentForImportWithMinimumData();
			shipmentValue.ShipmentDetails.LocalClient = new Xsd.Organisation();
			shipmentValue.ShipmentDetails.LocalClient.EDICode = "SPLATY";
			shipmentValue.ShipmentDetails.LocalClient.OrganisationDetails = new Xsd.OrganisationDetail();
			shipmentValue.ShipmentDetails.LocalClient.OrganisationDetails.Name = "SPLATY";
			shipmentValue.ShipmentDetails.LocalClient.IsSpecified = true;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonShipment importedShipment = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertNull("Must NOT import", importedShipment.Job);

			SystemDataRegistry.Instance.AllowBillingImportIntoShipment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			importedShipment = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertEquals("Must import", false, importedShipment.Job.LocalChargesPK.IsEmpty);
		}

		public void TestShipmentImport_DoesNotCreateDuplicatedJob_WhenShipmentInDB()
		{
			var shipment = Factory.New<TBusinessObject>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var shipmentInAnotherFactory = newFactory.Load<TBusinessObject>(shipment.PK);
			var loader = new JobHeader.Loader(shipmentInAnotherFactory);
			var job = loader.TryCreateWithMutex();

			var xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetails.LocalClient = new Xsd.Organisation();
			xmlShipment.ShipmentDetails.LocalClient.EDICode = "SPLATY";
			xmlShipment.ShipmentDetails.LocalClient.IsSpecified = true;

			Assert("Precondition - Shipment IsInDatabase", shipment.IsInDatabase);

			var notificationBuffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notificationBuffer);
			var adapter = new ShipmentValueObjectDataAdapter<TBusinessObject>();

			adapter.ImportFromValueObject(shipment, xmlShipment, context);
			Assert(notificationBuffer.HasErrors);
			AssertEquals("Error: You have created the job S00001000 on another form, but haven't saved it yet.\r\nPlease close or save other forms that use job S00001000 to continue.\r\nShipment S00001000 updated\r\n", notificationBuffer.AsString);
			job.Dispose();
		}

		public void TestShipmentImport_DoesNotCreateDuplicatedJob_WhenShipmentNotInDB()
		{
			var shipment = Factory.New<TBusinessObject>();
			var loader = new JobHeader.Loader(shipment);
			var job = loader.TryCreateWithMutex();

			var xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetails.LocalClient = new Xsd.Organisation();
			xmlShipment.ShipmentDetails.LocalClient.EDICode = "SPLATY";
			xmlShipment.ShipmentDetails.LocalClient.IsSpecified = true;

			Assert("Precondition - Shipment IsInDatabase", !shipment.IsInDatabase);

			var notificationBuffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notificationBuffer);
			var adapter = new ShipmentValueObjectDataAdapter<TBusinessObject>();

			adapter.ImportFromValueObject(shipment, xmlShipment, context);
			var zQuery = new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK);
			var jobs = Factory.Load<JobHeader>(zQuery);

			Assert("No Errors", !notificationBuffer.HasErrors);
			AssertEquals("Job Count", 1, jobs.Length);
			AssertEquals(job, jobs.First());
			job.Dispose();
		}

		public void TestMinimumRequirementsMetForNewImport()
		{
			#region Setup

			const Xsd.TransportMode transportMode = Xsd.TransportMode.SEA;
			var xsdConsignee = CreateXsdOrganisation(Consignee.OH_Code);
			var xsdConsignor = CreateXsdOrganisation(Consignor.OH_Code);
			var cneOverrideAddr = new Xsd.DocAddress { AddressType = Xsd.DocAddressAddressType.CED, AddressLine1 = "Consignee Override Address" };
			var cnrOverrideAddr = new Xsd.DocAddress { AddressType = Xsd.DocAddressAddressType.CRD, AddressLine1 = "Consignor Override Address" };

			Factory.Save();

			#endregion

			CombineAssertions(delegate
			{
				AssertNewShipmentImportFails("Missing ALL required elements", CreateXsdShipmentForImport(null, null, null));
				AssertNewShipmentImportFails("Missing Transport Mode", CreateXsdShipmentForImport(null, xsdConsignee, xsdConsignor));
				AssertNewShipmentImportFails("Missing Consignor/Consignee", CreateXsdShipmentForImport(transportMode, null, null));
				AssertNewShipmentImportSucceeds("All required fields", CreateXsdShipmentForImport(transportMode, xsdConsignor, xsdConsignee));
				AssertNewShipmentImportSucceeds("Minimum required fields (Mode+Consignor)", CreateXsdShipmentForImport(transportMode, xsdConsignor, null));
				AssertNewShipmentImportSucceeds("Minimum required fields (Mode+Consignee)", CreateXsdShipmentForImport(transportMode, null, xsdConsignee));

				var shipmentValue = CreateXsdShipmentForImport(transportMode, null, null);
				shipmentValue.ShipmentDetails.DocAddressesSpecified = true;
				shipmentValue.ShipmentDetails.DocAddresses = new Xsd.DocAddresses();

				shipmentValue.ShipmentDetails.DocAddresses.DocAddress.Add(cneOverrideAddr);
				AssertNewShipmentImportSucceeds("Consignee override", shipmentValue);

				shipmentValue.ShipmentDetails.DocAddresses.DocAddress.Clear();
				shipmentValue.ShipmentDetails.DocAddresses.DocAddress.Add(cnrOverrideAddr);
				AssertNewShipmentImportSucceeds("Consignor override", shipmentValue);
			});
		}

		public void TestShipment_Export_TEU()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			Shipment.FillWithValidTestData();
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			Adapter.ExportToValueObject(Shipment, xmlShipment, new ValueObjectExportContext(Notify));
			Assert("No TEU", !xmlShipment.ShipmentDetails.TEUSpecified);
			AssertEquals("Zero TEU", ZDecimal.Zero, xmlShipment.ShipmentDetails.TEU);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			Consol.Containers.AddNew();
			Consol.Containers[0].JC_RC = Factory.New<RefContainer>().PK;
			Consol.Containers[0].JC_ContainerNum = "CONTNR1";
			Consol.Containers[0].JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			Consol.Containers[0].Container.RC_TEU = 2.3m;
			Consol.Containers.AddNew();
			Consol.Containers[1].JC_RC = Factory.New<RefContainer>().PK;
			Consol.Containers[1].JC_ContainerNum = "CONTNR2";
			Consol.Containers[1].JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			Consol.Containers[1].Container.RC_TEU = 3.4m;
			Shipment.Consols.Add(Consol);
			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines[0].JL_JC = Consol.Containers[0].PK;
			Shipment.OuterPackLines.AddNew();
			Shipment.OuterPackLines[1].JL_JC = Consol.Containers[1].PK;

			Adapter.ExportToValueObject(Shipment, xmlShipment, new ValueObjectExportContext(Notify));
			AssertEquals("TEU", 5.7m, xmlShipment.ShipmentDetails.TEU);
		}

		public void TestShipment_Import_CustomsEntryNumber()
		{
			TConsol consol = Factory.New<TConsol>();
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter(consol);
			NotificationBuffer notify = new NotificationBuffer();

			TBusinessObject shipment = Factory.New<TBusinessObject>();

			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetailsSpecified = true;
			Xsd.CustomsEntryNumber cen = xmlShipment.ShipmentDetails.CustomsEntryNumbers.AddNew();
			cen.Country = "NZ";
			cen.Number = "123";
			cen.Type = "CAN";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObject(shipment, xmlShipment, context);
			AssertEquals(1, shipment.CusEntryNumbers.Count);
			AssertEquals(shipment.PK, shipment.CusEntryNumbers[0].CE_ParentID);
		}

		public void TestCollectionSchema()
		{
			AssertNotNull("Should have the CollectionSchema specified", GetNewShipmentValueObjectDataAdapter().CollectionSchema);
		}

		public void TestImportNotesExist()
		{
			Xsd.NotesNote note = new Xsd.NotesNote();
			note.NoteData = "you are a wineo";
			note.NoteType = Xsd.NotesNoteNoteType.BookingNotes;

			Xsd.Shipment shipmentValue = new Xsd.Shipment();
			shipmentValue.Notes = new Xsd.NotesNoteCollection();
			shipmentValue.Notes.Add(note);

			TBusinessObject shipmentBizObj = Factory.New<TBusinessObject>();

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipmentBizObj, shipmentValue, context);

			AssertEquals(true, shipmentBizObj.Notes.HasNotes);
		}

		public void TestExportNotesExist()
		{
			Xsd.Shipment shipment1 = new Xsd.Shipment();

			TBusinessObject shipmentBizObj = Factory.New<TBusinessObject>();
			StmNote note1 = shipmentBizObj.Notes.AddNew();
			note1.ST_NoteType = "PUB";
			note1.ST_NoteText = "Hello Chello";
			note1.ST_Description = PredefinedNoteTypes.Instance.BookingNotes.Description;

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			adapter.ExportToValueObject(shipmentBizObj, shipment1, new ValueObjectExportContext(new NotificationBuffer()));

			AssertNotNull(shipment1.Notes);
			AssertEquals(true, shipmentBizObj.Notes.HasNotes);
		}

		public void TestImportPackLineContainerWithExistingConsol()
		{
			TConsol consol = Factory.New<TConsol>();
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER";

			Xsd.Shipment shipmentValue = CreateXsdShipmentForImportWithMinimumData();
			shipmentValue.ShipmentDetailsSpecified = true;
			shipmentValue.ShipmentDetails.Packages = new Xsd.PackageCollection();
			Xsd.Package package = shipmentValue.ShipmentDetails.Packages.AddNew();
			package.ContainerNumber = "container";

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter(consol);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertEquals("Container should be attached to the outer pack line", "CONTAINER", shipment.OuterPackLines[0].JL_Calc_ContainerNum);
		}

		public void TestImportPackLineContainer_JC_GrossWeightOutOfDecimalRange()
		{
			var consol = Factory.New<TConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER";
			container.JC_TareWeight = 2000m;

			var shipmentValue = CreateXsdShipmentForImportWithMinimumData();
			shipmentValue.ShipmentDetailsSpecified = true;
			shipmentValue.ShipmentDetails.Packages = new Xsd.PackageCollection();
			shipmentValue.ShipmentDetails.Weight.Value = 999990m;
			shipmentValue.ShipmentDetails.Weight.DimensionType = "KG";

			var package = shipmentValue.ShipmentDetails.Packages.AddNew();
			package.ContainerNumber = "CONTAINER";
			package.Weight.Value = 999990m;
			package.Weight.DimensionType = "KG";

			Factory.SuspendValidation();
			var adapter = GetNewShipmentValueObjectDataAdapter(consol);
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var shipment = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			Factory.ResumeValidation();

			AssertEquals("Container should be attached to the outer pack line", "CONTAINER", shipment.OuterPackLines[0].JL_Calc_ContainerNum);

			var firstContainer = shipment.Containers.FirstOrDefault();
			AssertNotNull(firstContainer);
			AssertEquals("JC_GrossWeight should not be out of decimal(9,3) and is set to 999999. ", 999999m, firstContainer.JC_GrossWeight);

			ErrorReporter.Clear();

			Factory.Save();
			Assert("No error report is generated.", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			var buffer = context.Notifications as NotificationBuffer;
			Assert("Warning log should be contained.", buffer.HasWarnings);
			Assert("JC_GrossWeight warning log should be contained.", buffer.AsString.Contains("Warning: Attempted to insert '1001990' into Field [JC_GrossWeight] which has a maximum numeric value of '999999'. Field was truncated to the max value."));

			ErrorReporter.Clear();
		}

		public void TestImportLegacyXML_JC_GrossWeight_IsTruncatedForOutOfRangeDecimal()
		{
			var consol = Factory.New<TConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER";
			container.JC_TareWeight = 1m;

			var existingShipmentOnConsol = consol.Shipments.AddNew();
			existingShipmentOnConsol.OuterPackLines.AddNew().JL_ActualWeight = 2000m;

			var shipmentToImportXML = CreateXsdShipmentForImportWithMinimumData();
			shipmentToImportXML.ShipmentDetailsSpecified = true;
			shipmentToImportXML.ShipmentDetails.Weight.Value = 999990m;
			shipmentToImportXML.ShipmentDetails.Weight.DimensionType = "KG";
			shipmentToImportXML.ShipmentDetails.Packages = new Xsd.PackageCollection();

			var updateShipmentXMLPack1 = shipmentToImportXML.ShipmentDetails.Packages.AddNew();
			updateShipmentXMLPack1.ContainerNumber = "CONTAINER";
			updateShipmentXMLPack1.Weight.Value = 999990;
			updateShipmentXMLPack1.Weight.DimensionType = "KG";

			var updateShipmentXMLPack2 = shipmentToImportXML.ShipmentDetails.Packages.AddNew();
			updateShipmentXMLPack2.ContainerNumber = "CONTAINER";
			updateShipmentXMLPack2.Weight.Value = 8;
			updateShipmentXMLPack2.Weight.DimensionType = "KG";

			Factory.SuspendValidation();
			var adapter = GetNewShipmentValueObjectDataAdapter(consol);
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var shipment = adapter.CreateOrUpdateFromValueObject(shipmentToImportXML, context);
			Factory.ResumeValidation();

			AssertEquals("Container should be attached to the outer pack line", "CONTAINER", shipment.OuterPackLines[0].JL_Calc_ContainerNum);

			var firstContainer = consol.Containers.OfType<CommonContainer>().FirstOrDefault();
			AssertNotNull(firstContainer);
			AssertEquals("JC_GrossWeight should not be out of decimal(9,3) and is set to 999999.999", 999999m, firstContainer.JC_GrossWeight);

			ErrorReporter.Clear();

			Factory.Save();
			Assert("No error report is generated.", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			var buffer = context.Notifications as NotificationBuffer;
			Assert("Warning log should be contained.", buffer.HasWarnings);
			Assert("JC_GrossWeight warning log should be contained.", buffer.AsString.Contains("Warning: Attempted to insert '1001999' into Field [JC_GrossWeight] which has a maximum numeric value of '999999'. Field was truncated to the max value."));

			ErrorReporter.Clear();
		}

		public void TestWhenImportingOriginNotDefaultedFromConsignor()
		{
			Xsd.Shipment shipmentValue = CreateXsdShipmentForImportWithMinimumData();
			shipmentValue.ShipmentDetailsSpecified = true;
			shipmentValue.ShipmentDetails.Consignor = new Xsd.Organisation();
			shipmentValue.ShipmentDetails.Consignor.EDICode = "cons";
			shipmentValue.ShipmentDetails.Consignor.OrganisationDetails = new Xsd.OrganisationDetail();
			shipmentValue.ShipmentDetails.Consignor.OrganisationDetails.Location = new Xsd.UNLOCO();
			shipmentValue.ShipmentDetails.Consignor.OrganisationDetails.Location.Value = "AUPER";
			shipmentValue.ShipmentDetails.PortOfOrigin = new Xsd.Movement();
			shipmentValue.ShipmentDetails.PortOfOrigin.Port = new Xsd.UNLOCO();
			shipmentValue.ShipmentDetails.PortOfOrigin.Port.Value = "MYPKG";

			TConsol consol = Factory.New<TConsol>();
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter(consol);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertEquals("Location shouldn't come off the consignor", true, "AUPER" != shipment.JS_RL_NKOrigin);
			AssertEquals("Location should come from the value object", "MYPKG", shipment.JS_RL_NKOrigin);
		}

		public void TestWhenImportingExportBrokerNotImportedIfRegistryItemNotSet()
		{
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			Xsd.Shipment shipmentValue = CreateXsdShipmentForImportWithMinimumData();
			shipmentValue.ShipmentDetails.ExportBroker = new Xsd.Organisation();
			shipmentValue.ShipmentDetails.ExportBroker.EDICode = "SPLATY";
			shipmentValue.ShipmentDetails.ExportBroker.OrganisationDetails = new Xsd.OrganisationDetail();
			shipmentValue.ShipmentDetails.ExportBroker.OrganisationDetails.Name = "SPLATY";

			SystemDataRegistry.Instance.AllowExportBrokerImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonShipment importedShipment = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertEquals("Should not import the export broker when the flag is false", true, importedShipment.JS_OH_ExportBroker.IsEmpty);
			SystemDataRegistry.Instance.AllowExportBrokerImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			importedShipment = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertEquals("Must import the export broker when the flag is set", false, importedShipment.JS_OH_ExportBroker.IsEmpty);
		}

		public void TestCreateOrUpdateFromValueObject()
		{
			CommonShipment existingShipment = Factory.NewWithValidTestData<TBusinessObject>();
			existingShipment.JS_UniqueConsignRef = "uniqueref";
			existingShipment.JS_HouseBill = "sometext";

			CommonShipment anotherShipment = Factory.NewWithValidTestData<TBusinessObject>();
			anotherShipment.JS_UniqueConsignRef = "abcd";
			anotherShipment.JS_HouseBill = "qwerty";

			Xsd.Shipment xsdShipment = CreateXsdShipmentForImportWithMinimumData();
			xsdShipment.ShipmentDetails.AgentReference = "";

			ShipmentCollection collection = new ShipmentCollection(Factory);
			collection.Load();
			AssertEquals("Precondition", 2, collection.Count);

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.ShipmentNumber);
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			adapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			AssertEquals("Notifications contains errors", true, notify.HasErrors);

			collection.Load();
			AssertEquals("No new shipments was created", 2, collection.Count);

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.HouseBill);
			notify = new NotificationBuffer();
			context = new ValueObjectImportContext(Factory, notify);

			adapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			AssertEquals("Notifications does not contain error", false, notify.HasErrors);

			collection.Load();
			AssertEquals("New shipment was created", 3, collection.Count);

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.ShipmentThenHouseBill);
			notify = new NotificationBuffer();
			context = new ValueObjectImportContext(Factory, notify);

			adapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			AssertEquals("Notifications does not contain error", false, notify.HasErrors);

			collection.Load();
			AssertEquals("New shipment was created", 4, collection.Count);
		}

		public void TestFindBusinessObject()
		{
			CommonShipment garbageShipment = Factory.NewWithValidTestData<TBusinessObject>();

			CommonShipment housebillOnlyShipment1 = Factory.NewWithValidTestData<TBusinessObject>();
			housebillOnlyShipment1.JS_HouseBill = "housebill";

			CommonShipment housebillOnlyShipment2 = Factory.NewWithValidTestData<TBusinessObject>();
			housebillOnlyShipment2.JS_HouseBill = "housebill";
			housebillOnlyShipment2.JS_E_DEP = new ZDateTime(2011, 1, 2);

			CommonShipment housebillOnlyShipment3 = Factory.NewWithValidTestData<TBusinessObject>();
			housebillOnlyShipment3.JS_HouseBill = "housebill";
			housebillOnlyShipment3.JS_RL_NKOrigin = "AUSYD";

			CommonShipment uniqueRefAndHousebillShipment = Factory.NewWithValidTestData<TBusinessObject>();
			uniqueRefAndHousebillShipment.JS_UniqueConsignRef = "uniqueref";
			uniqueRefAndHousebillShipment.JS_HouseBill = "sometext";

			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();

			Xsd.Shipment xsdShipment = new Xsd.Shipment();
			xsdShipment.ShipmentDetails.AgentReference = "";
			xsdShipment.ShipmentIdentifier = new Xsd.ShipmentIdentifierCollection();
			Xsd.ShipmentIdentifier houseIdentifier = xsdShipment.ShipmentIdentifier.AddNew();
			houseIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			houseIdentifier.Value = "housebill";

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.ShipmentNumber);
			CommonShipment foundShipment = FindBusinessObject(adapter, xsdShipment, context);
			AssertNull("ImportType SHP: If agents ref not specified, should not find shipment", foundShipment);

			xsdShipment.ShipmentDetails.AgentReference = "uniqueref";
			foundShipment = FindBusinessObject(adapter, xsdShipment, context);
			AssertEquals("ImportType SHP: If agents ref is specified, should find the correct shipment", uniqueRefAndHousebillShipment.PK, foundShipment.PK);

			xsdShipment.ShipmentDetails.AgentReference = "blah";
			foundShipment = FindBusinessObject(adapter, xsdShipment, context);
			AssertNull("ImportType SHP: If agents ref not found, should not fallback to the shipment with the housebill specified", foundShipment);

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.ShipmentThenHouseBill);
			foundShipment = FindBusinessObject(adapter, xsdShipment, context);
			AssertEquals("ImportType SHB: If agents ref not specified, should find the correct shipment by housebill number", housebillOnlyShipment1.PK, foundShipment.PK);

			xsdShipment.ShipmentDetails.AgentReference = "uniqueref";
			foundShipment = FindBusinessObject(adapter, xsdShipment, context);
			AssertEquals("ImportType SHB: If agents ref is specified, should find the correct shipment", uniqueRefAndHousebillShipment.PK, foundShipment.PK);

			xsdShipment.ShipmentDetails.AgentReference = "blah";
			foundShipment = FindBusinessObject(adapter, xsdShipment, context);
			AssertEquals("ImportType SHB: If agents ref not found, should  should find the correct shipment by housebill number", housebillOnlyShipment1.PK, foundShipment.PK);

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.HouseBill);
			foundShipment = FindBusinessObject(adapter, xsdShipment, context);
			AssertEquals("ImportType HBL: Should find the correct shipment by housebill number", housebillOnlyShipment1.PK, foundShipment.PK);

			houseIdentifier.Value = "blah";
			foundShipment = FindBusinessObject(adapter, xsdShipment, context);
			AssertNull("ImportType HBL: no shipment was found by housebill number", foundShipment);

			houseIdentifier.Value = "housebill";
			xsdShipment.ShipmentDetails.PortOfOrigin.EstimatedDateTime = new ZDateTime(2011, 1, 2);
			foundShipment = FindBusinessObject(adapter, xsdShipment, context);
			AssertEquals(housebillOnlyShipment2.PK, foundShipment.PK);

			xsdShipment.ShipmentDetails.PortOfOrigin.Port.Value = "AUSYD";
			foundShipment = FindBusinessObject(adapter, xsdShipment, context);
			AssertEquals(housebillOnlyShipment3.PK, foundShipment.PK);
		}

		protected CommonShipment FindBusinessObject(ShipmentValueObjectDataAdapter<TBusinessObject> adapter, Xsd.Shipment value, ValueObjectImportContext context)
		{
			MethodInfo method = adapter.GetType().GetMethod("FindBusinessObject", BindingFlags.NonPublic | BindingFlags.Instance);
			CommonShipment result = (CommonShipment)method.Invoke(adapter, new object[] { value, context });

			return result;
		}

		public void TestFindBusinessObject_WithConsolAttached()
		{
			TConsol consol = Factory.NewWithValidTestData<TConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "housebill";
			CommonShipment decoyShipment = Factory.New<TBusinessObject>();
			decoyShipment.JS_HouseBill = "housebill";
			CommonShipment shipmentWithNoHouseBill = Factory.NewWithValidTestData<TBusinessObject>();
			shipmentWithNoHouseBill.JS_HouseBill = "";

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter(consol);
			Xsd.Shipment value = new Xsd.Shipment();
			value.ShipmentIdentifier = new Xsd.ShipmentIdentifierCollection();
			Xsd.ShipmentIdentifier identifier = value.ShipmentIdentifier.AddNew();
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			identifier.Value = "housebill";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonShipment foundShipment = FindBusinessObject(adapter, value, context);
			AssertEquals("Should find the shipment with the same house bill", foundShipment.PK, shipment.PK);
			foundShipment.JS_HouseBill = "";
			foundShipment = FindBusinessObject(adapter, value, context);
			AssertNull("Should find no shipment", foundShipment);
		}

		public void TestShipmentWithMultiplePackLines()
		{
			TConsol consol = Factory.NewWithValidTestData<TConsol>();
			consol.AutomaticallyUpdatePackLineContainers = false;
			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "C1";
			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "C2";
			CommonContainer container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "C3";

			TBusinessObject shipment = (TBusinessObject)consol.Shipments.AddNew();
			PackLine line1 = shipment.OuterPackLines.AddNew();
			line1.Containers.Add(container1);
			PackLine line2 = shipment.OuterPackLines.AddNew();
			line2.Containers.Add(container2);

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			Xsd.Shipment shipmentXSD = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("adapter's packlines count should be 2", 2, shipmentXSD.ShipmentDetails.Packages.Count);

			PackLine line3 = shipment.OuterPackLines.AddNew();
			line3.Containers.Add(container3);
			shipmentXSD = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Adapter's packlines count should now be 3", 3, shipmentXSD.ShipmentDetails.Packages.Count);
		}

		public void TestOrganisationTypeOnImport()
		{
			Xsd.Shipment shipmentValue = CreateXsdShipmentForImportWithMinimumData();
			shipmentValue.ShipmentDetailsSpecified = true;
			shipmentValue.ShipmentDetails.Consignee = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "Consignee", OrganisationTypes.Consignee);
			shipmentValue.ShipmentDetails.Consignor = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "Consignor", OrganisationTypes.Consignor);
			shipmentValue.ShipmentDetails.ImportBroker = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "ImportBroker", OrganisationTypes.Broker);
			shipmentValue.ShipmentDetails.ExportBroker = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "ExportBroker", OrganisationTypes.Broker);
			shipmentValue.ShipmentDetails.Deliver.DeliveryAgent = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "DeliveryAgent", OrganisationTypes.Forwarder);
			shipmentValue.ShipmentDetails.Pickup.CartageCompany = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "PickupCartageCompany", OrganisationTypes.Carrier);
			shipmentValue.ShipmentDetails.Deliver.CartageCompany = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "DeliveryCartageCompany", OrganisationTypes.Carrier);

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonShipment importedShipment = GetNewShipmentValueObjectDataAdapter().CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertEquals("Consignee", "Consignee", importedShipment.Consignee.OH_FullName);
			AssertEquals("Consignee is consignee", true, importedShipment.Consignee.OH_IsConsignee);
			AssertEquals("Consignor", "Consignor", importedShipment.Consignor.OH_FullName);
			AssertEquals("Consignor is consignor", true, importedShipment.Consignor.OH_IsConsignor);
			AssertEquals("ImportBroker", "ImportBroker", importedShipment.ImportBroker.OH_FullName);
			AssertEquals("ImportBroker is broker", true, importedShipment.ImportBroker.OH_IsBroker);
			AssertEquals("ExportBroker", "ExportBroker", importedShipment.ExportBroker.OH_FullName);
			AssertEquals("ExportBroker is broker", true, importedShipment.ExportBroker.OH_IsBroker);
			AssertEquals("DeliveryAgent", "DeliveryAgent", importedShipment.DeliveryAgent.OH_FullName);
			AssertEquals("DeliveryAgent is Forwarded", true, importedShipment.DeliveryAgent.OH_IsForwarder);
			AssertEquals("PickupCartageCompany", "PickupCartageCompany", importedShipment.DocsAndCartage.PickupCartageCo.OH_FullName);
			AssertEquals("PickupCartageCompany is Transport", true, importedShipment.DocsAndCartage.PickupCartageCo.OH_IsShippingProvider);
			AssertEquals("DeliveryCartageCompany", "DeliveryCartageCompany", importedShipment.DocsAndCartage.DeliveryCartageCo.OH_FullName);
			AssertEquals("DeliveryCartageCompany is Transport", true, importedShipment.DocsAndCartage.DeliveryCartageCo.OH_IsShippingProvider);
		}

		Xsd.Shipment CreateShipmentXSD(Xsd.TransportMode transportMode, Xsd.ContainerMode containerMode, ZString origin, ZString dest)
		{
			Xsd.Shipment shipmentValue = new Xsd.Shipment();
			shipmentValue.ShipmentDetails.TransportMode = transportMode;
			shipmentValue.ShipmentDetails.PackingMode = Xsd.ContainerMode.FCL;
			shipmentValue.ShipmentDetails.PortOfOrigin = new Xsd.Movement();
			shipmentValue.ShipmentDetails.PortOfOrigin.Port = new Xsd.UNLOCO();
			shipmentValue.ShipmentDetails.PortOfOrigin.Port.Value = origin;
			shipmentValue.ShipmentDetails.PortofDestination = new Xsd.Movement();
			shipmentValue.ShipmentDetails.PortofDestination.Port = new Xsd.UNLOCO();
			shipmentValue.ShipmentDetails.PortofDestination.Port.Value = dest;

			return shipmentValue;
		}

		#region Import customs brokers / cartage organisations

		public void TestImport_DeliveryCartage()
		{
			OrgHeader consignee = CreateOrganisation("Consignee", OrganisationTypes.Consignee);
			OrgHeader deliveryCartage = CreateOrganisation("DeliveryCartage", OrganisationTypes.Carrier);
			OrgHeader givenCartage = CreateOrganisation("GivenCartage", OrganisationTypes.Carrier);

			consignee.SetRelatedParty(deliveryCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

			Factory.Save();

			Xsd.Shipment shipmentValue = CreateShipmentXSD(Xsd.TransportMode.SEA, Xsd.ContainerMode.FCL, "USLAX", "AUSYD");
			shipmentValue.ShipmentDetails.Consignee = CreateXsdOrganisation("Consignee");
			shipmentValue.ShipmentDetailsSpecified = true;

			Action<string, ZGuid> assertDeliveryCartage = (message, expectedCartagePK) =>
			{
				ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
				CommonShipment importedShipment = new ShipmentValueObjectDataAdapter<TBusinessObject>().CreateOrUpdateFromValueObject(shipmentValue, context);
				AssertEquals(message, expectedCartagePK, importedShipment.DocsAndCartage.DeliveryCartageCoPK);
			};

			shipmentValue.ShipmentDetails.Deliver.CartageCompany = CreateXsdOrganisation("GivenCartage");
			AssertEquals("Precondition: delivery cartage specified in xml", true, shipmentValue.ShipmentDetails.Deliver.CartageCompany.IsSpecified);

			assertDeliveryCartage("Delivery cartage set to the one specified in xml", givenCartage.PK);

			shipmentValue.ShipmentDetails.Deliver.CartageCompany = new Xsd.Organisation();
			AssertEquals("Precondition: delivery cartage not specified in xml", false, shipmentValue.ShipmentDetails.Deliver.CartageCompany.IsSpecified);

			assertDeliveryCartage("Delivery cartage defaulted to consignee's related", deliveryCartage.PK);
		}

		public void TestImport_PickupCartage()
		{
			OrgHeader consignee = CreateOrganisation("Consignee", OrganisationTypes.Consignee);
			OrgHeader pickupCartage = CreateOrganisation("PickupCartage", OrganisationTypes.Carrier);
			OrgHeader givenCartage = CreateOrganisation("GivenCartage", OrganisationTypes.Carrier);

			consignee.SetRelatedParty(pickupCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

			Factory.Save();

			Xsd.Shipment shipmentValue = CreateShipmentXSD(Xsd.TransportMode.SEA, Xsd.ContainerMode.FCL, "USLAX", "AUSYD");
			shipmentValue.ShipmentDetails.Consignee = CreateXsdOrganisation("Consignee");
			shipmentValue.ShipmentDetailsSpecified = true;

			Action<string, ZGuid> assertPickupCartage = (message, expectedCartagePK) =>
			{
				ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
				CommonShipment importedShipment = new ShipmentValueObjectDataAdapter<TBusinessObject>().CreateOrUpdateFromValueObject(shipmentValue, context);
				AssertEquals(message, expectedCartagePK, importedShipment.DocsAndCartage.PickupCartageCoPK);
			};

			shipmentValue.ShipmentDetails.Pickup.CartageCompany = CreateXsdOrganisation("GivenCartage");
			AssertEquals("Precondition: pickup cartage specified in xml", true, shipmentValue.ShipmentDetails.Pickup.CartageCompany.IsSpecified);

			assertPickupCartage("Pickup cartage set to the one specified in xml", givenCartage.PK);

			shipmentValue.ShipmentDetails.Pickup.CartageCompany = new Xsd.Organisation();
			AssertEquals("Precondition: pickup cartage not specified in xml", false, shipmentValue.ShipmentDetails.Pickup.CartageCompany.IsSpecified);

			assertPickupCartage("Pickup cartage not specified in xml => left empty", ZGuid.Empty);
		}

		public void TestImport_CustomsImportBroker()
		{
			OrgHeader consignee = CreateOrganisation("Consignee", OrganisationTypes.Consignee);
			OrgHeader importBroker = CreateOrganisation("ImportBroker", OrganisationTypes.Broker);
			OrgHeader relatedBroker = CreateOrganisation("RelatedBroker", OrganisationTypes.Broker);
			OrgHeader givenBroker = CreateOrganisation("GivenBroker", OrganisationTypes.Broker);

			CommonShipment existingShipment = Factory.New<CommonShipment>();
			existingShipment.JS_UniqueConsignRef = "HELLO";

			Factory.Save();

			Xsd.Shipment shipmentValue = CreateShipmentXSD(Xsd.TransportMode.SEA, Xsd.ContainerMode.FCL, "USLAX", "AUSYD");
			shipmentValue.ShipmentDetails.AgentReference = "HELLO";
			shipmentValue.ShipmentDetails.Consignee = CreateXsdOrganisation("Consignee");
			shipmentValue.ShipmentDetailsSpecified = true;

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.ShipmentNumber);

			Action<string, ZGuid> assertImportBroker = (message, expectedBrokerPK) =>
			{
				ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
				CommonShipment importedShipment = new ShipmentValueObjectDataAdapter<TBusinessObject>().CreateOrUpdateFromValueObject(shipmentValue, context);
				AssertEquals("Existing shipment matched by number", existingShipment.PK, importedShipment.PK);
				AssertEquals(message, expectedBrokerPK, importedShipment.JS_OH_ImportBroker);
			};

			consignee.SetRelatedParty(relatedBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty);
			AssertEquals("Precondition: consignee related broker", relatedBroker, consignee.DeliverySeaCustomsBroker);

			shipmentValue.ShipmentDetails.ImportBroker = CreateXsdOrganisation("GivenBroker");
			AssertEquals("Precondition: import broker specified in xml", true, shipmentValue.ShipmentDetails.ImportBroker.IsSpecified);

			existingShipment.JS_OH_ImportBroker = importBroker.PK;
			assertImportBroker("Import broker overriden to the one specified in xml", givenBroker.PK);

			existingShipment.JS_OH_ImportBroker = ZGuid.Empty;
			assertImportBroker("Import broker overriden to the one specified in xml", givenBroker.PK);

			shipmentValue.ShipmentDetails.ImportBroker = new Xsd.Organisation();
			AssertEquals("Precondition: import broker not specified in xml", false, shipmentValue.ShipmentDetails.ImportBroker.IsSpecified);

			existingShipment.JS_OH_ImportBroker = ZGuid.Empty;
			assertImportBroker("Import broker not specified in xml and empty on matched shipment => defaulted to consignee's related broker", relatedBroker.PK);

			existingShipment.JS_OH_ImportBroker = importBroker.PK;
			assertImportBroker("Import broker not specified in xml and not empty on matched shipment => not changed", importBroker.PK);
		}

		public void TestImport_CustomsExportBroker()
		{
			OrgHeader consignor = CreateOrganisation("Consignor", OrganisationTypes.Consignor);
			OrgHeader exportBroker = CreateOrganisation("ExportBroker", OrganisationTypes.Broker);
			OrgHeader relatedBroker = CreateOrganisation("RelatedBroker", OrganisationTypes.Broker);
			OrgHeader givenBroker = CreateOrganisation("GivenBroker", OrganisationTypes.Broker);

			CommonShipment existingShipment = Factory.New<CommonShipment>();
			existingShipment.JS_UniqueConsignRef = "HELLO";

			Factory.Save();

			Xsd.Shipment shipmentValue = CreateShipmentXSD(Xsd.TransportMode.SEA, Xsd.ContainerMode.FCL, "AUSYD", "USLAX");
			shipmentValue.ShipmentDetails.AgentReference = "HELLO";
			shipmentValue.ShipmentDetails.Consignor = CreateXsdOrganisation("Consignor");
			shipmentValue.ShipmentDetailsSpecified = true;

			SystemDataRegistry.Instance.ImportShipmentNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.ShipmentNumberImportTypes.Code.ShipmentNumber);

			Action<string, ZGuid> assertExportBroker = (message, expectedBrokerPK) =>
				{
					ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
					CommonShipment importedShipment = new ShipmentValueObjectDataAdapter<TBusinessObject>().CreateOrUpdateFromValueObject(shipmentValue, context);
					AssertEquals("Existing shipment matched by number", existingShipment.PK, importedShipment.PK);
					AssertEquals(message, expectedBrokerPK, importedShipment.JS_OH_ExportBroker);
				};

			consignor.SetRelatedParty(relatedBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);
			AssertEquals("Precondition: consignor related broker", relatedBroker, consignor.PickupSeaCustomsBroker);

			shipmentValue.ShipmentDetails.ExportBroker = CreateXsdOrganisation("GivenBroker");
			AssertEquals("Precondition: export broker specified in xml", true, shipmentValue.ShipmentDetails.ExportBroker.IsSpecified);

			existingShipment.JS_OH_ExportBroker = exportBroker.PK;
			assertExportBroker("Export broker overriden to the one specified in xml", givenBroker.PK);

			existingShipment.JS_OH_ExportBroker = ZGuid.Empty;
			assertExportBroker("Export broker overriden to the one specified in xml", givenBroker.PK);

			shipmentValue.ShipmentDetails.ExportBroker = new Xsd.Organisation();
			AssertEquals("Precondition: export broker not specified in xml", false, shipmentValue.ShipmentDetails.ExportBroker.IsSpecified);

			existingShipment.JS_OH_ExportBroker = ZGuid.Empty;
			assertExportBroker("Export broker not specified in xml and empty on matched shipment => defaulted to consignor's related broker", relatedBroker.PK);

			existingShipment.JS_OH_ExportBroker = exportBroker.PK;
			assertExportBroker("Export broker not specified in xml and not empty on matched shipment => not changed", exportBroker.PK);
		}

		OrgHeader CreateOrganisation(ZString fullName, OrganisationTypes orgType)
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OrganisationTypes = orgType;
			result.OH_Code = fullName.SubstringSafe(0, 9).ToUpper();
			result.OH_FullName = fullName;
			result.MainAddress.OA_Address1 = "address";

			return result;
		}

		Xsd.Organisation CreateXsdOrganisation(ZString fullName)
		{
			Xsd.Organisation result = new Xsd.Organisation();
			result.OrganisationDetails.Addresses.GetOrCreateMainAddress().AddressLine1 = "address";
			result.OwnerCode = fullName.SubstringSafe(0, 9).ToUpper();
			result.EDICode = result.OwnerCode;
			result.OrganisationDetails.Name = fullName;

			return result;
		}

		public void TestDefaultCustomImportBroker()
		{
			//Import 
			AssertDefaultCustomBroker(true, "MYPKG", "AUSYD");

			//Export
			AssertDefaultCustomBroker(false, "AUSYD", "MYPKG");
		}

		void AssertDefaultCustomBroker(bool isImport, ZString origin, ZString dest)
		{
			Xsd.Shipment shipmentValue = CreateShipmentXSD(Xsd.TransportMode.SEA, Xsd.ContainerMode.FCL, origin, dest);

			OrgHeader seaBroker = Factory.New<OrgHeader>();
			OrgHeader airBroker = Factory.New<OrgHeader>();
			seaBroker.OH_FullName = "SeaBroker";
			airBroker.OH_FullName = "AirBroker";

			TBusinessObject shipment = Factory.New<TBusinessObject>();
			OrgHeader consigneeOrConsignor = Factory.New<OrgHeader>();

			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = dest;

			if (isImport)
			{
				shipment.ConsigneePK = consigneeOrConsignor.PK;
				consigneeOrConsignor.SetRelatedParty(seaBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty);
				consigneeOrConsignor.SetRelatedParty(airBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
			}
			else
			{
				shipment.ConsignorPK = consigneeOrConsignor.PK;
				consigneeOrConsignor.SetRelatedParty(seaBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, ZString.Empty);
				consigneeOrConsignor.SetRelatedParty(airBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty);
			}

			shipmentValue.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			ShipmentValueObjectDataAdapter<TBusinessObject> dataAdapter = new ShipmentValueObjectDataAdapter<TBusinessObject>();

			if (isImport)
			{
				AssertNull("shipment's import broker is null", shipment.ImportBroker);

				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				dataAdapter.DefaultImportBroker(shipment);
				AssertEquals("ImportBroker", "SeaBroker", shipment.ImportBroker.OH_FullName);

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				dataAdapter.DefaultImportBroker(shipment);
				AssertEquals("ImportBroker", "AirBroker", shipment.ImportBroker.OH_FullName);
			}
			else
			{
				AssertNull("shipment's export broker is null", shipment.ExportBroker);

				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				dataAdapter.DefaultExportBroker(shipment);
				AssertEquals("ExportBroker", "SeaBroker", shipment.ExportBroker.OH_FullName);

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				dataAdapter.DefaultExportBroker(shipment);
				AssertEquals("ExportBroker", "AirBroker", shipment.ExportBroker.OH_FullName);
			}
		}

		public void TestDefaultImportExportCartage_MatchesALL()
		{
			var shipmentValue = CreateShipmentXSD(Xsd.TransportMode.ROA, Xsd.ContainerMode.BCN, "MYPKG", "AUSYD");
			var deliveryCartage = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCartage.OH_FullName = "Delivery Cartage";
			var pickupCartage = Factory.NewWithValidTestData<OrgHeader>();
			pickupCartage.OH_FullName = "Pickup Cartage";

			TBusinessObject shipment = Factory.New<TBusinessObject>();
			var consignee = Factory.New<OrgHeader>();
			var consignor = Factory.New<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;

			consignee.SetRelatedParty(deliveryCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
			consignor.SetRelatedParty(pickupCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);

			var dataAdapter = new ShipmentValueObjectDataAdapter<TBusinessObject>();
			dataAdapter.DefaultImportCartage(shipment, shipmentValue);
			dataAdapter.DefaultExportCartage(shipment, shipmentValue);
			AssertEquals("Delivery Cartage", shipment.DocsAndCartage.DeliveryCartageCo.OH_FullName);
			AssertEquals("Pickup Cartage", shipment.DocsAndCartage.PickupCartageCo.OH_FullName);
		}

		public void TestDefaultImportCartage()
		{
			Xsd.Shipment shipmentValue = CreateShipmentXSD(Xsd.TransportMode.SEA, Xsd.ContainerMode.FCL, "MYPKG", "AUSYD");

			OrgHeader seaFCLCartage = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader seaLCLCartage = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader airCartage1 = Factory.NewWithValidTestData<OrgHeader>();
			seaFCLCartage.OH_FullName = "SeaFCLCartage";
			seaLCLCartage.OH_FullName = "SeaLCLCartage";
			airCartage1.OH_FullName = "AirCartage";

			TBusinessObject shipment = Factory.New<TBusinessObject>();
			OrgHeader consignee = Factory.New<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;

			consignee.SetRelatedParty(seaFCLCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			consignee.SetRelatedParty(seaLCLCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			consignee.SetRelatedParty(airCartage1, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);

			shipmentValue.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			shipmentValue.ShipmentDetails.PackingMode = Xsd.ContainerMode.FCL;
			ShipmentValueObjectDataAdapter<TBusinessObject> dataAdapter = new ShipmentValueObjectDataAdapter<TBusinessObject>();
			dataAdapter.DefaultImportCartage(shipment, shipmentValue);
			AssertEquals("ImportCartage", "SeaFCLCartage", shipment.DocsAndCartage.DeliveryCartageCo.OH_FullName);

			shipmentValue.ShipmentDetails.PackingMode = Xsd.ContainerMode.LCL;
			dataAdapter.DefaultImportCartage(shipment, shipmentValue);
			AssertEquals("ImportCartage", "SeaLCLCartage", shipment.DocsAndCartage.DeliveryCartageCo.OH_FullName);

			shipmentValue.ShipmentDetails.TransportMode = Xsd.TransportMode.AIR;
			dataAdapter.DefaultImportCartage(shipment, shipmentValue);
			AssertEquals("ImportCartage", "AirCartage", shipment.DocsAndCartage.DeliveryCartageCo.OH_FullName);

			OrgHeader airCartage = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader fclCartage = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader lclCartage = Factory.NewWithValidTestData<OrgHeader>();

			FreightDataRegistry.Instance.FCLCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, fclCartage.PK.ToGuid());
			FreightDataRegistry.Instance.LCLCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, lclCartage.PK.ToGuid());
			FreightDataRegistry.Instance.AIRCartageCompany.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, airCartage.PK.ToGuid());

			consignee.AllRelatedParties.RemoveAndDeleteAll();

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			GlbBranch.CurrentBranch.ExtraPorts.AddNew().GY_RL_NKAdditionalBranchRelatedPort = "AUBNE";
			GlbBranch.CurrentBranch.ExtraPorts.AddNew().GY_RL_NKAdditionalBranchRelatedPort = "AUMEL";

			OrgHeader orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			orgConsignee.OH_RL_NKClosestPort = "NZAKL";
			shipment.ConsignorPK = orgConsignee.PK;

			shipmentValue.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			shipmentValue.ShipmentDetails.PackingMode = Xsd.ContainerMode.FCL;
			shipment.DocsAndCartage.DeliveryCartageCoPK = ZGuid.Empty;

			dataAdapter.DefaultImportCartage(shipment, shipmentValue);
			AssertEquals("No default cartage company", ZGuid.Empty, shipment.DocsAndCartage.DeliveryCartageCoPK);

			orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			orgConsignee.OH_RL_NKClosestPort = "AUSYD";
			shipment.ConsigneePK = orgConsignee.PK;
			shipment.DocsAndCartage.DeliveryCartageCoPK = ZGuid.Empty;

			dataAdapter.DefaultImportCartage(shipment, shipmentValue);
			AssertEquals("Default cartage company - defaulting from registry, consignee unloco = branch home port", fclCartage.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);

			orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			orgConsignee.OH_RL_NKClosestPort = "AUBNE";
			shipment.ConsigneePK = orgConsignee.PK;
			shipment.DocsAndCartage.DeliveryCartageCoPK = ZGuid.Empty;

			dataAdapter.DefaultImportCartage(shipment, shipmentValue);
			AssertEquals("Default cartage company - defaulting from registry, consignee unloco = one of branch related ports", fclCartage.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);

			shipmentValue.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			shipmentValue.ShipmentDetails.PackingMode = Xsd.ContainerMode.FCL;

			dataAdapter.DefaultImportCartage(shipment, shipmentValue);
			AssertEquals("FCL Cartage Company", fclCartage.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);

			shipmentValue.ShipmentDetails.PackingMode = Xsd.ContainerMode.LCL;
			dataAdapter.DefaultImportCartage(shipment, shipmentValue);
			AssertEquals("LCL Cartage Company", lclCartage.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);

			shipmentValue.ShipmentDetails.TransportMode = Xsd.TransportMode.AIR;
			dataAdapter.DefaultImportCartage(shipment, shipmentValue);
			AssertEquals("AIR Cartage Company", airCartage.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);
		}

		#endregion

		public void TestContainerNumberNotFoundValidation()
		{
			TConsol consol = Factory.New<TConsol>();
			Xsd.Shipment shipmentValue = CreateXsdShipmentForImportWithMinimumData();
			shipmentValue.ShipmentDetailsSpecified = true;
			Xsd.Package package = shipmentValue.ShipmentDetails.Packages.AddNew();
			package.ContainerNumber = "splaty";

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter(consol);
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertEquals(
				"Should have an error for the container number",
				true, notify.ContainsNotificationType(FreightErrorType.ContainerNumberNotFound));
			AssertEquals(
				"Should have the correct message, with ticks around the container number in case the container number is empty",
				true, notify.AsString.IndexOf("Container number not found (splaty)") != -1);
		}

		public void TestContainerNumberCanBeEmpty()
		{
			TConsol consol = Factory.New<TConsol>();
			Xsd.Shipment shipmentValue = new Xsd.Shipment();
			Xsd.Package package = shipmentValue.ShipmentDetails.Packages.AddNew();
			package.ContainerNumber = "";

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter(consol);
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertEquals(
				"Should not have an error for the container number as it is valid for it to be empty",
				false, notify.ContainsNotificationType(FreightErrorType.ContainerNumberNotFound));
		}

		public void TestImportChargeableWeight_NotCalculated()
		{
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetailsSpecified = true;
			xmlShipment.ShipmentDetails.Weight = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(10m), "KG");
			xmlShipment.ShipmentDetails.Volume = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(10m), "M3");
			xmlShipment.ShipmentDetails.ChargeableWeight = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(0m), "KG");

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = new ShipmentValueObjectDataAdapter<TBusinessObject>();
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals("Chargeable weight should not be Calculated", 0m, shipment.JS_ActualChargeable);
			AssertEquals("Chargeable weight should not be Calculated", 0m, shipment.JS_DocumentedChargeable);
			AssertEquals("Chargeable weight should not be Calculated", 0m, shipment.JS_ManifestedChargeable);

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals("Chargeable weight should not be Calculated", 0m, shipment.JS_ActualChargeable);
			AssertEquals("Chargeable weight should not be Calculated", 0m, shipment.JS_DocumentedChargeable);
			AssertEquals("Chargeable weight should not be Calculated", 0m, shipment.JS_ManifestedChargeable);
		}

		public void TestImportChargeableWeight_Given()
		{
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetailsSpecified = true;
			xmlShipment.ShipmentDetails.Weight = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(10m), "KG");
			xmlShipment.ShipmentDetails.Volume = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(10m), "M3");
			xmlShipment.ShipmentDetails.ChargeableWeight = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(73m), "KG");

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = new ShipmentValueObjectDataAdapter<TBusinessObject>();
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals("Chargeable weight has been set from xml, should keep as 73", 73m, shipment.JS_ActualChargeable);
			AssertEquals("Chargeable weight has been set from xml, should keep as 73", 73m, shipment.JS_DocumentedChargeable);
			AssertEquals("Chargeable weight has been set from xml, should keep as 73", 73m, shipment.JS_ManifestedChargeable);

			shipment.JS_ActualChargeable = 50m;
			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals("Chargeable weight has been set from xml, should keep as 73", 73m, shipment.JS_ActualChargeable);
			AssertEquals("Chargeable weight has been set from xml, should keep as 73", 73m, shipment.JS_DocumentedChargeable);
			AssertEquals("Chargeable weight has been set from xml, should keep as 73", 73m, shipment.JS_ManifestedChargeable);
		}

		public void TestImportChargeableWeight_NotGiven()
		{
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetailsSpecified = true;
			xmlShipment.ShipmentDetails.Weight = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(10m), "KG");
			xmlShipment.ShipmentDetails.Volume = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(10m), "M3");

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = new ShipmentValueObjectDataAdapter<TBusinessObject>();
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals("Chargeable weight should be calculated", 10m, shipment.JS_ActualChargeable);
			AssertEquals("Chargeable weight should be calculated", 10m, shipment.JS_DocumentedChargeable);
			AssertEquals("Chargeable weight should be calculated", 10m, shipment.JS_ManifestedChargeable);

			shipment.JS_ActualChargeable = 20m;
			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals("Chargeable weight should not be calculated", 20m, shipment.JS_ActualChargeable);
			AssertEquals("Chargeable weight should not be calculated", 20m, shipment.JS_DocumentedChargeable);
			AssertEquals("Chargeable weight should not be calculated", 20m, shipment.JS_ManifestedChargeable);
		}

		public void TestImportDeliveryFromDate()
		{
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetailsSpecified = true;
			xmlShipment.ShipmentDetails.Deliver.DeliveryFrom = new ZDateTime(2005, 8, 9);

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals(xmlShipment.ShipmentDetails.Deliver.DeliveryFrom, shipment.DocsAndCartage.JP_EstimatedDelivery);

			xmlShipment.ShipmentDetails.Deliver.DeliveryFrom = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2005, 7, 6);
			AssertEquals(new ZDateTime(2005, 7, 6), shipment.DocsAndCartage.JP_EstimatedDelivery);
		}

		public void TestImportDeliveryRequiredByDate()
		{
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetailsSpecified = true;
			xmlShipment.ShipmentDetails.Deliver.DeliveryRequiredBy = new ZDateTime(2005, 8, 9);

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals(xmlShipment.ShipmentDetails.Deliver.DeliveryRequiredBy, shipment.DocsAndCartage.JP_DeliveryRequiredBy);

			xmlShipment.ShipmentDetails.Deliver.DeliveryRequiredBy = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_DeliveryRequiredBy = new ZDateTime(2005, 7, 6);
			AssertEquals(new ZDateTime(2005, 7, 6), shipment.DocsAndCartage.JP_DeliveryRequiredBy);
		}

		public void TestImportDeliveryAgent()
		{
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetailsSpecified = true;

			TBusinessObject shipment = Factory.New<TBusinessObject>();
			OrgHeader deliveryAgent = Factory.New<OrgHeader>();
			deliveryAgent.OH_Code = "OLDAGENT";
			shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, xmlShipment, context);
			AssertEquals("Delivery agent was not overridden with empty value from XML", "OLDAGENT", shipment.DeliveryAgent.OH_Code);

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			adapter.ImportFromValueObject(shipment, xmlShipment, context);
			AssertNull("Delivery agent was overridden with empty value from XML", shipment.DeliveryAgent);

			xmlShipment.ShipmentDetails.Deliver.DeliveryAgent = CreateXsdOrganisation("NEWAGENT");

			adapter.ImportFromValueObject(shipment, xmlShipment, context);
			AssertEquals("Delivery agent was overridden with new value from XML", "NEWAGE", shipment.DeliveryAgent.OH_Code);
		}

		public void TestImportCartageAdvisedDate()
		{
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetailsSpecified = true;
			xmlShipment.ShipmentDetails.Deliver.CartageAdvised = new ZDateTime(2005, 8, 9);

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals(xmlShipment.ShipmentDetails.Deliver.CartageAdvised, shipment.DocsAndCartage.JP_DeliveryCartageAdvised);

			xmlShipment.ShipmentDetails.Deliver.CartageAdvised = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2005, 7, 6);
			AssertEquals(new ZDateTime(2005, 7, 6), shipment.DocsAndCartage.JP_DeliveryCartageAdvised);
		}

		public void TestImportGoodsDeliveredDate()
		{
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetailsSpecified = true;
			xmlShipment.ShipmentDetails.Deliver.GoodsDelivered = new ZDateTime(2005, 8, 9);

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals(xmlShipment.ShipmentDetails.Deliver.GoodsDelivered, shipment.DocsAndCartage.JP_DeliveryCartageCompleted);

			xmlShipment.ShipmentDetails.Deliver.GoodsDelivered = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2005, 7, 6);
			AssertEquals(new ZDateTime(2005, 7, 6), shipment.DocsAndCartage.JP_DeliveryCartageCompleted);
		}

		public void TestImportPickupFromDate()
		{
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetailsSpecified = true;
			xmlShipment.ShipmentDetails.Pickup.PickupFrom = new ZDateTime(2005, 8, 9);

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals(xmlShipment.ShipmentDetails.Pickup.PickupFrom, shipment.DocsAndCartage.JP_EstimatedPickup);

			xmlShipment.ShipmentDetails.Pickup.PickupFrom = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2005, 7, 6);
			AssertEquals(new ZDateTime(2005, 7, 6), shipment.DocsAndCartage.JP_EstimatedPickup);
		}

		public void TestImportPickupRequiredByDate()
		{
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetailsSpecified = true;
			xmlShipment.ShipmentDetails.Pickup.PickupRequiredBy = new ZDateTime(2005, 8, 9);

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals(xmlShipment.ShipmentDetails.Pickup.PickupRequiredBy, shipment.DocsAndCartage.JP_PickupRequiredBy);

			xmlShipment.ShipmentDetails.Pickup.PickupRequiredBy = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2005, 7, 6);
			AssertEquals(new ZDateTime(2005, 7, 6), shipment.DocsAndCartage.JP_PickupRequiredBy);
		}

		public void TestImportPickupCartageAdvisedDate()
		{
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetailsSpecified = true;
			xmlShipment.ShipmentDetails.Pickup.CartageAdvised = new ZDateTime(2005, 8, 9);

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals(xmlShipment.ShipmentDetails.Pickup.CartageAdvised, shipment.DocsAndCartage.JP_PickupCartageAdvised);

			xmlShipment.ShipmentDetails.Pickup.CartageAdvised = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_PickupCartageAdvised = new ZDateTime(2005, 7, 6);
			AssertEquals(new ZDateTime(2005, 7, 6), shipment.DocsAndCartage.JP_PickupCartageAdvised);
		}

		public void TestImportGoodsPickupDate()
		{
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetailsSpecified = true;
			xmlShipment.ShipmentDetails.Pickup.GoodsPickup = new ZDateTime(2005, 8, 9);

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals(xmlShipment.ShipmentDetails.Pickup.GoodsPickup, shipment.DocsAndCartage.JP_PickupCartageCompleted);

			xmlShipment.ShipmentDetails.Pickup.GoodsPickup = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2005, 7, 6);
			AssertEquals(new ZDateTime(2005, 7, 6), shipment.DocsAndCartage.JP_PickupCartageCompleted);
		}

		public void TestImportDateofReceipt()
		{
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetailsSpecified = true;
			xmlShipment.ShipmentDetails.Pickup.DateOfReceipt = new ZDateTime(2005, 8, 9);

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals(xmlShipment.ShipmentDetails.Pickup.DateOfReceipt, shipment.JS_A_RCV);

			xmlShipment.ShipmentDetails.Pickup.DateOfReceipt = ZDateTime.Empty;
			shipment.JS_A_RCV = new ZDateTime(2005, 7, 6);
			AssertEquals(new ZDateTime(2005, 7, 6), shipment.JS_A_RCV);
		}

		public void TestImportAgentsReference()
		{
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetails.AgentReference = "BLAH";

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals("Shipment should NOT have uniqueconsign ref = to agents ref", ZString.Empty, shipment.JS_UniqueConsignRef);
		}

		public void TestImportAgentsReferenceIsSavedAsOtherAgentReference()
		{
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetails.AgentReference = "hello";

			var shipment = Factory.New<TBusinessObject>();

			Action<string, ZString[]> assertOtherAgentReferences = (message, expectedReferences) =>
			{
				var actualReferences = shipment.Numbers.GetAllReferenceNumbersByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.OtherAgentReference);
				AssertContainsExactElementsInAnyOrder(message, expectedReferences, actualReferences);
			};

			assertOtherAgentReferences("Precondition", Array.Empty<ZString>());

			var adapter = GetNewShipmentValueObjectDataAdapter();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, xmlShipment, context);
			assertOtherAgentReferences("Agent reference was saved as Other Agent Reference", new ZString[] { "hello" });

			adapter.ImportFromValueObject(shipment, xmlShipment, context);
			assertOtherAgentReferences("Not creating duplicates", new ZString[] { "hello" });

			xmlShipment.ShipmentDetails.AgentReference = "";
			adapter.ImportFromValueObject(shipment, xmlShipment, context);
			assertOtherAgentReferences("Not adding empty agent reference", new ZString[] { "hello" });
		}

		public void TestImportInterimReceipt()
		{
			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			xmlShipment.ShipmentDetails.InterimReceipt = "BLAH";

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertEquals("Shipment Interim Receipt set to \'BLAH\'", "BLAH", shipment.JS_InterimReceipt);
		}

		public void TestImportCustomAttributes()
		{
			Xsd.Shipment shipmentValue = CreateXsdShipmentForImportWithMinimumData();
			shipmentValue.ShipmentDetailsSpecified = true;

			shipmentValue.ShipmentDetails.Custom.CustomAttribute1 = "CustAttrib1";
			shipmentValue.ShipmentDetails.Custom.CustomAttribute2 = "CustAttrib2";
			shipmentValue.ShipmentDetails.Custom.Date1 = new ZDateTime(2005, 11, 30, 15, 0, 0);
			shipmentValue.ShipmentDetails.Custom.Date2 = ZDateTime.Empty;

			shipmentValue.ShipmentDetails.Custom.Decimal1 = 21m;
			shipmentValue.ShipmentDetails.Custom.Decimal2 = 86.45m;
			shipmentValue.ShipmentDetails.Custom.Flag1 = Xsd.TrueFalse.@true;
			shipmentValue.ShipmentDetails.Custom.Flag2 = Xsd.TrueFalse.@false;

			shipmentValue.ShipmentDetails.Custom.Decimal1Specified = true;
			shipmentValue.ShipmentDetails.Custom.Decimal2Specified = true;
			shipmentValue.ShipmentDetails.Custom.Flag1Specified = true;
			shipmentValue.ShipmentDetails.Custom.Flag2Specified = true;

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertEquals("Custom Attribute 1", "CustAttrib1", shipment.DocsAndCartage.JP_CustomAttrib1);
			AssertEquals("Custom Attribute 2", "CustAttrib2", shipment.DocsAndCartage.JP_CustomAttrib2);
			AssertEquals("Date 1", new ZDateTime(2005, 11, 30, 15, 0, 0), shipment.DocsAndCartage.JP_CustomDate1);
			AssertEquals("Date 1", ZDateTime.Empty, shipment.DocsAndCartage.JP_CustomDate2);
			AssertEquals("Decimal 1", 21m, shipment.DocsAndCartage.JP_CustomDecimal1);
			AssertEquals("Decimal 2", 86.45m, shipment.DocsAndCartage.JP_CustomDecimal2);
			AssertEquals("Flag 1", true, shipment.DocsAndCartage.JP_CustomFlag1);
			AssertEquals("Flag 1", false, shipment.DocsAndCartage.JP_CustomFlag2);

			shipmentValue.ShipmentDetails.Custom.Decimal1Specified = false;
			shipmentValue.ShipmentDetails.Custom.Decimal2Specified = false;
			shipmentValue.ShipmentDetails.Custom.Flag1Specified = false;
			shipmentValue.ShipmentDetails.Custom.Flag2Specified = false;

			adapter = GetNewShipmentValueObjectDataAdapter();
			shipment = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertEquals("Decimal 1", 0m, shipment.DocsAndCartage.JP_CustomDecimal1);
			AssertEquals("Decimal 2", 0m, shipment.DocsAndCartage.JP_CustomDecimal2);
			AssertEquals("Flag 1", false, shipment.DocsAndCartage.JP_CustomFlag1);
			AssertEquals("Flag 1", false, shipment.DocsAndCartage.JP_CustomFlag2);
		}

		public void TestImport_CreateTransportPlan()
		{
			Xsd.Shipment shipmentValue = CreateXsdShipmentForImportWithMinimumData();
			shipmentValue.ShipmentDetailsSpecified = true;

			Xsd.PlannedLeg plannedLeg = shipmentValue.ShipmentDetails.TransportPlan.AddNew();
			plannedLeg.TransportMode = Xsd.TransportMode.AIR;
			plannedLeg.TransportType = Xsd.PlannedLegTransportType.Flight1;
			plannedLeg.TransportTypeSpecified = true;
			plannedLeg.PortOfDischarge = XsdMovement.FromPortEstimatedActualDates(Factory, "AUBNE", new ZDateTime(2005, 11, 21, 17, 7, 23), new ZDateTime(2005, 11, 21, 17, 8, 45));
			plannedLeg.PortOfLoading = XsdMovement.FromPortEstimatedActualDates(Factory, "NZAKL", new ZDateTime(2005, 11, 21, 17, 9, 52), new ZDateTime(2005, 11, 21, 17, 10, 59));

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);

			AssertEquals("1 Shipment Transport should be created", 1, shipment.Transports.Count);
			Transport transport = shipment.Transports[0];
			AssertEquals("Transport Mode", Core.Constants.TransportModes.Air, transport.JW_TransportMode);
			AssertEquals("Transport Type", Core.Constants.TransportPlanningType.Flight1, transport.JW_TransportType);
			AssertEquals("Port of Discharge", "AUBNE", transport.JW_RL_NKDiscPort);
			AssertEquals("Port of Loading", "NZAKL", transport.JW_RL_NKLoadPort);
			AssertEquals("Estimated Time of Arrival", new ZDateTime(2005, 11, 21, 17, 7, 0), transport.JW_ETA);
			AssertEquals("Actual Time of Arrival", new ZDateTime(2005, 11, 21, 17, 9, 0), transport.JW_ATA);
			AssertEquals("Estimated Time of Departure", new ZDateTime(2005, 11, 21, 17, 10, 00), transport.JW_ETD);
			AssertEquals("Actual Time of Departure", new ZDateTime(2005, 11, 21, 17, 11, 00), transport.JW_ATD);
		}

		public void TestImportTransportPlan_TransportTypeNotSpecified()
		{
			Xsd.Shipment shipmentValue = CreateXsdShipmentForImportWithMinimumData();
			shipmentValue.ShipmentDetailsSpecified = true;
			Xsd.PlannedLeg plannedLeg = shipmentValue.ShipmentDetails.TransportPlan.AddNew();
			plannedLeg.TransportType = Xsd.PlannedLegTransportType.OnForwarding;
			plannedLeg.TransportTypeSpecified = false;

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertEquals("1 Shipment Transport should be created", 1, shipment.Transports.Count);
			Transport transport = shipment.Transports[0];
			AssertEquals("Transport Type", "MAI", transport.JW_TransportType);
		}

		public void TestImport_UpdateTransportPlan()
		{
			Xsd.Shipment shipmentValue = CreateXsdShipmentForImportWithMinimumData();
			shipmentValue.ShipmentDetailsSpecified = true;

			Xsd.PlannedLeg plannedLeg = shipmentValue.ShipmentDetails.TransportPlan.AddNew();
			plannedLeg.TransportMode = Xsd.TransportMode.AIR;
			plannedLeg.TransportType = Xsd.PlannedLegTransportType.Flight1;
			plannedLeg.TransportTypeSpecified = true;
			Xsd.FlightWithFlightNumber flight = new Xsd.FlightWithFlightNumber();
			flight.FlightNoJourneyNoTruckRegNo = "QF1244";
			plannedLeg.Item = flight;

			ZDateTime eta = new ZDateTime(2005, 11, 21, 17, 7, 00);
			ZDateTime ata = new ZDateTime(2005, 11, 21, 17, 9, 00);
			ZDateTime etd = new ZDateTime(2005, 11, 21, 17, 10, 00);
			ZDateTime atd = new ZDateTime(2005, 11, 21, 17, 11, 00);

			plannedLeg.PortOfDischarge = XsdMovement.FromPortEstimatedActualDates(Factory, "AUBNE", eta, ata);
			plannedLeg.PortOfLoading = XsdMovement.FromPortEstimatedActualDates(Factory, "NZAKL", etd, atd);

			CommonShipment shipment = Factory.New<TBusinessObject>();
			Transport transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			transport.JW_ETA = eta;
			transport.JW_ATA = ata;
			transport.JW_ETD = etd;
			transport.JW_ATD = atd;
			transport.JW_IsLinked = true;

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			shipment = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);

			AssertEquals("1 Shipment Transport should be created", 1, shipment.Transports.Count);
			transport = shipment.Transports[0];
			AssertEquals("Transport Mode", Core.Constants.TransportModes.Air, transport.JW_TransportMode);
			AssertEquals("Transport Type", Core.Constants.TransportPlanningType.Flight1, transport.JW_TransportType);
			AssertEquals("Port of Discharge", "AUBNE", transport.JW_RL_NKDiscPort);
			AssertEquals("Port of Loading", "NZAKL", transport.JW_RL_NKLoadPort);
			AssertEquals("Estimated Time of Arrival", eta, transport.JW_ETA);
			AssertEquals("Actual Time of Arrival", ata, transport.JW_ATA);
			AssertEquals("Estimated Time of Departure", etd, transport.JW_ETD);
			AssertEquals("Actual Time of Departure", atd, transport.JW_ATD);
			AssertEquals("VoyageFlight", "QF1244", transport.JW_VoyageFlight);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImporteDocs()
		{
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			Xsd.Shipment xSDshipment = CreateXsdShipmentForImportWithMinimumData();
			Xsd.Document document = xSDshipment.Documents.AddNew();
			document.Data = File.ReadAllBytes(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "DocumentScanning", "DocumentScanning.Business.Test", "TestDocs", "Small.tif"));
			document.DataType = "TIF";
			document.Date = new ZDateTime(2005, 12, 6);
			document.Description = "Housebill";
			document.DocumentType = "HBL";
			document.IsPublished = Xsd.TrueFalse.@true;

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonShipment shipment = adapter.CreateOrUpdateFromValueObject(xSDshipment, importContext);

			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			IStorageMainForPK documentFactory = (IStorageMainForPK)documentFactoryProvider.GetFactory(Factory);
			IDocumentsView storageMain = documentFactory.GetStorageMain(shipment.PK);
			AssertNotNull("Shipment should have a StorageMain", storageMain);
			AssertEquals("StorageMain should have 1 StorageDocs", 1, storageMain.DocumentCollectionView.Count);
			BusinessObject storageDoc = ((IBusinessObjectCollectionView)storageMain.DocumentCollectionView).ToArray()[0];
			AssertEquals("Date Type", "TIF", storageDoc[StorageDocsSchema.SC_DataType.Name]);
			AssertEquals("Data", document.Data, storageDoc[StorageDocsSchema.SC_ImageData.Name]);
			AssertEquals("Date", new ZDateTime(2005, 12, 6), storageDoc[StorageDocsSchema.SC_Date.Name]);
			AssertEquals("Description", "Housebill", storageDoc[StorageDocsSchema.SC_Desc.Name]);
			AssertEquals("Document Type", "HBL", storageDoc[StorageDocsSchema.SC_DocType.Name]);
		}

		public void TestImportDecimals_OutOfRange()
		{
			var adapter = GetNewShipmentValueObjectDataAdapter();
			var xsdShipment = CreateXsdShipmentForImportWithMinimumData();
			var details = xsdShipment.ShipmentDetails;

			var outOfSqlRangeDecimal = 9876543210.1M;
			var bigOutOfSqlRangeDecimal = Decimal.MaxValue;
			var expectedValue = 0M;

			details.Volume.Value = outOfSqlRangeDecimal;
			details.Weight.Value = outOfSqlRangeDecimal;
			details.ChargeableWeight.Value = outOfSqlRangeDecimal;
			details.LoadingMeters = outOfSqlRangeDecimal;
			details.GoodsValue.Value = bigOutOfSqlRangeDecimal;
			details.InsuranceValue.Value = bigOutOfSqlRangeDecimal;
			details.FreightRate.Value = bigOutOfSqlRangeDecimal;
			details.ConsignorCODAmount = bigOutOfSqlRangeDecimal;

			details.Custom.Decimal1 = outOfSqlRangeDecimal;
			details.Custom.Decimal2 = outOfSqlRangeDecimal;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var shipment = adapter.CreateOrUpdateFromValueObject(xsdShipment, context);

			AssertEquals("JS_ActualChargeable", expectedValue, shipment.JS_ActualChargeable);
			AssertEquals("JS_ActualVolume", expectedValue, shipment.JS_ActualVolume);
			AssertEquals("JS_ActualWeight", expectedValue, shipment.JS_ActualWeight);
			AssertEquals("JS_DocumentedChargeable", expectedValue, shipment.JS_DocumentedChargeable);
			AssertEquals("JS_DocumentedLoadingMeters", expectedValue, shipment.JS_DocumentedLoadingMeters);
			AssertEquals("JS_DocumentedVolume", expectedValue, shipment.JS_DocumentedVolume);
			AssertEquals("JS_DocumentedWeight", expectedValue, shipment.JS_DocumentedWeight);
			AssertEquals("JS_LoadingMeters", expectedValue, shipment.JS_LoadingMeters);
			AssertEquals("JS_ManifestedChargeable", expectedValue, shipment.JS_ManifestedChargeable);
			AssertEquals("JS_ManifestedLoadingMeters", expectedValue, shipment.JS_ManifestedLoadingMeters);
			AssertEquals("JS_ManifestedVolume", expectedValue, shipment.JS_ManifestedVolume);
			AssertEquals("JS_ManifestedWeight", expectedValue, shipment.JS_ManifestedWeight);
			AssertEquals("JS_GoodsValue", expectedValue, shipment.JS_GoodsValue);
			AssertEquals("JS_InsuranceValue", expectedValue, shipment.JS_InsuranceValue);
			AssertEquals("JS_UnitFreightRate", expectedValue, shipment.JS_UnitFreightRate);
			AssertEquals("JS_ShipperCODAmount", expectedValue, shipment.JS_ShipperCODAmount);

			AssertEquals("JP_CustomDecimal1", expectedValue, shipment.DocsAndCartage.JP_CustomDecimal1);
			AssertEquals("JP_CustomDecimal2", expectedValue, shipment.DocsAndCartage.JP_CustomDecimal2);

			var expectedVolume = 111.1M;
			var expectedWeight = 222.2M;
			var expectedChargeable = 333.3M;
			var expectedLoadingMeters = 444.4M;
			var expectedMonetaryValue = 555.5M;
			var expectedCustomValue = 666.6M;

			details.Volume.Value = expectedVolume;
			details.Weight.Value = expectedWeight;
			details.ChargeableWeight.Value = expectedChargeable;
			details.LoadingMeters = expectedLoadingMeters;
			details.GoodsValue.Value = expectedMonetaryValue;
			details.InsuranceValue.Value = expectedMonetaryValue;
			details.FreightRate.Value = expectedMonetaryValue;
			details.ConsignorCODAmount = expectedMonetaryValue;

			details.Custom.Decimal1 = expectedCustomValue;
			details.Custom.Decimal2 = expectedCustomValue;

			shipment = adapter.CreateOrUpdateFromValueObject(xsdShipment, context);

			AssertEquals("JS_ActualChargeable", expectedChargeable, shipment.JS_ActualChargeable);
			AssertEquals("JS_ActualVolume", expectedVolume, shipment.JS_ActualVolume);
			AssertEquals("JS_ActualWeight", expectedWeight, shipment.JS_ActualWeight);
			AssertEquals("JS_DocumentedChargeable", expectedChargeable, shipment.JS_DocumentedChargeable);
			AssertEquals("JS_DocumentedLoadingMeters", expectedLoadingMeters, shipment.JS_DocumentedLoadingMeters);
			AssertEquals("JS_DocumentedVolume", expectedVolume, shipment.JS_DocumentedVolume);
			AssertEquals("JS_DocumentedWeight", expectedWeight, shipment.JS_DocumentedWeight);
			AssertEquals("JS_LoadingMeters", expectedLoadingMeters, shipment.JS_LoadingMeters);
			AssertEquals("JS_ManifestedChargeable", expectedChargeable, shipment.JS_ManifestedChargeable);
			AssertEquals("JS_ManifestedLoadingMeters", expectedLoadingMeters, shipment.JS_ManifestedLoadingMeters);
			AssertEquals("JS_ManifestedVolume", expectedVolume, shipment.JS_ManifestedVolume);
			AssertEquals("JS_ManifestedWeight", expectedWeight, shipment.JS_ManifestedWeight);
			AssertEquals("JS_GoodsValue", expectedMonetaryValue, shipment.JS_GoodsValue);
			AssertEquals("JS_InsuranceValue", expectedMonetaryValue, shipment.JS_InsuranceValue);
			AssertEquals("JS_UnitFreightRate", expectedMonetaryValue, shipment.JS_UnitFreightRate);
			AssertEquals("JS_ShipperCODAmount", expectedMonetaryValue, shipment.JS_ShipperCODAmount);

			AssertEquals("JP_CustomDecimal1", expectedCustomValue, shipment.DocsAndCartage.JP_CustomDecimal1);
			AssertEquals("JP_CustomDecimal2", expectedCustomValue, shipment.DocsAndCartage.JP_CustomDecimal2);
		}

		public void TestExportDeliveryFromDate()
		{
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			shipment.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2005, 4, 13);
			Xsd.Shipment xmlShipment = GetExportedValueObject(shipment);
			AssertEquals(shipment.DocsAndCartage.JP_EstimatedDelivery, xmlShipment.ShipmentDetails.Deliver.DeliveryFrom);
		}

		public void TestExportDeliveryRequiredByDate()
		{
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			shipment.DocsAndCartage.JP_DeliveryRequiredBy = new ZDateTime(2005, 4, 13);
			Xsd.Shipment xmlShipment = GetExportedValueObject(shipment);
			AssertEquals(shipment.DocsAndCartage.JP_DeliveryRequiredBy, xmlShipment.ShipmentDetails.Deliver.DeliveryRequiredBy);
		}

		public void TestExportDeliveryLegs()
		{
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			PackLine line = shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
			confirm.EU_GoodsSignForBy = "Mr Bob";

			Xsd.Shipment xmlShipment = GetExportedValueObject(shipment);
			Assert(xmlShipment.ShipmentDetails.Deliver.DeliveryLegs.IsSpecified);
			AssertEquals(1, xmlShipment.ShipmentDetails.Deliver.DeliveryLegs.Count);
			AssertEquals(confirm.EU_GoodsSignForBy, xmlShipment.ShipmentDetails.Deliver.DeliveryLegs[0].GoodsRecBy);
			AssertEquals(Enterprise.DataTransfer.Xml.XsdVersion1.ContainerLegType.DLV, xmlShipment.ShipmentDetails.Deliver.DeliveryLegs[0].LegType);
			//AssertEquals(Enterprise.DataTransfer.Xml.XsdVersion1.ContainerLegType.FCI, XmlShipment.ShipmentDetails.Deliver.DeliveryLegs[0].LegType);
		}

		public void TestExportDeliveryLegsDoesNotCreateConfirmIfNotExist()
		{
			var shipment = Factory.New<TBusinessObject>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var transportCoAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportCoAddress.FillWithValidTestData();

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "CRMORG";

			var consol = shipment.Consols.AddNew();

			var container = consol.Containers.AddNew();
			shipment.OuterPackLines.AddNew().SetContainer(container.PK);

			var now = ZDateTime.Now;

			var confirm = container.Confirms.AddNew();
			SetupConfirmData(confirm, transportCoAddress, Constants.PickupDeliveryConfirmTypes.DestinationDelivery, Constants.FCLEquipmentNeeded.SideLoader, now);
			confirm.EU_JC = container.PK;
			confirm.ConfirmAddress.OrganisationPK = header.PK;

			var xmlShipment = GetExportedValueObject(shipment);
			Assert(xmlShipment.ShipmentDetails.Deliver.DeliveryLegs.IsSpecified);
			AssertEquals(1, xmlShipment.ShipmentDetails.Deliver.DeliveryLegs.Count);
			AssertEquals(confirm.EU_GoodsSignForBy, xmlShipment.ShipmentDetails.Deliver.DeliveryLegs[0].GoodsRecBy);
			AssertEquals(Enterprise.DataTransfer.Xml.XsdVersion1.ContainerLegType.DLV, xmlShipment.ShipmentDetails.Deliver.DeliveryLegs[0].LegType);

			confirm.Delete();
			Factory.Save();

			xmlShipment = GetExportedValueObject(shipment);
			var confirms = container.Confirms.Where(x => x.EU_PickupDeliveryType == Constants.PickupDeliveryConfirmTypes.DestinationDelivery);
			AssertEquals(0, confirms.Count());
		}

		void SetupConfirmData(CommonPickupDeliveryConfirm confirm, OrgAddress transportCoAddress, ZString pickupDeliveryType, ZString dropMode, ZDateTime plannedTime)
		{
			AssertNotNull(confirm);
			AssertNotNull(transportCoAddress);

			confirm.EU_PickupDeliveryType = pickupDeliveryType;
			confirm.EU_OA_TransportProvider = transportCoAddress.PK;
			confirm.EU_DropMode = dropMode;
			confirm.EU_VehicleRegistration = "AAA111";
			confirm.EU_PickupDeliveryInstruction = "pick up delivery instruction";
			confirm.EU_PlannedPickupDeliveryTime = plannedTime;
			confirm.EU_RequestedPickupDeliveryTime = plannedTime.AddDays(1);
			confirm.EU_PickupDeliveryTime = plannedTime.AddDays(2);
			confirm.EU_GoodsSignForBy = "Nobody";
			confirm.EU_Distance = 3m;
			confirm.EU_DistanceUnit = "KM";
		}

		public void TestExportCartageAdvisedDate()
		{
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2005, 4, 13);
			Xsd.Shipment xmlShipment = GetExportedValueObject(shipment);
			AssertEquals(shipment.DocsAndCartage.JP_DeliveryCartageAdvised, xmlShipment.ShipmentDetails.Deliver.CartageAdvised);
		}

		public void TestExportGoodsDeliveredDate()
		{
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2005, 4, 13);
			Xsd.Shipment xmlShipment = GetExportedValueObject(shipment);
			AssertEquals(shipment.DocsAndCartage.JP_DeliveryCartageCompleted, xmlShipment.ShipmentDetails.Deliver.GoodsDelivered);
		}

		public void TestExportDeliveryAgent()
		{
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			deliveryAgent.OH_FullName = "EDI Delivery Agent";
			deliveryAgent.OH_Code = "EDIDLAG";
			shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;
			Xsd.Shipment xmlShipment = GetExportedValueObject(shipment);
			AssertEquals(shipment.DeliveryAgent.OH_Code, xmlShipment.ShipmentDetails.Deliver.DeliveryAgent.EDICode);
			AssertEquals(shipment.DeliveryAgent.OH_FullName, xmlShipment.ShipmentDetails.Deliver.DeliveryAgent.OrganisationDetails.Name);
		}

		public void TestExportPickupCartageCo()
		{
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			var pickupCartageCo = Factory.NewWithValidTestData<OrgHeader>();
			pickupCartageCo.OH_FullName = "EDI Pickup Cartage";
			pickupCartageCo.OH_Code = "EDIDLAG";
			shipment.DocsAndCartage.PickupCartageCoPK = pickupCartageCo.PK;
			Xsd.Shipment xmlShipment = GetExportedValueObject(shipment);
			AssertEquals(shipment.DocsAndCartage.PickupCartageCo.OH_Code, xmlShipment.ShipmentDetails.Pickup.CartageCompany.EDICode);
			AssertEquals(shipment.DocsAndCartage.PickupCartageCo.OH_FullName, xmlShipment.ShipmentDetails.Pickup.CartageCompany.OrganisationDetails.Name);
		}

		public void TestExportDeliveryCartageCo()
		{
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			var deliveryCartageCo = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCartageCo.OH_FullName = "EDI Delivery Cartage";
			deliveryCartageCo.OH_Code = "EDIDLAG";
			shipment.DocsAndCartage.DeliveryCartageCoPK = deliveryCartageCo.PK;
			Xsd.Shipment xmlShipment = GetExportedValueObject(shipment);
			AssertEquals(shipment.DocsAndCartage.DeliveryCartageCo.OH_Code, xmlShipment.ShipmentDetails.Deliver.CartageCompany.EDICode);
			AssertEquals(shipment.DocsAndCartage.DeliveryCartageCo.OH_FullName, xmlShipment.ShipmentDetails.Deliver.CartageCompany.OrganisationDetails.Name);
		}

		Xsd.Shipment GetExportedValueObject(TBusinessObject shipment)
		{
			Xsd.Shipment result = new Xsd.Shipment();
			TConsol consol = Factory.New<TConsol>();

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter(consol);
			NotificationBuffer notify = new NotificationBuffer();
			adapter.ExportToValueObject(shipment, result, new ValueObjectExportContext(notify));

			return result;
		}

		[ExpectNoExceptions]
		public void TestAgentReferenceNotUsedToPopulatedUniqueConsignRef()
		{
			TBusinessObject shipment = Factory.NewWithValidTestData<TBusinessObject>();
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();

			NotificationBuffer notify = new NotificationBuffer();
			Xsd.Shipment shipmentValue = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(notify));

			BusinessObjectFactory factoryForSaving = NewFactory();
			TBusinessObject shipment1 = factoryForSaving.New<TBusinessObject>();
			ValueObjectImportContext context = new ValueObjectImportContext(factoryForSaving, notify);
			adapter.ImportFromValueObject(shipment1, shipmentValue, context);
			factoryForSaving.Save();
			TBusinessObject shipment2 = factoryForSaving.New<TBusinessObject>();
			adapter.ImportFromValueObject(shipment2, shipmentValue, context);
			factoryForSaving.Save();
		}

		public void TestAddImportEvent()
		{
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			Xsd.Shipment xsdShipment = CreateXsdShipmentForImportWithMinimumData();
			Xsd.ShipmentIdentifier identifier = xsdShipment.ShipmentIdentifier.AddNew();
			identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			identifier.Value = "H11111111";
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			TBusinessObject importedShipment = adapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			StmALog[] dataImportEvents = importedShipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertEquals("DIM event should be added to new shipment", 1, dataImportEvents.Length);

			Factory.Save();

			xsdShipment.ShipmentDetails.BookingReference = "S22222222";
			xsdShipment.ShipmentDetailsSpecified = true;
			importedShipment = adapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			dataImportEvents = importedShipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
			AssertAddImportOnSubsequentImport(dataImportEvents);
		}

		protected virtual void AssertAddImportOnSubsequentImport(StmALog[] logs)
		{
			AssertEquals("DIM event should be added to existing shipment", 2, logs.Length);
		}

		public void TestAddExportEvent()
		{
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			TBusinessObject shipment = Factory.NewWithValidTestData<TBusinessObject>();
			StmALog[] dataExportEvents = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("No DEX event should be added to shipment", 0, dataExportEvents.Length);

			adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));
			dataExportEvents = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("DEX event should be added to shipment", 1, dataExportEvents.Length);

			adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));
			dataExportEvents = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code));
			AssertEquals("DEX event should be added to shipment", 2, dataExportEvents.Length);
		}

		public void TestExportTransportPlan()
		{
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			Transport transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = new ZDateTime(2005, 11, 21, 16, 40, 12);
			transport.JW_ATD = new ZDateTime(2005, 11, 21, 16, 41, 4);
			transport.JW_ETA = new ZDateTime(2005, 11, 21, 16, 42, 34);
			transport.JW_ATA = new ZDateTime(2005, 11, 21, 16, 43, 56);

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();

			Xsd.Shipment result = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("1 TransportPlan should be created", 1, result.ShipmentDetails.TransportPlan.Count);
			Xsd.PlannedLeg transportPlan = result.ShipmentDetails.TransportPlan[0];
			AssertEquals("TransportType OnForwarding/ONF", Xsd.PlannedLegTransportType.OnForwarding, transportPlan.TransportType);
			AssertEquals("TransportType Specified", true, transportPlan.TransportTypeSpecified);
			AssertEquals("TransportMode should be Sea", Xsd.TransportMode.SEA, transportPlan.TransportMode);
			AssertEquals("Port of Loading should be AUSYD", "AUSYD", transportPlan.PortOfLoading.Port.Value);
			AssertEquals("Port of Discharge should be USLAX", "USLAX", transportPlan.PortOfDischarge.Port.Value);
			AssertEquals("Estimated time of Departure", new ZDateTime(2005, 11, 21, 16, 40, 00), transportPlan.PortOfLoading.EstimatedDateTime);
			AssertEquals("Actual time of Departure", new ZDateTime(2005, 11, 21, 16, 41, 0), transportPlan.PortOfLoading.ActualDateTime);
			AssertEquals("Estimated time of Arrival", new ZDateTime(2005, 11, 21, 16, 43, 00), transportPlan.PortOfDischarge.EstimatedDateTime);
			AssertEquals("Acutal time of Arrival", new ZDateTime(2005, 11, 21, 16, 44, 00), transportPlan.PortOfDischarge.ActualDateTime);

			transport.JW_TransportType = ZString.Empty;
			result = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("1 TransportPlan should be created", 1, result.ShipmentDetails.TransportPlan.Count);
			transportPlan = result.ShipmentDetails.TransportPlan[0];
			AssertEquals("TransportType Specified", false, transportPlan.TransportTypeSpecified);
		}

		public void TestExportCustomAttributes()
		{
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			shipment.DocsAndCartage.JP_CustomAttrib1 = "CustAttrib1";
			shipment.DocsAndCartage.JP_CustomAttrib2 = "CustAttrib2";
			shipment.DocsAndCartage.JP_CustomDate1 = new ZDateTime(2005, 11, 3);
			shipment.DocsAndCartage.JP_CustomDate2 = new ZDateTime(2005, 11, 30);
			shipment.DocsAndCartage.JP_CustomDecimal1 = 23m;
			shipment.DocsAndCartage.JP_CustomDecimal2 = 86m;
			shipment.DocsAndCartage.JP_CustomFlag1 = true;
			shipment.DocsAndCartage.JP_CustomFlag2 = false;

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();

			Xsd.Shipment result = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Custom Attribute 1", "CustAttrib1", result.ShipmentDetails.Custom.CustomAttribute1);
			AssertEquals("Custom Attribute 2", "CustAttrib2", result.ShipmentDetails.Custom.CustomAttribute2);
			AssertEquals("Custom Date 1", new ZDateTime(2005, 11, 3), result.ShipmentDetails.Custom.Date1);
			AssertEquals("Custom Date 2", new ZDateTime(2005, 11, 30), result.ShipmentDetails.Custom.Date2);
			AssertEquals("Custom Decimal 1", 23m, result.ShipmentDetails.Custom.Decimal1);
			AssertEquals("Custom Decimal 2", 86m, result.ShipmentDetails.Custom.Decimal2);
			AssertEquals("Custom Flag 1", Xsd.TrueFalse.@true, result.ShipmentDetails.Custom.Flag1);
			AssertEquals("Custom Flag 2", Xsd.TrueFalse.@false, result.ShipmentDetails.Custom.Flag2);

			AssertEquals("Custom Decimal 1 Should Be specified", true, result.ShipmentDetails.Custom.Decimal1Specified);
			AssertEquals("Custom Decimal 2 Should Be specified", true, result.ShipmentDetails.Custom.Decimal2Specified);
			AssertEquals("Custom Flat 1 Should be specified", true, result.ShipmentDetails.Custom.Flag1Specified);
			AssertEquals("Custom Flat 2 Should be specified", true, result.ShipmentDetails.Custom.Flag1Specified);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExporteDocs_IncludeeDocsIsFalse()
		{
			TBusinessObject shipment = SetUpStorageMainAndStorageDocs();
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			SystemDataRegistry.Instance.IncludeShipmenteDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Xsd.Shipment result = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Documents should NOT be specified", false, result.Documents.IsSpecified);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExporteDocs_IncludeeDocsIsTrue()
		{
			TBusinessObject shipment = SetUpStorageMainAndStorageDocs();
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			SystemDataRegistry.Instance.IncludeShipmenteDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Xsd.Shipment result = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("Documents count should be 2", 2, result.Documents.Count);
			AssertEquals("DataType", "TIF", result.Documents[0].DataType);
			AssertEquals("Date", new ZDateTime(2005, 10, 11), result.Documents[0].Date);
			AssertEquals("Document Type", "MBL", result.Documents[0].DocumentType);
			AssertEquals("Data", true, result.Documents[0].Data.Length > 1000);
			AssertEquals("Description", "Testing Consol", result.Documents[0].Description);

			AssertEquals("DataType", "PDF", result.Documents[1].DataType);
			AssertEquals("Date", new ZDateTime(2005, 10, 11), result.Documents[1].Date);
			AssertEquals("Document Type", "QUO", result.Documents[1].DocumentType);
			AssertEquals("Data", true, result.Documents[1].Data.Length > 1000);
			AssertEquals("Description", "Testing Consol 2", result.Documents[1].Description);
		}

		[ExpectNoExceptions()]
		public void TestShipmentHasNoStorageMain()
		{
			TBusinessObject shipment = Factory.NewWithValidTestData<TBusinessObject>();
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			SystemDataRegistry.Instance.IncludeShipmenteDocs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Xsd.Shipment result = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Document Counts should be 0", 0, result.Documents.Count);
		}

		public void TestNotificationOnExportForOuterPacksPackTypeNotAlreadyDefinedInRegistry()
		{
			TBusinessObject shipment = Factory.NewWithValidTestData<TBusinessObject>();
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			shipment.JS_OuterPacks = 1;
			shipment.JS_F3_NKPackType = "XXX";
			AssertEquals("Precondition", false, PkgUnitXmlCodeMappings.Instance.ContainsEnterpriseCode("XXX"));
			NotificationBuffer notify = new NotificationBuffer();
			adapter.ExportToValueObject(shipment, new ValueObjectExportContext(notify));
			AssertEquals(true, notify.AsString.Contains(
				$"A non-system defined package type ({shipment.JS_F3_NKPackType}) on shipment {shipment.JS_UniqueConsignRef} has been exported. The organization that imports this XML file may not have that package type in their registry (they can add it in Registry -> Freight -> Shipment -> Packages -> Freight Packs. If they do not have the package type, their import will not fail, but the record will have an error on the package type field when they try to edit it."));

			AssertEquals("Precondition", true, PkgUnitXmlCodeMappings.Instance.Any());
			shipment.JS_F3_NKPackType = PkgUnitXmlCodeMappings.Instance[0].EnterpriseCode;
			shipment.JS_OuterPacks = 1;
			NotificationBuffer notifyWhenCorrectCode = new NotificationBuffer();
			Xsd.Shipment shipmentValueWhenCorrectCode = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(notifyWhenCorrectCode));
			AssertEquals(notifyWhenCorrectCode.AsString, false, notifyWhenCorrectCode.AsString.Contains("A non-system defined package type ("));
			AssertEquals(shipmentValueWhenCorrectCode.ShipmentDetails.TotalOuterPacksQty.DimensionType, PkgUnitXmlCodeMappings.Instance.GetExternalCode(shipment.JS_F3_NKPackType, null, notifyWhenCorrectCode));
		}

		public void TestNotificationOnExportForNonSystemOuterPacksPackTypeEvenAlreadyDefinedInRegistry()
		{
			TBusinessObject shipment = Factory.NewWithValidTestData<TBusinessObject>();
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			shipment.JS_OuterPacks = 1;
			shipment.JS_F3_NKPackType = "XXX";
			AssertEquals("Precondition", false, PkgUnitXmlCodeMappings.Instance.ContainsEnterpriseCode("XXX"));
			NotificationBuffer notify = new NotificationBuffer();
			adapter.ExportToValueObject(shipment, new ValueObjectExportContext(notify));
			AssertEquals(true, notify.AsString.Contains(
				$"A non-system defined package type ({shipment.JS_F3_NKPackType}) on shipment {shipment.JS_UniqueConsignRef} has been exported. The organization that imports this XML file may not have that package type in their registry (they can add it in Registry -> Freight -> Shipment -> Packages -> Freight Packs. If they do not have the package type, their import will not fail, but the record will have an error on the package type field when they try to edit it."));

			AssertEquals("Precondition", true, PkgUnitXmlCodeMappings.Instance.Any());
			shipment.JS_F3_NKPackType = PkgUnitXmlCodeMappings.Instance[0].EnterpriseCode;
			shipment.JS_OuterPacks = 1;
			NotificationBuffer notifyWhenCorrectCode = new NotificationBuffer();
			Xsd.Shipment shipmentValueWhenCorrectCode = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(notifyWhenCorrectCode));
			AssertEquals(notifyWhenCorrectCode.AsString, false, notifyWhenCorrectCode.AsString.Contains("A non-system defined package type ("));
			AssertEquals(shipmentValueWhenCorrectCode.ShipmentDetails.TotalOuterPacksQty.DimensionType, PkgUnitXmlCodeMappings.Instance.GetExternalCode(shipment.JS_F3_NKPackType, null, notifyWhenCorrectCode));
		}

		public void TestExportPickupAndDeliveryInformation()
		{
			Xsd.Shipment shipmentValue = new Xsd.Shipment();

			CommonShipment shipment = CreateNewPopulatedShipment(Core.Constants.TransportModes.Sea);
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_OA_ImportReleaseDepot = AlternateDepot.MainAddress.PK;
			shipment.JS_WarehouseLocation = "lalala";

			NotificationBuffer buffer = new NotificationBuffer();
			//import
			shipmentValue = Adapter.ExportToValueObject(((TBusinessObject)shipment), new ValueObjectExportContext(buffer));
			AssertEquals("deliver -> CFS should be specified", true, shipmentValue.ShipmentDetails.Deliver.CFS.IsSpecified);
			Xsd.CFSAddress cfsAddress = shipmentValue.ShipmentDetails.Deliver.CFS;

			AssertEquals("cfs warehouse location", shipment.JS_WarehouseLocation, cfsAddress.Location);
			AssertEquals("cfs organisation", AlternateDepot.OH_Code, cfsAddress.Address.Organisation.EDICode);
			AssertEquals("cfs address", 1, cfsAddress.Address.AddressSequenceRef);
			AssertEquals("PICKUP -> CFS should NOT specified for import shipment", false, shipmentValue.ShipmentDetails.Pickup.CFS.Address.IsSpecified);

			buffer.Clear();

			shipment.JS_OA_ExportReceivingDepot = AlternateDepot.MainAddress.PK;

			shipmentValue = Adapter.ExportToValueObject(((TBusinessObject)shipment), new ValueObjectExportContext(buffer));
			AssertEquals("deliver -> CFS should be specified", true, shipmentValue.ShipmentDetails.Pickup.CFS.Address.IsSpecified);
			cfsAddress = shipmentValue.ShipmentDetails.Pickup.CFS;

			AssertEquals("cfs warehouse location", shipment.JS_WarehouseLocation, cfsAddress.Location);
			AssertEquals("cfs organisation", AlternateDepot.OH_Code, cfsAddress.Address.Organisation.EDICode);
			AssertEquals("cfs address", 1, cfsAddress.Address.AddressSequenceRef);
		}

		public OrgHeader AlternateDepot
		{
			get
			{
				if (alternateDepot == null)
				{
					alternateDepot = Factory.New<OrgHeader>();
					alternateDepot.OH_Code = "LCLCFS";
					alternateDepot.OH_IsMiscFreightServices = true;
					alternateDepot.OH_IsPackDepot = true;
					alternateDepot.OH_IsUnpackDepot = true;
					alternateDepot.OH_FullName = "Depot";
					alternateDepot.MainAddress.OA_Address1 = "Test Address Line";
					alternateDepot.MainAddress.OA_Address2 = "Test Address Line2";
					alternateDepot.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				}
				return alternateDepot;
			}
		}
		OrgHeader alternateDepot;

		public void TestImportIncoIncoterm()
		{
			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Xsd.Shipment shipmentValue = CreateXsdShipmentForImportWithMinimumData();
			shipmentValue.ShipmentDetailsSpecified = true;
			shipmentValue.ShipmentDetails.Incoterm = "UUU";

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			NotificationBuffer notifications = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notifications);

			CommonShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertEquals("UUU", shipment.JS_INCO);

			OrgPatternMatchOverride orgOverride = Factory.New<OrgPatternMatchOverride>();
			orgOverride.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.IncoTerm;
			orgOverride.OO_ForeignCode = "UUU";
			orgOverride.OO_LocalCode = "PPD";
			orgOverride.OO_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			shipment = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertEquals("PPD", shipment.JS_INCO);

			shipmentValue.ShipmentDetails.Incoterm = "";
			adapter.ImportFromValueObject((TBusinessObject)shipment, shipmentValue, context);
			AssertEquals("PPD", shipment.JS_INCO);

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			adapter.ImportFromValueObject((TBusinessObject)shipment, shipmentValue, context);
			AssertEquals("", shipment.JS_INCO);
		}

		public void TestNotificationOnImportForOuterPacksPackTypeNotDefinedInRegistry()
		{
			Xsd.Shipment shipmentValue = CreateXsdShipmentForImportWithMinimumData();
			shipmentValue.ShipmentDetailsSpecified = true;
			shipmentValue.ShipmentDetails.TotalOuterPacksQty.Value = 10;
			shipmentValue.ShipmentDetails.TotalOuterPacksQty.DimensionType = "ZZZ";
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			NotificationBuffer notifications = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notifications);
			CommonShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			Assert(notifications.AsString.Contains($"Warning: The Package type (ZZZ) of Shipment is not valid. When you edit this record, the package type field will error. To avoid this error you can either use Code Mapping to map this value to the appropriate {Core.Constants.ProductName} package type or you can add this value to the reference files (Reference Files -> Package Types)"));
			AssertEquals("Shipment.PackageType", shipmentValue.ShipmentDetails.TotalOuterPacksQty.DimensionType, shipment.JS_F3_NKPackType);

			CodeDescriptionPairList freightPacksAfterRetrieving = new RefPackTypeCollection(Factory).GetAsCodeDescriptionPair();
			shipmentValue.ShipmentDetails.TotalOuterPacksQty.DimensionType = freightPacksAfterRetrieving[0].Code;
			NotificationBuffer notificationsWhenCorrectCode = new NotificationBuffer();
			context = new ValueObjectImportContext(Factory, notificationsWhenCorrectCode);
			CommonShipment shipmentWhenCorrectCode = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertEquals(notifications.AsString, false, notificationsWhenCorrectCode.AsString.Contains("A non-system defined package type ("));
			AssertEquals("Shipment.PackageType", shipmentValue.ShipmentDetails.TotalOuterPacksQty.DimensionType, shipmentWhenCorrectCode.JS_F3_NKPackType);

			shipmentValue.ShipmentDetails.TotalOuterPacksQty.DimensionType = "ZZZ";

			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
			interchange.InterchangeInfo.EDIOrganisation.OwnerCode = "MAPPINGOWNERCODE";

			OrgHeader otherOrg = Factory.NewWithValidTestData<OrgHeader>();

			OrgPatternMatchOverride matchOwnerCodeToCurrentCompany = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			using (matchOwnerCodeToCurrentCompany.GetValidationSuspender())
			{
				matchOwnerCodeToCurrentCompany.OO_Relationship = "ORG";
				matchOwnerCodeToCurrentCompany.OO_OH = Env.CurrentCompany.OrganisationPK;
				matchOwnerCodeToCurrentCompany.OO_ForeignCode = "MAPPINGOWNERCODE";
				matchOwnerCodeToCurrentCompany.OO_LocalGuid = otherOrg.PK;

				var orgOverride = Factory.New<OrgPatternMatchOverride>();
				orgOverride.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.PackageType;
				orgOverride.OO_ForeignCode = "ZZZ";
				orgOverride.OO_LocalCode = freightPacksAfterRetrieving[0].Code;
				orgOverride.OO_OH = otherOrg.PK;
				Factory.Save();

				context = new ValueObjectImportContext(Factory, interchange, notificationsWhenCorrectCode);
				shipmentWhenCorrectCode = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
				AssertEquals(notifications.AsString, false, notificationsWhenCorrectCode.AsString.Contains("A non-system defined package type ("));
				AssertEquals("Shipment.PackageType", freightPacksAfterRetrieving[0].Code, shipmentWhenCorrectCode.JS_F3_NKPackType);
			}
		}

		#region DocAddress Testing

		#region Export

		public void TestDocAddressExportWithRealOrgHeadersLegacyProvision()
		{
			TBusinessObject shipment = CreateNewShipmentWithEmptyFields();
			AddConsigneeConsignorToShipment(shipment);

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = new ShipmentValueObjectDataAdapter<TBusinessObject>();
			Xsd.Shipment result = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("Consignee", Consignee.OH_FullName, result.ShipmentDetails.Consignee.OrganisationDetails.Name);
			AssertEquals("Consignor", Consignor.OH_FullName, result.ShipmentDetails.Consignor.OrganisationDetails.Name);
			AssertEquals("NotifyParty", Consignee.OH_FullName, result.ShipmentDetails.NotifyParty.Organisation.OrganisationDetails.Name);

			AssertEquals("Consignee Delivery", ConsigneeDLV.OA_Address1, result.ShipmentDetails.Deliver.Address.AddressLine1);
			AssertEquals("Consignee Delivery", ConsignorPIC.OA_Address1, result.ShipmentDetails.Pickup.Address.AddressLine1);
		}

		public void TestDocAddressExportWithRealOrgHeadersLegacyProvisionWith4Orgs()
		{
			TBusinessObject shipment = CreateNewShipmentWithEmptyFields();
			AddConsigneeConsignorToShipment(shipment);

			OrgHeader newDelivery = Factory.New<OrgHeader>();
			newDelivery.MainAddress.OA_Address1 = "new deliver 1";
			newDelivery.OH_FullName = "New Delivery Org";
			newDelivery.OH_Code = "NDORG";
			OrgHeader newPickup = Factory.New<OrgHeader>();
			newPickup.MainAddress.OA_Address1 = "new pickup 1";
			newPickup.MainAddress.OA_Address2 = "new pickup 2";
			newPickup.OH_FullName = "New Pickup Org";
			newPickup.OH_Code = "NPORG";

			shipment.ConsigneeDeliveryAddress.E2_OA_Address = newDelivery.MainAddress.PK;
			shipment.ConsignorPickupAddress.E2_OA_Address = newPickup.MainAddress.PK;

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = new ShipmentValueObjectDataAdapter<TBusinessObject>();
			Xsd.Shipment result = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("Consignee", Consignee.OH_FullName, result.ShipmentDetails.Consignee.OrganisationDetails.Name);
			AssertEquals("Consignor", Consignor.OH_FullName, result.ShipmentDetails.Consignor.OrganisationDetails.Name);
			AssertEquals("NotifyParty", Consignee.OH_FullName, result.ShipmentDetails.NotifyParty.Organisation.OrganisationDetails.Name);

			AssertEquals("Consignee Delivery Co.Name should be in Address1.", newDelivery.OH_FullName, result.ShipmentDetails.Deliver.Address.CompanyName);
			AssertEquals("Consignor Pickup Co.Name should be in Address1.", newPickup.OH_FullName, result.ShipmentDetails.Pickup.Address.CompanyName);
			AssertEquals("Consignee Delivery", newDelivery.MainAddress.OA_Address1, result.ShipmentDetails.Deliver.Address.AddressLine1);
			AssertEquals("Consignor Pickup", newPickup.MainAddress.OA_Address2, result.ShipmentDetails.Pickup.Address.AddressLine2);
		}

		public void TestDocAddressExportWithOverridesLegacyProvision()
		{
			TBusinessObject shipment = CreateNewShipmentWithEmptyFields();
			AddConsigneeConsignorToShipment(shipment);

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_Address1 = Consignee.MainAddress.OA_Address1;
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_Address1 = Consignor.MainAddress.OA_Address1;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipment.ConsignorPickupAddress.E2_AddressOverride = true;
			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = new ShipmentValueObjectDataAdapter<TBusinessObject>();
			Xsd.Shipment result = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("Consignee is Specified.", true, result.ShipmentDetails.Consignee.IsSpecified);
			AssertEquals("Consignor is Specified.", true, result.ShipmentDetails.Consignor.IsSpecified);
			AssertEquals("NotifyParty is Specified.", true, result.ShipmentDetails.NotifyParty.IsSpecified);

			AssertEquals("Consignee", Consignee.OH_FullName, result.ShipmentDetails.Consignee.OrganisationDetails.Name);
			AssertEquals("Consignor", Consignor.OH_FullName, result.ShipmentDetails.Consignor.OrganisationDetails.Name);
			AssertEquals("NotifyParty", Consignee.OH_FullName, result.ShipmentDetails.NotifyParty.Organisation.OrganisationDetails.Name);

			AssertEquals("Consignee is UnmatchedOrg", "UNMATCHED", result.ShipmentDetails.Consignee.EDICode);
			AssertEquals("Consignor is UnmatchedOrg", "UNMATCHED", result.ShipmentDetails.Consignee.EDICode);
			AssertEquals("NotifyParty is UnmatchedOrg", "UNMATCHED", result.ShipmentDetails.NotifyParty.Organisation.EDICode);

			AssertEquals("Consignee Delivery is Specified.", true, result.ShipmentDetails.Deliver.Address.IsSpecified);
			AssertEquals("Consignee Delivery is Specified.", true, result.ShipmentDetails.Pickup.Address.IsSpecified);

			AssertEquals("Consignee Delivery", shipment.ConsigneeDeliveryAddress.E2_CompanyName, result.ShipmentDetails.Deliver.Address.CompanyName);
			AssertEquals("Consignee Delivery", shipment.ConsignorPickupAddress.E2_CompanyName, result.ShipmentDetails.Pickup.Address.CompanyName);

			AssertEquals("Consignee Delivery", shipment.ConsigneeDeliveryAddress.E2_Address1, result.ShipmentDetails.Deliver.Address.AddressLine1);
			AssertEquals("Consignee Delivery", shipment.ConsignorPickupAddress.E2_Address1, result.ShipmentDetails.Pickup.Address.AddressLine1);
		}

		public void TestDocAddressExportWithRealOrgHeaders()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TBusinessObject shipment = CreateNewShipmentWithEmptyFields();
			AddConsigneeConsignorToShipment(shipment);

			var adapter = new ShipmentValueObjectDataAdapter<TBusinessObject>();
			var valueObjectExportContext = new ValueObjectExportContext(new NotificationBuffer());
			var result = adapter.ExportToValueObject(shipment, valueObjectExportContext);

			var xsdDocAddresses = result.ShipmentDetails.DocAddresses.DocAddress;
			var cED = xsdDocAddresses.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CED);
			var cRD = xsdDocAddresses.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CRD);
			var cEG = xsdDocAddresses.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CEG);
			var cRG = xsdDocAddresses.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CRG);
			var nPP = xsdDocAddresses.GetOrCreateAddressByType(Xsd.DocAddressAddressType.NPP);

			AssertNotNull("Consignee Address should be set.", cED);
			AssertNotNull("Consignee Delivery Address should be set.", cEG);
			AssertNotNull("Consignor Address should be set.", cRD);
			AssertNotNull("Consignor Pickup Address should be set.", cRG);
			AssertNotNull("Notify Party Address should be set.", nPP);

			AssertEquals("Consignee", Consignee.OH_FullName, cED.AddressReference.Organisation.OrganisationDetails.Name);
			AssertEquals("Consignor", Consignor.OH_FullName, cRD.AddressReference.Organisation.OrganisationDetails.Name);
			AssertEquals("NotifyParty", Consignee.OH_FullName, nPP.AddressReference.Organisation.OrganisationDetails.Name);

			AssertEquals("Consignee Delivery", ConsigneeDLV.OA_Address1, cEG.AddressReference.Organisation.OrganisationDetails.Addresses[1].AddressLine1);
			AssertEquals("Consignor Pickup", ConsignorPIC.OA_Address1, cRG.AddressReference.Organisation.OrganisationDetails.Addresses[1].AddressLine1);
		}

		public void TestDocAddressExportWithRealOrgHeadersWith4Orgs()
		{
			TBusinessObject shipment = CreateNewShipmentWithEmptyFields();
			AddConsigneeConsignorToShipment(shipment);

			OrgHeader newDelivery = Factory.New<OrgHeader>();
			newDelivery.MainAddress.OA_Address1 = "new deliver 1";
			newDelivery.OH_FullName = "New Delivery Org";
			newDelivery.OH_Code = "NDORG";
			OrgHeader newPickup = Factory.New<OrgHeader>();
			newPickup.MainAddress.OA_Address1 = "new pickup 1";
			newPickup.MainAddress.OA_Address2 = "new pickup 2";
			newPickup.OH_FullName = "New Pickup Org";
			newPickup.OH_Code = "NPORG";

			shipment.ConsigneeDeliveryAddress.E2_OA_Address = newDelivery.MainAddress.PK;
			shipment.ConsignorPickupAddress.E2_OA_Address = newPickup.MainAddress.PK;

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = new ShipmentValueObjectDataAdapter<TBusinessObject>();
			Xsd.Shipment result = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));

			Xsd.DocAddressCollection xsdDocAddresses = result.ShipmentDetails.DocAddresses.DocAddress;

			Xsd.DocAddress cED = xsdDocAddresses.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CED);
			Xsd.DocAddress cRD = xsdDocAddresses.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CRD);
			Xsd.DocAddress cEG = xsdDocAddresses.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CEG);
			Xsd.DocAddress cRG = xsdDocAddresses.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CRG);
			Xsd.DocAddress nPD = xsdDocAddresses.GetOrCreateAddressByType(Xsd.DocAddressAddressType.NPP);

			AssertNotNull("Consignee Address should be set.", cED);
			AssertNotNull("Consignee Delivery Address should be set.", cEG);
			AssertNotNull("Consignor Address should be set.", cRD);
			AssertNotNull("Consignor Pickup Address should be set.", cRG);
			AssertNotNull("Notify Party Address should be set.", nPD);

			AssertEquals("Consignee", Consignee.OH_FullName, cED.AddressReference.Organisation.OrganisationDetails.Name);
			AssertEquals("Consignor", Consignor.OH_FullName, cRD.AddressReference.Organisation.OrganisationDetails.Name);
			AssertEquals("NotifyParty", Consignee.OH_FullName, nPD.AddressReference.Organisation.OrganisationDetails.Name);

			AssertEquals("Consignee Delivery", newDelivery.MainAddress.OA_Address1, cEG.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
			AssertEquals("Consignor Pickup", newPickup.MainAddress.OA_Address1, cRG.AddressReference.Organisation.OrganisationDetails.Addresses[0].AddressLine1);
		}

		public void TestDocAddressExportWithOverrides()
		{
			TBusinessObject shipment = CreateNewShipmentWithEmptyFields();
			AddConsigneeConsignorToShipment(shipment);

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_Address1 = Consignee.MainAddress.OA_Address1;
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_Address1 = Consignor.MainAddress.OA_Address1;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipment.ConsignorPickupAddress.E2_AddressOverride = true;
			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = new ShipmentValueObjectDataAdapter<TBusinessObject>();
			Xsd.Shipment result = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));

			Xsd.DocAddressCollection xsdDocAddresses = result.ShipmentDetails.DocAddresses.DocAddress;

			Xsd.DocAddress cED = xsdDocAddresses.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CED);
			Xsd.DocAddress cRD = xsdDocAddresses.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CRD);
			Xsd.DocAddress cEG = xsdDocAddresses.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CEG);
			Xsd.DocAddress cRG = xsdDocAddresses.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CRG);
			Xsd.DocAddress nPD = xsdDocAddresses.GetOrCreateAddressByType(Xsd.DocAddressAddressType.NPP);

			AssertNotNull("Consignee Address should be set.", cED);
			AssertNotNull("Consignee Delivery Address should be set.", cEG);
			AssertNotNull("Consignor Address should be set.", cRD);
			AssertNotNull("Consignor Pickup Address should be set.", cRG);
			AssertNotNull("Notify Party Address should be set.", nPD);

			AssertEquals("Consignee", Consignee.OH_FullName, cED.CompanyName);
			AssertEquals("Consignor", Consignor.OH_FullName, cRD.CompanyName);
			AssertEquals("NotifyParty", Consignee.OH_FullName, nPD.CompanyName);

			AssertEquals("Consignee", Consignee.MainAddress.OA_Address1, cED.AddressLine1);
			AssertEquals("Consignor", Consignor.MainAddress.OA_Address1, cRD.AddressLine1);
			AssertEquals("Consignee Delivery, If same company name - synch with consignee documentary", Consignee.MainAddress.OA_Address1, cEG.AddressLine1);
			AssertEquals("Consignor Pickup,  If same company name - synch with consignor documentary", Consignor.MainAddress.OA_Address1, cRG.AddressLine1);
		}

		#endregion

		#region Import

		public void TestOrganisationTypeOnImportLegacyProvision()
		{
			Xsd.Shipment shipmentValue = CreateXsdShipmentForImportWithMinimumData();
			shipmentValue.ShipmentDetails.Consignee = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "Consignee", OrganisationTypes.Consignee);
			shipmentValue.ShipmentDetails.Consignor = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "Consignor", OrganisationTypes.Consignor);

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonShipment importedShipment = new ShipmentValueObjectDataAdapter<TBusinessObject>().CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertEquals("Consignee", "Consignee", importedShipment.Consignee.OH_FullName);
			AssertEquals("Consignee is consignee", true, importedShipment.Consignee.OH_IsConsignee);
			AssertEquals("Consignor", "Consignor", importedShipment.Consignor.OH_FullName);
			AssertEquals("Consignor is consignor", true, importedShipment.Consignor.OH_IsConsignor);

			AssertEquals("Consignee", "Consignee", importedShipment.ConsigneeDocumentaryAddress.Organisation.OH_FullName);
			AssertEquals("Consignee", "Consignee", importedShipment.ConsigneeDeliveryAddress.Organisation.OH_FullName);
			AssertEquals("Consignor", "Consignor", importedShipment.ConsignorDocumentaryAddress.Organisation.OH_FullName);
			AssertEquals("Consignor", "Consignor", importedShipment.ConsignorPickupAddress.Organisation.OH_FullName);
		}

		public void TestOrganisationTypeOnImportThroughDocAddressesRealOrgs()
		{
			var shipment = Factory.NewWithValidTestData<TBusinessObject>();
			Factory.Save();
			AddConsigneeConsignorToShipment(shipment);
			Factory.Save();

			Xsd.Shipment shipmentValue = CreateXsdShipmentForImportWithMinimumData();
			Xsd.DocAddress docAddressCED = shipmentValue.ShipmentDetails.DocAddresses.DocAddress.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CED);
			Xsd.DocAddress docAddressCRD = shipmentValue.ShipmentDetails.DocAddresses.DocAddress.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CRD);
			Xsd.DocAddress docAddressCEG = shipmentValue.ShipmentDetails.DocAddresses.DocAddress.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CEG);
			Xsd.DocAddress docAddressCRG = shipmentValue.ShipmentDetails.DocAddresses.DocAddress.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CRG);
			Xsd.DocAddress docAddressNPD = shipmentValue.ShipmentDetails.DocAddresses.DocAddress.GetOrCreateAddressByType(Xsd.DocAddressAddressType.NPP);

			AddressValueObjectHelper helper = new AddressValueObjectHelper("");

			docAddressCED.AddressReference = helper.ToAddressReference(Consignee.MainAddress, new ValueObjectExportContext(new NotificationBuffer()));
			docAddressCRD.AddressReference = helper.ToAddressReference(Consignor.MainAddress, new ValueObjectExportContext(new NotificationBuffer()));

			docAddressCEG.AddressReference = helper.ToAddressReference(Consignee.GetAddressWithFallback(AddressType.DLV), new ValueObjectExportContext(new NotificationBuffer()));
			docAddressCRG.AddressReference = helper.ToAddressReference(Consignor.GetAddressWithFallback(AddressType.PIC), new ValueObjectExportContext(new NotificationBuffer()));

			OrgHeader notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_Code = "Notify";
			notifyParty.OH_FullName = "NotifyParty";
			notifyParty.MainAddress.OA_Address1 = "NP1";

			docAddressNPD.AddressReference = helper.ToAddressReference(notifyParty.MainAddress, new ValueObjectExportContext(new NotificationBuffer()));

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonShipment importedShipment = new ShipmentValueObjectDataAdapter<TBusinessObject>().CreateOrUpdateFromValueObject(shipmentValue, context);

			AssertEquals("Consignee", "Consignee", importedShipment.Consignee.OH_FullName);
			AssertEquals("Consignee is consignee", true, importedShipment.Consignee.OH_IsConsignee);
			AssertEquals("Consignor", "Consignor", importedShipment.Consignor.OH_FullName);
			AssertEquals("Consignor is consignor", true, importedShipment.Consignor.OH_IsConsignor);

			AssertEquals("NotifyParty", "NotifyParty", importedShipment.NotifyPartyDocumentaryAddress.Organisation.OH_FullName);

			AssertEquals("Consignee", "Consignee", importedShipment.ConsigneeDeliveryAddress.Organisation.OH_FullName);
			AssertEquals("Consignor", "Consignor", importedShipment.ConsignorPickupAddress.Organisation.OH_FullName);
		}

		public void TestOrganisationTypeOnImportThroughDocAddressesWithOverrides()
		{
			var shipment = Factory.NewWithValidTestData<TBusinessObject>();
			Factory.Save();
			AddConsigneeConsignorToShipment(shipment);
			Factory.Save();

			Xsd.Shipment shipmentValue = CreateXsdShipmentForImportWithMinimumData();
			Xsd.DocAddress docAddressCED = shipmentValue.ShipmentDetails.DocAddresses.DocAddress.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CED);
			Xsd.DocAddress docAddressCRD = shipmentValue.ShipmentDetails.DocAddresses.DocAddress.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CRD);
			Xsd.DocAddress docAddressCEG = shipmentValue.ShipmentDetails.DocAddresses.DocAddress.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CEG);
			Xsd.DocAddress docAddressCRG = shipmentValue.ShipmentDetails.DocAddresses.DocAddress.GetOrCreateAddressByType(Xsd.DocAddressAddressType.CRG);
			Xsd.DocAddress docAddressNPD = shipmentValue.ShipmentDetails.DocAddresses.DocAddress.GetOrCreateAddressByType(Xsd.DocAddressAddressType.NPP);

			FillDocAddress(docAddressCED, "CED");
			FillDocAddress(docAddressCRD, "CRD");
			FillDocAddress(docAddressCEG, "CEG");
			FillDocAddress(docAddressCRG, "CRG");
			FillDocAddress(docAddressNPD, "NPD");

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonShipment importedShipment = new ShipmentValueObjectDataAdapter<TBusinessObject>().CreateOrUpdateFromValueObject(shipmentValue, context);

			CheckDocAddress(importedShipment.ConsigneeDocumentaryAddress, "CED");
			CheckDocAddress(importedShipment.ConsignorDocumentaryAddress, "CRD");
			CheckDocAddress(importedShipment.ConsigneeDeliveryAddress, "CEG");
			CheckDocAddress(importedShipment.ConsignorPickupAddress, "CRG");
			CheckDocAddress(importedShipment.NotifyPartyDocumentaryAddress, "NPD");
		}

		public void TestOrganisationTypeOnImportThroughDocAddressesWithOverridesLegacyProvision()
		{
			var shipment = Factory.NewWithValidTestData<TBusinessObject>();

			Factory.Save();
			AddConsigneeConsignorToShipment(shipment);
			Factory.Save();

			FillRealDocAddress(shipment.ConsigneeDocumentaryAddress, "CED");
			FillRealDocAddress(shipment.ConsigneeDeliveryAddress, "CEG");
			FillRealDocAddress(shipment.ConsignorDocumentaryAddress, "CRD");
			FillRealDocAddress(shipment.ConsignorPickupAddress, "CRG");
			FillRealDocAddress(shipment.NotifyPartyDocumentaryAddress, "NPD");

			ShipmentValueObjectDataAdapterForDocAddressLegacyTest adapter = new ShipmentValueObjectDataAdapterForDocAddressLegacyTest();
			Xsd.Shipment result = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("Consignee should be unmatched Org.", "UNMATCHED", result.ShipmentDetails.Consignee.EDICode);
			AssertEquals("Consignor should be unmatched Org.", "UNMATCHED", result.ShipmentDetails.Consignor.EDICode);

			AssertEquals("Consignee should be misc Org.", "CED-Company", result.ShipmentDetails.Consignee.OrganisationDetails.Name);
			AssertEquals("Consignor should be misc Org.", "CRD-Company", result.ShipmentDetails.Consignor.OrganisationDetails.Name);

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonShipment importedShipment = new ShipmentValueObjectDataAdapter<TBusinessObject>().CreateOrUpdateFromValueObject(result, context);
			AssertEquals("Consignee is consignee", true, importedShipment.Consignee.OH_IsConsignee);
			AssertEquals("Consignor is consignor", true, importedShipment.Consignor.OH_IsConsignor);
			CheckDocAddress(importedShipment.ConsigneeDocumentaryAddress, "CED");
			CheckDocAddress(importedShipment.ConsignorDocumentaryAddress, "CRD");
			CheckDocAddressFromPickupDeliveryOverride(importedShipment.ConsigneeDeliveryAddress, importedShipment.ConsigneeDocumentaryAddress, "CEG");
			CheckDocAddressFromPickupDeliveryOverride(importedShipment.ConsignorPickupAddress, importedShipment.ConsignorDocumentaryAddress, "CRG");
		}

		public void TestOrganisationTypeOnImportWith2Orgs4AddressesLegacyProvision()
		{
			Xsd.Shipment shipmentValue = CreateXsdShipmentForImportWithMinimumData();
			shipmentValue.ShipmentDetails.Consignee = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "Consignee", OrganisationTypes.Consignee);
			shipmentValue.ShipmentDetails.Consignor = OrganisationValueObjectDataAdapterTest.NewOrgAndDecoyOrgBizoWithTypeAndValue(Factory, "Consignor", OrganisationTypes.Consignor);

			shipmentValue.ShipmentDetails.Deliver.Address.AddressLine1 = "D-Add1";
			shipmentValue.ShipmentDetails.Deliver.Address.AddressLine2 = "D-Add2";
			shipmentValue.ShipmentDetails.Deliver.Address.CityOrSuburb = "D-City";
			shipmentValue.ShipmentDetails.Deliver.Address.StateOrProvince = "D-State";
			shipmentValue.ShipmentDetails.Deliver.Address.PostCode = "D-Post123";

			shipmentValue.ShipmentDetails.Pickup.Address.AddressLine1 = "P-Add1";
			shipmentValue.ShipmentDetails.Pickup.Address.AddressLine2 = "P-Add2";
			shipmentValue.ShipmentDetails.Pickup.Address.CityOrSuburb = "P-City";
			shipmentValue.ShipmentDetails.Pickup.Address.StateOrProvince = "P-State";
			shipmentValue.ShipmentDetails.Pickup.Address.PostCode = "P-Post123";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			CommonShipment importedShipment = new ShipmentValueObjectDataAdapter<TBusinessObject>().CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertEquals("Consignee", "Consignee", importedShipment.Consignee.OH_FullName);
			AssertEquals("Consignee is consignee", true, importedShipment.Consignee.OH_IsConsignee);
			AssertEquals("Consignor", "Consignor", importedShipment.Consignor.OH_FullName);
			AssertEquals("Consignor is consignor", true, importedShipment.Consignor.OH_IsConsignor);

			ZGuid miscOrgPK = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;

			AssertEquals("Consignee.", "Consignee", importedShipment.ConsigneeDocumentaryAddress.Organisation.OH_FullName);
			AssertEquals("Consignee Deliver is MISC because of override.", miscOrgPK, importedShipment.ConsigneeDeliveryAddress.Organisation.PK);
			AssertEquals("Consignor.", "Consignor", importedShipment.ConsignorDocumentaryAddress.Organisation.OH_FullName);
			AssertEquals("Consignor Pickup is MISC because of override.", miscOrgPK, importedShipment.ConsignorPickupAddress.Organisation.PK);

			AssertEquals("Consignee Delivery Address Address1.", "D-Add1", importedShipment.ConsigneeDeliveryAddress.E2_Address1);
			AssertEquals("Consignee Delivery Address Address2.", "D-Add2", importedShipment.ConsigneeDeliveryAddress.E2_Address2);
			AssertEquals("Consignee Delivery Address City.", "D-City", importedShipment.ConsigneeDeliveryAddress.E2_City);
			AssertEquals("Consignee Delivery Address State.", "D-State", importedShipment.ConsigneeDeliveryAddress.E2_State);
			AssertEquals("Consignee Delivery Address State.", "D-Post123", importedShipment.ConsigneeDeliveryAddress.E2_Postcode);

			AssertEquals("Consignor Pickup Address Address1.", "P-Add1", importedShipment.ConsignorPickupAddress.E2_Address1);
			AssertEquals("Consignor Pickup Address Address2.", "P-Add2", importedShipment.ConsignorPickupAddress.E2_Address2);
			AssertEquals("Consignor Pickup Address City.", "P-City", importedShipment.ConsignorPickupAddress.E2_City);
			AssertEquals("Consignor Pickup Address State.", "P-State", importedShipment.ConsignorPickupAddress.E2_State);
			AssertEquals("Consignor Pickup Address State.", "P-Post123", importedShipment.ConsignorPickupAddress.E2_Postcode);
		}

		#endregion

		#endregion

		public void TestExportBranch()
		{
			TBusinessObject exportFromShipment = Factory.NewWithValidTestData<TBusinessObject>();
			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_ParentID = exportFromShipment.PK;

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();

			Xsd.Shipment shipmentValueObject = adapter.ExportToValueObject(exportFromShipment, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, shipmentValueObject.ShipmentIdentifier.FindFirst(Xsd.ShipmentIdentifierType.Branch).Value);
		}

		[ExpectNoExceptions]
		public void TestImportBranch()
		{
			Enterprise.Integration.Accounting.IAccounting accounting = ObjectFactory.Get<Enterprise.Integration.Accounting.IAccounting>();

			var jobBranchDefaultOrderRule = accounting.Registry.JobBranchDefaultOrderRule;

			var originalJobBranchDefaultOrderRuleValue = jobBranchDefaultOrderRule.Value;

			try
			{
				var newJobBranchDefaultOrderRuleValue = Activator.CreateInstance(jobBranchDefaultOrderRule.Value.GetType());

				ReflectionUtil.SetPropertyValue(newJobBranchDefaultOrderRuleValue, "DefaultToBlank", (ZShort)0);
				ReflectionUtil.SetPropertyValue(newJobBranchDefaultOrderRuleValue, "DefaultToBranchRelatedToPortOrWarehouseBranch", (ZShort)1);
				ReflectionUtil.SetPropertyValue(newJobBranchDefaultOrderRuleValue, "DefaultToBranchOfOrganisation", (ZShort)0);
				ReflectionUtil.SetPropertyValue(newJobBranchDefaultOrderRuleValue, "DefaultToLoginUserDefault", (ZShort)0);

				jobBranchDefaultOrderRule.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, newJobBranchDefaultOrderRuleValue);

				Xsd.Shipment shipmentValue = new Xsd.Shipment();

				Xsd.ShipmentIdentifier identifier1 = shipmentValue.ShipmentIdentifier.AddNew();
				identifier1.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
				identifier1.Value = "S000001";

				Xsd.ShipmentIdentifier identifier2 = shipmentValue.ShipmentIdentifier.AddNew();
				identifier2.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Branch;
				identifier2.Value = "XXX";

				shipmentValue.ShipmentDetails.LocalClient = new Xsd.Organisation();
				shipmentValue.ShipmentDetails.LocalClient.EDICode = "SMO";
				shipmentValue.ShipmentDetails.LocalClient.OrganisationDetails = new Xsd.OrganisationDetail();
				shipmentValue.ShipmentDetails.LocalClient.OrganisationDetails.Name = "LOCAL CLIENT";
				shipmentValue.ShipmentDetails.LocalClient.IsSpecified = true;

				shipmentValue.ShipmentDetailsSpecified = true;

				ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
				TBusinessObject shipment = Factory.New<TBusinessObject>();
				ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
				adapter.ImportFromValueObject(shipment, shipmentValue, context);

				Factory.Save();

				JobHeader.Loader loader = new JobHeader.Loader(shipment);
				JobHeader jobHeader = loader.Load();

				AssertEquals(GlbBranch.CurrentBranch.PK, jobHeader.JH_GB);
			}
			finally
			{
				jobBranchDefaultOrderRule.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, originalJobBranchDefaultOrderRuleValue);
			}
		}

		public void TestImportReferenceNumbers()
		{
			CustomsReferenceNumberTypeCollection entryTypeList = new CustomsReferenceNumberTypeCollection();
			entryTypeList.Add("AAA", (NoResString)"AAA Test Entry Type").IsUnique = true;
			entryTypeList.Add("BBB", (NoResString)"BBB Test Entry Type").IsUnique = false;

			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, entryTypeList);

			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			Xsd.ReferenceNumber xsdNumber1 = xmlShipment.ShipmentDetails.ReferenceNumbers.AddNew();
			xsdNumber1.Type = "AAA";
			xsdNumber1.Number = "11111";
			xsdNumber1.Country.Value = "AW";

			Xsd.ReferenceNumber xsdNumber2 = xmlShipment.ShipmentDetails.ReferenceNumbers.AddNew();
			xsdNumber2.Type = "AAA";
			xsdNumber2.Number = "22222";
			xsdNumber2.Country.Value = "AW";

			Xsd.ReferenceNumber xsdNumber3 = xmlShipment.ShipmentDetails.ReferenceNumbers.AddNew();
			xsdNumber3.Type = "AAA";
			xsdNumber3.Number = "33333";
			xsdNumber3.Country.Value = "ZW";

			Xsd.ReferenceNumber xsdNumber4 = xmlShipment.ShipmentDetails.ReferenceNumbers.AddNew();
			xsdNumber4.Type = "BBB";
			xsdNumber4.Number = "11111";
			xsdNumber4.Country.Value = ZString.Empty;

			Xsd.ReferenceNumber xsdNumber5 = xmlShipment.ShipmentDetails.ReferenceNumbers.AddNew();
			xsdNumber5.Type = "BBB";
			xsdNumber5.Number = "22222";
			xsdNumber5.Country.Value = "BR";

			Xsd.ReferenceNumber xsdNumber6 = xmlShipment.ShipmentDetails.ReferenceNumbers.AddNew();
			xsdNumber6.Type = "BBB";
			xsdNumber6.Number = "33333";
			xsdNumber6.Country.Value = "BR";

			TBusinessObject shipment = Factory.New<TBusinessObject>();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertContainsExactElementsInAnyOrder(new[] { "AAA|22222|AW", "AAA|33333|ZW", "BBB|11111|", "BBB|22222|BR", "BBB|33333|BR" },
				shipment.Numbers.Cast<CusEntryNumber>().Select((c) => String.Concat(c.CE_EntryType, "|", c.CE_EntryNum, "|", c.CE_RN_NKCountryCode)).ToArray());
		}

		public void TestImportReferenceNumbers_OverrideDuplicates()
		{
			CustomsReferenceNumberTypeCollection entryTypeList = new CustomsReferenceNumberTypeCollection();
			entryTypeList.Add("AAA", (NoResString)"AAA Test Entry Type").IsUnique = true;
			entryTypeList.Add("BBB", (NoResString)"BBB Test Entry Type").IsUnique = false;

			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, entryTypeList);

			Xsd.Shipment xmlShipment = new Xsd.Shipment();
			Xsd.ReferenceNumber xsdNumber1 = xmlShipment.ShipmentDetails.ReferenceNumbers.AddNew();
			xsdNumber1.Type = "AAA";
			xsdNumber1.Number = "11111";
			xsdNumber1.Country.Value = "AW";

			Xsd.ReferenceNumber xsdNumber2 = xmlShipment.ShipmentDetails.ReferenceNumbers.AddNew();
			xsdNumber2.Type = "BBB";
			xsdNumber2.Number = "11111";
			xsdNumber2.Country.Value = "BO";

			Xsd.ReferenceNumber xsdNumber3 = xmlShipment.ShipmentDetails.ReferenceNumbers.AddNew();
			xsdNumber3.Type = "BBB";
			xsdNumber3.Number = "22222";
			xsdNumber3.Country.Value = "BR";

			Xsd.ReferenceNumber xsdNumber4 = xmlShipment.ShipmentDetails.ReferenceNumbers.AddNew();
			xsdNumber4.Type = "BBB";
			xsdNumber4.Number = "33333";
			xsdNumber4.Country.Value = "BR";

			TBusinessObject shipment = Factory.New<TBusinessObject>();

			CusEntryNumber number1 = shipment.Numbers.AddNew();
			number1.CE_EntryType = "AAA";
			number1.CE_EntryNum = "00000";
			number1.CE_RN_NKCountryCode = "AW";

			CusEntryNumber number2 = shipment.Numbers.AddNew();
			number2.CE_EntryType = "BBB";
			number2.CE_EntryNum = "00000";
			number2.CE_RN_NKCountryCode = ZString.Empty;

			CusEntryNumber number3 = shipment.Numbers.AddNew();
			number3.CE_EntryType = "BBB";
			number3.CE_EntryNum = "11111";
			number3.CE_RN_NKCountryCode = ZString.Empty;

			CusEntryNumber number4 = shipment.Numbers.AddNew();
			number4.CE_EntryType = "BBB";
			number4.CE_EntryNum = "33333";
			number4.CE_RN_NKCountryCode = "BR";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			adapter.ImportFromValueObject(shipment, xmlShipment, context);

			AssertContainsExactElementsInAnyOrder(new[] { "AAA|11111|AW", "BBB|00000|", "BBB|11111|BO", "BBB|11111|", "BBB|22222|BR", "BBB|33333|BR" },
				shipment.Numbers.Cast<CusEntryNumber>().Select((c) => String.Concat(c.CE_EntryType, "|", c.CE_EntryNum, "|", c.CE_RN_NKCountryCode)).ToArray());
		}

		public void TestExportReferenceNumbers()
		{
			CustomsReferenceNumberTypeCollection entryTypeList = new CustomsReferenceNumberTypeCollection();
			entryTypeList.Add("TST", (NoResString)"Test Entry Type");
			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, entryTypeList);

			TBusinessObject shipment = Factory.NewWithValidTestData<TBusinessObject>();
			CusEntryNumber number = shipment.Numbers.AddNew();
			number.CE_EntryType = "TST";
			number.CE_EntryNum = "123456";
			number.CE_RN_NKCountryCode = "ZW";

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();

			Xsd.Shipment shipmentValueObject = adapter.ExportToValueObject(shipment, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals(1, shipmentValueObject.ShipmentDetails.ReferenceNumbers.Count);
			AssertEquals("TST", shipmentValueObject.ShipmentDetails.ReferenceNumbers[0].Type);
			AssertEquals("123456", shipmentValueObject.ShipmentDetails.ReferenceNumbers[0].Number);
			AssertEquals("ZW", shipmentValueObject.ShipmentDetails.ReferenceNumbers[0].Country.Value);
		}

		#region TestExportIncludeCharges

		public void TestExportIncludeCharges()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "HouseBill";
			JobHeader header = Factory.NewJobForTesting<JobHeader>();
			header.JH_ParentID = shipment.PK;
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = header.PK;
			charge.JR_InvoiceType = Enterprise.ZArchitecture.Core.AgencyInvoiceTypesList.Codes.ForeignCollect;

			ShipmentValueObjectDataAdapter<CommonShipment> adapter = new ShipmentValueObjectDataAdapter<CommonShipment>();
			ValueObjectExportContext context = new ValueObjectExportContext(new NotificationBuffer());

			SystemDataRegistry.Instance.IncludeBillingInfoInShipmentXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Xsd.Shipment valueObject = adapter.ExportToValueObject(shipment, context);
			Assert(!valueObject.Billing.IsSpecified);

			SystemDataRegistry.Instance.IncludeBillingInfoInShipmentXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			valueObject = adapter.ExportToValueObject(shipment, context);

			Assert(valueObject.Billing.IsSpecified);
		}

		#endregion

		#region Consignor COD (Amount and Type, both Import and Export)

		public void TestImportConsignorCOD()
		{
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			NotificationBuffer notifications = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notifications);
			Xsd.Shipment shipmentValue = CreateXsdShipmentForImportWithMinimumData();

			CommonShipment shipment = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			Assert("Shipper COD Amount empty", shipment.JS_ShipperCODAmount.IsEmpty);
			Assert("Shipper COD Type empty", shipment.JS_ShipperCODPayMethod.IsEmpty);

			shipmentValue.ShipmentDetails.ConsignorCODAmount = 555.66m;
			shipmentValue.ShipmentDetails.ConsignorCODAmountSpecified = true;
			shipmentValue.ShipmentDetails.ConsignorCODType = "ABC";

			shipment = adapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			Assert("Shipper COD Amount is $555.66", shipment.JS_ShipperCODAmount == 555.66m);
			Assert("Shipper COD Type is ABC", shipment.JS_ShipperCODPayMethod == "ABC");
		}

		public void TestExportConsignorCOD()
		{
			TBusinessObject exportFromShipment = Factory.NewWithValidTestData<TBusinessObject>();
			exportFromShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			exportFromShipment.JS_RL_NKOrigin = "AUBNE";
			exportFromShipment.JS_RL_NKDestination = "AUSYD";
			exportFromShipment.IsDomesticFreight = true;

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			Xsd.Shipment shipmentValueObject = adapter.ExportToValueObject(exportFromShipment, new ValueObjectExportContext(new NotificationBuffer()));

			Assert("Consignor COD Amount not specified", !shipmentValueObject.ShipmentDetails.ConsignorCODAmountSpecified);

			exportFromShipment.JS_ShipperCODAmount = 555.66m;
			exportFromShipment.JS_ShipperCODPayMethod = "ABC";

			shipmentValueObject = adapter.ExportToValueObject(exportFromShipment, new ValueObjectExportContext(new NotificationBuffer()));
			Assert("Consignor COD Amount is specified", shipmentValueObject.ShipmentDetails.ConsignorCODAmountSpecified);
			Assert("Consignor COD Amount is $555.66", shipmentValueObject.ShipmentDetails.ConsignorCODAmount == 555.66m);
			Assert("Consignor COD Type is ABC", shipmentValueObject.ShipmentDetails.ConsignorCODType == "ABC");
		}

		#endregion

		#region Zones - Shipment Details Pickup and Delivery Zones

		public void TestExportZones()
		{
			TBusinessObject exportFromShipment = Factory.NewWithValidTestData<TBusinessObject>();
			exportFromShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			exportFromShipment.JS_RL_NKOrigin = "AUBNE";
			exportFromShipment.JS_RL_NKDestination = "AUSYD";
			exportFromShipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = TransportProvider1.MainAddress.PK;
			exportFromShipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = TransportProvider1.MainAddress.PK;
			exportFromShipment.IsDomesticFreight = false;

			exportFromShipment.ConsignorPickupAddress.E2_Postcode = "4300";
			exportFromShipment.ConsigneeDeliveryAddress.E2_Postcode = "4110";

			// ACI Zone information...
			RefDomesticCartageZone refDomesticCartageZone1 = CreateRefDomesticZone("4300", "AUBNE", "AFR");
			RefDomesticCartageZone refDomesticCartageZone2 = CreateRefDomesticZone("4110", "AUSYD", "EUO");
			RefDomesticCartageZone refDomesticCartageZone3 = CreateRefDomesticZone("4300", "AUSGO", "EUR");
			RefDomesticCartageZone refDomesticCartageZone4 = CreateRefDomesticZone("4110", "AUSHB", "IND");
			RefDomesticCartageZone refDomesticCartageZone5 = CreateRefDomesticZone("4200", "AUSGO", "EUO");
			RefDomesticCartageZone refDomesticCartageZone6 = CreateRefDomesticZone("4220", "AUSHB", "AFR");
			Factory.Save();

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			Xsd.Shipment shipmentValueObject = adapter.ExportToValueObject(exportFromShipment, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("ACI Zone and Cartage Zone should be empty.", false, shipmentValueObject.ShipmentDetails.Deliver.Zones.IsSpecified);
			AssertEquals("ACI Zone and Cartage Zone should be empty.", false, shipmentValueObject.ShipmentDetails.Pickup.Zones.IsSpecified);

			RestShipmentZonesCachedValues(exportFromShipment);
			exportFromShipment.IsDomesticFreight = true;

			shipmentValueObject = adapter.ExportToValueObject(exportFromShipment, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("ACI Zone and Cartage Zone should be empty.", false, shipmentValueObject.ShipmentDetails.Deliver.Zones.IsSpecified);
			AssertEquals("ACI Zone and Cartage Zone should be empty.", false, shipmentValueObject.ShipmentDetails.Pickup.Zones.IsSpecified);

			CreateRateTransportZone();   // Cartage Zone information
			RestShipmentZonesCachedValues(exportFromShipment);

			shipmentValueObject = adapter.ExportToValueObject(exportFromShipment, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("At least one ACI Zone or Cartage Zone should be set.", true, shipmentValueObject.ShipmentDetails.Deliver.Zones.IsSpecified);
			AssertEquals("At least one ACI Zone or Cartage Zone should be set.", true, shipmentValueObject.ShipmentDetails.Pickup.Zones.IsSpecified);

			AssertCartageZones(shipmentValueObject.ShipmentDetails.Deliver.Zones, "Region1", 1);
			AssertCartageZones(shipmentValueObject.ShipmentDetails.Pickup.Zones, "Region2", 1);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
			shipmentValueObject = adapter.ExportToValueObject(exportFromShipment, new ValueObjectExportContext(new NotificationBuffer()));
			RestShipmentZonesCachedValues(exportFromShipment);

			AssertACIZones(shipmentValueObject.ShipmentDetails.Deliver.Zones, "EUO", 2);
			AssertACIZones(shipmentValueObject.ShipmentDetails.Pickup.Zones, "AFR", 2);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			RestShipmentZonesCachedValues(exportFromShipment);

			shipmentValueObject = adapter.ExportToValueObject(exportFromShipment, new ValueObjectExportContext(new NotificationBuffer()));
			AssertACIZones(shipmentValueObject.ShipmentDetails.Deliver.Zones, "EUO", 2);
			AssertACIZones(shipmentValueObject.ShipmentDetails.Pickup.Zones, "AFR", 2);
		}

		void RestShipmentZonesCachedValues(TBusinessObject shipment)
		{
			var consigneePostCode = shipment.ConsigneeDeliveryAddress.E2_Postcode;
			var consignorPostCode = shipment.ConsignorPickupAddress.E2_Postcode;

			shipment.ConsigneeDeliveryAddress.E2_Postcode = "FOO";
			shipment.ConsigneeDeliveryAddress.E2_RN_NKCountryCode = "AU";
			shipment.ConsignorPickupAddress.E2_Postcode = "BAR";
			shipment.ConsignorPickupAddress.E2_RN_NKCountryCode = "AU";

			var reloaded = shipment.JS_Calc_ACIConsigneeDestinationZone;
			reloaded = shipment.JS_Calc_ACIConsignorOriginZone;
			reloaded = shipment.JS_Calc_DeliveryCartageZone;
			reloaded = shipment.JS_Calc_PickupCartageZone;

			shipment.ConsigneeDeliveryAddress.E2_Postcode = consigneePostCode;
			shipment.ConsignorPickupAddress.E2_Postcode = consignorPostCode;
		}

		void AssertCartageZones(Xsd.ZoneCollection zones, string expectedCode, int expectedTotalZones)
		{
			AssertZonesCore(zones, Xsd.ZoneType.CartageZone, expectedCode, expectedTotalZones);
		}

		void AssertACIZones(Xsd.ZoneCollection zones, string expectedCode, int expectedTotalZones)
		{
			AssertZonesCore(zones, Xsd.ZoneType.ACIZone, expectedCode, expectedTotalZones);
		}

		void AssertZonesCore(Xsd.ZoneCollection zones, Xsd.ZoneType expectedType, string expectedCode, int expectedTotalZones)
		{
			AssertEquals(true, zones.IsSpecified);
			AssertEquals(expectedTotalZones, zones.Count);

			int expectedCodeFoundCounter = 0;
			foreach (Xsd.Zone zone in zones)
			{
				if (zone.Code == expectedCode)
				{
					expectedCodeFoundCounter++;
					AssertEquals("Zone IsSpecified", true, zone.IsSpecified);
					AssertEquals("Zone Type", expectedType, zone.Type);
				}
			}
			AssertEquals("Zone Code found", 1, expectedCodeFoundCounter);
		}

		RefDomesticCartageZone CreateRefDomesticZone(ZString postCode, ZString loco, ZString zone)
		{
			var unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, loco);
			var iata = unloco != null ? unloco.RL_IATA : ZString.Empty;

			var refDomesticCartageZone = Factory.New<RefDomesticCartageZone>();
			refDomesticCartageZone.F1_CityTownPostCode = postCode;
			refDomesticCartageZone.F1_RL_NKLoco = loco;
			refDomesticCartageZone.F1_Zone = zone;
			refDomesticCartageZone.F1_PortCode = iata;
			return refDomesticCartageZone;
		}

		void CreateRateTransportZone()
		{
			ObjectFactory.Configure(TestConfiguration.ConfigurationLocation);
			IRateTransportZoneTestHelper zoneTestHelper = ObjectFactory.Get<IRateTransportZoneTestHelper>();
			zoneTestHelper.AddTestZone("Region1", "4110", "4200", "");
			zoneTestHelper.AddTestZone("Region1", "", "", "Neverland");
			zoneTestHelper.AddTestZone("Region2", "4300", "4399", "");
			zoneTestHelper.CreateRateTransportZones(Factory, TransportProvider1);
		}

		OrgHeader TransportProvider1
		{
			get
			{
				if (transportProvider1 == null)
				{
					transportProvider1 = OrgHeader.New(Factory);
					transportProvider1.OH_FullName = "Transport Provider 1";
					transportProvider1.OH_Code = "TRAN1";
					transportProvider1.MainAddress.OA_Address1 = "Some Address";
				}
				return transportProvider1;
			}
		}
		OrgHeader transportProvider1;

		#endregion

		public void TestBlankValue()
		{
			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			ZString aud = Constants.CurrencyCodes.Australia;

			TBusinessObject importToShipment = Factory.New<TBusinessObject>();
			importToShipment.JS_RS_NKServiceLevel = "STD";
			importToShipment.JS_UnitFreightRate = 1000;
			importToShipment.JS_RX_NKFrtRateCurrency = aud;
			importToShipment.JS_GoodsValue = 1000;
			importToShipment.JS_InsuranceValue = 2000;

			Xsd.Shipment shipmentValueObject = new Xsd.Shipment();

			adapter.ImportFromValueObject(importToShipment, shipmentValueObject, context);
			AssertEquals("STD", importToShipment.JS_RS_NKServiceLevel);
			AssertEquals(1000M, importToShipment.JS_UnitFreightRate);
			AssertEquals(aud, importToShipment.JS_RX_NKFrtRateCurrency);
			AssertEquals(1000M, importToShipment.JS_GoodsValue);
			AssertEquals(2000M, importToShipment.JS_InsuranceValue);

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			adapter.ImportFromValueObject(importToShipment, shipmentValueObject, context);
			AssertEquals("", importToShipment.JS_RS_NKServiceLevel);
			AssertEquals(0M, importToShipment.JS_UnitFreightRate);
			AssertEquals(aud, importToShipment.JS_RX_NKFrtRateCurrency);
			AssertEquals(0M, importToShipment.JS_GoodsValue);
			AssertEquals(0M, importToShipment.JS_InsuranceValue);
		}

		public void TestImportCustomValues()
		{
			Xsd.Shipment shipmentValue = new Xsd.Shipment();
			shipmentValue.ShipmentDetails.CustomValues.Add(new Xsd.CustomValue() { Name = "Date1", Type = "DateTime", Value = "2009-12-09T00:00:00" });
			shipmentValue.ShipmentDetails.CustomValues.Add(new Xsd.CustomValue() { Name = "Str1", Type = "String", Value = "test 1" });

			TBusinessObject shipmentBizObj = Factory.New<TBusinessObject>();

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipmentBizObj, shipmentValue, context);

			AssertEquals(new ZDateTime(2009, 12, 9), shipmentBizObj.GetUserDefinedValue<ZDateTime>("Date1"));
			AssertEquals("test 1", shipmentBizObj.GetUserDefinedValue<ZString>("Str1"));
		}

		public void TestExportCustomValues()
		{
			Xsd.Shipment shipment1 = new Xsd.Shipment();

			TBusinessObject shipmentBizObj = Factory.New<TBusinessObject>();
			shipmentBizObj.SetUserDefinedValue("Date1", new ZDateTime(2009, 12, 9));
			shipmentBizObj.SetUserDefinedValue("Str1", new ZString("test 1"));

			ShipmentValueObjectDataAdapter<TBusinessObject> adapter = GetNewShipmentValueObjectDataAdapter();
			adapter.ExportToValueObject(shipmentBizObj, shipment1, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(2, shipment1.ShipmentDetails.CustomValues.Count);
			AssertEquals("Date1", shipment1.ShipmentDetails.CustomValues[0].Name);
			AssertEquals("DateTime", shipment1.ShipmentDetails.CustomValues[0].Type);
			AssertEquals("2009-12-09T00:00:00", shipment1.ShipmentDetails.CustomValues[0].Value);
			AssertEquals("Str1", shipment1.ShipmentDetails.CustomValues[1].Name);
			AssertEquals("String", shipment1.ShipmentDetails.CustomValues[1].Type);
			AssertEquals("test 1", shipment1.ShipmentDetails.CustomValues[1].Value);
		}

		#region DocDataValueObjectDataAdapter

		public void TestExportShipmentWithDocData()
		{
			TBusinessObject shipment = Factory.New<TBusinessObject>();
			DocumentNote docNote = DocumentNote.LoadNote(shipment);
			StmSystemDefinedField field = Factory.New<StmSystemDefinedField>();
			field.S1_Name = "Notify Party";
			StmSystemDefinedFieldWrapper wrapper = new StmSystemDefinedFieldWrapper(field);

			docNote.SystemDefinedFieldWrappers.RemoveAndDeleteAll();
			docNote.SystemDefinedFieldWrappers.Add(wrapper);
			docNote.SetSystemDefinedFieldValue("Notify Party", "Test");

			ValueObjectExportContext expContext = new ValueObjectExportContext(new NotificationBuffer());
			Xsd.Shipment xsdShipment = Adapter.ExportToValueObject(shipment, expContext);

			AssertEquals(1, xsdShipment.DocData.SystemDefinedData.Count);
			AssertEquals("Notify Party", xsdShipment.DocData.SystemDefinedData[0].Name);
			AssertEquals("Test", xsdShipment.DocData.SystemDefinedData[0].Value);
		}

		public void TestImportShipmentWithDocData()
		{
			Xsd.Shipment xsdShipment = new Xsd.Shipment();
			var definedData = xsdShipment.DocData.SystemDefinedData.AddNew();
			definedData.Name = "Notify Party";
			definedData.Value = "Test";

			TBusinessObject shipment = Factory.New<TBusinessObject>();
			DocumentNote docNote = DocumentNote.LoadNote(shipment);
			StmSystemDefinedField field = Factory.New<StmSystemDefinedField>();
			field.S1_Name = "Notify Party";

			StmSystemDefinedFieldWrapper wrapper = new StmSystemDefinedFieldWrapper(field);

			docNote.SystemDefinedFieldWrappers.RemoveAndDeleteAll();
			docNote.SystemDefinedFieldWrappers.Add(wrapper);
			docNote.SetSystemDefinedFieldValue("Notify Party", "Before Test");

			ValueObjectImportContext impContext = new ValueObjectImportContext(Factory, new NotificationBuffer());

			Adapter.ImportFromValueObject(shipment, xsdShipment, impContext);

			AssertEquals(1, docNote.GetSystemDefinedFieldList().Count);
			AssertEquals("Test", docNote.GetSystemDefinedFieldValue("Notify Party"));
		}

		#endregion

		#region Implementation

		protected virtual ShipmentValueObjectDataAdapter<TBusinessObject> GetNewShipmentValueObjectDataAdapter()
		{
			return new ShipmentValueObjectDataAdapter<TBusinessObject>();
		}

		protected virtual ShipmentValueObjectDataAdapter<TBusinessObject> GetNewShipmentValueObjectDataAdapter(TConsol consol)
		{
			return new ShipmentValueObjectDataAdapter<TBusinessObject>(consol);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		TBusinessObject SetUpStorageMainAndStorageDocs()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbIfNotExists(adminConnection, Db.DatabaseName + "_SD001");
			}

			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			IDocumentFactory documentFactory = documentFactoryProvider.GetFactory(Factory);
			var boFactory = documentFactory as BusinessObjectFactory;
			var sDDatabaseFactory = documentFactory.GetFactory(1);

			var shipment = boFactory.NewWithValidTestData<TBusinessObject>();
			BusinessObject storageMain = (BusinessObject)boFactory.New<IStorageMain>();
			BusinessObject storageDocs = (BusinessObject)sDDatabaseFactory.New<IStorageDocs>();
			BusinessObject storageFile = (BusinessObject)sDDatabaseFactory.New<IStorageFile>();

			storageMain[StorageMainSchema.SM_ParentFK.Name] = Shipment.PK;
			storageMain[StorageMainSchema.SM_DB.Name] = 1;
			storageDocs[StorageDocsSchema.SC_SM.Name] = storageMain.PK;
			storageFile[StorageDocsSchema.SC_SM.Name] = storageMain.PK;

			storageDocs[StorageDocsSchema.SC_DataType.Name] = "TIF";
			storageFile[StorageDocsSchema.SC_DataType.Name] = "PDF";
			ZDateTime documentDate = new ZDateTime(2005, 10, 11);
			storageDocs[StorageDocsSchema.SC_Date.Name] = documentDate;
			storageDocs[StorageDocsSchema.SC_DocType.Name] = "MBL";
			storageFile[StorageDocsSchema.SC_Date.Name] = documentDate;
			storageFile[StorageDocsSchema.SC_DocType.Name] = "QUO";
			SmallTifFileAsZblob = File.ReadAllBytes(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "DocumentScanning", "DocumentScanning.Business.Test", "TestDocs", "Small.tif"));
			storageDocs[StorageDocsSchema.SC_ImageData.Name] = SmallTifFileAsZblob;
			storageDocs[StorageDocsSchema.SC_Desc.Name] = "Testing Consol";
			storageDocs[StorageDocsSchema.SC_IsPublished.Name] = true;
			SmallTifFileAsZblob = File.ReadAllBytes(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "DocumentScanning", "DocumentScanning.Business.Test", "TestDocs", "Sample.PDF"));
			storageFile[StorageDocsSchema.SC_ImageData.Name] = SmallTifFileAsZblob;
			storageFile[StorageDocsSchema.SC_Desc.Name] = "Testing Consol 2";
			storageFile[StorageDocsSchema.SC_IsPublished.Name] = true;

			documentFactory.Save();

			return Shipment;
		}

		#region Consignor/Consignee for DocAddress Tests

		void AddConsigneeConsignorToShipment(CommonShipment shipment)
		{
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = Consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = Consignor.PK;
			shipment.NotifyPartyDocumentaryAddress.ContactPK = NotifyPartyContact.PK;
		}

		#region Consignee

		OrgHeader Consignee
		{
			get
			{
				if (consignee == null)
				{
					consignee = Factory.New<OrgHeader>();
					consignee.OH_Code = "ConsEE";
					consignee.OH_FullName = "Consignee";
					consignee.OH_IsConsignee = true;
					consignee.MainAddress.OA_Address1 = "COnsEELine1";
					consigneeDLV = Consignee.Addresses.AddNew();
					consigneeDLV.AddressCapability.SetCapabilityEnabled(nameof(AddressType.DLV));

					consigneeDLV.OA_Address1 = "ConsigneeDLV line 1";
				}

				return consignee;
			}
		}

		OrgAddress ConsigneeDLV
		{
			get
			{
				if (consigneeDLV == null)
				{
					object touchConsignee = Consignee;
				}
				return consigneeDLV;
			}
		}

		OrgContact NotifyPartyContact
		{
			get
			{
				if (notifyPartyContact == null)
				{
					notifyPartyContact = Consignee.Contacts.AddNew();
					notifyPartyContact.OC_ContactName = "Wally";
				}
				return notifyPartyContact;
			}
		}

		OrgHeader consignee;
		OrgAddress consigneeDLV;
		OrgContact notifyPartyContact;

		#endregion

		#region Consignor

		OrgHeader Consignor
		{
			get
			{
				if (consignor == null)
				{
					consignor = Factory.New<OrgHeader>();
					consignor.OH_Code = "ConsCR";
					consignor.OH_FullName = "Consignor";
					consignor.OH_IsConsignor = true;
					consignor.MainAddress.OA_Address1 = "COnsCRLine1";
					consignorPIC = Consignor.Addresses.AddNew();
					consignorPIC.AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));

					consignorPIC.OA_Address1 = "ConsignorPIC line 1";
				}
				return consignor;
			}
		}

		OrgAddress ConsignorPIC
		{
			get
			{
				if (consignorPIC == null)
				{
					object touchConsignor = Consignor;
				}
				return consignorPIC;
			}
		}

		OrgHeader consignor;
		OrgAddress consignorPIC;

		#endregion

		void FillDocAddress(Xsd.DocAddress docAddress, string prefix)
		{
			docAddress.CompanyName = prefix + "-Company";
			docAddress.AddressLine1 = prefix + "-Add1";
			docAddress.AddressLine2 = prefix + "-Add2";
			docAddress.CityOrSuburb = prefix + "-City";
			docAddress.StateOrProvince = prefix + "-State";
			docAddress.PostCode = prefix + "-PCODE";
			docAddress.CountryCode = new ZString(prefix).SubstringSafe(1, 2);
			docAddress.ContactName = prefix + "-Contact";
			Xsd.TelephoneNumber phone = docAddress.TelephoneNumbers.AddNew();
			phone.NumberType = Xsd.TelephoneNumberNumberType.Business;
			phone.Value = prefix + "-123";
			Xsd.TelephoneNumber fax = docAddress.TelephoneNumbers.AddNew();
			fax.NumberType = Xsd.TelephoneNumberNumberType.Fax;
			fax.Value = prefix + "-456";
			docAddress.Email = prefix + "-email@123.com";
		}

		void FillRealDocAddress(JobDocAddress docAddress, string prefix)
		{
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = prefix + "-Company";
			docAddress.E2_Address1 = prefix + "-Add1";
			docAddress.E2_Address2 = prefix + "-Add2";
			docAddress.E2_City = prefix + "-City";
			docAddress.E2_State = prefix + "-State";
			docAddress.E2_Postcode = prefix + "-PCODE";
			docAddress.E2_RN_NKCountryCode = new ZString(prefix).SubstringSafe(1, 2);
			docAddress.E2_Contact = prefix + "-Contact";
			docAddress.E2_Phone = prefix + "-123";
			docAddress.E2_Fax = prefix + "-456";
			docAddress.E2_Email = prefix + "-email@123.com";
		}

		void CheckDocAddress(JobDocAddress docAddress, string prefix)
		{
			AssertEquals(prefix + "-Company", docAddress.E2_CompanyName);
			AssertEquals(prefix + "-Add1", docAddress.E2_Address1);
			AssertEquals(prefix + "-Add2", docAddress.E2_Address2);
			AssertEquals(prefix + "-City", docAddress.E2_City);
			AssertEquals(prefix + "-State", docAddress.E2_State);
			AssertEquals(prefix + "-PCODE", docAddress.E2_Postcode);
			AssertEquals(new ZString(prefix).SubstringSafe(1, 2), docAddress.E2_RN_NKCountryCode);
			AssertEquals(true, prefix + "-Contact" == docAddress.E2_Contact || docAddress.E2_Contact == "");
			AssertEquals(prefix + "-123", docAddress.E2_Phone);
			AssertEquals(prefix + "-456", docAddress.E2_Fax);
			AssertEquals(prefix + "-email@123.com", docAddress.E2_Email);
		}

		void CheckDocAddressFromPickupDeliveryOverride(JobDocAddress docAddress, JobDocAddress documentaryAddress, string prefix)
		{
			AssertEquals(prefix + "-Company", docAddress.E2_CompanyName);
			AssertEquals(prefix + "-Add1", docAddress.E2_Address1);
			AssertEquals(prefix + "-Add2", docAddress.E2_Address2);
			AssertEquals(prefix + "-City", docAddress.E2_City);
			AssertEquals(prefix + "-State", docAddress.E2_State);
			AssertEquals(prefix + "-PCODE", docAddress.E2_Postcode);
			AssertEquals(documentaryAddress.E2_RN_NKCountryCode, docAddress.E2_RN_NKCountryCode);
			AssertEquals("The Transport Manager", docAddress.E2_Contact);
			AssertEquals(documentaryAddress.E2_Phone, docAddress.E2_Phone);
			AssertEquals(documentaryAddress.E2_Fax, docAddress.E2_Fax);
			AssertEquals(documentaryAddress.E2_Email, docAddress.E2_Email);
		}

		#endregion

		void AssertNewShipmentImportSucceeds(string message, Xsd.Shipment shipmentValue)
		{
			var notifyBuffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notifyBuffer);
			var valueAdapter = GetNewShipmentValueObjectDataAdapter();
			var shipmentCollection = new ShipmentCollection(Factory);
			shipmentCollection.Load();
			var numShipments = shipmentCollection.Count;

			valueAdapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertEquals(message + " should not have import errors", false, notifyBuffer.HasErrors);
			shipmentCollection.Load();
			AssertEquals(message + " should have imported single shipment", ++numShipments, shipmentCollection.Count);
		}

		void AssertNewShipmentImportFails(string message, Xsd.Shipment shipmentValue)
		{
			var notifyBuffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notifyBuffer);
			var valueAdapter = GetNewShipmentValueObjectDataAdapter();
			var shipmentCollection = new ShipmentCollection(Factory);
			shipmentCollection.Load();
			var numShipments = shipmentCollection.Count;

			valueAdapter.CreateOrUpdateFromValueObject(shipmentValue, context);
			AssertEquals(message + " should have import errors", true, notifyBuffer.HasErrors);
			shipmentCollection.Load();
			AssertEquals(message + " should not have imported any shipments", numShipments, shipmentCollection.Count);
		}

		Xsd.Shipment CreateXsdShipmentForImportWithMinimumData()
		{
			return CreateXsdShipmentForImport(Xsd.TransportMode.SEA, CreateXsdOrganisation("CONSIGNEE"), CreateXsdOrganisation("CONSIGNOR"));
		}

		Xsd.Shipment CreateXsdShipmentForImport(Xsd.TransportMode? transportMode, Xsd.Organisation consignee, Xsd.Organisation consignor)
		{
			var xsdShipment = new Xsd.Shipment { ShipmentDetailsSpecified = true, ShipmentDetails = new Xsd.ShipmentShipmentDetails() };
			if (transportMode != null)
			{ xsdShipment.ShipmentDetails.TransportMode = (Xsd.TransportMode)transportMode; }
			if (consignee != null)
			{ xsdShipment.ShipmentDetails.Consignee = consignee; }
			if (consignor != null)
			{ xsdShipment.ShipmentDetails.Consignor = consignor; }

			return xsdShipment;
		}

		ZBlob SmallTifFileAsZblob;

		#endregion

		#region Overrides for base test

		protected override void TestExportToAndImportFromAndExportToValueObject(BusinessObjectAndExpectedOutputFileName sample)
		{
			Assert(true);
		}

		protected override TBusinessObject NewBusinessObject()
		{
			return Factory.New<TBusinessObject>();
		}

		protected override ValueObjectDataAdapter<TBusinessObject, Xsd.Shipment> GetNewBizObjXmlDataAdapter()
		{
			TConsol consol = Factory.New<TConsol>();
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "1234";
			return GetNewShipmentValueObjectDataAdapter(Factory.New<TConsol>());
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "Shipments"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "Shipment"; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			TBusinessObject shipment = Factory.NewWithValidTestData<TBusinessObject>(TestBusinessObjectKind.NoData);
			if (shipment.JS_RX_NKGoodsValueCurr != shipment.JS_RX_NKInsuranceCurrency)
			{
				shipment.JS_RX_NKInsuranceCurrency = shipment.JS_RX_NKGoodsValueCurr;
			}
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.DataTransfer.Test.Freight.Testing.EmptyShipmentWithDocData.xml");

			XDocument xmlDoc = XDocument.Load(expectedOutputFilename);
			XNamespace nameSpace = xmlDoc.Root.Name.Namespace;
			XElement dateElement = xmlDoc.Root.Element(nameSpace + "ShipmentDetails")?.Element(nameSpace + "HBLIssueDate");
			if (dateElement != null)
			{
				dateElement.Value = shipment.JS_HouseBillIssueDate.ToString(("yyyy-MM-ddTHH:mm:ss"));
			}
			xmlDoc.Save(expectedOutputFilename);

			return new BusinessObjectAndExpectedOutputFileName(shipment, expectedOutputFilename, ValidationKind.None, "Empty shipment");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile(FullyPopulatedEmptyShipmentSampleEmbeddedResource);
			XDocument xmlDoc = XDocument.Load(expectedOutputFilename);
			XNamespace nameSpace = xmlDoc.Root.Name.Namespace;
			XElement dateElement = xmlDoc.Root.Element(nameSpace + "ShipmentDetails")?.Element(nameSpace + "HBLIssueDate");
			if (dateElement != null)
			{
				dateElement.Value = ZDateTime.Today.ToString("yyyy-MM-ddTHH:mm:ss");
			}
			else
			{
				throw new Exception();
			}
			xmlDoc.Save(expectedOutputFilename);

			return new BusinessObjectAndExpectedOutputFileName(CreateNewShipmentWithEmptyFields(), expectedOutputFilename, ValidationKind.None, "Populated Shipment with empty fields");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile(FullyPopulatedAirSampleEmbeddedResource);
			return new BusinessObjectAndExpectedOutputFileName((TBusinessObject)CreateNewPopulatedShipment(Constants.TransportModes.Air), expectedOutputFilename, ValidationKind.Xsd | ValidationKind.FactorySave, "Populated Air Shipment");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			ArrayList result = new ArrayList();
			var expectedSeaOutputFilename = resourceRetriever.Value.SaveResourceToFile(FullyPopulatedSeaSampleEmbeddedResource);
			result.Add(new BusinessObjectAndExpectedOutputFileName((TBusinessObject)CreateNewPopulatedShipment(Constants.TransportModes.Sea), expectedSeaOutputFilename, ValidationKind.Xsd | ValidationKind.FactorySave, "Populated Sea Shipment"));
			var expectedRailOutputFilename = resourceRetriever.Value.SaveResourceToFile(FullyPopulatedRailSampleEmbeddedResource);
			result.Add(new BusinessObjectAndExpectedOutputFileName((TBusinessObject)CreateNewPopulatedShipment(Constants.TransportModes.Rail), expectedRailOutputFilename, ValidationKind.Xsd | ValidationKind.FactorySave, "Populated Rail Shipment"));
			return (BusinessObjectAndExpectedOutputFileName[])result.ToArray(typeof(BusinessObjectAndExpectedOutputFileName));
		}

		protected virtual string FullyPopulatedAirSampleEmbeddedResource => "Enterprise.Freight.DataTransfer.Test.Freight.Testing.AirShipment.xml";

		protected virtual string FullyPopulatedSeaSampleEmbeddedResource => "Enterprise.Freight.DataTransfer.Test.Freight.Testing.SeaShipment.xml";

		protected virtual string FullyPopulatedRailSampleEmbeddedResource => "Enterprise.Freight.DataTransfer.Test.Freight.Testing.RailShipment.xml";

		protected virtual string FullyPopulatedEmptyShipmentSampleEmbeddedResource => "Enterprise.Freight.DataTransfer.Test.Freight.Testing.PopulatedShipmentWithEmptyFields.xml";

		protected virtual TBusinessObject CreateNewShipmentWithEmptyFields()
		{
			CommonShipment result = (CommonShipment)Factory.NewWithValidTestData(ShipmentType, TestBusinessObjectKind.PopulateAllDependentAndRelatedObjectsDeeply);

			result.Consignee.OH_FullName = "Buyer";
			result.Consignee.MainAddress.OA_Address1 = "Buyer";
			result.Consignor.OH_FullName = "Supplier";
			result.Consignor.MainAddress.OA_Address1 = "Supplier";
			result.ImportBroker.OH_FullName = "ImportBroker";
			result.ImportBroker.MainAddress.OA_Address1 = "ImportBroker";
			result.ExportBroker.OH_FullName = "ExportBroker";
			result.ExportBroker.MainAddress.OA_Address1 = "ExportBroker";
			result.DeliveryAgent.OH_FullName = "DeliveryAgent";
			result.DeliveryAgent.MainAddress.OA_Address1 = "DeliveryAgent";
			result.DocsAndCartage.PickupCartageCo.OH_FullName = "PickupCartageCompany";
			result.DocsAndCartage.PickupCartageCo.MainAddress.OA_Address1 = "PickupCartageCompany";
			result.DocsAndCartage.DeliveryCartageCo.OH_FullName = "DeliveryCartageCompany";
			result.DocsAndCartage.DeliveryCartageCo.MainAddress.OA_Address1 = "DeliveryCartageCompany";

			result.JS_RX_NKFrtRateCurrency = "TOP";
			result.JS_OA_ImportReleaseDepot = Guid.Empty;
			result.JS_OA_ExportReceivingDepot = Guid.Empty;
			Transport transport = result.Transports[0];
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			result.Numbers.RemoveAll();
			return (TBusinessObject)result;
		}

		protected virtual Type ShipmentType
		{
			get { return typeof(TBusinessObject); }
		}

		protected override void OnBeforeImportFromValueObjectForExportImportExportTest(BusinessObject bizObjOriginallyExportedFrom, BusinessObject bizObjToImportTo)
		{
			base.OnBeforeImportFromValueObjectForExportImportExportTest(bizObjOriginallyExportedFrom, bizObjToImportTo);

			JobHeader originalJobHeader = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, bizObjOriginallyExportedFrom.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
			if (originalJobHeader != null && !originalJobHeader.JH_GS_NKRepSales.IsEmpty)
			{
				JobHeader newJobHeader = Factory.NewJobForTesting<JobHeader>();
				newJobHeader.JH_GS_NKRepSales = originalJobHeader.JH_GS_NKRepSales;
				newJobHeader.JH_ParentID = bizObjToImportTo.PK;
			}

			CommonShipment shipment = (CommonShipment)bizObjOriginallyExportedFrom;
			this.shipment.JS_OA_ExportReceivingDepot = Guid.Empty;
			this.shipment.JS_OA_ImportReleaseDepot = Guid.Empty;
		}

		protected virtual CommonShipment CreateNewPopulatedShipment(string transportMode)
		{
			CommonShipment shipment = (TBusinessObject)Factory.New(ShipmentType);
			shipment.Logs.AddNew(Events.Booked, new ZDateTimeOffset(2004, 1, 1));
			shipment.Notes.AddNew(true, "Test Note", "Test Note Text");
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "Very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very very long detailed goods description");

			shipment.JS_HouseBill = "housebill";
			shipment.JS_TransportMode = transportMode;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_BookingReference = "booking_reference";
			shipment.JS_InterimReceipt = "Interim_Receipt";
			shipment.JS_GoodsDescription = "goods description";

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_E_DEP = new ZDateTime(2005, 1, 1);
			shipment.JS_RL_NKDestination = "MYPKG";
			shipment.JS_E_ARV = new ZDateTime(2005, 2, 2);

			shipment.ConsigneePK = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			shipment.Consignee.OH_Code = "BUYBUY";
			shipment.Consignee.OH_FullName = "Buyer";
			shipment.Consignee.MainAddress.OA_Address1 = "Buyer";
			shipment.ConsignorPK = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			shipment.Consignor.OH_Code = "SUPSUP";
			shipment.Consignor.OH_FullName = "Supplier";
			shipment.Consignor.MainAddress.OA_Address1 = "Supplier";
			shipment.JS_OH_ImportBroker = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			shipment.ImportBroker.OH_Code = "IMPBRK";
			shipment.ImportBroker.OH_FullName = "ImportBroker";
			shipment.ImportBroker.MainAddress.OA_Address1 = "ImportBroker";
			shipment.JS_OH_ExportBroker = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			shipment.ExportBroker.OH_Code = "EXPBRK";
			shipment.ExportBroker.OH_FullName = "ExportBroker";
			shipment.ExportBroker.MainAddress.OA_Address1 = "ExportBroker";
			shipment.JS_OH_DeliveryAgent = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			shipment.DeliveryAgent.OH_Code = "DLVAGT";
			shipment.DeliveryAgent.OH_FullName = "DeliveryAgent";
			shipment.DeliveryAgent.MainAddress.OA_Address1 = "DeliveryAgent";
			shipment.DocsAndCartage.PickupCartageCoPK = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			shipment.DocsAndCartage.PickupCartageCo.OH_Code = "PICCAR";
			shipment.DocsAndCartage.PickupCartageCo.OH_FullName = "PickupCartageCompany";
			shipment.DocsAndCartage.PickupCartageCo.MainAddress.OA_Address1 = "PickupCartageCompany";
			shipment.DocsAndCartage.DeliveryCartageCoPK = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			shipment.DocsAndCartage.DeliveryCartageCo.OH_Code = "DLVCAR";
			shipment.DocsAndCartage.DeliveryCartageCo.OH_FullName = "DeliveryCartageCompany";
			shipment.DocsAndCartage.DeliveryCartageCo.MainAddress.OA_Address1 = "DeliveryCartageCompany";

			shipment.JS_TotalPackageCount = 10;
			shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bundle;
			shipment.JS_OuterPacks = 5;
			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Basket;

			shipment.JS_ActualWeight = 25;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_ActualVolume = 50;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			shipment.JS_LoadingMeters = 20.48m;
			shipment.JS_GoodsValue = 2000000;
			shipment.JS_RX_NKGoodsValueCurr = "USD";
			shipment.JS_ActualChargeable = 123;
			shipment.JS_UnitFreightRate = 1000000;
			shipment.JS_RX_NKFrtRateCurrency = "NZD";

			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.BankSightDraft;
			shipment.JS_MarksAndNumbers = "marks and numbers";
			shipment.JS_RS_NKServiceLevel = "D2D";
			shipment.JS_INCO = Core.Constants.IncoTerms.CarriagePaidTo;
			shipment.JS_AdditionalTerms = "additional terms";

			shipment.JS_BookingReference = "booking_reference";
			shipment.JS_HouseBillIssueDate = new ZDateTime(2005, 3, 3);
			shipment.JS_ShippedOnBoardDate = new ZDateTime(2005, 4, 4);
			shipment.JS_A_BKD = new ZDateTime(2009, 6, 1);

			shipment.JS_HBLContainerPackModeOverride = Core.Constants.ContainerModes.LCL;
			shipment.JS_ShippedOnBoard = "LDN";
			shipment.JS_NoOriginalBills = 5;
			shipment.JS_NoCopyBills = 6;

			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 0;
			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 1;

			CusEntryNumber entryNumber = shipment.CusEntryNumbers.AddNew();
			entryNumber.CE_ParentID = shipment.PK;
			entryNumber.CE_EntryNum = "cusentrynum";

			shipment.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2005, 2, 1);
			shipment.DocsAndCartage.JP_DeliveryRequiredBy = new ZDateTime(2005, 3, 1);
			shipment.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2005, 4, 1);
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2005, 5, 1);
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDeliveryAddress.E2_Address1 = "ADDRESS1";
			shipment.ConsigneeDeliveryAddress.E2_Address2 = "ADDRESS2";
			shipment.ConsigneeDeliveryAddress.E2_City = "CITY";
			shipment.ConsigneeDeliveryAddress.E2_State = "NSW";
			shipment.ConsigneeDeliveryAddress.E2_Postcode = "PCODE";
			shipment.ConsigneeDeliveryAddress.E2_RN_NKCountryCode = "AU";

			shipment.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2005, 2, 1);
			shipment.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2005, 3, 1);
			shipment.DocsAndCartage.JP_PickupCartageAdvised = new ZDateTime(2005, 4, 1);
			shipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2005, 5, 1);
			shipment.ConsignorPickupAddress.E2_AddressOverride = true;
			shipment.ConsignorPickupAddress.E2_Address1 = "ADDRESS1";
			shipment.ConsignorPickupAddress.E2_Address2 = "ADDRESS2";
			shipment.ConsignorPickupAddress.E2_City = "CITY";
			shipment.ConsignorPickupAddress.E2_State = "NSW";
			shipment.ConsignorPickupAddress.E2_Postcode = "PCODE";
			shipment.ConsignorPickupAddress.E2_RN_NKCountryCode = "AU";

			shipment.DocsAndCartage.JP_CustomAttrib1 = "CustAttrib1";
			shipment.DocsAndCartage.JP_CustomAttrib2 = "CustAttrib2";
			shipment.DocsAndCartage.JP_CustomDate1 = new ZDateTime(2005, 11, 30);
			shipment.DocsAndCartage.JP_CustomDate2 = new ZDateTime(2005, 12, 25);
			shipment.DocsAndCartage.JP_CustomDecimal1 = 98.6m;
			shipment.DocsAndCartage.JP_CustomDecimal2 = 579.21m;
			shipment.DocsAndCartage.JP_CustomFlag1 = true;
			shipment.DocsAndCartage.JP_CustomFlag2 = false;
			shipment.JS_A_RCV = new ZDateTime(2005, 6, 1);

			Transport transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = transportMode;
			transport1.JW_RL_NKLoadPort = "USLAX";
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_ETA = new ZDateTime(2005, 11, 21, 17, 55, 21);
			transport1.JW_ETD = new ZDateTime(2005, 11, 18, 17, 55, 12);
			transport1.JW_ATA = new ZDateTime(2005, 11, 21, 18, 34, 21);
			transport1.JW_ATD = new ZDateTime(2005, 11, 18, 18, 21, 54);
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;

			Transport transport2 = shipment.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Storage;
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_ETA = new ZDateTime(2005, 11, 21, 18, 30, 00);
			transport2.JW_ETD = new ZDateTime(2005, 11, 25, 5, 45, 00);
			transport2.JW_ATA = new ZDateTime(2005, 11, 21, 20, 15, 00);
			transport2.JW_ATD = new ZDateTime(2005, 11, 25, 6, 00, 00);

			GlbStaff salesRep = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "JA");
			if (salesRep == null)
			{
				salesRep = Factory.New<GlbStaff>();
				salesRep.GS_Code = "JA";
				salesRep.GS_FullName = "JESSICA ALLEN";
				salesRep.GS_IsSalesRep = true;
			}

			JobHeader jobHeader = new JobHeader.Loader(shipment).TryLoadOrCreate();
			jobHeader.JH_GS_NKRepSales = salesRep.GS_Code;
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_OA_LocalChargesAddr = shipment.Consignor.MainAddress.PK;

			CusEntryNumber referenceNumber = shipment.Numbers.AddNew();
			referenceNumber.CE_EntryType = "COC";
			referenceNumber.CE_EntryNum = "1111111";

			AddCharge(jobHeader, "FRT", Enterprise.ZArchitecture.Core.AgencyInvoiceTypesList.Codes.ForeignCollect, "USD", 1500, "USD", 1000, shipment.Consignor);

			return shipment;
		}

		void AddCharge(JobHeader header, string chargeCode, string invoiceType, string sellCurrencyCode, decimal sellAmount, string costCurrencyCode, decimal costAmount, OrgHeader debtor)
		{
			ZQuery chargeCodeFilter = new ZQuery();
			chargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
			chargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_GC, header.JH_GC);

			IBusinessObjectCollection charges = (IBusinessObjectCollection)header["Charges"];

			JobCharge charge = (JobCharge)charges.AddNew();
			charge.JR_JH = header.PK;
			charge.JR_AC = header.Factory.LoadTop1<AccChargeCode>(chargeCodeFilter).PK;
			charge.JR_RX_NKSellCurrency = sellCurrencyCode;
			charge.JR_OSSellAmt = sellAmount;
			charge.JR_RX_NKCostCurrency = costCurrencyCode;
			charge.JR_OSCostAmt = costAmount;
			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_InvoiceType = invoiceType;
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					// covered by other xml data adapters
					"ShipmentDetails/Deliver/Address/AddressCapabilities/IsMainAddress",
					"ShipmentDetails/Deliver/Address/AddressCapabilities/AddressType",
					"ShipmentDetails/Deliver/Address/AddressCode",
					"ShipmentDetails/Deliver/DeliveryLegs/GoodsRecBy",
					"ShipmentDetails/Pickup/Address/AddressCapabilities/IsMainAddress",
					"ShipmentDetails/Pickup/Address/AddressCapabilities/AddressType",
					"ShipmentDetails/Pickup/Address/AddressCode",
					"Events",
					"ShipmentDetails/PortOfOrigin/ActualDateTime",
					"ShipmentDetails/PortofDestination/ActualDateTime",
					"ShipmentDetails/Consignee",
					"ShipmentDetails/Consignor",
					"ShipmentDetails/NotifyParty/Organisation",
					"ShipmentDetails/ImportBroker",
					"ShipmentDetails/ExportBroker",
					"ShipmentDetails/ExportBroker",
					"Orders",
					"ShipmentDetails/Packages",
					"ShipmentDetails/DateCreated",
					"Notes",
					"ShipmentDetails/Deliver/DeliveryAgent",
					"ShipmentIdentifier/Masterbill",
					"Documents",
					"ShipmentDetails/DocAddresses",
					"ShipmentDetails/InnerPackages",
					"ShipmentDetails/LocalClient",
					"ShipmentDetails/TEU",
					"ARInvoices",
					"ShipmentDetails/Deliver/CartageCompany",
					"ShipmentDetails/Pickup/CartageCompany",
					"ShipmentDetails/InsuranceValue/CurrencyCode",
					"ShipmentDetails/Deliver/Address/Language",
					"ShipmentDetails/Pickup/Address/Language",
					"CustomValues",
					"ShipmentDetails/CustomValues",
					"ShipmentDetails/TransportPlan/Item/LoadPortETA",
					"ShipmentDetails/TransportPlan/Item/LoadPortATA",
					"ShipmentDetails/TransportPlan/LegOrderNumber",

					// not used for this data adapter
					"Invoices",
					"Declaration",
					"ShipmentDetails/OwnerReference",
					"ShipmentDetails/ShipmentStatus", // dunno wtf this is meant to be
					"ShipmentDetails/CustomsEntries",
					"ShipmentDetails/ShipmentType",
					"ShipmentDetails/TransportPlan/Item/IsTranshipment",
					"ShipmentDetails/TransportPlan/Item/IsPublished",
					"ShipmentDetails/TransportPlan/Item/ArrivalBerth",
					"ShipmentDetails/TransportPlan/Item/DepartureCTO",
					"ShipmentDetails/TransportPlan/Item/ArrivalCTO",
					"ShipmentDetails/TransportPlan/Item/DepartureBerth",
					"ShipmentDetails/TransportPlan/Item/DocCutOffDate",
					"Billing/ChargeLines/Debtor",
					"Billing/ChargeLines/Creditor",
					"Billing/ChargeLines/Collect",
					"Billing/LocalClient",
					"Billing/ChargeLines/LocalSellAmount/CurrencyCode",
					"Billing/ChargeLines/LocalCostAmount/CurrencyCode",
					"Billing/ChargeLines/SellExchangeRate",
					"Billing/ChargeLines/CostExchangeRate",
					"Billing/ChargeLines/InvoiceNumber",
					"Billing/ChargeLines/InvoiceDate",
					"Billing/ChargeLines/SellRatingOverride",
					"Billing/ChargeLines/RevenueIsPosted",
					"Billing/ChargeLines/CostIsPosted",
					"Billing/ChargeLines/ARInvoiceNumber",
					"Billing/ChargeLines/SupplierReference",

					// todo
					"ShipmentDetails/CustomsEntryNumbers/Type",
					"ShipmentDetails/AgentReference",
					"ShipmentDetails/Volume/Description",
					"ShipmentDetails/TotalInnerPacksQty/Description",
					"ShipmentDetails/TotalOuterPacksQty/Description",
					"ShipmentDetails/Weight/Description",
					"ShipmentDetails/ChargeableWeight/Description",
					"ShipmentDetails/DeclarationStyle",
					"ShipmentDetails/Deliver/Zones",
					"ShipmentDetails/Pickup/Zones",
					"ShipmentDetails/ConsignorCODAmount",
					"ShipmentDetails/ConsignorCODType",

					//not used in delivery and pickup address
					"ShipmentDetails/Deliver/Address/Location/Value",
					"ShipmentDetails/Deliver/Address/Location/City",
					"ShipmentDetails/Deliver/Address/Location/Country",
					"ShipmentDetails/Deliver/Address/AddressType",
					"ShipmentDetails/Deliver/Address/Email",
					"ShipmentDetails/Deliver/Address/TelephoneNumbers/Value",
					"ShipmentDetails/Deliver/Address/CompanyName",
					"ShipmentDetails/Deliver/Address/Sequence",
					"ShipmentDetails/Deliver/Address/IsDefault",
					"ShipmentDetails/Pickup/Address/Location/Value",
					"ShipmentDetails/Pickup/Address/Location/City",
					"ShipmentDetails/Pickup/Address/Location/Country",
					"ShipmentDetails/Pickup/Address/AddressType",
					"ShipmentDetails/Pickup/Address/Email",
					"ShipmentDetails/Pickup/Address/TelephoneNumbers/Value",
					"ShipmentDetails/Pickup/Address/CompanyName",
					"ShipmentDetails/Pickup/Address/Sequence",
					"ShipmentDetails/Pickup/Address/IsDefault",

					//Cover by other DataAdapter or has a separate test to cover
					"ShipmentDetails/Pickup/CFS/Address",
					"ShipmentDetails/Pickup/CFS/Location",
					"ShipmentDetails/Deliver/CFS/Address",
					"ShipmentDetails/Deliver/CFS/Location",
					"ShipmentDetails/ForwardingShipmentType",

					// these fields are deprecated and not used any longer - routings used instead
					"ShipmentDetails/OnForwardTo",
					"ShipmentDetails/OnForwardToETA",

					"ShipmentDetails/ExporterStatement",
					"DocData/SystemDefinedData/Category",
					"DocData/SystemDefinedData/Name",
					"DocData/SystemDefinedData/Value",
					"DocData/UserDefinedData/Name",
					"DocData/UserDefinedData/Value",
					"AWBHeaders"
				};
			}
		}

		#endregion

		TConsol Consol
		{
			get { return consol ?? (consol = Factory.New<TConsol>()); }
		}
		TConsol consol;

		TBusinessObject Shipment
		{
			get { return shipment ?? (shipment = Factory.New<TBusinessObject>()); }
		}
		TBusinessObject shipment;

		ShipmentValueObjectDataAdapter<TBusinessObject> Adapter
		{
			get { return adapter ?? (adapter = GetNewShipmentValueObjectDataAdapter(Consol)); }
		}
		ShipmentValueObjectDataAdapter<TBusinessObject> adapter;

		NotificationBuffer Notify
		{
			get { return notify ?? (notify = new NotificationBuffer()); }
		}
		NotificationBuffer notify;

		protected override void SetUp()
		{
			base.SetUp();
			SystemDataRegistry.Instance.AllowExportBrokerImport.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.IncludeBillingInfoInShipmentXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.AllowBillingImportIntoShipment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		class ShipmentValueObjectDataAdapterForDocAddressLegacyTest : ShipmentValueObjectDataAdapter<TBusinessObject>
		{
			protected override void ExportDocAddresses(TBusinessObject shipment, Xsd.Shipment shipmentValue, IValueObjectExportContext notify)
			{
			}
		}
	}
}
