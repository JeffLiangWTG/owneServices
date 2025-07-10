//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoSGAddInfoValidation
//
//    This class should be used for overriding validation in AutoSGAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGAddInfoValidation : AutoSGAddInfoValidation
	{
		public SGAddInfoValidation(AutoSGAddInfo parent)
			: base(parent)
		{
			if (!parent.GetType().IsSubclassOf(typeof(AddInfo)))
			{
				throw new ArgumentException("Parent is not a subclass of AddInfo");
			}
		}

		public new AddInfo Parent
		{
			get { return (AddInfo)base.Parent; }
		}
	}
}
