using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(SalesTeam.Schema.GG_Code), DescriptionProperty(SalesTeam.Schema.GG_Desc)]
	public class SalesTeam : GlbGroup
	{
		public SalesTeam(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = Res.GetString("5e6a00db-b787-4640-b6c1-23eed519e67c", "Sales Team");
				if (!IsDeleted && !GG_Code.IsEmpty)
				{
					result += " (" + GG_Code + ")";
				}
				return result;
			}
		}

		protected override ZString HumanReadableNameForPluralCore
		{
			get { return Res.GetString("92a39480-2b81-4b80-97ea-cb5addfa1529", "Sales Teams"); }
		}

		#endregion

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GG_IsSales = true;
			GG_GC = GlbCompany.CurrentCompany.PK;
		}

		/// <summary>
		/// Avoid handling security permissions for Sales Teams
		/// </summary>
		protected override void SetDefaultValuesCore()
		{
		}

		#endregion

		#region ParentTeamPk

		[RelatedBusinessObject("ParentTeam")]
		[List("Lookups.ParentTeams")]
		public ZGuid ParentTeamPk
		{
			get { return base.GG_GG_ParentGroup; }
			set
			{
				base.GG_GG_ParentGroup = value;
			}
		}

		public SalesTeam ParentTeam
		{
			get { return Factory.Load<SalesTeam>(GG_GG_ParentGroup); }
		}

		public ZPropertyInfo ParentTeamPkInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(GlbGroupSchema.Constants.GG_GG_ParentGroup); }
		}

		protected bool ParentTeamPk_ReadOnly
		{
			get
			{
				return GG_GG_ParentGroup_ReadOnly;
			}
		}

		#endregion

		#region Related Business Objects

		#region Commission Rules

		[ChildEditable]
		public AccGroupCommissionRuleCollection CommissionRules
		{
			get
			{
				if (commissionRules == null)
				{
					commissionRules = new AccGroupCommissionRuleCollection(this);
					RegisterEditableChildObject(commissionRules);
				}

				return commissionRules;
			}
		}
		AccGroupCommissionRuleCollection commissionRules;

		#endregion

		#region Covered Locations

		#region CoveredCountries

		[ChildEditable]
		public GlbGroupCountryCollection CoveredCountries
		{
			get
			{
				if (coveredCountries == null)
				{
					coveredCountries = new GlbGroupCountryCollection(this);
					RegisterEditableChildObject(coveredCountries);
				}

				return coveredCountries;
			}
		}
		GlbGroupCountryCollection coveredCountries;

		#endregion

		#region CoveredUnlocos

		[ChildEditable]
		public GlbGroupUnlocoCollection CoveredUnlocos
		{
			get
			{
				if (coveredUnlocos == null)
				{
					coveredUnlocos = new GlbGroupUnlocoCollection(this);
					RegisterEditableChildObject(coveredUnlocos);
				}

				return coveredUnlocos;
			}
		}
		GlbGroupUnlocoCollection coveredUnlocos;

		#endregion

		public ZBool Covers(RefCountry country)
		{
			if (country == null)
			{
				return false;
			}

			return CoveredCountries.Any(coveredCountry => coveredCountry.RN_Code == country.RN_Code);
		}

		public ZBool Covers(RefUNLOCO unloco)
		{
			if (unloco == null)
			{
				return false;
			}

			return
				CoveredUnlocos.Any(coveredUnloco => coveredUnloco.RL_Code == unloco.RL_Code) ||
				CoveredCountries.Any(coveredCountry => coveredCountry.RN_Code == unloco.RL_RN_NKCountryCode);
		}

		#endregion

		#endregion

		#region Saving

		public override void OnSaving()
		{
			base.OnSaving();
			if (ShouldDetachSalesReps)
			{
				this.Staff.RemoveAll();
			}
		}

		public bool ShouldDetachSalesReps
		{
			get { return !GG_IsActive && Staff.Count > 0; }
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			CommissionRules.DeleteAll();
			CoveredCountries.RemoveAllFromRelationship();
			CoveredUnlocos.RemoveAllFromRelationship();

			base.Delete();
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint GetSecurityCheckpointForReadOnlySecurity()
		{
			return IsInDatabase ? Env.Security.SalesTeamsEdit : Env.Security.SalesTeamsNew;
		}

		#endregion

		#region Lookups

		public new SalesTeamLookups Lookups
		{
			get { return (SalesTeamLookups)base.Lookups; }
		}

		protected override GlbGroupLookups GetNewLookups()
		{
			return new SalesTeamLookups(this);
		}

		#endregion

		#region Validation

		protected override bool EnableLightValidationIfAvailable
		{
			get { return false; }
		}

		public new SalesTeamValidation Validation
		{
			get { return (SalesTeamValidation)base.Validation; }
		}

		protected override GlbGroupValidation GetNewValidation()
		{
			return new SalesTeamValidation(this);
		}

		#endregion

		protected override void FillGroupSecurityCollection()
		{
			// security not applicable to sales teams
		}
	}
}
