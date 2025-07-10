using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public sealed class TransportCoHotlinkOpener
	{
		#region Instance

		public static TransportCoHotlinkOpener Instance
		{
			get { return instance ?? (instance = new TransportCoHotlinkOpener()); }
		}

		TransportCoHotlinkOpener()
		{
		}
		[ThreadStatic]
		static TransportCoHotlinkOpener instance;

		#endregion

		#region OpenWebSite

		public ZString OpenWebSite(ZString transportReferenceNo, OrgHeader transportCo)
		{
			ZString result = "";

			if (string.IsNullOrEmpty(transportReferenceNo))
			{
				result = Res.GetString("e45ccdb6-41c7-4b1d-82a5-6972fb03bdd2", "The Transport Reference is empty.");
			}
			else if (transportCo == null)
			{
				result = Res.GetString("dadebdad-cf1a-41a8-bd7a-d097a2a08dee", "No Transport Company has been selected.");
			}
			else
			{
				ZString url = transportCo.CartageTransportWebSite;
				if (url.IsEmpty)
				{
					result = Res.GetString("7b730ae9-f0a4-4097-b6c4-1310230734c3", "The Transport Company {0} does not have a {1} web address.", transportCo.OH_FullNameTruncated, CartageTrackingDescription);
				}
				else if (!UrlValidation.IsValidUrl(url))
				{
					// in case someone hacks a bad url into the db.
					result = Res.GetString("bf14fce9-6dce-40bf-aec7-d0e8a5d32d6b", "The Transport Company {0} does not have a valid {1} web address.", transportCo.OH_FullNameTruncated, CartageTrackingDescription);
				}
				else if (!url.ToLower().Contains(Constants.TransportCoHotlinkOpener.CargoWiseREF.ToLower()))
				{
					result = Res.GetString("776d6214-cb6c-4657-9c9c-1b9827ec397a", "The {0} web address for transport company {1} is missing the text {2}.", CartageTrackingDescription, transportCo.OH_FullNameTruncated, Constants.TransportCoHotlinkOpener.CargoWiseREF);
				}
				else
				{
#if DEBUG
					if (!Globals.IsTest)
					{
#endif
						try
						{
							ZString urlWithTransportRef = url.ReplaceIgnoringCase(Constants.TransportCoHotlinkOpener.CargoWiseREF, transportReferenceNo);
							WebUrlLauncher.Launch(urlWithTransportRef);
						}
						catch (Exception e) when (!e.IsCriticalException())
						{
							result = e.Message;
						}
					}
#if DEBUG
				}
#endif
			}

			return result;
		}

		public ZString CartageTrackingDescription
		{
			get { return OrgWebUrlList.Descriptions.CartageTracking; }
		}

		#endregion
	}
}
