using System;
using CargoWise.Types;

namespace Enterprise.Recruitment.Common
{
	public class HRTaskStage : IComparable
	{
		public ZInt Sequence { get; set; }
		public ZString Description { get; set; }

		public int CompareTo(object obj)
			=> obj is HRTaskStage other ? Sequence.CompareTo(other.Sequence) : 1;

		public static explicit operator ZString(HRTaskStage stage)
			=> stage?.ToString() ?? ZString.Empty;

		public override string ToString()
		=> string.IsNullOrEmpty(Description) ?
			string.Empty : Sequence == int.MaxValue ?
				Description : $"{Sequence} - {Description}";
	}
}
