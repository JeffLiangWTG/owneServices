using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class BondedGoods : IBondedGoods
	{
		public BondedGoods(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, "entryHeader");
			entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, "entryInstruction");
		}

		readonly CusEntryHeader entryHeader;

		readonly CusEntryInstruction entryInstruction;

		public ZString AddDutyReasonCode => ZString.Empty;

		public IEnumerable<IBondedGoodsInvoice> BondedGoodsInvoices
		{
			get
			{
				var governmentUniformInvoices = entryHeader?.Declaration?.GovernmentUniformInvoices;
				if (governmentUniformInvoices != null)
				{
					foreach (GovernmentUniformInvoiceData invoiceData in governmentUniformInvoices)
					{
						yield return new BondedGoodsInvoices(invoiceData);
					}
				}
			}
		}

		public IBondedGoodsMonthlyReport BondedGoodsMonthlyReport => new BondedGoodsMonthlyReport(entryInstruction);

		public IBondedParty InBondedParty
		{
			get
			{
				IBondedParty result = null;
				var warehouse2 = entryInstruction.Warehouse2;
				if (warehouse2 != null)
				{
					if (!SharedHelper.DoesNotHaveCCPAndCPWNumbers(warehouse2))
					{
						result = new BondedParty(warehouse2);
					}
				}
				return result;
			}
		}

		public IBondedParty OutBondedParty
		{
			get
			{
				IBondedParty result = null;
				var warehouse = entryInstruction.Warehouse;
				if (warehouse != null)
				{
					if (!SharedHelper.DoesNotHaveCCPAndCPWNumbers(warehouse))
					{
						result = new BondedParty(warehouse);
					}
				}
				return result;
			}
		}

		public IEnumerable<IBondedParty> PreBondedParties => null;

		public ZString DocumentCode => (entryInstruction?.CEI_BillOfMaterials ?? ZBool.False) ? YesNoList.Codes.Yes : YesNoList.Codes.No;

		public ZString Refundable => (entryInstruction?.CEI_DutyRefund ?? ZBool.False) ? YesNoList.Codes.Yes : YesNoList.Codes.No;

		public IEnumerable<IBondedParty> BondedFactories
		{
			get
			{
				var bondedFactories = entryHeader?.Declaration?.BondedFactories?.Where(a => a.E2_AddressType == DocAddressTypes.Codes.BondedFactory);
				if (bondedFactories != null)
				{
					foreach (var bondedFactory in bondedFactories)
					{
						var customsControlID = SharedHelper.GetCustomsControlID(bondedFactory.Address)?.OK_CustomsRegNo ?? ZString.Empty;
						if (!customsControlID.IsEmpty)
						{
							yield return new BondedGoodsBondedFactories(bondedFactory, customsControlID);
						}
					}
				}
			}
		}
	}
}
