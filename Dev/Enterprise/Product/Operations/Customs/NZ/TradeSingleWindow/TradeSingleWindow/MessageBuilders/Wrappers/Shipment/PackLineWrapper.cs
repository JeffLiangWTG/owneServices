using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public class PackLineWrapper : IICRConsignmentItem
	{
		public PackLineWrapper(ForwardingPackLine packLine)
		{
			this.packLine = Argument.NotNull(packLine, "PackLine cannot be null");
		}

		readonly ForwardingPackLine packLine;

		#region IICRConsignmentItem

		public ZShort SequenceNumber => 1;

		public ZBool IsEmptyContainer
		{
			get { return false; }
		}

		public ZString GoodsDescription
		{
			get { return packLine.JL_DetailedDescription; }
		}

		public ZString IdentityNumber
		{
			get { return ""; }  //TODO: VV = Vehicle Identification Number, CN = Chassis Number, BN = Serial Number, CX = Identification Tag, CY = Identification Tattoo, MC = Microchip
		}

		public ZDecimal Value
		{
			get { return packLine.JL_LinePrice; }
		}

		public ZString Currency
		{
			get { return ""; }  //TODO: ItemCurrency
		}

		public ZString IdentityType
		{
			get { return ""; }  //TODO: VV = Vehicle Identification Number, CN = Chassis Number, BN = Serial Number, CX = Identification Tag, CY = Identification Tattoo, MC = Microchip
		}

		public IEnumerable<IClassification> Classifications
		{
			get
			{
				if (packLine.UNDGs.Count > 0)
				{
					yield return new ICRClassification(packLine.UNDGs[0].UNDGSubstance?.DG_Code ?? ZString.Empty, ClassificationTypeList.Codes.SSO);
				}

				if (!packLine.JL_HarmonisedCode.IsEmpty)
				{
					yield return new ICRClassification(packLine.JL_HarmonisedCode, ClassificationTypeList.Codes.HS);
				}
			}
		}

		ZBool IICRConsignmentItem.SendFlashpointTemp => packLine.UNDGs.Count > 0;

		public ZDecimal FlashpointTempInCelsius
		{
			get
			{
				var result = 0m;
				if (packLine.UNDGs.Count > 0)
				{
					result = packLine.UNDGs[0].DI_DGFlashPoint;
				}

				return result;
			}
		}

		public ITemperatureRequirements Temperatures    //TODO: Storage, Min & Max temps
		{
			get
			{
				ITemperatureRequirements result = null;
				//if (entryLine.RandomLine.JI_TemperatureDetailsToBeSent)
				//{
				//    result = new TemperatureRequirements(entryLine.RandomLine.JI_StorageTemp, entryLine.RandomLine.JI_MinTemp, entryLine.RandomLine.JI_MaxTemp);
				//}

				return result;
			}
		}

		public ZDecimal GrossWeightInKg
		{
			get
			{
				ZDecimal result = 0m;
				try
				{
					result = Core.Constants.Weight.Convert(packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ, DeclaredWeightUQ);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{ }
				return result;
			}
		}

		ZString DeclaredWeightUQ
		{
			get { return Core.Constants.Weight.Kilograms; }
		}

		public ZString GoodsOriginCountry
		{
			get { return packLine.JL_RN_NKOrigin; }
		}

		public ZInt PackageQty
		{
			get { return packLine.JL_PackageCount; }
		}

		public ZString PackageType
		{
			get { return packLine.JL_F3_NKPackType; }
		}

		public ZString ContainerNumber
		{
			get { return packLine.JL_Calc_ContainerNumber; }
		}

		public ZString MPIApprovedSystemNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
