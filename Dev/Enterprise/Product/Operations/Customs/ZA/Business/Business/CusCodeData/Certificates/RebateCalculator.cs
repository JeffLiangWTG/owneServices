using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	public abstract class RebateCalculator<T>
		where T : CertificateCusCodeData
	{
		class PermitCertificate
		{
			public CusPermitHeader PermitHeader { get; set; }
			public T Certificate { get; set; }
		}

		public RebateCalculator(JobDeclaration declaration)
		{
			permitDictionary = new Dictionary<CusPermitHeader, ZDecimal>();
			var certificates = GetAllCertificates(declaration);

			foreach (var permit in certificates.Cast<T>().Where(x => x.PermitType == PermitTypeCore).OrderBy(x => x.CY_Order).Select(x => new PermitCertificate { PermitHeader = x.PermitHeader, Certificate = x }).Distinct())
			{
				if (!permitDictionary.ContainsKey(permit.PermitHeader))
				{
					var valueBalance = permit.Certificate.RemainingValueExcludingThisDeclaration - certificates.OfType<T>().Where(c => c.CY_Code == permit.Certificate.CY_Code).Sum(c => c.ValueUsedByThisDeclaration);
					permitDictionary.Add(permit.PermitHeader, valueBalance);
				}
			}
		}

		public ZDecimal AmountToRebate { get; set; }

		protected abstract ZString PermitTypeCore { get; }

		protected abstract IEnumerable<T> GetCertificatesForEntryLine(CusEntryLine entryLine);

		protected abstract IEnumerable<T> GetAllCertificates(JobDeclaration declaration);

		protected abstract ZString GetPRVValue(decimal value);

		public List<Action<ZShort>> CalculateRebatedValueForDuty(ZDecimal adjustmentValue, CusEntryLine entryLine, decimal adjustmentFactor = 1m)
		{
			var additionalInfoActions = new List<Action<ZShort>>();

			var permitsForEntryLine = GetCertificatesForEntryLine(entryLine).Cast<T>().Where(x => x.PermitType == PermitTypeCore).OrderBy(x => x.CY_Order).Select(x => x.PermitHeader);

			var adjustmentToMake = adjustmentValue * adjustmentFactor;
			var adjustmentsMade = 0.0m;

			var permitKeys = new List<CusPermitHeader>(permitDictionary.Keys.Where(x => permitsForEntryLine.Contains(x)));

			foreach (var permit in permitKeys)
			{
				var balance = permitDictionary[permit];

				if (balance > 0 && adjustmentToMake > 0)
				{
					if (adjustmentToMake <= balance)
					{
						adjustmentsMade += adjustmentToMake;
						permitDictionary[permit] = permitDictionary[permit] - adjustmentToMake;
						var adjustmentMadeForAction = adjustmentToMake;

						additionalInfoActions.Add(index =>
						{
							var addInfoPRV = entryLine.AdditionalInformationCodes.AddNew();
							addInfoPRV.CY_Code = UniversalReferenceConstants.AdditionalInformation.ProductionRebateValue;
							addInfoPRV.CY_Data = GetPRVValue(adjustmentMadeForAction);
							var grouping = entryLine.AdditionalInformationCodes.Cast<AdditionalInformation>().Count(x => x.CY_Code == UniversalReferenceConstants.AdditionalInformation.ProductionRebateValue);
							addInfoPRV.CY_Order = index;

							var addInfoPRC = entryLine.AdditionalInformationCodes.AddNew();
							addInfoPRC.CY_Code = UniversalReferenceConstants.AdditionalInformation.ProductionRebateCertificate;
							addInfoPRC.CY_Data = string.Concat(permit.CPH_SubType, permit.CPH_Number);
							addInfoPRC.CY_Order = index;
						});

						adjustmentToMake = 0;
					}
					else if (adjustmentToMake > balance)
					{
						var actualAdjustment = balance;
						adjustmentsMade += balance;
						permitDictionary[permit] = permitDictionary[permit] - actualAdjustment;

						additionalInfoActions.Add(index =>
						{
							var addInfoPRV = entryLine.AdditionalInformationCodes.AddNew();
							addInfoPRV.CY_Code = UniversalReferenceConstants.AdditionalInformation.ProductionRebateValue;
							addInfoPRV.CY_Data = GetPRVValue(actualAdjustment);
							var grouping = entryLine.AdditionalInformationCodes.Cast<AdditionalInformation>().Count(x => x.CY_Code == UniversalReferenceConstants.AdditionalInformation.ProductionRebateValue);
							addInfoPRV.CY_Order = index;

							var addInfoPRC = entryLine.AdditionalInformationCodes.AddNew();
							addInfoPRC.CY_Code = UniversalReferenceConstants.AdditionalInformation.ProductionRebateCertificate;
							addInfoPRC.CY_Data = string.Concat(permit.CPH_SubType, permit.CPH_Number);
							addInfoPRC.CY_Order = index;
						});

						adjustmentToMake -= actualAdjustment;
					}
				}
			}

			AmountToRebate = adjustmentsMade;
			return additionalInfoActions;
		}

		readonly IDictionary<CusPermitHeader, ZDecimal> permitDictionary;
	}
}
