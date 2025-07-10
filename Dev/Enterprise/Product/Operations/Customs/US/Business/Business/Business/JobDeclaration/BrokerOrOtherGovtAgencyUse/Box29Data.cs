using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class Box29Data : NonPersistentBusinessObject, IBox29Supportable, IObsoleteValidation
	{
		#region Schema

		public static class Schema
		{
			public const string US_Box29Text = "US_Box29Text";
			public const string US_Box29IncludeContainers = "US_Box29IncludeContainers";
		}

		#endregion

		public Box29Data(IBox29Supportable box29Data)
		{
			this.box29Data = box29Data;
		}

		readonly IBox29Supportable box29Data;

		#region IBox29Supportable Members

		public ZString US_Box29Text
		{
			get { return box29Data.US_Box29Text; }
			set { box29Data.US_Box29Text = value; }
		}

		public ZPropertyInfo US_Box29TextInfo
		{
			get { return box29Data == null ? null : GetWrappedZPropertyInfo(Schema.US_Box29Text, x => box29Data.US_Box29TextInfo); }
		}

		public ZBool US_Box29IncludeContainers
		{
			get { return box29Data.US_Box29IncludeContainers; }
			set { box29Data.US_Box29IncludeContainers = value; }
		}

		public ZPropertyInfo US_Box29IncludeContainersInfo
		{
			get { return box29Data == null ? null : GetWrappedZPropertyInfo(Schema.US_Box29IncludeContainers, x => box29Data.US_Box29IncludeContainersInfo); }
		}

		#endregion
	}
}
