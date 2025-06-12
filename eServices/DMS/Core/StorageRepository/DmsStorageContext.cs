using Microsoft.EntityFrameworkCore;

namespace eServices.Dms.Core.StorageRepository;

public partial class DmsStorageContext(DbContextOptions<DmsStorageContext> options) : DbContext(options)
{
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<DmsStorageCatalog>(entity =>
		{
			entity.Property(e => e.SC_CreateTime).HasDefaultValueSql("(sysutcdatetime())");
			entity.Property(e => e.SC_CreateUser).HasDefaultValueSql("(suser_name())");
			entity.Property(e => e.SC_VersionMajor).HasComputedColumnSql("(convert([int],left([SC_Version],charindex('.',[SC_Version])-(1))))", false);
			entity.Property(e => e.SC_VersionMinor).HasComputedColumnSql("(convert([int],substring([SC_Version],charindex('.',[SC_Version])+(1),case when charindex('-',[SC_Version])>(0) then charindex('-',[SC_Version])-(1) else len([SC_Version]) end-charindex('.',[SC_Version]))))", false);
			entity.Property(e => e.SC_VersionLabel).HasComputedColumnSql("(substring([SC_Version],charindex('-',[SC_Version]),case when charindex('-',[SC_Version])>(0) then (len([SC_Version])-charindex('-',[SC_Version]))+(1) else (0) end))", false);
		});
	}

	public virtual DbSet<DmsStorageCatalog> DmsStorageCatalog { get; set; }
}
