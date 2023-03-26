using DBDockerizer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace DBDockerizer.Handlers
{
    public static class SqlServerHandler
    {

        // System.Management.Automation;
        private static readonly PowerShell ps = PowerShell.Create();

        public static string CreateSqlContainer(this SqlContainer sqlContainer)
        {

            string script = $"docker container run --name {sqlContainer.ContainerName}  --rm  -d   -e  ACCEPT_EULA=Y  -e  MSSQL_SA_PASSWORD={sqlContainer.SqlPass}  -p {sqlContainer.SqlPorts}  {sqlContainer.SqlImage}";
            //string script = $"docker container stop {sqlContainer.ContainerName}";
            return ExecuteCommand(script);
        }

        private static string ExecuteCommand(string script)
        {
            ps.Commands.Clear();
            string errorMsg = string.Empty;

            ps.AddScript(script);
            ps.AddCommand("Out-String");

            PSDataCollection<PSObject> outputCollection = new();
            ps.Streams.Error.DataAdded += (sender, e) =>
            {
                errorMsg = ((PSDataCollection<ErrorRecord>)sender!)[e.Index].ToString();
            };

            IAsyncResult result = ps.BeginInvoke<PSObject, PSObject>(null, outputCollection);

            //wait powershell to finish executing
            ps.EndInvoke(result);

            StringBuilder sb = new();
            foreach (var outPutItem in outputCollection)
            {
                sb.AppendLine(outPutItem.BaseObject.ToString());
            }

            if (!string.IsNullOrEmpty(errorMsg))
            {
                throw new Exception(errorMsg);
            }

            return sb.ToString();
        }

        public static string RemoveSqlContainer(this SqlContainer sqlContainer)
        {
            /*_ = ExecuteCommand($"docker container rm {sqlContainer.ContainerName} -f");*/
            return ExecuteCommand($"docker stop {sqlContainer.ContainerName}");
        }


        //public static string ResetDocker()
        //{
        //    string script = "echo y | docker stop $(docker ps -a -q)";
        //    _ = RunScript(script);

        //    script = "echo y | docker rm $(docker ps -a -q)";
        //    _ = RunScript(script);

        //    script = "echo y |  docker image prune  -a";
        //    _ = RunScript(script);

        //    script = "echo y |  docker system prune  -a";
        //    return RunScript(script);
        //}

    }
}
