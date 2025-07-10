using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.GUI
{
	public class PotentialDuplicationModel : INotifyPropertyChanged
	{
		public Guid PK { get; set; }
		public ConfidenceRating Confidence { get; set; }
		public string ConfidenceString => DedupePanelTranslationHelper.GetTranslation(Confidence.ToString());
		public string Code { get; set; }
		public string Name { get; set; }
		public string UNLOCO { get; set; }
		public string OrganisationType { get; set; }
		public string DebtorCompany { get; set; }
		public string CreditorCompany { get; set; }
		public string Active => IsActive ? Res.GetString("912a711f-7e14-445b-81e0-0985c1673083", "Yes") : Res.GetString("039cef22-86fe-4458-ae9d-3d430e169f3f", "No");
		public double Score { get; set; }
		public string ConfidenceScore => string.Format(CultureInfo.CurrentCulture, "{0}%", Confidence == ConfidenceRating.None ? 0 : Score * 100);
		public string EnterpriseId { get; set; }
		public string EnterpriseCode { get; set; }
		public string CompanyCode { get; set; }
		public string ProductId { get; set; }
		public IEnumerable<DeduplicationPresenterModel> DeduplicationPresenterModels { get; set; }
		public bool IsActive { get; set; }
		public bool IsDummy { get; set; }
		public bool IsSameCountryAsMaster { get; set; } = true;
		public string PersonType { get; set; }
		public string Phone { get; set; }
		public string Title { get; set; }
		public string Status { get; set; }
		public string IgnoredByStaff { get; set; }
		public string IgnoredForEveryoneStaff { get; set; }
		public string StatusDescription { get; set; }
		public string RelatedTo { get; set; }

		bool isDissolved;
		public bool IsDissolved
		{
			get => isDissolved;
			set
			{
				isDissolved = value;
				OnPropertyChanged(nameof(IsDissolved));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public bool IsIgnored
		{
			get
			{
				var isIgnored = false;

				if (!string.IsNullOrEmpty(IgnoredForEveryoneStaff))
				{
					isIgnored = true;
				}

				else if (!string.IsNullOrEmpty(IgnoredByStaff) && IgnoredByStaff.Contains(GlbStaff.CurrentUser.GS_Code))
				{
					var staff = IgnoredByStaff.Split(',').ToList();
					if (staff.Any(s => s.Trim().Equals(GlbStaff.CurrentUser.GS_Code)))
					{
						isIgnored = true;
					}
				}

				return isIgnored;
			}
		}
	}
}
