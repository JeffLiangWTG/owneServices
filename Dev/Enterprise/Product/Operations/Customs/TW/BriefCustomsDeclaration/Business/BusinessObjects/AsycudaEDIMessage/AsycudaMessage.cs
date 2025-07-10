using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaMessage : AsycudaEDIMessage
	{
		public AsycudaMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZBool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride() => ZBool.True;

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.GetOutgoingTWCustomsMessageNumber().GetNextFormatted(Factory);
		}
	}
}
