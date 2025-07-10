using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class CusBrokerageBoxNumberValidation
	{
		public CusBrokerageBoxNumberValidation(CusBrokerageBoxNumber parent)
		{
			this.parent = parent;
		}

		readonly CusBrokerageBoxNumber parent;

		public void ValidateAll()
		{
			ValidateBoxNumber();
			ValidateCustomsOfficeArea();
			ValidateIsDefaultBoxNumber();
		}

		public void ValidateBoxNumber()
		{
			var targetInfo = parent.BoxNumberInfo;
			targetInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(targetInfo);
			var boxNumber = parent.BoxNumber;
			var customsOfficeArea = parent.CustomsOfficeArea;
			if (parent.Collection?.Cast<CusBrokerageBoxNumber>()?.Any(x => x != parent && x.BoxNumber == boxNumber && x.CustomsOfficeArea == customsOfficeArea) ?? ZBool.False)
			{
				targetInfo.AddError(ValidationConstants.CusBrokerageBoxNumber.BoxNumberAgain);
			}

			if (!Regex.IsMatch(boxNumber, @"^[A-Z0-9]{3}$"))
			{
				targetInfo.AddError(ValidationConstants.CusBrokerageBoxNumber.InvalidBoxNumber);
			}
		}

		public void ValidateCustomsOfficeArea()
		{
			var targetInfo = parent.CustomsOfficeAreaInfo;
			targetInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(targetInfo, parent.Lookups.CustomsOfficeAreaList);
			MandatoryValidation.CheckEntered(targetInfo);
		}

		public void ValidateIsDefaultBoxNumber()
		{
			var targetInfo = parent.IsDefaultBoxNumberInfo;
			targetInfo.ClearAllNotifications();
			var customsOfficeArea = parent.CustomsOfficeArea;
			var boxNumberCollection = parent.Collection?.Cast<CusBrokerageBoxNumber>()?.Where(x => x != parent && x.CustomsOfficeArea == customsOfficeArea);
			if (parent.IsDefaultBoxNumber && (boxNumberCollection?.Any(x => x.IsDefaultBoxNumber) ?? ZBool.False))
			{
				targetInfo.AddError(ValidationConstants.CusBrokerageBoxNumber.CustomsOfficeAreaIsDefaultAgain);
			}
			if (!parent.IsDefaultBoxNumber && (boxNumberCollection?.All(x => !x.IsDefaultBoxNumber) ?? ZBool.False))
			{
				targetInfo.AddError(ValidationConstants.CusBrokerageBoxNumber.UnselectedDefaultBoxNumber);
			}
		}
	}
}
