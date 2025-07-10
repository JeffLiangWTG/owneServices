using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;

namespace Enterprise.GlobalCommercialInvoice.Business
{
	/// <summary>
	/// Header for a Global Commercial Invoice.
	/// </summary>
	public class GlobalCommercialInvoiceLineIntegratedCollection : ActiveBusinessObjectCollection<GlobalCommercialInvoiceLine>
	{
		/// <summary>
		/// The collection of headers of the same invoice.
		/// </summary>
		readonly GlobalCommercialInvoiceHeaderCollection headers;

		/// <summary>
		/// Previously used header PK that is used to suggest a header when a new line is being created.
		/// </summary>
		[SuppressCollectionStateTest]
		ZGuid lastHeaderPK;

		/// <summary>
		/// Flag that prevents from recursion while recalculating line numbers.
		/// </summary>
		[SuppressCollectionStateTest]
		bool isRecalculatingLineNumbers;

		/// <summary>
		/// The host (parent) business object.
		/// Normally a Shipment or Booking.
		/// </summary>
		BusinessObject hostBusinessObject { get; }

		/// <summary>
		/// Header for a Global Commercial Invoice.
		/// </summary>
		/// <param name="factory">Business Object Factory.</param>
		/// <param name="invoiceHeaders">Parent <see cref="GlobalCommercialInvoiceHeaderCollection"/>.</param>
		public GlobalCommercialInvoiceLineIntegratedCollection(BusinessObject hostBizObj, GlobalCommercialInvoiceHeaderCollection invoiceHeaders)
			: base(hostBizObj.Factory, new GlobalCommercialInvoiceLineIntegratedCollectionRelationship(invoiceHeaders))
		{
			lastHeaderPK = ZGuid.Empty;
			hostBusinessObject = hostBizObj;
			headers = invoiceHeaders;
			headers.ToList().ForEach(header => { header.PropertyValueChanged += InvoiceHeader_PropertyValueChanged; });
			headers.CollectionCountChange += InvoiceHeader_OnCollectionCountChange;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GlobalCommercialInvoiceLine"/> class.
		/// Here we can assign the <see cref="headers"/> to the <see cref="GlobalCommercialInvoiceLine"/> instance
		/// and also set the header value.
		/// </summary>
		/// <param name="newElement">New instance of the <see cref="GlobalCommercialInvoiceLine"/> class.</param>
		protected override void SetDefaultsForNewElementCore(GlobalCommercialInvoiceLine newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.Headers = headers;
			newElement.PropertyValueChanged += InvoiceLine_PropertyValueChanged;

			if (headers.Count > 0)
			{
				newElement.GIL_GIH_Header = lastHeaderPK.IsEmpty ? headers.Last().PK : lastHeaderPK;
				newElement.GIL_GIH_HeaderInfo.PushValueIntoRow();

				var headerExistingLines = this
					.Where((x) => x.GIL_GIH_Header == newElement.GIL_GIH_Header)
					.ToArray();
				var lastLineNo = headerExistingLines.Length > 0 ? headerExistingLines.Max((x) => x.GIL_LineNo) : 0;

				newElement.GIL_LineNo = (short)(lastLineNo + 1);
				newElement.GIL_LineNoInfo.PushValueIntoRow();

				SetDefaultsForNewElementUnitOfMeasurement(newElement);
			}
		}

		/// <summary>
		/// Allows to update an existing item of the <see cref="GlobalCommercialInvoiceLine"/> type.
		/// Here we can assign the <see cref="headers"/> to the <see cref="GlobalCommercialInvoiceLine"/> instance.
		/// </summary>
		/// <param name="loadingObject">The instance of <see cref="GlobalCommercialInvoiceLine"/> to update.</param>
		protected override void OnLoadingIntoCollectionCore(GlobalCommercialInvoiceLine loadingObject)
		{
			base.OnLoadingIntoCollectionCore(loadingObject);

			loadingObject.Headers = headers;
			loadingObject.PropertyValueChanged -= InvoiceLine_PropertyValueChanged;
			loadingObject.PropertyValueChanged += InvoiceLine_PropertyValueChanged;
		}

		/// <summary>
		/// Is called when a <see cref="GlobalCommercialInvoiceLine"/> is deleted.
		/// </summary>
		/// <param name="line">An instance of <see cref="GlobalCommercialInvoiceLine"/> to be deleted.</param>
		public override void Delete(GlobalCommercialInvoiceLine line)
		{
			var headerPK = line.GIL_GIH_Header;
			line.PropertyValueChanged -= InvoiceLine_PropertyValueChanged;
			base.Delete(line);

			RecalculateLineNumbers(headerPK);
		}

		/// <summary>
		/// Deletes related lines when a header is deleted.
		/// </summary>
		/// <param name="headerPK">Deleted <see cref="GlobalCommercialInvoiceHeader"/> PK.</param>
		void RemovedHeaderDeleteLine(ZGuid headerPK) => this.Where((x) => x.GIL_GIH_Header == headerPK).ToList().ForEach(Delete);

		#region Invoice Line Events

		/// <summary>
		/// An event handler that is called when a <see cref="GlobalCommercialInvoiceLine"/> (i.e. collection item) property value is changed.
		/// </summary>
		/// <param name="sender">An instance of <see cref="GlobalCommercialInvoiceLine"/> that is changed.</param>
		/// <param name="args">Event args.</param>
		void InvoiceLine_PropertyValueChanged(object sender, ZPropertyValueChangedEventArgs args)
		{
			var line = (GlobalCommercialInvoiceLine)sender;

			if (args.Property.Name == nameof(GlobalCommercialInvoiceLine.GIL_GIH_Header))
			{
				var oldHeaderPK = (ZGuid)args.OldValue;
				lastHeaderPK = line.GIL_GIH_Header;
				RecalculateLineNumbers(oldHeaderPK);
				RecalculateLineNumbers(line.GIL_GIH_Header, line);

				var oldHeader = headers.FirstOrDefault((x) => x.PK == oldHeaderPK);
				var header = headers.Single((x) => x.PK == line.GIL_GIH_Header);

				if (oldHeader is not null && (line.GIL_Description.IsEmpty || line.GIL_Description == oldHeader.GIH_Description))
				{
					line.GIL_Description = header.GIH_Description;
				}

				header.PropertyValueChanged -= InvoiceHeader_PropertyValueChanged;
				header.PropertyValueChanged += InvoiceHeader_PropertyValueChanged;
			}
			else if (args.Property.Name == nameof(GlobalCommercialInvoiceLine.GIL_LineNo))
			{
				RecalculateLineNumbers(line.GIL_GIH_Header, line);
			}
		}

		#endregion

		/// <summary>
		/// Recalculates line numbers for a header.
		/// </summary>
		/// <param name="affectedHeaderId">Header ID (PK) that contains lines to recalculate.</param>
		/// <param name="affectedLine">Line that caused recalculation. Used to sort lines. Might be null.</param>
		void RecalculateLineNumbers(ZGuid sourceHeaderPK, GlobalCommercialInvoiceLine sourceInvoiceLine = null)
		{
			if (!isRecalculatingLineNumbers && this.Count > 0)
			{
				isRecalculatingLineNumbers = true;

				var invoiceLines = this
					.Where((x) => x.GIL_GIH_Header == sourceHeaderPK)
					.ToArray();

				if (invoiceLines.Length > 0)
				{
					var reordered = invoiceLines
						.OrderBy((x) => x.GIL_LineNo)
						.ThenByDescending((x) => x == sourceInvoiceLine)
						.ToArray();

					short i = 0;
					while (i < reordered.Length)
					{
						var line = reordered[i];
						line.GIL_LineNo = ++i;
						line.GIL_LineNoInfo.PushValueIntoRow();
					}
				}

				isRecalculatingLineNumbers = false;
			}
		}

		#region Invoice Header Events

		/// <summary>
		/// likely handles changes to a header property value
		/// </summary>
		/// <param name="sender">An instance of <see cref="GlobalCommercialInvoiceLine"/> that is changed.</param>
		/// <param name="args">Event args.</param>
		void InvoiceHeader_PropertyValueChanged(object sender, ZPropertyValueChangedEventArgs args)
		{
			var header = (GlobalCommercialInvoiceHeader)sender;

			if (args.Property.Name == nameof(GlobalCommercialInvoiceHeader.GIH_Description))
			{
				var oldHeaderDescription = (ZString)args.OldValue;
				var linesToUpdate = this
					.Where((x) => x.GIL_GIH_Header == header.PK && x.GIL_Description == oldHeaderDescription);

				foreach (var line in linesToUpdate)
				{
					line.GIL_Description = header.GIH_Description;
				}
			}
		}

		void InvoiceHeader_OnCollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			lastHeaderPK = this.headers.LastOrDefault()?.PK ?? ZGuid.Empty;

			if (e.ItemRemoved) // Remove all related lines if the header is removed.
			{
				RemovedHeaderDeleteLine(e.BizObject.PK);
			}
		}

		#endregion

		/// <summary>
		/// likely initializes default values for a newly created unit of measurement element
		/// </summary>
		/// <param name="hostBusinessObject">The host(parent) business object. Normally a Shipment or Booking.</param>
		/// <param name="invoiceLine">New invoice line element</param>
		void SetDefaultsForNewElementUnitOfMeasurement(GlobalCommercialInvoiceLine invoiceLine)
		{
			if (hostBusinessObject is IGlobalCommercialInvoiceJobProvider provider)
			{
				invoiceLine.GIL_VolumeUQ = provider.VolumeUnitOfMeasurement;
				invoiceLine.GIL_VolumeUQInfo.PushValueIntoRow();
				invoiceLine.GIL_GrossWeightUQ = provider.WeightUnitOfMeasurement;
				invoiceLine.GIL_GrossWeightInfo.PushValueIntoRow();
				invoiceLine.GIL_NetWeightUQ  = invoiceLine.GIL_GrossWeightUQ;
				invoiceLine.GIL_NetWeightUQInfo.PushValueIntoRow();
			}
		}
	}
}
