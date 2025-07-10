using System;
using Enterprise.Customs.Common;
using ChargeTypes = Enterprise.Customs.PL.Business.Declaration.PLCustomsChargeTypeList.Codes;
using IncoTerms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.Customs.PL.Business.Declaration;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:Field names should begin with lower-case letter", Justification = "Charge provider codes all start with numbers")]
internal class ChargesProvider
{
	public static ICustomsChargeCode[] Codes
	{
		get
		{
			return new[]
			{
				AL,
				AB,
				AD,
				AE,
				AF,
				AG,
				AH,
				AI,
				AJ,
				AK,
				AN,
				BA,
				BB,
				BD,
				BE,
				BF,
				BC,
				BG,
				_071V,
				_072X,
				_073V,
				_074A,
				_080B,
				_1STW,
				_2STW
			};
		}
	}

	public static string[] ConfiguredIncoTerms
	{
		get
		{
			return new[]
			{
				IncoTerms.ExWorks,
				IncoTerms.Other,
				IncoTerms.FreeCarrier,
				IncoTerms.FreeAlongsideShip,
				IncoTerms.FreeOnBoard,
				IncoTerms.CostAndFreight,
				IncoTerms.CostInsuranceAndFreight,
				IncoTerms.CarriagePaidTo,
				IncoTerms.CarriageAndInsurancePaidTo,
				IncoTerms.DeliveredAtPlace,
				IncoTerms.DeliveredAtPlaceUnloaded,
				IncoTerms.DeliveredAtTerminal,
				IncoTerms.DeliveredExQuay,
				IncoTerms.DeliveredDutyUnpaid,
				IncoTerms.DeliveredDutyPaid,

				// Referencing purposes
				IncoTerms.DeliveredAtFrontier,
				IncoTerms.DeliveredExShip
			};
		}
	}

	#region Customs Charge Code Configuration

