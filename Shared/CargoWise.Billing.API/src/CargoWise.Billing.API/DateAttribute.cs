using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.Billing.API
{
	public class DateAttribute : ValidationAttribute
	{
		private readonly int minYears;
		private readonly int minMonths;
		private readonly int minDays;
		private readonly int maxYears;
		private readonly int maxMonths;
		private readonly int maxDays;

		public DateAttribute(int minYears, int minMonths, int minDays, int maxYears, int maxMonths, int maxDays) : base()
		{
			this.minYears = minYears;
			this.minMonths = minMonths;
			this.minDays = minDays;
			this.maxYears = maxYears;
			this.maxMonths = maxMonths;
			this.maxDays = maxDays;
		}


		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if (!DateTime.TryParse(value.ToString(), out var utcDateTime))
				return new ValidationResult($"Invalid DateTime Format, Value is : {value}");
			if (DateTime.Today.AddYears(minYears).AddMonths(minMonths).AddDays(minDays) < utcDateTime
			    &&
			    utcDateTime < DateTime.Today.AddYears(maxYears).AddMonths(maxMonths).AddDays(maxDays))
			{
				return ValidationResult.Success;
			}
			return new ValidationResult(ErrorMessage);
		}
	}
}