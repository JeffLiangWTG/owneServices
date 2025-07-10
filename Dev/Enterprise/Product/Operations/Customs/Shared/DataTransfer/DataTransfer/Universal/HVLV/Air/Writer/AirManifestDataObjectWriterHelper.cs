using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest
{
	public class AirManifestDataObjectWriterHelper : UniversalCommonHelper
	{
		public AirManifestDataObjectWriterHelper(AirManifestDataObjectWriterHelper list)
			: this(list.factory)
		{
			this.list = list;
		}

		public AirManifestDataObjectWriterHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AirManifestDataObjectWriterHelper(CusMAWB mawb)
			: this(mawb.Factory)
		{
			this.mawb = mawb;
		}

		public AirManifestDataObjectWriterHelper(CusHAWB hawb, AirManifestDataObjectWriterHelper list)
			: this(list)
		{
			this.hawb = hawb;
			this.mawb = hawb.MAWB;
		}

		public AirManifestDataObjectWriterHelper(CusHAWB hawb)
			: this(hawb.Factory)
		{
			this.hawb = hawb;
			this.mawb = hawb.MAWB;
		}

		public static ZString GetCountryCode(CusMAWB mawb)
		{
			var result = ZString.Empty;
			if (mawb != null)
			{
				var branch = mawb.Branch ?? mawb.Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
				var country = branch.Country ?? mawb.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				result = country.RN_Code;
			}
			return result;
		}

		public static ZString GetCountryCode(CusHAWB hawb)
		{
			var result = ZString.Empty;
			if (hawb != null)
			{
				result = GetCountryCode(hawb.MAWB);
			}
			return result;
		}

		protected CusMAWB MAWB
		{
			get
			{
				var result = mawb;
				if (result == null && list != null)
				{
					result = list.MAWB;
				}
				return result;
			}
		}
		readonly CusMAWB mawb;

		protected CusHAWB HAWB
		{
			get
			{
				var result = hawb;
				if (result == null && list != null)
				{
					result = list.HAWB;
				}
				return result;
			}
		}
		readonly CusHAWB hawb;

		readonly protected AirManifestDataObjectWriterHelper list;

		#region List

		public ICodeDescriptionPairList ForwardingTransportTypeList
		{
			get { return factory.GetCachedCodeDescriptionPairList(OLookUpEditType.TransportType); }
		}

		public ICodeDescriptionPairList ForwardingAgentTypeList
		{
			get { return factory.GetCachedCodeDescriptionPairList(OLookUpEditType.AgentType); }
		}

		public ICodeDescriptionPairList ForwardingShipmentTypeList
		{
			get { return factory.GetCachedCodeDescriptionPairList(OLookUpEditType.ShipmentType); }
		}

		public WayBillTypeList WayBillTypeList
		{
			get { return factory.GetCachedValue<WayBillTypeList>(); }
		}

		public Freight.Common.Business.BindToLists BindToLists
		{
			get { return Freight.Common.Business.BindToLists.GetCachedLists(factory); }
		}

		#endregion
	}
}
