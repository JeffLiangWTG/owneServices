using CargoWise.Types;

namespace Enterprise.Customs.US.DIS.Business
{
	class DISAdditionalDataWrapper : IDISAdditionalData
	{
		public DISAdditionalDataWrapper(DISAdditionalData additionalData)
		{
			this.additionalData = additionalData;
		}

		readonly DISAdditionalData additionalData;

		ZString IDISAdditionalData.FieldName
		{
			get { return additionalData.Name; }
		}

		ZString IDISAdditionalData.Value
		{
			get { return additionalData.Data; }
		}
	}
}
