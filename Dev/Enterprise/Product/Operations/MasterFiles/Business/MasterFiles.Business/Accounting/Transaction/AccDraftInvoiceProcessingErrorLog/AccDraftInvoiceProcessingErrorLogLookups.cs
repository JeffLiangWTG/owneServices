using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccDraftInvoiceProcessingErrorLogLookups : AutoAccDraftInvoiceProcessingErrorLogLookups
	{
		public AccDraftInvoiceProcessingErrorLogLookups(AutoAccDraftInvoiceProcessingErrorLog parent) : base(parent)
		{
		}

		public ReadOnlyCodeDescriptionPairList ErrorCodeList => GetErrorCodes();

		CodeDescriptionPairList GetErrorCodes()
		{
			var dicErrorCodes = new Dictionary<string, string> {
				{ AccDraftInvoiceProcessingErrorCodes.CreditorNotProvided
					, Res.GetString("6A1C10A6-3CC5-40B4-AD7E-E6370F3CA1C2", "Creditor not provided") }
				, { AccDraftInvoiceProcessingErrorCodes.TransactionAmountNotProvided
					, Res.GetString("7A5F4D50-8DA8-42E9-9CFF-016568114091", "Transaction amount not provided") }
				, { AccDraftInvoiceProcessingErrorCodes.NoJobsFound
					, Res.GetString("7FB1C13E-377C-4FC9-A71C-01FBDA9E46D3", "No jobs found") }
				, { AccDraftInvoiceProcessingErrorCodes.NoReferencesProvided
					, Res.GetString("F44CFDF5-54BC-47EF-B15C-132AD8764544", "No references provided") }
				, { AccDraftInvoiceProcessingErrorCodes.TransactionNumberNotProvided
					, Res.GetString("577C0706-0038-42B3-B0FD-0D5CF09071B8", "Transaction number not provided") }
				, { AccDraftInvoiceProcessingErrorCodes.CurrencyNotProvided
					, Res.GetString("30D35E12-AD8F-4C61-8C19-A7A6A263B46C", "Currency not provided") }
				, { AccDraftInvoiceProcessingErrorCodes.NoAccrualsFound
					, Res.GetString("E31878D6-D7CA-4CC2-8BFC-3785D95B5F0A", "No accruals found") }
				, { AccDraftInvoiceProcessingErrorCodes.NoMatchingAccrualFfound
					, Res.GetString("84B094DD-5BBA-4030-AACC-F3AF064E16F8", "No matching accruals found") }
				, { AccDraftInvoiceProcessingErrorCodes.MultipleMatchingAccrualsFound
					, Res.GetString("725845FD-52AF-4AA3-956C-CE260C794F3A", "Multiple matching accruals found") }
				, { AccDraftInvoiceProcessingErrorCodes.SubtotalsDoNotMatchInvoiceTotal
					, Res.GetString("080A1357-F3FA-4DF1-A375-CD23F6D6A3EF", "Subtotals do not match invoice total") }
				, { AccDraftInvoiceProcessingErrorCodes.LineAmountsDoNotMatchInvoiceTotal
					, Res.GetString("90A92DBB-D325-4DE4-9D3C-6CA4D6FA1531", "Line amounts do not match invoice total") }
				, { AccDraftInvoiceProcessingErrorCodes.UnableToPost
					, Res.GetString("283F1271-35AE-4ADA-B65E-CAA65D72E609", "Unable to post, please review") }
				, { AccDraftInvoiceProcessingErrorCodes.SystemExceptionOccurred
					, Res.GetString("EC65ED82-17FA-4A18-9FA9-813AB9B02772", "System exception occurred") }
				, { AccDraftInvoiceProcessingErrorCodes.CriticalValidationError
					, Res.GetString("0F0564BF-5051-4C55-9B6A-77AD8EDCCF31", "Critical validation error") }
				, { AccDraftInvoiceProcessingErrorCodes.UnsupportedFileTypeForParsing
					, Res.GetString("6E71BDC7-11B2-467A-9425-F19B30AAC729", "Unsupported file type for parsing") }
				, { AccDraftInvoiceProcessingErrorCodes.DuplicateInvoiceNumber
					, Res.GetString("04D1B88C-8442-4DDD-A301-384B802D44B2", "Duplicate invoice number") }
				, { AccDraftInvoiceProcessingErrorCodes.UnexpectedDueDate
					, Res.GetString("A3EC4E6D-7095-4D0D-8029-C947E7CBFF0C", "Unexpected due date") }
			};

			var result = new CodeDescriptionPairList();

			foreach (var kv in dicErrorCodes)
			{
				result.AddPair(kv.Key, kv.Value);
			}

			return result;
		}
	}
}
