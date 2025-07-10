# Prerequisites
# Install dotnet ef as a global tool
dotnet tool install --global dotnet-ef --version 7.0.5

# Generate DbContext and Entity classes
dotnet ef dbcontext scaffold "data source=localhost;initial catalog=RefDbRepoSafe;integrated security=True;MultipleActiveResultSets=True;TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer --use-database-names --data-annotations --context SafeDbContext --no-onconfiguring --context-dir DbContext --output-dir Models --namespace CargoWise.RefDbRepo.Service.Schema_0_9_New --force
