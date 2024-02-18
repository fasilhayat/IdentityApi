using IdentityApi.Model;
using System.Collections.Frozen;
using System.Text;
using System.Text.Json;

const string dataSourceFolder = @"var/data";
const string dataSourceFilePath = @$"{dataSourceFolder}/medlemmer.json";

FileSystemWatcher watcher;
FrozenDictionary<string, string> identityStore;
FrozenDictionary<string, string> cprStore;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

WatchDataSourceFolder();
LoadSource();

app.MapGet("/id/{id}",(string id) => GetValue(id, identityStore));
app.MapGet("/cpr/{cpr}", (string cpr) => GetValue(cpr, cprStore));
app.Run();

void WatchDataSourceFolder()
{
    watcher = new FileSystemWatcher();
    watcher.Path = dataSourceFolder;
    watcher.NotifyFilter = NotifyFilters.LastWrite;
    watcher.Filter = "*.json";
    watcher.Changed += OnChanged;
    watcher.EnableRaisingEvents = true;
}

void LoadSource()
{
    Console.WriteLine("Reading json datafile");
    var json = JsonDocument.Parse(File.ReadAllText(dataSourceFilePath, Encoding.UTF8));
    var forretningsnoegler = json.Deserialize<IList<Noeglering>>();
    identityStore = forretningsnoegler!.ToFrozenDictionary(x => x.Identitetsnoegle!, x => x.Cprnummer!);
    cprStore = forretningsnoegler!.ToFrozenDictionary(x => x.Cprnummer!, x => x.Identitetsnoegle!);
    Console.WriteLine($"identitystore size: {identityStore.Count} | cprstore size: {cprStore.Count}");
}

void OnChanged(object source, FileSystemEventArgs e)
{
    Thread.Sleep(100); // Wait for release lock on file
    LoadSource();
}

IResult GetValue(string key, IReadOnlyDictionary<string, string> storage)
{
    storage.TryGetValue(key, out var value);
    return Results.Content(value, contentType: "text/plain", statusCode: string.IsNullOrEmpty(value) ? 404 : 200);
}