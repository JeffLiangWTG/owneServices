using CargoWise.Common;
namespace Enterprise.Customs.US.AIM.Messaging.Generators
{
	public class FDMBlockGenerator : AIMBlockGenerator
	{
		public FDMBlockGenerator(IAIMMessageHeader departureMessageHeader)
		{
			this.departureMessageHeader = (IDepartureMessageHeader)Argument.NotNull(departureMessageHeader, nameof(departureMessageHeader));
		}

		protected override void PopulateMessageBlocks()
		{
			PopulateStandardMessageIdentifier(departureMessageHeader);
			PopulateDepartureDetails(departureMessageHeader.Departure);
		}

		readonly IDepartureMessageHeader departureMessageHeader;
	}
}
