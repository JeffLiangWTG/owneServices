using System;
using System.Linq;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class InvoiceTypeCalculationProviderTest : TestCase
	{
		public void TestBillInLocalCurrency()
		{
			var supportedInvoiceTypes = new string[] {
				InvoiceTypesList.Codes.DoNotPost,

				InvoiceTypesList.Codes.FinalInvoice,
				InvoiceTypesList.Codes.DisbursementInvoice,
				InvoiceTypesList.Codes.InvoicePerTaxCode,
				InvoiceTypesList.Codes.DestinationChargesInvoice,
				AgencyInvoiceTypesList.Codes.LocalCollect,
				AgencyInvoiceTypesList.Codes.LocalPrePaid,
				AgencyInvoiceTypesList.Codes.Misc,

				InvoiceTypesList.Codes.FinalInvoice_Batching,
				InvoiceTypesList.Codes.DisbursementInvoice_Batching,
				InvoiceTypesList.Codes.InvoicePerTaxCode_Batching,
				InvoiceTypesList.Codes.DestinationChargesInvoice_Batching,
				AgencyInvoiceTypesList.Codes.LocalCollect_Batching,
				AgencyInvoiceTypesList.Codes.LocalPrePaid_Batching,
				AgencyInvoiceTypesList.Codes.Misc_Batching
			};

			foreach (var invoiceType in new InvoiceTypesList().GetAllCodes().Union(new AgencyInvoiceTypesList().GetAllCodes()))
			{
				AssertEquals($"Invoice Type {invoiceType} BillInLocalCurrency", supportedInvoiceTypes.Contains(invoiceType), InvoiceTypeCalculationProvider.BillInLocalCurrency(invoiceType));
			}
		}

		#region Disbursement Invoice Types

		public void TestDisbursementInvoiceTypes()
		{
			AssertNotNull(InvoiceTypeCalculationProvider.DisbursementInvoiceTypes);
			// Assert it si simply wrapping property of AccTransactionHeader
			AssertEquals("Length", AccTransactionHeader.DisbursementInvoiceTypes.Length, InvoiceTypeCalculationProvider.DisbursementInvoiceTypes.Length);

			foreach (string invoiceType in AccTransactionHeader.DisbursementInvoiceTypes)
			{
				Assert("Contains " + invoiceType, InvoiceTypeCalculationProvider.DisbursementInvoiceTypes.Contains(invoiceType));
			}
		}

		public void TestIsDisbursementInvoiceType()
		{
			// Assert it is just a wrapper of AccTransactionHeader.IsDisbursementInvoiceType
			AssertEquals("Null", AccTransactionHeader.IsDisbursementInvoiceType(null), AccTransactionHeader.IsDisbursementInvoiceType(null));
			AssertEquals("Empty string", AccTransactionHeader.IsDisbursementInvoiceType(string.Empty), AccTransactionHeader.IsDisbursementInvoiceType(string.Empty));
			AssertEquals("Random string", AccTransactionHeader.IsDisbursementInvoiceType("!@#"), AccTransactionHeader.IsDisbursementInvoiceType("!@#"));

			foreach (CodeDescriptionPair invoiceType in new InvoiceTypesList())
			{
				AssertEquals(string.Format("Invoice Type '{0}' gives the same result", invoiceType.Code),
					AccTransactionHeader.IsDisbursementInvoiceType(invoiceType.Code), InvoiceTypeCalculationProvider.IsDisbursementInvoiceType(invoiceType.Code));
			}
		}

		#endregion

		#region Disbursement Non Deferred Invoice Types

		public void TestDisbursementNonDeferredInvoiceTypes()
		{
			AssertNotNull(InvoiceTypeCalculationProvider.DisbursementNonDeferredInvoiceTypes);

			foreach (string invoiceType in InvoiceTypeCalculationProvider.DisbursementNonDeferredInvoiceTypes)
			{
				Assert("DisbursementInvoiceTypes contains " + invoiceType, AccTransactionHeader.DisbursementInvoiceTypes.Contains(invoiceType));
				Assert("NonDeferredInvoiceTypes contains " + invoiceType, InvoiceTypeCalculationProvider.NonDeferredInvoiceTypes.Contains(invoiceType));
			}
		}

		#endregion

		#region Deferred Charge Tests

		public void TestDeferredInvoiceTypes()
		{
			int invoiceTypeLength = 0;
			CodeDescriptionPairList combinedList = new InvoiceTypesList();
			combinedList.AddRange(new AgencyInvoiceTypesList());

			foreach (CodeDescriptionPair invoiceType in combinedList)
			{
				if (invoiceType.Code[invoiceType.Code.Length - 1] == 'D' && invoiceType.Code != InvoiceTypesList.Codes.DoNotPost)
				{
					invoiceTypeLength++;
					Assert(invoiceType.Code + " should be in DeferredInvoiceTypes.", Array.IndexOf(InvoiceTypeCalculationProvider.DeferredInvoiceTypes, invoiceType.Code) > -1);
					Assert(invoiceType.Code + " should not be in NonDeferredInvoiceTypes.", Array.IndexOf(InvoiceTypeCalculationProvider.NonDeferredInvoiceTypes, invoiceType.Code) == -1);
				}
				else
				{
					Assert(invoiceType.Code + " should not be in DeferredInvoiceTypes.", Array.IndexOf(InvoiceTypeCalculationProvider.DeferredInvoiceTypes, invoiceType.Code) == -1);
				}
			}
			AssertEquals(invoiceTypeLength, InvoiceTypeCalculationProvider.DeferredInvoiceTypes.Length);
		}

		public void TestNonDeferredInvoiceTypes()
		{
			int invoiceTypeLength = 0;
			CodeDescriptionPairList combinedList = new InvoiceTypesList();
			combinedList.AddRange(new AgencyInvoiceTypesList());

			foreach (CodeDescriptionPair invoiceType in combinedList)
			{
				if (invoiceType.Code[invoiceType.Code.Length - 1] != 'D' && invoiceType.Code != InvoiceTypesList.Codes.DoNotPost)
				{
					invoiceTypeLength++;
					Assert(invoiceType.Code + " should be in NonDeferredInvoiceTypes.", Array.IndexOf(InvoiceTypeCalculationProvider.NonDeferredInvoiceTypes, invoiceType.Code) > -1);
					Assert(invoiceType.Code + " should not be in DeferredInvoiceTypes.", Array.IndexOf(InvoiceTypeCalculationProvider.DeferredInvoiceTypes, invoiceType.Code) == -1);
				}
				else
				{
					Assert(invoiceType.Code + " should not be in NonDeferredInvoiceTypes.", Array.IndexOf(InvoiceTypeCalculationProvider.NonDeferredInvoiceTypes, invoiceType.Code) == -1);
				}
			}
			AssertEquals(invoiceTypeLength, InvoiceTypeCalculationProvider.NonDeferredInvoiceTypes.Length);
		}

		public void TestConvertNonDeferredInvoiceTypeToDeferredOne()
		{
			foreach (CodeDescriptionPair invoiceType in new InvoiceTypesList())
			{
				if (Array.IndexOf(InvoiceTypeCalculationProvider.NonDeferredInvoiceTypes, invoiceType.Code) > -1)
				{
					string convertedInvoiceType = InvoiceTypeCalculationProvider.ConvertNonDeferredInvoiceTypeToDeferredOne(invoiceType.Code);
					Assert(invoiceType.Code + " should be in DeferredInvoiceTypes.", Array.IndexOf(InvoiceTypeCalculationProvider.DeferredInvoiceTypes, convertedInvoiceType) > -1);
				}
				else
				{
					AssertEquals("Invoice type not in NonDeferredTypes should not be converted.", invoiceType.Code, InvoiceTypeCalculationProvider.ConvertNonDeferredInvoiceTypeToDeferredOne(invoiceType.Code));
				}
			}
		}

		public void TestConvertDeferredInvoiceTypeToNonDeferredOne()
		{
			foreach (CodeDescriptionPair invoiceType in new InvoiceTypesList())
			{
				if (Array.IndexOf(InvoiceTypeCalculationProvider.DeferredInvoiceTypes, invoiceType.Code) > -1)
				{
					string convertedInvoiceType = InvoiceTypeCalculationProvider.ConvertDeferredInvoiceTypeToNonDeferredOne(invoiceType.Code);
					Assert(invoiceType.Code + " should not be in NonDeferredInvoiceTypes.", Array.IndexOf(InvoiceTypeCalculationProvider.NonDeferredInvoiceTypes, convertedInvoiceType) > -1);
				}
				else
				{
					AssertEquals("Invoice type not in NonDeferredTypes should not be converted.", invoiceType.Code, InvoiceTypeCalculationProvider.ConvertDeferredInvoiceTypeToNonDeferredOne(invoiceType.Code));
				}
			}
		}

		public void TestConvertSelfBillingToDeferredAndBackAgain()
		{
			AssertEquals("Self Billing - Non-Deferred to Deferred", InvoiceTypesList.Codes.SelfBillingInvoice_Batching, InvoiceTypeCalculationProvider.ConvertNonDeferredInvoiceTypeToDeferredOne(InvoiceTypesList.Codes.SelfBillingInvoice));
			AssertEquals("Self Billing - Deferred to Non-Deferred", InvoiceTypesList.Codes.SelfBillingInvoice, InvoiceTypeCalculationProvider.ConvertDeferredInvoiceTypeToNonDeferredOne(InvoiceTypesList.Codes.SelfBillingInvoice_Batching));
		}

		#endregion
	}
}
