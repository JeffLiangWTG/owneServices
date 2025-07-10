using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ACEDrawbackAcknowledgeAndSign))]
	public class ACEDrawbackAcknowledgeAndSignTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ACEDrawbackAcknowledgeAndSign(Declaration, UpdateActionCode.Add);
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					invoiceLine.JI_Description = "testing";
				}

				return declaration;
			}
		}
		JobDeclaration declaration;
	}
}
