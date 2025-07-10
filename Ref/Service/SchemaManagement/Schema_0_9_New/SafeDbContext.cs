using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	public partial class SafeDbContext : DbContext
	{
		readonly string nameOrConnectionString;
		public SafeDbContext() : this("Name=ConnectionStrings:Safe") { }

		public SafeDbContext(string nameOrConnectionString)
		{
			this.nameOrConnectionString = nameOrConnectionString;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseSqlServer(nameOrConnectionString,
					x => x.UseNetTopologySuite());
			}
		}

		public virtual int SetUserId(string userId)
		{
			return Database.ExecuteSqlRaw($"EXEC dbo.SetUserId '{userId}'");
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
				});
				entity.HasKey(e => e.ZAT_PK);
				entity.Property(x => x.ZAT_SysStartTime).ValueGeneratedOnAddOrUpdate();
				entity.Property(x => x.ZAT_SysEndTime).ValueGeneratedOnAddOrUpdate();
				entity.Property(x => x.ZAT_IsSystem).ValueGeneratedOnAddOrUpdate();
				entity.Ignore(e => e.ZAT_IsEditable);
			});

			modelBuilder.Entity<RefCusCodeListUserView>(entity =>
			{
				entity.ToTable("RefCusCodeListUserView", tb =>
				{
					tb.HasTrigger("RefCusCodeListUserView_Version_Create");
					tb.HasTrigger("RefCusCodeListUserView_Version_Update");
				});
				entity.HasKey(e => e.ZZD_PK);
				entity.Ignore(e => e.ZZD_IsEditable);
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
				entity.Ignore(e => e.ZZE_IsEditable);
			});

			modelBuilder.Entity<RefCusProcedureUserView>(entity =>
			{
				entity.ToTable("RefCusProcedureUserView", tb =>
				{
					tb.HasTrigger("RefCusProcedureUserView_Version_Create");
					tb.HasTrigger("RefCusProcedureUserView_Version_Update");
				});
				entity.HasKey(e => e.ZZ6_PK);
				entity.Ignore(e => e.ZZ6_IsEditable);
			});

			modelBuilder.Entity<RefCusProcedureAttributeUserView>(entity =>
			{
				entity.ToTable("RefCusProcedureAttributeUserView", tb =>
				{
					tb.HasTrigger("RefCusProcedureAttributeUserView_Version_Create");
					tb.HasTrigger("RefCusProcedureAttributeUserView_Version_Update");
					tb.HasTrigger("RefCusProcedureAttributeUserView_Version_Delete");
				});
				entity.HasKey(e => e.ZXB_PK);
				entity.Ignore(e => e.ZXB_IsEditable);
			});

			modelBuilder.Entity<RefPortPolygonUserView>(entity =>
			{
				entity.ToTable("RefPortPolygonUserView", tb =>
				{
					tb.HasTrigger("RefPortPolygonUserView_Version_Create");
					tb.HasTrigger("RefPortPolygonUserView_Version_Update");
				});
				entity.HasKey(e => e.RPP_PK);
			});

			modelBuilder.Entity<RefShippingLineUserView>(entity =>
			{
				entity.ToTable("RefShippingLineUserView", tb =>
				{
					tb.HasTrigger("RefShippingLineUserView_Version_Create");
					tb.HasTrigger("RefShippingLineUserView_Version_Update");
				});
				entity.HasKey(e => e.RSL_PK);
				entity.Ignore(e => e.RSL_IsEditable);
			});

			modelBuilder.Entity<RefStlScriptUserView>(entity =>
			{
				entity.ToTable("RefStlScriptUserView", tb =>
				{
					tb.HasTrigger("RefStlScriptUserView_Version_Create");
					tb.HasTrigger("RefStlScriptUserView_Version_Update");
				});
				entity.HasKey(e => e.STL_PK);
				entity.Ignore(e => e.STL_IsEditable);
			});

			modelBuilder.Entity<RefUNLOCOUserView>(entity =>
			{
				entity.ToTable("RefUNLOCOUserView", tb =>
				{
					tb.HasTrigger("RefUNLOCOUserView_Version_Create");
					tb.HasTrigger("RefUNLOCOUserView_Version_Update");
				});
				entity.HasKey(e => e.RL_PK);
			});

			modelBuilder.Entity<RefVesselUserView>(entity =>
			{
				entity.ToTable("RefVesselUserView", tb =>
				{
					tb.HasTrigger("RefVesselUserView_Version_Create");
					tb.HasTrigger("RefVesselUserView_Version_Update");
				});
				entity.HasKey(e => e.RV_PK);
			});
		}
	}
}
