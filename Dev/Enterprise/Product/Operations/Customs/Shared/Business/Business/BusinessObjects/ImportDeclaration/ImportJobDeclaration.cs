using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// This class creates a new declaration and defaults its data from a declaration selected in another country.
	/// </summary>
	public class ImportJobDeclaration : NonPersistentBusinessObject
	{
		public ImportJobDeclaration(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region Declaration PK

		public ZGuid DeclarationPK
		{
			get { return fDeclarationPK; }
			set
			{
				if (fDeclarationPK != value)
				{
					SetNonPersistentPropertyValue(DeclarationPKInfo, ref fDeclarationPK, value);
					Validation.ValidateDeclarationPK();
				}
			}
		}
		ZGuid fDeclarationPK;

		public ZPropertyInfo DeclarationPKInfo
		{
			get { return GetZPropertyInfo(nameof(DeclarationPK)); }
		}

		protected bool DeclarationPK_ReadOnly
		{
			get { return CountryCodeInfo.HasNotifications() || CountryCode.IsEmpty || CountryCode == GlbCompany.CurrentCompany.GC_RN_NKCountryCode; }
		}

		#endregion

		#region Declaration

		protected BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null || fDeclaration.PK != DeclarationPK)
				{
					fDeclaration = Factory.Load<BaseJobDeclaration>(DeclarationPK);
				}

				return fDeclaration;
			}
		}
		BaseJobDeclaration fDeclaration;

		#endregion

		#region Country Code

		[CargoWise.ComponentModel.MaxLength(2)]
		public ZString CountryCode
		{
			get { return fCountryCode; }
			set
			{
				if (fCountryCode != value)
				{
					CheckMaximumLength(CountryCodeInfo, value);
					SetNonPersistentPropertyValue(CountryCodeInfo, ref fCountryCode, value);

					Validation.ValidateCountryCode();
					if (DeclarationPKInfo.ReadOnly)
					{
						DeclarationPK = ZGuid.Empty;
					}

					RefreshBinding();
				}
			}
		}
		ZString fCountryCode;

		public ZPropertyInfo CountryCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CountryCode)); }
		}

		#endregion

		#region Validation

		public ImportJobDeclarationValidation Validation
		{
			get { return new ImportJobDeclarationValidation(this); }
		}

		#endregion

		#region Lookups

		public ImportJobDeclarationLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new ImportJobDeclarationLookups(this);
				}

				return fLookups;
			}
		}
		ImportJobDeclarationLookups fLookups;

		#endregion

		public BaseJobDeclaration CreateNewStandAloneDeclaration(BusinessObjectFactory factoryToCreateDecIn)
		{
			if (Declaration != null)
			{
				ImportedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(Declaration, CloneType.CountryToCountryCopy, GlbBranch.CurrentBranch.PK).Clone();
				ImportDeclarationData();
			}

			return ImportedDeclaration;
		}

		public BaseJobDeclaration CreateDeclarationAgainstShipment()
		{
			if (Declaration != null)
			{
				ImportedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(Declaration, CloneType.CountryToCountryCopyWithinShipment, GlbBranch.CurrentBranch.PK).Clone();
				ImportedDeclaration.JE_OverrideFreightDefaults = true;

				ImportDeclarationData();
			}

			return ImportedDeclaration;
		}

		void ImportDeclarationData()
		{
			using (ImportedDeclaration.GetValidationSuspender())
			using (ImportedDeclaration.SuspendMarkApportionmentDirty())
			{
				using (ImportedDeclaration.SuspendJE_OH_ImporterSetting())
				using (ImportedDeclaration.SuspendJE_OH_SupplierSetting())
				{
					if (ImportedDeclaration.Importer is OrgHeader importer && !importer.IsMiscellaneous && importer.UNLOCO != null)
					{
						ImportedDeclaration.DefaultMessageTypeFromImporterUNLOCO(importer);
					}
					else if (ImportedDeclaration.Supplier is OrgHeader supplier && !supplier.IsMiscellaneous && supplier.UNLOCO != null)
					{
						ImportedDeclaration.DefaultMessageTypeFromSupplierUNLOCO(supplier);
					}
					else
					{
						ImportedDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					}
				}

				ImportedDeclaration.JE_PaymentMethod = ZString.Empty;//database default has been set to 'DEF'

				using (ImportedDeclaration.SuspendJE_MessageTypeSetting())
				{
					ImportedDeclaration.DefaultImporterDocAddresses(ImportedDeclaration.JE_OH_Importer);
					ImportedDeclaration.DefaultSupplierDocAddresses(ImportedDeclaration.JE_OH_Supplier);
				}

				ImportDeclarationInvoiceLinesData();
				PerformCountrySpecificImporting();
				ImportedDeclaration.ResumeApportionment();
			}
		}

		void ImportDeclarationInvoiceLinesData()
		{
			foreach (BaseJobComInvoiceLine invoiceLine in ImportedDeclaration.InvoiceLines)
			{
				invoiceLine.UpdatePartSyncManagerAndRefresh(!invoiceLine.IsAdvanceShippingNoticeInvoice);
			}
		}

		#region Load Declaration

		public void LoadDeclarationForShipment(string shipmentReference)
		{
			ZDBOnlyQuery queryForDecsCreatedInOtherCountries = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			queryForDecsCreatedInOtherCountries.AddToFilter(JobDeclarationSchema.JE_DeclarationReference, shipmentReference);

			ZDBOnlySubQuery branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
			ZDBOnlySubQuery countryQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), GlbBranchSchema.GB_RL_NKHomePort);
			countryQuery.AddToFilter(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			branchQuery.AddSubQuery(GlbBranchSchema.GB_RL_NKHomePort, RefUNLOCOSchema.RL_Code, countryQuery, JoinCondition.And);

			queryForDecsCreatedInOtherCountries.AddSubQuery(branchQuery, JoinCondition.And);
			BaseJobDeclaration[] jobDeclarations = (BaseJobDeclaration[])Factory.Load(typeof(BaseJobDeclaration), queryForDecsCreatedInOtherCountries);

			if (jobDeclarations.Length == 1)
			{
				DeclarationPK = jobDeclarations[0].PK;
			}
			else if (jobDeclarations.Length > 1)
			{
				foreach (BaseJobDeclaration declarationAttachedToShipment in jobDeclarations)
				{
					if (declarationAttachedToShipment.JE_RL_NKFinalDestination.StartsWith(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
					{
						DeclarationPK = declarationAttachedToShipment.PK;
						break;
					}
					else if (declarationAttachedToShipment.JE_RL_NKOrigin.StartsWith(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
					{
						DeclarationPK = declarationAttachedToShipment.PK;
						break;
					}
				}
			}
		}

		#endregion

		protected virtual void PerformCountrySpecificImporting()
		{
		}

		protected BaseJobDeclaration ImportedDeclaration
		{
			get { return fImportedDeclaration; }
			set { fImportedDeclaration = value; }
		}
		BaseJobDeclaration fImportedDeclaration;
	}
}
