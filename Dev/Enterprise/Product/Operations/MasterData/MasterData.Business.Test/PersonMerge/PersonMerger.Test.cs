using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterData.Business.Tests
{
	public class PersonMergerTest : TestCaseWithFactory
	{
		#region Constructor

		public void TestConstructor_ThrowsException_WithRetainedParam_IsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PersonMerger(null, Factory.NewWithValidTestData<GlbPerson>()));
		}

		public void TestConstructor_ThrowsException_WithDissolvedParam_IsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PersonMerger(Factory.NewWithValidTestData<GlbPerson>(), null));
		}

		public void TestConstructor_ThrowsException_WithTransactionSaver_IsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PersonMerger(Factory.NewWithValidTestData<GlbPerson>(), Factory.NewWithValidTestData<GlbPerson>(), null, null));
		}

		public void TestRetainedPerson_DoesNotHaveUnsavedChanges()
		{
			//	TODO: Add Tests to Ensure Retained Person does not have unsaved changes
			Assert(true);
		}

		public void TestDissolvedPerson_DoesNotHaveUnsavedChanges()
		{
			//	TODO: Add Tests to Ensure Dissolved Person does not have unsaved changes
			Assert(true);
		}

		public void TestRetainedPerson_ThrowsException_WhenNotSavedInDatabase()
		{
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();

			AssertExceptionThrown<InvalidOperationException>(() => new PersonMerger(retainedPerson, dissolvedPerson));
		}

		public void TestDissolvedPerson_ThrowsException_WhenNotSavedInDatabase()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();

			AssertExceptionThrown<InvalidOperationException>(() => new PersonMerger(retainedPerson, dissolvedPerson));
		}

		public void TestMergeWillNotSucceed_WhenDissolvedOrRetainedIsLockedByOthers()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				Assert(connection.TryGetLock((PersonMerger.DissolvedAndRetainedPersonLockKey + "," + dissolvedPerson.PK).ToUpperInvariant(), out var dissolvedAppLock));
				Assert(connection.TryGetLock((PersonMerger.DissolvedAndRetainedPersonLockKey + "," + retainedPerson.PK).ToUpperInvariant(), out var retainedAppLock));

				using (dissolvedAppLock)
				using (retainedAppLock)
				{
					AssertEquals("Should fail.", false, new PersonMerger(retainedPerson, dissolvedPerson).Merge());
				}
			}
		}

		#endregion

		#region CalculatePersonsMerge

		public void TestPrecalculatePersonsMerge_CorrectlyDetectsEmptyDate()
		{
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson.PER_BirthDate = new ZDate(2018, 10, 29);

			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_BirthDate = ZDate.Empty;

			Factory.Save();

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				CombineAssertions(() =>
				{
					AssertEquals(true, merger.MergePreview.CopiedProperties.ContainsCode(GlbPersonSchema.Constants.PER_BirthDate));
					AssertEquals(false, merger.MergePreview.DiscardedProperties.ContainsCode(GlbPersonSchema.Constants.PER_BirthDate));
					AssertEquals(false, merger.MergePreview.IdenticalProperties.ContainsCode(GlbPersonSchema.Constants.PER_BirthDate));
				});
			}
		}

		public void TestPrecalculatePersonsMerge_CorrectlyDetectsDateNotSet()
		{
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson.PER_BirthDate = new ZDate(2018, 10, 29);

			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_BirthDate = new ZDate();

			Factory.Save();

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				CombineAssertions(() =>
				{
					AssertEquals(true, merger.MergePreview.CopiedProperties.ContainsCode(GlbPersonSchema.Constants.PER_BirthDate));
					AssertEquals(false, merger.MergePreview.DiscardedProperties.ContainsCode(GlbPersonSchema.Constants.PER_BirthDate));
					AssertEquals(false, merger.MergePreview.IdenticalProperties.ContainsCode(GlbPersonSchema.Constants.PER_BirthDate));
				});
			}
		}

		public void TestPrecalculatePersonsMerge_CorrectlyDetectsInvalidDate()
		{
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson.PER_BirthDate = new ZDate(2018, 10, 29);

			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_BirthDate = ZDate.Invalid;

			Factory.Save();

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				CombineAssertions(() =>
				{
					AssertEquals(true, merger.MergePreview.CopiedProperties.ContainsCode(GlbPersonSchema.Constants.PER_BirthDate));
					AssertEquals(false, merger.MergePreview.DiscardedProperties.ContainsCode(GlbPersonSchema.Constants.PER_BirthDate));
					AssertEquals(false, merger.MergePreview.IdenticalProperties.ContainsCode(GlbPersonSchema.Constants.PER_BirthDate));
				});
			}
		}

		public void TestPrecalculatePersonsMerge_MixOfDiscardedCopiedAndRetainedFields()
		{
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson.PER_FullName = "Bob Smith";
			dissolvedPerson.PER_FriendlyName = "Bobby";
			dissolvedPerson.PER_EmailAddress = "a@b.com";
			dissolvedPerson.PER_City = "Newmarket";
			dissolvedPerson.PER_State = "Auckland";
			dissolvedPerson.PER_RN_NKNationalityCodeISO = "NZ";
			dissolvedPerson.PER_Gender = "M";
			dissolvedPerson.PER_PreferredLanguage = "English";
			dissolvedPerson.PER_NameTitle = "Mr.";
			dissolvedPerson.PER_BirthDate = new ZDate(1990, 10, 29);

			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";
			retainedPerson.PER_FriendlyName = "Bobby";
			retainedPerson.PER_EmailAddress = "a@b.com";
			retainedPerson.PER_City = "Sydney";
			retainedPerson.PER_State = "NSW";
			retainedPerson.PER_RN_NKNationalityCodeISO = "AU";
			retainedPerson.PER_Gender = "N";
			retainedPerson.PER_PreferredLanguage = string.Empty;
			retainedPerson.PER_NameTitle = string.Empty;
			retainedPerson.PER_BirthDate = new ZDate(1990, 01, 01);

			Factory.Save();

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder(new[]
					{
					GlbPersonSchema.Constants.PER_RN_NKNationalityCodeISO,
					GlbPersonSchema.Constants.PER_City,
					GlbPersonSchema.Constants.PER_State,
					GlbPersonSchema.Constants.PER_Gender,
					GlbPersonSchema.Constants.PER_BirthDate
					}, merger.MergePreview.DiscardedProperties.GetAllCodes());

					AssertContainsExactElementsInAnyOrder(new[]
					{
					GlbPersonSchema.Constants.PER_NameTitle,
					GlbPersonSchema.Constants.PER_PreferredLanguage
					}, merger.MergePreview.CopiedProperties.GetAllCodes());

					AssertContainsExactElementsInAnyOrder(new[]
					{
					GlbPersonSchema.Constants.PER_FullName,
					GlbPersonSchema.Constants.PER_FriendlyName,
					GlbPersonSchema.Constants.PER_EmailAddress,
					GlbPersonSchema.Constants.PER_WebAccessEnabled,
					GlbPersonSchema.Constants.PER_LoginDisabledUntilUtc
					}, merger.MergePreview.IdenticalProperties.GetAllCodes());
				});
			}
		}

		public void TestPrecalculatePersonsMerge_AllIdenticalProperties()
		{
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson.PER_FullName = "Bob Smith";
			dissolvedPerson.PER_FriendlyName = "Bobby";
			dissolvedPerson.PER_EmailAddress = "a@b.com";
			dissolvedPerson.PER_City = "Melbourne";
			dissolvedPerson.PER_State = "VIC";
			dissolvedPerson.PER_RN_NKNationalityCodeISO = "AU";
			dissolvedPerson.PER_Gender = "M";
			dissolvedPerson.PER_PreferredLanguage = "English";
			dissolvedPerson.PER_BirthDate = new ZDate(1990, 10, 29);

			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";
			retainedPerson.PER_FriendlyName = "Bobby";
			retainedPerson.PER_EmailAddress = "a@b.com";
			retainedPerson.PER_City = "Melbourne";
			retainedPerson.PER_State = "VIC";
			retainedPerson.PER_RN_NKNationalityCodeISO = "AU";
			retainedPerson.PER_Gender = "M";
			retainedPerson.PER_PreferredLanguage = "English";
			retainedPerson.PER_BirthDate = new ZDate(1990, 10, 29);

			Factory.Save();

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				CombineAssertions(() =>
				{
					AssertEquals(0, merger.MergePreview.DiscardedProperties.Count);
					AssertEquals(0, merger.MergePreview.CopiedProperties.Count);

					AssertContainsExactElementsInAnyOrder(new[]
					{
					GlbPersonSchema.Constants.PER_FullName,
					GlbPersonSchema.Constants.PER_FriendlyName,
					GlbPersonSchema.Constants.PER_EmailAddress,
					GlbPersonSchema.Constants.PER_City,
					GlbPersonSchema.Constants.PER_State,
					GlbPersonSchema.Constants.PER_RN_NKNationalityCodeISO,
					GlbPersonSchema.Constants.PER_Gender,
					GlbPersonSchema.Constants.PER_BirthDate,
					GlbPersonSchema.Constants.PER_PreferredLanguage,
					GlbPersonSchema.Constants.PER_WebAccessEnabled,
					GlbPersonSchema.Constants.PER_LoginDisabledUntilUtc
					}, merger.MergePreview.IdenticalProperties.GetAllCodes());
				});
			}
		}

		public void TestPrecalculatePersonsMerge_SingleCopiedProperty()
		{
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson.PER_FullName = "Bob Smith";
			dissolvedPerson.PER_NameTitle = "Dr.";
			dissolvedPerson.PER_FriendlyName = "Bobby";
			dissolvedPerson.PER_EmailAddress = "a@b.com";
			dissolvedPerson.PER_City = "Melbourne";
			dissolvedPerson.PER_State = "VIC";
			dissolvedPerson.PER_RN_NKNationalityCodeISO = "AU";
			dissolvedPerson.PER_Gender = "M";
			dissolvedPerson.PER_PreferredLanguage = "English";

			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";
			retainedPerson.PER_FriendlyName = "Bobby";
			retainedPerson.PER_EmailAddress = "a@b.com";
			retainedPerson.PER_City = "Melbourne";
			retainedPerson.PER_State = "VIC";
			retainedPerson.PER_RN_NKNationalityCodeISO = "AU";
			retainedPerson.PER_Gender = "M";
			retainedPerson.PER_PreferredLanguage = "English";

			Factory.Save();

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				CombineAssertions(() =>
				{
					AssertEquals(true, merger.MergePreview.CopiedProperties.ContainsOnly(GlbPersonSchema.Constants.PER_NameTitle));
					AssertEquals(0, merger.MergePreview.DiscardedProperties.Count);

					AssertContainsExactElementsInAnyOrder(new[]
					{
					GlbPersonSchema.Constants.PER_FullName,
					GlbPersonSchema.Constants.PER_FriendlyName,
					GlbPersonSchema.Constants.PER_EmailAddress,
					GlbPersonSchema.Constants.PER_City,
					GlbPersonSchema.Constants.PER_State,
					GlbPersonSchema.Constants.PER_RN_NKNationalityCodeISO,
					GlbPersonSchema.Constants.PER_Gender,
					GlbPersonSchema.Constants.PER_PreferredLanguage,
					GlbPersonSchema.Constants.PER_WebAccessEnabled,
					GlbPersonSchema.Constants.PER_LoginDisabledUntilUtc
					}, merger.MergePreview.IdenticalProperties.GetAllCodes());
				});
			}
		}

		public void TestPrecalculatePersonsMerge_DoesNotCopyPartialGroup()
		{
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson.PER_HomeAddress1 = "42 Wallaby Way";
			dissolvedPerson.PER_City = "Sydney";
			dissolvedPerson.PER_State = "NSW";

			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_City = "Alexandria";
			retainedPerson.PER_State = "NSW";

			Factory.Save();

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				CombineAssertions(() =>
				{
					AssertEquals(false, merger.MergePreview.CopiedProperties.ContainsCode(GlbPersonSchema.Constants.PER_HomeAddress1));

					AssertEquals(true, merger.MergePreview.DiscardedProperties.ContainsCode(GlbPersonSchema.Constants.PER_HomeAddress1));
					AssertEquals(true, merger.MergePreview.DiscardedProperties.ContainsCode(GlbPersonSchema.Constants.PER_City));

					AssertEquals(true, merger.MergePreview.IdenticalProperties.ContainsCode(GlbPersonSchema.Constants.PER_State));
				});
			}
		}

		public void TestPrecalculatePersonsMerge_MergedProperties()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";
			retainedPerson.PER_FriendlyName = "Bobby";
			retainedPerson.PER_Gender = "M";
			retainedPerson.PER_PasswordHash = new ZBlob(new byte[] { 2, 3, 4, 5 });
			retainedPerson.PER_PasswordHashIterations = 1;
			retainedPerson.PER_PasswordSalt = new ZBlob(new byte[] { 2, 3, 4, 5 });
			retainedPerson.PER_WebAccessEnabled = false;
			var idpUserId = Guid.NewGuid();
			retainedPerson.PER_IDPUserId = idpUserId;

			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson.PER_FullName = "Peter Smith";
			dissolvedPerson.PER_NameTitle = "Mr.";
			dissolvedPerson.PER_FriendlyName = "Peter";
			dissolvedPerson.PER_EmailAddress = "a@b.com";
			dissolvedPerson.PER_City = "Melbourne";
			dissolvedPerson.PER_Gender = "N";

			Factory.Save();

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				CombineAssertions(() =>
				{
					AssertEquals(12, merger.MergePreview.MergedProperties.Count);
					AssertCodeDescriptionPairListContainsPair(merger.MergePreview.MergedProperties, GlbPersonSchema.Constants.PER_FullName, "Bob Smith");
					AssertCodeDescriptionPairListContainsPair(merger.MergePreview.MergedProperties, GlbPersonSchema.Constants.PER_FriendlyName, "Bobby");
					AssertCodeDescriptionPairListContainsPair(merger.MergePreview.MergedProperties, GlbPersonSchema.Constants.PER_Gender, "Man");
					AssertCodeDescriptionPairListContainsPair(merger.MergePreview.MergedProperties, GlbPersonSchema.Constants.PER_NameTitle, "Mr.");
					AssertCodeDescriptionPairListContainsPair(merger.MergePreview.MergedProperties, GlbPersonSchema.Constants.PER_EmailAddress, "a@b.com");
					AssertCodeDescriptionPairListContainsPair(merger.MergePreview.MergedProperties, GlbPersonSchema.Constants.PER_City, "Melbourne");
					AssertCodeDescriptionPairListContainsPair(merger.MergePreview.MergedProperties, GlbPersonSchema.Constants.PER_WebAccessEnabled, "No");
					AssertCodeDescriptionPairListContainsPair(merger.MergePreview.MergedProperties, GlbPersonSchema.Constants.PER_PasswordHash, "System.Byte[]");
					AssertCodeDescriptionPairListContainsPair(merger.MergePreview.MergedProperties, GlbPersonSchema.Constants.PER_PasswordHashIterations, "1");
					AssertCodeDescriptionPairListContainsPair(merger.MergePreview.MergedProperties, GlbPersonSchema.Constants.PER_PasswordSalt, "System.Byte[]");
					AssertCodeDescriptionPairListContainsPair(merger.MergePreview.MergedProperties, GlbPersonSchema.Constants.PER_LoginDisabledUntilUtc, "");
					AssertCodeDescriptionPairListContainsPair(merger.MergePreview.MergedProperties, GlbPersonSchema.Constants.PER_IDPUserId, idpUserId.ToString());
				});
			}
		}

		public void TestPersonMerge_RetainingApplicant_DoesNotAddTemporarySuffixToApplicantEmail()
		{
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedApplicant = PersonAssociations.AddNewHRJobApplicantToPerson(Factory, dissolvedPerson);

			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var retainedContact = PersonAssociations.AddNewContactToPerson(Factory, retainedPerson);

			retainedContact.OC_ContactName = "John Smith";
			retainedPerson.PER_EmailAddress = "test@email.com";

			dissolvedPerson.PER_FullName = "John Smith";
			dissolvedApplicant.HA_EmailAddress = "test1@email.com";
			dissolvedPerson.PER_WebAccessEnabled = true;

			Factory.Save();

			using (var personMerger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				AssertCollectionContains("Precondition: Person has web access enabled", GlbPersonSchema.Constants.PER_WebAccessEnabled, personMerger.MergePreview.CopiedProperties.GetAllCodes());

				personMerger.Merge();

				AssertEquals("Job applicant email address should not contain +dissolved after merging", "test1@email.com", dissolvedApplicant.HA_EmailAddress);
			}
		}

		#endregion

		#region HumanReadableForm

		public void TestDisplayedGender_IsHumanReadableForm()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_Gender = Core.Constants.Genders.Woman;

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_Gender = Core.Constants.Genders.Woman;

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_Gender = Core.Constants.Genders.Man;

			var dissolvedPerson3 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson3.PER_Gender = Core.Constants.Genders.NotSpecified;

			var dissolvedPerson4 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson4.PER_Gender = Core.Constants.Genders.NonBinary;

			var dissolvedPerson5 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson5.PER_Gender = Core.Constants.Genders.Custom;

			var dissolvedPerson6 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson6.PER_Gender = Core.Constants.Genders.Agender;

			Factory.Save();

			using (var merger1 = new PersonMerger(retainedPerson, dissolvedPerson1))
			using (var merger2 = new PersonMerger(retainedPerson, dissolvedPerson2))
			using (var merger3 = new PersonMerger(retainedPerson, dissolvedPerson3))
			using (var merger4 = new PersonMerger(retainedPerson, dissolvedPerson4))
			using (var merger5 = new PersonMerger(retainedPerson, dissolvedPerson5))
			using (var merger6 = new PersonMerger(retainedPerson, dissolvedPerson6))
			{
				CombineAssertions(() =>
				{
					AssertCodeDescriptionPairListContainsPair(merger1.MergePreview.IdenticalProperties, GlbPersonSchema.Constants.PER_Gender, "Woman");
					AssertCodeDescriptionPairListContainsPair(merger2.MergePreview.DiscardedProperties, GlbPersonSchema.Constants.PER_Gender, "Man");
					AssertCodeDescriptionPairListContainsPair(merger3.MergePreview.DiscardedProperties, GlbPersonSchema.Constants.PER_Gender, "Not Specified");
					AssertCodeDescriptionPairListContainsPair(merger4.MergePreview.DiscardedProperties, GlbPersonSchema.Constants.PER_Gender, "Non-Binary");
					AssertCodeDescriptionPairListContainsPair(merger5.MergePreview.DiscardedProperties, GlbPersonSchema.Constants.PER_Gender, "Custom");
					AssertCodeDescriptionPairListContainsPair(merger6.MergePreview.DiscardedProperties, GlbPersonSchema.Constants.PER_Gender, "Agender");
				});
			}
		}

		public void TestDisplayedPreferredLanguage_IsHumanReadableForm()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_PreferredLanguage = Core.SharedConstants.Languages.English;

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_PreferredLanguage = Core.SharedConstants.Languages.EnglishBritish;

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_PreferredLanguage = Core.SharedConstants.Languages.Dutch;

			var dissolvedPerson3 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson3.PER_PreferredLanguage = Core.SharedConstants.Languages.ChineseSimplified;

			var dissolvedPerson4 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson4.PER_PreferredLanguage = "XY-ZZ";

			Factory.Save();

			using (var merger1 = new PersonMerger(retainedPerson, dissolvedPerson1))
			using (var merger2 = new PersonMerger(retainedPerson, dissolvedPerson2))
			using (var merger3 = new PersonMerger(retainedPerson, dissolvedPerson3))
			using (var merger4 = new PersonMerger(retainedPerson, dissolvedPerson4))
			{
				CombineAssertions(() =>
				{
					AssertCodeDescriptionPairListContainsPair(merger1.MergePreview.MergedProperties, GlbPersonSchema.Constants.PER_PreferredLanguage, "English");
					AssertCodeDescriptionPairListContainsPair(merger1.MergePreview.DiscardedProperties, GlbPersonSchema.Constants.PER_PreferredLanguage, "English (British)");
					AssertCodeDescriptionPairListContainsPair(merger2.MergePreview.DiscardedProperties, GlbPersonSchema.Constants.PER_PreferredLanguage, "Dutch");
					AssertCodeDescriptionPairListContainsPair(merger3.MergePreview.DiscardedProperties, GlbPersonSchema.Constants.PER_PreferredLanguage, "Chinese - Simplified");
					AssertCodeDescriptionPairListContainsPair(merger4.MergePreview.DiscardedProperties, GlbPersonSchema.Constants.PER_PreferredLanguage, "XY-ZZ");
				});
			}
		}

		public void TestDisplayedCountry_IsHumanReadableForm()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_RN_NKCountry = Core.Constants.CountryCodes.Denmark;

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;

			var dissolvedPerson3 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson3.PER_RN_NKCountry = Core.Constants.CountryCodes.Japan;

			var dissolvedPerson4 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson4.PER_RN_NKCountry = "ZY";

			Factory.Save();

			using (var merger1 = new PersonMerger(retainedPerson, dissolvedPerson1))
			using (var merger2 = new PersonMerger(retainedPerson, dissolvedPerson2))
			using (var merger3 = new PersonMerger(retainedPerson, dissolvedPerson3))
			using (var merger4 = new PersonMerger(retainedPerson, dissolvedPerson4))
			{
				CombineAssertions(() =>
				{
					AssertCodeDescriptionPairListContainsPair(merger1.MergePreview.MergedProperties, GlbPersonSchema.Constants.PER_RN_NKCountry, "Australia");
					AssertCodeDescriptionPairListContainsPair(merger1.MergePreview.DiscardedProperties, GlbPersonSchema.Constants.PER_RN_NKCountry, "Denmark");
					AssertCodeDescriptionPairListContainsPair(merger2.MergePreview.DiscardedProperties, GlbPersonSchema.Constants.PER_RN_NKCountry, "United States");
					AssertCodeDescriptionPairListContainsPair(merger3.MergePreview.DiscardedProperties, GlbPersonSchema.Constants.PER_RN_NKCountry, "Japan");
					AssertCodeDescriptionPairListContainsPair(merger4.MergePreview.DiscardedProperties, GlbPersonSchema.Constants.PER_RN_NKCountry, "ZY");
				});
			}
		}

		public void TestDisplayedPhoneNumber_IsHumanReadableForm()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_HomePhone = "+61 3 1415 9265";
			retainedPerson.PER_MobilePhone = "+61123456780";
			retainedPerson.PER_MobilePhone2 = "+61123456781";
			retainedPerson.PER_FaxNumber = "+61123456782";

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_HomePhone = "+61222333444";

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_MobilePhone = "11122223456";

			var dissolvedPerson3 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson3.PER_MobilePhone2 = "0431415927";

			var dissolvedPerson4 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson4.PER_FaxNumber = "(02)12349876";

			Factory.Save();

			using (var merger1 = new PersonMerger(retainedPerson, dissolvedPerson1))
			using (var merger2 = new PersonMerger(retainedPerson, dissolvedPerson2))
			using (var merger3 = new PersonMerger(retainedPerson, dissolvedPerson3))
			using (var merger4 = new PersonMerger(retainedPerson, dissolvedPerson4))
			{
				CombineAssertions(() =>
				{
					AssertCodeDescriptionPairListContainsPair(merger1.MergePreview.MergedProperties, GlbPersonSchema.Constants.PER_HomePhone, "+61 3 1415 9265");
					AssertCodeDescriptionPairListContainsPair(merger1.MergePreview.DiscardedProperties, GlbPersonSchema.Constants.PER_HomePhone, "+61 2 2233 3444");
					AssertCodeDescriptionPairListContainsPair(merger2.MergePreview.DiscardedProperties, GlbPersonSchema.Constants.PER_MobilePhone, "11122223456");
					AssertCodeDescriptionPairListContainsPair(merger3.MergePreview.DiscardedProperties, GlbPersonSchema.Constants.PER_MobilePhone2, "+61 431 415 927");
					AssertCodeDescriptionPairListContainsPair(merger4.MergePreview.DiscardedProperties, GlbPersonSchema.Constants.PER_FaxNumber, "+61 2 1234 9876");
				});
			}
		}

		#endregion

		#region MergeNote

		[TestDate(2000, 01, 01)]
		public void TestPersonMerge_WithSomeDiscardedProperties_ShouldCreateNote()
		{
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson.PER_FullName = "Bob Smith";
			dissolvedPerson.PER_FriendlyName = "Bobby";
			dissolvedPerson.PER_EmailAddress = "a@b.com";
			dissolvedPerson.PER_City = "Newmarket";
			dissolvedPerson.PER_State = "Auckland";
			dissolvedPerson.PER_Gender = "M";
			dissolvedPerson.PER_BirthDate = new ZDate(1990, 10, 29);

			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";
			retainedPerson.PER_FriendlyName = "Bobby";
			retainedPerson.PER_EmailAddress = "a@b.com";
			retainedPerson.PER_City = "Sydney";
			retainedPerson.PER_State = "NSW";
			retainedPerson.PER_Gender = "N";
			retainedPerson.PER_BirthDate = new ZDate(1990, 01, 01);

			Factory.Save();

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				AssertContainsExactElementsInAnyOrder(new[]
				{
					GlbPersonSchema.Constants.PER_City,
					GlbPersonSchema.Constants.PER_State,
					GlbPersonSchema.Constants.PER_Gender,
					GlbPersonSchema.Constants.PER_BirthDate
				}, merger.MergePreview.DiscardedProperties.GetAllCodes());

				merger.Merge();
			}

			var query = new ZQuery(StmNoteSchema.ST_ParentID, retainedPerson.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, "GlbPerson");
			query.AddToFilter(StmNoteSchema.ST_Description, "Properties Discarded During Merge");

			var loadedNotes = Factory.Load<StmNote>(query);

			AssertEquals("Should be exactly one merge note", 1, loadedNotes.Length);
			var mergeNote = loadedNotes.Single();

			var expectedNoteText = @"Person Bob Smith was merged into this Person on 01-Jan-00.
