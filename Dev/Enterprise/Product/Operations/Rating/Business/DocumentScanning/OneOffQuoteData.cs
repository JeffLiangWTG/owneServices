using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(OneOffQuoteData),
	Enterprise.Core.Constants.DocManagerCodes.OneOffQuote)]

namespace Enterprise.Rating.Business
{
	public class OneOffQuoteData : AssemblyData
	{
		public override Type BusinessObjectType => ObjectFactory.GetType<IViewQuotedBooking>();

		protected override Type CollectionType => ObjectFactory.GetType<IViewOneOffQuoteCollection>();

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => ObjectFactory.Get<BusinessObjectCollection>("IViewOneOffQuoteCollection", factory);

		public override ModuleIdentifier ModuleID => ModuleIDs.OneOffQuotes;

		public override string ReferenceType => Core.Constants.ReferenceTypes.ClientSupplierRelationship;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("0dfa0eba-6858-4f9e-ac57-88ebee080042", "One Off Quote");

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}


