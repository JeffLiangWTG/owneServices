using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business
{
	public static class ACEOceanManifestIllegalCharacters
	{
		public static ZString ReplaceIllegalCharacters(ZString stringValueToBeReplaced, char characterToReplaceWith = ' ')
		{
			return MessageBlockStringDataCorrector.ReplaceInvalidCharacters(stringValueToBeReplaced, AMSCharacterTypeString.Constants.Special, characterToReplaceWith);
		}

		public static void MessageErrorIfThereAreIllegalCharacters(ZPropertyInfo propertyInfo)
		{
			AMSCharactersValidator.ValidateCharacters(propertyInfo);
		}

		public static void WarnIllegalCharactersWereReplaced(ZPropertyInfo propertyInfo)
		{
			AMSCharactersValidator.ValidateCharacters(propertyInfo, true);
		}
	}
}
