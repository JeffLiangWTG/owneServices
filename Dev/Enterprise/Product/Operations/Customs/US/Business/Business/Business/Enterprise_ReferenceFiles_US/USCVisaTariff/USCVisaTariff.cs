using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public sealed class USCVisaTariff : AutoUSCVisaTariff
	{
		public USCVisaTariff(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("Visa")]
		public override ZGuid UK_UO
		{
			get { return base.UK_UO; }
			set { base.UK_UO = value; }
		}

		public USCVisa Visa
		{
			get
			{
				if (fVisa == null)
				{
					fVisa = Factory.Load<USCVisa>(UK_UO);
				}
				return fVisa;
			}
		}
		USCVisa fVisa;
	}
}
