using CargoWise.Application;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPartRelationLookups : AutoOrgPartRelationLookups
	{
		public OrgPartRelationLookups(AutoOrgPartRelation parent)
			: base(parent)
		{
		}

		#region Headers

		public override OrgHeaderCollection Headers
		{
			get
			{
				return new WarehouseClientCollectionWithSecurityCheck(Factory);
			}
		}

		#endregion

		#region CartonGroups

		public IWhsCartonGroupCollection CartonGroups
		{
			get { return Factory.GetCachedValue("OrgPartRelationLookups|CartonGroups", () => ObjectFactory.New<IWhsCartonGroupCollection>(Factory)); }
		}

		#endregion

		#region UQ_List

		public CodeDescriptionPairList UQ_List
		{
			get { return new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits(); }
		}

		#endregion

		#region RFAttributeConfirmList

		public CodeDescriptionPairList RFAttributeConfirmList => Parent.Factory.GetCachedValue<CodeDescriptionPairList>("RFAttributeConfirmList", () => new RFAttributeConfirmCode());

		#endregion

		#region PickModeList

		public WhsPickMode PickModeList
		{
			get { return Factory.GetCachedValue("OrgPartRelationLookups|PickModeList", () => new WhsPickMode()); }
		}

		#endregion

		#region JulianBatchNumberFormatsList

		public JulianBatchNumberFormatList JulianBatchNumberFormatsList
		{
			get { return Factory.GetCachedValue("OrgPartRelationLookups|JulianBatchNumberFormatsList", () => new JulianBatchNumberFormatList()); }
		}

		#endregion

		#region HoldCodes

		public IWhsInventoryHeldCodeCollection HoldCodes
		{
			get
			{
				var parentAsRelation = (OrgPartRelation)Parent;
				var key = $"OrgPartRelationLookups|HoldCodes|{parentAsRelation.OU_OH}";
				return Factory.GetCachedValue(key,
					() => ObjectFactory.Get<IWhsInventoryHeldCodeCollection>(nameof(IWhsInventoryHeldCodeCollection), Factory, ((OrgPartRelation)Parent).OU_OH));
			}
		}

		#endregion
	}
}
