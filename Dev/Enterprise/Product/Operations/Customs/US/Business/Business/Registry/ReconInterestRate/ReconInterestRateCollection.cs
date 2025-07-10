using System.ComponentModel;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class ReconInterestRateCollection : RegistryBusinessObjectCollectionTemplate, IBindingListView
	{
		public ReconInterestRateCollection()
		{
		}

		public new ReconInterestRate this[int index]
		{
			get { return (ReconInterestRate)Elements[index]; }
		}

		public new ReconInterestRate AddNew()
		{
			return (ReconInterestRate)base.AddNew();
		}

		public ReconInterestRate AddNew(ZDateTime startDate, ZDateTime endDate, ZDecimal rate)
		{
			ReconInterestRate result = AddNew();
			result.EndDate = endDate;
			result.StartDate = startDate;
			result.Rate = rate;
			return result;
		}

		public void AddDefaultValues()
		{
			AddNew(new ZDateTime(2012, 01, 01), new ZDateTime(2012, 03, 31), 3m);
			AddNew(new ZDateTime(2011, 10, 01), new ZDateTime(2011, 12, 31), 3m);
			AddNew(new ZDateTime(2011, 04, 01), new ZDateTime(2011, 09, 30), 4m);
			AddNew(new ZDateTime(2011, 01, 01), new ZDateTime(2011, 03, 31), 3m);
			AddNew(new ZDateTime(2010, 10, 01), new ZDateTime(2010, 12, 31), 4m);
			AddNew(new ZDateTime(2010, 07, 01), new ZDateTime(2010, 09, 30), 4m);
			AddNew(new ZDateTime(2010, 04, 01), new ZDateTime(2010, 06, 30), 4m);
			AddNew(new ZDateTime(2010, 01, 01), new ZDateTime(2010, 03, 31), 4m);
			AddNew(new ZDateTime(2009, 10, 01), new ZDateTime(2009, 12, 31), 4m);
			AddNew(new ZDateTime(2009, 07, 01), new ZDateTime(2009, 09, 30), 4m);
			AddNew(new ZDateTime(2009, 04, 01), new ZDateTime(2009, 06, 30), 4m);
			AddNew(new ZDateTime(2009, 01, 01), new ZDateTime(2009, 03, 31), 5m);
			AddNew(new ZDateTime(2008, 10, 01), new ZDateTime(2008, 12, 31), 6m);
			AddNew(new ZDateTime(2008, 07, 01), new ZDateTime(2008, 09, 30), 5m);
			AddNew(new ZDateTime(2008, 04, 01), new ZDateTime(2008, 06, 30), 6m);
			AddNew(new ZDateTime(2008, 01, 01), new ZDateTime(2008, 03, 31), 7m);
			AddNew(new ZDateTime(2007, 10, 01), new ZDateTime(2007, 12, 31), 8m);
			AddNew(new ZDateTime(2007, 07, 01), new ZDateTime(2007, 09, 30), 8m);
			AddNew(new ZDateTime(2007, 04, 01), new ZDateTime(2007, 06, 30), 8m);
			AddNew(new ZDateTime(2007, 01, 01), new ZDateTime(2007, 03, 31), 8m);
			AddNew(new ZDateTime(2006, 10, 01), new ZDateTime(2006, 12, 31), 8m);
			AddNew(new ZDateTime(2006, 07, 01), new ZDateTime(2006, 09, 30), 8m);
			AddNew(new ZDateTime(2006, 04, 01), new ZDateTime(2006, 06, 30), 7m);
			AddNew(new ZDateTime(2006, 01, 01), new ZDateTime(2006, 03, 31), 7m);
			AddNew(new ZDateTime(2005, 10, 01), new ZDateTime(2005, 12, 31), 7m);
			AddNew(new ZDateTime(2005, 07, 01), new ZDateTime(2005, 09, 30), 6m);
			AddNew(new ZDateTime(2005, 04, 01), new ZDateTime(2005, 06, 30), 6m);
			AddNew(new ZDateTime(2005, 01, 01), new ZDateTime(2005, 03, 30), 5m);
			AddNew(new ZDateTime(2004, 10, 01), new ZDateTime(2004, 12, 31), 5m);
			AddNew(new ZDateTime(2004, 07, 01), new ZDateTime(2004, 09, 30), 4m);
			AddNew(new ZDateTime(2004, 04, 01), new ZDateTime(2004, 06, 30), 5m);
			AddNew(new ZDateTime(2004, 01, 01), new ZDateTime(2004, 03, 31), 4m);
			AddNew(new ZDateTime(2003, 10, 01), new ZDateTime(2003, 12, 31), 4m);
			AddNew(new ZDateTime(2003, 01, 01), new ZDateTime(2003, 09, 30), 5m);
			AddNew(new ZDateTime(2002, 01, 01), new ZDateTime(2002, 12, 31), 6m);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ReconInterestRate();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ReconInterestRateCollection();
		}

		#region IBindingListView Members

		ListSortDescriptionCollection IBindingListView.SortDescriptions
		{
			get { return new ListSortDescriptionCollection(DefaultSortDirections); }
		}

		ListSortDescription[] DefaultSortDirections
		{
			get
			{
				if (defaultSortDirections == null)
				{
					defaultSortDirections = new ListSortDescription[] { new ListSortDescription(ReconInterestRateCollection.GetProperties(TypeOfElements)[ReconInterestRate.Schema.StartDate], ListSortDirection.Descending) };
				}
				return defaultSortDirections;
			}
		}
		ListSortDescription[] defaultSortDirections;

		#endregion
	}
}
