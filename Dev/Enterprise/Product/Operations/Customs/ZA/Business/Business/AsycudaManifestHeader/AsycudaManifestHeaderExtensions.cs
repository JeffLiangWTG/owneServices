using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public static class AsycudaManifestHeaderExtensions
	{
		public static COSTCOEDIMessage GetLastAcceptedCOSTCOEDIMessage(this AsycudaManifestHeader header)
		{
			return GetLastAcceptedOutgoingEDIMessage<COSTCOEDIMessage>(header);
		}

		public static GOVGIOEDIMessage GetLastAcceptedGOVGIOEDIMessage(this AsycudaManifestHeader header)
		{
			return GetLastAcceptedOutgoingEDIMessage<GOVGIOEDIMessage>(header);
		}

		static T GetLastAcceptedOutgoingEDIMessage<T>(this AsycudaManifestHeader header)
			where T : SARSEDIMessage
		{
			T result = null;

			var messages = header.Messages;
			var factory = header.Factory;
			var acceptedCUSRESs = messages.GetMatchingMessages(
					EDIMessage.ApplicationCodes.SouthAfricanCustoms,
					new ZString[] { SARSEDIMessage.MessageTypes.CUSRES },
					EDIMessage.Direction.Receive).OfType<CUSRESEDIMessage>()
				.Where(x => IsAccepted(factory, x))
				.OrderByDescending(x => x.EM_MessageDateTime);

			T[] outgoingMessages = null;
			foreach (var cusres in acceptedCUSRESs)
			{
				if (outgoingMessages == null)
				{
					outgoingMessages = messages.OfType<T>().ToArray();
				}

				var lrn = cusres.LocalReferenceNumber;
				result = outgoingMessages.FirstOrDefault(x => x.LocalReferenceNumber == lrn);
				if (result != null)
				{
					break;
				}
			}

			return result;
		}

		static bool IsAccepted(BusinessObjectFactory factory, CUSRESEDIMessage msg) => AsycudaUniversalReference.CustomsStatusAttributeHelper.IsCustomsCleared(factory, Core.Constants.CountryCodes.SouthAfrica, msg.EntryStatus);

		public static OrgHeader ZaOrgProxyForManifestMessaging(BusinessObjectFactory factory)
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.SouthAfrica)
			{
				return GlbCompany.CurrentCompany.OrgProxy;
			}
			else
			{
				OrgHeader result = null;
				var registry = ZACustomsRegistry.Instance.DefaultBranchForManifestSubmission.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (!string.IsNullOrEmpty(registry))
				{
					var branch = factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, registry));
					if (branch != null)
					{
						var cusCodes = ZACustomsRegistry.GetActiveAgentOrgCusCodes(branch);
						if (cusCodes.Any())
						{
							result = branch.OrgProxy;
						}
					}
				}
				return result;
			}
		}
	}
}