The following properties were discarded from Person Bob Smith during this process:
Gender: Man
Birth Date: 29-Oct-90
City: Newmarket
State: Auckland
";
			AssertEquals("Should be saved in the database", true, mergeNote.IsInDatabase);
			AssertEquals("Note content", expectedNoteText, mergeNote.ST_NoteDataAsText.ToString());
		}

		public void TestPersonMerge_WithNoDiscardedProperties_ShouldNotCreateNote()
		{
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson.PER_FullName = "Bob Smith";
			dissolvedPerson.PER_FriendlyName = "Bobby";
			dissolvedPerson.PER_EmailAddress = "a@b.com";
			dissolvedPerson.PER_City = "Newmarket";
			dissolvedPerson.PER_State = "Auckland";
			dissolvedPerson.PER_Gender = "M";
			dissolvedPerson.PER_BirthDate = new ZDate(1990, 10, 29);

			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";
			retainedPerson.PER_FriendlyName = "Bobby";
			retainedPerson.PER_EmailAddress = "a@b.com";
			retainedPerson.PER_City = string.Empty;
			retainedPerson.PER_State = string.Empty;
			retainedPerson.PER_Gender = "M";
			retainedPerson.PER_BirthDate = new ZDate(1990, 10, 29);

			Factory.Save();

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				AssertEquals("Should not have discarded properties", 0, merger.MergePreview.DiscardedProperties.Count);

				merger.Merge();
			}

			var query = new ZQuery(StmNoteSchema.ST_ParentID, retainedPerson.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, "GlbPerson");
			query.AddToFilter(StmNoteSchema.ST_Description, SQLComparisonOperator.Contains, "Properties Discarded During Merge");

			var loadedNotes = Factory.Load<StmNote>(query);

			AssertEquals("Should not have any merge note", 0, loadedNotes.Length);
		}

		public void TestPersonMerge_ContainingRelatedChildPerson_CreatesAnEDTLogWithChildCounts()
		{
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson.PER_FullName = "Dissolved";
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();

			var dissolvedOrgContact = Factory.NewWithValidTestData<OrgContact>();
			dissolvedOrgContact.OC_PER = dissolvedPerson.PK;
			dissolvedOrgContact.UpdateFromPerson(dissolvedPerson);

			var dissolvedGlbStaff = Factory.NewWithValidTestData<GlbStaff>();
			dissolvedGlbStaff.SetFromPerson(dissolvedPerson);

			var dissolvedHRJobApplicant = Factory.New<HRJobApplicant>();
			dissolvedHRJobApplicant.SetFromPerson(dissolvedPerson);

			Factory.Save();

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var query = new ZQuery(StmALogSchema.SL_Parent, retainedPerson.PK);
			query.AddToFilter(StmALogSchema.SL_Table, "GlbPerson");
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "Merged");
			var loadedLogs = Factory.Load<StmALog>(query);
			var loadedLog = loadedLogs.Single();

			CombineAssertions(() =>
			{
				AssertEquals("EDT", loadedLog.Event.SE_Code);
				AssertEquals($"Edited a record", loadedLog.SL_EventDescription);
				AssertEquals($"Merged 'Dissolved' into this person and inherited 1 staff, 1 contact, 1 job applicant.", loadedLog.SL_Reference);
			});
		}

		public void TestPersonMerge_ContainingRelatedChildPerson_CreatesAnEDTLogWithoutChildCounts()
		{
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPersonFullName = dissolvedPerson.PER_FullName;

			Factory.Save();

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var query = new ZQuery(StmALogSchema.SL_Parent, retainedPerson.PK);
			query.AddToFilter(StmALogSchema.SL_Table, "GlbPerson");
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "Merged");
			var loadedLogs = Factory.Load<StmALog>(query);
			var loadedLog = loadedLogs.Single();

			AssertEquals("EDT", loadedLog.Event.SE_Code);
			AssertEquals($"Edited a record", loadedLog.SL_EventDescription);
			AssertEquals($"Merged '{dissolvedPersonFullName}' into this person.", loadedLog.SL_Reference);
		}
		#endregion

		#region CopyPropertiesFromDissolvedPerson

		public void TestCopyPropertiesFromDissolvedPerson_WithSomePropertiesToCopy()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Jack Li";
			personRetained.PER_Gender = "M";
			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Jill Li";
			personDissolved.PER_HomeAddress1 = "72 O'Riordan Street";
			personDissolved.PER_MobilePhone = "0449743938";
			var idpUserId = Guid.NewGuid();
			personDissolved.PER_IDPUserId = idpUserId;
			Factory.Save();

			using (var personMerger = new PersonMerger(personRetained, personDissolved))
			{
				AssertContainsExactElementsInAnyOrder("Precondition: ", new[]
				{
					GlbPersonSchema.Constants.PER_HomeAddress1,
					GlbPersonSchema.Constants.PER_MobilePhone,
					GlbPersonSchema.Constants.PER_IDPUserId
				}, personMerger.MergePreview.CopiedProperties.GetAllCodes());

				personMerger.Merge();
			}
			CombineAssertions("Check valid properties for merging are copied", () =>
			{
				AssertEquals("Jack Li", personRetained.PER_FullName);
				AssertEquals("M", personRetained.PER_Gender);
				AssertEquals("0449743938", personRetained.PER_MobilePhone);
				AssertEquals("72 O'Riordan Street", personRetained.PER_HomeAddress1);
				AssertEquals(idpUserId, personRetained.PER_IDPUserId);
			});
		}

		public void TestCopyPropertiesFromDissolvedPerson_WebAccessEnabled()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Rand Al'Thor";
			personRetained.PER_WebAccessEnabled = false;

			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Lews Therin Telamon";
			personDissolved.PER_EmailAddress = "lanfear@theways.com";
			personDissolved.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_PasswordHashIterations = 9239;
			personDissolved.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_WebAccessEnabled = true;

			Factory.Save();

			using (var personMerger = new PersonMerger(personRetained, personDissolved))
			{
				AssertCollectionContains("Precondition: ", GlbPersonSchema.Constants.PER_WebAccessEnabled, personMerger.MergePreview.CopiedProperties.GetAllCodes());

				personMerger.Merge();
			}

			CombineAssertions("Check that WebAccessEnabled and related password fields are copied.", () =>
			{
				AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), personRetained.PER_PasswordHash);
				AssertEquals(9239, personRetained.PER_PasswordHashIterations);
				AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), personRetained.PER_PasswordSalt);
				AssertEquals(true, personRetained.PER_WebAccessEnabled);
			});
		}

		public void TestCopyPropertiesFromDissolvedPerson_ShouldRemoveContactPasswordFields()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Rand Al'Thor";
			personRetained.PER_WebAccessEnabled = false;
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = personRetained.PER_FullName;
			contact1.OC_PER = personRetained.PK;
			contact1.SetHashedPassword("1234");

			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Lews Therin Telamon";
			personDissolved.PER_EmailAddress = "lanfear@theways.com";
			personDissolved.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_PasswordHashIterations = 9239;
			personDissolved.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_WebAccessEnabled = true;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = personDissolved.PER_FullName;
			contact2.OC_PER = personDissolved.PK;
			contact2.SetHashedPassword("1234");

			Factory.Save();

			using (var personMerger = new PersonMerger(personRetained, personDissolved))
			{
				AssertCollectionContains("Precondition: ", GlbPersonSchema.Constants.PER_WebAccessEnabled, personMerger.MergePreview.CopiedProperties.GetAllCodes());

				personMerger.Merge();
			}

			CombineAssertions("Precondition: Check that WebAccessEnabled and related password fields are copied.", () =>
			{
				AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), personRetained.PER_PasswordHash);
				AssertEquals(9239, personRetained.PER_PasswordHashIterations);
				AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), personRetained.PER_PasswordSalt);
				AssertEquals(true, personRetained.PER_WebAccessEnabled);
			});

			CombineAssertions("Check that contact passwords are removed.", () =>
			{
				Assert(contact1.OC_PasswordHash.IsEmpty);
				Assert(contact1.OC_PasswordSalt.IsEmpty);
				AssertEquals(0, contact1.OC_PasswordHashIterations);
				Assert(contact2.OC_PasswordHash.IsEmpty);
				Assert(contact2.OC_PasswordSalt.IsEmpty);
				AssertEquals(0, contact2.OC_PasswordHashIterations);
			});
		}

		public void TestCopyPropertiesFromDissolvedPerson_NoPasswordOnRetainedPersonShouldCopyDissolvedPassword()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Rand Al'Thor";

			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Lews Therin Telamon";
			personDissolved.PER_EmailAddress = "lanfear@theways.com";
			personDissolved.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_PasswordHashIterations = 9239;
			personDissolved.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });

			Factory.Save();

			using (var personMerger = new PersonMerger(personRetained, personDissolved))
			{
				personMerger.Merge();
			}

			CombineAssertions("Check that password fields are copied.", () =>
			{
				AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), personRetained.PER_PasswordHash);
				AssertEquals(9239, personRetained.PER_PasswordHashIterations);
				AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), personRetained.PER_PasswordSalt);
			});

			AssertEquals("Should not send an email since no contacts applicable", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestCopyPropertiesFromDissolvedPerson_RetainedPersonPasswordPrecedence()
		{
			SystemDataRegistry.Instance.PersonMergeWithPasswordNotificationEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate() { EmailSubject = "nuts", EmailBody = "bolts" });
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Rand Al'Thor";
			personRetained.PER_PasswordHash = new ZBlob(new byte[] { 2, 3, 4, 5 });
			personRetained.PER_PasswordHashIterations = 2918;
			personRetained.PER_PasswordSalt = new ZBlob(new byte[] { 2, 3, 4, 5 });
			personRetained.PER_EmailAddress = "rand@al.com";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = personRetained.PK;
			contact1.OC_Email = "other@email.com";
			contact1.OC_ContactName = personRetained.PER_FullName;

			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Lews Therin Telamon";
			personDissolved.PER_EmailAddress = "lanfear@theways.com";
			personDissolved.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_PasswordHashIterations = 9239;
			personDissolved.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_PER = personDissolved.PK;
			contact2.OC_Email = "another@one.com";
			contact2.OC_ContactName = personDissolved.PER_FullName;

			Factory.Save();

			using (var personMerger = new PersonMerger(personRetained, personDissolved))
			{
				personMerger.Merge();
			}

			AssertEquals("Precondition", "rand@al.com", personRetained.PER_EmailAddress);
			CombineAssertions("Check that password fields are retained.", () =>
			{
				AssertEquals(new ZBlob(new byte[] { 2, 3, 4, 5 }), personRetained.PER_PasswordHash);
				AssertEquals(2918, personRetained.PER_PasswordHashIterations);
				AssertEquals(new ZBlob(new byte[] { 2, 3, 4, 5 }), personRetained.PER_PasswordSalt);
			});

			AssertEquals("Should send an email to personal email address", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var mergeNotificationEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Should send to the personal email addresses", 2, mergeNotificationEmail.Recipients.Count);
			AssertContains("nuts", mergeNotificationEmail.Subject);
			AssertContains("bolts", mergeNotificationEmail.Body);
		}

		public void TestCopyPropertiesFromDissolvedPerson_MultiMergeShouldNotSendEmail()
		{
			SystemDataRegistry.Instance.PersonMergeWithPasswordNotificationEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate() { EmailSubject = "nuts", EmailBody = "bolts" });
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Rand Al'Thor";
			personRetained.PER_PasswordHash = new ZBlob(new byte[] { 2, 3, 4, 5 });
			personRetained.PER_PasswordHashIterations = 2918;
			personRetained.PER_PasswordSalt = new ZBlob(new byte[] { 2, 3, 4, 5 });
			personRetained.PER_EmailAddress = "rand@al.com";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = personRetained.PK;
			contact1.OC_Email = "other@email.com";
			contact1.OC_ContactName = personRetained.PER_FullName;

			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Lews Therin Telamon";
			personDissolved.PER_EmailAddress = "lanfear@theways.com";
			personDissolved.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_PasswordHashIterations = 9239;
			personDissolved.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_PER = personDissolved.PK;
			contact2.OC_Email = "another@one.com";
			contact2.OC_ContactName = personDissolved.PER_FullName;

			Factory.Save();

			using (var personMerger = new PersonMerger(personRetained, personDissolved))
			{
				personMerger.Merge(PersonMergeMode.Multi);
			}

			AssertEquals("Precondition", "rand@al.com", personRetained.PER_EmailAddress);
			CombineAssertions("Precondition: Check that password fields are retained.", () =>
			{
				AssertEquals(new ZBlob(new byte[] { 2, 3, 4, 5 }), personRetained.PER_PasswordHash);
				AssertEquals(2918, personRetained.PER_PasswordHashIterations);
				AssertEquals(new ZBlob(new byte[] { 2, 3, 4, 5 }), personRetained.PER_PasswordSalt);
			});

			AssertEquals("Should not send any emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestMergeShouldNotSendEmailIfSaveFailed()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Rand Al'Thor";
			personRetained.PER_PasswordHash = new ZBlob(new byte[] { 2, 3, 4, 5 });
			personRetained.PER_PasswordHashIterations = 2918;
			personRetained.PER_PasswordSalt = new ZBlob(new byte[] { 2, 3, 4, 5 });
			personRetained.PER_EmailAddress = "rand@al.com";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = personRetained.PK;
			contact1.OC_Email = "other@email.com";
			contact1.OC_ContactName = personRetained.PER_FullName;

			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Lews Therin Telamon";
			personDissolved.PER_EmailAddress = "lanfear@theways.com";
			personDissolved.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_PasswordHashIterations = 9239;
			personDissolved.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });

			Factory.Save();

			var failSaver = new PersonMergeTransactionSaverForTransactionFailedTest();

			using (var personMerger = new PersonMerger(personRetained, personDissolved, failSaver, new PersonMergeBusinessObjectFactoryLoader()))
			{
				personMerger.Merge();
			}

			AssertEquals("Should not send any emails since save failed", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestCopyPropertiesFromDissolvedPerson_WithAllPropertiesToCopy()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Jill Li";

			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Juli";
			personDissolved.PER_FriendlyName = "Rabbit Ju";
			personDissolved.PER_LegalName = "Juliana";
			personDissolved.PER_NameSuffix = "Ju";
			personDissolved.PER_NameTitle = "Title";
			personDissolved.PER_Gender = "M";
			personDissolved.PER_BirthDate = new ZDate(2011, 6, 9);
			personDissolved.PER_DriversLicenseNumber = "26925401690";
			personDissolved.PER_PersonalInfo = "C# developer";
			personDissolved.PER_Picture = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_PreferredLanguage = "English";
			personDissolved.PER_RN_NKNationalityCodeISO = "00";
			personDissolved.PER_HomeAddress2 = "72 O'Riordan Street";
			personDissolved.PER_City = "London";
			personDissolved.PER_State = "State";
			personDissolved.PER_Postcode = "212006";
			personDissolved.PER_RN_NKCountry = "01";
			personDissolved.PER_HomeAddress1 = "72 O'Riordan Street";
			personDissolved.PER_HomePhone = "0449743928";
			personDissolved.PER_FaxNumber = "025-0449743928";
			personDissolved.PER_MobilePhone = "15950003749";
			personDissolved.PER_MobilePhone2 = "12742301312";
			personDissolved.PER_EmailAddress = "abc@hotmail.com";
			personDissolved.PER_EmailAddress2 = "123@163.com";
			personDissolved.PER_Passport = "1892498724-08141";
			personDissolved.PER_PassportExpiryDate = new ZDate(2200, 12, 30);
			personDissolved.PER_PassportPlaceOfIssue = "02";
			personDissolved.PER_ChallengePhrase = "01";
			personDissolved.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_ChallengePhraseType = "01";
			personDissolved.PER_PasswordHashIterations = 9239;
			personDissolved.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_WebAccessEnabled = true;
			Factory.Save();

			using (var personMerger = new PersonMerger(personRetained, personDissolved))
			{
				AssertContainsExactElementsInAnyOrder("Precondition: ", new[]
				{
				GlbPersonSchema.Constants.PER_FriendlyName,
				GlbPersonSchema.Constants.PER_LegalName,
				GlbPersonSchema.Constants.PER_NameSuffix,
				GlbPersonSchema.Constants.PER_NameTitle,
				GlbPersonSchema.Constants.PER_BirthDate,
				GlbPersonSchema.Constants.PER_DriversLicenseNumber,
				GlbPersonSchema.Constants.PER_PersonalInfo,
				GlbPersonSchema.Constants.PER_Picture,
				GlbPersonSchema.Constants.PER_PreferredLanguage,
				GlbPersonSchema.Constants.PER_RN_NKNationalityCodeISO,
				GlbPersonSchema.Constants.PER_HomeAddress2,
				GlbPersonSchema.Constants.PER_City,
				GlbPersonSchema.Constants.PER_State,
				GlbPersonSchema.Constants.PER_Postcode,
				GlbPersonSchema.Constants.PER_RN_NKCountry,
				GlbPersonSchema.Constants.PER_HomeAddress1,
				GlbPersonSchema.Constants.PER_HomePhone,
				GlbPersonSchema.Constants.PER_FaxNumber,
				GlbPersonSchema.Constants.PER_MobilePhone,
				GlbPersonSchema.Constants.PER_MobilePhone2,
				GlbPersonSchema.Constants.PER_EmailAddress,
				GlbPersonSchema.Constants.PER_EmailAddress2,
				GlbPersonSchema.Constants.PER_Passport,
				GlbPersonSchema.Constants.PER_PassportExpiryDate,
				GlbPersonSchema.Constants.PER_PassportPlaceOfIssue,
				GlbPersonSchema.Constants.PER_ChallengePhrase,
				GlbPersonSchema.Constants.PER_ChallengePhraseType,
				GlbPersonSchema.Constants.PER_PasswordHash,
				GlbPersonSchema.Constants.PER_PasswordHashIterations,
				GlbPersonSchema.Constants.PER_PasswordSalt,
				GlbPersonSchema.Constants.PER_WebAccessEnabled
			}, personMerger.MergePreview.CopiedProperties.GetAllCodes());

				personMerger.Merge();
			}

			CombineAssertions("Check all valid properties for merging are copied", () =>
			{
				AssertEquals("Rabbit Ju", personRetained.PER_FriendlyName);
				AssertEquals("Juliana", personRetained.PER_LegalName);
				AssertEquals("Ju", personRetained.PER_NameSuffix);
				AssertEquals("Title", personRetained.PER_NameTitle);
				AssertEquals(new ZDate(2011, 6, 9), personRetained.PER_BirthDate);
				AssertEquals("26925401690", personRetained.PER_DriversLicenseNumber);
				AssertEquals("C# developer", personRetained.PER_PersonalInfo);
				AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), personRetained.PER_Picture);
				AssertEquals("English", personRetained.PER_PreferredLanguage);
				AssertEquals("00", personRetained.PER_RN_NKNationalityCodeISO);
				AssertEquals("72 O'Riordan Street", personRetained.PER_HomeAddress2);
				AssertEquals("London", personRetained.PER_City);
				AssertEquals("State", personRetained.PER_State);
				AssertEquals("212006", personRetained.PER_Postcode);
				AssertEquals("01", personRetained.PER_RN_NKCountry);
				AssertEquals("72 O'Riordan Street", personRetained.PER_HomeAddress1);
				AssertEquals("0449743928", personRetained.PER_HomePhone);
				AssertEquals("025-0449743928", personRetained.PER_FaxNumber);
				AssertEquals("15950003749", personRetained.PER_MobilePhone);
				AssertEquals("12742301312", personRetained.PER_MobilePhone2);
				AssertEquals("abc@hotmail.com", personRetained.PER_EmailAddress);
				AssertEquals("123@163.com", personRetained.PER_EmailAddress2);
				AssertEquals("1892498724-08141", personRetained.PER_Passport);
				AssertEquals(new ZDate(2200, 12, 30), personRetained.PER_PassportExpiryDate);
				AssertEquals("02", personRetained.PER_PassportPlaceOfIssue);
				AssertEquals("01", personRetained.PER_ChallengePhrase);
				AssertEquals("01", personRetained.PER_ChallengePhraseType);
				AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), personRetained.PER_PasswordHash);
				AssertEquals(9239, personRetained.PER_PasswordHashIterations);
				AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), personRetained.PER_PasswordSalt);
				AssertEquals(true, personRetained.PER_WebAccessEnabled);
			});
		}

		public void TestCopyPropertiesFromDissolvedPerson_DoesNotCopyDiscardedProperties()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Juli Wang";
			personRetained.PER_MobilePhone = "0449743928";
			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_MobilePhone = "0449743938";
			personDissolved.PER_FullName = "Jill Li";
			Factory.Save();

			using (var personMerger = new PersonMerger(personRetained, personDissolved))
			{
				AssertEquals("Precondition: ", true, personMerger.MergePreview.CopiedProperties.GetAllCodes().IsNullOrEmpty());

				personMerger.Merge();
				CombineAssertions("Check discarded properties are excluded", () =>
				{
					AssertEquals("0449743928", personRetained.PER_MobilePhone);
					AssertEquals(false, personRetained.PER_MobilePhoneInfo.HasChanges);
				});
			}
		}

		public void TestCopyPropertiesFromDissolvedPerson_WithEmptyPropertiesToCopy()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Jack Li";
			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Jill Li";
			Factory.Save();

			using (var personMerger = new PersonMerger(personRetained, personDissolved))
			{
				AssertEquals("Precondition: ", true, personMerger.MergePreview.CopiedProperties.GetAllCodes().IsNullOrEmpty());

				personMerger.Merge();
				CombineAssertions("Check no properties are copied", () =>
				{
					AssertEquals("Jack Li", personRetained.PER_FullName);
					AssertEquals(false, personRetained.PER_FullNameInfo.HasChanges);
				});
			}
		}

		public void TestPersonMerge_WithMultipleMerges_ShouldNotThrowErrors()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";
			retainedPerson.PER_FriendlyName = "Bobby";
			retainedPerson.PER_EmailAddress = "a@b.com";
			retainedPerson.PER_City = "Sydney";
			retainedPerson.PER_State = "NSW";
			retainedPerson.PER_Gender = "N";
			retainedPerson.PER_BirthDate = new ZDate(1990, 01, 01);

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Bob Smith";
			dissolvedPerson1.PER_FriendlyName = "Bobby";
			dissolvedPerson1.PER_EmailAddress = "a@b.com";
			dissolvedPerson1.PER_City = "Newmarket";
			dissolvedPerson1.PER_State = "Auckland";
			dissolvedPerson1.PER_Gender = "M";
			dissolvedPerson1.PER_BirthDate = new ZDate(1990, 10, 29);

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Ming Ling";
			dissolvedPerson2.PER_FriendlyName = "Ezekiel";
			dissolvedPerson2.PER_EmailAddress = "ml@izekiel.com";
			dissolvedPerson2.PER_City = "Timbuktu";
			dissolvedPerson2.PER_State = "Timbuktu Region";
			dissolvedPerson2.PER_Gender = "F";
			dissolvedPerson2.PER_BirthDate = new ZDate(1420, 1, 23);

			var dissolvedPerson3 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Liu Jianguo";
			dissolvedPerson2.PER_FriendlyName = "Abdul";
			dissolvedPerson2.PER_EmailAddress = "LJ@abdul.com";
			dissolvedPerson2.PER_City = "Champagne";
			dissolvedPerson2.PER_State = "Reims";
			dissolvedPerson2.PER_Gender = "F";
			dissolvedPerson2.PER_BirthDate = new ZDate(1911, 1, 19);

			Factory.Save();

			using (var merger1 = new PersonMerger(retainedPerson, dissolvedPerson1))
			{
				AssertNoExceptionThrown(() =>
				{
					merger1.Merge();
				});
			}

			using (var merger2 = new PersonMerger(retainedPerson, dissolvedPerson2))
			{
				AssertNoExceptionThrown(() =>
				{
					merger2.Merge();
				});
			}

			using (var merger3 = new PersonMerger(retainedPerson, dissolvedPerson3))
			{
				AssertNoExceptionThrown(() =>
				{
					merger3.Merge();
				});
			}

			var query = new ZQuery(StmNoteSchema.ST_ParentID, retainedPerson.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, "GlbPerson");
			query.AddToFilter(StmNoteSchema.ST_Description, SQLComparisonOperator.Contains, "Properties Discarded During Merge");

			var loadedNotes = Factory.Load<StmNote>(query);

			AssertEquals("Should be 3 merge notes", 3, loadedNotes.Length);
		}

		public void TestPersonMerge_DissolvedPersonHasHRJobApplicantAndWebAccessEnabled_ShouldNotThrowAnError()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Abra Kedabra";
			retainedPerson.PER_EmailAddress = string.Empty;
			retainedPerson.PER_WebAccessEnabled = false;

			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson.PER_FullName = "Abra Kedabra";
			dissolvedPerson.PER_EmailAddress = "akedabra@test.com";
			dissolvedPerson.PER_WebAccessEnabled = true;

			var dissolvedHRJobApplicant = Factory.NewWithValidTestData<HRJobApplicant>();
			dissolvedHRJobApplicant.HA_PER = dissolvedPerson.PK;
			dissolvedHRJobApplicant.HA_FullName = dissolvedPerson.PER_FullName;
			dissolvedHRJobApplicant.HA_EmailAddress = dissolvedPerson.PER_EmailAddress;
			dissolvedHRJobApplicant.HA_Gender = Core.Constants.Genders.Man; //Make it different to retained person's gender so that it will be flagged to be saved in merge process

			Factory.Save();

			string msg = null;
			using (var personMerger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				try
				{
					personMerger.Merge();
				}
				catch (Exception ex)
				{
					msg = ex.Message;
				}
			}

			AssertNull("Should not throw an error message", msg);

			var newFactory = new BusinessObjectFactory();
			var newRetainedPerson = newFactory.Load<GlbPerson>(retainedPerson.PK);

			CombineAssertions("After merge", () =>
			{
				AssertEquals("Retained person's email address", "akedabra@test.com", retainedPerson.PER_EmailAddress);
				AssertEquals("Retained person's web access enabled", true, newRetainedPerson.PER_WebAccessEnabled);
			});
		}

		public void TestPersonMerge_CreateGlbPersonIdentifier()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Jack Li";
			personRetained.PER_Gender = "M";
			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Jill Li";
			personDissolved.PER_HomeAddress1 = "72 O'Riordan Street";
			personDissolved.PER_MobilePhone = "0449743938";
			Factory.Save();

			using (var personMerger = new PersonMerger(personRetained, personDissolved))
			{
				AssertContainsExactElementsInAnyOrder("Precondition: ", new[]
				{
					GlbPersonSchema.Constants.PER_HomeAddress1,
					GlbPersonSchema.Constants.PER_MobilePhone
				}, personMerger.MergePreview.CopiedProperties.GetAllCodes());

				personMerger.Merge();
			}
			CombineAssertions("Check valid properties for merging are copied", () =>
			{
				AssertEquals("Jack Li", personRetained.PER_FullName);
				AssertEquals("M", personRetained.PER_Gender);
				AssertEquals("0449743938", personRetained.PER_MobilePhone);
				AssertEquals("72 O'Riordan Street", personRetained.PER_HomeAddress1);
			});

			var query = new ZQuery(new ZQuery(GlbMergedPersonSchema.GMP_PER_Person, personRetained.PK));
			var mergedPerson = Factory.Load<GlbMergedPerson>(query);
			AssertEquals(1, mergedPerson.Length);
			AssertEquals(personDissolved.PK, mergedPerson[0].GMP_MergedPerson);
		}

		#endregion

		#region PersonMergeTransactionSaver

		public void TestPersonMergeTransactionSaver_ContactsRollsBack_WhenExceptionIsThrownInKernel()
		{
			var retainedContact = PersonAssociations.CreateNewContact(Factory);
			retainedContact.OC_HomePhone = "+61111111111";
			retainedContact.OC_ContactName = "Retained";

			var dissolvedContact = PersonAssociations.CreateNewContact(Factory);
			dissolvedContact.OC_ContactName = "Dissolved";
			dissolvedContact.OC_HomePhone = "+61333333333";

			Factory.Save();

			var retainedPerson = retainedContact.Person;
			var dissolvedPerson = dissolvedContact.Person;

			var retainedContactPK = retainedContact.PK;
			var dissolvedContactPK = dissolvedContact.PK;
			var retainedPersonPK = retainedPerson.PK;
			var dissolvedPersonPK = dissolvedPerson.PK;

			var loadedRetainedContact1_BeforeMerging = Factory.LoadTop1<OrgContact>(new ZQuery(OrgContactSchema.PK, retainedContactPK));
			var loadedDissolvedContact1_BeforeMerging = Factory.LoadTop1<OrgContact>(new ZQuery(OrgContactSchema.PK, dissolvedContactPK));

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("+61111111111", loadedRetainedContact1_BeforeMerging.OC_HomePhone);
				AssertEquals("+61333333333", loadedDissolvedContact1_BeforeMerging.OC_HomePhone);
				AssertEquals("Retained", loadedRetainedContact1_BeforeMerging.OC_ContactName);
				AssertEquals("Dissolved", loadedDissolvedContact1_BeforeMerging.OC_ContactName);
			});

			var hasException = false;
			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson, new PersonMergeTransactionSaverForTest(true), new PersonMergeBusinessObjectFactoryLoader()))
			{
				try
				{
					merger.Merge();
				}
				catch (Exception)
				{
					hasException = true;
				}
			}

			var loadedRetainedContact1 = Factory.LoadTop1<OrgContact>(new ZQuery(OrgContactSchema.PK, retainedContactPK));
			var loadedDissolvedContact1 = Factory.LoadTop1<OrgContact>(new ZQuery(OrgContactSchema.PK, dissolvedContactPK));

			CombineAssertions("Everything should rollback", () =>
			{
				AssertEquals("Exception was thrown in the process", true, hasException);
				AssertEquals("retainedContact1 properties should remain unchanged", "+61111111111", loadedRetainedContact1.OC_HomePhone);
				AssertEquals("dissolvedContact1 properties should remain unchanged", "+61333333333", loadedDissolvedContact1.OC_HomePhone);
				AssertEquals("retainedContact1 properties should remain unchanged", "Retained", loadedRetainedContact1.OC_ContactName);
				AssertEquals("dissolvedContact1 properties should remain unchanged", "Dissolved", loadedDissolvedContact1.OC_ContactName);
			});
		}

		public void TestPersonMergeTransactionSaver_StaffsRollBack_WhenExceptionIsThrownInKernel()
		{
			var retainedStaff = PersonAssociations.CreateNewStaff(Factory);
			retainedStaff.GS_FullName = "Retained";
			retainedStaff.GS_HomePhone = "+61111111111";

			var dissolvedStaff = PersonAssociations.CreateNewStaff(Factory);
			dissolvedStaff.GS_FullName = "Dissolved";
			dissolvedStaff.GS_HomePhone = "+61333333333";

			Factory.Save();

			var retainedPerson = retainedStaff.Person;
			var dissolvedPerson = dissolvedStaff.Person;

			var retainedStaffPK = retainedStaff.PK;
			var dissolvedStaffPK = dissolvedStaff.PK;
			var retainedPersonPK = retainedPerson.PK;
			var dissolvedPersonPK = dissolvedPerson.PK;

			var loadedRetainedStaff_BeforeMerging = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, retainedStaffPK));
			var loadedDissolvedStaff_BeforeMerging = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, dissolvedStaffPK));

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("+61111111111", loadedRetainedStaff_BeforeMerging.GS_HomePhone);
				AssertEquals("+61333333333", loadedDissolvedStaff_BeforeMerging.GS_HomePhone);
				AssertEquals("Retained", loadedRetainedStaff_BeforeMerging.GS_FullName);
				AssertEquals("Dissolved", loadedDissolvedStaff_BeforeMerging.GS_FullName);
			});

			var hasException = false;
			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson, new PersonMergeTransactionSaverForTest(true), new PersonMergeBusinessObjectFactoryLoader()))
			{
				try
				{
					merger.Merge();
				}
				catch (Exception)
				{
					hasException = true;
				}
			}

			var loadedRetainedStaff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, retainedStaffPK));
			var loadedDissolvedStaff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, dissolvedStaffPK));

			CombineAssertions("Everything should rollback", () =>
			{
				AssertEquals("Exception was thrown in the process", true, hasException);
				AssertEquals("retainedStaff properties should remain unchanged", "+61111111111", loadedRetainedStaff.GS_HomePhone);
				AssertEquals("dissolvedStaff properties should remain unchanged", "+61333333333", loadedDissolvedStaff.GS_HomePhone);
				AssertEquals("retainedStaff properties should remain unchanged", "Retained", loadedRetainedStaff.GS_FullName);
				AssertEquals("dissolvedStaff properties should remain unchanged", "Dissolved", loadedDissolvedStaff.GS_FullName);
			});
		}

		public void TestPersonMergeTransactionSaver_ApplicantsRollBack_WhenExceptionIsThrownInKernel()
		{
			var retainedApplicant = PersonAssociations.CreateNewApplicant(Factory);
			Factory.Save();
			var retainedPerson = retainedApplicant.Person;
			retainedPerson.PER_FullName = "Retained";
			retainedPerson.PER_HomePhone = "+61111111111";

			var dissolvedApplicant = PersonAssociations.CreateNewApplicant(Factory);
			Factory.Save();
			var dissolvedPerson = dissolvedApplicant.Person;
			dissolvedPerson.PER_FullName = "Dissolved";
			dissolvedPerson.PER_HomePhone = "+63333333333";

			Factory.Save();

			var retainedApplicantPK = retainedApplicant.PK;
			var dissolvedApplicantPK = dissolvedApplicant.PK;
			var dissolvedPersonPK = dissolvedPerson.PK;
			var retainedPersonPK = retainedPerson.PK;

			var loadedRetainedApplicant_BeforeMerging = Factory.LoadTop1<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.PK, retainedApplicantPK));
			var loadedDissolvedApplicant_BeforeMerging = Factory.LoadTop1<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.PK, dissolvedApplicantPK));

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("+61111111111", loadedRetainedApplicant_BeforeMerging.HA_HomePhone);
				AssertEquals("+63333333333", loadedDissolvedApplicant_BeforeMerging.HA_HomePhone);
				AssertEquals("Retained", loadedRetainedApplicant_BeforeMerging.HA_FullName);
				AssertEquals("Dissolved", loadedDissolvedApplicant_BeforeMerging.HA_FullName);
			});

			var hasException = false;
			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson, new PersonMergeTransactionSaverForTest(true), new PersonMergeBusinessObjectFactoryLoader()))
			{
				try
				{
					merger.Merge();
				}
				catch (Exception)
				{
					hasException = true;
				}
			}

			var loadedRetainedApplicant = Factory.LoadTop1<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.PK, retainedApplicantPK));
			var loadedDissolvedApplicant = Factory.LoadTop1<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.PK, dissolvedApplicantPK));

			CombineAssertions("Everything should rollback", () =>
			{
				AssertEquals("Exception was thrown in the process", true, hasException);
				AssertEquals("retainedAppliacnt properties should remain unchanged", "+61111111111", loadedRetainedApplicant.HA_HomePhone);
				AssertEquals("dissolvedAppliacnt properties should remain unchanged", "+63333333333", loadedDissolvedApplicant.HA_HomePhone);
				AssertEquals("retainedAppliacnt properties should remain unchanged", "Retained", loadedRetainedApplicant.HA_FullName);
				AssertEquals("dissolvedAppliacnt properties should remain unchanged", "Dissolved", loadedDissolvedApplicant.HA_FullName);
			});
		}

		public void TestPersonMergeTransactionSaver_DeletesDissolvedPerson()
		{
			var factory = new BusinessObjectFactory();
			var personRetained = factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Jack Li";
			personRetained.PER_Gender = "M";

			var personDissolved = factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Jill Li";
			personDissolved.PER_HomeAddress1 = "72 O'Riordan Street";
			personDissolved.PER_MobilePhone = "0449743938";
			factory.Save();

			using (var personMerger = new PersonMerger(personRetained, personDissolved))
			{
				personMerger.Merge();
			}
			AssertEquals("Dissolved person should be deleted.", false, Factory.ExistsInDatabase(AutoGlbPerson.Schema.TableName, new ZQuery(GlbPersonSchema.PK, personDissolved.PK)));
		}

		public void TestPersonMergeTransactionSaver_SuccessStatusIsTrue_WhenMergingSuccessful()
		{
			var transactionSaver = new PersonMergeTransactionSaverForTest();
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			using (var personMerger = new PersonMerger(personRetained, personDissolved, transactionSaver, new PersonMergeBusinessObjectFactoryLoader()))
			{
				personMerger.Merge();
			}

			AssertEquals("Success status of Transaction Saver should be true", true, transactionSaver.IsSuccessful);
		}

		public void TestPersonMergeTransactionSaver_SuccessStatusIsFalse_WhenMergingUnsuccessful()
		{
			var transactionSaver = new PersonMergeTransactionSaverForTest(true);
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			using (var personMerger = new PersonMerger(personRetained, personDissolved, transactionSaver, new PersonMergeBusinessObjectFactoryLoader()))
			{
				try
				{
					personMerger.Merge();
				}
				catch (Exception)
				{
				}
			}

			AssertEquals("Success status of Transaction Saver should be false", false, transactionSaver.IsSuccessful);
		}

		public void TestPersonMergeTransactionSaver_ShouldUseDifferentFactoryForMerging()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			var personMergeTransactionSaver = new PersonMergeTransactionSaver();
			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson, personMergeTransactionSaver, new PersonMergeBusinessObjectFactoryLoader()))
			{
				merger.Merge();
			}

			var usedFactories = personMergeTransactionSaver.Factories;
			AssertEquals("PersonMergeTransactionSaver should not use the main Factory", false, usedFactories.Contains(Factory));
		}

		public void TestPersonMergeTransactionSaver_ShouldUpdateAllExistingFactories()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var retainedPersonPK = retainedPerson.PK;
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPersonPK = dissolvedPerson.PK;
			Factory.Save();
			var anotherFactory1 = new BusinessObjectFactory();
			var anotherFactory2 = new BusinessObjectFactory();
			var allFactories = new[] { Factory, anotherFactory1, anotherFactory2 };

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			CombineAssertions(() =>
			{
				foreach (var factory in allFactories)
				{
					AssertEquals($"Dissolved person should be deleted in {factory.NameForDebugging} after merge", false, IsPersonExistInFactory(factory, dissolvedPersonPK));
				}
			});
		}

		public void TestPersonMergeTransactionSaver_ShouldRollbackAllExistingFactories_WhenExceptionIsThrownInKernel()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var retainedPersonPK = retainedPerson.PK;
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPersonPK = dissolvedPerson.PK;
			Factory.Save();
			var anotherFactory1 = new BusinessObjectFactory();
			var anotherFactory2 = new BusinessObjectFactory();
			var allFactories = new[] { Factory, anotherFactory1, anotherFactory2 };

			var hasException = false;
			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson, new PersonMergeTransactionSaverForTest(true), new PersonMergeBusinessObjectFactoryLoader()))
			{
				try
				{
					merger.Merge();
				}
				catch (Exception)
				{
					hasException = true;
				}
			}

			CombineAssertions(() =>
			{
				AssertEquals("Exception was thrown in the process", true, hasException);
				foreach (var factory in allFactories)
				{
					AssertEquals($"Dissolved person should not be deleted in {factory.NameForDebugging} after merge", true, IsPersonExistInFactory(factory, dissolvedPersonPK));
				}
			});
		}

		#region UpdatesPersonFKReferences

		public void TestPersonMergeTransactionSaver_ShouldReassignMultipleDifferentDissolvedChildren()
		{
			var personMergeTransactionSaver = new PersonMergeTransactionSaver();
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();

			var retainedHRJobApplicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var dissolvedHRJobApplicant = Factory.NewWithValidTestData<HRJobApplicant>();
			retainedHRJobApplicant.HA_PER = retainedPerson.PK;
			dissolvedHRJobApplicant.HA_PER = dissolvedPerson.PK;

			var retainedContact = Factory.NewWithValidTestData<OrgContact>();
			var dissolvedContact = Factory.NewWithValidTestData<OrgContact>();
			retainedContact.OC_PER = retainedPerson.PK;
			dissolvedContact.OC_PER = dissolvedPerson.PK;

			var retainedStaff = Factory.NewWithValidTestData<GlbStaff>();
			var dissolvedStaff = Factory.NewWithValidTestData<GlbStaff>();
			retainedStaff.GS_PER = retainedPerson.PK;
			dissolvedStaff.GS_PER = dissolvedPerson.PK;

			var allHRJobApplicants = new[] { retainedHRJobApplicant.PK, dissolvedHRJobApplicant.PK };
			var allContacts = new[] { retainedContact.PK, dissolvedContact.PK };
			var allStaffs = new[] { retainedStaff.PK, dissolvedStaff.PK };

			Factory.Save();

			var loadedHRJobApplicants_BeforeMerging = Factory.Load<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.PK, allHRJobApplicants));
			var loadedContacts_BeforeMerging = Factory.Load<OrgContact>(new ZQuery(OrgContactSchema.PK, allContacts));
			var loadedStaffs_BeforeMerging = Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.PK, allStaffs));
			CombineAssertions("Precondition", () =>
			{
				AssertEquals(false, loadedHRJobApplicants_BeforeMerging.All(applicant => applicant.HA_PER == retainedPerson.PK));
				AssertEquals(false, loadedContacts_BeforeMerging.All(contact => contact.OC_PER == retainedPerson.PK));
				AssertEquals(false, loadedStaffs_BeforeMerging.All(staff => staff.GS_PER == retainedPerson.PK));
			});

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var newFactory = new BusinessObjectFactory();
			var loadedHRJobApplicants_AfterMerging = newFactory.Load<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.PK, allHRJobApplicants));
			var loadedContacts_AfterMerging = newFactory.Load<OrgContact>(new ZQuery(OrgContactSchema.PK, allContacts));
			var loadedStaffs_AfterMerging = newFactory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.PK, allStaffs));
			CombineAssertions(() =>
			{
				AssertEquals("All related HR Job Applicants should have the same HA_PER after merge", true, loadedHRJobApplicants_AfterMerging.All(applicant => applicant.HA_PER == retainedPerson.PK));
				AssertEquals("All related Contacts should have the same OC_PER after merge", true, loadedContacts_AfterMerging.All(contact => contact.OC_PER == retainedPerson.PK));
				AssertEquals("All related Staffs should have the same HA_PER after merge", true, loadedStaffs_AfterMerging.All(staff => staff.GS_PER == retainedPerson.PK));
			});
		}

		public void TestPersonMergeTransactionSaver_ShouldNotReassignRetainedPerson()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var retainedHRJobApplicant = Factory.NewWithValidTestData<HRJobApplicant>();
			retainedHRJobApplicant.HA_PER = retainedPerson.PK;
			var retainedContact = Factory.NewWithValidTestData<OrgContact>();
			retainedContact.OC_PER = retainedPerson.PK;
			var retainedStaff = Factory.NewWithValidTestData<GlbStaff>();
			retainedStaff.GS_PER = retainedPerson.PK;
			Factory.Save();

			var loadedHRJobApplicant_BeforeMerging = Factory.LoadTop1<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.PK, retainedHRJobApplicant.PK));
			var loadedContact_BeforeMerging = Factory.LoadTop1<OrgContact>(new ZQuery(OrgContactSchema.PK, retainedContact.PK));
			var loadedStaff_BeforeMerging = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, retainedStaff.PK));
			CombineAssertions("Preconditions", () =>
			{
				AssertEquals(retainedPerson.PK, loadedHRJobApplicant_BeforeMerging.HA_PER);
				AssertEquals(retainedPerson.PK, loadedContact_BeforeMerging.OC_PER);
				AssertEquals(retainedPerson.PK, loadedStaff_BeforeMerging.GS_PER);
			});

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var newFactory = new BusinessObjectFactory();
			var loadedHRJobApplicant_AfterMerging = newFactory.LoadTop1<HRJobApplicant>(new ZQuery(HRJobApplicantSchema.PK, retainedHRJobApplicant.PK));
			var loadedContact_AfterMerging = newFactory.LoadTop1<OrgContact>(new ZQuery(OrgContactSchema.PK, retainedContact.PK));
			var loadedStaff_AfterMerging = newFactory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, retainedStaff.PK));
			CombineAssertions(() =>
			{
				AssertEquals("Existing related HR Job Applicants should not have changed", retainedPerson.PK, loadedHRJobApplicant_AfterMerging.HA_PER);
				AssertEquals("Existing related contacts should have the same OC_PER after merge", retainedPerson.PK, loadedContact_AfterMerging.OC_PER);
				AssertEquals("Existing related Staffs should have the same GS_PER after merge", retainedPerson.PK, loadedStaff_AfterMerging.GS_PER);
			});
		}

		public void TestPersonMergeTransactionSaver_ShouldReassignDissolvedPersonsPatternMatchingRegCode()
		{
			var personMergeTransactionSaver = new PersonMergeTransactionSaver();
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var patternMatchingRegCode = Factory.NewWithValidTestData<PatternMatchingRegCode>();
			patternMatchingRegCode.PMR_PER = dissolvedPerson.PK;
			Factory.Save();

			var loadedPatternMatchingRegCode_BeforeMerging = Factory.LoadTop1<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PK, patternMatchingRegCode.PK));
			AssertEquals("Precondition: ", dissolvedPerson.PK, loadedPatternMatchingRegCode_BeforeMerging.PMR_PER);

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var newFactory = new BusinessObjectFactory();
			var loadedPatternMatchingRegCod_AfterMerginge = newFactory.LoadTop1<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PK, patternMatchingRegCode.PK));
			AssertEquals("Related PatternMatchingRegCode of dissolvedPerson should have retainedPersonPK as PMR_PER after merge", retainedPerson.PK, loadedPatternMatchingRegCod_AfterMerginge.PMR_PER);
		}

		public void TestPersonMergeTransactionSaver_ShouldReassignDissolvedPersonsPatternMatchingPhone()
		{
			var personMergeTransactionSaver = new PersonMergeTransactionSaver();
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();

			var patternMatchingPhone = Factory.NewWithValidTestData<PatternMatchingPhone>();
			patternMatchingPhone.PMP_PER = dissolvedPerson.PK;

			Factory.Save();
			var loadedPatternMatchingPhone_BeforeMerging = Factory.LoadTop1<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PK, patternMatchingPhone.PK));
			AssertEquals("Precondition: ", dissolvedPerson.PK, loadedPatternMatchingPhone_BeforeMerging.PMP_PER);

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var newFactory = new BusinessObjectFactory();
			var loadedPatternMatchingPhone_AfterMerging = newFactory.LoadTop1<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PK, patternMatchingPhone.PK));
			AssertEquals("Related PatternMatchingPhone of dissolvedPerson should have retainedPersonPK as PMP_PER after merge", retainedPerson.PK, loadedPatternMatchingPhone_AfterMerging.PMP_PER);
		}

		public void TestPersonMergeTransactionSaver_ShouldReassignDissolvedPersonsPatternMatchingName()
		{
			var personMergeTransactionSaver = new PersonMergeTransactionSaver();
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();

			var patternMatchingName = Factory.NewWithValidTestData<PatternMatchingName>();
			patternMatchingName.PMN_PER = dissolvedPerson.PK;
			Factory.Save();

			var loadedPatternMatchingName_BeforeMerging = Factory.LoadTop1<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PK, patternMatchingName.PK));
			AssertEquals("Precondition: ", dissolvedPerson.PK, loadedPatternMatchingName_BeforeMerging.PMN_PER);

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var newFactory = new BusinessObjectFactory();
			var loadedPatternMatchingName_AfterMerging = newFactory.LoadTop1<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PK, patternMatchingName.PK));
			AssertEquals("Related PatternMatchingName of dissolvedPerson should have retainedPersonPK as PMN_PER after merge", retainedPerson.PK, loadedPatternMatchingName_AfterMerging.PMN_PER);
		}

		public void TestPersonMergeTransactionSaver_ShouldReassignDissolvedPersonsPatternMatchingEmail()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var patternMatchingEmail = Factory.NewWithValidTestData<PatternMatchingEmail>();
			patternMatchingEmail.PME_PER = dissolvedPerson.PK;
			Factory.Save();

			var loadedPatternMatchingEmail_BeforeMerging = Factory.LoadTop1<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PK, patternMatchingEmail.PK));
			AssertEquals("Precondition: ", dissolvedPerson.PK, loadedPatternMatchingEmail_BeforeMerging.PME_PER);

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var newFactory = new BusinessObjectFactory();
			var loadedPatternMatchingEmail_AfterMerging = newFactory.LoadTop1<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PK, patternMatchingEmail.PK));
			AssertEquals("Related PatternMatchingEmail of dissolvedPerson should have retainedPersonPK as PME_PER after merge", retainedPerson.PK, loadedPatternMatchingEmail_AfterMerging.PME_PER);
		}

		public void TestPersonMergeTransactionSaver_ShouldReassignDissolvedPersonsPatternMatchingDomain()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var patternMatchingDomain = Factory.NewWithValidTestData<PatternMatchingDomain>();
			patternMatchingDomain.PMD_PER = dissolvedPerson.PK;
			Factory.Save();

			var loadedPatternMatchingDomain_BeforeMerging = Factory.LoadTop1<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PK, patternMatchingDomain.PK));
			AssertEquals("Precondition:", dissolvedPerson.PK, loadedPatternMatchingDomain_BeforeMerging.PMD_PER);

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var newFactory = new BusinessObjectFactory();
			var loadedPatternMatchingDomain_AfterMerging = newFactory.LoadTop1<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PK, patternMatchingDomain.PK));
			AssertEquals("Related PatternMatchingDomain of dissolvedPerson should have retainedPersonPK as PMD_PER after merge", retainedPerson.PK, loadedPatternMatchingDomain_AfterMerging.PMD_PER);
		}

		public void TestPersonMergeTransactionSaver_ShouldReassignDissolvedPersonsPatternMatchingAddress()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var patternMatchingAddress = Factory.NewWithValidTestData<PatternMatchingAddress>();
			patternMatchingAddress.PMA_PER = dissolvedPerson.PK;
			Factory.Save();

			var dissolvedPatternMatchingAddress_BeforeMerging = Factory.LoadTop1<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_PER, dissolvedPerson.PK));
			var dissolvedPatternMatchingAddressPK = dissolvedPatternMatchingAddress_BeforeMerging.PK;
			AssertEquals("Precondition: ", dissolvedPerson.PK, dissolvedPatternMatchingAddress_BeforeMerging.PMA_PER);

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var newFactory = new BusinessObjectFactory();
			var dissolvedPatternMatchingAddress_AfterMerging = newFactory.LoadTop1<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PK, dissolvedPatternMatchingAddressPK));
			AssertEquals("Related PatternMatchingAddress of dissolvedPerson should have retainedPersonPK as PMA_PER after merge", retainedPerson.PK, dissolvedPatternMatchingAddress_AfterMerging.PMA_PER);
		}

		public void TestPersonMergeTransactionSaver_ShouldReassignDissolvedGlbVisitor()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var glbVisitorPK = Guid.NewGuid();
			Factory.Save();

			var insertNewGlbVisitor_SqlCommand = $@"INSERT INTO dbo.GlbVisitor (VTR_PK, VTR_PER) VALUES ('{glbVisitorPK}', '{dissolvedPerson.PK}')";
			Db.Connection.ExecuteNonQuery(insertNewGlbVisitor_SqlCommand);

			var getGlbVsitor_PER_SqlCommand = $@"SELECT VTR_PER from dbo.GlbVisitor WHERE VTR_PK = '{glbVisitorPK}'";
			var glbVisitor_PER_BeforeMerging = (Guid)Db.Connection.ExecuteScalar(getGlbVsitor_PER_SqlCommand);
			AssertEquals("Precondition: ", dissolvedPerson.PK, glbVisitor_PER_BeforeMerging);

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var glbVisitor_PER_AfterMerging = (Guid)Db.Connection.ExecuteScalar(getGlbVsitor_PER_SqlCommand);
			AssertEquals("All related glbVisitors should have retainedPersonPK as the VTR_PK after merge", retainedPerson.PK, glbVisitor_PER_AfterMerging);
		}

		public void TestPersonMergeTransactionSaver_ShouldDeleteDissolvedContactGlbPersonPrimaryRelationship()
		{
			var retainedContact = PersonAssociations.CreateNewContact(Factory);
			var dissolvedContact = PersonAssociations.CreateNewContact(Factory);
			Factory.Save();

			var dissolvedContactPrimaryRelationshipPK = dissolvedContact.Person.PrimaryRelationship.PK;
			AssertEquals(true, Factory.ExistsInDatabase(GlbPersonPrimaryRelationshipSchema.Constants.TableName, new ZQuery(GlbPersonPrimaryRelationshipSchema.PK, dissolvedContactPrimaryRelationshipPK)));

			using (var merger = new PersonMerger(retainedContact.Person, dissolvedContact.Person))
			{
				merger.Merge();
			}

			AssertEquals(false, Factory.ExistsInDatabase(GlbPersonPrimaryRelationshipSchema.Constants.TableName, new ZQuery(GlbPersonPrimaryRelationshipSchema.PK, dissolvedContactPrimaryRelationshipPK)));
		}

		public void TestPersonMergeTransactionSaver_ShouldDeleteDissolvedStaffGlbPersonPrimaryRelationship()
		{
			var retainedStaff = PersonAssociations.CreateNewStaff(Factory);
			var dissolvedStaff = PersonAssociations.CreateNewStaff(Factory);
			Factory.Save();

			var dissolvedStaffPrimaryRelationshipPK = dissolvedStaff.Person.PrimaryRelationship.PK;
			AssertEquals(true, Factory.ExistsInDatabase(GlbPersonPrimaryRelationshipSchema.Constants.TableName, new ZQuery(GlbPersonPrimaryRelationshipSchema.PK, dissolvedStaffPrimaryRelationshipPK)));

			using (var merger = new PersonMerger(retainedStaff.Person, dissolvedStaff.Person))
			{
				merger.Merge();
			}

			AssertEquals(false, Factory.ExistsInDatabase(GlbPersonPrimaryRelationshipSchema.Constants.TableName, new ZQuery(GlbPersonPrimaryRelationshipSchema.PK, dissolvedStaffPrimaryRelationshipPK)));
		}

		public void TestPersonMergeTransactionSaver_ShouldReassignDissolveCusInBondPerson()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var glbBranch = Factory.NewWithValidTestData<GlbCompany>().Branches.AddNew();
			var cusInBondHeaderPK = Guid.NewGuid();
			var cusInBondPersonPK = Guid.NewGuid();
			Factory.Save();

			var insertNewBH_SqlCommand = $@"INSERT INTO dbo.CusInBondHeader (BH_PK, BH_GB, BH_SystemCreateTimeUtc, BH_SystemCreateUser, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser) VALUES ('{cusInBondHeaderPK}', '{glbBranch.PK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(insertNewBH_SqlCommand);
			var insertNewCP_SqlCommand = $@"INSERT INTO dbo.CusInBondPerson (CP_PK, CP_PER, CP_BH_Header, CP_SystemCreateTimeUtc, CP_SystemCreateUser, CP_SystemLastEditTimeUtc, CP_SystemLastEditUser) VALUES ('{cusInBondPersonPK}', '{dissolvedPerson.PK}', '{cusInBondHeaderPK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(insertNewCP_SqlCommand);

			var getCusInBondPerson_PER_SqlCommand = $@"SELECT CP_PER from dbo.CusInBondPerson WHERE CP_PK = '{cusInBondPersonPK}'";
			var cusPersonPER_BeforeMerging = (Guid)Db.Connection.ExecuteScalar(getCusInBondPerson_PER_SqlCommand);
			AssertEquals("Precondition: ", dissolvedPerson.PK, cusPersonPER_BeforeMerging);

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var cusPersonPER_AfterMerging = (Guid)Db.Connection.ExecuteScalar(getCusInBondPerson_PER_SqlCommand);
			AssertEquals("All related cusInBondPersons should have retainedPersonPK as the CP_PER after merge", retainedPerson.PK, cusPersonPER_AfterMerging);
		}

		public void TestPersonMergeTransactionSaver_ShouldReassignDissolvedCusPerson()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var cusPersonPK = Guid.NewGuid();
			Factory.Save();

			var declaration = Factory.New<IBaseJobDeclaration>() as BusinessObject;
			var insertNewCPN_SqlCommand = $@"INSERT INTO dbo.CusPerson (CPN_PK, CPN_PER_Person, CPN_ParentTableCode, CPN_ParentID) VALUES ('{cusPersonPK}', '{dissolvedPerson.PK}', '{declaration.TablePrefix}', '{declaration.PK}')";
			Db.Connection.ExecuteNonQuery(insertNewCPN_SqlCommand);

			var getCusPersonPER_SqlCommand = $@"SELECT CPN_PER_Person from dbo.CusPerson WHERE CPN_PK = '{cusPersonPK}'";
			var cusPerson_PER_BeforeMerging = (Guid)Db.Connection.ExecuteScalar(getCusPersonPER_SqlCommand);
			AssertEquals("Precondition: ", dissolvedPerson.PK, cusPerson_PER_BeforeMerging);

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var cusPerson_PER_AfterMerging = (Guid)Db.Connection.ExecuteScalar(getCusPersonPER_SqlCommand);
			AssertEquals("All related cusPersons should have retainedPersonPK as the CPN_PER_Person after merge", retainedPerson.PK, cusPerson_PER_AfterMerging);
		}

		public void TestPersonMergeTransactionSaver_ShouldNullifyDissolvedPersonFK_WhenCannotReassign_UniqueNullable()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var toBeDissolvedRecordPK = Guid.NewGuid();
			Factory.Save();

			var setup_SqlCommand = $@"
