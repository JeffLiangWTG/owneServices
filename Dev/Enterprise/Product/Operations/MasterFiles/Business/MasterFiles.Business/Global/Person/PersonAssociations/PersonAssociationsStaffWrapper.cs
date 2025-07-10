using System.Globalization;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class PersonAssociationsStaffWrapper : PersonAssociationsTreeBizObjWrapper
	{
		public PersonAssociationsStaffWrapper(PersonAssociationsTreeModel treeModel, GlbStaff staff)
			: base(treeModel)
		{
			this.Staff = staff;
		}

		#region Properties

		public GlbStaff Staff { get; }

		public bool ViewAllowed
		{
			get { return Env.CurrentUserPK == Staff.PK || Env.Security.StaffViewOtherStaffDetails.IsAllowed; }
		}

		public bool ViewWorkingAddressAllowed => Env.CurrentUserPK == Staff.PK || Env.Security.PersonIntelligenceViewPrimaryWorkplace.IsAllowed;

		#endregion

		#region Overrides

		#region Active

		public override ZBool Active => Staff.GS_IsActive;

		#endregion

		#region City

		public override ZString City => ViewWorkingAddressAllowed ? PrimarySource.City : ViewDeniedMessage;

		#endregion

		#region Description

		public override ZString Description
		{
			get
			{
				if (Staff == null)
				{
					return base.Unknown;
				}

				return Staff.Person != null
					? string.IsNullOrEmpty(Staff.Person.PER_LegalName)
						? Staff.GS_FullName
						: Staff.Person.PER_LegalName
					: Staff.GS_FullName;
			}
		}

		#endregion

		#region Grouping

		public override ZString Grouping => Enterprise.MasterFiles.Business.Res.GetString("PersonAssociationsStaffWrapper|Staff", "Staff");

		#endregion

		#region State

		public override ZString State => ViewWorkingAddressAllowed ? PrimarySource.State : ViewDeniedMessage;

		#endregion

		#region CreatedTime

		public override ZString CreatedTime => Staff.GS_SystemCreateTimeUtc.ToBestReadableDateString();

		#endregion

		#region Country

		public override ZString Country => ViewWorkingAddressAllowed ? PrimarySource.Country : ViewDeniedMessage;

		#endregion

		#region Email

		public override ZString Email => ViewAllowed ? Staff.GS_EmailAddress : ViewDeniedMessage;

		#endregion

		#region Primary Working Address

		public override ZBool IsPrimary
		{
			get => TreeModel.Person.PrimaryRelationship != null && TreeModel.Person.PrimaryRelationship.Primary.PK == Staff.PK;
			set
			{
				if (value)
				{
					TreeModel.Person.SetPrimaryRelationship(Staff);
				}
				else
				{
					TreeModel.Person.RemovePrimaryRelationship();
				}
			}
		}

		public override ZBool IsPrimary_Visible => true;

		public override ZString WorkingAddressUNLOCO => ViewWorkingAddressAllowed ? PrimarySource.UNLOCO : ViewDeniedMessage;

		public IGlbPersonPrimarySource PrimarySource => Staff;

		#endregion

		#endregion

		#region Methods

		internal ZString GetInitials(ZString fullName)
		{
			var nameParts = fullName.Trim().Split(' ');
			if (nameParts.Length != 2)
			{
				return ZString.Empty;
			}

			var firstInitial = nameParts[0].Substring(0, 1).ToUpper();
			var lastInitial = nameParts[1].Substring(0, 1).ToUpper();

			return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.", firstInitial, lastInitial);
		}

		internal ZString GetOfficeLocation(GlbBranch homeBranch)
		{
			if (homeBranch == null)
			{
				return ZString.Empty;
			}

			var city = homeBranch.City;
			return city.Length == 0 || city == Enterprise.MasterFiles.Business.Res.GetString("PersonAssociationsStaffWrapper|NotSet", "NOT SET")
				? ZString.Empty
				: (ZString)string.Format(CultureInfo.InvariantCulture, "{0} {1}", city,
					Enterprise.MasterFiles.Business.Res.GetString("PersonAssociationsStaffWrapper|Office", "Office"));
		}

		#endregion
	}
}
