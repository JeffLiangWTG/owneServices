using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CBP1302DocumentLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CBP1302DocumentLine(CusInBondBill bill)
			: base(bill.Factory)
		{
			this.Bill = bill;
			FirstContainerInBill = false;
		}
		internal readonly CusInBondBill Bill;

		public ZString LastForeignPortCombined
		{
			get
			{
				return (FirstContainerInBill) ?
					ZString.Format("{0} {1}", Bill.B0_RL_NKLastForeignPort, Bill.B0_LastForeignPortKCode) :
					ZString.Empty;
			}
		}

		public ZString ForeignPortOfContractCombined
		{
			get
			{
				return (FirstContainerInBill) ?
					ZString.Format("{0} {1}", Bill.B0_RL_NKForeignPortOfContract, Bill.B0_ForeignPortOfContractKCode) :
					ZString.Empty;
			}
		}

		public ZString Addresses
		{
			get
			{
				var result = ZString.Empty;

				if (ShowForeignShipper)
				{
					result += "SH: " + Bill.ForeignShipper.AddressAsASingleLine + System.Environment.NewLine + System.Environment.NewLine;
				}
				else if (ShowConsignee)
				{
					result += "CO: " + Bill.Consignee.AddressAsASingleLine + System.Environment.NewLine + System.Environment.NewLine;
				}
				else if (ShowNotifyParty)
				{
					result += "NF: " + Bill.NotifyParty1.AddressAsASingleLine + System.Environment.NewLine + System.Environment.NewLine;
				}

				return result;
			}
		}

		public ZString ContainersMarksSeals { get; set; }
		public ZString PackagesAndDescriptions { get; set; }
		public ZString WeightAndUnit { get; set; }
		public ZBool FirstContainerInBill { get; set; }
		public ZBool ShowForeignShipper { get; set; }
		public ZBool ShowConsignee { get; set; }
		public ZBool ShowNotifyParty { get; set; }

		public ZString BillNumber
		{
			get
			{
				var result = new ZStringBuilder();
				if (FirstContainerInBill)
				{
					result.AppendIfNotEmpty(Bill.B0_IssuerCode, Bill.B0_MasterBillNumber);

					var header = Bill.Header;
					if (header != null && header.HasMultiDischargePorts)
					{
						result.AppendIfNotEmpty(PortOfDestDCode);
					}
				}

				return result.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine);
			}
		}

		public ZString PortOfDestDCode
		{
			get
			{
				var result = ZString.Empty;
				var port = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, Bill.B0_InBondPortOfDestDCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
				if (port != null)
				{
					result = ZString.Format("{0} {1}", port.ZZD_Code, port.ZZD_Description);
				}
				return result;
			}
		}

		public ZString Dispositions
		{
			get
			{
				var result = ZString.Empty;

				if (FirstContainerInBill)
				{
					if (Bill.DispositionCodes.Count > 0)
					{
						var endLine = System.Environment.NewLine + System.Environment.NewLine;
						result = BillNumber + endLine + "DISPOSITIONS:" + System.Environment.NewLine + Bill.DispositionCodes.GetDispositionsFor1302Document(endLine);
					}
					else
					{
						result = BillNumber;
					}
				}

				return result;
			}
		}
	}
}
