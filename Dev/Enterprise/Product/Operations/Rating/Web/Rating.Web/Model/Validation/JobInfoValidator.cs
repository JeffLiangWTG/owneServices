using System.Linq;
using FluentValidation;

namespace Enterprise.Rating.Web.Model.Validation
{
	class JobInfoValidator : BaseValidator<JobInfo>
	{
		public JobInfoValidator(SourceEndpoint source, bool isContainerized) : base(source)
		{
			if (Source == SourceEndpoint.JobCharges)
			{
				RuleFor(i => i.Containers)
					.NotEmpty()
					.WithMessage(Res.GetString("2E58434A-E579-46B6-95FA-558BDF01798B", "{0} of {1} should contain at least one element for calculating job charges.", nameof(JobInfo.Containers), nameof(RateQuery.JobInfo)));

				RuleFor(i => i.Containers)
					.Must(cs => !cs.Where(c => c == null).Any())
					.WithMessage(Res.GetString("6297F138-2D10-4A98-859A-147D08716FFA", "Provided {0} is not valid. It shouldn't contain null elements.", nameof(JobInfo.Containers)))
					.When(i => i.Containers?.Any() ?? false);

				RuleFor(i => i.Containers)
					.ForEach(jc => jc.SetValidator(new JobContainerValidator(Source, isContainerized)))
					.When(i => i.Containers?.Any() ?? false);

				RuleFor(i => i.JobServices)
					.Must(js => !js.Where(s => s == null).Any())
					.WithMessage(Res.GetString("BC809B40-B786-496B-AF2A-B2820EABE8F0", "Provided {0} is not valid. It shouldn't contain null elements.", nameof(JobInfo.JobServices)))
					.When(i => i.JobServices?.Any() ?? false);

				RuleFor(i => i.JobServices)
					.ForEach(jc => jc.SetValidator(new JobServiceValidator(Source)))
					.When(i => (i.JobServices?.Any() ?? false));

				RuleFor(i => i.GoodsValueCurrency)
					.NotEmpty()
					.WithMessage(Res.GetString("EB221187-0FDE-4140-BE4C-9B4D2AFBB24D", "{0} is Mandatory when {1} is specified.", nameof(JobInfo.GoodsValueCurrency), nameof(JobInfo.GoodsValue)))
					.When(i => i.GoodsValue > 0);

				RuleFor(i => i.InsuranceValueCurrency)
					.NotEmpty()
					.WithMessage(Res.GetString("D660B72E-5F89-48C3-899F-EB31AC27850C", "{0} is Mandatory when {1} is specified.", nameof(JobInfo.InsuranceValueCurrency), nameof(JobInfo.InsuranceValue)))
					.When(i => i.InsuranceValue > 0);

				RuleFor(i => i.PickupDropMode)
					.Must(dm => JobInfo.DropModes.All.Contains(dm))
					.When(i => !string.IsNullOrEmpty(i.PickupDropMode))
					.WithMessage
						(
							Res.GetString
								(
									"73611C9D-5953-405D-A340-D964B83A717C",
									"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
									nameof(JobInfo.PickupDropMode),
									string.Join(", ", JobInfo.DropModes.All.Select(t => $"'{t}'"))
								)
						);

				RuleFor(i => i.DeliveryDropMode)
					.Must(dm => JobInfo.DropModes.All.Contains(dm))
					.When(i => !string.IsNullOrEmpty(i.DeliveryDropMode))
					.WithMessage
						(
							Res.GetString
								(
									"6ACAF06F-3903-4C5E-A1D6-A58DBAF11B52",
									"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
									nameof(JobInfo.DeliveryDropMode),
									string.Join(", ", JobInfo.DropModes.All.Select(t => $"'{t}'"))
								)
						);

				RuleForEach(x => x.CustomFields)
					.SetValidator(new CustomFieldValidator(Source))
					.When(x => x.CustomFields != null);

				RuleFor(x => x.CustomFields)
					.Must(cf => cf.Select(x => x.Name.ToLowerInvariant()).Distinct().Count() == cf.Length)
					.WithMessage(Res.GetString("975ddf56-4ac7-448b-9787-2436725abef0", "{0} contains duplicate entries for {1} field", nameof(JobInfo.CustomFields), nameof(CustomField.Name)))
					.When(x => x.CustomFields != null);
			}
		}
	}
}
