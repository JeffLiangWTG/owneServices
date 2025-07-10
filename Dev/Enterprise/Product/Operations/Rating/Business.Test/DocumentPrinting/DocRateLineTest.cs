using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business.Test.DocumentPrinting
{
	public class DocRateLineTest : RatingTestCase
	{
		public void TestOrgLevelSortOrder()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.NewWithValidTestData<RatingHeader>();
			header.TH_OH = org.PK;
			header.TH_RateType = "QTE";
			header.TH_QuoteDate = ZDate.Today;

			var chargeCodes = Factory.Load<AccChargeCode>(new ZQuery());
			var chargeCode = chargeCodes[0];
			var chargeCode2 = chargeCodes[1];

			var entry = header.AddRateEntry("AIR", "LSE", "AUSYD");
			var line = entry.AddFlatRateLine(chargeCode.AC_Code, 100);
			line.TL_AC = chargeCode.PK;
			using (RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var wrapper = new DocRateLine(line);
				wrapper.ClientForSorting = org;
				AssertEquals("No Sort order set so should return 0", 0, ((ISortableDocLine)wrapper).OrgLevelSortOrder);

				var chargeOrder = org.RatingDocumentsChargeOrders.AddNew();
				chargeOrder.RCO_AC_ChargeCode = chargeCode.PK;
				chargeOrder.RCO_DocumentType = "All";
				chargeOrder.RCO_PrintOrder = 1;

				Factory.Save();

				AssertEquals("Sort order added to list so should return 1", 1, ((ISortableDocLine)wrapper).OrgLevelSortOrder);

				chargeOrder.RCO_AC_ChargeCode = chargeCode2.PK;
				chargeOrder.RCO_PrintOrder = 2;
				AssertEquals("Sort order is to the wrong ChargeCode so should return 0", 0, ((ISortableDocLine)wrapper).OrgLevelSortOrder);
			}
		}
	}
}
