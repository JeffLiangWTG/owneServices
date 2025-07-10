using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public sealed class CargoSecurityExportAWBHeaderValidation : CommonExportAWBHeaderValidation
	{
		public CargoSecurityExportAWBHeaderValidation(ConsolExportAWBHeader parent)
			: base(parent)
		{
		}

		public new ConsolExportAWBHeader Parent
		{
			get { return (ConsolExportAWBHeader)base.Parent; }
		}

		public override void ValidateAll()
		{
			ValidateEH_AgentApprovalNumber();
			ValidateEH_RN_NKAgentApprovalCountryCode();
			ValidateEH_AdditionalScreeningMethods();
			ValidateEH_SecurityStatusIssuedBy();
			ValidateEH_SecurityStatusIssueDate();
			ValidateEH_SecurityStatus();
		}

		internal IEnumerable<ZPropertyInfo> AWBHeaderSecurityProperties
		{
			get
			{
				yield return Parent.EH_AgentApprovalCategoryInfo;
				yield return Parent.EH_AgentApprovalNumberInfo;
				yield return Parent.EH_AgentApprovalExpiryDateInfo;
				yield return Parent.EH_RN_NKAgentApprovalCountryCodeInfo;
				yield return Parent.EH_SecurityStatusInfo;
				yield return Parent.EH_SecurityStatusIssuedByInfo;
				yield return Parent.EH_SecurityStatusIssueDateInfo;
				yield return Parent.EH_AdditionalScreeningMethodsInfo;
			}
		}

		protected override void CheckEH_AgentApprovalNumber()
		{
			var configuration = SupplyChainSecurityConfiguration.New(Parent.EH_RN_NKAgentApprovalCountryCode);
			configuration.CheckEH_AgentApprovalNumberAdditionalValidation(Parent);

			if (!Parent.EH_AgentApprovalNumberInfo.HasErrors()
				&& Parent.EH_AgentApprovalNumber.IsEmpty
				&& !Parent.EH_AgentApprovalNumberInfo.HasMessageErrors())
			{
				Parent.EH_AgentApprovalNumberInfo.AddMessageError(Res.GetString("1d4bd3bf-1807-4198-b6be-b6cae4fa885c", "Enter the identifier of the Regulated Agent issuing the security status."));
			}
		}

		protected override void CheckEH_RN_NKAgentApprovalCountryCode()
		{
			ListValidation.ErrorIfInvalidCode(Parent.EH_RN_NKAgentApprovalCountryCodeInfo, Parent.Lookups.IssuingCountryList);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.EH_RN_NKAgentApprovalCountryCodeInfo);

			if (!Parent.EH_RN_NKAgentApprovalCountryCodeInfo.HasNotifications()
				&& Parent.EH_RN_NKAgentApprovalCountryCode != GlbCompany.CurrentCompany.Country.Code)
			{
				Parent.EH_RN_NKAgentApprovalCountryCodeInfo.AddWarning(Res.GetString("7422b886-8363-42f6-bda8-32319338cf49", "The country/region here is different from the currently logged in country/region."));
			}
		}

		protected override void CheckEH_AdditionalScreeningMethods()
		{
			if (!Parent.EH_AdditionalScreeningMethods.IsEmpty)
			{
				return;
			}

			if (Parent.HasOtherScreeningMethod)
			{
				Parent.EH_AdditionalScreeningMethodsInfo.AddMessageError(Res.GetString("2971c635-95e5-4517-838c-4c6b96025d61", "One or more of the Shipments have Inspection type \"{0}\" - {1}.  Please specify the alternative method of screening used.",
					ScreeningMethods.Codes.SubjectedToAnyOtherMeans,
					ScreeningMethods.Descriptions.SubjectedToAnyOtherMeans));
			}
		}

		protected override void CheckEH_SecurityStatusIssuedBy()
		{
			if (Parent.EH_SecurityStatusIssuedBy.IsEmpty)
			{
				Parent.EH_SecurityStatusIssuedByInfo.AddMessageError(Res.GetString("c8054f19-58c5-4d1e-8dda-3f0b1c7e2576", "Person Screening is required."));
			}

			var configuration = SupplyChainSecurityConfiguration.New();

			if (Parent.SecurityStatusIssuedBy != null)
			{
				var warningMessage = configuration.GetUncertifiedUserForScreeningError(Parent.SecurityStatusIssuedBy);

				if (!warningMessage.IsEmpty)
				{
					Parent.EH_SecurityStatusIssuedByInfo.AddWarning(warningMessage);
				}
			}
		}

		protected override void CheckEH_SecurityStatusIssueDate()
		{
			if (Parent.EH_SecurityStatusIssueDate.IsEmpty)
			{
				Parent.EH_SecurityStatusIssueDateInfo.AddMessageError(Res.GetString("4a9da26a-a792-46f9-8232-5ff9cc8fafaf", "This defaults from the Consol > Docs > Master Bill Issue Date field. Check it has been entered."));
			}
		}

		protected override void CheckEH_SecurityStatus()
		{
			if (Parent.EH_SecurityStatus.IsEmpty)
			{
				Parent.EH_SecurityStatusInfo.AddMessageError(Res.GetString("3920de7c-d23a-4447-961f-fea11dbb00e3", "Security Status is required. This can be entered on Consol > Docs > Security Status."));
			}
			else if (Parent.EH_SecurityStatus ==
				AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft)
			{
				Parent.EH_SecurityStatusInfo.AddMessageError(Res.GetString("46791ad7-7215-484a-9031-763d58f6b9b5",
					"The Special Handling Code on Consol > AWB is {0}. The Security Declaration can only be issued if the cargo is secured. Check the security status of the attached Shipments.",
					AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft));
			}
			else if (Parent.SupplyChainSecurityConfiguration.UseAccountConsignorValidationForPassengerAircraft
				&& Parent.SupplyChainSecurityConfiguration.IsEnabled
				&& Parent.EH_SecurityStatus == AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft)
			{
				if (Parent.CargoSecurityKnownShippers
					.Cast<ExportAWBSecurityStatusLine>()
					.Any(line => line.EAS_ApprovalCategory == AviationSecuritySchemeMembership.Codes.AccountConsignor))
				{
					Parent.EH_SecurityStatusInfo.AddMessageError(Res.GetString("cf9ef4bd-a738-4f0c-b33b-3286ac4b6ee7",
						"This code is not valid if any of the shipments attached have been received from an {0}. This defaults from AWB > Special Handling Codes.",
						AviationSecuritySchemeMembership.Descriptions.AccountConsignor));
				}
			}
		}
	}
}
