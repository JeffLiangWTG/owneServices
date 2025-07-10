using Enterprise.MasterFiles.Integration;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public interface IScreenedParty : IDeniedPartyResultItemV4
	{
		string PartyName { get; }
		DpsImageSources EntityTypeIcon { get; set; }
	}
}
