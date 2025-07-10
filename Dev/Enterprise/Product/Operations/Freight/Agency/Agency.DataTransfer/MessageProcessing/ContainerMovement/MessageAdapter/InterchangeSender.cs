using CargoWise.Common;
using Enterprise.Freight.Agency.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing
{
	class InterchangeSender : ICMMOrganisationData
	{
		readonly string code;

		public InterchangeSender(EDIInterchange interchange)
		{
			Argument.NotNull(interchange, "interchange");
			code = interchange.EI_From;
		}

		#region ICMMOrganisationData Members

		string ICMMOrganisationData.Code
		{
			get { return code; }
		}

		CMMOrganisationType ICMMOrganisationData.CodeType
		{
			get { return CMMOrganisationType.MutuallyDefined; }
		}

		#endregion
	}
}
