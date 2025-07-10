using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	abstract class CommonCMRDataParserAbstractTest
	{
		protected abstract string TestFileFolderName { get; }

		protected abstract string TextFileName { get; }

		protected abstract string XMLFileName { get; }

		[SetUp]
		public virtual void Setup()
		{
			executingAssembly = Assembly.GetExecutingAssembly();
		}
		protected Assembly executingAssembly;
	}
}
