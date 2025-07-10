using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PG04ConstituentElementCollection))]
	public class PG04ConstituentElementCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestType()
		{
			AssertEquals(typeof(ConstituentElement), PG04ConstituentElements.AddNew().GetType());
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return PG04ConstituentElements;
		}

		PG04ConstituentElementCollection PG04ConstituentElements
		{
			get { return elements ?? (elements = PGA.PG04ConstituentElements); }
		}
		PG04ConstituentElementCollection elements;

		PGA PGA
		{
			get
			{
				if (pga == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
					JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					pga = invoiceLine.LaceyActLines.AddNew();
				}

				return pga;
			}
		}
		PGA pga;

		#endregion
	}
}
