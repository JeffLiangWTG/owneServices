using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Web;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.Tracking.Business
{
	public class TrackingTransactionFilterBusinessObject : ARTransactionFilterBusinessObject
	{
		public TrackingTransactionFilterBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.AH_OH = LoggedInUser != null ? LoggedInUser.ParentOrg.PK : ZGuid.Empty;
			this.PaymentStatus = AccountingUtils.PaymentStatusTypes.Unpaid;
			if (Companies.Count > 0)
			{
				this.Company = Companies[0].PK;
			}
		}

		#region Filter

		public override ZQuery Filter
		{
			get
			{
				ZQuery result = base.Filter;
				result.IsNoResultQuery |= HasErrors || LoggedInUser == null || AH_OH.IsEmpty;

				if (LoggedInUser != null)
				{
					result.AddToFilter(AccTransactionHeaderSchema.AH_OH, SQLComparisonOperator.Equal, LoggedInUser.OC_OH);
				}
				if (FilterByCompany != null)
				{
					result.AddToFilter(AccTransactionHeaderSchema.AH_GC, FilterByCompanyPK);
				}
				else
				{
					result.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
				}

				return result;
			}
		}

		protected override bool FilterByCurrentCompany
		{
			get { return false; }
		}

		/// <summary>
		/// Helps to search for job invoice numbers as clients have no knowledge of AH_TransactionNumber and AH_ConsolidatedInvoiceRef
		/// just need to search by both fields
		/// </summary>
		protected override ZQuery GetTransactionNumberFilter(ZString transactionNumber)
		{
			ZQuery result = base.GetTransactionNumberFilter(transactionNumber);
			result.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, FilterOperator, transactionNumber);
			return result;
		}

		#endregion Filter

		#region GetStatementForSelectedCompany

		public byte[] GetStatementForSelectedCompany()
		{
			byte[] pDF = System.Array.Empty<byte>();
			if (Company.IsValid)
			{
				GlbCompany selectedCompany = Companies.FindByPK(Company) as GlbCompany;
				if (selectedCompany != null && selectedCompany.Branches.Count > 0)
				{
					GlbBranch branch = GetBranchForStatementPrinting(selectedCompany);

					using (WebLoginBranch statementBranch = new WebLoginBranch(branch))
					{
						Statement printableStatement = GetStatementForBranch(branch);
						if (printableStatement != null && AH_OH.IsValid)
						{
							printableStatement.OH_PK = AH_OH;
							printableStatement.CreditStatements = Statement.CreditOptions.AllDocuments;
							PrintTask task = printableStatement.GetPrintTask();
							if (task != null && task.Count > 0)
							{
								pDF = new DocumentUtility(Factory).GetDocument(task[0], LoggedInUser, DataContentTypes.Pdf);
							}
						}
						else
						{
							ZString message = string.Format((NoResString)"Failed to create Statement for branch {0} ({1}) for company {2}\n", branch.GB_BranchName, branch.PK, branch.Company.GC_Name); // Programmer's exception message
							if (printableStatement == null)
							{
								message += string.Format((NoResString)"PrintableStatement is null\n"); // Programmer's exception message
							}
							else if (!AH_OH.IsValid)
							{
								message += string.Format((NoResString)"AccHeader OrgHeader is invalid: {0}\n", AH_OH); // Programmer's exception message
								message += string.Format("LoggedInUser: {0}\n", LoggedInUser == null ? "NULL" : string.Format("{0} ({1})", LoggedInUser.OC_Email, LoggedInUser.Header.OH_Code)); // Programmer's exception message
							}
							ErrorReporter.ReportOnce("WEBSTATEMENT_STATEMENTFAIL", message);
						}
					}
				}
				else
				{
					string key = "WEBSTATEMENT_UNKNOWNERROR";
					string message = string.Format((NoResString)"Failed to generate web statement for company.\n"); // Programmer's exception message
					if (selectedCompany == null)
					{
						key = "WEBSTATEMENT_COMPANYNULL";
						message += string.Format((NoResString)"Cannot find company in collection of companies by PK {0}. ", Company); // Programmer's exception message
						message += string.Format((NoResString)"\nCompanies collection contain the following {0} items: ", Companies.Count); // Programmer's exception message
						foreach (GlbCompany company in Companies)
						{
							message += string.Format("{0} [{1}]\n", company.GC_Name, company.PK);
						}
					}
					else if (selectedCompany.Branches.Count < 1)
					{
						key = "WEBSTATEMENT_NOBRANCHES";
						message += string.Format((NoResString)"No branches found for company {0}({1}).\n", selectedCompany.GC_Code, selectedCompany.GC_Name); // Programmer's exception message
					}
					message += (NoResString)"\n\nAdditional Information:\n"; // Programmer's exception message
					message += string.Format("LoggedInUser: {0}\n", LoggedInUser == null ? new ZString("NULL") : LoggedInUser.OC_Email); // Programmer's exception message
					message += string.Format((NoResString)"Organization: {0}\n", LoggedInUser == null ? "NULL" : string.Format("{0} ({1})", LoggedInUser.Header.OH_Code, LoggedInUser.Header.OH_FullNameTruncated)); // Programmer's exception message
					message += string.Format((NoResString)"Company: {0}\n", Company); // Programmer's exception message
					message += string.Format("SelectedCompany: {0}\n", selectedCompany == null ? "NULL" : string.Format("{0} ({1})", selectedCompany.GC_Code, selectedCompany.GC_Name)); // Programmer's exception message
					message += string.Format("Filter.AH_OH: {0}\n", AH_OH); // Programmer's exception message
					ErrorReporter.ReportOnce(key, message);
				}
			}

			return pDF;
		}

		protected GlbBranch GetBranchForStatementPrinting(GlbCompany selectedCompany)
		{
			GlbBranch branch = selectedCompany.Branches[0];

			if (AH_OH.IsValid && !AH_OH.IsEmpty)
			{
				OrgHeader org = selectedCompany.Factory.Load<OrgHeader>(AH_OH);

				if (org != null && org.CompanyData != null && org.CompanyData.ControllingBranch != null
					&& selectedCompany.Branches.FindByPK(org.CompanyData.ControllingBranch.PK) != null)
				{
					branch = org.CompanyData.ControllingBranch;
				}
			}
			return branch;
		}