CREATE TABLE [dbo].[TestTable](
	[TT_PK] [uniqueidentifier] NOT NULL,
	[TT_PER] [uniqueidentifier] NULL)

CREATE UNIQUE INDEX [FK_TT_PER] ON [dbo].[TestTable]
(
	[TT_PER] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

INSERT INTO [TestTable] (TT_PK, TT_PER) VALUES ('{toBeDissolvedRecordPK}', '{dissolvedPerson.PK}')
INSERT INTO [TestTable] (TT_PK, TT_PER) VALUES (NEWID(), '{retainedPerson.PK}')
";
			Db.Connection.ExecuteNonQuery(setup_SqlCommand);

			var getToBeDissolvedRecordPER_BeforeMerging = $"SELECT [TT_PER], 0 FROM [dbo].[TestTable] WHERE [TT_PK] = '{toBeDissolvedRecordPK}'";
			var toBeDissolvedPER_BeforeMerging = Db.Connection.ExecuteScalar(getToBeDissolvedRecordPER_BeforeMerging);
			AssertNotNull(toBeDissolvedPER_BeforeMerging);
			//AssertEquals("Precondition: ", dissolvedPerson.PK, toBeDissolvedPER_BeforeMerging);

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var toBeDissolvedPER_AfterMerging = Db.Connection.ExecuteScalar(getToBeDissolvedRecordPER_BeforeMerging);
			AssertEquals(typeof(DBNull), toBeDissolvedPER_AfterMerging.GetType());
		}

		public void TestPersonMergeTransactionSaver_ShouldDeleteRecordWithDissolvedPersonFK_WhenCannotReassign_UniqueNonNullable()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var toBeDissolvedRecordPK = Guid.NewGuid();
			Factory.Save();

			var setup_SqlCommand = $@"
