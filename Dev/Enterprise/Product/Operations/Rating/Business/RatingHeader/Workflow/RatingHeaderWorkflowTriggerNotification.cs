using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public static class RatingHeaderWorkflowTriggerNotification
	{
		public static class RatingHeaderReplacementConstants
		{
			public static string Organisation
			{
				get { return Res.GetString("e1d46d2d-2b2f-4193-98c4-ce4003fec5e9", "(*Organization*)"); }
			}
		}

		#region Overrides

		public static ZString Substitute(ProcessTaskNotification action, BusinessObject bizo, ZString message)
		{
			var result = message;

			if (bizo is RatingHeader ratingHeader && ratingHeader.Header != null)
			{
				result = result.Replace(RatingHeaderReplacementConstants.Organisation, ratingHeader.Header.OH_FullNameTruncated);
			}

			return result;
		}

		#endregion
	}
}


