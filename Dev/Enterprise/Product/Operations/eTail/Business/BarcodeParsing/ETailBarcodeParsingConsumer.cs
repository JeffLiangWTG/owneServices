using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BarcodeParsing.Business;
using Enterprise.BarcodeParsingEngine;
using Enterprise.BarcodeParsingEngine.ETail;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eTail.Business
{
	class ETailBarcodeParsingConsumer : BarcodeParsingConsumer<ETailTargetField>
	{
		public ETailBarcodeParsingConsumer(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override string BuyerCaption => Res.GetString("260cedf6-3809-4b97-8463-13c776ab2076", "eTailer");

		protected override string SupplierCaption => Res.GetString("2fca2574-25e3-4681-9f5b-301aa75e57e3", "Depot");

		protected override ZString ModuleCode => BarcodeModuleTypes.Codes.ETail;

		protected override OrgHeaderCollection Buyers => new OrganisationsFindBoxCollection(Factory); // Dmitry B now owns eTail but is unsure who the eTailer is. this will therefore possibly change once he gets back to us, or even after checkin.

		protected override OrgHeaderCollection Suppliers => new PackDepotCollection(Factory);

		protected override ReadOnlyCodeDescriptionPairList TargetFields => new ETailTargetFields();

		protected override IEnumerable<ZString> GS1TargetFieldsToDefault => Array.Empty<ZString>();

		protected override bool IsRelatedEntityAvailable => false;

		protected override bool IsBuyerAvailable => true;

		protected override bool IsSupplierAvailable => true;

		protected override IBusinessObjectCollection GetRelatedEntityList(OrgHeader buyer, OrgHeader supplier) => null;

		protected override IEnumerable<FormatType> GetValidFieldFormatsForTargetField(ZBool isGS1, ZString targetField) => Enum.GetValues(typeof(FormatType)).Cast<FormatType>();
	}
}
