using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.Business
{
	public class InvoiceHeaderPackagePivot : AutoCusHouseContPackInvoiceHeaderPivot
		, Integration.Customs.Shared.IInvoiceHeaderPackagePivot
		, ICusPackagePivot
		, IClusterKeyWorker
	{
		public InvoiceHeaderPackagePivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("Package")]
		public override ZGuid CHZ_CW
		{
			get { return base.CHZ_CW; }
			set
			{
				var oldValue = base.CHZ_CW;

				if (oldValue != value)
				{
					base.CHZ_CW = value;
					ClearPivotsOnChildInvoiceLine();
				}
			}
		}

		public override bool SupportsNotes => false;

		public BasePackage Package
		{
			get { return Factory.Load<BasePackage>(CHZ_CW); }
		}

		[RelatedBusinessObject("Declaration")]
		public override ZGuid CHZ_JE
		{
			get { return base.CHZ_JE; }
			set { base.CHZ_JE = value; }
		}

		public BaseJobDeclaration Declaration
		{
			get { return Factory.Load<BaseJobDeclaration>(CHZ_JE); }
		}

		[RelatedBusinessObject("InvoiceHeader")]
		public override ZGuid CHZ_JZ
		{
			get { return base.CHZ_JZ; }
			set
			{
				var oldValue = base.CHZ_JZ;

				if (oldValue != value)
				{
					base.CHZ_JZ = value;
					ClearPivotsOnChildInvoiceLine();
				}
			}
		}

		public BaseJobComInvoiceHeader InvoiceHeader
		{
			get { return Factory.Load<BaseJobComInvoiceHeader>(CHZ_JZ); }
		}

		void ClearPivotsOnChildInvoiceLine()
		{
			var invoice = InvoiceHeader;
			var package = Package;

			if (invoice != null && package != null)
			{
				var pivotsWithSamePackageOnLine = invoice.InvoiceLines
					.Cast<BaseJobComInvoiceLine>()
					.Select(c => c.PackagesPivot)
					.ToArray();

				foreach (var pivotCollection in pivotsWithSamePackageOnLine)
				{
					var pivotsForDelete = pivotCollection
						.Cast<InvoiceLinePackagePivot>()
						.Where(c => c.CHC_CW == package.PK)
						.ToArray();

					pivotsForDelete.ForEach(c => pivotCollection.RemoveAndDelete(c));
				}
			}
		}

		#region ICusPackagePivot

		ZGuid ICusPackagePivot.PackagePK
		{
			get => CHZ_CW;
			set => CHZ_CW = value;
		}

		ZGuid ICusPackagePivot.DeclarationPK
		{
			get => CHZ_JE;
			set => CHZ_JE = value;
		}

		ZGuid ICusPackagePivot.ParentPK
		{
			get => CHZ_JZ;
		}

		ZInt ICusPackagePivot.NumberOfPacks
		{
			get => CHZ_NumberOfPacks;
			set
			{
				CHZ_NumberOfPacks = value;
				InvoiceHeader?.SyncParentPivotPackNum(Package?.ParentPackage);
			}
		}

		ICusLinkPackageSupporter ICusPackagePivot.PivotSupporter => InvoiceHeader;

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new InvoiceHeaderPackagePivotFetchStrategy(this);
		}

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CHZ_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(BaseJobDeclaration);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CHZ_JEInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion
	}
}
