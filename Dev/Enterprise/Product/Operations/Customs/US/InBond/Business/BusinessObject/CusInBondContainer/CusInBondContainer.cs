using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.US.InBond.Business.Universal.Constants.Header.UniversalCopyIgnoreElement;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	[UniversalCopyIgnoreElement(UNDGDataItems, CusInBondVehicleCtrls, CusInBondCargoDescs, Schema.BC_MessageStatus)]
	[UniversalCopyWithExtendedEntities]
	public class CusInBondContainer : Customs.Business.CusInBondContainer
		, Integration.Customs.US.InBond.ICusInBondContainer
		, IInBondLineDetailsHeader
		, IInBondContainer
		, IUNDGDataItemProvider
		, ICusAddInfoTypeSupporter
		, Customs.Business.ICusInBondCargoDescTypeProvider
		, IDispositionCodeDateParent
		, IResetToOriginal
	{
		public CusInBondContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New Properties

		public ZBool ShouldSend
		{
			get { return fShouldSend; }
			set { fShouldSend = value; }
		}
		ZBool fShouldSend;

		public new CusInBondBill Bill
		{
			get { return (CusInBondBill)base.Bill; }
		}

		public bool IsDetailedInBond
		{
			get
			{
				CusInBondHeader header = Header;
				return header != null && header.IsDetailedInBond;
			}
		}

		bool CopyParentDefault
		{
			get
			{
				var header = Header;
				return header != null && header.ShouldSynchronise;
			}
		}

		string ParentTableName
		{
			get
			{
				var header = Header;
				return header != null ? header.ParentTableName : string.Empty;
			}
		}

		public bool IsWaitingForResponse
		{
			get { return LogManager.IsAnAwaitingStatus(BC_MessageStatus); }
		}

		public bool IsAcceptedByCustoms
		{
			get { return LogManager.IsAcceptedByCustoms(BC_MessageStatus); }
		}

		public bool IsWithdrawn
		{
			get { return LogManager.IsWithdrawn(BC_MessageStatus); }
		}

		internal ImportMessageStatusList StatusList => LogManager.StatusList;
		#endregion

		#region Override Properties

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondContainer|BC_MessageStatus", Caption = "Message Status", MediumCaption = "Msg. Status", ShortCaption = "Msg. Status")]
		public override ZString BC_MessageStatus { get => base.BC_MessageStatus; set => base.BC_MessageStatus = value; }

		public override ZGuid BC_ParentID
		{
			get { return base.BC_ParentID; }
			set
			{
				var oldValue = base.BC_ParentID;
				base.BC_ParentID = value;
				if (oldValue != base.BC_ParentID)
				{
					Commodities.MarkAsNeedingValidation();
				}
			}
		}

		#region BC_RC

		[List(nameof(Lookups) + "." + nameof(CusInBondContainerLookups.ContainerTypes))]
		[ReadOnlyMember(nameof(BC_RC_ReadOnly))]
		public override ZGuid BC_RC
		{
			get { return base.BC_RC; }
			set { base.BC_RC = value; }
		}

		bool BC_RC_ReadOnly
		{
			get { return IsNonContainerized; }
		}

		#endregion

		#region BC_Seal1

		[ReadOnlyMember(nameof(BC_Seal1_ReadOnly))]
		public override ZString BC_Seal1
		{
			get { return base.BC_Seal1; }
			set { base.BC_Seal1 = value; }
		}

		bool BC_Seal1_ReadOnly
		{
			get { return IsNonContainerized; }
		}

		#endregion

		#region BC_Seal2

		[ReadOnlyMember(nameof(BC_Seal2_ReadOnly))]
		public override ZString BC_Seal2
		{
			get { return base.BC_Seal2; }
			set { base.BC_Seal2 = value; }
		}

		bool BC_Seal2_ReadOnly
		{
			get { return IsNonContainerized; }
		}

		#endregion

		#region BC_PieceCount
		[ReadOnlyMember(nameof(BC_PieceCount_ReadOnly))]
		public override ZInt BC_PieceCount
		{
			get { return base.BC_PieceCount; }
			set
			{
				var oldValue = base.BC_PieceCount;
				base.BC_PieceCount = value;
				if (oldValue != base.BC_PieceCount)
				{
					Commodities.MarkAsNeedingValidation();
				}
			}
		}

		public bool IsPieceCountReadOnly
		{
			get { return BC_PieceCount_ReadOnly; }
		}

		bool BC_PieceCount_ReadOnly
		{
			get
			{
				return MoveHeader != null && MoveHeader.SupportsBondedWarehousing;
			}
		}

		#endregion

		public override ZString BC_ContainerNum
		{
			get { return base.BC_ContainerNum; }
			set
			{
				ZString oldValue = BC_ContainerNum;
				base.BC_ContainerNum = value;
				if (!IsCopying && oldValue != BC_ContainerNum)
				{
					UpdateContainerDetails();
				}
			}
		}

		public new CusInBondContainerLookups Lookups
		{
			get { return (CusInBondContainerLookups)base.Lookups; }
		}

		public new CusInBondContainerValidation Validation
		{
			get { return (CusInBondContainerValidation)base.Validation; }
		}

		public override bool CanDelete
		{
			get { return base.CanDelete && !CopyParentDefault; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString result = base.ReasonForNotAbleToDelete;
				if (result.IsEmpty && !CanDelete)
				{
					result = MultilingualString.Join(" ", ResString.GetMultilingualString("3B9872CA-9843-43C3-9A53-956BCF982E47", "This Container cannot be deleted"),
						CopyParentDefault
						? ValidationConstants.Synchronize.SynchronizedFromParent(ParentTableName) : (NoResString)".");
				}
				return result;
			}
		}

		#endregion

		#region Related Objects

		public new CusInBondHeader Header
		{
			get { return (CusInBondHeader)base.Header; }
		}

		public new CusInBondMoveHeader MoveHeader
		{
			get { return (CusInBondMoveHeader)base.MoveHeader; }
		}

		public new CusInBondMoveDetail MoveDetail
		{
			get { return (CusInBondMoveDetail)base.MoveDetail; }
		}

		[ChildEditable]
		[UniversalCopyCollectionEntity(CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.Constants.BY_ParentID, CusInBondCargoDescSchema.Constants.BY_ParentTableCode)]
		public new CusInBondCargoDescCollection Commodities
		{
			get { return (CusInBondCargoDescCollection)base.Commodities; }
		}

		protected override Customs.Business.ICusInBondCargoDescCollection<Customs.Business.CusInBondCargoDesc> GetNewCommoditiesCollection()
		{
			return new CusInBondCargoDescCollection(this);
		}

		[ChildEditable]
		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this);
					fMessages.Load();
					fMessages.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(fMessages);
				}
				return fMessages;
			}
		}
		EDIMessageCollection fMessages;

		#region UNDGs

		[ChildEditable]
		[UniversalCopyCollectionEntity(UNDGDataItemSchema.Constants.TableName, UNDGDataItemSchema.Constants.DI_ParentID, UNDGDataItemSchema.Constants.DI_ParentTableCode)]
		public new UNDGDataItemCollection UNDGs
		{
			get
			{
				if (fUNDGs == null)
				{
					fUNDGs = new UNDGDataItemCollection(this);
					RegisterEditableChildObject(fUNDGs);
				}
				return fUNDGs;
			}
		}
		UNDGDataItemCollection fUNDGs;

		#endregion

		#endregion

		#region Implementation

		protected override Customs.Business.CusInBondMoveDetail MoveDetailCore
		{
			get { return Factory.Load<CusInBondMoveDetail>(BC_ParentID); }
		}

		void UpdateContainerDetails()
		{
			if (IsNonContainerized)
			{
				BC_RC = ZGuid.Empty;
				BC_Seal1 = ZString.Empty;
				BC_Seal2 = ZString.Empty;
			}
			else if (!BC_ContainerNum.IsEmpty)
			{
				CusInBondMoveHeader moveHeader = MoveHeader;
				if (moveHeader != null)
				{
					bool foundContainer = false;
					foreach (CusInBondMoveDetail moveDetail in moveHeader.MovementDetails)
					{
						if (moveDetail.PK != BC_ParentID)
						{
							foreach (CusInBondContainer otherContainer in moveDetail.Containers)
							{
								if (otherContainer.BC_ContainerNum == BC_ContainerNum)
								{
									foundContainer = true;
									BC_RC = otherContainer.BC_RC;
									BC_Seal1 = otherContainer.BC_Seal1;
									BC_Seal2 = otherContainer.BC_Seal2;
									break;
								}
							}
						}
						if (foundContainer)
						{
							break;
						}
					}
				}
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override Customs.Business.CusInBondContainerLookups GetNewLookups()
		{
			return new CusInBondContainerLookups(this);
		}

		protected override Customs.Business.CusInBondContainerValidation GetNewValidation()
		{
			return new CusInBondContainerValidation(this);
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		public IDisposable SuspendDefaultingChildLines()
		{
			return new DefaultChildLinesSuspender(this);
		}

		public bool IsDefaultChildLinesSuspended
		{
			get { return _defaultChildLinesIndex > 0; }
		}

		int _defaultChildLinesIndex;

		class DefaultChildLinesSuspender : IDisposable
		{
			public DefaultChildLinesSuspender(CusInBondContainer container)
			{
				this.container = container;
				container._defaultChildLinesIndex++;
			}

			readonly CusInBondContainer container;

			#region IDisposable Members

			public void Dispose()
			{
				container._defaultChildLinesIndex--;
			}

			#endregion
		}

		#endregion

		#region IInBondLineDetailsHeader Members

		IInBondContainer IInBondLineDetailsHeader.Container
		{
			get { return this; }
		}

		IEnumerable<IHazardousMaterial> IInBondLineDetailsHeader.HazardousLines
		{
			get
			{
				foreach (UNDGDataItem undg in UNDGs)
				{
					yield return new HazardousMaterial(undg);
				}
			}
		}

		IEnumerable<IInBondTariffLineDetails> IInBondLineDetailsHeader.TariffLines
		{
			get
			{
				foreach (IInBondTariffLineDetails commodity in GetSortedByTariffAndPartCommodities())
				{
					yield return commodity;
				}
			}
		}

		IEnumerable<CusInBondCargoDesc> GetSortedByTariffAndPartCommodities()
		{
			return Commodities.OfType<CusInBondCargoDesc>().OrderBy(x => CusInBondCargoDescComparer.GetOrderKey(x));
		}

		#endregion

		#region IInBondContainer Members

		ZString IInBondContainer.ContainerDescriptionCode
		{
			get
			{
				RefContainer container = Container;
				return container == null ? ZString.Empty : container.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates);
			}
		}

		ZString IInBondContainer.ContainerNumber
		{
			get { return BC_ContainerNum; }
		}

		ZString IInBondContainer.SealNumber1
		{
			get { return BC_Seal1; }
		}

		ZString IInBondContainer.SealNumber2
		{
			get { return BC_Seal2; }
		}
		ZInt IInBondContainer.PieceCount
		{
			get { return BC_PieceCount; }
		}

		#endregion

		#region IControllerIDProvider Members

		IControllerIDProvider ControllerIDProvider
		{
			get { return Header; }
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

		#region IMessageAttachee Members

		GlbBranch IMessageAttachee.Branch
		{
			get
			{
				CusInBondHeader header = Header;
				return header != null ? header.Branch : null;
			}
		}

		BusinessObjectFactory IMessageAttachee.Factory
		{
			get { return Factory; }
		}

		ZString IMessageAttachee.MessageStatus
		{
			get { return BC_MessageStatus; }
			set { BC_MessageStatus = value; }
		}

		CBPEDIMessageCollection IMessageAttachee.Messages
		{
			get { return Messages; }
		}

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return Header; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return Header.BH_JobReference; }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get
			{
				var header = Header;
				return header != null ? header.Logs : null;
			}
		}

		void IIMessageAttacheeWithDisposition.MarkPreviousDispositionsInactive(ZDateTime dispositionDate)
		{
		}

		#endregion

		#region IDispositionCodeDateParent Members

		CodeDescriptionPairList IDispositionCodeDateParent.DispositionCodeDescriptionList
		{
			get { return Factory.GetCachedValue<DispositionList>(); }
		}

		string IDispositionCodeDateParent.GetDispositionDescriptionBasedOnSource(ZString dispositionSource, ZString code)
		{
			return "";
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
			result.Add(CusAddInfoTypeAttribute.Codes.USDisposition, ObjectFactory.GetType<Integration.Customs.US.IDispositionData>());
			return result;
		}

		#endregion

		#region ICusInBondCargoDescTypeProvider Members
		Type Customs.Business.ICusInBondCargoDescTypeProvider.CusInBondCargoDescType => typeof(CusInBondCargoDesc);
		#endregion

		#region IIMessageAttacheeWithDisposition Members

		[ReadOnly(true)]
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

		void IIMessageAttacheeWithDisposition.UpdateDispositionInformation(ZString dispositionCode, ZDateTime dispositionDate)
		{
			DispositionCodes.AddNewIfNotExist(dispositionCode, dispositionDate);
		}

		#endregion

		#region IResetToOriginal
		public ZString InBondNumber => MoveHeader?.InBondNumber ?? ZString.Empty;

		ZString IResetToOriginal.MovementDescription => MoveHeader?.MovementDescription ?? ZString.Empty;

		ZString IResetToOriginal.CustomsStatus => BC_MessageStatus;

		public StatusLogManager LogManager
		{
			get { return fLogManager ?? (fLogManager = new StatusLogManager(Logs)); }
		}
		StatusLogManager fLogManager;

		void IResetToOriginal.ResetStatus(ZString reason)
		{
			Logs.AddNew(Events.ResetEntryMessageItemFunction, string.Format(CultureInfo.CurrentCulture, ResetStatusMessage, reason));
			BC_MessageStatus = ZString.Empty;
		}

		public const string ResetStatusMessage = "Message Status Reset Reason: {0}.";

		bool IsInBondNumberKnownToCustoms
		{
			get { return IsWaitingForResponse || (!LogManager.HasAWithdrawnLog && LogManager.HasAClearLog); }
		}

		public ZBool IsResetableToOriginal
		{
			get
			{
				var shouldForceAndResetInBondNumber = IsInBondNumberKnownToCustoms;
				return !InBondNumber.IsEmpty && shouldForceAndResetInBondNumber;
			}
		}

		public ZString BillNumber => Bill?.B0_MasterBillNumber ?? ZString.Empty;

		ZString IResetToOriginal.ContainerNumber => BC_ContainerNum;

		ZString IResetToOriginal.Level => "Containers";
		#endregion
	}
}
