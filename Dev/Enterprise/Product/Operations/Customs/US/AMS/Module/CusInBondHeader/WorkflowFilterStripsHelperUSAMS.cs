using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Module
{
	public class WorkflowFilterStripsHelperUSAMS : WorkflowFilterStripsHelper
	{
		public WorkflowFilterStripsHelperUSAMS(Type businessObjectType, ZString templateCode, BusinessObjectFactory factory)
			: base(businessObjectType, templateCode, factory)
		{
			AlternativeTaskParentColumn = CusInBondHeaderSchema.BH_ParentID;
		}

		public WorkflowFilterStripsHelperUSAMS(Type businessObjectType)
			: base(businessObjectType)
		{
			AlternativeTaskParentColumn = CusInBondHeaderSchema.BH_ParentID;
		}
	}
}
