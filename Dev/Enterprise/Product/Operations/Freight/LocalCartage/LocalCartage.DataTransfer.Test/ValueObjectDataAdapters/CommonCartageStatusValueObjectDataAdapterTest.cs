using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Testing
{
	[TestedType(typeof(CommonCartageStatusValueObjectDataAdapter))]
	sealed class CommonCartageStatusValueObjectDataAdapterTest : CommonCartageValueObjectDataAdapterTest
	{
		protected override ValueObjectDataAdapter<CommonCartage, Xsd.CartageJob> GetNewBizObjXmlDataAdapter()
		{
			return new CommonCartageStatusValueObjectDataAdapter();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var populatedCartage = GetSampleJobCartage(Constants.TransportModes.Sea);
			var populatedJobCartageStatusPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.LocalCartage.DataTransfer.Testing.TestFiles.PopulatedJobCartageStatus.xml");
			return new BusinessObjectAndExpectedOutputFileName(populatedCartage, populatedJobCartageStatusPath, ValidationKind.Xsd, "Populated Port Transport Job");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var emptyCartage = Factory.New<CommonCartage>();
			var emptyCartageJobStatusPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.LocalCartage.DataTransfer.Testing.TestFiles.EmptyCartageJobStatus.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyCartage, emptyCartageJobStatusPath, ValidationKind.None, "Empty Port Transport Job");
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				List<string> result = new List<string>(base.XmlNodesToExcludeFromCoverageTest);
				result.AddRange(new string[] { "TransportReference", "InvoiceNumber", "ServiceLevel", "InsuranceAmount", "TransportBillTo", "TransportBillToAddress/AddressReference/Organisation/OrganisationDetails/Addresses/Language", "TransportBillToAddress/Language", "TransportBillToAddress/RegistrationNumber", "BillToAddress/AddressReference/Organisation/OrganisationDetails/Addresses/Language", "BillToAddress/Language", "BillToAddress/RegistrationNumber", "CarrierAddress/AddressReference/Organisation/OrganisationDetails/Addresses/Language", "CarrierAddress/Language", "CarrierAddress/RegistrationNumber", "TransportBillTo/EDICode", "TransportBillTo/OwnerCode", "FreightCharges/ChargeCode", "FreightCharges/Description", "FreightCharges/RateChargeUnits", "FreightCharges/TotalAmount/CurrencyCode", "CartageLegs/MostDangerousGoodsStandard" });
				return result.ToArray();
			}
		}

		public new void TestImportFromValueObjectNotSupportedException()
		{
			Assert(true);
		}

		public override void TestTargetType()
		{
			CommonCartageStatusValueObjectDataAdapter adapter = new CommonCartageStatusValueObjectDataAdapter();
			AssertEquals("TargetType", Xsd.InterchangeInfoTargetType.LocalCartageStatus, adapter.TargetType);
		}

		public void TestImportUpdatesCorrectDateOnShipment()
		{
			CurrentDate = ZDateTime.Today;
			var zeroDurationDateTime = new ZDateTime(ZDateTime.Today.Year, 1, 1);
			var exportShipment = GetNewShipment(Core.Constants.TransportCodes.Air, true, "S00001000");
			var importShipment = GetNewShipment(Core.Constants.TransportCodes.Air, false, "S0001001");
			Factory.Save();
			AssertEquals("Is Pickup", true, exportShipment.IsExport());
			AssertEquals("JP_PickupCartageCompleted", true, exportShipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty);
			AssertEquals("JP_DeliveryCartageCompleted", true, exportShipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);
			AssertEquals("Is Pickup", false, importShipment.IsExport());
			AssertEquals("JP_PickupCartageCompleted", true, importShipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty);
			AssertEquals("JP_DeliveryCartageCompleted", true, importShipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);
			var exportAdapter = new CommonCartageStatusValueObjectDataAdapter();
			var exportContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var exportXsdCartage = GetXsdCartage(exportShipment.JS_UniqueConsignRef, true, false);
			exportAdapter.CreateOrUpdateFromValueObject(exportXsdCartage, exportContext);
			AssertEquals(CurrentDate.AddHours(2).AddMinutes(2), exportShipment.DocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals(true, exportShipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);
			var importAdapter = new CommonCartageStatusValueObjectDataAdapter();
			var importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var importXsdCartage = GetXsdCartage(importShipment.JS_UniqueConsignRef, false, false);
			importAdapter.CreateOrUpdateFromValueObject(importXsdCartage, importContext);
			AssertEquals(true, importShipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty);
			AssertEquals(CurrentDate.AddHours(6).AddMinutes(4), importShipment.DocsAndCartage.JP_DeliveryCartageCompleted);
		}

		public void TestImportParentInformationFromValueObjectOnly_Shipment_Export()
		{
			CurrentDate = ZDateTime.Today;
			var zeroDurationDateTime = ZDateTime.DefaultDurationEpoch;
			var shipment = GetNewShipment(Core.Constants.TransportCodes.Sea, true);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			Factory.Save();
			AssertEquals("PickupCartageCompleted", true, shipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty);
			AssertEquals("JP_PickupTruckWaitTime", true, shipment.DocsAndCartage.JP_PickupTruckWaitTime.IsEmpty);
			var adapter = new CommonCartageStatusValueObjectDataAdapter();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var xsdCartage = GetXsdCartage(shipment.JS_UniqueConsignRef, true, true);
			adapter.CreateOrUpdateFromValueObject(xsdCartage, context);
			Factory.Save(); // confirmations set Shipment Completed
			var jobContainer = shipment.Containers.First();
			AssertEquals("T00001000", jobContainer.JC_DepartureCartageRef);
			AssertEquals("", jobContainer.JC_ArrivalCartageRef);
			AssertEquals("DepSlotRef", jobContainer.JC_DepartureSlotReference);
			AssertEquals("", jobContainer.JC_ArrivalSlotReference);
			AssertEquals(CurrentDate.AddDays(2), jobContainer.JC_DepartureSlotDateTime);
			AssertEquals(ZDateTime.Empty, jobContainer.JC_ArrivalSlotDateTime);
			AssertEquals(ZeroDurationDateTime.AddHours(6), jobContainer.DepartureTruckWaitTime);
			AssertEquals(ZDateTime.Empty, jobContainer.ArrivalTruckWaitTime);
			AssertEquals(CurrentDate.AddHours(6).AddMinutes(4), jobContainer.JC_FCLWharfGateIn);
			AssertEquals(ZDateTime.Empty, jobContainer.JC_FCLWharfGateOut);
			AssertEquals(ContainerYard.PK, jobContainer.JC_OA_DepartureContainerYardAddress_ZAddress.OrgPK);
			AssertEquals(ZGuid.Empty, jobContainer.JC_OA_ArrivalContainerYardAddress_ZAddress.OrgPK);
			AssertEquals(CurrentDate.AddHours(6).AddMinutes(3), jobContainer.JC_ContainerYardEmptyPickupGateOut);
			AssertEquals(ZDateTime.Empty, jobContainer.JC_ContainerYardEmptyReturnGateIn);
			AssertEquals(CurrentDate.AddHours(4).AddMinutes(2), shipment.DocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryCartageCompleted);
			AssertEquals(zeroDurationDateTime.AddHours(6), shipment.DocsAndCartage.JP_PickupTruckWaitTime);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryTruckWaitTime);
			AssertHasStatusUpdateLog(shipment.Logs, "WKD-Work Completed");
			AssertHasStatusUpdateNote(shipment.Notes);
		}

		public void TestImportParentInformationFromValueObjectOnly_Shipment_Import()
		{
			CurrentDate = ZDateTime.Today;
			var zeroDurationDateTime = ZDateTime.DefaultDurationEpoch;
			var shipment = GetNewShipment(Core.Constants.TransportCodes.Sea, false);
			shipment.Consols[0].JK_OA_ContainerYardEmptyPickupAddress = ZGuid.Empty;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			Factory.Save();
			AssertEquals("PickupCartageCompleted", true, shipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty);
			AssertEquals("JP_PickupTruckWaitTime", true, shipment.DocsAndCartage.JP_PickupTruckWaitTime.IsEmpty);
			var adapter = new CommonCartageStatusValueObjectDataAdapter();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var xsdCartage = GetXsdCartage(shipment.JS_UniqueConsignRef, false, true);
			adapter.CreateOrUpdateFromValueObject(xsdCartage, context);
			Factory.Save(); // confirmations set Shipment Completed
			var jobContainer = shipment.Containers.First();
			AssertEquals("", jobContainer.JC_DepartureCartageRef);
			AssertEquals("T00001000", jobContainer.JC_ArrivalCartageRef);
			AssertEquals("", jobContainer.JC_DepartureSlotReference);
			AssertEquals(ZDateTime.Empty, jobContainer.JC_DepartureSlotDateTime);
			AssertEquals(CurrentDate.AddDays(1), jobContainer.JC_ArrivalSlotDateTime);
			AssertEquals("ArrivalSlotRef", jobContainer.JC_ArrivalSlotReference);
			AssertEquals(ZDateTime.Empty, jobContainer.DepartureTruckWaitTime);
			AssertEquals(ZeroDurationDateTime.AddHours(6), jobContainer.ArrivalTruckWaitTime);
			AssertEquals(ZDateTime.Empty, jobContainer.JC_FCLWharfGateIn);
			AssertEquals(CurrentDate.AddHours(3).AddMinutes(3), jobContainer.JC_FCLWharfGateOut);
			AssertEquals(ZGuid.Empty, jobContainer.JC_OA_DepartureContainerYardAddress_ZAddress.OrgPK);
			AssertEquals(ContainerYard.PK, jobContainer.JC_OA_ArrivalContainerYardAddress_ZAddress.OrgPK);
			AssertEquals(ZDateTime.Empty, jobContainer.JC_ContainerYardEmptyPickupGateOut);
			AssertEquals(CurrentDate.AddHours(5).AddMinutes(4), jobContainer.JC_ContainerYardEmptyReturnGateIn);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals(CurrentDate.AddHours(4).AddMinutes(4), shipment.DocsAndCartage.JP_DeliveryCartageCompleted);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_PickupTruckWaitTime);
			AssertEquals(zeroDurationDateTime.AddHours(6), shipment.DocsAndCartage.JP_DeliveryTruckWaitTime);
			AssertHasStatusUpdateLog(shipment.Logs, "WKD-Work Completed");
			AssertHasStatusUpdateNote(shipment.Notes);
		}

		public void TestImportParentInformationFromValueObjectOnly_Shipment_Export_MultiContainersNotAllDelivered()
		{
			CurrentDate = ZDateTime.Today;
			var zeroDurationDateTime = new ZDateTime(ZDateTime.Today.Year, 1, 1);
			var shipment = GetNewShipment(Core.Constants.TransportCodes.Sea, true);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			var consol = shipment.Consols[0];
			consol.JK_OA_ContainerYardEmptyPickupAddress = ZGuid.Empty;
			var extraContainer = consol.Containers.AddNew();
			extraContainer.JC_ContainerNum = "Extra";
			shipment.OuterPackLines.AddNew().SetContainer(consol, extraContainer);
			Factory.Save();
			var adapter = new CommonCartageStatusValueObjectDataAdapter();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var xsdCartage = GetXsdCartage(shipment.JS_UniqueConsignRef, true, true);
			adapter.CreateOrUpdateFromValueObject(xsdCartage, context);
			Factory.Save(); // confirmations set Shipment Completed
			var jobContainer = shipment.Containers.First();
			AssertEquals("T00001000", jobContainer.JC_DepartureCartageRef);
			AssertEquals("", jobContainer.JC_ArrivalCartageRef);
			AssertEquals("", extraContainer.JC_DepartureCartageRef);
			AssertEquals("", extraContainer.JC_ArrivalCartageRef);
			AssertEquals("DepSlotRef", jobContainer.JC_DepartureSlotReference);
			AssertEquals("", jobContainer.JC_ArrivalSlotReference);
			AssertEquals("", extraContainer.JC_DepartureSlotReference);
			AssertEquals("", extraContainer.JC_ArrivalSlotReference);
			AssertEquals(CurrentDate.AddDays(2), jobContainer.JC_DepartureSlotDateTime);
			AssertEquals(ZDateTime.Empty, jobContainer.JC_ArrivalSlotDateTime);
			AssertEquals(ZDateTime.Empty, extraContainer.JC_DepartureSlotDateTime);
			AssertEquals(ZDateTime.Empty, extraContainer.JC_ArrivalSlotDateTime);
			AssertEquals(ZeroDurationDateTime.AddHours(6), jobContainer.DepartureTruckWaitTime);
			AssertEquals(ZDateTime.Empty, jobContainer.ArrivalTruckWaitTime);
			AssertEquals(ZDateTime.Empty, extraContainer.DepartureTruckWaitTime);
			AssertEquals(ZDateTime.Empty, extraContainer.ArrivalTruckWaitTime);
			AssertEquals(CurrentDate.AddHours(6).AddMinutes(4), jobContainer.JC_FCLWharfGateIn);
			AssertEquals(ZDateTime.Empty, jobContainer.JC_FCLWharfGateOut);
			AssertEquals(ZDateTime.Empty, extraContainer.JC_FCLWharfGateIn);
			AssertEquals(ZDateTime.Empty, extraContainer.JC_FCLWharfGateOut);
			AssertEquals(ContainerYard.PK, jobContainer.JC_OA_DepartureContainerYardAddress_ZAddress.OrgPK);
			AssertEquals(ZGuid.Empty, jobContainer.JC_OA_ArrivalContainerYardAddress_ZAddress.OrgPK);
			AssertEquals(ZGuid.Empty, extraContainer.JC_OA_DepartureContainerYardAddress_ZAddress.OrgPK);
			AssertEquals(ZGuid.Empty, extraContainer.JC_OA_ArrivalContainerYardAddress_ZAddress.OrgPK);
			AssertEquals(CurrentDate.AddHours(6).AddMinutes(3), jobContainer.JC_ContainerYardEmptyPickupGateOut);
			AssertEquals(ZDateTime.Empty, jobContainer.JC_ContainerYardEmptyReturnGateIn);
			AssertEquals(ZDateTime.Empty, extraContainer.JC_ContainerYardEmptyPickupGateOut);
			AssertEquals(ZDateTime.Empty, extraContainer.JC_ContainerYardEmptyReturnGateIn);
			AssertEquals("Not all containers are complete, so don't update", ZDateTime.Empty, shipment.DocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryCartageCompleted);
			AssertEquals("Not all containers are complete, so don't update", ZDateTime.Empty, shipment.DocsAndCartage.JP_PickupTruckWaitTime);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryTruckWaitTime);
			AssertHasStatusUpdateLog(shipment.Logs, "WKD-Work Completed");
			AssertHasStatusUpdateNote(shipment.Notes);
		}

		void AssertHasStatusUpdateLog(Logs logCollection, ZString reference)
		{
			bool result = false;
			StmALogDependentCollection logs = logCollection.GetAllLogs();
			foreach (StmALog log in logs)
			{
				if (log.SL_SE_NKEvent == Events.StatusUpdated.Code && log.SL_Reference.IndexOf(reference) > -1)
				{
					result = true;
					break;
				}
			}

			AssertEquals("Should have log", true, result);
		}

		void AssertHasStatusUpdateNote(Notes noteCollection)
		{
			StmNote[] notes = noteCollection.FindByDescription(FreightConstants.LocalCartageNote);
			AssertEquals("One note should be created", 1, notes.Length);
		}

		Xsd.CartageJob GetXsdCartage(ZString jobReference, bool isExport, bool isContainerised)
		{
			Xsd.CartageJob xsdCartage = new Xsd.CartageJob();
			xsdCartage.Action = FreightConstants.CartageStatusExport;
			xsdCartage.ActionType = FreightConstants.LocalCartageBookingStatus.Codes.WorkCompleted;
			xsdCartage.JobNumber = "T00001000";
			xsdCartage.MessageDescription = "comment";
			xsdCartage.MessageResponseAddress = Env.Registry.MailboxEmailAddress;
			xsdCartage.MessageSystemType = "ediEnterprise";
			xsdCartage.ClientJobReference = jobReference;
			if (isContainerised)
			{
				if (!isExport)
				{
					CreateCartageLeg(xsdCartage, Xsd.DocAddressAddressType.LCT, Xsd.DocAddressAddressType.LCI, CurrentDate, "", true);
					CreateCartageLeg(xsdCartage, Xsd.DocAddressAddressType.LCI, Xsd.DocAddressAddressType.LCY, CurrentDate.AddHours(1), ContainerYard.OH_Code, true);
				}
				else
				{
					CreateCartageLeg(xsdCartage, Xsd.DocAddressAddressType.LCE, Xsd.DocAddressAddressType.LCT, CurrentDate.AddHours(2), "", true);
					CreateCartageLeg(xsdCartage, Xsd.DocAddressAddressType.LCY, Xsd.DocAddressAddressType.LCE, CurrentDate.AddHours(3), ContainerYard.OH_Code, true);
				}
			}
			else
			{
				if (isExport)
				{
					CreateCartageLeg(xsdCartage, Xsd.DocAddressAddressType.LCE, Xsd.DocAddressAddressType.LCF, CurrentDate, "", false);
				}
				else
				{
					CreateCartageLeg(xsdCartage, Xsd.DocAddressAddressType.LCF, Xsd.DocAddressAddressType.LCI, CurrentDate.AddHours(2), "", false);
				}
			}

			return xsdCartage;
		}

		void CreateCartageLeg(Xsd.CartageJob xsdCartage, Xsd.DocAddressAddressType picType, Xsd.DocAddressAddressType dlvType, ZDateTime currentDateTime, ZString eDICode, bool isContainerised)
		{
			var xsdCartageLeg = xsdCartage.CartageLegs.AddNew();
			xsdCartageLeg.CartageLegDates.PickupDemurrage = ZeroDurationDateTime.AddHours(1);
			xsdCartageLeg.CartageLegDates.DeliveryDemurrage = ZeroDurationDateTime.AddHours(2);
			xsdCartageLeg.CartageLegDates.PickupTimeInDate = currentDateTime.AddHours(2).AddMinutes(2);
			xsdCartageLeg.CartageLegDates.PickupTimeOutDate = currentDateTime.AddHours(3).AddMinutes(3);
			xsdCartageLeg.CartageLegDates.DeliverTimeInDate = currentDateTime.AddHours(4).AddMinutes(4);
			xsdCartageLeg.CartageLegDates.DeliverTimeOutDate = currentDateTime.AddHours(5).AddMinutes(5);
			xsdCartageLeg.Pickup.DocAddress.AddressType = picType;
			xsdCartageLeg.Pickup.DocAddress.AddressReference.Organisation.EDICode = eDICode;
			xsdCartageLeg.Delivery.DocAddress.AddressType = dlvType;
			xsdCartageLeg.Delivery.DocAddress.AddressReference.Organisation.EDICode = eDICode;
			if (isContainerised)
			{
				CreateContainerLeg(xsdCartageLeg);
			}
		}

		void CreateContainerLeg(Xsd.CartageLeg xsdCartageLeg)
		{
			var xsdContainerLeg = new Xsd.CartageLegContainer();
			xsdContainerLeg.ContainerNumber = "CONTAINER1";
			xsdContainerLeg.ContainerAdditionalInfo.ArrivalSlotRef = "ArrivalSlotRef";
			xsdContainerLeg.ContainerAdditionalInfo.DepartureSlotRef = "DepSlotRef";
			xsdContainerLeg.ContainerAdditionalInfo.ArrivalSlotDate = CurrentDate.AddDays(1);
			xsdContainerLeg.ContainerAdditionalInfo.DepartureSlotDate = CurrentDate.AddDays(2);
			xsdContainerLeg.ContainerAdditionalInfo.EmptyReturnedByDate = CurrentDate.AddDays(3);
			xsdContainerLeg.ContainerAdditionalInfo.PackDate = CurrentDate.AddDays(4);
			xsdContainerLeg.ContainerAdditionalInfo.UnpackDate = CurrentDate.AddDays(5);
			xsdCartageLeg.Item = xsdContainerLeg;
		}

		OrgHeader ContainerYard
		{
			get
			{
				if (fContainerYard == null)
				{
					fContainerYard = Factory.NewWithValidTestData<OrgHeader>();
				}

				return fContainerYard;
			}
		}

		OrgHeader fContainerYard;

		public void TestImportStatusForDeclaration()
		{
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_MessageType] = "IMP";
			Factory.Save();
			StmALog log = declaration.GetLogs().MostRecentLogByEventTime(Events.StatusUpdated);
			AssertNull("Should not have any status update logged", log);
			AssertEquals("Precondition:", false, declaration.HasChanges);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			ZString reference = new ZString(declaration[JobDeclarationSchema.Constants.JE_DeclarationReference]);
			Xsd.CartageJob xsdCartage = GetXsdCartage(reference, false, true);
			Adapter.CreateOrUpdateFromValueObject(xsdCartage, context);
			AssertEquals("Declaration should have changes", true, declaration.HasChanges);
			log = declaration.GetLogs().MostRecentLogByEventTime(Events.StatusUpdated);
			AssertNotNull("Should have a status update logged", log);
		}

		[TestDate(2011, 10, 11)]
		public override void TestImportCommentsAndBookingStatus()
		{
			CommonShipment shipment = GetNewShipment("AIR", true);
			var transportAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportAddress.Header.OH_IsLocalTransport = true;
			transportAddress.Header.OH_Code = "TRANSCOSYD";
			transportAddress.Header.OH_FullName = "TransCo";
			Factory.Save();
			AssertNull(shipment.Logs.MostRecentLogByEventTime(Events.StatusUpdated));
			AssertNull(shipment.Logs.MostRecentLogByEventTime(Events.DataImport, FreightConstants.CargoWiseOnePortTransportXMLFile));
			StmNote[] notes = shipment.Notes.FindByDescription(FreightConstants.LocalCartageNote);
			AssertEquals("There should be no note", 0, notes.Length);
			Xsd.CartageJob cartageValue = GetXsdCartage(shipment.JS_UniqueConsignRef, false, true);
			cartageValue.Action = ExpectedBookingAction;
			cartageValue.ActionType = Enterprise.Freight.Common.Business.CommonFreightConstants.LocalCartageBookingStatus.Codes.PreBookingAdvice;
			cartageValue.MessageDescription = "hello";
			cartageValue.BillTo = new OrganisationValueObjectDataAdapter().ExportToValueObject(transportAddress.Header, null);
			var buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			var interchange = (Xsd.XmlInterchange)context.Interchange;
			var interchangeInfo = interchange.InterchangeInfo;
			var source = interchangeInfo.Source;
			source.CompanyCode = "EDI";
			source.EnterpriseCode = "CAR";
			source.OriginServer = "SER";
			Adapter.CreateOrUpdateFromValueObject(cartageValue, context);
			StmALog statusUpdateLog = shipment.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
			AssertNotNull("STU event", statusUpdateLog);
			AssertEquals("STU event reference", "PBA-Pre Booking Advice (PortTransport) received from TRANSCOSYD - TransCo, Reference S00001000.", statusUpdateLog.SL_Reference);
			AssertNotNull("DIM event", shipment.Logs.MostRecentLogByEventTime(Events.DataImport, FreightConstants.CargoWiseOnePortTransportXMLFile));
			notes = shipment.Notes.FindByDescription(FreightConstants.LocalCartageNote);
			AssertEquals("there should be a note", 1, notes.Length);
			AssertEquals("Note contents", true, notes[0].ST_NoteDataAsText.Contains(@"@11-Oct-11 00:00 - PBA-Pre Booking Advice (PortTransport) received from TRANSCOSYD - TransCo, Reference S00001000.
Comments: hello"));
			AssertMultilineEquals("", @"Successfully matched organization with code 'TRANSCOSYD', Mapping Organization: TESORGBNE, Matching by Foreign code: TRANSCOSYD, Using: Similarity Matcher, Found match: True
@11-Oct-11 00:00 - PBA-Pre Booking Advice (PortTransport) received from TRANSCOSYD - TransCo, Reference S00001000.
Comments: hello
Job S00001000 updated.
", buffer.AsString, '\n');
		}

		public void TestImportParentInformationFromValueObjectOnly_LoadList_Export()
		{
			CurrentDate = ZDateTime.Today;
			var loadList = GetNewLoadList(Core.Constants.TransportCodes.Air, true);
			Factory.Save();
			var adapter = new CommonCartageStatusValueObjectDataAdapter();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var xsdCartage = GetXsdCartage(loadList.JK_UniqueConsignRef, true, true);
			adapter.CreateOrUpdateFromValueObject(xsdCartage, context);
			var jobContainer = loadList.Containers[0];
			Factory.Save();
			AssertEquals("T00001000", jobContainer.JC_DepartureCartageRef);
			AssertEquals("", jobContainer.JC_ArrivalCartageRef);
			AssertEquals("DepSlotRef", jobContainer.JC_DepartureSlotReference);
			AssertEquals("", jobContainer.JC_ArrivalSlotReference);
			AssertEquals(CurrentDate.AddDays(2), jobContainer.JC_DepartureSlotDateTime);
			AssertEquals(ZDateTime.Empty, jobContainer.JC_ArrivalSlotDateTime);
			AssertEquals(ZeroDurationDateTime.AddHours(6), jobContainer.DepartureTruckWaitTime);
			AssertEquals(ZDateTime.Empty, jobContainer.ArrivalTruckWaitTime);
			AssertEquals(CurrentDate.AddHours(6).AddMinutes(4), jobContainer.JC_FCLWharfGateIn);
			AssertEquals(ZDateTime.Empty, jobContainer.JC_FCLWharfGateOut);
			AssertEquals(ContainerYard.PK, jobContainer.JC_OA_DepartureContainerYardAddress_ZAddress.OrgPK);
			AssertEquals(CurrentDate.AddHours(6).AddMinutes(3), jobContainer.JC_ContainerYardEmptyPickupGateOut);
			AssertEquals(ZGuid.Empty, jobContainer.JC_OA_ArrivalContainerYardAddress_ZAddress.OrgPK);
			AssertEquals(ZDateTime.Empty, jobContainer.JC_ContainerYardEmptyReturnGateIn);
			var newLoadList = new BusinessObjectFactory().Load<CommonConsol>(loadList.PK);
			AssertHasStatusUpdateLog(newLoadList.Logs, "WKD-Work Completed");
			AssertHasStatusUpdateNote(newLoadList.Notes);
		}

		public void TestImportParentInformationFromValueObjectOnly_LoadList_Import()
		{
			CurrentDate = ZDateTime.Today;
			var loadList = GetNewLoadList(Core.Constants.TransportCodes.Air, false);
			loadList.JK_OA_ContainerYardEmptyPickupAddress = ZGuid.Empty;
			Factory.Save();
			var adapter = new CommonCartageStatusValueObjectDataAdapter();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var xsdCartage = GetXsdCartage(loadList.JK_UniqueConsignRef, false, true);
			adapter.CreateOrUpdateFromValueObject(xsdCartage, context);
			var jobContainer = loadList.Containers[0];
			Factory.Save();
			AssertEquals("", jobContainer.JC_DepartureCartageRef);
			AssertEquals("T00001000", jobContainer.JC_ArrivalCartageRef);
			AssertEquals("", jobContainer.JC_DepartureSlotReference);
			AssertEquals("ArrivalSlotRef", jobContainer.JC_ArrivalSlotReference);
			AssertEquals(ZDateTime.Empty, jobContainer.JC_DepartureSlotDateTime);
			AssertEquals(CurrentDate.AddDays(1), jobContainer.JC_ArrivalSlotDateTime);
			AssertEquals(ZDateTime.Empty, jobContainer.DepartureTruckWaitTime);
			AssertEquals(ZeroDurationDateTime.AddHours(6), jobContainer.ArrivalTruckWaitTime);
			AssertEquals(ZDateTime.Empty, jobContainer.JC_FCLWharfGateIn);
			AssertEquals(CurrentDate.AddHours(3).AddMinutes(3), jobContainer.JC_FCLWharfGateOut);
			AssertEquals(ZGuid.Empty, jobContainer.JC_OA_DepartureContainerYardAddress_ZAddress.OrgPK);
			AssertEquals(ZDateTime.Empty, jobContainer.JC_ContainerYardEmptyPickupGateOut);
			AssertEquals(ContainerYard.PK, jobContainer.JC_OA_ArrivalContainerYardAddress_ZAddress.OrgPK);
			AssertEquals(CurrentDate.AddHours(5).AddMinutes(4), jobContainer.JC_ContainerYardEmptyReturnGateIn);
			var newLoadList = new BusinessObjectFactory().Load<CommonConsol>(loadList.PK);
			AssertHasStatusUpdateLog(newLoadList.Logs, "WKD-Work Completed");
			AssertHasStatusUpdateNote(newLoadList.Notes);
		}

		public void TestImportCommentsAndBookingStatus_QuotedBooking()
		{
			IQuotedBooking quotedBooking = GetNewQuoteBooking(Core.Constants.TransportCodes.Air);
			Factory.Save();
			IStmNoteParent quotedBookingWithNotes = (IStmNoteParent)quotedBooking;
			IStmALogParent quotedBookingWithLogs = (IStmALogParent)quotedBooking;
			IStmALogParent bookingWithLogs = (IStmALogParent)quotedBooking.ForwardingShipment;
			AssertCollectionContains("prerequisite", bookingWithLogs, quotedBookingWithLogs.BusinessObjectsWithRelatedEvents);
			AssertNull(bookingWithLogs.Logs.MostRecentLogByEventTime(Events.StatusUpdated));
			AssertNull(bookingWithLogs.Logs.MostRecentLogByEventTime(Events.DataImport, FreightConstants.CargoWiseOnePortTransportXMLFile));
			StmNote[] notes = quotedBookingWithNotes.Notes.FindByDescription(FreightConstants.LocalCartageNote);
			AssertEquals("There should be no note", 0, notes.Length);
			Xsd.CartageJob cartageValue = GetXsdCartage(quotedBooking.UniqueConsignRef, false, false);
			cartageValue.Action = ExpectedBookingAction;
			cartageValue.ActionType = "ABC";
			cartageValue.MessageDescription = ZString.Empty;
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			Adapter.CreateOrUpdateFromValueObject(cartageValue, context);
			StmALog statusUpdateLog = bookingWithLogs.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
			AssertNotNull("STU event", statusUpdateLog);
			AssertEquals("STU event reference", "ABC-ABC (Port Transport) received from  - , Reference S00001000.", statusUpdateLog.SL_Reference);
			AssertNotNull("DIM event", bookingWithLogs.Logs.MostRecentLogByEventTime(Events.DataImport, FreightConstants.CargoWiseOnePortTransportXMLFile));
			notes = quotedBookingWithNotes.Notes.FindByDescription(FreightConstants.LocalCartageNote);
			AssertEquals("there should be a note", 1, notes.Length);
			AssertEquals("Note contents", true, notes[0].ST_NoteDataAsText.Contains("ABC-ABC (Port Transport) received from  - , Reference S00001000."));
		}

		public void TestImportParentInformationFromValueObjectOnly_WhsOrder()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var clientPK = helper.CreateClient("CLIENT");
			var whs = helper.CreateWarehouse("WHS", "A");
			var orderPK = helper.CreateWhsOrder(clientPK, whs.PK, "OR1", new NotificationBuffer());
			Factory.Save();
			var orderType = ObjectFactory.GetType<IWhsOrder>();
			var order = Factory.Load(orderType, orderPK);
			var orderAsCartageParent = order as ICartageParent;
			var orderNotes = order.GetNotes();
			var orderLogs = order.GetLogs();
			var xsdCartage = GetXsdCartage(orderAsCartageParent.UniqueConsignmentID, false, false);
			// Preconditions
			AssertNull(orderLogs.MostRecentLogByEventTime(Events.StatusUpdated));
			AssertNull(orderLogs.MostRecentLogByEventTime(Events.DataImport, FreightConstants.CargoWiseOnePortTransportXMLFile));
			AssertEquals("There should be no note", 0, orderNotes.FindByDescription(FreightConstants.LocalCartageNote).Length);
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			Adapter.CreateOrUpdateFromValueObject(xsdCartage, context);
			var statusUpdateLog = orderLogs.MostRecentLogByEventTime(Events.StatusUpdated);
			AssertNotNull("Should have status update event", statusUpdateLog);
			AssertEquals("WKD-Work Completed/Container De-hired (Port Transport) received from  - , Reference W00000001.", statusUpdateLog.SL_Reference);
			AssertNotNull("DIM event", orderLogs.MostRecentLogByEventTime(Events.DataImport, FreightConstants.CargoWiseOnePortTransportXMLFile));
			var localCartageNotes = orderNotes.FindByDescription(FreightConstants.LocalCartageNote);
			AssertEquals("there should be a note", 1, localCartageNotes.Length);
			AssertEquals("Note contents", true, localCartageNotes[0].ST_NoteDataAsText.Contains("WKD-Work Completed/Container De-hired (Port Transport) received from  - , Reference W00000001."));
			AssertEquals("Note contents", true, localCartageNotes[0].ST_NoteDataAsText.Contains("Comments: comment"));
		}

		protected override ZString ExpectedBookingAction
		{
			get
			{
				return FreightConstants.CartageStatusExport;
			}
		}

		protected override ZString ExpectedBookingStatus
		{
			get
			{
				return FreightConstants.LocalCartageBookingStatus.Codes.WorkCommenced;
			}
		}

		protected override ZString ExpectedCartageType
		{
			get
			{
				return Constants.CartageJobType.NEW_AirExport;
			}
		}

		protected override ZString ExpectedReference
		{
			get
			{
				return "";
			}
		}

		protected override void AssertXsdLegs(Xsd.CartageJob xsdCartage)
		{
			AssertEquals("1 Xsd Leg should have been created", 1, xsdCartage.CartageLegs.Count);
			Xsd.CartageLeg xsdLeg = xsdCartage.CartageLegs[0];
			AssertEquals("Pickup Org", "Consignee for test", xsdLeg.Pickup.DocAddress.AddressReference.Organisation.OrganisationDetails.Name);
			AssertEquals("Delivery Org", "New CTO", xsdLeg.Delivery.DocAddress.AddressReference.Organisation.OrganisationDetails.Name);
			AssertEquals("DangerousGoodsCode", "SUBSb", xsdLeg.DangerousGoods[0].UNDGCode);
			Xsd.CartageLegPackageRecords xsdPacks = xsdLeg.Item as Xsd.CartageLegPackageRecords;
		}

		protected override void AssertXsdSailing(Xsd.CartageJob xsdCartage)
		{
			Xsd.FlightWithFlightNumber xsdFlightWithFlightNumber = xsdCartage.SailingInfo.Item as Xsd.FlightWithFlightNumber;
			AssertNotNull("XsdFlightWithFlightNumber should not be null", xsdFlightWithFlightNumber);
			AssertEquals("ETA", CurrentDate.AddDays(1), xsdFlightWithFlightNumber.ETA);
			AssertEquals("ETD", CurrentDate, xsdFlightWithFlightNumber.ETD);
			AssertEquals("FlightNoJourneyNoTruckRegNo", "QF281", xsdFlightWithFlightNumber.FlightNoJourneyNoTruckRegNo);
			AssertEquals("AvailableDate", CurrentDate.AddDays(1), xsdFlightWithFlightNumber.Dates.AvailableDate);
			AssertEquals("StorageDate", CurrentDate.AddDays(1), xsdFlightWithFlightNumber.Dates.StorageDate);
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get
			{
				return false;
			}
		}

		protected override CommonCartage GetJobCartage()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			CommonCartage result = Factory.New<CommonCartage>();
			result.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			result.FirstDocAddress.E2_OA_Address = Consignee_GB.MainAddress.PK;
			result.SecondDocAddress.E2_OA_Address = CTO.MainAddress.PK;
			result.LocalClientAddressPK = Consignor_AU.MainAddress.PK;
			result.JJ_GoodsDescription = "Goods description";
			result.JJ_JX_Sailing = Sailing.PK;
			result.JJ_OuterPacks = 20;
			result.JJ_F3_NKPackType = "PLT";
			result.JJ_Weight = 5.3m;
			result.JJ_Volume = 10.1m;
			result.LooseBookedMoves[0].EW_BookedLength = 5.0m;
			result.LooseBookedMoves[0].EW_BookedWidth = 3.0m;
			result.LooseBookedMoves[0].EW_BookedHeight = 1.0m;
			UNDGDataItem item = result.LooseBookedMoves[0].UNDGs.AddNew();
			item.DI_DG = Substance.PK;
			result.JJ_GoodsDescription = "Goods description";
			return result;
		}

		protected override void SetupConsolContainer(CommonContainer container)
		{
			container.JC_ContainerNum = "Container1";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_RC = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
			container.JC_SealNum = "123456";
			container.JC_AdditionalSealNum = "789";
			container.JC_SetPointTempUnit = "C";
		}

		JobSailing Sailing
		{
			get
			{
				JobVoyage voyage = Factory.New<JobVoyage>();
				voyage.JV_FlightDate = new ZDateTime(2006, 5, 2);
				voyage.JV_VoyageFlight = "QF281";
				VoyageOrigin origin = Factory.New<VoyageOrigin>();
				origin.JA_RL_NKPortOfLoading = "AUSYD";
				origin.JA_JV = voyage.PK;
				origin.JA_E_DEP = CurrentDate;
				VoyageDestination dest = Factory.New<VoyageDestination>();
				dest.JB_RL_NKPortOfDischarge = "GBLON";
				dest.JB_JV = voyage.PK;
				dest.JB_E_ARV = CurrentDate.AddDays(1);
				JobSailing result = Factory.New<JobSailing>();
				result.JX_JA = origin.PK;
				result.JX_JB = dest.PK;
				result.Destination.JB_AvailabilityDate = CurrentDate.AddDays(1);
				result.Destination.JB_StorageDate = CurrentDate.AddDays(1);
				result.JX_DepotAvailabilityDate = CurrentDate.AddDays(1);
				result.JX_DepotStorageDate = CurrentDate.AddDays(1);
				return result;
			}
		}
	}
}
