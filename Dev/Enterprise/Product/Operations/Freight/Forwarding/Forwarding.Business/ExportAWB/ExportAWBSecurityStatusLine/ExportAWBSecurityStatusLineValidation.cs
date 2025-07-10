//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoExportAWBSecurityStatusLineValidation
//
//    This class should be used for overriding validation in AutoExportAWBSecurityStatusLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ExportAWBSecurityStatusLineValidation : Forwarding.AWB.Business.ExportAWBSecurityStatusLineValidation
	{
		public ExportAWBSecurityStatusLineValidation(ExportAWBSecurityStatusLine parent)
			: base(parent)
		{
		}

		new ExportAWBSecurityStatusLine Parent
		{
			get { return (ExportAWBSecurityStatusLine)base.Parent; }
		}

		#region EAS_ApprovalCategory

		protected override void CheckEAS_ApprovalCategory()
		{
			var parent = Parent;

			if (!SupplyChainSecurityConfiguration.New().UseConsignmentSecurityDeclaration)
			{
				return;
			}

			if (parent.Type != SecurityStatusLineType.KnownConsignor)
			{
				return;
			}

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.Japan)
			{
				if (!parent.Shipments.Any())
				{
					parent.EAS_ApprovalCategoryInfo.AddWarning(Res.GetString("75ca649e-0994-42a9-acd9-916e7b1d945b", "This entry has no corresponding Shipments."));
				}
			}

			if (Parent.EAS_ApprovalCategory == AviationSecuritySchemeMembership.Codes.RegulatedAgent)
			{
				parent.EAS_ApprovalCategoryInfo.AddWarning(Res.GetString("f1cbeff0-9901-42de-8c34-b39fce26f508", "Ensure this RA is the screening organization of at least one linked Shipment. Their details will be sent in the message with the 'OSS' identifier."));
			}

			if (Parent.ApprovalCategoryList != null)
			{
				if (Parent.ApprovalCategoryList.Count != 0 || !Parent.EAS_ApprovalCategory.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(parent.EAS_ApprovalCategoryInfo);
				}
			}

			if (parent.EAS_ApprovalCategory.IsEmpty)
			{
				if (parent.EAS_ApprovalNumber.IsEmpty && (Parent.ApprovalCategoryList?.Count ?? 0) != 0)
				{
					parent.EAS_ApprovalCategoryInfo.AddError(Res.GetString("730ca11d-c7a2-4941-8eea-0496f312fbd4", "You have to enter Known Type or Approval Number."));
				}
				else if ((Parent.ApprovalCategoryList?.Count ?? 0) != 0)
				{
					parent.EAS_ApprovalCategoryInfo.AddMessageError(Res.GetString("dc013d24-7724-421e-afa4-7ed9f0a206c0", "Enter a Known Type - AC, KC or RA."));
				}

				return;
			}

			if (!parent.EAS_ApprovalCategoryInfo.HasErrors()
				&& parent.Master != null
				&& parent.Master.Consol != null
				&& parent.Master.Consol.IsDirect
				&& parent.Master.CargoSecurityKnownShippers.Skip(1).Any())
			{
				parent.EAS_ApprovalCategoryInfo.AddMessageError(Res.GetString("6a69b320-d7f1-4992-af1b-a7ca15110bec", "Only one entry is allowed for direct consolidations."));
			}

			if (!parent.EAS_ApprovalCategoryInfo.HasErrors()
				&& parent.Master != null
				&& parent.Master.CargoSecurityKnownShippers.Any())
			{
				var hasDuplicate = parent
					.Master
					.CargoSecurityKnownShippers
					.Cast<ExportAWBSecurityStatusLine>()
					.Any(elem => elem != parent
						&& elem.EAS_ApprovalCategory == parent.EAS_ApprovalCategory
						&& elem.EAS_ApprovalNumber == parent.EAS_ApprovalNumber);

				if (hasDuplicate)
				{
					parent.EAS_ApprovalCategoryInfo.AddMessageError(Res.GetString("bd2082cc-20fd-444f-82b1-86390ae4beaa", "Duplicate entries are not allowed."));
				}
			}

			ValidateRegulatedAgentApprovalNumber(parent.EAS_ApprovalCategoryInfo);
		}

		#endregion

		#region EAS_RN_NKCountryCode

		protected override void CheckEAS_RN_NKCountryCode()
		{
			if (Parent.Type != SecurityStatusLineType.KnownConsignor)
			{
				return;
			}

			ListValidation.ErrorIfInvalidCode(Parent.EAS_RN_NKCountryCodeInfo);

			if (Parent.EAS_RN_NKCountryCode.IsEmpty)
			{
				Parent.EAS_RN_NKCountryCodeInfo.AddWarning(Res.GetString("bb619521-6b54-49f5-8d65-cac5074cda9d", "Country should be entered before type."));
			}

			base.CheckEAS_RN_NKCountryCode();
		}

		#endregion

		#region EAS_ApprovalNumber

		protected override void CheckEAS_ApprovalNumber()
		{
			var parent = Parent;

			if (!SupplyChainSecurityConfiguration.New().UseConsignmentSecurityDeclaration)
			{
				return;
			}

			if (parent.Type != SecurityStatusLineType.KnownConsignor)
			{
				return;
			}

			if (parent.Master == null)
			{
				return;
			}

			var countryCode = parent.Master.EH_RN_NKAgentApprovalCountryCode;
			var config = SupplyChainSecurityConfiguration.New(countryCode);

			if (parent.EAS_ApprovalNumber.IsEmpty)
			{
				var errorMessage = string.Empty;

				if (parent.Organization == null && parent.Shipments.Any())
				{
					if (parent.Shipments.Count <= 3)
					{
						errorMessage = Res.GetString("73309e39-b9ea-4d8f-80fd-ed242352eb5a",
							"Enter the Approval Number of the Known Party or add Consignor to {0} and configure Supply Chain Security.",
							string.Join(", ", parent.Shipments.Select(s => s.JS_UniqueConsignRef)));
					}
					else
					{
						errorMessage = Res.GetString("923bebf2-f332-4e22-ac72-b85d15d4a04c",
							"Enter the Approval Number of the Known Party or add Consignor to {0} and {1} others and configure Supply Chain Security.",
							string.Join(", ", parent.Shipments.Take(2).Select(s => s.JS_UniqueConsignRef)),
							parent.Shipments.Count - 2);
					}
				}
				else if (parent.Organization != null)
				{
					errorMessage = Res.GetString("402fd2f7-8a41-4a26-8935-7f0858713979",
						"Enter the Approval Number of the Known Party or update Supply Chain Security on {0}.",
						parent.Organization.OH_FullName);
				}
				else
				{
					errorMessage = Res.GetString("3c847a54-822a-4f1c-ad12-f8682a403a40", "Enter the Approval Number of the Known Party.");
				}

				parent.EAS_ApprovalNumberInfo.AddMessageError(errorMessage);
			}

			if (parent.EAS_ApprovalNumber.IsEmpty && (Parent.ApprovalCategoryList?.Count ?? 0) == 0)
			{
				parent.EAS_ApprovalNumberInfo.AddError(Res.GetString("e4ab4521-9392-4419-94d8-70a30b570454", "You have to enter Approval Number (Code)."));
			}

			ValidateRegulatedAgentApprovalNumber(parent.EAS_ApprovalNumberInfo);
		}

		#endregion

		#region EAS_ApprovalExpiryDate

		protected override void CheckEAS_ApprovalExpiryDate()
		{
			if (Parent.Type != SecurityStatusLineType.KnownConsignor)
			{
				return;
			}

			base.CheckEAS_ApprovalExpiryDate();

			if (Parent.EAS_ApprovalExpiryDate.IsValid)
			{
				var consol = Parent.Master?.Consol;
				if (consol != null
					&& consol.Shipments.Any()
					&& consol.Shipments.Cast<ForwardingShipment>().First().AviationSecurity.SupplyChainSecurityConfiguration.IsEnabled)
				{
					AddErrorForApprovalExpiryWillHaveExpired();
					AddWarningIfDateDoesNotMatchOrgApproval();
				}
				else
				{
					AddErrorForApprovalExpiryDateHasPast();
				}
			}
		}

		void AddErrorForApprovalExpiryWillHaveExpired()
		{
			var consol = Parent.Master?.Consol;
			if (consol != null)
			{
				var firstShipment = consol.Shipments.Cast<ForwardingShipment>().OrderBy(s => s.AviationSecurity.ShipmentDateForAviationSecurity).FirstOrDefault();
				if (firstShipment != null && firstShipment.AviationSecurity.ShipmentDateForAviationSecurity > Parent.EAS_ApprovalExpiryDate)
				{
					var dateType = firstShipment.AviationSecurity.ShipmentDateTypeForAviationSecurity;
					if (dateType == AviationSecuritySupport.AviationSecurityDateTypes.CurrentDate)
					{
						AddErrorForApprovalExpiryDateHasPast();
					}
					else
					{
						var dateTypeDescription = ZString.Empty;
						switch (dateType)
						{
							case AviationSecuritySupport.AviationSecurityDateTypes.MasterBillIssueDate:
								dateTypeDescription = Res.GetString("fca84af9-f76b-431c-8741-f10783dce309", "master bill issue date");
								break;
							case AviationSecuritySupport.AviationSecurityDateTypes.ConsolETD:
								dateTypeDescription = Res.GetString("1c5893bb-a18f-429a-b4f1-787be23595d1", "consol ETD");
								break;
							case AviationSecuritySupport.AviationSecurityDateTypes.ShipmentETD:
								dateTypeDescription = Res.GetString("36a5e380-eeb0-449e-846c-4c6fde2698fb", "ETD of {0}", firstShipment.HumanReadableName);
								break;
							case AviationSecuritySupport.AviationSecurityDateTypes.ShipmentCreatedDate:
								dateTypeDescription = Res.GetString("cffa85ec-8115-4855-a4de-6f9f1381392a", "creation date of {0}", firstShipment.HumanReadableName);
								break;
						}

						if (Parent.EAS_ApprovalExpiryDate.IsInThePastDatePartOnly)
						{
							Parent.EAS_ApprovalExpiryDateInfo.AddError(Res.GetString("59d2d16d-b5f7-47e1-8951-e3761cf654c6", "This organization's known status expired prior to the {0}.", dateTypeDescription));
						}
						else
						{
							Parent.EAS_ApprovalExpiryDateInfo.AddError(Res.GetString("f5f3c4e6-2ba1-4250-8804-31db69caa65d", "This organization's known status will expire prior to the {0}.", dateTypeDescription));
						}
					}
				}
			}
		}

		void AddErrorForApprovalExpiryDateHasPast()
		{
			if (Parent.EAS_ApprovalExpiryDate < ZDate.Today
				&& (Parent.EAS_ApprovalExpiryDateInfo.HasChanges || !Parent.IsInDatabase))
			{
				Parent.EAS_ApprovalExpiryDateInfo.AddError(Res.GetString("0dff1520-9dcd-4076-ba3b-eac05d1344f7", "This organization's known status has expired."));
			}
		}

		void AddWarningIfDateDoesNotMatchOrgApproval()
		{
			var header = Parent.Master as ConsolExportAWBHeader;
			if (header != null)
			{
				var permit = header.ExtractPermitDetails(Parent.Shipments)
					.FirstOrDefault(p => p.Type == Parent.EAS_ApprovalCategory
						&& p.ApprovalCountry == Parent.EAS_RN_NKCountryCode
						&& p.Number == Parent.EAS_ApprovalNumber
						&& p.ExpiryDate != Parent.EAS_ApprovalExpiryDate);
				if (permit != null)
				{
					Parent.EAS_ApprovalExpiryDateInfo.AddWarning(Res.GetString("1cb2ef0d-f67e-49b2-bb3d-e619e1705060", "This date has been manually added here so does not reflect that saved against the organization."));
				}
			}
		}

		#endregion

		#region EAS_ScreeningMethod

		protected override void CheckEAS_ScreeningMethod()
		{
			base.CheckEAS_ScreeningMethod();

			var parent = Parent;

			if (parent.Type != SecurityStatusLineType.ScreeningMethod
				|| parent.EAS_ScreeningMethodInfo.HasNotifications())
			{
				return;
			}

			if (parent.EAS_ScreeningMethod != FreightDataRegistry.AviationSecurity_Unknown_Code)
			{
				var screeningMethodList = ShipmentInspectionTypeLists.GetCountrySpecificScreeningMethods(GlbCompany.CurrentCompany.Country.Code);
				if (!parent.EAS_ScreeningMethod.IsEmpty && !screeningMethodList.ContainsCode(parent.EAS_ScreeningMethod))
				{
					parent.EAS_ScreeningMethodInfo.AddMessageError(
						 Res.GetString("efa2a1c0-2c23-4efb-b8b5-8e4fed61bd9e", "{0} - is an invalid IATA code.", parent.EAS_ScreeningMethod));
				}
				return;
			}

			var consol = parent.Master != null
				? parent.Master.Parent as ForwardingConsol
				: null;

			if (consol == null)
			{
				return;
			}

			if (consol.IsDirect)
			{
				parent.EAS_ScreeningMethodInfo.AddMessageError(Res.GetString("06dda3e5-5376-45ce-ae6e-4ad87d50d6f1", "For Direct Consolidation this should either be a Known Consignor, Account Consignor or Regulated Agent or the Screening Method or Grounds for Exemption should be specified."));
			}
			else
			{
				if (consol.IsAWBHeaderAccessible && consol.SupplyChainSecurityConfiguration.IsPackLevelScreeningRequired(consol))
				{
					consol.SupplyChainSecurityConfiguration.CheckEAS_ScreeningMethod_PackLevelScreeningValidation(parent, consol);
				}
				else
				{
					var unsecuredShipments = consol
						.Shipments
						.Cast<ForwardingShipment>()
						.Where(shipment => shipment.IsFHLShipment()
							&& shipment.AviationSecurity.HasUnknownInspectionTypeCode)
						.Select(shipment => shipment.JS_UniqueConsignRef)
						.Take(3);

					parent.EAS_ScreeningMethodInfo.AddMessageError(Res.GetString("6ea22a7b-2db8-444f-9a2f-b2461cb66d76", "Shipment/s {0} have not been secured (or there is no Screening Method or Grounds for Exemption) and their Consignors are not approved to screen air cargo.", string.Join(", ", unsecuredShipments)));
				}
			}
		}

		#endregion

		#region EAS_ExemptionGround

		protected override void CheckEAS_ExemptionGround()
		{
			base.CheckEAS_ExemptionGround();

			var parent = Parent;
			if (!parent.EAS_ExemptionGround.IsEmpty && !ShipmentInspectionType.IsIATAExemptionCode(parent.EAS_ExemptionGround))
			{
				parent.EAS_ExemptionGroundInfo.AddMessageError(
					 Res.GetString("efa2a1c0-2c23-4efb-b8b5-8e4fed61bd9e", "{0} - is an invalid IATA code.", parent.EAS_ExemptionGround));
			}
		}

		#endregion

		#region Implementation

		void ValidateRegulatedAgentApprovalNumber(ZPropertyInfo info)
		{
			if (Parent.Master != null && !Parent.Master.EH_AgentApprovalNumber.IsEmpty)
			{
				if (Parent.EAS_ApprovalCategory == AviationSecuritySchemeMembership.Codes.RegulatedAgent
					&& !Parent.EAS_ApprovalNumber.IsEmpty
					&& Parent.EAS_ApprovalNumber == Parent.Master.EH_AgentApprovalNumber)
				{
					info.AddMessageError(Res.GetString("ef696128-64fe-4566-9fea-9aa6b254eadb", "The Approval Number of the Known Party is same as Regulated Agent Identifier."));
				}
			}
		}

		#endregion
	}
}
