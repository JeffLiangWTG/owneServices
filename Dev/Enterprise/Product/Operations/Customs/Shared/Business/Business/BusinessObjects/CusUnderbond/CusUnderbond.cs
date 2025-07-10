using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public abstract class CusUnderbond : AutoCusUnderbond, Integration.Customs.ICusUnderbond, IStatusNeedsRecalculationProvider, IDetailsTabPageHeadingProvider
	{
		public abstract new class Schema : AutoCusUnderbond.Schema
		{
			public const string LinkedObjectStringRepresentation = "LinkedObjectStringRepresentation";
			public const string NR_UC__C4_SendersMessageReference = "NR_UC__C4_SendersMessageReference";
			public const string ApprovalStatus = "ApprovalStatus";
		}

		protected CusUnderbond(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CusUnderbond LoadFromSendersReference(BusinessObjectFactory factory, ZString sendersReference)
		{
			return factory.LoadFromNaturalKey<CusUnderbond>(CusUnderbondSchema.C4_SendersMessageReference, sendersReference);
		}

		public static readonly CusUnderbondTypeDecider TypeDecider = new CusUnderbondTypeDecider();

		public void PopulateC4_SendersMessageReferenceIfNeeded()
		{
			PopulateFormattedNumberPropertyIfRequired(C4_SendersMessageReferenceInfo, Env.NumberFountains.CusUnderbondNumberFountain, ignoreInDatabaseCheck: true);
		}

		public override ZBool C4_IsMoveFromDischarge
		{
			get { return base.C4_IsMoveFromDischarge; }
			set
			{
				base.C4_IsMoveFromDischarge = value;
				if (value)
				{
					C4_OA_DischargeAddress = ZGuid.Empty;
					C4_DischargePremiseID = "";
				}
			}
		}

		#region Responsible Party ID

		ZString MatchCCPCode(ZString premiseIDForMatching, ReadOnlyCodeDescriptionPairList list)
		{
			string result = list.GetDescriptionFromCode(premiseIDForMatching)
			?? GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;

			return result;
		}

		public ZString C4_Calculated_ResponsiblePartyID
		{
			get { return MatchCCPCode(C4_DestinationPremiseID, FreightDataRegistry.Instance.OuturnResponsiblePartyIDOverride.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty)); }
		}

		#endregion

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			CusUnderbond underbond = (CusUnderbond)base.CloneInternal(args);
			underbond.C4_SendersMessageReference = ZString.Empty;
			return underbond;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateC4_SendersMessageReferenceIfNeeded();
			if (!IsDeleted && CanDoUBM)
			{
				_ = UnderbondStatus.Code;// do not remove this, touching UnderbondStatus.Code creates CusEntryNum
			}
			if (CanDoOutturn)
			{
				if (!IsDeleted)
				{
					_ = OutturnStatus.Code;// do not remove this, touching OutturnStatus.Code creates CusEntryNum
				}
			}
			else
			{
				if (OutturnStatus != null)
				{
					OutturnStatus.DeleteStatusIfDefaultValue();
				}
			}
		}

		public virtual bool CanDoUBM => true;

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("efc17a3e-7108-4644-8597-b8c4dfdcbefa", "Underbond {0}", C4_SendersMessageReference).Trim(); }
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				if (!IsInDatabase)
				{
					C4_SendersMessageReference = ZString.Empty;
					C4_MessageStatus = ZString.Empty;
				}
				else
				{
					C4_MessageStatus = (ZString)C4_MessageStatusInfo.OriginalValue;
				}
			}
			base.OnSaved(saveSucceeded);
		}

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				if (fUniqueIndexFailureHandler == null)
				{
					fUniqueIndexFailureHandler = new UnderbondNumberFountainUniqueIndexFailureHandler(this);
				}

				yield return fUniqueIndexFailureHandler;
			}
		}

		IUniqueIndexFailureHandler fUniqueIndexFailureHandler;

		protected class UnderbondNumberFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public UnderbondNumberFountainUniqueIndexFailureHandler(CusUnderbond underbond) : base(Schema.NR_UC__C4_SendersMessageReference, underbond)
			{
			}

			protected override INumberFountainProxy NumberFountainToFix
			{
				get { return Env.NumberFountains.CusUnderbondNumberFountain; }
			}
		}

		#endregion

		#region FillWithValidTestData
