using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ClientContractNumberRegistryDataType : BillCustomisationRegistryDataType
	{
		public ClientContractNumberRegistryDataType() : base(new ClientContractNumberCustomisation())
		{
			Categories = NumberCustomisationElementCategories.ClientContract;
			FountainPrefix = "CLA";
			GeneratedNumberName = ResString.GetMultilingualString("9055b887-dfe5-4d79-a27b-4faaccd9d09b", "Client Contract Number Format");
			MaxLength = RatingContractSchema.RCT_ContractNumber.MaxLength;
		}

		protected override Type DataTypeCore => typeof(ClientContractNumberCustomisation);
	}
}
