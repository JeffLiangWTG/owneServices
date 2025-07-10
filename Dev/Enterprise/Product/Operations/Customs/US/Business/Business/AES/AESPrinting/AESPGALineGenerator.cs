using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input;

namespace Enterprise.Customs.US.AES
{
	public class AESPGALineGenerator
	{
		public static IEnumerable<ZString> GeneratePGALines(IEnumerable<MessageBlock> blocks)
		{
			var aesGenerator = new AESPGALineGenerator();
			var nmfsStringLine = ZString.Empty;

			foreach (AESCommShipPGAXP block in blocks)
			{
				var blockID = block.PGAID;
				switch (blockID)
				{
					case USConstants.AES.AESAMSID:
						yield return aesGenerator.GenerateAESAMSLine(block);
						break;
					case USConstants.AES.AESEPAID:
						yield return aesGenerator.GenerateAESEPALine(block);
						break;
					case USConstants.AES.NMFSNM7ID:
						if (!nmfsStringLine.IsEmpty)
						{
							yield return "PGA Code: NMFS\r\n" + nmfsStringLine.TrimEnd() + "\r\n";
							nmfsStringLine = ZString.Empty;
						}
						nmfsStringLine = aesGenerator.GenerateAESNM7Line(block);
						break;
					case USConstants.AES.NMFSNM8ID:
						nmfsStringLine += aesGenerator.GenerateAESNM8Line(block);
						break;
					case USConstants.AES.NMFSNM9ID:
						nmfsStringLine += aesGenerator.GenerateAESNM9Line(block);
						break;
					case USConstants.AES.AESATFID:
						yield return aesGenerator.GenerateAESATFLine(block);
						break;
					case USConstants.AES.FWSFW7ID:
						yield return aesGenerator.GenerateAESFW7Line(block);
						break;
					case USConstants.AES.FWSFW8ID:
						break;
					case USConstants.AES.AESDEAID:
						yield return aesGenerator.GenerateAESDEALine(block);
						break;
					case USConstants.AES.AESTTBID:
						yield return aesGenerator.GenerateAESTTBLine(block);
						break;
					default:
						break;
				}
			}

			if (!nmfsStringLine.IsEmpty)
			{
				yield return "PGA Code: NMFS\r\n" + nmfsStringLine.TrimEnd() + "\r\n";
			}
		}

		ZString GenerateAESEPALine(AESCommShipPGAXP block)
		{
			var stringBuilder = new ZStringBuilder();
			var tempString = ZString.Empty;

			if (!block.PGAData.IsEmpty)
			{
				var epaPGABlock = new EPA_EP1PGADetails();
				epaPGABlock.Deserialise(block.PGAData.PadRight(80));

				if (!epaPGABlock.EPAConsentNumber.IsEmpty)
				{
					stringBuilder.Append("EPA Consent Number: " + epaPGABlock.EPAConsentNumber + "  ");
				}

				if (!epaPGABlock.EPAHazardousWasteManifestTrackingNumber.IsEmpty)
				{
					stringBuilder.Append("Hazardous Waste Manifest Tracking No.: " + epaPGABlock.EPAHazardousWasteManifestTrackingNumber + "  ");
				}

				if (!epaPGABlock.EPANetQuantity.IsEmpty)
				{
					stringBuilder.Append("EPA Net Quantity: " + epaPGABlock.EPANetQuantity + " ");
					if (!epaPGABlock.EPAUnitOfQuantity.IsEmpty)
					{
						stringBuilder.Append(epaPGABlock.EPAUnitOfQuantity + "  ");
					}
				}

				if (!stringBuilder.IsEmpty)
				{
					tempString = "PGA Code: EPA\r\n";
					tempString += stringBuilder.ToString().TrimEnd();
					tempString += "\r\n";
				}
			}
			return tempString;
		}

		ZString GenerateAESAMSLine(AESCommShipPGAXP block)
		{
			var stringBuilder = new ZStringBuilder();
			var tempString = ZString.Empty;

			if (!block.PGAData.IsEmpty)
			{
				var epaPGABlock = new AMS_AM1PGADetails();
				epaPGABlock.Deserialise(block.PGAData.PadRight(80));

				if (!epaPGABlock.AMSExportCertificateNumber.IsEmpty)
				{
					stringBuilder.Append("Export Certificate Number: " + epaPGABlock.AMSExportCertificateNumber + "  ");
				}

				if (!stringBuilder.IsEmpty)
				{
					tempString = "PGA Code: AMS\r\n";
					tempString += stringBuilder.ToString().TrimEnd();
					tempString += "\r\n";
				}
			}
			return tempString;
		}

