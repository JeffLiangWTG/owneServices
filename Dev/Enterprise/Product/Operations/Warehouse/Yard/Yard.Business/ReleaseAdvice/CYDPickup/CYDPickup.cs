using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New bizo for Container Yard project")]
	[UniversalDataContext(DataContextType.CYDPickup)]
	public class CYDPickup : AutoCYDPickup, ICYDPickup, IProcessHandlingInfoProvider
	{
		public CYDPickup(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("PickupHeader")]
		public override ZGuid YPL_YPH_PickupHeader { get => base.YPL_YPH_PickupHeader; set => base.YPL_YPH_PickupHeader = value; }

		public CYDPickupHeader PickupHeader
		{
			get => Factory.Load<CYDPickupHeader>(YPL_YPH_PickupHeader);
		}

		[RelatedBusinessObject("ReleaseAdviceLine")]
		public override ZGuid YPL_YEL_ReleaseAdviceLine { get => base.YPL_YEL_ReleaseAdviceLine; set => base.YPL_YEL_ReleaseAdviceLine = value; }

		public CYDReleaseAdviceLine ReleaseAdviceLine
		{
			get => Factory.Load<CYDReleaseAdviceLine>(YPL_YEL_ReleaseAdviceLine);
		}

		public CYDYardUnitState LinkedYardUnit
		{
			get => Factory.LoadTop1<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_YPL_Pickup, PK));
		}

		[RelatedBusinessObject("UnitLineItem")]
		public override ZGuid YPL_YLI_UnitLineItem { get => base.YPL_YLI_UnitLineItem; set => base.YPL_YLI_UnitLineItem = value; }

		public CYDUnitLineItem UnitLineItem
		{
			get => Factory.Load<CYDUnitLineItem>(YPL_YLI_UnitLineItem);
		}

		#endregion

		#region ICYDPickup

		ICYDYardUnitState ICYDPickup.GetLinkedYardUnit => LinkedYardUnit;

		ICYDUnitLineItem  ICYDPickup.GetUnitLineItem => UnitLineItem;

		#endregion

		#region	IProcessHandlingInfoProvider Members

		public ProcessHandlingInfo ProcessHandlingInfo => new CYDPickupProcessHandlingInfo(this);

		#endregion

		#region Implementation

		public override void Delete()
		{
			if (LinkedYardUnit != null)
			{
				LinkedYardUnit.YUS_YEL_ReleaseLine = ZGuid.Empty;
				LinkedYardUnit.YUS_YPL_Pickup = ZGuid.Empty;
			}

			base.Delete();
		}

		public override void OnSaving()
		{
			if (!IsDeleted)
			{
				PopulateFormattedNumberPropertyIfRequired(YPL_PickupIDInfo, Env.NumberFountains.CYDPickupID);
			}

			base.OnSaving();
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
