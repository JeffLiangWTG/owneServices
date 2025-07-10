using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.Business
{
	public class InvoiceLinePackagePivot
		: AutoCusHouseContPackInvoiceLinePivot
		, Integration.Customs.Shared.IInvoiceLinePackagePivot // Note, the only place we need to Spring-load this type is during the crappy unit test BusinessObjectFactoryTest.TestGetBusinessObjectBaseTypeFromTablePrefix().  Never in real life. 
		, ICusPackagePivot
		, ICusQuantityPivot
		, IClusterKeyWorker
	{
		public InvoiceLinePackagePivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new InvoiceLinePackagePivotFetchStrategy(this);
		}

		[RelatedBusinessObject("Package")]
		public override ZGuid CHC_CW
		{
			get { return base.CHC_CW; }
			set
			{
				var oldValue = CHC_CW;
				base.CHC_CW = value;
				if (!IsCopying && oldValue != CHC_CW && InvoiceLine is BaseJobComInvoiceLine invoiceLine)
				{
					invoiceLine.RefreshPackagesPivotMergeKeyIfNeeded();
				}
			}
		}

		public override bool SupportsNotes => false;

		public BasePackage Package
		{
			get { return LoadPackageCore(); }
		}

		protected virtual BasePackage LoadPackageCore()
		{
			return Factory.Load<BasePackage>(CHC_CW);
		}

		[RelatedBusinessObject("InvoiceLine")]
		public override ZGuid CHC_JI
		{
			get { return base.CHC_JI; }
			set { base.CHC_JI = value; }
		}

		public BaseJobComInvoiceLine InvoiceLine
		{
			get { return GetInvoiceLine(); }
		}

		protected virtual BaseJobComInvoiceLine GetInvoiceLine()
		{
			return Factory.Load<BaseJobComInvoiceLine>(CHC_JI);
		}

		[RelatedBusinessObject("Declaration")]
		public override ZGuid CHC_JE
		{
			get { return base.CHC_JE; }
			set { base.CHC_JE = value; }
		}

		public override ZInt CHC_NumberOfPacks
		{
			get => base.CHC_NumberOfPacks;
			set
			{
				base.CHC_NumberOfPacks = value;
				Package?.Validation.ValidateCW_PackQty();
				InvoiceLine?.SyncParentPivotPackNum(Package?.ParentPackage);
			}
		}

		public BaseJobDeclaration Declaration
		{
			get { return Factory.Load<BaseJobDeclaration>(CHC_JE); }
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new InvoiceLinePackagePivotUniqueIndexFailureHandler(this)); }
		}
		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		#region ICusPackagePivot

		ZGuid ICusPackagePivot.PackagePK
		{
			get => CHC_CW;
			set => CHC_CW = value;
		}

		ZGuid ICusPackagePivot.DeclarationPK
		{
			get => CHC_JE;
			set => CHC_JE = value;
		}

		ZGuid ICusPackagePivot.ParentPK
		{
			get => CHC_JI;
		}

		ZInt ICusPackagePivot.NumberOfPacks
		{
			get => CHC_NumberOfPacks;
			set => CHC_NumberOfPacks = value;
		}

		ZDecimal ICusQuantityPivot.Quantity
		{
			get => CHC_Quantity;
			set => CHC_Quantity = value;
		}

		ICusLinkPackageSupporter ICusPackagePivot.PivotSupporter => InvoiceLine;

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CHC_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(BaseJobDeclaration);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CHC_JEInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion
	}
}
