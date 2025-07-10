using System;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public abstract class BlockAttribute : Attribute
	{
		protected BlockAttribute(string mandatoryCharacters, string version = "")
		{
			MandatoryCharacters = mandatoryCharacters;
			Version = version;
		}
		public readonly string MandatoryCharacters;
		public readonly string Version;
	}
}
