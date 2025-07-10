using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public class BaseTradeNetPermitContainer : IPrintPermitContainers
	{
		public BaseTradeNetPermitContainer(TransportEquipment equipment)
		{
			this.equipment = Argument.NotNull(equipment, nameof(equipment));
			sequenceNumber = int.TryParse(equipment.SequenceNumeric, out sequenceNumber) ? sequenceNumber : -1;
		}

		readonly TransportEquipment equipment;
		readonly int sequenceNumber;

		#region IPrintPermitContainers Members

		public ZString ContainerSequenceNumber1
		{
			get { return sequenceNumber > 0 ? sequenceNumber.ToString("00").PadLeft(5, ' ') : ""; }
		}

		public ZString ContainerIdentifier1 => GetContainerIdentifier(equipment);

		public ZString ContainerSequenceNumber2 => ZString.Empty;

		public ZString ContainerIdentifier2 => ZString.Empty;

		#endregion

		ZString GetContainerIdentifier(TransportEquipment equipment) => string.Join(" ", new[]
		{
			equipment.EquipmentID.PadRight(13, ' '),
			GetSizeTypeCode(),
			equipment.EquipmentWeightMeasureNumericSpecified ? equipment.EquipmentWeightMeasureNumeric.ToString().PadLeft(3, '0') : string.Empty,
			equipment.TransportEquipmentSeal?.SealID
		}.Where(c => !string.IsNullOrWhiteSpace(c)));

		string GetSizeTypeCode()
		{
			ZString sizeTypeCode = equipment.SizeTypeCode;

			string result;

			switch (sizeTypeCode.Length)
			{
				case 5:
					{
						result = FormattableString.Invariant($"{sizeTypeCode.SubstringSafe(0, 3)} {sizeTypeCode.SubstringSafe(3, 2)}");
						break;
					}

				case 2:
					{
						result = sizeTypeCode.PadLeft(6, ' ');
						break;
					}

				case 3:
				default:
					{
						result = sizeTypeCode.PadRight(6, ' ');
						break;
					}
			}

			return result;
		}
	}
}
