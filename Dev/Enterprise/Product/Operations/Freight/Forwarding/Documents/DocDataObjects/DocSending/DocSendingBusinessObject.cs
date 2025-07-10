using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocSending
{
	public sealed class DocSendingBusinessObject : NonPersistentBusinessObject
	{
		public ZGuid Id { get; set; }

		public ZString Name { get; set; }
		public ZString Description { get; set; }

		public ZString DocumentType { get; set; }

		public ZBlob ImageData { get; set; }

		#region Include

		public ZBool Include
		{
			get => include;
			set
			{
				if (SetNonPersistentPropertyValue(IncludeInfo, ref include, value))
				{
					if (!value && Certify)
					{
						Certify = false;
					}
				}
			}
		}
		ZBool include;

		public ZPropertyInfo IncludeInfo => GetZPropertyInfo(nameof(Include));

		#endregion

		#region Certify

		public ZBool Certify
		{
			get => certify;
			set
			{
				if (SetNonPersistentPropertyValue(CertifyInfo, ref certify, value))
				{
					if (value && !Include)
					{
						Include = true;
					}
				}
			}
		}
		ZBool certify;

		public ZPropertyInfo CertifyInfo => GetZPropertyInfo(nameof(Certify));

		#endregion
	}
}