#if DEBUG

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new CusUnderbondBusinessObjectTestDataHelper();
		}

		class CusUnderbondBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override void PopulateUniqueString(ZPropertyInfo property, PropertyDescriptor[] propertyPath, int maxLength)
			{
				if (property.Name == CusUnderbondSchema.C4_SendersMessageReference.Name)
				{
					base.PopulateUniqueString(property, propertyPath, 20);
				}
				else
				{
					base.PopulateUniqueString(property, propertyPath, maxLength);
				}
			}
		}

#endif
		#endregion

		public ZString Details
		{
			get
			{
				StringBuilder resultBuilder = new StringBuilder();
				StringBuilder extrasStringBuilder = resultBuilder;

				if (LinkedObject != null)
				{
					ZString parentDetails = LinkedObject.Details;
					if (!parentDetails.IsEmpty)
					{
						resultBuilder.Append(parentDetails);
						extrasStringBuilder = new StringBuilder();
					}
				}

				ZString dischargeDetails = GetDetails(DischargeAddress, C4_DischargePremiseID);
				ZString originDetails = GetDetails(OriginAddress, C4_OriginPremiseID);
				ZString destinationDetails = GetDetails(DestinationAddress, C4_DestinationPremiseID);

				if (!dischargeDetails.IsEmpty)
				{
					extrasStringBuilder.Append(Res.GetString("895e6377-e1f4-4c44-b1fe-0575981adfb9", "Discharge Establishment: {0}", dischargeDetails) + "\r\n");
				}

				if (!originDetails.IsEmpty)
				{
					extrasStringBuilder.Append(Res.GetString("6e38d40e-c3cb-4598-bfdd-fe885acce7dd", "Origin Establishment: {0}", originDetails) + "\r\n");
				}

				if (!destinationDetails.IsEmpty)
				{
					extrasStringBuilder.Append(Res.GetString("ebc0d6fe-8c58-4649-b4e1-c5eccec00fce", "Destination Establishment: {0}", destinationDetails) + "\r\n");
				}

				if (!C4_ModeOfMovement.IsEmpty)
				{
					extrasStringBuilder.Append(Res.GetString("c128dc73-684e-4436-9227-881fc305f8c6", "Mode of Movement: {0}", C4_ModeOfMovement) + "\r\n");
				}

				if (!C4_UnderbondBySeaVessel.IsEmpty)
				{
					extrasStringBuilder.Append(Res.GetString("9bbc40bb-df27-4900-98cc-4a32c572de09", "Underbond by Sea Vessel: {0}", C4_UnderbondBySeaVessel) + "\r\n");
				}

				if (!C4_UnderbondBySeaVoyage.IsEmpty)
				{
					extrasStringBuilder.Append(Res.GetString("b23e83b4-5b3b-436c-a91e-4a35afa7b4b2", "Underbond by Sea Voyage: {0}", C4_UnderbondBySeaVoyage) + "\r\n");
				}

				if (extrasStringBuilder != resultBuilder && extrasStringBuilder.Length > 0)
				{
					resultBuilder.Append("\r\n\r\n" + extrasStringBuilder);
				}

				return resultBuilder.ToString();
			}
		}

		ZString GetDetails(OrgAddress address, ZString premiseID)
		{
			ZString result = ZString.Empty;
			if (address != null)
			{
				result = address.Header.OH_FullNameTruncated + " - " + address.OA_Code;
			}

			if (!premiseID.IsEmpty)
			{
				if (!result.IsEmpty)
				{
					result += " - ";
				}

				result += premiseID;
			}
			return result;
		}

		public CusEntryNumber[] CusEntryNumbers
		{
			get
			{
				ZQuery query = new ZQuery(CusEntryNumSchema.CE_ParentID, PK);
				query.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusUnderbond.Schema.TableName);
				query.FetchOnlyFromLocalCache = !IsInDatabase;
				return Factory.Load<CusEntryNumber>(query);
			}
		}

		public override void Delete()
		{
			Outturns.RemoveAndDeleteAll();
			foreach (var cusEntryNumber in CusEntryNumbers)
			{
				cusEntryNumber.Delete();
			}
			base.Delete();
		}

		public virtual bool VoyageAndVesselDetailsVisible
		{
			get
			{
				return true;
			}
		}

		public bool TranshipmentPortVisible
		{
			get { return TranshipmentPortVisibleCore(); }
		}

		protected virtual bool TranshipmentPortVisibleCore()
		{
			return true;
		}

		[MaxLength(1024)]
		public virtual ZString ApprovalStatus
		{
			get
			{
				return "";
			}
		}

		public ZPropertyInfo ApprovalStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ApprovalStatus); }
		}

		public CusEntryNumStatus underbondStatus;
		public CusEntryNumStatus UnderbondStatus
		{
			get
			{
				if (underbondStatus == null)
				{
					underbondStatus = new CusEntryNumStatus(this, Lookups.UnderbondStatusList, DefaultUnderbondStatus, CusEntryNumber.EntryType.UnderbondStatus, CountryCode);
				}
				return underbondStatus;
			}
		}

		protected virtual internal ZString DefaultUnderbondStatus
		{
			get { return ZString.Empty; }
		}

		protected virtual CusEntryNumStatus OutturnStatusCore
		{
			get { return new CusEntryNumStatus(this, Lookups.OutturnStatusList, DefaultOutturnStatus, CusEntryNumber.EntryType.OutturnStatus, CountryCode); }
		}

		public CusEntryNumStatus OutturnStatus
		{
			get { return _outturnStatusCore ?? (_outturnStatusCore = OutturnStatusCore); }
		}
		CusEntryNumStatus _outturnStatusCore;

		protected virtual internal ZString DefaultOutturnStatus
		{
			get { return ZString.Empty; }
		}

		protected virtual ZString CountryCode => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		public bool CanDoOutturn
		{
			get
			{
				return GetCanDoOutturn();
			}
		}

		public event EventHandler CanDoOutturnChanged;

		protected internal void FireCanDoOutturnChangedEvent()
		{
			if (CanDoOutturnChanged != null)
			{
				CanDoOutturnChanged(this, new EventArgs());
			}
		}

		protected virtual bool GetCanDoOutturn()
		{
			return false;
		}

		public bool IsUnderbondForSeaShipment
		{
			get { return GetIsUnderbondForSeaShipment(); }
		}

		public virtual void SetDefaultValuesFromParent()
		{
		}

		protected virtual bool GetIsUnderbondForSeaShipment()
		{
			return false;
		}

		public bool IsLastMessageDateSupported
		{
			get { return GetIsLastMessageDateSupported(); }
		}

		protected virtual bool GetIsLastMessageDateSupported()
		{
			return false;
		}

		TypeLoaderCollection fParentLoaders;
		protected TypeLoaderCollection ParentLoaders
		{
			get
			{
				if (fParentLoaders == null)
				{
					fParentLoaders = GetParentLoaders();
				}
				return fParentLoaders;
			}
		}

		protected virtual TypeLoaderCollection GetParentLoaders()
		{
			return new TypeLoaderCollection();
		}

		#region Collections

		EDIMessageCollection fMessages;
		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = GetNewEDIMessageCollection();
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}

		protected virtual EDIMessageCollection GetNewEDIMessageCollection()
		{
			return new EDIMessageCollection(this, Factory);
		}

		[ChildEditable(true)]
		public CusUnderbondCusOutturnCollection Outturns
		{
			get
			{
				if (fOutturns == null)
				{
					fOutturns = GetNewOutturnCollection();
					fOutturns.Load();
					RegisterEditableChildObject(fOutturns);
				}
				return fOutturns;
			}
		}
		CusUnderbondCusOutturnCollection fOutturns;

		protected virtual CusUnderbondCusOutturnCollection GetNewOutturnCollection()
		{
			return new CusUnderbondCusOutturnCollection(this);
		}

		#endregion

		#region Related Business Objects

		public ICusUnderbondDependentCollectionParent LinkedObject
		{
			get { return linkedObject != null && linkedObject.LinkPK == C4_ParentID ? linkedObject : (linkedObject = C4_ParentID.IsValid ? (ICusUnderbondDependentCollectionParent)ParentLoaders.LoadBusinessObject(Factory, C4_ParentTableCode, C4_ParentID) : null); }
			set
			{
				ParentLoaders.SetTablePrefixAndPK(value as BusinessObject, C4_ParentTableCodeInfo, C4_ParentIDInfo);
				linkedObject = value;
				LinkedObjectStringRepresentationInfo.RefreshBinding();
			}
		}
		ICusUnderbondDependentCollectionParent linkedObject;

		[MaxLength(25)]
		public ZString LinkedObjectStringRepresentation
		{
			get
			{
				return LinkedObject?.UnderbondHumanReadableName ?? ZString.Empty;
			}
		}

		public ZPropertyInfo LinkedObjectStringRepresentationInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.LinkedObjectStringRepresentation);
			}
		}

		public virtual ICusUnderbondDependentCollectionParent[] GetAllPossibleCollectionProviders()
		{
			return Array.Empty<ICusUnderbondDependentCollectionParent>();
		}

		#endregion

		#region Overridden Properties

		public override ZGuid C4_ParentID
		{
			get => base.C4_ParentID;
			set
			{
				if (C4_ParentID != value)
				{
					base.C4_ParentID = value;
					linkedObject = null;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusUnderbondLookups.TranshipBySeaVessels))]
		public override ZString C4_TranshipBySeaVessel
		{
			get => base.C4_TranshipBySeaVessel;
			set => base.C4_TranshipBySeaVessel = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusUnderbondLookups.UnderbondBySeaVessels))]
		public override ZString C4_UnderbondBySeaVessel
		{
			get => base.C4_UnderbondBySeaVessel;
			set => base.C4_UnderbondBySeaVessel = value;
		}

		[BusinessObjectTestExclude]
		public override ZString C4_ParentTableCode
		{
			get { return base.C4_ParentTableCode; }
			set
			{
				if (C4_ParentTableCode != value)
				{
					base.C4_ParentTableCode = value;
					linkedObject = null;
				}
			}
		}

		public override ZString C4_DischargePremiseID
		{
			get
			{
				if (DischargeAddress != null)
				{
					return DischargeAddress.LocalControlledPremisesID;
				}
				else
				{
					return base.C4_DischargePremiseID;
				}
			}
			set
			{
				base.C4_DischargePremiseID = value;
			}
		}

		protected bool C4_DischargePremiseID_ReadOnly
		{
			get { return !C4_OA_DischargeAddress.IsEmpty; }
		}

		public override ZString C4_OriginPremiseID
		{
			get
			{
				if (OriginAddress != null)
				{
					return OriginAddress.LocalControlledPremisesID;
				}
				else
				{
					return base.C4_OriginPremiseID;
				}
			}
			set
			{
				base.C4_OriginPremiseID = value;
			}
		}

		protected bool C4_OriginPremiseID_ReadOnly
		{
			get { return !C4_OA_OriginAddress.IsEmpty; }
		}

		public override ZString C4_DestinationPremiseID
		{
			get
			{
				if (DestinationAddress != null)
				{
					return DestinationAddress.LocalControlledPremisesID;
				}
				else
				{
					return base.C4_DestinationPremiseID;
				}
			}
			set
			{
				base.C4_DestinationPremiseID = value;
			}
		}

		protected bool C4_DestinationPremiseID_ReadOnly
		{
			get { return !C4_OA_DestinationAddress.IsEmpty; }
		}

		[ReadOnly(true)]
		public override ZString C4_SendersMessageReference
		{
			get { return base.C4_SendersMessageReference; }
			set { base.C4_SendersMessageReference = value; }
		}

		public override ZGuid C4_OA_OriginAddress
		{
			get { return base.C4_OA_OriginAddress; }
			set
			{
				base.C4_OA_OriginAddress = value;
				Validation.ValidateC4_OriginPremiseID();
			}
		}

		public override ZGuid C4_OA_DestinationAddress
		{
			get { return base.C4_OA_DestinationAddress; }
			set
			{
				base.C4_OA_DestinationAddress = value;
				Validation.ValidateC4_DestinationPremiseID();
			}
		}

		#endregion

		#region IStatusNeedsRecalculationProvider Members

		public bool StatusNeedsRecalculation
		{
			get
			{
				return fMessages != null && Messages.HasChanges;
			}
		}

		#endregion

		#region IStmALogParent Members

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>();
				result.AddRange(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(Outturns);

				return result.ToArray();
			}
		}

		#endregion
		#region IDetailsTabPageHeadingProvider Members

		string IDetailsTabPageHeadingProvider.Heading
		{
			get { return Res.GetString("866b0886-680d-4f31-b0df-c1e30c9ffbe7", "Underbond {0}", C4_SendersMessageReference).Trim(); }
		}

		#endregion

		#region ICusUnderbond Members

		BusinessObject Integration.Customs.ICusUnderbond.LinkedObject
		{
			get
			{
				return LinkedObject as BusinessObject;
			}
			set
			{
				LinkedObject = value as ICusUnderbondDependentCollectionParent;
			}
		}

		#endregion
	}
}
