using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("New BusinessObject for Container Yard project")]
	public class CYDYardStorageFreeDays : AutoCYDYardStorageFreeDays
	{
		public CYDYardStorageFreeDays(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new abstract class Schema : AutoCYDYardStorageFreeDays.Schema
		{
			public const string YardUnitLength = "YardUnitLength";
		}

		#region YFD_WW_Yard

		[List("Lookups.AllYards")]
		public override ZGuid YFD_WW_Yard
		{
			get { return base.YFD_WW_Yard; }
			set { base.YFD_WW_Yard = value; }
		}

		public IWhsWarehouse Yard
		{
			get => Factory.Load<IWhsWarehouse>(YFD_WW_Yard);
		}

		#endregion

		#region YFD_TransportMode

		[List("Lookups.YardTransportModes")]
		public override ZString YFD_TransportMode
		{
			get { return base.YFD_TransportMode; }
			set { base.YFD_TransportMode = value; }
		}

		#endregion

		#region YFD_UnitType

		[List("Lookups.UnitTypes")]
		public override ZString YFD_UnitType
		{
			get { return base.YFD_UnitType; }
			set { base.YFD_UnitType = value; }
		}

		#endregion

		#region YFD_UnitLoad

		[List("Lookups.UnitLoads")]
		public override ZString YFD_UnitLoad
		{
			get { return base.YFD_UnitLoad; }
			set { base.YFD_UnitLoad = value; }
		}

		protected bool YFD_UnitLoad_ReadOnly
		{
			get { return YFD_UnitType != ContainerYardConstants.YardUnitType.Codes.CNT; }
		}

		#endregion

		#region YardUnitLength

		[List("Lookups.UnitLengthList")]
		public ZString YardUnitLength
		{
			get => YFD_YardUnitLength != 0 ? YFD_YardUnitLength.ToString() : string.Empty;
			set
			{
				YFD_YardUnitLength = ZDecimal.ParseSafe(value, 0);
				YFD_YardUnitLengthInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo YardUnitLengthInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.YardUnitLength, x => YFD_YardUnitLengthInfo); }
		}

		#endregion

		#region YFD_ContainerClass

		[List("Lookups.ContainerClasses")]
		public override ZString YFD_ContainerClass
		{
			get { return base.YFD_ContainerClass; }
			set { base.YFD_ContainerClass = value; }
		}

		#endregion

		#region Implementation

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			YFD_TransportMode = string.Empty;
			YFD_UnitType = string.Empty;
			YFD_UnitLoad = string.Empty;
			YFD_ContainerClass = string.Empty;
			YFD_YardUnitLength = 0;
			YFD_UnitOfDimension = "FT";
		}
#endif

		#endregion
	}
}
