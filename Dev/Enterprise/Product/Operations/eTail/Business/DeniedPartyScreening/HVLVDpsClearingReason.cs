using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business.DeniedPartyScreening
{
	public class HVLVDpsClearingReason : NonPersistentBusinessObject<HVLVDpsClearingReasonValidation>
	{
		public HVLVDpsClearingReason(HVLVDpsMatch[] matches, BusinessObjectFactory factory) : base(factory)
		{
			Argument.NotNull(matches, nameof(matches));
			Argument.NotNull(factory, nameof(factory));
		}

		public override HVLVDpsClearingReasonValidation GetNewValidation()
		{
			return new HVLVDpsClearingReasonValidation(this);
		}

		[ResourceStringData("Enterprise.eTail.Business.DeniedPartyScreening.HVLVDpsClearingReason.ClearingReason", Caption = "Clearing Reason")]
		[List(nameof(ClearingReasonList))]
		public ZString ClearingReason
		{
			get => clearingReason;
			set
			{
				clearingReason = value;
				ClearingReasonInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateClearingReason();
				}

				ClearingReasonText = GetDefaultClearingReasonText();
			}
		}

		ZString clearingReason;

		public ZPropertyInfo ClearingReasonInfo => GetZPropertyInfo(nameof(ClearingReason));

		public ZString ClearingReasonTitle
		{
			get
			{
				return ClearingReasonList.GetDescriptionFromCode(ClearingReason);
			}
		}

		ZString clearingReasonText;

		[ResourceStringData("Enterprise.eTail.Business.DeniedPartyScreening.HVLVDpsClearingReason.ClearingReasonText", Caption = "Clearing Reason Text")]
		public ZString ClearingReasonText
		{
			get => clearingReasonText;
			set
			{
				clearingReasonText = value;
				ClearingReasonTextInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateClearingReasonText();
				}
			}
		}

		public ZPropertyInfo ClearingReasonTextInfo => GetZPropertyInfo(nameof(ClearingReasonText));

		public bool ClearingReasonText_ReadOnly
		{
			get
			{
				if (ClearingReason == RequireReasonForCLRRegistryConstants.Code.Other)
				{
					return false;
				}

				var registryPresetReason = GetRegistryPresetReason();

				return registryPresetReason == null || !registryPresetReason.IsMandatory;
			}
		}

		RequireReasonForCLRItem GetRegistryPresetReason()
		{
			return OrganisationsDataRegistry.Instance
				.DeniedPartyScreeningRequireReasonForClearing.Value.ItemCollection.Cast<RequireReasonForCLRItem>()
				.FirstOrDefault(i => i.Code == ClearingReason);
		}

		string GetDefaultClearingReasonText()
		{
			var registryPresetReason = GetRegistryPresetReason();

			if (registryPresetReason == null)
			{
				return string.Empty;
			}

			if (string.IsNullOrWhiteSpace(registryPresetReason.ClearingReason) && !registryPresetReason.IsMandatory)
			{
				return StmEntityScreeningLogSchema.PJ_ClearedReason.SqlDbDefault.ToString();
			}

			return registryPresetReason.ClearingReason;
		}

		public CodeDescriptionPairList ClearingReasonList => Factory.GetCachedValue("Enterprise.eTail.Business.DeniedPartyScreening.HVLVDpsClearingReason|ClearingReasonList", () =>
		{
			var clearingReasonList = new CodeDescriptionPairList();

			foreach (RequireReasonForCLRItem item in OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.Value.ItemCollection)
			{
				clearingReasonList.AddPair(item.Code, item.Title);
			}

			if (Env.Security.OrgDeniedPartyScreeningAllowOtherReason.IsAllowed)
			{
				clearingReasonList.AddPair(RequireReasonForCLRRegistryConstants.Code.Other, RequireReasonForCLRRegistryConstants.Description.Other);
			}

			return clearingReasonList;
		});
	}
}
