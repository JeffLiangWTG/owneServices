using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business
{
	public class FTPSettingsValidation : ZValidation
	{
		public FTPSettingsValidation(FTPSettings parent) : base(parent)
		{
			this.parent = parent;
		}

		readonly FTPSettings parent;

		public override void ValidateAll()
		{
			parent.ClearAllNotifications();
			CheckEnglishCharacters();
			CheckExportUnionUserPassword();
			ValidatePathStartsWithSlash(parent.InboxInfo);
			ValidatePathStartsWithSlash(parent.OutboxInfo);
		}

		public override Type AutoValidationType => typeof(FTPSettingsValidation);

		void CheckEnglishCharacters()
		{
			ValidateEnglishCharacters(parent.ExportUnionUserCodeInfo, parent.ExportUnionUserPasswordInfo, parent.ExportUnionPaymentPasswordInfo);
		}

		public void CheckExportUnionUserPassword()
		{
			if (!parent.ExportUnionUserCode.IsEmpty)
			{
				MandatoryValidation.CheckEntered(parent.ExportUnionUserPasswordInfo);
			}
			else
			{
				MandatoryValidation.CheckNotEntered(parent.ExportUnionUserPasswordInfo);
			}
		}

		void ValidateEnglishCharacters(params ZPropertyInfo[] properties)
		{
			foreach (var property in properties)
			{
				EnglishCharactersValidation.ErrorIfNotWesternEuropean(property);
			}
		}

		void ValidatePathStartsWithSlash(ZPropertyInfo propertyInfo)
		{
			var value = propertyInfo.Value?.ToString();

			if (!string.IsNullOrEmpty(value) && !value.StartsWith("/"))
			{
				propertyInfo.AddMessageError(Res.GetString("4A6E2E7F-794B-41A0-AEBD-7E2E92308F0E", "The text must start with a forward slash (/)."));
			}
		}
	}
}
