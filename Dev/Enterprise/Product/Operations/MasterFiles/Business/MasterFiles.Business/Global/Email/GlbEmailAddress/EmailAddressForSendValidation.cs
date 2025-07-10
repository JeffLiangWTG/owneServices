using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class EmailAddressForSendValidation
	{
		public EmailAddressForSendValidation(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");

			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public void ValidateEmailAddress(ZPropertyInfo propertyInfo)
		{
			ValidateEmailAddress(propertyInfo, (ZString)propertyInfo.Value);
		}

		public void ValidateEmailAddress(ZPropertyInfo parentPropertyInfo, params ZString[] emailAddresses)
		{
			var validEmailAddresses = new List<ZString>();
			foreach (var emailAddress in emailAddresses)
			{
				if (!EmailAddressValidation.IsEmailAddressValidAndNotEmpty(emailAddress))
				{
					parentPropertyInfo.AddError(Res.GetString("0ae4d331-64d5-4571-a4db-20c465a3479f", @"The email address ""{0}"" is invalid.", emailAddress));
				}
				else
				{
					validEmailAddresses.Add(emailAddress);
				}
			}

			var glbEmailAddresses = GlbEmailAddress.Load(factory, validEmailAddresses);
			foreach (var glbEmailAddress in glbEmailAddresses)
			{
				if (glbEmailAddress.GI_DeliveryStatus == EmailDeliveryReportStatus.Codes.NonDeliveryReport)
				{
					var message = GetHasNdrWarningMessage(glbEmailAddress);
					parentPropertyInfo.AddWarning(message);
				}
			}
		}

		public void ValidateCommaSeparatedEmailAddresses(ZPropertyInfo propertyInfo)
		{
			var commaSeparatedEmailAddresses = (ZString)propertyInfo.Value;
			var emailAddresses = commaSeparatedEmailAddresses.Trim().ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(ea => new ZString(ea.Trim())).ToArray();
			ValidateEmailAddress(propertyInfo, emailAddresses);
		}

		public static string GetHasNdrWarningMessage(GlbEmailAddress glbEmailAddress)
		{
			Argument.NotNull(glbEmailAddress, "glbEmailAddress");
			return Res.GetString("7c72836c-5966-413b-8f2b-f716ab3f0ab8", @"The last email sent to ""{0}"" received a Non-Delivery Receipt (at {1}).", glbEmailAddress.GI_EmailAddress, glbEmailAddress.GI_DeliveryReportTimeUtc.ToString());
		}
	}
}
