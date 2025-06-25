using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CargoWise.Billing.API;


namespace CargoWise.Billing.API
{
	public static class BillingTransactionValidator
	{
		public static void ValidateTransaction(BillingTransaction transaction)
		{
			if (!ValidateTransactionCore(transaction, out var validationResults))
			{
				throw new ValidationException("Transaction validation failed.",
					validationResults.Select(error => error.ErrorMessage));
			}
		}

		public static void ValidateTransactions(IEnumerable<BillingTransaction> transactions)
		{
			int index = 1;
			foreach (var transaction in transactions)
			{
				if (!ValidateTransactionCore(transaction, out var validationResults))
				{
					throw new ValidationException("Validation failed for transaction " + index + " of " + transactions.Count(),
						validationResults.Select(error => error.ErrorMessage));
				}
				++index;
			}
		}

		private static bool ValidateTransactionCore(BillingTransaction transaction, out List<ValidationResult> validationResults)
		{
			var context = new ValidationContext(transaction, serviceProvider: null, items: null);
			validationResults = new List<ValidationResult>();
			return Validator.TryValidateObject(transaction, context, validationResults, true);
		}

	}
}