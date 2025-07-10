using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New BusinessObject for Container Yard project")]
	public class CYDUnitLineItem : AutoCYDUnitLineItem, ICYDUnitLineItem
	{
		public CYDUnitLineItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("ContainerType")]
		public override ZGuid YLI_RC_ContainerType { get => base.YLI_RC_ContainerType; set => base.YLI_RC_ContainerType = value; }
		public CYDUnitLineItem MovementHeader
		{
			get => Factory.Load<CYDUnitLineItem>(YLI_RC_ContainerType);
		}

		[RelatedBusinessObject("MachineryLineItem")]
		public override ZGuid YLI_YMI_MachineryLineItem { get => base.YLI_YMI_MachineryLineItem; set => base.YLI_YMI_MachineryLineItem = value; }
		public CYDMachineryLineItem MachineryLineItem
		{
			get => Factory.Load<CYDMachineryLineItem>(YLI_YMI_MachineryLineItem);
		}

		#endregion

		#region Implementation

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			YLI_Quantity = 1;
			YLI_RC_ContainerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			YLI_Type = YardUnitType.Codes.Container;
		}
#endif

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion
	}
}
