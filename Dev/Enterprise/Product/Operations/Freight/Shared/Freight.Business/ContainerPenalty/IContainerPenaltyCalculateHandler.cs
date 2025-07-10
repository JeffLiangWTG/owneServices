namespace Enterprise.Freight.Business
{
	public interface IContainerPenaltyCalculateHandler
	{
		void HandleContainerDateChanging(ContainerPenaltyRelatedDateType dateType);

		void HandleContainerAdded();

		void HandleTransportATAChanged();

		void HandleTransportATDChanged();

		void HandleContainerAllocation(PackLine packLine);

		void HandleContainerDeallocation(PackLine packLine);
	}
}