		ZString GenerateAESATFLine(AESCommShipPGAXP block)
		{
			var stringBuilder = new ZStringBuilder();
			var tempString = ZString.Empty;

			if (!block.PGAData.IsEmpty)
			{
				var epaPGABlock = new ATF_AT6PGADetails();
				epaPGABlock.Deserialise(block.PGAData.PadRight(80));

				if (!epaPGABlock.PermitQuantity.IsEmpty)
				{
					stringBuilder.Append("Quantity: " + epaPGABlock.PermitQuantity + "  ");
				}

				if (!epaPGABlock.CategoryCode.IsEmpty)
				{
					stringBuilder.Append("Category Code: " + epaPGABlock.CategoryCode + "  ");
				}

				if (!epaPGABlock.FFLNumber.IsEmpty)
				{
					stringBuilder.Append("FFL Number: " + epaPGABlock.FFLNumber + "  ");
				}

				if (!epaPGABlock.FFLExemptionCode.IsEmpty)
				{
					stringBuilder.Append("FFL Exemption Code: " + epaPGABlock.FFLExemptionCode + "  ");
				}

				if (!epaPGABlock.PermitNumber.IsEmpty)
				{
					stringBuilder.Append("Permit Number: " + epaPGABlock.PermitNumber + "  ");
				}

				if (!epaPGABlock.PermitExemptionCode.IsEmpty)
				{
					stringBuilder.Append("Permit Exemption Code: " + epaPGABlock.PermitExemptionCode + "  ");
				}

				if (!stringBuilder.IsEmpty)
				{
					tempString = "PGA Code: ATF\r\n";
					tempString += stringBuilder.ToString().TrimEnd();
					tempString += "\r\n";
				}
			}
			return tempString;
		}

		ZString GenerateAESFW7Line(AESCommShipPGAXP block)
		{
			var stringBuilder = new ZStringBuilder();
			var tempString = ZString.Empty;

			if (!block.PGAData.IsEmpty)
			{
				var epaPGABlock = new FWS_FW7PGADetails();
				epaPGABlock.Deserialise(block.PGAData.PadRight(80));

				if (!epaPGABlock.EDEcsConfirmatioNnumber.IsEmpty)
				{
					stringBuilder.Append("eDecs Confirmation: " + epaPGABlock.EDEcsConfirmatioNnumber + "  ");
				}

				if (!epaPGABlock.TaxonomicSerial.IsEmpty)
				{
					stringBuilder.Append("Taxonomic Serial Number: " + epaPGABlock.TaxonomicSerial + "  ");
				}

				if (!epaPGABlock.FWSPurposeCode.IsEmpty)
				{
					stringBuilder.Append("Purpose Code: " + epaPGABlock.FWSPurposeCode + "  ");
				}

				if (!epaPGABlock.FWSDescriptionCode.IsEmpty)
				{
					stringBuilder.Append("Wildlife Description Code: " + epaPGABlock.FWSDescriptionCode + "  ");
				}

				if (!epaPGABlock.SourceCode.IsEmpty)
				{
					stringBuilder.Append("Wildlife Source: " + epaPGABlock.SourceCode + "  ");
				}

				if (!epaPGABlock.FWSWildlifeCategoryCode.IsEmpty)
				{
					stringBuilder.Append("Wildlife Category Code: " + epaPGABlock.FWSWildlifeCategoryCode + "  ");
				}

				if (!epaPGABlock.ExemptionCertificationCode.IsEmpty)
				{
					stringBuilder.Append("Certification Code: " + epaPGABlock.ExemptionCertificationCode + "  ");
				}

				if (!epaPGABlock.SpeciesCountryOfOrigin.IsEmpty)
				{
					stringBuilder.Append("Species Origin: " + epaPGABlock.SpeciesCountryOfOrigin + "  ");
				}

				if (!epaPGABlock.StateOfSpeciesOrigin.IsEmpty)
				{
					stringBuilder.Append("State: " + epaPGABlock.StateOfSpeciesOrigin + "  ");
				}

				if (!stringBuilder.IsEmpty)
				{
					tempString = "PGA Code: FWS\r\n";
					tempString += stringBuilder.ToString().TrimEnd();
					tempString += "\r\n";
				}
			}
			return tempString;
		}

		ZString GenerateAESDEALine(AESCommShipPGAXP block)
		{
			var stringBuilder = new ZStringBuilder();
			var tempString = ZString.Empty;

			if (!block.PGAData.IsEmpty)
			{
				var epaPGABlock = new DEAPGADetails();
				epaPGABlock.Deserialise(block.PGAData.PadRight(80));

				if (!epaPGABlock.DrugCode.IsEmpty)
				{
					stringBuilder.Append("Drug Code: " + epaPGABlock.DrugCode + "  ");
				}

				if (!epaPGABlock.Quantity.IsEmpty)
				{
					stringBuilder.Append("Weight: " + epaPGABlock.Quantity + "  ");
				}

				if (!epaPGABlock.UnitofMeasure.IsEmpty)
				{
					stringBuilder.Append("Weight UQ: " + epaPGABlock.UnitofMeasure + "  ");
				}

				if (!epaPGABlock.PermitTransactionID.IsEmpty)
				{
					stringBuilder.Append("Permit Number: " + epaPGABlock.PermitTransactionID + "  ");
				}

				if (!epaPGABlock.Entity.IsEmpty)
				{
					stringBuilder.Append("Registration Number: " + epaPGABlock.Entity + "  ");
				}

				if (!stringBuilder.IsEmpty)
				{
					tempString = "PGA Code: DEA\r\n";
					tempString += stringBuilder.ToString().TrimEnd();
					tempString += "\r\n";
				}
			}
			return tempString;
		}

