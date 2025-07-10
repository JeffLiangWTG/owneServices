using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists
{
	public class CargoTypeList : CodeDescriptionEnumList<BACCApplicationTypeDetailsShipmentVoyageCargoType>
	{
		public static class Codes
		{
			public const string Bulk = ContainerModeList.Codes.Bulk;
			public const string Empty = ContainerModeList.Codes.Empty;
			public const string Fak = "FAK";
			public const string Fcl = ContainerModeList.Codes.FCL;
			public const string Lcl = ContainerModeList.Codes.LCL;
		}

		public static class Descriptions
		{
			public const string Bulk = "Bulk";
			public const string Empty = "Empty";
			public const string Fak = "FAK";
			public const string Fcl = "FCL";
			public const string Lcl = "LCL";
		}

		public CargoTypeList()
		{
			AddPair(Codes.Bulk, Descriptions.Bulk, BACCApplicationTypeDetailsShipmentVoyageCargoType.B);
			AddPair(Codes.Empty, Descriptions.Empty, BACCApplicationTypeDetailsShipmentVoyageCargoType.MT);
			AddPair(Codes.Fak, Descriptions.Fak, BACCApplicationTypeDetailsShipmentVoyageCargoType.FAK);
			AddPair(Codes.Fcl, Descriptions.Fcl, BACCApplicationTypeDetailsShipmentVoyageCargoType.FCL);
			AddPair(Codes.Lcl, Descriptions.Lcl, BACCApplicationTypeDetailsShipmentVoyageCargoType.LCL);
		}

		public static string GetDefaultCargoType(CusContainerCollection containers)
		{
			if (containers.Count > 0)
			{
				ZString modeOfAllContainers = containers[0].CO_FCL_LCL_AIR;
				foreach (CusContainer container in containers)
				{
					if (container.CO_FCL_LCL_AIR != modeOfAllContainers)
					{
						return Codes.Fak;
					}
				}
				switch (modeOfAllContainers)
				{
					case ContainerModeList.Codes.Bulk:
						return Codes.Bulk;
					case ContainerModeList.Codes.Empty:
						return Codes.Empty;
					case ContainerModeList.Codes.FCL:
						return Codes.Fcl;
					case ContainerModeList.Codes.LCL:
						return Codes.Lcl;
				}
			}
			return Codes.Fak;
		}
	}
}
