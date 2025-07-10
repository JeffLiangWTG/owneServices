using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Warehouse.DataTransfer.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsDocketValueObjectDataUniversalAdapter : ValueObjectDataAdapter<WhsDocket, Xsd.WhsDocket>
	{
		#region Overrides

		public override System.Xml.Schema.XmlSchema CollectionSchema => WarehouseXmlSchemaDefinitions.Instance.WhsDocketsSchema;

		protected override WhsDocket FindBusinessObject(Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			var iValueObjectDataAdapter = Adapter(value, context);
			return iValueObjectDataAdapter != null ? (WhsDocket)iValueObjectDataAdapter.FindBusinessObject(value, context) : null;
		}

		protected override WhsDocket NewBusinessObject(Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			var iValueObjectDataAdapter = Adapter(value, context);
			return iValueObjectDataAdapter != null ? (WhsDocket)iValueObjectDataAdapter.NewBusinessObject(value, context) : null;
		}

		public override void ImportFromValueObject(WhsDocket bizObj, Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			var iValueObjectDataAdapter = Adapter(value, context);
			if (iValueObjectDataAdapter != null)
			{
				iValueObjectDataAdapter.ImportFromValueObject(bizObj, value, context);
			}
		}

		public void ImportFromValueObject(WhsDocketCollection whsDockets, Xsd.WhsDockets value, IValueObjectImportContext context)
		{
			foreach (Xsd.WhsDocket xsdValue in value.WhsDocket)
			{
				var docket = CreateOrUpdateFromValueObject(xsdValue, context);
				if (docket != null)
				{
					whsDockets.Add(docket);
				}
			}
		}

		protected override void ExportToValueObjectCore(WhsDocket bizObj, Xsd.WhsDocket constructedValueObject, IValueObjectExportContext context)
		{
			throw new NotSupportedException("The method or operation is not implemented.");
		}

		protected override void ImportFromValueObjectCore(WhsDocket bizObj, Xsd.WhsDocket value, IValueObjectImportContext context)
		{
		}

		public override string RootCollectionElementName => "WhsDockets";

		public override string RootElementName => "WhsDocket";

		public override System.Xml.Schema.XmlSchema Schema => WarehouseXmlSchemaDefinitions.Instance.SingleWhsDocketSchema;

		#endregion

		#region Implementation

		protected IValueObjectDataAdapter GetAdapter(ZString docketType)
		{
			IValueObjectDataAdapter result = null;
			switch (docketType)
			{
				case DocketTypes.Codes.WhsOrder:
					result = new WhsOrderValueObjectDataAdapter();
					break;
				case DocketTypes.Codes.WhsASN:
					result = new WhsReceiveValueObjectDataAdapter();
					break;
			}
			return result;
		}

		protected IValueObjectDataAdapter Adapter(Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			return Adapter(value.Identifier.DocketType, context);
		}

		protected IValueObjectDataAdapter Adapter(ZString docketType, IValueObjectImportContext context)
		{
			if (adapter == null || currentAdapterDocketType != docketType)
			{
				adapter = GetAdapter(docketType);
				currentAdapterDocketType = docketType;
			}

			if (adapter == null)
			{
				context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("9c1c7e62-f8eb-4a79-88a8-1260bbf4b1ed", "'{0}' is not a valid docket type.", docketType)));
			}

			return adapter;
		}

		ZString currentAdapterDocketType = ZString.Empty;
		IValueObjectDataAdapter adapter;

#if DEBUG
		public IValueObjectDataAdapter LastUsedAdapter
		{
			get
			{
				return adapter;
			}
		}
#endif

		#endregion
	}
}
