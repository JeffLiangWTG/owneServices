using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsDocketLineAndConversionsWebServiceResponse : WebServiceResponse
	{
		#region Constructors

		public WhsDocketLineAndConversionsWebServiceResponse()
			: base()
		{
		}

		#endregion

		#region Properties

		public WhsDocketLineInfo LineInfo
		{
			get
			{
				if (lineInfo == null)
				{
					lineInfo = new WhsDocketLineInfo();
				}
				return lineInfo;
			}
			set
			{
				lineInfo = value;
			}
		}

		#endregion

		public UnitConversionCollection Conversions
		{
			get { return conversions ?? (conversions = new UnitConversionCollection()); }
			set { conversions = value; }
		}

		#region Implementation

		WhsDocketLineInfo lineInfo;
		UnitConversionCollection conversions;

		#endregion
	}
}
