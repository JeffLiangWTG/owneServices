using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.GlobalCommercialInvoice.Business
{
	/// <summary>
	/// Header for a Global Commercial Invoice.
	/// </summary>
	public sealed class GlobalCommercialInvoiceHeaderCollection : ActiveBusinessObjectCollection<GlobalCommercialInvoiceHeader>
	{
		/// <summary>
		/// Parent Business Object ID, i.e. PK.
		/// </summary>
		public ZGuid ParentID { get; }

		/// <summary>
		/// Parent DB table name prefix code.
		/// </summary>
		public ZString ParentTableCode { get; }

		/// <summary>
		/// Header for a Global Commercial Invoice.
		/// </summary>
		/// <param name="factory">Business Object Factory.</param>
		/// <param name="parentID">Parent ID, i.e. PK.</param>
		/// <param name="parentTableCode">Parent DB table name prefix code.</param>
		public GlobalCommercialInvoiceHeaderCollection(BusinessObjectFactory factory, ZGuid parentID, ZString parentTableCode)
			: base(factory, new ZQuery(GlobalCommercialInvoiceHeaderSchema.GIH_ParentID, parentID))
		{
			ParentID = parentID;
			ParentTableCode = parentTableCode;
		}

		/// <summary>
		/// Gets an empty instance of <see cref="GlobalCommercialInvoiceHeaderCollection"/>.
		/// </summary>
		/// <param name="factory">Business Object Factory.</param>
		/// <returns>An empty instance of <see cref="GlobalCommercialInvoiceHeaderCollection"/>.</returns>
		public static GlobalCommercialInvoiceHeaderCollection GetEmpty(BusinessObjectFactory factory) =>
			new(factory, ZGuid.Empty, ZString.Empty);

		/// <summary>
		/// Initializes a new instance of the <see cref="GlobalCommercialInvoiceHeader"/> class.
		/// </summary>
		/// <param name="newElement">New instance of the <see cref="GlobalCommercialInvoiceHeader"/> class.</param>
		protected override void SetDefaultsForNewElementCore(GlobalCommercialInvoiceHeader newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.Headers = this;
			newElement.GIH_ParentID = ParentID;
			newElement.GIH_ParentTableCode = ParentTableCode;

			newElement.PropertyValueChanged += OnPropertyValueChanged;
		}

		/// <summary>
		/// Allows to update an existing item of the <see cref="GlobalCommercialInvoiceHeader"/> type.
		/// Here we can assign the headers to the <see cref="GlobalCommercialInvoiceHeader"/> instance.
		/// </summary>
		/// <param name="loadingObject">The instance of <see cref="GlobalCommercialInvoiceHeader"/> to update.</param>
		protected override void OnLoadingIntoCollectionCore(GlobalCommercialInvoiceHeader loadingObject)
		{
			base.OnLoadingIntoCollectionCore(loadingObject);

			loadingObject.Headers = this;

			loadingObject.PropertyValueChanged -= OnPropertyValueChanged;
			loadingObject.PropertyValueChanged += OnPropertyValueChanged;

			loadingObject.Validation.ValidateGIH_InvoiceNumber();
		}

		/// <summary>
		/// Is called when a <see cref="GlobalCommercialInvoiceHeader"/> is deleted.
		/// </summary>
		/// <param name="header">An instance of <see cref="GlobalCommercialInvoiceHeader"/> to be deleted.</param>
		public override void Delete(GlobalCommercialInvoiceHeader header)
		{
			var sourceInvoiceNumber = header.GIH_InvoiceNumber;

			header.PropertyValueChanged -= OnPropertyValueChanged;
			base.Delete(header);

			this.Where((x) => !x.IsDeleted && x.GIH_InvoiceNumber == sourceInvoiceNumber)
				.ToList()
				.ForEach((x) => x.Validation.ValidateGIH_InvoiceNumber());
		}

		/// <summary>
		/// An event handler that is called when a <see cref="GlobalCommercialInvoiceHeader"/> (i.e. collection item) property value is changed.
		/// </summary>
		/// <param name="sender">An instance of <see cref="GlobalCommercialInvoiceHeader"/> that is changed.</param>
		/// <param name="args">Event args.</param>
		void OnPropertyValueChanged(object sender, ZPropertyValueChangedEventArgs args)
		{
			if (args.Property.Name == nameof(GlobalCommercialInvoiceHeader.GIH_InvoiceNumber))
			{
				var header = (GlobalCommercialInvoiceHeader)sender;
				var sourceInvoiceNumbers = new ZString[] { (ZString)args.OldValue, header.GIH_InvoiceNumber };

				foreach (var invoiceNumber in sourceInvoiceNumbers)
				{
					if (!invoiceNumber.IsEmpty)
					{
						this.Where((x) => !x.IsDeleted && x != header && x.GIH_InvoiceNumber == invoiceNumber)
							.ToList()
							.ForEach((x) => x.Validation.ValidateGIH_InvoiceNumber());
					}
				}
			}
		}
	}
}
