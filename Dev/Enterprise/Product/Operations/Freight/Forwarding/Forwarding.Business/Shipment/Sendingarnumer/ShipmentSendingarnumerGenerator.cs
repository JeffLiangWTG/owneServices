using System.Globalization;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class INumberFountainExtensions
	{
		public static void Reset(this INumberFountainProxy fountain)
		{
			using (var transactionManager = Db.Connection.BeginTransactionWithManager()) // Don't shoot me - cut/paste code
			{
				fountain.SetNext(Db.Connection, nextValue: 1);
				transactionManager.CommitTransaction(); // Don't shoot me - cut/paste code
			}
		}

		public static string GetNextCarrierNumber(this INumberFountainProxy fountain)
		{
			using (var transactionManager = Db.Connection.BeginTransactionWithManager()) // Don't shoot me - cut/paste code
			{
				var result = fountain.GetNextFormatted(Db.Connection);
				transactionManager.CommitTransaction(); // Don't shoot me - cut/paste code
				return result;
			}
		}
	}

	public class ShipmentSendingarnumerGenerator
	{
		public ShipmentSendingarnumerGenerator(ForwardingConsol consol)
		{
			Consol = consol;
			Factory = Consol.Factory;
			Sendingarnumer = new Sendingarnumer(Consol.JK_CRN.ToUpper());
			SendingarnumerHelper = new ShipmentSendingarnumerHelper(Sendingarnumer);
		}

		public void GenerateSendingarnumers()
		{
			if (Consol != null && Consol.Shipments.Count > 0)
			{
				Factory.Save();
				int numberOfShipments = Consol.Shipments.Count;
				long nextLong = NumberFountain.PeekPreliminary(Factory);
				int excessLimit = 0;

				if (Consol.IsAirExpress)
				{
					int expressCounter = 0;
					foreach (ForwardingShipment shipment in Consol.Shipments)
					{
						if (SendingarnumerHelper.GetExpressCarrierNumber(shipment).IsEmpty)
						{
							expressCounter++;
						}
					}
					numberOfShipments = expressCounter;
					excessLimit = 99;
				}
				else
				{
					excessLimit = 999;
				}

				if (nextLong + numberOfShipments - 1 > excessLimit)
				{
					using (var transactionManager = Db.Connection.BeginTransactionWithManager()) // need to be in a second transaction to hit the NumberFountain.
					{
						NumberFountain.Reset();
						transactionManager.CommitTransaction(); // need to be in a second transaction to hit the NumberFountain.
					}
				}
				int expressOverValuedShipmentCarrierNumber = 4;

				foreach (ForwardingShipment shipment in Consol.Shipments)
				{
					if (Consol.IsAirExpress)
					{
						ZString expressCarrierNumber = SendingarnumerHelper.GetExpressCarrierNumber(shipment);

						if (!expressCarrierNumber.IsEmpty)
						{
							Sendingarnumer.CarrierNumber = Consol.SendingarnumerHelper.CarrierNumberPrefixCharacter + expressCarrierNumber;
						}
						else
						{
							Sendingarnumer.CarrierNumber = Consol.SendingarnumerHelper.CarrierNumberPrefixCharacter + expressOverValuedShipmentCarrierNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
							expressOverValuedShipmentCarrierNumber++;
						}
					}
					else
					{
						Sendingarnumer.CarrierNumber = Consol.SendingarnumerHelper.CarrierNumberPrefixCharacter + NumberFountain.GetNextCarrierNumber();
					}

					Sendingarnumer.CheckDigit = Sendingarnumer.GenerateCheckDigitFromCode();
					shipment.CustomsEntryNumber = Sendingarnumer.Code;
				}
			}
		}

		#region NumberFountain
		INumberFountainProxy NumberFountain
		{
			get
			{
				if (numberFountain == null)
				{
					if (Consol.IsAir)
					{
						if (Consol.IsImport())
						{
							numberFountain = SendingarnumerNumberFountains.Instance.SendingarnumerImportAir;
						}
						else if (Consol.IsExport())
						{
							numberFountain = SendingarnumerNumberFountains.Instance.SendingarnumerExportAir;
						}
					}
					else if (Consol.IsSea)
					{
						if (Consol.IsImport())
						{
							numberFountain = SendingarnumerNumberFountains.Instance.SendingarnumerImportSea;
						}
						else if (Consol.IsExport())
						{
							numberFountain = SendingarnumerNumberFountains.Instance.SendingarnumerExportSea;
						}
					}
				}
				return numberFountain;
			}
		}
		INumberFountainProxy numberFountain;
		#endregion

		#region Implementation
		readonly ForwardingConsol Consol;
		readonly BusinessObjectFactory Factory;
		readonly Sendingarnumer Sendingarnumer;
		readonly ShipmentSendingarnumerHelper SendingarnumerHelper;

		#endregion
	}
}
