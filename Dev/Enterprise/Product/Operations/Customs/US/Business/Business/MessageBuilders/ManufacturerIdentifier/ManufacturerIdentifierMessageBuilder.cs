using CargoWise.Common.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class ManufacturerIdentifierMessageBuilder
	{
		public static MQEDIMessage Generate(IManufacturerAdd messageData, string updateActionCode)
		{
			MQEDIMessage message = null;
			if (messageData != null)
			{
				ABIInputBlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
				block.B.ApplicationIdentifier = ACEApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressAdd;
				GenerateBlocks(block, messageData, updateActionCode);
				message = block.CreateMessage<MQEDIMessage>(messageData.Factory);
			}
			return message;
		}

		static void GenerateBlocks(ABIInputBlockControlGenerator block, IManufacturerAdd messageData, string updateActionCode)
		{
			if (messageData.AddressPK.IsValid)
			{
				AMFDollarA amfa = new AMFDollarA();
				amfa.UserData = messageData.AddressPK.ToString();
				block.AddMessageBlock(amfa);
			}

			AMFDollar1 amf1 = new AMFDollar1();
			amf1.UpdateActionCode = updateActionCode;
			if (updateActionCode == ManufacturerIdentifierManager.Constants.UpdateActionCode.Add)
			{
				amf1.ISOCountryCode = messageData.Country;
				amf1.FirmName = messageData.FirmName.Left(70);
			}
			amf1.UpdateSequenceNumber = 1;
			block.AddMessageBlock(amf1);

			ZString remainingFirmName = messageData.FirmName.SubstringSafe(70);

			if (!remainingFirmName.IsEmpty || !messageData.Street.IsEmpty)
			{
				AMFDollar2 amf2 = new AMFDollar2();
				amf2.FirmName = remainingFirmName;
				amf2.Street = messageData.Street.Left(43);
				block.AddMessageBlock(amf2);
			}

			AMFDollar3 amf3 = new AMFDollar3();
			amf3.Street = messageData.Street.SubstringSafe(43);
			amf3.City = messageData.City.Left(23);
			block.AddMessageBlock(amf3);

			AMFDollar4 amf4 = new AMFDollar4();
			amf4.ZIPOrPostalCode = messageData.Zip;
			amf4.ManufacturerIDCode = messageData.MID;
			block.AddMessageBlock(amf4);
		}
	}
}
