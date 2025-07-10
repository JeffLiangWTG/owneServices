using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class LazyGlbEmailAddress : IGlbEmailAddress
	{
		public LazyGlbEmailAddress(BusinessObjectFactory factory, Func<ZString> emailAddressGetter)
		{
			this.factory = factory;
			this.emailAddressGetter = emailAddressGetter;
		}

		readonly BusinessObjectFactory factory;
		readonly Func<ZString> emailAddressGetter;

		public ZDateTime GI_DeliveryReportTimeUtc
		{
			get
			{
				var glbEmailAddress = GlbEmailAddress;
				if (glbEmailAddress != null)
				{
					return glbEmailAddress.GI_DeliveryReportTimeUtc;
				}

				return ZDateTime.Empty;
			}
			set
			{
				var glbEmailAddress = GlbEmailAddress;
				if (value.IsEmpty)
				{
					if (glbEmailAddress != null)
					{
						glbEmailAddress.GI_DeliveryReportTimeUtc = ZDateTime.Empty;
					}
				}
				else
				{
					glbEmailAddress = glbEmailAddress ?? GetNewGlbEmailAddressIfValid();
					if (glbEmailAddress != null)
					{
						glbEmailAddress.GI_DeliveryReportTimeUtc = value;
					}
				}
			}
		}

		public ZString GI_DeliveryStatus
		{
			get
			{
				var glbEmailAddress = GlbEmailAddress;
				if (glbEmailAddress != null)
				{
					return glbEmailAddress.GI_DeliveryStatus;
				}

				return ZString.Empty;
			}
			set
			{
				var glbEmailAddress = GlbEmailAddress;
				if (string.IsNullOrEmpty(value))
				{
					if (glbEmailAddress != null)
					{
						glbEmailAddress.GI_DeliveryStatus = ZString.Empty;
					}
				}
				else
				{
					glbEmailAddress = glbEmailAddress ?? GetNewGlbEmailAddressIfValid();
					if (glbEmailAddress != null)
					{
						glbEmailAddress.GI_DeliveryStatus = value;
						glbEmailAddress.GI_DeliveryReportTimeUtc = ZDateTime.UtcNow;
					}
				}
			}
		}

		public ZString GI_EmailAddress
		{
			get { return emailAddressGetter(); }
		}

		public GlbEmailAddress GlbEmailAddress
		{
			get { return GlbEmailAddress.Load(factory, GI_EmailAddress); }
		}

		GlbEmailAddress GetNewGlbEmailAddressIfValid()
		{
			var emailAddress = GI_EmailAddress;
			if (string.IsNullOrEmpty(emailAddress))
			{
				return null;
			}

			var result = factory.New<GlbEmailAddress>();
			result.GI_EmailAddress = emailAddress;
			return result;
		}
	}
}
