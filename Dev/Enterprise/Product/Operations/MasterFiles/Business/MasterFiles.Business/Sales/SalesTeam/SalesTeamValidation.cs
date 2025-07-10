using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class SalesTeamValidation : GlbGroupValidation
	{
		public SalesTeamValidation(SalesTeam parent)
			: base(parent)
		{
		}

		public new SalesTeam Parent
		{
			get { return (SalesTeam)base.Parent; }
		}

		#region GG_Code

		protected override void DuplicateCodeError()
		{
			Parent.GG_CodeInfo.AddError(Res.GetString("c498664c-d4b9-4c90-86e1-876a46c5e5b5", "Sales Team Code must be unique in the system."));
		}

		#endregion

		#region GG_GC

		protected override void CheckGG_GC()
		{
			base.CheckGG_GC();

			if (Parent.GG_GC.IsValid && Parent.GG_GC != GlbCompany.CurrentCompany.PK && !Env.Security.SalesTeamsAllowSearchOutsideLoginCompany.IsAllowed)
			{
				Parent.GG_GCInfo.AddError(Res.GetString("40b448d1-3083-4212-99f7-cb26c34c00c6", @"You do not have the appropriate security rights to view or edit Sales Teams outside your current login company ({0}).

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: 

{1}", GlbCompany.CurrentCompany.GC_Code, Env.Security.SalesTeamsAllowSearchOutsideLoginCompany.DisplayTextPathToSecurityRight));
			}
		}

		#endregion

		#region Group Members

		protected void ValidateGroupMembers()
		{
			if (Parent.IsTopLevel && Parent.Staff.Count > 0)
			{
				var parentTeamCoveredUnlocoCodes = new HashSet<ZString>(Parent.CoveredUnlocos.Select(unloco => unloco.RL_Code));
				var parentTeamCoveredUnlocoCountryCodes = new HashSet<ZString>(Parent.CoveredUnlocos.Select(unloco => unloco.RL_RN_NKCountryCode));
				var parentTeamCoveredCountryCodes = new HashSet<ZString>(Parent.CoveredCountries.Select(country => country.RN_Code));

				Parent.RemoveRowError(OverlappingTeamCoverageErrorMessage);
				foreach (var salesRep in Parent.Staff.Cast<GlbStaff>().Where(x => x.CurrentGroupLink != null))
				{
					salesRep.ClearRowNotifications();

					foreach (var otherSalesTeam in salesRep.SalesTeams.Cast<SalesTeam>().Where(team => team.PK != Parent.PK))
					{
						if (otherSalesTeam.GG_GC == Parent.GG_GC)
						{
							if (otherSalesTeam.CoveredCountries.Any(country => parentTeamCoveredCountryCodes.Contains(country.RN_Code) || parentTeamCoveredUnlocoCountryCodes.Contains(country.RN_Code))
								|| otherSalesTeam.CoveredUnlocos.Any(unloco => parentTeamCoveredUnlocoCodes.Contains(unloco.RL_Code) || parentTeamCoveredCountryCodes.Contains(unloco.RL_RN_NKCountryCode)))
							{
								salesRep.AddRowError(Res.GetString("09d83ec3-4938-4342-9f16-60601787ad62", "Already assigned to {0} which has overlapping coverage area with this team.", otherSalesTeam.HumanReadableName));
								Parent.AddRowError(OverlappingTeamCoverageErrorMessage);
							}
						}
					}
				}
			}
		}

		string OverlappingTeamCoverageErrorMessage
		{
			get { return Res.GetString("728946c1-0750-457d-ac6d-165975466404", "There are staff that are assigned to other Sales Teams that have overlapping coverage area with this team."); }
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateGroupMembers();
		}
	}
}
