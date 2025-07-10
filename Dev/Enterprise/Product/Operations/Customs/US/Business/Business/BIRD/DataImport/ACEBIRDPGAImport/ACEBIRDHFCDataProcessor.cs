using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business
{
	public class ACEBIRDHFCDataProcessor : ACEBIRDCommonPGADataProcessor
	{
		public ACEBIRDHFCDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_HFCDisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_HFCIndInfo; }
		}

		public override ZString PGAName
		{
			get { return GovernmentAgencyProgramCodeList.Codes.HFC; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			var hfcHeader = invoiceLine.USHFCHeaders.AddNew();
			hfcHeader.US_HFCImageSent = pg01.ElectronicImageSubmitted == "Y";

			USHFCDetail currentHFCDetail = null;

			var currentCertifyingIndividual = ZString.Empty;
			foreach (var block in pgaBlocks)
			{
				var pg02Block = block as AEPAPG02;
				if (pg02Block != null)
				{
					if (!pg02Block.ProductCodeNumber.IsEmpty)
					{
						currentHFCDetail = hfcHeader.USHFCDetails.AddNew();
						currentHFCDetail.US_LPCONumber = pg02Block.ProductCodeNumber;
					}
				}
				else
				{
					var pg04Block = block as AEPAPG04;
					if (pg04Block != null)
					{
						if (currentHFCDetail == null)
						{
							currentHFCDetail = hfcHeader.USHFCDetails.AddNew();
						}

						currentHFCDetail.US_NameOfActiveIngredient = pg04Block.NameOfTheConstituentElement;
						currentHFCDetail.US_ActiveIngredientPercentage = pg04Block.PercentOfConstituentElement;
						currentHFCDetail = null;
					}
					else
					{
						var pg07Block = block as AEPAPG07;
						if (pg07Block != null)
						{
							hfcHeader.US_ASHRAENumber = pg07Block.ItemIdentityNumber;
						}
						else
						{
							var pg19Block = block as AEPAPG19;
							if (pg19Block != null)
							{
								if (pg19Block.EntityRoleCode == EntityRoleCodeList.Codes.CertifyingIndividual)
								{
									hfcHeader.US_CertifyingIndividual = EntityRoleCodeList.Codes.CustomsBroker;
								}
								else
								{
									currentCertifyingIndividual = pg19Block.EntityRoleCode;
								}
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
								else
								{
									var pg29Block = block as AEPAPG29;
									if (pg29Block != null)
									{
										hfcHeader.US_NetWeight = pg29Block.CommodityNetQuantityPGALineNet;
									}
									else
									{
										var pg55 = block as AEPAPG55;
										if (pg55 != null && !currentCertifyingIndividual.IsEmpty)
										{
											hfcHeader.US_CertifyingIndividual = currentCertifyingIndividual;
											currentCertifyingIndividual = ZString.Empty;
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
