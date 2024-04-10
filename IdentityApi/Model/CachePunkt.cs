using System.Collections.Immutable;

namespace IdentityApi.Model;

public class CachePunkt
{
    public CachePunkt(ImmutableDictionary<string, string> cprStore, ImmutableDictionary<string, string> identityStore)
    {
        CprStore = cprStore;
        IdentityStore = identityStore;
    }

    public ImmutableDictionary<string, string> CprStore { get; }

    public ImmutableDictionary<string, string> IdentityStore { get; }
}