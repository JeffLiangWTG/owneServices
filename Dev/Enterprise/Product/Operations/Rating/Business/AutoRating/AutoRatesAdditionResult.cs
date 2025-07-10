using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class ChargeWrapper
	{
		public ChargeWrapper(IChargeWithChargeCode charge, AutoRateInfo info)
		{
			Charge = Argument.NotNull(charge, nameof(charge));
			Info = info;
		}

		public IChargeWithChargeCode Charge;
		public string ChargeCode => string.IsNullOrWhiteSpace(Charge.AC_Code) ? "null" : Charge.AC_Code;

		public AutoRateInfo Info { get; }
		public bool IsCreatedFromWiseCost => Info?.IsFromRatesService ?? false;

		public static LambdaComparer<ChargeWrapper> Comparer
		{
			get { return new LambdaComparer<ChargeWrapper>((x, y) => x.Charge.PK.Equals(y.Charge.PK), x => x.Charge.PK.GetHashCode()); }
		}

		public void RecordPercentageApplied(IRatingContext context)
		{
			if (Info == null)
			{
				throw new InvalidOperationException("This should only be called on created/modified charges having Info, not on deleted ones");
			}

			if (Info.PercentageLinesApplied.Any())
			{
				var set = context.PercentageLinesApplied.GetOrAdd(Charge.PK);
				set.UnionWith(Info.PercentageLinesApplied);
			}
		}
	}

	public class AutoRatesAdditionResult
	{
		public AutoRatesAdditionResult(List<ChargeWrapper> createdCharges, List<ChargeWrapper> modifiedCharges, int deletedChargesCount, List<AutoRateInfo> autoRates, CostSell costOrSell)
		{
			AutoRates = new ReadOnlyCollection<AutoRateInfo>(autoRates);
			this.createdCharges = createdCharges.Distinct(ChargeWrapper.Comparer).ToArray();
			this.modifiedCharges = modifiedCharges.Distinct(ChargeWrapper.Comparer).ToArray();
			DeletedChargesCount = deletedChargesCount;
			this.costOrSell = costOrSell;
		}

		public ReadOnlyCollection<AutoRateInfo> AutoRates { get; }

		public IEnumerable<ChargeWrapper> CreatedCharges => createdCharges.Where(x => !x.Charge.IsDeleted);
		readonly IEnumerable<ChargeWrapper> createdCharges;

		public IEnumerable<ChargeWrapper> ModifiedCharges => modifiedCharges.Where(x => !x.Charge.IsDeleted);
		readonly IEnumerable<ChargeWrapper> modifiedCharges;

		public int DeletedChargesCount { get; set; }
		readonly CostSell costOrSell;
		public string ErrorMessage { get; }

		public string GetLog()
		{
			var builder = new ZStringBuilder();

			var type = costOrSell == CostSell.Cost
				? Res.GetString("7de7cedc-c085-42e8-bbab-72fb727530d6", "costs")
				: Res.GetString("aa4c4483-6802-4a5e-86d5-c0f3a029fd71", "rates");

			if (AutoRates.Count > 0)
			{
				builder.AppendLine(Res.GetString("86a383de-95b9-411c-9274-9be867952ddc", "The following {0} were found:", type));
				builder.AppendLine(GetSourcesLogString(AutoRates));
			}
			else
			{
				builder.AppendLine(Res.GetString("2f80c4c9-0d10-4289-93d2-511352ea2da1", "No {0} were found.", type));
			}

			var createdChargesCount = CreatedCharges.Count();
			var modifiedChargesCount = ModifiedCharges.Count();

			if (createdChargesCount > 0)
			{
				builder.AppendLine(Res.GetString("5d488d7b-8307-4951-9341-41b2d1f0ead2", "Charges created: {0}", GetChargeCodesLogString(CreatedCharges)));
			}

			if (modifiedChargesCount > 0)
			{
				builder.AppendLine(Res.GetString("3d5fc3cc-2771-4e02-8697-d1e4ae4c41cb", "Charges modified: {0}", GetChargeCodesLogString(ModifiedCharges)));
			}

			if (DeletedChargesCount > 0)
			{
				builder.AppendLine(Res.GetString("c04089a0-2320-43fe-8fc3-55b9db520bb2", "Charges deleted: {0} Deleted Charges", DeletedChargesCount));
			}

			if (AutoRates.Count > 0 && createdChargesCount == 0 && modifiedChargesCount == 0)
			{
				builder.AppendLine(Res.GetString("ec983a7e-624b-493f-be99-2c97405d0dff", "No {0} were changed or created.", type));
			}

			return builder.ToString();
		}

		static string GetSourcesLogString(IEnumerable<AutoRateInfo> rateInfos)
		{
			var builder = new ZStringBuilder();

			var infos = rateInfos.Where(x => !x.IsInclusiveCalculator).Where(x => x.ChargeCode != null).OrderBy(x => x.ChargeCode.AC_Code);
			foreach (var info in infos)
			{
				if (string.IsNullOrEmpty(info.RateSource))
				{
					builder.Append("  " + Res.GetString("633d91db-402c-4b3e-ab7d-a64f87a124ad", "{0} {1} charge", BulletPoint, info.ChargeCode.AC_Code));
				}
				else
				{
					builder.Append("  " + Res.GetString("3859254a-384a-4b80-a2d2-636b28dc5524", "{0} {1} charge from {2}", BulletPoint, info.ChargeCode.AC_Code, info.RateSource));
				}
			}

			var sourcesLogString = builder.ToStringWithNewLineBetweenAppends();
			var sourcesLogStringWithDuplicates = LoggerDecorator.GroupDuplicateLines(sourcesLogString).ToStringWithNewLineBetweenStrings();

			return sourcesLogStringWithDuplicates;
		}

		static string GetChargeCodesLogString(IEnumerable<ChargeWrapper> charges)
		{
			var chargeCodes = charges
				.Where(x => x?.Charge != null && !string.IsNullOrWhiteSpace(x.ChargeCode))
				.Select(c => c.ChargeCode)
				.OrderBy(x => x);
			var groupedChargeCodeSummary = LoggerDecorator.GroupDuplicateLines(string.Join(System.Environment.NewLine, chargeCodes));
			var result = new ZStringBuilder(groupedChargeCodeSummary).ToStringWithDelimiterBetweenAppends(", ");

			return result;
		}

		public static ZString GetReferenceForCAREvent(GlbCompany company)
		{
			var companyCode = company != null ? company.GC_Code : ZString.Empty;
			return (NoResString)"Company: " + companyCode;// this is a system word used for filtering logs
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Non translateable text, it's the bulletpoint character")]
		const string BulletPoint = "\u2022";
	}
}

