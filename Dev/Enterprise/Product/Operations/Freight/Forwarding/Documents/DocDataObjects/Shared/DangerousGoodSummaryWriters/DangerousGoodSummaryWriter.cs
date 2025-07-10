using System;
using System.Text;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public static class DangerousGoodsSummaryWriter
	{
		public static string WriteSummary(this IDangerousGood dangerousGood)
		{
			if (dangerousGood == null)
			{
				return string.Empty;
			}

			var summary = new StringBuilder();

			summary.Append(dangerousGood.DangerousGoodsPrefix());

			switch (dangerousGood.Standard)
			{
				case UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR:
					summary.Append(dangerousGood.WriteCFRSummary());
					break;

				case UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO:
				default:
					summary.Append(dangerousGood.WriteIMOSummary());
					break;
			}

			summary.Append(dangerousGood.DangerousGoodsSuffix());
			summary.Append(dangerousGood.NetExplosiveContentDescription());

			return summary.ToString();
		}

		static string DangerousGoodsPrefix(this IDangerousGood dangerousGood)
		{
			var packages = dangerousGood.QuantityAndPackageDescription();
			return packages;
		}

		static string DangerousGoodsSuffix(this IDangerousGood dangerousGood)
		{
			var contacts = dangerousGood.ContactDescription();
			if (!string.IsNullOrEmpty(contacts))
			{
				return ", " + contacts;
			}

			return string.Empty;
		}

		static string QuantityAndPackageDescription(this IDangerousGood dangerousGood) =>
			(dangerousGood.PackageType?.Code.IsEmpty ?? true)
				? string.Empty
				: FormattableString.Invariant($"{dangerousGood.Quantity} {dangerousGood.PackageType.Description} of "); // Fixed format text for document

		internal static string UNNumber(this IDangerousGood dangerousGood) =>
			FormattableString.Invariant($"{dangerousGood.UNNumberPrefx()}{dangerousGood.Unno}"); // Fixed format text for document

		static string UNNumberPrefx(this IDangerousGood dangerousGood) => dangerousGood.Unno == "8000" // consumer commodity i.e. undefined or other
			? "ID"
			: "UN";

		internal static string PackingGroupDescription(this IDangerousGood dangerousGood) =>
			string.IsNullOrWhiteSpace(dangerousGood.PackingGroup)
				? string.Empty
				: FormattableString.Invariant($"PG {dangerousGood.PackingGroup}"); // Fixed format text for document

		internal static string FlashPointDescription(this IDangerousGood dangerousGood)
		{
			var flashPointValue = Utilities.Round(dangerousGood.FlashPoint?.Value ?? 0, 1);

			if (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.Value)
			{
				return dangerousGood.FlashPoint != null
				? FormattableString.Invariant($"({flashPointValue}C c.c.)")
				: string.Empty;
			}
			else
			{
				return flashPointValue == 0
				? string.Empty
				: FormattableString.Invariant($"({flashPointValue}C c.c.)"); // Fixed format text for document
			}
		}

		static string ContactDescription(this IDangerousGood dangerousGood)
		{
			return string.IsNullOrWhiteSpace(dangerousGood.Contact?.FullName + dangerousGood.Contact?.Phone)
				? string.Empty
				: FormattableString.Invariant($"contact {dangerousGood.Contact.FullName} {dangerousGood.Contact.Phone}").TrimEnd(); // Fixed format text for document
		}

		public static string LimitedQuantityDescription(this IDangerousGood dangerousGood)
		{
			if (dangerousGood == null)
			{
				return string.Empty;
			}

			return dangerousGood.PackedInLimitedQuantity ? (NoResString)"LTD QTY" : string.Empty; // not translatable
		}

		public static string NetExplosiveContentDescription(this IDangerousGood dangerousGood)
		{
			if (dangerousGood == null)
			{
				return string.Empty;
			}

			if ((dangerousGood.NetExplosiveWeight == null) || (dangerousGood.NetExplosiveWeight.Value == 0))
			{
				return string.Empty;
			}

			return FormattableString.Invariant($"\nNEC: {dangerousGood.NetExplosiveWeight.Value} {dangerousGood.NetExplosiveWeight.Unit.Code}");
		}
	}
}
