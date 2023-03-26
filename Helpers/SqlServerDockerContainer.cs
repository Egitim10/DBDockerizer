using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBDockerizer.Helpers
{
    public class SqlServerDockerContainer : IDisposable
    {
        private readonly DockerClient _dockerClient;
        private readonly string _containerId;
        public readonly string ConnectionString;

        public SqlServerDockerContainer(SqlConnection sqlConnection)
        {
            _dockerClient = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();
            var password = "Abc1234Sql";

            var containerConfig = new CreateContainerParameters
            {
                Image = "mcr.microsoft.com/mssql/server:2022-latest",
                Name = "sql",
                //Name = $"sql-server-{Guid.NewGuid()}",
                Tty = true,
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                        //{ "1442/tcp", new List<PortBinding> { new PortBinding { HostPort = "1442" } } }
                        {"1433/tcp", new List<PortBinding> { new PortBinding { HostPort = "1442" } } }
                },
                 Mounts = new List<Mount>
                {
                    new Mount
                    {
                        Type = "bind",
                        Source = Path.Combine(Environment.CurrentDirectory, "Data"),
                        Target = "/var/opt/mssql/data"
                    }
                },
                    PublishAllPorts = true
                },
                Env = new List<string>
            {
                $"ACCEPT_EULA=Y",
                $"SA_PASSWORD={password}",
            }
            };

            //var container = _dockerClient.Containers.CreateContainer(containerConfig);

            var container = _dockerClient.Containers.CreateContainerAsync(containerConfig).Result;

            _dockerClient.Containers.StartContainerAsync(container.ID, new ContainerStartParameters()).Wait();

            _containerId = container.ID;

            ConnectionString = new SqlConnectionStringBuilder(sqlConnection.ConnectionString)
            {
                DataSource = "localhost,1442",
                UserID = "sa",
                Password = password
            }.ConnectionString;
        }

        public void Dispose()
        {
            _dockerClient.Containers.StopContainerAsync(_containerId, new ContainerStopParameters()).Wait();
            _dockerClient.Containers.RemoveContainerAsync(_containerId, new ContainerRemoveParameters()).Wait();
        }

    }

}
