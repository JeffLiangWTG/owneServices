using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class CustomesToTWEventTest : IntegrationTestCaseWithFactory
	{
		public void TestCustomsUXMLEvent_CENAndCRN_AndAddOnValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);
				warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "NZCHC";

				var consol = CreateConsol("MSB1", vessel, "NZCHC", "FRBNE");
				consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

				CreateWorkflowTemplateForShipmentToSendCustomsEventsToForwarder();
				var shipment = CreateShipment(consol, "HSB1", "NZCHC", "FRBNE", today.AddDays(-1), today.AddDays(9), consignor, consignee);
				var packline = CreateOuterPackline(shipment, 10, Enterprise.Core.Constants.PkgUnit.Pallet);
				shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;

				TriggerAndFireTransitRequestUsingBookingRequested(consol);

				var rcn = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();

				var eventQuery = new ZQuery();
				eventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsNumberEnteredCode);
				AssertEquals("No CEN event from shipment yet.", 0, rcn.Logs.Find(eventQuery).Length);

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_MessageType = "IMP";

				var entryInstruction = declaration.CustomsEntryInstructions[0];
				entryInstruction.CEI_TotalInnerPackages = 50;

				var package = declaration.Packages[0];
				package.CW_PackQty = 100;

				var invoiceHeader = declaration.Invoices.AddNew();
				var line1 = invoiceHeader.InvoiceLines.AddNew();
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var procedure = helper.CreateOrFindExistingRefCusProcedure("FR", "AA", "01", "01", "011", "DESC.", "IMP,EXP", "A");
				line1.JI_Procedure = "0101011";

				var merger = new LineMerger(declaration);
				merger.DoMerge();
				var entryHeader = declaration.ActiveEntryHeaders[0];

				entryHeader.EntryNumber = "123123";
				entryHeader.CH_BGMReference = "UpdateAnyField";

				entryHeader.CH_EntryStatus = "100";
				entryHeader.CH_BGMReference = "UpdateAnyField";
				Factory.Save();

				var factoryAfterPublishingCRNEvent = new BusinessObjectFactory() { RefreshEnabled = false };
				var matchingConsignmentAfterPublishingCRNEvent = factoryAfterPublishingCRNEvent.Load<WhsItemReceiveConsignment>(rcn.PK);

				var additionReferenceQuery = new ZQuery();
				additionReferenceQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, matchingConsignmentAfterPublishingCRNEvent.PK);
				var additionReferences = Factory.Load<CusEntryNumber>(additionReferenceQuery);
				var cenAdditionReference = additionReferences.FirstOrDefault(p => p.CE_EntryType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
				AssertNotNull("CEN Addition Reference.", cenAdditionReference);
				var crnAdditionReference = additionReferences.FirstOrDefault(p => p.CE_EntryType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
				AssertNotNull("CRN Addition Reference.", crnAdditionReference);

				var cenAddOnValueQuery = new ZQuery();
				cenAddOnValueQuery.AddToFilter(GenCustomAddOnValueSchema.XV_ParentID, cenAdditionReference.PK);
				cenAddOnValueQuery.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, cenAdditionReference.TablePrefix);
				cenAddOnValueQuery.AddToFilter(GenCustomAddOnValueSchema.XV_Type, "INT");
				var cenAddOnValues = Factory.Load<GenCustomAddOnValue>(cenAddOnValueQuery);
				AssertEquals("CEN Addition Reference has AddOnValue of out pack qty.", "100", cenAddOnValues.FirstOrDefault(p => p.XV_Name == "TWOuterPackQty")?.XV_Data);
				AssertEquals("CEN Addition Reference has AddOnValue of inner pack qty.", "50", cenAddOnValues.FirstOrDefault(p => p.XV_Name == "TWInnerPackQty")?.XV_Data);

				var crnAddOnValueQuery = new ZQuery();
				crnAddOnValueQuery.AddToFilter(GenCustomAddOnValueSchema.XV_ParentID, cenAdditionReference.PK);
				crnAddOnValueQuery.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, cenAdditionReference.TablePrefix);
				crnAddOnValueQuery.AddToFilter(GenCustomAddOnValueSchema.XV_Type, "INT");
				var crnAddOnValues = Factory.Load<GenCustomAddOnValue>(crnAddOnValueQuery);
				AssertEquals("CRN Addition Reference has AddOnValue of out pack qty.", "100", crnAddOnValues.FirstOrDefault(p => p.XV_Name == "TWOuterPackQty")?.XV_Data);
				AssertEquals("CRN Addition Reference has AddOnValue of inner pack qty.", "50", crnAddOnValues.FirstOrDefault(p => p.XV_Name == "TWInnerPackQty")?.XV_Data);
			}
		}
	}
}
