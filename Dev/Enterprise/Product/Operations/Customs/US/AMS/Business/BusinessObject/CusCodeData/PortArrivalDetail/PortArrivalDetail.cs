using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Business
{
	public class PortArrivalDetail : NonPersistentBusinessObject
	{
		public PortArrivalDetail(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static class Schema
		{
			public const string PortCode = "PortCode";
			public const string ActualArrivalDate = "ActualArrivalDate";
		}

		#region New Properties

		#region PortCode

		public ZString PortCode
		{
			get { return portCode; }
			set
			{
				portCode = value;
				PortCodeInfo.RefreshBinding();
			}
		}
		ZString portCode;

		public ZPropertyInfo PortCodeInfo
		{
			get { return GetZPropertyInfo(Schema.PortCode); }
		}

		public ZDateTime ActualArrivalDate
		{
			get { return arrivalDate; }
			set
			{
				arrivalDate = value;
				ActualArrivalDateInfo.RefreshBinding();
			}
		}
		ZDateTime arrivalDate;

		public ZPropertyInfo ActualArrivalDateInfo
		{
			get { return GetZPropertyInfo(Schema.ActualArrivalDate); }
		}

		#endregion
		#endregion
	}
}
