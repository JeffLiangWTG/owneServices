namespace Enterprise.Freight.LocalCartage.Business
{
	public interface ILegsOverrider
	{
		void SetLegs(CommonCartageLeg[] legs, bool useLegForCartageInfo, bool useLegsAsAddressInfo, bool removeContainers);
	}
}
