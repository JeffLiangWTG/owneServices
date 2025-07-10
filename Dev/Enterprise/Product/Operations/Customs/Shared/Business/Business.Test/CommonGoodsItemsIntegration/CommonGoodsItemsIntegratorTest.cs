using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing.CommonGoodsItemsIntegration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.Business.CommonGoodsItemsIntegration.Testing
{
	class CommonGoodsItemsIntegratorTest : BaseCommonGoodsItemsIntegratorTest<BaseJobDeclaration, CusEntryHeader, CommonGoodsItemsIntegrator, NctsHeaderToAttachCollection>
	{
		protected override (CusEntryHeader, BaseJobComInvoiceLine line1, BaseJobComInvoiceLine line2) CreateEntryWithTwoInvoiceLines(BusinessObjectFactory factory)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;

			entryHeader.MovementReferenceNumberSetter("MRN123");

			invoiceLine1.FillWithValidTestData();
			invoiceLine1.JI_Description = "STUFF AND THINGS";
			invoiceLine1.JI_Weight = 1234.56m;
			invoiceLine1.JI_NetWeight = 1.4m;
			invoiceLine1.JI_WeightUQ = Weight.Grams;
			invoiceLine1.JI_NetWeightUQ = Weight.Kilograms;
			invoiceLine1.JI_Tariff = "1234567800";
			invoiceLine1.JI_LinePrice = 888.99m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.FillWithValidTestData();

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "HB-1234";

			var package1 = declaration.Packages.AddNew();
			package1.CW_HouseBill = houseBill.CU_BillUniqueCode;
			package1.CW_PackType = "BG";
			package1.CW_MarksAndNos = "M-1234";
			var packagePivot1 = invoiceLine1.PackagesPivot.AddNew();
			packagePivot1.CHC_CW = package1.PK;
			packagePivot1.CHC_NumberOfPacks = 123;

			var package2 = declaration.Packages.AddNew();
			package2.CW_HouseBill = houseBill.CU_BillUniqueCode;
			package2.CW_PackType = "BX";
			package2.CW_MarksAndNos = "M-2345";
			var packagePivot2 = invoiceLine1.PackagesPivot.AddNew();
			packagePivot2.CHC_CW = package2.PK;
			packagePivot2.CHC_NumberOfPacks = 234;

			return (entryHeader, invoiceLine1, invoiceLine2);
		}

		protected override void AssertExpectedPropertiesOnGoodsItem(ICommonGoodsItem goodsItem, BaseJobComInvoiceLine line1)
		{
			AssertEquals("STUFF AND THINGS", goodsItem.GoodsDescription);
			AssertEquals(1234.56m, goodsItem.GrossMass);
			AssertEquals(1.4m, goodsItem.NetMass);
			AssertEquals(Weight.Grams, goodsItem.GrossMassUnit);
			AssertEquals(Weight.Kilograms, goodsItem.NetMassUnit);
			AssertEquals("1234.56.78 00", goodsItem.CommodityCode);
			AssertEquals(ZString.Empty, goodsItem.DispatchCountry);
			AssertEquals(ZString.Empty, goodsItem.DestinationCountry);
			AssertEquals(888.99m, goodsItem.Value);

			var packages = goodsItem.Packages.ToArray();
			AssertEquals(2, packages.Length);

			AssertEquals("HB:HB-1234", packages[0].BillOrReferenceNumber);
			AssertEquals("BG", packages[0].PackageType);
			AssertEquals(123, packages[0].PackageCount);
			AssertEquals("M-1234", packages[0].MarksAndNumbers);

			AssertEquals("HB:HB-1234", packages[1].BillOrReferenceNumber);
			AssertEquals("BX", packages[1].PackageType);
			AssertEquals(234, packages[1].PackageCount);
			AssertEquals("M-2345", packages[1].MarksAndNumbers);
		}

		protected override ICommonGoodsItem CreateCommonGoodsItem()
		{
			return new CommonGoodsItem()
			{
				GoodsDescription = "STUFF AND THINGS",
				GrossMass = 1234.56m,
				NetMass = 1.4m,
				GrossMassUnit = Weight.Grams,
				NetMassUnit = Weight.Kilograms,
				CommodityCode = "1234.56.78 00",
				Value = 888.99m,
				Packages = new[]
				{
					new CommonPackage
					{
						BillOrReferenceNumber = "HB-1234",
						PackageType = "BG",
						PackageCount = 123,
						MarksAndNumbers = "M-1234"
					},
					new CommonPackage
					{
						BillOrReferenceNumber = "HB-1234",
						PackageType = "BX",
						PackageCount = 234,
						MarksAndNumbers = "M-2345"
					}
				}
			};
		}

		protected override void AssertExpectedPropertiesOnInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
			AssertEquals("STUFF AND THINGS", invoiceLine.JI_Description);
			AssertEquals(1234.56m, invoiceLine.JI_Weight);
			AssertEquals(1.4m, invoiceLine.JI_NetWeight);
			AssertEquals(Weight.Grams, invoiceLine.JI_WeightUQ);
			AssertEquals(Weight.Kilograms, invoiceLine.JI_NetWeightUQ);
			AssertEquals("1234.56.78 00", invoiceLine.JI_FormattedTariff);
			AssertEquals(888.99m, invoiceLine.JI_LinePrice);

			var declaration = invoiceLine.Declaration;
			AssertEquals(1, declaration.Bills.Count);
			AssertEquals("HB:HB-1234", declaration.Bills[0].CU_BillUniqueCode);
			AssertEquals(2, declaration.Packages.Count);

			var package1 = declaration.Packages[0];
			AssertEquals("HB:HB-1234", package1.CW_HouseBill);
			AssertEquals("BG", package1.CW_PackType);
			AssertEquals(123, package1.CW_PackQty);
			AssertEquals("M-1234", package1.CW_MarksAndNos);

			var package2 = declaration.Packages[1];
			AssertEquals("HB:HB-1234", package2.CW_HouseBill);
			AssertEquals("BX", package2.CW_PackType);
			AssertEquals(234, package2.CW_PackQty);
			AssertEquals("M-2345", package2.CW_MarksAndNos);
		}
	}
}
