using System.ComponentModel;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public class DuplicationPersonCandidate : DuplicationCandidate
	{
		public DuplicationPersonCandidate(DeduplicationPresenterModel presenterModel, DeduplicationGlbPerson glbPerson)
			: base(presenterModel, glbPerson)
		{
			Phone = glbPerson != null
				? PhoneNumberFormatter.FormatInternational(glbPerson.PER_MobilePhone, Environment.Env.CurrentCompany.Country.Code).ToString()
				: TranslatedMasterIsNullDescription;

			var personFactory = (glbPerson?.Master.Factory.IsOwnedByCurrentThread ?? false) ? glbPerson.Master.Factory : ReadOnlyFactory;
			RelatedTo = glbPerson?.GetRelatedTo(personFactory) ?? string.Empty;

			Code = string.IsNullOrEmpty(RelatedTo)
				? glbPerson?.PER_FullName
				: string.Format(CultureInfo.InvariantCulture, "{0}: {1}", glbPerson?.PER_FullName, RelatedTo);

			GlbPerson = glbPerson;
		}

		DuplicationPersonCandidate() : base() { }

		public static DuplicationPersonCandidate Empty { get; } = new DuplicationPersonCandidate();

		public DeduplicationGlbPerson GlbPerson { get; }

		public ZPropertyInfo PhoneInfo => GetZPropertyInfo(nameof(Phone));
		public ZString Phone { get; }

		public ZPropertyInfo TitleInfo => GetZPropertyInfo(nameof(Title));
		public ZString Title { get; } = ZString.Empty;

		public ZPropertyInfo RelatedToInfo => GetZPropertyInfo(nameof(RelatedTo));
		public ZString RelatedTo { get; }

		public ZPropertyInfo IsDissolvedInfo => GetZPropertyInfo(nameof(IsDissolved));
		public ZBool IsDissolved
		{
			get => isDissolved;
			set
			{
				isDissolved = value;
				IsDissolvedInfo.RefreshBinding();
				IsDissolvedChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDissolved)));
			}
		}
		bool isDissolved;

		public string Code { get; }

		#region Implementation

		public event PropertyChangedEventHandler IsDissolvedChanged;

		PhoneNumberFormatterAndValidator PhoneNumberFormatter => phoneNumberFormatter ?? (phoneNumberFormatter = new PhoneNumberFormatterAndValidator());
		PhoneNumberFormatterAndValidator phoneNumberFormatter;

		ReadOnlyBusinessObjectFactory ReadOnlyFactory
		{
			get
			{
				if (readOnlyFactory == null || !readOnlyFactory.IsOwnedByCurrentThread)
				{
					readOnlyFactory = new ReadOnlyBusinessObjectFactory();
				}

				return readOnlyFactory;
			}
		}
		ReadOnlyBusinessObjectFactory readOnlyFactory;

		protected override string TranslatedMasterIsNullDescription { get; } = Res.GetString("e96160d3-34d9-4ffd-8921-ef8d3fc24ec2", "Person is null");

		#endregion Implementation
	}
}
