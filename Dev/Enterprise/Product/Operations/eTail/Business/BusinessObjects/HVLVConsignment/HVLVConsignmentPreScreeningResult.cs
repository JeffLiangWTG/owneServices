using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using CargoWise.Common;
using Enterprise.eTail.Integration;
using Enterprise.Integration;
using Newtonsoft.Json;
using static Enterprise.Registry.Business.HVLVPreScreeningField;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentPreScreeningResult : IHVLVConsignmentPreScreeningResult
	{
		public HVLVConsignmentPreScreeningResult(HVLVConsignment consignment)
		{
			Consignment = Argument.NotNull(consignment, nameof(consignment));
		}

		public void AddWarningMessage(string message, IGlbGroup notificationGroup = null, bool notifyStaffMember = false)
		{
			PreScreeningWarningList.Add(new PreScreenNotificationDetail(message, notificationGroup, notifyStaffMember, Consignment, ValidationRuleCodes.Warning));
		}

		public void AddErrorMessage(string message, IGlbGroup notificationGroup = null, bool notifyStaffMember = false)
		{
			PreScreeningErrorList.Add(new PreScreenNotificationDetail(message, notificationGroup, notifyStaffMember, Consignment, ValidationRuleCodes.Error));
		}

		public void AddNotifyOnlyWarningMessage(string message, IGlbGroup notificationGroup = null, bool notifyStaffMember = false)
		{
			PreScreeningNotifyOnlyWarningList.Add(new PreScreenNotificationDetail(message, notificationGroup, notifyStaffMember, Consignment, ValidationRuleCodes.Warning));
		}

		public void CombineResponse(IHVLVConsignmentPreScreeningResult other)
		{
			if (Consignment.PK == other.ConsignmentPK)
			{
				PreScreeningWarningList.AddRange(other.PreScreeningWarningDetails);
				PreScreeningErrorList.AddRange(other.PreScreeningErrorDetails);
				PreScreeningNotifyOnlyWarningList.AddRange(other.PreScreeningNotifyOnlyWarningDetails);
			}
		}

		public ReadOnlyCollection<IPreScreenNotificationDetail> PreScreeningWarningDetails => new ReadOnlyCollection<IPreScreenNotificationDetail>(PreScreeningWarningList);
		List<IPreScreenNotificationDetail> PreScreeningWarningList => preScreeningWarningList ?? (preScreeningWarningList = new List<IPreScreenNotificationDetail>());
		List<IPreScreenNotificationDetail> preScreeningWarningList;

		public ReadOnlyCollection<IPreScreenNotificationDetail> PreScreeningErrorDetails => new ReadOnlyCollection<IPreScreenNotificationDetail>(PreScreeningErrorList);
		List<IPreScreenNotificationDetail> PreScreeningErrorList => preScreeningErrorList ?? (preScreeningErrorList = new List<IPreScreenNotificationDetail>());
		List<IPreScreenNotificationDetail> preScreeningErrorList;

		public ReadOnlyCollection<IPreScreenNotificationDetail> PreScreeningNotifyOnlyWarningDetails => new ReadOnlyCollection<IPreScreenNotificationDetail>(PreScreeningNotifyOnlyWarningList);
		List<IPreScreenNotificationDetail> PreScreeningNotifyOnlyWarningList => preScreeningNotifyOnlyWarningList ?? (preScreeningNotifyOnlyWarningList = new List<IPreScreenNotificationDetail>());
		List<IPreScreenNotificationDetail> preScreeningNotifyOnlyWarningList;

		public string PreScreeningStatus => PreScreeningWarningDetails.Count == 0 && PreScreeningErrorDetails.Count == 0 ? HVLVConsignmentPreScreeningStatusCodes.Codes.Passed : HVLVConsignmentPreScreeningStatusCodes.Codes.Failed;

		[JsonIgnore]
		public IHVLVConsignment Consignment { get; }

		public string FormattedWarningMessage
		{
			get
			{
				var result = new StringBuilder();
				foreach (var detailGroupPair in PreScreeningWarningDetails)
				{
					result.AppendLine(detailGroupPair.Message);
				}

				return result.ToString().Trim();
			}
		}

		public string FormattedErrorMessage
		{
			get
			{
				var result = new StringBuilder();
				foreach (var detailGroupPair in PreScreeningErrorDetails)
				{
					result.AppendLine(detailGroupPair.Message);
				}

				return result.ToString().Trim();
			}
		}

		public string FormattedNotifyOnlyWarningMessage
		{
			get
			{
				var result = new StringBuilder();
				foreach (var detailGroupPair in PreScreeningNotifyOnlyWarningDetails)
				{
					result.AppendLine(detailGroupPair.Message);
				}

				return result.ToString().Trim();
			}
		}

		public Guid ConsignmentPK => Consignment.PK.ToGuid();
	}
}
