namespace IdentityApi;

using Model;
using System.Collections.Concurrent;
using System.Text.Json;

public class Program
{
    public static void Main(string[] args)
    {
        var filePath = @"var/data/medlemmer.json";
        var json = JsonDocument.Parse(File.ReadAllText(filePath));
        var deserialized = json.Deserialize<List<Medlem>>();

        var identityStore = new ConcurrentDictionary<uint, string>();
        var cprStore = new ConcurrentDictionary<string, uint>();

        if (deserialized != null)
            foreach (var medlem in deserialized)
            {
                identityStore.TryAdd(medlem.Identitetsnoegle, medlem.Cprnummer!);
                cprStore.TryAdd(medlem.Cprnummer!, medlem.Identitetsnoegle);
            }
        
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/id/{id}", (uint id) => GetCpr(id, identityStore));
        app.MapGet("/cpr/{cpr}", (string cpr) =>  GetIdentitetsnoegle(cpr, cprStore));
        app.Run();
    }

    public static IResult GetCpr(uint id, ConcurrentDictionary<uint, string> identityStore)
    {
        identityStore.TryGetValue(id, out var cpr);
        return Results.Content(string.IsNullOrEmpty(cpr) ? "null" : cpr, contentType: "text/plain");
    }

    public static IResult GetIdentitetsnoegle(string cprnummer, ConcurrentDictionary<string, uint> cprStore)
    {
        cprStore.TryGetValue(cprnummer, out var identitetsnoegle);
        return Results.Content(identitetsnoegle == 0 ? "null" : identitetsnoegle.ToString(), contentType: "text/plain");
    }
}