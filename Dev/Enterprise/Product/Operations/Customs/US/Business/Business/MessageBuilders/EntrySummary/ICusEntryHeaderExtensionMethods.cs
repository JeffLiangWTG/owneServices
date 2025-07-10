// -----------------------------------------------------------------------
// <copyright file="ICusEntryHeaderExtensionMethods.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business
{
	public static class ICusEntryHeaderExtensionMethods
	{
		internal static ZString GetSinglePGAEntryStatus(this JobDeclaration declaration, ZString pgaCode)
		{
			var result = declaration.GetPGAEntryStatus();
			var status = ZString.Empty;
			result.TryGetValue(pgaCode, out status);
			return status;
		}

		public static bool IsCottonFeeDeMinimusApplicable(this JobDeclaration declaration)
		{
			return IsCottonFeeDeMinimusApplicable(declaration.IsACE);
		}

		public static bool IsCottonFeeDeMinimusApplicable(this ReconOriginalEntryHeader originalReconEntry)
		{
			return !originalReconEntry.IsACE;
		}

		static bool IsCottonFeeDeMinimusApplicable(bool isACE)
		{
			return !isACE;
		}

		public static bool IsBillDetailRequired(this MessageBuilders.ICusEntryHeader entryHeader)
		{
			return IsBillDetailRequiredCore(entryHeader.EntryType, entryHeader.ModeOfTransportationCode);
		}

		public static bool IsBillDetailRequired(this JobDeclaration declaration)
		{
			return IsBillDetailRequiredCore(declaration.US_EntryType, declaration.JE_Calc_USTransportMode);
		}

		public static bool ShouldForceFilingInACECargoRelease(this JobDeclaration declaration)
		{
			var isDeclarationLodged = declaration.FormalEntry != null && declaration.FormalEntry.HasBeenLodgedAtCustoms;
			return !isDeclarationLodged && !declaration.US_EntryType.IsEmpty && EntryTypeList.ShouldBeFiledInACE(declaration.US_EntryType);
		}

		static bool IsBillDetailRequiredCore(ZString entryType, ZString modeOfTransportationCode)
		{
			return !EntryTypeList.IsExWarehouseType(entryType);
		}

		/// <summary>
		/// Only for ACS declarations or ACE declarations certified in ACS.
		/// </summary>
		public static void UpdateAndLogFDAStatusDetails(this CusEntryHeader entry, ZString messageType, ZString? previousFDAStatus = null)
		{
			if (entry != null)
			{
				var declaration = entry.Declaration;
				if (!previousFDAStatus.HasValue || previousFDAStatus.Value != declaration.FDAStatus)
				{
					string fdaStatusReference = entry.Declaration != null && !entry.Declaration.FDAStatus.IsEmpty ? " - FDA " + entry.Declaration.FDAStatus : "";
					if (!string.IsNullOrEmpty(fdaStatusReference))
					{
						ISimplifiedMessageLinkedObjectExtensionMethod.LogMessageStatusChangeEventAgainstTopLevelBusinessObject(entry, messageType + fdaStatusReference);
					}
				}
			}
		}

		internal static Dictionary<ZString, ZString> GetPGAEntryStatus(this JobDeclaration declaration)
		{
			var result = new Dictionary<ZString, ZString>();
			foreach (CusDisposition cusDisposition in declaration.EntryPGACusDispositions)
			{
				result[cusDisposition.CDI_StatusKey] = cusDisposition.CDI_Status;
			}
			return result;
		}

		internal static void LogPGAEntryStatus(this JobDeclaration declaration, Dictionary<ZString, ZString> previousPGAEntryStatus, string messageType)
		{
			foreach (CusDisposition pgaDisposition in declaration.EntryPGACusDispositions)
			{
				var previousStatus = ZString.Empty;
				previousPGAEntryStatus.TryGetValue(pgaDisposition.CDI_StatusKey, out previousStatus);
				if (previousStatus == ZString.Empty || previousStatus != pgaDisposition.CDI_Status)
				{
					var reference = ZString.Format("{0} - PGA {1} {2}", messageType, pgaDisposition.CDI_StatusKey, pgaDisposition.CDI_Status);
					declaration.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.MessageStatusChange, reference, DateTimeParser.GetFromJobBranchCurrentTime(declaration.Branch), null);
				}
			}
		}

		internal static void LogPGALineStatus(this JobDeclaration declaration, IEnumerable<IPGADispositionProvider> dispositionProviders, string messageType)
		{
			var statusMapping = new Dictionary<ZString, List<string>>();

			foreach (IPGADispositionProvider so70 in dispositionProviders)
			{
				if (!so70.OtherAgencyQuotaIdentifier.IsEmpty && !so70.PGALineDispositionCode.IsEmpty)//PGALineDispositionCode
				{
					List<string> eventReference;
					if (!statusMapping.TryGetValue(so70.OtherAgencyQuotaIdentifier, out eventReference))
					{
						eventReference = new List<string>();
					}
					if (!eventReference.Contains(so70.PGALineDispositionCode))
					{
						eventReference.Add(so70.PGALineDispositionCode);
						statusMapping[so70.OtherAgencyQuotaIdentifier] = eventReference;
					}
				}
			}

			foreach (var element in statusMapping)
			{
				var reference = ZString.Format("{0} - PGA Line Status {1} {2}", messageType, element.Key, new ZStringBuilder(element.Value).ToStringWithDelimiterBetweenAppends(", "));
				declaration.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.MessageStatusChange, reference, DateTimeParser.GetFromJobBranchCurrentTime(declaration.Branch), null);
			}
		}

		public static ZBool ShouldCertifyCargoReleaseEnabledFromEntrySummary(this CusEntryHeader entry)
		{
			var result = false;
			var declaration = entry.Declaration;
			if (declaration != null && declaration.IsImport && declaration.US_EnableENS && declaration.US_CertifyCargoRelease)
			{
				result = declaration.JE_EntryAuthorisationDate.IsEmpty && declaration.ReleaseStatus != CRLReleaseStatusList.Codes.REL && !entry.HasCargoReleaseBeenCertified && !entry.IsCargoReleaseBeingCertified;
			}

			return result;
		}
	}
}
