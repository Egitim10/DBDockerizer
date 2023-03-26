using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBDockerizer.Models
{
    public class SqlContainer
    {
        public string ContainerName { get; set; } = "sql";
        public string SqlImage { get; set; } = "mcr.microsoft.com/mssql/server:2022-latest";
        public string SqlPass { get; set; } = "Abc1234Sql";
        public string SqlPorts { get; set; } = "1442:1433";
        public string IpAddresse { get; set; } = "192.168.1.19";
    }
}
