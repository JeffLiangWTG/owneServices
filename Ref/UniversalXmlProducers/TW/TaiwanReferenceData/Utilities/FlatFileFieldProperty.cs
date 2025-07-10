namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public struct FlatFileFieldProperty
	{
		public FlatFileFieldProperty(int Name, int Length)
		{
			this.Name = Name;
			this.Length = Length;
		}

		public readonly int Name;
		public readonly int Length;
	}
}
