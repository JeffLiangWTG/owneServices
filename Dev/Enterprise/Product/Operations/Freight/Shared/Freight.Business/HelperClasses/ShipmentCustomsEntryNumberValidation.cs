using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class ShipmentCustomsEntryNumberValidation : ZValidation
	{
		public ShipmentCustomsEntryNumberValidation(ShipmentCustomsEntryNumber parent)
			: base(parent)
		{
			Argument.NotNull(parent, "parent");
			Parent = parent;
		}

		protected ShipmentCustomsEntryNumber Parent { get; private set; }

		public override Type AutoValidationType
		{
			get { return typeof(ShipmentCustomsEntryNumberValidation); }
		}

		public override void ValidateAll()
		{
			ValidateEntryType();
			ValidateEntryNumber();
			ValidateIssueDate();
			ValidateExpiryDate();
		}

		public void ValidateEntryType()
		{
			ValidateCalculatedProperty(Parent.EntryTypeInfo);
		}

		protected virtual void CheckEntryType()
		{
			if (!Parent.EntryTypeInfo.ReadOnly)
			{
				ListValidation.ErrorIfInvalidCode(Parent.EntryTypeInfo, Parent.EntryType_List);
				if (CusEntryNumberTypes.EU.EUCustomsEntryTypeList.ContainsCode(Parent.EntryType) && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedKingdom)
				{
					Parent.EntryTypeInfo.AddWarning(Res.GetString("a21612bf-77de-4292-a6d4-2fe6efa3c41f", "This selection is only valid for locations where EU customs rules apply."));
				}
			}

			if (Parent.IsEntryNumberForShipmentBinding && Parent.EntryType.IsEmpty && !Parent.EntryNumber.IsEmpty && !Parent.IsManyCustomsEntryNumber)
			{
				Parent.EntryTypeInfo.AddError(Res.GetString("ba7333d4-984a-48b8-a955-8e269fcff9b5", "Please enter an Entry Type"));
			}
		}

		public void ValidateEntryNumber()
		{
			ValidateCalculatedProperty(Parent.EntryNumberInfo);
		}

		protected virtual void CheckEntryNumber()
		{
		}

		public void ValidateIssueDate()
		{
			ValidateCalculatedProperty(Parent.IssueDateInfo);
		}

		protected virtual void CheckIssueDate()
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.IssueDateInfo);
			TypeValidation.CheckValidZDateTimeRange(Parent.IssueDateInfo);
			CheckIssueDateEarlierThanExpiry(Parent.IssueDateInfo, Res.GetString("FD74F016-8912-4CEB-AC2B-21FF29ECE6B7", "The '{0}' must be before the '{1}'.", Parent.IssueDateInfo.HumanReadableName, Parent.ExpiryDateInfo.HumanReadableName));
		}

		public void ValidateExpiryDate()
		{
			ValidateCalculatedProperty(Parent.ExpiryDateInfo);
		}

		protected virtual void CheckExpiryDate()
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.ExpiryDateInfo);
			TypeValidation.CheckValidZDateTimeRange(Parent.ExpiryDateInfo);
			CheckIssueDateEarlierThanExpiry(Parent.ExpiryDateInfo, Res.GetString("AE95BD31-97CA-41FB-AE41-5A924BEA20ED", "The '{0}' must be after the '{1}'.", Parent.ExpiryDateInfo.HumanReadableName, Parent.IssueDateInfo.HumanReadableName));
		}

		void CheckIssueDateEarlierThanExpiry(ZPropertyInfo info, string errorMessage)
		{
			if (!Parent.ExpiryDate.IsEmpty && !Parent.IssueDate.IsEmpty && Parent.ExpiryDate < Parent.IssueDate)
			{
				info.AddMessageError(errorMessage);
			}
		}
	}
}
