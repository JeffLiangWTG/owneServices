using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CargoManifestQuerySendingObject : NonPersistentBusinessObject, IObsoleteValidation, ICargoManifestQuerySendingObject
	{
		public CargoManifestQuerySendingObject(CargoManifestStatusQueryHeaderObject sendingHeader, ICargoManifestStatusQueryData queryData)
			: base(sendingHeader.Factory)
		{
			this.queryData = queryData;
			this.sendingHeader = sendingHeader;

			DefaultLimitOutputOptionIfRequired();
		}
		readonly ICargoManifestStatusQueryData queryData;
		readonly CargoManifestStatusQueryHeaderObject sendingHeader;

		public static class Schema
		{
			public const string ShouldSendMessage = "ShouldSendMessage";
			public const string RequestForRelatedBOL = "RequestForRelatedBOL";
			public const string UpdateEntryWithResults = "UpdateEntryWithResults";
			public const string LimitOutputOption = "LimitOutputOption";
		}

		void DefaultLimitOutputOptionIfRequired()
		{
			if (!sendingHeader.ActionCode.IsEmpty)
			{
				LimitOutputOption = sendingHeader.ActionCode == CargoManifestStatusQueryActionList.Codes.Entry ?
					LimitOutputCodeList.Codes._0MostRecentResults : LimitOutputCodeList.Codes._2AllAvailableResults;
			}
		}

		public ZString HumanFriendlyReference
		{
			get
			{
				var result = queryData != null ? queryData.HumanFriendlyReference : ZString.Empty;

				if (!result.IsEmpty && sendingHeader.Header.TransportMode == TransportTypeList.Codes.Air)
				{
					if (result.Contains('(') && result.Contains(')'))
					{
						if (sendingHeader.ActionCode == CargoManifestStatusQueryActionList.Codes.MAWB)
						{
							result = result.SubstringSafe(result.IndexOf('(') + 1).Replace(")", "");
						}
						else if (sendingHeader.ActionCode == CargoManifestStatusQueryActionList.Codes.HAWB)
						{
							result = result.SubstringSafe(0, result.IndexOf('(') - 1);
						}
					}
				}

				return result;
			}
		}

		#region Related BOL Indicator

		[ReadOnlyMember(nameof(RequestForRelatedBOL_ReadOnly))]
		public ZBool RequestForRelatedBOL
		{
			get { return fRequestForRelatedBOL; }
			set
			{
				SetNonPersistentPropertyValue(RequestForRelatedBOLInfo, ref fRequestForRelatedBOL, value);
				ValidateRequestForRelatedBOL();
				ValidateUpdateEntryResults();
			}
		}
		ZBool fRequestForRelatedBOL;

		public ZPropertyInfo RequestForRelatedBOLInfo
		{
			get { return GetZPropertyInfo(Schema.RequestForRelatedBOL); }
		}

		bool RequestForRelatedBOL_ReadOnly
		{
			get
			{
				return sendingHeader != null && sendingHeader.ActionCode != CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill &&
					sendingHeader.ActionCode != CargoManifestStatusQueryActionList.Codes.MAWB;
			}
		}

		public void ValidateRequestForRelatedBOL()
		{
			RequestForRelatedBOLInfo.ClearAllNotifications();

			if (!IsValidationSuspended && sendingHeader != null &&
				(sendingHeader.Header.TransportMode == TransportTypeList.Codes.Sea || sendingHeader.Header.TransportMode == TransportTypeList.Codes.Rail))
			{
				var isMasterBill = queryData.QueryActionType == CargoManifestQueryActionType.BillOfLading || queryData.QueryActionType == CargoManifestQueryActionType.MAWB;
				var isHouseBill = queryData.QueryActionType == CargoManifestQueryActionType.HAWB || queryData.QueryActionType == CargoManifestQueryActionType.HouseBill;

				if ((isMasterBill && HasHouseBill || isHouseBill && HasSubHouseBill) && UpdateEntryWithResults && !RequestForRelatedBOL)
				{
					RequestForRelatedBOLInfo.AddMessageError(RelatedBOLNotSelected);
				}
			}
		}

		internal const string RelatedBOLNotSelected = "Please request a related Bill of Lading.";

		IEnumerable<CargoManifestQuerySendingObject> HouseBills
		{
			get
			{
				return sendingHeader.SendingObjects.OfType<CargoManifestQuerySendingObject>().
						Where(obj => obj.queryData != null && (obj.queryData.QueryActionType == CargoManifestQueryActionType.HAWB ||
							obj.queryData.QueryActionType == CargoManifestQueryActionType.HouseBill));
			}
		}

		bool HasHouseBill
		{
			get
			{
				return HouseBills.Any();
			}
		}

		bool HasSubHouseBill
		{
			get
			{
				return sendingHeader.SendingObjects.OfType<CargoManifestQuerySendingObject>().Any(obj => obj.queryData != null && obj.queryData.QueryActionType == CargoManifestQueryActionType.SubHouseBill);
			}
		}

		#endregion

		#region Update Entry With Results

		[ReadOnlyMember(nameof(UpdateEntryWithResults_ReadOnly))]
		public ZBool UpdateEntryWithResults
		{
			get { return fUpdateEntryWithResults; }
			set
			{
				SetNonPersistentPropertyValue(UpdateEntryWithResultsInfo, ref fUpdateEntryWithResults, value);
				ValidateRequestForRelatedBOL();
				ValidateUpdateEntryResults();
			}
		}
		ZBool fUpdateEntryWithResults;

		public ZPropertyInfo UpdateEntryWithResultsInfo
		{
			get { return GetZPropertyInfo(Schema.UpdateEntryWithResults); }
		}

		bool UpdateEntryWithResults_ReadOnly
		{
			get
			{
				var canUpdateForEntryQuery = sendingHeader != null && sendingHeader.Header.IsACEQuery && USCustomsDataRegistry.Instance.RequestForBillAndEntryData.Value &&
					sendingHeader.ActionCode == CargoManifestStatusQueryActionList.Codes.Entry;

				return !canUpdateForEntryQuery && sendingHeader != null &&
					sendingHeader.ActionCode != CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill &&
					sendingHeader.ActionCode != CargoManifestStatusQueryActionList.Codes.HAWB &&
					sendingHeader.ActionCode != CargoManifestStatusQueryActionList.Codes.MAWB;
			}
		}

		public void ValidateUpdateEntryResults()
		{
			if (!IsValidationSuspended && sendingHeader != null && sendingHeader.Header.TransportMode == TransportTypeList.Codes.Air && !HasHouseBill)
			{
				UpdateEntryWithResultsInfo.ClearAllNotifications();
				if (UpdateEntryWithResults)
				{
					UpdateEntryWithResultsInfo.AddMessageError(UpdateEntryShouldNotBeSelected);
				}
			}

			ValidateLimitOutputOption();
		}
		internal const string UpdateEntryShouldNotBeSelected = "Should not be selected as there is no HAWB.";

		#endregion

		#region Limit Output

		[MaxLength(21)]
		[List(nameof(LimitOutputCodes))]
		public ZString LimitOutputOption
		{
			get { return fLimitOutputOption; }
			set
			{
				SetNonPersistentPropertyValue(LimitOutputOptionInfo, ref fLimitOutputOption, value);
				ValidateLimitOutputOption();
			}
		}
		ZString fLimitOutputOption;

		public ZPropertyInfo LimitOutputOptionInfo
		{
			get { return GetZPropertyInfo(Schema.LimitOutputOption); }
		}

		public CodeDescriptionPairList LimitOutputCodes
		{
			get { return Factory.GetCachedValue<LimitOutputCodeList>(); }
		}

		/// <summary>
		/// ACS query message does not support limit output option
		/// </summary>
		internal bool LimitOutputOption_ReadOnly
		{
			get { return sendingHeader != null && sendingHeader.ActionCode == CargoManifestStatusQueryActionList.Codes.Entry && !sendingHeader.Header.IsACEQuery; }
		}

		public void ValidateLimitOutputOption()
		{
			if (!IsValidationSuspended)
			{
				LimitOutputOptionInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCode(LimitOutputOptionInfo, LimitOutputCodes);

				if (UpdateEntryWithResults && !LimitOutputOption.IsEmpty && LimitOutputOption != LimitOutputCodeList.Codes._0MostRecentResults)
				{
					LimitOutputOptionInfo.AddError(LimitOutputShouldBeBlank);
				}
			}
		}
		internal const string LimitOutputShouldBeBlank = "Output Option should be 'Most recent results' if Update Entry with Results is ticked.";

		#endregion

		#region ShouldSendMessage

		public ZBool ShouldSendMessage
		{
			get { return shouldSend; }
			set { SetNonPersistentPropertyValue(ShouldSendMessageInfo, ref shouldSend, value); }
		}
		ZBool shouldSend;

		public ZPropertyInfo ShouldSendMessageInfo
		{
			get { return this.GetZPropertyInfo(Schema.ShouldSendMessage); }
		}

		#endregion

		#region ICargoManifestQuerySendingObject Members

		ZString ICargoManifestQuerySendingObject.EntryOrInBondNumber
		{
			get { return queryData.EntryOrInBondNumber; }
		}

		ZString ICargoManifestQuerySendingObject.BillIssuerCode
		{
			get { return queryData.BillIssuerCode; }
		}

		ZString ICargoManifestQuerySendingObject.MasterBillNumber
		{
			get
			{
				return queryData.QueryActionType == CargoManifestQueryActionType.MAWB ||
					sendingHeader.ActionCode == CargoManifestStatusQueryActionList.Codes.HAWB ? queryData.MasterAirWayBillNumber : queryData.BillNumber;
			}
		}

		ZString ICargoManifestQuerySendingObject.HouseBillNumber
		{
			get
			{
				var result = ZString.Empty;

				if (queryData.QueryActionType == CargoManifestQueryActionType.HAWB)
				{
					result = queryData.HouseAirWayBillNumber;
				}
				else if (queryData.QueryActionType == CargoManifestQueryActionType.HouseBill || queryData.QueryActionType == CargoManifestQueryActionType.SubHouseBill)
				{
					result = queryData.BillNumber;
				}
				else if (queryData.QueryActionType == CargoManifestQueryActionType.MAWB && sendingHeader.ActionCode == CargoManifestStatusQueryActionList.Codes.HAWB)
				{
					result = queryData.HouseAirWayBillNumber;
				}
				return result;
			}
		}

		public void LinkToMessage(EDIMessage message)
		{
			if (queryData != null)
			{
				queryData.LinkToMessage(message);
			}
		}

		ZString ICargoManifestQuerySendingObject.ActionCode
		{
			get { return sendingHeader.ActionCode; }
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			ValidateRequestForRelatedBOL();
			ValidateUpdateEntryResults();
			ValidateLimitOutputOption();

			base.RunPreSaveValidationCore();
		}
	}
}
