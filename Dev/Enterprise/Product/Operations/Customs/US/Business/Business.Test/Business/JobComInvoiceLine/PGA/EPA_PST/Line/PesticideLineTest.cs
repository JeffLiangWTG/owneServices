using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PesticideLine))]
	public class PesticideLineTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<PesticideLine>
	{
		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return PesticideLine;
		}

		#endregion

		#region Implementation

		protected override IEnumerable<PesticideLine> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var pesticide = invoiceLine.PSTLines.AddNew();
			yield return pesticide.PesticideLines.AddNew();
		}

		PesticideLine PesticideLine
		{
			get
			{
				if (pesticideLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					var pesticide = invoiceLine.PSTLines.AddNew();
					pesticideLine = pesticide.PesticideLines.AddNew();
				}
				return pesticideLine;
			}
		}
		PesticideLine pesticideLine;

		#endregion
	}
}
