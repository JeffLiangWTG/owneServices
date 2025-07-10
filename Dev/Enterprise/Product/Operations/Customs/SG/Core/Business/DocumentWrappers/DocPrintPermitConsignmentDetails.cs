using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.SG.V4.Business
{
	public class DocPrintPermitConsignmentDetails : DocumentWrapper
	{
		protected DocPrintPermitConsignmentDetails(PrintPermitConsignmentDetails consignmentDetails, BusinessObjectFactory factoryToWrap)
			: base(consignmentDetails, factoryToWrap)
		{
		}

		public static DocPrintPermitConsignmentDetails New(PrintPermitConsignmentDetails consignmentDetails, BusinessObjectFactory factoryToWrap)
		{
			return consignmentDetails != null ? new DocPrintPermitConsignmentDetails(consignmentDetails, factoryToWrap) : null;
		}

		protected IPrintPermitConsignment inConsignment
		{
			get { return ((PrintPermitConsignmentDetails)WrappedObject).Consignment; }
		}

		#region IPrintPermitConsignment

		public ZString SerialNb
		{
			get { return inConsignment.SerialNb; }
		}

		public ZString HSCode
		{
			get { return inConsignment.HSCode; }
		}

		public ZString BrandName
		{
			get { return inConsignment.BrandName; }
		}

		public ZString ManufacturerName
		{
			get { return inConsignment.ManufacturerName; }
		}

		public ZString HSQuantity
		{
			get { return inConsignment.HSQuantity; }
		}

		public ZString HSQuantityUnit
		{
			get { return inConsignment.HSQuantityUnit; }
		}

		public ZString Marking
		{
			get { return inConsignment.Marking; }
		}

		public ZString CityOfOrigin
		{
			get { return inConsignment.CityOfOrigin; }
		}

		public ZString Model
		{
			get { return inConsignment.Model; }
		}

		public ZString DutQuantity
		{
			get { return inConsignment.DutQuantity; }
		}

		public ZString DutQuantityUnit
		{
			get { return inConsignment.DutQuantityUnit; }
		}

		public ZString InwardMawbObl
		{
			get { return inConsignment.InwardMawbObl; }
		}

		public ZString InwardHawbHbl
		{
			get { return inConsignment.InwardHawbHbl; }
		}

		public ZString OutwardMawbObl
		{
			get { return inConsignment.OutwardMawbObl; }
		}

		public ZString OutwardHawbHbl
		{
			get { return inConsignment.OutwardHawbHbl; }
		}

		public ZString GoodsDescription
		{
			get { return inConsignment.GoodsDescription; }
		}

		public ZString UnitPrice
		{
			get { return inConsignment.UnitPrice.ToString(4); }
		}

		public ZString UnitPriceCurrency
		{
			get { return inConsignment.UnitPriceCurrency; }
		}

		public ZString CustomsDutyPayable
		{
			get { return inConsignment.CustomsDutyPayable.ToString(2); }
		}

		public ZString ExciseDutyPayable
		{
			get { return inConsignment.ExciseDutyPayable.ToString(2); }
		}

		public ZString OtherTaxPayable
		{
			get { return inConsignment.OtherTaxPayable.ToString(2); }
		}

		public ZString CurrentLotNb
		{
			get { return inConsignment.CurrentLotNb; }
		}

		public ZString PreviousLotNb
		{
			get { return inConsignment.PreviousLotNb; }
		}

		public ZString CifFobLspValue
		{
			get { return inConsignment.CifFobLspValue.ToString(2); }
		}

		public ZString LspAmount
		{
			get { return inConsignment.LspAmount.ToString(2); }
		}

		public ZString GstAmount
		{
			get { return inConsignment.GstAmount.ToString(2); }
		}

		public ZString CASCProductCode
		{
			get { return inConsignment.CASCProductCode; }
		}

		public ZString CASCProductQty
		{
			get { return inConsignment.CASCProductQty.ToString(4); }
		}

		public ZString CASCProductUQ
		{
			get { return inConsignment.CASCProductUQ; }
		}

		public ZString EngineNbChassisNb
		{
			get { return inConsignment.EngineNbChassisNb; }
		}

		public ZString OuterPackQty
		{
			get { return inConsignment.OuterPackQty > 0 ? inConsignment.OuterPackQty.ToString() : ""; }
		}

		public ZString OuterPackUq
		{
			get { return inConsignment.OuterPackUQ; }
		}

		public ZString InPackQty
		{
			get { return inConsignment.InPackQty > 0 ? inConsignment.InPackQty.ToString() : ""; }
		}

		public ZString InPackUQ
		{
			get { return inConsignment.InPackUQ; }
		}

		public ZString InnerPackQty
		{
			get { return inConsignment.InnerPackQty > 0 ? inConsignment.InnerPackQty.ToString() : ""; }
		}

		public ZString InnerPackUQ
		{
			get { return inConsignment.InnerPackUQ; }
		}

		public ZString InmostPackQty
		{
			get { return inConsignment.InmostPackQty > 0 ? inConsignment.InmostPackQty.ToString() : ""; }
		}

		public ZString InmostPackUQ
		{
			get { return inConsignment.InmostPackUQ; }
		}

		public ZString PackingAndGoodsDesc => packingAndGoodsDesc ?? (packingAndGoodsDesc = GetPackingAndGoodsDescCore());
		string packingAndGoodsDesc;

		ZString GetPackingAndGoodsDescCore()
		{
			var result = ZString.Empty;

			if (!OuterPackQty.IsEmpty)
			{
				result = OuterPackQty.PadLeft(8, ' ') + " " + OuterPackUq.PadLeft(3, ' ');
			}

			if (!InPackQty.IsEmpty)
			{
				result += "  " + InPackQty.PadLeft(8, ' ') + " " + InPackUQ;
			}

			if (!result.IsEmpty)
			{
				result = result.PadRight(50, ' ');
			}

			if (!InnerPackQty.IsEmpty)
			{
				result += InnerPackQty.PadLeft(8, ' ') + " " + InnerPackUQ.PadLeft(3, ' ');
			}

			if (!InmostPackQty.IsEmpty)
			{
				result += "  " + InmostPackQty.PadLeft(8, ' ') + " " + InmostPackUQ;
			}

			if (result.Length > 50)
			{
				result = result.PadRight(100, ' ');
			}
			else if (!result.IsEmpty)
			{
				result = result.PadRight(50, ' ');
			}

			result += SplitGoodsDescriptionElegantly();

			return result;
		}

		string SplitGoodsDescriptionElegantly()
		{
			var chopText = GoodsDescription.TrimEnd();

			chopText = chopText.Replace("\n", Space);
			chopText = chopText.Replace("\r", Space);

			var array = chopText.Split(' ');

			var sb = new ZStringBuilder();
			var lines = new List<string>();
			var index = 0;

			var maxCount = 13;
			var maxLength = 50;

			void AppendLineIfNeeded()
			{
				if (sb.Length > 0)
				{
					var line = sb.ToString().PadRight(maxLength, ' ');
					lines.Add(line);

					sb.Clear();
				}
			}

			foreach (var text in array)
			{
				var currentText = text;

				if (string.IsNullOrWhiteSpace(currentText))
				{
					continue;
				}

				if (currentText.Length > maxLength)
				{
					AppendLineIfNeeded();

					var innerLines = currentText.SplitIntoArray(maxLength, maxCount);
					lines.AddRange(innerLines.Take(innerLines.Length - 1));

					currentText = innerLines.LastOrDefault();
					index = 0;
				}

				var currentLength = index == 0
					? currentText.Length
					: sb.Length + 1 + currentText.Length;

				if (currentLength <= maxLength)
				{
					if (index > 0)
					{
						sb.Append(Space);
					}

					sb.Append(currentText);
					index++;
				}
				else
				{
					AppendLineIfNeeded();
					sb.Append(currentText);
				}

				if (lines.Count == maxCount)
				{
					break;
				}
			}

			if (sb.Length > 0 && lines.Count < maxCount)
			{
				lines.Add(sb.ToString().PadRight(maxLength, ' '));
			}

			return string.Join(string.Empty, lines.Take(maxCount));
		}

		const string Space = " ";

		public ZString LineValue1
		{
			get { return inConsignment.LineValue1; }
		}

		public ZString LineUnit1
		{
			get { return inConsignment.LineUnit1; }
		}

		public ZString LineValue2
		{
			get { return inConsignment.LineValue2; }
		}

		public ZString LineUnit2
		{
			get { return inConsignment.LineUnit2; }
		}

		public ZString LineValue3
		{
			get { return inConsignment.LineValue3; }
		}

		public ZString LineUnit3
		{
			get { return inConsignment.LineUnit3; }
		}

		public ZString LineValue4
		{
			get { return inConsignment.LineValue4; }
		}

		public ZString LineUnit4
		{
			get { return inConsignment.LineUnit4; }
		}

		public ZString LineValue5
		{
			get { return inConsignment.LineValue5; }
		}

		public ZString LineUnit5
		{
			get { return inConsignment.LineUnit5; }
		}

		public ZString LineValue6
		{
			get { return inConsignment.LineValue6; }
		}

		public ZString LineValue7
		{
			get { return inConsignment.LineValue7; }
		}

		public ZString LineValue8
		{
			get { return inConsignment.LineValue8; }
		}

		public DocCASCProductCodeCollection CASCProductCodes
		{
			get
			{
				if (productCodes == null)
				{
					productCodes = new DocCASCProductCodeCollection(Factory);

					foreach (var prdocutCode in inConsignment.CASCProductCodes ?? Array.Empty<ICASCProductCode>())
					{
						productCodes.Add(DocCASCProductCode.New(prdocutCode, Factory));
					}
				}

				return productCodes;
			}
		}
		DocCASCProductCodeCollection productCodes;

		public DocEngineOrChassisNumberCollection EngineOrChassisNumbers
		{
			get
			{
				if (engineOrChassisNumbers == null)
				{
					engineOrChassisNumbers = new DocEngineOrChassisNumberCollection(Factory);

					foreach (var engineOrChassisNumber in inConsignment.EngineOrChassisNumbers ?? Array.Empty<IEngineOrChassisNumber>())
					{
						engineOrChassisNumbers.Add(DocEngineOrChassisNumber.New(engineOrChassisNumber, Factory));
					}
				}

				return engineOrChassisNumbers;
			}
		}
		DocEngineOrChassisNumberCollection engineOrChassisNumbers;

		#endregion
	}

	public class DocPrintPermitConsignmentDetailsCollection : DocumentWrapperCollection
	{
		public DocPrintPermitConsignmentDetailsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocPrintPermitConsignmentDetails this[int index]
		{
			get { return (DocPrintPermitConsignmentDetails)base[index]; }
		}
	}
}
