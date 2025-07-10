using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class JobRequiredDocAttrib : AutoJobRequiredDocAttrib
	{
		public JobRequiredDocAttrib(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List("Lookups.AttributeNameList")]
		public override ZString D0_AttribName
		{
			get { return base.D0_AttribName; }
			set { base.D0_AttribName = value; }
		}

		[List("Lookups.AttributeValueList")]
		public override ZString D0_AttribValue
		{
			get { return base.D0_AttribValue; }
			set { base.D0_AttribValue = value; }
		}

		public Type[] GetCustomDocumentAttributeValueTypes()
		{
			var types = new List<Type> { typeof(JobRequiredDocAttrib), typeof(OrgHeader) };

			if (RequiredDocument.EQ_DocUsage == JobRequiredDocument.DocUsage.Broker)
			{
				types.Add(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			}
			return types.ToArray();
		}

		public BusinessObject[] GetCustomDocumentAttributeValueRoots()
		{
			return Array.Empty<BusinessObject>();
		}

		[MaxLength(JobRequiredDocAttrib.Schema.D0_AttribValueMaxLength)]
		[RootTypeProvider(nameof(GetCustomDocumentAttributeValueTypes), nameof(GetCustomDocumentAttributeValueRoots))]
		[List("Lookups.AttributeValueList")]
		public ZString D0_AttribDisplayValue
		{
			get
			{
				var result = D0_AttribValue;
				if (IsCompanyCode && ZGuid.IsGuid(D0_AttribValue))
				{
					var company = Factory.Load<GlbCompany>(new ZGuid(D0_AttribValue));
					result = company != null ? company.GC_Code : ZString.Empty;
				}
				else if (IsCeilingLimit)
				{
					if (TryParseCeilingLimit(result, out var res))
					{
#if NETFRAMEWORK
						result = res.ToString("N", Culture.CurrentCompanyCountryCulture);
#else
						// more informations: https://github.com/dotnet/runtime/issues/45695
						// NumberFormatInfo.NumberDecimalDigits change for en-US, en-GB, de-DE, fr-FR and other cultures in netcore
						result = res.ToString("N2", Culture.CurrentCompanyCountryCulture);
#endif
					}
				}
				return result;
			}
			set
			{
				var oldValue = D0_AttribValue;
				if (IsCompanyCode)
				{
					var company = Factory.LoadFromNaturalKey<GlbCompany>(ZArchitecture.Schema.GlbCompanySchema.GC_Code, value);
					if (company != null)
					{
						D0_AttribValue = company.PK.ToString();
					}
				}
				else if (IsDocumentReceivedDate)
				{
					if (ZDateTime.TryParseISO8601Date(value, out var result))
					{
						D0_AttribValue = result.ToBestReadableDateTimeString();
					}
					else
					{
						D0_AttribValue = ZDateTime.Invalid.ToString();
					}
				}
				else if (IsCeilingLimit)
				{
					if (TryParseCeilingLimit(value, out var result, true))
					{
						D0_AttribValue = result.ToString("F", Culture.Invariant);
					}
					else
					{
						D0_AttribValue = "<INVALID>";
					}
				}
				else
				{
					D0_AttribValue = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateD0_AttribDisplayValue();
					if (oldValue != D0_AttribValue && IsCustomsDistrict)
					{
						var requiredDocument = RequiredDocument;
						var boxNumberDocAttrib = requiredDocument?.BoxNumberDocAttrib;
						if (boxNumberDocAttrib != null)
						{
							boxNumberDocAttrib.D0_AttribDisplayValueInfo.RefreshBinding();
							boxNumberDocAttrib.D0_AttribDisplayValue = requiredDocument.GetDefaultBoxNumber(D0_AttribValue);
						}
					}
				}
				D0_AttribDisplayValueInfo.RefreshBinding();
			}
		}

		public static bool TryParseCeilingLimit(ZString ceilingLimitAsText, out ZDecimal result, bool fromDisplay = false)
		{
			return ZDecimal.TryParse(ceilingLimitAsText, fromDisplay ? Culture.CurrentCompanyCountryCulture : Culture.Invariant, out result);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1110:DoNotUseColumnNamesDirectly", Justification = "Baseline")]
		public ZPropertyInfo D0_AttribDisplayValueInfo => GetZPropertyInfo(nameof(D0_AttribDisplayValue));

		public ZString AttribValueFieldType
		{
			get
			{
				switch (D0_AttribName)
				{
					case JobRequiredDocAttribTypeList.Codes.Direction:
					case JobRequiredDocAttribTypeList.Codes.CostaRicaEXVDocumentType:
					case JobRequiredDocAttribTypeList.Codes.CustomsDistrict:
					case JobRequiredDocAttribTypeList.Codes.BoxNumber:
					case JobRequiredDocAttribTypeList.Codes.BondedID:
						return nameof(FieldType.TextDropEdit);
					case JobRequiredDocAttribTypeList.Codes.DocumentReceivedDate:
					case JobRequiredDocAttribTypeList.Codes.BuyerIssueDate:
						return nameof(FieldType.DateTime);
					case JobRequiredDocAttribTypeList.Codes.CeilingLimit:
						return nameof(FieldType.Decimal);
					case JobRequiredDocAttribTypeList.Codes.TradePreferenceCode:
						return IsCARelatedCountry ? nameof(FieldType.TextDropEdit) : nameof(FieldType.Text);
					default:
						if (IsCustomAttribute)
						{
							return nameof(FieldType.TextMacro);
						}
						else
						{
							return nameof(FieldType.Text);
						}
				}
			}
		}

		public override bool ReadOnly => IsSellerControlNumber && RequiredDocument.IsEXVDebtor && IsInDatabase;

		public bool IsPortOfEntry => D0_AttribName == JobRequiredDocAttribTypeList.Codes.PortOfEntry;

		public bool IsDirection => D0_AttribName == JobRequiredDocAttribTypeList.Codes.Direction;

		public bool IsSellerControlNumber => D0_AttribName == JobRequiredDocAttribTypeList.Codes.SellerControlNumber;

		public bool IsDocumentReceivedDate => D0_AttribName == JobRequiredDocAttribTypeList.Codes.DocumentReceivedDate;

		public bool IsBuyerIssueDate => D0_AttribName == JobRequiredDocAttribTypeList.Codes.BuyerIssueDate;

		public bool IsCeilingLimit => D0_AttribName == JobRequiredDocAttribTypeList.Codes.CeilingLimit;

		public bool IsGovernmentAuthorisationReference => D0_AttribName == JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference;

		public bool IsCompanyCode => D0_AttribName == JobRequiredDocAttribTypeList.Codes.CompanyCode;

		public bool IsCostaRicaEXVDocumentType => D0_AttribName == JobRequiredDocAttribTypeList.Codes.CostaRicaEXVDocumentType;

		public bool IsIssuingAuthorityName => D0_AttribName == JobRequiredDocAttribTypeList.Codes.IssuingAuthorityName;

		public bool IsCustomsDistrict => D0_AttribName == JobRequiredDocAttribTypeList.Codes.CustomsDistrict;

		public bool IsBoxNumber => D0_AttribName == JobRequiredDocAttribTypeList.Codes.BoxNumber;

		public bool IsBondedID => D0_AttribName == JobRequiredDocAttribTypeList.Codes.BondedID;

		public bool IsTradePreferenceCode => D0_AttribName == JobRequiredDocAttribTypeList.Codes.TradePreferenceCode;

		public bool IsCustomAttribute => WorkflowDataRegistry.Instance.EnableWorkflowValidation.Value.Count > 0 && Regex.IsMatch(D0_AttribName, "^Custom[1-9]:", RegexOptions.IgnoreCase);

		public bool IsCARelatedCountry => RequiredDocument is JobRequiredDocument reqDoc && reqDoc.EQ_RN_NKRelatedCountry == Core.Constants.CountryCodes.Canada;
	}
}
