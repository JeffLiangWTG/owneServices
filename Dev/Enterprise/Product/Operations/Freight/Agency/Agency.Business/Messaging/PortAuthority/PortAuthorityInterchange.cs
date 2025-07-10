using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public class PortAuthorityInterchange : EDIInterchange
	{
		public PortAuthorityInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		protected override ZString GetInterchangeNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("I",
				MessagingHelper.Hash(EI_From),
				(NoResString)"Port" + MessagingHelper.Hash(EI_To) // hard-coded constant
			).GetNextFormatted(Factory).ToUpper(CultureInfo.InvariantCulture);
		}
	}
}
