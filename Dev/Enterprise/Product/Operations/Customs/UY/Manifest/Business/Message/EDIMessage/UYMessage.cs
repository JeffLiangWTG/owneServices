using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class UYMessage : EDIMessage, Integration.Customs.UY.IUYMessage
	{
		public UYMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{ }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.UYCustoms;
		}

		protected override string GetMessageReferenceNumber() => Env.NumberFountains.GetOutgoingUYCustomsMessageNumber().GetNext(Factory).ToString();

		protected override IStreamFormatter MessageStreamFormatter => new UYCMessageStreamFormatter(new ZStringBuilder().ToStringWithNewLineBetweenAppends());

		protected override string MessageNumberPlaceHolderOverride => MessageNumberPlaceHolderHtml;

		public override bool UsesPlaceHolders => true;
	}
}
