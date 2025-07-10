using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface ICO2eCalculationSupporter : ICO2eProvider
	{
		BusinessObjectFactory Factory { get; }

		List<string> ValidateInputs();
		void OnRequested();
		void OnRejected(string reason);
		void OnCalculated(bool succeeded);
		bool SaveEmissionsLogToNoteOnCalculated { get; }

		AdditionalCalculationSupporter[] AdditionalCalculationSupporters { get; }
		void OnAdditionalSupporterCalculated(ICO2eCalculationSupporter additionalSupporter);
	}

	public struct AdditionalCalculationSupporter
	{
		public AdditionalCalculationSupporter(ICO2eCalculationSupporter supporter, Func<ZDecimal> getApportionedValue)
		{
			Supporter = supporter;
			GetApportionedValue = getApportionedValue;
		}

		public ICO2eCalculationSupporter Supporter;
		public Func<ZDecimal> GetApportionedValue;
	}
}
