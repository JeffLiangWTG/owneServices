using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.SG.V4.DataTransfer.Universal.Testing
{
	partial class StandaloneCommercialInvoiceDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestInvoiceLineSGAddInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.MarksAndNumbers = "SG Marks";
			invoiceLine.SG_LotNo = "P17";
			invoiceLine.SG_PreviousLotNo = "K110";
			invoiceLine.SG_InwardHAWB = "HB4029";
			invoiceLine.SG_InwardMAWB = "61800429487";
			invoiceLine.SG_OutwardMAWB = "08100239287";
			invoiceLine.SG_TotalDutiableWGTVOLQTY = 840m;
			invoiceLine.SG_TotalDutiableWGTVOLQTYUnit = "BOX";

			var writer = new StandaloneCommercialInvoiceDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)));
			var shipment = writer.GetDataObject(invoice);
			var addInfos = shipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].AddInfoCollection;
			bool hasLotNo = false;
			bool hasPreviousLotNo = false;
			bool hasInwardHB = false;
			bool hasInwardMawb = false;
			bool hasOutwardMawb = false;
			bool hasTotalDutiableWGTVOLQTY = false;
			bool hasTotalDutiableWGTVOLQTYUnit = false;
			foreach (var addInfo in addInfos)
			{
				if (addInfo.Key.ToString() == "SG50")
				{
					hasLotNo = addInfo.Value.ToString() == "P17";
				}

				if (addInfo.Key.ToString() == "SG70")
				{
					hasPreviousLotNo = addInfo.Value.ToString() == "K110";
				}

				if (addInfo.Key.ToString() == "SG47")
				{
					hasInwardHB = addInfo.Value.ToString() == "HB4029";
				}

				if (addInfo.Key.ToString() == "SG48")
				{
					hasInwardMawb = addInfo.Value.ToString() == "61800429487";
				}

				if (addInfo.Key.ToString() == "SG63")
				{
					hasOutwardMawb = addInfo.Value.ToString() == "08100239287";
				}

				if (addInfo.Key.ToString() == "SG89")
				{
					hasTotalDutiableWGTVOLQTY = addInfo.Value.ToString() == "840";
				}

				if (addInfo.Key.ToString() == "SG90")
				{
					hasTotalDutiableWGTVOLQTYUnit = addInfo.Value.ToString() == "BOX";
				}
			}

			Assert("Add info for SG_LotNo is being populated", hasLotNo);
			Assert("Add info for SG_PreviousLotNo is being populated", hasPreviousLotNo);
			Assert("Add info for SG_InwardHAWB is being populated", hasInwardHB);
			Assert("Add info for SG_InwardMAWB is being populated", hasInwardMawb);
			Assert("Add info for SG_OutwardMAWB is being populated", hasOutwardMawb);
			Assert("Add info for SG_TotalDutiableWGTVOLQTY is being populated", hasTotalDutiableWGTVOLQTY);
			Assert("Add info for SG_TotalDutiableWGTVOLQTYUnit is being populated", hasTotalDutiableWGTVOLQTYUnit);
		}

		public void TestAddInfoNumericData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SG_UnitDutiableWGTVOLQTY = 590.75m;
			invoiceLine.SG_UnitDutiableWGTVOLQTYUnit = "STK";
			invoiceLine.SG_TotalDutiableWGTVOLQTY = 840m;
			invoiceLine.SG_TotalDutiableWGTVOLQTYUnit = "BOX";

			var writer = new StandaloneCommercialInvoiceDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)));
			var shipment = writer.GetDataObject(invoice);
			var addInfos = shipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].AddInfoCollection;
			AssertNotNull(addInfos);

			bool hasSG_UnitDutiableWGTVOLQTY = false;
			bool hasSG_UnitDutiableWGTVOLQTYUnit = false;
			bool hasTotalDutiableWGTVOLQTY = false;
			bool hasTotalDutiableWGTVOLQTYUnit = false;
			foreach (var addInfo in addInfos)
			{
				if (addInfo.Key.ToString() == "SG92")
				{
					hasSG_UnitDutiableWGTVOLQTY = addInfo.Value.ToString() == "590.75";
				}

				if (addInfo.Key.ToString() == "SG93")
				{
					hasSG_UnitDutiableWGTVOLQTYUnit = addInfo.Value.ToString() == "STK";
				}

				if (addInfo.Key.ToString() == "SG89")
				{
					hasTotalDutiableWGTVOLQTY = addInfo.Value.ToString() == "840";
				}

				if (addInfo.Key.ToString() == "SG90")
				{
					hasTotalDutiableWGTVOLQTYUnit = addInfo.Value.ToString() == "BOX";
				}
			}

			Assert("Add info for SG_UnitDutiableWGTVOLQTY is being populated", hasSG_UnitDutiableWGTVOLQTY);
			Assert("Add info for SG_UnitDutiableWGTVOLQTYUnit is being populated", hasSG_UnitDutiableWGTVOLQTYUnit);
			Assert("Add info for SG_TotalDutiableWGTVOLQTY is being populated", hasTotalDutiableWGTVOLQTY);
			Assert("Add info for SG_TotalDutiableWGTVOLQTYUnit is being populated", hasTotalDutiableWGTVOLQTYUnit);
		}
	}
}
