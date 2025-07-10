//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccChargeGovtChargeCodeOverrideLookups
//
//    This class should be used for overriding collections in AutoAccChargeGovtChargeCodeOverrideLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeGovtChargeCodeOverrideLookups : AutoAccChargeGovtChargeCodeOverrideLookups
	{
		public AccChargeGovtChargeCodeOverrideLookups(AutoAccChargeGovtChargeCodeOverride parent) : base(parent)
		{
		}

		new AccChargeGovtChargeCodeOverride Parent => (AccChargeGovtChargeCodeOverride)base.Parent;

		public CodeDescriptionPairList JobTypes
		{
			get
			{
				return jobTypes ?? (jobTypes = GetJobTypes());

				CodeDescriptionPairList GetJobTypes()
				{
					var result = Parent.JobTypeDirectionAndTransportListProvider.JobTypeList;

					if (!result.ContainsCode(AccountingMasterFilesConstants.JobTypes.NonJobRelated))
					{
						result.Add(new CodeDescriptionPair(AccountingMasterFilesConstants.JobTypes.NonJobRelated, Res.GetString("D7E9FDC3-0492-431A-A4DA-C175932F0DB7", "Non-Job")));
					}
					if (!result.ContainsCode(JobInvoicingConsumerTypes.OneOffQuotationCode))
					{
						result.Add(JobInvoicingConsumerTypes.OneOffQuotation);
					}
					result.Sort();

					return result;
				}
			}
		}

		CodeDescriptionPairList jobTypes;

		public CodeDescriptionPairList DirectionList => Parent.JobTypeDirectionAndTransportListProvider.DirectionList;

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				if (Parent.ACG_JobType.ToString() == JobInvoicingConsumerTypes.QuotedBookingCode
					|| Parent.ACG_JobType.ToString() == JobInvoicingConsumerTypes.OneOffQuotationCode)
				{
					var list = new CodeDescriptionPairList(Parent.JobTypeDirectionAndTransportListProvider.TransportModeList);

					list.RemoveCode(Constants.TransportModes.FixedTransportInstallations);
					list.RemoveCode(Constants.TransportModes.InlandWaterwayTransport);
					list.RemoveCode(Constants.TransportModes.OwnPropulsion);
					list.RemoveCode(Constants.TransportModes.Mail);
					list.RemoveCode(Constants.TransportModes.SeaAir);
					list.RemoveCode(Constants.TransportModes.AirSea);
					return list;
				}
				else if (Parent.ACG_JobType.ToString() == JobInvoicingConsumerTypes.CFSLoadListCode
						|| Parent.ACG_JobType.ToString() == JobInvoicingConsumerTypes.CFSShipmentCode)
				{
					var list = new CodeDescriptionPairList(Parent.JobTypeDirectionAndTransportListProvider.TransportModeList);
					list.RemoveCode(Constants.TransportModes.FixedTransportInstallations);
					list.RemoveCode(Constants.TransportModes.InlandWaterwayTransport);
					list.RemoveCode(Constants.TransportModes.OwnPropulsion);
					list.RemoveCode(Constants.TransportModes.Mail);
					list.RemoveCode(Constants.TransportModes.SeaAir);
					list.RemoveCode(Constants.TransportModes.AirSea);
					list.RemoveCode(Constants.TransportModes.Courier);
					return list;
				}

				return Parent.JobTypeDirectionAndTransportListProvider.TransportModeList;
			}
		}

		#region Cost / Sell List

		public static class CostSellAllCodes
		{
			public const string Cost = "COS";
			public const string Revenue = "REV";
			public const string All = "ALL";
		}

		public CodeDescriptionPairList CostSellList
		{
			get
			{
				CodeDescriptionPairList fCostSellList = new CodeDescriptionPairList();
				fCostSellList.AddPair(CostSellAllCodes.All, Res.GetString("A109E358-136B-436A-95A3-9DD1D7BA3981", "Cost and Revenue"));
				fCostSellList.AddPair(CostSellAllCodes.Cost, Res.GetString("35D9D403-FDDB-42B7-9103-A6F2AE939738", "Cost"));
				fCostSellList.AddPair(CostSellAllCodes.Revenue, Res.GetString("FDFE42C2-A57C-4156-916B-5B6D96517165", "Revenue"));

				return fCostSellList;
			}
		}

		#endregion
	}
}
