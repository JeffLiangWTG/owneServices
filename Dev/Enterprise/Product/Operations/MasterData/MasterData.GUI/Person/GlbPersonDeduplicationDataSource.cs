using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.GUI
{
	class GlbPersonDeduplicationDataSource : DeduplicationDataSource<IGlbPerson, DuplicationPersonCandidate>
	{
		public GlbPersonDeduplicationDataSource(DeduplicationGlbPerson master, IEnumerable<IDeduplicationGlowObject> targets, IEnumerable<DeduplicationPresenterModel> results)
			: base(master, targets.Cast<DeduplicationGlbPerson>(), results)
		{
		}

		protected override string CountryCode
		{
			get
			{
				return ((DeduplicationGlbPerson)MasterGlow).CountryCode;
			}
		}

		protected override Guid GetPK(IGlbPerson target)
		{
			return target.PER_PK;
		}

		ReadOnlyBusinessObjectFactory readOnlyFactory;
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

		PhoneNumberFormatterAndValidator PhoneNumberFormatter => phoneNumberFormatter ?? (phoneNumberFormatter = new PhoneNumberFormatterAndValidator());
		PhoneNumberFormatterAndValidator phoneNumberFormatter;

		#region WPF

		protected override PotentialDuplicationModel GetPotentialDuplicationModel(IGlbPerson currentBizo, IGrouping<Guid, DeduplicationPresenterModel> target)
		{
			var person = currentBizo as DeduplicationGlbPerson;
			var model = target.FirstOrDefault();
			var personFactory = person?.Master.Factory;
			var relatedTo = person != null ? person.GetRelatedTo(personFactory.IsOwnedByCurrentThread ? personFactory : ReadOnlyFactory) : string.Empty;
			var code = string.IsNullOrEmpty(relatedTo) ? person?.PER_FullName : string.Format(CultureInfo.InvariantCulture, "{0}: {1}", person?.PER_FullName, relatedTo);

			return new PotentialDuplicationModel
			{
				Code = code,
				Confidence = model != null ? model.Confidence : ConfidenceRating.Undefined,
				Name = person?.PER_FullName,
				PK = model != null ? target.Key : Guid.Empty,
				Score = model != null ? model.MainScore : 0,
				DeduplicationPresenterModels = target,
				PersonType = person != null ? person.Master.Info : Res.GetString("1e8d6ffd-5196-4179-87ee-1975a4e617ad", "Person is null"),
				Phone = person != null
					? PhoneNumberFormatter.FormatInternational(person.PER_MobilePhone, Environment.Env.CurrentCompany.Country.Code).ToString()
					: Res.GetString("1e8d6ffd-5196-4179-87ee-1975a4e617ad", "Person is null"),
				IsActive = (person?.PER_IsActive ?? false),
				IsDummy = (person?.IsDummy ?? false),
				RelatedTo = relatedTo
			};
		}

		#endregion WPF

		#region WinForm

		protected override DuplicationPersonCandidate GetDuplicationCandidate(IGlbPerson currentBizo, IGrouping<Guid, DeduplicationPresenterModel> target)
		{
			var person = currentBizo as DeduplicationGlbPerson;
			var model = target.FirstOrDefault();

			return new DuplicationPersonCandidate(model, person)
			{
				TargetPK = model != null ? target.Key : Guid.Empty,
				DeduplicationPresenterModels = target,
			};
		}

		protected override IEnumerable<DuplicationPersonCandidate> RatingCandidates(IEnumerable<DuplicationPersonCandidate> candidates)
		{
			var ratedCandidates = new List<DuplicationPersonCandidate>();

			if (candidates.Any())
			{
				var ratingGroups = candidates.OrderByDescending(x => x.Confidence).GroupBy(x => x.Confidence);

				foreach (var group in ratingGroups)
				{
					ratedCandidates.AddRange(group.Where(x => !x.IsDummy).OrderByDescending(x => x.Score));
				}
			}

			return ratedCandidates;
		}

		#endregion WinForm
	}
}
