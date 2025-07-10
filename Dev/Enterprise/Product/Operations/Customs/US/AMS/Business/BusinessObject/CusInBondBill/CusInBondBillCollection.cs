using System.ComponentModel;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondBillCollection : CusInBondBillCollection<CusInBondBill>, ISailingSynchronisationTargetCollection<BillOfLading, CusInBondBill>
		, IBindingList
	{
		public CusInBondBillCollection(CusInBondHeader master)
			: base(master)
		{
		}

		public CusInBondHeader Master
		{
			get { return (CusInBondHeader)Relationship.Master; }
		}

		public CusInBondBill this[ZString issuerCode, ZString billNumber]
		{
			get
			{
				CusInBondBill result = null;
				foreach (var bill in this)
				{
					if (!bill.IsDeleted && bill.B0_IssuerCode == issuerCode && bill.B0_MasterBillNumber == billNumber)
					{
						result = bill;
						break;
					}
				}
				return result;
			}
		}

		public bool HasNVOCCBill
		{
			get { return this.Any(x => x.IsNVOCCBill); }
		}

		#region Implementation

		protected override bool MatchesFilterCore(CusInBondBill element, bool fetchOnlyFromLocalCache)
		{
			return !Master.IsNVOCCHeader || !element.IsOceanBillType;
		}

		protected override bool AllowNew
		{
			get
			{
				var master = Master;
				return master != null && !master.ShouldSynchronise;
			}
		}

		protected override void DefaultFromHeader(CusInBondBill newBill)
		{
			var master = Master;
			master.InitializeNewBill(newBill);
		}

		protected override void DefaultFromPreviousBill(CusInBondBill newBill, CusInBondBill previousBill)
		{
			if (previousBill == null)
			{
				DefaultFromHeader(newBill);
			}
			else
			{
				newBill.B0_IssuerCode = previousBill.B0_IssuerCode;
				newBill.B0_BillStatus = previousBill.B0_BillStatus;
				newBill.B0_RL_NKPortOfLading = previousBill.B0_RL_NKPortOfLading;
				newBill.B0_PortOfLadingKCode = previousBill.B0_PortOfLadingKCode;
				newBill.B0_RL_NKLastForeignPort = previousBill.B0_RL_NKLastForeignPort;
				newBill.B0_LastForeignPortKCode = previousBill.B0_LastForeignPortKCode;
				newBill.B0_RL_NKForeignPortOfContract = previousBill.B0_RL_NKForeignPortOfContract;
				newBill.B0_ForeignPortOfContractKCode = previousBill.B0_ForeignPortOfContractKCode;
				newBill.B0_RL_NKInBondPortOfDest = previousBill.B0_RL_NKInBondPortOfDest;
				newBill.B0_InBondPortOfDestDCode = previousBill.B0_InBondPortOfDestDCode;
				newBill.B0_DateOfDischarge = previousBill.B0_DateOfDischarge;

				foreach (var snp in previousBill.SecondaryNotifyParties)
				{
					var scac = snp.CY_Data;
					if (!scac.IsEmpty)
					{
						newBill.SecondaryNotifyParties.AddNewIfNotExist(scac);
					}
				}
			}
		}

		protected override CusInBondBill GetPreviousBill()
		{
			CusInBondBill result = null;
			for (var i = Count - 1; i >= 0; i--)
			{
				var bill = this[i];
				if (!bill.IsDeleted)
				{
					result = bill;
					break;
				}
			}
			return result;
		}

		#endregion
	}
}
