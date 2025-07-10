using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class ConstituentWrapper : IConstituent
	{
		public ConstituentWrapper(ZString elementDescription, ZString levelID, ZString thickness)
		{
			this.elementDescription = elementDescription;
			this.levelID = levelID;
			this.thickness = thickness;
		}

		ZString IConstituent.ElementDescription => elementDescription;

		ZString IConstituent.LevelID => levelID;

		ZString IConstituent.Thickness => thickness;

		readonly ZString elementDescription;

		readonly ZString levelID;

		readonly ZString thickness;
	}
}
