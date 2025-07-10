using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondBillLookups : Customs.Business.CusInBondBillLookups
	{
		public CusInBondBillLookups(CusInBondBill parent)
			: base(parent)
		{
		}

		protected new CusInBondBill Parent
		{
			get { return (CusInBondBill)base.Parent; }
		}

		public IBusinessObjectCollection CarrierAndFIRMSCollection
		{
			get
			{
				var header = Parent.Header;
				return header != null && header.BH_FTZMove
					? new USCCarrierAndFIRMSCollection(Factory)
					: new USCarrierCombinedCollection(Factory);
			}
		}

		public ZZRefCusCodeListCombinedCollection ForeignPorts
		{
			get
			{
				CusInBondHeader header = Parent.Header;
				var attributeFilters = new List<RefCusCodeListAttributeFilter>();
				if (header != null && (header.IsTruck || header.IsRail))
				{
					attributeFilters.Add(new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.PortValidType, SQLComparisonOperator.Equal, new ZString[] { ForeignPortTypeList.Codes.Common, ForeignPortTypeList.Codes.InBond }));
				}
				else
				{
					attributeFilters.Add(new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.PortValidType, SQLComparisonOperator.Equal, ForeignPortTypeList.Codes.Common));
				}
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(
					Factory,
					Core.Constants.CountryCodes.UnitedStates,
					new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port },
					ZDateTime.Today,
					attributeFilters: attributeFilters);
			}
		}

		public ZZRefCusCodeListCombinedCollection RegionDistrictPorts
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public InBondManifestUQList ManifestUnitList
		{
			get { return Factory.GetCachedValue<InBondManifestUQList>(); }
		}

		public CodeDescriptionPairList WeightUnitList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public CodeDescriptionPairList VolumeUnitList
		{
			get { return Factory.GetCachedValue<VolumeUnitList>(); }
		}

		public ConsignorCollection Shippers
		{
			get { return new ConsignorCollection(Factory); }
		}

		public ConsigneeCollection Consignees
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}
	}
}
