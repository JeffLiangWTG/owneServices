using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;

namespace Enterprise.Freight.Forwarding.GUI.DangerousGoods
{
	public class UrlGenerator : IUrlGenerator
	{
		readonly Dictionary<string, string> _businessEntityTypeToGlowEndPointMapping;

		public UrlGenerator(Dictionary<string, string> businessEntityTypeToGlowEndPointMapping)
		{
			Argument.NotNull(businessEntityTypeToGlowEndPointMapping, nameof(businessEntityTypeToGlowEndPointMapping));
			_businessEntityTypeToGlowEndPointMapping = businessEntityTypeToGlowEndPointMapping;
		}

		public string GenerateUrl(IBusiness businessEntity)
		{
			var businessEntityType = businessEntity.GetType().FullName;
			if (!_businessEntityTypeToGlowEndPointMapping.TryGetValue(businessEntityType, out var endPoint))
			{
				throw new ArgumentException($"GLOW‌ end point for this type of business entity is missing: {businessEntityType}");
			}

			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl(
				endPoint,
				businessEntity.HumanReadableName,
				additionalQueryStrings: new[] { ("entityPK", businessEntity.Identifier.ToString()) });

			return url?.ToString();
		}
	}
}
