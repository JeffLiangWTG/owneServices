using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class DocumentActionReasonModel : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DocumentActionReasonModel(ICodeDescriptionPairList reasonCodesList)
		{
			Argument.NotNull(reasonCodesList, nameof(reasonCodesList));
			ReasonCodesList = reasonCodesList;
		}

		[List("ReasonCodesList")]
		public ZString ReasonCode
		{
			get => reasonCode;
			set
			{
				if (SetNonPersistentPropertyValue(ReasonCodeInfo, ref reasonCode, value)
					&& !IsValidationSuspended)
				{
					ValidateReasonCode();
				}
			}
		}

		ZString reasonCode;

		public ZPropertyInfo ReasonCodeInfo => GetZPropertyInfo(nameof(ReasonCode));

		public ICodeDescriptionPairList ReasonCodesList { get; }

		#region ReasonText

		public ZString ReasonText
		{
			get => reasonText;
			set
			{
				if (SetNonPersistentPropertyValue(ReasonTextInfo, ref reasonText, value))
				{
					ValidateReasonText();
				}
			}
		}

		ZString reasonText;

		public ZPropertyInfo ReasonTextInfo => GetZPropertyInfo(nameof(ReasonText));

		#endregion ReasonText

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			this.ValidateReasonCode();
			this.ValidateReasonText();
		}

		public void ValidateReasonCode()
		{
			ReasonCodeInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(ReasonCodeInfo);

			if (!ReasonCodeInfo.HasNotifications())
			{
				ListValidation.ErrorIfInvalidCode(ReasonCodeInfo);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		public void ValidateReasonText()
		{
			ReasonTextInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(ReasonTextInfo);
			if (HasNonAsciiCharacters(ReasonText))
			{
				const string errorMessage = "Most messaging providers do not support non ASCII characters.";

				ReasonTextInfo.AddMessageError(errorMessage);
			}
		}

		public bool HasNonAsciiCharacters(ZString value)
		{
			var encoding = Encoding.GetEncoding("ISO-8859-1");
			var bytes = encoding.GetBytes(value);
			var result = encoding.GetString(bytes);
			return !string.Equals(value, result);
		}

		#endregion

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} - {1}", ReasonCode, ReasonText);
		}
	}
}
