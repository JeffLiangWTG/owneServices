using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[ModuleID(ModuleId.ExportClassification)]
	public class ExportClassificationCollection : Customs.Business.BaseClassificationCollection<CusClassification>
	{
		public ExportClassificationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((CusClassification)child).CC_ClassificationType = CusClassification.ClassificationType.EXP;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(CusClassificationSchema.CC_ClassificationType, CusClassification.ClassificationType.EXP);
			return result;
		}
	}
}
