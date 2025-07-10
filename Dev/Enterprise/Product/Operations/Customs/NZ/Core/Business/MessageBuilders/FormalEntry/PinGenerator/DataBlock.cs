using System.Collections;
using System.Text;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.PinGenerator
{
	/// <summary>
	/// Summary description for DataBlock.
	/// </summary>
	public class DataBlock
	{
		public DataBlock(string blockCode, string blockName)
		{
			BlockCode = blockCode;
			BlockName = blockName;
			dataFields = new ArrayList();
		}

		public readonly string BlockCode;
		public readonly string BlockName;

		public void AddDataField(string fieldName, string fieldValue)
		{
			DataField dataField = new DataField(fieldName, fieldValue);
			dataFields.Add(dataField);
		}

		public string GetHumanReadableBlock()
		{
			StringBuilder humanReadableBlock = new StringBuilder();
			foreach (DataField dataField in dataFields)
			{
				humanReadableBlock.Append(dataField.FieldName + "=[" + dataField.FieldValue + "]\r\n");
			}
			humanReadableBlock.Append("block_" + BlockCode.ToLower() + "=[" + GetPinGenerationBlock() + "]\r\n");
			return humanReadableBlock.ToString();
		}

		public string GetPinGenerationBlock()
		{
			StringBuilder pinGenerationBlock = new StringBuilder();
			foreach (DataField dataField in dataFields)
			{
				pinGenerationBlock.Append(dataField.FieldValue);
			}
			int extraSpacesRequired = 8 - (pinGenerationBlock.Length % 8);
			if (extraSpacesRequired < 8)
			{
				for (; extraSpacesRequired > 0; extraSpacesRequired--)
				{
					pinGenerationBlock.Append(" ");
				}
			}
			return pinGenerationBlock.ToString();
		}

		protected ArrayList dataFields;
	}
}
