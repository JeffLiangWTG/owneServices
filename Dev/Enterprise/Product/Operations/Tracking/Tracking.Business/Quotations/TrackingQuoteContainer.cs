using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Tracking.Business.Quotations
{
	public class TrackingQuoteContainer : RateOneOffContainers
	{
		public TrackingQuoteContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region Container_List

		public RefContainerCollection Container_List
		{
			get { return ListProvider != null ? ListProvider.Container_List : new ContainerHelper(Factory).List(string.Empty); }
		}

		#endregion

		#region ListProvider

		public IContainerListProvider ListProvider { get; set; }

		#endregion

		#endregion
	}
}
