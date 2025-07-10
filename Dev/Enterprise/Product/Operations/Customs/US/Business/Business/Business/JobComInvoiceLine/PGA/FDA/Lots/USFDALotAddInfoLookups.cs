
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USFDALotAddInfoLookups : AutoUSFDALotAddInfoLookups
	{
		public USFDALotAddInfoLookups(AutoUSFDALotAddInfo parent) : base(parent)
		{
		}

		public TemperatureQualifierList TemperatureQualifierList
		{
			get { return Factory.GetCachedValue<TemperatureQualifierList>(); }
		}

		public DegreeTypeList DegreeTypeList
		{
			get { return Factory.GetCachedValue<DegreeTypeList>(); }
		}

		public PGAStorageTypeList LocationOfTempList
		{
			get { return Factory.GetCachedValue<PGAStorageTypeList>(); }
		}

		public CodeDescriptionPairList LotNumberTypeCodeList
		{
			get
			{
				return Factory.GetCachedValue("CPSCLotNumberTypeCodeList",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(LotNumberQualifierList.Codes._1, LotNumberQualifierList.Descriptions._1);
						result.AddPair(LotNumberQualifierList.Codes._2, LotNumberQualifierList.Descriptions._2);
						return result;
					});
			}
		}
	}
}
