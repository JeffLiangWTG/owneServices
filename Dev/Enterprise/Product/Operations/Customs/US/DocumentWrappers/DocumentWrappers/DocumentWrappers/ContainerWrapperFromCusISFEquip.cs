using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.US.DocumentWrappers
{
	public class ContainerWrapperFromCusISFEquip : ContainerWrapperEmpty
	{
		public ContainerWrapperFromCusISFEquip(CusISFEquip equipmentBO, BusinessObjectFactory factory)
			: base(equipmentBO, factory)
		{
			Argument.NotNull(factory, "factory");
			this.equipmentBO = equipmentBO ?? factory.GetNull<CusISFEquip>();
		}
		readonly CusISFEquip equipmentBO;

		protected override FreightWrapper GetFreightJob()
		{
			var freightWrappers = FreightWrapper.New(equipmentBO.Header, Factory);
			if (freightWrappers.Length > 0)
			{
				return freightWrappers[0];
			}
			return null;
		}

		protected override WeightWrapper GetWeightTare()
		{
			return new WeightWrapper(ZDecimal.Zero, Core.Constants.Weight.Kilograms, 3, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override ZString GetContainerNo()
		{
			return equipmentBO.BE_ContainerNum;
		}

		protected override ZString GetContainerNumberOrTypeCount()
		{
			return ContainerNo;
		}
	}
}
