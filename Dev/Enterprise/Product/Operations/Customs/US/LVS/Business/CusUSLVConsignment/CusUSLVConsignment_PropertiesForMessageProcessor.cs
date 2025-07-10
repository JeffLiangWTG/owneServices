using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.US.LVS;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Customs.US.LVS.Business
{
	public partial class CusUSLVConsignment : IEntryHeaderParentBusinessObject, ISimplifiedMessageLinkedObject, ICusUSLVConsignment
	{
		#region IEntryHeaderParentBusinessObject

		BusinessObject IEntryHeaderParentBusinessObject.LinkedObject => this;

		CusEntryHeader IEntryHeaderParentBusinessObject.EntrySummaryEntry => null;

		CusEntryHeader IEntryHeaderParentBusinessObject.CargoReleaseEntry => null;

		ISimplifiedMessageLinkedObject IEntryHeaderParentBusinessObject.SimplifiedEntry => this;

		ZString IEntryHeaderParentBusinessObject.ReferenceNumber => Shipment.ULH_JobNumber + " / " + CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(Shipment.ULH_EntryFilerCode, CE_EntryNum);

		Guid IEntryHeaderParentBusinessObject.RegistryCompanyPK => Shipment.RegistryCompanyPK;

		Guid IEntryHeaderParentBusinessObject.RegistryBranchPK => Shipment.RegistryBranchPK;

		GlbStaff IEntryHeaderParentBusinessObject.CusAgent => null;

		OrgHeader IEntryHeaderParentBusinessObject.Importer => null;

		DispositionDataCollection IEntryHeaderParentBusinessObject.DispositionCodes => this.DispositionCodes;

		IDisposable IEntryHeaderParentBusinessObject.ReleaseStatusChangingSuspender => null;

		ErrorsRecordCollection IEntryHeaderParentBusinessObject.ENSStatusNotifications => new ErrorsRecordCollection(Factory);

		ZString IEntryHeaderParentBusinessObject.ReleaseStatus
		{
			get => CE_EntryStatus;
			set => CE_EntryStatus = value;
		}

		ZDateTime IEntryHeaderParentBusinessObject.ReleaseDateTime
		{
			get => CE_IssueDate;
			set => CE_IssueDate = value;
		}

		bool IEntryHeaderParentBusinessObject.ShouldUpdateDeclarationWithCargoReleaseResults
		{
			get
			{
				return USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.GetFallBackValueAtAllLevels(Shipment.RegistryCompanyPK, ((IEntryHeaderParentBusinessObject)this).RegistryBranchPK, Guid.Empty);
			}
		}

		void IEntryHeaderParentBusinessObject.UpdateQuotaStatus(ZString quotaStatus)
		{
		}

		void IEntryHeaderParentBusinessObject.MarkAIIRequested()
		{
		}

		void IEntryHeaderParentBusinessObject.UpdateMessageLinkedParentBOAfterReleased(ASESSO10Base blockSO10, IEnumerable<ASESSO40Base> blockSO40List, IEnumerable<ASESSO50Base> blockSO50List)
		{
			var messageProcessingObject = this as IEntryHeaderParentBusinessObject;
			var shouldUpdateManifestDetails = messageProcessingObject.ShouldUpdateDeclarationWithCargoReleaseResults && messageProcessingObject.ReleaseStatus == CRLReleaseStatusList.Codes.REL;

			if (shouldUpdateManifestDetails)
			{
				var blankPropertyInfos = GetBlankProperties();
				UpdateConsignmentBlockSO10(blankPropertyInfos, blockSO10);
			}
		}

		void IEntryHeaderParentBusinessObject.AddOrUpdateCusDisposition(Dictionary<ZString, IPGADispositionProvider> pgaEntryStatusMapping, List<IPGADispositionProvider> pgaLineStatusList)
		{
		}

		void IEntryHeaderParentBusinessObject.LogPGAEntryAndLineStatus(Dictionary<ZString, ZString> previousPGAEntryStatus, List<IPGADispositionProvider> dispositionProviders, ZString messageType)
		{
		}

		Bill IEntryHeaderParentBusinessObject.GetFirstBillHasSameNumber(ZString billNumber)
		{
			return null;
		}

		ZString IEntryHeaderParentBusinessObject.GetUnableToDeactivateStatementLineRemarkIfNecessary()
		{
			return ZString.Empty;
		}

		Dictionary<ZString, ZString> IEntryHeaderParentBusinessObject.GetPGAEntryStatus()
		{
			return new Dictionary<ZString, ZString>();
		}

		OGADispositionData IEntryHeaderParentBusinessObject.AddOGADispositionData(IPGADispositionProvider dispositionProvider, ZString source)
		{
			return null;
		}

		#endregion

		#region ISimplifiedMessageLinkedObject

		BusinessObject ISimplifiedMessageLinkedObject.LinkedObject => this;

		IEntryHeaderParentBusinessObject ISimplifiedMessageLinkedObject.ParentBusinessObject => this;

		ZBool ISimplifiedMessageLinkedObject.IsCargoReleaseBeingCertified => false;

		bool ISimplifiedMessageLinkedObject.IsFormalEntry => false;

		bool ISimplifiedMessageLinkedObject.IsBorderCargoRelease => false;

		bool ISimplifiedMessageLinkedObject.IsCargoRelease => false;

		bool ISimplifiedMessageLinkedObject.IsACECargoRelease => true;

		bool ISimplifiedMessageLinkedObject.IsLVSCargoRelease => true;

		bool ISimplifiedMessageLinkedObject.UseCodeIsHVL => Shipment.ULH_UseCode == LVSConstants.ETailUseCode;

		ZString ISimplifiedMessageLinkedObject.CargoReleaseCertifiedStatus { get; set; }

		ZString ISimplifiedMessageLinkedObject.EntryFilerCode => Shipment.ULH_EntryFilerCode;

		void ISimplifiedMessageLinkedObject.ClearCRLCertStatusFromAllHeaders()
		{
		}

		void ISimplifiedMessageLinkedObject.DeactiveStatementLineIfRequired(ZString[] clearDeletedStatus, ZString messageStatus)
		{
		}

		void ISimplifiedMessageLinkedObject.AutoSendEntrySummaryQueryIfEligible()
		{
		}

		void ISimplifiedMessageLinkedObject.UpdateACEFDALineInfoIfRequired(OGADispositionData processingOGADispositionData)
		{
		}

		void ISimplifiedMessageLinkedObject.UpdateFWSLineInfoIfRequired(OGADispositionData processingOGADispositionData)
		{
		}

		#endregion

		#region Other Functions

		List<ZPropertyInfo> GetBlankProperties()
		{
			var propertyInfos = new List<ZPropertyInfo>()
			{
				Shipment.ULH_PortOfEntryInfo,
				Shipment.ULH_CarrierSCACInfo,
				Shipment.ULH_VoyageFlightNoInfo,
				Shipment.ULH_DischargeDateInfo,
			};

			return propertyInfos.Where(x => x.Value.IsDefault || x.Value.IsEmpty).ToList();
		}

		void UpdateConsignmentBlockSO10(IEnumerable<ZPropertyInfo> blankPropertyInfos, ASESSO10Base blockSO10)
		{
			if (blockSO10 != null)
			{
				UpdateValueIfBlank(blankPropertyInfos, Shipment.ULH_PortOfEntryInfo, blockSO10.DistrictPortOfEntry);
				UpdateValueIfBlank(blankPropertyInfos, Shipment.ULH_CarrierSCACInfo, blockSO10.CarrierCode);
				UpdateValueIfBlank(blankPropertyInfos, Shipment.ULH_VoyageFlightNoInfo, blockSO10.VoyageFlightTripManifestNumber);
				UpdateValueIfBlank(blankPropertyInfos, Shipment.ULH_DischargeDateInfo, blockSO10.EstimatedDateOfArrival);
			}
		}

		void UpdateValueIfBlank(IEnumerable<ZPropertyInfo> blankPropertyInfoList, ZPropertyInfo propertyInfo, IZType value)
		{
			if (blankPropertyInfoList.Contains(propertyInfo))
			{
				propertyInfo.Value = value;
			}
		}

		KeyValuePair<string, string>[] ISimplifiedMessageLinkedObject.GetMSCEventParameters()
		{
			return new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(Params.CustomsReferenceNumber, Shipment.ULH_EntryFilerCode + CE_EntryNum),
				new KeyValuePair<string, string>(Params.ReferenceNumber, ULB_HouseBill),
				new KeyValuePair<string, string>(Params.MessageType, Core.Constants.EventReferenceMessageTypes.CargoRelease)
			};
		}

		ZString ISimplifiedMessageLinkedObject.GetMSCEventReferenceForCargoReleaseResponse(Messaging.Business.CBPEDIMessage message)
		{
			var keyValuePairArray = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(Params.CustomsReferenceNumber, Shipment.ULH_EntryFilerCode + CE_EntryNum),
				new KeyValuePair<string, string>(Params.ReferenceNumber, ULB_HouseBill),
				new KeyValuePair<string, string>(Params.Type, message.EM_MessageType),
				new KeyValuePair<string, string>(Params.Status, ULB_MessageStatus)
			};

			return string.Concat(keyValuePairArray.Select(x => $"|{x.Key}={x.Value}"));
		}

		#endregion
	}
}
