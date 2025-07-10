using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class DirtyRecordFinder : IDirtyRecordFinder
	{
		protected virtual int LargeOrgContactsThreshold => 100;
		protected virtual int MediumOrgContactsThreshold => 20;
		protected virtual int NumberOfEmailAddressesThreshold => 20;
		protected virtual int NumberOfPhoneNumbersThreshold => 50;
		const float MaxDomainUniquenessRatio = 0.6F;
		const float MinEmailUniquenessRatio = 0.8F;
		const float MinPhoneUniquenessRatio = 0.8F;
		const float MinNonEmptyFieldsRatio = 1.0F;
		const float MaxEmptyContactsRatio = 0.9F;

		int numberOfContacts;
		int emailCount;
		int phoneCount;
		readonly OrgHeader org;
		string dirtyReason;

		public DirtyRecordFinder(OrgHeader org)
		{
			this.org = org;
			numberOfContacts = -1;
		}

		IEnumerable<OrgContact> AllContacts => org.Contacts.Cast<OrgContact>();

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		int NumberOfContacts
		{
			get
			{
				if (numberOfContacts == -1)
				{
					var sql = FormattableString.Invariant($@"SELECT COUNT(1) FROM [{OrgContactSchema.Constants.TableName}] WHERE {OrgContactSchema.Constants.OC_OH} = @OrgPk");
					using (var command = Db.Connection.Command(sql))
					{
						command.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, org.PK.ToGuid());
						var result = command.ExecuteScalar();
						numberOfContacts = Convert.ToInt32(result, CultureInfo.InvariantCulture);
					}
				}

				return numberOfContacts;
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Used by debugger only")]
		bool IsEmailConditionMet()
		{
			var isDomainUniquenessConditionMet = false;
			var isEmailUniquenessConditionMet = false;

			emailCount = AllContacts.Count(contact => !string.IsNullOrEmpty(contact.OC_Email));

			if (emailCount > NumberOfEmailAddressesThreshold)
			{
				var uniqueDomains = AllContacts
					.Where(contact => !string.IsNullOrEmpty(contact.OC_Email))
					.Select(contact => contact.OC_Email.ToLower().Substring(contact.OC_Email.IndexOf('@') + 1))
					.Distinct()
					.Count();

				var domainUniquenessRatio = (float)uniqueDomains / emailCount;
				isDomainUniquenessConditionMet = domainUniquenessRatio > MaxDomainUniquenessRatio;

				if (isDomainUniquenessConditionMet)
				{
					dirtyReason += $"\nDomain uniqueness ratio is too high: {domainUniquenessRatio}";
				}

				var uniqueEmails = AllContacts
					.Where(contact => !string.IsNullOrEmpty(contact.OC_Email))
					.Select(contact => contact.OC_Email.ToLower())
					.Distinct()
					.Count();

				var emailUniquenessRatio = (float)uniqueEmails / emailCount;
				isEmailUniquenessConditionMet = emailUniquenessRatio < MinEmailUniquenessRatio;

				if (isEmailUniquenessConditionMet)
				{
					dirtyReason += $"\nEmail uniqueness ratio is too low: {emailUniquenessRatio}";
				}
			}

			return isDomainUniquenessConditionMet || isEmailUniquenessConditionMet;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Used by debugger only")]
		bool IsPhoneConditionMet()
		{
			var isPhoneUniquenessConditionMet = false;
			phoneCount = AllContacts.Count(contact => !string.IsNullOrEmpty(contact.OC_Phone));

			if (phoneCount > NumberOfPhoneNumbersThreshold)
			{
				var uniquePhones = AllContacts
					.Where(contact => !string.IsNullOrEmpty(contact.OC_Phone))
					.Select(contact => contact.OC_Phone)
					.Distinct()
					.Count();

				var phoneUniquenessRatio = (float)uniquePhones / phoneCount;
				isPhoneUniquenessConditionMet = phoneUniquenessRatio < MinPhoneUniquenessRatio;

				if (isPhoneUniquenessConditionMet)
				{
					dirtyReason += $"\nPhone uniqueness ratio is too low: {phoneUniquenessRatio}";
				}
			}
			return isPhoneUniquenessConditionMet;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Used by debugger only")]
		bool IsNonEmptyFieldsConditionMet()
		{
			var isEmptyFieldsRatioConditionMet = false;
			var isEmptyContactsConditionMet = false;

			var titleCount = AllContacts.Count(contact => !string.IsNullOrEmpty(contact.OC_Title));
			var mobilesCount = AllContacts.Count(contact => !string.IsNullOrEmpty(contact.OC_Mobile));
			var faxesCount = AllContacts.Count(contact => !string.IsNullOrEmpty(contact.OC_Fax));
			var emptyContactsCount = AllContacts.Count(contact => string.IsNullOrEmpty(contact.OC_Mobile) && string.IsNullOrEmpty(contact.OC_Title) && string.IsNullOrEmpty(contact.OC_Fax) && string.IsNullOrEmpty(contact.OC_Phone) && string.IsNullOrEmpty(contact.OC_Email));
			var nonEmptyFieldsRatio = (float)(emailCount + phoneCount + titleCount + mobilesCount + faxesCount) / NumberOfContacts;
			var emptyContactsRatio = (float)emptyContactsCount / NumberOfContacts;

			isEmptyFieldsRatioConditionMet = nonEmptyFieldsRatio < MinNonEmptyFieldsRatio;
			if (isEmptyFieldsRatioConditionMet)
			{
				dirtyReason += $"\nNon-empty fields ratio is too low: {nonEmptyFieldsRatio}";
			}

			isEmptyContactsConditionMet = emptyContactsRatio > MaxEmptyContactsRatio;
			if (isEmptyContactsConditionMet)
			{
				dirtyReason += $"\nContacts with insufficient details ratio is too high: {emptyContactsRatio}";
			}

			return isEmptyFieldsRatioConditionMet || isEmptyContactsConditionMet;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Used by debugger only")]
		bool IsMediumOrgDirtyConditionMet()
		{
			var emptyContactsCount = AllContacts.Count(contact => string.IsNullOrEmpty(contact.OC_Mobile) && string.IsNullOrEmpty(contact.OC_Title) && string.IsNullOrEmpty(contact.OC_Fax) && string.IsNullOrEmpty(contact.OC_Phone) && string.IsNullOrEmpty(contact.OC_Email));
			var isEmptyContactsConditionMet = emptyContactsCount == NumberOfContacts;
			if (isEmptyContactsConditionMet)
			{
				dirtyReason += (NoResString)"\nNo contacts with sufficient details found";
			}
			return isEmptyContactsConditionMet;
		}

		public bool IsOrgDirtyForDeduplication()
		{
			var isOrgDirtyForDeduplication = false;
			dirtyReason = (NoResString)"The record is considered dirty for the following reasons:";

			if (NumberOfContacts > LargeOrgContactsThreshold)
			{
				isOrgDirtyForDeduplication = IsEmailConditionMet() | IsPhoneConditionMet() | IsNonEmptyFieldsConditionMet();
			}
			else if (NumberOfContacts > MediumOrgContactsThreshold)
			{
				isOrgDirtyForDeduplication = IsMediumOrgDirtyConditionMet();
			}
			else
			{
				dirtyReason = "";
			}

			return isOrgDirtyForDeduplication;
		}

		public string GetDirtyReason() => dirtyReason;
	}
}
