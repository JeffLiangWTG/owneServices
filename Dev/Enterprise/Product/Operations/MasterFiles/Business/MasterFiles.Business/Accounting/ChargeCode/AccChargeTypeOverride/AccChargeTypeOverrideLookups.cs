using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeTypeOverrideLookups : AutoAccChargeTypeOverrideLookups
	{
		public AccChargeTypeOverrideLookups(AutoAccChargeTypeOverride parent)
			: base(parent)
		{ }

		new AccChargeTypeOverride Parent
		{
			get { return (AccChargeTypeOverride)base.Parent; }
		}

		#region Job Types

		public CodeDescriptionPairList JobTypes
		{
			get
			{
				var fJobTypes = JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes();
				fJobTypes.Add(new AllJobsConsumerType());

				return fJobTypes;
			}
		}

		#endregion

		#region Direction List

		public CodeDescriptionPairList DirectionList
		{
			get
			{
				CodeDescriptionPairList fDirectionList = new CodeDescriptionPairList();
				fDirectionList.AddPair("ALL", Res.GetString("aa07e555-7643-4ddd-a952-77a0d0f39065", "Import and Export"));
				fDirectionList.AddPair(Core.Constants.Sales.Mode.Import, Res.GetString("1976d300-2de6-4dba-a71a-2f912e9000a5", "Import"));
				fDirectionList.AddPair(Core.Constants.Sales.Mode.Export, Res.GetString("91126bdf-51de-4246-8489-0eefd288cac4", "Export"));

				return fDirectionList;
			}
		}

		#endregion

		#region Charge Types

		public CodeDescriptionPairList AC_ChargeType_List
		{
			get
			{
				CodeDescriptionPairList fAC_ChargeType_List = new CodeDescriptionPairList(OLookUpEditType.ChargeTypes);
				fAC_ChargeType_List.RemoveCode(Core.Constants.ChargeType.NonAccrual);
				fAC_ChargeType_List.RemoveCode(Core.Constants.ChargeType.Overhead);
				fAC_ChargeType_List.RemoveCode(Core.Constants.ChargeType.Comment);

				return fAC_ChargeType_List;
			}
		}

		#endregion

		#region Invoice Types

		public CodeDescriptionPairList InvoiceTypes
		{
			get
			{
				CodeDescriptionPairList fInvoiceTypes = new CodeDescriptionPairList();
				fInvoiceTypes.AddPair("DEF", Res.GetString("396a6f15-0ac9-4b2b-b752-40568b40a185", "Default - Based on Debtor's Invoice Posting Style"));

				if (!Parent.AN_JobType.IsEmpty && JobTypes.ContainsCode(Parent.AN_JobType))
				{
					JobInvoicingConsumerType consumerType = JobTypes[Parent.AN_JobType] as JobInvoicingConsumerType;
					fInvoiceTypes.AddRange(consumerType != null ? consumerType.InvoiceTypeList : new InvoiceTypesList());
				}
				return fInvoiceTypes;
			}
		}

		#endregion
	}
}