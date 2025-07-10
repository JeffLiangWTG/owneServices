using CargoWise.Types;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SG4MessageManager : MessageManager
	{
		public SG4MessageManager(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override ZString MessageApplicationCode => ApplicationCodeList.Codes.SGCustomsTradenet4;
	}
}
