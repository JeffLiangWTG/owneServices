using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public abstract class NumberGeneratorTarget
	{
		public ZString Value { get; internal set; }

		public NumberGeneratorContext Context { get; set; }

		public BillOfLadingNumberCustomisation NumberCustomisation
		{
			get { return numberCustomisation ?? (numberCustomisation = GetNumberCustomisationCore()); }
		}
		BillOfLadingNumberCustomisation numberCustomisation;

		public string Name
		{
			get { return GetNameCore(); }
		}

		public int MaxLength
		{
			get { return GetMaxLengthCore(); }
		}

		public abstract string NumberCustomisationLocation { get; }
		protected abstract BillOfLadingNumberCustomisation GetNumberCustomisationCore();
		protected abstract int GetMaxLengthCore();
		protected abstract ZString GetNameCore();

		public INumberFountainProxy FountainUsedForGeneration { get; set; }
		public ZString ValuePrefix { get; set; }
		public ZString ValueSuffix { get; set; }
		public ZString FountainValue { get; set; }
	}
}
