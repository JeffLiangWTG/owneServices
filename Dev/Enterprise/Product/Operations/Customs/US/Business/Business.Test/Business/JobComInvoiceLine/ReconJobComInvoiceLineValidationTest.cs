using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconJobComInvoiceLineValidationTest : TestCaseWithFactory
	{
		public void TestCheckJI_CustomsSecondQuantity()
		{
			ReconInvoiceLine.JI_Tariff = Tariff.UE_Tariff;

			ReconInvoiceLine.JI_CustomsSecondQuantity = 0m;
			ReconInvoiceLine.Validation.ValidateJI_CustomsSecondQuantity();
			AssertHasWarning(ReconInvoiceLine.JI_CustomsSecondQuantityInfo, ValidationConstants.WarnIfStatQTYisZero);

			ReconInvoiceLine.JI_CustomsSecondQuantity = 1m;
			AssertNoWarning(ReconInvoiceLine.JI_CustomsSecondQuantityInfo, ValidationConstants.WarnIfStatQTYisZero);
		}

		public void TestCheckJI_CustomsThirdQuantity()
		{
			ReconInvoiceLine.JI_Tariff = Tariff.UE_Tariff;

			ReconInvoiceLine.JI_CustomsThirdQuantity = 0m;
			ReconInvoiceLine.Validation.ValidateJI_CustomsThirdQuantity();
			AssertHasWarning(ReconInvoiceLine.JI_CustomsThirdQuantityInfo, ValidationConstants.WarnIfStatQTYisZero);

			ReconInvoiceLine.JI_CustomsThirdQuantity = 1m;
			AssertNoWarning(ReconInvoiceLine.JI_CustomsThirdQuantityInfo, ValidationConstants.WarnIfStatQTYisZero);
		}

		public void TestCheckJI_Tariff()
		{
			Validation.ValidateJI_Tariff();
			AssertHasMessageError(ReconInvoiceLine.JI_TariffInfo, "Tariff may not be empty");

			//just checking that validation is working
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "98000000";
			tariff.UE_DateFrom = new ZDate(2000, 1, 1);
			tariff.UE_DateTo = new ZDate(2000, 12, 31);

			ReconInvoiceLine.JI_Tariff = tariff.UE_Tariff;
			Assert("Tariff Validation is tested in InvoiceLine, this is just to make sure it is hooked up", ReconInvoiceLine.JI_TariffInfo.HasMessageErrors());
		}

		public void TestJI_TariffWith98_99()
		{
			ReconInvoiceLine.JI_Tariff = "98010040";
			AssertNoMessageError(ReconInvoiceLine.JI_TariffInfo, TariffValidator.TariffNumber99ShouldNotBeEnteredHere);

			ReconInvoiceLine.JI_Tariff = "9810001000";
			AssertNoMessageError(ReconInvoiceLine.JI_TariffInfo, TariffValidator.TariffNumber99ShouldNotBeEnteredHere);

			ReconInvoiceLine.JI_Tariff = USCTariff.CAFTABenefitsApplicable;
			AssertHasMessageError(ReconInvoiceLine.JI_TariffInfo, TariffValidator.TariffNumber99ShouldNotBeEnteredHere);

			ReconInvoiceLine.JI_Tariff = "9802008068";
			AssertNoMessageError(ReconInvoiceLine.JI_TariffInfo, TariffValidator.TariffNumber99ShouldNotBeEnteredHere);

			ReconInvoiceLine.JI_Tariff = "6211330054";
			AssertNoMessageError(ReconInvoiceLine.JI_TariffInfo, TariffValidator.TariffNumber99ShouldNotBeEnteredHere);
		}

		public void TestCheckJI_CustomsQuantity()
		{
			ReconInvoiceLine.JI_CustomsQuantity = 123.5678m;
			ReconInvoiceLine.US_R_OrigFirstQty = 123.5678m;
			ReconInvoiceLine.Validation.ValidateJI_CustomsQuantity();
			AssertNoMessageError("Should be the same as OrigianlCustomsQuantity", ReconInvoiceLine.JI_CustomsQuantityInfo, ValidationConstants.ShouldTheSameAsOriginalCustomsQuantity);

			ReconInvoiceLine.JI_CustomsQuantity = 123.56m;
			ReconInvoiceLine.US_R_OrigFirstQty = 123.5678m;
			ReconInvoiceLine.Validation.ValidateJI_CustomsQuantity();
			AssertHasMessageError("Should be the same as OrigianlCustomsQuantity", ReconInvoiceLine.JI_CustomsQuantityInfo, ValidationConstants.ShouldTheSameAsOriginalCustomsQuantity);

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000111122";
			tariff.UE_Unit1 = "KG";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now;
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.CompoundSpecificAndAdValoremFirstQuantity;

			ReconInvoiceLine.JI_Tariff = tariff.UE_Tariff;
			Validation.ValidateJI_CustomsQuantity();
			Assert("Tariff Validation is tested in InvoiceLine, this is just to make sure it is hooked up", ReconInvoiceLine.JI_CustomsQuantityInfo.HasNotifications());
		}

		public void TestRunReconRelatedValidationsOnly()
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

			ReconInvoiceLine.ClearAllNotifications();
			Recon.JE_OH_Importer = organisation.PK;
			ReconInvoiceLine.JI_Tariff = "98090010";
			ReconInvoiceLine.JI_CustomsUnitQty = "KG";
			ReconInvoiceLine.JI_CustomsSecondQuantity = 10;
			ReconInvoiceLine.US_R_OrigFirstQty = 10;
			ReconInvoiceLine.JI_CustomsQuantity = 10;
			Validation.ValidateAll();
			AssertEquals("No message errors expected. There is a validation that is not applicable to Recon", false, ReconInvoiceLine.HasMessageErrors);
			AssertEquals("No errors expected. There is a validation that is not applicable to Recon", false, ReconInvoiceLine.HasErrors);
			AssertEquals("No warnings expected. There is a validation that is not applicable to Recon", false, ReconInvoiceLine.HasWarnings);
		}

		public void TestReconTariffForCombinedLines()
		{
			var errorText = "Tariff may not be empty";
			ReconInvoiceLine.JI_Tariff = ZString.Empty;
			AssertHasMessageErrorContaining(ReconInvoiceLine.JI_TariffInfo, errorText);

			ReconInvoiceLine.US_SupTariff = "99038817";
			ReconInvoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(ReconInvoiceLine.JI_TariffInfo, errorText);

			ReconInvoiceLine.JI_Tariff = "8541406015";
			AssertNoMessageErrorContaining(ReconInvoiceLine.JI_TariffInfo, errorText);

			ReconInvoiceLine.JI_Tariff = ZString.Empty;
			var reconChildLine = ReconInvoice.JobComInvoiceLines.AddNew();
			reconChildLine.JI_ParentID = ReconInvoiceLine.PK;
			reconChildLine.US_SupTariff = "99034525";
			ReconInvoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(ReconInvoiceLine.JI_TariffInfo, errorText);

			reconChildLine.JI_Tariff = "8541406015";
			ReconInvoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining(ReconInvoiceLine.JI_TariffInfo, errorText);

			reconChildLine.JI_Tariff = ZString.Empty;
			ReconInvoiceLine.US_SupTariff = "9802005060";
			ReconInvoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(ReconInvoiceLine.JI_TariffInfo, errorText);

			reconChildLine.JI_Tariff = "8541406015";
			reconChildLine.US_SupTariff = "99034525";
			ReconInvoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining(ReconInvoiceLine.JI_TariffInfo, errorText);
		}

		public void TestValidateMaximumNumberOfEntryLines()
		{
			for (int i = 0; i < 9999; i++)
			{
				ReconInvoice.InvoiceLines.AddNew();
			}

			var lastLine = ReconInvoice.InvoiceLines.AddNew();
			var firstLine = ReconInvoice.InvoiceLines[0];
			firstLine.Validation.ValidateAll();
			AssertHasRowMessageError(firstLine, ReconJobComInvoiceLineValidation.MaximumNumberOfEntryLinesExceeded);

			ReconInvoice.InvoiceLines.Remove(lastLine);
			lastLine.Delete();
			firstLine.Validation.ValidateAll();
			AssertNoRowMessageError(firstLine, ReconJobComInvoiceLineValidation.MaximumNumberOfEntryLinesExceeded);
		}

		ReconJobComInvoiceLineValidation Validation => (ReconJobComInvoiceLineValidation)ReconInvoiceLine.Validation;

		ReconDeclaration recon;
		ReconDeclaration Recon
		{
			get
			{
				if (recon == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					recon = new ReconDeclaration(declaration);
				}

				return recon;
			}
		}

		JobComInvoiceHeader reconInvoice;
		JobComInvoiceHeader ReconInvoice
		{
			get
			{
				if (reconInvoice == null)
				{
					reconInvoice = Recon.Invoices.AddNew();
					reconInvoice.US_CH_ReconEntry = Recon.OriginalEntries.AddNew().CH_PK;
					reconInvoice.ReconOriginalEntry.US_R_DutyRateDate = ZDateTime.Now;
				}

				return reconInvoice;
			}
		}

		JobComInvoiceLine reconInvoiceLine;
		JobComInvoiceLine ReconInvoiceLine => reconInvoiceLine ?? (reconInvoiceLine = ReconInvoice.InvoiceLines.AddNew());

		USCTariff Tariff
		{
			get
			{
				if (tariff == null)
				{
					tariff = Factory.New<USCTariff>();
					tariff.UE_Tariff = "00000000";
					tariff.UE_Unit1 = "LTR";
					tariff.UE_Unit2 = "LTR";
					tariff.UE_Unit3 = "LTR";
					tariff.UE_DateFrom = new ZDateTime(2008, 9, 11);
					tariff.UE_DateTo = ZDateTime.Now;
				}

				return tariff;
			}
		}
		USCTariff tariff;
	}
}
