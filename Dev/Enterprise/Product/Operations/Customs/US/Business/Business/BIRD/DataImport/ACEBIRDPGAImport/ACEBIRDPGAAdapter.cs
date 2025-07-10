using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business
{
	public class ACEBIRDPGAAdapter
	{
		public void DoImport(JobComInvoiceLine invoiceLine, List<PGAData> pgaDataList, INotifications notifications)
		{
			var regularInvoiceLine = invoiceLine.IsCombinedLine() ? invoiceLine.ChildLines.FirstOrDefault(x => x.IsNormalTariffLine()) : invoiceLine;

			foreach (var pgaData in pgaDataList)
			{
				ProcessPGAData(regularInvoiceLine, pgaData, notifications);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void ProcessPGAData(JobComInvoiceLine invoiceLine, PGAData pgaData, INotifications notifications)
		{
			foreach (var pg01 in pgaData.PGADataLines)
			{
				var governmentAgencyCode = pg01.Key.GovernmentAgencyCode;
				ACEBIRDCommonPGADataProcessor processor = null;

				switch (governmentAgencyCode)
				{
					case ACEGovernmentAgenciesCodeList.Codes.FDA:
						processor = new ACEBIRDFDADataProcessor(invoiceLine);
						break;
					case ACEGovernmentAgenciesCodeList.Codes.FSI:
						processor = new ACEBIRDFSISDataProcessor(invoiceLine);
						break;
					case ACEGovernmentAgenciesCodeList.Codes.NHT:
						processor = new ACEBIRDNHTSADataProcessor(invoiceLine);
						break;
					case ACEGovernmentAgenciesCodeList.Codes.EPA:
						processor = ProcessEPAData(invoiceLine, pg01.Key);
						break;
					case ACEGovernmentAgenciesCodeList.Codes.NMF:
						processor = ProcessNMFSData(invoiceLine, pg01.Key);
						break;
					case ACEGovernmentAgenciesCodeList.Codes.DTC:
						processor = new ACEBIRDDDTCDataProcessor(invoiceLine);
						break;
					case ACEGovernmentAgenciesCodeList.Codes.TTB:
						processor = new ACEBIRDTTBDataProcessor(invoiceLine);
						break;
					case ACEGovernmentAgenciesCodeList.Codes.AMS:
						var program = pg01.Key.GovernmentAgencyProgramCode + pg01.Key.GovernmentAgencyProcessingCode;
						if (program == AMSProgramList.Codes.OR1 || program == AMSProgramList.Codes.OR2)
						{
							processor = new ACEBIRDAMSNOPDataProcessor(invoiceLine);
						}
						else
						{
							processor = new ACEBIRDAMSDataProcessor(invoiceLine);
						}
						break;
					case ACEGovernmentAgenciesCodeList.Codes.APH:
						processor = ProcessAPHISOrLaceyData(invoiceLine, pg01.Key);
						break;
					case ACEGovernmentAgenciesCodeList.Codes.FWS:
						processor = new ACEBIRDFWSDataProcessor(invoiceLine);
						break;
					case GovernmentAgencyProgramCodeList.Codes.ATF:
						processor = new ACEBIRDATFDataProcessor(invoiceLine);
						break;
					case GovernmentAgencyProgramCodeList.Codes.OMC:
						processor = new ACEBIRDOMCDataProcessor(invoiceLine);
						break;
					case GovernmentAgencyProgramCodeList.Codes.DEA:
						processor = new ACEBIRDDEADataProcessor(invoiceLine);
						break;
					case ACEGovernmentAgenciesCodeList.Codes.CPS:
						processor = new ACEBIRDCPSCDataProcessor(invoiceLine);
						break;
				}

				if (processor != null)
				{
					processor.DoImport(pg01.Key, pg01.Value, notifications);
				}
				else if (notifications != null)
				{
					var governmentAgencyProgramCode = pg01.Key.GovernmentAgencyProgramCode;
					notifications.AddWarning(ZString.Format("{0}{1} import via BIRD is not supported yet.", governmentAgencyCode, governmentAgencyProgramCode.IsEmpty ? "" : " - " + governmentAgencyProgramCode));
				}
			}
		}

		ACEBIRDCommonPGADataProcessor ProcessEPAData(JobComInvoiceLine invoiceLine, AEPAPG01 pg01)
		{
			switch (pg01.GovernmentAgencyProgramCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.ODS:
					return new ACEBIRDODSDataProcessor(invoiceLine);
				case "TS1":
					return new ACEBIRDTSCADataProcessor(invoiceLine);
				case GovernmentAgencyProgramCodeList.Codes.VNE:
					return new ACEBIRDVNEDataProcessor(invoiceLine);
				case GovernmentAgencyProgramCodeList.Codes.HFC:
					return new ACEBIRDHFCDataProcessor(invoiceLine);
				case PSTProductTypeList.Codes.PS1:
				case PSTProductTypeList.Codes.PS2:
				case PSTProductTypeList.Codes.PS3:
					return new ACEBIRDPSTDataProcessor(invoiceLine);
			}

			return null;
		}

		ACEBIRDCommonPGADataProcessor ProcessNMFSData(JobComInvoiceLine invoiceLine, AEPAPG01 pg01)
		{
			switch (pg01.GovernmentAgencyProgramCode)
			{
				case NMFSProgramCodeList.Codes._370:
					return new ACEBIRDNMFS370DataProcessor(invoiceLine);
				case NMFSProgramCodeList.Codes.AMR:
					return new ACEBIRDNMFSAMRDataProcessor(invoiceLine);
				case NMFSProgramCodeList.Codes.HMS:
					return new ACEBIRDNMFSHMSDataProcessor(invoiceLine);
				case NMFSProgramCodeList.Codes.SIM:
					return new ACEBIRDNMFSSIMPDataProcessor(invoiceLine);
			}

			return null;
		}

		ACEBIRDCommonPGADataProcessor ProcessAPHISOrLaceyData(JobComInvoiceLine invoiceLine, AEPAPG01 pg01)
		{
			switch (pg01.GovernmentAgencyProgramCode)
			{
				case APHISProgramCodeList.Codes.AAC:
				case APHISProgramCodeList.Codes.ABS:
				case APHISProgramCodeList.Codes.APQ:
				case APHISProgramCodeList.Codes.AVS:
					return new ACEBIRDAPHISDataProcessor(invoiceLine);
				case "APL":
					return new ACEBIRDLaceyDataProcessor(invoiceLine);
			}

			return null;
		}
	}
}
