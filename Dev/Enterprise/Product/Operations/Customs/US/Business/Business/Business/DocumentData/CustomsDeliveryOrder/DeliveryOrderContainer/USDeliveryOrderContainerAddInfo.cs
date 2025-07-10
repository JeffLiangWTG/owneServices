using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USDeliveryOrderContainer)]
	public class USDeliveryOrderContainerAddInfo : AutoUSDeliveryOrderContainerAddInfo
	{
		public USDeliveryOrderContainerAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new DeliveryOrderContainer Parent
		{
			get { return (DeliveryOrderContainer)base.Parent; }
			protected set { base.Parent = value; }
		}

		public override ZString US_ContainerNumber
		{
			get { return base.US_ContainerNumber; }
			set
			{
				ZString oldValue = US_ContainerNumber;
				base.US_ContainerNumber = value;
				if (!IsCopying && oldValue != US_ContainerNumber)
				{
					UpdateContainerDetails();
				}
			}
		}

		void UpdateContainerDetails()
		{
			DeliveryOrderContainer container = Parent;
			DeliveryOrderHeader header = container != null ? container.Parent : null;
			JobDeclaration declaration = header != null ? header.Declaration : null;
			if (declaration != null)
			{
				CusContainer cusContainer = declaration.CusContainers.Find(US_ContainerNumber);
				if (cusContainer != null)
				{
					US_ContainerSeal = cusContainer.CO_Seal;
					RefContainer containerType = cusContainer.Container;
					US_ContainerType = containerType == null ? ZString.Empty : containerType.RC_Code.Left(USDeliveryOrderContainerAddInfo.Schema.US_ContainerTypeMaxLength);
					US_ContainerMode = cusContainer.CO_FCL_LCL_AIR;
					US_Weight = cusContainer.CO_Weight;
					US_WeightUQ = cusContainer.CO_WeightUQ;
					var jobContainer = cusContainer.JobContainer;
					if (jobContainer != null)
					{
						US_SlotReference = jobContainer.JC_ArrivalSlotReference;
						US_LastFreeDay = jobContainer.JC_LastFreeDay;
					}
				}
			}
		}
	}
}
