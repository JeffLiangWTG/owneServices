using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DeniedPartyScreening.Business
{
	public class ReconcileParameters
	{
		public DateTime? StartDateUtc { get; set; }
		public DateTime? EndDateUtc { get; set; }
		public int? BatchSize { get; set; }
		public bool IsEnabled { get; set; }
	}

	class ReconcileParametersFromFeatureControl
	{
		public DateTime? StartDateUtc { get; set; }
		public DateTime? EndDateUtc { get; set; }
		public int? BatchSize { get; set; }
	}

	[CodeAlive("Will be used by entity reconcile service task")]
	public static class DpsEntityReconcileHelper
	{
		public static async Task<ReconcileParameters> GetReconcileParametersFromFeatureDataAsync()
		{
			var ret = new ReconcileParameters
			{
				IsEnabled = false
			};

			var featureData = await ObjectFactory.Get<IFeatureControlManager>().GetFeatureDataAsync(LicenceFeatureCodeList.Codes.DpsEntityReconcileServiceTaskFlag, CancellationToken.None);

			if (featureData is null)
			{
				return ret;
			}

			var isDataDeserializable = featureData.TryDeserializeParameterAsJson<ReconcileParametersFromFeatureControl>(out var featureParameters);

			if (!isDataDeserializable)
			{
				ErrorReporter.ReportOnce(nameof(GetReconcileParametersFromFeatureDataAsync), $"Feature Control Data has been inputted incorrectly. [[{featureData.Parameter}]]");
				return ret;
			}

			ret.StartDateUtc = featureParameters.StartDateUtc;
			ret.EndDateUtc = featureParameters.EndDateUtc;
			ret.BatchSize = featureParameters.BatchSize;
			ret.IsEnabled = true;

			return ret;
		}

		public static byte[] GetEntityDetailsHash(BusinessObject entity, DpsEntityDetailsHasher entityHasher)
		{
			var candidateCreator = new DpsCandidateCreator();

			var candidate = entity switch
			{
				OrgHeader orgHeader => candidateCreator.NewRequestHeader(orgHeader),
				RefVessel refVessel => candidateCreator.NewRequestHeader(refVessel),
				_ => null
			};

			if (candidate == null)
			{
				return Array.Empty<byte>();
			}

			var entityDetailsForHahshing = new DpsEntityDetailsForHashing
			{
				Names = candidate.DpsNameCandidates.Select(name => name.FullName),
				ScreeningStatus = ((IScreeningPartyProvider)entity).ScreeningStatus,
				RegCodeDetails = candidate.DpsRegistrationCodeCandidates?
					.Where(regCode => new[] { "IMO", OrgCusCode.CodeTypes.PassportID, OrgCusCode.CodeTypes.DataUniversalNumberingSystem }.Contains(regCode.RegCodeType))
					.Select(regCode => new DpsRegCodeDetails
					{
						RegCodeValue = regCode.RegCodeValue,
						RegCodeType = regCode.RegCodeType,
						RegCountryCode = regCode.RegCountryCode
					}),
			};

			return entityHasher.GetHashAndReset(entityDetailsForHahshing);
		}
	}
}
