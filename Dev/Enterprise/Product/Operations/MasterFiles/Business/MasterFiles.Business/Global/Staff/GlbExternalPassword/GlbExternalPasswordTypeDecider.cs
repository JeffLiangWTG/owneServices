using System;
using System.Collections;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbExternalPasswordTypeDecider : TypeDecider
	{
		public override Type GetTypeForNew() => typeof(GlbExternalPassword);

		public override Type GetTypeForBinding() => typeof(GlbExternalPassword);

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			if (row == null)
			{
				return typeof(GlbExternalPassword);
			}

			var passwordType = row[GlbExternalPassword.Schema.GP_PasswordType].ToString().ToUpper();
			if (string.IsNullOrEmpty(passwordType))
			{
				return typeof(GlbExternalPassword);
			}

			if (passwordType == PasswordTypesList.Codes.EIM)
			{
				var hasCertificate = !string.IsNullOrEmpty(row[GlbExternalPassword.Schema.GP_Certificate].ToString());
				if (hasCertificate)
				{
					return typeof(EInvoicingCertificateCredential);
				}
				else
				{
					return typeof(EInvoicingPasswordCredential);
				}
			}

			var providers = ObjectFactory.Get<Hashtable>("GlbExternalPasswordProviders");
			var objectHandle = (ObjectHandle)providers[passwordType];
			if (objectHandle != null)
			{
				var result = objectHandle.GetObjectType();
				if (typeof(TypeDecider).IsAssignableFrom(result))
				{
					var decider = (TypeDecider)Activator.CreateInstance(result);
					result = decider.GetTypeForLoad(row, factory);
				}
				return result;
			}

			return typeof(GlbExternalPassword);
		}
	}
}
