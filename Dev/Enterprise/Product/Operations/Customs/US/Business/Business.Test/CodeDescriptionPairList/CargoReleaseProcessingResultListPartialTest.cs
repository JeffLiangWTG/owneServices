using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CargoReleaseProcessingResultListTest : TestCase
	{
		public void TestProperties()
		{
			AssertEquals(true, CargoReleaseProcessingResultList.AIIRecordsMayBeRequired(CargoReleaseProcessingResultList.Codes.CondReleaseSpecDocReview));
			AssertEquals(true, CargoReleaseProcessingResultList.AIIRecordsMayBeRequired(CargoReleaseProcessingResultList.Codes.FurtherDocReviewRequired));
			AssertEquals(true, CargoReleaseProcessingResultList.AIIRecordsMayBeRequired(CargoReleaseProcessingResultList.Codes.ReleaseRemovedFurtherDocReviewRequired));
			AssertEquals(false, CargoReleaseProcessingResultList.AIIRecordsMayBeRequired(CargoReleaseProcessingResultList.Codes.AgricultureManifestHoldRemoved));
			AssertEquals(false, CargoReleaseProcessingResultList.AIIRecordsMayBeRequired(CargoReleaseProcessingResultList.Codes.EntryCancellationUnset));
			AssertEquals(false, CargoReleaseProcessingResultList.AIIRecordsMayBeRequired(CargoReleaseProcessingResultList.Codes.ElectronicInvoiceRequired));
			AssertEquals(false, CargoReleaseProcessingResultList.AIIRecordsMayBeRequired(CargoReleaseProcessingResultList.Codes.EntryCancelled));
			AssertEquals(false, CargoReleaseProcessingResultList.AIIRecordsMayBeRequired(CargoReleaseProcessingResultList.Codes.EntryDeletedByCBP));

			AssertEquals(true, CargoReleaseProcessingResultList.IsEntryDeletionDispositionCode(CargoReleaseProcessingResultList.Codes.EntryDeletedByCBP));
			AssertEquals(true, CargoReleaseProcessingResultList.IsEntryDeletionDispositionCode(CargoReleaseProcessingResultList.Codes.EntryCancelled));
			AssertEquals(true, CargoReleaseProcessingResultList.IsEntryDeletionDispositionCode(CargoReleaseProcessingResultList.Codes.CancellationRequestRejected));

			AssertEquals(false, CargoReleaseProcessingResultList.IsDocRequiredDispositionCode(CargoReleaseProcessingResultList.Codes.PendingIntenstiveReview));
			AssertEquals(true, CargoReleaseProcessingResultList.IsDocRequiredDispositionCode(CargoReleaseProcessingResultList.Codes.DocRequiredForCorrectionRequest));
			AssertEquals(true, CargoReleaseProcessingResultList.IsDocRequiredDispositionCode(CargoReleaseProcessingResultList.Codes.DocRequiredForCancellationRequest));
			AssertEquals(true, CargoReleaseProcessingResultList.IsDocRequiredDispositionCode(CargoReleaseProcessingResultList.Codes.DocumentRequired));

			AssertEquals(true, CargoReleaseProcessingResultList.IsReleased(CargoReleaseProcessingResultList.Codes.Released));
			AssertEquals(true, CargoReleaseProcessingResultList.IsReleased(CargoReleaseProcessingResultList.Codes.ReleaseDateUpdate));

			AssertEquals(true, CargoReleaseProcessingResultList.IsCancelled(CargoReleaseProcessingResultList.Codes.EntryCancelled));

			AssertEquals(true, CargoReleaseProcessingResultList.IsNotReleasedCancellationPending(CargoReleaseProcessingResultList.Codes.EntryCancelled));
			AssertEquals(true, CargoReleaseProcessingResultList.IsNotReleasedCancellationPending(CargoReleaseProcessingResultList.Codes.EntryCancellationUnset));
			AssertEquals(true, CargoReleaseProcessingResultList.IsNotReleasedCancellationPending(CargoReleaseProcessingResultList.Codes.EntryWillBeCancelledIn7Days));

			AssertEquals(true, CargoReleaseProcessingResultList.IsDeleted(CargoReleaseProcessingResultList.Codes.EntryDeletedByCBP));

			AssertEquals(true, CargoReleaseProcessingResultList.IsExam(CargoReleaseProcessingResultList.Codes.PendingIntenstiveReview));
			AssertEquals(true, CargoReleaseProcessingResultList.IsExam(CargoReleaseProcessingResultList.Codes.OverrideToIntensive));

			AssertEquals(true, CargoReleaseProcessingResultList.IsHold(CargoReleaseProcessingResultList.Codes.ManifestHoldCBP));
			AssertEquals(true, CargoReleaseProcessingResultList.IsHold(CargoReleaseProcessingResultList.Codes.ManifestHoldAgriculture));
			AssertEquals(true, CargoReleaseProcessingResultList.IsHold(CargoReleaseProcessingResultList.Codes.CBPHold));

			AssertEquals(true, CargoReleaseProcessingResultList.IsHoldRemoved(CargoReleaseProcessingResultList.Codes.ManifestHoldCBP, CargoReleaseProcessingResultList.Codes.CBPManifestHoldRemoved));
			AssertEquals(true, CargoReleaseProcessingResultList.IsHoldRemoved(CargoReleaseProcessingResultList.Codes.ManifestHoldCBP, CargoReleaseProcessingResultList.Codes.AgricultureManifestHoldRemoved));
			AssertEquals(true, CargoReleaseProcessingResultList.IsHoldRemoved(CargoReleaseProcessingResultList.Codes.ManifestHoldCBP, CargoReleaseProcessingResultList.Codes.CBPHoldRemoved));
			AssertEquals(true, CargoReleaseProcessingResultList.IsHoldRemoved(CargoReleaseProcessingResultList.Codes.ManifestHoldAgriculture, CargoReleaseProcessingResultList.Codes.CBPManifestHoldRemoved));
			AssertEquals(true, CargoReleaseProcessingResultList.IsHoldRemoved(CargoReleaseProcessingResultList.Codes.ManifestHoldAgriculture, CargoReleaseProcessingResultList.Codes.AgricultureManifestHoldRemoved));
			AssertEquals(true, CargoReleaseProcessingResultList.IsHoldRemoved(CargoReleaseProcessingResultList.Codes.ManifestHoldAgriculture, CargoReleaseProcessingResultList.Codes.CBPHoldRemoved));
			AssertEquals(true, CargoReleaseProcessingResultList.IsHoldRemoved(CargoReleaseProcessingResultList.Codes.CBPHold, CargoReleaseProcessingResultList.Codes.CBPManifestHoldRemoved));
			AssertEquals(true, CargoReleaseProcessingResultList.IsHoldRemoved(CargoReleaseProcessingResultList.Codes.CBPHold, CargoReleaseProcessingResultList.Codes.AgricultureManifestHoldRemoved));
			AssertEquals(true, CargoReleaseProcessingResultList.IsHoldRemoved(CargoReleaseProcessingResultList.Codes.CBPHold, CargoReleaseProcessingResultList.Codes.CBPHoldRemoved));
		}

		public void TestGetListForQuotaStatus()
		{
			var list = CargoReleaseProcessingResultList.GetListForQuotaStatus(new BusinessObjectFactory());
			Assert(list.ContainsCode(CargoReleaseProcessingResultList.Codes.QuotaPending));
			Assert(list.ContainsCode(CargoReleaseProcessingResultList.Codes.QuotaRejected));
			Assert(list.ContainsCode(CargoReleaseProcessingResultList.Codes.QuotaReserved));
			Assert(list.ContainsCode(CargoReleaseProcessingResultList.Codes.QuotaAccepted));
		}

		public void TestDispositionCodeForSO50Block()
		{
			var dispositionCodesNotRelatedToSO50 = new string[]
			{
				"97", "76", "87", "01", "02", "86", "31", "85", "84", "96", "17", "24", "23", "21", "06", "79", "88", "89", "25", "F0", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8",
				"F9", "FA", "FB", "FC", "FD", "FE", "FF", "FG", "FH", "FI", "FJ", "FK", "FL", "FM", "FN", "FO", "FP", "FQ", "FR", "FS", "FT", "FU", "13", "11", "FV", "FX", "FY", "FZ",
				"G0", "G1", "G2", "G3", "G4", "G5", "G6", "G7", "G8", "G9", "GA", "GB", "GC", "GD", "GE", "GF", "GG", "GH", "GI", "GJ", "GK", "GL", "GM", "GN", "GO", "GP", "GQ", "GR",
				"GS", "GT", "GU", "GV", "GX", "GY", "GZ", "H0", "H1", "H2", "H3", "H4", "H5", "H6", "H7", "H8", "H9", "HA", "HB", "HC", "HD", "HE", "HF", "HG", "HH", "HI", "HJ", "HK",
				"HL", "HM", "HN", "HO", "HP", "HQ", "HR", "HS", "HT", "HU", "HV", "HX", "HY", "HZ", "I0", "I1", "I2", "I3", "I4", "I5", "I6", "I7", "I8", "I9", "IA", "IB", "IC", "ID",
				"IE", "IF", "IG", "IH", "II", "IJ", "IK", "IL", "IM", "IN", "80", "81", "75", "04", "IO", "IP", "IQ", "IR", "IS", "IT", "IU", "IV", "IX", "IY", "IZ", "J0", "J1", "J2",
				"J3", "J4", "J5", "J6", "J7", "J8", "J9", "JA", "JB", "JC", "JD", "JE", "JF", "JG", "JH", "JI", "JJ", "JK", "JL", "JM", "JN", "JO", "JP", "JQ", "JR", "JS", "JT", "JU",
				"JV", "JX", "JY", "JZ", "K0", "K1", "K2", "K3", "K4", "K5", "K6", "K7", "K8", "K9", "KA", "KB", "KC", "KD", "KE", "KF", "KG", "KH", "KI", "KJ", "KK", "KL", "KM", "KN",
				"KO", "KP", "KQ", "KR", "KS", "KT", "KU", "KV", "KX", "KY", "KZ", "L0", "L1", "L2", "L3", "L4", "L5", "L6", "L7", "L8", "L9", "LA", "LB", "LC", "LD", "LE", "LF", "LG",
				"LH", "LI", "LJ", "LK", "LL", "LM", "LN", "LO", "LP", "LQ", "LR", "LS", "LT", "LU", "LV", "LX", "LY", "LZ", "M0", "M1", "M2", "M3", "M4", "M5", "M6", "M7", "M8", "M9",
				"MA", "MB", "MC", "MD", "ME", "MF", "MG", "MH", "MI", "MJ", "MK", "ML", "MM", "MN", "MO", "MP", "MQ", "MR", "MS", "MT", "MU", "MV", "MX", "MY", "MZ", "N1", "N2", "N3",
				"N4", "35", "08", "07", "05", "16", "03", "73", "70", "71", "72", "98", "22", "12", "99", "82", "83", "90", "26", "28", "29" };

			var dispositionCodesRelatedToSO50 = new string[]
			{
				"91", "92", "93", "94", "95", "51", "52", "53", "54", "55", "56", "57", "58", "59", "61", "62", "63", "74"
			};

			var processingResultList = new CargoReleaseProcessingResultList();
			var processingResultCodesList = processingResultList.GetAllCodes();
			var newCodesAddedIntoList = processingResultCodesList.Except(dispositionCodesNotRelatedToSO50).Except(dispositionCodesRelatedToSO50);
			if (newCodesAddedIntoList.Any())
			{
				Assert(string.Format("New code(s) '{0}' added into the list. If the new code is related to SO50 block, please also add the code and description into dbo.csfn_GetLatestCargoReleaseBillStatusAsStringInline function.", string.Join("','", newCodesAddedIntoList)), false);
			}
			else
			{
				Assert("No new codes added.", true);
			}
		}

		public void TestDispositionCodesForReleaseStatus()
		{
			var dispositionCodesRelatedToReleaseStatus = new string[] { "03", "07", "21", "22", "23", "24", "25", "29", "51", "52", "53", "84", "85", "90", "96", "97", "98", "99" };
			var distinctDispositions = CargoReleaseProcessingResultList.GetDispositionCodesRelatedToReleaseStatus().Distinct().OrderBy(x => x);
			AssertEquals(string.Join(", ", dispositionCodesRelatedToReleaseStatus), string.Join(", ", distinctDispositions));
		}

		public void Test04And26And28()
		{
			var codeList = new CargoReleaseProcessingResultList();
			var desc = codeList.GetDescriptionFromCode("04");
			AssertEquals("Entry Detained", desc);
			desc = codeList.GetDescriptionFromCode("26");
			AssertEquals("No Bill Match After 30 Days", desc);
			desc = codeList.GetDescriptionFromCode("28");
			AssertEquals("No Bill Match After 60 Days", desc);
		}
	}
}
