using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	[Flags]
	public enum ChargedParty
	{
		None = 0,
		Agent = 1,
		LocalClient = 2,
		Consignee = 4,
		Consignor = 8,
		Unknown = 16,
		ControllingCustomerFallingBackToLocalClient = 32,
		ControllingCustomerFallingBackToAgent = 64,
	}

	sealed public class IncoTerm
	{
		public IncoTerm(IncoTermChargeCodes incoTermChargeCodes)
		{
			IncoTermChargeCodes = incoTermChargeCodes;
		}

		IncoTermChargeCodes IncoTermChargeCodes { get; }

		#region Get LocalClientAgent

		/// <summary>
		/// The logic is as follows:
		/// --> Export
		///		--> Consignor Charges
		///			--> Bill To Local Client
		///		--> Consignee Charges
		///			--> Local Services	
		///				--> Bill to Overseas Agent
		///			--> Overseas Services
		///				--> Do Not Bill (Already billed to Overseas Agent)
		///	--> Import
		///		--> Consignee Charges
		///			--> Bill To Local Client
		///		--> Consignor Charges
		///			--> Local Services
		///				--> Bill to Overseas Agent
		///			--> Overseas Services
		///				--> Do Not Bill (Already billed to Overseas Agent)
		/// </summary>
		public ChargedParty GetLocalClientOrAgent(Directions exportImport, string chargeGroup, bool overseasAgentApplicable = true)
		{
			var result = ChargedParty.None;

			if (GetLocalClientOrAgentCore(exportImport, chargeGroup, IncoTermCode) == ChargedParty.LocalClient)
			{
				result = ChargedParty.LocalClient;
			}
			else if (overseasAgentApplicable && GetLocalClientOrAgentCore(exportImport, chargeGroup) == ChargedParty.LocalClient)
			{
				result = ChargedParty.Agent;
			}

			return result;
		}

		static ChargedParty GetLocalClientOrAgentCore(Directions exportImport, string chargeGroup, string incoTermCode)
		{
			var consignorConsignee = IncoTermRegistry.GetConsignorConsignee(chargeGroup, incoTermCode);
			return GetLocalClientOrAgentByConsignorConsignee(exportImport, consignorConsignee);
		}

		static ChargedParty GetLocalClientOrAgentByConsignorConsignee(Directions exportImport, string consignorConsignee)
		{
			switch (consignorConsignee)
			{
				case Constants.PaymentParty.Consignor:
					return GetLocalClientOrAgentForConsignor(exportImport);
				case Constants.PaymentParty.Consignee:
					return GetLocalClientOrAgentForConsignee(exportImport);
				default:
					return ChargedParty.LocalClient;
			}
		}

		static ChargedParty GetLocalClientOrAgentForConsignor(Directions exportImport)
		{
			switch (exportImport)
			{
				case Directions.Export:
					return ChargedParty.LocalClient;
				case Directions.Import:
					return ChargedParty.Agent;
				default:
					return ChargedParty.Unknown;
			}
		}

		static ChargedParty GetLocalClientOrAgentForConsignee(Directions exportImport)
		{
			switch (exportImport)
			{
				case Directions.Import:
					return ChargedParty.LocalClient;
				case Directions.Export:
					return ChargedParty.Agent;
				default:
					return ChargedParty.Unknown;
			}
		}

		static ChargedParty GetLocalClientOrAgentCore(Directions exportImport, string chargeGroup)
		{
			if (chargeGroup.In(ChargeCodeGroupList.Codes.Freight, ChargeCodeGroupList.Codes.Insurance))
			{
				return GetLocalClientOrAgentForConsignor(exportImport);
			}

			var prepaidCollect = PaymentTermInfos.GetPrepaidCollect(chargeGroup);
			return GetLocalClientOrAgentByPrepaidCollect(exportImport, prepaidCollect);
		}

		public static ChargedParty GetLocalClientOrAgentByPrepaidCollect(Directions exportImport, string prepaidCollect)
		{
			switch (exportImport)
			{
				case Directions.Export:
					switch (prepaidCollect)
					{
						case Constants.PaymentType.Prepaid:
							return ChargedParty.LocalClient;
						case Constants.PaymentType.Collect:
							return ChargedParty.Agent;
						default:
							return ChargedParty.Unknown;
					}
				case Directions.Import:
					switch (prepaidCollect)
					{
						case Constants.PaymentType.Prepaid:
							return ChargedParty.Agent;
						case Constants.PaymentType.Collect:
							return ChargedParty.LocalClient;
						default:
							return ChargedParty.Unknown;
					}
				default:
					return ChargedParty.Unknown;
			}
		}

		#endregion

		public ZString IncoTermCode => IncoTermChargeCodes.IncoTerm;

		public override string ToString() => IncoTermCode;
	}
}
