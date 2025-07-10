using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.ZArchitecture.Schema;
using USIntegration = Enterprise.Integration.Customs.US;

namespace Enterprise.Customs.US.Business
{
	public class UpdateCertificateRequestDate : USIntegration.IUpdateCertificateRequestDate
	{
		ZGuid[] USIntegration.IUpdateCertificateRequestDate.Update(Integration.ILogger logger, ZGuid[] jobDeclarationPKs)
		{
			var factory = new BusinessObjectFactory();

			var query = new ZQuery(JobDeclarationSchema.PK, jobDeclarationPKs);
			var declarations = factory.Load<JobDeclaration>(query);

			#region Update declarations

			foreach (JobDeclaration declaration in declarations)
			{
				declaration.SuspendAddingWorkflow = true;
				try
				{
					if (!declaration.US_CertReqDate.IsValid)
					{
						var ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
						if (ensEntry != null)
						{
							if (declaration.IsACE)
							{
								UpdateCertReqDateForAENS10(ensEntry);
							}
							else
							{
								UpdateCertReqDateForENS30(ensEntry);
							}
						}
					}
				}
				catch (InvalidMessageFormatException messageFormatException)
				{
					if (logger != null)
					{
						logger.Log(Integration.LogType.Error, messageFormatException.Message);
					}
				}
			}
			#endregion

			try
			{
				factory.Save();
				return jobDeclarationPKs;
			}
			catch (ZSaveException saveException)
			{
				if (logger != null)
				{
					logger.Log(Integration.LogType.Error, saveException.Message);
				}
			}

			return null;
		}

		void UpdateCertReqDateForENS30(CusEntryHeader ensEntry)
		{
			var messageTypeQuery = new ZQuery();
			messageTypeQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			messageTypeQuery.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			messageTypeQuery.AddToFilter(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.EntrySummary);
			messageTypeQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USCustomsImport);
			var messagesACSENS = new TypedEnumerable<MQEDIMessage>(ensEntry.Messages.Find(messageTypeQuery));

			var ensBlocks = from ensMessage in messagesACSENS
							from ens30Block in ensMessage.MessageBlock.MessageBlocks.OfType<ENS30>()
							where ens30Block.ReleaseCertificationCode == 1
							orderby ensMessage.EM_SystemCreateTimeUtc
							select new { ensMessage.EM_SystemCreateTimeUtc, ens30Block.ReleaseCertificationCode };
			if (ensBlocks.Any())
			{
				ensEntry.Declaration.US_CertReqDate = ensBlocks.First().EM_SystemCreateTimeUtc;
			}
		}

		void UpdateCertReqDateForAENS10(CusEntryHeader ensEntry)
		{
			var messageTypeQuery = new ZQuery();
			messageTypeQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			messageTypeQuery.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			messageTypeQuery.AddToFilter(EDIMessageSchema.EM_MessageType, ACEApplicationIdentifierCodeList.Codes.EntrySummary);
			messageTypeQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USCustomsImport);
			var messagesACEENS = new TypedEnumerable<MQEDIMessage>(ensEntry.Messages.Find(messageTypeQuery));

			var aensBlocks = from aensMessage in messagesACEENS
							 from aens10Block in aensMessage.MessageBlock.MessageBlocks.OfType<AENS10>()
							 where aens10Block.CargoReleaseCertificationRequestIndicator == "Y"
							 orderby aensMessage.EM_SystemCreateTimeUtc
							 select new { aensMessage.EM_SystemCreateTimeUtc, aens10Block.CargoReleaseCertificationRequestIndicator };
			if (aensBlocks.Any())
			{
				ensEntry.Declaration.US_CertReqDate = aensBlocks.First().EM_SystemCreateTimeUtc;
			}
		}
	}
}
