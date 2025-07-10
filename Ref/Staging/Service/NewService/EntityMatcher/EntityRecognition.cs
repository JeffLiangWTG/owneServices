using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using edu.stanford.nlp.ie.crf;
using edu.stanford.nlp.util;

namespace CargoWise.RefDbRepo.Staging.NewService
{
	public class EntityRecognition : IEntityRecognition
	{
		public EntityRecognition()
		{
			var binFolder = string.Empty;
			binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var loadPath = Path.Combine(binFolder, "english.all.3class.distsim.crf.ser.gz");
			classifier = CRFClassifier.getClassifierNoExceptions(loadPath);
		}

		readonly CRFClassifier classifier;

		public IEnumerable<string> GetNamedEntities(string text)
		{
			var results = classifier.classifyToCharacterOffsets(text ?? string.Empty);
			if (results.size() > 0)
			{
				for (int i = 0; i < results.size(); i++)
				{
					var result = (Triple)results.get(i);
					var startOffset = int.Parse(result.second().ToString(), CultureInfo.InvariantCulture);
					var endOffSet = int.Parse(result.third().ToString(), CultureInfo.InvariantCulture);
					yield return text.Substring(startOffset, endOffSet - startOffset);
				}
			}
			else
			{
				yield return text;
			}
		}
	}
}
