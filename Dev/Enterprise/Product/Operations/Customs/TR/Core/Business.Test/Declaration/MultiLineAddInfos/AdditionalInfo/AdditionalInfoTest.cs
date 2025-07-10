using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(AdditionalInfo))]
	public class AdditionalInfoTest : Customs.Business.Testing.CusSupportingInfoTest<AdditionalInfo>
	{
		public void TestValidation()
		{
			AssertType<AdditionalInfoValidation>(additionalInfo.Validation);
		}

		protected override IEnumerable<AdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			yield return declaration.AdditionalInfos.AddNew();

			var invoice = declaration.Invoices.AddNew();
			yield return invoice.AdditionalInfos.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			yield return invoiceLine.AdditionalInfos.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			return declaration.AdditionalInfos.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => additionalInfo;

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			additionalInfo = declaration.AdditionalInfos.AddNew();
		}
		AdditionalInfo additionalInfo;
	}
}
