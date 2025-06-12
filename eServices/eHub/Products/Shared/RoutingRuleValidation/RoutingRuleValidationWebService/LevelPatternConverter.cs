using log4net.Layout.Pattern;
using log4net.Core;
using System.Collections.Generic;
using log4net.Util;
using System.IO;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation
{
	public class LevelPatternConverter : PatternConverter
	{
		private static readonly Dictionary<string, string> LevelAbbreviations = new Dictionary<string, string>
		{
			{ "INFO", "INF" },
			{ "DEBUG", "DBG" },
			{ "WARN", "WRN" },
			{ "ERROR", "ERR" },
			{ "FATAL", "FAL" },
			{ "TRACE", "TRC" }
		};

		protected override void Convert(TextWriter writer, object state)
		{
			if (state is LoggingEvent loggingEvent)
			{
				string levelName = loggingEvent.Level.Name;
				string abbreviatedLevel;

				if (LevelAbbreviations.TryGetValue(levelName, out string abbreviation))
				{
					abbreviatedLevel = abbreviation;
				}
				else
				{
					abbreviatedLevel = "UKN";
				}

				writer.Write(abbreviatedLevel);
			}
		}
	}
}
