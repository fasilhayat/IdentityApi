using System.Collections.Concurrent;
using System.Text.Json;
using IdentityApi.Model;

const string filePath = @"var/data/medlemmer.json";
var json = JsonDocument.Parse(File.ReadAllText(filePath));
var deserialized = json.Deserialize<IEnumerable<Medlem>>();

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

app.MapGet("/id/{id}", GetCpr);
app.MapGet("/cpr/{cpr}", GetIdentitetsnoegle);
app.Run();

IResult GetCpr(uint id)
{
    identityStore.TryGetValue(id, out var cpr);
    return Results.Content(cpr, contentType: "text/plain", statusCode: string.IsNullOrEmpty(cpr) ? 404 : 200);
}

IResult GetIdentitetsnoegle(string cprnummer)
{
    cprStore.TryGetValue(cprnummer, out var identitetsnoegle);
    return Results.Content(identitetsnoegle.ToString(), contentType: "text/plain", statusCode: identitetsnoegle == 0 ? 404 : 200);
}