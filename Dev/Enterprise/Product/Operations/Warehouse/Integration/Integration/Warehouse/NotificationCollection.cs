using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Integration
{
	// do not add to this class
	// sub-class if you want to add more functionality

	public class NotificationCollection
	{
		#region Errors

		public bool HasErrors
		{
			get { return (ErrorList.Count > 0); }
		}

		public StringCollectionX ErrorList
		{
			get
			{
				if (fErrorList == null)
				{
					fErrorList = new StringCollectionX();
				}
				return fErrorList;
			}
		}
		StringCollectionX fErrorList;

		#endregion

		#region Warnings

		public bool HasWarnings
		{
			get { return (WarningList.Count > 0); }
		}

		public StringCollectionX WarningList
		{
			get
			{
				if (fWarningList == null)
				{
					fWarningList = new StringCollectionX();
				}
				return fWarningList;
			}
		}
		StringCollectionX fWarningList;

		#endregion
	}
}
