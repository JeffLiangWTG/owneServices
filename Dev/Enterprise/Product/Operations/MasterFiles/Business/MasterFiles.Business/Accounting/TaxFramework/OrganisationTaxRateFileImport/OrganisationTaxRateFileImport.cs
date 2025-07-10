using System.ComponentModel;
using System.Drawing;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrganisationTaxRateFileImport : NonPersistentBusinessObject<OrganisationTaxRateFileImportValidation>
	{
		public OrganisationTaxRateFileImport()
			: base(new BusinessObjectFactory())
		{
		}

		[ReadOnly(true)]
		public ZString FileName
		{
			get => fileName;
			set { SetNonPersistentPropertyValue(FileNameInfo, ref fileName, value); }
		}
		ZString fileName;
		public ZPropertyInfo FileNameInfo => GetZPropertyInfo(nameof(FileName));

		[List(nameof(RateSourceList))]
		[ResourceStringData("8D3CFBC4-978B-4F78-8C5F-E1007D5EDA97", Caption = "Rate Source")]
		[MaxLength(3)]
		public ZString RateSource
		{
			get => rateSource;
			set
			{
				CheckMaximumLength(RateSourceInfo, value);
				rateSource = value;
				Validation.ValidateRateSource();
				RateSourceInfo.RefreshBinding();
			}
		}
		ZString rateSource;

		public ZPropertyInfo RateSourceInfo => GetZPropertyInfo(nameof(RateSource));
		public CodeDescriptionPairList RateSourceList => rateSourceList ?? (rateSourceList = new AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods());
		CodeDescriptionPairList rateSourceList;

		[ChildEditable(false)]
		public OrganisationTaxRateFileImportLineCollection ImportLines
		{
			get
			{
				if (importLines == null)
				{
					importLines = new OrganisationTaxRateFileImportLineCollection(Factory);
					RegisterEditableChildObject(importLines);
				}

				return importLines;
			}
		}
		OrganisationTaxRateFileImportLineCollection importLines;

		[List(nameof(TaxConfigurations))]
		[ResourceStringData("DEBF7225-1035-4BF8-AAEC-606F3773760E", Caption = "Tax Configuration")]
		public ZGuid TaxConfiguration
		{
			get => taxConfiguration;
			set
			{
				taxConfiguration = value;
				Validation.ValidateTaxConfiguration();
				TaxConfigurationInfo.RefreshBinding();
			}
		}
		ZGuid taxConfiguration;

		public ZPropertyInfo TaxConfigurationInfo => GetZPropertyInfo(nameof(TaxConfiguration));

		ZString SelectedTaxConfigurationCode => Factory.Load<AccTaxConfiguration>(TaxConfiguration)?.ETC_Code ?? ZString.Empty;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			AddOrgTaxRateLogNote();
		}

		void AddOrgTaxRateLogNote()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);

			var importLinesForNotes = ImportLines.Cast<OrganisationTaxRateFileImportLine>().Where(x => !x.IsOrgTaxRateInDb).ToArray();

			if (importLinesForNotes.Length > 0)
			{
				var rtfString = new FormattedRtfString();

				rtfString = rtfString.Add((NoResString)"System Created Organization Rates Import Log" + System.Environment.NewLine, FontStyle.Bold);
				rtfString = rtfString.Add((NoResString)"Tax: " + SelectedTaxConfigurationCode + System.Environment.NewLine, FontStyle.Regular);
				rtfString = rtfString.Add((NoResString)"Start Date: " + importLinesForNotes[0].StartDateInfo.Value + System.Environment.NewLine, FontStyle.Regular);
				rtfString = rtfString.Add((NoResString)"End Date: " + importLinesForNotes[0].EndDateInfo.Value + System.Environment.NewLine, FontStyle.Regular);
				rtfString = rtfString.Add((NoResString)"Rate Source: " + RateSourceInfo.Value + System.Environment.NewLine, FontStyle.Regular);
				rtfString = rtfString.Add((NoResString)"Importing User: " + GlbStaff.CurrentUser.GS_LoginName + System.Environment.NewLine + System.Environment.NewLine, FontStyle.Regular);
				rtfString = rtfString.Add($"Summary: ", FontStyle.Bold);
				rtfString = rtfString.Add($@"{importLinesForNotes.Length} rate records added." + System.Environment.NewLine + System.Environment.NewLine, FontStyle.Regular);

				rtfString = rtfString.Add((NoResString)"Updated Organization details:" + System.Environment.NewLine, FontStyle.Bold);

				foreach (OrganisationTaxRateFileImportLine itemRow in importLinesForNotes)
				{
					rtfString = rtfString.Add($@"Organization Code: {itemRow.OrganizationCode}; Organization Name: {itemRow.OrganizationName}; Reg. Code: {itemRow.RegistrationCode}; Rate Numerator: {itemRow.RateNumerator}; Rate Denominator: {itemRow.RateDenominator}" + System.Environment.NewLine, System.Drawing.FontStyle.Regular);
				}

				var note = currentCompany.OrgProxy.Notes.AddNew(false, PredefinedNoteTypes.Instance.OrganizationTaxConfigurationRatesImportLog.MultilingualDescription.GetUnresolvedString(), rtfString);
				note.ST_GC_RelatedCompany = currentCompany.PK;
			}
		}

		public AccTaxConfiguration TaxConfigurationObject => Factory.Load<AccTaxConfiguration>(TaxConfiguration);

		public AccTaxConfigurationCollection TaxConfigurations
			=> taxConfigurations ?? (taxConfigurations = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>()
						.GetTaxFrameworkConfigurationHelper()
						.GetTaxConfigurationThatSupportsOrganisationRates(Factory, GlbCompany.CurrentCompany));
		AccTaxConfigurationCollection taxConfigurations;

		public override OrganisationTaxRateFileImportValidation GetNewValidation() => new OrganisationTaxRateFileImportValidation(this);
	}
}
