using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRateTariffLevelValidation : AutoOrgRateTariffLevelValidation
	{
		public OrgRateTariffLevelValidation(AutoOrgRateTariffLevel parent) : base(parent)
		{
		}

		public new OrgRateTariffLevel Parent
		{
			get { return (OrgRateTariffLevel)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTariffLevelAsString();
		}

		protected override void CheckP7_TariffType()
		{
			base.CheckP7_TariffType();
			Validate(Parent.P7_TariffTypeInfo);
		}

		protected override void CheckP7_Direction()
		{
			base.CheckP7_Direction();
			Validate(Parent.P7_DirectionInfo);
		}

		protected override void CheckP7_Mode()
		{
			base.CheckP7_Mode();
			Validate(Parent.P7_ModeInfo);
		}

		protected override void CheckP7_GC()
		{
			base.CheckP7_GC();
			if (!GlbCompany.CurrentCompany.GC_IsActive)
			{
				Parent.P7_GCInfo.AddError(Res.GetString("F2CE3211-0706-41AC-8B7C-F20E0D6D0200", "Changes to the Company Tariff and Group Rate Usage are invalid as the current logged in company is inactive."));
			}
		}

		protected override void CheckP7_ExpiryDate()
		{
			base.CheckP7_ExpiryDate();
			if (Parent.P7_ExpiryDate < Parent.P7_StartDate)
			{
				Parent.P7_ExpiryDateInfo.AddError(Res.GetString("b58cc499-aad2-491b-86a4-69e3f98dc5c4", "Expiry date must be after the start date."));
			}
			CheckForDuplicates();
		}

		protected override void CheckP7_StartDate()
		{
			base.CheckP7_StartDate();
			if (Parent.P7_StartDate > Parent.P7_ExpiryDate)
			{
				Parent.P7_StartDateInfo.AddError(Res.GetString("8bc3c167-9679-4052-a98d-f7d636d327f8", "Start date must be before the expiry date."));
			}
			CheckForDuplicates();
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			if (info == Parent.P7_GCInfo)
			{
				return false;
			}

			return base.ShouldValidateFKToCancelledRecord(info);
		}

		bool CheckForDateOverlap(OrgRateTariffLevel level1, OrgRateTariffLevel level2)
		{
			return (level1.P7_StartDate.IsEmpty || level2.P7_ExpiryDate.IsEmpty || level1.P7_StartDate <= level2.P7_ExpiryDate) &&
				   (level2.P7_StartDate.IsEmpty || level1.P7_ExpiryDate.IsEmpty || level2.P7_StartDate <= level1.P7_ExpiryDate);
		}

		void CheckForDuplicates()
		{
			var errorMessage = Res.GetString("0149a52b-e9f3-492a-b14b-18de75e48f20", "Duplicate or overlapping entries could not be saved. Please enter unique Tariff Type, Transport Mode, Service Direction with no overlapping Start and Expiry Dates.");
			Parent.RemoveRowError(errorMessage);

			if (Parent.Parent != null)
			{
				IEnumerable<OrgRateTariffLevel> levels = Parent.Parent.RateTariffLevels.Cast<OrgRateTariffLevel>();

				if (levels.Any(other => other.PK != Parent.PK && other.P7_TariffType == Parent.P7_TariffType && other.P7_Mode == Parent.P7_Mode && other.P7_Direction == Parent.P7_Direction && CheckForDateOverlap(Parent, other)))
				{
					Parent.AddRowError(errorMessage);
				}
			}
		}

		void Validate(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				MandatoryValidation.CheckEntered(info);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(info);
				CheckForDuplicates();
			}
		}

		#region TariffLevelAsString

		public void ValidateTariffLevelAsString()
		{
			ValidateCalculatedProperty(Parent.TariffLevelAsStringInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Necessary for it to work correctly")]
		void CheckTariffLevelAsString()
		{
			ListValidation.ErrorIfInvalidCode(Parent.TariffLevelAsStringInfo);
		}

		#endregion
	}
}
