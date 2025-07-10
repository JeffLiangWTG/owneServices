using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.Access.Business
{
	public partial class AsycudaManifestHeader
	{
		#region ValuationDate

		public ZDateTime ValuationDate
		{
			get
			{
				return ZDate.Today;
			}
		}

		#endregion

		public override bool LockedBills => true;

		public override bool IsDeclarationCreationEnabled => GlbCompany.CurrentCompany.GC_RN_NKCountryCode == AMA_RN_NKCountry;

		public override ZString CustomsSystem => "SGCustoms";

		public override ZString AMA_RN_NKCountry
		{
			get => base.AMA_RN_NKCountry;
			set
			{
				var oldValue = AMA_RN_NKCountry;
				base.AMA_RN_NKCountry = value;
				if (!IsCopying && oldValue != value)
				{
					AMA_ContainerMode = Core.Constants.ContainerModes.Other;
				}
			}
		}

		public override ASYCUDA.Business.BaseMessageSendingNotificationHelper GetMessageSendingNotificationHelper()
		{
			return new SGMessageSendingNotificationHelper(this);
		}
	}
}
