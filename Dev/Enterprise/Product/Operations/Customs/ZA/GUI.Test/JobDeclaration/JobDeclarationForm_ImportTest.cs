using System;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class JobDeclarationForm_ImportTest : JobDeclarationFormAbstractTest
	{
		public void TestDoNotMergeOnFormSaving()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = declaration.InvoiceLines.AddNew();
				declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew().Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 100m);
				Factory.Save();
				using (JobDeclarationForm form = new JobDeclarationForm(declaration))
				{
					invoiceLine.JI_LinePrice = 100m;
					form.FireSaveButton();
					AssertEquals("Duty amount that is fed by third party software should have kept: JobDeclaration.ShouldMergeOnSaving should be false", 100m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
				}
			}
		}

		public override ZString MessageTypeForFormBashing => ZAJobMessageTypeList.Codes.Import;
	}
}
