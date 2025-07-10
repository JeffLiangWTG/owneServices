using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.LandedCosting.Business
{
	[DependentBusinessObject(typeof(LandedCostHistory), "LandedLineCostItems")]
	public class LandedLineCostItem : AutoLandedLineCostItem, Integration.LandedCosting.ILandedLineCostItem, IClusterKeyWorker
	{
		public LandedLineCostItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		[List("Lookups.CostTypeList")]
		public override ZString LZ_CostType
		{
			get { return base.LZ_CostType; }
			set { base.LZ_CostType = value; }
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (LZ_CostAmount.IsEmpty || LZ_CostType.IsEmpty)
			{
				this.Delete();
			}
		}

		#region Implementation of IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)LZ_ClusterKeyInfo;

		Type IClusterKeyWorker.ParentBizObjType => typeof(LandedCostHistory);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)LZ_LHInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion
	}
}