CREATE TABLE [dbo].[TestTable](
	[TT_PK] [uniqueidentifier] NOT NULL,
	[TT_PER] [uniqueidentifier] NOT NULL)

CREATE UNIQUE INDEX [FK_TT_PER] ON [dbo].[TestTable]
(
	[TT_PER] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

INSERT INTO [TestTable] (TT_PK, TT_PER) VALUES ('{toBeDissolvedRecordPK}', '{dissolvedPerson.PK}')
INSERT INTO [TestTable] (TT_PK, TT_PER) VALUES (NEWID(), '{retainedPerson.PK}')
";
			Db.Connection.ExecuteNonQuery(setup_SqlCommand);

			var getToBeDissolvedRecordsCount_SqlCommand = $"SELECT COUNT(*) FROM [dbo].[TestTable] WHERE [TT_PK] = '{toBeDissolvedRecordPK}'";
			var countToBeDissolvedRecords_BeforeMerging = (int)Db.Connection.ExecuteScalar(getToBeDissolvedRecordsCount_SqlCommand);
			AssertEquals("Precondition: ", 1, countToBeDissolvedRecords_BeforeMerging);

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var countToBeDissolvedRecords_AfterMerging = (int)Db.Connection.ExecuteScalar(getToBeDissolvedRecordsCount_SqlCommand);
			AssertEquals("Record with non-nullable FK with unique index constraint should be deleted after merge", 0, countToBeDissolvedRecords_AfterMerging);
		}

		public void TestPersonMergeTransactionSaver_ShouldThrowDeveloperException_WhenCannotReassignAndCannotDeleteDissolvedPersonRecord()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var toBeDissolvedRecordPK = Guid.NewGuid();
			Factory.Save();

			var setup1_SqlCommand = $@"
