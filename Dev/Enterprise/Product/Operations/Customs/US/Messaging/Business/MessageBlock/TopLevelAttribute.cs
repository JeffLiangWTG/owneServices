using System;

namespace Enterprise.Customs.US.Messaging.Business
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class TopLevelAttribute : LevelAttribute
	{
		/// <summary>
		/// This attribute should be applied to each Processor class to describe the top level MessageBlock class.
		/// </summary>
		/// <param name="applicationIdentifier">The application this message is processing for</param>
		/// <param name="messageBlockType">The MessageBlock class to be processed by this Processor</param>
		/// <param name="possibleChildren">The allowable children that may follow the 'type' MessageBlock</param>
		public TopLevelAttribute(Type messageBlockType, params Type[] possibleChildren)
			: base(messageBlockType, possibleChildren)
		{
		}
	}
}
