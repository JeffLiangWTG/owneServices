using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class SanctionsAdditionalInfo : ISanctionsAdditionalInfo
	{
		public SanctionsAdditionalInfo(ZString recordID, ZString recordType, ZString fieldNmae, ZString fieldValue)
		{
			this.recordID = recordID;
			this.recordType = recordType;
			this.fieldName = fieldNmae;
			this.fieldValue = fieldValue;
		}
		readonly ZString recordID;
		readonly ZString recordType;
		readonly ZString fieldName;
		readonly ZString fieldValue;

		ZString ISanctionsAdditionalInfo.RecordID => this.recordID;

		ZString ISanctionsAdditionalInfo.RecordType => this.recordType;

		ZString ISanctionsAdditionalInfo.FieldName => this.fieldName;

		ZString ISanctionsAdditionalInfo.FieldValue => this.fieldValue;
	}
}