CREATE TABLE [dbo].[TestTable](
	[TT_PK] [uniqueidentifier] NOT NULL,
	[TT_PER] [uniqueidentifier] NOT NULL,
CONSTRAINT [PK_UX_TT_PK] PRIMARY KEY NONCLUSTERED 
	(
		[TT_PK] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE UNIQUE INDEX [FK_TT_PER] ON [dbo].[TestTable]
(
	[TT_PER] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

INSERT INTO [TestTable] (TT_PK, TT_PER) VALUES ('{toBeDissolvedRecordPK}', '{dissolvedPerson.PK}')
INSERT INTO [TestTable] (TT_PK, TT_PER) VALUES (NEWID(), '{retainedPerson.PK}')";

			var setup2_SqlCommand = $@"
CREATE TABLE [dbo].[TestReferencingTable](
	[TR_PK] [uniqueidentifier] NOT NULL,
	[TR_TT] [uniqueidentifier] NOT NULL)

ALTER TABLE [dbo].[TestReferencingTable]  WITH CHECK ADD  CONSTRAINT [TestReferencingTable_TR_TT_FK2_TestTable_RRR_120N] FOREIGN KEY([TR_TT])
REFERENCES [dbo].[TestTable] ([TT_PK])

INSERT INTO [TestReferencingTable] (TR_PK, TR_TT) VALUES (NEWID(), '{toBeDissolvedRecordPK}')
";
			Db.Connection.ExecuteNonQuery(setup1_SqlCommand);
			Db.Connection.ExecuteNonQuery(setup2_SqlCommand);

			var hasException = false;

			ExceptionReporterTestListener.Instance.Clear();

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				try
				{
					merger.Merge();
				}
				catch (Exception)
				{
					hasException = true;

					CombineAssertions(() =>
					{
						AssertEquals("Exception was thrown in the process", true, hasException);
						AssertEquals("Developer Exception was reported", 1, ExceptionReporterTestListener.Instance.Count);
					});
				}
				finally
				{
					ExceptionReporterTestListener.Instance.Clear();
				}
			}
		}

		#endregion

		#endregion

		#region SyncRelatedChildRecords

		public void TestSyncRelatedChildRecords_SyncsRelatedChildContacts()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "John Smith";
			retainedPerson.PER_HomePhone = "+61222333444";
			retainedPerson.PER_BirthDate = new ZDate(1991, 01, 01);

			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson.PER_FullName = "Johnny Smith";
			dissolvedPerson.PER_HomePhone = "+61999888777";
			dissolvedPerson.PER_BirthDate = new ZDate(1991, 01, 01);

			var attachedContact1 = Factory.NewWithValidTestData<OrgContact>();
			attachedContact1.OC_PER = dissolvedPerson.PK;
			attachedContact1.UpdateFromPerson(dissolvedPerson);

			Factory.Save();

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals(attachedContact1.OC_ContactName, dissolvedPerson.PER_FullName);
				AssertEquals(attachedContact1.OC_HomePhone, dissolvedPerson.PER_HomePhone);
				AssertEquals(attachedContact1.OC_Birthday, dissolvedPerson.PER_BirthDate);
			});

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			CombineAssertions(() =>
			{
				AssertEquals(attachedContact1.OC_ContactName, retainedPerson.PER_FullName);
				AssertEquals(attachedContact1.OC_HomePhone, retainedPerson.PER_HomePhone);
				AssertEquals(attachedContact1.OC_Birthday, retainedPerson.PER_BirthDate);
			});
		}

		public void TestSyncRelatedChildRecords_SyncsRelatedChildStaff()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "John Smith";
			retainedPerson.PER_HomePhone = "+61222333444";
			retainedPerson.PER_BirthDate = new ZDate(1991, 01, 01);

			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson.PER_FullName = "Johnny Smith";
			dissolvedPerson.PER_HomePhone = "+61999888777";
			dissolvedPerson.PER_BirthDate = new ZDate(1991, 01, 01);

			var attachedStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			attachedStaff1.SetFromPerson(dissolvedPerson);

			Factory.Save();

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals(attachedStaff1.GS_FullName, dissolvedPerson.PER_FullName);
				AssertEquals(attachedStaff1.GS_HomePhone, dissolvedPerson.PER_HomePhone);
				AssertEquals(attachedStaff1.GS_Birthdate, dissolvedPerson.PER_BirthDate);
			});

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			CombineAssertions(() =>
			{
				AssertEquals(attachedStaff1.GS_FullName, retainedPerson.PER_FullName);
				AssertEquals(attachedStaff1.GS_HomePhone, retainedPerson.PER_HomePhone);
				AssertEquals(attachedStaff1.GS_Birthdate, retainedPerson.PER_BirthDate);
			});
		}

		public void TestSyncRelatedChildRecords_SyncsRelatedChildApplicants()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "John Smith";
			retainedPerson.PER_HomePhone = "+61222333444";
			retainedPerson.PER_BirthDate = new ZDate(1991, 01, 01);

			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson.PER_FullName = "Johnny Smith";
			dissolvedPerson.PER_HomePhone = "+61999888777";
			dissolvedPerson.PER_BirthDate = new ZDate(1991, 01, 01);

			var attachedApplicant1 = Factory.New<HRJobApplicant>();
			attachedApplicant1.SetFromPerson(dissolvedPerson);

			Factory.Save();

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("Applicant 1: FullName:", dissolvedPerson.PER_FullName, attachedApplicant1.HA_FullName);
				AssertEquals("Applicant 1: HomePhone:", dissolvedPerson.PER_HomePhone, attachedApplicant1.HA_HomePhone);
				AssertEquals("Applicant 1: BirthDate:", dissolvedPerson.PER_BirthDate, attachedApplicant1.HA_Birthdate);
			});

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			CombineAssertions(() =>
			{
				AssertEquals("Applicant 1: FullName:", retainedPerson.PER_FullName, attachedApplicant1.HA_FullName);
				AssertEquals("Applicant 1: HomePhone:", retainedPerson.PER_HomePhone, attachedApplicant1.HA_HomePhone);
				AssertEquals("Applicant 1: BirthDate:", retainedPerson.PER_BirthDate, attachedApplicant1.HA_Birthdate);
			});
		}

		#endregion

		#region Password History

		public void TestSyncPasswordHistory()
		{
			WebDataRegistry.Instance.WebPasswordHistoryCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 13);

			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "John Smith";
			retainedPerson.PER_HomePhone = "+61222333444";
			retainedPerson.PER_BirthDate = new ZDate(1991, 01, 01);
			retainedPerson.SetHashedPassword("rp001");
			var retainedContact1 = PersonAssociations.AddNewContactToPerson(Factory, retainedPerson);
			retainedContact1.OC_ContactName = "John Smith";

			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson.PER_FullName = "Johnny Smith";
			dissolvedPerson.PER_HomePhone = "+61999888777";
			dissolvedPerson.PER_BirthDate = new ZDate(1991, 01, 01);
			var dissolvedContact1 = PersonAssociations.AddNewContactToPerson(Factory, dissolvedPerson);
			dissolvedContact1.OC_ContactName = "John Smith";
			dissolvedContact1.SetHashedPassword("dc001");
			Factory.Save();

			retainedPerson.SetHashedPassword("rp001+");
			dissolvedContact1.SetHashedPassword("dc001+");
			Factory.Save();

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var newFactory = new BusinessObjectFactory();
			var newRetainedPerson = newFactory.Load<GlbPerson>(retainedPerson.PK);
			var newDissolvedPerson = newFactory.Load<GlbPerson>(dissolvedPerson.PK);
			var newRetainedContact1 = newFactory.Load<OrgContact>(retainedContact1.PK);
			var newDissolvedContact1 = newFactory.Load<OrgContact>(dissolvedContact1.PK);

			CombineAssertions("Assertions", () =>
			{
				Assert("#1", newRetainedPerson.HasPasswordBeenUsed("rp001"));
				Assert("#2", newRetainedPerson.HasPasswordBeenUsed("rp001+"));
				Assert("#3", !newRetainedPerson.HasPasswordBeenUsed("rp001+++"));

				Assert("#4", newRetainedPerson.HasPasswordBeenUsed("dc001"));
				Assert("#5", newRetainedPerson.HasPasswordBeenUsed("dc001+"));

				AssertEquals("#6", null, newDissolvedPerson);
				Assert("#7", !newDissolvedContact1.HasPasswordBeenUsed("dc001"));
				Assert("#8", !newDissolvedContact1.HasPasswordBeenUsed("dc001+"));
				Assert("#9", !newDissolvedContact1.HasPasswordBeenUsed("dc001+++"));
			});
		}

		#endregion

		#region Integration Tests

		public void TestPersonMerge_Integration_1RetainedApplicant_1DissolvedApplicant()
		{
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedApplicant = PersonAssociations.AddNewHRJobApplicantToPerson(Factory, dissolvedPerson);

			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var retainedApplicant = PersonAssociations.AddNewHRJobApplicantToPerson(Factory, retainedPerson);

			Factory.Save();

			var dissolvedPersonPK = dissolvedPerson.PK;
			var retainedPersonPK = retainedPerson.PK;
			var dissolvedApplicantPK = dissolvedApplicant.PK;
			var retainedApplicantPK = retainedApplicant.PK;

			var dissolvedPerson_BeforeMerging = GetPersonByPKfromDB(Factory, dissolvedPersonPK);
			var retainedPerson_BeforeMerging = GetPersonByPKfromDB(Factory, retainedPersonPK);
			var dissolvedApplicant_BeforeMerging = GetApplicantByPKfromDB(Factory, dissolvedApplicantPK);
			var retainedApplicant_BeforeMerging = GetApplicantByPKfromDB(Factory, retainedApplicantPK);

			var applicants_DissolvedPerson_BeforeMerging = GetPersonsApplicantsFromDB(Factory, dissolvedPersonPK);
			var applicants_RetainedPerson_BeforeMerging = GetPersonsApplicantsFromDB(Factory, retainedPersonPK);

			var contacts_DissolvedPerson_BeforeMerging = GetPersonsContactsFromDB(Factory, dissolvedPersonPK);
			var contacts_RetainedPerson_BeforeMerging = GetPersonsContactsFromDB(Factory, retainedPersonPK);

			var staffs_DissolvedPerson_BeforeMerging = GetPersonsStaffsFromDB(Factory, dissolvedPersonPK);
			var staffs_RetainedPerson_BeforeMerging = GetPersonsStaffsFromDB(Factory, retainedPersonPK);

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals(dissolvedPerson, dissolvedPerson_BeforeMerging);
				AssertEquals(retainedPerson, retainedPerson_BeforeMerging);
				AssertEquals(dissolvedApplicant, dissolvedApplicant_BeforeMerging);
				AssertEquals(retainedApplicant, retainedApplicant_BeforeMerging);

				AssertEquals(1, dissolvedPerson_BeforeMerging.ApplicantCollection.Count);
				AssertEquals(0, dissolvedPerson_BeforeMerging.ContactCollection.Count);
				AssertEquals(0, dissolvedPerson_BeforeMerging.StaffCollection.Count);

				AssertEquals(1, retainedPerson_BeforeMerging.ApplicantCollection.Count);
				AssertEquals(0, retainedPerson_BeforeMerging.ContactCollection.Count);
				AssertEquals(0, retainedPerson_BeforeMerging.StaffCollection.Count);

				AssertEquals(true, IsPersonExistInDB(Factory, dissolvedPersonPK));
				AssertEquals(true, IsPersonExistInDB(Factory, retainedPersonPK));
				AssertEquals(true, IsApplicantExistInDB(Factory, dissolvedApplicantPK));
				AssertEquals(true, IsApplicantExistInDB(Factory, retainedApplicantPK));

				AssertEquals(1, applicants_DissolvedPerson_BeforeMerging.Length);
				AssertEquals(1, applicants_RetainedPerson_BeforeMerging.Length);

				AssertEquals(0, contacts_DissolvedPerson_BeforeMerging.Length);
				AssertEquals(0, contacts_RetainedPerson_BeforeMerging.Length);

				AssertEquals(0, staffs_DissolvedPerson_BeforeMerging.Length);
				AssertEquals(0, staffs_RetainedPerson_BeforeMerging.Length);

				AssertEquals(dissolvedPersonPK, dissolvedApplicant_BeforeMerging.HA_PER);
				AssertEquals(retainedPersonPK, retainedApplicant_BeforeMerging.HA_PER);
			});

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var dissolvedPerson_AfterMerging = GetPersonByPKfromDB(Factory, dissolvedPersonPK);
			var retainedPerson_AfterMerging = GetPersonByPKfromDB(Factory, retainedPersonPK);
			var dissolvedApplicant_AfterMerging = GetApplicantByPKfromDB(Factory, dissolvedApplicantPK);
			var retainedApplicant_AfterMerging = GetApplicantByPKfromDB(Factory, retainedApplicantPK);

			var applicants_DissolvedPerson_AfterMerging = GetPersonsApplicantsFromDB(Factory, dissolvedPersonPK);
			var applicants_RetainedPerson_AfterMerging = GetPersonsApplicantsFromDB(Factory, retainedPersonPK);

			var contacts_DissolvedPerson_AfterMerging = GetPersonsContactsFromDB(Factory, dissolvedPersonPK);
			var contacts_RetainedPerson_AfterMerging = GetPersonsContactsFromDB(Factory, retainedPersonPK);

			var staffs_DissolvedPerson_AfterMergingMerging = GetPersonsStaffsFromDB(Factory, dissolvedPersonPK);
			var staffs_RetainedPerson_AfterMerging = GetPersonsStaffsFromDB(Factory, retainedPersonPK);

			CombineAssertions(() =>
			{
				AssertEquals("Dissolved person should be deleted in factory after merge", false, IsPersonExistInFactory(Factory, dissolvedPersonPK));
				AssertEquals("Retained person should not be deleted in factory after merge", true, IsPersonExistInFactory(Factory, retainedPersonPK));
				AssertEquals("Dissovled applicant should not be deleted in factory after merge", true, IsApplicantExistInFactory(Factory, dissolvedApplicantPK));
				AssertEquals("Retained applicant should not be deleted in factory after merge", true, IsApplicantExistInFactory(Factory, retainedApplicantPK));

				AssertEquals("Dissolved person should be deleted in database after merge", false, IsPersonExistInDB(Factory, dissolvedPersonPK));
				AssertEquals("Retained person should not be deleted in database after merge", true, IsPersonExistInDB(Factory, retainedPersonPK));
				AssertEquals("Dissovled applicant should not be deleted in database after merge", true, IsApplicantExistInDB(Factory, dissolvedApplicantPK));
				AssertEquals("Retained applicant should not be deleted in database after merge", true, IsApplicantExistInDB(Factory, retainedApplicantPK));

				AssertEquals("Retained person should have 2 applicants in ApplicantCollection after merge", 2, retainedPerson_AfterMerging.ApplicantCollection.Count);
				AssertEquals("Retained person should have 0 contacts in ContactCollection after merge", 0, retainedPerson_AfterMerging.ContactCollection.Count);
				AssertEquals("Retained person should have 0 staffs in StaffCollection after merge", 0, retainedPerson_AfterMerging.StaffCollection.Count);

				AssertEquals("Dissolved person should have 0 applicants after merge", 0, applicants_DissolvedPerson_AfterMerging.Length);
				AssertEquals("Retained person should have 2 applicants after merge", 2, applicants_RetainedPerson_AfterMerging.Length);

				AssertEquals("Dissolved contacts should remain 0 after merge", 0, contacts_DissolvedPerson_AfterMerging.Length);
				AssertEquals("Retained contacts should remain 0 after merge", 0, contacts_RetainedPerson_AfterMerging.Length);

				AssertEquals("Dissolved staffs should remain 0 after merge", 0, staffs_DissolvedPerson_AfterMergingMerging.Length);
				AssertEquals("Retained staffs should remain 0 after merge", 0, staffs_RetainedPerson_AfterMerging.Length);

				AssertEquals("Dissovled applicant's HA_PER should change to retained person's PK", retainedPersonPK, dissolvedApplicant_AfterMerging.HA_PER);
				AssertEquals("Retained applicant's HA_PER should remain a retained person's PK", retainedPersonPK, retainedApplicant_AfterMerging.HA_PER);
			});
		}

		public void TestPersonMerge_Integration_1RetainedContact_1DissolvedContact()
		{
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedContact = PersonAssociations.AddNewContactToPerson(Factory, dissolvedPerson);

			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var retainedContact = PersonAssociations.AddNewContactToPerson(Factory, retainedPerson);

			Factory.Save();

			var dissolvedPersonPK = dissolvedPerson.PK;
			var retainedPersonPK = retainedPerson.PK;
			var dissolvedContactPK = dissolvedContact.PK;
			var retainedContactPK = retainedContact.PK;

			var dissolvedPerson_BeforeMerging = GetPersonByPKfromDB(Factory, dissolvedPersonPK);
			var retainedPerson_BeforeMerging = GetPersonByPKfromDB(Factory, retainedPersonPK);
			var dissolvedContact_BeforeMerging = GetContactByPKfromDB(Factory, dissolvedContactPK);
			var retainedContact_BeforeMerging = GetContactByPKfromDB(Factory, retainedContactPK);

			var applicants_DissolvedPerson_BeforeMerging = GetPersonsApplicantsFromDB(Factory, dissolvedPersonPK);
			var applicants_RetainedPerson_BeforeMerging = GetPersonsApplicantsFromDB(Factory, retainedPersonPK);

			var contacts_DissolvedPerson_BeforeMerging = GetPersonsContactsFromDB(Factory, dissolvedPersonPK);
			var contacts_RetainedPerson_BeforeMerging = GetPersonsContactsFromDB(Factory, retainedPersonPK);

			var staffs_DissolvedPerson_BeforeMerging = GetPersonsStaffsFromDB(Factory, dissolvedPersonPK);
			var staffs_RetainedPerson_BeforeMerging = GetPersonsStaffsFromDB(Factory, retainedPersonPK);

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals(dissolvedPerson, dissolvedPerson_BeforeMerging);
				AssertEquals(retainedPerson, retainedPerson_BeforeMerging);
				AssertEquals(dissolvedContact, dissolvedContact_BeforeMerging);
				AssertEquals(retainedContact, retainedContact_BeforeMerging);

				AssertEquals(0, dissolvedPerson_BeforeMerging.ApplicantCollection.Count);
				AssertEquals(1, dissolvedPerson_BeforeMerging.ContactCollection.Count);
				AssertEquals(0, dissolvedPerson_BeforeMerging.StaffCollection.Count);

				AssertEquals(0, retainedPerson_BeforeMerging.ApplicantCollection.Count);
				AssertEquals(1, retainedPerson_BeforeMerging.ContactCollection.Count);
				AssertEquals(0, retainedPerson_BeforeMerging.StaffCollection.Count);

				AssertEquals(true, IsPersonExistInDB(Factory, dissolvedPersonPK));
				AssertEquals(true, IsPersonExistInDB(Factory, retainedPersonPK));
				AssertEquals(true, IsContactExistInDB(Factory, dissolvedContactPK));
				AssertEquals(true, IsContactExistInDB(Factory, retainedContactPK));

				AssertEquals(true, IsPersonExistInFactory(Factory, dissolvedPersonPK));
				AssertEquals(true, IsPersonExistInFactory(Factory, retainedPersonPK));
				AssertEquals(true, IsContactExistInFactory(Factory, dissolvedContactPK));
				AssertEquals(true, IsContactExistInFactory(Factory, retainedContactPK));

				AssertEquals(0, applicants_DissolvedPerson_BeforeMerging.Length);
				AssertEquals(0, applicants_RetainedPerson_BeforeMerging.Length);

				AssertEquals(1, contacts_DissolvedPerson_BeforeMerging.Length);
				AssertEquals(1, contacts_RetainedPerson_BeforeMerging.Length);

				AssertEquals(0, staffs_DissolvedPerson_BeforeMerging.Length);
				AssertEquals(0, staffs_RetainedPerson_BeforeMerging.Length);

				AssertEquals(dissolvedPersonPK, dissolvedContact_BeforeMerging.OC_PER);
				AssertEquals(retainedPersonPK, retainedContact_BeforeMerging.OC_PER);
			});

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var dissolvedPerson_AfterMerging = GetPersonByPKfromDB(Factory, dissolvedPersonPK);
			var retainedPerson_AfterMerging = GetPersonByPKfromDB(Factory, retainedPersonPK);
			var dissolvedContact_AfterMerging = GetContactByPKfromDB(Factory, dissolvedContactPK);
			var retainedContact_AfterMerging = GetContactByPKfromDB(Factory, retainedContactPK);

			var applicants_DissolvedPerson_AfterMerging = GetPersonsApplicantsFromDB(Factory, dissolvedPersonPK);
			var applicants_RetainedPerson_AfterMerging = GetPersonsApplicantsFromDB(Factory, retainedPersonPK);

			var contacts_DissolvedPerson_AfterMerging = GetPersonsContactsFromDB(Factory, dissolvedPersonPK);
			var contacts_RetainedPerson_AfterMerging = GetPersonsContactsFromDB(Factory, retainedPersonPK);

			var staffs_DissolvedPerson_AfterMergingMerging = GetPersonsStaffsFromDB(Factory, dissolvedPersonPK);
			var staffs_RetainedPerson_AfterMerging = GetPersonsStaffsFromDB(Factory, retainedPersonPK);

			CombineAssertions(() =>
			{
				AssertEquals("Dissolved person should be deleted in database after merge", false, IsPersonExistInDB(Factory, dissolvedPersonPK));
				AssertEquals("Retained person should not be deleted in database after merge", true, IsPersonExistInDB(Factory, retainedPersonPK));
				AssertEquals("Dissovled contact should not be deleted in database after merge", true, IsContactExistInDB(Factory, dissolvedContactPK));
				AssertEquals("Retained contact should not be deleted in database after merge", true, IsContactExistInDB(Factory, retainedContactPK));

				AssertEquals("Dissolved person should be deleted in database after merge", false, IsPersonExistInFactory(Factory, dissolvedPersonPK));
				AssertEquals("Retained person should not be deleted in database after merge", true, IsPersonExistInFactory(Factory, retainedPersonPK));
				AssertEquals("Dissovled contact should not be deleted in database after merge", true, IsContactExistInFactory(Factory, dissolvedContactPK));
				AssertEquals("Retained contact should not be deleted in database after merge", true, IsContactExistInFactory(Factory, retainedContactPK));

				AssertEquals("Retained person should have 0 applicants in ApplicantCollection after merge", 0, retainedPerson_AfterMerging.ApplicantCollection.Count);
				AssertEquals("Retained person should have 2 contacts in ContactCollection after merge", 2, retainedPerson_AfterMerging.ContactCollection.Count);
				AssertEquals("Retained person should have 0 staffs in StaffCollection after merge", 0, retainedPerson_AfterMerging.StaffCollection.Count);

				AssertEquals("Dissolved applicants should remain 0 after merge", 0, applicants_DissolvedPerson_AfterMerging.Length);
				AssertEquals("Retained applicants should remain 0 after merge", 0, applicants_RetainedPerson_AfterMerging.Length);

				AssertEquals("Dissolved contacts should have 0 after merge", 0, contacts_DissolvedPerson_AfterMerging.Length);
				AssertEquals("Retained contacts should have 2 after merge", 2, contacts_RetainedPerson_AfterMerging.Length);

				AssertEquals("Dissolved staffs should remain 0 after merge", 0, staffs_DissolvedPerson_AfterMergingMerging.Length);
				AssertEquals("Retained staffs should remain 0 after merge", 0, staffs_RetainedPerson_AfterMerging.Length);

				AssertEquals("Dissovled contact's OC_PER should change to retained person's PK", retainedPersonPK, dissolvedContact_AfterMerging.OC_PER);
				AssertEquals("Retained contact's OC_PER should remain a retained person's PK", retainedPersonPK, retainedContact_AfterMerging.OC_PER);
			});
		}

		public void TestPersonMerge_Integration_1RetainedStaff_1DissolvedStaff()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var retainedStaff = PersonAssociations.AddNewStaffToPerson(Factory, retainedPerson);

			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedStaff = PersonAssociations.AddNewStaffToPerson(Factory, dissolvedPerson);

			Factory.Save();

			var dissolvedPersonPK = dissolvedPerson.PK;
			var retainedPersonPK = retainedPerson.PK;
			var dissolvedStaffPK = dissolvedStaff.PK;
			var retainedStaffPK = retainedStaff.PK;

			var dissolvedPerson_BeforeMerging = GetPersonByPKfromDB(Factory, dissolvedPersonPK);
			var retainedPerson_BeforeMerging = GetPersonByPKfromDB(Factory, retainedPersonPK);
			var dissolvedStaff_BeforeMerging = GetStaffByPKfromDB(Factory, dissolvedStaffPK);
			var retainedStaff_BeforeMerging = GetStaffByPKfromDB(Factory, retainedStaffPK);

			var applicants_DissolvedPerson_BeforeMerging = GetPersonsApplicantsFromDB(Factory, dissolvedPersonPK);
			var applicants_RetainedPerson_BeforeMerging = GetPersonsApplicantsFromDB(Factory, retainedPersonPK);

			var contacts_DissolvedPerson_BeforeMerging = GetPersonsContactsFromDB(Factory, dissolvedPersonPK);
			var contacts_RetainedPerson_BeforeMerging = GetPersonsContactsFromDB(Factory, retainedPersonPK);

			var staffs_DissolvedPerson_BeforeMerging = GetPersonsStaffsFromDB(Factory, dissolvedPersonPK);
			var staffs_RetainedPerson_BeforeMerging = GetPersonsStaffsFromDB(Factory, retainedPersonPK);

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals(dissolvedPerson, dissolvedPerson_BeforeMerging);
				AssertEquals(retainedPerson, retainedPerson_BeforeMerging);
				AssertEquals(dissolvedStaff, dissolvedStaff_BeforeMerging);
				AssertEquals(retainedStaff, retainedStaff_BeforeMerging);

				AssertEquals(true, IsPersonExistInDB(Factory, dissolvedPersonPK));
				AssertEquals(true, IsPersonExistInDB(Factory, retainedPersonPK));
				AssertEquals(true, IsStaffExistInDB(Factory, dissolvedStaffPK));
				AssertEquals(true, IsStaffExistInDB(Factory, retainedStaffPK));

				AssertEquals(0, dissolvedPerson_BeforeMerging.ApplicantCollection.Count);
				AssertEquals(0, dissolvedPerson_BeforeMerging.ContactCollection.Count);
				AssertEquals(1, dissolvedPerson_BeforeMerging.StaffCollection.Count);

				AssertEquals(0, retainedPerson_BeforeMerging.ApplicantCollection.Count);
				AssertEquals(0, retainedPerson_BeforeMerging.ContactCollection.Count);
				AssertEquals(1, retainedPerson_BeforeMerging.StaffCollection.Count);

				AssertEquals(0, applicants_DissolvedPerson_BeforeMerging.Length);
				AssertEquals(0, applicants_RetainedPerson_BeforeMerging.Length);

				AssertEquals(0, contacts_DissolvedPerson_BeforeMerging.Length);
				AssertEquals(0, contacts_RetainedPerson_BeforeMerging.Length);

				AssertEquals(1, staffs_DissolvedPerson_BeforeMerging.Length);
				AssertEquals(1, staffs_RetainedPerson_BeforeMerging.Length);

				AssertEquals(dissolvedPersonPK, dissolvedStaff_BeforeMerging.GS_PER);
				AssertEquals(retainedPersonPK, retainedStaff_BeforeMerging.GS_PER);
			});

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var dissolvedPerson_AfterMerging = GetPersonByPKfromDB(Factory, dissolvedPersonPK);
			var retainedPerson_AfterMerging = GetPersonByPKfromDB(Factory, retainedPersonPK);
			var dissolvedStaff_AfterMerging = GetStaffByPKfromDB(Factory, dissolvedStaffPK);
			var retainedStaff_AfterMerging = GetStaffByPKfromDB(Factory, retainedStaffPK);

			var applicants_DissolvedPerson_AfterMerging = GetPersonsApplicantsFromDB(Factory, dissolvedPersonPK);
			var applicants_RetainedPerson_AfterMerging = GetPersonsApplicantsFromDB(Factory, retainedPersonPK);

			var contacts_DissolvedPerson_AfterMerging = GetPersonsContactsFromDB(Factory, dissolvedPersonPK);
			var contacts_RetainedPerson_AfterMerging = GetPersonsContactsFromDB(Factory, retainedPersonPK);

			var staffs_DissolvedPerson_AfterMergingMerging = GetPersonsStaffsFromDB(Factory, dissolvedPersonPK);
			var staffs_RetainedPerson_AfterMerging = GetPersonsStaffsFromDB(Factory, retainedPersonPK);

			CombineAssertions(() =>
			{
				AssertEquals("Dissolved person should be deleted in factory after merge", false, IsPersonExistInFactory(Factory, dissolvedPersonPK));
				AssertEquals("Retained person should not be deleted in factory after merge", true, IsPersonExistInFactory(Factory, retainedPersonPK));
				AssertEquals("Dissovled staff should not be deleted in factory after merge", true, IsStaffExistInFactory(Factory, dissolvedStaffPK));
				AssertEquals("Retained staff should not be deleted in factory after merge", true, IsStaffExistInFactory(Factory, retainedStaffPK));

				AssertEquals("Dissolved person should be deleted in database after merge", false, IsPersonExistInDB(Factory, dissolvedPersonPK));
				AssertEquals("Retained person should not be deleted in database after merge", true, IsPersonExistInDB(Factory, retainedPersonPK));
				AssertEquals("Dissovled staff should not be deleted in database after merge", true, IsStaffExistInDB(Factory, dissolvedStaffPK));
				AssertEquals("Retained staff should not be deleted in database after merge", true, IsStaffExistInDB(Factory, retainedStaffPK));

				AssertEquals("Retained person should have 0 applicants in ApplicantCollection after merge", 0, retainedPerson_AfterMerging.ApplicantCollection.Count);
				AssertEquals("Retained person should have 0 contacts in ContactCollection after merge", 0, retainedPerson_AfterMerging.ContactCollection.Count);
				AssertEquals("Retained person should have 2 staffs in StaffCollection after merge", 2, retainedPerson_AfterMerging.StaffCollection.Count);

				AssertEquals("Dissolved applicants should remain 0 after merge", 0, applicants_DissolvedPerson_AfterMerging.Length);
				AssertEquals("Retained applicants should remain 0 after merge", 0, applicants_RetainedPerson_AfterMerging.Length);

				AssertEquals("Dissolved contacts should remain 0 after merge", 0, contacts_DissolvedPerson_AfterMerging.Length);
				AssertEquals("Retained contacts should remain 0 after merge", 0, contacts_RetainedPerson_AfterMerging.Length);

				AssertEquals("Dissolved person should have 0 staffs after merge", 0, staffs_DissolvedPerson_AfterMergingMerging.Length);
				AssertEquals("Retained person should have 2 staffs after merge", 2, staffs_RetainedPerson_AfterMerging.Length);

				AssertEquals("Dissovled staff's GS_PER should change to retained person's PK", retainedPersonPK, dissolvedStaff_AfterMerging.GS_PER);
				AssertEquals("Retained staff's GS_PER should remain a retained person's PK", retainedPersonPK, retainedStaff_AfterMerging.GS_PER);
			});
		}

		public void TestPersonMerge_Integration_0RetainedChildren_3DissolvedChildren()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();

			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedApplicant = PersonAssociations.AddNewHRJobApplicantToPerson(Factory, dissolvedPerson);
			var dissolvedContact = PersonAssociations.AddNewContactToPerson(Factory, dissolvedPerson);
			var dissolvedStaff = PersonAssociations.AddNewStaffToPerson(Factory, dissolvedPerson);

			Factory.Save();

			var retainedPersonPK = retainedPerson.PK;

			var dissolvedPersonPK = dissolvedPerson.PK;
			var dissolvedApplicantPK = dissolvedApplicant.PK;
			var dissolvedContactPK = dissolvedContact.PK;
			var dissolvedStaffPK = dissolvedStaff.PK;

			var dissolvedPerson_BeforeMerging = GetPersonByPKfromDB(Factory, dissolvedPersonPK);
			var retainedPerson_BeforeMerging = GetPersonByPKfromDB(Factory, retainedPersonPK);

			var dissolvedApplicant_BeforeMerging = GetApplicantByPKfromDB(Factory, dissolvedApplicantPK);
			var dissolvedContact_BeforeMerging = GetContactByPKfromDB(Factory, dissolvedContactPK);
			var dissolvedStaff_BeforeMerging = GetStaffByPKfromDB(Factory, dissolvedStaffPK);

			var contacts_DissolvedPerson_BeforeMerging = GetPersonsContactsFromDB(Factory, dissolvedPersonPK);
			var contacts_RetainedPerson_BeforeMerging = GetPersonsContactsFromDB(Factory, retainedPersonPK);

			var applicants_DissolvedPerson_BeforeMerging = GetPersonsApplicantsFromDB(Factory, dissolvedPersonPK);
			var applicants_RetainedPerson_BeforeMerging = GetPersonsApplicantsFromDB(Factory, retainedPersonPK);

			var staffs_DissolvedPerson_BeforeMerging = GetPersonsStaffsFromDB(Factory, dissolvedPersonPK);
			var staffs_RetainedPerson_BeforeMerging = GetPersonsStaffsFromDB(Factory, retainedPersonPK);

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals(retainedPerson, retainedPerson_BeforeMerging);
				AssertEquals(dissolvedPerson, dissolvedPerson_BeforeMerging);

				AssertEquals(dissolvedApplicant, dissolvedApplicant_BeforeMerging);
				AssertEquals(dissolvedContact, dissolvedContact_BeforeMerging);
				AssertEquals(dissolvedStaff, dissolvedStaff_BeforeMerging);

				AssertEquals(true, IsPersonExistInFactory(Factory, retainedPersonPK));
				AssertEquals(true, IsPersonExistInFactory(Factory, dissolvedPersonPK));
				AssertEquals(true, IsApplicantExistInFactory(Factory, dissolvedApplicantPK));
				AssertEquals(true, IsContactExistInFactory(Factory, dissolvedContactPK));
				AssertEquals(true, IsStaffExistInFactory(Factory, dissolvedStaffPK));

				AssertEquals(true, IsPersonExistInDB(Factory, retainedPersonPK));
				AssertEquals(true, IsPersonExistInDB(Factory, dissolvedPersonPK));
				AssertEquals(true, IsApplicantExistInDB(Factory, dissolvedApplicantPK));
				AssertEquals(true, IsContactExistInDB(Factory, dissolvedContactPK));
				AssertEquals(true, IsStaffExistInDB(Factory, dissolvedStaffPK));

				AssertEquals(1, dissolvedPerson_BeforeMerging.ApplicantCollection.Count);
				AssertEquals(1, dissolvedPerson_BeforeMerging.ContactCollection.Count);
				AssertEquals(1, dissolvedPerson_BeforeMerging.StaffCollection.Count);

				AssertEquals(0, retainedPerson_BeforeMerging.ApplicantCollection.Count);
				AssertEquals(0, retainedPerson_BeforeMerging.ContactCollection.Count);
				AssertEquals(0, retainedPerson_BeforeMerging.StaffCollection.Count);

				AssertEquals(0, applicants_RetainedPerson_BeforeMerging.Length);
				AssertEquals(1, applicants_DissolvedPerson_BeforeMerging.Length);

				AssertEquals(0, contacts_RetainedPerson_BeforeMerging.Length);
				AssertEquals(1, contacts_DissolvedPerson_BeforeMerging.Length);

				AssertEquals(0, staffs_RetainedPerson_BeforeMerging.Length);
				AssertEquals(1, staffs_DissolvedPerson_BeforeMerging.Length);

				AssertEquals(dissolvedPersonPK, dissolvedApplicant_BeforeMerging.HA_PER);
				AssertEquals(dissolvedPersonPK, dissolvedContact_BeforeMerging.OC_PER);
				AssertEquals(dissolvedPersonPK, dissolvedStaff_BeforeMerging.GS_PER);
			});

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var dissolvedPerson_AfterMerging = GetPersonByPKfromDB(Factory, dissolvedPersonPK);
			var dissolvedApplicant_AfterMerging = GetApplicantByPKfromDB(Factory, dissolvedApplicantPK);
			var dissolvedContact_AfterMerging = GetContactByPKfromDB(Factory, dissolvedContactPK);
			var dissolvedStaff_AfterMerging = GetStaffByPKfromDB(Factory, dissolvedStaffPK);

			var retainedPerson_AfterMerging = GetPersonByPKfromDB(Factory, retainedPersonPK);

			var applicants_RetainedPerson_AfterMerging = GetPersonsApplicantsFromDB(Factory, retainedPersonPK);
			var applicants_DissolvedPerson_AfterMerging = GetPersonsApplicantsFromDB(Factory, dissolvedPersonPK);

			var contacts_DissolvedPerson_AfterMerging = GetPersonsContactsFromDB(Factory, dissolvedPersonPK);
			var contacts_RetainedPerson_AfterMerging = GetPersonsContactsFromDB(Factory, retainedPersonPK);

			var staffs_RetainedPerson_AfterMerging = GetPersonsStaffsFromDB(Factory, retainedPersonPK);
			var staffs_DissolvedPerson_AfterMerging = GetPersonsStaffsFromDB(Factory, dissolvedPersonPK);

			CombineAssertions(() =>
			{
				AssertEquals("Dissolved person should be deleted in factory after merge", false, IsPersonExistInFactory(Factory, dissolvedPersonPK));
				AssertEquals("Retained person should not be deleted in factory after merge", true, IsPersonExistInFactory(Factory, retainedPersonPK));

				AssertEquals("Dissovled applicant should not be deleted in factory after merge", true, IsApplicantExistInFactory(Factory, dissolvedApplicantPK));
				AssertEquals("Dissovled contact should not be deleted in factory after merge", true, IsContactExistInFactory(Factory, dissolvedContactPK));
				AssertEquals("Dissovled staff should not be deleted in factory after merge", true, IsStaffExistInFactory(Factory, dissolvedStaffPK));

				AssertEquals("Dissolved person should be deleted in database after merge", false, IsPersonExistInDB(Factory, dissolvedPersonPK));
				AssertEquals("Retained person should not be deleted in database after merge", true, IsPersonExistInDB(Factory, retainedPersonPK));

				AssertEquals("Dissovled applicant should not be deleted in database after merge", true, IsApplicantExistInDB(Factory, dissolvedApplicantPK));
				AssertEquals("Dissovled contact should not be deleted in database after merge", true, IsContactExistInDB(Factory, dissolvedContactPK));
				AssertEquals("Dissovled staff should not be deleted in database after merge", true, IsStaffExistInDB(Factory, dissolvedStaffPK));

				AssertEquals("Retained person should have 1 applicant in ApplicantCollection after merge", 1, retainedPerson_AfterMerging.ApplicantCollection.Count);
				AssertEquals("Retained person should have 1 contact in ContactCollection after merge", 1, retainedPerson_AfterMerging.ContactCollection.Count);
				AssertEquals("Retained person should have 1 staff in StaffCollection after merge", 1, retainedPerson_AfterMerging.StaffCollection.Count);

				AssertEquals("Dissolved person should have 0 applicants after merge", 0, applicants_DissolvedPerson_AfterMerging.Length);
				AssertEquals("Retained person should have 1 applicant after merge", 1, applicants_RetainedPerson_AfterMerging.Length);

				AssertEquals("Dissolved person should have 0 contacts after merge", 0, contacts_DissolvedPerson_AfterMerging.Length);
				AssertEquals("Retained person should have 1 contacts after merge", 1, contacts_RetainedPerson_AfterMerging.Length);

				AssertEquals("Dissolved person should have 0 staffs after merge", 0, staffs_DissolvedPerson_AfterMerging.Length);
				AssertEquals("Retained person should have 1 staff after merge", 1, staffs_RetainedPerson_AfterMerging.Length);

				AssertEquals("Dissovled applicants's HA_PER should change to retained person's PK", retainedPersonPK, dissolvedApplicant_AfterMerging.HA_PER);
				AssertEquals("Dissovled contact's OC_PER should change to retained person's PK", retainedPersonPK, dissolvedContact_AfterMerging.OC_PER);
				AssertEquals("Dissovled staff's GS_PER should change to retained person's PK", retainedPersonPK, dissolvedStaff_AfterMerging.GS_PER);
			});
		}

		public void TestPersonMerge_Integration_3RetainedChildren_0DissolvedChildren()
		{
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();

			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var retainedApplicant = PersonAssociations.AddNewHRJobApplicantToPerson(Factory, retainedPerson);
			var retainedContact = PersonAssociations.AddNewContactToPerson(Factory, retainedPerson);
			var retainedStaff = PersonAssociations.AddNewStaffToPerson(Factory, retainedPerson);

			Factory.Save();

			var dissolvedPersonPK = dissolvedPerson.PK;
			var retainedPersonPK = retainedPerson.PK;
			var retainedApplicantPK = retainedApplicant.PK;
			var retainedContactPK = retainedContact.PK;
			var retainedStaffPK = retainedStaff.PK;

			var dissolvedPerson_BeforeMerging = GetPersonByPKfromDB(Factory, dissolvedPersonPK);
			var retainedPerson_BeforeMerging = GetPersonByPKfromDB(Factory, retainedPersonPK);

			var retainedApplicant_BeforeMerging = GetApplicantByPKfromDB(Factory, retainedApplicantPK);
			var retainedContact_BeforeMerging = GetContactByPKfromDB(Factory, retainedContactPK);
			var retainedStaff_BeforeMerging = GetStaffByPKfromDB(Factory, retainedStaffPK);

			var applicants_DissolvedPerson_BeforeMerging = GetPersonsApplicantsFromDB(Factory, dissolvedPersonPK);
			var applicants_RetainedPerson_BeforeMerging = GetPersonsApplicantsFromDB(Factory, retainedPersonPK);

			var contacts_DissolvedPerson_BeforeMerging = GetPersonsContactsFromDB(Factory, dissolvedPersonPK);
			var contacts_RetainedPerson_BeforeMerging = GetPersonsContactsFromDB(Factory, retainedPersonPK);

			var staffs_DissolvedPerson_BeforeMerging = GetPersonsStaffsFromDB(Factory, dissolvedPersonPK);
			var staffs_RetainedPerson_BeforeMerging = GetPersonsStaffsFromDB(Factory, retainedPersonPK);

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals(dissolvedPerson, dissolvedPerson_BeforeMerging);
				AssertEquals(retainedPerson, retainedPerson_BeforeMerging);
				AssertEquals(retainedApplicant, retainedApplicant_BeforeMerging);
				AssertEquals(retainedContact, retainedContact_BeforeMerging);
				AssertEquals(retainedStaff, retainedStaff_BeforeMerging);

				AssertEquals(true, IsPersonExistInFactory(Factory, dissolvedPersonPK));
				AssertEquals(true, IsPersonExistInFactory(Factory, retainedPersonPK));
				AssertEquals(true, IsApplicantExistInFactory(Factory, retainedApplicantPK));
				AssertEquals(true, IsContactExistInFactory(Factory, retainedContactPK));
				AssertEquals(true, IsStaffExistInFactory(Factory, retainedStaffPK));

				AssertEquals(true, IsPersonExistInDB(Factory, dissolvedPersonPK));
				AssertEquals(true, IsPersonExistInDB(Factory, retainedPersonPK));
				AssertEquals(true, IsApplicantExistInDB(Factory, retainedApplicantPK));
				AssertEquals(true, IsContactExistInDB(Factory, retainedContactPK));
				AssertEquals(true, IsStaffExistInDB(Factory, retainedStaffPK));

				AssertEquals(0, dissolvedPerson_BeforeMerging.ApplicantCollection.Count);
				AssertEquals(0, dissolvedPerson_BeforeMerging.ContactCollection.Count);
				AssertEquals(0, dissolvedPerson_BeforeMerging.StaffCollection.Count);

				AssertEquals(1, retainedPerson_BeforeMerging.ApplicantCollection.Count);
				AssertEquals(1, retainedPerson_BeforeMerging.ContactCollection.Count);
				AssertEquals(1, retainedPerson_BeforeMerging.StaffCollection.Count);

				AssertEquals(0, applicants_DissolvedPerson_BeforeMerging.Length);
				AssertEquals(1, applicants_RetainedPerson_BeforeMerging.Length);

				AssertEquals(0, contacts_DissolvedPerson_BeforeMerging.Length);
				AssertEquals(1, contacts_RetainedPerson_BeforeMerging.Length);

				AssertEquals(0, staffs_DissolvedPerson_BeforeMerging.Length);
				AssertEquals(1, staffs_RetainedPerson_BeforeMerging.Length);

				AssertEquals(retainedPersonPK, retainedStaff_BeforeMerging.GS_PER);
			});

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var dissolvedPerson_AfterMerging = GetPersonByPKfromDB(Factory, dissolvedPersonPK);
			var retainedPerson_AfterMerging = GetPersonByPKfromDB(Factory, retainedPersonPK);
			var retainedApplicant_AfterMerging = GetApplicantByPKfromDB(Factory, retainedApplicantPK);
			var retainedContact_AfterMerging = GetContactByPKfromDB(Factory, retainedContactPK);
			var retainedStaff_AfterMerging = GetStaffByPKfromDB(Factory, retainedStaffPK);

			var applicants_DissolvedPerson_AfterMerging = GetPersonsApplicantsFromDB(Factory, dissolvedPersonPK);
			var applicants_RetainedPerson_AfterMerging = GetPersonsApplicantsFromDB(Factory, retainedPersonPK);

			var contacts_DissolvedPerson_AfterMerging = GetPersonsContactsFromDB(Factory, dissolvedPersonPK);
			var contacts_RetainedPerson_AfterMerging = GetPersonsContactsFromDB(Factory, retainedPersonPK);

			var staffs_DissolvedPerson_AfterMergingMerging = GetPersonsStaffsFromDB(Factory, dissolvedPersonPK);
			var staffs_RetainedPerson_AfterMerging = GetPersonsStaffsFromDB(Factory, retainedPersonPK);

			CombineAssertions(() =>
			{
				AssertEquals("Dissolved person should be deleted in factory after merge", false, IsPersonExistInFactory(Factory, dissolvedPersonPK));
				AssertEquals("Retained person should not be deleted in factory after merge", true, IsPersonExistInFactory(Factory, retainedPersonPK));

				AssertEquals("Retained applicant should not be deleted in factory after merge", true, IsApplicantExistInFactory(Factory, retainedApplicantPK));
				AssertEquals("Retained contact should not be deleted in factory after merge", true, IsContactExistInFactory(Factory, retainedContactPK));
				AssertEquals("Retained staff should not be deleted in factory after merge", true, IsStaffExistInFactory(Factory, retainedStaffPK));

				AssertEquals("Dissolved person should be deleted in database after merge", false, IsPersonExistInDB(Factory, dissolvedPersonPK));
				AssertEquals("Retained person should not be deleted in database after merge", true, IsPersonExistInDB(Factory, retainedPersonPK));

				AssertEquals("Retained applicant should not be deleted in database after merge", true, IsApplicantExistInDB(Factory, retainedApplicantPK));
				AssertEquals("Retained contact should not be deleted in database after merge", true, IsContactExistInDB(Factory, retainedContactPK));
				AssertEquals("Retained staff should not be deleted in database after merge", true, IsStaffExistInDB(Factory, retainedStaffPK));

				AssertEquals("Retained person should have 1 applicant in ApplicantCollection after merge", 1, retainedPerson_AfterMerging.ApplicantCollection.Count);
				AssertEquals("Retained person should have 1 contact in ContactCollection after merge", 1, retainedPerson_AfterMerging.ContactCollection.Count);
				AssertEquals("Retained person should have 1 staff in StaffCollection after merge", 1, retainedPerson_AfterMerging.StaffCollection.Count);

				AssertEquals("Dissolved applicants should remain 0 after merge", 0, applicants_DissolvedPerson_AfterMerging.Length);
				AssertEquals("Retained applicants should remain 1 after merge", 1, applicants_RetainedPerson_AfterMerging.Length);

				AssertEquals("Dissolved contacts should remain 0 after merge", 0, contacts_DissolvedPerson_AfterMerging.Length);
				AssertEquals("Retained contacts should remain 1 after merge", 1, contacts_RetainedPerson_AfterMerging.Length);

				AssertEquals("Dissolved personshould remain 0 after merge", 0, staffs_DissolvedPerson_AfterMergingMerging.Length);
				AssertEquals("Retained person should remain 1 after merge", 1, staffs_RetainedPerson_AfterMerging.Length);

				AssertEquals("Retained applicant's HA_PER should remain a retained person's PK", retainedPersonPK, retainedApplicant_AfterMerging.HA_PER);
				AssertEquals("Retained contact's OC_PER should remain a retained person's PK", retainedPersonPK, retainedContact_AfterMerging.OC_PER);
				AssertEquals("Retained staff's GS_PER should remain a retained person's PK", retainedPersonPK, retainedStaff_AfterMerging.GS_PER);
			});
		}

		public void TestPersonMerge_Integration_9RetainedChildren_9DissolvedChildren()
		{
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedApplicant1 = PersonAssociations.AddNewHRJobApplicantToPerson(Factory, dissolvedPerson);
			var dissolvedApplicant2 = PersonAssociations.AddNewHRJobApplicantToPerson(Factory, dissolvedPerson);
			var dissolvedApplicant3 = PersonAssociations.AddNewHRJobApplicantToPerson(Factory, dissolvedPerson);
			var dissolvedContact1 = PersonAssociations.AddNewContactToPerson(Factory, dissolvedPerson);
			var dissolvedContact2 = PersonAssociations.AddNewContactToPerson(Factory, dissolvedPerson);
			var dissolvedContact3 = PersonAssociations.AddNewContactToPerson(Factory, dissolvedPerson);
			var dissolvedStaff1 = PersonAssociations.AddNewStaffToPerson(Factory, dissolvedPerson);
			var dissolvedStaff2 = PersonAssociations.AddNewStaffToPerson(Factory, dissolvedPerson);
			var dissolvedStaff3 = PersonAssociations.AddNewStaffToPerson(Factory, dissolvedPerson);

			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var retainedApplicant1 = PersonAssociations.AddNewHRJobApplicantToPerson(Factory, retainedPerson);
			var retainedApplicant2 = PersonAssociations.AddNewHRJobApplicantToPerson(Factory, retainedPerson);
			var retainedApplicant3 = PersonAssociations.AddNewHRJobApplicantToPerson(Factory, retainedPerson);
			var retainedContact1 = PersonAssociations.AddNewContactToPerson(Factory, retainedPerson);
			var retainedContact2 = PersonAssociations.AddNewContactToPerson(Factory, retainedPerson);
			var retainedContact3 = PersonAssociations.AddNewContactToPerson(Factory, retainedPerson);
			var retainedStaff1 = PersonAssociations.AddNewStaffToPerson(Factory, retainedPerson);
			var retainedStaff2 = PersonAssociations.AddNewStaffToPerson(Factory, retainedPerson);
			var retainedStaff3 = PersonAssociations.AddNewStaffToPerson(Factory, retainedPerson);

			Factory.Save();

			var dissolvedPersonPK = dissolvedPerson.PK;
			var dissolvedApplicant1PK = dissolvedApplicant1.PK;
			var dissolvedApplicant2PK = dissolvedApplicant2.PK;
			var dissolvedApplicant3PK = dissolvedApplicant3.PK;
			var dissolvedContact1PK = dissolvedContact1.PK;
			var dissolvedContact2PK = dissolvedContact2.PK;
			var dissolvedContact3PK = dissolvedContact3.PK;
			var dissolvedStaff1PK = dissolvedStaff1.PK;
			var dissolvedStaff2PK = dissolvedStaff2.PK;
			var dissolvedStaff3PK = dissolvedStaff3.PK;

			var retainedPersonPK = retainedPerson.PK;
			var retainedApplicant1PK = retainedApplicant1.PK;
			var retainedApplicant2PK = retainedApplicant2.PK;
			var retainedApplicant3PK = retainedApplicant3.PK;
			var retainedContact1PK = retainedContact1.PK;
			var retainedContact2PK = retainedContact2.PK;
			var retainedContact3PK = retainedContact3.PK;
			var retainedStaff1PK = retainedStaff1.PK;
			var retainedStaff2PK = retainedStaff2.PK;
			var retainedStaff3PK = retainedStaff3.PK;

			var retainedPerson_BeforeMerging = GetPersonByPKfromDB(Factory, retainedPersonPK);
			var retainedApplicant1_BeforeMerging = GetApplicantByPKfromDB(Factory, retainedApplicant1PK);
			var retainedApplicant2_BeforeMerging = GetApplicantByPKfromDB(Factory, retainedApplicant2PK);
			var retainedApplicant3_BeforeMerging = GetApplicantByPKfromDB(Factory, retainedApplicant3PK);
			var retainedContact1_BeforeMerging = GetContactByPKfromDB(Factory, retainedContact1PK);
			var retainedContact2_BeforeMerging = GetContactByPKfromDB(Factory, retainedContact2PK);
			var retainedContact3_BeforeMerging = GetContactByPKfromDB(Factory, retainedContact3PK);
			var retainedStaff1_BeforeMerging = GetStaffByPKfromDB(Factory, retainedStaff1PK);
			var retainedStaff2_BeforeMerging = GetStaffByPKfromDB(Factory, retainedStaff2PK);
			var retainedStaff3_BeforeMerging = GetStaffByPKfromDB(Factory, retainedStaff3PK);

			var dissolvedPerson_BeforeMerging = GetPersonByPKfromDB(Factory, dissolvedPersonPK);
			var dissolvedApplicant1_BeforeMerging = GetApplicantByPKfromDB(Factory, dissolvedApplicant1PK);
			var dissolvedApplicant2_BeforeMerging = GetApplicantByPKfromDB(Factory, dissolvedApplicant2PK);
			var dissolvedApplicant3_BeforeMerging = GetApplicantByPKfromDB(Factory, dissolvedApplicant3PK);
			var dissolvedContact1_BeforeMerging = GetContactByPKfromDB(Factory, dissolvedContact1PK);
			var dissolvedContact2_BeforeMerging = GetContactByPKfromDB(Factory, dissolvedContact2PK);
			var dissolvedContact3_BeforeMerging = GetContactByPKfromDB(Factory, dissolvedContact3PK);
			var dissolvedStaff1_BeforeMerging = GetStaffByPKfromDB(Factory, dissolvedStaff1PK);
			var dissolvedStaff2_BeforeMerging = GetStaffByPKfromDB(Factory, dissolvedStaff2PK);
			var dissolvedStaff3_BeforeMerging = GetStaffByPKfromDB(Factory, dissolvedStaff3PK);

			var applicants_DissolvedPerson_BeforeMerging = GetPersonsApplicantsFromDB(Factory, dissolvedPersonPK);
			var applicants_RetainedPerson_BeforeMerging = GetPersonsApplicantsFromDB(Factory, retainedPersonPK);

			var contacts_DissolvedPerson_BeforeMerging = GetPersonsContactsFromDB(Factory, dissolvedPersonPK);
			var contacts_RetainedPerson_BeforeMerging = GetPersonsContactsFromDB(Factory, retainedPersonPK);

			var staffs_DissolvedPerson_BeforeMerging = GetPersonsStaffsFromDB(Factory, dissolvedPersonPK);
			var staffs_RetainedPerson_BeforeMerging = GetPersonsStaffsFromDB(Factory, retainedPersonPK);

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals(dissolvedPerson, dissolvedPerson_BeforeMerging);
				AssertEquals(dissolvedApplicant1, dissolvedApplicant1_BeforeMerging);
				AssertEquals(dissolvedContact1, dissolvedContact1_BeforeMerging);
				AssertEquals(dissolvedStaff1, dissolvedStaff1_BeforeMerging);
				AssertEquals(dissolvedApplicant2, dissolvedApplicant2_BeforeMerging);
				AssertEquals(dissolvedContact2, dissolvedContact2_BeforeMerging);
				AssertEquals(dissolvedStaff2, dissolvedStaff2_BeforeMerging);
				AssertEquals(dissolvedApplicant3, dissolvedApplicant3_BeforeMerging);
				AssertEquals(dissolvedContact3, dissolvedContact3_BeforeMerging);
				AssertEquals(dissolvedStaff3, dissolvedStaff3_BeforeMerging);

				AssertEquals(retainedPerson, retainedPerson_BeforeMerging);
				AssertEquals(retainedApplicant1, retainedApplicant1_BeforeMerging);
				AssertEquals(retainedContact1, retainedContact1_BeforeMerging);
				AssertEquals(retainedStaff1, retainedStaff1_BeforeMerging);
				AssertEquals(retainedApplicant2, retainedApplicant2_BeforeMerging);
				AssertEquals(retainedContact2, retainedContact2_BeforeMerging);
				AssertEquals(retainedStaff2, retainedStaff2_BeforeMerging);
				AssertEquals(retainedApplicant3, retainedApplicant3_BeforeMerging);
				AssertEquals(retainedContact3, retainedContact3_BeforeMerging);
				AssertEquals(retainedStaff3, retainedStaff3_BeforeMerging);

				AssertEquals(true, IsPersonExistInFactory(Factory, dissolvedPersonPK));
				AssertEquals(true, IsApplicantExistInFactory(Factory, dissolvedApplicant1PK));
				AssertEquals(true, IsApplicantExistInFactory(Factory, dissolvedApplicant2PK));
				AssertEquals(true, IsApplicantExistInFactory(Factory, dissolvedApplicant3PK));
				AssertEquals(true, IsContactExistInFactory(Factory, dissolvedContact1PK));
				AssertEquals(true, IsContactExistInFactory(Factory, dissolvedContact2PK));
				AssertEquals(true, IsContactExistInFactory(Factory, dissolvedContact3PK));
				AssertEquals(true, IsStaffExistInFactory(Factory, dissolvedStaff1PK));
				AssertEquals(true, IsStaffExistInFactory(Factory, dissolvedStaff2PK));
				AssertEquals(true, IsStaffExistInFactory(Factory, dissolvedStaff3PK));

				AssertEquals(true, IsPersonExistInFactory(Factory, retainedPersonPK));
				AssertEquals(true, IsApplicantExistInFactory(Factory, retainedApplicant1PK));
				AssertEquals(true, IsApplicantExistInFactory(Factory, retainedApplicant2PK));
				AssertEquals(true, IsApplicantExistInFactory(Factory, retainedApplicant3PK));
				AssertEquals(true, IsContactExistInFactory(Factory, retainedContact1PK));
				AssertEquals(true, IsContactExistInFactory(Factory, retainedContact2PK));
				AssertEquals(true, IsContactExistInFactory(Factory, retainedContact3PK));
				AssertEquals(true, IsStaffExistInFactory(Factory, retainedStaff1PK));
				AssertEquals(true, IsStaffExistInFactory(Factory, retainedStaff2PK));
				AssertEquals(true, IsStaffExistInFactory(Factory, retainedStaff3PK));

				AssertEquals(true, IsPersonExistInDB(Factory, dissolvedPersonPK));
				AssertEquals(true, IsApplicantExistInDB(Factory, dissolvedApplicant1PK));
				AssertEquals(true, IsApplicantExistInDB(Factory, dissolvedApplicant2PK));
				AssertEquals(true, IsApplicantExistInDB(Factory, dissolvedApplicant3PK));
				AssertEquals(true, IsContactExistInDB(Factory, dissolvedContact1PK));
				AssertEquals(true, IsContactExistInDB(Factory, dissolvedContact2PK));
				AssertEquals(true, IsContactExistInDB(Factory, dissolvedContact3PK));
				AssertEquals(true, IsStaffExistInDB(Factory, dissolvedStaff1PK));
				AssertEquals(true, IsStaffExistInDB(Factory, dissolvedStaff2PK));
				AssertEquals(true, IsStaffExistInDB(Factory, dissolvedStaff3PK));

				AssertEquals(true, IsPersonExistInDB(Factory, retainedPersonPK));
				AssertEquals(true, IsApplicantExistInDB(Factory, retainedApplicant1PK));
				AssertEquals(true, IsApplicantExistInDB(Factory, retainedApplicant2PK));
				AssertEquals(true, IsApplicantExistInDB(Factory, retainedApplicant3PK));
				AssertEquals(true, IsContactExistInDB(Factory, retainedContact1PK));
				AssertEquals(true, IsContactExistInDB(Factory, retainedContact2PK));
				AssertEquals(true, IsContactExistInDB(Factory, retainedContact3PK));
				AssertEquals(true, IsStaffExistInDB(Factory, retainedStaff1PK));
				AssertEquals(true, IsStaffExistInDB(Factory, retainedStaff2PK));
				AssertEquals(true, IsStaffExistInDB(Factory, retainedStaff3PK));

				AssertEquals(3, dissolvedPerson_BeforeMerging.ApplicantCollection.Count);
				AssertEquals(3, dissolvedPerson_BeforeMerging.ContactCollection.Count);
				AssertEquals(3, dissolvedPerson_BeforeMerging.StaffCollection.Count);

				AssertEquals(3, retainedPerson_BeforeMerging.ApplicantCollection.Count);
				AssertEquals(3, retainedPerson_BeforeMerging.ContactCollection.Count);
				AssertEquals(3, retainedPerson_BeforeMerging.StaffCollection.Count);

				AssertEquals(3, applicants_DissolvedPerson_BeforeMerging.Length);
				AssertEquals(3, applicants_RetainedPerson_BeforeMerging.Length);

				AssertEquals(3, contacts_DissolvedPerson_BeforeMerging.Length);
				AssertEquals(3, contacts_RetainedPerson_BeforeMerging.Length);

				AssertEquals(3, staffs_DissolvedPerson_BeforeMerging.Length);
				AssertEquals(3, staffs_RetainedPerson_BeforeMerging.Length);

				AssertEquals(retainedPersonPK, retainedApplicant1_BeforeMerging.HA_PER);
				AssertEquals(retainedPersonPK, retainedApplicant2_BeforeMerging.HA_PER);
				AssertEquals(retainedPersonPK, retainedApplicant3_BeforeMerging.HA_PER);
				AssertEquals(retainedPersonPK, retainedContact1_BeforeMerging.OC_PER);
				AssertEquals(retainedPersonPK, retainedContact2_BeforeMerging.OC_PER);
				AssertEquals(retainedPersonPK, retainedContact3_BeforeMerging.OC_PER);
				AssertEquals(retainedPersonPK, retainedStaff1_BeforeMerging.GS_PER);
				AssertEquals(retainedPersonPK, retainedStaff2_BeforeMerging.GS_PER);
				AssertEquals(retainedPersonPK, retainedStaff3_BeforeMerging.GS_PER);
				AssertEquals(dissolvedPersonPK, dissolvedApplicant1_BeforeMerging.HA_PER);
				AssertEquals(dissolvedPersonPK, dissolvedApplicant2_BeforeMerging.HA_PER);
				AssertEquals(dissolvedPersonPK, dissolvedApplicant3_BeforeMerging.HA_PER);
				AssertEquals(dissolvedPersonPK, dissolvedContact1_BeforeMerging.OC_PER);
				AssertEquals(dissolvedPersonPK, dissolvedContact2_BeforeMerging.OC_PER);
				AssertEquals(dissolvedPersonPK, dissolvedContact3_BeforeMerging.OC_PER);
				AssertEquals(dissolvedPersonPK, dissolvedStaff1_BeforeMerging.GS_PER);
				AssertEquals(dissolvedPersonPK, dissolvedStaff2_BeforeMerging.GS_PER);
				AssertEquals(dissolvedPersonPK, dissolvedStaff3_BeforeMerging.GS_PER);
			});

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var retainedPerson_AfterMerging = GetPersonByPKfromDB(Factory, retainedPersonPK);
			var retainedApplicant1_AfterMerging = GetApplicantByPKfromDB(Factory, retainedApplicant1PK);
			var retainedApplicant2_AfterMerging = GetApplicantByPKfromDB(Factory, retainedApplicant2PK);
			var retainedApplicant3_AfterMerging = GetApplicantByPKfromDB(Factory, retainedApplicant3PK);
			var retainedContact1_AfterMerging = GetContactByPKfromDB(Factory, retainedContact1PK);
			var retainedContact2_AfterMerging = GetContactByPKfromDB(Factory, retainedContact2PK);
			var retainedContact3_AfterMerging = GetContactByPKfromDB(Factory, retainedContact3PK);
			var retainedStaff1_AfterMerging = GetStaffByPKfromDB(Factory, retainedStaff1PK);
			var retainedStaff2_AfterMerging = GetStaffByPKfromDB(Factory, retainedStaff2PK);
			var retainedStaff3_AfterMerging = GetStaffByPKfromDB(Factory, retainedStaff3PK);

			var dissolvedPerson_AfterMerging = GetPersonByPKfromDB(Factory, dissolvedPersonPK);
			var dissolvedApplicant1_AfterMerging = GetApplicantByPKfromDB(Factory, dissolvedApplicant1PK);
			var dissolvedApplicant2_AfterMerging = GetApplicantByPKfromDB(Factory, dissolvedApplicant2PK);
			var dissolvedApplicant3_AfterMerging = GetApplicantByPKfromDB(Factory, dissolvedApplicant3PK);
			var dissolvedContact1_AfterMerging = GetContactByPKfromDB(Factory, dissolvedContact1PK);
			var dissolvedContact2_AfterMerging = GetContactByPKfromDB(Factory, dissolvedContact2PK);
			var dissolvedContact3_AfterMerging = GetContactByPKfromDB(Factory, dissolvedContact3PK);
			var dissolvedStaff1_AfterMerging = GetStaffByPKfromDB(Factory, dissolvedStaff1PK);
			var dissolvedStaff2_AfterMerging = GetStaffByPKfromDB(Factory, dissolvedStaff2PK);
			var dissolvedStaff3_AfterMerging = GetStaffByPKfromDB(Factory, dissolvedStaff3PK);

			var applicants_DissolvedPerson_AfterMerging = GetPersonsApplicantsFromDB(Factory, dissolvedPersonPK);
			var applicants_RetainedPerson_AfterMerging = GetPersonsApplicantsFromDB(Factory, retainedPersonPK);

			var contacts_DissolvedPerson_AfterMerging = GetPersonsContactsFromDB(Factory, dissolvedPersonPK);
			var contacts_RetainedPerson_AfterMerging = GetPersonsContactsFromDB(Factory, retainedPersonPK);

			var staffs_DissolvedPerson_AfterMerging = GetPersonsStaffsFromDB(Factory, dissolvedPersonPK);
			var staffs_RetainedPerson_AfterMerging = GetPersonsStaffsFromDB(Factory, retainedPersonPK);

			CombineAssertions(() =>
			{
				AssertEquals("Dissolved person should be deleted in factory after merge", false, IsPersonExistInFactory(Factory, dissolvedPersonPK));
				AssertEquals("Retained person should not be deleted in factory after merge", true, IsPersonExistInFactory(Factory, retainedPersonPK));

				AssertEquals("Retained applicant1 should not be deleted in factory after merge", true, IsApplicantExistInFactory(Factory, retainedApplicant1PK));
				AssertEquals("Retained applicant2 should not be deleted in factory after merge", true, IsApplicantExistInFactory(Factory, retainedApplicant1PK));
				AssertEquals("Retained applicant3 should not be deleted in factory after merge", true, IsApplicantExistInFactory(Factory, retainedApplicant3PK));
				AssertEquals("Retained contact1 should not be deleted in factory after merge", true, IsContactExistInFactory(Factory, retainedContact1PK));
				AssertEquals("Retained contact2 should not be deleted in factory after merge", true, IsContactExistInFactory(Factory, retainedContact2PK));
				AssertEquals("Retained contact3 should not be deleted in factory after merge", true, IsContactExistInFactory(Factory, retainedContact3PK));
				AssertEquals("Retained staff1 should not be deleted in factory after merge", true, IsStaffExistInFactory(Factory, retainedStaff1PK));
				AssertEquals("Retained staff2 should not be deleted in factory after merge", true, IsStaffExistInFactory(Factory, retainedStaff2PK));
				AssertEquals("Retained staff3 should not be deleted in factory after merge", true, IsStaffExistInFactory(Factory, retainedStaff3PK));

				AssertEquals("Dissolved applicant1 should not be deleted in factory after merge", true, IsApplicantExistInFactory(Factory, dissolvedApplicant1PK));
				AssertEquals("Dissolved applicant2 should not be deleted in factory after merge", true, IsApplicantExistInFactory(Factory, dissolvedApplicant1PK));
				AssertEquals("Dissolved applicant3 should not be deleted in factory after merge", true, IsApplicantExistInFactory(Factory, dissolvedApplicant3PK));
				AssertEquals("Dissolved contact1 should not be deleted in factory after merge", true, IsContactExistInFactory(Factory, dissolvedContact1PK));
				AssertEquals("Dissolved contact2 should not be deleted in factory after merge", true, IsContactExistInFactory(Factory, dissolvedContact2PK));
				AssertEquals("Dissolved contact3 should not be deleted in factory after merge", true, IsContactExistInFactory(Factory, dissolvedContact3PK));
				AssertEquals("Dissolved staff1 should not be deleted in factory after merge", true, IsStaffExistInFactory(Factory, dissolvedStaff1PK));
				AssertEquals("Dissolved staff2 should not be deleted in factory after merge", true, IsStaffExistInFactory(Factory, dissolvedStaff2PK));
				AssertEquals("Dissolved staff3 should not be deleted in factory after merge", true, IsStaffExistInFactory(Factory, dissolvedStaff3PK));

				AssertEquals("Dissolved person should be deleted in database after merge", false, IsPersonExistInDB(Factory, dissolvedPersonPK));
				AssertEquals("Retained person should not be deleted in database after merge", true, IsPersonExistInDB(Factory, retainedPersonPK));

				AssertEquals("Retained applicant1 should not be deleted in database after merge", true, IsApplicantExistInDB(Factory, retainedApplicant1PK));
				AssertEquals("Retained applicant2 should not be deleted in database after merge", true, IsApplicantExistInDB(Factory, retainedApplicant1PK));
				AssertEquals("Retained applicant3 should not be deleted in database after merge", true, IsApplicantExistInDB(Factory, retainedApplicant3PK));
				AssertEquals("Retained contact1 should not be deleted in database after merge", true, IsContactExistInDB(Factory, retainedContact1PK));
				AssertEquals("Retained contact2 should not be deleted in database after merge", true, IsContactExistInDB(Factory, retainedContact2PK));
				AssertEquals("Retained contact3 should not be deleted in database after merge", true, IsContactExistInDB(Factory, retainedContact3PK));
				AssertEquals("Retained staff1 should not be deleted in database after merge", true, IsStaffExistInDB(Factory, retainedStaff1PK));
				AssertEquals("Retained staff2 should not be deleted in database after merge", true, IsStaffExistInDB(Factory, retainedStaff2PK));
				AssertEquals("Retained staff3 should not be deleted in database after merge", true, IsStaffExistInDB(Factory, retainedStaff3PK));

				AssertEquals("Dissolved applicant1 should not be deleted in database after merge", true, IsApplicantExistInDB(Factory, dissolvedApplicant1PK));
				AssertEquals("Dissolved applicant2 should not be deleted in database after merge", true, IsApplicantExistInDB(Factory, dissolvedApplicant1PK));
				AssertEquals("Dissolved applicant3 should not be deleted in database after merge", true, IsApplicantExistInDB(Factory, dissolvedApplicant3PK));
				AssertEquals("Dissolved contact1 should not be deleted in database after merge", true, IsContactExistInDB(Factory, dissolvedContact1PK));
				AssertEquals("Dissolved contact2 should not be deleted in database after merge", true, IsContactExistInDB(Factory, dissolvedContact2PK));
				AssertEquals("Dissolved contact3 should not be deleted in database after merge", true, IsContactExistInDB(Factory, dissolvedContact3PK));
				AssertEquals("Dissolved staff1 should not be deleted in database after merge", true, IsStaffExistInDB(Factory, dissolvedStaff1PK));
				AssertEquals("Dissolved staff2 should not be deleted in database after merge", true, IsStaffExistInDB(Factory, dissolvedStaff2PK));
				AssertEquals("Dissolved staff3 should not be deleted in database after merge", true, IsStaffExistInDB(Factory, dissolvedStaff3PK));

				AssertEquals("Retained person should have 6 applicants in ApplicantCollection after merge", 6, retainedPerson_AfterMerging.ApplicantCollection.Count);
				AssertEquals("Retained person should have 6 contacts in ContactCollection after merge", 6, retainedPerson_AfterMerging.ContactCollection.Count);
				AssertEquals("Retained person should have 6 staffs in StaffCollection after merge", 6, retainedPerson_AfterMerging.StaffCollection.Count);

				AssertEquals("Dissolved person should have 0 applicants after merge", 0, applicants_DissolvedPerson_AfterMerging.Length);
				AssertEquals("Retained person should have 6 applicants after merge", 6, applicants_RetainedPerson_AfterMerging.Length);

				AssertEquals("Dissolved person should have 0 applicants after merge", 0, contacts_DissolvedPerson_AfterMerging.Length);
				AssertEquals("Retained person should have 6 applicants after merge", 6, contacts_RetainedPerson_AfterMerging.Length);

				AssertEquals("Dissolved person should have 0 applicants after merge", 0, staffs_DissolvedPerson_AfterMerging.Length);
				AssertEquals("Retained person should have 6 applicants after merge", 6, staffs_RetainedPerson_AfterMerging.Length);

				AssertEquals("Retained applicant1's HA_PER should remain a retained person's PK", retainedPersonPK, retainedApplicant1_AfterMerging.HA_PER);
				AssertEquals("Retained applicant2's HA_PER should remain a retained person's PK", retainedPersonPK, retainedApplicant2_AfterMerging.HA_PER);
				AssertEquals("Retained applicant3's HA_PER should remain a retained person's PK", retainedPersonPK, retainedApplicant3_AfterMerging.HA_PER);
				AssertEquals("Retained contact1's OC_PER should remain a retained person's PK", retainedPersonPK, retainedContact1_AfterMerging.OC_PER);
				AssertEquals("Retained contact2's OC_PER should remain a retained person's PK", retainedPersonPK, retainedContact2_AfterMerging.OC_PER);
				AssertEquals("Retained contact3's OC_PER should remain a retained person's PK", retainedPersonPK, retainedContact3_AfterMerging.OC_PER);
				AssertEquals("Retained staff1's GS_PER should remain a retained person's PK", retainedPersonPK, retainedStaff1_AfterMerging.GS_PER);
				AssertEquals("Retained staff2's GS_PER should remain a retained person's PK", retainedPersonPK, retainedStaff2_AfterMerging.GS_PER);
				AssertEquals("Retained staff3's GS_PER should remain a retained person's PK", retainedPersonPK, retainedStaff3_AfterMerging.GS_PER);

				AssertEquals("Dissovled applicant1's HA_PER should change to retained person's PK", retainedPersonPK, dissolvedApplicant1_AfterMerging.HA_PER);
				AssertEquals("Dissovled applicant2's HA_PER should change to retained person's PK", retainedPersonPK, dissolvedApplicant2_AfterMerging.HA_PER);
				AssertEquals("Dissovled applicant3's HA_PER should change to retained person's PK", retainedPersonPK, dissolvedApplicant3_AfterMerging.HA_PER);
				AssertEquals("Dissovled contact1's OC_PER should change to retained person's PK", retainedPersonPK, dissolvedContact1_AfterMerging.OC_PER);
				AssertEquals("Dissovled contact2's OC_PER should change to retained person's PK", retainedPersonPK, dissolvedContact2_AfterMerging.OC_PER);
				AssertEquals("Dissovled contact3's OC_PER should change to retained person's PK", retainedPersonPK, dissolvedContact3_AfterMerging.OC_PER);
				AssertEquals("Dissovled staff1's GS_PER should change to retained person's PK", retainedPersonPK, dissolvedStaff1_AfterMerging.GS_PER);
				AssertEquals("Dissovled staff2's GS_PER should change to retained person's PK", retainedPersonPK, dissolvedStaff2_AfterMerging.GS_PER);
				AssertEquals("Dissovled staff3's GS_PER should change to retained person's PK", retainedPersonPK, dissolvedStaff3_AfterMerging.GS_PER);
			});
		}

		public void TestPersonMerge_Integration_0RetainedChildren_0DissolvedChildren()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();

			Factory.Save();

			var dissolvedPersonPK = dissolvedPerson.PK;
			var retainedPersonPK = retainedPerson.PK;

			var dissolvedPerson_BeforeMerging = GetPersonByPKfromDB(Factory, dissolvedPersonPK);
			var retainedPerson_BeforeMerging = GetPersonByPKfromDB(Factory, retainedPersonPK);

			var applicants_DissolvedPerson_BeforeMerging = GetPersonsApplicantsFromDB(Factory, dissolvedPersonPK);
			var applicants_RetainedPerson_BeforeMerging = GetPersonsApplicantsFromDB(Factory, retainedPersonPK);

			var contacts_DissolvedPerson_BeforeMerging = GetPersonsContactsFromDB(Factory, dissolvedPersonPK);
			var contacts_RetainedPerson_BeforeMerging = GetPersonsContactsFromDB(Factory, retainedPersonPK);

			var staffs_DissolvedPerson_BeforeMerging = GetPersonsStaffsFromDB(Factory, dissolvedPersonPK);
			var staffs_RetainedPerson_BeforeMerging = GetPersonsStaffsFromDB(Factory, retainedPersonPK);

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals(dissolvedPerson, dissolvedPerson_BeforeMerging);
				AssertEquals(retainedPerson, retainedPerson_BeforeMerging);

				AssertEquals(true, IsPersonExistInFactory(Factory, dissolvedPersonPK));
				AssertEquals(true, IsPersonExistInFactory(Factory, retainedPersonPK));

				AssertEquals(true, IsPersonExistInDB(Factory, dissolvedPersonPK));
				AssertEquals(true, IsPersonExistInDB(Factory, retainedPersonPK));

				AssertEquals(0, dissolvedPerson_BeforeMerging.ApplicantCollection.Count);
				AssertEquals(0, dissolvedPerson_BeforeMerging.ContactCollection.Count);
				AssertEquals(0, dissolvedPerson_BeforeMerging.StaffCollection.Count);

				AssertEquals(0, retainedPerson_BeforeMerging.ApplicantCollection.Count);
				AssertEquals(0, retainedPerson_BeforeMerging.ContactCollection.Count);
				AssertEquals(0, retainedPerson_BeforeMerging.StaffCollection.Count);

				AssertEquals(0, applicants_DissolvedPerson_BeforeMerging.Length);
				AssertEquals(0, applicants_RetainedPerson_BeforeMerging.Length);

				AssertEquals(0, contacts_DissolvedPerson_BeforeMerging.Length);
				AssertEquals(0, contacts_RetainedPerson_BeforeMerging.Length);

				AssertEquals(0, staffs_DissolvedPerson_BeforeMerging.Length);
				AssertEquals(0, staffs_RetainedPerson_BeforeMerging.Length);
			});

			using (var merger = new PersonMerger(retainedPerson, dissolvedPerson))
			{
				merger.Merge();
			}

			var dissolvedPerson_AfterMerging = GetPersonByPKfromDB(Factory, dissolvedPersonPK);
			var retainedPerson_AfterMerging = GetPersonByPKfromDB(Factory, retainedPersonPK);

			var applicants_DissolvedPerson_AfterMerging = GetPersonsApplicantsFromDB(Factory, dissolvedPersonPK);
			var applicants_RetainedPerson_AfterMerging = GetPersonsApplicantsFromDB(Factory, retainedPersonPK);

			var contacts_DissolvedPerson_AfterMerging = GetPersonsContactsFromDB(Factory, dissolvedPersonPK);
			var contacts_RetainedPerson_AfterMerging = GetPersonsContactsFromDB(Factory, retainedPersonPK);

			var staffs_DissolvedPerson_AfterMergingMerging = GetPersonsStaffsFromDB(Factory, dissolvedPersonPK);
			var staffs_RetainedPerson_AfterMerging = GetPersonsStaffsFromDB(Factory, retainedPersonPK);

			CombineAssertions(() =>
			{
				AssertEquals("Dissolved person should be deleted in factory after merge", false, IsPersonExistInFactory(Factory, dissolvedPersonPK));
				AssertEquals("Retained person should not be deleted in factory after merge", true, IsPersonExistInFactory(Factory, retainedPersonPK));

				AssertEquals("Dissolved person should be deleted in database after merge", false, IsPersonExistInDB(Factory, dissolvedPersonPK));
				AssertEquals("Retained person should not be deleted in database after merge", true, IsPersonExistInDB(Factory, retainedPersonPK));

				AssertEquals("Retained person should have 0 applicants in ApplicantCollection after merge", 0, retainedPerson_AfterMerging.ApplicantCollection.Count);
				AssertEquals("Retained person should have 0 contacts in ContactCollection after merge", 0, retainedPerson_AfterMerging.ContactCollection.Count);
				AssertEquals("Retained person should have 0 staffs in StaffCollection after merge", 0, retainedPerson_AfterMerging.StaffCollection.Count);

				AssertEquals("Dissolved applicants should remain 0 after merge", 0, applicants_DissolvedPerson_AfterMerging.Length);
				AssertEquals("Retained applicants should remain 0 after merge", 0, applicants_RetainedPerson_AfterMerging.Length);

				AssertEquals("Dissolved contacts should remain 0 after merge", 0, contacts_DissolvedPerson_AfterMerging.Length);
				AssertEquals("Retained contacts should remain 0 after merge", 0, contacts_RetainedPerson_AfterMerging.Length);

				AssertEquals("Dissolved staffs should remain 0 after merge", 0, staffs_DissolvedPerson_AfterMergingMerging.Length);
				AssertEquals("Retained staffs should remain 0 after merge", 0, staffs_RetainedPerson_AfterMerging.Length);
			});
		}

		#endregion

		#region Implementation

		void AssertCodeDescriptionPairListContainsPair(CodeDescriptionPairList pairList, string code, string description)
		{
			AssertEquals($"Pairlist contains {code}:", true, pairList.ContainsCode(code));
			AssertEquals($"Description for {code} = {description}:", description, pairList.GetDescriptionFromCode(code));
		}

		public class PersonMergeTransactionSaverForTest : PersonMergeTransactionSaver
		{
			readonly bool shouldAddFailingFactory;
			readonly Exception exceptionToThrow;

			public PersonMergeTransactionSaverForTest(bool shouldAddFailingFactory = false)
			{
				this.shouldAddFailingFactory = shouldAddFailingFactory;
			}

			public PersonMergeTransactionSaverForTest(Exception exceptionToThrow, bool shouldAddFailingFactory = false)
			{
				this.shouldAddFailingFactory = shouldAddFailingFactory;
				this.exceptionToThrow = exceptionToThrow;
			}

			public override void Save(GlbPerson retainedPerson, GlbPerson dissolvedPerson)
			{
				if (shouldAddFailingFactory)
				{
					if (Factories.Count == 1)
					{
						Factories.Add(Factories[0]);
						Factories[0] = new SaveAlwaysFailsFactory(exceptionToThrow);
					}
					else
					{
						Factories.Add(new SaveAlwaysFailsFactory(exceptionToThrow));
					}
				}

				base.Save(retainedPerson, dissolvedPerson);
			}
		}

		GlbPerson GetPersonByPKfromDB(BusinessObjectFactory factory, ZGuid personPK)
		{
			return factory.Load<GlbPerson>(personPK);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		bool IsPersonExistInDB(BusinessObjectFactory factory, ZGuid personPK)
		{
			return factory.ExistsInDatabase(GlbPerson.Schema.TableName, new ZQuery(GlbPersonSchema.PK, personPK));
		}

		bool IsPersonExistInFactory(BusinessObjectFactory factory, ZGuid personPK)
		{
			return factory.Exists(typeof(GlbPerson), new ZQuery(GlbPersonSchema.PK, personPK));
		}

		GlbStaff[] GetPersonsStaffsFromDB(BusinessObjectFactory factory, ZGuid personPK)
		{
			return factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_PER, personPK));
		}

		GlbStaff GetStaffByPKfromDB(BusinessObjectFactory factory, ZGuid staffPK)
		{
			var query = new ZQuery(GlbStaffSchema.PK, staffPK);
			query.ReLoadExistingRows = true;
			return factory.LoadTop1<GlbStaff>(query);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		bool IsStaffExistInDB(BusinessObjectFactory factory, ZGuid staffPK)
		{
			return factory.ExistsInDatabase(GlbStaff.Schema.TableName, new ZQuery(GlbStaffSchema.PK, staffPK));
		}

		bool IsStaffExistInFactory(BusinessObjectFactory factory, ZGuid staffPK)
		{
			return factory.Exists(typeof(GlbStaff), new ZQuery(GlbStaffSchema.PK, staffPK));
		}

		OrgContact[] GetPersonsContactsFromDB(BusinessObjectFactory factory, ZGuid personPK)
		{
			return factory.Load<OrgContact>(new ZQuery(OrgContactSchema.OC_PER, personPK));
		}

		OrgContact GetContactByPKfromDB(BusinessObjectFactory factory, ZGuid contactPK)
		{
			var query = new ZQuery(OrgContactSchema.PK, contactPK);
			query.ReLoadExistingRows = true;
			return factory.LoadTop1<OrgContact>(query);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		bool IsContactExistInDB(BusinessObjectFactory factory, ZGuid contactPK)
		{
			return factory.ExistsInDatabase(OrgContact.Schema.TableName, new ZQuery(OrgContactSchema.PK, contactPK));
		}

		bool IsContactExistInFactory(BusinessObjectFactory factory, ZGuid contactPK)
		{
			return factory.Exists(typeof(OrgContact), new ZQuery(OrgContactSchema.PK, contactPK));
		}

		HRJobApplicant[] GetPersonsApplicantsFromDB(BusinessObjectFactory factory, ZGuid personPK)
		{
			var query = new ZQuery(HRJobApplicantSchema.HA_PER, personPK);
			query.ReLoadExistingRows = true;
			return factory.Load<HRJobApplicant>(query);
		}

		HRJobApplicant GetApplicantByPKfromDB(BusinessObjectFactory factory, ZGuid applicantPK)
		{
			var query = new ZQuery(HRJobApplicantSchema.PK, applicantPK);
			query.ReLoadExistingRows = true;
			return factory.LoadTop1<HRJobApplicant>(query);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		bool IsApplicantExistInDB(BusinessObjectFactory factory, ZGuid applicantPK)
		{
			return factory.ExistsInDatabase(HRJobApplicant.Schema.TableName, new ZQuery(HRJobApplicantSchema.PK, applicantPK));
		}

		bool IsApplicantExistInFactory(BusinessObjectFactory factory, ZGuid applicantPK)
		{
			return factory.Exists(typeof(HRJobApplicant), new ZQuery(HRJobApplicantSchema.PK, applicantPK));
		}

		public static class PersonAssociations
		{
			public static GlbStaff AddNewStaffToPerson(BusinessObjectFactory factory, GlbPerson person)
			{
				var newStaff = factory.NewWithValidTestData<GlbStaff>();
				newStaff.UpdateFromPerson(person);
				newStaff.GS_LoginName = $"{Guid.NewGuid()}";
				newStaff.GS_PER = person.PK;
				return newStaff;
			}

			public static OrgContact AddNewContactToPerson(BusinessObjectFactory factory, GlbPerson person)
			{
				var org = factory.NewWithValidTestData<OrgHeader>();
				var newContact = org.Contacts.AddNew();
				newContact.OC_PER = person.PK;
				person.ContactCollection.Add(newContact);
				return newContact;
			}

			public static HRJobApplicant AddNewHRJobApplicantToPerson(BusinessObjectFactory factory, GlbPerson person)
			{
				var newHRJobApplicant = factory.NewWithValidTestData<HRJobApplicant>();
				newHRJobApplicant.UpdateFromPerson(person);
				newHRJobApplicant.HA_EmailAddress = $"{Guid.NewGuid()}@email.com";
				newHRJobApplicant.HA_PER = person.PK;
				person.ApplicantCollection.Add(newHRJobApplicant);
				return newHRJobApplicant;
			}

			public static GlbStaff CreateNewStaff(BusinessObjectFactory factory)
			{
				var newStaff = factory.NewWithValidTestData<GlbStaff>();
				newStaff.GS_LoginName = $"{Guid.NewGuid()}";
				return newStaff;
			}

			public static OrgContact CreateNewContact(BusinessObjectFactory factory)
			{
				var org = factory.NewWithValidTestData<OrgHeader>();
				var newContact = org.Contacts.AddNew();
				return newContact;
			}

			public static HRJobApplicant CreateNewApplicant(BusinessObjectFactory factory)
			{
				var newHRJobApplicant = factory.NewWithValidTestData<HRJobApplicant>();
				newHRJobApplicant.HA_EmailAddress = $"{Guid.NewGuid()}@email.com";
				return newHRJobApplicant;
			}
		}

		class PersonMergeTransactionSaverForTransactionFailedTest : IPersonMergeTransactionSaver
		{
			public bool IsSuccessful => false;

			List<IFactory> factories;

			public List<IFactory> Factories
			{
				get { return factories ?? (factories = new List<IFactory>()); }
			}

			public IPersonMergeTransactionSaver AddParticipant(IFactory factory)
			{
				Factories.Add(factory);

				return this;
			}

			public void Save(GlbPerson retainedPerson, GlbPerson dissolvedPerson)
			{
			}
		}

		#endregion
	}
}
