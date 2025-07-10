using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using GW = Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.Customs.NO.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
public sealed class NODocSADH : DocBaseWrapper
{
	NODocSADH(CusEntryHeader entryHeader, BusinessObjectFactory factory) : base(entryHeader, factory)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		declaration = Argument.NotNull(this.entryHeader.Declaration, nameof(this.entryHeader.Declaration));
	}

	readonly CusEntryHeader entryHeader;
	readonly JobDeclaration declaration;

	public NODocSADHInvoiceCollection Invoices => invoices ??= new (entryHeader.InvoiceHeaders, Factory);
	NODocSADHInvoiceCollection invoices;

	public NODocSADHLineCollection Lines => lines ??= new (entryHeader.MergedLines, Factory);
	NODocSADHLineCollection lines;

	public NODocSADHLineTaxCollection Box47Taxes => box47Taxes ??= GetBox47TaxesCore();
	NODocSADHLineTaxCollection box47Taxes;

	public ZInt Box47TaxesTotalSum => Box47Taxes.Cast<NODocSADHLineTax>().Sum(x => x.ChargeAmountTotal);

	NODocSADHLineTaxCollection GetBox47TaxesCore()
	{
		var allChargesFromEntryHeader = entryHeader.MergedLines
			.Cast<CusEntryLine>()
			.SelectMany(line => line.Fees)
			.Cast<CusEntryLineFee>()
			.Where(fee => !fee.CF_IsLandedCostOnly)
			.GroupBy(fee => fee.CF_ChargeType.Substring(0, 2))
			.Select(fee => new
			{
				ChargeAmount = fee.Sum(fee => fee.CF_ChargeAmount),
				Fee = fee.FirstOrDefault()
			});

		var lineTaxCollection = new NODocSADHLineTaxCollection(Factory);
		foreach (var singleCharge in allChargesFromEntryHeader)
		{
			var charge = new NODocSADHLineTax(singleCharge.Fee, (ZInt)singleCharge.ChargeAmount, Factory);
			lineTaxCollection.Add(charge);
		}

		lineTaxCollection.Sort<NODocSADHLineTax>(DutyComparer.Comparison);
		return lineTaxCollection;
	}

	public static NODocSADH New(CusEntryHeader entryHeader, BusinessObjectFactory factory) => new (entryHeader, factory);

	#region SADH Page 1

	public ZString BoxAContent
	{
		get
		{
			return new ZStringBuilder()
				.Append(LineDeclarationText)
				.AppendIfNotEmpty(System.Environment.NewLine, entryHeader.CH_EntryReleaseDate.ToString(DateFormat))
				.AppendIfNotEmpty(System.Environment.NewLine, EntryReleaseNumberFirst6Chars)
				.AppendIfNotEmpty(System.Environment.NewLine, EntryReleaseNumberLast10Chars)
				.ToString();
		}
	}

	public ZString Box1DeclarationType => declaration.JE_MessageSubType;

	public GW.OrganisationWrapper Box2Supplier
	{
		get
		{
			GW.OrganisationWrapper result = null;
			if (declaration.SupplierDocumentaryAddress != null)
			{
				result = GetOrganisationWrapper();
			}
			return result;
		}
	}

	public ZBool ShowSupplierOrganisationNumber => declaration.IsExport;

	public ZInt Box6TotalNoOfUnits => declaration.JE_TotalNoOfPacks;

	public GW.OrganisationWrapper Box8Importer
	{
		get
		{
			GW.OrganisationWrapper result = null;
			if (declaration.ImporterDocumentaryAddress != null)
			{
				result = new GW.OrganisationWrapper(GW.OrganisationUsageType.Consignee, declaration.ImporterDocumentaryAddress, Factory);
			}
			return result;
		}
	}

	public DocAddress Box14Declarant
		=> declaration.DeclarantAddress is OrgAddress declarantAddress
			? DocAddress.New(declarantAddress, Factory)
			: null;

	public ZString Box1EntryStyle => entryHeader.EntryInstruction?.CEI_Style ?? ZString.Empty;

	public ZInt Box5NoOfEntries => entryHeader.MergedLines.Count;

	public ZString Box7ReferenceNumber => declaration.JE_DeclarationReference;

	public ZString Box8ImporterOrganisationNumber => GetImporterOrganisationNumber();

	public ZInt Box12TotalFreightCost
	{
		get => (ZInt)ZArchitecture.Core.Utilities.Round(Lines.Cast<NODocSADHLine>().Sum(l => l.TotalFreightCostInLocalCurrency), 1);
	}

	public ZString Box15PortOfOrigin => declaration.JE_GoodsOrigin;

	public ZString Box19ContainerMode
		=> declaration.JE_ContainerMode == Core.Constants.ContainerModes.Containerised
			? GoodsTransportedInContainerIndicator
			: GoodsTransportedNotInContainerIndicator;

	public ZString Box20IncoTerm => RandomJobComInvoiceHeader?.JZ_IncoTerm ?? ZString.Empty;

	public ZString Box20IncoTermPlace => RandomJobComInvoiceHeader?.JZ_IncoTermPlace ?? ZString.Empty;

	public ZString Box21TransportId => declaration.JE_VesselName;

	public ZString Box21TransportNationality => declaration.JE_RN_NKTransportNationality;

	public ZString Box22Currency => !box22Currency.IsEmpty ? box22Currency : (box22Currency = GetBox22Currency().currency);
	ZString box22Currency;

	public ZString Box22InvTotalAmount => GetBox22Amount();

	public ZString Box23ExchangeRate
	{
		get
		{
			var refExchangeRate = entryHeader switch
			{
				{ IsMultiInvoiceCurrency: true } => ZDecimal.Zero,
				{ RandomEntryLine.RandomLine.InvoiceLineCurrencyExchangeRateForCustoms: var rate } => rate,
				_ => ZDecimal.Zero,
			};
			ZDecimal exchangeRateToDisplay = refExchangeRate.IsEmpty ? 1m : refExchangeRate;
			return exchangeRateToDisplay.ToString(ExchangeRateDecimalFormat);
		}
	}

	public ZString Box24TransactionNature => entryHeader.RandomHeader.JZ_ValuationCode;

	public ZString Box25TransportMode => declaration.JE_TransportMode;

	public ZString Box28FinancialInformation => Box28FinancialInformationText;

	public ZString Box30LocationOfGoods => GetBox30LocationOfGoods();

	public ZString Box37ProcedureCode
	{
		get
		{
			if (entryHeader.InvoiceLines.FirstOrDefault() is not { } invoice)
			{
				return ZString.Empty;
			}
			var result = invoice.JI_Procedure;
			if (result == ZString.Empty)
			{
				result = entryHeader.Procedure;
			}
			return result;
		}
	}

	public ZString Box45Adjustments
	{
		get
		{
			var adjustments = (ZInt)entryHeader.MergedLines.Sum(lin => lin.AdjustmentsRounded);
			if (adjustments.IsEmpty)
			{
				return ZString.Empty;
			}
			return adjustments.ToString();
		}
	}

	public ZString Box46StatisticalValue
	{
		get
		{
			var statisticalValue = (ZDecimal)entryHeader.MergedLines.Sum(lin => lin.CL_StatisticalValue.Round(0));
			if (statisticalValue.IsEmpty)
			{
				return ZString.Empty;
			}
			return statisticalValue.ToString();
		}
	}

	public ZString Box48DeferredPayment
	{
		get
		{
			switch (entryHeader.CH_PaymentMethod)
			{
				case NOPaymentMethodCodeList.Codes.ImportersDeferred:
					return declaration.Importer.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber);
				case NOPaymentMethodCodeList.Codes.Cash:
					return PaymentCash;
				case NOPaymentMethodCodeList.Codes.ForwardersDayCredit:
					return PaymentForwarderDayCredit;
				default:
					return ZString.Empty;
			}
		}
	}

	public ZString Box49EntryNumber
	{
		get
		{
			var result = declaration.JE_GoodsNumber;
			if (!declaration.JE_Position.IsEmpty)
			{
				result += '/' + declaration.JE_Position;
			}

			if (entryHeader.EntryInstruction is { CEI_SubPosition.IsEmpty: false } instruction)
			{
				result += '/' + instruction.CEI_SubPosition;
			}

			return result;
		}
	}

	public ZString Box54Place => declaration?.DeclarantAddress?.OA_City ?? ZString.Empty;

	public ZString Box54Date => entryHeader.EntryReleaseNumberIssueDate.ToShortDateString();

	public ZString Box54NameOfDeclarantAndRepresentative
	{
		get
		{
			switch (declaration.JE_DeclarantType)
			{
				case RepresentationTypeList.Codes.Direct:
					return declaration.DeclarantAddress is not null
						? $"{declaration.DeclarantAddress.Header.OH_FullName} by {declaration.Branch.Company.GC_Name}"
						: ZString.Empty;
				case RepresentationTypeList.Codes.Self:
				case RepresentationTypeList.Codes.Indirect:
				default:
					return declaration.Branch.Company.GC_Name;
			}
		}
	}

	public ZString Box54SignatoryNameAndPosition
	{
		get
		{
			if (declaration is { CusAgent: { } agent })
			{
				var name = agent.GS_FullName;
				if (!agent.GS_Title.IsEmpty)
				{
					name += " (" + agent.GS_Title + ")";
				}
				return name;
			}
			return ZString.Empty;
		}
	}

	public ZString Box54SignatoryEmailDetails => declaration.CusAgent != null ? declaration.CusAgent.GS_EmailAddress : ZString.Empty;

	#endregion

	#region SADH Page 2

	public ZString DeclarationReference => declaration.JE_DeclarationReference;

	public ZString EntryReleaseDate => entryHeader.CH_EntryReleaseDate.ToString(DateFormat);

	public ZString EntryReleaseNumberFirst6Chars => entryHeader.EntryReleaseNumber.Left(6);

	public ZString EntryReleaseNumberLast10Chars => entryHeader.EntryReleaseNumber.Right(10);

	public ZString OwnerReference => declaration.JE_OwnerRef;

	#endregion

	JobComInvoiceHeader RandomJobComInvoiceHeader => randomJobComInvoiceHeader ??= entryHeader.RandomHeader;
	JobComInvoiceHeader randomJobComInvoiceHeader;

	(int count, ZString currency) GetBox22Currency()
	{
		var uniqueCurrencies = entryHeader.MergedLines
			.SelectMany(x => x.InvoiceLines)
			.Cast<JobComInvoiceLine>()
			.Select(invoice => invoice.InvoiceHeader.JZ_RX_NKInvoice_Currency.ToString());
		var uniqueCurrenciesHash = new HashSet<string>(uniqueCurrencies);
		return uniqueCurrenciesHash.Count == 1 ? (1, uniqueCurrenciesHash.FirstOrDefault()) : (uniqueCurrenciesHash.Count, Core.Constants.CurrencyCodes.Norway);
	}

	ZString GetBox22Amount()
	{
		var currencyCountAndType = GetBox22Currency();
		Func<JobComInvoiceLine, ZDecimal> amountSelector;

		if (currencyCountAndType.count > 1 || currencyCountAndType.currency.EqualsIgnoringCase(Core.Constants.CurrencyCodes.Norway))
		{
			amountSelector = i => i.InvoiceHeader.JZ_InvoiceAmountInLocalCurrency;
		}
		else
		{
			amountSelector = i => i.InvoiceHeader.JZ_InvoiceAmount;
		}

		var totalAmount = CalculateBox22Amount(amountSelector);
		return new ZString(totalAmount.ToString());
	}

	ZDecimal CalculateBox22Amount(Func<JobComInvoiceLine, ZDecimal> amountSelector)
	{
		var totalAmount = new ZDecimal(0);

		foreach (var mergedLine in entryHeader.MergedLines)
		{
			totalAmount += mergedLine.InvoiceLines
				.Cast<JobComInvoiceLine>()
				.Select(amountSelector)
				.Sum(amount => amount);
		}

		return totalAmount;
	}

	ZString GetBox30LocationOfGoods()
	{
		var location = declaration.JE_LocationOfGoods;
		if (location != ZString.Empty)
		{
			var description = declaration.Lookups.LocationOfGoodsCollection.GetDescriptionFromCode(location);
			if (!description.IsNullOrEmpty())
			{
				return description + " - " + location;
			}
		}
		return location;
	}

	ZString GetImporterOrganisationNumber()
	{
		var importerHeader = declaration.Importer;
		if (importerHeader is null)
		{
			return ZString.Empty;
		}

		var orgType = importerHeader.OH_Category;
		var isPrivatePerson = orgType == OrgConstants.Category.NaturalPersonIndividual;
		var codeTypes = isPrivatePerson
			? new ZString[] { OrgCusCode.USACodeTypes.SocialSecurityNumber }
			: new ZString[] { OrgCusCode.NorwayCodeTypes.MVA, OrgCusCode.CodeTypes.OrganizationNumber };

		var cusCodes = importerHeader.CustomsCodes.GetOrgCusCodesForMatchingCodesIgnoringCountry(codeTypes);
		var code = cusCodes.FirstOrDefault()?.OK_CustomsRegNo ?? ZString.Empty;
		return isPrivatePerson
			? MaskSensitiveInformation(code)
			: code;
	}

	static ZString MaskSensitiveInformation(ZString code)
	{
		var codeLength = code.Length;
		if (codeLength > 6)
		{
			return code.Substring(0, 6).PadRight(codeLength, '*');
		}

		return code;
	}

	GW.OrganisationWrapper GetOrganisationWrapper() => new (GW.OrganisationUsageType.Consignor, declaration.SupplierDocumentaryAddress, Factory);

	const string DateFormat = "yyyyMMdd";
	const string LineDeclarationText = "LINJEDEKLARERT";
	const string GoodsTransportedInContainerIndicator = "1";
	const string GoodsTransportedNotInContainerIndicator = "0";
	const string ExchangeRateDecimalFormat = "F6";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description String")]
	const string Box28FinancialInformationText = "SE FAKTURAOVERSIKT";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description String")]
	const string PaymentCash = "Kontant";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description String")]
	const string PaymentForwarderDayCredit = "Dagsoppgjør";
}
