using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5167
{
	public class AdditionalDeclaration : IAdditionalDeclaration
	{
		public AdditionalDeclaration(string id)
		{
			ID = id;
		}

		public ZString ID { get; private set; }

		public ZString TypeCode => ZString.Empty;

		public ZDecimal SequenceNumeric => ZDecimal.Zero;
	}
}
