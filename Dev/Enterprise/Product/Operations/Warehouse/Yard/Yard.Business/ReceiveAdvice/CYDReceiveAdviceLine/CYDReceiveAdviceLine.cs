using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New bizo for Container Yard project")]
	public class CYDReceiveAdviceLine : AutoCYDReceiveAdviceLine
	{
		public CYDReceiveAdviceLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("ReceiveAdvice")]
		public override ZGuid YRL_YRA_ReceiveAdvice { get => base.YRL_YRA_ReceiveAdvice; set => base.YRL_YRA_ReceiveAdvice = value; }

		public CYDReceiveAdvice ReceiveAdvice
		{
			get => Factory.Load<CYDReceiveAdvice>(YRL_YRA_ReceiveAdvice);
		}

		[RelatedBusinessObject("UnitLineItem")]
		public override ZGuid YRL_YLI_UnitLineItem { get => base.YRL_YLI_UnitLineItem; set => base.YRL_YLI_UnitLineItem = value; }

		public CYDUnitLineItem UnitLineItem
		{
			get => Factory.Load<CYDUnitLineItem>(YRL_YLI_UnitLineItem);
		}

		#endregion

		#region Implementation

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			UnitLineItem.YLI_Quantity = 1;
			UnitLineItem.YLI_RC_ContainerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			UnitLineItem.YLI_Type = YardUnitType.Codes.Container;
		}
#endif

		#endregion
	}
}
