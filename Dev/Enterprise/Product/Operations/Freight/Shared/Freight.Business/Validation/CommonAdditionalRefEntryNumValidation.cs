using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business
{
	public class CommonAdditionalRefEntryNumValidation : CusEntryNumValidation
	{
		public CommonAdditionalRefEntryNumValidation(AutoCusEntryNum parent)
			: base(parent)
		{
		}

		protected override void CheckCE_EntryType()
		{
			base.CheckCE_EntryType();

			var entryTypesForSystemOnly = new ZString[]
			{
				CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CarrierMessageReference,
				CusEntryNumLookups.HIR
			};

			if (entryTypesForSystemOnly.Contains(Parent.CE_EntryType)
				&& !Parent.CE_EntryIsSystemGenerated
				&& (Parent.CE_EntryTypeInfo.HasChanges || !Parent.IsInDatabase))
			{
				Parent.CE_EntryTypeInfo.AddError(Res.GetString("A7CFF9AD-F11D-47EA-810D-1EB0781468CC", "{0} is reserved for system use and cannot be manually entered.", Parent.CE_EntryType));
			}
		}
	}
}
