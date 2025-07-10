using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(VNEAdditionalNumberCollection))]
	public class VNEAdditionalNumberCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<VNEAdditionalNumber>
	{
		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		protected override Customs.Business.CusCodeDataCollection<VNEAdditionalNumber> GetCusCodeDataCollection()
		{
			return AdditionalNumbers;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return AdditionalNumbers.AddNew();
		}

		VNEAdditionalNumberCollection AdditionalNumbers
		{
			get
			{
				if (additionalNumbers == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					var vne = invoiceLine.VehicleLines.AddNew();
					var detail = vne.VehicleAndEngineDetails.AddNew();
					additionalNumbers = detail.AdditionalNumbers;
				}
				return additionalNumbers;
			}
		}
		VNEAdditionalNumberCollection additionalNumbers;
	}
}
