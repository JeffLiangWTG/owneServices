using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using GlowRegistry = Enterprise.Registry.Business.GlowRegistry;

namespace Enterprise.MasterFiles.Business
{
	public class TrackingUrlCreator
	{
		public static TrackingUrlCreator Instance
		{
			get { return new TrackingUrlCreator(); }
		}

		#region Generate URL

		public string CreateUrl(ZGuid contactPK, TrackingConstants.BusinessContext businessContext, ZGuid businessContextPK, params ZGuid[] businessContextAdditionalRefs)
		{
			if (GetEnableNeoHyperlinks() && GetIsNeoEnabled())
			{
				var alias = businessContext.ToNeoAlias();
				if (!string.IsNullOrEmpty(alias))
				{
					return TrackingUrlBuilder.BuildNeoUrl(GetGlowPortalsRootUrl(), alias, GetGuid(businessContextPK));
				}

				if (businessContext.CanUseGuestTracking())
				{
					return TrackingUrlBuilder.BuildNeoGuestTrackingUrl(GetGlowPortalsRootUrl(), GetGuid(businessContextPK));
				}
			}

			return TrackingUrlBuilder.BuildUrl(GetWebTrackerRootUrl(), GetGuid(contactPK), businessContext, GetGuid(businessContextPK), businessContextAdditionalRefs.Select(x => x.ToGuid()).ToArray());
		}

		public Guid GetGuid(ZGuid value)
		{
			if (value.IsEmpty || !value.IsValid)
			{
				return Guid.Empty;
			}
			return value.ToGuid();
		}

		public string CreateUrl(ZGuid contactPK)
		{
			return CreateUrl(contactPK, TrackingConstants.BusinessContext.NoBusinessContext, ZString.Empty);
		}

		public string CreateUrl(ZGuid contactPK, TrackingConstants.BusinessContext businessContext, ZString businessContextNK)
		{
			if (GetEnableNeoHyperlinks() && GetIsNeoEnabled())
			{
				var alias = businessContext.ToNeoAlias();
				if (!string.IsNullOrEmpty(alias))
				{
					return TrackingUrlBuilder.BuildNeoUrl(GetGlowPortalsRootUrl(), alias, businessContextNK);
				}

				if (businessContext.CanUseGuestTracking())
				{
					return TrackingUrlBuilder.BuildNeoGuestTrackingUrl(GetGlowPortalsRootUrl(), businessContextNK);
				}
			}

			return TrackingUrlBuilder.BuildUrl(GetWebTrackerRootUrl(), GetGuid(contactPK), businessContext, businessContextNK);
		}

		bool GetEnableNeoHyperlinks()
		{
			return GlowRegistry.Instance.NeoEnableHyperlinksInDocumentMacros.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		bool GetIsNeoEnabled()
		{
			return ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.NeoFeature) != null;
		}

		string GetGlowPortalsRootUrl()
		{
			return GlowRegistry.Instance.GlowPortalsUri.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		string GetWebTrackerRootUrl()
		{
			return WebDataRegistry.Instance.WebTrackerUrl.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
		}

		#endregion
	}
}
