using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business
{
	public class CommonContainerAdditionalRefEntryNumValidation : CusEntryNumValidation
	{
		public CommonContainerAdditionalRefEntryNumValidation(AutoCusEntryNum parent)
			: base(parent)
		{
		}

		protected override void CheckCE_EntryType()
		{
			base.CheckCE_EntryType();

			var entryTypesForSystemOnly = new ZString[]
			{
				CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CarrierMessageReference,
			};

			if (entryTypesForSystemOnly.Contains(Parent.CE_EntryType)
				&& !Parent.CE_EntryIsSystemGenerated
				&& (Parent.CE_EntryTypeInfo.HasChanges || !Parent.IsInDatabase))
			{
				Parent.CE_EntryTypeInfo.AddError(Res.GetString("6358246C-7C02-4F4B-A270-8BBA7F1CF40A", "{0} is reserved for system use and cannot be manually entered.", Parent.CE_EntryType));
			}
		}
	}
}
