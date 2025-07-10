using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusInBondEventCollection<T> : ActiveBusinessObjectCollection<T> where T : CusInBondEvent
	{
		public CusInBondEventCollection(CusInBondHeader master, ZString typeToMatch)
			: base(master, new ZQuery(CusInBondEventSchema.BN_Type, typeToMatch))
		{
			this.typeToMatch = Argument.NotNullOrEmpty(typeToMatch, nameof(typeToMatch));
		}
		readonly string typeToMatch;

		protected override void SetDefaultsForNewElementCore(T newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.BN_Type = typeToMatch;
		}
	}
}
