using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

public partial class StagingDbContext : DbContext
{
	public StagingDbContext() : this("Name=ConnectionStrings:Staging")
	{
	}

	public StagingDbContext(string nameOrConnectionString, bool autoDetectChanges = true, IInterceptor[] interceptors = null) : base()
	{
		this.nameOrConnectionString = nameOrConnectionString;
		this.autoDetectChanges = autoDetectChanges;
		this.interceptors = interceptors;
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		if (!optionsBuilder.IsConfigured)
		{
			optionsBuilder.UseSqlServer(nameOrConnectionString,
				x => x.UseNetTopologySuite());
			if (!autoDetectChanges)
			{
				optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
			}
			if (interceptors != null)
			{
				optionsBuilder.AddInterceptors(interceptors);
			}
		}
	}

#pragma warning disable CA1822 // Mark members as static
	partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
#pragma warning restore CA1822 // Mark members as static
	{
		//re-configure views
		ReconfigureViews(modelBuilder);
	}

	static void ReconfigureViews(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<RefAccTaxRateUserView>(entity =>
		{
			entity.ToTable("RefAccTaxRateUserView", tb =>
			{
				tb.HasTrigger("RefAccTaxRateUserView_Version_Create");
				tb.HasTrigger("RefAccTaxRateUserView_Version_Update");
				tb.HasTrigger("RefAccTaxRateUserView_Version_Delete");
			});
			entity.HasKey(e => e.ZAT_PK);
			entity.Property(x => x.ZAT_IsSystem).ValueGeneratedOnAddOrUpdate();
		});

		modelBuilder.Entity<RefCusCodeListUserView>(entity =>
		{
			entity.ToTable("RefCusCodeListUserView", tb =>
			{
				tb.HasTrigger("RefCusCodeListUserView_Version_Create");
				tb.HasTrigger("RefCusCodeListUserView_Version_Update");
				tb.HasTrigger("RefCusCodeListUserView_Version_Delete");
			});
			entity.HasKey(e => e.ZZD_PK);
			entity.Property(x => x.ZZD_IsSystem).ValueGeneratedOnAddOrUpdate();
		});

		modelBuilder.Entity<RefCusCodeListAttributeUserView>(entity =>
		{
			entity.ToTable("RefCusCodeListAttributeUserView", tb =>
			{
				tb.HasTrigger("RefCusCodeListAttributeUserView_Version_Create");
				tb.HasTrigger("RefCusCodeListAttributeUserView_Version_Update");
				tb.HasTrigger("RefCusCodeListAttributeUserView_Version_Delete");
			});
			entity.HasKey(e => e.ZZE_PK);
		});

		modelBuilder.Entity<RefCusCodeListAttributeUserView>(entity =>
		{
			entity.ToTable("RefCusCodeListAttributeUserView", tb =>
			{
				tb.HasTrigger("RefCusCodeListAttributeUserView_Version_Create");
				tb.HasTrigger("RefCusCodeListAttributeUserView_Version_Update");
				tb.HasTrigger("RefCusCodeListAttributeUserView_Version_Delete");
			});
			entity.HasKey(e => e.ZZE_PK);
		});
	}

	readonly string nameOrConnectionString;
	readonly bool autoDetectChanges;
	readonly IInterceptor[] interceptors;
}
