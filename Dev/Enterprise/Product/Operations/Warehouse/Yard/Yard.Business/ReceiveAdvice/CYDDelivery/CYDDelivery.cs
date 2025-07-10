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
	[UniversalDataContext(DataContextType.CYDDelivery)]
	public class CYDDelivery : AutoCYDDelivery,
		ICYDDelivery,
		IProcessHandlingInfoProvider
	{
		public CYDDelivery(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("DeliveryHeader")]
		public override ZGuid YDL_YDH_DeliveryHeader { get => base.YDL_YDH_DeliveryHeader; set => base.YDL_YDH_DeliveryHeader = value; }

		public CYDDeliveryHeader DeliveryHeader
		{
			get => Factory.Load<CYDDeliveryHeader>(YDL_YDH_DeliveryHeader);
		}

		[RelatedBusinessObject("ReceiveAdviceLine")]
		public override ZGuid YDL_YRL_ReceiveAdviceLine { get => base.YDL_YRL_ReceiveAdviceLine; set => base.YDL_YRL_ReceiveAdviceLine = value; }

		public CYDReceiveAdviceLine ReceiveAdviceLine
		{
			get => Factory.Load<CYDReceiveAdviceLine>(YDL_YRL_ReceiveAdviceLine);
		}

		public CYDYardUnitState LinkedYardUnit
		{
			get => Factory.LoadTop1<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_YDL_Delivery, PK));
		}

		[RelatedBusinessObject("UnitLineItem")]
		public override ZGuid YDL_YLI_UnitLineItem { get => base.YDL_YLI_UnitLineItem; set => base.YDL_YLI_UnitLineItem = value; }

		public CYDUnitLineItem UnitLineItem
		{
			get => Factory.Load<CYDUnitLineItem>(YDL_YLI_UnitLineItem);
		}

		#endregion

		#region ICYDDelivery

		ICYDYardUnitState ICYDDelivery.GetLinkedYardUnit => LinkedYardUnit;

		ICYDUnitLineItem ICYDDelivery.GetUnitLineItem => UnitLineItem;

		#endregion

		#region	IProcessHandlingInfoProvider Members

		public ProcessHandlingInfo ProcessHandlingInfo => new CYDDeliveryProcessHandlingInfo(this);

		#endregion

		#region Implementation

		public override void OnSaving()
		{
			if (!IsDeleted)
			{
				PopulateFormattedNumberPropertyIfRequired(YDL_DeliveryIDInfo, Env.NumberFountains.CYDDeliveryID);
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
