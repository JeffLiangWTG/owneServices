using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business;

public class AlternativeEvidence : JobDeclarationAlternativeEvidence
{
	public AlternativeEvidence(JobDeclarationMessageSendingObject jobDeclarationMessageSendingObject) : base(jobDeclarationMessageSendingObject.Factory)
	{
	}

	[List(nameof(Lookups) + "." + nameof(AlternativeEvidenceLookups.DocTypeList))]
	[MaxLength(2)]
	public override ZString DocType
	{
		get => base.DocType;
		set => base.DocType = value;
	}

	[ResourceStringData("DDBEBF46-A1A1-4505-9E8A-C97A2EDBD860", Caption = "Doc. Type Description")]
	public ZString DocTypeDescription => Lookups.DocTypeList.GetDescriptionFromCode(DocType);

	public new AlternativeEvidenceLookups Lookups => (AlternativeEvidenceLookups)base.Lookups;

	protected override JobDeclarationAlternativeEvidenceLookups GetNewLookups() => new AlternativeEvidenceLookups(this);
}