#if DEBUG
		protected virtual
#endif
 Statement GetStatementForBranch(GlbBranch branch)
		{
			return Statement.New(branch);
		}
		#endregion

		#region Lookups

		#region Companies

		public GlbCompanyCollection Companies
		{
			get
			{
				if (fCompanies == null)
				{
					TrackingSiteUser siteUser = WebEnv.AppInstance.SiteUser as TrackingSiteUser;
					var parentOrgPK = siteUser != null && siteUser.LoggedInOrganisation != null ? siteUser.LoggedInOrganisation.PK : ZGuid.Empty;

					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_GC);
					subQuery.AddToFilter(OrgCompanyDataSchema.OB_OH, parentOrgPK);
					subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, true);

					var filter = new ZDBOnlyQuery(typeof(GlbCompany));
					filter.AddToFilter(GlbCompanySchema.GC_IsActive, true);
					filter.AddSubQuery(GlbCompanySchema.PK, subQuery, JoinCondition.And);

					fCompanies = new GlbCompanyCollection(Factory);
					fCompanies.AdditionalFilter = filter;
				}
				return fCompanies;
			}
		}
		GlbCompanyCollection fCompanies;

		#endregion

		#region AH_NumberFilter_List

		public new ZQueryProviderCodeDescriptionListBase AH_NumberFilter_List
		{
			get
			{
				if (fAH_NumberFilter_List == null)
				{
					fAH_NumberFilter_List = base.AH_NumberFilter_List;
					fAH_NumberFilter_List.RemoveCode(QueryDeciderNoSelectionCode);
					fAH_NumberFilter_List.RemoveCode(AccountingUtils.NumberFilterTypes.ConsolidationNumber);
				}
				return fAH_NumberFilter_List;
			}
		}
		ZQueryProviderCodeDescriptionListBase fAH_NumberFilter_List;

		protected override IReadOnlyCollection<string> NumberFilterCommonItems
		{
			get { return new string[] { AccountingUtils.NumberFilterTypes.TransactionNumber }; }
		}

		#endregion AH_NumberFilterList

		#region AH_DateFilter_List

		public override ZQueryProviderCodeDescriptionListBase AH_DateFilter_List
		{
			get
			{
				if (fAH_DateFilter_List == null)
				{
					fAH_DateFilter_List = new ZQueryProviderCodeDescriptionListWith2FilterArguments();

					fAH_DateFilter_List.Add(AccountingUtils.DateFilterTypes.TransactionDate, ResString.GetMultilingualString("862a6a54-73b9-421b-b90d-2ceaf7371e06", "Transaction Date"),
						AccTransactionHeaderSchema.AH_InvoiceDate, AccTransactionHeaderSchema.AH_InvoiceDate);

					fAH_DateFilter_List.Add(AccountingUtils.DateFilterTypes.DueDate, ResString.GetMultilingualString("902210bb-de43-4a8f-af5a-682d414c5e5b", "Due Date"),
						AccTransactionHeaderSchema.AH_DueDate, AccTransactionHeaderSchema.AH_DueDate);

					fAH_DateFilter_List.AddQueryProviderCompositionForAll(0);
				}
				return fAH_DateFilter_List;
			}
		}
		ZQueryProviderCodeDescriptionListWith2FilterArguments fAH_DateFilter_List;

		#endregion AH_DateFilter_List

		#region TransactionTypeList

		public override CodeDescriptionPairList TransactionTypeList
		{
			get
			{
				if (fTransactionTypeList == null)
				{
					fTransactionTypeList = new CodeDescriptionPairList();
					fTransactionTypeList.AddPair("ALL", Res.GetString("f9ba2c58-6de3-4583-8844-75a5c9dab00f", "All Transactions"));
					fTransactionTypeList.AddPair("CRD", Res.GetString("84e6b432-bf8b-4564-b835-367d300dec39", "Credit Note"));
					fTransactionTypeList.AddPair("INV", Res.GetString("857626a6-3dd3-4946-bbd7-06254eb05a0e", "Invoice"));
				}
				return fTransactionTypeList;
			}
		}

		#endregion TransactionTypeList

		#endregion

		#region Properties

		#region LoggedInUser

		public OrgContact LoggedInUser
		{
			get { return fLoggedInUser; }
			set
			{
				fLoggedInUser = value;
				if (fLoggedInUser != null && fLoggedInUser.ParentOrg != null)
				{
					AH_OH = fLoggedInUser.ParentOrg.PK;
				}
				else
				{
					AH_OH = ZGuid.Empty;
				}
			}
		}
		OrgContact fLoggedInUser;

		#endregion

		#region Company

		public ZGuid Company
		{
			get { return fCompany; }
			set
			{
				fCompany = value;
				CompanyInfo.RefreshBinding();
			}
		}
		ZGuid fCompany;

		public ZPropertyInfo CompanyInfo
		{
			get { return GetZPropertyInfo(nameof(Company)); }
		}

		#endregion

		#region FilterByCompany

		GlbCompany FilterByCompany
		{
			get { return Factory.Load<GlbCompany>(FilterByCompanyPK); }
		}

		#endregion

		#region FilterByCompanyPK

		public ZGuid FilterByCompanyPK
		{
			get { return fFilterByCompanyPK; }
			set
			{
				fFilterByCompanyPK = value;
				FilterByCompanyPKInfo.RefreshBinding();
			}
		}
		ZGuid fFilterByCompanyPK;

		public ZPropertyInfo FilterByCompanyPKInfo
		{
			get { return GetZPropertyInfo(nameof(FilterByCompanyPK)); }
		}

		#endregion

		#endregion

		#region Validation

		protected override void CheckAH_TransactionType()
		{
		}

		#endregion
	}
}
