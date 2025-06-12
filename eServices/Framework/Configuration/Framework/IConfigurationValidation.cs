using System;
using System.Collections.Generic;
using System.Text;
using eServices.Configuration.Schemas;

namespace eServices.Configuration.Framework
{
	public interface IConfigurationValidation
	{
		Tuple<ConfigurationMessage, StringBuilder> ValidateAndGenerateResponseIfFailed(ConfigurationMessage configuration);

		ConfigurationMessage GenerateErrorConfigurationResponse(ConfigurationMessage configuration, params string[] errors);
	}
}
