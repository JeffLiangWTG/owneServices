using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.ZArchitecture.Schema;
using USIntegration = Enterprise.Integration.Customs.US;

namespace Enterprise.Customs.US.Business
{
	public class UpdateBillDispositionSource : USIntegration.IUpdateBillDispositionSource
	{
		ZGuid[] USIntegration.IUpdateBillDispositionSource.Update(Integration.ILogger logger, ZGuid[] pKs)
		{
			var factory = new BusinessObjectFactory();

			var query = new ZQuery(CusDecHouseBillSchema.PK, pKs);
			var bills = factory.Load<Bill>(query);

			foreach (Bill bill in bills)
			{
				var declaration = bill.Declaration;
				declaration.SuspendAddingWorkflow = true;

				if (declaration.ActiveEntryHeaders.SimplifiedEntry != null)
				{
					var sODipositionBlocks = declaration.ActiveEntryHeaders.SimplifiedEntry != null ? GetSODipositionBlocks(declaration.ActiveEntryHeaders.SimplifiedEntry.Messages) : null;
					var iSDipositionBlocks = GetISDipositionBlocks(declaration.Messages, bill.Messages);

					foreach (ASESSO50 disposition in sODipositionBlocks)
					{
						var dispositionDateTime = disposition.GetDispositionDateTime();
						var billDisposition = bill.DispositionCodes.OfType<DispositionData>().FirstOrDefault(x => x.US_Code == disposition.DispositionCode &&
												x.US_DispositionDate == dispositionDateTime);

						if (billDisposition != null && billDisposition.US_Source.IsEmpty &&
							//if IS and SO messages have the same dispotion code and time - we cannot decide and leave source empty
							!iSDipositionBlocks.Any(iSDisp => iSDisp.DispositionCode == disposition.DispositionCode && iSDisp.DispositionDateTime == dispositionDateTime))
						{
							billDisposition.US_Source = BillDispositionSourceList.Codes.SO;
						}
					}

					foreach (IDispositionDetailProvider disposition in iSDipositionBlocks)
					{
						var billDisposition = bill.DispositionCodes.OfType<DispositionData>().FirstOrDefault(x => x.US_Code == disposition.DispositionCode &&
												x.US_DispositionDate == disposition.DispositionDateTime);

						if (billDisposition != null && billDisposition.US_Source.IsEmpty &&
							//if IS and SO messages have the same dispotion code and time - we cannot decide and leave source empty
							!sODipositionBlocks.Any(sO => sO.DispositionCode == disposition.DispositionCode &&
													sO.GetDispositionDateTime() == disposition.DispositionDateTime))
						{
							billDisposition.US_Source = BillDispositionSourceList.Codes.IS;
						}
					}
				}
			}

			try
			{
				factory.Save();
				return pKs;
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

		List<ASESSO50> GetSODipositionBlocks(EDIMessageCollection messages)
		{
			var messageQuery = new ZQuery();
			messageQuery.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			messageQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USCustomsImport);
			messageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			messageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus);
			var soMessages = messages.Find(messageQuery);

			var result = new List<ASESSO50>();
			foreach (EDIMessage message in soMessages)
			{
				result.AddRange(message.MessageBlock.MessageBlocks.OfType<ASESSO50>());
			}
			return result;
		}

		List<IDispositionDetailProvider> GetISDipositionBlocks(EDIMessageCollection declarationMessages, EDIMessageCollection billMessages)
		{
			var messageQuery = new ZQuery();
			messageQuery.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			messageQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USCustomsImport);
			messageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			messageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse);
			var foundMessages = declarationMessages.Find(messageQuery);
			if (foundMessages.Length == 0)
			{
				foundMessages = billMessages.Find(messageQuery);
			}

			var result = new List<IDispositionDetailProvider>();
			foreach (EDIMessage message in foundMessages)
			{
				result.AddRange(message.MessageBlock.MessageBlocks.OfType<IDispositionDetailProvider>());
			}
			return result;
		}
	}
}
