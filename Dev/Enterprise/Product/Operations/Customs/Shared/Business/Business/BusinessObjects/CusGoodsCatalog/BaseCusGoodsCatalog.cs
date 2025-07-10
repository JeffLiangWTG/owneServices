using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[CodeProperty(nameof(CGC_CatalogCode))]
	[DescriptionProperty(nameof(CGC_Description))]
	[SingleObjectAroundARow()]
	public class BaseCusGoodsCatalog : AutoCusGoodsCatalog, Integration.Customs.ICusGoodsCatalog, ITemplateCopyable, IWorkflowProvider, IDocManagerSupport
	{
		public static readonly TypeDecider TypeDecider = new BaseCusGoodsCatalogTypeDecider();

		public BaseCusGoodsCatalog(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("C27F5A2A-DB4E-4B47-AAB1-79EA40486B62", "Goods Catalog");

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public Type GetProductionInfoType(ZString type) => GetProductionInfoTypeCore(type);

		protected virtual Type GetProductionInfoTypeCore(ZString type) => typeof(BaseCusGoodsCatalogProductionInfo);

		[List(nameof(Lookups) + "." + nameof(CusGoodsCatalogLookups.TypeList))]
		public override ZString CGC_Type { get => base.CGC_Type; set => base.CGC_Type = value; }

		[List(nameof(Lookups) + "." + nameof(CusGoodsCatalogLookups.StatusTypeList))]
		public override ZString CGC_AuthorityStatus { get => base.CGC_AuthorityStatus; set => base.CGC_AuthorityStatus = value; }

		public virtual ZString CGC_AuthorityStatusDescription => Lookups.StatusTypeList.GetDescriptionFromCode(CGC_AuthorityStatus);

		[List(nameof(Lookups) + "." + nameof(CusGoodsCatalogLookups.MessageStatusList))]
		public override ZString CGC_MessageStatus { get => base.CGC_MessageStatus; set => base.CGC_MessageStatus = value; }

		public virtual ZString CGC_MessageStatusDescription => Lookups.MessageStatusList.GetDescriptionFromCode(CGC_MessageStatus);

		public override ZString CGC_Tariff { get => base.CGC_Tariff; set => base.CGC_Tariff = FormatTariffForSaving(value).Left(CGC_TariffInfo.MaxLength); }

		public TariffView UniversalTariff => CGC_Tariff.IsEmpty ? null : new TariffView.Loader(Factory).LoadMostRecentCachedTariff(CustomsCountryCode, UniversalTariffType, CGC_Tariff, ZDateTime.Today);

		public virtual ZString UniversalTariffType => Constants.TariffTypes.HarmonizedSystem;

		public ZString CountryCode => (Company ?? GlbCompany.CurrentCompany).GC_RN_NKCountryCode;

		public virtual ZString CustomsCountryCode => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CountryCode);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CGC_GC_Company = GlbCompany.CurrentCompany.PK;
		}

		IBusiness ITemplateCopyable.TemplateCopy() => Clone();

		protected override bool SupportsCloneCore() => true;

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			yield return Schema.CGC_CatalogCode;
			yield return Schema.CGC_MessageStatus;
			yield return Schema.CGC_AuthorityIdentifier;
			yield return Schema.CGC_AuthorityVersion;
			yield return Schema.CGC_AuthorityStatus;
			yield return Schema.CGC_CustomsStatus;
		}

		#region IWorkflowProvider

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
					workflowItems = this.GetOrCreateProcessTaskCollection(GetNewCusGoodsCatalogTaskCollection);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		protected virtual ProcessTaskCollection GetNewCusGoodsCatalogTaskCollection()
		{
			return new ProcessTaskCollection<CusGoodsCatalogProcessTask, BaseCusGoodsCatalog>(this);
		}

		ZString IWorkflowProviderCore.WorkflowType => WorkflowDescriptors.CusGoodsCatalogWorkflowDescriptorCode;

		public IWorkflowInformationProvider GetWorkflowInformationProvider() => null;

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

		#endregion

		#region Workflow

		public bool SupportsWorkflow => SupportsWorkflowCore;

		protected virtual bool SupportsWorkflowCore => false;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (SupportsWorkflow)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		#endregion

		public override void Delete()
		{
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			this.DeleteChildren<BaseCusGoodsCatalogProductionInfo>(CusGoodsCatalogProductionInfoSchema.CGI_CGC_Catalog);
			base.Delete();
		}

		protected virtual ZString FormatTariffForSaving(ZString unformattedTariff) => unformattedTariff;

		#region IDocManagerSupportMembers

		DocManagerInfo docManagerInfo;

		public DocManagerInfo DocManagerInfo => docManagerInfo ??= new DocManagerInfo(this, Core.Constants.DocManagerCodes.CusGoodsCatalog);

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			CGC_CatalogCode = "0001";
			CGC_Description = "description";
			CGC_Type = GoodsCatalogTypeList.Codes.Import;
			CGC_OH_Owner = Factory.LoadTop1<OrgHeader>(new ZQuery())?.PK ?? ZGuid.Empty;
		}

#endif
	}
}
