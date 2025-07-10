using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class TWCustomsNumberViewStmNumsWrapperValidation : CustomsNumberViewStmNumsWrapperValidation
	{
		public TWCustomsNumberViewStmNumsWrapperValidation(TWCustomsNumberViewStmNumsWrapper parent)
			: base(parent)
		{ }

		protected override void ValidateAllCore()
		{
			ValidateMessageType();
			ValidateStartNumber();
			ValidateEndNumber();
			ValidateCurrentValue();
			ValidateRangeType();
		}

		public void ValidateMessageType()
		{
			ValidateCalculatedProperty(Parent.MessageTypeInfo);
		}

		public void ValidateStartNumber()
		{
			ValidateCalculatedProperty(Parent.StartNumberInfo);
		}

		public void ValidateEndNumber()
		{
			ValidateCalculatedProperty(Parent.EndNumberInfo);
		}

		public void ValidateCurrentValue()
		{
			ValidateCalculatedProperty(Parent.CurrentValueInfo);
		}

		public void ValidateRangeType()
		{
			ValidateCalculatedProperty(Parent.RangeTypeInfo);
		}

		protected void CheckMessageType()
		{
			var parent = Parent;
			var messageTypeInfo = parent.MessageTypeInfo;
			MandatoryValidation.CheckEntered(messageTypeInfo);
			ListValidation.ErrorIfInvalidCode(messageTypeInfo);
		}

		protected void CheckRangeType()
		{
			var parent = Parent;
			var rangeTypeInfo = parent.RangeTypeInfo;
			MandatoryValidation.CheckEntered(rangeTypeInfo);
			ListValidation.ErrorIfInvalidCode(rangeTypeInfo);
			parent.AddAdditionalRangeTypeValidation();
		}

		protected void CheckEndNumber()
		{
			var parent = Parent;
			var endNumberInfo = parent.EndNumberInfo;
			if (CheckNumber(parent, parent.SN_MaximumValue, endNumberInfo))
			{
				if (parent.SN_MaximumValue < parent.SN_MinimumValue)
				{
					endNumberInfo.AddError(ValidationConstants.TWCustomsNumberViewStmNumsWrapper.EndNumberCannotBeSmallerThanStartNumber);
				}
				if (parent.SN_MaximumValue < parent.SN_Value)
				{
					endNumberInfo.AddError(ValidationConstants.TWCustomsNumberViewStmNumsWrapper.HasUsedRange);
				}
			}
		}

		protected void CheckStartNumber()
		{
			var parent = Parent;
			var startNumberInfo = parent.StartNumberInfo;
			CheckNumber(parent, parent.SN_MinimumValue, startNumberInfo);
		}

		protected void CheckCurrentValue()
		{
			var parent = Parent;
			var currentValueInfo = parent.CurrentValueInfo;
			if (CheckNumber(parent, parent.SN_Value, currentValueInfo))
			{
				if (parent.SN_MinimumValue > parent.SN_Value)
				{
					currentValueInfo.AddError(ValidationConstants.TWCustomsNumberViewStmNumsWrapper.CurrentValueCannotBeSmallerThanStartNumber);
				}
			}
		}

		bool CheckNumber(TWCustomsNumberViewStmNumsWrapper parent, ZLong number, ZPropertyInfo numberInfo)
		{
			bool result = true;
			if (number <= ZLong.Zero)
			{
				numberInfo.AddError(ValidationConstants.TWCustomsNumberViewStmNumsWrapper.InvalidNumber);
				var allowedFormatDescription = parent.Sequenceformatter?.AllowedFormatDescription ?? ZString.Empty;
				if (!allowedFormatDescription.IsEmpty)
				{
					numberInfo.AddError(allowedFormatDescription);
				}
				result = false;
			}
			return result;
		}

		protected new TWCustomsNumberViewStmNumsWrapper Parent => (TWCustomsNumberViewStmNumsWrapper)base.Parent;
	}
}
