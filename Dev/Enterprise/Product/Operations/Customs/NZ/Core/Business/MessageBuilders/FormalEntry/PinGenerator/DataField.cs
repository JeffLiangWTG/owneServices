
namespace Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.PinGenerator
{
	public class DataField
	{
		public DataField(string fieldName, string fieldValue)
		{
			FieldName = fieldName;
			FieldValue = fieldValue;
		}

		public readonly string FieldName;
		public readonly string FieldValue;
	}
}
