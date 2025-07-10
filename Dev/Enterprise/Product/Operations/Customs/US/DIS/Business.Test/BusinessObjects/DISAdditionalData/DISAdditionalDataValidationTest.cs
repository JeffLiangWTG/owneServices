using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISAdditionalDataValidationTest : TestCaseWithFactory
	{
		public void TestCheckName()
		{
			var data = new DISAdditionalData(Factory);
			data.Name = ZString.Empty;
			AssertHasMessageErrorContaining(data.NameInfo, MandatoryValidation.YouHaveNotEntered);
			data.Name = "78423";
			AssertNoMessageErrorContaining(data.NameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckData()
		{
			var data = new DISAdditionalData(Factory);
			data.Data = ZString.Empty;
			AssertHasMessageErrorContaining(data.DataInfo, MandatoryValidation.YouHaveNotEntered);
			data.Data = "78423";
			AssertNoMessageErrorContaining(data.DataInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckInvalidCharatersOnData()
		{
			var msgData = string.Format(DISDocumentValidation.DataHasInvalidCharacters, DISAdditionalData.Schema.Data);
			var msgName = string.Format(DISDocumentValidation.DataHasInvalidCharacters, DISAdditionalData.Schema.Name);
			var data = new DISAdditionalData(Factory);
			AssertNoWarningContaining(data.DataInfo, msgData);
			AssertNoWarningContaining(data.NameInfo, msgName);
			data.Data = "BBÉCCÉDD";
			AssertHasWarningContaining(data.DataInfo, msgData);
			data.Name = "DÉAÉRÉRÉN";
			AssertHasWarningContaining(data.NameInfo, msgName);
		}
	}
}
