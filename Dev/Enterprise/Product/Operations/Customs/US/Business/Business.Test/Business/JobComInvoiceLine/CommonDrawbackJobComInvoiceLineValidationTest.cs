using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class CommonDrawbackJobComInvoiceLineValidationTest : JobComInvoiceLineValidationTest
	{
		public void TestRunDrawbackRelatedValidationsOnly()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();

			var customAttrib1 = organisation.CustomLabels.AddNew();
			customAttrib1.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute1;
			customAttrib1.OT_IsMandatory = true;
			var customAttrib2 = organisation.CustomLabels.AddNew();
			customAttrib2.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute2;
			customAttrib2.OT_IsMandatory = true;
			var customAttrib3 = organisation.CustomLabels.AddNew();
			customAttrib3.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute3;
			customAttrib3.OT_IsMandatory = true;
			var customAttrib4 = organisation.CustomLabels.AddNew();
			customAttrib4.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute4;
			customAttrib4.OT_IsMandatory = true;
			var customAttrib5 = organisation.CustomLabels.AddNew();
			customAttrib5.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute5;
			customAttrib5.OT_IsMandatory = true;
			var customAttrib6 = organisation.CustomLabels.AddNew();
			customAttrib6.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute6;
			customAttrib6.OT_IsMandatory = true;
			var customTextBlob1 = organisation.CustomLabels.AddNew();
			customTextBlob1.OT_FieldName = "ComInvoiceLine.CustomTextBlob1";
			customTextBlob1.OT_IsMandatory = true;
			var customFlag1 = organisation.CustomLabels.AddNew();
			customFlag1.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomFlag1;
			customFlag1.OT_IsMandatory = true;
			var customFlag2 = organisation.CustomLabels.AddNew();
			customFlag2.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomFlag2;
			customFlag2.OT_IsMandatory = true;
			var customFlag3 = organisation.CustomLabels.AddNew();
			customFlag3.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomFlag3;
			customFlag3.OT_IsMandatory = true;
			var customFlag4 = organisation.CustomLabels.AddNew();
			customFlag4.OT_FieldName = "ComInvoiceLine.CustomFlag4";
			customFlag4.OT_IsMandatory = true;
			var customFlag5 = organisation.CustomLabels.AddNew();
			customFlag5.OT_FieldName = "ComInvoiceLine.CustomFlag5";
			customFlag5.OT_IsMandatory = true;
			var customDate1 = organisation.CustomLabels.AddNew();
			customDate1.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomDate1;
			customDate1.OT_IsMandatory = true;
			var customDate2 = organisation.CustomLabels.AddNew();
			customDate2.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomDate2;
			customDate2.OT_IsMandatory = true;
			var customDate3 = organisation.CustomLabels.AddNew();
			customDate3.OT_FieldName = Enterprise.Core.Constants.CustomLabels.ComInvoiceLine.CustomDate3;
			customDate3.OT_IsMandatory = true;

			declaration.JE_OH_Importer = organisation.PK;

			InvoiceLine.JI_Tariff = "6211330054";
			InvoiceLine.JI_CustomsQuantity = 10;
			InvoiceLine.JI_CustomsUnitQty = "KG";
			InvoiceLine.JI_CustomsSecondQuantity = 10;
			((CommonDrawbackJobComInvoiceLineValidation)InvoiceLine.Validation).ValidateAll();
			AssertEquals("No message errors expected. There is a validation that is not applicable to Recon", false, InvoiceLine.HasMessageErrors);
			AssertEquals("No errors expected. There is a validation that is not applicable to Recon", false, InvoiceLine.HasErrors);
			AssertEquals("No warnings expected. There is a validation that is not applicable to Recon", false, InvoiceLine.HasWarnings);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
		}
		protected JobDeclaration declaration;

		JobComInvoiceLine invoiceLine;
		protected JobComInvoiceLine InvoiceLine
		{
			get
			{
				var invoice = declaration.Invoices.AddNew();
				return invoiceLine ?? (invoiceLine = invoice.JobComInvoiceLines.AddNew());
			}
		}
	}
}
