using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business
{
	public class ACEBIRDDEADataProcessor : ACEBIRDCommonPGADataProcessor
	{
		public ACEBIRDDEADataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_DEADisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_DEAIndInfo; }
		}

		public override ZString PGAName
		{
			get { return "DEA"; }
		}

		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			var deaHeader = invoiceLine.DEAHeaders.AddNew();

			DEAConstituent currentConstituent = null;

			foreach (var block in pgaBlocks)
			{
				var pg02Block = block as AEPAPG02;
				if (pg02Block != null)
				{
					currentConstituent = null;

					if (pg02Block.ItemType == PG02ItemTypeList.Codes.Component && pg02Block.ProductCodeQualifier == ProductCodeQualifiersList.Codes.ControlledSubstancesActNumber)
					{
						currentConstituent = deaHeader.Constituents.AddNew();
						currentConstituent.US_ProductCode = pg02Block.ProductCodeNumber;
					}
				}
				else
				{
					var pg04Block = block as AEPAPG04;
					if (pg04Block != null)
					{
						if (currentConstituent != null)
						{
							currentConstituent.US_Weight = pg04Block.QuantityOfConstituentElement;
							currentConstituent.US_WeightUQ = pg04Block.UnitOfMeasureConstituentElement;
						}

						currentConstituent = null;
					}
					else
					{
						var pg06Block = block as AEPAPG06;
						if (pg06Block != null)
						{
							if (pg06Block.SourceTypeCode == SourceTypeCodesList.Codes.CountryOfShipment)
							{
								deaHeader.US_CountryOfShipment = pg06Block.CountryCode;
							}
						}
						else
						{
							var pg14Block = block as AEPAPG14;
							if (pg14Block != null)
							{
								if (pg14Block.LPCOTransactionType == LPCOTransactionTypeList.Codes.SingleUse)
								{
									deaHeader.US_PermitNumber = pg14Block.LPCONumberorName;
								}
							}
							else
							{
								var pg19Block = block as AEPAPG19;
								if (pg19Block != null)
								{
									if (pg19Block.EntityRoleCode == EntityRoleCodeList.Codes.LPCOAuthorizedParty && pg19Block.EntityIdentificationCode == EntityIdentificationCodesList.Codes.DEARegistrationNumber)
									{
										deaHeader.US_RegistrationNumber = pg19Block.EntityNumber;
									}
								}
								else
								{
									var pg22Block = block as AEPAPG22;
									if (pg22Block != null)
									{
										deaHeader.US_FormID = DEAFormTypeList.GetFormTypeFromDocumentIdentifier(pg22Block.DocumentIdentifier);
									}
									else
									{
										var pg30Block = block as AEPAPG30;
										if (pg30Block != null)
										{
											if (pg30Block.InspectionLaboratoryTestingStatus == InspectionStatusList.Codes.BTAAnticipatedArrivalInformation)
											{
												var dateTime = DateTimeParser.GetDateTimeFromZDateAndStringTime(pg30Block.AnticipatedArrivalDate, pg30Block.ArrivalTime);
												invoiceLine.Factory.GetCachedValue<BIRDUpdateHeaderHelperTool>().UpdateOrWarn(invoiceLine.Declaration, JobDeclaration.Schema.US_FDAADTA, dateTime, dateTime.ToLongTimeString(), "PGA Arrival Date/Time", notifications);
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
