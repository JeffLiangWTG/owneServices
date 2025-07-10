using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineReservedField))]
	sealed class JobComInvoiceLineReservedFieldTest : ReservedFieldTest<JobComInvoiceLineReservedField>
	{
		[ExpectNoExceptions]
		public override void TestSetDefaultValues()
		{
			base.TestSetDefaultValues();
			var reservedField = (ReservedField)GetNewBusinessObject();
			NUnit.Framework.Assert.That(reservedField.CY_ParentTableCode, NUnit.Framework.Is.EqualTo(JobComInvoiceLineSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestParent()
		{
			NUnit.Framework.Assert.That(reservedField.Parent, NUnit.Framework.Is.EqualTo(invoiceLine).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRefreshBindingCalledWhenSetCY_Code()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var reservedFieldCode1InfoRefreshed = false;
			line.ReservedFieldCode1Info.ValueChanged += (e, s) => { reservedFieldCode1InfoRefreshed = true; };
			var reservedFieldCode2InfoRefreshed = false;
			line.ReservedFieldCode2Info.ValueChanged += (e, s) => { reservedFieldCode2InfoRefreshed = true; };

			var field1 = line.ReservedFields.AddNew();
			field1.CY_Code = "1";
			var field2 = line.ReservedFields.AddNew();
			field2.CY_Code = "2";

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(reservedFieldCode1InfoRefreshed, NUnit.Framework.Is.True, "reservedFieldCode1InfoRefreshed");
				NUnit.Framework.Assert.That(reservedFieldCode2InfoRefreshed, NUnit.Framework.Is.True, "reservedFieldCode2InfoRefreshed");
			});
		}

		[ExpectNoExceptions]
		public void TestRefreshBindingCalledWhenSetCY_Dat()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var reservedFieldValue1InfoRefreshed = false;
			line.ReservedFieldValue1Info.ValueChanged += (e, s) => { reservedFieldValue1InfoRefreshed = true; };
			var reservedFieldValue2InfoRefreshed = false;
			line.ReservedFieldValue2Info.ValueChanged += (e, s) => { reservedFieldValue2InfoRefreshed = true; };

			var field1 = line.ReservedFields.AddNew();
			field1.CY_Data = "A";
			var field2 = line.ReservedFields.AddNew();
			field2.CY_Data = "B";

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(reservedFieldValue1InfoRefreshed, NUnit.Framework.Is.True, "reservedFieldValue1InfoRefreshed");
				NUnit.Framework.Assert.That(reservedFieldValue2InfoRefreshed, NUnit.Framework.Is.True, "reservedFieldValue2InfoRefreshed");
			});
		}

		#region Implementation
		protected override IEnumerable<JobComInvoiceLineReservedField> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var invoiceLine = factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			yield return invoiceLine.ReservedFields.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return invoiceLine.ReservedFields.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = Factory.New<JobComInvoiceLine>();
			reservedField = invoiceLine.ReservedFields.AddNew();
		}

		JobComInvoiceLine invoiceLine;
		JobComInvoiceLineReservedField reservedField;
		#endregion
	}
}
