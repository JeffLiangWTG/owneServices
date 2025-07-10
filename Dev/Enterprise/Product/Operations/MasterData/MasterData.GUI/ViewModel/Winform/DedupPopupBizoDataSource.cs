using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.GUI
{
	public class DedupPopupBizoDataSource : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		readonly Action<ZGuid> showDetailsAction;
		readonly IEnumerable<ScoringResult> scoringResults;
		readonly IDeduplicatable master;
		PhoneNumberFormatterAndValidator phoneNumberFormatter;
		List<DeduplicationResultsListDataSource> deduplicationResults;

		public DedupPopupBizoDataSource(IDeduplicatable master, IEnumerable<ScoringResult> results, Action<ZGuid> openDetailsFormAction)
		{
			showDetailsAction = openDetailsFormAction;
			scoringResults = results;
			this.master = master;
		}

		public DeduplicationResultsListDataSource SelectedResult { get; set; }

		public bool ShouldCreatePopup { get; private set; } = true;

		public bool MasterIsOrgHeader => master.BizoType == typeof(OrgHeader);

		public List<DeduplicationResultsListDataSource> DeduplicationResults
		{
			get
			{
				if (deduplicationResults == null)
				{
					deduplicationResults = BuildResultList();
					ShouldCreatePopup = deduplicationResults.Any();
				}
				return deduplicationResults;
			}
			private set
			{
				deduplicationResults = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeduplicationResults)));
			}
		}

		public static bool IsSelectedItemInDB(Type type, ZGuid targetPk)
		{
			return new BusinessObjectFactory().Load(type, targetPk) != null;
		}

		public void OpenDetailsFormAction()
		{
			if (SelectedResult != null)
			{
				if (IsSelectedItemInDB(master.BizoType, SelectedResult.PK))
				{
					showDetailsAction?.Invoke(SelectedResult.PK);
				}
				else
				{
					Globals.Message.ShowInformation(Constants.DuplicateRecordRemovedPressCtrlG);
					DeduplicationResults = BuildResultList();
				}
			}
		}

		List<DeduplicationResultsListDataSource> BuildResultList()
		{
			return new List<DeduplicationResultsListDataSource>(scoringResults.Select(result =>
			{
				var bizO = (master as BusinessObject)?.Factory.Load(master.BizoType, result.TargetPK);
				DeduplicationResultsListDataSource model = null;
				if (bizO != null)
				{
					var iDedupBizO = (IDeduplicatable)bizO;
					model = new DeduplicationResultsListDataSource
					{
						PK = bizO.PK,
						IsActive = iDedupBizO.IsActive,
						ToolTip = result.Score * 100 + "%" + (iDedupBizO.IsActive ? string.Empty : ", " + Constants.InactiveToolTip),
						Confidence = result.ConfidenceRating,
						Score = result.Score
					};

					if (bizO is OrgHeader header)
					{
						EnrichOrgHeaderModel(model, header);
					}
					else if (bizO is GlbPerson peron)
					{
						EnrichPersonModel(model, peron);
					}
				}

				return model;
			}).WhereNotNull().OrderByDescending(x => x.Score).ThenByDescending(x => x.IsActive));
		}

		void EnrichOrgHeaderModel(DeduplicationResultsListDataSource model, OrgHeader header)
		{
			model.InfoCaption = Constants.CaptionCode;
			model.Info = ((IDeduplicatable)header).Info;
			model.FullName = ((IDeduplicatable)header).FullName;
			model.FullNameCaption = Constants.CaptionName;
		}

		void EnrichPersonModel(DeduplicationResultsListDataSource model, GlbPerson person)
		{
			var staffs = person.StaffCollection;
			var contacts = person.ContactCollection;
			var applicants = person.ApplicantCollection;
			var primaryRelationship = person.PrimaryRelationship;
			if (staffs.Any() && primaryRelationship != null && primaryRelationship.PPR_PrimaryTableCode == GlbStaffSchema.Constants.Prefix)
			{
				CreateStaffModel(model, person);
			}
			else if (contacts.Any() && primaryRelationship != null && primaryRelationship.PPR_PrimaryTableCode == OrgContactSchema.Constants.Prefix)
			{
				CreateContactModel(model, person);
			}
			else if (applicants.Any())
			{
				var applicant = applicants.FirstOrDefault() as IHRJobApplicant;
				CreateApplicantModel(model, applicant);
			}
			else
			{
				CreatePersonModel(model, person);
			}

			CreateAssociations(model, staffs, contacts, applicants);
		}

		void CreateContactModel(DeduplicationResultsListDataSource model, GlbPerson person)
		{
			var contact = person.ContactCollection.Cast<OrgContact>().FirstOrDefault(x => x.PK == person.PrimaryRelationship.PPR_PrimaryId);
			model.FullName = person.PER_FullName + GetTitle(person.PrimaryJobTitle);
			model.PersonType = Constants.PersonType_Contact;
			model.PersonTypePadding = ControlDpiScalingHelper.NewScaledPadding(5, 0, 23, 0, isInStandardDpi: true);

			var companyName = person.CompanyName;
			var oiAddress = contact?.EmailContactItems.Cast<EmailContactItem>().FirstOrDefault(x => !string.IsNullOrEmpty(x.OI_Address))?.OI_Address ?? string.Empty;

			if (!string.IsNullOrEmpty(companyName))
			{
				model.MainInfo = companyName;
				model.Email = oiAddress;
				model.EmailPanelVisible = !string.IsNullOrEmpty(oiAddress);
			}
			else
			{
				model.MainInfo = oiAddress;
			}
		}

		void CreateAssociations(DeduplicationResultsListDataSource model, GlbStaffCollection staffs, OrgContactCollection contacts, BusinessObjectCollection applicants)
		{
			var association = string.Empty;
			if (staffs.Count + contacts.Count + applicants.Count > 1)
			{
				var contactsAssociation = staffs.Any() ? ResString.GetMultilingualString("12087b87-9c0a-4b39-9279-e60904b1da33", "Contacts ({0})", contacts.Count) : string.Empty;
				var staffsAssociation = staffs.Any() ? ResString.GetMultilingualString("11087b87-9c0d-4b19-9279-e60903b1da33", "Staffs ({0})", staffs.Count) : string.Empty;
				var applicantsAssociation = applicants.Any() ? ResString.GetMultilingualString("a98c37ea-8530-4065-9c56-d8db6520dfc8", "Applicants ({0})", contacts.Count) : string.Empty;
				association = string.Join(" ", new string[] { contactsAssociation, staffsAssociation, applicantsAssociation }.Where(x => !string.IsNullOrEmpty(x)));
			}

			model.InfoCaption = Constants.AssociationsCaption;
			model.Associations = association;
		}

		void CreatePersonModel(DeduplicationResultsListDataSource model, GlbPerson person)
		{
			model.FullName = person.PER_FullName;
			model.PersonType = Constants.PersonType_Person;
			model.PersonTypePadding = ControlDpiScalingHelper.NewScaledPadding(5, 0, 28, 0, isInStandardDpi: true);

			var emailAddress = string.IsNullOrEmpty(person.PER_EmailAddress) ? person.PER_EmailAddress2 : person.PER_EmailAddress;
			var phoneNumber = new string[] {
				person.PER_MobilePhone_Formatted,
				person.PER_HomePhone_Formatted,
				person.PER_MobilePhone2,
				person.PER_FaxNum_Formatted,
				person.PER_HomePhoneInternal,
				person.PER_MobilePhoneInternal,
				person.PER_MobilePhone2Internal,
			}.FirstOrDefault(x => !string.IsNullOrEmpty(x)) ?? string.Empty;
			var formattedPhoneNumber = (phoneNumberFormatter = phoneNumberFormatter ?? new PhoneNumberFormatterAndValidator()).FormatInternational(phoneNumber, Environment.Env.CurrentCompany.Country.Code).ToString();

			if (!string.IsNullOrEmpty(emailAddress))
			{
				model.MainInfo = emailAddress;
				model.Phone = formattedPhoneNumber;
				model.PhonePanelVisible = !string.IsNullOrEmpty(formattedPhoneNumber);
			}
			else
			{
				model.MainInfo = formattedPhoneNumber;
			}
		}

		void CreateApplicantModel(DeduplicationResultsListDataSource model, IHRJobApplicant applicant)
		{
			model.FullName = applicant.HA_FullName;
			model.PersonType = Constants.PersonType_Applicant;
			model.PersonTypePadding = ControlDpiScalingHelper.NewScaledPadding(5, 0, 15, 0, isInStandardDpi: true);

			var emailAddress = applicant.HA_EmailAddress;
			var phoneNumber = new string[] { applicant.HA_FaxNum, applicant.HA_MobilePhone, applicant.HA_HomePhone, applicant.HA_WorkPhone }.FirstOrDefault(x => !string.IsNullOrEmpty(x)) ?? string.Empty;
			var formattedPhoneNumber = (phoneNumberFormatter = phoneNumberFormatter ?? new PhoneNumberFormatterAndValidator()).FormatInternational(phoneNumber, Environment.Env.CurrentCompany.Country.Code).ToString();

			if (!string.IsNullOrEmpty(emailAddress))
			{
				model.MainInfo = emailAddress;
				model.Phone = formattedPhoneNumber;
				model.PhonePanelVisible = !string.IsNullOrEmpty(formattedPhoneNumber);
			}
			else
			{
				model.MainInfo = formattedPhoneNumber;
			}
		}

		void CreateStaffModel(DeduplicationResultsListDataSource model, GlbPerson person)
		{
			var staff = person.StaffCollection.FirstOrDefault(x => x.PK == person.PrimaryRelationship.PPR_PrimaryId);
			model.FullName = person.PER_FullName + GetTitle(person.PrimaryJobTitle);
			model.PersonType = Constants.PersonType_Staff;
			model.PersonTypePadding = ControlDpiScalingHelper.NewScaledPadding(5, 0, 39, 0, isInStandardDpi: true);

			var emailAddress = staff?.EmailAddresses.FirstOrDefault(x => !string.IsNullOrEmpty(x.GSE_EmailAddress))?.GSE_EmailAddress ?? string.Empty;

			var companyName = person.CompanyName;
			if (string.IsNullOrEmpty(companyName))
			{
				model.MainInfo = emailAddress;
				model.EmailPanelVisible = false;
			}
			else
			{
				model.MainInfo = companyName;
				model.Email = emailAddress;
				model.EmailPanelVisible = !string.IsNullOrEmpty(model.Email);
			}
		}

		string GetTitle(string title)
		{
			return string.IsNullOrEmpty(title) ? string.Empty : " - " + title;
		}
	}
}
