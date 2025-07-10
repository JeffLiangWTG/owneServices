using System;
using System.Xml.Schema;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public class CommonPickupDeliveryConfirmValueObjectDataAdapter : ValueObjectDataAdapter<CommonPickupDeliveryConfirm, Xsd.ContainerLeg>
	{
		#region Override

		public override XmlSchema Schema
		{
			get { return FreightXmlSchemaDefinitions.Instance.SingleContainerLeg; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return null; }
		}

		public override string RootCollectionElementName
		{
			get { return "ContainerLegs"; }
		}

		public override string RootElementName
		{
			get { return "ContainerLeg"; }
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(CommonPickupDeliveryConfirm bizObj, Xsd.ContainerLeg constructedValueObject, IValueObjectExportContext context)
		{
			string errorContext = Res.GetString("540153b7-91ca-4ab2-9ef6-4e429705993a", "House bill:") + (bizObj.FirstShipment != null ? bizObj.FirstShipment.JS_HouseBill.ToString() : string.Empty);
			constructedValueObject.LegType = ContainerLegTypeToXmlCodeMappings.Instance.GetExternalCode(bizObj.EU_PickupDeliveryType, errorContext, context);
			constructedValueObject.GoodsRecBy = bizObj.EU_GoodsSignForBy;
		}

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(CommonPickupDeliveryConfirm bizObj, Xsd.ContainerLeg value, IValueObjectImportContext context)
		{
			throw new NotSupportedException("Import functionality is not available for pickup/delivery confirmations");
		}

		#endregion
	}
}
