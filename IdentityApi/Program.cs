using System.Collections.Frozen;
using System.Text.Json;
using IdentityApi.Model;

const string filePath = @"var/data/medlemmer.json";
var json = JsonDocument.Parse(File.ReadAllText(filePath));
var medlemmer = json.Deserialize<IList<Medlem>>();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

if (medlemmer is { Count: > 0 })
{
    var identityStore = medlemmer.ToFrozenDictionary(x => x.Identitetsnoegle, x=> x.Cprnummer!);
    var cprStore = medlemmer.ToFrozenDictionary(x => x.Cprnummer!, x => x.Identitetsnoegle);

    app.MapGet("/id/{id}", (uint id) => GetCpr(id, identityStore));
    app.MapGet("/cpr/{cpr}", (string cpr) => GetIdentitetsnoegle(cpr, cprStore));
}
app.Run();

IResult GetCpr(uint id, FrozenDictionary<uint, string> identityStore)
{
    identityStore.TryGetValue(id, out var cpr);
    return Results.Content(cpr, contentType: "text/plain", statusCode: string.IsNullOrEmpty(cpr) ? 404 : 200);
}

IResult GetIdentitetsnoegle(string cpr, FrozenDictionary<string, uint> cprStore)
{
    cprStore.TryGetValue(cpr, out var identitetsnoegle);
    return Results.Content(identitetsnoegle == 0 ? string.Empty : identitetsnoegle.ToString(), contentType: "text/plain", statusCode: identitetsnoegle == 0 ? 404 : 200);
}