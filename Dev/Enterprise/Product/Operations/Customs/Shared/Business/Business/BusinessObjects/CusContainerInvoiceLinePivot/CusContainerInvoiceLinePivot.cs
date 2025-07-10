using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	[CodeProperty(CusContainerInvoiceLinePivot.Schema.ContainerNumber)]
	public class CusContainerInvoiceLinePivot : AutoCusContainerInvoiceLinePivot, Integration.Customs.Shared.ICusContainerInvoiceLinePivot, ITypeDeciderContext, IClusterKeyWorker
	{
		public CusContainerInvoiceLinePivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusContainerInvoiceLinePivot.Schema
		{
			public const string ContainerNumber = "ContainerNumber";
		}

		public override bool SupportsNotes => false;

		public static readonly CusContainerInvoiceLinePivotTypeDecider TypeDecider = new CusContainerInvoiceLinePivotTypeDecider();

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CusContainerInvoiceLinePivotFetchStrategy(this);
		}

		#region Field Overrides
		[RelatedBusinessObject("Container")]
		public override ZGuid C2_CO
		{
			get { return base.C2_CO; }
			set { base.C2_CO = value; }
		}

		[RelatedBusinessObject("InvoiceLine")]
		public override ZGuid C2_JI
		{
			get { return base.C2_JI; }
			set { base.C2_JI = value; }
		}
		#endregion

		[ResourceStringData("Enterprise.Customs.Business.CusContainerInvoiceLinePivot|ContainerNumber", Caption = "Container Number")]
		public ZString ContainerNumber
		{
			get
			{
				var container = Container;
				return container == null ? ZString.Empty : container.CO_ContainerNumber;
			}
		}

		public ZPropertyInfo ContainerNumberInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerNumber); }
		}

		#region Related BO's
		public BaseCusContainer Container
		{
			get { return Factory.Load<BaseCusContainer>(C2_CO); }
		}

		public BaseJobComInvoiceLine InvoiceLine
		{
			get { return Factory.Load<BaseJobComInvoiceLine>(C2_JI); }
		}
		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if (Container == null)
			{
				var containers = InvoiceLine?.Declaration?.CusContainers;
				if (containers != null && containers.Count > 0)
				{
					C2_CO = containers[0].PK;
				}
			}
		}
#endif

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => (Container as ITypeDeciderContext)?.Country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)C2_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(BaseCusContainer);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)C2_COInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion
	}
}
