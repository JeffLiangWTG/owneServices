using CargoWise.Customs.NL.MessageContracts.Interfaces;

namespace Enterprise.Customs.NL.Business;

public class ClassificationWrapper : IClassification
{
	public ClassificationWrapper(int sequenceNo, string id, string identificationTypeCode)
	{
		this.sequenceNo = sequenceNo;
		this.id = id;
		this.identificationTypeCode = identificationTypeCode;
	}
	readonly int sequenceNo;
	readonly string id;
	readonly string identificationTypeCode;

	public int SequenceNo => sequenceNo;
	public string Id => id;
	public string IdentificationTypeCode => identificationTypeCode;
	public string CCQualifierCode => null;
}
