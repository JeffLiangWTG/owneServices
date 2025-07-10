using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class BoleroInvitationDetails : NonPersistentBusinessObject
	{
		public BoleroInvitationDetails(OrgHeader org)
		{
			Org = org;
		}

		public static class Schema
		{
			public const string SelectedContactPK = "SelectedContactPK";
			public const string SelectedContactEmail = "SelectedContactEmail";
			public const string YourName = "YourName";
			public const string YourEmail = "YourEmail";
			public const string YourMessage = "YourMessage";
		}

		public OrgHeader Org { get; }

		public OrgContactDependentCollection ActiveContacts
		{
			get
			{
				if (activeContacts == null)
				{
					var filter = new ZQuery(OrgContactSchema.OC_IsActive, ZBool.True);
					activeContacts = new OrgContactDependentCollection(Org, filter);
					activeContacts.Load();
					RegisterEditableChildObject(activeContacts);
				}

				return activeContacts;
			}
		}

		OrgContactDependentCollection activeContacts;

		[List("ActiveContacts")]
		public ZGuid SelectedContactPK
		{
			get { return selectedContactPK; }
			set
			{
				if (selectedContactPK != value)
				{
					selectedContactPK = value;
					Validation.ValidateSelectedContactPK();
					Validation.ValidateSelectedContactEmail();
					SelectedContactPKInfo.RefreshBinding();
				}
			}
		}

		ZGuid selectedContactPK;

		public ZPropertyInfo SelectedContactPKInfo => new StringPropertyInfoWithFixedDescription(this, Schema.SelectedContactPK);

		public ZString SelectedContactEmail
		{
			get
			{
				var contact = Org.Factory.Load<OrgContact>(SelectedContactPK);
				return contact?.Email ?? ZString.Empty;
			}
		}

		public ZPropertyInfo SelectedContactEmailInfo => new StringPropertyInfoWithFixedDescription(this, Schema.SelectedContactEmail);

		public ZString YourName => Env.CurrentUser.FullName;

		public ZPropertyInfo YourNameInfo => new StringPropertyInfoWithFixedDescription(this, Schema.YourName);

		public ZString YourEmail => Env.CurrentUser.EmailAddress;

		public ZPropertyInfo YourEmailInfo => new StringPropertyInfoWithFixedDescription(this, Schema.YourEmail);

		[MaxLength(2000)]
		public ZString YourMessage
		{
			get => yourMessage;
			set
			{
				if (yourMessage != value)
				{
					CheckMaximumLength(YourMessageInfo, value);
					yourMessage = value.Trim();
					YourMessageInfo.RefreshBinding();
				}
			}
		}

		ZString yourMessage;

		public ZPropertyInfo YourMessageInfo => new StringPropertyInfoWithFixedDescription(this, Schema.YourMessage);

		#region Validation

		public BoleroInvitationDetailsValidation Validation => GetNewValidation();

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		protected virtual BoleroInvitationDetailsValidation GetNewValidation() => new BoleroInvitationDetailsValidation(this);

		#endregion

		#region StringPropertyInfoWithFixedDescription

		class StringPropertyInfoWithFixedDescription : ZPropertyInfoString
		{
			public StringPropertyInfoWithFixedDescription(BusinessObject bizObj, string name) : base(bizObj, name)
			{
			}

			protected override ZString GetHumanReadableNameCore()
			{
				string result;
				switch (Name)
				{
					case Schema.SelectedContactPK:
						result = Res.GetString("1343ce58-bef8-4594-bcb7-bebc4818dbe2", "contact name");
						break;
					case Schema.SelectedContactEmail:
						result = Res.GetString("54e5d06e-f0d1-4d46-8a91-0462a2d9d8c4", "contact email address");
						break;
					case Schema.YourName:
						result = Res.GetString("750af380-e7b1-411e-8244-6f9cab521225", "login user's name");
						break;
					case Schema.YourEmail:
						result = Res.GetString("a01b79c6-7517-4e6f-9111-c403b28ff139", "login user's email address");
						break;
					default:
						result = Res.GetString("8966fbbc-bd96-4c5f-9e12-1715025b8865", "message");
						break;
				}

				return result;
			}
		}

		#endregion
	}
}
