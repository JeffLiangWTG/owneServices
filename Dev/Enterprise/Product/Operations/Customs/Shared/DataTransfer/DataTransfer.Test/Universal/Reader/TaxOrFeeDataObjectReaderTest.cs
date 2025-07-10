using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestBasicTaxOrFeeLevelFieldMappings()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = (BaseJobDeclaration)Factory.BOFactory.New<Integration.Customs.GB.IJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				var tax1 = CreateTaxOrFee("A00", "F");
				var tax2 = CreateTaxOrFee("A00", "D");
				var tax3 = CreateTaxOrFee("B00", "G");
				var invoiceLineDataObject = new UniversalCustoms.CommercialInvoiceLine()
				{
					TaxOrFeeCollection = new List<UniversalCustoms.TaxOrFee>(new[] { tax1, tax2, tax3 })
				};
				var taxLines = new TaxOrFeeCollectionDataObjectReader(logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.CountryCodes.UnitedKingdom))
									.ReadIntoDataRows(invoiceLine.PK, invoiceLine.TablePrefix, invoiceLine.IsInDatabase, invoiceLineDataObject);

				AssertNotNull(taxLines);

				#region Check Contents of Business Object

				CombineAssertions(delegate
				{
					AssertEquals("Tax lines", 3, taxLines.Length);
					var row1 = taxLines[0][JobComInvoiceLineTaxSchema.Constants.JLT_Type].ToString() + taxLines[0][JobComInvoiceLineTaxSchema.Constants.JLT_MethodOfPayment].ToString();
					var row2 = taxLines[1][JobComInvoiceLineTaxSchema.Constants.JLT_Type].ToString() + taxLines[1][JobComInvoiceLineTaxSchema.Constants.JLT_MethodOfPayment].ToString();
					var row3 = taxLines[2][JobComInvoiceLineTaxSchema.Constants.JLT_Type].ToString() + taxLines[2][JobComInvoiceLineTaxSchema.Constants.JLT_MethodOfPayment].ToString();
					Assert(
							("A00F" == row1 && "A00D" == row2 && "B00G" == row3) ||
							("A00F" == row1 && "A00D" == row3 && "B00G" == row2) ||
							("A00F" == row2 && "A00D" == row1 && "B00G" == row3) ||
							("A00F" == row2 && "A00D" == row3 && "B00G" == row1) ||
							("A00F" == row3 && "A00D" == row1 && "B00G" == row2) ||
							("A00F" == row3 && "A00D" == row2 && "B00G" == row1)
							);
					foreach (var t in taxLines)
					{
						AssertEquals(123.45m, t.GetValue(JobComInvoiceLineTaxSchema.JLT_Amount));
						AssertEquals(1234.56m, t.GetValue(JobComInvoiceLineTaxSchema.JLT_BaseValue));
						AssertEquals(456m, t.GetValue(JobComInvoiceLineTaxSchema.JLT_BaseQuantity));
						AssertEquals("ABCD", t.GetValue(JobComInvoiceLineTaxSchema.JLT_MethodOfCalculation));
						AssertEquals("X", t.GetValue(JobComInvoiceLineTaxSchema.JLT_RateOverrideReasonCode));
						AssertEquals(200m, t.GetValue(JobComInvoiceLineTaxSchema.JLT_Rate));
					}
				});

				#endregion
			}
		}

		internal static UniversalCustoms.TaxOrFee CreateTaxOrFee(string feeCode, string mop)
		{
			return new UniversalCustoms.TaxOrFee()
			{
				Type = new CodeDescriptionPair6Char() { Code = feeCode, Description = "Anything" },
				Amount = 123.45m,
				BaseValue = 1234.56m,
				BaseQuantity = 456m,
				MethodOfCalculation = new CodeDescriptionPair4Char() { Code = "ABCD" },
				MethodOfPayment = new CodeDescriptionPair() { Code = mop },
				RateReasonOverride = new CodeDescriptionPair() { Code = "X" },
				Rate = 200m
			};
		}
	}
}
