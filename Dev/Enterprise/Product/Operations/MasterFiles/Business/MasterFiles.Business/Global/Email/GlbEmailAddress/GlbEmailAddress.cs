using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbEmailAddress : AutoGlbEmailAddress, IGlbEmailAddress
	{
		public GlbEmailAddress(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			this.SetConcurrencyPolicy(nameof(GI_DeliveryStatus), ConcurrencyPolicy.Ignore);
			this.SetConcurrencyPolicy(nameof(GI_DeliveryReportTimeUtc), ConcurrencyPolicy.Ignore);
		}

		#region Static

		public static GlbEmailAddress Load(BusinessObjectFactory factory, ZString emailAddress)
		{
			var addressOnly = GetAddressOnly(emailAddress);
			return factory.LoadFromNaturalKey<GlbEmailAddress>(GlbEmailAddressSchema.GI_EmailAddress, addressOnly);
		}

		public static GlbEmailAddress LoadOrNew(BusinessObjectFactory factory, ZString emailAddress)
		{
			Argument.NotNullOrEmpty(emailAddress, "emailAddress");

			var result = Load(factory, emailAddress);
			if (result == null)
			{
				result = factory.New<GlbEmailAddress>();
				result.GI_EmailAddress = emailAddress;
			}

			return result;
		}

		public static GlbEmailAddress[] Load(BusinessObjectFactory factory, IEnumerable<ZString> emailAddresses, ZQuery query = null)
		{
			var addresses = emailAddresses.Select(x => GetAddressOnly(x));
			var emailAddressFilter = new ZQuery(GlbEmailAddressSchema.GI_EmailAddress, addresses);
			if (query == null)
			{
				query = emailAddressFilter;
			}
			else
			{
				query.AddToFilter(emailAddressFilter);
			}

			return factory.Load<GlbEmailAddress>(query);
		}

		static string GetAddressOnly(ZString emailAddress)
		{
			if (emailAddress.IsEmpty)
			{
				return emailAddress;
			}

			try
			{
				var mailAddress = new MailAddress(emailAddress);
				return mailAddress.Address;
			}
			catch (FormatException)
			{
				return emailAddress;
			}
		}

		#endregion

		#region Properties

		[EmailAddress]
		public override ZString GI_EmailAddress
		{
			get { return base.GI_EmailAddress; }
			set
			{
				if (base.GI_EmailAddress != value)
				{
					base.GI_EmailAddress = GetAddressOnly(value);
				}
			}
		}

		#endregion
	}
}
