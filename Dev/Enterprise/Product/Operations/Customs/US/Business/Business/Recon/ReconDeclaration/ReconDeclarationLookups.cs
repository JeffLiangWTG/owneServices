using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class ReconDeclarationLookups : IInvoicesProviderLookups
	{
		public ReconDeclarationLookups(ReconDeclaration reconDeclaration, JobDeclaration declaration)
		{
			this.declaration = declaration;
			this.reconDeclaration = reconDeclaration;
			factory = declaration.Factory;
		}

		readonly BusinessObjectFactory factory;
		readonly ReconDeclaration reconDeclaration;
		readonly JobDeclaration declaration;

		public InvoiceHeaderWithNoDeclarationCollection InvoicesToAttach
		{
			get { return declaration.Lookups.InvoicesToAttach; }
		}

		public RefCurrencyCollection CurrencyList
		{
			get { return new RefCurrencyCollection(factory); }
		}

		public ConsigneeCollection ImporterList
		{
			get { return new ConsigneeCollection(factory); }
		}

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(factory); }
		}

		public ReconTeamsList US_TeamNoList
		{
			get { return factory.GetCachedValue<ReconTeamsList>(); }
		}

		public ReconIssueCodeList US_IssueCodeList
		{
			get
			{
				return factory.GetCachedValue("Recon Issue Code without NA", delegate
				{
					var result = new ReconIssueCodeList();
					result.RemoveCode(ReconIssueCodeList.Codes.NotApplicable);
					return result;
				});
			}
		}

		public PaymentTypeList US_PaymentTypeList
		{
			get { return PaymentTypeList.GetCachedReconPaymentTypeList(factory); }
		}

		public ZZRefCusCodeListCombinedCollection SchDPortList
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public GlbBranchCollection BranchList
		{
			get { return new GlbBranchCollection(factory); }
		}

		public CodeDescriptionPairList ImportEntrySourceList
		{
			get { return factory.GetCachedValue<ReconciliationImportEntrySourceList>(); }
		}

		public GlbStaffCollection CusAgents
		{
			get { return new GlbStaffCollection(factory); }
		}

		public JobApplicationCodeList ApplicationCodeList
		{
			get { return factory.GetCachedValue<JobApplicationCodeList>(); }
		}

		public CodeDescriptionPairList SuretyCodeList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				OrgHeaderWrapper iORWrapper = reconDeclaration.IORWrapper;
				if (iORWrapper != null)
				{
					foreach (CusBondDetail bondDetail in iORWrapper.BondDetails)
					{
						ZString suretyCode = bondDetail.PW_SuretyCode;
						if (!suretyCode.IsEmpty && !result.ContainsCode(suretyCode))
						{
							result.AddPair(suretyCode);
						}
					}
				}
				return result;
			}
		}

		public CodeDescriptionPairList US_YesNoList
		{
			get { return YesNoDefaultList.GetCachedYesNoList(factory); }
		}

		public ReconCustomsValueRecalculationTypeList RecalculationTypeList
		{
			get { return factory.GetCachedValue<ReconCustomsValueRecalculationTypeList>(); }
		}

		public CusStatementHeaderCollection StatementList
		{
			get { return new CusStatementHeaderCollection(factory); }
		}

		public RefServiceLevelCollection ServiceLevels
		{
			get { return new RefServiceLevelCollection(factory); }
		}

		public CodeDescriptionPairList GetGovNumTypeList(JobDocAddressLookups lookups)
		{
			return factory.GetCachedValue("ReconGovNumTypeList", delegate
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "Employee Identification Number - IRS#");
				result.AddPair(OrgCusCode.USACodeTypes.SocialSecurityNumber, "Social Security Number");
				result.AddPair(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "CBP Assigned Number");
				return result;
			});
		}

		#region IInvoicesProviderLookups Members

		IBusinessObjectCollection IInvoicesProviderLookups.InvoicesToAttach
		{
			get { return InvoicesToAttach; }
		}

		#endregion
	}
}
