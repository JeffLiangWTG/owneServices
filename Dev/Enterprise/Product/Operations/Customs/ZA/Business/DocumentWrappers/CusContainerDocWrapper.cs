using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Edifact.D96B.Segments;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class CusContainerDocWrapper : NonPersistentBusinessObject, IContainerInformation
	{
		public CusContainerDocWrapper(IContainerInformation input, BusinessObjectFactory factory) : base(factory ?? new BusinessObjectFactory())
		{
			if (input != null)
			{
				this.ContainerNumber = input.ContainerNumber;
				this.ContainerMode = input.ContainerMode;
				this.FirstSealNumber = input.FirstSealNumber;
				this.SecondSealNumber = input.SecondSealNumber;
			}
		}

		public CusContainerDocWrapper(EQDSegment eqd, BusinessObjectFactory factory) : base(factory ?? new BusinessObjectFactory())
		{
			if (eqd != null)
			{
				ContainerNumber = eqd.EquipmentIdentification.EquipmentIdentificationNumber;
				ContainerMode = eqd.FullEmptyIndicatorCoded.ToString();
				var sealNumbers = new ZString(eqd.EquipmentSizeAndType.EquipmentSizeAndType);
				FirstSealNumber = sealNumbers.SubstringSafe(0, 15).Trim();
				SecondSealNumber = sealNumbers.SubstringSafe(15, 15).Trim();
			}
		}

		public ZString ContainerNumber { get; private set; }

		public ZString ContainerMode { get; private set; }

		public ZString ContainerModeDescription => Factory.GetCachedValue<Universal.ContainerModeCodeList>().GetDescriptionFromCode(ContainerMode);

		public ZString FirstSealNumber { get; private set; }

		public ZString SecondSealNumber { get; private set; }
	}
}
