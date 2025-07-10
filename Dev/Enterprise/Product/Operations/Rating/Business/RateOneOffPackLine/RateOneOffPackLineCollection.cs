using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.Business
{
	[CodeAlive("is going to used to store the Loose Cargo in One Off Quote")]
	public class RateOneOffPackLineCollection : ActiveBusinessObjectCollection<RateOneOffPackLine>
	{
		public RateOneOffPackLineCollection(RateOneOffShipment master) : base(master.Factory, master, null, RateOneOffPackLineSchema.TPL_TT_RateOneOffShipment)
		{ }

		public int LooseCargoPackageCount
		{
			get
			{
				var result = 0;
				foreach (var packLine in this)
				{
					result += packLine.TPL_PackLineCount;
				}

				return result;
			}
		}
	}
}
