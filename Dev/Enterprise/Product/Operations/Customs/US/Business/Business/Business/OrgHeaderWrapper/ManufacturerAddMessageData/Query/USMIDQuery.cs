using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Customs.US.Business
{
	public class USMIDQuery : NonPersistentBusinessObject, IObsoleteValidation
	{
		public USMIDQuery(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Schema
		public static class Schema
		{
			public const string US_AutoCreateOrganization = "US_AutoCreateOrganization";
			public const string US_MID = "US_MID";
			public const int US_MIDMaxLength = 15;
		}
		#endregion

		#region Properties

		public ZBool US_AutoCreateOrganization
		{
			get { return autoCreateOrganization; }
			set
			{
				if (SetNonPersistentPropertyValue(US_AutoCreateOrganizationInfo, ref autoCreateOrganization, value))
				{
					US_AutoCreateOrganizationInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					ValidateUS_AutoCreateOrganization();
				}
			}
		}
		ZBool autoCreateOrganization;

		public virtual ZPropertyInfo US_AutoCreateOrganizationInfo
		{
			get { return GetZPropertyInfo(Schema.US_AutoCreateOrganization); }
		}

		public ZString US_MID
		{
			get { return mid; }
			set
			{
				value = value.KeepAlphanumericCharacters();
				value = value.ConvertToWesternEuropeanCharacters();
				CheckMaximumLength(US_MIDInfo, value);
				if (SetNonPersistentPropertyValue(US_MIDInfo, ref mid, value))
				{
					US_MIDInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					ValidateUS_MID();
				}
			}
		}
		ZString mid;

		public virtual ZPropertyInfo US_MIDInfo
		{
			get { return GetZPropertyInfo(Schema.US_MID); }
		}

		void ValidateUS_MID()
		{
			US_MIDInfo.ClearAllNotifications();
			OrgCusCodeValidation.ValidateManufacturerID((ZPropertyInfoString)US_MIDInfo, Factory, address: null, checkDuplicates: US_AutoCreateOrganization);
		}

		void ValidateUS_AutoCreateOrganization()
		{
			US_AutoCreateOrganizationInfo.ClearAllNotifications();
			if (US_AutoCreateOrganization && !Env.Security.OrganisationNew.IsAllowed)
			{
				US_AutoCreateOrganizationInfo.AddError(UserCreateNewOrganizationError);
			}
		}
		public const string UserCreateNewOrganizationError = "User is not authorized to create new organizations.";

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateUS_MID();
			ValidateUS_AutoCreateOrganization();
		}

		#endregion
	}
}
