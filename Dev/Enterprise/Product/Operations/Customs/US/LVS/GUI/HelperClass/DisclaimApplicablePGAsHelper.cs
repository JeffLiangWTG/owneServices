using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.GUI
{
	public static class DisclaimApplicablePGAsHelper
	{
		public static string DisclaimApplicablePGAsMenuCaption => Res.GetString("5175ee4d-9aec-45d7-9f42-267e3a1ec812", "Disclaim Applicable PGAs");

		public static ZMenuItem AddDisclaimApplicablePGAsMenuItem(EventHandler disclaimApplicablePGAsEventHandler)
		{
			return new ZMenuItem(DisclaimApplicablePGAsMenuCaption, disclaimApplicablePGAsEventHandler) { Name = "DisclaimApplicablePGAsMenuCaption" };
		}

		public static void ApplyDisclaimReasonsToConsignments(IOperationalActionSectionLog log, CusUSLVItemPGADisclaimOptionCollection disclaimOptions, BusinessObject[] targets)
		{
			var options = disclaimOptions.ToDictionary();
			if (options.Count > 0)
			{
				foreach (var target in targets)
				{
					var consignment = target as CusUSLVConsignment;
					if (consignment == null && target is USConsignmentCombined consignmentCombined)
					{
						consignment = consignmentCombined.Consignment;
					}

					if (consignment != null)
					{
						var reason = GetReasonToSkipApplyConsignment(consignment);
						if (string.IsNullOrEmpty(reason))
						{
							foreach (CusUSLVItem item in consignment.CusUSLVItems)
							{
								foreach (CusUSLVItemPGAWrapper pgaWrapper in item.ItemPGAWrapperCollection)
								{
									var program = pgaWrapper.AgencyCode;
									if (options.ContainsKey(program) && item.PGARequirementIndicator.HasPGAProgram(program))
									{
										if (item.PGARequirementIndicator.IsPGAProgramMayRequired(program))
										{
											var disclaimReasonCode = options[program];
											if (pgaWrapper.PGADisclaimReasonList.ContainsCode(disclaimReasonCode))
											{
												pgaWrapper.DisclaimReason = disclaimReasonCode;
											}
											else
											{
												log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("5f4084ca-8077-4ca2-beaf-ca674e87399a", "House Bill {0}: For tariff ({1}), {2} is not a valid disclaim reason code of {3}.", item.Consignment.ULB_HouseBill, item.ULI_Tariff, disclaimReasonCode, pgaWrapper.AgencyCode));
											}
										}
										else
										{
											log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("9f6acffa-aab6-4dd3-a486-39cb38848151", "House Bill {0}: For tariff ({1}), {2} is flagged as 'Must Be Declared', disclaim reason code is generally not allowed.", item.Consignment.ULB_HouseBill, item.ULI_Tariff, pgaWrapper.AgencyCode));
										}
									}
								}
							}
						}
						else
						{
							log.Notify(OperationalActionLogErrorLevel.Warning, reason);
						}
					}
				}
			}
		}

		static string GetReasonToSkipApplyConsignment(CusUSLVConsignment consignment)
		{
			var reason = string.Empty;
			if (!consignment.CE_EntryLineReference.IsEmpty)
			{
				reason = Res.GetString("165b32fa-57c0-4577-a7f6-d7ad01c3f3f1", "House Bill {0}: PGA disclaim reason cannot be applied to this consignment because this consignment has been converted to customs declaration.", consignment.ULB_HouseBill);
			}
			else if (consignment.Messages.Count > 0)
			{
				reason = Res.GetString("5e5b7199-579a-4379-8071-5e4155dc6ee4", "House Bill {0}: PGA disclaim reason cannot be applied to this consignment because Cargo Release transaction exists.", consignment.ULB_HouseBill);
			}
			return reason;
		}
	}
}
