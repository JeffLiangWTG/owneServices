using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Test
{
	sealed class AENS54Test : TestCase
	{
		public void TestAllDeclarationTypeCodeHasExtendedFieldsDefined()
		{
			var declarationTypeCodes = new AdditionalDeclarationTypeCodeList();
			var aens54Mock = new Mock<AENS54>();
			var serialiserSupporter = (IExtendedFieldsHumanFriendlySerialiserSupporter)aens54Mock.Object;
			foreach (var code in declarationTypeCodes.GetAllCodes())
			{
				aens54Mock.Object.ImportersAdditionalDeclarationTypeCode = code;
				var mapping = serialiserSupporter.GetExtendedFieldsMappings("AAA");
				AssertNotEquals($"Please add a field mapping for declaration type code {code}", 0, mapping.Count);
			}
		}
	}
}
