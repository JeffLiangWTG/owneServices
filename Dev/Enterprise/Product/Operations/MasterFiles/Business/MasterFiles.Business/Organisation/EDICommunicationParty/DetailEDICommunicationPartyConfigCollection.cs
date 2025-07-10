using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class DetailEDICommunicationPartyConfigCollection : DependentBusinessObjectCollection<EDICommunicationPartyConfig, EDICommunicationParty>
	{
		public DetailEDICommunicationPartyConfigCollection(EDICommunicationParty party)
			: base(party)
		{
		}

		protected override string FkColumnName => nameof(EDICommunicationPartyConfig.ECC_ECP_Party);
	}
}
