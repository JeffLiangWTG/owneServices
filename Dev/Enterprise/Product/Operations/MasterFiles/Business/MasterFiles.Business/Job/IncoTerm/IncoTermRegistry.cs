using System;
using System.Linq;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public sealed class IncoTermRegistry : RegistryItemSet
	{
		#region Construction

		public static IncoTermRegistry Instance => instance ??= new IncoTermRegistry();

		[ThreadStatic]
		static IncoTermRegistry instance;

		IncoTermRegistry()
		{
		}

		#endregion

		public override bool IsForProductivityWise => false;

		#region Charge Always Codes

		public ChargeCodeListRegistryItem ChargeLocalClientAlwaysCodes
			=> GetItem("ChargeLocalClientAlwaysCodes", delegate
			{
				return new ChargeCodeListRegistryItem
				(
					"ChargeLocalClientAlwaysCodes",
					RawDataRegistry.Categories.AutoRating_ChargeCodes,
					ResString.GetMultilingualString("8735b68c-6120-49a7-b22a-6cc1a400a1b2", "Charge Local Client Always Charge Codes"),
					ResString.GetMultilingualString("7bbbb922-76f5-68b4-4128-513c8cdf6547", "Charge Codes to charge Local Client regardless of Incoterm"),
					string.Empty,
					RegistryFindBoxFilter.None
				);
			});

		public ChargeCodeListRegistryItem ChargeAgentAlwaysCodes
			=> GetItem("ChargeAgentAlwaysCodes", delegate
			{
				return new ChargeCodeListRegistryItem
				(
					"ChargeAgentAlwaysCodes",
					RawDataRegistry.Categories.AutoRating_ChargeCodes,
					ResString.GetMultilingualString("c4bf13a2-ab18-41af-add9-3323935ac782", "Charge Agent Always Charge Codes"),
					ResString.GetMultilingualString("2b21c46a-aa45-3d8e-4882-143eb2bdc0f6", "Charge Codes to charge Agent regardless of Incoterm"),
					string.Empty,
					RegistryFindBoxFilter.None
				);
			});

		#endregion

		#region TryGetValue

		public static bool TryGetValue(string incoTerm, out IncoTerm value)
		{
			if (TryGetValue(incoTerm, out IncoTermChargeCodes incoTermChargeCodes))
			{
				value = new IncoTerm(incoTermChargeCodes);
				return true;
			}

			value = null;
			return false;
		}

		static bool TryGetValue(string incoTerm, out IncoTermChargeCodes value)
		{
			var incoTermChargeCodesCollection = RegistryFactory.Instance.GetCachedValue("IncoTermList", () => RatingDataRegistry.Instance.IncoTermDefinition.Value);
			foreach (IncoTermChargeCodes incoTermChargeCodes in incoTermChargeCodesCollection)
			{
				if (incoTermChargeCodes.IncoTerm == incoTerm)
				{
					value = incoTermChargeCodes;
					return true;
				}
			}

			value = null;
			return false;
		}

		#endregion

		public static string[] Keys
			=> new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms)
				.Cast<CodeDescriptionPair>()
				.Select(x => x.Code)
				.ToArray();

		public static ChargedParty GetLocalClientOrAgentRegardlessOfIncoterm(AccChargeCode chargeCode, bool overseasAgentApplicable = true)
		{
			if (chargeCode != null)
			{
				var chargeCodePK = chargeCode.PK.ToGuid();
				if (Instance.ChargeLocalClientAlwaysCodes.GetAsGuidArray().Any(x => x == chargeCodePK))
				{
					return ChargedParty.LocalClient;
				}

				if (overseasAgentApplicable && Instance.ChargeAgentAlwaysCodes.GetAsGuidArray().Any(x => x == chargeCodePK))
				{
					return ChargedParty.Agent;
				}
			}

			return ChargedParty.None;
		}

		public static string GetConsignorConsignee(string chargeGroup, string incoTerm)
		{
			if (TryGetValue(incoTerm, out IncoTermChargeCodes incoTermChargeCodes))
			{
				switch (chargeGroup)
				{
					case ChargeCodeGroupList.Codes.Freight:
						return incoTermChargeCodes.Freight;
					case ChargeCodeGroupList.Codes.Origin:
						return incoTermChargeCodes.Origin;
					case ChargeCodeGroupList.Codes.OriginBrokerage:
					case ChargeCodeGroupList.Codes.OriginBrokerageOnly:
						return incoTermChargeCodes.OriginBrokerage;
					case ChargeCodeGroupList.Codes.Loading:
						return incoTermChargeCodes.Loading;
					case ChargeCodeGroupList.Codes.Insurance:
						return incoTermChargeCodes.Insurance;
					case ChargeCodeGroupList.Codes.Unloading:
						return incoTermChargeCodes.Unloading;
					case ChargeCodeGroupList.Codes.Destination:
						return incoTermChargeCodes.Destination;
					case ChargeCodeGroupList.Codes.CustomsDuty:
						return incoTermChargeCodes.CustomsDuty;
					case ChargeCodeGroupList.Codes.Brokerage:
					case ChargeCodeGroupList.Codes.BrokerageOnly:
						return incoTermChargeCodes.Brokerage;
				}
			}

			return default;
		}

		public static string GetPrepaidCollect(string chargeGroup, string incoTerm)
		{
			var chargeParty = GetConsignorConsignee(chargeGroup, incoTerm);
			switch (chargeParty)
			{
				case Constants.PaymentParty.Consignor:
					return Constants.PaymentType.Prepaid;
				case Constants.PaymentParty.Consignee:
					return Constants.PaymentType.Collect;
				default:
					return default;
			}
		}
	}
}
