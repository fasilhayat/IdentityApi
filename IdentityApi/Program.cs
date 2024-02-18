using IdentityApi.Model;
using System.Collections.Frozen;
using System.Text;
using System.Text.Json;
using System.Runtime.Caching;

const string dataSourceFolder = @"var/data";
const string dataSourceFilePath = @$"{dataSourceFolder}/medlemmer.json";

FrozenDictionary<string, string> identityStore;
FrozenDictionary<string, string> cprStore;
ObjectCache? cache = MemoryCache.Default;
TimeSpan slidingExpïryTime = new(0, 0, 25);

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

LoadSource();
app.MapGet("/id/{id}",(string id) => GetValue("identity",id));
app.MapGet("/cpr/{cpr}", (string cpr) => GetValue("cpr",cpr));
app.Run();

void AddToCache(string cacheKey, FrozenDictionary<string, string> inMemoryStorage)
{
    cache.Add(cacheKey, inMemoryStorage, new CacheItemPolicy { SlidingExpiration = slidingExpïryTime, RemovedCallback = CacheRemovedCallback});
}

void LoadSource()
{
    Console.WriteLine("Reading json datafile");
    var json = JsonDocument.Parse(File.ReadAllText(dataSourceFilePath, Encoding.UTF8));
    var forretningsnoegler = json.Deserialize<IList<Noeglering>>();

    identityStore = forretningsnoegler!.ToFrozenDictionary(x => x.Identitetsnoegle!, x => x.Cprnummer!);
    AddToCache("identity", identityStore);

    cprStore = forretningsnoegler!.ToFrozenDictionary(x => x.Cprnummer!, x => x.Identitetsnoegle!);
    AddToCache("cpr", cprStore);
    Console.WriteLine($"identity key count: {identityStore.Count} | cpr key count: {cprStore.Count}");
}

IResult GetValue(string cacheKey, string key)
{
    var cachedItem = (IReadOnlyDictionary<string, string>) cache.GetCacheItem(cacheKey)!.Value;
    cachedItem.TryGetValue(key, out var value);
    return Results.Content(value, contentType: "text/plain", statusCode: string.IsNullOrEmpty(value) ? 404 : 200);
}

void CacheRemovedCallback(CacheEntryRemovedArguments arguments)
{
    Console.WriteLine($"Cache empty: {arguments.RemovedReason} | cache: '{arguments.CacheItem.Key}' removed - reloading data");
    LoadSource();
}