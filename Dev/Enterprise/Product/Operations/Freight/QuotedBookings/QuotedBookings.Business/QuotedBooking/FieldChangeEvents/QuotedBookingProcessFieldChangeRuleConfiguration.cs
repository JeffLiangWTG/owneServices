using System.Collections.Generic;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	class QuotedBookingProcessFieldChangeRuleConfiguration : IProcessFieldChangeRuleConfiguration
	{
		public IEnumerable<ITableSchema> Schemas => new ITableSchema[] { JobShipmentSchema.Instance, RatingHeaderSchema.Instance };

		public HashSet<ZString> BlacklistedColumns => new HashSet<ZString>()
		{
			JobShipmentSchema.Constants.JS_IsForwardRegistered,
			JobShipmentSchema.Constants.JS_TH_OneTimeQuote,
			RatingHeaderSchema.Constants.TH_OneTimeQuote,
			RatingHeaderSchema.Constants.TH_QuoteDate,
			RatingHeaderSchema.Constants.TH_OH,
			RatingHeaderSchema.Constants.TH_GlobalRateLevel,
		};

		public ZString GetFieldColumnDescription(SchemaColumn fieldColumn) => ProcessFieldChangeRuleColumnDescription.DefaultDescription(fieldColumn);
	}
}
