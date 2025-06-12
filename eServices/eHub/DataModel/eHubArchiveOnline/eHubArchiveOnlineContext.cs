using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.DataModel.eHubArchiveOnline
{
	public partial class eHubArchiveOnlineContext : ContextBase
	{
		public eHubArchiveOnlineContext()
		{
		}

		public eHubArchiveOnlineContext(string connectionString) : base(connectionString) { }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
			modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
		}
		public virtual DbSet<eHubArchiveMessage> eHubArchiveMessages { get; set; }
		public virtual DbSet<eHubArchiveMessageDecoded> eHubArchiveMessagesDecoded { get; set; }
		public virtual DbSet<eHubClient> eHubClients { get; set; }
	}
}
