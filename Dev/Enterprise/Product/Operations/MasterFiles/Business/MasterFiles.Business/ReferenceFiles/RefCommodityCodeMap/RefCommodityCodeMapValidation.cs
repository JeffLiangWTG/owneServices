using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.MasterFiles.Business
{
	public class RefCommodityCodeMapValidation : AutoRefCommodityCodeMapValidation
	{
		public RefCommodityCodeMapValidation(AutoRefCommodityCodeMap parent) : base(parent)
		{
		}

		protected new RefCommodityCodeMap Parent => (RefCommodityCodeMap)base.Parent;

		static string InvalidUsageErrorMessage => Res.GetString("10393b92-66e0-4c9c-839e-b6945ab7c489", "The Usage code is not supported for the Country/Region '{0}'.");

		static string DuplicateCodeMessage => Res.GetString("b572bfc4-8e4e-4d3c-9aff-c856281774c4", "Only one local code can be configured for each unique Country/Region & Usage pair.");

		protected override void CheckLC_LocalCodeProvider()
		{
			base.CheckLC_LocalCodeProvider();
			MandatoryValidation.CheckEntered(Parent.LC_LocalCodeProviderInfo);

			if (Parent.LC_LocalCodeProvider.IsEmpty)
			{
				return;
			}

			ListValidation.IfInvalidCode(
				NotificationType.Error,
				Parent.LC_LocalCodeProviderInfo,
				Parent.Lookups.LocalProviders,
				string.Format(InvalidUsageErrorMessage, Parent.Country?.RN_Desc ?? ZString.Empty));

			CheckParentIsUnique();
		}

		protected override void CheckLC_LocalCode()
		{
			base.CheckLC_LocalCode();
			MandatoryValidation.CheckEntered(Parent.LC_LocalCodeInfo);
		}

		protected override void CheckLC_RN_NKCountry()
		{
			base.CheckLC_RN_NKCountry();
			ListValidation.ErrorIfInvalidCode(Parent.LC_RN_NKCountryInfo);
		}

		protected override void CheckLC_RH_NKCommodityCode()
		{
			base.CheckLC_RH_NKCommodityCode();
			MandatoryValidation.CheckEntered(Parent.LC_RH_NKCommodityCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.LC_RH_NKCommodityCodeInfo);
		}

		void CheckParentIsUnique()
		{
			var commodityCode = Parent.CommodityCode;
			if (commodityCode == null)
			{
				return;
			}

			var othersSameParent =
				commodityCode.RefCommodityCodeMaps.Any(x =>
					x != Parent &&
					x.LC_LocalCodeProvider == Parent.LC_LocalCodeProvider &&
					x.LC_RN_NKCountry == Parent.LC_RN_NKCountry);

			if (othersSameParent)
			{
				Parent.LC_LocalCodeProviderInfo.AddError(DuplicateCodeMessage);
			}
		}
	}
}

