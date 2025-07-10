using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.QuotedBookings.DataTransfer
{
	public class QuotedBookingContainerValueObjectDataAdapter<TBusinessObject, TValueObject> : JobContainerValueObjectDataAdapter<TBusinessObject, TValueObject>
		where TBusinessObject : CommonContainer
		where TValueObject : Xsd.Container
	{
		public QuotedBookingContainerValueObjectDataAdapter(QuotedBooking quotedBooking)
		{
			this.QuotedBooking = quotedBooking;
		}

		public readonly QuotedBooking QuotedBooking;

		protected override TBusinessObject FindBusinessObject(TValueObject containerValue, IValueObjectImportContext context)
		{
			ZQuery filter = new ZQuery(JobContainerSchema.JC_ContainerNum, SQLComparisonOperator.Equal, containerValue.ContainerNumber);
			BusinessObject[] result = QuotedBooking.QuotedBookingContainers.Find(filter);
			return (result.Length == 1) ? (TBusinessObject)result[0] : null;
		}

		protected override bool RegistryDefaultForImporting
		{
			get { return SystemRegistry.UpdateBookingContainersDuringAutomaticImport.Value; }
		}

		#region Implementation

		protected override TBusinessObject NewBusinessObject(TValueObject value, IValueObjectImportContext context)
		{
			return (TBusinessObject)(CommonContainer)QuotedBooking.QuotedBookingContainers.AddNew();
		}

		#endregion
	}
}
