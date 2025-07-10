using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public sealed class PersonMerger : IDisposable
	{
		readonly BusinessObjectFactory newFactory;
		GlbPerson retainedPerson;
		GlbPerson dissolvedPerson;
		readonly List<Guid> personsToAddLocks;
		readonly BusinessObjectFactory originalFactory;
		readonly GlbPerson originalRetainedPerson;
		readonly GlbPerson originalDissolvedPerson;
		readonly IPersonMergeTransactionSaver transactionSaver;
		readonly LoadWithAppLockResult<Guid> dissolvedAndRetainedPersonPkWithLock;
		readonly PersonMergeBusinessObjectFactoryLoader businessObjectFactoryLoaderProvider;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This Exception message is not visible to the user")]
		public const string PersonNotInDatabaseExceptionMessage = "Either dissolvedPerson or retainedPerson cannot be found in the database";
		public const string DissolvedAndRetainedPersonLockKey = "PersonMergerDissolvedAndRetained";

		public PersonMerger(GlbPerson retainedPerson, GlbPerson dissolvedPerson)
			: this(retainedPerson, dissolvedPerson, new PersonMergeTransactionSaver(), new PersonMergeBusinessObjectFactoryLoader())
		{
		}

		public PersonMerger(GlbPerson retainedPerson, GlbPerson dissolvedPerson, IPersonMergeTransactionSaver transactionSaver, PersonMergeBusinessObjectFactoryLoader businessObjectFactoryLoaderProvider)
		{
			Argument.NotNull(businessObjectFactoryLoaderProvider, nameof(businessObjectFactoryLoaderProvider));
			Argument.NotNull(retainedPerson, nameof(retainedPerson));
			Argument.NotNull(dissolvedPerson, nameof(dissolvedPerson));
			Argument.NotNull(transactionSaver, nameof(transactionSaver));

			if (!retainedPerson.IsInDatabase || !dissolvedPerson.IsInDatabase)
			{
				throw new InvalidOperationException(PersonNotInDatabaseExceptionMessage);
			}

			originalFactory = retainedPerson.Factory;
			originalRetainedPerson = retainedPerson;
			originalDissolvedPerson = dissolvedPerson;

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			personsToAddLocks = new List<Guid> { dissolvedPerson.PK.ToGuid(), retainedPerson.PK.ToGuid() };
			dissolvedAndRetainedPersonPkWithLock = personsToAddLocks.ApplyAppLocks(DissolvedAndRetainedPersonLockKey);
			this.transactionSaver = transactionSaver;
			this.businessObjectFactoryLoaderProvider = businessObjectFactoryLoaderProvider;
		}

		public MultiPersonMergerResult multiPersonMergerResult { get; private set; }

		public bool Merge(PersonMergeMode mode = PersonMergeMode.Single)
		{
			if (dissolvedAndRetainedPersonPkWithLock.ItemsWithLocks.Count == personsToAddLocks.Count)
			{
				dissolvedPerson = newFactory.Load<GlbPerson>(originalDissolvedPerson.PK);
				retainedPerson = newFactory.Load<GlbPerson>(originalRetainedPerson.PK);
			}

			if (dissolvedPerson != null && retainedPerson != null)
			{
				var logString = CreateLogString();
				PersonMergeEmailSender emailSender = null;
				var dissolvedPersonPK = ZGuid.Empty;
				if (mode == PersonMergeMode.Single)
				{
					emailSender = new PersonMergeEmailSender(retainedPerson, new ReadOnlyCollection<GlbPerson>(new[] { dissolvedPerson }));
					dissolvedPersonPK = dissolvedPerson.PK;
				}

				CopyPropertiesFromDissolvedPerson(MergePreview);
				SyncRelatedPersonRecords();
				CreateNote();
				CreateEditLog(logString);
				CreateMergedPerson(retainedPerson.PK, dissolvedPerson.PK);
				UpdateDissolvedPerson(dissolvedPerson);
				transactionSaver
					.AddParticipant(newFactory)
					.Save(retainedPerson, dissolvedPerson);

				if (transactionSaver.IsSuccessful)
				{
					SyncPersonBusinessObjectFactory(mode);

					if (emailSender != null && !dissolvedPersonPK.IsEmpty)
					{
						emailSender.MarkPersonAsDissolved(dissolvedPersonPK);
						var mergeNotificationEmail = emailSender.GetMergedAccountsEmail();
						if (mergeNotificationEmail != null)
						{
							Env.OutgoingMailManager.CreateAndSave(mergeNotificationEmail);
						}
					}
				}
			}

			return transactionSaver.IsSuccessful;
		}

		void SyncPersonBusinessObjectFactory(PersonMergeMode mode)
		{
			if (mode == PersonMergeMode.Single)
			{
				businessObjectFactoryLoaderProvider.Reload(newFactory, originalRetainedPerson, dissolvedPerson, originalFactory, mode);
			}
			else
			{
				multiPersonMergerResult = new MultiPersonMergerResult
				{
					DissolvedPerson = dissolvedPerson,
					IsSuccessful = transactionSaver.IsSuccessful,
					NewFactory = newFactory,
					OriginalFactory = originalFactory
				};
			}
		}

		void SyncRelatedPersonRecords()
		{
			foreach (var staff in dissolvedPerson.StaffCollection)
			{
				staff.UpdateFromPerson(retainedPerson);
			}

			foreach (var contact in dissolvedPerson.ContactCollection.Cast<OrgContact>().ToArray())
			{
				contact.UpdateFromPerson(retainedPerson);

				if (retainedPerson.HasPassword)
				{
					contact.RemovePasswordAndHash();
				}
			}

			if (retainedPerson.HasPassword)
			{
				retainedPerson.ContactCollection.Cast<OrgContact>().ForEach(x => x.RemovePasswordAndHash());
			}

			PasswordHistoryHelper.MovePasswordHistories(dissolvedPerson, retainedPerson);

			foreach (var applicant in dissolvedPerson.ApplicantCollection.Cast<HRJobApplicant>().ToArray())
			{
				applicant.UpdateFromPerson(retainedPerson);
			}
		}

		void CreateEditLog(string logString)
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			retainedPerson.Logs.AddNew(AutoEvents.EditedARecord, logString);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

		string CreateLogString()
		{
			string logString;
			var staffCount = dissolvedPerson.StaffCollection.Count;
			var contactCount = dissolvedPerson.ContactCollection.Count;
			var applicantCount = dissolvedPerson.ApplicantCollection.Count;

			if (staffCount + contactCount + applicantCount == 0)
			{
				logString = string.Format(CultureInfo.InvariantCulture, (NoResString)"Merged '{0}' into this person.",
								dissolvedPerson.PER_FullName);
			}
			else
			{
				logString = string.Format(CultureInfo.InvariantCulture, (NoResString)"Merged '{0}' into this person and inherited {1} staff, {2} contact, {3} job applicant.",
								dissolvedPerson.PER_FullName,
								staffCount,
								contactCount,
								applicantCount);
			}

			return logString;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		void CreateNote()
		{
			if (mergePreview.DiscardedProperties.Count > 0)
			{
				var mergeNote = newFactory.New<StmNote>();
				mergeNote.ST_ParentID = retainedPerson.PK;
				mergeNote.ST_Table = GlbPerson.Schema.TableName;
				mergeNote.ST_NoteType = nameof(StmNoteVisibility.INT);
				mergeNote.ST_IsCustomDescription = true;
				mergeNote.ST_Description = ResString.GetMultilingualString("d5d62e9b-601c-43eb-a2dc-10b7b67c83b1", "Properties Discarded During Merge");

				var noteData = new ZStringBuilder();

				noteData.AppendLine(ResString.GetMultilingualString("2ec12864-3b10-4d96-a1d8-bfa8cb2271a4",
					"Person {0} was merged into this Person on {1}.",
					dissolvedPerson.PER_FullName,
					ZDateTime.Today.ToShortDateString()));

				noteData.AppendLine(ResString.GetMultilingualString("d9565fce-ada2-492b-929c-1f6a360f3c81",
					"The following properties were discarded from Person {0} during this process:",
					dissolvedPerson.PER_FullName));

				foreach (var discardedProperty in mergePreview.DiscardedProperties.Cast<ICodeDescription>())
				{
					var propertyInfo = retainedPerson.FindPropertyInfo(discardedProperty.Code);
					var readableName = propertyInfo != null ? propertyInfo.HumanReadableName : (ZString)discardedProperty.Code;
					var detail = readableName + ": " + discardedProperty.Description;
					noteData.AppendLine(detail);
				}
				mergeNote.ST_NoteData = ZBlob.FromUTF8(noteData.ToString());
			}
		}

		void UpdateDissolvedPerson(GlbPerson dissolvedPerson)
		{
			dissolvedPerson.PER_IDPUserId = Guid.Empty;
		}

		void CreateMergedPerson(ZGuid retainedPersonPk, ZGuid dissolvedPersonPk)
		{
			var mergedPerson = newFactory.New<GlbMergedPerson>();
			mergedPerson.GMP_PER_Person = retainedPersonPk;
			mergedPerson.GMP_MergedPerson = dissolvedPersonPk;
		}

		void CopyPropertiesFromDissolvedPerson(PersonMergePreview personMergePreview)
		{
			var propertiesToCopy = personMergePreview.CopiedProperties.Cast<ICodeDescription>();
			foreach (var property in propertiesToCopy)
			{
				retainedPerson[property.Code] = dissolvedPerson[property.Code];
			}

			if (propertiesToCopy.Select(x => x.Code).Contains(GlbPersonSchema.Constants.PER_WebAccessEnabled))
			{
				dissolvedPerson.PER_WebAccessEnabled = false;
				dissolvedPerson.PER_EmailAddress = dissolvedPerson.PER_EmailAddress.Insert(dissolvedPerson.PER_EmailAddress.IndexOf("@", StringComparison.OrdinalIgnoreCase), HRJobApplicant.DissolvedAlias);
			}
		}

		PersonMergePreview mergePreview;
		public PersonMergePreview MergePreview => mergePreview ?? (mergePreview = CalculatePersonsMerge());

		PersonMergePreview CalculatePersonsMerge()
		{
			var newMergePreview = new PersonMergePreview();
			var shouldCopyWebAccess = !originalRetainedPerson.PER_WebAccessEnabled && originalDissolvedPerson.PER_WebAccessEnabled;

			foreach (var propertyGroup in newMergePreview.ColumnGroupingsToMerge)
			{
				var retainedPersonPropertiesWithNonEmptyValue = propertyGroup.Where(propertyName => !PropertyIsEmpty(originalRetainedPerson, propertyName));
				var dissolvedPersonPropertiesWithNonEmptyValue = propertyGroup.Where(propertyName => !PropertyIsEmpty(originalDissolvedPerson, propertyName));
				var retainedGroupEmpty = !retainedPersonPropertiesWithNonEmptyValue.Any();
				var isWebAccessGroup = propertyGroup.Contains(GlbPersonSchema.Constants.PER_WebAccessEnabled);

				foreach (var propertyName in retainedPersonPropertiesWithNonEmptyValue)
				{
					newMergePreview.MergedProperties.AddPair(propertyName, ExtractReadablePropertyValue(originalRetainedPerson, propertyName));
				}

				foreach (var propertyName in dissolvedPersonPropertiesWithNonEmptyValue)
				{
					var dissolvedPersonPropertyReadableValue = ExtractReadablePropertyValue(originalDissolvedPerson, propertyName);

					if (retainedGroupEmpty)
					{
						newMergePreview.CopiedProperties.AddPair(propertyName, dissolvedPersonPropertyReadableValue);
						newMergePreview.MergedProperties.AddPair(propertyName, dissolvedPersonPropertyReadableValue);
					}
					else
					{
						if (isWebAccessGroup && shouldCopyWebAccess)
						{
							newMergePreview.CopiedProperties.AddPair(propertyName, dissolvedPersonPropertyReadableValue);
							newMergePreview.MergedProperties.AddOverwriteIfExists(new CodeDescriptionPair(propertyName, dissolvedPersonPropertyReadableValue));
						}
						else
						{
							var collectionToAddTo = originalDissolvedPerson[propertyName].Equals(originalRetainedPerson[propertyName]) ? newMergePreview.IdenticalProperties : newMergePreview.DiscardedProperties;
							collectionToAddTo.AddPair(propertyName, dissolvedPersonPropertyReadableValue);
						}
					}
				}
			}

			return newMergePreview;

			bool PropertyIsEmpty(GlbPerson person, string propertyName)
			{
				return IsZTypeEmpty((IZType)person[propertyName])
					|| (propertyName == GlbPersonSchema.Constants.PER_PasswordHashIterations && (ZInt)person[propertyName] == 0)
					|| (propertyName == GlbPersonSchema.Constants.PER_IDPUserId && (ZGuid)person[propertyName] == Guid.Empty);
			}
		}

		PhoneNumberFormatterAndValidator phoneFormatter;
		PhoneNumberFormatterAndValidator PhoneFormatter => phoneFormatter ?? (phoneFormatter = new PhoneNumberFormatterAndValidator());

		string ExtractReadablePropertyValue(GlbPerson person, string propertyName)
		{
			var lookUp = new GlbPersonLookups(person);
			var result = person[propertyName].ToString();

			if (propertyName == GlbPersonSchema.Constants.PER_Gender)
			{
				result = lookUp.Genders.GetMultilingualDescriptionFromCode(result) ?? result;
			}
			else if (propertyName == GlbPersonSchema.Constants.PER_PreferredLanguage)
			{
				var languageNames = new CodeDescriptionPairList(OLookUpEditType.Language);
				result = languageNames.GetMultilingualDescriptionFromCode(result) ?? result;
			}
			else if (propertyName == GlbPersonSchema.Constants.PER_RN_NKCountry)
			{
				var country = originalFactory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, result);
				result = country != null ? country.RN_DescMultilingual : result;
			}
			else if (propertyName == GlbPersonSchema.Constants.PER_HomePhone ||
				propertyName == GlbPersonSchema.Constants.PER_MobilePhone ||
				propertyName == GlbPersonSchema.Constants.PER_MobilePhone2 ||
				propertyName == GlbPersonSchema.Constants.PER_FaxNumber)
			{
				var formattedResult = PhoneFormatter.FormatInternational(result, Env.CurrentCompany.Country.Code).ToString();
				result = !string.IsNullOrEmpty(formattedResult) ? formattedResult : result;
			}
			else if (person[propertyName] is ZBool boolValue)
			{
				result = boolValue ? ResString.GetMultilingualString("30958b71-ec72-4581-b00c-0bede46616f8", "Yes") : ResString.GetMultilingualString("c21e810e-0173-49e8-88f3-5f20dccb5963", "No");
			}

			return result;
		}

		bool IsZTypeEmpty(IZType item) => item == null || ((item is ZString || item is ZDate || item is ZBlob) && (item.IsEmpty || !item.IsValid));

		public void Dispose()
		{
			dissolvedAndRetainedPersonPkWithLock?.Dispose();
		}
	}
}
