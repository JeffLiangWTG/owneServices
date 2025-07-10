using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New bizo for Container Yard project")]
	public class MNRServiceTemplate : AutoMNRServiceTemplate
	{
		public MNRServiceTemplate(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("Yard")]
		public override ZGuid MST_WW_Yard { get => base.MST_WW_Yard; set => base.MST_WW_Yard = value; }

		public WhsWarehouse Yard
		{
			get => Factory.Load<WhsWarehouse>(MST_WW_Yard);
		}

		#endregion

		#region Implementation

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			MST_Name = "TestName";
			MST_Description = "Test";
			MST_ContainerType = "FLT";
		}
#endif

		#endregion
	}
}
