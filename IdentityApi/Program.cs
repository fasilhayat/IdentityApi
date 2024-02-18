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

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

LoadSource();

app.MapGet("/id/{id}",(string id) => GetValue("identitystore",id));
app.MapGet("/cpr/{cpr}", (string cpr) => GetValue("cprstore",cpr));
app.Run();

void AddToCache(string storageName, FrozenDictionary<string, string> inMemoryStorage)
{
    cache.Add(storageName, inMemoryStorage, new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 0, 60), RemovedCallback = CacheRemovedCallback});
}

void LoadSource()
{
    Console.WriteLine("Reading json datafile");
    var json = JsonDocument.Parse(File.ReadAllText(dataSourceFilePath, Encoding.UTF8));
    var forretningsnoegler = json.Deserialize<IList<Noeglering>>();

    identityStore = forretningsnoegler!.ToFrozenDictionary(x => x.Identitetsnoegle!, x => x.Cprnummer!);
    AddToCache("identitystore", identityStore);

    cprStore = forretningsnoegler!.ToFrozenDictionary(x => x.Cprnummer!, x => x.Identitetsnoegle!);
    AddToCache("cprstore", cprStore);
    Console.WriteLine($"identitystore size: {identityStore.Count} | cprstore size: {cprStore.Count}");
}

IResult GetValue(string cacheItemKey, string key)
{
    var cachedItem = (IReadOnlyDictionary<string, string>) cache.GetCacheItem(cacheItemKey)!.Value;
    cachedItem.TryGetValue(key, out var value);
    return Results.Content(value, contentType: "text/plain", statusCode: string.IsNullOrEmpty(value) ? 404 : 200);
}

void CacheRemovedCallback(CacheEntryRemovedArguments arguments)
{
    Console.WriteLine($"Cached empty: {arguments.RemovedReason} | Item {arguments.CacheItem.Key} removed - reloading data");
    LoadSource();
}