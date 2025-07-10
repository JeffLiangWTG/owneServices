using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(RegoNumberCollection))]
	sealed class RegoNumberCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<RegoNumber>
	{
		protected override Customs.Business.CusCodeDataCollection<RegoNumber> GetCusCodeDataCollection() => new RegoNumberCollection(AIILine);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			RegoNumber result = Factory.New<RegoNumber>();
			result.CY_ParentID = AIILine.PK;
			result.CY_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			return result;
		}

		AIILine aiiLine;
		AIILine AIILine
		{
			get
			{
				if (aiiLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
					declaration.US_EnableAII = true;
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					aiiLine = invoiceLine.FirstAIILine;
				}

				return aiiLine;
			}
		}
	}
}
