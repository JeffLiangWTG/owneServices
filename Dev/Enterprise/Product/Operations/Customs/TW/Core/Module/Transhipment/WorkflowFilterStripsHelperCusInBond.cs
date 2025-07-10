using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Transhipment.Module
{
	public class WorkflowFilterStripsHelperCusInBond : WorkflowFilterStripsHelper
	{
		public WorkflowFilterStripsHelperCusInBond(Type businessObjectType, ZString templateCode, BusinessObjectFactory factory)
			: base(businessObjectType, templateCode, factory)
		{
			AlternativeTaskParentColumn = CusInBondHeaderSchema.BH_ParentID;
		}

		public WorkflowFilterStripsHelperCusInBond(Type businessObjectType)
			: base(businessObjectType)
		{
			AlternativeTaskParentColumn = CusInBondHeaderSchema.BH_ParentID;
		}
	}
}
