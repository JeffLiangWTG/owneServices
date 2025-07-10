using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New bizo for Container Yard project")]
	public class CYDReleaseAdviceLine : AutoCYDReleaseAdviceLine
	{
		public CYDReleaseAdviceLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("ReleaseAdvice")]
		public override ZGuid YEL_YRE_ReleaseAdvice { get => base.YEL_YRE_ReleaseAdvice; set => base.YEL_YRE_ReleaseAdvice = value; }

		public CYDReleaseAdvice ReleaseAdvice
		{
			get => Factory.Load<CYDReleaseAdvice>(YEL_YRE_ReleaseAdvice);
		}

		[RelatedBusinessObject("UnitLineItem")]
		public override ZGuid YEL_YLI_UnitLineItem { get => base.YEL_YLI_UnitLineItem; set => base.YEL_YLI_UnitLineItem = value; }

		public CYDUnitLineItem UnitLineItem
		{
			get => Factory.Load<CYDUnitLineItem>(YEL_YLI_UnitLineItem);
		}

		public int TotalPickupQuantity
		{
			get => Factory.Load<CYDPickup>(new ZQuery(CYDPickupSchema.YPL_YEL_ReleaseAdviceLine, PK)).Sum(pickup => pickup.UnitLineItem.YLI_Quantity);
		}

		public int TotalAvailablePickupQuantity
		{
			get => UnitLineItem.YLI_Quantity - TotalPickupQuantity;
		}

		public bool IsAvailableToPickup
		{
			get => TotalPickupQuantity < UnitLineItem.YLI_Quantity;
		}

		#endregion

		#region Implementation

		public override void Delete()
		{
			var pickups = Factory.Load<CYDPickup>(new ZQuery(CYDPickupSchema.YPL_YEL_ReleaseAdviceLine, PK));
			foreach (var pickup in pickups)
			{
				pickup.Delete();
			}

			base.Delete();
		}

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
