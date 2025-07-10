using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgInvoiceRollupOrGroupLookups : AutoOrgInvoiceRollupOrGroupLookups
	{
		public OrgInvoiceRollupOrGroupLookups(AutoOrgInvoiceRollupOrGroup parent)
			: base(parent)
		{
		}

		OrgInvoiceRollupOrGroup InvoiceRollupOrGroup
		{
			get { return (OrgInvoiceRollupOrGroup)Parent; }
		}

		#region Invoice Styles

		public CodeDescriptionPairList InvoicePostingOptionsList
		{
			get
			{
				CodeDescriptionPairList list = InvoiceRollupOrGroup.InvoiceRollupOrGroupHelper.InvoicePostingOptionsList;
				list.Insert(0, new CodeDescriptionPair(OrgInvoiceRollupOrGroup.InvoicePostingOptionDefaultCode, DescriptionForDefaultCode));

				return list;
			}
		}

		#endregion

		#region Invoice Line Display Options List

		public CodeDescriptionPairList InvoiceLineDisplayOptionsList
		{
			get
			{
				CodeDescriptionPairList list = InvoiceRollupOrGroup.InvoiceRollupOrGroupHelper.InvoiceLineDisplayOptionsList;
				list.Insert(0, new CodeDescriptionPair(OrgInvoiceRollupOrGroup.InvoiceLineDisplayOptionDefaultCode, DescriptionForDefaultCode));

				return list;
			}
		}

		#endregion

		#region Job Types

		public CodeDescriptionPairList JobTypeList
		{
			get { return InvoiceRollupOrGroup.InvoiceRollupOrGroupHelper.JobTypeList; }
		}

		public static class JobType_List
		{
			public static readonly CodeDescriptionPair All = new CodeDescriptionPair("ALL", ResString.GetMultilingualString("dcfbad37-068e-41ea-b284-f722da62f3f2", "All Job Types"));
			public static readonly CodeDescriptionPair ShipmentAndBrokerage = new CodeDescriptionPair("SAB", ResString.GetMultilingualString("0adfd0bd-35df-4365-9800-64403f9ee14f", "Shipment and Brokerage"));
			public static readonly CodeDescriptionPair NonJobRelated = new CodeDescriptionPair("MSC", ResString.GetMultilingualString("071dacaa-76a5-46ba-bbac-0d17c1d15222", "Non Job Related"));
		}

		public static bool IsJobTypeShipmentBrokerageOrBoth(ZString jobType)
		{
			return jobType == JobInvoicingConsumerTypes.Shipment.Code ||
					jobType == JobInvoicingConsumerTypes.Brokerage.Code ||
					jobType == OrgInvoiceRollupOrGroupLookups.JobType_List.ShipmentAndBrokerage.Code;
		}

		public static CodeDescriptionPairList JobTypeFullList
		{
			get
			{
				JobInvoicingConsumerTypes jobTypes = JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes();
				jobTypes.Add(JobType_List.ShipmentAndBrokerage);
				jobTypes.Add(JobType_List.NonJobRelated);
				jobTypes.Sort();
				jobTypes.Insert(0, JobType_List.All);
				return jobTypes;
			}
		}

		#endregion

		#region Transport Modes

		public CodeDescriptionPairList TransportModeList
		{
			get { return InvoiceRollupOrGroup.InvoiceRollupOrGroupHelper.TransportModeList; }
		}

		#endregion

		#region Directions

		public CodeDescriptionPairList ServiceDirectionList
		{
			get { return InvoiceRollupOrGroup.InvoiceRollupOrGroupHelper.ServiceDirectionList; }
		}

		#endregion

		#region ServiceLevels

		public CodeDescriptionPairList ServiceLevelList => InvoiceRollupOrGroup.InvoiceRollupOrGroupHelper.ServiceLevelList;

		#endregion

		#region Group/Sub-Total Style

		public CodeDescriptionPairList GroupOrSubTotalList
		{
			get
			{
				CodeDescriptionPairList list = InvoiceRollupOrGroup.InvoiceRollupOrGroupHelper.GroupOrSubTotalList;
				list.Insert(0, new CodeDescriptionPair(OrgInvoiceRollupOrGroup.GroupOrSubtotalOptionDefaultCode, DescriptionForDefaultCode));

				return list;
			}
		}

		public CodeDescriptionPairList GroupOrSubTotalStyleList
		{
			get
			{
				var invoiceRollupOrGroupHelper = InvoiceRollupOrGroup.InvoiceRollupOrGroupHelper;
				var list = invoiceRollupOrGroupHelper.GroupOrSubTotalStyleList;

				if (!invoiceRollupOrGroupHelper.IsGroupOrSubTotalOnlyForCLCAndNOG(InvoiceRollupOrGroup.PG_GroupOrSubTotal))
				{
					list.Insert(0, new CodeDescriptionPair(OrgInvoiceRollupOrGroup.GroupOrSubtotalStyleOptionDefaultCode, DescriptionForDefaultCode));
				}
				return list;
			}
		}

		public static CodeDescriptionPairList GetGroupOrSubTotalStyleList(ZString jobType)
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			if (IsJobTypeShipmentBrokerageOrBoth(jobType) || jobType == OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code || jobType == JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
			{
				list.AddRange(OrgCodeLists.InvoiceLineGroupings_List);
			}
			else if (jobType == OrgInvoiceRollupOrGroupLookups.JobType_List.NonJobRelated.Code)
			{
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.None, OrgDescriptions.InvoiceLineGroupings.None);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.CCD, OrgDescriptions.InvoiceLineGroupings.CCD);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.CCG, OrgDescriptions.InvoiceLineGroupings.CCG);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.CLC, OrgDescriptions.InvoiceLineGroupings.CLC);
			}
			else
			{
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.All, OrgDescriptions.InvoiceLineGroupings.All);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.None, OrgDescriptions.InvoiceLineGroupings.None);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.CCG, OrgDescriptions.InvoiceLineGroupings.CCG);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.CCD, OrgDescriptions.InvoiceLineGroupings.CCD);
				list.AddPair(OrgConstants.InvoiceLineGroupings.Code.CLC, OrgDescriptions.InvoiceLineGroupings.CLC);
			}

			return list;
		}

		public static string GroupOrSubTotalStyleCodeWhenRegistryValueInvalid
		{
			get { return OrgConstants.InvoiceLineGroupings.Code.None; }
		}

		string DescriptionForDefaultCode
		{
			get { return Res.GetString("f2230631-e930-4620-8745-33c13f55fb29", "Use Defaults from Registry - refer to registry setting for details"); }
		}

		#endregion
	}
}
