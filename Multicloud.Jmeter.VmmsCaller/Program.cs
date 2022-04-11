using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System.Diagnostics;



var credentials = SdkContext.AzureCredentialsFactory.FromFile("my.azureauth");

var azure = Azure
    .Configure()
    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
    .Authenticate(credentials).WithDefaultSubscription();

var vmScaleSet = azure.VirtualMachineScaleSets.GetByResourceGroup("jmt-resources", "jmt-vmss");

var networkInterfaces = vmScaleSet.ListNetworkInterfaces();

var listaIps = new List<string>();

foreach (var vmNetworkInterface in networkInterfaces)
{
    listaIps.Add(vmNetworkInterface.PrimaryPrivateIP);
}

var ipsNormalized = string.Join(",", listaIps);

//Gerar a variavel de ambiente
"log_folder=$(date +%Y%m%d%H%M%S)".Bash();

//Criar a pasta com o nome
"sudo mkdir /home/ubuntu/$log_folder".Bash();

//Executar
var output = $"sudo ./../../../apache-jmeter-5.4.3/bin/jmeter -n -t ./../../script.jmx -R {ipsNormalized} -l results_$log_folder.jtl -Jserver.rmi.ssl.disable=true -o /home/ubuntu/$log_folder".Bash();
Console.WriteLine(output);

public static class ShellHelper
{
    public static string Bash(this string cmd)
    {
        var escapedArgs = cmd.Replace("\"", "\\\"");

        var process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return result;
    }
}
