namespace Enterprise.Customs.Business
{
	public class ProductInformationDeliveryContext
	{
		public string sender;

		public string[] receivers;

		public Integration integration;

		public Context context;

		public Classification[] requested_classifications;

		public Line[] lines;

		public class Integration
		{
			public string type;
			public string ehub_id;
			public string declaration_number;
		}

		public class Context
		{
			public string supplier;
			public string consignee;
			public string masterbill;
			public string housebill;
			public string voyage_flight;
			public string owner_ref;
			public string port_of_loading;
		}

		public class Classification
		{
			public string imp_exp;
			public string country;
		}
#pragma warning disable CW1161 // Res.GetString Analyzer
		public const string JobTypeImport = "i";
		public const string JobTypeExport = "e";
#pragma warning restore CW1161 // Res.GetString Analyzer

		public class Line
		{
			public Integration integration;
			public Context context;
			public string desc;

			public class Integration
			{
				public string invoice_number;
				public string invoice_line;
			}

			public class Context
			{
				public string part_no;
			}
		}
	}
}
