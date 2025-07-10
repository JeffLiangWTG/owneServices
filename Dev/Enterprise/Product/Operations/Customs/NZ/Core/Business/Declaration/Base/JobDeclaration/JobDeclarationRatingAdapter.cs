using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class JobDeclarationRatingAdapter<T> : BaseJobDeclarationRatingAdapter<T>
		where T : JobDeclaration
	{
		public JobDeclarationRatingAdapter(T parent) : base(parent)
		{
			Argument.NotNull(parent, "parent");
			this.parent = parent;
		}

		readonly T parent;

		#region AutoRatingStatusInfo

		protected override AutoRatingStatusInfo GetStatusInformationCore()
		{
			var result = base.GetStatusInformationCore();
			if (result.CanExecute && result.Message.IsEmpty)
			{
				var message = JobDeclaration.GetWarningMessageIfCurrentEntryTotalAmountAndReturnedOneAreDifferent(parent.CusEntryHeader);
				if (!message.IsEmpty)
				{
					result = new AutoRatingStatusInfo(true, string.Format("{0}\r\n\r\nAre you sure you want to continue with AutoRating?", message));
				}
			}
			return result;
		}

		#endregion
	}
}
