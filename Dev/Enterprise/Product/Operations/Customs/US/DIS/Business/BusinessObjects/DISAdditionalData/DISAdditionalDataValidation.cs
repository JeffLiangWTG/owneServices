using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISAdditionalDataValidation : AutoDISAdditionalDataValidation
	{
		public DISAdditionalDataValidation(AutoDISAdditionalData bizObj)
			: base(bizObj)
		{
		}

		protected override void CheckName()
		{
			base.CheckName();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.NameInfo);
			var datafield = Parent.Name;
			if (DISDocumentValidation.HasInvalidCharacters(datafield))
			{
				Parent.NameInfo.AddWarning(string.Format(DISDocumentValidation.DataHasInvalidCharacters, DISAdditionalData.Schema.Name));
			}
		}

		protected override void CheckData()
		{
			base.CheckData();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.DataInfo);

			var datafield = Parent.Data;
			if (DISDocumentValidation.HasInvalidCharacters(datafield))
			{
				Parent.DataInfo.AddWarning(string.Format(DISDocumentValidation.DataHasInvalidCharacters, DISAdditionalData.Schema.Data));
			}
		}
	}
}
