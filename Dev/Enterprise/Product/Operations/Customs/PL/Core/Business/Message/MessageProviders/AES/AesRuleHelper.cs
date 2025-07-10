using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public static class AesRuleHelper
{
	public static bool CheckRuleR0089E(CusEntryInstruction cusEntryInstruction) =>
		cusEntryInstruction is CusEntryInstruction && cusEntryInstruction.CEI_Procedure == Constants.ProcedureCodes._76;

	public static bool HasSubStyleEqualsXorYorZ(this CusEntryInstruction cusEntryInstruction)
	{
		var subStyle = cusEntryInstruction.CEI_SubStyle;
		return subStyle == Constants.SubStyleCodes.X
				|| subStyle == Constants.SubStyleCodes.Y
				|| subStyle == Constants.SubStyleCodes.Z;
	}

	public static bool HasSubStyleEqualsBorCorEorF(this CusEntryInstruction cusEntryInstruction)
	{
		var subStyle = cusEntryInstruction.CEI_SubStyle;
		return subStyle == EntrySubStyleList.Codes.IncompleteDeclaration
				|| subStyle == EntrySubStyleList.Codes.SimplifiedDeclaration
				|| subStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB
				|| subStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
	}

	public static IEnumerable<AdditionalInfo> GetAdditionalInfosBySubtypeAndDistinctByCode(this CusEntryInstruction entryInstruction, string subtype, ZString[] excludeCodes = null)
	{
		excludeCodes ??= Array.Empty<ZString>();
		var additionalInfos = entryInstruction.AdditionalInfos
			.Cast<AdditionalInfo>()
			.Where(x => x.CSI_SubType == subtype);
		return (subtype == AdditionalInfoKindList.Codes.INF)
			? additionalInfos.DistinctSpecificDocuments(excludeCodes)
			: additionalInfos;
	}

	public static T ApplyC0050Rule<T>(string identificationNumber, Func<T> valueProvider) => string.IsNullOrWhiteSpace(identificationNumber) ? valueProvider() : default;

	public static string ApplyE1104Rule(string value, bool isAesTransitionPeriod) => isAesTransitionPeriod ? value.LeftOrNull(35) : value;

	public static class RuleR0093E
	{
		public enum Patterns
		{
			n1an2,
			n1an3,
			a1an4
		}

		public static int GetKeyEuLessThanPl(ZString code, Patterns pattern) =>
			GetRegexByPattern(pattern) is Regex regex && regex.IsMatch(code) ? 1 : 0;

		static readonly Regex n1an2RegEx = new Regex("^[\\d]{1}[\\w]{2}$", RegexOptions.Compiled);
		static readonly Regex n1an3RegEx = new Regex("^[\\d]{1}[\\w]{3}$", RegexOptions.Compiled);
		static readonly Regex a1an4RegEx = new Regex("^[a-zA-Z]{1}[\\w]{4}$", RegexOptions.Compiled);

		static Regex GetRegexByPattern(Patterns pattern) => pattern switch
		{
			Patterns.n1an2 => n1an2RegEx,
			Patterns.n1an3 => n1an3RegEx,
			Patterns.a1an4 => a1an4RegEx,
			_ => null,
		};
	}

	public class PreviousDocumentEqualityComparer : IEqualityComparer<IPreviousDocument>
	{
		PreviousDocumentEqualityComparer()
		{
		}

		public static PreviousDocumentEqualityComparer Instance { get; } = new PreviousDocumentEqualityComparer();

		public bool Equals(IPreviousDocument x, IPreviousDocument y)
		{
			if (ReferenceEquals(x, y))
			{
				return true;
			}

			if (x is null || y is null || x.GetType() != y.GetType())
			{
				return false;
			}

			return x.GoodsItemNumber == y.GoodsItemNumber
					&& x.TypeOfPackages == y.TypeOfPackages
					&& x.NumberOfPackages == y.NumberOfPackages
					&& x.MeasurementUnitAndQualifier == y.MeasurementUnitAndQualifier
					&& x.QuantityValue == y.QuantityValue
					&& x.Type == y.Type
					&& x.Description == y.Description;
		}

		public int GetHashCode(IPreviousDocument obj)
		{
			unchecked
			{
				var hashCode = obj.GoodsItemNumber.GetHashCode();
				hashCode = (hashCode * 397) ^ (obj.TypeOfPackages != null ? obj.TypeOfPackages.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ obj.NumberOfPackages.GetHashCode();
				hashCode = (hashCode * 397) ^ (obj.MeasurementUnitAndQualifier != null ? obj.MeasurementUnitAndQualifier.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ obj.QuantityValue.GetHashCode();
				hashCode = (hashCode * 397) ^ (obj.Type != null ? obj.Type.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (obj.Description != null ? obj.Description.GetHashCode() : 0);
				return hashCode;
			}
		}
	}

	public class SupportingDocumentEqualityProvider : IEqualityComparer<ISupportingDocument>
	{
		SupportingDocumentEqualityProvider()
		{
		}

		public static SupportingDocumentEqualityProvider Instance { get; } = new SupportingDocumentEqualityProvider();

		public bool Equals(ISupportingDocument x, ISupportingDocument y)
		{
			if (ReferenceEquals(x, y))
			{
				return true;
			}

			if (x is null || y is null || x.GetType() != y.GetType())
			{
				return false;
			}

			return x.IssuingAuthorityName == y.IssuingAuthorityName
					&& Nullable.Equals(x.ValidityDateValue, y.ValidityDateValue)
					&& x.MeasurementUnitAndQualifier == y.MeasurementUnitAndQualifier
					&& x.QuantityValue == y.QuantityValue
					&& x.Currency == y.Currency
					&& x.AmountValue == y.AmountValue
					&& x.DocumentLineItemNumber == y.DocumentLineItemNumber
					&& x.Type == y.Type
					&& x.Description == y.Description;
		}

		public int GetHashCode(ISupportingDocument obj)
		{
			unchecked
			{
				var hashCode = (obj.IssuingAuthorityName != null ? obj.IssuingAuthorityName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ obj.ValidityDateValue.GetHashCode();
				hashCode = (hashCode * 397) ^ (obj.MeasurementUnitAndQualifier != null ? obj.MeasurementUnitAndQualifier.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ obj.QuantityValue.GetHashCode();
				hashCode = (hashCode * 397) ^ (obj.Currency != null ? obj.Currency.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ obj.AmountValue.GetHashCode();
				hashCode = (hashCode * 397) ^ obj.DocumentLineItemNumber.GetHashCode();
				hashCode = (hashCode * 397) ^ (obj.Type != null ? obj.Type.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (obj.Description != null ? obj.Description.GetHashCode() : 0);
				return hashCode;
			}
		}
	}
}
