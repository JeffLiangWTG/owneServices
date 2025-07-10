using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class CusGoodsLocationValidation
	{
		public CusGoodsLocationValidation(CusGoodsLocation parent)
		{
			this.parent = parent;
		}

		readonly CusGoodsLocation parent;

		public void ValidateAll()
		{
			ValidateCustomsOffice();
			ValidateMessageType();
			ValidateGoodsLocation();
		}

		public void ValidateCustomsOffice()
		{
			var parent = this.parent;
			var targetInfo = parent.CustomsOfficeInfo;
			targetInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(targetInfo);

			CheckDuplicatedData(parent, parent.CustomsOffice, parent.MessageType);
			if (!parent.CustomsOffice.IsEmpty)
			{
				CheckInvalidGoodsLocation(parent, targetInfo);
			}
		}

		void CheckDuplicatedData(CusGoodsLocation parent, ZString customsOffice, ZString messageType)
		{
			parent.ClearRowNotifications();
			if (parent.Collection?.Cast<CusGoodsLocation>()?.Any(x => x != parent && x.CustomsOffice == customsOffice && x.MessageType == messageType) ?? false)
			{
				parent.AddRowError(ValidationConstants.CusGoodsLocation.CannotBeDuplicated);
			}
		}

		public void ValidateGoodsLocation()
		{
			var parent = this.parent;
			var targetInfo = parent.GoodsLocationInfo;
			targetInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(targetInfo);
			CheckInvalidGoodsLocation(parent, targetInfo);
		}

		void CheckInvalidGoodsLocation(CusGoodsLocation parent, ZPropertyInfo targetInfo)
		{
			if (!parent.GoodsLocation.IsEmpty && parent.GoodsLocationItem == null)
			{
				targetInfo.AddError(ValidationConstants.CusGoodsLocation.InvalidGoodsLocation);
			}
		}

		public void ValidateMessageType()
		{
			var parent = this.parent;
			var targetInfo = parent.MessageTypeInfo;
			targetInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.ErrorIfInvalidCode(targetInfo);
			CheckDuplicatedData(parent, parent.CustomsOffice, parent.MessageType);
		}
	}
}
