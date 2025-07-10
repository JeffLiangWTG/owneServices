using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[UniversalDataContext(DataContextType.SeaOceanBill)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public abstract class BaseCusSCAOceanBill : AutoCusSCAOceanBill, Integration.Customs.Shared.IBaseCusSCAOceanBill, IUniversalXMLNoteParent, ISynchroniserReadOnlyMembersProvider, IBranchProvider, IDocManagerSupport, IJobNumber, IWorkflowProvider
	{
		protected BaseCusSCAOceanBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly TypeDecider TypeDecider = new BaseCusSCAOceanBillTypeDecider();

		public static TriLockMutex CreateMutexForConsol(ZGuid consolPK, string countryCode)
		{
			Argument.NotNull(countryCode, nameof(countryCode));
			return new TriLockMutex(MutexIDs.CusSCAOceanBillJobBeingCreatedForConsol, consolPK.ToString() + countryCode);
		}

		public abstract ZString[] ApplicationCodesForBase { get; }

		[ReadOnly(true)]
		public override ZString CB_ApplicationCode
		{
			get { return base.CB_ApplicationCode; }
		}

		[List(nameof(Lookups) + "." + nameof(CusSCAOceanBillLookups.VesselNames))]
		public override ZString CB_VesselName { get => base.CB_VesselName; set => base.CB_VesselName = value; }

		public virtual RefVessel VesselName
		{
			get
			{
				RefVessel result = null;
				var vessels = Factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, CB_VesselName));
				if (vessels != null && vessels.Length == 1)
				{
					result = vessels[0];
				}
				return result;
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (CB_ParentId.IsValid && CB_ParentTableCode == JobConsolSchema.Constants.Prefix && !IsInDatabase && ShouldCheckOceanBillWithSameConsolExist)
			{
				LockConsol(CB_ParentId);
				if (OceanBillExistInDatabase)
				{
					throw new DuplicatedOceanBillException();
				}
			}
		}

		protected virtual ZBool ShouldCheckOceanBillWithSameConsolExist => true;

		protected bool OceanBillExistInDatabase
		{
			get
			{
				var query = new ZQuery(CusSCAOceanBillSchema.CB_ApplicationCode, ApplicationCodesForBase);
				query.AddToFilter(CusSCAOceanBillSchema.CB_ParentTableCode, JobConsolSchema.Constants.Prefix);
				query.AddToFilter(CusSCAOceanBillSchema.CB_ParentId, CB_ParentId);
				return Factory.ExistsInDatabase(BaseCusSCAOceanBill.Schema.TableName, query);
			}
		}

		protected void LockConsol(ZGuid pK)
		{
			string sQLText = "SELECT " + ForwardingConsol.Schema.PK
				+ " FROM "
				+ ForwardingConsol.Schema.TableName
				+ " with (UPDLOCK, ROWLOCK) WHERE "
				+ ForwardingConsol.Schema.PK + " = '" + pK.ToString() + "'";

			var cmd = CargoWise.Data.Db.Connection.Command(sQLText);
			cmd.ExecuteNonQuery();
		}

		public ForwardingConsol Consol
		{
			get
			{
				if (CB_ParentTableCode == JobConsolSchema.Constants.Prefix)
				{
					if (consol == null)
					{
						consol = new CachedProperty<ForwardingConsol>(Factory, delegate
						{ return Factory.Load<ForwardingConsol>(CB_ParentId); });
					}
					return consol.Value;
				}
				else
				{
					return null;
				}
			}
		}
		CachedProperty<ForwardingConsol> consol;

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = Res.GetString("ae9c4b68-5474-4c81-a48a-faf975f61611", "Sea Cargo Report");
				var parameters = new ZStringBuilder();
				parameters.AppendIfNotEmpty(Res.GetString("5feb376c-ebae-435e-aed9-a9663225fd2b", "OBL") + ": ", CB_OceanBill);
				parameters.AppendIfNotEmpty(Res.GetString("f44f4de0-f7f2-4c18-81c2-59c46c53aa75", "MHB") + ": ", CB_MasterHouseBill);
				if (!parameters.IsEmpty)
				{
					result += " (" + parameters.ToStringWithDelimiterBetweenAppends(" ") + ")";
				}
				return result;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CB_GB = GlbBranch.CurrentBranch.PK;
		}

		protected internal virtual bool IsCrossCompanyRecord
		{
			get { return false; }
		}

		public BusinessObject EffectiveResponsiblePartyOrgHeader
		{
			get { return GetEffectiveResponsiblePartyOrgHeader(); }
		}

		protected virtual OrgHeader GetEffectiveResponsiblePartyOrgHeader()
		{
			return null;
		}

		public ZString ReasonEffectiveResponsiblePartyIsUnavailable
		{
			get { return GetReasonEffectiveResponsiblePartyIsUnavailable(); }
		}

		protected virtual ZString GetReasonEffectiveResponsiblePartyIsUnavailable()
		{
			var result = ZString.Empty;
			if (EffectiveResponsiblePartyOrgHeader == null)
			{
				result = Res.GetString("F52E144E-9014-4633-A16E-ACC576835719", "Responsible party is not specified");
			}
			return result;
		}

		#region Loader
		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public BaseCusSCAOceanBill LoadFromConsolAndApplicationCode(CommonConsol consol, ZString[] applicationCode)
			{
				var query = GetConsolFilter(consol);
				query.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, applicationCode);
				return Factory.LoadTop1<BaseCusSCAOceanBill>(query);
			}

			public BaseCusSCAOceanBill[] LoadFromConsol(CommonConsol consol)
			{
				return Factory.Load<BaseCusSCAOceanBill>(GetConsolFilter(consol));
			}

			ZQuery GetConsolFilter(CommonConsol consol)
			{
				var result = new ZQuery(CusSCAOceanBillSchema.CB_ParentId, consol.PK);
				result.AddToFilter(CusSCAOceanBillSchema.CB_ParentTableCode, JobConsolSchema.Constants.Prefix);
				result.FetchOnlyFromLocalCache = !consol.IsInDatabase;
				result.ReLoadExistingRows = true;
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(BaseCusSCAOceanBill);
			}
		}
		#endregion

		public virtual void DefaultFromConsol()
		{
			throw new NotImplementedException("Must implement in subclass");
		}

		public virtual void LoadHouseBills()
		{
			throw new NotImplementedException("Must implement in subclass");
		}

		#region Workflow

		public bool SupportsWorkflow
		{
			get { return SupportsWorkflowCore; }
		}

		protected virtual bool SupportsWorkflowCore
		{
			get { return false; }
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			var consol = Consol;
			if (consol != null && consol.IsCancelled != IsCancelled)
			{
				IsCancelled = consol.IsCancelled;
			}

			if (SupportsWorkflow && Consol == null)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		#endregion

		#region IWorkflowProvider

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return workflowInformationProvider ?? (workflowInformationProvider = new CusSCAOceanBillWorkflowInformationProvider(this));
		}
		IWorkflowInformationProvider workflowInformationProvider;

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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(GetNewCusSCAOceanBillProcessTaskCollection);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		protected virtual ProcessTaskCollection GetNewCusSCAOceanBillProcessTaskCollection()
		{
			return new ProcessTaskCollection<CusSCAOceanBillProcessTask, BaseCusSCAOceanBill>(this);
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return GetTemplateSelectionCriteria();
		}

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			return GetTemplateSelectionCriteriaCore();
		}

		protected virtual IColumnValueRanker GetTemplateSelectionCriteriaCore()
		{
			return new ColumnValueRanker();
		}

		ZString IWorkflowProviderCore.WorkflowType => WorkflowType;

		public ZString WorkflowType => WorkflowTypeCore;

		protected virtual ZString WorkflowTypeCore
		{
			get { return CusSCAOceanBillWorkflowDescriptor.Constants.Code; }
		}

		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = GetDocManagerInfo());

		public string JobNumber => CB_MessageReference;

		DocManagerInfo docManagerInfo;

		protected abstract DocManagerInfo GetDocManagerInfo();

		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion
	}

	[Serializable]
	public class DuplicatedOceanBillException : ApplicationException
	{
		public DuplicatedOceanBillException()
			: base()
		{
		}

#if NETFRAMEWORK
		protected DuplicatedOceanBillException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

#if DEBUG
	public class TestCusSCAOceanBill : BaseCusSCAOceanBill
	{
		public TestCusSCAOceanBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString[] ApplicationCodesForBase
		{
			get
			{
				return new ZString[] { Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea, Core.Constants.Customs.CusSCAOceanBillApplicationCodes.BaseTesting };
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CB_GB = GlbBranch.CurrentBranch.PK;
			CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.BaseTesting;
		}

		protected override DocManagerInfo GetDocManagerInfo() => new DocManagerInfo(this, Core.Constants.DocManagerCodes.SCAOceanBill);
	}
#endif
}
