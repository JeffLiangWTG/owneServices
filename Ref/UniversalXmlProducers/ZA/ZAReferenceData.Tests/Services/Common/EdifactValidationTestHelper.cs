using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using Enterprise.Edifact.Auto;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common
{
	internal static class EdifactValidationTestHelper
	{
		public static T PrepareMsgSection<T>(string[] content, T newT, StringBuilder errorCollector) where T : MessageSectionBase
		{
			T sec = null;

			if (content?.Any() ?? false)
			{
				sec = newT;
				sec.PopulateFromMessage(ZACharacterSet.Instance, new Enterprise.Edifact.MessageDataStream(content));
			}

			if (errorCollector != null)
			{
				errorCollector.Clear();
			}

			return sec;
		}

		public static T PrepareSecGroup<T>(string[] content, StringBuilder errorCollector) where T : SegmentGroup, new()
		{
			T sec = null;

			if (content?.Any() ?? false)
			{
				sec = new T();
				sec.Parse(ZACharacterSet.Instance, new Enterprise.Edifact.MessageDataStream(content));
			}

			if (errorCollector != null)
			{
				errorCollector.Clear();
			}

			return sec;
		}
	}
}
