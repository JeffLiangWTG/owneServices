using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.FRReferenceData.Services;

namespace CargoWise.RefDbRepo.FRReferenceData.CmdLine
{
	public abstract class DataCollectorProgram
	{
		public void Run()
		{
			var error = Errors.No;
			ExecuteTasks(ref error);
			Environment.Exit((int)error);
		}

		public void ExecuteTasks(ref Errors error)
		{
			PrepareDataSource(ref error, out var publicationDate);

			var generators = GetGenerators().ToArray();
			GenerateURDFiles(generators, publicationDate, ref error);
			Console.WriteLine("All expected XML files have been successfully created.");
		}

		protected abstract void PrepareDataSource(ref Errors error, out DateTime publicationDate);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Exception will be treated in a report.")]
		static bool GenerateURDFiles(IUniversalReferenceDataFileGenerator[] generators, DateTime publicationDate, ref Errors error)
		{
			var success = true;
			foreach (var generator in generators)
			{
				try
				{
					success &= generator.GenerateFiles(publicationDate, ref error);
				}
				catch (Exception e)
				{
					success = false;
					Console.Error.WriteLine(e);
				}
			}
			return success;
		}

		public abstract string CommandArgument { get; }

		protected abstract IEnumerable<IUniversalReferenceDataFileGenerator> GetGenerators();
	}
}
