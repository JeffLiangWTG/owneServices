using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CusInBondMoveDetailLookups : Customs.Business.CusInBondMoveDetailLookups
	{
		public CusInBondMoveDetailLookups(CusInBondMoveDetail parent)
			: base(parent)
		{
		}

		public virtual ICodeDescriptionPairList MessageStatusList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public virtual ICodeDescriptionPairList CustomsStatusList
		{
			get { return new CodeDescriptionPairList(); }
		}

		protected new CusInBondMoveDetail Parent
		{
			get { return (CusInBondMoveDetail)base.Parent; }
		}

		public RefVesselCollection ConveyanceList
		{
			get { return new RefVesselCollection(Factory); }
		}

		public ZZRefCusCodeListCombinedCollection ForeignPorts
		{
			get
			{
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(
					Factory,
					Core.Constants.CountryCodes.UnitedStates,
					new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port },
					ZDateTime.Today,
					new[]
					{
						new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.PortValidType, SQLComparisonOperator.Equal,
						new ZString[] { ForeignPortTypeList.Codes.Common, ForeignPortTypeList.Codes.InBond })
					});
			}
		}
	}
}
