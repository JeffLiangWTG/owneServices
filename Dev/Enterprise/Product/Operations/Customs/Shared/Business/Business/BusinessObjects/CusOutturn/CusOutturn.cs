using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[UniversalDataContext(DataContextType.Outturn)]
	public class CusOutturn : AutoCusOutturn
		, Integration.Customs.ICusOutturn
		, ICanDelete
	{
		#region Schema

		public abstract new class Schema : AutoCusOutturn.Schema
		{
			public const string ParentStringRepresentation = "ParentStringRepresentation";
			public const string UnderbondResponsiblePartyID = "UnderbondResponsiblePartyID";
		}

		#endregion

		public CusOutturn(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region New

		public static CusOutturn New(BusinessObjectFactory factory, BusinessObject parent) => New<CusOutturn>(factory, parent);

		protected static T New<T>(BusinessObjectFactory factory, BusinessObject parent) where T : CusOutturn
		{
			var outturn = factory.New<T>();
			outturn.C5_ParentID = parent.PK;
			outturn.C5_ParentTableCode = parent.TablePrefix;
			return outturn;
		}

		#endregion

		#region TypeDecider

		public static readonly CusOutturnTypeDecider TypeDecider = new CusOutturnTypeDecider();

		#endregion

		#region Properties

		#region C5_ReceiptOnlyIndicator

		public override ZBool C5_ReceiptOnlyIndicator
		{
			get { return base.C5_ReceiptOnlyIndicator; }
			set
			{
				bool isDiff = base.C5_ReceiptOnlyIndicator != value;
				base.C5_ReceiptOnlyIndicator = value;
				if (isDiff && !IsCopying && Underbond != null)
				{
					Underbond.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region Responsible Party ID
		public virtual ZString UnderbondResponsiblePartyID
		{
			get { return Underbond != null ? Underbond.C4_Calculated_ResponsiblePartyID : ZString.Empty; }
		}
		public ZPropertyInfo UnderbondResponsiblePartyIDInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.UnderbondResponsiblePartyID);
			}
		}
		#endregion

		#endregion

		#region Business Object Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateParentStringRepresentation();
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (ShouldLogCustomsStatusChangedEvent && !C5_CustomsStatusInfo.OriginalValue.Equals(C5_CustomsStatus))
			{
				var outlinedStatus = GetCustomsOutlinedStatus(C5_CustomsStatus);
				var parameters = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, outlinedStatus),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, C5_CustomsStatus)
				};

				if (ShouldPublishCustomStatusChangeEvent)
				{
					parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Service, CustomsStatusLogSubscriber.PublishCustomsStatusChangedEventService));
				}

				customsEntryStatusLog = Logs.AddNew(Events.CustomsEntryStatus, parameters.ToArray());
			}
		}

		protected virtual ZString GetCustomsOutlinedStatus(ZString cS_CustomsStatus)
		{
			return ZString.Empty;
		}

		StmALog customsEntryStatusLog;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded)
			{
				if (IsInDatabase)
				{
					C5_LastMessageDate = (ZDateTime)C5_LastMessageDateInfo.OriginalValue;
					C5_MessageStatus = (ZString)C5_MessageStatusInfo.OriginalValue;
				}
				else
				{
					C5_LastMessageDate = ZDateTime.Empty;
					C5_MessageStatus = ZString.Empty;
				}

				customsEntryStatusLog?.Delete();
			}

			customsEntryStatusLog = null;
		}

		protected virtual bool ShouldLogCustomsStatusChangedEvent => false;

		protected virtual bool ShouldPublishCustomStatusChangeEvent => false;

		#endregion

		#region Parent Loaders

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

		#endregion

		#region Statuses

		#region CustomsStatus

		public override ZString C5_CustomsStatus
		{
			get
			{
				ZString result = base.C5_CustomsStatus;
				if (UseParentStatus && Parent != null && !Parent.CargoStatus.IsEmpty)
				{
					result = Parent.CargoStatus;
				}

				return result;
			}
		}

		protected virtual bool UseParentStatus
		{
			get { return true; }
		}

		public CusStatus CustomsStatus
		{
			get { return CustomsStatusCore; }
		}

		protected virtual CusStatus CustomsStatusCore
		{
			get { return new CusStatus(C5_CustomsStatusInfo); }
		}

		#endregion

		#region MessageStatus

		public CusStatus MessageStatus
		{
			get { return MessageStatusCore; }
		}

		protected virtual CusStatus MessageStatusCore
		{
			get { return new CusStatus(C5_MessageStatusInfo); }
		}

		#endregion

		#endregion

		#region Related Objects

		#region Messages

		EDIMessageCollection fMessages;
		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this, Factory);
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}

		public bool MessagesHasChanges
		{
			get
			{
				if (fMessages == null)
				{
					var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, PK);
					query.IgnoreActiveFilter = true;
					query.FetchOnlyFromLocalCache = true;
					if (Factory.Load<EDIMessage>(query).Length == 0)
					{
						// no need to load all messages if there is no related changed message
						return false;
					}
				}
				return Messages.HasChanges;
			}
		}

		#endregion

		#region Underbond

		public CusUnderbond Underbond
		{
			get { return (CusUnderbond)Factory.Load(CusUnderbondType, C5_C4_Underbond); }
		}

		protected virtual Type CusUnderbondType
		{
			get { return typeof(CusUnderbond); }
		}

		#endregion

		#region Parent

		IOutturnableLine fParent;
		public virtual IOutturnableLine Parent
		{
			get
			{
				if (fParent == null)
				{
					if (!C5_ParentID.IsEmpty)
					{
						fParent = (IOutturnableLine)ParentLoaders.LoadBusinessObject(Factory, C5_ParentTableCode, C5_ParentID);
					}
				}
				else if (fParent.IsDeleted)
				{
					fParent = null;
				}
				return fParent;
			}
			set
			{
				if (value != null)
				{
					C5_ParentID = value.LinkPK;
					C5_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(value.LinkTableName);
				}
				else
				{
					C5_ParentID = ZGuid.Empty;
					C5_ParentTableCode = ZString.Empty;
				}
			}
		}

		public override ZGuid C5_ParentID
		{
			get { return base.C5_ParentID; }
			set
			{
				base.C5_ParentID = value;
				fParent = null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Validators used via reflection")]
		int ParentStringRepresentationMaxLength
		{
			get { return Underbond != null && Underbond.LinkedObject == null ? Schema.C5_HouseBillMaxLength : 70; }
		}

		[BusinessObjectTestExclude]
		[CargoWise.ComponentModel.MaxLength("ParentStringRepresentationMaxLength")]
		public ZString ParentStringRepresentation
		{
			get
			{
				ZString result = "";
				if (Parent == null)
				{
					if (!C5_HouseBill.IsEmpty)
					{
						result = C5_HouseBill;
					}
				}
				else
				{
					result = Parent.UnderbondHumanReadableName;
				}
				return result;
			}
			set
			{
				bool finished = false;
				if (!value.IsEmpty)
				{
					CheckMaximumLength(ParentStringRepresentationInfo, value);
					if (Underbond != null && Underbond.LinkedObject == null)
					{
						C5_HouseBill = value;
					}
					if (Underbond != null && Underbond.LinkedObject != null)
					{
						foreach (IOutturnableLine possibleParent in Underbond.LinkedObject.OutturnableLines)
						{
							if (possibleParent.UnderbondHumanReadableName == value)
							{
								Parent = possibleParent;
								C5_OuterPacks = possibleParent.PackagesManifested;
								finished = true;
								break;
							}
						}
					}
				}
				if (!finished)
				{
					Parent = null;
				}
				ParentStringRepresentationInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateParentStringRepresentation();
				}
			}
		}

		public ZPropertyInfo ParentStringRepresentationInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.ParentStringRepresentation);
			}
		}

		#endregion

		#endregion

		#region CanDelete

		bool ICanDelete.CanDelete
		{
			get { return Messages.Count == 0; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("f202f681-99c2-43de-a2bc-ac281f37303e", "This outturn line has associated messages which must be retained for auditing purposes. It cannot therefore be deleted."); }
		}

		#endregion

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());
		SetterSuspender setterSuspender;
	}
}
