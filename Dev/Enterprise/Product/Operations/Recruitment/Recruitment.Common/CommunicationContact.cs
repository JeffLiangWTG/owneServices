using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruitment.Common
{
	public class CommunicationContact : NonPersistentBusinessObject
	{
		public CommunicationContact(BusinessObjectFactory factory, CodeDescriptionPair position, GlbStaff staff)
			: base(factory)
		{
			base.ReadOnly = true;

			Position = position.Description;
			Staff = staff.GS_Code;
		}

		public CommunicationContact(BusinessObjectFactory factory, CodeDescriptionPair position, string email)
			: base(factory)
		{
			base.ReadOnly = true;

			Position = position.Description;
			Email = email;
		}

		internal CommunicationContact(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public static CommunicationContact CreateUncommittedRow(BusinessObjectFactory factory)
			=> new CommunicationContact(factory);

		public CodeDescriptionPairList Positions { get; }
		= new CodeDescriptionPairList()
		{
					CommunicationContactPositions.Manager,
					CommunicationContactPositions.InterestedParty,
					CommunicationContactPositions.SeniorDeveloper,
					CommunicationContactPositions.TeamLead,
					CommunicationContactPositions.OtherPosition,
					CommunicationContactPositions.Reference
		};

		public ZString Position
		{
			get => position;
			set
			{
				if (position == CommunicationContactPositions.Reference.Description || value == CommunicationContactPositions.Reference.Description)
				{
					Staff = ZString.Empty;
					Email = ZString.Empty;
				}

				_ = SetNonPersistentPropertyValue(PositionInfo, ref position, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateAll();
				}
			}
		}
		ZString position;

		public ZPropertyInfo PositionInfo => GetZPropertyInfo(nameof(Position));

		[MaxLength(3)]
		[List(nameof(StaffMembers))]
		public ZString Staff
		{
			get => staff;
			set
			{
				_ = SetNonPersistentPropertyValue(StaffInfo, ref staff, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateStaff();
				}
			}
		}
		ZString staff;

		protected bool Staff_ReadOnly => string.IsNullOrEmpty(Position) || Position == CommunicationContactPositions.Reference.Description;

		public ZPropertyInfo StaffInfo => GetZPropertyInfo(nameof(Staff));

		public GlbStaff AsGlbStaff => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, Staff);

		public virtual GlbStaffCollection StaffMembers => new GlbStaffCollection(Factory);

		public ZString Email
		{
			get => string.IsNullOrEmpty(Staff)
				? email
				: AsGlbStaff?.GS_EmailAddress ?? ZString.Empty;
			set
			{
				_ = SetNonPersistentPropertyValue(EmailInfo, ref email, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateEmail();
				}
			}
		}
		ZString email;

		protected bool Email_ReadOnly => Position != CommunicationContactPositions.Reference.Description;

		public ZPropertyInfo EmailInfo => GetZPropertyInfo(nameof(Email));

		public CommunicationContactValidation Validation => new CommunicationContactValidation(this);
	}
}
