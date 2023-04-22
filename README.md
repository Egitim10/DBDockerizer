# DBDockerizer

This library allows developers to easily create and manage SQL Server containers within their development environment
during integration tests whlie Docker Desktop is running
Basicly; it runs Powershell Commands in order to create a container.
Feel free to participate and add additional features. Such as different databases :)


NuGet
=====
[Package Link](https://www.nuget.org/packages/DBDockerizer)


Usage
=====


    /// SqlContainer Model by default
    public class SqlContainer
    {
        public string ContainerName { get; set; } = "sql";
        public string SqlImage { get; set; } = "mcr.microsoft.com/mssql/server:2022-latest";
        public string SqlPass { get; set; } = "Abc1234Sql";
        public string SqlPorts { get; set; } = "1442:1433";
        public string IpAddresse { get; set; } = "192.168.1.19";
    }

=====



    public class ApiFixture : WebApplicationFactory<ApiMarker>, IAsyncLifetime
    {

        private string sqlConnStr = "Data Source=localhost,1442;Initial Catalog=ApiDB;User Id=sa;Password=Abc1234Sql;encrypt=false";
        
        ///===Create SqlContainer instance
        public SqlContainer sqlContainer = new();
        private WireMockTestServer wireMockTestServer = new();
        public async Task InitializeAsync()
        {
            ///===Call  to create Sql Container
            ///===Pay attention that Docker Desktop is running
            sqlContainer.CreateSqlContainer();
            await Task.Delay(2000);
            //return Task.CompletedTask;
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {

            builder.ConfigureLogging(log =>
            {
                log.ClearProviders();
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<MyDbContetx>));

                services.AddDbContext<MyDbContetx>(ops =>
                        ops.UseSqlServer(sqlConnStr, options => options.EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null
                        )));

                ServiceProvider serviceProvider = services.BuildServiceProvider();
                IServiceScope serviceScope = serviceProvider.CreateScope();
                MyDbContetx webApiDbContext = serviceScope.ServiceProvider.GetRequiredService<MyDbContetx>();
                webApiDbContext.Database.EnsureDeleted();
                webApiDbContext.Database.EnsureCreated();


                services.AddHttpClient("dummyjson", httpClient =>
                {
                    httpClient.BaseAddress = new Uri($"{wireMockTestServer._serverUrl}");
                    httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
                    httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "Egitim10");
                });


            });

        }
        public new Task DisposeAsync()
        {
        
            sqlContainer.RemoveSqlContainer();
            return Task.CompletedTask;

        }

    }

}
