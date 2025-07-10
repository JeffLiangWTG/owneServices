using System.Collections.Generic;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input;

namespace Enterprise.Customs.US.Business
{
	public class ExportPGABlocksCreator
	{
		public static IEnumerable<MessageBlock> BuildPGABlocks(IAESTIRCommodityLineItem line)
		{
			var result = new List<MessageBlock>();
			var creator = new ExportPGABlocksCreator();
			result.AddRange(creator.GenerateAMSBlocks(line));
			result.AddRange(creator.GenerateEPABlocks(line));
			result.AddRange(creator.GenerateNMFSBlocks(line));
			result.AddRange(creator.GenerateATFBlocks(line));
			result.AddRange(creator.GenerateDEABlocks(line));
			result.AddRange(creator.GenerateFWSBlocks(line));
			result.AddRange(creator.GenerateTTBBlocks(line));
			return result;
		}

		#region AMS Blocks

		IEnumerable<MessageBlock> GenerateAMSBlocks(IAESTIRCommodityLineItem line)
		{
			if (OGAIndicatorList.IsToBeDeclared(line.AMSIndicator) && line.ExportAMS != null)
			{
				yield return new AESCommShipPGAXP()
				{
					PGAID = USConstants.AES.AESAMSID,
					PGAData = new AMS_AM1PGADetails()
					{
						AMSExportCertificateNumber = line.ExportAMS.ExportCertificateNo
					}.Serialise().Substring(0, 73)
				};
			}
		}

		#endregion

		#region EPA Blocks

		IEnumerable<MessageBlock> GenerateEPABlocks(IAESTIRCommodityLineItem line)
		{
			if (OGAIndicatorList.IsToBeDeclared(line.EPAIndicator) && line.ExportEPA != null)
			{
				yield return new AESCommShipPGAXP()
				{
					PGAID = USConstants.AES.AESEPAID,
					PGAData = new EPA_EP1PGADetails()
					{
						EPALicenceRequiredIndicator = "Y",
						EPAConsentNumber = line.ExportEPA.EPAConsentNumber,
						EPAHazardousWasteManifestTrackingNumber = line.ExportEPA.HazWasteManifestTrackingNumber,
						EPANetQuantity = line.ExportEPA.EPANetQuantity,
						EPAUnitOfQuantity = line.ExportEPA.EPANetQuantityUQ
					}.Serialise().Substring(0, 73)
				};
			}
		}

		#endregion

		#region NMFS Blocks

		IEnumerable<MessageBlock> GenerateNMFSBlocks(IAESTIRCommodityLineItem line)
		{
			if (OGAIndicatorList.IsToBeDeclared(line.NMFSIndicator) && line.ExportNMFSLines != null)
			{
				foreach (IAESNMFS nmfsLine in line.ExportNMFSLines)
				{
					yield return new AESCommShipPGAXP()
					{
						PGAID = USConstants.AES.NMFSNM7ID,
						PGAData = new NMFS_NM7PGADetails
						{
							Description = nmfsLine.Description,
							GovtAgencyProgramCode = nmfsLine.ProgramCode
						}.Serialise().Substring(0, 73)
					};

					yield return new AESCommShipPGAXP()
					{
						PGAID = USConstants.AES.NMFSNM8ID,
						PGAData = new NMFS_NM8PGADetails
						{
							ProcessingTypeCode = nmfsLine.ProcessingTypeCode,
							DocumentIdentifier = nmfsLine.DocumentType,
							DocumentNumber = nmfsLine.DocumentNumber,
							DocumentImagesIndicator = nmfsLine.DocumentImageSent,
							SourceCountry = nmfsLine.SourceCountry,
							CommodityHarvestingVesselCountryOfRegistry = nmfsLine.HarvestingCountry,
							GeographicLocation = nmfsLine.GeographicLocation
						}.Serialise().Substring(0, 73)
					};

					yield return new AESCommShipPGAXP()
					{
						PGAID = USConstants.AES.NMFSNM9ID,
						PGAData = new NMFS_NM9PGADetails
						{
							PermitNumber = nmfsLine.PermitNumber,
							LPCOQty = nmfsLine.Quantity,
							LPCOUnitOfMeasure = nmfsLine.UQ,
							CatchDocument = nmfsLine.CatchDocumentNumber,
							ReExportNumber = nmfsLine.ReExportNumber
						}.Serialise().Substring(0, 73)
					};
				}
			}
		}

