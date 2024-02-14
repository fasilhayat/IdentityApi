using System.Collections.Frozen;
using System.Text.Json;
using IdentityApi.Model;

var json = JsonDocument.Parse(File.ReadAllText(@"var/data/medlemmer.json"));
var forretningsnoegler = json.Deserialize<IList<Noeglering>>();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

if (forretningsnoegler is { Count: > 0 })
{
    var identityStore = forretningsnoegler.ToFrozenDictionary(x => x.Identitetsnoegle, x=> x.Cprnummer!);
    var cprStore = forretningsnoegler.ToFrozenDictionary(x => x.Cprnummer!, x => x.Identitetsnoegle);
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