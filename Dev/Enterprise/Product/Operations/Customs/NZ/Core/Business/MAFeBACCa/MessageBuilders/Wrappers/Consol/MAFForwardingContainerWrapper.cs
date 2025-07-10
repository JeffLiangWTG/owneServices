namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders
{
	using CargoWise.Types;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;
	using Enterprise.Customs.NZ.TradeSingleWindow;
	using Enterprise.Freight.Forwarding.Business;

	public partial class MAFPlugInSupportConsolWrapper
	{
		class MAFForwardingContainerWrapper : IMAFContainer
		{
			internal MAFForwardingContainerWrapper(ForwardingContainer container)
			{
				this.container = container;
			}

			ZString IMAFContainer.ContainerNumber
			{
				get { return container.JC_ContainerNum; }
			}

			ZString IMAFContainer.ContainerType
			{
				get { return ContainerTypeList.Codes.MixedContainerTypes; }
			}

			public ZBool IsFullContainer
			{
				get { return container.JC_ContainerMode == ContainerModeList.Codes.FCL; }
			}

			readonly ForwardingContainer container;
		}
	}
}