	public static CustomsChargeCode AL => al ?? (al = new CustomsChargeCode(ChargeTypes.AL, PLCustomsChargeTypeList.Descriptions.AL)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = true
	});
	[ThreadStatic]
	static CustomsChargeCode al;

	public static CustomsChargeCode AB => ab ?? (ab = new CustomsChargeCode(ChargeTypes.AB, PLCustomsChargeTypeList.Descriptions.AB)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = true
	});
	[ThreadStatic]
	static CustomsChargeCode ab;

	public static CustomsChargeCode AD => ad ?? (ad = new CustomsChargeCode(ChargeTypes.AD, PLCustomsChargeTypeList.Descriptions.AD)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = false
	});
	[ThreadStatic]
	static CustomsChargeCode ad;

	public static CustomsChargeCode AE => ae ?? (ae = new CustomsChargeCode(ChargeTypes.AE, PLCustomsChargeTypeList.Descriptions.AE)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = true
	});
	[ThreadStatic]
	static CustomsChargeCode ae;

	public static CustomsChargeCode AF => af ?? (af = new CustomsChargeCode(ChargeTypes.AF, PLCustomsChargeTypeList.Descriptions.AF)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = true
	});
	[ThreadStatic]
	static CustomsChargeCode af;

	public static CustomsChargeCode AG => ag ?? (ag = new CustomsChargeCode(ChargeTypes.AG, PLCustomsChargeTypeList.Descriptions.AG)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = true
	});
	[ThreadStatic]
	static CustomsChargeCode ag;

	public static CustomsChargeCode AH => ah ?? (ah = new CustomsChargeCode(ChargeTypes.AH, PLCustomsChargeTypeList.Descriptions.AH)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = true
	});
	[ThreadStatic]
	static CustomsChargeCode ah;

	public static CustomsChargeCode AI => ai ?? (ai = new CustomsChargeCode(ChargeTypes.AI, PLCustomsChargeTypeList.Descriptions.AI)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = true
	});
	[ThreadStatic]
	static CustomsChargeCode ai;

	public static CustomsChargeCode AJ => aj ?? (aj = new CustomsChargeCode(ChargeTypes.AJ, PLCustomsChargeTypeList.Descriptions.AJ)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = true
	});
	[ThreadStatic]
	static CustomsChargeCode aj;

	public static CustomsChargeCode AK => ak ?? (ak = new CustomsChargeCode(ChargeTypes.AK, PLCustomsChargeTypeList.Descriptions.AK)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = false
	});
	[ThreadStatic]
	static CustomsChargeCode ak;

	public static CustomsChargeCode AN => an ?? (an = new CustomsChargeCode(ChargeTypes.AN, PLCustomsChargeTypeList.Descriptions.AN)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = true
	});
	[ThreadStatic]
	static CustomsChargeCode an;

	public static CustomsChargeCode BA => ba ?? (ba = new CustomsChargeCode(ChargeTypes.BA, PLCustomsChargeTypeList.Descriptions.BA)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = false,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = false
	});
	[ThreadStatic]
	static CustomsChargeCode ba;

	public static CustomsChargeCode BB => bb ?? (bb = new CustomsChargeCode(ChargeTypes.BB, PLCustomsChargeTypeList.Descriptions.BB)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = false,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = true
	});
	[ThreadStatic]
	static CustomsChargeCode bb;

	public static CustomsChargeCode BD => bd ?? (bd = new CustomsChargeCode(ChargeTypes.BD, PLCustomsChargeTypeList.Descriptions.BD)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = false,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = true
	});
	[ThreadStatic]
	static CustomsChargeCode bd;

	public static CustomsChargeCode BE => be ?? (be = new CustomsChargeCode(ChargeTypes.BE, PLCustomsChargeTypeList.Descriptions.BE)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = false,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = true
	});
	[ThreadStatic]
	static CustomsChargeCode be;

	public static CustomsChargeCode BF => bf ?? (bf = new CustomsChargeCode(ChargeTypes.BF, PLCustomsChargeTypeList.Descriptions.BF)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = false,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = true
	});
	[ThreadStatic]
	static CustomsChargeCode bf;

	public static CustomsChargeCode BC => bc ?? (bc = new CustomsChargeCode(ChargeTypes.BC, PLCustomsChargeTypeList.Descriptions.BC)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = false,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = false
	});
	[ThreadStatic]
	static CustomsChargeCode bc;

	public static CustomsChargeCode BG => bg ?? (bg = new CustomsChargeCode(ChargeTypes.BG, PLCustomsChargeTypeList.Descriptions.BG)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = false,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = true,
		IsIncoTermNeutral = true
	});
	[ThreadStatic]
	static CustomsChargeCode bg;

	public static CustomsChargeCode _071V => _071v ?? (_071v = new CustomsChargeCode(ChargeTypes._071V, PLCustomsChargeTypeList.Descriptions._071V)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = true
	});
	[ThreadStatic]
	static CustomsChargeCode _071v;

	public static CustomsChargeCode _072X => _072x ?? (_072x = new CustomsChargeCode(ChargeTypes._072X, PLCustomsChargeTypeList.Descriptions._072X)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = true,
		IsIncludedInITOTDeemedForThisCharge = true,
		IsIncoTermNeutral = true,
		ParentTypes = ChargeParentTypes.InvoiceLine
	});
	[ThreadStatic]
	static CustomsChargeCode _072x;

	public static CustomsChargeCode _073V => _073v ?? (_073v = new CustomsChargeCode(ChargeTypes._073V, PLCustomsChargeTypeList.Descriptions._073V)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = true,
		IsIncludedInITOTDeemedForThisCharge = true,
		IsIncoTermNeutral = true,
		ParentTypes = ChargeParentTypes.InvoiceLine
	});
	[ThreadStatic]
	static CustomsChargeCode _073v;

	public static CustomsChargeCode _074A => _074a ?? (_074a = new CustomsChargeCode(ChargeTypes._074A, PLCustomsChargeTypeList.Descriptions._074A)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = true,
		IsIncludedInITOTDeemedForThisCharge = true,
		IsIncoTermNeutral = true,
		ParentTypes = ChargeParentTypes.InvoiceLine
	});
	[ThreadStatic]
	static CustomsChargeCode _074a;

	public static CustomsChargeCode _080B => _080b ?? (_080b = new CustomsChargeCode(ChargeTypes._080B, PLCustomsChargeTypeList.Descriptions._080B)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = false,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = true,
		IsIncludedInITOTDeemedForThisCharge = true,
		IsIncoTermNeutral = true,
		ParentTypes = ChargeParentTypes.InvoiceLine
	});
	[ThreadStatic]
	static CustomsChargeCode _080b;

	public static CustomsChargeCode _1STW => _1stw ?? (_1stw = new CustomsChargeCode(ChargeTypes._1STW, PLCustomsChargeTypeList.Descriptions._1STW)
	{
		IsDutiable = false,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = false,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = true,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = false,
		IsIncludedInITOTDeemedForThisCharge = false,
		IsIncoTermNeutral = true
	});
	[ThreadStatic]
	static CustomsChargeCode _1stw;

	public static CustomsChargeCode _2STW => _2stw ?? (_2stw = new CustomsChargeCode(ChargeTypes._2STW, PLCustomsChargeTypeList.Descriptions._2STW)
	{
		IsDutiable = true,
		IsDutiableDeemedForThisCharge = true,
		IsVATible = true,
		IsVATibleDeemedForThisCharge = true,
		IsStatisticalValueApplicable = false,
		IsStatisticalValueApplicableDeemed = true,
		IsIncludedInITOTIfDeemed = true,
		IsIncludedInITOTDeemedForThisCharge = true,
		IsIncoTermNeutral = true
	});
	[ThreadStatic]
	static CustomsChargeCode _2stw;

	#endregion

	public ChargeConfiguration GetChargeConfiguration(string incoTerm, ICustomsChargeCode charge)
	{
		var chargeCode = charge.Code;
		return new ChargeConfiguration()
		{
			IsIncludedInInvoice = IsIncludedInInvoiceCharges.IsListed(incoTerm, chargeCode),
			IsIncludedInInvoiceAmountFixed = IsIncludedInInvoiceAmountFixedCharges.IsListed(incoTerm, chargeCode),
			IsMandatory = MandatoryIncotermCharges.IsListed(incoTerm, chargeCode),
			IsRecommended = RecommendedIncotermCharges.IsListed(incoTerm, chargeCode),
		};
	}

	#region Common Charge Configuraton

	readonly (string, string)[] IsIncludedInInvoiceCharges = new (string, string)[]
	{
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes._072X),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes._073V),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes._074A),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes._080B),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes._2STW),

		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.AL),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.AB),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.AE),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.AF),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.AG),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.AH),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.AI),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.AJ),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.AN),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.BB),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.BD),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.BE),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.BF),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.BG),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes._071V),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes._1STW),

		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.AD),

		(IncoTerms.CostAndFreight, ChargeTypes.AK),
		(IncoTerms.CostInsuranceAndFreight, ChargeTypes.AK),
		(IncoTerms.CarriageAndInsurancePaidTo, ChargeTypes.AK),
		(IncoTerms.CarriageAndInsurancePaidTo, ChargeTypes.BA),
		(IncoTerms.CarriagePaidTo, ChargeTypes.AK),
		(IncoTerms.CarriagePaidTo, ChargeTypes.BA),
		(IncoTerms.DeliveredAtFrontier, ChargeTypes.AK),
		(IncoTerms.DeliveredAtFrontier, ChargeTypes.BA),
		(IncoTerms.DeliveredAtPlace, ChargeTypes.AK),
		(IncoTerms.DeliveredAtPlace, ChargeTypes.BA),
		(IncoTerms.DeliveredAtTerminal, ChargeTypes.AK),
		(IncoTerms.DeliveredAtTerminal, ChargeTypes.BA),
		(IncoTerms.DeliveredDutyPaid, ChargeTypes.AK),
		(IncoTerms.DeliveredDutyPaid, ChargeTypes.BC),
		(IncoTerms.DeliveredDutyUnpaid, ChargeTypes.AK),
		(IncoTerms.DeliveredDutyUnpaid, ChargeTypes.BA),
		(IncoTerms.DeliveredExQuay, ChargeTypes.AK),
		(IncoTerms.DeliveredExQuay, ChargeTypes.BA),
		(IncoTerms.DeliveredExQuay, ChargeTypes.BC),
		(IncoTerms.DeliveredExShip, ChargeTypes.AK),
		(IncoTerms.DeliveredExShip, ChargeTypes.BA),
		(IncoTerms.DeliveredAtPlaceUnloaded, ChargeTypes.AK),
		(IncoTerms.DeliveredAtPlaceUnloaded, ChargeTypes.BA),
	};
	readonly (string, string)[] IsIncludedInInvoiceAmountFixedCharges = new (string, string)[]
	{
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes._072X),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes._073V),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes._074A),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes._080B),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes._2STW),

		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.AK),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.BA),
		(ChargeProviderHelper.AnyIncoTerms, ChargeTypes.BC),

		(IncoTerms.CostAndFreight, ChargeTypes.AD),
		(IncoTerms.CostInsuranceAndFreight, ChargeTypes.AD),
		(IncoTerms.CarriageAndInsurancePaidTo, ChargeTypes.AD),
		(IncoTerms.CarriagePaidTo, ChargeTypes.AD),
		(IncoTerms.DeliveredAtFrontier, ChargeTypes.AD),
		(IncoTerms.DeliveredAtPlace, ChargeTypes.AD),
		(IncoTerms.DeliveredAtTerminal, ChargeTypes.AD),
		(IncoTerms.DeliveredDutyPaid, ChargeTypes.AD),
		(IncoTerms.DeliveredDutyUnpaid, ChargeTypes.AD),
		(IncoTerms.DeliveredExQuay, ChargeTypes.AD),
		(IncoTerms.DeliveredExShip, ChargeTypes.AD),
		(IncoTerms.DeliveredAtPlaceUnloaded, ChargeTypes.AD),
		(IncoTerms.FreeAlongsideShip, ChargeTypes.AD),
		(IncoTerms.FreeCarrier, ChargeTypes.AD),
		(IncoTerms.FreeOnBoard, ChargeTypes.AD),
	};
	readonly (string, string)[] MandatoryIncotermCharges = new (string, string)[]
	{
		(IncoTerms.CarriageAndInsurancePaidTo, ChargeTypes.AK),
		(IncoTerms.CostInsuranceAndFreight, ChargeTypes.AK),
		(IncoTerms.CarriagePaidTo, ChargeTypes.AK),
		(IncoTerms.CostAndFreight, ChargeTypes.AK),
		(IncoTerms.DeliveredAtPlace, ChargeTypes.AK),
		(IncoTerms.DeliveredAtTerminal, ChargeTypes.AK),
		(IncoTerms.DeliveredDutyPaid, ChargeTypes.AK),
	};
	readonly (string, string)[] RecommendedIncotermCharges = new (string, string)[]
	{
		(IncoTerms.DeliveredAtPlace,  ChargeTypes.AK),
		(IncoTerms.DeliveredAtTerminal,  ChargeTypes.AK),
		(IncoTerms.ExWorks,  ChargeTypes.AK),
		(IncoTerms.DeliveredDutyPaid,  ChargeTypes.AK),
		(IncoTerms.CarriageAndInsurancePaidTo,  ChargeTypes.AK),
		(IncoTerms.CarriagePaidTo,  ChargeTypes.AK),
		(IncoTerms.CostInsuranceAndFreight,  ChargeTypes.AK),
		(IncoTerms.CostAndFreight,  ChargeTypes.AK),
		(IncoTerms.FreeOnBoard,  ChargeTypes.AK),
		(IncoTerms.FreeCarrier,  ChargeTypes.AK),
		(IncoTerms.FreeAlongsideShip,  ChargeTypes.AK)
	};

	#endregion
}
