using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeCreditorOverrideLookups : AutoAccChargeCreditorOverrideLookups
	{
		new AccChargeCreditorOverride Parent => base.Parent as AccChargeCreditorOverride;

		#region JobTypeList

		public CodeDescriptionPairList JobTypeList => Parent.JobTypeDirectionAndTransportListProvider.JobTypeList;

		public static class JobTypeAdditionalCodes
		{
			public const string All = "ALL";
		}

		#endregion

		#region DirectionList

		public CodeDescriptionPairList DirectionList => Parent.JobTypeDirectionAndTransportListProvider?.DirectionList;

		#endregion

		#region TransportModeList

		public CodeDescriptionPairList TransportModeList => Parent.JobTypeDirectionAndTransportListProvider.TransportModeList;

		public static class TransportModeAdditionalCodes
		{
			public const string All = "ALL";
		}

		#endregion

		#region DefaultingRuleList

		public CodeDescriptionPairList DefaultingRuleList
		{
			get
			{
				var resultList = DefaultingRuleListForInvalidJobType;
				return resultList;
			}
		}

		CodeDescriptionPairList DefaultingRuleListForInvalidJobType => new CodeDescriptionPairList { JobInvoicingConsumerType.ChargeCreditorDefaultingRulesBase.SpecificCreditorAlways };

		#endregion

		#region Creditor

		public override OrgHeaderCollection Creditors
		{
			get
			{
				if (fTransportProviders == null)
				{
					fTransportProviders = new OrgHeaderCollection(Factory);
					fTransportProviders.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property1", ZBool.True));
				}
				return fTransportProviders;
			}
		}

		OrgHeaderCollection fTransportProviders;

		#endregion

		#region CreditorRoleList

		public CodeDescriptionPairList CreditorRoleList
		{
			get
			{
				if (creditorRoleList == null)
				{
					creditorRoleList = new CodeDescriptionPairList();
					creditorRoleList.AddPair(DocAddressTypes.Codes.OverseasAgent, DocAddressTypes.Descriptions.OverseasAgent);
				}
				return creditorRoleList;
			}
		}
		CodeDescriptionPairList creditorRoleList;

		#endregion

		#region PaymentTermList

		public CodeDescriptionPairList PaymentTermList
		{
			get
			{
				if (paymentTermList == null)
				{
					paymentTermList = new CodeDescriptionPairList();
					paymentTermList.AddPair(PaymentTermAdditionalCodes.All, Res.GetString("C032A299-BD79-4E0D-9279-51254DB62AE2", "Any Payment Term"));
					paymentTermList.AddPair(Constants.PaymentType.Prepaid, Res.GetString("949f8062-3723-4575-9b58-64a8124baa7e", "Prepaid"));
					paymentTermList.AddPair(Constants.PaymentType.Collect, Res.GetString("f20a19ee-7af5-4e75-bd8f-e4e25c634641", "Collect"));
				}
				return paymentTermList;
			}
		}
		CodeDescriptionPairList paymentTermList;

		public static class PaymentTermAdditionalCodes
		{
			public const string All = "ALL";
		}
		#endregion

		public AccChargeCreditorOverrideLookups(AutoAccChargeCreditorOverride parent) : base(parent)
		{
		}
	}
}
