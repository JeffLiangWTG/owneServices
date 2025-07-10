using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	class NX5105Consignment_BondedGoods : IBondedGoods
	{
		readonly JobDeclaration declaration;
		readonly CusEntryInstruction entryInstruction;

		public NX5105Consignment_BondedGoods(CusEntryHeader entryHeader)
		{
			Argument.NotNull(entryHeader, "entryHeader");
			this.declaration = Argument.NotNull(entryHeader.Declaration, "declaration");
			this.entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, "entryInstruction");
		}

		ZString IBondedGoods.AddDutyReasonCode => entryInstruction.CEI_ReasonForDuty;

		IEnumerable<IBondedGoodsInvoice> IBondedGoods.BondedGoodsInvoices
		{
			get
			{
				foreach (GovernmentUniformInvoiceData uniform in declaration.GovernmentUniformInvoices.Cast<GovernmentUniformInvoiceData>().Where(x => !x.CY_Code.IsEmpty))
				{
					yield return new BondedGoodsInvoiceWrapper(uniform.CY_Code, uniform.Amount);
				}
			}
		}

		IBondedGoodsMonthlyReport IBondedGoods.BondedGoodsMonthlyReport
		{
			get
			{
				var whsMonth = entryInstruction.CEI_WHSMonth;
				var whsTradeReferenceNo = entryInstruction.CEI_WHSTradeReferenceNo;
				if (!whsMonth.IsEmpty || !whsTradeReferenceNo.IsEmpty)
				{
					return new BondedGoodsMonthlyReportWrapper(ZInt.ParseEmptyAsZero(whsMonth), whsTradeReferenceNo);
				}
				return null;
			}
		}

		IBondedParty IBondedGoods.InBondedParty
		{
			get
			{
				IBondedParty result = null;
				var warehouse2 = entryInstruction.Warehouse2;
				if (warehouse2 != null)
				{
					if (!SharedHelper.DoesNotHaveCCPAndCPWNumbers(warehouse2))
					{
						result = new NX5105BondedPartyWrapper(warehouse2, OrgCusCode.CodeTypes.WarehouseControlledPremisesID);
					}
				}
				return result;
			}
		}

		IBondedParty IBondedGoods.OutBondedParty
		{
			get
			{
				IBondedParty result = null;
				var warehouse = entryInstruction.Warehouse;
				if (warehouse != null)
				{
					if (!SharedHelper.DoesNotHaveCCPAndCPWNumbers(warehouse))
					{
						result = new NX5105BondedPartyWrapper(warehouse, OrgCusCode.CodeTypes.WarehouseControlledPremisesID);
					}
				}
				return result;
			}
		}

		IEnumerable<IBondedParty> IBondedGoods.PreBondedParties
		{
			get
			{
				foreach (var bondedFactory in declaration?.BondedFactories?.Cast<BondedFactory>().Where(x => x.Address != null))
				{
					var customsControlID = SharedHelper.GetCustomsControlID(bondedFactory.Address)?.OK_CustomsRegNo ?? ZString.Empty;
					if (!customsControlID.IsEmpty)
					{
						yield return new NX5105PreBondedPartyWrapper(bondedFactory.Address, OrgCusCode.CodeTypes.VATCode, customsControlID);
					}
				}
			}
		}

		public ZBool Empty
		{
			get
			{
				IBondedGoods iBondedGoods = this;
				return iBondedGoods.AddDutyReasonCode.IsEmpty
					&& iBondedGoods.BondedFactories == null
					&& !(iBondedGoods.BondedGoodsInvoices?.Any() ?? ZBool.False)
					&& iBondedGoods.BondedGoodsMonthlyReport == null
					&& iBondedGoods.DocumentCode.IsEmpty
					&& iBondedGoods.InBondedParty == null
					&& iBondedGoods.OutBondedParty == null
					&& !(iBondedGoods.PreBondedParties?.Any() ?? ZBool.False)
					&& iBondedGoods.Refundable.IsEmpty;
			}
		}

		#region Not Applicable

		ZString IBondedGoods.DocumentCode => ZString.Empty;

		ZString IBondedGoods.Refundable => ZString.Empty;

		IEnumerable<IBondedParty> IBondedGoods.BondedFactories => null;

		#endregion
	}
}
