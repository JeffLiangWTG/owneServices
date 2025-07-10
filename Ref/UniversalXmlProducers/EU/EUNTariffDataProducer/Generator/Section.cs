using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class Section : ISection
	{
		public int Number { get; }
		public string Description { get; }

		public Section(int number, string description)
		{
			Argument.NotNullOrEmpty(description, nameof(description));

			this.Number = number;
			this.Description = description;
		}
	}
}
