using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;

namespace Enterprise.Customs.US.Business
{
	public static class CargoManifestStatusQuerySender
	{
		public static void MarkForAutoSendingIfEligible(JobDeclaration declaration, ZBool shouldHeld)
		{
			var queryRegistry = USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, declaration.RegistryBranchPK, Guid.Empty);
			if (ShouldSendQueries(queryRegistry, declaration, shouldHeld) && (!shouldHeld || declaration.Messages.Cast<MQEDIMessage>().All(m => m.EM_HeldUntilDate == ZDateTime.Empty)))
			{
				var updateEntryWithResults = !shouldHeld && queryRegistry.UpdateEntryWithResults;
				var limitOutputOption = !shouldHeld ? LimitOutputCodeList.Codes._0MostRecentResults : LimitOutputCodeList.Codes._2AllAvailableResults;
				GenerateCargoManifestQuery(declaration, updateEntryWithResults, limitOutputOption);
				if (shouldHeld)
				{
					UpdateHeldUntilDateInHeldQueries(declaration, requireValidDate: false);
				}
			}
		}

		public static void UpdateHeldUntilDateIfEligible(JobDeclaration declaration, ZBool shouldHeld)
		{
			var queryRegistry = USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, declaration.RegistryBranchPK, Guid.Empty);
			if (ShouldSendQueries(queryRegistry, declaration, shouldHeld))
			{
				UpdateHeldUntilDateInHeldQueries(declaration, requireValidDate: true);
			}
		}

		static bool ShouldSendQueries(AutoQueryBillOfLadingCargoManifestStatus queryRegistry, JobDeclaration declaration, ZBool shouldHeld)
		{
			return (declaration.CargoReleaseStatus == Common.US.ImportMessageStatusList.Codes.ClearACECargoReleaseAdd || declaration.IsFTZAdmission)
					&& (queryRegistry.SendBasedOnETA && shouldHeld)
				|| (queryRegistry.SendOnFirstSave && !shouldHeld);
		}

		static void GenerateCargoManifestQuery(JobDeclaration declaration, ZBool updateEntryWithResults, ZString limitOutputOption)
		{
			if (declaration.IsSea || declaration.IsRail || declaration.IsTruck)
			{
				var sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration, new OceanRailTruckBillQueryFilter());
				sendingHeader.PopulateAndSendQueryMessages(CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill, updateEntryWithResults, limitOutputOption);
			}
			else if (declaration.IsAir)
			{
				var sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration, new AirAutoQueryFilter());
				sendingHeader.PopulateAndSendQueryMessages(CargoManifestStatusQueryActionList.Codes.HAWB, updateEntryWithResults, limitOutputOption);
				sendingHeader.PopulateAndSendQueryMessages(CargoManifestStatusQueryActionList.Codes.MAWB, updateEntryWithResults, limitOutputOption);
			}
		}

		static void UpdateHeldUntilDateInHeldQueries(JobDeclaration declaration, ZBool requireValidDate)
		{
			var date = declaration.JE_DateOfArrival;
			if (!date.IsEmpty)
			{
				var messages = GetHeldQueryMessages(declaration);
				foreach (var message in messages)
				{
					if (!requireValidDate || message.EM_HeldUntilDate.IsValid)
					{
						PopulateHeldUntilDate(message, date);
					}
				}
			}
		}

		static IEnumerable<MQEDIMessage> GetHeldQueryMessages(JobDeclaration declaration)
		{
			return declaration.Messages
							  .OfType<MQEDIMessage>()
							  .Where(m => m.IsCargoManifestQuery && m.EM_Status == MQEDIMessage.Status.Queued);
		}

		static void PopulateHeldUntilDate(MQEDIMessage message, ZDateTime etaDate)
		{
			var now = ZDateTime.Now;
			var timespanNow = new TimeSpan(now.Hour, now.Minute, 0);
			if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.CargoManifestHAWBQuery ||
				message.EM_MessageSubType == EM_MessageSubTypeList.Codes.CargoManifestMAWBQuery)
			{
				message.EM_HeldUntilDate = etaDate.Add(timespanNow);
			}
			else if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.CargoManifestBillOfLadingQuery)
			{
				message.EM_HeldUntilDate = etaDate.AddDays(-1).Add(timespanNow);
			}
		}
	}
}
