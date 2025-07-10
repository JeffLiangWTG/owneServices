using System.ComponentModel;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business;

[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
public class InvoiceAmountBoundaryCollection : RegistryBusinessObjectCollectionTemplate, IBindingListView
{
	public InvoiceAmountBoundaryCollection()
	{
	}

	public new InvoiceAmountBoundary this[int index]
	{
		get { return (InvoiceAmountBoundary)Elements[index]; }
	}

	public new InvoiceAmountBoundary AddNew()
	{
		return (InvoiceAmountBoundary)base.AddNew();
	}

	public InvoiceAmountBoundary AddNew(ZDateTime startDate, ZDateTime endDate, ZDecimal amount)
	{
		var item = AddNew();
		item.StartDate = startDate;
		item.EndDate = endDate;
		item.Amount = amount;
		return item;
	}

	public void AddDefaultValues()
	{
		AddNew(new ZDateTime(2024, 5, 5), new ZDateTime(2024, 12, 31), 25000m);
		AddNew(new ZDateTime(2025, 1, 1), new ZDateTime(2025, 12, 31), 20000m);
		AddNew(new ZDateTime(2026, 1, 1), new ZDateTime(2026, 12, 31), 15000m);
		AddNew(new ZDateTime(2027, 1, 1), new ZDateTime(2027, 12, 31), 10000m);
		AddNew(new ZDateTime(2028, 1, 1), new ZDateTime(2028, 12, 31), 5000m);
	}

	protected override BusinessObject CreateNonPersistentBusinessObject()
	{
		return new InvoiceAmountBoundary();
	}

	protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	{
		return new InvoiceAmountBoundaryCollection();
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
				defaultSortDirections = new ListSortDescription[] { new ListSortDescription(InvoiceAmountBoundaryCollection.GetProperties(TypeOfElements)[InvoiceAmountBoundary.Schema.StartDate], ListSortDirection.Descending) };
			}
			return defaultSortDirections;
		}
	}
	ListSortDescription[] defaultSortDirections;

	#endregion
}
