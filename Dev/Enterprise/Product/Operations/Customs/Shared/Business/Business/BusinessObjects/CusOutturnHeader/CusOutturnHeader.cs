using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business
{
	[UniversalDataContext(DataContextType.SeaCargoOutturn)]
	public class CusOutturnHeader : AutoCusOutturnHeader, ICusOutturnHeader, IStatusNeedsRecalculationProvider, ISendersMessageReferenceProvider, IDocManagerSupport, IWorkflowProvider
	{
		public CusOutturnHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Static

		public static CusOutturnHeader LoadFromSendersReference(BusinessObjectFactory factory, ZString sendersReference)
		{
			return factory.LoadTop1<CusOutturnHeader>(new ZQuery(Enterprise.ZArchitecture.Schema.CusOutturnHeaderSchema.C6_SendersMessageReference, sendersReference));
		}

		public static readonly CusOutturnHeaderTypeDecider TypeDecider = new CusOutturnHeaderTypeDecider();

		#endregion

		public ZString CustomsCountryCode => GlbCompany.CurrentCompany.GC_RN_NKCountryCode; // For now we will rely current login company; once we introduce _GB or _ApplicationCode then we should changed to reflect correctly.

		#region Collections

		#region Outturns

		[ChildEditable(true)]
		public CusOutturnHeaderCusOutturnCollection Outturns
		{
			get
			{
				if (fOutturns == null)
				{
					fOutturns = GetNewOutturns();
					fOutturns.Load();
					RegisterEditableChildObject(fOutturns);
				}

				return fOutturns;
			}
		}
		CusOutturnHeaderCusOutturnCollection fOutturns;

		protected virtual CusOutturnHeaderCusOutturnCollection GetNewOutturns()
		{
			return new CusOutturnHeaderCusOutturnCollection(this);
		}

		#endregion

		#endregion

		#region override

		[List(nameof(Lookups) + "." + nameof(CusOutturnHeaderLookups.VesselNames))]
		public override ZString C6_VesselName { get => base.C6_VesselName; set => base.C6_VesselName = value; }

		public virtual RefVessel VesselName
		{
			get
			{
				RefVessel result = null;
				var vessels = Factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, C6_VesselName));
				if (vessels != null && vessels.Length == 1)
				{
					result = vessels[0];
				}
				return result;
			}
		}
		#endregion

		#region Business Object Overrides

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

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public override void Delete()
		{
			Outturns.RemoveAndDeleteAll();
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			((ISendersMessageReferenceProvider)this).PopulateSendersReferenceIfNeeded();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded)
			{
				if (!IsInDatabase)
				{
					C6_SendersMessageReference = ZString.Empty;
					C6_MessageStatus = ZString.Empty;
				}
				else
				{
					C6_MessageStatus = (ZString)C6_MessageStatusInfo.OriginalValue;
				}
			}
		}

		#endregion

		#region IStatusNeedsRecalculationProvider Members

		public bool StatusNeedsRecalculation
		{
			get { return Messages.HasChanges; }
		}

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
		EDIMessageCollection fMessages;

		#endregion

		#region ISendersMessageReferenceProvider Members

		void ISendersMessageReferenceProvider.PopulateSendersReferenceIfNeeded()
		{
			PopulateFormattedNumberPropertyIfRequired(C6_SendersMessageReferenceInfo, Env.NumberFountains.CusOutturnHeader, ignoreInDatabaseCheck: true);
		}

		ZString ISendersMessageReferenceProvider.SendersReference
		{
			get { return C6_SendersMessageReference; }
		}

		#endregion

		#region IDocManagerSupportMembers

		public DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new CusOutturnHeaderDocManagerInfo(this, Enterprise.Core.Constants.DocManagerCodes.CusOutturnHeader)); }
		}

		CusOutturnHeaderDocManagerInfo docManagerInfo;

		#endregion

		#region IWorkflowProvider Members

		public IWorkflowInformationProvider GetWorkflowInformationProvider() => null;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria() => new ColumnValueRanker();

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(GetCusOutturnHeaderProcessTaskCollection);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		protected virtual ProcessTaskCollection GetCusOutturnHeaderProcessTaskCollection()
		{
			return new CusOutturnHeaderProcessTaskCollection(this);
		}

		public ZString WorkflowType => WorkflowDescriptors.SeaCargoOutturnWorkflowDescriptorCode;

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());
		SetterSuspender setterSuspender;
	}
}
