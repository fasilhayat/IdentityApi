using IdentityApi.Model;
using System.Collections.Immutable;
using System.Runtime.Caching;
using System.Text;
using System.Text.Json;

const string dataSourceFolder = @"var/data";
const string dataSourceFilePath = @$"{dataSourceFolder}/medlemmer.json";
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

LoadDataSource();
app.MapGet("/id/{id}", static (string id) => GetValue("identity", id));
app.MapGet("/cpr/{cpr}", static (string cpr) => GetValue("cpr", cpr));
app.Run();

static void LoadDataSource()
{
    Console.WriteLine($"{DateTime.Now} - Reading json datafile");
    var json = JsonDocument.Parse(File.ReadAllText(dataSourceFilePath, Encoding.UTF8));
    var forretningsnoegler = json.Deserialize<IList<Noeglering>>();

    var identityStore = forretningsnoegler!.ToImmutableDictionary(x => x.Identitetsnoegle!, x => x.Cprnummer!);
    var cprStore = forretningsnoegler!.ToImmutableDictionary(x => x.Cprnummer!, x => x.Identitetsnoegle!);
    var cachePunkt = new CachePunkt(cprStore, identityStore);

    MemoryCache.Default.Set("IdentityCache", cachePunkt, new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 0, 10, 0), RemovedCallback = CacheRemovedCallback });
}

static void CacheRemovedCallback(CacheEntryRemovedArguments arguments)
{
    LoadDataSource();
    GC.Collect();
}

static IResult GetValue(string cacheSubject, string key)
{
    var cachedItem = (CachePunkt) MemoryCache.Default.GetCacheItem("IdentityCache")!.Value;
    var datastore = cacheSubject == "identity" ? cachedItem.IdentityStore : cachedItem.CprStore;

    datastore.TryGetValue(key, out var value);
    return Results.Content(value, contentType: "text/plain", statusCode: string.IsNullOrEmpty(value) ? 404 : 200);
}