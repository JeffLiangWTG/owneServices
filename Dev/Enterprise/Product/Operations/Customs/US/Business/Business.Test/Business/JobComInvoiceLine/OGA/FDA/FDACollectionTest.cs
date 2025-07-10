using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FDACollection))]
	public class FDACollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaults()
		{
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			InvoiceLine.JI_Description = "TEST".PadRight(100, '1');
			InvoiceLine.JI_LinePrice = 20m;

			OrgHeader manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_Code = "ZXZCXC";
			manufacturer.OH_RL_NKClosestPort = "ZACPT";
			OrgCusCode cusCode = manufacturer.MainAddress.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.FoodFacilityRegistrationNumber;
			cusCode.OK_CustomsRegNo = "1234567890";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			OrgCusCode sFRCusCode = manufacturer.MainAddress.CustomsCodes.AddNew();
			sFRCusCode.OK_CodeType = OrgCusCode.USACodeTypes.ShipperRegistrationNumber;
			sFRCusCode.OK_CustomsRegNo = "25638524652";
			sFRCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			manufacturer.CustomsCodes.Add(sFRCusCode);

			OrgHeaderWrapper manufacturerWrapped = OrgHeaderWrapper.New(manufacturer);
			manufacturerWrapped.ZO_MFRRegExempt = "A";
			manufacturerWrapped.ZO_ProducerFirmType = ProducerFirmTypeList.Codes.G;
			Factory.Save();

			InvoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			InvoiceHeader.JZ_OA_SupplierAddress = manufacturer.MainAddress.PK;
			FDA fda = FDAs.AddNew();
			AssertEquals(InvoiceLine.JI_Description.Left(fda.US_FDACommercialDescInfo.MaxLength), fda.US_FDACommercialDesc);
			AssertEquals(InvoiceLine.JI_LinePrice, fda.US_InvCurrFDAValue);

			AssertEquals("ZA", fda.US_UC_NKFDAProduction);
			AssertEquals("A", fda.US_FME);
			AssertEquals(ProducerFirmTypeList.Codes.G, fda.US_PFT);
			AssertEquals("1234567890", fda.US_PFR);

			AssertEquals("25638524652", fda.US_SFR);
		}

		public void TestCopyPreviousLine()
		{
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			InvoiceLine.JI_Description = "TEST";
			InvoiceLine.JI_LinePrice = 5000m;

			OrgHeader manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_Code = "ZXZCXC";
			manufacturer.OH_RL_NKClosestPort = "ZACPT";
			OrgCusCode cusCode = manufacturer.MainAddress.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.FoodFacilityRegistrationNumber;
			cusCode.OK_CustomsRegNo = "1234567890";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			OrgCusCode sFRCusCode = manufacturer.MainAddress.CustomsCodes.AddNew();
			sFRCusCode.OK_CodeType = OrgCusCode.USACodeTypes.ShipperRegistrationNumber;
			sFRCusCode.OK_CustomsRegNo = "25638524652";
			sFRCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			manufacturer.CustomsCodes.Add(sFRCusCode);

			OrgHeaderWrapper manufacturerWrapped = OrgHeaderWrapper.New(manufacturer);
			manufacturerWrapped.ZO_MFRRegExempt = "A";
			manufacturerWrapped.ZO_ProducerFirmType = ProducerFirmTypeList.Codes.G;
			Factory.Save();

			InvoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			InvoiceHeader.JZ_OA_SupplierAddress = manufacturer.MainAddress.PK;
			FDA fda = FDAs.AddNew();

			AssertEquals(InvoiceLine.JI_Description, fda.US_FDACommercialDesc);
			AssertEquals(InvoiceLine.JI_LinePrice, fda.US_InvCurrFDAValue);
			AssertEquals("ZA", fda.US_UC_NKFDAProduction);
			AssertEquals("A", fda.US_FME);
			AssertEquals(ProducerFirmTypeList.Codes.G, fda.US_PFT);
			AssertEquals("1234567890", fda.US_PFR);
			AssertEquals("25638524652", fda.US_SFR);

			fda.US_ContainerDim1 = 0.2m;
			fda.US_ContainerDim2 = 0.5m;
			fda.US_CSH = "XX";
			fda.US_DimUQ = FDAMeasurementUnitList.Codes.InchesWithOneSixteenthDecimals;
			fda.US_FDAQty1 = 100;
			fda.US_FDAQty2 = 50;
			fda.US_InvCurrFDAValue = 3000m;
			fda.US_TradeBrandName = "HEINZ";
			fda.US_UC_NKFDAProduction = "MX";

			invoiceLine.Declaration.CopyLastFDADetailsToNewLine = true;
			FDAs.AddNew();

			AssertEquals(2, FDAs.Count);
			FDA copiedFDA = FDAs[1];
			AssertEquals("XX", copiedFDA.US_CSH);
			AssertEquals("MX", copiedFDA.US_UC_NKFDAProduction);
			AssertEquals("HEINZ", copiedFDA.US_TradeBrandName);
			AssertEquals("FDA value should calculate value remaining from Invoice Line value", 2000m, copiedFDA.US_InvCurrFDAValue);
			AssertEquals(0.2m, copiedFDA.US_ContainerDim1);
			AssertEquals(0.5m, copiedFDA.US_ContainerDim2);

			copiedFDA.US_InvCurrFDAValue = 1750;
			copiedFDA.US_TradeBrandName = "SPC";
			copiedFDA.US_UC_NKFDAProduction = "NZ";
			copiedFDA.US_FDACommercialDesc = "BAKED BEANS";

			FDAs.AddNew();

			AssertEquals(3, FDAs.Count);
			FDA line3FDA = FDAs[2];
			AssertEquals("XX", line3FDA.US_CSH);
			AssertEquals("NZ", line3FDA.US_UC_NKFDAProduction);
			AssertEquals("SPC", line3FDA.US_TradeBrandName);
			AssertEquals("BAKED BEANS", line3FDA.US_FDACommercialDesc);
			AssertEquals("FDA value should calculate value remaining from Invoice Line value", 250m, line3FDA.US_InvCurrFDAValue);

			line3FDA.US_TradeBrandName = "No FDA Value remains";

			FDAs.AddNew();

			AssertEquals(4, FDAs.Count);
			FDA line4FDA = FDAs[3];
			AssertEquals("XX", line4FDA.US_CSH);
			AssertEquals("NZ", line4FDA.US_UC_NKFDAProduction);
			AssertEquals("No FDA Value remains", line4FDA.US_TradeBrandName);
			AssertEquals("FDA value should calculate zero when no value is remaining from Invoice Line value", 0m, line4FDA.US_InvCurrFDAValue);
		}

		public void TestOnAdded()
		{
			InvoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewRequiredTariff;
			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertHasMessageError(InvoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDARequiredButDisclaimed);
			InvoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			FDA fda = FDAs.AddNew();
			AssertNoMessageError(InvoiceLine.US_FDAIndicatorInfo, CommonImportAddInfoJobComInvoiceLineValidation.Constants.FDA.FDARequiredButDisclaimed);
		}

		public void TestFDAValueIsRemainingValue()
		{
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			InvoiceLine.JI_Description = "TEST";
			InvoiceLine.JI_LinePrice = 5000m;

			FDA fdaLine1 = FDAs.AddNew();
			AssertEquals(InvoiceLine.JI_LinePrice, fdaLine1.US_InvCurrFDAValue);
			fdaLine1.US_InvCurrFDAValue = 1500m;

			FDA fdaLine2 = FDAs.AddNew();
			AssertEquals(3500m, fdaLine2.US_InvCurrFDAValue);
			fdaLine2.US_InvCurrFDAValue = 2900m;

			FDA fdaLine3 = FDAs.AddNew();
			AssertEquals(600m, fdaLine3.US_InvCurrFDAValue);
			fdaLine3.US_FDACommercialDesc = "Total customs value used now";

			FDA fdaLine4 = FDAs.AddNew();
			AssertEquals("Any new lines should now set value to zero", 0m, fdaLine4.US_InvCurrFDAValue);
		}

		public void TestDefaultInvValueWithApportionment()
		{
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			InvoiceLine.JI_Description = "TEST";
			InvoiceLine.JI_LinePrice = 5000m;

			var fdaLine1 = FDAs.AddNew();
			AssertEquals(InvoiceLine.JI_LinePrice, fdaLine1.US_InvCurrFDAValue);
			fdaLine1.US_InvCurrFDAValue = 1500m;

			InvoiceLine.Declaration.ResumeApportionment();

			var nonCommitted = (FDA)((System.ComponentModel.IBindingList)InvoiceLine.FDAs).AddNew();
			AssertEquals("Defaulted the remaining value", 3500m, nonCommitted.US_InvCurrFDAValue);
			AssertEquals("Declaration should not be marked as dirty", false, InvoiceLine.Declaration.ApportionmentDirty);

			((System.ComponentModel.ICancelAddNew)InvoiceLine.FDAs).CancelNew(1);
			AssertEquals("Declaration should not be marked as dirty", false, InvoiceLine.Declaration.ApportionmentDirty);

			nonCommitted = (FDA)((System.ComponentModel.IBindingList)InvoiceLine.FDAs).AddNew();
			((System.ComponentModel.ICancelAddNew)InvoiceLine.FDAs).EndNew(1);
			AssertEquals("Declaration should be marked as dirty now", true, InvoiceLine.Declaration.ApportionmentDirty);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return FDAs;
		}

		FDACollection FDAs
		{
			get { return fdas ?? (fdas = InvoiceLine.FDAs); }
		}
		FDACollection fdas;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;

		JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				if (invoiceHeader == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableENS = true;
					declaration.US_EnableCRL = true;
					invoiceHeader = declaration.Invoices.AddNew();
				}

				return invoiceHeader;
			}
		}
		JobComInvoiceHeader invoiceHeader;

		#endregion
	}
}
