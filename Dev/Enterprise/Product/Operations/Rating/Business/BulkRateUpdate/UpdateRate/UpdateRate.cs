using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class UpdateRate : AutoUpdateRate
	{
		public UpdateRate(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[List("Lookups.Clients")]
		public override ZGuid ClientPK
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.ClientPK; }
			set
			{
				var changed = ClientPK != value;
				base.ClientPK = value;

				if (changed)
				{
					DefaultFromClient();
				}
			}
		}

		public override ZString FullName
		{
			get { return Client == null ? ZString.Empty : Client.OH_FullNameTruncated; }
		}

		public OrgHeader Client
		{
			get { return Factory.Load<OrgHeader>(ClientPK); }
		}

		public ClientRate ClientRate
		{
			get
			{
				if (clientRate == null || clientRate.TH_OH != ClientPK)
				{
					clientRate = FindRateForClient(ClientPK);
				}

				return clientRate;
			}
		}
		ClientRate clientRate;

		public UpdateRateLookups Lookups
		{
			get { return lookups ?? (lookups = new UpdateRateLookups(this)); }
		}
		UpdateRateLookups lookups;

		#region Implementation

		void DefaultFromClient()
		{
			if (Client != null)
			{
				IncludeInUpdate = Client.CompanyData.OB_ARAutoUpdateRates;
			}
		}

		ClientRate FindRateForClient(ZGuid clientPK)
		{
			var filter = new ZQuery(RatingHeaderSchema.TH_OH, clientPK);
			filter.AddToFilter(RatingHeaderSchema.TH_GC, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.ClientRate);
			var result = Factory.LoadTop1<ClientRate>(filter);

			return result;
		}

		#endregion
	}
}

