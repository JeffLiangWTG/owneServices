using System;
using Dat.Integration;
using WTG.DevTools.Definitions;

namespace CargoWise.RefDbRepo.SpecialFileHandling.Test
{
	static class RepositoryKeyExtensions
	{
		public static IRepositoryKey WrapForExtensions(this RepositoryKey repositoryKey)
		{
			if (repositoryKey is null)
			{
				throw new ArgumentNullException(nameof(repositoryKey));
			}

			return new RepositoryKeyWrapper(repositoryKey);
		}

		sealed class RepositoryKeyWrapper : IRepositoryKey
		{
			public RepositoryKeyWrapper(RepositoryKey repositoryKey)
			{
				this.repositoryKey = repositoryKey;
			}

			string IRepositoryKey.TargetRepository => repositoryKey.TargetRepository;
			string IRepositoryKey.Branch => repositoryKey.Branch;
			string IRepositoryKey.Path => repositoryKey.Path;
			bool IRepositoryKey.IsReleaseBranch => repositoryKey.IsReleaseBranch;
			string IRepositoryKey.GetServerPath(string relativePath) => repositoryKey.GetServerPath(relativePath);

			readonly RepositoryKey repositoryKey;
		}
	}
}
