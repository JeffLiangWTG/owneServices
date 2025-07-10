using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// When hooked, this will produce a file with rejected associations
	/// </summary>
	public class TariffAssociationRejectionProcessor
	{
		const string WrongAssociationRejectionCode = "15F";
		const string AdditionalTariffNumberRequired = "739";

		readonly List<string> rejectionCodesToBeIgnored = new List<string>(new string[]
				{
						WrongAssociationRejectionCode,
						AdditionalTariffNumberRequired,
						"15K",//Duty Computation Not performed - ancilliary rejection due to wrong association
						"27D",//*CENSUS* OR-HI VAL/QTY (1)TARIFF1 - not a rejection
						"4D8",//BANNED IMPORT FROM COUNTRY
						"899",//WARNING-ZERO QTY FOR VALUE EDIT
						"27B",//*CENSUS* QTY1/QTY2 (TARIFF 1)
						"524",//TRANSACTION DATA REJECTED
						"0LK",//Visa category number invalid
						"89E",//Fee Entered. but Not required
						"27M",//*CENSUS* QTY2/QTY1 (TARIFF 1)
						"27F",//*CENSUS* OR-HI VAL/QTY(2)TARIFF 1
						"27C",//*CENSUS* OR-LO VAL/QTY(1) TARIFF1
				});

		public void CollectionWrongAssociationRejections(MQEDIMessage[] messages, string fileName)
		{
			using (StreamWriter writer = new StreamWriter(fileName))
			{
				foreach (MQEDIMessage message in messages)
				{
					if (!message.IsTransmitMessage)
					{
						MQEDIMessage incomingMessage = message;

						List<ZInt> linesWithRejectedAssociations = GetLineNumbersWithRejectedAssociations(incomingMessage);

						if (linesWithRejectedAssociations.Count > 0)
						{
							MQEDIMessage outgoingMessage = incomingMessage.OriginalMessage;

							if (outgoingMessage != null)
							{
								bool associationRejected = false;
								string tariff1 = null, tariff2 = null, tariff3 = null, tariff4 = null;

								foreach (MessageBlock block in outgoingMessage.MessageBlock.MessageBlocks)
								{
									if (block.MandatoryCharacters == "40")
									{
										if (associationRejected)
										{
											writer.WriteLine(tariff1 + " " + tariff2 + (tariff3 != null ? " " + tariff3 : "") + (tariff4 != null ? " " + tariff4 : ""));
										}

										associationRejected = linesWithRejectedAssociations.Contains(((ENS40)block).LineItemNumber);

										tariff1 = null;
										tariff2 = null;
										tariff3 = null;
										tariff4 = null;
									}

									if (associationRejected)
									{
										if (block.MandatoryCharacters == "50")
										{
											tariff1 = ((ENS50)block).TariffNumber1;
										}
										else if (block.MandatoryCharacters == "70")
										{
											tariff2 = ((ENS70)block).TariffNumber2;
										}
										else if (block.MandatoryCharacters == "80")
										{
											tariff3 = ((ENS80)block).TariffNumber3;
										}
										else if (block.MandatoryCharacters == "81")
										{
											tariff4 = ((ENS81)block).AdditionalTariffNumber;
										}
									}
								}

								if (associationRejected)
								{
									writer.WriteLine(tariff1 + " " + tariff2 + (tariff3 != null ? " " + tariff3 : "") + (tariff4 != null ? " " + tariff4 : ""));
								}
							}
						}
					}
				}
			}
		}

		List<ZInt> GetLineNumbersWithRejectedAssociations(MQEDIMessage incomingMessage)
		{
			List<ZInt> result = new List<ZInt>();

			foreach (MessageBlock block in incomingMessage.MessageBlock.MessageBlocks)
			{
				ENSEXX rejectionBlock = block as ENSEXX;
				if (rejectionBlock != null && rejectionBlock.ErrorMessageIdentifier == WrongAssociationRejectionCode)
				{
					if (rejectionBlock.ErrorMessageIdentifier == WrongAssociationRejectionCode ||
						rejectionBlock.ErrorMessageIdentifier == AdditionalTariffNumberRequired)
					{
						if (!result.Contains(rejectionBlock.LineNumber))
						{
							result.Add(rejectionBlock.LineNumber);
						}
					}
				}
			}

			return result;
		}

		public void CollectRejectionsOtherThanWrongAssociations(MQEDIMessage[] messages, string fileName)
		{
			Predicate<MessageBlock> selectMessageSegments = (x => x is ENSEXX &&
				!rejectionCodesToBeIgnored.Contains(((ENSEXX)x).ErrorMessageIdentifier));

			List<string> errorCodes = new List<string>();

			using (StreamWriter writer = new StreamWriter(fileName))
			{
				foreach (MQEDIMessage message in messages)
				{
					if (!message.IsTransmitMessage)
					{
						MQEDIMessage incomingMessage = message;

						List<MessageBlock> rejectionSegments = incomingMessage.MessageBlock.MessageBlocks.FindAll(selectMessageSegments);

						if (rejectionSegments.Count > 0)
						{
							foreach (ENSEXX errorSegment in rejectionSegments)
							{
								if (!errorCodes.Contains(errorSegment.ErrorMessageIdentifier))
								{
									errorCodes.Add(errorSegment.ErrorMessageIdentifier);

									writer.WriteLine(errorSegment.ErrorMessageIdentifier + " " + message.EM_MessageNum + " " + errorSegment.LineNumber);
								}
							}
						}
					}
				}
			}
		}
	}
}
