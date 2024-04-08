using IdentityApi.Model;
using System.Collections.Immutable;
using System.Runtime.Caching;
using System.Text;
using System.Text.Json;

const string dataSourceFolder = @"var/data";
const string dataSourceFilePath = @$"{dataSourceFolder}/medlemmer.json";
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

LoadSource();
app.MapGet("/id/{id}", static (string id) => GetValue("identity", id));
app.MapGet("/cpr/{cpr}", static (string cpr) => GetValue("cpr", cpr));
app.Run();

static void LoadSource()
{
    Console.WriteLine("Reading json datafile");
    var json = JsonDocument.Parse(File.ReadAllText(dataSourceFilePath, Encoding.UTF8));
    var forretningsnoegler = json.Deserialize<IList<Noeglering>>();

    var identityStore = forretningsnoegler!.ToImmutableDictionary(x => x.Identitetsnoegle!, x => x.Cprnummer!);
    AddToCache("identity", identityStore);

    //var cprStore = forretningsnoegler!.ToImmutableDictionary(x => x.Cprnummer!, x => x.Identitetsnoegle!);
    //AddToCache("cpr", cprStore);
    //Console.WriteLine($"identity key count: {identityStore.Count} | cpr key count: {cprStore.Count}");
}

static void AddToCache(string cacheKey, ImmutableDictionary<string, string> inMemoryStorage)
{
    Console.WriteLine($"Added in memory-cache: '{cacheKey}' | key count: {inMemoryStorage.Count}");
    MemoryCache.Default.Set(cacheKey, inMemoryStorage, new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 0, 0, 30), RemovedCallback = CacheRemovedCallback });
    Console.WriteLine($"Antal keys: {MemoryCache.Default.GetCount()}");
}

static void CacheRemovedCallback(CacheEntryRemovedArguments arguments)
{
    Console.WriteLine($"Cache empty: {arguments.RemovedReason} | cache: '{arguments.CacheItem.Key}' removed - reloading data");
    GC.Collect();
    LoadSource();
}

static IResult GetValue(string cacheKey, string key)
{
    Console.WriteLine($"Requested cache-key: '{cacheKey}' | key: {key}");
    var cachedItem = (IReadOnlyDictionary<string, string>)MemoryCache.Default.GetCacheItem(cacheKey)!.Value;
    cachedItem.TryGetValue(key, out var value);
    return Results.Content(value, contentType: "text/plain", statusCode: string.IsNullOrEmpty(value) ? 404 : 200);
}