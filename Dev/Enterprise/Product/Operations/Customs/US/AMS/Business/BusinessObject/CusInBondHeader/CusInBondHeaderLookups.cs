using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondHeaderLookups : Customs.Business.CusInBondHeaderLookups
	{
		public CusInBondHeaderLookups(CusInBondHeader parent)
			: base(parent)
		{
		}

		public override GlbBranchCollection Branches
		{
			get { return new GlbBranchNotCurrentCompanyRelatedCollection(Factory); }
		}

		public ZZRefCusCodeListCombinedCollection ScheduleDList
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public ZZRefCusCodeListCombinedCollection FIRMSList
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today); }
		}

		public USCarrierCombinedCollection SCACList
		{
			get
			{
				var result = new USCarrierCombinedCollection(Factory);
				if (!Parent.BH_ImportTransportMode.IsEmpty)
				{
					result.AdditionalFilter = new ZQuery(USCarrierCombinedSchema.UI_ModeOfTransportation, SQLComparisonOperator.StartsWith, Parent.BH_ImportTransportMode.SubstringSafe(0, 1));
					result.AddNotificationWhenAdditionalFilterNotMetOverride = AddNotificationWhenAdditionalFilterNotMetOverrideDelegate;
				}
				return result;
			}
		}

		void AddNotificationWhenAdditionalFilterNotMetOverrideDelegate(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			if (!Parent.BH_ImportTransportMode.IsEmpty)
			{
				var carrier = (USCarrierCombined)selectedBusinessObject;
				if (carrier.UI_ModeOfTransportation != Parent.BH_ImportTransportMode)
				{
					errors.Add("This carrier's mode of transportation is different to the import carrier's transport mode.");
				}
			}
		}

		public CodeDescriptionPairList ConveyanceTransportTypeList
		{
			get { return TransportTypeList.GetConveyanceTransportTypeList(Factory); }
		}

		public RefVesselCollection RefVessels
		{
			get { return new RefVesselCollection(Factory); }
		}

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		protected new CusInBondHeader Parent
		{
			get { return (CusInBondHeader)base.Parent; }
		}
	}
}
