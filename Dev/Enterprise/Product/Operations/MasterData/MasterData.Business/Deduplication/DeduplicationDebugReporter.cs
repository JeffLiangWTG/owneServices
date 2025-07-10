using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationDebugReporter : NonPersistentBusinessObject
	{
		readonly ZString detailsText;

		public DeduplicationDebugReporter(ZString detailsText)
		{
			Argument.NotNull(detailsText, nameof(detailsText));

			this.detailsText = detailsText;
		}

		#region Properties

		public ZString DetailsText
		{
			get
			{
				return detailsText;
			}
		}

		public ZPropertyInfo DetailsTextInfo
		{
			get { return GetZPropertyInfo(nameof(DetailsText)); }
		}

		#endregion
	}
}
