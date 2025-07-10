using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USPGAAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_CertifyingIndividual()
		{
			PGA.US_CertifyingIndividual = ZString.Empty;
			AssertHasMessageErrorContaining(PGA.US_CertifyingIndividualInfo, MandatoryValidation.YouHaveNotEntered);

			PGA.US_CertifyingIndividual = PartyTypeList.Codes.Shipper;
			AssertHasMessageErrorContaining(PGA.US_CertifyingIndividualInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_PGAContactName()
		{
			PGA.AddInfoValidation.ValidateUS_PGAContactName();
			AssertHasMessageErrorContaining(PGA.US_PGAContactNameInfo, MandatoryValidation.YouHaveNotEntered);
			PGA.US_PGAContactName = "TEST NAME";
			AssertNoMessageErrorContaining(PGA.US_PGAContactNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_PGAContactPhoneNo()
		{
			PGA.AddInfoValidation.ValidateUS_PGAContactPhoneNo();
			AssertHasMessageErrorContaining(PGA.US_PGAContactPhoneNoInfo, MandatoryValidation.YouHaveNotEntered);
			PGA.US_PGAContactPhoneNo = "123";
			AssertNoMessageErrorContaining(PGA.US_PGAContactPhoneNoInfo, MandatoryValidation.YouHaveNotEntered);
			PGA.US_PGAContactPhoneNo = "2345678901";
			AssertNoMessageErrorContaining(PGA.US_PGAContactPhoneNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_PGAContactEmail()
		{
			PGA.AddInfoValidation.ValidateUS_PGAContactEmail();
			AssertHasMessageErrorContaining(PGA.US_PGAContactEmailInfo, MandatoryValidation.YouHaveNotEntered);
			PGA.US_PGAContactEmail = "~";
			AssertNoMessageErrorContaining(PGA.US_PGAContactEmailInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarningContaining(PGA.US_PGAContactEmailInfo, "Invalid email format");
		}

		public void TestCheckUS_PGACommercialDescription()
		{
			PGA.US_PGACommercialDescription = "";
			AssertHasMessageError(PGA.US_PGACommercialDescriptionInfo, USPGAAddInfoValidation.CommercialDescriptionMandatory);

			PGA.US_PGACommercialDescription = "descr";
			AssertNoMessageError(PGA.US_PGACommercialDescriptionInfo, USPGAAddInfoValidation.CommercialDescriptionMandatory);
		}

		public void TestCheckUS_PGALineValue()
		{
			PGA.InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			PGA.InvoiceLine.JI_LinePrice = 8553.60m;
			PGA.US_PGALineValue = 8555m;
			PGA.InvoiceLine.Declaration.ApportionmentDirty = false;
			PGA.AddInfoValidation.ValidateUS_PGALineValue();
			AssertHasMessageErrorContaining(PGA.US_PGALineValueInfo, USPGAAddInfoValidation.PGAValueShouldBeLessThanCustomsValue + "8554.");

			PGA.US_PGALineValue = 8554m;
			PGA.InvoiceLine.Declaration.ApportionmentDirty = false;
			PGA.AddInfoValidation.ValidateUS_PGALineValue();
			AssertNoMessageErrorContaining(PGA.US_PGALineValueInfo, USPGAAddInfoValidation.PGAValueShouldBeLessThanCustomsValue + "8554.");
		}

		public void TestCheckUS_InvCurrPGAValue()
		{
			PGA.InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			PGA.InvoiceLine.JI_LinePrice = 8553.60m;
			PGA.US_InvCurrPGAValue = 0m;
			AssertHasMessageErrorContaining(PGA.US_InvCurrPGAValueInfo, ValidationConstants.FDA.OGAInvValue);

			PGA.US_InvCurrPGAValue = 8555m;
			AssertNoMessageErrorContaining(PGA.US_InvCurrPGAValueInfo, ValidationConstants.FDA.OGAInvValue);
			AssertHasMessageErrorContaining(PGA.US_InvCurrPGAValueInfo, string.Format(USPGAAddInfoValidation.TotalInvCurrValueGreaterThanLinePrice, PGA.US_InvCurrPGAValue));
		}

		public void TestValidateThatThereAreConsitituentElements()
		{
			PGA.US_PGACommercialDescription = "descr";
			PGA.US_PGALineValue = 10m;
			PGA.AddInfoValidation.ValidateAll();
			AssertEquals(true, PGA.HasRowNotifications);

			PGA.PG04ConstituentElements.AddNew();
			PGA.AddInfoValidation.ValidateAll();
			AssertEquals(false, PGA.HasRowMessageErrors);
		}

		public void TestValidateThatThereAreContainers()
		{
			PGA.US_PGACommercialDescription = "descr";
			PGA.US_PGALineValue = 10m;
			PGA.AddInfoValidation.ValidateAll();
			AssertEquals(true, PGA.HasRowWarnings);

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "MAEUTTTT";
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("MAEUTTTT").IsForInvoiceLine = true;
			var relatedContainer = pga.ContainersForInvoiceLine.FindByContainerNumber("MAEUTTTT");
			relatedContainer.IsForPGALine = true;
			PGA.AddInfoValidation.ValidateAll();
			AssertEquals(false, PGA.HasRowWarnings);
		}

		PGA PGA
		{
			get
			{
				if (pga == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
					declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableCRL = true;
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					pga = invoiceLine.LaceyActLines.AddNew();
				}
				return pga;
			}
		}
		PGA pga;
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
	}
}
