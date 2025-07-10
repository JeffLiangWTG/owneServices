using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class CYDYardStorageFreeDaysLookups : AutoCYDYardStorageFreeDaysLookups
	{
		public CYDYardStorageFreeDaysLookups(AutoCYDYardStorageFreeDays parent) : base(parent)
		{
		}

		public BusinessObjectCollection AllYards
		{
			get
			{
				return new CYDYardCollection(Factory).AllYards;
			}
		}

		public CodeDescriptionPairList YardTransportModes
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(TransportModes.Road, ResString.GetMultilingualString("8e181ff7-b057-43ec-b22d-29a5f9482a19", "Road Freight"));
				return list;
			}
		}

		public CodeDescriptionPairList UnitTypes
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(ContainerYardConstants.YardUnitType.Codes.CNT, ContainerYardConstants.YardUnitType.Descriptions.CNT);
				list.AddPair(ContainerYardConstants.YardUnitType.Codes.CHS, ContainerYardConstants.YardUnitType.Descriptions.CHS);
				list.AddPair(ContainerYardConstants.YardUnitType.Codes.GEN, ContainerYardConstants.YardUnitType.Descriptions.GEN);
				list.AddPair(ContainerYardConstants.YardUnitType.Codes.BLK, ContainerYardConstants.YardUnitType.Descriptions.BLK);
				return list;
			}
		}

		public CodeDescriptionPairList UnitLoads
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(ContainerYardConstants.YardUnitLoad.Codes.EMP, ContainerYardConstants.YardUnitLoad.Descriptions.EMP);
				list.AddPair(ContainerYardConstants.YardUnitLoad.Codes.LAD, ContainerYardConstants.YardUnitLoad.Descriptions.LAD);
				return list;
			}
		}

		public CodeDescriptionPairList UnitLengthList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				var result = Factory.Load<RefContainer>(new ZQuery(RefContainerSchema.RC_ShippingMode, SQLComparisonOperator.NotEqual, RefContainerLookups.ShippingModes.Air))
					.Select(c => c.RC_Length)
					.Where(c => c != 0)
					.Distinct()
					.OrderBy(c => c);
				foreach (var length in result)
				{
					list.AddPair(length.ToString(), $"{length} foot");
				}
				return list;
			}
		}

		public ReadOnlyCodeDescriptionPairList ContainerClasses
		{
			get
			{
				return Env.Registry.ContainerStorageClass;
			}
		}
	}
}
