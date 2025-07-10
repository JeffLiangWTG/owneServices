using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public interface IPGADataCorrection : IPGALineStatus
	{
		bool SettingStatusInProgress { get; set; }
		bool SuspendTrackingStatusChange { get; }
		event EventHandler<HasChangesChangedEventArgs> HasChangesChanged;
		JobComInvoiceLine InvoiceLine { get; }
		ZPropertyInfo TrackingStatusInfo { get; }

		string[] GetIndicatorFields();
		string[] GetDislaimReasonFields();

		string[] GetRelatedInvoiceLineFields();

		string[] GetRelatedInvoiceFields();

		string[] GetRelatedContainerFields();

		string[] GetRelatedDeclarationFields();
	}

	public interface IPGADataCorrectionCollection
	{
		BusinessObject Master { get; }
		bool AllowAddNewPGALines { get; set; }
		IEnumerable<IPGADataCorrection> CorrectionItems { get; }
	}

	public static class IPGADataCorrectionCollectionExtension
	{
		public static ZDateTime GetReleaseDate(this IPGADataCorrectionCollection correctionCollection)
		{
			var result = ZDateTime.Empty;
			if (correctionCollection.Master is JobComInvoiceLine invoiceLine)
			{
				result = invoiceLine.InvoiceHeader?.JobDeclaration?.JE_EntryAuthorisationDate ?? ZDateTime.Empty;
			}
			return result;
		}

		public static ZBool HasSpecificPGALines(this IPGADataCorrectionCollection correctionCollection, ZString agenciesCode)
		{
			return correctionCollection?.CorrectionItems?.HasSpecificPGALines(agenciesCode) ?? false;
		}
	}

	public static class IPGADataCorrectionExtension
	{
		public static ZBool HasSpecificPGALines(this IEnumerable<IPGADataCorrection> dataCorrections, ZString agenciesCode)
		{
			return dataCorrections.Any(x => x.PGALineStatusAgencyCode == agenciesCode);
		}
	}

	public static class PGADataCorrectionSupporter
	{
		public static bool IsDeletedOrBeingDeleted(this IPGADataCorrection supporter)
		{
			var bizObj = supporter as BusinessObject;
			var result = bizObj != null;
			if (result)
			{
				result = bizObj.IsDeleted || bizObj.IsDeleting;
				if (!result)
				{
					result = PGATrackingStatusList.IsDeletedOrBeingDeleted((ZString)supporter.TrackingStatusInfo.Value);
				}
			}
			return result;
		}

		public static void MarkStatusAsSubmittingToCustoms(this IPGADataCorrection supporter, bool shouldBeDeclared)
		{
			if (supporter != null)
			{
				var info = supporter.GetTrackingStatusInfo();
				var status = info.Value;
				switch (status)
				{
					case PGATrackingStatusList.Codes.ToBeUpdated:
					case PGATrackingStatusList.Codes.Added:
					case PGATrackingStatusList.Codes.Updating:
						if (shouldBeDeclared)
						{
							info.Value = PGATrackingStatusList.Codes.Updating;
						}
						break;
					case PGATrackingStatusList.Codes.ToBeDeleted:
						info.Value = PGATrackingStatusList.Codes.Deleting;
						break;
					case "":
						if (shouldBeDeclared)
						{
							info.Value = PGATrackingStatusList.Codes.Adding;
						}
						break;
				}
			}
		}

		public static bool AreAllLinesToBeDeleted(this IEnumerable<IPGADataCorrection> pgaLines)
		{
			return pgaLines != null && pgaLines.Any() && !pgaLines.Any(x => !x.TrackingStatusInfo.Value.Equals(PGATrackingStatusList.Codes.ToBeDeleted));
		}

		public static bool HasLineToBeAmended(this IEnumerable<IPGADataCorrection> pgaLines)
		{
			return pgaLines != null && pgaLines.Any(line => PGALinesForPGACorrection.NeedToAmend(line));
		}

		public static bool HasTrackingIDChanged(this IPGADataCorrection supporter)
		{
			var result = false;
			var invoiceLine = supporter?.InvoiceLine;
			if (invoiceLine != null)
			{
				result = HasTrackingIDChanged(invoiceLine);
				if (!result)
				{
					result = invoiceLine.SecondaryTariffLines.Any(x => HasTrackingIDChanged(x));
				}
			}
			return result;
		}

		static bool HasTrackingIDChanged(JobComInvoiceLine invoiceLine)
		{
			var result = false;
			if (invoiceLine != null)
			{
				var currentTrackingID = GenerateTrackingID(invoiceLine);
				var previousTrackingID = invoiceLine.GetTrackingID();
				if (!(currentTrackingID == EmptyTrackingID && previousTrackingID.IsEmpty))
				{
					result = currentTrackingID != invoiceLine.GetTrackingID();
				}
			}
			return result;
		}
		const string EmptyTrackingID = "_0__0_0";

		static ZString GenerateTrackingID(JobComInvoiceLine invoiceLine)
		{
			var messageType = invoiceLine.Declaration != null && invoiceLine.Declaration.US_PGAExpeditedRelease ? CusEntryHeaderMessageTypeList.Codes.EntrySummary : CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			var entryLine = invoiceLine.GetEntryLineFor(messageType, false);
			var supTariff = invoiceLine.US_SupTariff;
			var supLine = entryLine == null || supTariff.IsEmpty || supTariff == TariffViewAsCodeDescription.NotApplicableCode ? null : invoiceLine.GetEntryLineFor(messageType, true);
			return ZString.Format("{0}_{1}_{2}_{3}_{4}", invoiceLine.JI_Tariff, entryLine?.CL_LineNumber ?? ZShort.Zero, supTariff, supLine?.CL_LineNumber ?? ZShort.Zero, invoiceLine.SecondaryTariffLines.Count());
		}

		public static void ClearTrackingID(this JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_PGATrackingID = ZString.Empty;
		}

		public static ZString GetTrackingID(this JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.US_PGATrackingID;
		}

		public static void SetTrackingID(this JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_PGATrackingID = GenerateTrackingID(invoiceLine);
		}

		public static bool AllowAddNewLineToPGACollection(this JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine == null || invoiceLine.IsExport)
			{
				return true;
			}

			if (invoiceLine.Declaration is JobDeclaration declaration)
			{
				var entryHeader = declaration.GetEntryHeaderForPGACorrection();
				if (entryHeader != null && entryHeader.HasBeenLodgedAtCustoms)
				{
					return false;
				}
			}

			if (!invoiceLine.GetTrackingID().IsEmpty)
			{
				return false;
			}

			return true;
		}

		public static void RemoveTrackingID(this JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_PGATrackingID = ZString.Empty;
		}

		public static void OnStatusUpdated(this IPGADataCorrection supporter, ZString oldValue, ZString newValue)
		{
			if (oldValue != newValue)
			{
				var wasTracking = PGATrackingStatusList.ShouldTrack(oldValue);
				var shouldTracking = PGATrackingStatusList.ShouldTrack(newValue);
				var invoiceLine = supporter?.InvoiceLine;
				if (wasTracking != shouldTracking)
				{
					if (invoiceLine != null)
					{
						var helper = invoiceLine.Declaration?.PGATrackerHelper;
						if (helper != null)
						{
							if (shouldTracking)
							{
								helper.RegisterTracker(supporter, invoiceLine);
							}
							else
							{
								helper.UnRegisterTracker(supporter, invoiceLine);
							}
						}
					}
				}

				if (newValue == PGATrackingStatusList.Codes.ToBeDeleted || newValue == PGATrackingStatusList.Codes.ToBeUpdated)
				{
					invoiceLine?.Declaration?.UpdatePGADataReplacementUpdateRequired();
				}

				if (supporter != null)
				{
					var bizObj = supporter as BusinessObject;
					if (bizObj != null)
					{
						var readOnly = bizObj.ReadOnly;
						bizObj.SetReadOnlyIncludingChildren(supporter.IsPGALineReadOnly());
						if (readOnly != bizObj.ReadOnly)
						{
							bizObj.RefreshBindingIncludingChildren();
						}
					}
				}
			}
		}

		static ZPropertyInfoString GetTrackingStatusInfo(this IPGADataCorrection supporter)
		{
			var trackingStatusInfo = supporter.TrackingStatusInfo;
			var wrappedInfo = trackingStatusInfo as ZWrappedPropertyInfo;
			var info = (ZPropertyInfoString)(wrappedInfo?.InnerInfo ?? trackingStatusInfo);
			return info;
		}

		public static void RelatedDataChanged(this IPGADataCorrection supporter)
		{
			if (supporter != null && !supporter.SettingStatusInProgress && !supporter.SuspendTrackingStatusChange)
			{
				var info = supporter.GetTrackingStatusInfo();
				var status = info.Value;
				if (status == PGATrackingStatusList.Codes.Added)
				{
					try
					{
						supporter.SettingStatusInProgress = true;
						info.Value = PGATrackingStatusList.Codes.ToBeUpdated;
					}
					finally
					{
						supporter.SettingStatusInProgress = false;
					}
				}
			}
		}

		public static void RegisterTrackerIfNeeded(this IPGADataCorrection supporter)
		{
			if (supporter != null)
			{
				var status = (ZString)supporter.TrackingStatusInfo.Value;
				if (PGATrackingStatusList.ShouldTrack(status))
				{
					var invoiceLine = supporter?.InvoiceLine;
					if (invoiceLine != null)
					{
						var helper = invoiceLine.Declaration?.PGATrackerHelper;
						if (helper != null)
						{
							helper.RegisterTracker(supporter, invoiceLine);
						}
					}
				}
			}
		}

		public static void UnRegisterTrackerIfNeeded(this IPGADataCorrection supporter)
		{
			if (supporter != null)
			{
				var status = (ZString)supporter.TrackingStatusInfo.Value;
				if (PGATrackingStatusList.ShouldTrack(status))
				{
					var invoiceLine = supporter?.InvoiceLine;
					if (invoiceLine != null)
					{
						var helper = invoiceLine.Declaration?.PGATrackerHelper;
						if (helper != null)
						{
							helper.UnRegisterTracker(supporter, invoiceLine);
						}
					}
				}
			}
		}

		public static bool CanBeChangedToBeUpdated(this IPGADataCorrection supporter)
		{
			var status = supporter == null ? ZString.Empty : (ZString)supporter.TrackingStatusInfo.Value;
			return PGATrackingStatusList.CanBeChangedToBeUpdated(status);
		}

		public static bool IncludedInMessage(this IPGADataCorrection supporter)
		{
			var status = supporter == null ? ZString.Empty : (ZString)supporter.TrackingStatusInfo.Value;
			return status != PGATrackingStatusList.Codes.Deleted && status != PGATrackingStatusList.Codes.ToBeDeleted && status != PGATrackingStatusList.Codes.Deleting;
		}

		public static bool IsPGALineReadOnly(this IPGADataCorrection supporter)
		{
			var status = supporter == null ? ZString.Empty : (ZString)supporter.TrackingStatusInfo.Value;
			return status != ZString.Empty && status != PGATrackingStatusList.Codes.ToBeUpdated;
		}

		public static void SetTrackStatusAfterMessageIsLodged(this IPGADataCorrection supporter)
		{
			if (supporter != null)
			{
				var status = (ZString)supporter.TrackingStatusInfo.Value;
				if (status == PGATrackingStatusList.Codes.Adding || status == PGATrackingStatusList.Codes.Updating)
				{
					supporter.TrackingStatusInfo.Value = (ZString)PGATrackingStatusList.Codes.Added;
				}
				else if (status == PGATrackingStatusList.Codes.Deleting)
				{
					supporter.TrackingStatusInfo.Value = (ZString)PGATrackingStatusList.Codes.Deleted;
				}
			}
		}

		public static void SetTrackStatusAfterDeletionMessageIsAccepted(this IPGADataCorrection supporter)
		{
			if (supporter != null)
			{
				supporter.TrackingStatusInfo.Value = ZString.Empty;
			}
		}

		public static void SetTrackStatusAfterMessageFailure(this IPGADataCorrection supporter)
		{
			if (supporter != null)
			{
				var info = supporter.GetTrackingStatusInfo();
				var status = info.Value;
				switch (status)
				{
					case PGATrackingStatusList.Codes.Updating:
						info.Value = PGATrackingStatusList.Codes.ToBeUpdated;
						break;
					case PGATrackingStatusList.Codes.Deleting:
						info.Value = PGATrackingStatusList.Codes.ToBeDeleted;
						break;
					case PGATrackingStatusList.Codes.Adding:
						info.Value = "";
						break;
				}
			}
		}

		public static void SetTrackStatusAfterSOMessageIsRejected(this IPGADataCorrection supporter)
		{
			if (supporter != null)
			{
				var trackingStatusInfo = supporter.GetTrackingStatusInfo();
				var currentStatus = trackingStatusInfo.Value;
				switch (currentStatus)
				{
					case PGATrackingStatusList.Codes.Added:
						trackingStatusInfo.Value = PGATrackingStatusList.Codes.ToBeUpdated;
						break;
					case PGATrackingStatusList.Codes.Deleted:
						trackingStatusInfo.Value = PGATrackingStatusList.Codes.ToBeDeleted;
						break;
				}
			}
		}

		public static void UpdatePGADataReplacementUpdateRequired(this JobDeclaration declaration)
		{
			if (declaration != null && declaration.IsPGAEntryHasBeenLodgedAtCustoms)
			{
				declaration.US_PGAReplaceUpdateNeeded = YesNoList.Codes.Yes;
			}
		}

		public static bool PGALinesCanBeDeleted(this IPGADataCorrection tracker)
		{
			return tracker != null && tracker.TrackingStatusInfo.Value.IsEmpty;
		}
	}
}
