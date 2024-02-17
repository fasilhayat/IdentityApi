using IdentityApi.Model;
using System.Collections.Frozen;
using System.Text;
using System.Text.Json;

const string dataSourceFolder = @"var/data";
const string dataSourceFile = "medlemmer.json";
const string dataSourceFilePath = $"{dataSourceFolder}/{dataSourceFile}";

FrozenDictionary<string, string> identityStore;
FrozenDictionary<string, string> cprStore;
IList<Noeglering>? forretningsnoegler;
FileSystemWatcher watcher;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

WatchDataSourceFolder();
LoadSource();

app.MapGet("/id/{id}", GetCpr);
app.MapGet("/cpr/{cpr}", GetIdentitetsnoegle);
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
    forretningsnoegler = json.Deserialize<IList<Noeglering>>();
    identityStore = forretningsnoegler!.ToFrozenDictionary(x => x.Identitetsnoegle!, x => x.Cprnummer!);
    cprStore = forretningsnoegler!.ToFrozenDictionary(x => x.Cprnummer!, x => x.Identitetsnoegle!);
    Console.WriteLine($"identitystore size: {identityStore.Count} | cprstore size: {cprStore.Count}");
}

void OnChanged(object source, FileSystemEventArgs e)
{
    Thread.Sleep(100); // Release lock
    LoadSource();
}

IResult GetCpr(string id)
{
    identityStore.TryGetValue(id, out var cpr);
    return Results.Content(cpr, contentType: "text/plain", statusCode: string.IsNullOrEmpty(cpr) ? 404 : 200);
}

IResult GetIdentitetsnoegle(string cpr)
{
    cprStore.TryGetValue(cpr, out var identitetsnoegle);
    return Results.Content(identitetsnoegle, contentType: "text/plain", statusCode: string.IsNullOrEmpty(identitetsnoegle) ? 404 : 200);
}