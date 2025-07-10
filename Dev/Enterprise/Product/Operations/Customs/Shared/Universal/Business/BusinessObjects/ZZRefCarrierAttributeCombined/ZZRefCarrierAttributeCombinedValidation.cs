//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefCarrierAttributeCombinedValidation
//
//    This class should be used for overriding validation in AutoZZRefCarrierAttributeCombinedValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public class ZZRefCarrierAttributeCombinedValidation : AutoZZRefCarrierAttributeCombinedValidation
	{
		public ZZRefCarrierAttributeCombinedValidation(AutoZZRefCarrierAttributeCombined parent)
			: base(parent)
		{ }

		protected new ZZRefCarrierAttributeCombined Parent => (ZZRefCarrierAttributeCombined)base.Parent;

		protected override void CheckZZG_Name()
		{
			base.CheckZZG_Name();

			var targetInfo = Parent.ZZG_NameInfo;
			ListValidation.ErrorIfInvalidCode(targetInfo, Parent.Lookups.NameList);

			var attributeName = Parent.ZZG_Name;
			if (!attributeName.IsEmpty)
			{
				if (Parent.Carrier?.Attributes.Any(x => x.ZZG_Name.EqualsIgnoringCase(attributeName) && x.PK != Parent.PK) ?? false)
				{
					targetInfo.AddError(Res.GetString("5E534F23-AE1A-49DD-9324-D42B82B7E71E", "Duplicate attribute name '{0}' is not allowed.", attributeName));
				}
			}
		}
	}
}
