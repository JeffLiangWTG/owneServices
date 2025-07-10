using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Deployment.ParameterCreation;

namespace CargoWise.RefDbRepo.Deployment
{
	static class ParameterCreationStrategyFactory
	{
		static readonly Dictionary<string, Func<IParameterCreationStrategy[]>> Strategies =
			new Dictionary<string, Func<IParameterCreationStrategy[]>>
			{
				{ "TEST", () => new [] { new TestEnvParameterCreationStrategy() } },
				{ "ALPHA", () => new[] { new AlphaEnvParameterCreationStrategy() } },
				{ "BETA", () => new[] { new BetaEnvParameterCreationStrategy() } },
				{ "GAMMA", () => new[] { new GammaEnvParameterCreationStrategy() } },
				{ "DELTA", () => new[] { new DeltaEnvParameterCreationStrategy() } },
				{ "EPSILON", () => new[] { new EpsilonEnvParameterCreationStrategy() } },
				{ "ZETA", () => new[] { new ZetaEnvParameterCreationStrategy() } },
				{ "THETA", () => new[] { new ThetaEnvParameterCreationStrategy() } },
				{ "ETA", () => new[] { new EtaEnvParameterCreationStrategy() } },
				{ "UAT", () => new[] { new UATEnvParameterCreationStrategy() } },
				{ "PRO", () => new[] { new ProEnvParameterCreationStrategy() } }
			};

		public static IParameterCreationStrategy[] GetStrategies(string env)
		{
			if (Strategies.TryGetValue(env, out var strategyFunc))
			{
				return strategyFunc();
			}
			throw new NotImplementedException();
		}
	}
}
