using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business
{
	public class ACEBIRDLaceyDataProcessor : ACEBIRDCommonPGADataProcessor
	{
		public ACEBIRDLaceyDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_LaceyDisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_LaceyIndicatorInfo; }
		}

		public override ZString PGAName
		{
			get { return "ACE Lacey"; }
		}

		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			var lacey = invoiceLine.LaceyActLines.AddNew();
			ConstituentElement constElement = null;

			foreach (var block in pgaBlocks)
			{
				var pg10Block = block as AEPAPG10;
				if (pg10Block != null)
				{
					lacey.US_PGACommercialDescription = pg10Block.CommodityCharacteristicDescription;
				}
				else
				{
					var pg04Block = block as AEPAPG04;
					if (pg04Block != null)
					{
						if (pg04Block.QuantityOfConstituentElement.IsEmpty && pg04Block.UnitOfMeasureConstituentElement.IsEmpty && pg04Block.PercentOfConstituentElement.IsEmpty && pg04Block.ConstituentActiveIngredientQualifier.IsEmpty)
						{
							lacey.US_UnknownBreakdownTotal = true;
							lacey.US_NameOfConstituentElement = pg04Block.NameOfTheConstituentElement;
						}
						else
						{
							constElement = lacey.PG04ConstituentElements.AddNew();
							constElement.US_PGANameOfTheConstituentElement = pg04Block.NameOfTheConstituentElement;
							constElement.US_PGAQuantityOfConstituentElement = pg04Block.QuantityOfConstituentElement;
							constElement.US_PGAUnitOfMeasure = pg04Block.UnitOfMeasureConstituentElement;
							constElement.US_PGAPercentOfConstituentElement = pg04Block.PercentOfConstituentElement;
							lacey.US_UnknownBreakdown = pg04Block.ConstituentActiveIngredientQualifier == "Y";
						}
					}
					else
					{
						var pg05Block = block as AEPAPG05;
						if (pg05Block != null)
						{
							if (lacey.US_UnknownBreakdownTotal)
							{
								constElement = lacey.PG04ConstituentElements.AddNew();
							}

							if (constElement != null)
							{
								constElement.US_GenusName = pg05Block.ScientificGenusName;
								constElement.US_SpecialUseDesignation = pg05Block.ScientificGenusName == ConstituentElement.Special;
								constElement.US_SpeciesName = pg05Block.ScientificSpeciesName;
							}
						}
						else
						{
							var pg06Block = block as AEPAPG06;
							if (pg06Block != null)
							{
								if (lacey.US_UnknownBreakdownTotal)
								{
									var country = lacey.LaceyCountries.AddNew();
									country.US_CountryCode = pg06Block.CountryCode;
								}
								else if (constElement != null)
								{
									constElement.US_UnknownBreakdownCountryCode = pg06Block.CountryCode;
								}
							}
							else
							{
								var pg29Block = block as AEPAPG29;
								if (pg29Block != null)
								{
									if (lacey.US_UnknownBreakdownTotal)
									{
										lacey.US_QuantityOfConstituentElement = pg29Block.CommodityNetQuantityPGALineNet;
										lacey.US_UnitOfMeasure = pg29Block.UnitOfMeasurePGALineNet;
									}
								}
								else
								{
									var pg19Block = block as AEPAPG19;
									if (pg19Block != null)
									{
										if (pg19Block.EntityRoleCode == EntityRoleCodeList.Codes.Importer && invoiceLine.Declaration != null && invoiceLine.Declaration.IOROrgPK.IsEmpty)
										{
											notifications.AddWarning("Importer of record should have populated from SE10 or ENS10 record. Please supply EIN/CBN/SSN of an importer of record in one of the records.");
										}
									}
									else
									{
										var pg21Block = block as AEPAPG21;
										if (pg21Block != null)
										{
											lacey.US_CertifyingIndividual = pg21Block.IndividualQualifier;
											lacey.US_PGAContactName = pg21Block.IndividualName;
											lacey.US_PGAContactPhoneNo = pg21Block.TelephoneNumberOfTheIndividual;
											lacey.US_PGAContactEmail = pg21Block.EmailAddressOrFaxNumberForTheIndividual;
										}
										else
										{
											var pg25Block = block as AEPAPG25;
											if (pg25Block != null)
											{
												lacey.US_InvCurrPGAValue = pg25Block.PGALineValue;
											}
											else
											{
												var pg27Block = block as AEPAPG27;
												if (pg27Block != null)
												{
													CreatContainersOnDeclaration(pg27Block.ContainerNumberEquipmentID);
													CreatContainersOnDeclaration(pg27Block.ContainerNumberEquipmentID1);
													CreatContainersOnDeclaration(pg27Block.ContainerNumberEquipmentID2);
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

		void CreatContainersOnDeclaration(ZString containerNumber)
		{
			if (!containerNumber.IsEmpty && !invoiceLine.Declaration.CusContainers.OfType<CusContainer>().Any(x => x.CO_ContainerNumber == containerNumber))
			{
				invoiceLine.Declaration.CusContainers.AddNew().CO_ContainerNumber = containerNumber;
			}
		}
	}
}
