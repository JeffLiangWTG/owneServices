using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(GovernmentUniformInvoiceCollection))]
	sealed class GovernmentUniformInvoiceCollectionTest : CusCodeDataCollectionTest<GovernmentUniformInvoiceData>
	{
		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		protected override Customs.Business.CusCodeDataCollection<GovernmentUniformInvoiceData> GetCusCodeDataCollection()
		{
			return Declaration.GovernmentUniformInvoices;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			GovernmentUniformInvoiceData result = Factory.New<GovernmentUniformInvoiceData>();
			result.CY_ParentID = Declaration.PK;
			result.CY_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			return result;
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
	}
}
