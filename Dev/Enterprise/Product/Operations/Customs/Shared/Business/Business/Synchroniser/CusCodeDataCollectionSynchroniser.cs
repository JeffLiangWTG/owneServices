using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusCodeDataCollectionSynchroniser<T> : GenericCollectionSynchroniser<CusCodeDataSynchroniser>
		where T : CusCodeData
	{
		public CusCodeDataCollectionSynchroniser(BaseJobComInvoiceLine source, BaseJobComInvoiceLine destination,
				CusCodeDataCollection<T> sourceCollection, CusCodeDataCollection<T> destinationCollection)
			: base(source, destination, sourceCollection, destinationCollection)
		{
		}

		protected override bool CompareBizosEqual(BusinessObject source, BusinessObject destination)
		{
			var dataSource = (CusCodeData)source;
			var dataDestination = (CusCodeData)destination;
			return dataSource.CY_Data == dataDestination.CY_Data &&
							dataSource.CY_Code == dataDestination.CY_Code;
		}

		protected override CusCodeDataSynchroniser CreateNewSynchroniser(BusinessObject destination, BusinessObject source)
		{
			return new CusCodeDataSynchroniser((CusCodeData)destination, (CusCodeData)source);
		}
	}
}
