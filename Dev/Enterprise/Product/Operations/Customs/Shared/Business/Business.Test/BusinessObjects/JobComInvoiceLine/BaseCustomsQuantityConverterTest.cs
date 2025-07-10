using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseCustomsQuantityConverterTest : TestCaseWithFactory
	{
		public void TestCalculateCustomsQuantityWhenPartIsThere()
		{
			OrgPartUnit partUnit = product.PartUnits.AddNew();
			partUnit.OF_ParentPackType = "CTN";
			partUnit.OF_PackType = "BOX";
			partUnit.OF_QuantityInParent = 24;

			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.JI_CustomsUnitQty = "BOX";
			invoiceLine.JI_InvoiceUQ = "CTN";
			invoiceLine.JI_InvoiceQuantity = 100;
			AssertEquals("Customs Quantity is set", ExpectedCustomsQuantityOfTestCalculateCustomsQuantityWhenPartIsThere, invoiceLine.JI_CustomsQuantity);
		}

		public virtual void TestCalculateCustomsQuantityWhenNetWeightIsPresent()
		{
			invoiceLine.JI_CustomsUnitQty = "BOT";
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_NetWeight = 1000m;
			AssertEquals(0m, invoiceLine.JI_CustomsQuantity);
			invoiceLine.JI_CustomsUnitQty = "T";
			AssertEquals(1m, invoiceLine.JI_CustomsQuantity);
		}

		public void TestCountrySpecificPartConversion()
		{
			OrgPartUnit partUnit = product.PartUnits.AddNew();
			partUnit.OF_ParentPackType = "CTN";
			partUnit.OF_PackType = "BOX";
			partUnit.OF_QuantityInParent = 24;

			partUnit = product.PartUnits.AddNew();
			partUnit.OF_ParentPackType = "BOX";
			partUnit.OF_PackType = "BOT";
			partUnit.OF_QuantityInParent = 12;

			invoiceLine.CustomsQuantityConverter = new TestConverter(invoiceLine, (ZPropertyInfoDecimal)invoiceLine.JI_CustomsQuantityInfo, (ZPropertyInfoString)invoiceLine.JI_CustomsUnitQtyInfo);

			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.JI_CustomsUnitQty = "BOT";
			invoiceLine.JI_InvoiceUQ = "CTN";
			invoiceLine.JI_InvoiceQuantity = 100;

			//24*12*100 = 28800
			AssertEquals("Conversion Factor", ExpectedCustomsQuantityOfTestCountrySpecificPartConversion, invoiceLine.JI_CustomsQuantity);
		}

		public void TestCalculateCountrySpecificQuantity()
		{
			OrgPartUnit partUnit = product.PartUnits.AddNew();
			partUnit.OF_ParentPackType = "CTN";
			partUnit.OF_PackType = "BOX";
			partUnit.OF_QuantityInParent = 24;

			invoiceLine.CustomsQuantityConverter = new TestConverter(invoiceLine, (ZPropertyInfoDecimal)invoiceLine.JI_CustomsQuantityInfo, (ZPropertyInfoString)invoiceLine.JI_CustomsUnitQtyInfo);

			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.JI_CustomsUnitQty = "BOX";
			invoiceLine.JI_InvoiceQuantity = 100;
			invoiceLine.JI_InvoiceUQ = "CTN";

			AssertEquals("Volume Should be set", ExpectedVolumeOfTestCalculateCountrySpecificQuantity, invoiceLine.JI_Volume);
		}

		protected virtual ZDecimal ExpectedCustomsQuantityOfTestCalculateCustomsQuantityWhenPartIsThere => 2400m;
		protected virtual ZDecimal ExpectedCustomsQuantityOfTestCountrySpecificPartConversion => 14400m;
		protected virtual ZDecimal ExpectedVolumeOfTestCalculateCountrySpecificQuantity => 2400m;

		protected class TestConverter : BaseCustomsQuantityConverter
		{
			public TestConverter(BaseJobComInvoiceLine invoiceLine, ZPropertyInfoDecimal customsQuantityInfo, ZPropertyInfoString customsUnitOfQuantityInfo)
				: base(invoiceLine, customsQuantityInfo, customsUnitOfQuantityInfo)
			{
			}

			protected override bool IsPartSpecificConversion
			{
				get { return InvoiceLine.JI_PartNo == "XXX123" && InvoiceLine.JI_CustomsUnitQty == "BOT"; }
			}

			protected override ZDecimal CalculatePartSpecificConversionFactor()
			{
				return UnitConverter.ConversionFactor(InvoiceLine.JI_InvoiceUQ, InvoiceLine.JI_CustomsUnitQty) / 2;
			}

			protected override void CalculateCountrySpecificQuantity()
			{
				InvoiceLine.JI_Volume = InvoiceLine.JI_CustomsQuantity;
			}
		}

		#region Implementation

		BaseJobDeclaration jobDeclaration;
		BaseJobComInvoiceHeader invoiceHeader;
		BaseJobComInvoiceLine invoiceLine;
		MasterFiles.Business.OrgSupplierPart product;
		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = BaseJobDeclaration.New(Factory);
			invoiceHeader = jobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_JE = jobDeclaration.PK;
			invoiceHeader.JZ_OH_Supplier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			product = MasterFiles.Business.OrgSupplierPart.New(Factory);
			product.OP_PartNum = "XXX123";

			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = invoiceHeader.JZ_OH_Supplier;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			Factory.Save();
		}

		#endregion
	}
}
