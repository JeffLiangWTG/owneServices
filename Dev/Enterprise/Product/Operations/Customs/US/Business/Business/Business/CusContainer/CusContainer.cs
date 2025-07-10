using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[CodeProperty(CusContainer.Schema.CO_ContainerNumber), DescriptionProperty(CusContainer.Schema.CO_ContainerNumber)]
	public class CusContainer : TypeSafeCusContainer
		, Integration.Customs.US.ICusContainer
		, IIMessageAttacheeWithDisposition
		, IDispositionCodeDateParent
		, IAESTIRTransportationDetail
		, IContainer
		, IContainerDetail
		, IMessageAttacheeInDeclaration
		, IAdditionalReferenceNumberSupporter
		, ICusAddInfoTypeSupporter
	{
		public CusContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region JobContainer proxies

		public CusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers
		{
			get
			{
				var fwContainer = JobContainer as ForwardingContainer;
				return fwContainer != null ? fwContainer.AdditionalReferenceNumbers : DummyAdditionalReferenceNumbers;
			}
		}

		CusEntryNumAdditionalReferenceCollection DummyAdditionalReferenceNumbers
		{
			get { return fDummyAdditionalReferenceNumbers ?? (fDummyAdditionalReferenceNumbers = new CusEntryNumAdditionalReferenceCollection(this)); }
		}
		CusEntryNumAdditionalReferenceCollection fDummyAdditionalReferenceNumbers;

		[MaxLength(35)]
		public ZString AMSNumber
		{
			get
			{
				var fwContainer = JobContainer as ForwardingContainer;
				return fwContainer != null ? fwContainer.AMSNumber : ZString.Empty;
			}
			set
			{
				var fwContainer = JobContainer as ForwardingContainer;
				if (fwContainer != null)
				{
					fwContainer.AMSNumber = value;
				}
			}
		}

		public ZPropertyInfo AMSNumberInfo
		{
			get
			{
				var fwContainer = JobContainer as ForwardingContainer;
				if (fwContainer != null)
				{
					return GetWrappedZPropertyInfo(nameof(AMSNumber), x => fwContainer.AMSNumberInfo);
				}
				else
				{
					return GetZPropertyInfo(nameof(AMSNumber));
				}
			}
		}

		[MaxLength(11)]
		public ZString ITReferenceNumber
		{
			get
			{
				var fwContainer = JobContainer as ForwardingContainer;
				return fwContainer != null ? fwContainer.ITReferenceNumber : ZString.Empty;
			}
			set
			{
				var fwContainer = JobContainer as ForwardingContainer;
				if (fwContainer != null)
				{
					fwContainer.ITReferenceNumber = value;
				}
			}
		}

		public ZPropertyInfo ITReferenceNumberInfo
		{
			get
			{
				var fwContainer = JobContainer as ForwardingContainer;
				if (fwContainer != null)
				{
					return GetWrappedZPropertyInfo(nameof(ITReferenceNumber), x => fwContainer.ITReferenceNumberInfo);
				}
				else
				{
					return GetZPropertyInfo(nameof(ITReferenceNumber));
				}
			}
		}

		#endregion

		#region Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : Customs.Business.FetchStrategies.BaseCusContainerFetchStrategy
		{
			public Strategy(CusContainer container)
				: base(container)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
			}
		}

		#endregion

		#region Validation Modes

		public ValidationModes ValidationModes
		{
			get; set;
		}

		#endregion

		#region Overriden Properties

		public override ZString CO_ContainerNumber
		{
			get { return base.CO_ContainerNumber; }
			set
			{
				if (!IsCopying)
				{
					var oldValue = CO_ContainerNumber;
					if (oldValue != value)
					{
						Declaration?.PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
					}
				}
				base.CO_ContainerNumber = value;
			}
		}

		public override ZGuid CO_RC
		{
			get { return base.CO_RC; }
			set
			{
				if (!IsCopying)
				{
					var oldValue = CO_RC;
					if (oldValue != value)
					{
						Declaration?.PGATrackerHelper.LoadAllPGARelatedDataIfNeeded();
					}
				}
				base.CO_RC = value;
			}
		}

		public override ZString CO_MessageStatus
		{
			get { return base.CO_MessageStatus; }
			set
			{
				ZString oldStatus = CO_MessageStatus;
				base.CO_MessageStatus = value;
				if (oldStatus != CO_MessageStatus)
				{
					var statusList = (ImportMessageStatusList)Lookups.MessageStatusList;
					LogManager.AddAClearLogIfNecessary(oldStatus, CO_MessageStatus, statusList);
				}
			}
		}

		internal StatusLogManager LogManager
		{
			get
			{
				if (fLogManager == null)
				{
					fLogManager = new StatusLogManager(Logs, Declaration?.Branch);
				}
				return fLogManager;
			}
		}
		StatusLogManager fLogManager;
		#endregion

		#region Overriden Methods

		public override void Delete()
		{
			MessageAttacheesAddedOrDeletedEvent.InvokeMessageAttacheeAddedOrDeletedService(Factory, this, MessageAttacheeActionType.Deleted);
			base.Delete();
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			List<string> result = new List<string>(base.GetPropertiesToExcludeFromCloning());
			result.Add(CusContainerSchema.Constants.CO_MessageStatus);
			return result;
		}

		#endregion

		#region Collection

		public DispositionDataCollection DispositionCodes
		{
			get
			{
				if (fDispositionCodes == null)
				{
					fDispositionCodes = new DispositionDataCollection(this);
					fDispositionCodes.Load();
				}
				return fDispositionCodes;
			}
		}
		DispositionDataCollection fDispositionCodes;

		#endregion

		#region IControllerIDProvider Members

		IControllerIDProvider ControllerIDProvider
		{
			get { return Declaration; }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.ControllerID : null;
			}
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.BusinessObjectPK : Guid.Empty;
			}
		}

		#endregion

		#region IMessageAttacheeInDeclaration Members

		ZString IMessageAttacheeWithCBPSenderReference.EntryFilerCode
		{
			get { return ((IMessageAttacheeInDeclaration)Declaration).EntryFilerCode; }
		}

		ZString IMessageAttacheeInDeclaration.EntryNumber
		{
			get { return ((IMessageAttacheeInDeclaration)Declaration).EntryNumber; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.ProcessingDistrictPort
		{
			get { return Declaration.ProcessingDistrictPort; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.ProcessingOfficeCode
		{
			get { return Declaration.ProcessingOfficeCode; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.TransportMode
		{
			get { return Declaration.TransportMode; }
		}

		ZDateTime IMessageAttacheeInDeclaration.ReleaseDate
		{
			get { return ZDateTime.Empty; }
		}

		IReadOnlyList<ZGuid> IMessageAttacheeInDeclaration.ParentPKsOfMessages
		{
			get { return new ZGuid[] { PK }; }
		}

		ZGuid IMessageAttacheeInDeclaration.DeclarationPK
		{
			get { return CO_JE; }
		}

		bool IMessageAttacheeInDeclaration.IsActive
		{
			get { return true; }
		}

		GlbBranch IMessageAttachee.Branch
		{
			get { return Declaration != null ? Declaration.Branch : Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK); }
		}

		Guid IMessageAttacheeWithCBPSenderReference.CompanyPK
		{
			get { return Declaration?.RegistryCompanyPK ?? GlbCompany.CurrentCompany.PK.ToGuid(); }
		}

		ZString IMessageAttachee.MessageStatus
		{
			get { return CO_MessageStatus; }
			set { CO_MessageStatus = value; }
		}

		ZString IMessageAttacheeInDeclaration.MessageStatusDescription
		{
			get { return Lookups.MessageStatusList.GetDescriptionFromCode(CO_MessageStatus); }
		}

		CBPEDIMessageCollection IMessageAttachee.Messages
		{
			get { return Messages; }
		}

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return Declaration; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return Declaration.JE_DeclarationReference; }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get { return Declaration != null ? Declaration.Logs : null; }
		}

		ZString IMessageAttacheeInDeclaration.JobReferenceNumber
		{
			get { return Declaration != null ? Declaration.JE_DeclarationReference : ZString.Empty; }
		}

		ZString IMessageAttacheeInDeclaration.EntryStatus
		{
			get { return ZString.Empty; }
		}

		MessageAttacheeRecordType IMessageAttacheeInDeclaration.RecordType
		{
			get { return MessageAttacheeRecordType.Container; }
		}

		ZString IMessageAttacheeInDeclaration.RecordTypeDescription
		{
			get { return MessageAttacheeRecordTypeDescriptions.Container; }
		}

		ZString IMessageAttacheeInDeclaration.HumanFriendlyReference
		{
			get { return CO_ContainerNumber; }
		}

		IEnumerable<INotification> IMessageAttacheeInDeclaration.GetBusinessLayerNotificationsToAddToWrapper()
		{
			yield break;//as container is exposed on the form and wrapper does not need to add again
		}

		ZDateTime IMessageAttacheeInDeclaration.TIBExpiryDate
		{
			get { return ZDateTime.Empty; }
		}

		ZInt IMessageAttacheeInDeclaration.TIBNumOfExtensions
		{
			get { return ZInt.Zero; }
		}

		#endregion

		#region IDispositionCodeDateParent Members

		CodeDescriptionPairList IDispositionCodeDateParent.DispositionCodeDescriptionList
		{
			get { return fDispositionList ?? (fDispositionList = new DispositionList()); }
		}
		DispositionList fDispositionList;

		string IDispositionCodeDateParent.GetDispositionDescriptionBasedOnSource(ZString dispositionSource, ZString code)
		{
			return "";
		}

		#endregion

		#region IContainer Members

		ZString IContainer.ContainerNumber
		{
			get { return CO_ContainerNumber; }
		}

		ZString IContainer.ContainerType
		{
			get { return Container != null ? Container.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates) : ZString.Empty; }
		}

		#endregion

		#region RailCar types

		public bool IsRailCar(string usContainercode)
		{
			return RailCarCodes.ContainsCode(usContainercode);
		}

		public static CodeDescriptionPairList RailCarCodes
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(USContainerCodeList.Codes.BE, USContainerCodeList.Descriptions.BE);
				result.AddPair(USContainerCodeList.Codes.BF, USContainerCodeList.Descriptions.BF);
				result.AddPair(USContainerCodeList.Codes.BG, USContainerCodeList.Descriptions.BG);
				result.AddPair(USContainerCodeList.Codes.BH, USContainerCodeList.Descriptions.BH);
				result.AddPair(USContainerCodeList.Codes.BJ, USContainerCodeList.Descriptions.BJ);
				result.AddPair(USContainerCodeList.Codes.BX, USContainerCodeList.Descriptions.BX);
				result.AddPair(USContainerCodeList.Codes.CA, USContainerCodeList.Descriptions.CA);
				result.AddPair(USContainerCodeList.Codes.DX, USContainerCodeList.Descriptions.DX);
				result.AddPair(USContainerCodeList.Codes.ET, USContainerCodeList.Descriptions.ET);
				result.AddPair(USContainerCodeList.Codes.FX, USContainerCodeList.Descriptions.FX);
				result.AddPair(USContainerCodeList.Codes.HC, USContainerCodeList.Descriptions.HC);
				result.AddPair(USContainerCodeList.Codes.HO, USContainerCodeList.Descriptions.HO);
				result.AddPair(USContainerCodeList.Codes.HP, USContainerCodeList.Descriptions.HP);
				result.AddPair(USContainerCodeList.Codes.HT, USContainerCodeList.Descriptions.HT);
				result.AddPair(USContainerCodeList.Codes.IX, USContainerCodeList.Descriptions.IX);
				result.AddPair(USContainerCodeList.Codes.LO, USContainerCodeList.Descriptions.LO);
				result.AddPair(USContainerCodeList.Codes.NX, USContainerCodeList.Descriptions.NX);
				result.AddPair(USContainerCodeList.Codes.PP, USContainerCodeList.Descriptions.PP);
				result.AddPair(USContainerCodeList.Codes.RR, USContainerCodeList.Descriptions.RR);
				result.AddPair(USContainerCodeList.Codes.SA, USContainerCodeList.Descriptions.SA);
				result.AddPair(USContainerCodeList.Codes.SK, USContainerCodeList.Descriptions.SK);
				result.AddPair(USContainerCodeList.Codes.SR, USContainerCodeList.Descriptions.SR);
				result.AddPair(USContainerCodeList.Codes.UA, USContainerCodeList.Descriptions.UA);
				result.AddPair(USContainerCodeList.Codes.UB, USContainerCodeList.Descriptions.UB);
				result.AddPair(USContainerCodeList.Codes.UC, USContainerCodeList.Descriptions.UC);
				result.AddPair(USContainerCodeList.Codes.UD, USContainerCodeList.Descriptions.UD);
				result.AddPair(USContainerCodeList.Codes.UE, USContainerCodeList.Descriptions.UE);
				result.AddPair(USContainerCodeList.Codes.WY, USContainerCodeList.Descriptions.WY);
				return result;
			}
		}

		#endregion

		#region IMessageResponseNotificator Members

		ZString IMessageResponseNotificator.GetFallbackEmailAddressRecipient()
		{
			IMessageResponseNotificator notificator = Declaration;
			return notificator != null ? notificator.GetFallbackEmailAddressRecipient() : ZString.Empty;
		}

		#endregion

		#region IAESTIRTransportationDetail Members

		ZString IAESTIRTransportationDetail.EquipmentNumber
		{
			get { return MessageBlockStringDataCorrector.KeepOnlyValidCharacters(CO_ContainerNumber.Replace(" ", ""), ABICharacterTypeString.Constants.Alphanumeric, 14); }
		}

		ZString IAESTIRTransportationDetail.SealNumber
		{
			get { return MessageBlockStringDataCorrector.KeepOnlyValidCharacters(CO_Seal.Replace(" ", ""), ABICharacterTypeString.Constants.Alphanumeric, 15); }
		}

		ZString IAESTIRTransportationDetail.TransportationReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IIMessageAttacheeWithDisposition Members

		void IIMessageAttacheeWithDisposition.UpdateDispositionInformation(ZString dispositionCode, ZDateTime dispositionDate)
		{
			DispositionCodes.AddNewIfNotExist(dispositionCode, dispositionDate);
		}

		void IIMessageAttacheeWithDisposition.MarkPreviousDispositionsInactive(ZDateTime dispositionDate)
		{
		}

		#endregion

		#region IAdditionalReferenceNumberSupporter

		CusEntryNumAdditionalReferenceCollection IAdditionalReferenceNumberSupporter.AdditionalReferenceNumbers
		{
			get { return AdditionalReferenceNumbers; }
		}

		void IAdditionalReferenceNumberSupporter.CreateOrUpdate(ZString numberType, ZString countryCode, ZString value, INotifications notify)
		{
		}

		bool IAdditionalReferenceNumberSupporter.IncludeSpecialCustomsInstructionsItems
		{
			get { throw new NotImplementedException(); }
		}

		void IAdditionalReferenceNumberSupporter.OnEntryNumChanged(CusEntryNumber additionalReferenceNumber)
		{
		}

		void IAdditionalReferenceNumberSupporter.AdditionalEntryNumberValidation(ZPropertyInfo info, ZString type, ZString number)
		{
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USDisposition, typeof(DispositionData));
			return result;
		}

		#endregion

		#region IContainerDetail Members

		ZString IContainerDetail.ContainerEquipmentID
		{
			get { return CO_ContainerNumber; }
		}

		ZShort IContainerDetail.ContainerLength
		{
			get
			{
				var result = ZShort.Zero;
				var containerType = Container;
				if (containerType != null)
				{
					result = containerType.RC_Length > 99m ? (short)99 : (short)containerType.RC_Length.ToZInt();
				}
				return result;
			}
		}

		ZBool IContainerDetail.IsRefrigerated
		{
			get
			{
				var containerType = Container;
				return containerType != null && containerType.RC_ContainerType == Core.Constants.ContainerTypes.Refrigerated;
			}
		}

		#endregion
	}
}
