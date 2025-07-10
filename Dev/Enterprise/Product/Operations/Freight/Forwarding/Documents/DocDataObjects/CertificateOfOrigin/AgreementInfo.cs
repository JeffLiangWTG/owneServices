using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	public class AgreementInfo : DocDataObject
	{
		#region HasBeenAcknowledged

		public ZBool HasBeenAcknowledged
		{
			get => hasBeenAcknowledged;
			set
			{
				if (SetNonPersistentPropertyValue(HasBeenAcknowledgedInfo, ref hasBeenAcknowledged, value))
				{
					Validate(HasBeenAcknowledgedInfo);
				}
			}
		}

		ZBool hasBeenAcknowledged;

		public ZPropertyInfo HasBeenAcknowledgedInfo => GetZPropertyInfo(nameof(HasBeenAcknowledged));

		#endregion

		#region VersionNo

		public ZInt VersionNo
		{
			get => versionNo;
			set
			{
				if (SetNonPersistentPropertyValue(VersionNoInfo, ref versionNo, value))
				{
					Validate(VersionNoInfo);
				}
			}
		}
		ZInt versionNo;

		public ZPropertyInfo VersionNoInfo => GetZPropertyInfo(nameof(VersionNo));

		#endregion
	}
}
