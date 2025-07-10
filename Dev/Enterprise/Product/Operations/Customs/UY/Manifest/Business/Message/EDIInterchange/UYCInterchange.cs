using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class UYCInterchange : EDIInterchange
	{
		public const string UYCustomsForTest = "UYCustomsTEST";

		public const string UYCustomsForProd = "UYCustomsPROD";

		public UYCInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = ApplicationCodes.UYCustoms;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override Type GetMessageTypeToCreate(ZString messageText)
		{
			return typeof(UYMessage);
		}

		protected override bool ShouldSendViaEHubCore => EI_TransportType != EDIInterchange.TransportType.xT;
	}
}
