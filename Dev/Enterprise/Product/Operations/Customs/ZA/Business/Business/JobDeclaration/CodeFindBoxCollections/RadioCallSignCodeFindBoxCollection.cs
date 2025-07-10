using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	[ModuleID(ModuleId.RefVesselZZ)]
	public class RadioCallSignCodeFindBoxCollection : ActiveBusinessObjectCollection<RefVesselZZForRadioCallSign>
	{
		public RadioCallSignCodeFindBoxCollection(BusinessObjectFactory factory) : base(factory, new ZQuery(RefVesselZZSchema.ZZO_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.SouthAfrica))
		{
		}

		public static class FilterConstants
		{
			public const string RadioCallSign = "Radio Call Sign";
			public const string VesselName = "Vessel Name";
		}

		protected override void SetDefaultsForNewElementCore(RefVesselZZForRadioCallSign newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.ZZO_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
		}

		public static RadioCallSignCodeFindBoxCollection GetCachedCollection(BusinessObjectFactory factory, ZString radioCall, ZString vesselName)
		{
			var key = string.Format(CultureInfo.InvariantCulture, "RadioCallSignCodeFindBoxCollection_{0}_{1}", radioCall, vesselName);
			return factory.GetCachedValue(key, () =>
				{
					var collection = new RadioCallSignCodeFindBoxCollection(factory);
					if (!radioCall.IsEmpty)
					{
						collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterConstants.RadioCallSign, "Property", radioCall));
					}
					if (!vesselName.IsEmpty)
					{
						collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterConstants.VesselName, "Property", vesselName));
					}
					return collection;
				});
		}
	}

	[CodeProperty(RefVesselZZ.Schema.ZZO_RadioCallSign)]
	[DescriptionProperty(RefVesselZZ.Schema.ZZO_Code)]
	public class RefVesselZZForRadioCallSign : RefVesselZZ
	{
		public RefVesselZZForRadioCallSign(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			vesselRow = row;
		}

		protected override ZString CarrierCodesCore => string.Join(", ", GetRefCarrierCodes().Select(x => x.ZZ4_Code));

		protected override ZString CarrierNamesCore => string.Join(", ", GetRefCarrierCodes().Select(x => x.ZZ4_Description));

		RefCarrierCode[] GetRefCarrierCodes()
		{
			var vesselPK = vesselRow[RefVesselZZSchema.Constants.PK];
			if (vesselPK != vesselRowPK)
			{
				vesselRowPK = vesselPK;
				var query = new ZDBOnlyQuery(typeof(RefCarrierCode));
				var refCarrierVesselPivotSubQuery = new ZDBOnlySubQuery(typeof(RefCarrierVesselPivot), RefCarrierVesselPivotSchema.ZZQ_ZZ4);
				var refVesselZZSubQuery = new ZDBOnlySubQuery(typeof(RefVesselZZ), RefCarrierVesselPivotSchema.ZZQ_ZZO);
				refVesselZZSubQuery.AddToFilter(RefVesselZZSchema.PK, vesselRowPK);
				refCarrierVesselPivotSubQuery.AddSubQuery(refVesselZZSubQuery, JoinCondition.And);
				query.AddSubQuery(refCarrierVesselPivotSubQuery, JoinCondition.And);
				carrierResults = Factory.Load<RefCarrierCode>(query);
			}
			return carrierResults;
		}

		readonly DataRow vesselRow;
		object vesselRowPK;
		RefCarrierCode[] carrierResults;
	}
}
