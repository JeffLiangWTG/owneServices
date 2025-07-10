namespace Enterprise.Freight.Business.Testing
{
	sealed class AssertionSailing
	{
		public string Load { get; set; }
		public string Discharge { get; set; }
		public bool IsPublished { get; set; }

		public override string ToString()
		{
			return string.Format("Load: {0}\r\nDischarge: {1}\r\nIsPublished: {2}", Load, Discharge, IsPublished);
		}

		public override bool Equals(object obj)
		{
			var other = (AssertionSailing)obj;
			return Load == other.Load && Discharge == other.Discharge && IsPublished == other.IsPublished;
		}

		public override int GetHashCode()
		{
			return Load.GetHashCode() ^ Discharge.GetHashCode() ^ IsPublished.GetHashCode();
		}
	}
}
