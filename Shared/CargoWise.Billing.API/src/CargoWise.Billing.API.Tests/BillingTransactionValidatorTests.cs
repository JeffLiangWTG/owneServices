using System;
using NUnit.Framework;

namespace CargoWise.Billing.API.Tests
{
	[TestFixture]
	public class BillingTransactionValidatorTests
	{
		[Test]
		public void TestTransactionWithIncorrectData()
		{
			var transaction = new BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDEFXYZ_",
				ClientNumber = "222222222222222222222222222222222222222222222222222",
				ClientStaffCode = "ABC_",
				PriceItemCode = "DEF_",
				Reference2 = "REFERENCE 22222222222222222222222222222222222222222",
				Reference3 = "REFERENCE 3________________________________________",
				Reference4 = "REFERENCE 4________________________________________",
				Reference5 = "REFERENCE 5________________________________________",
				MessageTrackingID = "MessageTrackingID_______________________________________%",
				ReportingSource = "XYZ_",
				ServiceOccuredUTC = DateTime.MinValue
			};
			Assert.That(() => BillingTransactionValidator.ValidateTransaction(transaction), Throws.TypeOf<ValidationException>().With.Property("Errors").EqualTo(new[]
			{
				"The Category field is required.",
				"The field PriceItemCode must be a string with a maximum length of 3.",
				"The field ReportingSource must be a string with a maximum length of 3.",
				"The field ServiceOccuredUTC is required and must be no more than 5 years in the past and no more than one month in the future.",
				"The field ClientID must be a string with a maximum length of 9.",
				"The field ClientNumber must be a string with a maximum length of 50.",
				"The field ClientStaffCode must be a string with a maximum length of 3.",
				"The Reference1 field is required.",
				"The field Reference2 must be a string with a maximum length of 50.",
				"The field Reference3 must be a string with a maximum length of 50.",
				"The field Reference4 must be a string with a maximum length of 50.",
				"The field Reference5 must be a string with a maximum length of 50.",
				"The field MessageTrackingID must be a string with a maximum length of 36."
			}));
		}

		[Test]
		public void TestTransactionWithIncorrectASCIIData()
		{
			var transaction = new BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDE XYZ",
				ClientNumber = "9876543210012345678?",
				ClientStaffCode = "A¦%",
				Category = "T#T",
				PriceItemCode = "DE:",
				Reference1 = "REFERENCE1á456",
				Reference2 = "REFERENCE2!^&¦",
				Reference3 = "REference3Ü!@5",
				ReportingSource = "X*Z",
				ServiceOccuredUTC = DateTime.UtcNow,
				Version = 1,
				MessageTrackingID = "55B2D0DA-8230-43BE-83BF-5C7D5766!EÜ",
			};
			Assert.That(() => BillingTransactionValidator.ValidateTransaction(transaction), Throws.TypeOf<ValidationException>().With.Property("Errors").EqualTo(new[]
			{
				"Field Category has invalid characters. It must contain only ASCII letters, digits, fullstop, hyphen or underscore.",
				"Field PriceItemCode has invalid characters. It must contain only ASCII letters, digits, fullstop, hyphen, hash or underscore.",
				"Field ReportingSource has invalid characters. It must contain only ASCII letters, digits, fullstop, hyphen or underscore.",
				"Field ClientID has invalid characters. It must contain only ASCII letters, digits, fullstop, hyphen, question mark or underscore.",
				"Field ClientNumber has invalid characters. It must contain only ASCII letters, digits, fullstop, hyphen or underscore."
			}));
		}

		[Test]
		public void TestRangeValidation()
		{
			var t1 = GetValidTransaction();
			var t2 = GetValidTransaction();
			var t3 = GetValidTransaction();
			var dtNow = DateTime.Now;
			t1.ServiceOccuredUTC = dtNow.AddYears(-5);
			t2.ServiceOccuredUTC = dtNow.AddYears(-5).AddDays(1);
			t3.ServiceOccuredUTC = dtNow.AddYears(-5).AddDays(2);

			t2.PriceItemCode = null;
			t3.PriceItemCode = null;

			try
			{
				BillingTransactionValidator.ValidateTransactions(new[] { t1, t2, t3 });
				Assert.Fail("Exception must be thrown.");
			}
			catch (ValidationException ex)
			{
				Assert.That(ex.Message, Is.EqualTo("Validation failed for transaction 2 of 3"));
			}
		}

		[Test]
		public void TestTransactionWithInvalidServiceOccuredUTCInTheFuture()
		{
			var transaction = GetValidTransaction();
			transaction.ServiceOccuredUTC = DateTime.Now.AddMonths(1).AddDays(-1);
			try
			{
				BillingTransactionValidator.ValidateTransaction(transaction);
			}
			catch (Exception ex)
			{
				Assert.Fail("Expected no exception, but got: " + ex.Message);
			}

			transaction.ServiceOccuredUTC = DateTime.Now.AddMonths(1);
			Assert.That(() => BillingTransactionValidator.ValidateTransaction(transaction), Throws.TypeOf<ValidationException>().With.Property("Errors").EqualTo(new[]
			{
				"The field ServiceOccuredUTC is required and must be no more than 5 years in the past and no more than one month in the future.",
			}));
		}

		[Test]
		public void TestTransactionWithInvalidServiceOccuredUTCInThePast()
		{
			var transaction = GetValidTransaction();
			transaction.ServiceOccuredUTC = DateTime.Now.AddYears(-5);
			try
			{
				BillingTransactionValidator.ValidateTransaction(transaction);
			}
			catch (Exception ex)
			{
				Assert.Fail("Expected no exception, but got: " + ex.Message);
			}

			transaction.ServiceOccuredUTC = DateTime.Now.AddYears(-5).AddDays(-1);
			Assert.That(() => BillingTransactionValidator.ValidateTransaction(transaction), Throws.TypeOf<ValidationException>().With.Property("Errors").EqualTo(new[]
			{
				"The field ServiceOccuredUTC is required and must be no more than 5 years in the past and no more than one month in the future.",
			}));
		}

		[Test]
		public void TestTransactionWithInvalidServiceOccured()
		{
			var transaction = new BillingTransactionTestClass
			{
				ServiceOccuredUTC = "13/13/2020",
			};
			Assert.That(() => BillingTransactionTestValidator.ValidateTransaction(transaction), Throws.TypeOf<ValidationException>().With.Property("Errors").EqualTo(new[]
			{
				"Invalid DateTime Format, Value is : 13/13/2020"
			}));
		}



		static BillingTransaction GetValidTransaction()
		{
			return new BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDEFXYZ",
				ClientNumber = "98765432100123456789",
				ClientStaffCode = "ABC",
				Category = "TST",
				PriceItemCode = "DEF",
				Reference1 = "REFERENCE 1",
				Reference2 = "REFERENCE 2",
				Reference3 = "REFERENCE 3",
				ReportingSource = "XYZ",
				ServiceOccuredUTC = DateTime.UtcNow,
				Version = 1,
				MessageTrackingID = "55B2D0DA-8230-43BE-83BF-5C7D5766343E",
			};
		}

	}

	public class BillingTransactionTestClass
	{
		[Date(-5, 0, 0, 0, 1, 0, ErrorMessage = "The field ServiceOccuredUTC is required and must be no more than 5 years in the past and no more than one month in the future.")]
		public string ServiceOccuredUTC { get; set; }
	}
}
