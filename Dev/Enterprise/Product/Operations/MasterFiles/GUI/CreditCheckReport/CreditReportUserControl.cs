using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using WTG.CreditCheck;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CreditReportUserControl : ZUserControl
	{
		public CreditReportUserControl()
		{
			InitializeComponent();

			ServiceWrapper = new CreditCheckServiceWrapper();
		}

		internal OrgHeader Header => BindingSource.DataSource as OrgHeader;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (!IsDisposing)
			{
				CreditCheckControl.Initialize(this);
			}
		}

		internal void SaveAndShowReport(CreditReportType creditReportType, string payload, string payloadExtension, bool isGet, string dunsRating, string failureRiskScore, string latePaymentScore)
		{
			var eDoc = Header.SaveAndShowReport(creditReportType, payload, payloadExtension, isGet, Identifiers.FirstOrDefault(u => u.Type == IdentifierType.DUNS)?.ID, dunsRating, failureRiskScore, latePaymentScore);
			CreditReportHelper.ViewReport(ParentForm as ZForm, eDoc);
		}

		internal void ShowLatestReport(CreditReportType creditReportType)
		{
			Header.ShowLatestReport(ParentForm as ZForm, creditReportType);
		}

		internal bool SaveIdentifiers(IEnumerable<Identifier> identifiers, bool silentSave)
		{
			var result = Header.SaveIdentifiers(identifiers, silentSave);

			if (!silentSave && !result && ParentForm is ZOrganisationsForm parentForm)
			{
				parentForm.OrganisationsTabControl.SelectedTab = parentForm.DetailsTabPage;
				parentForm.DetailsControl.DetailsTabControl.SelectedTab = parentForm.DetailsControl.ConfigTabPage;
				var configTabControl = parentForm.DetailsControl.ConfigUserControl.ConfigTabControl;

				if (configTabControl != null)
				{
					configTabControl.SelectedTab = configTabControl.Controls?.OfType<ZTabPage>().Single(c => c.Name == "RegistrationCodesTabPage");
				}
			}

			return result;
		}

		internal string EnterpriseCode => GlbCompany.CurrentCompany.LicenceKeyIdentifier;

		internal Guid EntityPk => Header?.PK.ToGuid() ?? Guid.Empty;

		CreditReportTermsAndCondition term;

		protected virtual CreditReportTermsAndCondition Term => term ?? (term = new CreditReportTermsAndCondition());

		internal virtual Dictionary<(CreditReportType ReportType, bool IsGet), (bool IsAllowed, string ErrorMessageForNotAllowed)> SecurityCheckpoints
			=> new Dictionary<(CreditReportType ReportType, bool IsGet), (bool IsAllowed, string ErrorMessageForNotAllowed)>()
			{
				[(CreditReportType.ComprehensiveReport, true)] = (Env.Security.OrganisationCreditReportsGetReportComprehensiveReport.IsAllowed, Env.Security.OrganisationCreditReportsGetReportComprehensiveReport.ErrorMessageForNotAllowed),
				[(CreditReportType.ComprehensiveReport, false)] = (Env.Security.OrganisationCreditReportsRenewReportComprehensiveReport.IsAllowed, Env.Security.OrganisationCreditReportsRenewReportComprehensiveReport.ErrorMessageForNotAllowed),

				[(CreditReportType.FailureRisk, true)] = (Env.Security.OrganisationCreditReportsGetReportFailureRisk.IsAllowed, Env.Security.OrganisationCreditReportsGetReportFailureRisk.ErrorMessageForNotAllowed),
				[(CreditReportType.FailureRisk, false)] = (Env.Security.OrganisationCreditReportsRenewReportFailureRisk.IsAllowed, Env.Security.OrganisationCreditReportsRenewReportFailureRisk.ErrorMessageForNotAllowed),

				[(CreditReportType.LatePaymentRisk, true)] = (Env.Security.OrganisationCreditReportsGetReportLatePaymentRisk.IsAllowed, Env.Security.OrganisationCreditReportsGetReportLatePaymentRisk.ErrorMessageForNotAllowed),
				[(CreditReportType.LatePaymentRisk, false)] = (Env.Security.OrganisationCreditReportsRenewReportLatePaymentRisk.IsAllowed, Env.Security.OrganisationCreditReportsRenewReportLatePaymentRisk.ErrorMessageForNotAllowed),

				[(CreditReportType.CommercialBureauEnquiry, true)] = (Env.Security.OrganisationCreditReportsGetReportCommercialBureauEnquiry.IsAllowed, Env.Security.OrganisationCreditReportsGetReportCommercialBureauEnquiry.ErrorMessageForNotAllowed),
				[(CreditReportType.CommercialBureauEnquiry, false)] = (Env.Security.OrganisationCreditReportsRenewReportCommercialBureauEnquiry.IsAllowed, Env.Security.OrganisationCreditReportsRenewReportCommercialBureauEnquiry.ErrorMessageForNotAllowed),
			};

		internal IEnumerable<Identifier> Identifiers
		{
			get
			{
				var result = new List<Identifier>();
				var customsCodes = Header.CustomsCodes.Cast<OrgCusCode>();
				foreach (var cusCode in customsCodes)
				{
					if (cusCode.OK_CodeType == OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber && cusCode.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Australia)
					{
						result.Add(new Identifier() { ID = cusCode.OK_CustomsRegNo, Type = IdentifierType.ABN });
					}
					else if (cusCode.OK_CodeType == OrgCusCode.MozambiqueCodeTypes.GCR && cusCode.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Australia)
					{
						result.Add(new Identifier() { ID = cusCode.OK_CustomsRegNo, Type = IdentifierType.ACN });
					}
					else if (cusCode.OK_CodeType == OrgCusCode.CodeTypes.CompanyNumber && cusCode.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.NewZealand)
					{
						result.Add(new Identifier() { ID = cusCode.OK_CustomsRegNo, Type = IdentifierType.NZBN });
					}
					else if (cusCode.OK_CodeType == OrgCusCode.SamoaCodeTypes.GST && cusCode.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.NewZealand)
					{
						result.Add(new Identifier() { ID = cusCode.OK_CustomsRegNo, Type = IdentifierType.NCN });
					}
					else if (cusCode.OK_CodeType == OrgCusCode.CodeTypes.DataUniversalNumberingSystem)
					{
						result.Add(new Identifier() { ID = cusCode.OK_CustomsRegNo, Type = IdentifierType.DUNS });
					}
				}

				return result;
			}
		}

		internal string[] AvailableCountries => OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.Value.Cast<CreditReportItem>().Where(x => x.CountryEnabledForOrganisation).Select(x => x.CountryCode.ToString()).ToArray();

		internal IEnumerable<CreditReportType> AvailableCreditReportTypes
		{
			get
			{
				var result = new List<CreditReportType>();

				var currentCountryRegistryItem = OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.Value.Cast<CreditReportItem>().FirstOrDefault(x => x.CountryEnabledForOrganisation && x.CountryCode == Header.CountryCode);
				if (currentCountryRegistryItem != null)
				{
					if (currentCountryRegistryItem.ComprehensiveReportEnabled)
					{
						result.Add(CreditReportType.ComprehensiveReport);
					}

					if (currentCountryRegistryItem.CommercialBureauEnquiryEnabled)
					{
						result.Add(CreditReportType.CommercialBureauEnquiry);
					}

					if (currentCountryRegistryItem.LatePaymentRiskEnabled)
					{
						result.Add(CreditReportType.LatePaymentRisk);
					}

					if (currentCountryRegistryItem.FailureRiskEnabled)
					{
						result.Add(CreditReportType.FailureRisk);
					}
				}

				return result;
			}
		}

		internal virtual IEnumerable<(CreditReportType ReportType, DateTime LastGetReportDate)> PurchasedReports => Header.GetPurchasedReports().Select(x => (x.CreditReportType, x.BuyReportDate.ToDateTime()));

		internal async Task<bool> ShowTermsAndAgreement()
		{
			var acknowledgeChecker = new TermsAcknowledgementChecker(Term, ParentForm);
			return await acknowledgeChecker.CheckTermAcknowledged();
		}

		internal ISupportCreditCheckService ServiceWrapper { get; set; }
	}
}
