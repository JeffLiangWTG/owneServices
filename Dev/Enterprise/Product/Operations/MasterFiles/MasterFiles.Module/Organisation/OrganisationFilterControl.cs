using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	[SuppressFormDesignerAnalysis]
	public partial class OrganisationFilterControl : ZFilterStripControl
	{
		public OrganisationFilterControl()
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				ImporterBondQueryDateVisibility();
				SetUnavailableColumns();
			}
		}

		public OrganisationFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: this(gridCollection, filterBusinessObject, OrgModuleType.Standard)
		{
		}

		public OrganisationFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject, OrgModuleType moduleType)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();

			WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, JobInvoicingConsumerTypes.Organisation.Code);
			ImporterBondQueryDateVisibility();
			SetUnavailableColumns();
			OrgFilterObject = (OrganisationFilterBusinessObject)filterBusinessObject;
			var properties = new CustomPropertyCollectionImpl((Func<string, object>)null, null);
			OrgHeader.PopulateProperties(properties);
			var initializer = new ZGridCustomColumnsInitializer(FilteredGrid, gridCollection, null);
			initializer.AddCustomColumns(properties);
		}

		void SetUnavailableColumns()
		{
			if (!OrganisationsDataRegistry.Instance.EnableCreditReports.Value || !OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.Value.Cast<CreditReportItem>().Any(x => x.CountryEnabledForCompany && x.CountryCode == Env.CurrentCompany.Country.Code))
			{
				Grid.SetAvailability(false, ["MiscServ+OM_CCCreditRating", "MiscServ+OM_CCLatePaymentScore", "MiscServ+OM_CCFailureRiskScore"]);
			}
		}

		protected readonly OrganisationFilterBusinessObject OrgFilterObject;

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new OrganisationFilterStrip();
		}

		void ImporterBondQueryDateVisibility()
		{
			var visibilityForImporterBondQueryDate = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates;
			Grid.SetAvailability(visibilityForImporterBondQueryDate, OrgHeader.Schema.ImporterBondQueryDate);
		}
	}
}