		#endregion

		#region ATF Blocks

		IEnumerable<MessageBlock> GenerateATFBlocks(IAESTIRCommodityLineItem line)
		{
			if (OGAIndicatorList.IsToBeDeclared(line.ATFIndicator) && line.ExportATF != null)
			{
				yield return new AESCommShipPGAXP()
				{
					PGAID = USConstants.AES.AESATFID,
					PGAData = new ATF_AT6PGADetails()
					{
						FFLNumber = line.ExportATF.FFLNumber,
						FFLExemptionCode = line.ExportATF.FFLExemptionCode,
						PermitNumber = line.ExportATF.PermitNumber,
						PermitExemptionCode = line.ExportATF.PermitExemptionCode,
						PermitQuantity = line.ExportATF.Quantity,
						CategoryCode = line.ExportATF.CategoryCode,
						Description = line.ExportATF.Description
					}.Serialise().Substring(0, 73)
				};
			}
		}

		#endregion

		#region DEA Blocks

		IEnumerable<MessageBlock> GenerateDEABlocks(IAESTIRCommodityLineItem line)
		{
			if (OGAIndicatorList.IsToBeDeclared(line.DEAIndicator) && line.ExportDEALines != null)
			{
				foreach (var deaLine in line.ExportDEALines)
				{
					yield return new AESCommShipPGAXP()
					{
						PGAID = USConstants.AES.AESDEAID,
						PGAData = new DEAPGADetails()
						{
							DrugCode = deaLine.DrugCode,
							Quantity = deaLine.Quantity,
							UnitofMeasure = deaLine.UnitOfMeasure,
							TransactionType = deaLine.TransactionType,
							PermitTransactionID = deaLine.PermitNumber,
							Entity = deaLine.RegistrationNumber
						}.Serialise().Substring(0, 73)
					};
				}
			}
		}

		#endregion

		#region FWS Blocks

		IEnumerable<MessageBlock> GenerateFWSBlocks(IAESTIRCommodityLineItem line)
		{
			if (OGAIndicatorList.IsToBeDeclared(line.FWSIndicator) && line.ExportFWS != null)
			{
				yield return new AESCommShipPGAXP()
				{
					PGAID = USConstants.AES.FWSFW7ID,
					PGAData = new FWS_FW7PGADetails()
					{
						EDEcsConfirmatioNnumber = line.ExportFWS.EDecsConfirmation,
						TaxonomicSerial = line.ExportFWS.TaxonomicSerialNumber,
						FWSPurposeCode = line.ExportFWS.PurposeCode,
						FWSDescriptionCode = line.ExportFWS.DescriptionCode,
						SpeciesCountryOfOrigin = line.ExportFWS.SpeciesOrigin,
						SourceCode = line.ExportFWS.SourceCode,
						ExemptionCertificationCode = line.ExportFWS.ExemptionCertification,
						FWSWildlifeCategoryCode = line.ExportFWS.WildlifeCategoryCode,
						StateOfSpeciesOrigin = line.ExportFWS.StateCode,
					}.Serialise().Substring(0, 73)
				};

				yield return new AESCommShipPGAXP()
				{
					PGAID = USConstants.AES.FWSFW8ID,
					PGAData = new FWS_FW8PGADetails()
					{
						CommercialDescription = line.ExportFWS.CommercialDescription
					}.Serialise().Substring(0, 73)
				};
			}
		}

		#endregion

		#region TTB Blocks

		IEnumerable<MessageBlock> GenerateTTBBlocks(IAESTIRCommodityLineItem line)
		{
			if (OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.TTBIndicator) && line.ExportTTBLines != null)
			{
				foreach (var exportTTB in line.ExportTTBLines)
				{
					yield return new AESCommShipPGAXP()
					{
						PGAID = USConstants.AES.AESTTBID,
						PGAData = new TTBPGADetails()
						{
							TTBPermitRegistryNumber = exportTTB.IRCNumber,
							Date = exportTTB.Date,
							Serial = exportTTB.SerialNumber,
							TTBDisclaimer = exportTTB.Disclaimer
						}.Serialise().Substring(0, 73)
					};
				}
			}
		}

		#endregion
	}
}
