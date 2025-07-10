using System;

namespace Enterprise.Customs.US.Messaging.Business
{
	public abstract class LevelAttribute : Attribute
	{
		protected LevelAttribute(Type messageBlockType, params Type[] possibleChildren)
		{
			MessageBlockType = messageBlockType;
			PossibleChildren = possibleChildren;
		}

		public readonly Type MessageBlockType;
		public readonly Type[] PossibleChildren;
	}
}
