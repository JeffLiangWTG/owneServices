using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngine.Web;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.Tracking.Business
{
	public class StatementService : IStatementService
	{
		public IWebTrackerPrintResult Print(Guid contactPK, Guid companyPK)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = nameof(StatementService) };

			var contact = factory.Load<OrgContact>(new ZGuid(contactPK));

			if (contact == null)
			{
				return null;
			}

			var subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_GC);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_OH, contact.ParentOrg.PK);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, true);

			var filter = new ZDBOnlyQuery(typeof(GlbCompany));
			filter.AddToFilter(GlbCompanySchema.GC_IsActive, true);
			filter.AddToFilter(GlbCompanySchema.PK, companyPK);
			filter.AddSubQuery(GlbCompanySchema.PK, subQuery, JoinCondition.And);

			var company = factory.LoadTop1<GlbCompany>(filter);
			if (company == null)
			{
				return null;
			}

			var controllingBranch = contact.ParentOrg?.CompanyData?.ControllingBranch;
			var branch = controllingBranch != null && controllingBranch.GB_GC == company.PK ? controllingBranch : company.FirstActiveBranch;

			using (var statementBranch = new WebLoginBranch(branch))
			{
				var printableStatement = Statement.New(branch);
				printableStatement.OH_PK = contact.OC_OH;
				printableStatement.CreditStatements = Statement.CreditOptions.AllDocuments;
				var task = printableStatement.GetPrintTask();
				if (task == null || task.Count == 0)
				{
					return new StatementPrintResult
					{
						ErrorMessage = ResString.GetMultilingualString("aafc6bf3-d531-4f5f-af30-03b93ec18f8c", "Cannot generate a statement because there are no transactions issued to your organization.")
					};
				}

				var document = new DocumentUtility(factory).GetDocument(task[0], contact, DataContentTypes.Pdf);

				return new StatementPrintResult
				{
					FileContents = new MemoryStream(document),
					FileName = "StatementOfAccount.pdf",
				};
			}
		}
	}
}