		ZString GenerateAESTTBLine(AESCommShipPGAXP block)
		{
			var stringBuilder = new ZStringBuilder();
			var tempString = ZString.Empty;

			if (!block.PGAData.IsEmpty)
			{
				var epaPGABlock = new TTBPGADetails();
				epaPGABlock.Deserialise(block.PGAData.PadRight(80));

				if (!epaPGABlock.TTBPermitRegistryNumber.IsEmpty)
				{
					stringBuilder.Append("Number for IRC: " + epaPGABlock.TTBPermitRegistryNumber + "  ");
				}

				if (!epaPGABlock.Date.IsEmpty)
				{
					stringBuilder.Append("Departure Date: " + epaPGABlock.Date + "  ");
				}

				if (!epaPGABlock.Serial.IsEmpty)
				{
					stringBuilder.Append("Serial Number: " + epaPGABlock.Serial + "  ");
				}

				if (!stringBuilder.IsEmpty)
				{
					tempString = "PGA Code: TTB\r\n";
					tempString += stringBuilder.ToString().TrimEnd();
					tempString += "\r\n";
				}
			}
			return tempString;
		}

		ZString GenerateAESNM7Line(AESCommShipPGAXP block)
		{
			var stringBuilder = new ZStringBuilder();
			var epaPGABlock = new NMFS_NM7PGADetails();
			if (block != null)
			{
				epaPGABlock.Deserialise(block.PGAData.PadRight(80));

				if (!block.PGAData.IsEmpty)
				{
					if (!epaPGABlock.Description.IsEmpty)
					{
						stringBuilder.Append("Goods Description: " + epaPGABlock.Description + "  ");
					}

					if (!epaPGABlock.GovtAgencyProgramCode.IsEmpty)
					{
						stringBuilder.Append("Program Type: " + epaPGABlock.GovtAgencyProgramCode + "  ");
					}
				}
			}

			return stringBuilder.ToString();
		}

		ZString GenerateAESNM8Line(AESCommShipPGAXP block)
		{
			var stringBuilder = new ZStringBuilder();

			if (block != null)
			{
				if (!block.PGAData.IsEmpty)
				{
					var epaPGABlock = new NMFS_NM8PGADetails();
					epaPGABlock.Deserialise(block.PGAData.PadRight(80));

					if (!epaPGABlock.ProcessingTypeCode.IsEmpty)
					{
						stringBuilder.Append("Category Code: " + epaPGABlock.ProcessingTypeCode + "  ");
					}

					if (!epaPGABlock.DocumentIdentifier.IsEmpty)
					{
						stringBuilder.Append("Document Type: " + epaPGABlock.DocumentIdentifier + "  ");
					}

					if (!epaPGABlock.DocumentNumber.IsEmpty)
					{
						stringBuilder.Append("eBCD Number: " + epaPGABlock.DocumentNumber + "  ");
					}

					if (!epaPGABlock.DocumentNumber.IsEmpty)
					{
						stringBuilder.Append("Harvested Country: " + epaPGABlock.SourceCountry + "  ");
					}

					if (!epaPGABlock.CommodityHarvestingVesselCountryOfRegistry.IsEmpty)
					{
						stringBuilder.Append("Vessel Country: " + epaPGABlock.CommodityHarvestingVesselCountryOfRegistry + "  ");
					}
				}
			}

			return stringBuilder.ToString();
		}

		ZString GenerateAESNM9Line(AESCommShipPGAXP block)
		{
			var stringBuilder = new ZStringBuilder();

			if (block != null)
			{
				if (!block.PGAData.IsEmpty)
				{
					var epaPGABlock = new NMFS_NM9PGADetails();
					epaPGABlock.Deserialise(block.PGAData.PadRight(80));

					if (!epaPGABlock.PermitNumber.IsEmpty)
					{
						stringBuilder.Append("IFTP Permit Number: " + epaPGABlock.PermitNumber + "  ");
					}

					if (!epaPGABlock.LPCOQty.IsEmpty)
					{
						stringBuilder.Append("Total Weight: " + epaPGABlock.LPCOQty);
						if (!epaPGABlock.LPCOUnitOfMeasure.IsEmpty)
						{
							stringBuilder.Append(epaPGABlock.LPCOUnitOfMeasure);
						}
						stringBuilder.Append("  ");
					}

					if (!epaPGABlock.CatchDocument.IsEmpty)
					{
						stringBuilder.Append("Catch Document Number: " + epaPGABlock.CatchDocument + "  ");
					}

					if (!epaPGABlock.ReExportNumber.IsEmpty)
					{
						stringBuilder.Append("Re-Export Approval No.: " + epaPGABlock.ReExportNumber + "  ");
					}
				}
			}

			return stringBuilder.ToString();
		}
	}
}
