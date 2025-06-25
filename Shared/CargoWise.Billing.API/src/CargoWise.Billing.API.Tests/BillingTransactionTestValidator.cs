using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CargoWise.Billing.API;
using CargoWise.Billing.API.Tests;


namespace CargoWise.Billing.API
{
	public static class BillingTransactionTestValidator
	{
		public static void ValidateTransaction(BillingTransactionTestClass transaction)
		{
			if (!ValidateTransactionCore(transaction, out var validationResults))
			{
				throw new ValidationException("Transaction validation failed.",
					validationResults.Select(error => error.ErrorMessage));
			}
		}

		private static bool ValidateTransactionCore(BillingTransactionTestClass transaction, out List<ValidationResult> validationResults)
		{
			var context = new ValidationContext(transaction, serviceProvider: null, items: null);
			validationResults = new List<ValidationResult>();
			return Validator.TryValidateObject(transaction, context, validationResults, true);
		}

	}
}