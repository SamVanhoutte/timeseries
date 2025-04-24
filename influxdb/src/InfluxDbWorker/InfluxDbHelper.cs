using InfluxDB.Client;

namespace InfluxDbWorker;

public class InfluxDbHelper
{
    internal static readonly string Token = "t8mvNAx4qJjk8TXu4ldEZ8QzFwrnPeU4FIJGHPuKSblgWpNL4d5lfng4WQpjRxyoF6lyYiOkLAxqJysKNp3Row==";

    public static async Task ShowOrganizationsAsync()
    {
        using var client = new InfluxDBClient("http://localhost:8086", Token);
        var orgClient = client.GetOrganizationsApi();
        var orgs = await orgClient.FindOrganizationsAsync();
        foreach (var org in orgs)
        {
            Console.WriteLine(org.Name);
        }
    }
}