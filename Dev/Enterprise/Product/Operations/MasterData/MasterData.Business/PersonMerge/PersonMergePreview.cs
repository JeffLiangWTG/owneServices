using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class PersonMergePreview : NonPersistentBusinessObject
	{
		public PersonMergePreview()
		{
			DiscardedProperties = new CodeDescriptionPairList();
			CopiedProperties = new CodeDescriptionPairList();
			IdenticalProperties = new CodeDescriptionPairList();
			MergedProperties = new CodeDescriptionPairList();
		}

		public CodeDescriptionPairList MergedProperties { get; }

		public CodeDescriptionPairList DiscardedProperties { get; }

		public CodeDescriptionPairList CopiedProperties { get; }

		public CodeDescriptionPairList IdenticalProperties { get; }

		public List<string[]> ColumnGroupingsToMerge { get; } = new List<string[]>()
		{
			new [] { GlbPersonSchema.Constants.PER_FullName },
			new [] { GlbPersonSchema.Constants.PER_FriendlyName },
			new [] { GlbPersonSchema.Constants.PER_LegalName },
			new [] { GlbPersonSchema.Constants.PER_NameSuffix },
			new [] { GlbPersonSchema.Constants.PER_NameTitle },
			new [] { GlbPersonSchema.Constants.PER_Gender },
			new [] { GlbPersonSchema.Constants.PER_BirthDate },
			new [] { GlbPersonSchema.Constants.PER_DriversLicenseNumber },
			new [] { GlbPersonSchema.Constants.PER_PersonalInfo },
			new [] { GlbPersonSchema.Constants.PER_Picture },
			new [] { GlbPersonSchema.Constants.PER_PreferredLanguage },
			new [] { GlbPersonSchema.Constants.PER_RN_NKNationalityCodeISO },
			new [] { GlbPersonSchema.Constants.PER_HomeAddress1, GlbPersonSchema.Constants.PER_HomeAddress2, GlbPersonSchema.Constants.PER_City, GlbPersonSchema.Constants.PER_State, GlbPersonSchema.Constants.PER_Postcode, GlbPersonSchema.Constants.PER_RN_NKCountry },
			new [] { GlbPersonSchema.Constants.PER_HomePhone },
			new [] { GlbPersonSchema.Constants.PER_FaxNumber },
			new [] { GlbPersonSchema.Constants.PER_MobilePhone },
			new [] { GlbPersonSchema.Constants.PER_MobilePhone2 },
			new [] { GlbPersonSchema.Constants.PER_EmailAddress },
			new [] { GlbPersonSchema.Constants.PER_EmailAddress2 },
			new [] { GlbPersonSchema.Constants.PER_Passport, GlbPersonSchema.Constants.PER_PassportExpiryDate, GlbPersonSchema.Constants.PER_PassportPlaceOfIssue },
			new [] { GlbPersonSchema.Constants.PER_ChallengePhrase, GlbPersonSchema.Constants.PER_ChallengePhraseType },
			new [] { GlbPersonSchema.Constants.PER_PasswordHash, GlbPersonSchema.Constants.PER_PasswordHashIterations, GlbPersonSchema.Constants.PER_PasswordSalt },
			new [] { GlbPersonSchema.Constants.PER_WebAccessEnabled },
			new [] { GlbPersonSchema.Constants.PER_LoginDisabledUntilUtc },
			new [] { GlbPersonSchema.Constants.PER_IDPUserId }
	};

		public readonly List<string> addressColumns = new List<string>()
		{
			GlbPersonSchema.Constants.PER_HomeAddress1,
			GlbPersonSchema.Constants.PER_HomeAddress2,
			GlbPersonSchema.Constants.PER_City,
			GlbPersonSchema.Constants.PER_State,
			GlbPersonSchema.Constants.PER_Postcode,
			GlbPersonSchema.Constants.PER_RN_NKCountry,
		};
	}
}
