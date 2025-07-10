using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class PowerOfAttorneryValidationTest : TestCaseWithFactory
	{
		bool ExtraMatch(JobRequiredDocument doc)
		{
			return false;
		}

		public void TestConstructor()
		{
			var poaValidator = new PowerOfAttorneyValidator("");
			AssertEquals("Power of Attorney", poaValidator.CountrySpecificNameForPOA);
		}

		public void TestValidatePOADates()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			using (declaration.SuspendValidationTesting())
			{
				OrgHeader importer = Factory.New<OrgHeader>();
				declaration.JE_OH_Importer = importer.PK;
				CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
				var poaValidator = new PowerOfAttorneyValidator("");
				poaValidator.ValidatePOADates(declaration, importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF", ExtraMatch);
				AssertNoWarnings(declaration.JE_OH_ImporterInfo);

				JobRequiredDocument poaDocument = importer.RequiredDocuments.AddNew("POA");
				poaDocument.EQ_DocType = "POA";
				poaDocument.EQ_DocDescription = "Power of Attorney";
				poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
				poaDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
				poaDocument.EQ_ValidToDate = ZDateTime.Empty;

				poaValidator.ValidatePOADates(declaration, importer, declaration.JE_OH_ImporterInfo, "POA", "POC", "POF", ExtraMatch);
				AssertHasWarnings(declaration.JE_OH_ImporterInfo);
			}
		}
	}
}
