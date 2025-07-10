using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface ILineNumberRange
	{
		ZShort StartNumber { get; }
		ZShort EndNumber { get; }
	}

	class LineNumberRange : ILineNumberRange
	{
		public LineNumberRange(ZShort startNumber, ZShort endNumber)
		{
			StartNumber = startNumber;
			EndNumber = endNumber;
		}

		public ZShort StartNumber;
		public ZShort EndNumber;

		#region ILineNumberRange Members

		ZShort ILineNumberRange.StartNumber
		{
			get { return StartNumber; }
		}

		ZShort ILineNumberRange.EndNumber
		{
			get { return EndNumber; }
		}

		#endregion
	}
}
