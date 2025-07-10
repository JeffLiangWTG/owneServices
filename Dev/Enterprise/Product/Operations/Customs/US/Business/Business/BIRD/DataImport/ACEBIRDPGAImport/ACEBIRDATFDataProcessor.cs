using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business
{
	public class ACEBIRDATFDataProcessor : ACEBIRDCommonPGADataProcessor
	{
		public ACEBIRDATFDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return null; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_ATFIndInfo; }
		}

		public override ZString PGAName
		{
			get { return "ATF"; }
		}

		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			var atf = invoiceLine.ATFLines.AddNew();

			foreach (var block in pgaBlocks)
			{
				var pg07Block = block as AEPAPG07;
				if (pg07Block != null)
				{
					atf.US_Model = pg07Block.TradeNameBrandName;
					atf.US_CaliberGaugeSize = pg07Block.Model;
				}
				else
				{
					var pg10Block = block as AEPAPG10;
					if (pg10Block != null)
					{
						atf.US_CategoryCode = pg10Block.CategoryCode;
						atf.US_ExtendedDescription = pg10Block.CommodityCharacteristicDescription;
					}
					else
					{
						var pg26Block = block as AEPAPG26;
						if (pg26Block != null)
						{
							atf.US_Quantity = pg26Block.Quantity;
						}
						else
						{
							var pg29Block = block as AEPAPG29;
							if (pg29Block != null)
							{
								atf.US_BarrelLength = pg29Block.CommodityNetQuantityIndividualUnitNet;
								atf.US_OverallLength = pg29Block.CommodityGrossQuantityIndividualUnitGross;
							}
							else
							{
								var pg14Block = block as AEPAPG14;
								if (pg14Block != null)
								{
									ProcessExemptionCodeAndNumbers(atf, pg14Block.LPCOType, pg14Block.LPCONumberorName, pg14Block.ExemptionCode);
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
									else
									{
										var pg32Block = block as AEPAPG32;
										if (pg32Block != null)
										{
											var countryOfExport = pg32Block.CommodityRoutingCountryCode;
											if (pg32Block.CommodityRoutingTypeCode == "198")
											{
												invoiceLine.Factory.GetCachedValue<BIRDUpdateHeaderHelperTool>().UpdateOrWarn(invoiceLine, JobComInvoiceLine.Schema.US_UC_NKCountryOfExport, countryOfExport, countryOfExport, "Export Country", notifications);
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

		void ProcessExemptionCodeAndNumbers(ATF atf, ZString type, ZString number, ZString exemptionCode)
		{
			ZPropertyInfo numberInfo = null;
			ZPropertyInfo exemptionCodeInfo = null;

			switch (type)
			{
				case LPCOTypeList.Codes.AT2:
					numberInfo = atf.US_FFLNumberInfo;
					exemptionCodeInfo = atf.US_FFLExemptionCodeInfo;
					break;
				case LPCOTypeList.Codes.AT3:
					numberInfo = atf.US_FELNumberInfo;
					exemptionCodeInfo = atf.US_FELExemptionCodeInfo;
					break;
				case LPCOTypeList.Codes.AT4:
					numberInfo = atf.US_PermitNumberInfo;
					exemptionCodeInfo = atf.US_PermitExemptionCodeInfo;
					break;
				case LPCOTypeList.Codes.AT5:
					numberInfo = atf.US_AECANumberInfo;
					exemptionCodeInfo = atf.US_AECAExemptionCodeInfo;
					break;
			}

			if (!number.IsEmpty && numberInfo != null)
			{
				numberInfo.Value = number;
			}

			if (!exemptionCode.IsEmpty && exemptionCodeInfo != null)
			{
				exemptionCodeInfo.Value = exemptionCode;
			}
		}
	}
}
