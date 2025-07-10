using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.eTail.Business
{
	[CodeProperty(nameof(HVO_PackageReference))]
	[DescriptionProperty(nameof(HVO_PackageReference))]
	[UniversalDataContext(DataContextType.HVLVOuterPackage)]
	public class HVLVOuterPackage : AutoHVLVOuterPackage,
		IDocumentSupportable,
		IDocManagerSupport,
		IHVLVOuterPackage,
		IWorkflowProvider,
		IJobNumber
	{
		public HVLVOuterPackage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public HVLVOriginLoadList LoadList => Factory.Load<HVLVOriginLoadList>(HVO_HVL_LoadList);

		[List("Lookups.HVO_HVL_LoadList_List")]
		[RelatedBusinessObject(nameof(LoadList))]
		public override ZGuid HVO_HVL_LoadList
		{
			get { return base.HVO_HVL_LoadList; }
			set
			{
				SetPropertyValue(HVO_HVL_LoadListInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateHVO_HVL_LoadList();
				}
			}
		}

		public override ZString HVO_UnitOfDimension
		{
			get => base.HVO_UnitOfDimension;
			set => base.HVO_UnitOfDimension = value.ToUpper();
		}

		public override ZString HVO_VolumeUQ
		{
			get => base.HVO_VolumeUQ;
			set => base.HVO_VolumeUQ = value.ToUpper();
		}

		public override ZString HVO_WeightUQ
		{
			get => base.HVO_WeightUQ;
			set => base.HVO_WeightUQ = value.ToUpper();
		}

		[ChildEditable]
		public HVLVOuterPackageItemCollection Items
		{
			get
			{
				if (items == null)
				{
					items = new HVLVOuterPackageItemCollection(this);
					items.Load();
					RegisterEditableChildObject(items);
				}

				return items;
			}
		}

		HVLVOuterPackageItemCollection items;

		public IEnumerable<HVLVItem> ActiveItems => Items.OfType<HVLVItem>().Where(item => item.HVI_IsActive);

		#region IDocumentSupportable

		public DocumentSupporter DocumentSupporter => new HVLVOuterPackageDocumentSupporter(this);

		#endregion

		#region IDocManagerSupport

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.HVLVOuterPackage));
		DocManagerInfo docManagerInfo;

		#endregion

		#region GS1 Prefix

		string GenerateSSCCNumber()
		{
			var gs1Wrapper = GS1Wrapper.GetGS1Info(null, Factory);
			return gs1Wrapper?.SSCCNumberFountain?.GetNextFormatted(Factory);
		}

		IEnumerable<ZString> GetPackageBarcodeCandidates()
		{
			yield return GenerateSSCCNumber();
			yield return Env.NumberFountains.HVLVOuterPackageBarcode.GetNextFormatted(Factory);
		}

		#endregion

		#region Overrides

		public override void OnSaving()
		{
			base.OnSaving();

			if (HVO_PackageBarcode.IsEmpty && !IsInDatabase)
			{
				HVO_PackageBarcode = GetPackageBarcodeCandidates().First(x => !x.IsEmpty);
			}
		}

		public override void Delete()
		{
			WorkflowItems.DeleteAll();
			base.Delete();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			base.OnFactorySavingBeforeTransactionCore();
		}

		#endregion

		#region IWorkflowProvider

		public ZString WorkflowType => WorkflowDescriptors.HVLVOuterPackageWorkflowDescriptorCode;

		public IWorkflowInformationProvider GetWorkflowInformationProvider() => null;

		public IColumnValueRanker GetTemplateSelectionCriteria() => new ColumnValueRanker();

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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new HVLVOuterPackageProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}

				return workflowItems;
			}
		}

		HVLVOuterPackageProcessTaskCollection workflowItems;

		#endregion

		#region IHVLVOuterPackage

		IOrgHeader IHVLVOuterPackage.LastMileCarrier => LastMileCarrier;

		IHVLVOriginLoadList IHVLVOuterPackage.LoadList => LoadList;

		IOrgHeader IHVLVOuterPackage.Owner => Owner;

		IEnumerable<IHVLVItem> IHVLVOuterPackage.ActiveItems => ActiveItems;

		IHVLVItemCollection IHVLVOuterPackage.Items => Items;

		#endregion

		#region IJobNumber

		public string JobNumber => HVO_PackageBarcode;

		#endregion
	}
}
