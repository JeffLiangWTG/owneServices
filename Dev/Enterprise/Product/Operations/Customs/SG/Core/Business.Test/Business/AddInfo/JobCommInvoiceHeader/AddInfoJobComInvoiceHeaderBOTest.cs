using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceHeader))]
	public class AddInfoJobComInvoiceHeaderBOTest : AddInfoBOTest
	{
		#region Overrides
		protected override Type GetExpectedLookupsType()
		{
			return typeof(AddInfoJobComInvoiceHeaderLookups);
		}

		protected override Type GetExpectedValidationType()
		{
			return typeof(AddInfoJobComInvoiceHeaderValidation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobComInvoiceHeader invoiceLine = Factory.New<JobComInvoiceHeader>();
			return new AddInfoJobComInvoiceHeader(invoiceLine.JZ_AddInfoInfo);
		}
		#endregion
	}
}
